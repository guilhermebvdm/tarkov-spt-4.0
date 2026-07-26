using System.Collections.Generic;
using EFT;
using UnityEngine;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Cap N2 da JANELA do ciclo de queda (spec 004 §1.7 — PA-01-01): helper EXCLUSIVO do 004 com causa
    /// própria (ESpeedLimit)1001 — NUNCA compartilhada com a 1000 do 003. A coexistência no frame de handoff é
    /// arbitrada pela min-composição nativa do dict SpeedLimits (MovementContext method_4() :1798) — undo cruzado
    /// impossível por construção. Independe do toggle 003 (não consulta TraumaLegsConsumer.IsActive).</summary>
    internal static class TraumaSpeedCap
    {
        /// <summary>Causa própria do 004 — fora do enum (mesmo padrão da 1000 do 003, TraumaLegsConsumer.cs:16).
        /// Consumidores futuros ganham causas 1002+ (premissa p/ item 011).</summary>
        internal const Player.ESpeedLimit FallCycleCause = (Player.ESpeedLimit)1001;

        /// <summary>Bookkeeping do 004 (consultado pelo re-log RECOMPUTE ANTES do gate IsActive do 003 — spec §4).</summary>
        private static readonly HashSet<Player> _applied = new HashSet<Player>();

        /// <summary>ref: item 020 — caps da janela do 004 ainda aplicados (deveria ser 0 fora de raid).</summary>
        internal static int ResidualCount => _applied.Count;

        internal static bool IsApplied(Player p) => !(p is null) && _applied.Contains(p);

        /// <summary>Apply Remove+Add (Add é no-op com causa existente — MovementContext.cs:1672-1679; recompute único
        /// por dirty-flag :2553-2558). Alvo = N2 efetivo via LineTargetPercent do 003 (min(N2, N1)). Após o Add,
        /// EnableSprint(false) INCONDICIONAL (PA-02-09 — corta sprint EM CURSO no engage-de-pé; sem gate no
        /// ConfigBlockSprintOnN2, coerência PA-02-01; quem SEGURA é o CanSprintPatch — padrão TraumaLegsConsumer.cs:134-135).</summary>
        internal static void Apply(Player p)
        {
            MovementContext mc = p != null ? p.MovementContext : null; // fake-null da Unity coberto pelo operador ==
            if (mc == null) { if (!(p is null)) _applied.Remove(p); return; }
            float pct = TraumaLegsConsumer.LineTargetPercent(TraumaLine.LegsFallCycle); // N2 efetivo (min N2,N1 — warn 1× do 003)
            float baseline = mc.MaxSpeed;                                // ref: MovementContext.cs:910 — baseline composto (D12)
            float cap = Mathf.Clamp01(pct / 100f) * baseline;
            mc.RemoveStateSpeedLimit(FallCycleCause);                    // ref: MovementContext.cs:1790
            mc.AddStateSpeedLimit(cap, FallCycleCause);                  // ref: MovementContext.cs:1672 (min-composição :1798)
            _applied.Add(p);
            // Esperado computado localmente (StateSpeedLimit fica stale até o recompute) — classificação CLAMP
            // oficial sai do re-log RECOMPUTE (mesmo tratamento de baseline drift do 003 — spec §7)
            float expected = cap;
            foreach (KeyValuePair<Player.ESpeedLimit, float> kv in mc.SpeedLimits) // ref: dict público MovementContext.cs:384
                expected = Mathf.Min(expected, kv.Value);
            bool clamped = expected < cap - 0.001f;
            mc.EnableSprint(false); // PA-02-09 — incondicional (ref: MovementContext.cs:2783)
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                $"[Trauma2] fall window cap {p.ProfileId} target={pct:0.#} cap={cap:0.###} baseline={baseline:0.###} expected={expected:0.###} clamped={(clamped ? "true" : "false")}");
        }

        /// <summary>Remove SÓ a causa 1001 — downed-safe (mesmo contrato CR do 003, TraumaLegsConsumer.cs:140-152):
        /// !IsAlive NUNCA pula a remoção (DOWNED do Fika revive com o MESMO MovementContext); só recompute e log
        /// ficam gateados em vivo.</summary>
        internal static void RemoveGuarded(Player p)
        {
            if (p is null) return;
            _applied.Remove(p); // bookkeeping fecha mesmo com o objeto destruído
            if (p == null || p.MovementContext == null) return;
            p.MovementContext.RemoveStateSpeedLimit(FallCycleCause);
            if (p.HealthController == null || !p.HealthController.IsAlive) return;
            p.UpdateSpeedLimitByHealth(); // AP-04: recompute oficial (Player.cs:29068)
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] fall window cap OFF {p.ProfileId}");
        }

        /// <summary>Mundo morto — caps morrem com os MovementContexts; só bookkeeping (padrão N1/003).</summary>
        internal static void Clear() => _applied.Clear();
    }
}
