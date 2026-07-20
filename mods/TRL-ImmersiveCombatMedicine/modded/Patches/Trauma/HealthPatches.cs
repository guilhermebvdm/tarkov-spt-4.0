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
        // + captura de HP PRÉ-tiro p/ o gatilho percentual (item 007)
        [HarmonyPriority(Priority.High)]
        static bool Prefix(Player __instance, DamageInfoStruct damageInfo, EBodyPart bodyPartType, out float __state)
        {
            __state = -1f;
            if (!TRLImmersiveCombatMedicinePlugin.ConfigMasterEnabled.Value) return true;
            if (__instance == null || !__instance.HealthController.IsAlive) return true;

            // ref: Player.cs:30475-30480 — ActiveHealthController.ApplyDamage MUTA o HP da parte dentro do
            // corpo original; capturar AQUI (Prefix, antes do corpo rodar) é o único jeito de ver o valor
            // PRÉ-tiro exato, sem a distorção de overkill que "postHp + damageInfo.Damage" teria.
            // ref: docs/trauma-primitives.md §P7 (ilspycmd_assembly_real) — DidBodyDamage é delta de
            // EBodyPart.Common com spill, NÃO o dano na parte; descartado como fonte.
            // CR-01-02 (code-review 007 r1): captura só depois do gate de master/vida — evita o custo (trivial,
            // mas desnecessário) de GetBodyPartHealth quando o mod inteiro está desligado.
            if (bodyPartType == EBodyPart.Chest || bodyPartType == EBodyPart.Head)
            {
                var ahc = __instance.ActiveHealthController; // ref: Player.cs:25291 — __instance já não-null aqui
                if (ahc != null) __state = ahc.GetBodyPartHealth(bodyPartType).Current; // ref: docs/trauma-primitives.md §P7 (ActiveHealthController.GetBodyPartHealth — protótipo compilado, PA-01-01)
            }

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

        // POSTFIX: Onde o Desmaio é calculado (limiar fixo legado REMOVIDO — item 007)
        [HarmonyPriority(Priority.Low)]
        static void Postfix(Player __instance, DamageInfoStruct damageInfo, EBodyPart bodyPartType, float __state)
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

                // ref: spec 007 — limiar fixo (>=35 tórax / >=10 cabeça, sem gate de analgésico) REMOVIDO;
                // ConfigConsumerBlackout2 é o ÚNICO gatilho restante (sub-toggle da lógica de entrada).
                // PA-02-05 (review 2): filtro de domínio (Chest/Head) explícito AQUI — sem isto, Evaluate
                // seria chamado para QUALQUER bodyPartType e dependeria do sentinel __state=-1f (setado só
                // p/ Chest/Head no Prefix) coincidir com o guard "preHitHp<=0f" para produzir o mesmo
                // resultado; explícito aqui, o else de domínio dentro de Evaluate fica puramente defensivo.
                bool shouldFaint = isValidTraumaType
                    && (bodyPartType == EBodyPart.Chest || bodyPartType == EBodyPart.Head)
                    && TRLImmersiveCombatMedicinePlugin.ConfigConsumerBlackout2.Value
                    && TraumaBlackoutTrigger.Evaluate(__instance, bodyPartType, __state);

                if (shouldFaint)
                {
                    // Configura Timers
                    // ref: CR-04 — GraceTimers NÃO nasce aqui: o grace de 5s é
                    // ancorado no WAKE (Plugin.WakeLocalPlayer / MainLoopPatch).
                    // ref: item 008 (RANGE-READY resolvido) — sorteio uniforme min-max no MESMO ponto que
                    // antes lia o fixo. min>max (config inválida) normalizado via Mathf.Min/Max — SEM
                    // warning/UI extra, mantendo o item simples (decisão da spec técnica §3). Com
                    // min==max, Random.Range devolve o valor determinístico — caso degenerado, não
                    // caso especial (nenhum branch dedicado). Todo o resto (wake, rampa visual,
                    // contusion, pacote de sync, espelhos) segue derivando do deadline gravado em
                    // BlackoutTimers — nada mais lê os configs de duração diretamente.
                    float configuredMin = TRLImmersiveCombatMedicinePlugin.ConfigBlackoutDurationMin.Value;
                    float configuredMax = TRLImmersiveCombatMedicinePlugin.ConfigBlackoutDurationMax.Value;
                    float rollMin = Mathf.Min(configuredMin, configuredMax);
                    float rollMax = Mathf.Max(configuredMin, configuredMax);
                    // PA-01-02 (review técnica 01): Random.Range(float,float) é inclusivo em AMBOS os extremos
                    // (assinatura distinta de Random.Range(int,int), exclusiva no max) — confirmado por decompile
                    // de UnityEngine.CoreModule.dll. Com min==max, sempre devolve exatamente esse valor.
                    float duration = UnityEngine.Random.Range(rollMin, rollMax); // ref: MedicalLogic.cs:366 — mesmo idioma (Range, não .value)
                    TraumaState.BlackoutTimers[id] = now + duration;
                    TraumaState.BlackoutStartTimes[id] = now;

                    // ref: item 008 — log de verificação estatística (critério de aceite da spec funcional:
                    // "verificável por log das durações sorteadas"). LogInfo one-time por desmaio (não por
                    // frame) — não gateado por ConfigVerboseEngineLog, ao contrário do log de decisão do
                    // TraumaBlackoutTrigger (007): aqui é lifecycle (1 evento por desmaio), não detalhe de
                    // polling, mesmo critério de "LogInfo para evento único" já usado em
                    // TRLImmersiveCombatMedicinePlugin.cs (log de "entrou em Coma/Desmaio").
                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                        $"[Blackout] {id} duração sorteada: {duration:F1}s (min={rollMin:F0} max={rollMax:F0})");

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
