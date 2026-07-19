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
            if (!TRLImmersiveCombatMedicinePlugin.ConfigBlockSprintOnN2.Value) return;
            if (!TraumaLegsConsumer.IsActive()) return;
            if (TraumaState.PlayerField == null) return;
            Player player;
            try { player = TraumaState.PlayerField.GetValue(__instance) as Player; } // padrão do CantStandUpPatch (campo _player)
            catch { return; }
            if (player == null) return;
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
