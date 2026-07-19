using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using UnityEngine;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>
    /// Motor de estados Trauma 2.0 (spec 002). Componente no GameObject do PLUGIN (padrão do repo —
    /// TRLImmersiveCombatMedicinePlugin.cs). Dono-only (D16). Sem Harmony próprio: 100% dirigido pelos
    /// eventos C# do IHealthController (P10 — EVENTO-first) + polling de reconciliação ≤4 Hz (D19)
    /// para os caminhos comprovadamente sem evento (FullRestoreBodyPart, revive Fika, transit heal).
    /// </summary>
    public sealed class TraumaEngine : MonoBehaviour
    {
        private static TraumaEngine _instance;

        // ---- Contrato público (comportamento 3 da funcional) ----
        public static event Action<TraumaTransition> StateChanged;
        public static event Action<Player, TraumaOneShotKind, TraumaLine> OneShotPublished; // já cooldown-gated

        // ---- Estado do motor ----
        private readonly List<PlayerTraumaRecord> _records = new List<PlayerTraumaRecord>();          // ordem de inserção = ordem de publicação
        private readonly Dictionary<Player, PlayerTraumaRecord> _recordsByPlayer = new Dictionary<Player, PlayerTraumaRecord>();
        private readonly Dictionary<(string ProfileId, TraumaOneShotKind Kind), float> _cooldownUntil =
            new Dictionary<(string, TraumaOneShotKind), float>(); // anti-thrash (decisão 19) — por jogador e por tipo

        // Listas pré-alocadas reusadas — sem alocação no caminho quente (orçamento do corner da funcional)
        private readonly List<PlayerTraumaRecord> _evalScratch = new List<PlayerTraumaRecord>();
        private readonly TraumaLine[] _newLines = new TraumaLine[3];

        private GameWorld _trackedWorld;      // padrão N1 do repo (BandAidController): null-detect de GameWorld
        private bool _personAddSubscribed;    // rastreamento ativo (subscribe OnPersonAdd + records vivos)
        private float _pollAccumulator;
        private bool _debugConsumerSubscribed; // stub AC6: SubscribeWithSnapshot tardio 1x na 1ª ativação do Debug Test Consumer

        private static readonly TraumaRegion[] DebugTestRegions =
            { TraumaRegion.Legs, TraumaRegion.Arms, TraumaRegion.Stomach };

        // ---- Consultas públicas ----

        public static TraumaLine GetLine(Player player, TraumaRegion region)
        {
            TraumaEngine inst = _instance;
            if (inst == null || player == null) return TraumaLine.None;
            return inst._recordsByPlayer.TryGetValue(player, out PlayerTraumaRecord rec)
                ? rec.Lines[(int)region]
                : TraumaLine.None;
        }

        public static bool TryGetSnapshot(Player player, out TraumaSnapshot snapshot)
        {
            snapshot = default;
            TraumaEngine inst = _instance;
            if (inst == null || player == null) return false;
            if (!inst._recordsByPlayer.TryGetValue(player, out PlayerTraumaRecord rec)) return false;
            snapshot.Legs = rec.Lines[(int)TraumaRegion.Legs];
            snapshot.Arms = rec.Lines[(int)TraumaRegion.Arms];
            snapshot.Stomach = rec.Lines[(int)TraumaRegion.Stomach];
            snapshot.StomachPainkillerAtEntry = rec.StomachPainkillerAtEntry;
            snapshot.UnderPainkiller = IsUnderPainkiller(player);
            return true;
        }

        /// <summary>Assinante tardio: recebe replay dos estados ativos como Establishing=true antes das transições futuras.</summary>
        public static void SubscribeWithSnapshot(Action<TraumaTransition> handler)
        {
            if (handler == null) return;
            TraumaEngine inst = _instance;
            if (inst != null)
            {
                for (int r = 0; r < inst._records.Count; r++)
                {
                    PlayerTraumaRecord rec = inst._records[r];
                    for (int i = 0; i < 3; i++)
                    {
                        TraumaLine line = rec.Lines[i];
                        if (line == TraumaLine.None) continue;
                        TraumaRegion region = (TraumaRegion)i;
                        bool pk = region == TraumaRegion.Stomach
                            ? rec.StomachPainkillerAtEntry // D8 — valor latched da entrada
                            : IsUnderPainkiller(rec.Player);
                        handler(new TraumaTransition(rec.Player, region, TraumaLine.None, line,
                            TraumaChangeReason.InitialEvaluation, TraumaChangeReason.InitialEvaluation,
                            establishing: true, painkillerActive: pk));
                    }
                }
            }
            StateChanged += handler;
        }

        /// <summary>Consulta "analgésico ativo agora" (consumida pelo 007 — decisões 9/15). Predicado do P3.</summary>
        public static bool IsUnderPainkiller(Player player)
        {
            // ref: predicado vanilla OnPainkillers — Player.cs:29070 (dump); trauma-primitives.md P3 Recomendação (1)
            IHealthController hc = player?.HealthController;
            if (hc == null) return false;
            if (hc.FindActiveEffect<GInterface358>() != null) return true;  // PainKiller — GInterface358 público (P3 evid.)
            return TRLImmersiveCombatMedicinePlugin.ConfigIncludeAdrenaline.Value
                && hc.FindActiveEffect<GInterface350>() != null;            // Berserk/adrenalina — ref: Player.cs:29070
        }

        /// <summary>Autoridade D16: humano local e bots do host/headless têm ActiveHealthController; espelhos são NetworkHealthControllerAbstractClass.</summary>
        internal static bool IsOwnedHere(Player p)
        {
            // ref: trauma-primitives.md P10 Recomendação (1) — "guard de autoridade natural"; fika ClientHealthController.cs:14, BotHealthController.cs:11-12
            return p != null && p.HealthController is ActiveHealthController;
        }

        /// <summary>Item 004/006 (adiamento D7): re-ancora o cooldown do one-shot na EXECUÇÃO (comportamento 6 — "cooldown conta da execução").</summary>
        public static void ReportOneShotExecuted(Player player, TraumaOneShotKind kind)
        {
            TraumaEngine inst = _instance;
            if (inst == null || player == null) return;
            inst._cooldownUntil[(player.ProfileId, kind)] = Time.time + OneShotCooldownSeconds();
        }

        // ---- Lifecycle (AP-01) ----

        private void Awake()
        {
            _instance = this;
            // Debug Test Consumer (AC5): registra-se p/ as TRÊS regiões; isActive lê o toggle F12 ao vivo.
            TraumaConsumerRegistry.Register(TraumaConsumerId.DebugTest, DebugTestRegions,
                () => TRLImmersiveCombatMedicinePlugin.ConfigDebugTestConsumer.Value);
        }

        /// <summary>Chamado do prefix existente de GameWorld.OnGameStarted (Plugin.OnRaidStartCleanup) — inclusive transit (novo GameWorld).</summary>
        internal static void OnRaidStarted()
        {
            TraumaEngine inst = _instance;
            if (inst == null) return;
            inst.ResetForNewRaidInternal(); // 1. reset idempotente (AC8)
            GameWorld gw = Singleton<GameWorld>.Instance;
            if (gw == null) return; // Update adota o mundo quando o singleton aparecer (defesa N1)
            inst._trackedWorld = gw;
            if (inst.IsEngineEnabled())
                inst.ActivateTracking(gw); // 2. subscribe OnPersonAdd + 3. sweep estabelecedor
        }

        /// <summary>Limpeza TOTAL, idempotente: unsubscribe de tudo, zera records, cooldowns e toasts-vistos (AC8).</summary>
        internal static void ResetForNewRaid()
        {
            _instance?.ResetForNewRaidInternal();
        }

        private void ResetForNewRaidInternal()
        {
            DeactivateTracking(publishExits: false); // fronteira de raid — limpeza SEM transições espúrias (comportamento 1)
            _cooldownUntil.Clear();
            TraumaObservability.ResetSeen();
            _pollAccumulator = 0f;
            _trackedWorld = null;
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo("[Trauma2] ResetForNewRaid");
        }

        private void Update()
        {
            // (b) null-detect de GameWorld (padrão N1 do BandAidController): GameWorld sumiu → ResetForNewRaid()
            GameWorld gw = Singleton<GameWorld>.Instance;
            if (gw == null)
            {
                if (_trackedWorld != null || _records.Count > 0) ResetForNewRaidInternal();
                return;
            }
            if (!ReferenceEquals(gw, _trackedWorld))
            {
                // Mundo novo detectado sem passar pelo prefix (defesa; transit normal passa pelo prefix) — reset + adotar
                ResetForNewRaidInternal();
                _trackedWorld = gw;
            }

            // (a) master legado + master Trauma 2.0 (abertura 5 ratificada: exige os DOIS on):
            //     caiu com estados ativos → publica saídas (Reason=EngineDisabled) e UntrackAll; religar → re-sweep estabelecedor
            bool engineOn = IsEngineEnabled();
            if (!engineOn)
            {
                if (_personAddSubscribed || _records.Count > 0)
                {
                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo("[Trauma2] engine disabled mid-raid — publishing exits");
                    DeactivateTracking(publishExits: true);
                }
                return;
            }
            if (!_personAddSubscribed)
            {
                ActivateTracking(gw); // boot com master off→on, ou religar mid-raid (avaliação estabelecedora)
            }

            // Stub do AC6: 1ª ativação do Debug Test Consumer assina TARDIAMENTE com snapshot (replay establishing=true)
            if (!_debugConsumerSubscribed && TRLImmersiveCombatMedicinePlugin.ConfigDebugTestConsumer.Value)
            {
                _debugConsumerSubscribed = true;
                SubscribeWithSnapshot(OnDebugConsumerTransition);
            }

            // (c) dirty-set: consolidar 1×/frame em ordem determinística (players por inserção; regiões na ordem do enum)
            ConsolidateDirty();

            // (d) polling de reconciliação (SÓ caminhos sem evento — D19; nunca por frame)
            float hz = Mathf.Clamp(TRLImmersiveCombatMedicinePlugin.ConfigPollingHz.Value, 1f, 4f);
            _pollAccumulator += Time.deltaTime;
            if (_pollAccumulator >= 1f / hz)
            {
                _pollAccumulator = 0f;
                Reconcile();
            }
        }

        private bool IsEngineEnabled()
        {
            return TRLImmersiveCombatMedicinePlugin.ConfigMasterEnabled.Value
                && TRLImmersiveCombatMedicinePlugin.ConfigTrauma2Enabled.Value;
        }

        private static float OneShotCooldownSeconds()
        {
            return Mathf.Clamp(TRLImmersiveCombatMedicinePlugin.ConfigOneShotCooldown.Value, 3f, 5f);
        }

        // ---- Rastreamento ----

        private void ActivateTracking(GameWorld gw)
        {
            if (_personAddSubscribed) return;
            // Ordem ratificada (abertura 6 da spec): subscribe de OnPersonAdd ANTES do sweep;
            // TrackPlayer idempotente absorve a sobreposição.
            gw.OnPersonAdd += OnPersonAdd; // ref: GameWorld.cs:991 (declaração), :2305 (Invoke no fim de RegisterPlayer — cobre bots mid-raid)
            _personAddSubscribed = true;

            var players = gw.RegisteredPlayers; // ref: uso existente no mod, BandAidNetworkHandler.FindPatient
            int owned = 0;
            for (int i = 0; i < players.Count; i++)
            {
                var p = players[i] as Player;
                if (!IsOwnedHere(p)) continue;
                TrackPlayer(p, establishing: true);
                owned++;
            }
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] tracking active — owned={owned} registered={players.Count}");
        }

        private void DeactivateTracking(bool publishExits)
        {
            if (publishExits && _records.Count > 0)
            {
                _evalScratch.Clear();
                _evalScratch.AddRange(_records);
                for (int r = 0; r < _evalScratch.Count; r++)
                {
                    PlayerTraumaRecord rec = _evalScratch[r];
                    for (int i = 0; i < 3; i++) // ordem determinística Legs→Arms→Estômago
                    {
                        TraumaLine from = rec.Lines[i];
                        rec.PendingReasons[i] = TraumaChangeReason.None;
                        if (from == TraumaLine.None) continue;
                        TraumaRegion region = (TraumaRegion)i;
                        bool pk = region == TraumaRegion.Stomach
                            ? rec.StomachPainkillerAtEntry
                            : IsUnderPainkiller(rec.Player);
                        rec.Lines[i] = TraumaLine.None;
                        var t = new TraumaTransition(rec.Player, region, from, TraumaLine.None,
                            TraumaChangeReason.EngineDisabled, TraumaChangeReason.EngineDisabled,
                            establishing: false, painkillerActive: pk);
                        StateChanged?.Invoke(t);
                        TraumaObservability.LogTransition(in t); // saída não gera one-shot nem toast (To=None)
                    }
                }
                _evalScratch.Clear();
            }

            while (_records.Count > 0)
            {
                UntrackPlayer(_records[_records.Count - 1].Player);
            }

            if (_personAddSubscribed)
            {
                if (_trackedWorld != null) _trackedWorld.OnPersonAdd -= OnPersonAdd;
                _personAddSubscribed = false;
            }
        }

        private void OnPersonAdd(IPlayer person)
        {
            // ref: GameWorld.cs:991/2305 — dispara p/ QUALQUER registro (donos e espelhos); IsOwnedHere filtra
            var p = person as Player;
            if (!IsOwnedHere(p)) return;
            TrackPlayer(p, establishing: true); // primeira avaliação de bot mid-raid é estabelecedora (corner da funcional)
        }

        private void TrackPlayer(Player p, bool establishing)
        {
            if (p == null || _recordsByPlayer.ContainsKey(p)) return; // idempotente (OnPersonAdd + sweep podem se sobrepor)
            if (!IsOwnedHere(p)) return;

            IHealthController hc = p.HealthController;
            var rec = new PlayerTraumaRecord { Player = p, Hc = hc };

            // Handlers SÓ marcam dirty — sem lógica (consolidação 1×/frame no Update). Closures POR RECORD
            // guardadas p/ o -= simétrico (IEffect NÃO expõe o dono — review 2 da spec).
            rec.OnDestroyedHandler = (part, damageType) => OnBodyPartDestroyed(rec, part);
            rec.OnRestoredHandler = (part, before) => OnBodyPartRestored(rec, part);
            rec.OnEffectHandler = e => OnEffectEvent(rec, e);
            rec.OnApplyDamageHandler = (part, damage, info) =>
            {
                // Só contexto de "motivo" (tipo/valor do dano) — NÃO fornece vida pré-tiro (domínio do P7/item 007)
                rec.LastDamageType = info.DamageType;
                rec.LastDamageValue = damage;
            };

            // Assinaturas EXATAS na interface (P10 Recomendação (2); re-verificadas por ilspycmd no assembly real):
            hc.BodyPartDestroyedEvent += rec.OnDestroyedHandler;  // Zerar — ref: AHC DestroyBodyPart :3867-3877 (P10 evid.)
            hc.BodyPartRestoredEvent += rec.OnRestoredHandler;    // des-Zerar (cirurgia local E remota) — ref: AHC :3891-3910
            hc.EffectStartedEvent += rec.OnEffectHandler;         // Quebrar / analgésico começa
            hc.EffectResidualEvent += rec.OnEffectHandler;        // des-Quebrar / analgésico EXPIRA — ref: P3 correção (NÃO EffectRemovedEvent — Removed = FadeOut +5..50s)
            hc.EffectRemovedEvent += rec.OnEffectHandler;         // cinto de segurança (P10 Recomendação (2))
            hc.ApplyDamageEvent += rec.OnApplyDamageHandler;      // contexto de motivo — ref: AHC ~3411 (âncora P7)
            p.OnPlayerDeadOrUnspawn += UntrackPlayer;             // ref: Player.cs:25510 (GDelegate71 = void(Player)); Invoke em OnDead e Dispose

            _records.Add(rec);
            _recordsByPlayer[p] = rec;

            // Estado inicial pelo QUERY, não por evento (Player.Init já rodou PropagateAllEffects — P3 Recomendação (3)):
            rec.LastPainkiller = IsUnderPainkiller(p);
            for (int i = 0; i < 3; i++) rec.PendingReasons[i] = TraumaChangeReason.InitialEvaluation;
            EvaluatePlayer(rec, establishing); // establishing NÃO publica one-shot nem toast (só log establishing=true)
        }

        private void UntrackPlayer(Player p)
        {
            // Limpeza SEM transições espúrias (comportamento 1): unsubscribe simétrico + remove record + limpa cooldowns. Sem StateChanged.
            if (p == null || !_recordsByPlayer.TryGetValue(p, out PlayerTraumaRecord rec)) return;

            IHealthController hc = rec.Hc;
            if (hc != null)
            {
                hc.BodyPartDestroyedEvent -= rec.OnDestroyedHandler;
                hc.BodyPartRestoredEvent -= rec.OnRestoredHandler;
                hc.EffectStartedEvent -= rec.OnEffectHandler;
                hc.EffectResidualEvent -= rec.OnEffectHandler;
                hc.EffectRemovedEvent -= rec.OnEffectHandler;
                hc.ApplyDamageEvent -= rec.OnApplyDamageHandler;
            }
            p.OnPlayerDeadOrUnspawn -= UntrackPlayer;

            _recordsByPlayer.Remove(p);
            _records.Remove(rec);

            string id = p.ProfileId;
            if (id != null)
            {
                _cooldownUntil.Remove((id, TraumaOneShotKind.InvoluntaryCrouch));
                _cooldownUntil.Remove((id, TraumaOneShotKind.InvoluntaryFall));
            }
        }

        // ---- Handlers de evento (só marcam dirty) ----

        private static bool TryRegionOf(EBodyPart part, out TraumaRegion region)
        {
            switch (part)
            {
                case EBodyPart.LeftLeg:
                case EBodyPart.RightLeg: region = TraumaRegion.Legs; return true;
                case EBodyPart.LeftArm:
                case EBodyPart.RightArm: region = TraumaRegion.Arms; return true;
                case EBodyPart.Stomach: region = TraumaRegion.Stomach; return true;
                default: region = TraumaRegion.Legs; return false; // Head/Chest = domínio do desmaio (item 007)
            }
        }

        private static void MarkDirty(PlayerTraumaRecord rec, TraumaRegion region, TraumaChangeReason reason)
        {
            rec.PendingReasons[(int)region] |= reason;
        }

        private void OnBodyPartDestroyed(PlayerTraumaRecord rec, EBodyPart part)
        {
            if (TryRegionOf(part, out TraumaRegion region)) MarkDirty(rec, region, TraumaChangeReason.Damage);
        }

        private void OnBodyPartRestored(PlayerTraumaRecord rec, EBodyPart part)
        {
            if (TryRegionOf(part, out TraumaRegion region)) MarkDirty(rec, region, TraumaChangeReason.BodyPartRestored);
        }

        private void OnEffectEvent(PlayerTraumaRecord rec, IEffect e) // rec via closure por-record — IEffect não identifica o jogador (review 2)
        {
            if (e == null) return;

            // Filtro por INTERFACE, nunca nested type concreto (P10 Recomendação (5)).
            if (e is GInterface342) // fratura — ref: GClass3009 IsBodyPartBroken via P10 evid.
            {
                if (!TryRegionOf(e.BodyPart, out TraumaRegion region) || region == TraumaRegion.Stomach) return;
                if (e.State == EEffectState.Started)
                    MarkDirty(rec, region, TraumaChangeReason.FractureGained);
                else if (e.State == EEffectState.Residued || e.State == EEffectState.Removed)
                    MarkDirty(rec, region, TraumaChangeReason.FractureHealed);
                return;
            }

            if (e is GInterface358 || e is GInterface350) // analgésico: PainKiller / Berserk (P3)
            {
                // Re-check de expiração é feito na consolidação re-derivando IsUnderPainkiller
                // (doses de itens diferentes = instâncias separadas — P3 Recomendação (2)).
                TraumaChangeReason reason;
                if (e.State == EEffectState.Started) reason = TraumaChangeReason.PainkillerGained;
                else if (e.State == EEffectState.Residued || e.State == EEffectState.Removed) reason = TraumaChangeReason.PainkillerLost;
                else return;
                MarkDirty(rec, TraumaRegion.Legs, reason);
                MarkDirty(rec, TraumaRegion.Arms, reason);
                // Estômago NÃO — latch D8: mudança de analgésico nunca gera transição de estômago (AC3)
            }
        }

        // ---- Avaliação consolidada ----

        private void ConsolidateDirty()
        {
            if (_records.Count == 0) return;
            _evalScratch.Clear();
            for (int i = 0; i < _records.Count; i++)
            {
                PlayerTraumaRecord rec = _records[i];
                if (rec.PendingReasons[0] != TraumaChangeReason.None
                    || rec.PendingReasons[1] != TraumaChangeReason.None
                    || rec.PendingReasons[2] != TraumaChangeReason.None)
                {
                    _evalScratch.Add(rec);
                }
            }
            for (int i = 0; i < _evalScratch.Count; i++)
            {
                PlayerTraumaRecord rec = _evalScratch[i];
                if (rec.Player == null || !_recordsByPlayer.ContainsKey(rec.Player)) continue; // untracked durante o dispatch
                EvaluatePlayer(rec, establishing: false);
            }
            _evalScratch.Clear();
        }

        private void EvaluatePlayer(PlayerTraumaRecord rec, bool establishing)
        {
            Player p = rec.Player;
            IHealthController hc = rec.Hc;
            if (p == null || hc == null) return;
            if (!hc.IsAlive)
            {
                // Morto → o caminho é UntrackPlayer (OnPlayerDeadOrUnspawn); nunca transição espúria aqui
                for (int i = 0; i < 3; i++) rec.PendingReasons[i] = TraumaChangeReason.None;
                return;
            }

            // 1. Predicado de analgésico (P3)
            bool pk = IsUnderPainkiller(p);

            // 2. Contagens (D4 — mesmo membro conta as duas condições)
            //    ref: IHealthController.IsBodyPartDestroyed/IsBodyPartBroken (P10 evid.; re-verificado por ilspycmd)
            int zeroedLegs = (hc.IsBodyPartDestroyed(EBodyPart.LeftLeg) ? 1 : 0) + (hc.IsBodyPartDestroyed(EBodyPart.RightLeg) ? 1 : 0);
            int brokenLegs = (hc.IsBodyPartBroken(EBodyPart.LeftLeg) ? 1 : 0) + (hc.IsBodyPartBroken(EBodyPart.RightLeg) ? 1 : 0);
            int zeroedArms = (hc.IsBodyPartDestroyed(EBodyPart.LeftArm) ? 1 : 0) + (hc.IsBodyPartDestroyed(EBodyPart.RightArm) ? 1 : 0);
            int brokenArms = (hc.IsBodyPartBroken(EBodyPart.LeftArm) ? 1 : 0) + (hc.IsBodyPartBroken(EBodyPart.RightArm) ? 1 : 0);
            bool stomachZeroed = hc.IsBodyPartDestroyed(EBodyPart.Stomach);

            // 3. Pernas/braços via resolver puro; 4. estômago é linha única LATCHED (D8) — não passa no resolver
            _newLines[(int)TraumaRegion.Legs] = TraumaMatrixResolver.ResolveLegs(zeroedLegs, brokenLegs, pk);
            _newLines[(int)TraumaRegion.Arms] = TraumaMatrixResolver.ResolveArms(zeroedArms, brokenArms, pk);
            _newLines[(int)TraumaRegion.Stomach] = stomachZeroed ? TraumaLine.StomachZeroed : TraumaLine.None;

            if (TRLImmersiveCombatMedicinePlugin.ConfigVerboseEngineLog.Value)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                    $"[Trauma2] eval {p.ProfileId} zL={zeroedLegs} bL={brokenLegs} zA={zeroedArms} bA={brokenArms} st={stomachZeroed} pk={pk} lastDmg={rec.LastDamageType}/{rec.LastDamageValue:0.#}");
            }

            // 5. Publicação em ordem determinística Legs→Arms→Stomach (corner de rajada da funcional)
            for (int i = 0; i < 3; i++)
            {
                TraumaRegion region = (TraumaRegion)i;
                TraumaLine from = rec.Lines[i];
                TraumaLine to = _newLines[i];
                TraumaChangeReason mask = rec.PendingReasons[i];
                rec.PendingReasons[i] = TraumaChangeReason.None; // zerada após consolidar (mesmo sem diff)
                if (from == to) continue;

                if (establishing) mask = TraumaChangeReason.InitialEvaluation; // não combina (doc do enum)
                else if (mask == TraumaChangeReason.None) mask = TraumaChangeReason.Reconciliation; // diff sem evento = caminho de reconciliação
                TraumaChangeReason reason = PrimaryReason(mask);

                bool pkForTransition = pk;
                if (region == TraumaRegion.Stomach)
                {
                    // Latch D8: analgésico DO INSTANTE da entrada (via polling = instante da DETECÇÃO — abertura 3 da spec);
                    // nova chance só em NOVA zerada (decisão 7)
                    if (to == TraumaLine.StomachZeroed) rec.StomachPainkillerAtEntry = pk;
                    pkForTransition = rec.StomachPainkillerAtEntry;
                }

                rec.Lines[i] = to;
                var t = new TraumaTransition(p, region, from, to, reason, mask, establishing, pkForTransition);
                StateChanged?.Invoke(t);

                if (!establishing)
                {
                    // Linhas de pernas que EMBUTEM one-shot (p=100%): re-entrada por expiração de analgésico
                    // TAMBÉM publica (decisão 14) — o cooldown decide (decisão 19)
                    if (to == TraumaLine.LegsCrouchPlusLimpN2) TryPublishOneShot(p, TraumaOneShotKind.InvoluntaryCrouch, to);
                    else if (to == TraumaLine.LegsFallCycle) TryPublishOneShot(p, TraumaOneShotKind.InvoluntaryFall, to);
                }

                // 6. Observabilidade: transição SEMPRE logada; toast gateado (humano local + consumidor ativo — decisão 20)
                TraumaObservability.LogTransition(in t);
                TraumaObservability.MaybeToastFirstOccurrence(in t);
            }

            rec.LastPainkiller = pk;
        }

        private bool TryPublishOneShot(Player p, TraumaOneShotKind kind, TraumaLine line)
        {
            // Cooldown anti-thrash (decisão 19): por jogador (profileId) e por tipo; ciclos internos dos
            // consumidores são ISENTOS (eles usam ReportOneShotExecuted só p/ re-ancorar no disparo adiado — D7)
            string id = p.ProfileId;
            float now = Time.time;
            var key = (id, kind);
            if (_cooldownUntil.TryGetValue(key, out float deadline) && now < deadline)
            {
                TraumaObservability.LogOneShot(p, kind, suppressedByCooldown: true);
                return false;
            }
            _cooldownUntil[key] = now + OneShotCooldownSeconds();
            TraumaObservability.LogOneShot(p, kind, suppressedByCooldown: false);
            OneShotPublished?.Invoke(p, kind, line);
            return true;
        }

        private void Reconcile()
        {
            // Re-deriva contagens de todos os rastreados e roda EvaluatePlayer SÓ p/ divergentes (Reason=Reconciliation).
            // Cobre FullRestoreBodyPart (sem evento — P10 evid.), revive Fika (RestoreBodyPartNoEvents) e transit heal.
            if (_records.Count == 0) return;
            if (TRLImmersiveCombatMedicinePlugin.ConfigVerboseEngineLog.Value)
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] reconcile sweep n={_records.Count}");

            _evalScratch.Clear();
            _evalScratch.AddRange(_records);
            for (int r = 0; r < _evalScratch.Count; r++)
            {
                PlayerTraumaRecord rec = _evalScratch[r];
                Player p = rec.Player;
                IHealthController hc = rec.Hc;
                if (p == null || hc == null || !_recordsByPlayer.ContainsKey(p) || !hc.IsAlive) continue;

                bool pk = IsUnderPainkiller(p);
                int zeroedLegs = (hc.IsBodyPartDestroyed(EBodyPart.LeftLeg) ? 1 : 0) + (hc.IsBodyPartDestroyed(EBodyPart.RightLeg) ? 1 : 0);
                int brokenLegs = (hc.IsBodyPartBroken(EBodyPart.LeftLeg) ? 1 : 0) + (hc.IsBodyPartBroken(EBodyPart.RightLeg) ? 1 : 0);
                int zeroedArms = (hc.IsBodyPartDestroyed(EBodyPart.LeftArm) ? 1 : 0) + (hc.IsBodyPartDestroyed(EBodyPart.RightArm) ? 1 : 0);
                int brokenArms = (hc.IsBodyPartBroken(EBodyPart.LeftArm) ? 1 : 0) + (hc.IsBodyPartBroken(EBodyPart.RightArm) ? 1 : 0);
                bool stomachZeroed = hc.IsBodyPartDestroyed(EBodyPart.Stomach);

                bool divergent = false;
                if (TraumaMatrixResolver.ResolveLegs(zeroedLegs, brokenLegs, pk) != rec.Lines[(int)TraumaRegion.Legs])
                {
                    MarkDirty(rec, TraumaRegion.Legs, TraumaChangeReason.Reconciliation);
                    divergent = true;
                }
                if (TraumaMatrixResolver.ResolveArms(zeroedArms, brokenArms, pk) != rec.Lines[(int)TraumaRegion.Arms])
                {
                    MarkDirty(rec, TraumaRegion.Arms, TraumaChangeReason.Reconciliation);
                    divergent = true;
                }
                if ((stomachZeroed ? TraumaLine.StomachZeroed : TraumaLine.None) != rec.Lines[(int)TraumaRegion.Stomach])
                {
                    MarkDirty(rec, TraumaRegion.Stomach, TraumaChangeReason.Reconciliation);
                    divergent = true;
                }
                if (divergent) EvaluatePlayer(rec, establishing: false);
            }
            _evalScratch.Clear();
        }

        /// <summary>Bit MAIS ALTO setado = motivo primário (bits numerados em ordem de precedência — review 2).</summary>
        private static TraumaChangeReason PrimaryReason(TraumaChangeReason mask)
        {
            int v = (int)mask;
            for (int i = 31; i >= 0; i--)
            {
                int bit = 1 << i;
                if ((v & bit) != 0) return (TraumaChangeReason)bit;
            }
            return TraumaChangeReason.None;
        }

        private static void OnDebugConsumerTransition(TraumaTransition t)
        {
            // Handler do stub Debug Test Consumer: fica assinado, mas silencia com o toggle off
            if (!TRLImmersiveCombatMedicinePlugin.ConfigDebugTestConsumer.Value) return;
            TraumaObservability.LogDebugConsumer(in t);
        }
    }
}
