using UnityEngine;
using System;
using System.Threading;

namespace TRL_SpeakFromTarkov.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class MicrophoneCapturer : MonoBehaviour
    {
        public Action<float[]> OnAudioDataCaptured;
        
        private string deviceName;
        private int targetSampleRate;
        private int targetFrameSize;
        
        private int actualSampleRate;
        private int captureFrameSize;
        
        private AudioClip micClip;
        private float[] captureBuffer;
        private float[] outputBuffer;
        
        private AudioSource _audioSource;
        
        // Ring Buffer (Thread Safe)
        private float[] ringBuffer;
        private volatile int writePos;
        private volatile int readPos;
        private volatile int availableSamples;
        private readonly object bufferLock = new object();
        
        // Thread de processamento em background (imune ao FPS do jogo)
        private Thread captureThread;
        private volatile bool isThreadRunning = false;
        
        // Controle de estado
        public bool IsRecording { get; private set; }
        private bool hasPlayed = false;
        private int dspConfirmed = 0;
        private float restartCooldown = 0f;
        
        // Filtros de áudio
        private AudioFilter audioFilter;
        private AudioFilter echoAudioFilter;
        
        public void Initialize(int sampleRate, int frameSize)
        {
            this.targetSampleRate = sampleRate;
            this.targetFrameSize = frameSize;
            this.outputBuffer = new float[targetFrameSize];
            
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
            
            _audioSource.spatialBlend = 0f;
            _audioSource.volume = 1f;
        }
        
        public bool StartCapture(string device)
        {
            deviceName = device;
            actualSampleRate = AudioSettings.outputSampleRate;
            
            int[] fallbackRates = new int[] { 48000, 44100, 32000, 24000, 16000, 8000 };
            
            if (!string.IsNullOrEmpty(deviceName) && !deviceName.Contains("Nenhum"))
            {
                foreach (var rate in fallbackRates)
                {
                    micClip = Microphone.Start(deviceName, true, 1, rate);
                    if (micClip != null && micClip.samples > 0 && micClip.frequency > 0) break;
                    VoIPPlugin.Log.LogWarning($"[SFT] Falha ao iniciar '{deviceName}' em {rate}Hz. Tentando próxima...");
                    micClip = null;
                }
            }
            
            if (micClip == null || micClip.frequency == 0)
            {
                VoIPPlugin.Log.LogWarning("[SFT] Tentando dispositivo PADRÃO do SO...");
                deviceName = null;
                foreach (var rate in fallbackRates)
                {
                    micClip = Microphone.Start(null, true, 1, rate);
                    if (micClip != null && micClip.samples > 0 && micClip.frequency > 0) break;
                    micClip = null;
                }
            }
            
            if (micClip == null || micClip.frequency == 0)
            {
                VoIPPlugin.Log.LogError("[SFT] Erro fatal: Unity não detectou áudio válido em nenhuma frequência.");
                return false;
            }

            captureFrameSize = (int)Math.Round(actualSampleRate * 0.02);
            captureBuffer = new float[captureFrameSize];
            
            ringBuffer = new float[actualSampleRate * 2];
            writePos = 0;
            readPos = 0;
            availableSamples = 0;
            hasPlayed = false;
            dspConfirmed = 0;
            
            audioFilter = new AudioFilter(targetSampleRate, targetFrameSize, VoIPPlugin.HPFCutoff.Value);
            audioFilter.OpenThreshold  = VoIPPlugin.NoiseGateThreshold.Value;
            audioFilter.CloseThreshold = VoIPPlugin.NoiseGateThreshold.Value * 0.6f;
            audioFilter.HoldTime       = VoIPPlugin.NoiseGateHoldMs.Value / 1000f;
            audioFilter.UseRNNoise            = VoIPPlugin.UseRNNoise.Value;
            audioFilter.RNNoiseVADThreshold   = VoIPPlugin.RNNoiseVADThreshold.Value;
            audioFilter.RNNoiseGateHold       = VoIPPlugin.RNNoiseGateHoldMs.Value / 1000f;
            
            _audioSource.clip = micClip;
            _audioSource.loop = true;
            _audioSource.mute = false;
            
            VoIPPlugin.Log.LogInfo($"[SFT] Captura DSP iniciada. DSP={actualSampleRate}Hz. RNNoise={VoIPPlugin.UseRNNoise.Value}.");
            IsRecording = true;
            
            // Inicia a Thread em Background
            isThreadRunning = true;
            captureThread = new Thread(CaptureLoop);
            captureThread.IsBackground = true;
            captureThread.Priority = System.Threading.ThreadPriority.AboveNormal;
            captureThread.Start();
            
            return true;
        }
        
        public void StopCapture()
        {
            isThreadRunning = false;
            if (captureThread != null && captureThread.IsAlive)
            {
                captureThread.Join(500);
            }
            
            if (IsRecording)
            {
                _audioSource.Stop();
                if (Microphone.IsRecording(deviceName))
                    Microphone.End(deviceName);
            }
            IsRecording = false;
            hasPlayed = false;
            dspConfirmed = 0;
            audioFilter?.Dispose();
            audioFilter = null;
        }
        
        // O loop de extração agora roda numa Thread própria (imune aos FPS do Tarkov)
        private void CaptureLoop()
        {
            while (isThreadRunning)
            {
                bool hasFrame = false;
                
                lock (bufferLock)
                {
                    if (availableSamples >= captureFrameSize)
                    {
                        for (int i = 0; i < captureFrameSize; i++)
                        {
                            captureBuffer[i] = ringBuffer[readPos];
                            readPos = (readPos + 1) % ringBuffer.Length;
                        }
                        availableSamples -= captureFrameSize;
                        hasFrame = true;
                    }
                }
                
                if (hasFrame)
                {
                    ProcessFrame(captureBuffer);
                }
                else
                {
                    // Evita fritar a CPU enquanto aguarda os próximos samples do DSP
                    Thread.Sleep(2);
                }
            }
        }
        
        void Update()
        {
            if (!IsRecording || micClip == null) return;
            
            if (micClip.frequency == 0 || micClip.samples == 0)
            {
                VoIPPlugin.Log.LogError($"[SFT] Clip ficou inválido (freq={micClip.frequency}). Abortando.");
                StopCapture();
                return;
            }
            
            if (hasPlayed && !Microphone.IsRecording(deviceName))
            {
                restartCooldown += Time.deltaTime;
                if (restartCooldown >= 2.0f)
                {
                    restartCooldown = 0f;
                    VoIPPlugin.Log.LogWarning("[SFT] Microphone parou. Reiniciando captura...");
                    StopCapture();
                    StartCapture(deviceName);
                    return;
                }
            }
            else
            {
                restartCooldown = 0f;
            }
            
            if (hasPlayed && !_audioSource.isPlaying && Microphone.IsRecording(deviceName))
            {
                VoIPPlugin.Log.LogWarning("[SFT] AudioSource parou. Relançando...");
                hasPlayed = false;
                dspConfirmed = 0;
                lock (bufferLock) { availableSamples = 0; writePos = 0; readPos = 0; }
            }
            
            if (!hasPlayed && Microphone.GetPosition(deviceName) > 0)
            {
                _audioSource.Play();
                hasPlayed = true;
            }
            
            if (dspConfirmed == 1)
            {
                dspConfirmed = 2;
            }
            
            if (audioFilter != null)
            {
                audioFilter.OpenThreshold  = VoIPPlugin.NoiseGateThreshold.Value;
                audioFilter.CloseThreshold = VoIPPlugin.NoiseGateThreshold.Value * 0.6f;
                audioFilter.HoldTime       = VoIPPlugin.NoiseGateHoldMs.Value / 1000f;
                audioFilter.UseRNNoise            = VoIPPlugin.UseRNNoise.Value;
                audioFilter.RNNoiseVADThreshold   = VoIPPlugin.RNNoiseVADThreshold.Value;
                audioFilter.RNNoiseGateHold       = VoIPPlugin.RNNoiseGateHoldMs.Value / 1000f;
            }
        }
        
        private void ProcessFrame(float[] frameData)
        {
            if (actualSampleRate == targetSampleRate)
                Array.Copy(frameData, outputBuffer, targetFrameSize);
            else
                Resample(frameData, outputBuffer);
            
            audioFilter?.Apply(outputBuffer);
            
            OnAudioDataCaptured?.Invoke(outputBuffer);
        }

        private void Resample(float[] input, float[] output)
        {
            float ratio = (float)input.Length / output.Length;
            for (int i = 0; i < output.Length; i++)
            {
                float inputIndex = i * ratio;
                int index1 = (int)inputIndex;
                int index2 = Math.Min(index1 + 1, input.Length - 1);
                int index0 = Math.Max(index1 - 1, 0);
                int index3 = Math.Min(index1 + 2, input.Length - 1);
                
                float y0 = input[index0];
                float y1 = input[index1];
                float y2 = input[index2];
                float y3 = input[index3];
                
                float mu = inputIndex - index1;
                float mu2 = mu * mu;
                float a0 = -0.5f * y0 + 1.5f * y1 - 1.5f * y2 + 0.5f * y3;
                float a1 = y0 - 2.5f * y1 + 2f * y2 - 0.5f * y3;
                float a2 = -0.5f * y0 + 0.5f * y2;
                float a3 = y1;
                
                output[i] = (a0 * mu * mu2) + (a1 * mu2) + (a2 * mu) + a3;
            }
        }

        void OnAudioFilterRead(float[] data, int channels)
        {
            if (!IsRecording || micClip == null || !hasPlayed) return;
            if (dspConfirmed == 0) dspConfirmed = 1;
            
            lock (bufferLock)
            {
                if (ringBuffer == null) return;
                
                int len = data.Length / channels;
                if (availableSamples + len > ringBuffer.Length)
                {
                    // Buffer overflow: ignora pacote
                    return;
                }
                
                for (int i = 0; i < data.Length; i += channels)
                {
                    float sample = 0f;
                    for (int c = 0; c < channels; c++) sample += data[i + c];
                    sample /= channels;
                    
                    ringBuffer[writePos] = sample;
                    writePos = (writePos + 1) % ringBuffer.Length;
                }
                availableSamples += len;
            }
            
            // Silencia a saída para não ouvir a própria voz
            Array.Clear(data, 0, data.Length);
        }
        
        void OnDestroy()
        {
            StopCapture();
            audioFilter?.Dispose();
        }
    }
}