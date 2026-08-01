using System;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using HarmonyLib;
using UnityEngine;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Primitiva COMPARTILHADA de pose involuntária (003 agachar; 004 queda; 006 reusa). Agachar é
    /// one-shot SÓ-PARA-BAIXO sem lock (decisão 5); queda é prone forçado com readback + fallback agachado
    /// (P4 rec. (2) — spec 004 §1.4). Guards D7 3 eixos com adiamento + re-validação + cancelamento; fila
    /// generalizada por kind com entradas internas sem refund (PA-01-03) e pump idempotente por frame (PA-01-12).</summary>
    internal static class TraumaPose
    {
        /// <summary>Fila de adiados com DEDUP por (player, kind). No HIT de dedup a entrada existente é ATUALIZADA
        /// (PublishDeadline + RequiredLine) — não é no-op: senão o cancel não devolve o cooldown do RE-publish
        /// (review 2 do 003, achado 2). O cancel só devolve cooldown se o stamp corrente ainda for o
        /// PublishDeadline guardado (re-ancorado não é apagado — review 1, achado 6). Enqueue INTERNO é RECUSADO
        /// com entrada NÃO-interna pendente do mesmo (player, kind) — a pendente já entrega a queda; evita o
        /// dedup sobrescrever Internal/refund (PA-01-03).</summary>
        private static readonly List<DeferredCrouch> _deferred = new List<DeferredCrouch>();
        private static readonly List<BotRestore> _botRestores = new List<BotRestore>();

        /// <summary>PA-01-06: fallback agachado da queda com prone pendente — re-tentado SÓ com a FSM do 004 em
        /// BLOQUEIO, cadência ≥0.5s (CanProne é SphereCast físico — MovementContext.cs:1181-1230); valor = próximo
        /// instante permitido. Limpo via ClearPronePending na transição p/ Released e no Disengage.</summary>
        private static readonly Dictionary<Player, float> _pronePending = new Dictionary<Player, float>();
        private static readonly List<Player> _proneScratch = new List<Player>();

        /// <summary>ref: item 020 — resíduo desta primitiva para a auditoria de fronteira de raid.</summary>
        internal static int ResidualCount => _deferred.Count + _botRestores.Count + _pronePending.Count;

        private static int _lastPumpFrame = -1; // PA-01-12: pump idempotente por FRAME, agnóstico ao chamador (003 e 004 chamam)

        private struct DeferredCrouch
        {
            internal Player Player;
            internal TraumaOneShotKind Kind;   // dedup por (player, kind) — primitiva compartilhada (006 reusa)
            internal TraumaRegion Region;
            internal TraumaLine RequiredLine;  // re-validação na execução: mudou → cancela
            internal float PublishDeadline;    // p/ ReportOneShotCanceled(player, kind, publishDeadline)
            internal bool Internal;            // 004: re-queda da janela — SEM refund no cancel (não houve publish — PA-01-03)
            internal Action<Player, bool> OnExecuted; // 004: callback de execução (fall) — bool = caiu prone (false = fallback agachado)
        }

        private struct BotRestore
        {
            internal Player Player;
            internal float RestoreAt;
        }

        // tarkin-ladders soft-dependency (guard de escada — P4 rec. (3c); string de tipo de terceiro)
        private static bool _ladderResolved;
        private static Type _ladderType;

        // SAIN soft-dependency (dip em combate — P6 rec. (7); null-check + no-op, padrão AggroHelper)
        private static bool _sainResolved;
        private static Type _sainBotComponentType;
        private static PropertyInfo _sainMoverProp;
        private static PropertyInfo _sainPoseProp;
        private static MethodInfo _sainSetTargetPose;

        // ref: spec 006 §1.10 — ganha a região p/ distinguir o agachar do estômago ("stomach-crouch") do de
        // pernas ("crouch") nos logs da fila/primitiva, sem alterar um byte dos formatos de pernas/queda
        // (region=Legs devolve exatamente os mesmos tokens de antes — diff de log vazio p/ kind=fall e Legs).
        private static string KindWord(TraumaOneShotKind kind, TraumaRegion region) =>
            kind == TraumaOneShotKind.InvoluntaryFall ? "fall"
                : region == TraumaRegion.Stomach ? "stomach-crouch" : "crouch";

        /// <summary>Guards D7 (3 eixos, TODOS adiam — nunca executam em contexto inválido).</summary>
        internal static bool CanForcePose(Player p, out string blockedBy)
        {
            var mc = p.MovementContext;
            // (a) vault/ar — ref: P4 rec. (3a); MovementContext.IsGrounded (:1089) + CurrentState.Name (:732)
            if (!mc.IsGrounded) { blockedBy = "airborne"; return false; }
            var cs = mc.CurrentState;
            if (cs != null)
            {
                EPlayerState st = cs.Name;
                if (st == EPlayerState.ClimbOver || st == EPlayerState.ClimbUp
                    || st == EPlayerState.VaultingFallDown || st == EPlayerState.VaultingLanding
                    || st == EPlayerState.Jump || st == EPlayerState.FallDown)
                {
                    blockedBy = st.ToString();
                    return false;
                }
            }
            // (b) BTR — ref: Player.cs:25413
            if (p.BtrState != EPlayerBtrState.Outside) { blockedBy = "btr"; return false; }
            // (c) escada — tarkin-ladders por reflection (vanilla não tem escada interativa; P4 correção D7)
            ResolveLadderType();
            if (_ladderType != null && p.GetComponent(_ladderType) != null) { blockedBy = "ladder"; return false; }
            blockedBy = null;
            return true;
        }

        /// <summary>Absorção D2 (spec 004 §1.8(a)) — chamada NO TOPO de TryInvoluntaryCrouch e BotCrouchDip (cobre
        /// 003/006 hoje): ciclo engajado (humano em qualquer fase OU bot em hold) → NÃO executa; refund
        /// do publish + log ABSORB — nunca descarte silencioso (funcional §4). Região SÓ p/ o log word (spec 006 §1.5).</summary>
        private static bool AbsorbIfCycleEngaged(Player p, TraumaOneShotKind kind, TraumaRegion region)
        {
            if (!TraumaFallCycleConsumer.IsCycleEngaged(p)) return false;
            if (TraumaEngine.TryGetOneShotDeadline(p, kind, out float d))
                TraumaEngine.ReportOneShotCanceled(p, kind, d);
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] {KindWord(kind, region)} ABSORB (fall-cycle) {p.ProfileId}");
            return true;
        }

        /// <summary>Agachar one-shot de HUMANO: pose já ≤ agachado/prone → NO-OP devolvendo o cooldown do publish
        /// (funcional §3 — "sem consumir cooldown"); guard falhou → ADIA; ok → SetPoseLevel(0f) sem force.</summary>
        internal static void TryInvoluntaryCrouch(Player p, TraumaRegion region, TraumaOneShotKind kind)
        {
            if (p == null || p.MovementContext == null)
            {
                // code-review 1 do 003, achado 4: contexto morto = one-shot nunca executará — refundar o
                // cooldown do publish (mesmo padrão do NOOP) antes de sair
                if (!(p is null) && TraumaEngine.TryGetOneShotDeadline(p, kind, out float dNull))
                    TraumaEngine.ReportOneShotCanceled(p, kind, dNull);
                return;
            }
            if (AbsorbIfCycleEngaged(p, kind, region)) return; // D2: ciclo de queda engajado absorve o agachar (spec 004 §1.8)
            var mc = p.MovementContext;
            if (mc.IsInPronePose || mc.PoseLevel <= 0.05f) // ref: MovementContext.cs:1016 — 0=agachado, 1=de pé
            {
                if (TraumaEngine.TryGetOneShotDeadline(p, kind, out float d0))
                    TraumaEngine.ReportOneShotCanceled(p, kind, d0);
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] {KindWord(kind, region)} NOOP (pose already low) {p.ProfileId}");
                return;
            }
            if (!CanForcePose(p, out string blockedBy))
            {
                Defer(p, region, kind, blockedBy, internalEntry: false, onExecuted: null);
                return;
            }
            if (!mc.SetPoseLevel(0f)) // ref: MovementContext.cs:2139 — sem force: animação vanilla; guard interno pode recusar
            {
                Defer(p, region, kind, "internal-guard", internalEntry: false, onExecuted: null);
                return;
            }
            TraumaEngine.ReportOneShotExecuted(p, kind); // D7 — cooldown conta da EXECUÇÃO
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] {KindWord(kind, region)} EXECUTED {p.ProfileId}");
        }

        /// <summary>Derrubar forçado do ciclo 004 (P4 rec. (2)): guards D7 → prone com readback → fallback agachado.
        /// internalFall=true (re-queda da janela) NÃO refunda cooldown ao cancelar (não houve publish — PA-01-03).
        /// onExecuted(p, caiuProne) é chamado na execução (imediata ou do pump); adiamento NÃO chama.</summary>
        internal static void TryInvoluntaryFall(Player p, TraumaRegion region, TraumaOneShotKind kind,
            bool internalFall, Action<Player, bool> onExecuted)
        {
            if (p == null || p.MovementContext == null)
            {
                // contexto morto = one-shot nunca executará — refund do publish (padrão code-review 1 do 003, achado 4)
                if (!internalFall && !(p is null) && TraumaEngine.TryGetOneShotDeadline(p, kind, out float dNull))
                    TraumaEngine.ReportOneShotCanceled(p, kind, dNull);
                return;
            }
            if (!CanForcePose(p, out string blockedBy))
            {
                Defer(p, region, kind, blockedBy, internalFall, onExecuted); // D7 adia (fase FallPending no consumidor)
                return;
            }
            ExecuteFall(p, kind, internalFall, onExecuted);
        }

        /// <summary>Passos 2-5 do stub (spec 004 §5): já prone → callback sem tocar pose; senão prone force +
        /// readback obrigatório (setter recusa SILENCIOSO quando CanProne falha — MovementContext.cs:714-717,
        /// :1181-1230) → fallback agachado + PronePending. Cooldown re-ancorado na EXECUÇÃO (D7) quando não-interno.</summary>
        private static void ExecuteFall(Player p, TraumaOneShotKind kind, bool internalFall, Action<Player, bool> onExecuted)
        {
            var mc = p.MovementContext;
            if (mc.IsInPronePose)
            {
                // já no chão — paridade "já prone → só BLOQUEIO" (funcional item 5): sem tocar pose
                if (!internalFall) TraumaEngine.ReportOneShotExecuted(p, kind); // ref: TraumaEngine.cs:117
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] fall EXECUTED (already-prone) {p.ProfileId}");
                onExecuted?.Invoke(p, true);
                return;
            }
            mc.SetPoseLevel(0f, true);   // ref: MovementContext.cs:2139 (descer sempre passa no CanStandAt)
            mc.IsInPronePose = true;     // ref: MovementContext.cs:676
            if (!mc.IsInPronePose)       // READBACK: recusa silenciosa do CanProne (ladeira>30°/sem espaço/UsingMeds)
            {
                mc.SetPoseLevel(0f);     // fallback AGACHADO (funcional §1 — bloqueio vale p/ a pose corrente)
                MarkPronePending(p);     // re-tentativa de prone no pump (PA-01-06)
                if (!internalFall) TraumaEngine.ReportOneShotExecuted(p, kind);
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] fall FALLBACK-CROUCH {p.ProfileId}");
                onExecuted?.Invoke(p, false);
                return;
            }
            if (!internalFall) TraumaEngine.ReportOneShotExecuted(p, kind); // D7: cooldown conta da execução
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] fall EXECUTED {p.ProfileId}");
            onExecuted?.Invoke(p, true);
        }

        private static void Defer(Player p, TraumaRegion region, TraumaOneShotKind kind, string reason,
            bool internalEntry, Action<Player, bool> onExecuted)
        {
            float deadline = 0f;
            if (!internalEntry) TraumaEngine.TryGetOneShotDeadline(p, kind, out deadline);
            TraumaLine required = TraumaEngine.GetLine(p, region);
            for (int i = 0; i < _deferred.Count; i++)
            {
                DeferredCrouch e = _deferred[i];
                // spec 006 §1.5: e.Region == region entra na chave de dedup — fecha o corner da funcional §4
                // (um adiado de PERNAS que recebia por cima a intenção do ESTÔMAGO virava UMA entrada
                // re-alvejada; agora as duas coexistem, cada uma re-validando a PRÓPRIA linha no pump).
                if (ReferenceEquals(e.Player, p) && e.Kind == kind && e.Region == region)
                {
                    // PA-01-03: enqueue INTERNO recusado com entrada NÃO-interna pendente — a pendente já
                    // entrega a queda; sobrescrever mataria o refund do publish original
                    if (internalEntry && !e.Internal)
                    {
                        TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                            $"[Trauma2] {KindWord(kind, region)} DEFER-SKIP (non-internal pending) {p.ProfileId}");
                        return;
                    }
                    // review 2, achado 2: hit de dedup ATUALIZA a entrada (re-publish traz stamp/linha novos)
                    // code-review 2, achado 3 / spec 006 §1.5: e.Region = region agora é NO-OP inofensivo (a
                    // região já é parte da chave de match) — mantido p/ o caso re-publish da MESMA região
                    // atualizar PublishDeadline/RequiredLine junto.
                    e.PublishDeadline = deadline;
                    e.Region = region;
                    e.RequiredLine = required;
                    e.Internal = internalEntry;
                    e.OnExecuted = onExecuted ?? e.OnExecuted;
                    _deferred[i] = e;
                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] {KindWord(kind, region)} DEFERRED ({reason}) {p.ProfileId}");
                    return;
                }
            }
            _deferred.Add(new DeferredCrouch
            {
                Player = p, Kind = kind, Region = region, RequiredLine = required,
                PublishDeadline = deadline, Internal = internalEntry, OnExecuted = onExecuted
            });
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] {KindWord(kind, region)} DEFERRED ({reason}) {p.ProfileId}");
        }

        /// <summary>Pump 1×/frame — IDEMPOTENTE por frame e agnóstico ao chamador (003 e 004 chamam — PA-01-12).
        /// Re-checa guards; contexto válido → RE-VALIDA o snapshot (GetLine == RequiredLine) — mudou (curado/
        /// analgésico) → CANCELA devolvendo cooldown do publish SÓ se não-interna. Dispatch por kind: crouch =
        /// SetPoseLevel(0f); fall = ExecuteFall (prone+readback+fallback). Também re-tenta o prone pendente do
        /// fallback agachado (PA-01-06).</summary>
        internal static void PumpDeferred()
        {
            if (Time.frameCount == _lastPumpFrame) return;
            _lastPumpFrame = Time.frameCount;

            for (int i = _deferred.Count - 1; i >= 0; i--)
            {
                DeferredCrouch e = _deferred[i];
                Player p = e.Player;
                if (p is null || p.MovementContext == null || TraumaEngine.GetLine(p, e.Region) != e.RequiredLine)
                {
                    _deferred.RemoveAt(i);
                    if (!e.Internal && !(p is null)) TraumaEngine.ReportOneShotCanceled(p, e.Kind, e.PublishDeadline);
                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                        $"[Trauma2] {KindWord(e.Kind, e.Region)} CANCELED (state-changed) {(p is null ? "?" : p.ProfileId)}");
                    continue;
                }
                var mc = p.MovementContext;
                if (e.Kind == TraumaOneShotKind.InvoluntaryFall)
                {
                    if (!CanForcePose(p, out _)) continue;   // segue adiado (D7)
                    _deferred.RemoveAt(i);
                    ExecuteFall(p, e.Kind, e.Internal, e.OnExecuted); // já prone voluntário → executa como already-prone (BLOQUEIO)
                    continue;
                }
                if (mc.IsInPronePose || mc.PoseLevel <= 0.05f)
                {
                    // agachou/deitou por conta própria enquanto adiado — intent satisfeita sem forçar (só-para-baixo)
                    _deferred.RemoveAt(i);
                    TraumaEngine.ReportOneShotCanceled(p, e.Kind, e.PublishDeadline);
                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] {KindWord(e.Kind, e.Region)} NOOP (pose already low) {p.ProfileId}");
                    continue;
                }
                if (!CanForcePose(p, out _)) continue;   // segue adiado
                if (!mc.SetPoseLevel(0f)) continue;      // guard interno ainda recusa — segue adiado
                _deferred.RemoveAt(i);
                TraumaEngine.ReportOneShotExecuted(p, e.Kind); // cooldown conta da execução (D7)
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] {KindWord(e.Kind, e.Region)} EXECUTED {p.ProfileId}");
            }

            PumpPronePending();
        }

        // ---- Prone pendente do fallback agachado (PA-01-06) ----

        internal static void MarkPronePending(Player p)
        {
            if (p is null) return;
            _pronePending[p] = Time.time + 0.5f; // cadência ≥0.5s a partir da ENTRADA (CanProne é SphereCast físico)
        }

        internal static void ClearPronePending(Player p)
        {
            if (p is null) return;
            _pronePending.Remove(p);
        }

        private static void PumpPronePending()
        {
            if (_pronePending.Count == 0) return;
            _proneScratch.Clear();
            foreach (KeyValuePair<Player, float> kv in _pronePending) _proneScratch.Add(kv.Key);
            for (int i = 0; i < _proneScratch.Count; i++)
            {
                Player p = _proneScratch[i];
                if (p is null || p == null || p.MovementContext == null) { _pronePending.Remove(p); continue; }
                var mc = p.MovementContext;
                if (mc.IsInPronePose) { _pronePending.Remove(p); continue; }             // prone alcançado por outro caminho
                if (!TraumaFallCycleConsumer.IsBlockedPhase(p)) continue;                 // re-tenta SÓ em BLOQUEIO (limpeza é do consumidor)
                if (Time.time < _pronePending[p]) continue;                               // cadência ≥0.5s
                _pronePending[p] = Time.time + 0.5f;
                // Reentry flag: os writes do próprio mod atravessam o prefix do CanStandAt durante o BLOQUEIO
                TraumaFallCycleConsumer.StandReentryFlag = true;
                try
                {
                    mc.SetPoseLevel(0f, true);
                    mc.IsInPronePose = true; // readback: CanProne ainda pode recusar (silencioso)
                }
                finally { TraumaFallCycleConsumer.StandReentryFlag = false; }
                if (mc.IsInPronePose)
                {
                    _pronePending.Remove(p);
                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] fall prone RETRY ok {p.ProfileId}");
                }
            }
            _proneScratch.Clear();
        }

        // ---- Cancelamentos ----

        /// <summary>Cancela todos os adiados (fim de raid / world-swap) devolvendo o cooldown do publish das
        /// entradas NÃO-internas. Dupla chamada 003+004 é idempotente (fila vazia na segunda).</summary>
        internal static void CancelAll(string reason)
        {
            if (_deferred.Count == 0) { _pronePending.Clear(); return; }
            for (int i = 0; i < _deferred.Count; i++)
            {
                DeferredCrouch e = _deferred[i];
                if (!e.Internal && !(e.Player is null)) TraumaEngine.ReportOneShotCanceled(e.Player, e.Kind, e.PublishDeadline);
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                    $"[Trauma2] {KindWord(e.Kind, e.Region)} CANCELED ({reason}) {(e.Player is null ? "?" : e.Player.ProfileId)}");
            }
            _deferred.Clear();
            _pronePending.Clear();
        }

        /// <summary>PA-01-04 (estendido no 006 — spec 006 §2): cancela SÓ (kind, região) — toggle-off do 003 usa
        /// CancelKind(InvoluntaryCrouch, Legs) e do 006 usa CancelKind(InvoluntaryCrouch, Stomach); nenhum varre
        /// o outro nem as quedas do 004 (InvoluntaryFall, Legs). Refund só de entradas não-internas.</summary>
        internal static void CancelKind(TraumaOneShotKind kind, TraumaRegion region, string reason)
        {
            for (int i = _deferred.Count - 1; i >= 0; i--)
            {
                DeferredCrouch e = _deferred[i];
                if (e.Kind != kind || e.Region != region) continue;
                _deferred.RemoveAt(i);
                if (!e.Internal && !(e.Player is null)) TraumaEngine.ReportOneShotCanceled(e.Player, e.Kind, e.PublishDeadline);
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                    $"[Trauma2] {KindWord(e.Kind, e.Region)} CANCELED ({reason}) {(e.Player is null ? "?" : e.Player.ProfileId)}");
            }
        }

        /// <summary>Cancela as quedas adiadas DESTE jogador (toggle-off do 004 / cura / ENTRADA DA PAUSA — PA-02-03:
        /// queda pendente nunca executa com a FSM em Paused). Refund só se não-interna.</summary>
        internal static void CancelFallsFor(Player p, string reason)
        {
            if (p is null) return;
            for (int i = _deferred.Count - 1; i >= 0; i--)
            {
                DeferredCrouch e = _deferred[i];
                if (!ReferenceEquals(e.Player, p) || e.Kind != TraumaOneShotKind.InvoluntaryFall) continue;
                _deferred.RemoveAt(i);
                if (!e.Internal) TraumaEngine.ReportOneShotCanceled(p, e.Kind, e.PublishDeadline);
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] fall CANCELED ({reason}) {p.ProfileId}");
            }
        }

        /// <summary>Dip de BOT (P6 rec. (7)) — FIRE-AND-FORGET, nunca entra na fila de adiados (review 1, achado 4):
        /// devolução imediata de controle (decisão 16) com restauração própria fora de combate. `region` é NOVO e
        /// opcional (spec 006 §2 — default Legs mantém o call site do 003 intocado); usado só p/ o log word ABSORB
        /// (logs "bot dip ..." ficam com formato inalterado — spec 006 §1.10 abertura 4).</summary>
        internal static void BotCrouchDip(Player botPlayer, TraumaRegion region = TraumaRegion.Legs)
        {
            if (botPlayer == null || botPlayer.MovementContext == null)
            {
                // code-review 2, achado 2: contexto morto = dip nunca executará — refundar o cooldown do
                // publish (mesmo padrão do achado 4 da review 1 em TryInvoluntaryCrouch)
                if (!(botPlayer is null) && TraumaEngine.TryGetOneShotDeadline(botPlayer, TraumaOneShotKind.InvoluntaryCrouch, out float dNull))
                    TraumaEngine.ReportOneShotCanceled(botPlayer, TraumaOneShotKind.InvoluntaryCrouch, dNull);
                return;
            }
            if (AbsorbIfCycleEngaged(botPlayer, TraumaOneShotKind.InvoluntaryCrouch, region)) return; // D2: bot em hold do ciclo (spec 004 §1.8)
            BotOwner bo = botPlayer.AIData?.BotOwner;
            if (bo == null)
            {
                if (TraumaEngine.TryGetOneShotDeadline(botPlayer, TraumaOneShotKind.InvoluntaryCrouch, out float dBo))
                    TraumaEngine.ReportOneShotCanceled(botPlayer, TraumaOneShotKind.InvoluntaryCrouch, dBo);
                return;
            }
            var mcBot = botPlayer.MovementContext;
            if (mcBot.IsInPronePose || mcBot.PoseLevel <= 0.05f)
            {
                // code-review 1 do 003, achado 3: só-para-baixo vale p/ bot também — pose já baixa é NOOP,
                // devolve o cooldown do publish e NÃO agenda restore (não há dip a desfazer)
                if (TraumaEngine.TryGetOneShotDeadline(botPlayer, TraumaOneShotKind.InvoluntaryCrouch, out float d0))
                    TraumaEngine.ReportOneShotCanceled(botPlayer, TraumaOneShotKind.InvoluntaryCrouch, d0);
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] bot dip NOOP (pose already low) {botPlayer.ProfileId}");
                return;
            }
            bool sain = TrySainSetTargetPose(botPlayer, 0f);      // em combate o SAIN dirige a pose — target via reflection
            bo.SetPose(0f);                                        // ref: BotOwner.cs:1120 — target de pose da IA vanilla
            botPlayer.MovementContext.SetPoseLevel(0f, true);      // dip imediato no dono (host/headless)
            float dip = Mathf.Clamp(TRLImmersiveCombatMedicinePlugin.ConfigBotCrouchDipSeconds.Value, 0.3f, 1.5f);
            _botRestores.Add(new BotRestore { Player = botPlayer, RestoreAt = Time.time + dip });
            TraumaEngine.ReportOneShotExecuted(botPlayer, TraumaOneShotKind.InvoluntaryCrouch); // cooldown vale p/ bot
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                $"[Trauma2] bot dip {botPlayer.ProfileId} mode={(sain ? "sain" : "vanilla")}");
        }

        /// <summary>Restauração do dip (fora de combate; em combate SAIN reescreve antes). Pump pelo consumidor.</summary>
        internal static void PumpBotRestores()
        {
            for (int i = _botRestores.Count - 1; i >= 0; i--)
            {
                BotRestore r = _botRestores[i];
                if (Time.time < r.RestoreAt) continue;
                _botRestores.RemoveAt(i);
                RestoreBot(r.Player);
            }
        }

        /// <summary>Restaura imediatamente todos os dips pendentes (toggle-off).</summary>
        internal static void FlushBotRestores()
        {
            for (int i = _botRestores.Count - 1; i >= 0; i--) RestoreBot(_botRestores[i].Player);
            _botRestores.Clear();
        }

        internal static void ClearBotRestores()
        {
            _botRestores.Clear(); // mundo morto — sem restore (objetos destruídos)
        }

        private static void RestoreBot(Player p)
        {
            if (p is null || p == null || p.MovementContext == null) return; // gerenciado E fake-null
            if (p.HealthController == null || !p.HealthController.IsAlive) return;
            p.AIData?.BotOwner?.SetPose(1f); // devolução — SAIN/BotMover re-decidem em seguida (decisão 16)
            TrySainSetTargetPose(p, 1f);
        }

        private static void ResolveLadderType()
        {
            if (_ladderResolved) return;
            _ladderResolved = true;
            _ladderType = Type.GetType("tarkin.ladders.bep.PlayerLadderController, tarkin.ladders.bep"); // ref: P4 rec. (3c)
            if (_ladderType != null) return;
            // P4 risco 3: warn quando o mod está na load order e a string de tipo quebrou (update do tarkin-ladders).
            // code-review 1 do 003, achado 5: match restrito a "ladders" no GUID — o OR "tarkin" dava
            // falso-positivo com outros mods do mesmo autor.
            foreach (var kv in BepInEx.Bootstrap.Chainloader.PluginInfos)
            {
                string key = kv.Key ?? string.Empty;
                if (key.IndexOf("ladders", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning(
                        "[Trauma2] tarkin-ladders presente mas PlayerLadderController não resolveu — guard de escada INATIVO");
                    break;
                }
            }
        }

        private static bool TrySainSetTargetPose(Player p, float pose)
        {
            try
            {
                ResolveSain();
                if (_sainBotComponentType == null || _sainSetTargetPose == null) return false;
                Component comp = p.gameObject.GetComponent(_sainBotComponentType);
                if (comp == null) return false;
                object mover = _sainMoverProp?.GetValue(comp);
                object poseObj = mover != null ? _sainPoseProp?.GetValue(mover) : null;
                if (poseObj == null) return false;
                _sainSetTargetPose.Invoke(poseObj, new object[] { pose });
                return true;
            }
            catch
            {
                return false; // soft-dep: qualquer quebra de shape do SAIN vira no-op silencioso (padrão AggroHelper)
            }
        }

        private static void ResolveSain()
        {
            if (_sainResolved) return;
            _sainResolved = true;
            // ref: P6 rec. (7) — nomes estáveis no SAIN 4.4.3 instalado; major update → no-op (risco 7 do P6)
            _sainBotComponentType = AccessTools.TypeByName("SAIN.Components.BotComponent");
            if (_sainBotComponentType == null) return;
            _sainMoverProp = AccessTools.Property(_sainBotComponentType, "Mover");
            Type moverType = _sainMoverProp?.PropertyType;
            _sainPoseProp = moverType != null ? AccessTools.Property(moverType, "Pose") : null;
            Type poseType = _sainPoseProp?.PropertyType;
            _sainSetTargetPose = poseType != null ? AccessTools.Method(poseType, "SetTargetPose") : null;
        }
    }
}
