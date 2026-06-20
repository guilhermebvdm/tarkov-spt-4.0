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
            if (!_audioLoaded)
            {
                if (!_isRoutineRunning)
                {
                    // Fallback to loading it if it hasn't loaded yet
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

        public static void LoadAudioClips()
        {
            if (_audioLoaded) return;
            
            // Load audio synchronously bypassing UnityWebRequest
            LoadAudioSynchronous();
        }

        private static UnityWebRequest _wwwIn;
        private static UnityWebRequest _wwwOut;
        private static UnityWebRequest _wwwHeart;

        private static void LoadAudioSynchronous()
        {
            string pluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string breathInWav = Path.Combine(pluginPath, "breath_in.wav");
            string breathOutWav = Path.Combine(pluginPath, "breath_out.wav");
            string heartbeatWav = Path.Combine(pluginPath, "heartbeat.wav");

            if (File.Exists(breathInWav))
            {
                Plugin.Logger.LogInfo($"[HoldBreath] Native loading WAV: {breathInWav}");
                _breathInClip = WavUtility.ToAudioClip(breathInWav, "breath_in");
                if (_breathInClip != null)
                {
                    _breathInClip.hideFlags = HideFlags.HideAndDontSave;
                    Plugin.Logger.LogInfo($"[HoldBreath] Success breath_in (Length: {_breathInClip.length})");
                }
            }
            
            if (File.Exists(breathOutWav))
            {
                Plugin.Logger.LogInfo($"[HoldBreath] Native loading WAV: {breathOutWav}");
                _breathOutClip = WavUtility.ToAudioClip(breathOutWav, "breath_out");
                if (_breathOutClip != null)
                {
                    _breathOutClip.hideFlags = HideFlags.HideAndDontSave;
                    Plugin.Logger.LogInfo($"[HoldBreath] Success breath_out (Length: {_breathOutClip.length})");
                }
            }

            if (File.Exists(heartbeatWav))
            {
                Plugin.Logger.LogInfo($"[HoldBreath] Native loading WAV: {heartbeatWav}");
                _heartbeatClip = WavUtility.ToAudioClip(heartbeatWav, "heartbeat");
                if (_heartbeatClip != null)
                {
                    _heartbeatClip.hideFlags = HideFlags.HideAndDontSave;
                    Plugin.Logger.LogInfo($"[HoldBreath] Success heartbeat (Length: {_heartbeatClip.length})");
                }
            }

            _audioLoaded = true;
        }

        public static class WavUtility
        {
            public static AudioClip ToAudioClip(string filePath, string clipName)
            {
                try
                {
                    byte[] fileData = File.ReadAllBytes(filePath);

                    int channels = fileData[22];
                    int sampleRate = BitConverter.ToInt32(fileData, 24);

                    int pos = 12;
                    // Search for "data" chunk
                    while (!(fileData[pos] == 100 && fileData[pos + 1] == 97 && fileData[pos + 2] == 116 && fileData[pos + 3] == 97))
                    {
                        pos += 4;
                        int chunkSize = BitConverter.ToInt32(fileData, pos);
                        pos += 4 + chunkSize;
                        if (pos >= fileData.Length - 8)
                        {
                            Plugin.Logger.LogError($"[HoldBreath] Invalid WAV format or missing data chunk: {clipName}");
                            return null;
                        }
                    }
                    pos += 4;
                    int subchunk2Size = BitConverter.ToInt32(fileData, pos);
                    pos += 4;

                    float[] samples = new float[subchunk2Size / 2];
                    for (int i = 0; i < samples.Length; i++)
                    {
                        short s = BitConverter.ToInt16(fileData, pos);
                        samples[i] = s / 32768f;
                        pos += 2;
                    }

                    AudioClip clip = AudioClip.Create(clipName, samples.Length / channels, channels, sampleRate, false);
                    clip.SetData(samples, 0);
                    return clip;
                }
                catch (Exception e)
                {
                    Plugin.Logger.LogError($"[HoldBreath] Error loading WAV manually: {e.Message}");
                    return null;
                }
            }
        }

    }
}
