using System.Collections.Generic;
using Comfort.Common;
using EFT;
using UnityEngine;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Consumidor de PERNAS (spec 003): mancar N1/N2 contínuo + agachar involuntário one-shot.
    /// Primeiro consumidor real do motor 002 — evento-first, zero polling próprio. Dono-only herdado
    /// (o motor só publica donos — D16). INTERIM do FallCycle REMOVIDO na entrega do 004 (spec 004 §1.7 —
    /// PA-01-02): a linha Cair é SAÍDA p/ efeitos do 003 (handoff explícito; o 004 assume com causa 1001).</summary>
    public sealed class TraumaLegsConsumer : MonoBehaviour
    {
        /// <summary>Causa PRÓPRIA no dicionário SpeedLimits — fora do enum (Player.cs:1584-1595 tem 9 valores);
        /// dict aceita chave fora do range (P1, inferência marcada — smoke test loga o efetivo).</summary>
        internal const Player.ESpeedLimit TraumaCause = (Player.ESpeedLimit)1000;

        private static TraumaLegsConsumer _instance;
        private static bool _n2ClampWarned; // warn 1×/sessão do min(N2, N1) — review 2 do 003, achado 4

        /// <summary>Efeito aplicado por DONO. Keyed por Player (review 1, achado 5): a referência é necessária
        /// p/ desfazer (RemoveCap) e re-estabelecer no religar. Entradas podem ficar STALE (morte não publica
        /// transição — comportamento 1 do motor; toggle-off durante a saída): saneadas nos sweeps — morto sai
        /// SEM log "cap OFF", GetLine==None é podado oportunisticamente com undo do cap, e null/world-swap de
        /// GameWorld limpa tudo (code-review 1 do 003, achado 2).</summary>
        private readonly Dictionary<Player, TraumaLine> _applied = new Dictionary<Player, TraumaLine>();
        private readonly List<Player> _sweepScratch = new List<Player>(); // reusada — sem alocação no caminho quente
        private bool _wasActive;
        private GameWorld _trackedWorld; // espelha a detecção de world-swap do motor (transit — code-review 1, achado 2)

        private static readonly TraumaRegion[] LegsRegions = { TraumaRegion.Legs };

        private void Awake()
        {
            _instance = this;
            // Registro no registry destrava o toast de 1ª ocorrência das linhas de perna (decisão 20)
            TraumaConsumerRegistry.Register(TraumaConsumerId.LegsEffects, LegsRegions, IsActive);
            TraumaEngine.SubscribeWithSnapshot(OnTransition);   // replay establishing cobre assinatura tardia
            TraumaEngine.OneShotPublished += OnOneShot;         // já cooldown-gated pelo motor (decisão 19)
        }

        /// <summary>Master legado + master Trauma 2.0 + toggle próprio (comportamento 9 do 002).</summary>
        internal static bool IsActive()
        {
            return TRLImmersiveCombatMedicinePlugin.ConfigMasterEnabled.Value
                && TRLImmersiveCombatMedicinePlugin.ConfigTrauma2Enabled.Value
                && TRLImmersiveCombatMedicinePlugin.ConfigConsumerLegsEffects.Value;
        }

        /// <summary>Consulta p/ os patches (§2 da spec): linha de perna atualmente APLICADA a este jogador.</summary>
        internal static bool TryGetApplied(Player p, out TraumaLine line)
        {
            line = TraumaLine.None;
            TraumaLegsConsumer inst = _instance;
            return inst != null && !(p is null) && inst._applied.TryGetValue(p, out line);
        }

        /// <summary>Linhas que recebem o efeito N2. FallCycle FORA desde o 004 (interim removido — spec 004 §1.7):
        /// o cap da JANELA é do TraumaSpeedCap (causa própria 1001), nunca deste consumidor.</summary>
        internal static bool IsN2Tier(TraumaLine line)
        {
            return line == TraumaLine.LegsLimpN2
                || line == TraumaLine.LegsCrouchPlusLimpN2;
        }

        /// <summary>Alvo em % do baseline por linha. Efetivo do N2 = min(N2, N1) — configuração invertida não deixa
        /// N2 mais leve que N1 (warn 1×; review 2 do 003, achado 4).</summary>
        internal static float LineTargetPercent(TraumaLine line)
        {
            float n1 = TRLImmersiveCombatMedicinePlugin.ConfigLegsN1TargetPercent.Value;
            if (line == TraumaLine.LegsLimpN1) return n1;
            float n2 = TRLImmersiveCombatMedicinePlugin.ConfigLegsN2TargetPercent.Value;
            if (n2 > n1)
            {
                if (!_n2ClampWarned)
                {
                    _n2ClampWarned = true;
                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning(
                        $"[Trauma2] config N2 Target ({n2:0.#}%) > N1 Target ({n1:0.#}%) — usando min(N2, N1) = {n1:0.#}%");
                }
                n2 = n1;
            }
            return n2;
        }

        private void OnTransition(TraumaTransition t)
        {
            // ref: code-review 1 do 004 (CR-01-04) — exceção de consumidor não pode subir p/ o
            // StateChanged?.Invoke do motor (mataria a publicação das regiões/records restantes do frame)
            try { OnTransitionCore(t); }
            catch (System.Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogError($"[Trauma2] LegsConsumer.OnTransition: {ex.Message}");
            }
        }

        private void OnTransitionCore(TraumaTransition t)
        {
            if (t.Region != TraumaRegion.Legs) return;
            if (!IsActive()) return; // toggle off = ignora (motor segue publicando — comportamento 9 do 002)
            Player p = t.Player;
            if (p is null) return;
            if (t.To == TraumaLine.None || t.To == TraumaLine.LegsFallCycle)
            {
                // Handoff explícito (PA-01-02): entrada na linha Cair é SAÍDA p/ efeitos do 003 — o 004 assume
                // com a causa própria 1001; a coexistência no frame de handoff é arbitrada pela min-composição
                // nativa do SpeedLimits (method_4 :1798), em qualquer ordem de handlers.
                if (_applied.Remove(p)) RemoveCapGuarded(p);
                return;
            }
            // Mapa linha→efeito: LimpN1→cap N1 | LimpN2/CrouchPlusLimpN2→cap N2.
            // Establishing aplica cap SEM one-shot (o motor nem publica one-shot em establishing — 002).
            // N1↔N2/rebaixamento: sempre via ApplyCap (Remove+Add — Add é no-op com causa existente,
            // MovementContext.cs:1672-1679; recompute único no ProcessSpeedLimits :2553-2558 → sem flicker).
            ApplyCap(p, t.To);
        }

        private void OnOneShot(Player p, TraumaOneShotKind kind, TraumaLine line)
        {
            // ref: CR-01-04 do 004 — mesmo isolamento do OnTransition (OneShotPublished?.Invoke vive no motor)
            try { OnOneShotCore(p, kind, line); }
            catch (System.Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogError($"[Trauma2] LegsConsumer.OnOneShot: {ex.Message}");
            }
        }

        private void OnOneShotCore(Player p, TraumaOneShotKind kind, TraumaLine line)
        {
            if (!IsActive() || kind != TraumaOneShotKind.InvoluntaryCrouch) return; // InvoluntaryFall = TraumaFallCycleConsumer (004)
            if (p is null) return;
            if (p.IsAI)
            {
                TraumaPose.BotCrouchDip(p); // fire-and-forget, fora da fila de adiados (review 1, achado 4)
                return;
            }
            TraumaPose.TryInvoluntaryCrouch(p, TraumaRegion.Legs, kind); // executa | adia | no-op só-para-baixo
        }

        private void ApplyCap(Player p, TraumaLine line)
        {
            MovementContext mc = p != null ? p.MovementContext : null; // fake-null da Unity coberto pelo operador ==
            if (mc == null) { _applied.Remove(p); return; }            // review 2, achado 3 — morto/destruído
            float pct = LineTargetPercent(line);
            float baseline = mc.MaxSpeed;                              // ref: MovementContext.cs:910 — baseline composto (Strength + CustomClasses, D12)
            float cap = Mathf.Clamp01(pct / 100f) * baseline;
            // Re-write da MESMA causa exige Remove+Add (Add é no-op com causa existente :1672-1679 — bloqueador da
            // review 1); sem janela sem cap: ambos só marcam SpeedLimitIsDirty, recompute único (:2553-2558)
            mc.RemoveStateSpeedLimit(TraumaCause);                     // ref: MovementContext.cs:1790
            mc.AddStateSpeedLimit(cap, TraumaCause);                   // ref: MovementContext.cs:1672 (min-composição :1798-1824)
            _applied[p] = line;
            // StateSpeedLimit fica STALE até o recompute (dirty-flag) — esperado computado LOCALMENTE
            // (review 1, achado 3); a classificação CLAMP oficial do AC4 sai do re-log RECOMPUTE do postfix
            float expected = cap;
            foreach (KeyValuePair<Player.ESpeedLimit, float> kv in mc.SpeedLimits) // ref: dict público MovementContext.cs:384
                expected = Mathf.Min(expected, kv.Value);
            bool clamped = expected < cap - 0.001f; // vanilla 0.3/0.2 mais duro que o alvo (delta nunca negativo por construção)
            if (IsN2Tier(line) && TRLImmersiveCombatMedicinePlugin.ConfigBlockSprintOnN2.Value)
                mc.EnableSprint(false); // corta sprint EM CURSO (ref: :2783); quem SEGURA é o gate CanSprint (SpeedLimitPatches)
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                $"[Trauma2] legs cap {p.ProfileId} line={line} target={pct:0.#} cap={cap:0.###} baseline={baseline:0.###} expected={expected:0.###} clamped={(clamped ? "true" : "false")}");
        }

        private void RemoveCapGuarded(Player p)
        {
            // review 2, achado 3: Player.RemoveStateSpeedLimit NÃO null-propaga (:25835-25837) — guard obrigatório.
            // code-review 2, achado 1: !IsAlive NÃO pode pular a remoção do cap — o DOWNED do Fika é IsAlive=false
            // com o MESMO MovementContext revivendo depois (FikaPlayer re-seta IsAlive); cap não removido no
            // toggle-off ficaria permanente pós-revive. Remoção do dict é inofensiva em cadáver; só o recompute
            // e o log "cap OFF" ficam gateados em vivo (code-review 1, achado 2: recompute em cadáver é ruído).
            if (p == null || p.MovementContext == null) return;
            p.MovementContext.RemoveStateSpeedLimit(TraumaCause);
            if (p.HealthController == null || !p.HealthController.IsAlive) return;
            p.UpdateSpeedLimitByHealth(); // AP-04: recompute oficial devolve sprint/caps ao estado canônico vanilla (Player.cs:29068)
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] legs cap OFF {p.ProfileId}");
        }

        private void Update()
        {
            GameWorld gw = Singleton<GameWorld>.Instance;
            if (gw == null)
            {
                // padrão N1: mundo morreu — caps morrem com os MovementContexts; só limpar bookkeeping
                if (_applied.Count > 0) _applied.Clear();
                TraumaPose.CancelAll("raid-end");
                TraumaPose.ClearBotRestores();
                _trackedWorld = null;
                _wasActive = IsActive();
                return;
            }
            if (!ReferenceEquals(gw, _trackedWorld))
            {
                // World-swap sem passar por null (transit) — espelha a detecção do motor (code-review 1, achado 2):
                // caps/players do mundo antigo morreram; só limpar bookkeeping e adotar o mundo novo
                if (_applied.Count > 0) _applied.Clear();
                TraumaPose.CancelAll("raid-end");
                TraumaPose.ClearBotRestores();
                _trackedWorld = gw;
            }

            bool active = IsActive();
            if (_wasActive && !active)
            {
                // Desligar mid-raid: desfaz os próprios efeitos NA HORA (corner da funcional).
                // RemoveCapGuarded pula mortos/destruídos sem log (code-review 1, achado 2).
                _sweepScratch.Clear();
                foreach (KeyValuePair<Player, TraumaLine> kv in _applied) _sweepScratch.Add(kv.Key);
                _applied.Clear();
                for (int i = 0; i < _sweepScratch.Count; i++) RemoveCapGuarded(_sweepScratch[i]);
                _sweepScratch.Clear();
                // PA-01-04 (estendido no 006): toggle-off do 003 cancela SÓ (kind, região=Legs) — nunca varre
                // as quedas do 004 nem os adiados de ESTÔMAGO do 006 (spec 006 §1.5, TraumaPose.cs:212).
                TraumaPose.CancelKind(TraumaOneShotKind.InvoluntaryCrouch, TraumaRegion.Legs, "toggle-off");
                TraumaPose.FlushBotRestores();
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo("[Trauma2] legs consumer OFF — caps desfeitos");
            }
            else if (!_wasActive && active)
            {
                // Religar mid-raid: estabelecer do snapshot SEM one-shot e SEM toast (corner religar — paridade
                // com avaliação inicial). Dependência internal registrada: TraumaEngine.IsOwnedHere (mesmo assembly).
                var players = gw.RegisteredPlayers;
                int established = 0;
                for (int i = 0; i < players.Count; i++)
                {
                    var p = players[i] as Player;
                    if (!TraumaEngine.IsOwnedHere(p)) continue;
                    TraumaLine line = TraumaEngine.GetLine(p, TraumaRegion.Legs);
                    if (line == TraumaLine.None || line == TraumaLine.LegsFallCycle) continue; // FallCycle é do 004 (PA-01-02)
                    ApplyCap(p, line);
                    established++;
                }
                if (established > 0)
                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] legs consumer ON — {established} estado(s) estabelecido(s) do snapshot");
            }
            _wasActive = active;

            if (active)
            {
                // Poda oportunista (code-review 1, achado 2): entrada cujo motor já diz None é stale
                // (ex.: saída publicada com o toggle off) — desfaz o cap vazado e limpa o bookkeeping.
                // FallCycle também é podada (spec 004 §1.7 — entrada na linha Cair perdida com o toggle off).
                _sweepScratch.Clear();
                foreach (KeyValuePair<Player, TraumaLine> kv in _applied)
                {
                    TraumaLine gl = TraumaEngine.GetLine(kv.Key, TraumaRegion.Legs);
                    if (gl == TraumaLine.None || gl == TraumaLine.LegsFallCycle) _sweepScratch.Add(kv.Key);
                }
                for (int i = 0; i < _sweepScratch.Count; i++)
                {
                    _applied.Remove(_sweepScratch[i]);
                    RemoveCapGuarded(_sweepScratch[i]);
                }
                _sweepScratch.Clear();

                TraumaPose.PumpDeferred();   // adiados D7: re-checa guards, re-valida snapshot, executa/cancela
                TraumaPose.PumpBotRestores(); // devolução do dip de bot fora de combate
            }
        }
    }
}
