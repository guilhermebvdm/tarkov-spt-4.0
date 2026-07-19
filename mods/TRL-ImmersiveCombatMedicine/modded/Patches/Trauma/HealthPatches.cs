using HarmonyLib;
using EFT;
using UnityEngine;
using TRLImmersiveCombatMedicine;
using TRLImmersiveCombatMedicine.Trauma;

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
                // ref: CR-04 — guard de re-entrada cobre blackout ATIVO e GRACE:
                // sem o FaintedPlayerIds aqui, cada hit forte pós-wake re-desmaiava
                // (loop que mantinha o alvo em ragdoll/Deadbody — "não acerto mais").
                // ref: CR-04-13 — bots ganham cooldown próprio (não têm grace).
                if (TraumaState.BlackoutTimers.ContainsKey(id) || TraumaState.FaintedPlayerIds.Contains(id)) return;
                if (TraumaState.BotFaintCooldowns.TryGetValue(id, out float cdUntil) && now < cdUntil) return;

                if (isValidTraumaType)
                {
                    bool isChestTrauma = (bodyPartType == EBodyPart.Chest && damageInfo.Damage >= 35f);
                    bool isHeadTrauma = (bodyPartType == EBodyPart.Head && damageInfo.Damage >= 10f);

                    if (isChestTrauma || isHeadTrauma)
                    {
                        // Configura Timers
                        // ref: CR-04 — GraceTimers NÃO nasce aqui: o grace de 5s é
                        // ancorado no WAKE (Plugin.WakeLocalPlayer / MainLoopPatch).
                        // ref: RANGE-READY — PONTO ÚNICO do roll futuro de duração
                        // aleatória (min-max): rolar AQUI e todo o resto (wake, rampa
                        // visual, contusion, pacote de sync, espelhos) deriva do
                        // deadline gravado em BlackoutTimers — nada mais lê a config.
                        float duration = TRLImmersiveCombatMedicinePlugin.ConfigBlackoutDuration.Value;
                        TraumaState.BlackoutTimers[id] = now + duration;
                        TraumaState.BlackoutStartTimes[id] = now;

                        // Efeitos Locais
                        // ref: CR-04-12 — SEM DoStun no entry: o ToggleDowned do frame
                        // seguinte pausava o efeito e o RETOMAVA no wake (~2-4s de
                        // "tela suja" pós-consciência). O impacto visual do blackout
                        // já vem do DeathFade/FastBlur do Fika.
                        if (__instance.Physical != null) __instance.Physical.Stamina.Current = 0f;
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
                    // ref: spec 004 §1.8(e) (PA-01-09) — o agachar LEGADO de estômago é escritor de pose FORA do
                    // motor (nenhum one-shot de estômago existe antes do 006) e NÃO passa pela absorção D2 do
                    // TraumaPose: guard próprio — ciclo de queda engajado suprime (sem ele, agacharia o jogador
                    // na JANELA/Rising por fora do TraumaPose).
                    if (TraumaFallCycleConsumer.IsCycleEngaged(__instance))
                    {
                        TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                            $"[Trauma2] stomach legacy suppressed (fall-cycle) {id}");
                    }
                    else
                    {
                        if (__instance.Physical != null) __instance.Physical.Stamina.Current = 0f;
                        __instance.MovementContext.SetPoseLevel(0f, true);
                        VoiceHelper.TriggerTraumaVoice(__instance, "Gut");
                    }
                }
            }
            // ref: spec 003 §4 (D10) — sub-bloco legado de PERNAS removido (seed de ImpactTimers/LegPenaltyTimers,
            // prone em hit e voz): a reação de pernas agora é do Trauma 2.0 (motor 002 + consumidor 003).
            // Desmaio (acima), estômago e braços seguem legados até os itens 007/006/005.
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
