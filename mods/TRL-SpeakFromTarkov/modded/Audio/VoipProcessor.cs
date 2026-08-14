using System;
using UnityEngine;
using Concentus.Structs;
using Concentus.Enums;
using Comfort.Common;

namespace TRL_SpeakFromTarkov.Audio
{
    public class VoipProcessor : MonoBehaviour
    {
        public Action<byte[], float> OnOpusDataEncoded = null!;
        
        private OpusEncoder encoder = null!;
        private int frameSize;
        
        // VAD e Níveis
        public float RawLevel { get; private set; }
        public float DisplayLevel { get; private set; }
        public float PeakLevel { get; private set; }
        public int LastOpusBytes { get; private set; }
        
        private byte[] opusBuffer = new byte[1275]; // Alocação única
        private float vadHoldTimer = 0f;
        
        public BotVoiceBridge botVoiceBridge { get; set; } = null!;
        
        public bool IsMuted { get; set; }
        public bool IsPTTActive { get; set; }
        public bool IsTransmitting { get; private set; }
        public VoipMode CurrentMode { get; set; } = VoipMode.VAD;

        public enum VoipMode { Open, PTT, VAD }
        
        public void Initialize(int sampleRate, int frameSize)
        {
            this.frameSize = frameSize;
            try
            {
#pragma warning disable CS0618
                encoder = new OpusEncoder(sampleRate, 1, OpusApplication.OPUS_APPLICATION_VOIP);
#pragma warning restore CS0618
                encoder.Bitrate = VoIPPlugin.OpusBitrate.Value;
                encoder.Complexity = VoIPPlugin.OpusComplexity.Value;
                
                if (VoIPPlugin.OpusFEC.Value)
                {
                    encoder.UseInbandFEC = true;
                }
            }
            catch (Exception ex)
            {
                VoIPPlugin.Log.LogError($"[SFT] Erro OpusEncoder: {ex.Message}");
            }
        }
        
        public void ProcessAudio(float[] pcmSamples)
        {
            float rms = GetRMS(pcmSamples);
            RawLevel = rms * VoIPPlugin.MicGain.Value;
            
            if (IsMuted) RawLevel = 0f;
            
            // Decaimento suave do Peak (Corrige HUD travada)
            if (rms > PeakLevel) PeakLevel = rms;
            else PeakLevel = Mathf.Lerp(PeakLevel, 0f, 0.02f * 0.5f); // 0.02f em vez de Time.deltaTime (Thread Safe)
            
            // VU meter: ataque instantâneo, decaimento suave
            if (RawLevel >= DisplayLevel)
                DisplayLevel = RawLevel;                                    // sobe na hora
            else
                DisplayLevel = Mathf.Lerp(DisplayLevel, RawLevel, 0.15f);   // desce devagar
            
            UpdateTransmittingState();
            
            // Só transmite se estiver ativo E se o nível de áudio for perceptível (RMS > 0.003f).
            // Isso impede o envio desnecessário de 50 pacotes mudos por segundo sobre o LiteNetLib do FIKA!
            if (IsTransmitting && RawLevel > 0.003f)
            {
                float outputVolume = VoIPPlugin.OutputVolume.Value;
                if (outputVolume != 1.0f)
                {
                    for (int i = 0; i < pcmSamples.Length; i++)
                    {
                        pcmSamples[i] *= outputVolume;
                    }
                }
                Transmit(pcmSamples);
            }
        }

        private void UpdateTransmittingState()
        {
            if (IsMuted)
            {
                IsTransmitting = false;
                return;
            }

            switch (CurrentMode)
            {
                case VoipMode.PTT:
                    IsTransmitting = IsPTTActive;
                    break;
                case VoipMode.Open:
                    IsTransmitting = true;
                    break;
                case VoipMode.VAD:
                    if (RawLevel > VoIPPlugin.VADThreshold.Value)
                        vadHoldTimer = VoIPPlugin.VADDecayTime.Value;
                        
                    if (vadHoldTimer > 0f)
                    {
                        vadHoldTimer -= 0.02f;
                        IsTransmitting = true;
                    }
                    else
                    {
                        IsTransmitting = false;
                    }
                    break;
                default:
                    IsTransmitting = false;
                    break;
            }
        }
        
        private void Transmit(float[] samples)
        {
            if (encoder == null) return;
            
            // Grampo de proteção contra estouro de amplitude (evita chiado de clipping digital no Opus)
            for (int i = 0; i < samples.Length; i++)
            {
                if (samples[i] > 0.95f) samples[i] = 0.95f;
                else if (samples[i] < -0.95f) samples[i] = -0.95f;
            }
            
            try
            {
#pragma warning disable CS0618
                int len = encoder.Encode(samples, 0, frameSize, opusBuffer, 0, opusBuffer.Length);
#pragma warning restore CS0618

                if (len > 0)
                {
                    LastOpusBytes = len;
                    byte[] poolBuffer = System.Buffers.ArrayPool<byte>.Shared.Rent(len);
                    try
                    {
                        Array.Copy(opusBuffer, poolBuffer, len);
                        byte[] finalData = new byte[len];
                        Array.Copy(poolBuffer, finalData, len);
                        OnOpusDataEncoded?.Invoke(finalData, DisplayLevel);
                    }
                    finally
                    {
                        System.Buffers.ArrayPool<byte>.Shared.Return(poolBuffer);
                    }
                }
            }
            catch (Exception ex)
            {
                VoIPPlugin.Log.LogError($"[SFT] Opus Encode falhou (frameSize={frameSize}, samples={samples.Length}): {ex.GetType().Name}");
            }
        }
        
        private float GetRMS(float[] samples)
        {
            float sum = 0f;
            for (int i = 0; i < samples.Length; i++) 
                sum += samples[i] * samples[i];
            return Mathf.Sqrt(sum / samples.Length);
        }
        
        void OnDestroy()
        {
            encoder = null!;
        }
    }
}
