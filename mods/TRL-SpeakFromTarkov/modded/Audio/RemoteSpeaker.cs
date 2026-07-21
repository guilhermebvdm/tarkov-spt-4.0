using System;
using UnityEngine;
using Concentus.Structs;
using System.Collections.Concurrent;

namespace TRL_SpeakFromTarkov.Audio
{
    public class RemoteSpeaker : MonoBehaviour
    {
        private OpusDecoder decoder;
        private AudioSource audioSource;

        private ConcurrentQueue<byte[]> packetQueue = new ConcurrentQueue<byte[]>();

        private int sampleRate;
        private int frameSize;
        private float[] opusDecodeBuffer;

        private float currentDistanceTarget = 30f;
        private float smoothedDistance = 30f;

        private float[] streamBuffer;
        private volatile int streamWritePos = 0;
        private volatile int streamReadPos  = 0;

        private bool isBuffering = true;
        
        // Jitter settings: initial buffering requires 150ms, recovery requires 40ms
        private int jitterInitialSamples;
        private int jitterRecoverySamples;
        private int currentJitterTarget;

        public void Initialize(int sampleRate, int frameSize, float spatialBlend = 1f)
        {
            this.sampleRate = sampleRate;
            this.frameSize  = frameSize;

            int jitterMs = 150;
            if (VoIPPlugin.NetworkJitterBufferMs != null)
                jitterMs = (int)VoIPPlugin.NetworkJitterBufferMs.Value;

            jitterInitialSamples = (int)(sampleRate * (jitterMs / 1000f));
            jitterRecoverySamples = (int)(sampleRate * 0.040f); // 40ms
            currentJitterTarget   = jitterInitialSamples;

            this.opusDecodeBuffer = new float[frameSize];
            this.streamBuffer     = new float[sampleRate * 3]; // 3s de buffer

            try
            {
#pragma warning disable CS0618
                decoder = new OpusDecoder(sampleRate, 1);
#pragma warning restore CS0618
            }
            catch (Exception ex)
            {
                VoIPPlugin.Log.LogError($"[SFT] RemoteSpeaker Opus erro: {ex.Message}");
            }

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.spatialize    = spatialBlend > 0f;
            audioSource.dopplerLevel  = 0f;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.minDistance = 2f;
            
            float maxDist = 30f;
            if (VoIPPlugin.MaxHearingDistance != null)
                maxDist = VoIPPlugin.MaxHearingDistance.Value;
                
            audioSource.maxDistance = maxDist;
            currentDistanceTarget = maxDist;
            smoothedDistance = maxDist;
            audioSource.loop          = true;
            audioSource.clip = AudioClip.Create("SftStream", sampleRate, 1, sampleRate, false);
            audioSource.Play();
        }

        public void SetVolume(float volume)
        {
            if (audioSource != null)
                audioSource.volume = volume;
        }

        public void EnqueuePacket(byte[] opusData, float voiceLevel = 0f)
        {
            packetQueue.Enqueue(opusData);
            
            float maxBase = 30f;
            if (VoIPPlugin.MaxHearingDistance != null)
                maxBase = VoIPPlugin.MaxHearingDistance.Value;
                
            // Mapeia VoiceLevel (0 a 1) para um multiplicador de distância.
            // Whisper (~0.01) -> 0.33x (10m). Normal (~0.1) -> 1.0x (30m). Grito (>0.3) -> 2.0x (60m).
            float distanceMultiplier = Mathf.Clamp((voiceLevel * 10f), 0.33f, 2.0f);
            currentDistanceTarget = maxBase * distanceMultiplier;
        }

        void Update()
        {
            if (audioSource != null)
            {
                smoothedDistance = Mathf.Lerp(smoothedDistance, currentDistanceTarget, Time.deltaTime * 5f);
                audioSource.maxDistance = smoothedDistance;
            }

            if (decoder == null) return;
            
            bool useFec = false;
            if (VoIPPlugin.OpusFEC != null)
                useFec = VoIPPlugin.OpusFEC.Value;

            while (packetQueue.TryDequeue(out byte[] opusData))
            {
#pragma warning disable CS0618
                int len = decoder.Decode(opusData, 0, opusData.Length, opusDecodeBuffer, 0, frameSize, useFec);
#pragma warning restore CS0618

                int currentWritePos = streamWritePos;
                for (int i = 0; i < len; i++)
                {
                    streamBuffer[(currentWritePos + i) % streamBuffer.Length] = opusDecodeBuffer[i];
                }
                
                streamWritePos = (currentWritePos + len) % streamBuffer.Length;
            }
        }

        void OnAudioFilterRead(float[] data, int channels)
        {
            int wPos = streamWritePos;
            int rPos = streamReadPos;
            int available = (wPos - rPos + streamBuffer.Length) % streamBuffer.Length;

            // FIX: Clock Drift (Overrun Prevention)
            // Se o relógio do Sender for levemente mais rápido que o do Receiver,
            // o buffer vai encher lentamente ao longo de minutos.
            // Para evitar delay absurdo ou estouro de buffer, nós dropamos pacotes velhos.
            int maxAllowedDelay = jitterInitialSamples * 2; // Pelo menos 2x o Jitter inicial
            if (available > maxAllowedDelay)
            {
                // Avança o ponteiro de leitura para manter apenas o equivalente ao Jitter Inicial
                rPos = (wPos - jitterInitialSamples + streamBuffer.Length) % streamBuffer.Length;
                available = jitterInitialSamples;
            }

            if (isBuffering)
            {
                if (available >= currentJitterTarget)
                    isBuffering = false;
            }
            else
            {
                if (available == 0)
                {
                    isBuffering = true;
                    // Switch to recovery target for future underruns to minimize gap
                    currentJitterTarget = jitterRecoverySamples;
                }
            }

            bool shouldPlay = !isBuffering;

            for (int i = 0; i < data.Length; i += channels)
            {
                float sample = 0f;

                if (shouldPlay && rPos != wPos)
                {
                    sample = streamBuffer[rPos];
                    rPos = (rPos + 1) % streamBuffer.Length;
                }

                for (int c = 0; c < channels; c++)
                    data[i + c] = sample;
            }

            streamReadPos = rPos;
        }

        void OnDestroy()
        {
            decoder = null;
        }
    }
}