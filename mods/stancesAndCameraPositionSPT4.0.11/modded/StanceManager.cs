using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using BepInEx.Configuration;
using CameraRotationMod.Patches;
using Comfort.Common;
using EFT;
using HarmonyLib;

namespace CameraRotationMod
{
    public enum Stance
    {
        Default,
        Stance1,
        Stance2,
        Stance3
    }

    /// <summary>
    /// Manages stance toggling between Default, Stance1, and Stance2 positions/rotations
    /// </summary>
    public static class StanceManager
    {
        // Backing field para a property CurrentStance — permite setter customizado que
        // dispara OnStanceChanged sem alterar os 3 call-sites internos que mutam.
        private static Stance _currentStanceField = Stance.Default;
        public static Stance CurrentStance
        {
            get => _currentStanceField;
            private set
            {
                if (value == _currentStanceField) return;
                var prev = _currentStanceField;
                _currentStanceField = value;
                OnStanceChanged(prev, value);
            }
        }

        public static void SetStance(Stance newStance)
        {
            if (CurrentStance != newStance)
            {
                Plugin.Logger.LogInfo($"[Spy] SetStance called: {CurrentStance} -> {newStance}");
            }
            CurrentStance = newStance;
        }



        private static void ApplyUserStance(Stance to)
        {
            if (CurrentStance != to)
            {
                Plugin.Logger.LogInfo($"[Stances] User changed stance from {CurrentStance} to {to}");
                CameraRotationMod.Patches.ApplyComplexRotationPatch.LogNextFrame = true;
            }
            CurrentStance = to;
        }

        public static bool IsInStance => CurrentStance != Stance.Default;
        
        private static ConfigEntry<KeyCode> _stanceToggleKeyConfig;
        private static ConfigEntry<bool> _enableMouseWheelCycleConfig;
        private static ConfigEntry<KeyCode> _mouseWheelModifierKeyConfig;
        private static float _lastScrollTime = 0f;
        private const float ScrollCooldown = 0.15f; // Prevent scroll spam

        // Tac Sprint variables - track state to avoid setting animator every frame
        public static bool _isTacSprintActive = false;
        private static bool _wasAiming = false;
        
        // Tac Sprint reset delay variables
        private static bool _isWaitingToResetTacSprint = false;
        private static float _tacSprintResetTimer = 0f;
        
        // ADS Tracking for Wiggle
        private static bool _wasAimingGlobal = false;

        // Track GameWorld to detect raid changes and reset state
        private static GameWorld _lastGameWorld = null;
        
        // Cached GameWorld reference for this frame (avoids multiple Singleton lookups)
        private static GameWorld _cachedGameWorld = null;
        private static int _cachedGameWorldFrame = -1;

        // Item 008: Action Stance variables
        private static bool _isActionStanceActive = false;
        private static Stance _preActionStance = Stance.Default;
        private static float _actionStanceStartTime = 0f;
        private static ConfigEntry<bool> _enableActionStanceSwapConfig;
        
        // Cached weapon properties - rebuilt when weapon changes
        private static Player.FirearmController _lastFirearmController = null;
        private static bool _cachedIsBullpup = false;
        private static int _cachedWeaponCellSizeX = 1;
        
        // Cached stance vectors - rebuilt when config changes
        private static bool _stanceValuesDirty = true;
        private static Vector3 _cachedADSRotation;
        private static Vector3 _cachedADSPosition;
        private static Vector3 _cachedStance1Rotation;
        private static Vector3 _cachedStance1Position;
        private static Vector3 _cachedStance2Rotation;
        private static Vector3 _cachedStance2Position;
        private static Vector3 _cachedStance3Rotation;
        private static Vector3 _cachedStance3Position;
        private static Vector3 _cachedDefaultPosition;
        
        // Cached sprint enabled flag - rebuilt when config changes
        private static bool _sprintEnabledDirty = true;
        private static bool _cachedAnySprintEnabled = false;
        
        /// <summary>
        /// Mark stance values as needing recalculation (called when config changes)
        /// </summary>
        public static void MarkStanceValuesDirty() => _stanceValuesDirty = true;
        
        /// <summary>
        /// Mark sprint enabled flag as needing recalculation (called when sprint config changes)
        /// </summary>
        public static void MarkSprintEnabledDirty() => _sprintEnabledDirty = true;

        public static void Initialize(ConfigEntry<KeyCode> stanceToggleKeyConfig)
        {
            _stanceToggleKeyConfig = stanceToggleKeyConfig;
            _enableMouseWheelCycleConfig = Plugin._EnableMouseWheelCycle;
            _mouseWheelModifierKeyConfig = Plugin._MouseWheelModifierKey;
            _enableActionStanceSwapConfig = Plugin._EnableActionStanceSwap;
        }

        public static void Update()
        {
            // backlog 002 F5 — aplicar stance inicial pendente (raid begin) no primeiro frame em
            // que ProceduralWeaponAnimation.HandsContainer estiver disponível.
            TryApplyPendingInitialStance();

            // backlog 002 F4 — despachar 2-frame pulse agendado (resurrect/reset).
            TryDispatchPendingResurrect();

            // backlog 002 F4 — guard de stale para weapon swap durante hold (sem button-up).
            EvaluateSnapStaleTimeout();
            
            // Block stance switching while sprinting, aiming, mounting (vanilla or custom), or prone
            var gameWorld = GetCachedGameWorld();
            bool isNativeMounting = false;
            bool isAiming = false;
            bool isInProne = false;
            bool isStationary = false;

            if (gameWorld?.MainPlayer != null)
            {
                var pwa = gameWorld.MainPlayer.ProceduralWeaponAnimation;
                if (pwa != null)
                {
                    isNativeMounting = pwa.IsMountedState || pwa.IsBipodUsed;
                    isAiming = pwa.IsAiming;
                }
                isInProne = gameWorld.MainPlayer.IsInPronePose;
                var mc = gameWorld.MainPlayer.MovementContext;
                isStationary = mc != null && mc.IsStationaryWeaponInHands;   // item 013: arma montada do cenário
            }

            _wasAimingGlobal = isAiming;

            bool isSprinting = gameWorld?.MainPlayer?.IsSprintEnabled == true;

            if (isNativeMounting || isInProne || isStationary)
            {
                // Se deitou, apoiou (bipé) ou entrou em arma montada, quebra a Action Stance e trava controles
                if (_isActionStanceActive) EndActionStance(forceCancel: true);

                // Forçar Stance 0 (item 013: inclui arma montada do cenário — evita desalinhamento visual)
                if (CurrentStance != Stance.Default)
                {
                    SetStance(Stance.Default);
                }
                return;
            }

            if (isSprinting)
            {
                if (_isActionStanceActive) EndActionStance(forceCancel: true);
                // item 013 (fix-01): NÃO forçar Stance 0 ao correr. A corrida acontece inteiramente na
                // stance atual (0/1/2/3), sem qualquer transição ou "flash" pela Stance 0. TacSprint normal.
                return; // Trava as hotkeys normais durante o sprint
            }

            // Action Stance: o término é detectado via ActionStanceOnIdlePatch (OnIdleStartEvent).
            // Sem polling aqui — o evento nativo é preciso e sem gaps.

            // Se estiver mirando (ADS), não processamos as hotkeys ou a roda do mouse,
            // mas ainda queremos rodar a ActionStance e UpdateTacSprint, então apenas pulamos a entrada.
            if (!isAiming)
            {
                // Process hotkeys first (returns true if handled)
                if (HandleStanceHotkeys()) return;

                // Toggle logic
                if (UnityEngine.Input.GetKeyDown(_stanceToggleKeyConfig.Value))
                {
                    ApplyUserStance(GetNextStance(CurrentStance));
                }

                // Mouse wheel cycling
                if (_enableMouseWheelCycleConfig?.Value == true)
                {
                    // Check if modifier key is held
                    if (UnityEngine.Input.GetKey(_mouseWheelModifierKeyConfig.Value))
                    {
                        float scrollDelta = UnityEngine.Input.GetAxis("Mouse ScrollWheel");

                        // Check cooldown to prevent rapid cycling
                        if (scrollDelta != 0 && Time.time - _lastScrollTime > ScrollCooldown)
                        {
                            // backlog 002 F2 — branch por modo de scroll.
                            switch (Plugin._MouseWheelScrollMode?.Value ?? ScrollMode.Linear)
                            {
                                case ScrollMode.Cycle:
                                    ApplyUserStance(scrollDelta > 0
                                        ? GetNextStance(CurrentStance)
                                        : GetPreviousStance(CurrentStance));
                                    break;
                                case ScrollMode.Linear:
                                    HandleLinearScroll(scrollDelta);
                                    break;
                            }
                            _lastScrollTime = Time.time;
                        }
                    }
                }
            }
        }

        // ==========================================================================
        // backlog 002 F2 — modo Linear de scroll
        // ==========================================================================

        /// <summary>
        /// Eixo fixo: Stance 1 (topo) ↔ Stance 0 (centro) ↔ Stance 2 (fundo).
        /// Stance 3 é off-axis: scroll-up vai pra Stance 1, scroll-down vai pra Stance 2.
        /// Toggles `_EnableStance*InCycle` são ignorados em Linear.
        /// </summary>
        private static void HandleLinearScroll(float scrollDelta)
        {
            Stance next = CurrentStance;
            if (scrollDelta > 0) // scroll-up
            {
                next = CurrentStance switch
                {
                    Stance.Stance2 => Stance.Default,
                    Stance.Default => Stance.Stance1,
                    Stance.Stance1 => Stance.Stance1,        // já no topo, no-op
                    Stance.Stance3 => Stance.Stance1,        // off-axis → topo
                    _ => CurrentStance,
                };
            }
            else if (scrollDelta < 0) // scroll-down
            {
                next = CurrentStance switch
                {
                    Stance.Stance1 => Stance.Default,
                    Stance.Default => Stance.Stance2,
                    Stance.Stance2 => Stance.Stance2,        // já no fundo, no-op
                    Stance.Stance3 => Stance.Stance2,        // off-axis → fundo
                    _ => CurrentStance,
                };
            }
            if (next != CurrentStance) ApplyUserStance(next);
        }

        // ==========================================================================
        // backlog 002 F3 — hotkeys dedicadas por stance
        // ==========================================================================

        /// <summary>
        /// Processa as 4 hotkeys (Stance0..Stance3). Retorna true se uma matchou — caller
        /// usa para early-return e evitar double-fire com tecla V (PA-04-02).
        /// Ordem crescente garante que hotkey de menor índice prioriza quando duas teclas coincidem (PA-01-04).
        /// </summary>
        private static bool HandleStanceHotkeys()
        {
            var gw = GetCachedGameWorld();
            if (gw?.MainPlayer == null) return false;
            if (gw.MainPlayer.IsSprintEnabled) return false;                                  // bloqueio sprint
            if (gw.MainPlayer.ProceduralWeaponAnimation?.IsAiming == true) return false;      // ignora em ADS
            if (gw.MainPlayer.IsInPronePose) return false;                                    // bloqueio prone

            if (TryHotkey(Plugin._Stance0Hotkey, Stance.Default)) return true;
            if (TryHotkey(Plugin._Stance1Hotkey, Stance.Stance1)) return true;
            if (TryHotkey(Plugin._Stance2Hotkey, Stance.Stance2)) return true;
            if (TryHotkey(Plugin._Stance3Hotkey, Stance.Stance3)) return true;
            return false;
        }

        private static bool TryHotkey(ConfigEntry<KeyCode> entry, Stance target)
        {
            var key = entry?.Value ?? KeyCode.None;
            if (key == KeyCode.None) return false;
            if (!UnityEngine.Input.GetKeyDown(key)) return false;

            // Toggle: pressionar a tecla da stance ativa retorna a Default — exceto a própria Default.
            if (CurrentStance == target)
            {
                if (target != Stance.Default) ApplyUserStance(Stance.Default);
                return true;
            }
            ApplyUserStance(target);
            return true;
        }

        // ==========================================================================
        // backlog 002 F4 — Snap to Stance 0 on Fire (intercept-and-resurrect)
        // ==========================================================================
        // Estratégia: Prefix em SetTriggerPressed da nested operation-base de FirearmController.
        // - press=true em snap-elegível → bloqueia trigger (return false), snap imediato.
        // - press=false: agenda ressurreição para o próximo frame se elapsed >= threshold.
        // - Frame N+1: dispatch synthetic true. Frame N+2: dispatch synthetic false (para fullauto não runaway).
        // - Reentry guard via `[ThreadStatic] _inSyntheticCall` no patch.

        private const float SnapIdleSentinel = -1f;
        // ref: CR-01-06 — fallback default; o valor efetivo vem de Plugin._SnapStaleTimeoutSec.
        private const float SnapStaleTimeoutDefaultSec = 2f;
        private static float _triggerDownTimeUnscaled = SnapIdleSentinel;
        private static bool  _snapInterceptActive;
        // ref: CR-01-02 — FirearmController que disparou o intercept; OnTriggerUpAfterIntercept valida que
        // o button-up vem do MESMO FC, evitando tiro espúrio se houver weapon swap mid-intercept.
        // 06-fix-01: trocado de `object operationInstance` para `Player.FirearmController` (patch target mudou).
        private static Player.FirearmController _interceptFc;

        // ==========================================================================
        // Item 008: Action Stance (Reload / Check)
        // ==========================================================================

        public static void StartActionStance()
        {
            if (_enableActionStanceSwapConfig?.Value != true) return;
            if (_isActionStanceActive) return;

            var gw = GetCachedGameWorld();
            if (gw?.MainPlayer?.IsSprintEnabled == true) return;
            if (gw?.MainPlayer?.ProceduralWeaponAnimation?.IsMountedState == true) return; // Não levanta a arma se montado (vanilla)
            
            Plugin.Logger.LogInfo($"[Spy] StartActionStance called. CurrentStance: {CurrentStance}");
            if (CurrentStance != Stance.Default)
            {
                _preActionStance = CurrentStance;
                SetStance(Stance.Default); // Força para Default (Vanilla) durante a recarga
            }
            else
            {
                // Já estava em Default, só marca que está ativo para não quebrar
                _preActionStance = Stance.Default;
            }
            
            _isActionStanceActive = true;
            _actionStanceStartTime = Time.time;
        }

        public static void EndActionStance(bool forceCancel = false)
        {
            if (!_isActionStanceActive) return;

            Plugin.Logger.LogInfo($"[Spy] EndActionStance called. forceCancel={forceCancel}, Time delta={Time.time - _actionStanceStartTime}");
            // Debounce: ignorar OnIdleStartEvent disparados nos primeiros 0.3s (podem ser idle
            // events do próprio início da operação antes da animação realmente começar).
            if (!forceCancel && (Time.time - _actionStanceStartTime < 0.3f)) return;

            _isActionStanceActive = false;

            // Reseta o estado do manual chambering para que o próximo tiro/operação de engatilhamento manual funcione normalmente
            Patches.ManualChamberingState.CanLoadChamber = true;
            Patches.ManualChamberingState.BlockChambering = false;

            // Se forceCancel for true (ex: começou a correr no meio do reload),
            // a própria lógica de sprint ou outras resetam a stance, então não voltamos para previousStance forçadamente
            // Se for falso, restauramos a stance original se estivermos em Default
            if (!forceCancel && CurrentStance == Stance.Default)
            {
                SetStance(_preActionStance);
            }
        }

        // PA-02-03: deferred resurrection (frame N+1: synthetic true).
        private static Player.FirearmController _pendingResurrectFc;
        private static MethodBase                _pendingResurrectMethod;
        // PA-03-01: 2-frame pulse (frame N+2: synthetic false → para fullauto após ~1 tiro).
        private static Player.FirearmController _pendingResetFc;
        private static MethodBase                _pendingResetMethod;
        // 06-fix-04: snapshot do IsTriggerPressed natural ANTES do synthetic true (frame N+1).
        // Necessário porque o synthetic true seta fc.IsTriggerPressed=true como side-effect, então
        // checar IsTriggerPressedNaturally() no frame N+2 não distingue "synthetic" de "natural".
        // Se este snapshot for false e IsTriggerPressedNaturally() for true no N+2 → é o synthetic
        // (envia false pra parar fullauto). Se for true em ambos → usuário re-apertou natural antes
        // do nosso synthetic (double-tap real — preserva sem reset).
        private static bool _wasNaturallyPressedBeforeResurrect;

        /// <summary>
        /// Chamado pelo Prefix de SetTriggerPressed quando press==true. Retorna true se deve
        /// BLOQUEAR o original (skip) — quando snap é elegível.
        /// 06-fix-01: parâmetro agora é Player.FirearmController (patch target trocado).
        /// ref: CR-01-04 — `IsHoldingFirearm()` removido; o caller (SnapFireTriggerPatch.Prefix)
        /// já validou que `fc == MainPlayer.HandsController` é `Player.FirearmController` antes
        /// de chamar esta função.
        /// </summary>
        public static bool TryInterceptTriggerDown(Player.FirearmController fc)
        {
            var gw = GetCachedGameWorld();
            if (gw?.MainPlayer == null) return false;
            
            if (gw.MainPlayer.ProceduralWeaponAnimation?.IsAiming == true)
            {
                Plugin.Logger.LogDebug("[F4] TryInterceptTriggerDown: IGNORADO (está mirando/ADS)");
                return false; 
            }
            if (CurrentStance == Stance.Default)
            {
                return false;
            }
            if (!Plugin._stanceConfigs.TryGetValue(CurrentStance, out var cfg))
            {
                return false;
            }
            if (cfg.SnapToStance0OnFire == null)
            {
                return false;
            }
            if (!cfg.SnapToStance0OnFire.Value)
            {
                Plugin.Logger.LogDebug($"[F4] TryInterceptTriggerDown: IGNORADO (SnapToStance0OnFire desligado na Stance {CurrentStance})");
                return false;                             
            }

            Plugin.Logger.LogDebug($"[F4] TryInterceptTriggerDown: INTERCEPTADO na Stance {CurrentStance}! Forçando Snap para Default (Vanilla)");
            ApplyUserStance(Stance.Default);

            // Marcar intercept ativo: timer começa, próximo button-up decide se ressuscita.
            _triggerDownTimeUnscaled = Time.unscaledTime;
            _snapInterceptActive = true;
            _interceptFc = fc;  // ref: CR-01-02 — anti-swap
            return true;   // Prefix retorna false → operation NÃO recebe trigger=true → tiro NÃO sai.
        }

        /// <summary>
        /// Chamado pelo Prefix de SetTriggerPressed quando press==false (button-up).
        /// Se elapsed >= threshold, agenda ressurreição para o próximo frame (PA-02-03).
        /// 06-fix-01: parâmetro agora é Player.FirearmController. Anti-swap valida `fc == _interceptFc`.
        /// </summary>
        public static void OnTriggerUpAfterIntercept(Player.FirearmController fc, MethodBase originalMethod)
        {
            if (!_snapInterceptActive) return;

            // ref: CR-01-02 — abortar se o FirearmController mudou entre down e up (weapon swap).
            if (fc != _interceptFc)
            {
                _snapInterceptActive = false;
                _triggerDownTimeUnscaled = SnapIdleSentinel;
                _interceptFc = null;
                Plugin.Logger.LogDebug("[F4] intercept abortado: FirearmController mudou entre button-down e button-up");
                return;
            }

            _snapInterceptActive = false;
            _interceptFc = null;

            float elapsedMs = (Time.unscaledTime - _triggerDownTimeUnscaled) * 1000f;
            _triggerDownTimeUnscaled = SnapIdleSentinel;

            int threshold = Plugin._SnapFireThreshold?.Value ?? 200;
            if (elapsedMs < threshold) return; // clique único — snap já aconteceu no down, sem fire.

            // Hold >= threshold → agendar synthetic true (frame N+1).
            _pendingResurrectFc      = fc;
            _pendingResurrectMethod  = originalMethod;
        }

        /// <summary>
        /// PA-03-01: chamado no início de Update. Despacha o 2-frame pulse:
        /// 1) frame N+2: synthetic false (reset do hold do frame N+1) — para fullauto após ~1 tiro.
        /// 2) frame N+1: synthetic true (resurrect) e agenda o reset do próximo frame.
        /// PA-03-02: valida que operation ainda é a CurrentOperation antes de cada Invoke.
        /// PA-04-03: skipa o reset se trigger está pressionado naturalmente (double-tap).
        /// </summary>
        private static void TryDispatchPendingResurrect()
        {
            // 1. Frame N+2: synthetic false (reset agendado no frame anterior).
            if (_pendingResetMethod != null)
            {
                var resetFc = _pendingResetFc;
                var resetMethod = _pendingResetMethod;
                _pendingResetFc = null;
                _pendingResetMethod = null;

                if (!IsFirearmControllerStillCurrent(resetFc))
                {
                    // PA-03-02: weapon swap / FC change — drop.
                }
                else if (_wasNaturallyPressedBeforeResurrect && IsTriggerPressedNaturally())
                {
                    // 06-fix-04: double-tap real — usuário re-apertou naturalmente ANTES do nosso
                    // synthetic true (frame N+0 do resurrect). Preserva o fire natural.
                    // Bug anterior (PA-04-03): checava só IsTriggerPressedNaturally(), que retorna
                    // true como side-effect do nosso próprio synthetic → reset era sempre skipado
                    // → fullauto runaway sem parar até esvaziar o magazine.
                }
                else
                {
                    SnapFireTriggerPatch.RaiseSyntheticTrigger(resetFc, pressed: false);
                }
                _wasNaturallyPressedBeforeResurrect = false;
            }

            // 2. Frame N+1: synthetic true (resurrect) + agenda reset.
            if (_pendingResurrectMethod != null)
            {
                var fc = _pendingResurrectFc;
                var method = _pendingResurrectMethod;
                _pendingResurrectFc = null;
                _pendingResurrectMethod = null;

                if (!IsFirearmControllerStillCurrent(fc))
                {
                    Plugin.Logger.LogDebug("[F4] resurrect skipped: FirearmController mudou entre frames");
                    return;
                }
                // 06-fix-04: snapshot ANTES do synthetic — usado no frame N+2 para distinguir
                // double-tap natural do side-effect do nosso próprio synthetic.
                _wasNaturallyPressedBeforeResurrect = IsTriggerPressedNaturally();
                SnapFireTriggerPatch.RaiseSyntheticTrigger(fc, pressed: true);
                _pendingResetFc = fc;
                _pendingResetMethod = method;
            }
        }

        /// <summary>
        /// PA-01-05: limpa intercept ativo se ficar pendurado > 2s sem button-up
        /// (provável weapon swap durante hold).
        /// </summary>
        private static void EvaluateSnapStaleTimeout()
        {
            if (!_snapInterceptActive) return;
            // ref: CR-01-06 — timeout agora vem de ConfigEntry advanced, com fallback ao default.
            float timeout = Plugin._SnapStaleTimeoutSec?.Value ?? SnapStaleTimeoutDefaultSec;
            if (Time.unscaledTime - _triggerDownTimeUnscaled <= timeout) return;
            _snapInterceptActive = false;
            _triggerDownTimeUnscaled = SnapIdleSentinel;
            _interceptFc = null;  // ref: CR-01-02 + 06-fix-01
            Plugin.Logger.LogDebug("[F4] snap intercept timed out (likely weapon swap during hold).");
        }

        /// <summary>
        /// PA-04-03: detecta se o gatilho está pressionado por input natural (double-tap rápido).
        /// ref: Player.cs:2714 — FirearmController.IsTriggerPressed.
        /// </summary>
        private static bool IsTriggerPressedNaturally()
        {
            var gw = GetCachedGameWorld();
            var fc = gw?.MainPlayer?.HandsController as Player.FirearmController;
            return fc?.IsTriggerPressed == true;
        }

        /// <summary>
        /// PA-03-02 + 06-fix-01: valida que `fc` ainda é o FirearmController ativo no MainPlayer.
        /// Cobre weapon swap entre frames. Simplificado vs versão anterior (não precisa de
        /// CurrentOperationGetter porque comparamos diretamente HandsController == fc).
        /// </summary>
        private static bool IsFirearmControllerStillCurrent(Player.FirearmController fc)
        {
            if (fc == null) return false;
            var gw = GetCachedGameWorld();
            return gw?.MainPlayer?.HandsController == fc;
        }

        // ==========================================================================
        // backlog 002 F5 — iniciar raid em Stance 3 - Low Ready
        // ==========================================================================

        private static Stance? _pendingInitialStance;

        /// <summary>
        /// Chamado por GameWorldOnGameStartedPatch (raid begin) quando o toggle F5 está habilitado.
        /// A aplicação efetiva é adiada para o primeiro frame em que ProceduralWeaponAnimation.HandsContainer
        /// estiver pronto (TryApplyPendingInitialStance).
        /// </summary>
        public static void QueueInitialStance(Stance s) => _pendingInitialStance = s;

        /// <summary>
        /// Aplica a stance inicial pendente assim que PWA.HandsContainer existir. Set imediato
        /// (sem animação) — SpringGetPatch.ResetState() seguido de CurrentStance = target faz
        /// o próximo postfix de Spring.Get inicializar com `_currentRotation = desiredRotation`.
        /// </summary>
        private static void TryApplyPendingInitialStance()
        {
            if (_pendingInitialStance == null) return;
            var gw = GetCachedGameWorld();
            if (gw?.MainPlayer?.ProceduralWeaponAnimation?.HandsContainer == null) return;
            // PA-04-04: F5 só em raid. Hideout não deve receber stance inicial.
            if (gw.MainPlayer is HideoutPlayer) return;

            var target = _pendingInitialStance.Value;
            _pendingInitialStance = null;

            // Set imediato — bypass spring lerp.
            CurrentStance = target;
        }

        /// <summary>
        /// Get the next enabled stance in the cycle, skipping disabled stances.
        /// backlog 002 F1: usa `_IncludeStance0InCycle` (lógica explícita) em vez de `_UseOnlyStances` (invertida).
        /// </summary>
        private static Stance GetNextStance(Stance current)
        {
            int maxAttempts = 5; // Prevent infinite loops
            int attempts = 0;

            Stance next = current;

            do
            {
                // Move to next stance in sequence
                next = next switch
                {
                    Stance.Default => Stance.Stance1,
                    Stance.Stance1 => Stance.Stance2,
                    Stance.Stance2 => Stance.Stance3,
                    Stance.Stance3 => Stance.Default,
                    _ => Stance.Default
                };

                attempts++;

                if (IsStanceEnabled(next))
                    return next;

            } while (attempts < maxAttempts && next != current);

            // If we cycled through everything and nothing is enabled, return current
            return current;
        }

        /// <summary>
        /// Get the previous enabled stance in the cycle, skipping disabled stances (for scroll down).
        /// </summary>
        private static Stance GetPreviousStance(Stance current)
        {
            int maxAttempts = 5; // Prevent infinite loops
            int attempts = 0;

            Stance prev = current;

            do
            {
                // Move to previous stance in sequence (reverse order)
                prev = prev switch
                {
                    Stance.Default => Stance.Stance3,
                    Stance.Stance1 => Stance.Default,
                    Stance.Stance2 => Stance.Stance1,
                    Stance.Stance3 => Stance.Stance2,
                    _ => Stance.Default
                };

                attempts++;

                if (IsStanceEnabled(prev))
                    return prev;

            } while (attempts < maxAttempts && prev != current);

            // If we cycled through everything and nothing is enabled, return current
            return current;
        }

        /// <summary>
        /// Check if a stance is enabled and should be included in the cycle.
        /// backlog 002 F1: Stance.Default usa `_IncludeStance0InCycle` (default false).
        /// </summary>
        private static bool IsStanceEnabled(Stance stance)
        {
            return stance switch
            {
                Stance.Default => Plugin._IncludeStance0InCycle?.Value ?? false,
                Stance.Stance1 => Plugin._EnableStance1?.Value ?? true,
                Stance.Stance2 => Plugin._EnableStance2?.Value ?? true,
                Stance.Stance3 => Plugin._EnableStance3?.Value ?? true,
                _ => false,
            };
        }
        
        /// <summary>
        /// Check if player is holding a firearm (not melee, empty hands, or using items)
        /// </summary>
        public static bool IsHoldingFirearm()
        {
            var gameWorld = GetCachedGameWorld();
            if (gameWorld?.MainPlayer == null)
                return false;
            
            // HandsController will be FirearmController when holding a gun
            // It will be different controller types for melee, meds, food, grenades, empty hands
            return gameWorld.MainPlayer.HandsController is Player.FirearmController;
        }
        
        /// <summary>
        /// Get cached GameWorld reference to avoid multiple Singleton lookups per frame
        /// </summary>
        public static GameWorld GetCachedGameWorld()
        {
            int currentFrame = Time.frameCount;
            if (_cachedGameWorldFrame != currentFrame)
            {
                _cachedGameWorld = Singleton<GameWorld>.Instance;
                _cachedGameWorldFrame = currentFrame;
            }
            return _cachedGameWorld;
        }
        
        /// <summary>
        /// Rebuild cached stance vectors from config values
        /// </summary>
        private static void RebuildCachedStanceValues()
        {
            if (!_stanceValuesDirty)
                return;
                
            _cachedADSRotation = new Vector3(
                Plugin._ADSHandsPitchRotation?.Value ?? 0f,
                Plugin._ADSHandsYawRotation?.Value ?? 0f,
                Plugin._ADSHandsRollRotation?.Value ?? 0f
            );
            
            _cachedADSPosition = new Vector3(
                Plugin._ADSHandsSidewaysOffset?.Value ?? 0f,        // X (Sideways)
                Plugin._ADSHandsForwardBackwardOffset?.Value ?? 0f, // Y (Local Y = Forward/Backward in Tarkov)
                Plugin._ADSHandsUpDownOffset?.Value ?? 0f           // Z (Local Z = Up/Down in Tarkov)
            );
            
            _cachedStance1Rotation = new Vector3(
                Plugin._Stance1HandsPitchRotation?.Value ?? 0f,
                Plugin._Stance1HandsYawRotation?.Value ?? 0f,
                Plugin._Stance1HandsRollRotation?.Value ?? 0f
            );
            
            _cachedStance1Position = new Vector3(
                Plugin._Stance1HandsSidewaysOffset?.Value ?? 0f,        // X (Sideways)
                Plugin._Stance1HandsForwardBackwardOffset?.Value ?? 0f, // Y (Local Y = Forward/Backward in Tarkov)
                Plugin._Stance1HandsUpDownOffset?.Value ?? 0f           // Z (Local Z = Up/Down in Tarkov)
            );
            
            _cachedStance2Rotation = new Vector3(
                Plugin._Stance2HandsPitchRotation?.Value ?? 0f,
                Plugin._Stance2HandsYawRotation?.Value ?? 0f,
                Plugin._Stance2HandsRollRotation?.Value ?? 0f
            );
            
            _cachedStance2Position = new Vector3(
                Plugin._Stance2HandsSidewaysOffset?.Value ?? 0f,        // X (Sideways)
                Plugin._Stance2HandsForwardBackwardOffset?.Value ?? 0f, // Y (Local Y = Forward/Backward in Tarkov)
                Plugin._Stance2HandsUpDownOffset?.Value ?? 0f           // Z (Local Z = Up/Down in Tarkov)
            );
            
            _cachedStance3Rotation = new Vector3(
                Plugin._Stance3HandsPitchRotation?.Value ?? 0f,
                Plugin._Stance3HandsYawRotation?.Value ?? 0f,
                Plugin._Stance3HandsRollRotation?.Value ?? 0f
            );
            
            _cachedStance3Position = new Vector3(
                Plugin._Stance3HandsSidewaysOffset?.Value ?? 0f,        // X (Sideways)
                Plugin._Stance3HandsForwardBackwardOffset?.Value ?? 0f, // Y (Local Y = Forward/Backward in Tarkov)
                Plugin._Stance3HandsUpDownOffset?.Value ?? 0f           // Z (Local Z = Up/Down in Tarkov)
            );
            
            _cachedDefaultPosition = (Plugin._DefaultHandsPositionEnabled?.Value ?? false)
                ? new Vector3(
                    Plugin._DefaultHandsSidewaysOffset?.Value ?? 0f,        // X (Sideways)
                    Plugin._DefaultHandsForwardBackwardOffset?.Value ?? 0f, // Y (Local Y = Forward/Backward in Tarkov)
                    Plugin._DefaultHandsUpDownOffset?.Value ?? 0f           // Z (Local Z = Up/Down in Tarkov)
                )
                : Vector3.zero;
            
            _stanceValuesDirty = false;
        }

        /// <summary>
        /// Get the current target rotation based on stance state and ADS state
        /// </summary>
        public static Vector3 GetTargetRotation(bool isAiming) => GetTargetRotation(CurrentStance, isAiming);

        // Item 014: overload parametrizado — usado pelo sync remoto para o stance sincronizado de cada player.
        public static Vector3 GetTargetRotation(Stance stance, bool isAiming)
        {
            // Ensure cached values are up to date
            RebuildCachedStanceValues();

            // If ADS and reset rotation is enabled, return ADS rotation
            if (isAiming && (Plugin._ResetOnADS?.Value ?? false))
            {
                return _cachedADSRotation;
            }

            // Return cached rotation based on the given stance
            return stance switch
            {
                Stance.Stance1 => _cachedStance1Rotation,
                Stance.Stance2 => _cachedStance2Rotation,
                Stance.Stance3 => _cachedStance3Rotation,
                _ => Vector3.zero
            };
        }

        /// <summary>
        /// Get the current target position based on stance state and ADS state
        /// </summary>
        public static Vector3 GetTargetPosition(bool isAiming) => GetTargetPosition(CurrentStance, isAiming);

        // Item 014: overload parametrizado — sync remoto.
        public static Vector3 GetTargetPosition(Stance stance, bool isAiming)
        {
            // Ensure cached values are up to date
            RebuildCachedStanceValues();

            // If ADS and reset is enabled, return ADS position
            if (isAiming && (Plugin._ResetOnADS?.Value ?? false))
            {
                return _cachedADSPosition;
            }

            // Return cached position based on the given stance
            return stance switch
            {
                Stance.Stance1 => _cachedStance1Position,
                Stance.Stance2 => _cachedStance2Position,
                Stance.Stance3 => _cachedStance3Position,
                _ => _cachedDefaultPosition
            };
        }

        /// <summary>
        /// Resets all stance state - called when entering new raid or GameWorld changes
        /// </summary>
        public static void ResetState()
        {
            CurrentStance = Stance.Default;

            _isTacSprintActive = false;
            _wasAiming = false;
            _isWaitingToResetTacSprint = false;
            _tacSprintResetTimer = 0f;
            _lastGameWorld = null;
            _lastScrollTime = 0f;
            _cachedGameWorld = null;
            _cachedGameWorldFrame = -1;
            _stanceValuesDirty = true;
            _sprintEnabledDirty = true;
            _lastFirearmController = null;
            _cachedIsBullpup = false;
            _cachedWeaponCellSizeX = 1;

            // backlog 002 F4 — snap state (06-fix-01: campos passaram para Player.FirearmController)
            _triggerDownTimeUnscaled = SnapIdleSentinel;
            _snapInterceptActive = false;
            _interceptFc = null;                  // ref: CR-01-02 + 06-fix-01
            _pendingResurrectFc = null;
            _pendingResurrectMethod = null;
            _pendingResetFc = null;
            _wasNaturallyPressedBeforeResurrect = false;  // 06-fix-04
            _pendingResetMethod = null;

            // backlog 002 F5 — initial stance pending
            _pendingInitialStance = null;
        }
        
        /// <summary>
        /// Rebuild cached 'any sprint enabled' flag from config values
        /// </summary>
        private static void RebuildCachedSprintEnabled()
        {
            if (!_sprintEnabledDirty)
                return;
            
            _cachedAnySprintEnabled = (Plugin._Stance1SprintAnimationEnabled?.Value ?? false) ||
                                      (Plugin._Stance2SprintAnimationEnabled?.Value ?? false) ||
                                      (Plugin._Stance3SprintAnimationEnabled?.Value ?? false);
            _sprintEnabledDirty = false;
        }
        
        /// <summary>
        /// Updates the tac sprint animation state based on stance and sprint status
        /// Uses state tracking to avoid setting animator parameters every frame
        /// </summary>
        public static void UpdateTacSprint()
        {
            // Early exit: if no stances have sprint animation enabled, skip all processing
            // Only check this if tac sprint is not currently active (need to disable it if active)
            if (!_isTacSprintActive && !_isWaitingToResetTacSprint)
            {
                RebuildCachedSprintEnabled();
                if (!_cachedAnySprintEnabled)
                    return;
            }
            
            var gameWorld = GetCachedGameWorld();
            if (gameWorld?.MainPlayer == null)
            {
                // Reset state when no player
                ResetState();
                return;
            }
            
            // Detect GameWorld change (new raid) and reset state
            if (_lastGameWorld != gameWorld)
            {
                // Reset but don't change stance preference - user may want to keep their stance
                _isTacSprintActive = false;
                _isWaitingToResetTacSprint = false;
                _tacSprintResetTimer = 0f;
                _wasAiming = false;
                _lastGameWorld = gameWorld;
            }

            var player = gameWorld.MainPlayer;
            
            // Handle delayed reset timer
            if (_isWaitingToResetTacSprint)
            {
                _tacSprintResetTimer -= Time.deltaTime;
                
                // If player starts sprinting again during delay, cancel the reset
                if (player.IsSprintEnabled && IsHoldingFirearm() && CanDoTacSprint(player))
                {
                    _isWaitingToResetTacSprint = false;
                    _tacSprintResetTimer = 0f;
                    _isTacSprintActive = true; // Re-enable without calling EnableTacSprint (already set)
                    return;
                }
                
                // Timer expired - complete the reset
                if (_tacSprintResetTimer <= 0f)
                {
                    DisableTacSprintImmediate(player);
                }
                return;
            }
            
            // CRITICAL: If player switched away from firearm (e.g., consumables, melee), 
            // immediately disable tac sprint to prevent stuck animation glitch
            if (_isTacSprintActive && !IsHoldingFirearm())
            {
                DisableTacSprintImmediate(player);
                return;
            }
            
            // Check if player is aiming - CRITICAL for preventing stuck sprint
            bool isAiming = player.ProceduralWeaponAnimation?.IsAiming ?? false;
            
            // If we just entered ADS, immediately disable tac sprint (no delay)
            if (isAiming && !_wasAiming && _isTacSprintActive)
            {
                DisableTacSprintImmediate(player);
                _wasAiming = isAiming;
                return;
            }
            
            _wasAiming = isAiming;
            
            // Don't allow tac sprint while aiming
            if (isAiming)
            {
                if (_isTacSprintActive)
                {
                    DisableTacSprintImmediate(player);
                }
                return;
            }

            // Check if sprint animation is enabled for current stance
            bool sprintEnabled = CurrentStance switch
            {
                Stance.Stance1 => Plugin._Stance1SprintAnimationEnabled?.Value ?? false,
                Stance.Stance2 => Plugin._Stance2SprintAnimationEnabled?.Value ?? false,
                Stance.Stance3 => Plugin._Stance3SprintAnimationEnabled?.Value ?? false,
                _ => false // Default stance - no special sprint animation
            };

            // If feature disabled for this stance, make sure tac sprint is off
            if (!sprintEnabled)
            {
                if (_isTacSprintActive)
                {
                    DisableTacSprintImmediate(player);
                }
                return;
            }

            // Check if we should enable tac sprint
            bool shouldEnableTacSprint = CanDoTacSprint(player);

            // State change: enable tac sprint
            if (shouldEnableTacSprint && !_isTacSprintActive)
            {
                EnableTacSprint(player);
            }
            // State change: disable tac sprint (with delay)
            else if (!shouldEnableTacSprint && _isTacSprintActive)
            {
                StartTacSprintReset(player);
            }
        }

        /// <summary>
        /// Enable tac sprint animation - only called once when entering state
        /// </summary>
        private static void EnableTacSprint(Player player)
        {
            if (player.HandsController is Player.FirearmController)
            {
                player.BodyAnimatorCommon.SetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH, 2f);
                _isTacSprintActive = true;
            }
        }

        /// <summary>
        /// Start delayed tac sprint reset - weapon stays in compact position for configured time
        /// </summary>
        private static void StartTacSprintReset(Player player)
        {
            float delay = Plugin._TacSprintResetDelay?.Value ?? 0.35f;
            
            if (delay <= 0f)
            {
                // No delay - instant reset
                DisableTacSprintImmediate(player);
                return;
            }
            
            // Start the delay timer
            _isTacSprintActive = false;
            _isWaitingToResetTacSprint = true;
            _tacSprintResetTimer = delay;
        }
        
        /// <summary>
        /// Immediately disable tac sprint animation - reset to actual weapon size
        /// Used for ADS, weapon swap, and other instant-reset scenarios
        /// </summary>
        private static void DisableTacSprintImmediate(Player player)
        {
            // Always mark as inactive first
            _isTacSprintActive = false;
            _isWaitingToResetTacSprint = false;
            _tacSprintResetTimer = 0f;
            
            // If holding a firearm, reset to actual weapon size (use cached cell size)
            if (player.HandsController is Player.FirearmController)
            {
                player.BodyAnimatorCommon.SetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH, (float)_cachedWeaponCellSizeX);
            }
            else
            {
                // Not holding a firearm - reset to neutral (1 = default for non-weapons)
                // This prevents the stuck animation glitch when switching to consumables
                player.BodyAnimatorCommon.SetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH, 1f);
            }
        }

        /// <summary>
        /// Check if player meets all conditions for tac sprint animation
        /// </summary>
        /// <summary>
        /// Rebuild cached weapon properties when the equipped firearm changes.
        /// Caches bullpup status and cell size to avoid per-frame string/calc overhead.
        /// </summary>
        private static void RebuildCachedWeaponProperties(Player.FirearmController fc)
        {
            if (fc == _lastFirearmController)
                return;
            
            _lastFirearmController = fc;
            
            var weapon = fc.Item;
            _cachedWeaponCellSizeX = weapon.CalculateCellSize().X;
            
            // Zero-allocation bullpup check using OrdinalIgnoreCase
            string templateName = weapon.Template.Name;
            string shortNameKey = weapon.Template.ShortNameLocalizationKey.ToString();
            
            _cachedIsBullpup = templateName.IndexOf("bullpup", StringComparison.OrdinalIgnoreCase) >= 0 ||
                               shortNameKey.IndexOf("aug", StringComparison.OrdinalIgnoreCase) >= 0 ||
                               shortNameKey.IndexOf("p90", StringComparison.OrdinalIgnoreCase) >= 0 ||
                               shortNameKey.IndexOf("mdr", StringComparison.OrdinalIgnoreCase) >= 0 ||
                               shortNameKey.IndexOf("rfb", StringComparison.OrdinalIgnoreCase) >= 0;
        }
        
        private static bool CanDoTacSprint(Player player)
        {
            // Must be in stance and sprinting
            if (!IsInStance || !player.IsSprintEnabled)
                return false;

            if (!(player.HandsController is Player.FirearmController fc))
                return false;

            // Rebuild cached weapon properties if weapon changed
            RebuildCachedWeaponProperties(fc);
            
            var weapon = fc.Item;
            
            // Use cached values instead of per-frame recalculation
            float weightLimit = _cachedIsBullpup ? Plugin._TacSprintWeightLimitBullpup.Value : Plugin._TacSprintWeightLimit.Value;
            
            return weapon.TotalWeight <= weightLimit &&
                   _cachedWeaponCellSizeX <= Plugin._TacSprintLengthLimit.Value &&
                   weapon.ErgonomicsTotal > Plugin._TacSprintErgoLimit.Value;
        }

        // ==========================================================================
        // Stamina + Velocidade por Stance (backlog 001)
        // ==========================================================================

        // Default `true` = "nenhuma raid começou ainda" — defende contra OnRaidEnd
        // disparando antes de qualquer OnRaidStart (ex.: BepInEx reload no hideout).
        private static bool _raidEnded = true;
        private static Stance _activeStaminaStance = Stance.Default;
        private static bool _staminaConfigDirty = true;
        private static float _lastAppliedSpeedLimit = -1f;   // -1 = nada aplicado; força re-apply
        private static float _cachedAimDrainRate = 3f;        // cacheado em OnRaidStart — fallback ao default vanilla
        public static float CachedAimDrainRate => _cachedAimDrainRate;   // item 012: base rate do StaminaController

        public static Stance GetActiveStaminaStance() => _activeStaminaStance;

        /// <summary>
        /// Marca config de stamina/velocidade como suja. Coerente com MarkStanceValuesDirty/MarkSprintEnabledDirty.
        /// </summary>
        public static void MarkStaminaConfigDirty() => _staminaConfigDirty = true;

        // === REFLECTION CACHEADA PARA BACKING FIELDS DE EVENTOS DE GClass774 ===
        //
        // Eventos públicos: OnValueChanged (action_3), OnChanged (action_2), OnThresholdPass (action_1).
        // Os backing fields são private e foram renomeados pelo decompilador.
        // Resolvemos por lista de candidatos: nome público primeiro, nomes do decompilador como fallback.
        private static readonly FieldInfo _onValueChangedBacking =
            ResolveBackingFieldByCandidates(typeof(GClass774), nameof(GClass774.OnValueChanged), "action_3");
        private static readonly FieldInfo _onThresholdPassBacking =
            ResolveBackingFieldByCandidates(typeof(GClass774), nameof(GClass774.OnThresholdPass), "action_1");

        /// <summary>
        /// Resolve um backing field privado de event Action tentando uma lista ordenada de candidatos.
        /// Estratégia: passar primeiro o nome do <b>evento público</b> (estável se BSG mantiver a API
        /// pública de GClass774), depois nomes do <b>decompilador ILSpy</b> (action_N, podem mudar
        /// entre patches do EFT porque os backing fields são privados/renomeados).
        /// Retorna o primeiro Action field encontrado, ou null se nenhum candidato bater.
        /// Falhas são detectadas no Awake via HasMissingReflection e logadas como warning.
        /// </summary>
        private static FieldInfo ResolveBackingFieldByCandidates(Type t, params string[] candidates)
        {
            foreach (var name in candidates)
            {
                var f = AccessTools.Field(t, name);
                if (f != null && f.FieldType == typeof(Action)) return f;
            }
            return null;
        }

        /// <summary>True se algum field-info essencial para HUD updates não foi resolvido.</summary>
        public static bool HasMissingReflection(out List<string> missing)
        {
            missing = new List<string>();
            if (_onValueChangedBacking == null) missing.Add("GClass774.OnValueChanged backing field");
            if (_onThresholdPassBacking == null) missing.Add("GClass774.OnThresholdPass backing field");
            return missing.Count > 0;
        }

        private static void NotifyHandsStaminaChanged(GClass774 hands, float prevValue)
        {
            try
            {
                // OnValueChanged — sinal "stamina mudou" que a HUD escuta
                ((Action)_onValueChangedBacking?.GetValue(hands))?.Invoke();
                // OnChanged — propaga para listeners gerais (método público, sem reflection)
                hands.InvokeChangedAction();
                // OnThresholdPass — só dispara quando cruza o threshold de 15f (som "tired")
                if ((hands.Current < 15f) ^ (prevValue < 15f))
                    ((Action)_onThresholdPassBacking?.GetValue(hands))?.Invoke();
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[NotifyHandsStaminaChanged] {ex}"); }
        }

        /// <summary>Hot-path guard: deve haver raid ativa, MainPlayer real (não Hideout).</summary>
        public static bool IsActiveContext()
        {
            var gw = Singleton<GameWorld>.Instance;
            if (gw == null || gw.MainPlayer == null) return false;
            if (gw.MainPlayer is HideoutPlayer)
                return Plugin._DebugApplyInHideout?.Value == true;
            return true;
        }

        public static void OnRaidStart()
        {
            try
            {
                _raidEnded = false;
                _activeStaminaStance = Stance.Default;
                _staminaConfigDirty = true;
                _lastAppliedSpeedLimit = -1f;

                // Cachear AimDrainRate (constante imutável em runtime) — evita Singleton lookup todo frame
                var backend = Singleton<BackendConfigSettingsClass>.Instance;
                if (backend?.Stamina != null)
                    _cachedAimDrainRate = backend.Stamina.AimDrainRate;

                StanceStaminaState.Reset();
                ApplyStaminaStance(_activeStaminaStance);
                Plugin.Logger.LogInfo("[StanceManager] Raid start — state initialized");
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[StanceManager.OnRaidStart] {ex}"); }
        }

        public static void OnRaidEnd()
        {
            if (_raidEnded) return;                  // idempotente
            _raidEnded = true;
            try
            {
                var mc = Singleton<GameWorld>.Instance?.MainPlayer?.MovementContext;
                mc?.RemoveStateSpeedLimit(Plugin.StanceSpeedLimitCause);

                StanceStaminaState.Reset();
                _activeStaminaStance = Stance.Default;
                _lastAppliedSpeedLimit = -1f;

                // Reuso: ResetState() já existe e limpa CurrentStance, tac sprint, caches.
                ResetState();

                Plugin.Logger.LogInfo("[StanceManager] Raid end — state cleaned");
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[StanceManager.OnRaidEnd] {ex}"); }
        }

        public static void ApplyStaminaStance(Stance stance)
        {
            if (!IsActiveContext()) return;
            if (!Plugin._stanceConfigs.TryGetValue(stance, out var cfg)) return;

            var mc = Singleton<GameWorld>.Instance.MainPlayer.MovementContext;

            mc.RemoveStateSpeedLimit(Plugin.StanceSpeedLimitCause);
            _lastAppliedSpeedLimit = -1f;

            // item 012: o multiplicador de stamina migrou para o StaminaController; aqui sobra só o speed-limit.
            bool inProne = Singleton<GameWorld>.Instance.MainPlayer.IsInPronePose;
            StanceStaminaState.IsSuspendedByProne = inProne && !cfg.ApplyWhenProne.Value;

            if (cfg.ModifiesMovementSpeed.Value && !StanceStaminaState.IsSuspendedByProne)
            {
                float fraction = cfg.MovementSpeedMultiplier.Value / 100f;
                float target = fraction * mc.MaxSpeed;
                mc.AddStateSpeedLimit(target, Plugin.StanceSpeedLimitCause);
                _lastAppliedSpeedLimit = target;
            }

            _staminaConfigDirty = false;
        }

        // CR-02-02: último estado de ADS (mira) enviado pela rede. Permite reenviar o stance quando o jogador
        // mira/desmira SEM trocar de stance — caso em que OnStanceChanged não dispara e o _isAiming do observado
        // ficaria preso no valor de quando a stance mudou.
        private static bool _lastSentAiming;

        private static void OnStanceChanged(Stance previousStance, Stance newStance)
        {
            try
            {
                var gw = GetCachedGameWorld();
                if (gw?.MainPlayer != null)
                {
                    // Usa singleton estático em vez de FindObjectOfType (evita hitch de FPS).
                    bool isAiming = false;
                    if (gw.MainPlayer.HandsController is Player.FirearmController fc)
                        isAiming = fc.IsAiming;

                    CameraRotationMod.Networking.FikaSyncManager.SendStance((int)newStance, isAiming);
                    _lastSentAiming = isAiming;   // CR-02-02: baseline para o tick de ADS não reenviar redundante.
                }

                _activeStaminaStance = newStance;
                ApplyStaminaStance(newStance);
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[StanceManager.OnStanceChanged] {ex}"); }
        }

        /// <summary>
        /// CR-02-02: detecta a transição de ADS (mira) mantendo a mesma stance ativa e reenvia o stance para os
        /// peers Fika. Sem isto, o offset de ADS da pose remota fica preso no valor de quando a stance mudou.
        /// Throttle natural: só envia na BORDA da mudança (compara com o último valor enviado). Chamado 1x/frame
        /// no Plugin.Update.
        /// </summary>
        public static void TickAdsNetworkSync()
        {
            try
            {
                // Só importa com stance ativa — em Default o observado ignora o offset (inStance = false).
                if (CurrentStance == Stance.Default) return;

                var gw = GetCachedGameWorld();
                if (gw?.MainPlayer == null) return;

                bool isAiming = gw.MainPlayer.HandsController is Player.FirearmController fc && fc.IsAiming;
                if (isAiming == _lastSentAiming) return;

                _lastSentAiming = isAiming;
                CameraRotationMod.Networking.FikaSyncManager.SendStance((int)CurrentStance, isAiming);
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[StanceManager.TickAdsNetworkSync] {ex}"); }
        }

        /// <summary>
        /// Controla delta de stamina das mãos por frame (drain e recovery).
        /// Multiplier &lt;1.0 = drain, 1.0 = vanilla (no-op), &gt;1.0 = recovery.
        /// Em ADS o vanilla toma conta — tick faz no-op.
        /// ref: fix-01 — fórmula unificada substituindo Drain-only anterior.
        /// </summary>
        public static void TickStanceStamina()
        {
            // item 012: o delta de stamina de braço migrou para StaminaController.Tick (autoridade única).
            // Aqui sobra apenas a re-aplicação do speed-limit quando a config de stance muda (dirty).
            try
            {
                if (_staminaConfigDirty) ApplyStaminaStance(_activeStaminaStance);
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[StanceManager.TickStanceStamina] {ex}"); }
        }

        /// <summary>Detecta entrada/saída de prone e re-aplica speed limit defensivamente (cobre MaxSpeed dinâmico).</summary>
        public static void EvaluateProneSuspensionTick()
        {
            try
            {
                if (!IsActiveContext()) return;
                if (!Plugin._stanceConfigs.TryGetValue(_activeStaminaStance, out var cfg)) return;

                var player = Singleton<GameWorld>.Instance.MainPlayer;
                bool wasSuspended = StanceStaminaState.IsSuspendedByProne;
                bool isSuspended = player.IsInPronePose && !cfg.ApplyWhenProne.Value;

                if (wasSuspended != isSuspended)
                {
                    StanceStaminaState.IsSuspendedByProne = isSuspended;
                    if (isSuspended)
                    {
                        player.MovementContext.RemoveStateSpeedLimit(Plugin.StanceSpeedLimitCause);
                        _lastAppliedSpeedLimit = -1f;
                        return;
                    }
                }

                // Re-aplicação defensiva — só executa se valor calculado mudou.
                // Cobre staleness de MaxSpeed (varia com skill Strength) sem disparar
                // OnCharacterControllerSpeedLimitChanged à toa.
                if (cfg.ModifiesMovementSpeed.Value && !StanceStaminaState.IsSuspendedByProne)
                {
                    var mc = player.MovementContext;
                    float target = (cfg.MovementSpeedMultiplier.Value / 100f) * mc.MaxSpeed;

                    // Tolerância evita re-apply por flutuação numérica
                    if (Mathf.Abs(target - _lastAppliedSpeedLimit) > 0.001f)
                    {
                        mc.RemoveStateSpeedLimit(Plugin.StanceSpeedLimitCause);
                        mc.AddStateSpeedLimit(target, Plugin.StanceSpeedLimitCause);
                        _lastAppliedSpeedLimit = target;
                    }
                }
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[StanceManager.EvaluateProneSuspensionTick] {ex}"); }
        }
    }
}
