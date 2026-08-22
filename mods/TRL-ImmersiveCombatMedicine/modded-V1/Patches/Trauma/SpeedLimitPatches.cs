using System.Collections.Generic;
using EFT;
using HarmonyLib;
using TrueTrauma;
using UnityEngine;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Gate de sprint do N2 (spec 003 §2 — review 1, strong): EnableSprint(false) é corte momentâneo e
    /// CanSprint devolve true sob OnPainkillers (curto-circuito MovementContext.cs:1256-1258, ANTES dos checks de
    /// perna) — re-apertar Shift/SAIN re-decidir destravaria. O gate no getter é a fonte estável (desvio registrado
    /// da rec. P1: o flag SprintDisabled corre risco de ser limpo pelos recomputes vanilla method_0/method_28).
    /// ObservedMovementContext SOBRESCREVE CanSprint (fika ObservedMovementContext.cs:34) — espelhos nunca passam
    /// pela base: dono-only por construção (AP-03 auditado).</summary>
    [HarmonyPatch(typeof(MovementContext), nameof(MovementContext.CanSprint), MethodType.Getter)] // ref: MovementContext.cs:1240 (virtual)
    internal static class CanSprintPatch
    {
        static void Postfix(MovementContext __instance, ref bool __result)
        {
            if (!__result) return; // já bloqueado — nada a fazer
            if (TraumaState.PlayerField == null) return;
            Player player;
            try { player = TraumaState.PlayerField.GetValue(__instance) as Player; } // padrão do CantStandUpPatch (campo _player)
            catch { return; }
            if (player == null) return; // player resolvido 1× e compartilhado pelos dois branches (spec 004 §4)
            // Branch do 004 NO TOPO, ANTES dos early-returns de config/IsActive do 003 (PA-02-01): sprint da
            // JANELA bloqueado com a linha Cair, INDEPENDENTE de Legs Effects/Block Sprint On N2 (contrato fixo
            // do cap N2 do ciclo — quem CORTA o sprint em curso é o TraumaSpeedCap.Apply, PA-02-09).
            if (TraumaFallCycleConsumer.IsActive()
                && TraumaEngine.GetLine(player, TraumaRegion.Legs) == TraumaLine.LegsFallCycle)
            {
                __result = false;
                return;
            }
            // Branch N1/N2 do 003 — gates originais valem SÓ aqui
            if (!TRLImmersiveCombatMedicinePlugin.ConfigBlockSprintOnN2.Value) return;
            if (!TraumaLegsConsumer.IsActive()) return;
            if (TraumaLegsConsumer.IsN2Tier(TraumaEngine.GetLine(player, TraumaRegion.Legs)))
                __result = false; // N2 bloqueia sprint, inclusive sob analgésico
        }
    }

    /// <summary>RE-LOG de calibração pós-recompute vanilla (a causa HealthCondition muda AQUI) — fonte OFICIAL da
    /// classificação CLAMP do AC4 (review 1 do 003, achado 3). Sem side-effect de gameplay; só roda para jogadores
    /// com cap do mod aplicado (estado raro). ObservedPlayer no-opa o método (dono-only — AP-03 auditado).</summary>
    [HarmonyPatch(typeof(Player), nameof(Player.UpdateSpeedLimitByHealth))] // ref: Player.cs:29068 (virtual)
    internal static class UpdateSpeedLimitByHealthPatch
    {
        static void Postfix(Player __instance)
        {
            // Re-log RECOMPUTE do cap 1001 (JANELA do 004) — consulta o bookkeeping do 004 ANTES do gate
            // IsActive do 003 (spec 004 §4): o cap da janela independe do toggle Legs Effects.
            MovementContext mc004 = __instance.MovementContext;
            if (mc004 != null && TraumaFallCycleConsumer.IsActive() && TraumaSpeedCap.IsApplied(__instance))
            {
                if (!mc004.SpeedLimits.TryGetValue(TraumaSpeedCap.FallCycleCause, out float fcap))
                    fcap = Mathf.Clamp01(TraumaLegsConsumer.LineTargetPercent(TraumaLine.LegsFallCycle) / 100f) * mc004.MaxSpeed;
                float fexpected = fcap;
                foreach (KeyValuePair<Player.ESpeedLimit, float> kv in mc004.SpeedLimits)
                    fexpected = Mathf.Min(fexpected, kv.Value);
                bool fclamped = fexpected < fcap - 0.001f;
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                    $"[Trauma2] fall window cap RECOMPUTE {__instance.ProfileId} cap={fcap:0.###} expected={fexpected:0.###} clamped={(fclamped ? "true" : "false")}");
            }

            if (!TraumaLegsConsumer.IsActive()) return;
            if (!TraumaLegsConsumer.TryGetApplied(__instance, out TraumaLine line)) return;
            MovementContext mc = __instance.MovementContext;
            if (mc == null) return;
            // Valor realmente aplicado da nossa causa (fallback: re-derivação do alvo se a causa sumiu)
            if (!mc.SpeedLimits.TryGetValue(TraumaLegsConsumer.TraumaCause, out float cap)) // ref: dict público MovementContext.cs:384
                cap = Mathf.Clamp01(TraumaLegsConsumer.LineTargetPercent(line) / 100f) * mc.MaxSpeed;
            float expected = cap;
            foreach (KeyValuePair<Player.ESpeedLimit, float> kv in mc.SpeedLimits)
                expected = Mathf.Min(expected, kv.Value);
            bool clamped = expected < cap - 0.001f;
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                $"[Trauma2] legs cap RECOMPUTE {__instance.ProfileId} cap={cap:0.###} expected={expected:0.###} clamped={(clamped ? "true" : "false")}");
        }
    }
}
