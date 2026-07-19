using System;
using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using Fika.Core.Main.Utils; // FikaBackendUtils.IsHeadless (gate P9 rec. (b) — PA-01-07)
using UnityEngine;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Consumidor de BRAÇOS (spec 005): tremor gerenciado + cancela-ADS escalonado + lockout.
    /// Evento-first (OnAimingChanged substitui o polling do legado). Efeitos SÓ no humano local
    /// (bots EXCLUÍDOS — funcional 5; headless no-op por construção). ZERO mudança no motor 002.</summary>
    public sealed class TraumaArmsConsumer : MonoBehaviour
    {
        private static TraumaArmsConsumer _instance;
        private static bool _timerClampWarned; // warn 1x do min() dos timers (§3)

        // ---- estado (humano local apenas — campos únicos, não dict) ----
        private TraumaLine _localLine;            // linha de braço APLICADA ao humano local
        private Player _localPlayer;              // referência p/ desfazer/re-estabelecer
        private Player _handsHookedPlayer;        // dono do HandsChangedEvent assinado (p/ -= simétrico)
        private Player.FirearmController _hookedFc;               // p/ -= simétrico de OnAimingChanged
        private Action<IHandsController> _handsHandler;           // p/ -= simétrico de HandsChangedEvent
        private Action<bool> _aimHandler;                         // p/ -= simétrico de OnAimingChanged
        private IHealthController _hookedHc;                      // dono do watchdog assinado (p/ -= simétrico)
        private Action<IEffect> _effectGoneHandler;               // watchdog (Residual/Removed) — p/ -= simétrico
        private bool _reestablishPending;         // remoção externa detectada → re-aplica no próximo Update
        private float _nextReestablishAt;         // piso de 0,5s entre re-applies do watchdog (PA-01-05)
        private int _reestablishCount;            // re-applies na linha CORRENTE (zera na mudança de linha)
        private static bool _reestablishStormWarned; // LogWarning 1x/sessão após 3 re-applies na mesma linha (PA-01-05)
        private float _aimAnchor = -1f;           // <0 = sem mira sustentada; senão Time.time da âncora
        private float _lockoutUntil;              // Time.time do fim do lockout (0 = inativo)
        private string _lockoutProfileId;         // dono do lockout (defesa contra stale entre raids)
        private bool _lockoutVoicePlayed;         // throttle: 1 voz por janela (funcional 4)
        private float _nextVoiceTryAt;            // piso de 0,3s entre Plays engolidos (PA-02-06 — hold-por-frame sob Blocker)
        private bool _lockoutVoiceSkipLogged;     // log voice=skipped no máx 1x/janela (PA-02-06)
        private bool _lockoutIncapacitatedLogged; // log voice=suppressed(incapacitated) no máx 1x/janela (CR-01-02)
        private bool _wasActive;
        private GameWorld _trackedWorld;          // world-swap sem null (transit) — lição CR-02 do 003

        private static readonly TraumaRegion[] ArmsRegions = { TraumaRegion.Arms };

        private void Awake()
        {
            _instance = this;
            TraumaConsumerRegistry.Register(TraumaConsumerId.ArmsEffects, ArmsRegions, IsActive); // destrava toast (decisão 20)
            TraumaEngine.SubscribeWithSnapshot(OnTransition); // replay establishing — ref: TraumaEngine.cs:72
        }

        /// <summary>Master legado + master Trauma 2.0 + toggle próprio (comportamento 9 do 002) + gate headless.</summary>
        internal static bool IsActive()
        {
            return !FikaBackendUtils.IsHeadless // P9 rec. (b): headless nem arma hooks/Update (fika FikaBackendUtils.cs:49) — PA-01-07
                && TRLImmersiveCombatMedicinePlugin.ConfigMasterEnabled.Value
                && TRLImmersiveCombatMedicinePlugin.ConfigTrauma2Enabled.Value
                && TRLImmersiveCombatMedicinePlugin.ConfigConsumerArmsEffects.Value;
        }

        /// <summary>Linhas que armam o timer de cancela-ADS (2 braços comprometidos).</summary>
        internal static bool IsAdsCancelTier(TraumaLine line)
        {
            return line == TraumaLine.ArmsTremorAdsCancel4s
                || line == TraumaLine.ArmsTremorAdsCancel3s
                || line == TraumaLine.ArmsTremorAdsCancel2s;
        }

        /// <summary>Timer efetivo por linha (§3); Z2+Q2 = min dos três — a linha mais severa (ranking do enum)
        /// nunca fica mais lenta que as menos severas (warn 1x — padrão min(N2,N1) do 003). Z2 vs Q2 livre
        /// (decisão 3 é default, não invariante). Lidos por .Value a cada uso (sem cache).</summary>
        internal static float LineCancelSeconds(TraumaLine line)
        {
            float z2 = Mathf.Clamp(TRLImmersiveCombatMedicinePlugin.ConfigArmsAdsCancelZ2Seconds.Value, 1f, 10f);
            float q2 = Mathf.Clamp(TRLImmersiveCombatMedicinePlugin.ConfigArmsAdsCancelQ2Seconds.Value, 1f, 10f);
            if (line == TraumaLine.ArmsTremorAdsCancel4s) return z2;
            if (line == TraumaLine.ArmsTremorAdsCancel3s) return q2;
            float z2q2 = Mathf.Clamp(TRLImmersiveCombatMedicinePlugin.ConfigArmsAdsCancelZ2Q2Seconds.Value, 1f, 10f);
            float effective = Mathf.Min(z2q2, Mathf.Min(z2, q2));
            if (effective < z2q2 && !_timerClampWarned)
            {
                _timerClampWarned = true;
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning(
                    $"[Trauma2] config ADS Cancel Z2+Q2 ({z2q2:0.#}s) > min(Z2 {z2:0.#}s, Q2 {q2:0.#}s) — usando min = {effective:0.#}s");
            }
            return effective;
        }

        private void OnTransition(TraumaTransition t)
        {
            // ref: CR-01-04 do 004 — exceção de consumidor não pode subir p/ o StateChanged?.Invoke do motor
            // (mataria a publicação das regiões/records restantes do frame)
            try { OnTransitionCore(t); }
            catch (Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogError($"[Trauma2] ArmsConsumer.OnTransition: {ex.Message}");
            }
        }

        private void OnTransitionCore(TraumaTransition t)
        {
            if (t.Region != TraumaRegion.Arms) return;
            if (!IsActive()) return; // toggle off = ignora (motor segue publicando — comportamento 9 do 002)
            Player p = t.Player;
            if (p is null) return;
            if (p.IsAI)
            {
                // Bots EXCLUÍDOS de tremor E cancela-ADS (funcional 5 — supersede D9 pré-spike). AC-8.
                if (t.To != TraumaLine.None)
                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] arms EXCLUDED (bot) {p.ProfileId} line={t.To}");
                return;
            }
            if (!p.IsYourPlayer) return; // defesa extra — motor só publica donos; espelho nunca chega (D16)
            ApplyLine(p, t.To);          // establishing idem: aplica SEM toast/voz (toast é gate do motor)
        }

        private void ApplyLine(Player p, TraumaLine line)
        {
            if (line == TraumaLine.None) { TearDownLocal("state-exit"); return; } // worldDead=false → Remove (AHC VIVO)
            if (line != _localLine) _reestablishCount = 0;   // linha nova → zera a contagem do watchdog (PA-01-05)
            _localPlayer = p;
            _localLine = line;
            // 1. Tremor: TODA linha de braço inclui tremor (decisão 3); Apply re-ancora se a âncora foi curada (PA-01-06)
            TraumaTremor.Apply(p, PickArmAnchor(p));  // âncora = braço zerado/quebrado (IsBodyPartDestroyed/Broken)
            EnsureWatchdog(p);                        // Residual/Removed → IsOurs + GetLine!=None → _reestablishPending
            // 2. Timer de ADS: tier 2-braços arma detecção por EVENTO; senão desarma e DESCARTA timer.
            //    Mudança de linha mid-ADS REINICIA com o N da nova linha (comportamento 3 da funcional —
            //    âncora = max(edge de mira, entrada/mudança de linha) → re-ancorar AGORA cobre os dois casos).
            if (IsAdsCancelTier(line))
            {
                EnsureAimHooks(p);
                _aimAnchor = IsAimingNow(p) ? Time.time : -1f; // linha mudou/ativou mid-ADS → âncora AGORA
            }
            else
            {
                TearDownAimHooks();
                _aimAnchor = -1f;
            }
        }

        /// <summary>Âncora do efeito = braço COMPROMETIDO (abertura 6 confirmada): Left se zerado/quebrado,
        /// senão Right. Escapa do lookup method_16&lt;Tremor&gt;(Head) de stim negativo (AHC:2789-2806);
        /// wiring do flag é part-agnostic (Player.cs:28959-28962).</summary>
        private static EBodyPart PickArmAnchor(Player p)
        {
            IHealthController hc = p.HealthController;
            if (hc != null && (hc.IsBodyPartDestroyed(EBodyPart.LeftArm) || hc.IsBodyPartBroken(EBodyPart.LeftArm)))
                return EBodyPart.LeftArm;
            return EBodyPart.RightArm;
        }

        private static bool IsAimingNow(Player p)
        {
            return p.ProceduralWeaponAnimation != null && p.ProceduralWeaponAnimation.IsAiming; // uso existente — MovementPatches.cs:123 (pré-005)
        }

        private void OnAimingChanged(bool aiming)
        {
            _aimAnchor = aiming ? Time.time : -1f; // edge por EVENTO (Player.cs:20037; invocado só em mudança real :20146-20149)
        }

        private void OnHandsChanged(IHandsController c)
        {
            // Troca de arma: re-subscrever no novo FirearmController (AP-08 — lição stances CR-01-02: nada age
            // sobre controller trocado); timer reseta (controller novo nasce sem mira); lockout NÃO reseta
            // (é do jogador — funcional/corner). c não-firearm (meds/granada) → desarmado até voltar.
            try
            {
                HookFirearmController(c as Player.FirearmController);
                _aimAnchor = -1f;
            }
            catch (Exception ex)
            {
                // dispatch do HandsChangedEvent é vanilla — exceção nossa não pode quebrar a troca de mãos
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogError($"[Trauma2] ArmsConsumer.OnHandsChanged: {ex.Message}");
            }
        }

        private void EnsureAimHooks(Player p)
        {
            if (!ReferenceEquals(p, _handsHookedPlayer))
            {
                if (_handsHookedPlayer != null && _handsHandler != null)
                    _handsHookedPlayer.HandsChangedEvent -= _handsHandler;
                if (_handsHandler == null) _handsHandler = OnHandsChanged;
                p.HandsChangedEvent += _handsHandler; // ref: Player.cs:25544 (invocado em :31646)
                _handsHookedPlayer = p;
            }
            HookFirearmController(p.HandsController as Player.FirearmController);
        }

        private void HookFirearmController(Player.FirearmController fc)
        {
            if (ReferenceEquals(fc, _hookedFc)) return;
            if (_hookedFc != null && _aimHandler != null) _hookedFc.OnAimingChanged -= _aimHandler;
            _hookedFc = fc;
            if (_hookedFc != null)
            {
                if (_aimHandler == null) _aimHandler = OnAimingChanged;
                _hookedFc.OnAimingChanged += _aimHandler; // ref: Player.cs:20037 (evento público herdado de AbstractHandsController)
            }
        }

        private void TearDownAimHooks()
        {
            if (_hookedFc != null && _aimHandler != null) _hookedFc.OnAimingChanged -= _aimHandler;
            _hookedFc = null;
            if (_handsHookedPlayer != null && _handsHandler != null)
                _handsHookedPlayer.HandsChangedEvent -= _handsHandler;
            _handsHookedPlayer = null;
        }

        private void EnsureWatchdog(Player p)
        {
            IHealthController hc = p.HealthController;
            if (hc == null || ReferenceEquals(hc, _hookedHc)) return;
            TearDownWatchdog();
            if (_effectGoneHandler == null) _effectGoneHandler = OnEffectGone;
            hc.EffectResidualEvent += _effectGoneHandler; // mesmas assinaturas do motor — ref: IHealthController (Action<IEffect>)
            hc.EffectRemovedEvent += _effectGoneHandler;
            _hookedHc = hc;
        }

        private void TearDownWatchdog()
        {
            if (_hookedHc != null && _effectGoneHandler != null)
            {
                _hookedHc.EffectResidualEvent -= _effectGoneHandler;
                _hookedHc.EffectRemovedEvent -= _effectGoneHandler;
            }
            _hookedHc = null;
        }

        private void OnEffectGone(IEffect e)
        {
            // Watchdog (re-estabelecimento — PA-01-05): dispara SÍNCRONO dentro do state machine do AHC —
            // NUNCA re-aplicar aqui (AP-07: flag + próximo Update). A remoção PRÓPRIA não latcha pending:
            // TraumaTremor.Remove anula Owned ANTES do ForceResidue (IsOurs falha o ReferenceEquals).
            if (!TraumaTremor.IsOurs(e)) return;
            if (_localPlayer == null || TraumaEngine.GetLine(_localPlayer, TraumaRegion.Arms) == TraumaLine.None) return;
            _reestablishPending = true;
        }

        /// <summary>Desfaz os efeitos locais + hooks/timer. worldDead=false (state-exit/toggle-off):
        /// `TraumaTremor.Remove` — ForceResidue no AHC VIVO (inclusive downed — CR-02). worldDead=true
        /// (raid-end/world-swap): `TraumaTremor.Discard` — bookkeeping-only, o AHC morreu com o Player e
        /// ForceResidue dispararia eventos/sync em objeto destruído (PA-01-01; padrão TraumaLegsConsumer).</summary>
        private void TearDownLocal(string reason, bool worldDead = false)
        {
            if (worldDead) TraumaTremor.Discard(reason); else TraumaTremor.Remove(reason);
            TearDownAimHooks();
            TearDownWatchdog();
            _aimAnchor = -1f;
            _localPlayer = null;
            _localLine = TraumaLine.None;
            _reestablishPending = false;
            _reestablishCount = 0;
        }

        private void ResetLockout()
        {
            _lockoutUntil = 0f;
            _lockoutProfileId = null;
            _lockoutVoicePlayed = false;
            _nextVoiceTryAt = 0f;
            _lockoutVoiceSkipLogged = false;
            _lockoutIncapacitatedLogged = false;
        }

        private void ExecuteCancel(Player p)
        {
            // guard IsAiming cobre blindfire (early-return vanilla Player.cs:13711-13716); armas montadas usam
            // o mesmo funil (:13721-13724); "mira" de UsableItemController fica FORA (funil separado — funcional)
            if (p.HandsController is IFirearmHandsController f && IsAimingNow(p))
                f.SetAim(false); // funil vanilla: teardown completo (setter IsAiming :12136-12168) + ToggleAimPacket(-1) ao peer (P9)
            float lockout = Mathf.Clamp(TRLImmersiveCombatMedicinePlugin.ConfigArmsReAdsLockoutSeconds.Value, 1f, 1.5f);
            _lockoutUntil = Time.time + lockout;
            _lockoutProfileId = p.ProfileId;
            _lockoutVoicePlayed = false;      // throttle re-armado por janela (funcional 4)
            _nextVoiceTryAt = 0f;             // PA-02-06: piso/skip-log zerados por janela
            _lockoutVoiceSkipLogged = false;
            _lockoutIncapacitatedLogged = false;
            _aimAnchor = -1f;
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                $"[Trauma2] ads CANCEL {p.ProfileId} line={_localLine} n={LineCancelSeconds(_localLine):0.##} lockout={lockout:0.##}");
        }

        /// <summary>Chamado pelo prefix de SetAim (ArmsAimPatches): true = BLOQUEAR o re-ADS.</summary>
        internal static bool TryBlockReAds(Player p)
        {
            TraumaArmsConsumer inst = _instance;
            if (inst == null || p is null) return false;
            if (!IsActive()) return false; // toggle off mid-raid → lockout morto (corner da funcional)
            if (inst._lockoutUntil <= 0f || Time.time >= inst._lockoutUntil) return false;
            if (!string.Equals(inst._lockoutProfileId, p.ProfileId, StringComparison.Ordinal)) return false;
            if (!inst._lockoutVoicePlayed && Time.time >= inst._nextVoiceTryAt) // PA-02-06: piso de re-tentativa
            {
                // Guard de INCAPACIDADE (PA-02-05 — reusa a fonte ÚNICA do predicado do 004,
                // TraumaFallCycleConsumer.IsPauseCondition, tornada internal pelo CR-01-01 do 005 code-review):
                // sem voz durante blackout legado, janela de wake (IsFainted ainda true) e downed Fika (!IsAlive);
                // o BLOQUEIO do re-ADS em si CONTINUA.
                bool incapacitated = TraumaFallCycleConsumer.IsPauseCondition(p);
                if (!incapacitated)
                {
                    // Decisão PA-02-03: extensão accept-gated do utilitário do 004 — OnAgony, Combat|Dying,
                    // demand:true, importance:100 (tags alinhadas ao 004 — TraumaVoice.cs:21); NÃO toca o
                    // anti-spam do PlayStrong (canal separado por consumidor). Fura Busy de importance<100
                    // (PhraseSpeakerClass.cs:206-227); peers ouvem via PhrasePacket (fika FikaPlayer.cs:1093-1103).
                    bool played = TraumaVoice.TryPlayStrong(p);
                    if (played)
                    {
                        inst._lockoutVoicePlayed = true; // PA-01-02: só consome a janela se TOCOU
                        TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                            $"[Trauma2] ads LOCKOUT BLOCK {p.ProfileId} remaining={inst._lockoutUntil - Time.time:0.##} voice=true");
                    }
                    else
                    {
                        // Engolido por (a) Busy com frase importance>=100 em curso (100 <= Int_0 → skip,
                        // PhraseSpeakerClass.cs:206-210 — inclusive o OnAgony do PRÓPRIO 004) ou (b) Blocker de
                        // GRUPO (SpeakerManager.FreeToSpeak :106-131, sem bypass por demand/importance).
                        // PA-02-06: piso de 0,3s limita hold-por-frame sob Blocker a ~5 Plays/janela
                        // (P-005-A não provada); log skipped no máx 1x/janela; tentativa após o piso re-tenta
                        // (PA-01-02 — cadência humana segue tocando na primeira janela livre).
                        inst._nextVoiceTryAt = Time.time + 0.3f;
                        if (!inst._lockoutVoiceSkipLogged)
                        {
                            inst._lockoutVoiceSkipLogged = true;
                            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                                $"[Trauma2] ads LOCKOUT BLOCK {p.ProfileId} remaining={inst._lockoutUntil - Time.time:0.##} voice=skipped(busy|blocked)");
                        }
                    }
                }
                else if (!inst._lockoutIncapacitatedLogged)
                {
                    // CR-01-02: sem esta linha, a tentativa bloqueada durante incapacidade não deixava rastro
                    // no log — impossível distinguir de "o jogador nem tentou re-ADS" ao validar o corner do §8.
                    inst._lockoutIncapacitatedLogged = true;
                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                        $"[Trauma2] ads LOCKOUT BLOCK {p.ProfileId} remaining={inst._lockoutUntil - Time.time:0.##} voice=suppressed(incapacitated)");
                }
            }
            return true; // prefix skipa o original: IsAiming não muda, AimingInterruptedByOverlap false → sem pacote (P9 corrigido)
        }

        private void Update()
        {
            GameWorld gw = Singleton<GameWorld>.Instance;
            if (gw == null)
            {
                // padrão N1: mundo morreu — efeito/hooks morreram com o Player; só limpar bookkeeping (AC-10);
                // worldDead=true → TraumaTremor.Discard, NUNCA ForceResidue em AHC morto (PA-01-01)
                TearDownLocal("raid-end", worldDead: true);
                ResetLockout();
                _trackedWorld = null;
                _wasActive = IsActive();
                return;
            }
            if (!ReferenceEquals(gw, _trackedWorld))
            {
                // World-swap sem passar por null (transit) — espelha a detecção do motor/003
                TearDownLocal("world-swap", worldDead: true);
                ResetLockout();
                _trackedWorld = gw;
            }

            bool active = IsActive();
            if (_wasActive && !active)
            {
                // Desligar mid-raid: tremor removido (ForceResidue — inclusive downed, CR-02) + lockout
                // cancelado + hooks off (corner da funcional)
                TearDownLocal("toggle-off");
                ResetLockout();
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo("[Trauma2] arms consumer OFF — tremor removido, lockout cancelado");
            }
            else if (!_wasActive && active)
            {
                // Religar mid-raid: estabelecer do snapshot SEM toast/voz (paridade com establishing).
                // Humano local = MainPlayer (padrão 004); linha AdsCancel-tier + já mirando → âncora = AGORA
                // (ApplyLine re-ancora — sem cancelamento retroativo). Dependência internal: TraumaEngine.IsOwnedHere.
                Player mp = gw.MainPlayer;
                if (mp != null && !mp.IsAI && TraumaEngine.IsOwnedHere(mp))
                {
                    TraumaLine line = TraumaEngine.GetLine(mp, TraumaRegion.Arms);
                    if (line != TraumaLine.None)
                    {
                        ApplyLine(mp, line);
                        TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo("[Trauma2] arms consumer ON — estado estabelecido do snapshot");
                    }
                }
            }
            _wasActive = active;
            if (!active) return;

            // Poda oportunista (padrão sweep 003/004): morte não publica transição (comportamento 1 do motor)
            // e saída publicada com o toggle off se perde — GetLine==None com efeito aplicado é stale.
            if (_localPlayer != null && TraumaEngine.GetLine(_localPlayer, TraumaRegion.Arms) == TraumaLine.None)
                TearDownLocal("stale");

            if (_reestablishPending)
            {
                Player p = _localPlayer;
                TraumaLine gl = p != null ? TraumaEngine.GetLine(p, TraumaRegion.Arms) : TraumaLine.None;
                if (p == null || gl == TraumaLine.None)
                {
                    _reestablishPending = false; // linha inativa → descarta pending
                }
                else if (Time.time >= _nextReestablishAt) // piso de 0,5s entre tentativas (PA-01-05)
                {
                    _reestablishPending = false;
                    _nextReestablishAt = Time.time + 0.5f;
                    if (p.HealthController != null && p.HealthController.IsAlive) // re-APPLY nunca em cadáver (Remove sim — CR-02)
                    {
                        _reestablishCount++;
                        if (_reestablishCount > 3 && !_reestablishStormWarned)
                        {
                            _reestablishStormWarned = true;
                            TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning(
                                "[Trauma2] tremor re-aplicado >3x na mesma linha — possível removedor externo por tick (conflito de mod)");
                        }
                        TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] tremor REESTABLISH {p.ProfileId} line={gl}");
                        TraumaTremor.Apply(p, PickArmAnchor(p));
                    }
                }
            }

            // Deadline do timer (SÓ com timer armado — 1 comparação de float/frame; zero polling de estado)
            if (_aimAnchor >= 0f && _localPlayer != null && IsAdsCancelTier(_localLine)
                && Time.time - _aimAnchor >= LineCancelSeconds(_localLine))
            {
                ExecuteCancel(_localPlayer);
            }
        }
    }
}
