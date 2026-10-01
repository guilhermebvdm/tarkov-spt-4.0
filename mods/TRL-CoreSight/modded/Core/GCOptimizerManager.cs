using System;
using EFT;
using TRLCoreSight.Configuration;
using UnityEngine;
using UnityEngine.Scripting;

namespace TRLCoreSight.Core
{
    /// <summary>
    /// Gerenciador de Anti-Stutter de Garbage Collection (Filtro de Sobrevivência).
    /// Inibe o Garbage Collector da Unity exclusivamente durante mira acionada (ADS)
    /// e em combate ativo (janela recente de disparos), permitindo que a engine do jogo
    /// realize suas coletas de memória nativamente e sem sobressaltos fora desses momentos.
    /// Possui salvaguarda de memória crítica para prevenir Out-Of-Memory (OOM).
    /// </summary>
    public class GCOptimizerManager : MonoBehaviour
    {
        public static GCOptimizerManager Instance { get; private set; }

        private Player _mainPlayer;
        private float _lastShotTime = -100f;

        public bool IsSuppressed { get; private set; }
        public int SuppressedEventsCount { get; private set; }
        public float AllocatedMemoryMB => (float)GC.GetTotalMemory(false) / (1024f * 1024f);

        private void Awake()
        {
            Instance = this;
            ModConfig.OnGCSettingsChanged += OnSettingsChanged;
        }

        public void Initialize(Player mainPlayer)
        {
            _mainPlayer = mainPlayer;
            _lastShotTime = -100f;
            Plugin.LogSource?.LogInfo("[TRL-CoreSight] GCOptimizerManager: Filtro de sobrevivência ativado (inibição em ADS/Combate).");
        }

        public void NotifyShotFired()
        {
            _lastShotTime = Time.time;
        }

        public void OnUpdate()
        {
            if (!ModConfig.ModEnabled.Value || !ModConfig.EnableGCOptimizer.Value || _mainPlayer == null)
            {
                RestoreGCMode();
                return;
            }

            // Salvaguarda: se o jogador foi eliminado, restaura o modo nativo imediatamente
            if (_mainPlayer.HealthController != null && !_mainPlayer.HealthController.IsAlive)
            {
                RestoreGCMode();
                return;
            }

            // 1. Fallback de Emergência: se o heap atingir o patamar crítico de RAM, força a liberação do GC nativo
            if (AllocatedMemoryMB >= ModConfig.CriticalMemoryLimitMB.Value)
            {
                RestoreGCMode();
                return;
            }

            float now = Time.time;

            // 2. Avaliação de condições de sobrevivência (Mira ADS e Disparos Recentes)
            bool isAiming = _mainPlayer.ProceduralWeaponAnimation != null && _mainPlayer.ProceduralWeaponAnimation.IsAiming;
            bool inCombat = (now - _lastShotTime) < ModConfig.CombatGracePeriod.Value;

            bool shouldSuppress = isAiming || inCombat;

            if (shouldSuppress)
            {
                if (GarbageCollector.GCMode != GarbageCollector.Mode.Disabled)
                {
                    GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
                    IsSuppressed = true;
                    SuppressedEventsCount++;
                }
            }
            else
            {
                // Fora de combate e fora de ADS: deixa o Tarkov e a Unity gerenciarem a RAM nativamente
                if (GarbageCollector.GCMode != GarbageCollector.Mode.Enabled)
                {
                    GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
                    IsSuppressed = false;
                }
            }
        }

        private void RestoreGCMode()
        {
            if (GarbageCollector.GCMode != GarbageCollector.Mode.Enabled)
            {
                GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
            }
            IsSuppressed = false;
        }

        private void OnSettingsChanged()
        {
            if (!ModConfig.ModEnabled.Value || !ModConfig.EnableGCOptimizer.Value)
            {
                RestoreGCMode();
            }
        }

        public void Cleanup()
        {
            ModConfig.OnGCSettingsChanged -= OnSettingsChanged;
            RestoreGCMode();
            Instance = null;
        }

        private void OnDestroy()
        {
            Cleanup();
        }
    }
}
