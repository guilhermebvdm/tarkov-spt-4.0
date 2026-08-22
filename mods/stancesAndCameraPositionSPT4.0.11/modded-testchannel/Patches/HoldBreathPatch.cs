using System;
using System.IO;
using System.Reflection;
using SPT.Reflection.Patching;
using EFT;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using BepInEx;

namespace CameraRotationMod.Patches
{
    public class HoldBreathPatch : ModulePatch
    {
        private static AudioClip _breathInClip;
        private static AudioClip _breathOutClip;
        private static AudioClip _heartbeatClip;
        private static AudioSource _audioSource;
        private static AudioSource _heartbeatSource;
        private static bool _audioLoaded = false;
        private static bool _isRoutineRunning = false;

        public static bool IsHoldingBreath = false;

        protected override MethodBase GetTargetMethod()
        {
            // public virtual void HoldBreath(bool enable) in BasePhysicalClass or PlayerPhysicalClass
            return AccessTools.Method(typeof(PlayerPhysicalClass), nameof(PlayerPhysicalClass.HoldBreath));
        }

        [PatchPostfix]
        private static void Postfix(PlayerPhysicalClass __instance, bool enable)
        {
            // Only care about the main player
            if (__instance.Player_0 == null || !__instance.Player_0.IsYourPlayer) return;

            // Only act if the state actually changed (HoldBreath can be spammed by the game)
            bool actuallyHolding = __instance.HoldingBreath;
            if (actuallyHolding != IsHoldingBreath)
            {
                IsHoldingBreath = actuallyHolding;

                if (Plugin._EnableCustomBreathAudio.Value)
                {
                    PlayBreathAudio(IsHoldingBreath);
                }
            }
        }

        private static void PlayBreathAudio(bool breathIn)
        {
            // Os clips podem ter sido descarregados numa transição de cena (menu -> hideout -> raid),
            // ficando com length 0 mesmo com a referência viva. Se inválidos, recarrega no contexto
            // atual e ignora este disparo (o próximo já toca).
            if (!_audioLoaded || !ClipsValid())
            {
                if (!_isRoutineRunning)
                {
                    Plugin.Logger.LogWarning("[HoldBreath] Clips ausentes/descarregados — recarregando no contexto atual.");
                    _audioLoaded = false;
                    LoadAudioClips();
                }
                return;
            }

            if (_audioSource == null)
            {
                GameObject audioObj = new GameObject("HoldBreathAudioSource");
                UnityEngine.Object.DontDestroyOnLoad(audioObj);
                _audioSource = audioObj.AddComponent<AudioSource>();
                _audioSource.spatialBlend = 0f; // 2D sound
                _audioSource.playOnAwake = false;

                _heartbeatSource = audioObj.AddComponent<AudioSource>();
                _heartbeatSource.spatialBlend = 0f;
                _heartbeatSource.playOnAwake = false;
                _heartbeatSource.loop = true;
            }

            AudioClip clip = breathIn ? _breathInClip : _breathOutClip;
            if (clip != null)
            {
                _audioSource.volume = breathIn ? Plugin._BreathInVolume.Value : Plugin._BreathOutVolume.Value;
                _audioSource.PlayOneShot(clip);
                Plugin.Logger.LogInfo($"[HoldBreath] Playing {(breathIn ? "breath_in" : "breath_out")} - Volume: {_audioSource.volume} - Clip length: {clip.length}");
            }
            else
            {
                Plugin.Logger.LogWarning($"[HoldBreath] Clip for {(breathIn ? "breath_in" : "breath_out")} is NULL!");
            }

            if (breathIn)
            {
                if (_heartbeatClip != null)
                {
                    _heartbeatSource.clip = _heartbeatClip;
                    _heartbeatSource.volume = Plugin._HeartbeatVolume.Value;
                    _heartbeatSource.Play();
                    Plugin.Logger.LogInfo($"[HoldBreath] Started heartbeat loop - Volume: {_heartbeatSource.volume}");
                }
            }
            else
            {
                if (_heartbeatSource.isPlaying)
                {
                    _heartbeatSource.Stop();
                }
            }
        }

        // Carregamento via decodificador NATIVO do Unity (UnityWebRequestMultimedia). Suporta
        // OGG/WAV/PCM/float sem parser manual. Substituiu o antigo WavUtility, que só lia PCM 16-bit
        // e produzia ruído saturado nos arquivos float32 (ver diagnóstico de áudio). Os assets foram
        // convertidos para OGG Vorbis mono (heartbeat: 23 MB -> ~460 KB). Roda como coroutine no
        // Plugin (MonoBehaviour) porque o request é assíncrono.
        public static void LoadAudioClips()
        {
            if (_audioLoaded || _isRoutineRunning) return;
            if (Plugin.Instance == null)
            {
                Plugin.Logger.LogWarning("[HoldBreath] Plugin.Instance ainda null — carregamento de áudio adiado.");
                return;
            }
            _isRoutineRunning = true;
            Plugin.Instance.StartCoroutine(LoadAudioRoutine());
        }

        private static IEnumerator LoadAudioRoutine()
        {
            string pluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            yield return LoadClip(Path.Combine(pluginPath, "breath_in.ogg"),  "breath_in",  c => _breathInClip = c);
            yield return LoadClip(Path.Combine(pluginPath, "breath_out.ogg"), "breath_out", c => _breathOutClip = c);
            yield return LoadClip(Path.Combine(pluginPath, "heartbeat.ogg"),  "heartbeat",  c => _heartbeatClip = c);

            _audioLoaded = true;
            _isRoutineRunning = false;
            Plugin.Logger.LogInfo("[HoldBreath] Audio load routine finished.");
        }

        private static IEnumerator LoadClip(string path, string clipName, Action<AudioClip> assign)
        {
            if (!File.Exists(path))
            {
                Plugin.Logger.LogWarning($"[HoldBreath] Audio file not found: {path}");
                yield break;
            }

            // "file://" + forward slashes — exigido pelo UnityWebRequest para caminhos locais no Windows.
            string uri = "file://" + path.Replace("\\", "/");
            using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.OGGVORBIS))
            {
                // streamAudio=false força o áudio a ser TOTALMENTE decodificado em memória. Sem isto,
                // o AudioClip fica em modo streaming e depende do DownloadHandler vivo; ao sair do
                // using (Dispose), o clip perde os dados — length vira 0 e PlayOneShot não toca nada.
                ((DownloadHandlerAudioClip)uwr.downloadHandler).streamAudio = false;

                yield return uwr.SendWebRequest();

                // uwr.error é estável entre versões do Unity (evita isNetworkError/isHttpError/result,
                // que mudam de assinatura/disponibilidade entre 2019/2020/2022).
                if (!string.IsNullOrEmpty(uwr.error))
                {
                    Plugin.Logger.LogError($"[HoldBreath] Failed to load {clipName}: {uwr.error}");
                    yield break;
                }

                AudioClip src = DownloadHandlerAudioClip.GetContent(uwr);
                if (src == null || src.samples == 0)
                {
                    Plugin.Logger.LogError($"[HoldBreath] Decoded clip inválido para {clipName} (src null ou 0 samples).");
                    yield break;
                }

                // Cópia STANDALONE: extrai o PCM decodificado e cria um AudioClip próprio, 100% em
                // memória. Imune ao Dispose do UnityWebRequest e ao modo streaming — que zerava
                // clip.length no momento do play (clip lia do download handler já descartado).
                float[] data = new float[src.samples * src.channels];
                src.GetData(data, 0);
                AudioClip clip = AudioClip.Create(clipName, src.samples, src.channels, src.frequency, false);
                clip.SetData(data, 0);
                clip.hideFlags = HideFlags.HideAndDontSave;
                assign(clip);
                Plugin.Logger.LogInfo($"[HoldBreath] Loaded {clipName} (length {clip.length:F2}s, ch {clip.channels}, freq {clip.frequency})");
            }
        }

        /// <summary>Todos os clips presentes e com dados válidos (length &gt; 0).</summary>
        private static bool ClipsValid()
        {
            return _breathInClip != null && _breathInClip.length > 0f
                && _breathOutClip != null && _breathOutClip.length > 0f
                && _heartbeatClip != null && _heartbeatClip.length > 0f;
        }

        private static void StopHeartbeat()
        {
            if (_heartbeatSource != null && _heartbeatSource.isPlaying)
                _heartbeatSource.Stop();
        }

        /// <summary>
        /// Início de raid/hideout (GameWorld.OnGameStarted). Carrega o áudio DENTRO do contexto da
        /// cena de jogo — evita os clips descarregados pela transição menu -> jogo (causa do length 0).
        /// Também zera o estado do hold (anti-heartbeat-órfão entre raids).
        /// </summary>
        public static void OnRaidStart()
        {
            IsHoldingBreath = false;
            StopHeartbeat();
            if (!_audioLoaded || !ClipsValid())
            {
                _audioLoaded = false;
                LoadAudioClips();
            }
        }

        /// <summary>Fim de raid/hideout — para o heartbeat e zera o estado (idempotente).</summary>
        public static void OnRaidEnd()
        {
            IsHoldingBreath = false;
            StopHeartbeat();
        }

    }
}
