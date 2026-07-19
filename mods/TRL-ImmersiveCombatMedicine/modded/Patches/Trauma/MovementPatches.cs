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

                    // ref: CR-04-19 — renovação de DoContusion REMOVIDA: era no-op para
                    // o humano local downed (Fika seta DamageCoeff=0 e o DoContusion
                    // guarda em DamageCoeff>0) e invisível para bots — o visual do
                    // blackout vem do DeathFade/FastBlur do Fika.

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

                    // Lógica de recuperação (Levantar ou ficar deitado)
                    if (__instance.IsAI && __instance.AIData?.BotOwner != null)
                    {
                        AggroHelper.UnpauseBot(__instance);

                        // ref: CR-01-19 — bot não tem grace period (o branch de grace é
                        // !IsAI): sem esta remoção, todo bot que desmaiou uma vez ficava
                        // PERMANENTEMENTE em FaintedPlayerIds — invisível aos outros bots.
                        // ref: CR-02 — via SyncFaintStatus(false) para TAMBÉM avisar os
                        // peers (o true do desmaio do bot é broadcast; sem o false, os
                        // clients ficavam com espelho órfão e o bot permanentemente mudo).
                        FikaBridge.SyncFaintStatus(__instance, false);
                        TraumaState.GraceTimers.Remove(id);
                        // ref: CR-04-13 — cooldown de re-desmaio do bot (sem grace,
                        // um hit forte no frame do wake re-derrubava em loop)
                        TraumaState.BotFaintCooldowns[id] = now + 8f;

                        if (__instance.Physical != null) __instance.Physical.Stamina.Current = __instance.Physical.Stamina.TotalCapacity;
                    }
                    else
                    {
                        // Jogador humano acorda, mas sem stamina
                        if (__instance.Physical != null) __instance.Physical.Stamina.Current = 0f;

                        // ref: CR-04 — grace de 5s ancorado no WAKE (idempotente com o
                        // WakeLocalPlayer do Plugin; requisito: proteção começa com o
                        // jogador já consciente e controlando)
                        TraumaState.GraceTimers[id] = now + 5f;
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

            // ref: spec 003 §4 (D10) — sistema legado de PERNAS aposentado: saíram o bloco humano
            // (prone forçado + voz + LegPenaltyTimers) e o bloco de bots 90 s (clamp de pé permanente —
            // causa-raiz P6 do "levanta e nunca mais cai"). Mancar é do Trauma 2.0 (motor 002 +
            // consumidor 003); a queda real chega no item 004.
        }
    }
}

