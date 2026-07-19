using Comfort.Common;
using EFT;
using TrueTrauma; // TraumaState (blackout legado)
using UnityEngine;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Consumidor do ciclo de queda (item 004). FSM 3 fases no DONO humano local; bots via TraumaBotFall.
    /// Motor intocado: InvoluntaryFall já publicado pela linha Cair (TraumaEngine.cs:572, cooldown por kind :27).
    /// Dono-only herdado do motor (D16 — humanos peers rodam o próprio ciclo no cliente deles).</summary>
    public sealed class TraumaFallCycleConsumer : MonoBehaviour
    {
        private enum FallPhase { None, FallPending, Blocked, Released, Rising, Window, Paused } // FallPending: queda PENDENTE na fila — de ENTRADA ou RE-QUEDA da janela (PA-01-03/PA-02-04) — sem timers/cap/negação

        private static TraumaFallCycleConsumer _instance;

        // FSM só do humano LOCAL (peers rodam o próprio ciclo no cliente deles — D16); bots no TraumaBotFall.
        private Player _local;                  // dono humano com ciclo ativo (null = sem ciclo)
        private FallPhase _phase;
        private float _phaseDeadline;           // deadline ABSOLUTO — timers F12 valem na PRÓXIMA fase (AC1)
        private bool _capApplied;               // cap N2 da JANELA (TraumaSpeedCap, causa 1001)
        private bool _releasedFromProne;        // pose no início da LIBERAÇÃO — decisão de levantar lida por POSE (PA-01-05)
        internal const float SlowRiseSeconds = 1.5f; // const interna (decisão 13 cobre só os 3 timers — premissa p/ 011)
        internal static bool StandReentryFlag;  // deixa os SetPoseLevel do próprio mod passarem pelo CanStandAt

        private bool _wasActive;
        private GameWorld _trackedWorld;        // padrão 003: world-swap/transit + null-detect

        private static readonly TraumaRegion[] LegsRegions = { TraumaRegion.Legs };

        private void Awake()
        {
            _instance = this;
            TraumaConsumerRegistry.Register(TraumaConsumerId.FallCycle, LegsRegions, IsActive); // toast (decisão 20; texto LegsFall já existe — TraumaLocale.cs:18)
            TraumaEngine.SubscribeWithSnapshot(OnTransition);   // replay establishing — ref: TraumaEngine.cs:72
            TraumaEngine.OneShotPublished += OnOneShot;         // ref: TraumaEngine.cs:22 (cooldown-gated)
        }

        /// <summary>Master legado + master Trauma 2.0 + toggle próprio (comportamento 9 do 002).</summary>
        internal static bool IsActive()
        {
            return TRLImmersiveCombatMedicinePlugin.ConfigMasterEnabled.Value
                && TRLImmersiveCombatMedicinePlugin.ConfigTrauma2Enabled.Value
                && TRLImmersiveCombatMedicinePlugin.ConfigConsumerFallCycle.Value;
        }

        /// <summary>Arbitragem D2 (consumida por TraumaPose e HealthPatches): ciclo engajado p/ este player (humano
        /// em qualquer fase ativa, ou bot em hold) → agachar involuntário/legado é ABSORVIDO/suprimido.</summary>
        internal static bool IsCycleEngaged(Player p)
        {
            TraumaFallCycleConsumer inst = _instance;
            if (inst == null || p is null) return false;
            if (p.IsAI) return TraumaBotFall.IsHeld(p.ProfileId);
            return ReferenceEquals(p, inst._local) && inst._phase != FallPhase.None;
        }

        /// <summary>Consumida pelo CantStandUpPatch (negação na origem) e pelo FallAttemptCommandPatch (som).</summary>
        internal static bool IsBlockedPhase(Player p)
        {
            TraumaFallCycleConsumer inst = _instance;
            return inst != null && ReferenceEquals(p, inst._local) && inst._phase == FallPhase.Blocked;
        }

        /// <summary>CR-02-02: fonte ÚNICA do predicado de pausa (D3 blackout legado + DOWNED Fika via !IsAlive
        /// com record vivo) — consumida pelo TickHumanCycle e pelo cinto do OnFallExecuted (CR-01-02). O subset
        /// D3 do OnOneShotCore fica FORA de propósito (é menor: o motor não publica p/ !IsAlive —
        /// TraumaEngine.cs:511-513).</summary>
        private static bool IsPauseCondition(Player p)
        {
            return TraumaState.BlackoutTimers.ContainsKey(p.ProfileId) || TraumaState.IsFainted
                || p.HealthController == null || !p.HealthController.IsAlive;
        }

        private void OnTransition(TraumaTransition t)
        {
            // CR-01-04: exceção de consumidor não pode subir p/ o StateChanged?.Invoke do motor (barramento de
            // TODOS os consumidores — mataria a publicação das regiões/records restantes do frame).
            try { OnTransitionCore(t); }
            catch (System.Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogError($"[Trauma2] FallCycle.OnTransition: {ex.Message}");
            }
        }

        private void OnTransitionCore(TraumaTransition t)
        {
            if (t.Region != TraumaRegion.Legs || !IsActive()) return;
            Player p = t.Player;
            if (p is null) return;
            if (p.IsAI) { TraumaBotFall.OnLine(p, t.To); return; } // caminho ÚNICO idempotente de entrada de bot (PA-02-06)
            if (!p.IsYourPlayer) return;

            if (t.To == TraumaLine.LegsFallCycle)
            {
                // ORDEM REAL: StateChanged dispara ANTES de OneShotPublished (TraumaEngine.cs:565 vs :571-572) —
                // NÃO engajar às cegas (PA-01-03). Establishing (spawn ferido/religar/adoção) = JANELA sem
                // one-shot/toast; já prone → BLOQUEIO (item 5 da funcional). Sem establishing: cooldown ativo
                // (deadline futuro) ⇒ o publish será SUPRIMIDO ⇒ engaja já (corner do thrash de analgésico);
                // senão o OnOneShot do MESMO frame conduz (executa → BLOQUEIO; D7 adia → FallPending).
                // Troca de linha DENTRO do ciclo não passa aqui (From==To não publica — TraumaEngine.cs:548).
                if (_phase != FallPhase.None) return;
                if (t.Establishing) { Engage(p, establishing: true); return; }
                if (TraumaEngine.TryGetOneShotDeadline(p, TraumaOneShotKind.InvoluntaryFall, out float cd) && cd > Time.time)
                    Engage(p, establishing: false); // one-shot NÃO virá — engaja (JANELA; prone → BLOQUEIO)
                // senão: aguarda o OnOneShot deste frame — nenhum estado criado aqui
            }
            else if (ReferenceEquals(p, _local))
            {
                // Saída da linha Cair (cura/analgésico/None): encerra NA HORA (decisão 1) — destrava levantar
                // lento em andamento, cancela adiados (refund), remove cap. 003 assume a linha nova (N1/N2).
                Disengage("line-exit");
            }
        }

        private void OnOneShot(Player p, TraumaOneShotKind kind, TraumaLine line)
        {
            // CR-01-04: mesmo isolamento do OnTransition (OneShotPublished?.Invoke vive no EvaluatePlayer do motor)
            try { OnOneShotCore(p, kind, line); }
            catch (System.Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogError($"[Trauma2] FallCycle.OnOneShot: {ex.Message}");
            }
        }

        private void OnOneShotCore(Player p, TraumaOneShotKind kind, TraumaLine line)
        {
            if (!IsActive() || kind != TraumaOneShotKind.InvoluntaryFall) return;
            if (p is null) return;
            if (p.IsAI) { TraumaBotFall.OnFallOneShot(p); return; } // held → só re-ancora cooldown (PA-02-06)
            if (!p.IsYourPlayer) return;

            // D3: Cair + desmaio no MESMO evento → desmaio vence; derrubar absorvido COM refund (nunca silencioso).
            if (TraumaState.BlackoutTimers.ContainsKey(p.ProfileId) || TraumaState.IsFainted)
            {
                if (TraumaEngine.TryGetOneShotDeadline(p, kind, out float d))
                    TraumaEngine.ReportOneShotCanceled(p, kind, d); // ref: TraumaEngine.cs:127
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] fall ABSORB (blackout) {p.ProfileId}");
                return; // wake re-avalia e entra em BLOQUEIO (já prone — blackout força prone por frame)
            }
            // Derrubar de entrada: fase FallPending ATÉ o callback (D7 pode ADIAR — escada/BTR/vault; PA-01-03):
            // sem timers, sem cap, negação OFF; encerrada por OnFallExecuted (→ BLOQUEIO), cancel ou saída da linha.
            _local = p;
            _phase = FallPhase.FallPending;
            TraumaPose.TryInvoluntaryFall(p, TraumaRegion.Legs, kind, internalFall: false, OnFallExecuted);
        }

        private void OnFallExecuted(Player p, bool fellProne)
        {
            // Chamado pelo TraumaPose na execução (imediata ou do pump). fellProne=false = fallback agachado
            // (PronePending re-tenta no pump; bloqueio vale p/ a pose corrente — funcional §1).
            // Cinto PA-02-03 + CR-01-02: além da fase Paused, checa o predicado de desmaio/downed DIRETO (helper
            // único IsPauseCondition — CR-02-02) — no frame de ENTRADA do blackout o pump do 003 roda ANTES do
            // tick do 004 (ordem de AddComponent) e a queda adiada executaria como already-prone com a FSM ainda
            // em FallPending → EnterBlocked + OnAgony de um inconsciente (replicado aos peers). Sem fase, sem
            // voz — o wake conduz (já prone).
            if (_phase == FallPhase.Paused || IsPauseCondition(p)) return;
            _local = p;
            EnterBlocked("fall-executed");
            TraumaVoice.PlayStrong(p); // OnAgony importance:100 — ref: PhraseSpeakerClass.cs:175
        }

        private void Engage(Player p, bool establishing)
        {
            _local = p;
            if (p.MovementContext != null && p.MovementContext.IsInPronePose) EnterBlocked(establishing ? "established-prone" : "engage-prone");
            else EnterWindow(establishing ? "established" : "engage-standing"); // sem one-shot, sem toast
        }

        private void EnterBlocked(string reason)
        {
            _phase = FallPhase.Blocked;
            _phaseDeadline = Time.time + Mathf.Clamp(TRLImmersiveCombatMedicinePlugin.ConfigFallBlockSeconds.Value, 5f, 60f);
            RemoveWindowCap();
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] fall cycle phase=Blocked reason={reason} {(_local is null ? "?" : _local.ProfileId)}");
        }

        private void EnterWindow(string reason)
        {
            _phase = FallPhase.Window;
            _phaseDeadline = Time.time + Mathf.Clamp(TRLImmersiveCombatMedicinePlugin.ConfigFallWindowSeconds.Value, 1f, 10f);
            ApplyWindowCap(); // cap N2 do 004 (independente do toggle 003) — TraumaSpeedCap causa 1001
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] fall cycle phase=Window reason={reason} {(_local is null ? "?" : _local.ProfileId)}");
        }

        private void ApplyWindowCap()
        {
            // Causa PRÓPRIA (ESpeedLimit)1001 (PA-01-01), Remove+Add + log de calibração + EnableSprint(false)
            // incondicional (PA-02-09) — coexiste com a causa 1000 do 003 pela min-composição nativa (method_4 :1798).
            TraumaSpeedCap.Apply(_local);
            _capApplied = true;
        }

        private void RemoveWindowCap()
        {
            if (_capApplied)
            {
                TraumaSpeedCap.RemoveGuarded(_local); // remove SÓ a causa 1001 (downed-safe, 003 CR)
                _capApplied = false;
            }
        }

        private void Disengage(string reason)
        {
            RemoveWindowCap();
            TraumaPose.CancelFallsFor(_local, reason);   // adiados de queda: refund quando não-internos
            TraumaPose.ClearPronePending(_local);        // PA-01-06: pendência de prone morre com o ciclo
            StandReentryFlag = false;
            _releasedFromProne = false;
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] fall cycle END ({reason}) {(_local is null ? "?" : _local.ProfileId)}");
            _phase = FallPhase.None;
            _local = null;
            // levantar destravado NA HORA (decisão 1) — IsBlockedPhase vira false com a fase
        }

        private void Update()
        {
            GameWorld gw = Singleton<GameWorld>.Instance;
            if (gw == null)
            {
                // padrão N1/003: mundo morreu — só bookkeeping (pose/caps morrem com o mundo)
                if (_phase != FallPhase.None) Disengage("raid-end");
                TraumaBotFall.ClearAll();
                TraumaVoice.Clear();     // PA-02-08: ponto de limpeza do anti-spam (ProfileIds da raid morta)
                TraumaSpeedCap.Clear();  // cinto: bookkeeping estático não atravessa raids (skill csharp §2)
                _trackedWorld = null; _wasActive = IsActive();
                return;
            }
            if (!ReferenceEquals(gw, _trackedWorld))
            {
                if (_phase != FallPhase.None) Disengage("world-swap"); // transit — espelha detecção do motor/003
                TraumaBotFall.ClearAll();
                TraumaVoice.Clear(); // PA-02-08
                TraumaSpeedCap.Clear();
                _trackedWorld = gw;
            }

            bool active = IsActive();
            if (_wasActive && !active)
            {
                // Toggle OFF mid-ciclo: prone deixa de ser forçado NA HORA, agendamentos cancelados, bots liberados
                if (_phase != FallPhase.None) Disengage("toggle-off");
                TraumaBotFall.ReleaseAll("toggle-off");
            }
            else if (!_wasActive && active)
            {
                // Religar = avaliação estabelecedora (JANELA; prone → BLOQUEIO) a partir do snapshot do motor
                Player mp = gw.MainPlayer;
                if (mp != null && TraumaEngine.IsOwnedHere(mp)
                    && TraumaEngine.GetLine(mp, TraumaRegion.Legs) == TraumaLine.LegsFallCycle)
                    Engage(mp, establishing: true);
                TraumaBotFall.EstablishFromSnapshot(gw); // bots com linha Cair → hold estabelecedor
            }
            _wasActive = active;
            if (!active) return;

            TickHumanCycle();
            TraumaPose.PumpDeferred();      // adiados D7 (crouch 003 + fall 004) + re-tentativa de prone do fallback
            TraumaBotFall.Pump();           // re-holds / releases / sweeps de bot
        }

        private void TickHumanCycle()
        {
            if (_phase == FallPhase.None) return;
            Player p = _local;
            // Poda: motor não rastreia mais / linha saiu com toggle off no meio (padrão sweep 003)
            if (p is null || p.MovementContext == null || TraumaEngine.GetLine(p, TraumaRegion.Legs) != TraumaLine.LegsFallCycle)
            { Disengage("stale"); return; }

            // D3/DOWNED: blackout legado OU downed Fika (!IsAlive com record vivo — contrato downed-safe do 003);
            // fonte única do predicado no helper IsPauseCondition (CR-02-02)
            bool paused = IsPauseCondition(p);
            if (paused)
            {
                if (_phase != FallPhase.Paused)
                {
                    RemoveWindowCap(); StandReentryFlag = false;
                    TraumaPose.CancelFallsFor(p, "paused"); // PA-02-03: queda PENDENTE não executa em PAUSED —
                    //   cancela com refund (não-internas; mesmo tratamento do fall ABSORB blackout, §1.8(c));
                    //   wake re-avalia já prone → BLOQUEIO, sem depender da entrada cancelada
                    _phase = FallPhase.Paused;
                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] fall cycle PAUSED (blackout|downed) {p.ProfileId}");
                    // nada escreve pose aqui (blackout já força prone por frame no MainLoopPatch :35-39 — sem double-writer)
                }
                return;
            }
            if (_phase == FallPhase.Paused)
            {
                // wake/revive: re-avalia snapshot — linha persiste → BLOQUEIO REINICIADO (acordou ≠ pronto p/ levantar)
                EnterBlocked("resume");
                return;
            }

            var mc = p.MovementContext;
            switch (_phase)
            {
                case FallPhase.FallPending:
                    break; // passivo (PA-01-03): poda stale/pausa acima cobrem; callback do pump/cancel encerram
                case FallPhase.Blocked:
                    if (Time.time >= _phaseDeadline)
                    {
                        _releasedFromProne = mc.IsInPronePose;  // decisão de levantar será lida por POSE (PA-01-05)
                        TraumaPose.ClearPronePending(p);        // fallback: bloqueio acabou — levanta de onde está (PA-01-06)
                        _phase = FallPhase.Released; // estável: sem timers até o jogador DECIDIR levantar
                        TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] fall cycle phase=Released {p.ProfileId}");
                    }
                    break;
                case FallPhase.Released:
                    // Decisão de levantar por POSE (PA-01-05): bloqueio em prone → saiu de prone; bloqueio em
                    // fallback agachado → PoseLevel SUBIU (>0.05f — só possível com a negação OFF, fim do BLOQUEIO).
                    // Sem decisão = LIBERAÇÃO estável (funcional §2) — nunca auto-Rising no fallback.
                    bool wantsUp = _releasedFromProne ? !mc.IsInPronePose : mc.PoseLevel > 0.05f;
                    if (wantsUp)
                    {
                        StandReentryFlag = true;
                        if (_releasedFromProne) mc.SetPoseLevel(0f, true); // get-up termina agachado — ref: MovementContext.cs:2139
                        _phase = FallPhase.Rising;          // rampa parte da POSE REAL corrente (PA-01-07)
                        TraumaVoice.PlayLight(p);           // OnBeingHurt demand:true — ref: Player.cs:28799
                        TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] fall cycle phase=Rising {p.ProfileId}");
                    }
                    break;
                case FallPhase.Rising:
                    // Rampa sobre a POSE REAL com readback (PA-01-07): SetPoseLevel recusa sob teto baixo
                    // (CanStandAt vanilla :2149) → a rampa ESTACIONA e retoma quando houver espaço.
                    float next = Mathf.MoveTowards(mc.PoseLevel, p.PoseMemo, Time.deltaTime / SlowRiseSeconds); // ref: Player.cs:23912
                    mc.SetPoseLevel(next);
                    if (mc.PoseLevel >= p.PoseMemo - 0.01f)
                    {
                        StandReentryFlag = false;
                        EnterWindow("rose"); // "de pé efetivo" = pose alvo REAL atingida (premissa técnica — item 011)
                    }
                    break;
                case FallPhase.Window:
                    if (Time.time >= _phaseDeadline)
                    {
                        if (mc.IsInPronePose) { EnterBlocked("window-expired-prone"); break; } // deitou voluntário — sem re-derrubar
                        // Re-queda INTERNA (isenta do cooldown do motor); fallback agachado se CanProne=false.
                        // PA-02-04: FallPending ANTES da chamada (mesmo shape da entrada) — D7 adiando NÃO
                        // re-chama/loga por frame (o dedup da fila loga a cada call — TraumaPose Defer);
                        // cap da janela SAI no adiamento (D7 sem locomoção normal — premissa p/ item 011).
                        // Execução imediata → OnFallExecuted re-entra BLOQUEIO no MESMO frame.
                        // TraumaPose RECUSA o enqueue interno se houver entrada NÃO-interna pendente (PA-01-03).
                        RemoveWindowCap();
                        _phase = FallPhase.FallPending;
                        TraumaPose.TryInvoluntaryFall(p, TraumaRegion.Legs, TraumaOneShotKind.InvoluntaryFall,
                            internalFall: true, OnFallExecuted);
                    }
                    break;
            }
        }
    }
}
