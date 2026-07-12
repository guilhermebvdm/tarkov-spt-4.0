using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using UnityEngine;
using TRLImmersiveCombatMedicine;

namespace TrueTrauma
{
    [HarmonyPatch(typeof(Player), "LateUpdate")]
    class MainLoopPatch
    {
        static void Postfix(Player __instance)
        {
            if (!TRLImmersiveCombatMedicinePlugin.ConfigMasterEnabled.Value) return;
            if (__instance == null || !__instance.HealthController.IsAlive) return;

            string id = __instance.ProfileId;
            float now = Time.time;

            // Verifica se este jogador (seja você ou outro) tem um timer de desmaio ativo
            bool isTimerActive = TraumaState.BlackoutTimers.ContainsKey(id);

                        // --- LÓGICA DE DESMAIO (BLACKOUT) ---
            if (TRLImmersiveCombatMedicinePlugin.ConfigBlackoutEnabled.Value && isTimerActive)
            {
                // CASO 1: AINDA ESTÁ DESMAIADO
                if (now < TraumaState.BlackoutTimers[id])
                {
                    // 1. Força a postura deitada
                    if (__instance.IsAI && __instance.AIData?.BotOwner != null)
                    {
                        AggroHelper.PauseBot(__instance);
                    }
                    
                    if (!__instance.MovementContext.IsInPronePose)
                    {
                        __instance.MovementContext.SetPoseLevel(0f, true);
                        __instance.MovementContext.IsInPronePose = true;
                    }

                    // 2. Mantém o efeito visual de tontura (Blur)
                    // ref: CR-01-27 — vanilla dispara DoContusion por EVENTO; por frame
                    // eram ~1200 passagens pelo pipeline de efeitos em 20s de blackout.
                    // Renovação por intervalo de 2s mantém o visual com ~10 chamadas.
                    if (!TraumaState.ContusionRenewTimers.TryGetValue(id, out float nextContusion) || now >= nextContusion)
                    {
                        TraumaState.ContusionRenewTimers[id] = now + 2f;
                        __instance.ActiveHealthController?.DoContusion(1f, 1f);
                    }

                    // 3. Força arma baixada e sem stamina visual
                    if (__instance.HandsController is IFirearmHandsController firearm) firearm.SetAim(false);
                    if (__instance.Physical != null)
                    {
                        __instance.Physical.Stamina.Current = 0f;
                        __instance.Physical.HandsStamina.Current = 0f;
                    }

                    // 4. Segurança Local (Redundância para o Bot não atirar)
                    if (!__instance.IsAI) AggroHelper.NeutralizeAggro(__instance);
                }
                // CASO 2: ACABOU O TEMPO (ACORDANDO)
                else
                {
                    // Limpa os timers de blackout (MAS mantém em FaintedPlayerIds e GraceTimers!)
                    TraumaState.BlackoutTimers.Remove(id);
                    TraumaState.BlackoutStartTimes.Remove(id);

                    TraumaState.ContusionRenewTimers.Remove(id);

                    // Lógica de recuperação (Levantar ou ficar deitado)
                    if (__instance.IsAI && __instance.AIData?.BotOwner != null)
                    {
                        AggroHelper.UnpauseBot(__instance);

                        // ref: CR-01-19 — bot não tem grace period (o branch de grace é
                        // !IsAI): sem esta remoção, todo bot que desmaiou uma vez ficava
                        // PERMANENTEMENTE em FaintedPlayerIds — invisível aos outros bots
                        // até o fim da raid.
                        TraumaState.FaintedPlayerIds.Remove(id);
                        TraumaState.GraceTimers.Remove(id);

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
            else if (!TRLImmersiveCombatMedicinePlugin.ConfigBlackoutEnabled.Value && isTimerActive)
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
                    FikaBridge.SyncFaintStatus(__instance, false);
                    AggroHelper.RestoreAggro(__instance);
                }
                else
                {
                    AggroHelper.NeutralizeAggro(__instance);
                }
            }

            // Braços Quebrados (Fadiga ao mirar)
            if (TRLImmersiveCombatMedicinePlugin.ConfigArmsEnabled.Value)
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
            if (!__instance.IsAI && TRLImmersiveCombatMedicinePlugin.ConfigLegsEnabled.Value)
            {
                bool leftLegDestroyed = HealthUtils.IsPartDestroyed(__instance, EBodyPart.LeftLeg);
                bool rightLegDestroyed = HealthUtils.IsPartDestroyed(__instance, EBodyPart.RightLeg);

                if (leftLegDestroyed && rightLegDestroyed)
                {
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

                        if (!__instance.MovementContext.IsInPronePose)
                        {
                            __instance.MovementContext.SetPoseLevel(0f, true);
                            __instance.MovementContext.IsInPronePose = true;
                        }

                        VoiceHelper.TriggerTraumaVoice(__instance, "Leg");
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

