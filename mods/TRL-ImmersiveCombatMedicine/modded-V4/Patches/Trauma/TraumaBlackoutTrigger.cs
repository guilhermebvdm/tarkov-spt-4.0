using EFT;
using UnityEngine;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Gatilho percentual de desmaio (spec 007) — substitui o limiar fixo absoluto legado.
    /// STATELESS: sem lifecycle de raid (nada a limpar em GameWorld.OnDestroy/BaseLocalGame.Stop — AP-01 N/A).
    /// Sem registro em TraumaConsumerRegistry (ver spec técnica §7 — nenhuma TraumaRegion cobre tórax/cabeça).</summary>
    internal static class TraumaBlackoutTrigger
    {
        // Constantes de decisão 8/9 (docs/trauma-matrix.md) — NÃO configuráveis: a spec funcional lista
        // exatamente 4 números expostos no F12 (2 percentuais + 2 pisos); as chances de roll são fixas.
        private const float ChestRollChance = 0.5f;
        private const float HeadRollChance = 0.5f;
        private const float HeadRollChancePainkiller = 0.25f;

        /// <summary>Chamado 1x por invocação de ApplyDamageInfo (= 1x por pellet/fragmento — decisão 15,
        /// garantido pelo ponto de patch, ver spec técnica §1/§6). preHitHp vem do __state do Prefix.</summary>
        internal static bool Evaluate(Player player, EBodyPart bodyPartType, float preHitHp)
        {
            if (player == null) return false;
            // Corner (spec funcional): vida pré-tiro já <= 0 (parte destruída por hit anterior no mesmo
            // frame) — não dispara o roll percentual (sem divisão por zero, sem percentual inválido).
            if (preHitHp <= 0f) return false;

            var ahc = player.ActiveHealthController; // ref: Player.cs:25291
            if (ahc == null) return false;

            float postHitHp = ahc.GetBodyPartHealth(bodyPartType).Current; // ref: docs/trauma-primitives.md §P7 (ActiveHealthController.GetBodyPartHealth — protótipo compilado, PA-01-01)
            float effectiveDamage = preHitHp - postHitHp; // pós-armadura/multiplicadores; clamp natural em overkill
            if (effectiveDamage <= 0f) return false;

            // Gate de analgésico NO INSTANTE do hit (motor já reservou este predicado p/ o item 007).
            bool underPainkiller = TraumaEngine.IsUnderPainkiller(player); // ref: TraumaEngine.cs:99

            float pctThreshold;
            float absFloor;
            float rollChance;

            if (bodyPartType == EBodyPart.Chest)
            {
                pctThreshold = TRLImmersiveCombatMedicinePlugin.ConfigBlackoutChestPercent.Value / 100f;
                absFloor = TRLImmersiveCombatMedicinePlugin.ConfigBlackoutChestAbsoluteFloor.Value;
                rollChance = underPainkiller ? 0f : ChestRollChance; // decisão 9 — imunidade TOTAL do tórax
            }
            else if (bodyPartType == EBodyPart.Head)
            {
                pctThreshold = TRLImmersiveCombatMedicinePlugin.ConfigBlackoutHeadPercent.Value / 100f;
                absFloor = TRLImmersiveCombatMedicinePlugin.ConfigBlackoutHeadAbsoluteFloor.Value;
                rollChance = underPainkiller ? HeadRollChancePainkiller : HeadRollChance; // cabeça NÃO é imune
            }
            else
            {
                return false; // domínio do desmaio é só tórax/cabeça (fora de escopo — spec funcional)
            }

            if (effectiveDamage < absFloor)
            {
                LogIgnored(player, bodyPartType, effectiveDamage, preHitHp, "piso absoluto");
                return false;
            }
            if (effectiveDamage < pctThreshold * preHitHp)
            {
                LogIgnored(player, bodyPartType, effectiveDamage, preHitHp, "percentual");
                return false;
            }

            // Extremos determinísticos (mesmo idioma do 006 — TraumaStomachConsumer.cs:73): rollChance=0 nunca sucede.
            bool success = rollChance > 0f && Random.value < rollChance;
            if (TRLImmersiveCombatMedicinePlugin.ConfigVerboseEngineLog.Value)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                    $"[Blackout2] {player.ProfileId} part={bodyPartType} dmg={effectiveDamage:0.#} preHp={preHitHp:0.#} pk={underPainkiller} chance={rollChance:0.##} success={success}");
            }
            return success;
        }

        private static void LogIgnored(Player player, EBodyPart part, float effectiveDamage, float preHitHp, string reason)
        {
            if (!TRLImmersiveCombatMedicinePlugin.ConfigVerboseEngineLog.Value) return;
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                $"[Blackout2] {player.ProfileId} part={part} dmg={effectiveDamage:0.#} preHp={preHitHp:0.#} ignorado ({reason})");
        }
    }
}
