using BepInEx.Logging;
using UnityEngine;
using System;
using System.Collections;
using Concentus.Structs;
using Concentus.Enums;
using BepInEx.Configuration;

namespace SpeakFromTarkov
{
    internal class VoiceChatManager : MonoBehaviour
    {
        public static VoiceChatManager Instance { get; private set; }
        private ManualLogSource Log => VoIPPlugin.Log;
        private AudioClip microphoneClip;
        private AudioSource echoSource;

        private int SampleRate => VoIPPlugin.SampleRate.Value;
        private int FrameSize => VoIPPlugin.FrameSize;

        public bool IsMuted { get; private set; } = false;
        private bool isSpeaking = false;
        public byte currentChannel = 1;
        private VoipMode currentMode = VoipMode.PTT;
        private float rawLevel = 0f;
        private float displayLevel = 0f;
        private float vadHoldTimer = 0f;
        private OpusEncoder encoder;
        private Texture2D statusTexture;
        private Texture2D backgroundTexture;

        // NOVO: Calibração automática
        private float peakRMS = 0f;
        private float autoMaxLevel = 0.02f;

        private enum VoipMode { Open, PTT, VAD }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // ECO COM PLAYONESHOT
            echoSource = gameObject.AddComponent<AudioSource>();
            echoSource.volume = 1f;
            echoSource.spatialBlend = 0f; // 2D

            Log.LogInfo("[SFT] VoiceChatManager iniciado.");
            InitializeTextures();
            InitializeOpus();
            StartCoroutine(InitializeMicrophoneCoroutine());
        }

        private IEnumerator InitializeMicrophoneCoroutine()
        {
            string device = VoIPPlugin.GetSelectedMicrophone();
            if (string.IsNullOrEmpty(device) || device.Contains("Nenhum"))
            {
                Log.LogError("[SFT] Nenhum microfone!");
                yield break;
            }

            yield return new WaitForSeconds(2f);

            int minFreq, maxFreq;
            Microphone.GetDeviceCaps(device, out minFreq, out maxFreq);
            Log.LogInfo($"[SFT] Mic Caps: {minFreq}-{maxFreq}Hz");

            int retries = 3;
            while (retries > 0)
            {
                Log.LogInfo($"[SFT] Iniciando mic ({retries})...");
                microphoneClip = Microphone.Start(device, true, 1, SampleRate);

                int timeout = 0;
                while (Microphone.GetPosition(device) < FrameSize && timeout < 1000)
                {
                    yield return null;
                    timeout++;
                }

                if (timeout < 1000)
                {
                    Log.LogInfo($"[SFT] Mic iniciado! Pos: {Microphone.GetPosition(device)}");
                    yield break;
                }

                Microphone.End(device);
                retries--;
                yield return new WaitForSeconds(2f);
            }
            Log.LogError("[SFT] Falha total no mic.");
        }

        private void InitializeOpus()
        {
            try
            {
#pragma warning disable CS0618
                encoder = new OpusEncoder(SampleRate, 1, OpusApplication.OPUS_APPLICATION_VOIP);
#pragma warning restore CS0618
                encoder.Bitrate = 12000;
                Log.LogInfo("[SFT] Opus OK.");
            }
            catch (Exception ex) { Log.LogError($"[SFT] Opus: {ex.Message}"); }
        }

        private void InitializeTextures()
        {
            statusTexture = new Texture2D(1, 1);
            backgroundTexture = new Texture2D(1, 1);
            backgroundTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.7f));
            backgroundTexture.Apply();
        }

        void Update()
        {
            if (microphoneClip == null) return;
            if (!Microphone.IsRecording(VoIPPlugin.GetSelectedMicrophone())) return;

            HandleKeys();
            UpdateAudioLevel();

            bool speak = ShouldSpeak();
            if (speak && !isSpeaking) { isSpeaking = true; StartSpeaking(); }
            else if (!speak && isSpeaking) isSpeaking = false;
        }

        private void UpdateAudioLevel()
        {
            int pos = Microphone.GetPosition(VoIPPlugin.GetSelectedMicrophone());
            if (pos <= 0) return;

            float[] samples = new float[FrameSize];
            microphoneClip.GetData(samples, Mathf.Max(0, pos - FrameSize));

            float rms = GetRMS(samples);
            rawLevel = rms * VoIPPlugin.MicGain.Value;

            if (IsMuted) rawLevel = 0f;

            // CALIBRAÇÃO AUTOMÁTICA
            if (rms > peakRMS) peakRMS = rms;
            if (peakRMS > 0.001f)
                autoMaxLevel = Mathf.Lerp(autoMaxLevel, peakRMS * 1.5f, 0.001f);

            displayLevel = Mathf.Lerp(displayLevel, rawLevel, 0.5f);

            Log.LogInfo($"[SFT] RMS: {rms:F6} | Peak: {peakRMS:F6} | AutoMax: {autoMaxLevel:F6} | Display: {displayLevel:F4}");
        }

        private bool ShouldSpeak()
        {
            if (IsMuted) return false;

            switch (currentMode)
            {
                case VoipMode.PTT:
                    return IsShortcutHeld(VoIPPlugin.PushToTalkKey.Value);
                case VoipMode.Open:
                    return true;
                case VoipMode.VAD:
                    if (rawLevel > VoIPPlugin.VADThreshold.Value)
                        vadHoldTimer = VoIPPlugin.VADDecayTime.Value;
                    if (vadHoldTimer > 0f)
                    {
                        vadHoldTimer -= Time.deltaTime;
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }

        private void StartSpeaking()
        {
            int pos = Microphone.GetPosition(VoIPPlugin.GetSelectedMicrophone());
            if (pos <= 0) return;

            float[] samples = new float[FrameSize];
            microphoneClip.GetData(samples, Mathf.Max(0, pos - FrameSize));

            if (IsMuted)
                Array.Clear(samples, 0, samples.Length);

            byte[] opusData = new byte[1275];
#pragma warning disable CS0618
            int len = encoder.Encode(samples, 0, FrameSize, opusData, 0, opusData.Length);
#pragma warning restore CS0618
            byte[] finalData = new byte[len];
            Array.Copy(opusData, finalData, len);

            NetworkManager.Instance?.BroadcastVoipData(finalData, currentChannel);

            // ECO COM PLAYONESHOT
            if (VoIPPlugin.EchoDelay.Value > 0f && !IsMuted)
                StartCoroutine(PlayEcho(samples));
        }

        private IEnumerator PlayEcho(float[] samples)
        {
            yield return new WaitForSeconds(VoIPPlugin.EchoDelay.Value);

            var clip = AudioClip.Create("SFT_Echo", samples.Length, 1, SampleRate, false);
            clip.SetData(samples, 0);

            echoSource.PlayOneShot(clip, VoIPPlugin.EchoVolume.Value);

            Destroy(clip, 2f);
        }

        private float GetRMS(float[] samples)
        {
            float sum = 0f;
            for (int i = 0; i < samples.Length; i++) sum += samples[i] * samples[i];
            return Mathf.Sqrt(sum / samples.Length);
        }

        private bool IsShortcutEmpty(KeyboardShortcut shortcut)
        {
            return shortcut.MainKey == KeyCode.None;
        }

        private bool IsShortcutHeld(KeyboardShortcut shortcut)
        {
            if (IsShortcutEmpty(shortcut)) return false;
            if (!Input.GetKey(shortcut.MainKey)) return false;

            foreach (var mod in shortcut.Modifiers)
            {
                if (!Input.GetKey(mod)) return false;
            }
            return true;
        }

        private bool IsShortcutDown(KeyboardShortcut shortcut)
        {
            if (IsShortcutEmpty(shortcut)) return false;
            if (!Input.GetKeyDown(shortcut.MainKey)) return false;

            foreach (var mod in shortcut.Modifiers)
            {
                if (!Input.GetKey(mod)) return false;
            }
            return true;
        }

        private void HandleKeys()
        {
            if (IsShortcutDown(VoIPPlugin.ToggleModeKey.Value))
            {
                currentMode = (VoipMode)(((int)currentMode + 1) % 3);
                Log.LogInfo($"[SFT] Modo → {currentMode}");
            }

            if (IsShortcutDown(VoIPPlugin.MuteKey.Value))
            {
                IsMuted = !IsMuted;
                Log.LogInfo($"[SFT] Mute: {(IsMuted ? "ON" : "OFF")}");
            }
        }

        void OnGUI() => DrawHUD();

        private void DrawHUD()
        {
            string channel = GetChannelName(currentChannel);
            Color status = GetStatusColor();
            string text = GetStatusText(channel);

            statusTexture.SetPixel(0, 0, status); statusTexture.Apply();
            GUI.DrawTexture(new Rect(10, 10, 30, 30), statusTexture);

            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(8, 8, 34, 2), statusTexture);
            GUI.DrawTexture(new Rect(8, 40, 34, 2), statusTexture);
            GUI.DrawTexture(new Rect(8, 8, 2, 34), statusTexture);
            GUI.DrawTexture(new Rect(40, 8, 2, 34), statusTexture);

            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(50, 8, 650, 32), backgroundTexture);

            GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 14, normal = { textColor = Color.white } };
            GUI.Label(new Rect(55, 12, 640, 24), text, style);
            DrawVADBar();
        }

        private Color GetStatusColor()
        {
            if (IsMuted) return Color.red;
            if (currentMode == VoipMode.Open) return Color.green;
            if (currentMode == VoipMode.PTT) return IsShortcutHeld(VoIPPlugin.PushToTalkKey.Value) ? Color.green : Color.red;
            return vadHoldTimer > 0f ? Color.green : Color.yellow;
        }

        private string GetStatusText(string channel)
        {
            string mode = currentMode.ToString();
            string state = IsMuted ? "[MUTADO]" :
                currentMode == VoipMode.PTT ? (IsShortcutHeld(VoIPPlugin.PushToTalkKey.Value) ? "TRANSMITINDO" : "AGUARDANDO") :
                currentMode == VoipMode.VAD ? (vadHoldTimer > 0f ? "FALANDO" : "SILENCIO") : "ABERTO";
            return $"{channel} | {mode} → {state}";
        }

        private void DrawVADBar()
        {
            float x = 55f, y = 45f, w = 200f, h = 8f;
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(x, y, w, h), backgroundTexture);

            // USA AUTO MAX LEVEL
            float fill = Mathf.Clamp01(displayLevel / autoMaxLevel);
            Color fillColor = fill > 0.7f ? Color.red : fill > 0.3f ? Color.yellow : Color.green;
            statusTexture.SetPixel(0, 0, fillColor); statusTexture.Apply();
            GUI.DrawTexture(new Rect(x, y, w * fill, h), statusTexture);

            GUI.color = Color.white;
            GUI.Label(new Rect(x, y + 10, w, 20), $"Áudio: {displayLevel:F4} | Max: {autoMaxLevel:F4}");
        }

        private string GetChannelName(byte ch)
        {
            switch (ch)
            {
                case 0: return "Canal 1 [RAID]";
                case 1: return "Canal 2 [LOBBY]";
                case 2: return "Canal 3 [ESPECTADOR]";
                default: return $"Canal {ch + 1}";
            }
        }

        public void SetGameStateChannel(bool inRaid) => currentChannel = inRaid ? (byte)0 : (byte)1;
        public void SetPlayerStatus(bool isSpectator) => currentChannel = isSpectator ? (byte)2 : currentChannel;

        public void Cleanup()
        {
            string device = VoIPPlugin.GetSelectedMicrophone();
            if (!string.IsNullOrEmpty(device) && Microphone.IsRecording(device))
                Microphone.End(device);
            encoder?.Dispose();
            if (echoSource != null) Destroy(echoSource);
            Log.LogInfo("[SFT] Cleanup completo.");
        }
    }
}