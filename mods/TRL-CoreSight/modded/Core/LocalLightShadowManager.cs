using System;
using System.Collections.Generic;
using EFT;
using TRLCoreSight.Configuration;
using UnityEngine;

namespace TRLCoreSight.Core
{
    /// <summary>
    /// Gerencia a projeção de sombras em tempo real de fontes de luz locais estáticas,
    /// aliviando o passe de cubemaps de sombra na GPU e prevenindo vazamentos de luz (Light Leaking).
    /// </summary>
    public class LocalLightShadowManager : MonoBehaviour
    {
        public static LocalLightShadowManager Instance { get; private set; }

        private readonly Dictionary<Light, LightShadows> _originalShadowStates = new Dictionary<Light, LightShadows>(512);
        private bool _isOptimized;

        public bool IsOptimized => _isOptimized;
        public int TotalManagedLights => _originalShadowStates.Count;
        public int OptimizedLightsCount { get; private set; }

        private void Awake()
        {
            Instance = this;
            ModConfig.OnLightSettingsChanged += OnSettingsChanged;
        }

        public void Initialize()
        {
            ScanAndCacheLights();
            ApplyOptimization();
        }

        /// <summary>
        /// Varre todas as fontes de luz da cena atual e armazena os estados originais de sombra daquelas elegíveis.
        /// </summary>
        public void ScanAndCacheLights()
        {
            _originalShadowStates.Clear();
            Light[] allLights = FindObjectsOfType<Light>();
            if (allLights == null || allLights.Length == 0)
            {
                return;
            }

            for (int i = 0; i < allLights.Length; i++)
            {
                Light light = allLights[i];
                if (light == null)
                {
                    continue;
                }

                // 1. Ignorar sol e iluminação global direcional (gerenciados pelo InteriorOcclusionWatcher / TOD_Sky)
                if (light.type == LightType.Directional)
                {
                    continue;
                }

                // 2. Ignorar se a luz já não projeta sombras originalmente
                if (light.shadows == LightShadows.None)
                {
                    continue;
                }

                // 3. Ignorar lanternas táticas e lasers de armas ou capacetes
                if (light.GetComponentInParent<TacticalComboVisualController>() != null ||
                    light.GetComponentInParent<Player>() != null)
                {
                    continue;
                }

                // 4. Ignorar efeitos efêmeros de tiro e sinalizadores
                string objName = light.gameObject.name.ToLower();
                if (objName.Contains("muzzle") || objName.Contains("flare") || objName.Contains("flash"))
                {
                    continue;
                }

                _originalShadowStates[light] = light.shadows;
            }

            Plugin.LogSource?.LogInfo($"[TRL-CoreSight] LocalLightShadowManager: {_originalShadowStates.Count} fontes de luz secundárias catalogadas.");
        }

        /// <summary>
        /// Aplica o modo de otimização selecionado no menu F12 sobre as luzes catalogadas.
        /// </summary>
        public void ApplyOptimization()
        {
            if (!ModConfig.ModEnabled.Value || !ModConfig.EnableLocalLightShadowCulling.Value)
            {
                RestoreOriginalShadows();
                return;
            }

            var mode = ModConfig.ShadowOptimizationMode.Value;
            float maxIntensity = ModConfig.WeakLightIntensityThreshold.Value;
            float maxRange = ModConfig.WeakLightRangeThreshold.Value;

            int count = 0;
            foreach (var kvp in _originalShadowStates)
            {
                Light light = kvp.Key;
                LightShadows originalShadow = kvp.Value;

                if (light == null)
                {
                    continue;
                }

                switch (mode)
                {
                    case LightShadowOptimizationMode.DisableWeakShadowsOnly:
                        // Oculta sombra apenas se intensidade e raio forem pequenos (blindagem contra light leaking)
                        if (light.intensity <= maxIntensity && light.range <= maxRange)
                        {
                            light.shadows = LightShadows.None;
                            count++;
                        }
                        else
                        {
                            light.shadows = originalShadow;
                        }
                        break;

                    case LightShadowOptimizationMode.DowngradeSoftToHard:
                        // Reduz sombras suaves (PCF taps caros) para duras (1 tap) sem vazar luz através de paredes
                        if (originalShadow == LightShadows.Soft)
                        {
                            light.shadows = LightShadows.Hard;
                            count++;
                        }
                        else
                        {
                            light.shadows = originalShadow;
                        }
                        break;

                    case LightShadowOptimizationMode.DisableAllSecondaryShadows:
                        // Desativação total de sombras em todas as luzes pontuais/spots secundárias
                        light.shadows = LightShadows.None;
                        count++;
                        break;
                }
            }

            OptimizedLightsCount = count;
            _isOptimized = true;
        }

        private void OnSettingsChanged()
        {
            ApplyOptimization();
        }

        /// <summary>
        /// Restaura deterministamente o estado original de todas as luzes alteradas.
        /// </summary>
        public void RestoreOriginalShadows()
        {
            foreach (var kvp in _originalShadowStates)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.shadows = kvp.Value;
                }
            }

            _isOptimized = false;
            OptimizedLightsCount = 0;
        }

        public void Cleanup()
        {
            ModConfig.OnLightSettingsChanged -= OnSettingsChanged;
            RestoreOriginalShadows();
            _originalShadowStates.Clear();
            Instance = null;
        }

        private void OnDestroy()
        {
            Cleanup();
        }
    }
}
