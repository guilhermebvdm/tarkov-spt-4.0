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

            // ref: spec 003 §4 (D10) — sub-bloco legado de PERNAS removido (seed de ImpactTimers/LegPenaltyTimers,
            // prone em hit e voz): a reação de pernas agora é do Trauma 2.0 (motor 002 + consumidor 003).
            // ref: spec 005 §1.7 (D10 — PA-02-04) — sub-bloco legado de BRAÇOS removido (voz "Arm" em hit de
            // braço zerado, gateado pela key inerte ConfigArmsEnabled): o feedback de entrada agora é o toast
            // de 1ª ocorrência + tremor visível do Trauma 2.0 (TraumaArmsConsumer) — paridade com o 003, que
            // removeu também a voz de hit de perna.
            // ref: spec 006 §1.9 (D10) — bloco legado de ESTÔMAGO removido por inteiro ("sem ar" por hit ≥35 fora
            // de prone: stamina zerada + SetPoseLevel(0f, true) + voz "Gut", INCLUSIVE bots — o Postfix não
            // filtrava IA). A reação de estômago agora é do Trauma 2.0 (motor 002 publica a zerada;
            // TraumaStomachConsumer rola p=75/25 pelo analgésico LATCHED da transição e agacha via TraumaPose,
            // chamada DIRETA sem publish). O guard próprio IsCycleEngaged (PA-01-09 do 004) morre junto — a
            // arbitragem D2 do estômago passa a ser a absorção padrão da primitiva (TraumaPose.AbsorbIfCycleEngaged,
            // já chamada no topo de TryInvoluntaryCrouch/BotCrouchDip). A key "Sistema de Estomago" fica INERTE
            // (remoção no item 010). Desmaio (acima) segue legado até o item 007.
        }
    }
}
