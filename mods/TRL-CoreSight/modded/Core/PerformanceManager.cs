using System;
using Comfort.Common;
using EFT;
using TRLCoreSight.Configuration;
using UnityEngine;

namespace TRLCoreSight.Core
{
    public class PerformanceManager : MonoBehaviour
    {
        public static PerformanceManager Instance { get; private set; }

        private Player _mainPlayer;
        private InteriorOcclusionWatcher _shadowWatcher;
        private BotPerformanceLimiter _botLimiter;
        private LootCullingManager _lootManager;
        private LocalLightShadowManager _lightManager;
        private GCOptimizerManager _gcOptimizer;
        private NaturalConcealmentManager _concealmentManager;
        private AmbientAudioCullingManager _audioManager;
        private DeclutterManager _declutterManager;
        private AimVegetationInspector _aimInspector;
        private FrametimeBenchmark _frametimeBenchmark;
        private float _originalLodBias;
        private float _currentLodBias;
        private float _originalFlyingShellsDistance;
        private bool _cleanedUp = false;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            _originalLodBias = QualitySettings.lodBias;
            _currentLodBias = _originalLodBias;
        }

        public void Initialize(Player mainPlayer)
        {
            _mainPlayer = mainPlayer;

            if (EFTHardSettings.Instance != null)
            {
                _originalFlyingShellsDistance = EFTHardSettings.Instance.FLYING_SHELLS_VISIBLE_DISTANCE;
                if (ModConfig.ModEnabled.Value && ModConfig.EnableShellCulling.Value)
                {
                    EFTHardSettings.Instance.FLYING_SHELLS_VISIBLE_DISTANCE = ModConfig.ShellCullingDistance.Value;
                }
            }

            try
            {
                _shadowWatcher = gameObject.AddComponent<InteriorOcclusionWatcher>();
                _shadowWatcher.Initialize(mainPlayer);
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight] Erro ao inicializar InteriorOcclusionWatcher: {ex.Message}");
            }

            try
            {
                _botLimiter = gameObject.AddComponent<BotPerformanceLimiter>();
                _botLimiter.Initialize(mainPlayer);
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight] Erro ao inicializar BotPerformanceLimiter: {ex.Message}");
            }

            try
            {
                _lootManager = gameObject.AddComponent<LootCullingManager>();
                _lootManager.Initialize(mainPlayer);
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight] Erro ao inicializar LootCullingManager: {ex.Message}");
            }

            try
            {
                _lightManager = gameObject.AddComponent<LocalLightShadowManager>();
                _lightManager.Initialize();
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight] Erro ao inicializar LocalLightShadowManager: {ex.Message}");
            }

            try
            {
                _gcOptimizer = gameObject.AddComponent<GCOptimizerManager>();
                _gcOptimizer.Initialize(mainPlayer);
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight] Erro ao inicializar GCOptimizerManager: {ex.Message}");
            }

            try
            {
                _concealmentManager = gameObject.AddComponent<NaturalConcealmentManager>();
                _concealmentManager.Initialize(mainPlayer);
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight] Erro ao inicializar NaturalConcealmentManager: {ex.Message}");
            }

            try
            {
                _audioManager = gameObject.AddComponent<AmbientAudioCullingManager>();
                _audioManager.Initialize();
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight] Erro ao inicializar AmbientAudioCullingManager: {ex.Message}");
            }

            try
            {
                _declutterManager = gameObject.AddComponent<DeclutterManager>();
                _declutterManager.OnRaidStarted();
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight] Erro ao inicializar DeclutterManager: {ex.Message}");
            }

            try
            {
                _aimInspector = gameObject.AddComponent<AimVegetationInspector>();
                _aimInspector.Initialize(mainPlayer);
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight] Erro ao inicializar AimVegetationInspector: {ex.Message}");
            }

            try
            {
                _frametimeBenchmark = gameObject.AddComponent<FrametimeBenchmark>();
                _frametimeBenchmark.Initialize();
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight] Erro ao inicializar FrametimeBenchmark: {ex.Message}");
            }

            // Aplicação da afinidade de hardware no início da partida
            CpuTopologyManager.ApplyAffinity();
            ModConfig.OnCpuSettingsChanged += OnCpuSettingsChanged;

            ModConfig.OnDumpRequested += OnDumpRequested;
        }

        private void Update()
        {
            if (!ModConfig.ModEnabled.Value)
            {
                return;
            }

            // Fixação/liberação da Main Thread para o PrimaryCluster (executado exclusivamente no
            // contexto da thread principal, dentro deste Update()).
            if (CpuTopologyManager.NeedsMainThreadPin)
            {
                CpuTopologyManager.PinCallingThreadToCluster();
            }
            else if (CpuTopologyManager.NeedsMainThreadUnpin)
            {
                CpuTopologyManager.UnpinCallingThread();
            }

            if (ModConfig.VisualizerShortcutKey != null && ModConfig.VisualizerShortcutKey.Value.IsDown())
            {
                ModConfig.ShowConcealmentVisualizer.Value = !ModConfig.ShowConcealmentVisualizer.Value;
                NotificationManagerClass.DisplayMessageNotification(
                    $"[TRL-CoreSight] Visualizador de Camuflagem (Vermelho 50%): {(ModConfig.ShowConcealmentVisualizer.Value ? "ATIVADO" : "DESATIVADO")}",
                    EFT.Communications.ENotificationDurationType.Default
                );
            }

            if (ModConfig.DumpShortcutKey != null && ModConfig.DumpShortcutKey.Value.IsDown())
            {
                VegetationAssetDumper.DumpActiveSceneVegetation();
            }

            if (ModConfig.EnableVegetationInspector != null && ModConfig.EnableVegetationInspector.Value)
            {
                if (ModConfig.InspectAimVegetationKey != null && ModConfig.InspectAimVegetationKey.Value.IsDown())
                {
                    Plugin.LogSource?.LogInfo("[TRL-CoreSight] Tecla '[' pressionada. Registrando na mira como CAMUFLAGEM...");
                    _aimInspector?.InspectAtAim(registerAsCamouflage: true, registerAsNonCamouflage: false);
                }

                if (ModConfig.RegisterNonConcealmentKey != null && ModConfig.RegisterNonConcealmentKey.Value.IsDown())
                {
                    Plugin.LogSource?.LogInfo("[TRL-CoreSight] Tecla ']' pressionada. Registrando na mira como NÃO-CAMUFLAGEM...");
                    _aimInspector?.InspectAtAim(registerAsCamouflage: false, registerAsNonCamouflage: true);
                }
            }

            bool isAiming = _mainPlayer != null && _mainPlayer.ProceduralWeaponAnimation != null && _mainPlayer.ProceduralWeaponAnimation.IsAiming;

            _shadowWatcher?.OnUpdate();
            _botLimiter?.OnUpdate();
            _lootManager?.OnUpdate();
            _gcOptimizer?.OnUpdate();
            _concealmentManager?.OnUpdate();
            _audioManager?.OnUpdate();
            _frametimeBenchmark?.OnUpdate();

            HandleDynamicLOD(isAiming);
        }

        private void HandleDynamicLOD(bool isAiming)
        {
            if (!ModConfig.EnableDynamicLODBias.Value || _mainPlayer == null || _mainPlayer.HandsController == null)
            {
                return;
            }

            float targetLod = isAiming ? ModConfig.AimLODBias.Value : ModConfig.BaseLODBias.Value;

            if (Math.Abs(_currentLodBias - targetLod) > 0.02f)
            {
                _currentLodBias = Mathf.MoveTowards(_currentLodBias, targetLod, 4.0f * Time.deltaTime);
                QualitySettings.lodBias = _currentLodBias;
            }
        }

        private void OnGUI()
        {
            if (!ModConfig.DebugMode.Value || _mainPlayer == null)
            {
                return;
            }

            GUI.color = Color.green;
            string status = $"[CoreSight Debug]\n" +
                            $"Interior: {(_shadowWatcher != null && _shadowWatcher.IsInside ? "SIM" : "NÃO")}\n" +
                            $"Shadow Dist: {QualitySettings.shadowDistance:F0}m\n" +
                            $"LOD Bias: {QualitySettings.lodBias:F2}\n" +
                            $"Bots Ocluídos: {(_botLimiter != null ? _botLimiter.OccludedBotsCount : 0)}\n" +
                            $"Shell Culling: {(ModConfig.EnableShellCulling.Value ? $"{ModConfig.ShellCullingDistance.Value:F0}m" : "OFF")}\n" +
                            $"Loot Ocluído: {(_lootManager != null ? _lootManager.CulledLootCount : 0)}\n" +
                            $"Luzes Otimizadas: {(_lightManager != null ? $"{_lightManager.OptimizedLightsCount}/{_lightManager.TotalManagedLights}" : "0")}\n" +
                            $"GC Status: {(_gcOptimizer != null && _gcOptimizer.IsSuppressed ? "SUPRIMIDO" : "NORMAL")} ({(_gcOptimizer != null ? _gcOptimizer.AllocatedMemoryMB.ToString("F0") : "0")}MB)\n" +
                            $"Camuflagem: {(_concealmentManager != null && _concealmentManager.IsConcealed ? "ATIVA" : (_concealmentManager != null && _concealmentManager.IsUnderCanopy ? "COPA" : "NÃO"))}\n" +
                            $"Áudio Ocluído: {(_audioManager != null ? $"{_audioManager.PausedSourcesCount}/{_audioManager.TotalManagedSources}" : "0")}";

            GUI.Label(new Rect(20, 200, 260, 220), status);
        }

        public void Cleanup()
        {
            // ref: 06-fix-01 — Cleanup() é chamado duas vezes por fim de raid (uma vez manual
            // via GameWorldOnDestroyPatch, outra vez natural pelo OnDestroy() da própria Unity
            // neste mesmo componente). Sem essa guarda, a segunda chamada reexecuta tudo sobre
            // sub-managers já destruídos, podendo lançar exceção Unity-side não capturada por
            // este método nem pelo patch (silenciosa no LogOutput.log) antes de restaurar a CPU.
            if (_cleanedUp)
            {
                return;
            }
            _cleanedUp = true;

            // Executado primeiro e sem depender de nada abaixo: se qualquer sub-manager mais
            // adiante lançar exceção durante o teardown (Cleanup() não tem try/catch por etapa,
            // diferente do Initialize()), a afinidade de CPU/thread ainda assim é restaurada,
            // em vez de ficar presa até o próximo boot do jogo.
            CpuTopologyManager.RestoreOriginalAffinity();

            QualitySettings.lodBias = _originalLodBias;

            if (EFTHardSettings.Instance != null && _originalFlyingShellsDistance > 0f)
            {
                EFTHardSettings.Instance.FLYING_SHELLS_VISIBLE_DISTANCE = _originalFlyingShellsDistance;
            }

            if (_shadowWatcher != null)
            {
                _shadowWatcher.Restore();
                Destroy(_shadowWatcher);
            }

            if (_botLimiter != null)
            {
                _botLimiter.RestoreAll();
                Destroy(_botLimiter);
            }

            if (_lootManager != null)
            {
                _lootManager.RestoreAll();
                Destroy(_lootManager);
            }

            if (_lightManager != null)
            {
                _lightManager.Cleanup();
                Destroy(_lightManager);
            }

            if (_gcOptimizer != null)
            {
                _gcOptimizer.Cleanup();
                Destroy(_gcOptimizer);
            }

            if (_concealmentManager != null)
            {
                _concealmentManager.Cleanup();
                Destroy(_concealmentManager);
            }

            if (_audioManager != null)
            {
                _audioManager.Cleanup();
                Destroy(_audioManager);
            }

            if (_declutterManager != null)
            {
                _declutterManager.OnRaidFinished();
                Destroy(_declutterManager);
            }

            if (_frametimeBenchmark != null)
            {
                _frametimeBenchmark.Cleanup();
                Destroy(_frametimeBenchmark);
            }

            ModConfig.OnDumpRequested -= OnDumpRequested;
            ModConfig.OnCpuSettingsChanged -= OnCpuSettingsChanged;

            Instance = null;
        }

        private void OnCpuSettingsChanged()
        {
            CpuTopologyManager.ApplyAffinity();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && ModConfig.CpuAffinityMode != null && ModConfig.CpuAffinityMode.Value != ECpuAffinityMode.Disabled)
            {
                CpuTopologyManager.ApplyAffinity();
            }
        }

        private void OnDumpRequested()
        {
            VegetationAssetDumper.DumpActiveSceneVegetation();
        }

        private void OnDestroy()
        {
            Cleanup();
        }
    }
}
