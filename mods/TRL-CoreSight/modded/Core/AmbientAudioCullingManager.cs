using System;
using System.Collections.Generic;
using EFT;
using TRLCoreSight.Configuration;
using UnityEngine;

namespace TRLCoreSight.Core
{
    /// <summary>
    /// Gerencia a supressão dinâmica (pausa/retomada) de emissores contínuos de áudio ambiente
    /// cujas distâncias ultrapassem a atenuação máxima de audibilidade do jogador.
    /// Reduz a carga de processamento DSP e AudioMixer na Unity.
    /// </summary>
    public class AmbientAudioCullingManager : MonoBehaviour
    {
        public static AmbientAudioCullingManager Instance { get; private set; }

        private readonly List<TrackedAudioSource> _ambientSources = new List<TrackedAudioSource>(256);
        private int _batchIndex;
        private const int BATCH_SIZE = 32;

        private float _nextRescanTime;
        private const float RESCAN_INTERVAL = 60.0f;

        public int TotalManagedSources => _ambientSources.Count;
        public int PausedSourcesCount { get; private set; }

        private struct TrackedAudioSource
        {
            public AudioSource Source;
            public Transform Transform;
            public float CutoffDistanceSqr;
            public bool IsPausedByMod;
        }

        private void Awake()
        {
            Instance = this;
            ModConfig.OnAudioSettingsChanged += OnSettingsChanged;
        }

        public void Initialize()
        {
            ScanAndRegisterAmbientSources();
            _nextRescanTime = Time.time + RESCAN_INTERVAL;
        }

        public void ScanAndRegisterAmbientSources()
        {
            try
            {
                AudioSource[] allSources = FindObjectsOfType<AudioSource>();
                if (allSources == null || allSources.Length == 0)
                {
                    return;
                }

                float margin = ModConfig.AudioCullingMargin.Value;

                // Mapeia fontes já conhecidas para preservar o estado de pausa atual
                Dictionary<AudioSource, bool> existingStates = new Dictionary<AudioSource, bool>(_ambientSources.Count);
                for (int i = 0; i < _ambientSources.Count; i++)
                {
                    var item = _ambientSources[i];
                    if (item.Source != null)
                    {
                        existingStates[item.Source] = item.IsPausedByMod;
                    }
                }

                _ambientSources.Clear();

                for (int i = 0; i < allSources.Length; i++)
                {
                    AudioSource src = allSources[i];
                    if (src == null || !src.loop)
                    {
                        continue; // Ignora tiros, passos e efeitos sonoros avulsos
                    }

                    // Ignora sons originados de qualquer Player ou Bot
                    if (src.GetComponentInParent<Player>() != null)
                    {
                        continue;
                    }

                    string objName = src.gameObject.name.ToLower();
                    if (objName.Contains("siren") || objName.Contains("alarm") || objName.Contains("voice") || objName.Contains("step"))
                    {
                        continue; // Sons críticos táticos/alarmes
                    }

                    float cutoffDist = Mathf.Max(src.maxDistance + margin, 15.0f);
                    bool wasPaused = existingStates.TryGetValue(src, out bool p) && p;

                    _ambientSources.Add(new TrackedAudioSource
                    {
                        Source = src,
                        Transform = src.transform,
                        CutoffDistanceSqr = cutoffDist * cutoffDist,
                        IsPausedByMod = wasPaused
                    });
                }

                Plugin.LogSource?.LogInfo($"[TRL-CoreSight] AmbientAudioCullingManager: {_ambientSources.Count} fontes contínuas de ambiente catalogadas.");
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight] AmbientAudioCullingManager.Scan: {ex.Message}");
            }
        }

        public void OnUpdate()
        {
            if (!ModConfig.ModEnabled.Value || !ModConfig.EnableAmbientAudioCulling.Value || _ambientSources.Count == 0)
            {
                if (PausedSourcesCount > 0)
                {
                    ResumeAll();
                }
                return;
            }

            // Re-escaneamento preventivo lento para capturar geradores ou interruptores ativados tardiamente
            if (Time.time >= _nextRescanTime)
            {
                _nextRescanTime = Time.time + RESCAN_INTERVAL;
                ScanAndRegisterAmbientSources();
            }

            Camera activeCam = (CameraClass.Exist && CameraClass.Instance.Camera != null)
                ? CameraClass.Instance.Camera
                : Camera.main;

            if (activeCam == null)
            {
                return;
            }

            Vector3 listenerPos = activeCam.transform.position;
            int total = _ambientSources.Count;
            int processed = 0;

            while (processed < BATCH_SIZE && _batchIndex < total)
            {
                TrackedAudioSource item = _ambientSources[_batchIndex];
                if (item.Source == null || item.Transform == null)
                {
                    _ambientSources.RemoveAt(_batchIndex);
                    total--;
                    continue;
                }

                if (!item.Source.isActiveAndEnabled)
                {
                    _batchIndex++;
                    processed++;
                    continue;
                }

                float sqrDist = (item.Transform.position - listenerPos).sqrMagnitude;
                bool shouldBeCulled = sqrDist > item.CutoffDistanceSqr;

                if (shouldBeCulled && !item.IsPausedByMod)
                {
                    // Apenas pausar se estava de fato tocando
                    if (item.Source.isPlaying)
                    {
                        item.Source.Pause();
                        item.IsPausedByMod = true;
                        PausedSourcesCount++;
                        _ambientSources[_batchIndex] = item;
                    }
                }
                else if (!shouldBeCulled && item.IsPausedByMod)
                {
                    // Retoma com segurança sem disparar se foi desligado pelo próprio jogo
                    item.Source.UnPause();
                    item.IsPausedByMod = false;
                    PausedSourcesCount = Mathf.Max(0, PausedSourcesCount - 1);
                    _ambientSources[_batchIndex] = item;
                }

                _batchIndex++;
                processed++;
            }

            if (_batchIndex >= total)
            {
                _batchIndex = 0;
            }
        }

        public void ResumeAll()
        {
            for (int i = 0; i < _ambientSources.Count; i++)
            {
                var item = _ambientSources[i];
                if (item.Source != null && item.IsPausedByMod)
                {
                    try
                    {
                        item.Source.UnPause();
                    }
                    catch { }
                    item.IsPausedByMod = false;
                    _ambientSources[i] = item;
                }
            }
            PausedSourcesCount = 0;
        }

        private void OnSettingsChanged()
        {
            if (!ModConfig.ModEnabled.Value || !ModConfig.EnableAmbientAudioCulling.Value)
            {
                ResumeAll();
            }
            else
            {
                ScanAndRegisterAmbientSources();
            }
        }

        public void Cleanup()
        {
            ModConfig.OnAudioSettingsChanged -= OnSettingsChanged;
            ResumeAll();
            _ambientSources.Clear();
            Instance = null;
        }

        private void OnDestroy()
        {
            Cleanup();
        }
    }
}
