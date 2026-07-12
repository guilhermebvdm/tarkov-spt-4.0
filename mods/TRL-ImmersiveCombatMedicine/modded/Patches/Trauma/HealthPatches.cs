using HarmonyLib;
using EFT;
using UnityEngine;
using TRLImmersiveCombatMedicine;

namespace TrueTrauma
{
    [HarmonyPatch(typeof(Player), "ApplyDamageInfo")]
    public static class DamageTriggerPatch
    {
        // PREFIX: Escudo de Dano Opcional (Recomendado manter para evitar morte por lag)
        [HarmonyPriority(Priority.High)]
        static bool Prefix(Player __instance, DamageInfoStruct damageInfo)
        {
            if (!TRLImmersiveCombatMedicinePlugin.ConfigMasterEnabled.Value) return true;
            if (__instance == null || !__instance.HealthController.IsAlive) return true;

            // Se o ID já estiver na lista de desmaiados, bloqueia dano de combate
            if (!__instance.IsAI && TraumaState.FaintedPlayerIds.Contains(__instance.ProfileId))
            {
                if (damageInfo.DamageType == EDamageType.Bullet 
                    || damageInfo.DamageType == EDamageType.Explosion
                    || damageInfo.DamageType == EDamageType.GrenadeFragment
                    || damageInfo.DamageType == EDamageType.Landmine
                    || damageInfo.DamageType == EDamageType.Sniper)
                {
                    return false;
                }
            }
            return true;
        }

        // POSTFIX: Onde o Desmaio é calculado
        [HarmonyPriority(Priority.Low)]
        static void Postfix(Player __instance, DamageInfoStruct damageInfo, EBodyPart bodyPartType)
        {
            if (__instance == null || !__instance.HealthController.IsAlive) return;
            if (!TRLImmersiveCombatMedicinePlugin.ConfigMasterEnabled.Value) return;

            float now = Time.time;
            string id = __instance.ProfileId;

            bool isValidTraumaType = damageInfo.DamageType == EDamageType.Bullet ||
                                     damageInfo.DamageType == EDamageType.Explosion ||
                                     damageInfo.DamageType == EDamageType.Sniper ||
                                     damageInfo.DamageType == EDamageType.Landmine ||
                                     damageInfo.DamageType == EDamageType.GrenadeFragment;

            // 1. LÓGICA DE DESMAIO
            if (TRLImmersiveCombatMedicinePlugin.ConfigBlackoutEnabled.Value)
            {
                if (TraumaState.BlackoutTimers.ContainsKey(id)) return;

                if (isValidTraumaType)
                {
                    bool isChestTrauma = (bodyPartType == EBodyPart.Chest && damageInfo.Damage >= 35f);
                    bool isHeadTrauma = (bodyPartType == EBodyPart.Head && damageInfo.Damage >= 10f);

                    if (isChestTrauma || isHeadTrauma)
                    {
                        // Configura Timers
                        float duration = TRLImmersiveCombatMedicinePlugin.ConfigBlackoutDuration.Value;
                        TraumaState.BlackoutTimers[id] = now + duration;
                        TraumaState.BlackoutStartTimes[id] = now;
                        TraumaState.GraceTimers[id] = now + duration + 5f;

                        // Efeitos Locais
                        if (__instance.Physical != null) __instance.Physical.Stamina.Current = 0f;
                        __instance.ActiveHealthController?.DoStun(60f, 1f); // Flashbang
                        __instance.MovementContext.IsInPronePose = true;
                        if (__instance.HandsController is IFirearmHandsController firearm) firearm.SetAim(false);

                        // --- A MUDANÇA CRUCIAL ---
                        // Sincroniza o status "Desmaiado" via Fika (ou localmente)
                        // Isso vai colocar o ID na lista negra dos bots no Host
                        FikaBridge.SyncFaintStatus(__instance, true);

                        return;
                    }
                }
            }

            // (MANTENHA O RESTANTE DO CÓDIGO DE ESTÔMAGO, PERNAS E BRAÇOS IGUAL AO SEU)
            if (TRLImmersiveCombatMedicinePlugin.ConfigStomachEnabled.Value)
            {
                bool isValidStomachDmg = damageInfo.DamageType == EDamageType.Bullet ||
                                         damageInfo.DamageType == EDamageType.Explosion ||
                                         damageInfo.DamageType == EDamageType.Sniper;

                if (isValidStomachDmg && bodyPartType == EBodyPart.Stomach && damageInfo.Damage >= 35f && !__instance.MovementContext.IsInPronePose)
                {
                    if (__instance.Physical != null) __instance.Physical.Stamina.Current = 0f;
                    __instance.MovementContext.SetPoseLevel(0f, true);
                    VoiceHelper.TriggerTraumaVoice(__instance, "Gut");
                }
            }
            if (TRLImmersiveCombatMedicinePlugin.ConfigLegsEnabled.Value && (bodyPartType == EBodyPart.LeftLeg || bodyPartType == EBodyPart.RightLeg))
            {
                if (HealthUtils.IsPartDestroyed(__instance, bodyPartType))
                {
                    TraumaState.ImpactTimers[id] = now + 1.0f;
                    
                    bool leftLegDestroyed = HealthUtils.IsPartDestroyed(__instance, EBodyPart.LeftLeg);
                    bool rightLegDestroyed = HealthUtils.IsPartDestroyed(__instance, EBodyPart.RightLeg);
                    if (leftLegDestroyed && rightLegDestroyed)
                    {
                        if (!TraumaState.LegPenaltyTimers.ContainsKey(id))
                            TraumaState.LegPenaltyTimers[id] = now;
                    }

                    if (!__instance.MovementContext.IsInPronePose)
                    {
                        __instance.MovementContext.IsInPronePose = true;
                    }
                    VoiceHelper.TriggerTraumaVoice(__instance, "Leg");
                }
            }
            if (TRLImmersiveCombatMedicinePlugin.ConfigArmsEnabled.Value && (bodyPartType == EBodyPart.LeftArm || bodyPartType == EBodyPart.RightArm))
            {
                if (HealthUtils.IsPartDestroyed(__instance, bodyPartType))
                {
                    VoiceHelper.TriggerTraumaVoice(__instance, "Arm");
                }
            }
        }
    }
}
