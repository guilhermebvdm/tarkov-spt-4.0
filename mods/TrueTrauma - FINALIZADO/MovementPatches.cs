using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using UnityEngine;

namespace TrueTrauma
{
    [HarmonyPatch(typeof(Player), "LateUpdate")]
    class MainLoopPatch
    {
        static void Postfix(Player __instance)
        {
            if (!TrueTraumaPlugin.ConfigMasterEnabled.Value) return;
            if (__instance == null || !__instance.HealthController.IsAlive) return;

            string id = __instance.ProfileId;
            float now = Time.time;

            // Verifica se este jogador (seja você ou outro) tem um timer de desmaio ativo
            bool isTimerActive = TraumaState.BlackoutTimers.ContainsKey(id);

            // --- LÓGICA DE DESMAIO (BLACKOUT) ---
            if (TrueTraumaPlugin.ConfigBlackoutEnabled.Value && isTimerActive)
            {
                // CASO 1: AINDA ESTÁ DESMAIADO
                if (now < TraumaState.BlackoutTimers[id])
                {
                    // 1. Força a postura deitada
                    if (!__instance.MovementContext.IsInPronePose)
                    {
                        __instance.MovementContext.SetPoseLevel(0f, true);
                        __instance.MovementContext.IsInPronePose = true;
                    }

                    // 2. Mantém o efeito visual de tontura (Blur)
                    // Usamos Contusion aqui para manter o efeito visual sem o flashbang branco do impacto
                    __instance.ActiveHealthController?.DoContusion(1f, 1f);

                    // 3. Força arma baixada e sem stamina visual
                    if (__instance.HandsController is IFirearmHandsController firearm) firearm.SetAim(false);
                    if (__instance.Physical != null)
                    {
                        __instance.Physical.Stamina.Current = 0f;
                        __instance.Physical.HandsStamina.Current = 0f;
                    }

                    // 4. Segurança Local (Redundância para o Bot não atirar)
                    if (!__instance.IsAI) AggroHelper.NeutralizeAggro(__instance);
                    if (__instance.IsAI) AggroHelper.PauseBot(__instance);
                }
                // CASO 2: ACABOU O TEMPO (ACORDANDO)
                else
                {
                    // Limpa os timers de blackout (MAS mantém em FaintedPlayerIds e GraceTimers!)
                    TraumaState.BlackoutTimers.Remove(id);
                    TraumaState.BlackoutStartTimes.Remove(id);

                    // NÃO remove de FaintedPlayerIds aqui! Jogador continua protegido durante o grace period.
                    // A remoção acontece quando o GraceTimer expirar (código mais abaixo).

                    // Lógica de recuperação (Levantar ou ficar deitado)
                    if (__instance.IsAI && __instance.AIData?.BotOwner != null)
                    {
                        AggroHelper.UnpauseBot(__instance);
                        __instance.MovementContext.IsInPronePose = false;
                        __instance.MovementContext.SetPoseLevel(1f, true);
                        if (__instance.Physical != null) __instance.Physical.Stamina.Current = __instance.Physical.Stamina.TotalCapacity;
                    }
                    else
                    {
                        // Jogador humano acorda, mas sem stamina
                        if (__instance.Physical != null) __instance.Physical.Stamina.Current = 0f;
                    }
                }
            }

            // Limpeza de segurança (caso desative o mod no meio da raid)
            else if (!TrueTraumaPlugin.ConfigBlackoutEnabled.Value && isTimerActive)
            {
                TraumaState.BlackoutTimers.Remove(id);
                TraumaState.BlackoutStartTimes.Remove(id);
            }

            // --- OUTRAS MECÂNICAS (BRAÇOS, PERNAS, ETC) ---

            // Grace Period (Tempo de graça pós-acordar)
            if (!__instance.IsAI && TraumaState.GraceTimers.ContainsKey(id))
            {
                if (now > TraumaState.GraceTimers[id])
                {
                    TraumaState.GraceTimers.Remove(id);

                    // AGORA sim: remove da lista de desmaiados e avisa a rede
                    FikaBridge.SyncFaintStatus(__instance, false);

                    // Força os bots a re-adquirirem o jogador como inimigo
                    AggroHelper.RestoreAggro(__instance);
                }
                else
                {
                    // Durante o grace period, mantém protegido
                    AggroHelper.NeutralizeAggro(__instance);
                }
            }

            // Braços Quebrados (Fadiga ao mirar)
            if (TrueTraumaPlugin.ConfigArmsEnabled.Value)
            {
                bool bothArmsDestroyed = HealthUtils.IsPartDestroyed(__instance, EBodyPart.LeftArm) && HealthUtils.IsPartDestroyed(__instance, EBodyPart.RightArm);
                bool isAiming = __instance.ProceduralWeaponAnimation != null && __instance.ProceduralWeaponAnimation.IsAiming;

                if (bothArmsDestroyed && isAiming)
                {
                    if (!TraumaState.AimingFatigueTimers.ContainsKey(id)) TraumaState.AimingFatigueTimers[id] = now;
                    else if (now > TraumaState.AimingFatigueTimers[id] + 1f)
                    {
                        if (__instance.HandsController is IFirearmHandsController f) f.SetAim(false);
                        VoiceHelper.TriggerTraumaVoice(__instance, "TryAim");
                        TraumaState.AimingFatigueTimers.Remove(id);
                    }
                }
                else
                {
                    if (TraumaState.AimingFatigueTimers.ContainsKey(id)) TraumaState.AimingFatigueTimers.Remove(id);
                }
            }

            // Pernas Quebradas (Lógica para punição de Humanos)
            if (!__instance.IsAI && TrueTraumaPlugin.ConfigLegsEnabled.Value)
            {
                bool leftLegDestroyed = HealthUtils.IsPartDestroyed(__instance, EBodyPart.LeftLeg);
                bool rightLegDestroyed = HealthUtils.IsPartDestroyed(__instance, EBodyPart.RightLeg);

                if (leftLegDestroyed && rightLegDestroyed)
                {
                    // Se o humano conseguir sair da postura deitada (passou os 10 segundos e levantou)
                    if (!__instance.MovementContext.IsInPronePose)
                    {
                        bool leftBroken = __instance.HealthController.IsBodyPartBroken(EBodyPart.LeftLeg);
                        bool rightBroken = __instance.HealthController.IsBodyPartBroken(EBodyPart.RightLeg);
                        
                        bool randomFracture = UnityEngine.Random.Range(0, 100) < 30;
                        bool appliedFracture = false;

                        if (randomFracture && (!leftBroken || !rightBroken))
                        {
                            EBodyPart target = !leftBroken ? EBodyPart.LeftLeg : EBodyPart.RightLeg;
                            __instance.ActiveHealthController?.DoFracture(target);
                            appliedFracture = true;
                        }

                        if (!appliedFracture)
                        {
                            DamageInfoStruct dmg = default;
                            dmg.Damage = 15f;
                            dmg.DamageType = EDamageType.Fall;
                            __instance.ActiveHealthController?.ApplyDamage(EBodyPart.LeftLeg, 15f, dmg);
                        }

                        // Força a postura deitada novamente com a devida validação
                        if (!__instance.MovementContext.IsInPronePose)
                        {
                            __instance.MovementContext.SetPoseLevel(0f, true);
                            __instance.MovementContext.IsInPronePose = true;
                        }

                        VoiceHelper.TriggerTraumaVoice(__instance, "Leg");

                        // Reinicia o timer de punição de 10s
                        TraumaState.LegPenaltyTimers[id] = now;
                    }
                }
                else
                {
                    if (TraumaState.LegPenaltyTimers.ContainsKey(id)) TraumaState.LegPenaltyTimers.Remove(id);
                }
            }

            // Pernas Quebradas (Lógica para Bots caírem)
            if (__instance.IsAI && __instance.AIData?.BotOwner != null)
            {
                float hpLeft = __instance.HealthController.GetBodyPartHealth(EBodyPart.LeftLeg).Current;
                float hpRight = __instance.HealthController.GetBodyPartHealth(EBodyPart.RightLeg).Current;
                bool legsGone = (hpLeft < 1f && hpRight < 1f);

                if (legsGone)
                {
                    if (!TraumaState.BotLegsBrokenStartTimes.ContainsKey(id)) TraumaState.BotLegsBrokenStartTimes.Add(id, now);
                    float tempoQuebrado = now - TraumaState.BotLegsBrokenStartTimes[id];

                    // Bot cai depois de 90 segundos com perna quebrada (exemplo)
                    if (tempoQuebrado >= 90f)
                    {
                        if (__instance.MovementContext.IsInPronePose)
                        {
                            __instance.MovementContext.IsInPronePose = false;
                            __instance.MovementContext.SetPoseLevel(1f, true);
                            if (__instance.AIData.BotOwner.WeaponManager?.Selector != null)
                            {
                                var sel = __instance.AIData.BotOwner.WeaponManager.Selector;
                                if (sel.EquipmentSlot != EquipmentSlot.FirstPrimaryWeapon && !sel.IsChanging) sel.TakeMainWeapon();
                            }
                            __instance.AIData.BotOwner.Steering?.LookToMovingDirection();
                            if (__instance.Physical != null) __instance.Physical.Stamina.Current = __instance.Physical.Stamina.TotalCapacity;
                        }
                    }
                }
                else
                {
                    if (TraumaState.BotLegsBrokenStartTimes.ContainsKey(id)) TraumaState.BotLegsBrokenStartTimes.Remove(id);
                }
            }
        }
    }
}