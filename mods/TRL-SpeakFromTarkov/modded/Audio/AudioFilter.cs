using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace TRL_SpeakFromTarkov.Audio
{
    /// <summary>
    /// Pipeline de filtros de áudio: RNNoise (neural) → fallback HPF+Gate.
    /// RNNoise processa em frames de 480 samples a 48kHz.
    /// </summary>
    public class AudioFilter : IDisposable
    {
        // ── RNNoise P/Invoke ──────────────────────────────────────────────────
        private const string RNNOISE_DLL = "rnnoise";

        [DllImport(RNNOISE_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr rnnoise_create(IntPtr model);

        [DllImport(RNNOISE_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern void rnnoise_destroy(IntPtr st);

        [DllImport(RNNOISE_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern float rnnoise_process_frame(IntPtr st, float[] outFrame, float[] inFrame);

        [DllImport(RNNOISE_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern int rnnoise_get_frame_size();

        private const int  RNNOISE_FRAME  = 480;      // sempre 480 samples @ 48kHz
        private const float PCM_SCALE     = 32768f;   // RNNoise espera range de short [-32768,32767]

        // ── Estado RNNoise ────────────────────────────────────────────────────
        private IntPtr  _rnState    = IntPtr.Zero;
        private bool    _rnAvailable = false;
        private float[] _rnInBuf    = new float[RNNOISE_FRAME];
        private float[] _rnOutBuf   = new float[RNNOISE_FRAME];

        // Filas para processamento em blocos (evita timing shifts e glitches)
        private float[] _inputQueue = new float[8192];
        private int _inputWrite = 0;
        private int _inputRead = 0;
        private int _inputCount = 0;

        private float[] _outputQueue = new float[8192];
        private int _outputWrite = 0;
        private int _outputRead = 0;
        private int _outputCount = 0;

        // ── HPF (fallback) ────────────────────────────────────────────────────
        private float _hpfAlpha;
        private float _hpfPrevIn  = 0f;
        private float _hpfPrevOut = 0f;

        // ── Noise Gate (fallback) ─────────────────────────────────────────────
        private float _gateGain      = 0f;
        private float _gateHoldTimer = 0f;
        private bool  _gateOpen      = false;

        // ── RNNoise Noise Gate ────────────────────────────────────────────────
        private float _rnGateGain      = 0f;
        private float _rnGateHoldTimer = 0f;
        private bool  _rnGateOpen      = false;

        private readonly float _frameDuration;
        private readonly float _attackRate;
        private readonly float _releaseRate;

        public float OpenThreshold  { get; set; } = 0.008f;
        public float CloseThreshold { get; set; } = 0.005f;
        public float HoldTime       { get; set; } = 0.15f;

        public bool UseRNNoise { get; set; } = true;
        public float RNNoiseVADThreshold { get; set; } = 0.85f;
        public float RNNoiseGateHold { get; set; } = 0.15f;

        // ── Novos Filtros Avançados (Qualidade Estúdio) ────────
        public bool EnableAGC { get; set; } = true;
        public bool EnableLimiter { get; set; } = true;
        
        private float _lpfAlpha;
        private float _lpfPrevOut = 0f;
        public float LPFCutoffHz 
        { 
            set 
            {
                float dt = 1f / 48000f; // Sample rate padrão
                float rc = 1f / (2f * Mathf.PI * value);
                _lpfAlpha = dt / (rc + dt);
            }
        }
        
        private float _agcCurrentGain = 1f;

        // ── VAD signal do RNNoise (0-1, disponível para uso externo) ──────────
        public float LastVadProbability { get; private set; } = 0f;

        public AudioFilter(int sampleRate, int frameSize, float hpfCutoffHz)
        {
            _frameDuration = (float)frameSize / sampleRate;
            _attackRate    = _frameDuration / 0.005f;
            _releaseRate   = _frameDuration / 0.080f;

            // Coeficiente IIR do HPF para fallback
            float dt   = 1f / sampleRate;
            float rc   = 1f / (2f * Mathf.PI * hpfCutoffHz);
            _hpfAlpha  = rc / (rc + dt);

            TryInitRNNoise();
            Reset(); // Inicializa as filas com latência de silêncio inicial
        }

        private void TryInitRNNoise()
        {
            try
            {
                _rnState     = rnnoise_create(IntPtr.Zero);
                _rnAvailable = (_rnState != IntPtr.Zero);

                if (_rnAvailable)
                    VoIPPlugin.Log.LogInfo($"[SFT] RNNoise carregado com sucesso. Frame={RNNOISE_FRAME} samples. Supressão neural ativa.");
            }
            catch (DllNotFoundException)
            {
                VoIPPlugin.Log.LogWarning("[SFT] rnnoise.dll não encontrada. Usando HPF+Gate como fallback.");
                _rnAvailable = false;
            }
            catch (Exception ex)
            {
                VoIPPlugin.Log.LogWarning($"[SFT] RNNoise falhou ao inicializar ({ex.GetType().Name}). Usando fallback.");
                _rnAvailable = false;
            }
        }

        /// <summary>
        /// Aplica o filtro ao buffer in-place.
        /// Se rnnoise.dll estiver disponível e ativado, usa supressão neural.
        /// Caso contrário, usa HPF + Noise Gate.
        /// </summary>
        public void Apply(float[] buffer)
        {
            // O AGC tenta equalizar o ganho de quem fala baixo vs quem grita ANTES do gate de ruído.
            if (EnableAGC) ApplyAGC(buffer);

            if (_rnAvailable && UseRNNoise)
            {
                ApplyHPF(buffer);
                if (_lpfAlpha > 0) ApplyLPF(buffer);
                ApplyRNNoise(buffer);
            }
            else
            {
                ApplyFallback(buffer);
            }
            
            // O limiter é a última barreira de proteção de áudio
            if (EnableLimiter) ApplyLimiter(buffer);
        }

        // ── RNNoise com Fila (Jitter-Free) ────────────────────────────────────
        private void ApplyRNNoise(float[] buffer)
        {
            // 1. Escreve a entrada na fila
            for (int i = 0; i < buffer.Length; i++)
            {
                _inputQueue[_inputWrite] = buffer[i];
                _inputWrite = (_inputWrite + 1) % _inputQueue.Length;
                _inputCount++;
            }

            // 2. Processa em blocos de 480 enquanto houver dados
            while (_inputCount >= RNNOISE_FRAME)
            {
                for (int i = 0; i < RNNOISE_FRAME; i++)
                {
                    _rnInBuf[i] = _inputQueue[_inputRead] * PCM_SCALE;
                    _inputRead = (_inputRead + 1) % _inputQueue.Length;
                }
                _inputCount -= RNNOISE_FRAME;

                float vad = rnnoise_process_frame(_rnState, _rnOutBuf, _rnInBuf);
                LastVadProbability = vad;

                // RNNoise JÁ remove o ruído atenuando-o na saída.
                // Não faremos "hard gate" com a probabilidade do VAD, pois isso corta a voz em sons fracos (f, s) causando o efeito "robotizado".
                
                for (int i = 0; i < RNNOISE_FRAME; i++)
                {
                    _outputQueue[_outputWrite] = _rnOutBuf[i] / PCM_SCALE;
                    _outputWrite = (_outputWrite + 1) % _outputQueue.Length;
                    _outputCount++;
                }
            }

            // 3. Lê o resultado da fila de saída
            int toRead = Math.Min(buffer.Length, _outputCount);
            for (int i = 0; i < toRead; i++)
            {
                buffer[i] = _outputQueue[_outputRead];
                _outputRead = (_outputRead + 1) % _outputQueue.Length;
            }
            _outputCount -= toRead;

            // Preenche o restante com silêncio se faltar dados (apenas nos primeiros milissegundos)
            if (toRead < buffer.Length)
            {
                for (int i = toRead; i < buffer.Length; i++)
                {
                    buffer[i] = 0f;
                }
            }
        }

        // ── Fallback: HPF + Noise Gate ────────────────────────────────────────
        private void ApplyFallback(float[] buf)
        {
            ApplyHPF(buf);
            ApplyNoiseGate(buf);
        }

        private void ApplyHPF(float[] buf)
        {
            for (int i = 0; i < buf.Length; i++)
            {
                float x = buf[i];
                float y = _hpfAlpha * (_hpfPrevOut + x - _hpfPrevIn);
                _hpfPrevIn  = x;
                _hpfPrevOut = y;
                buf[i]      = y;
            }
        }

        private void ApplyLPF(float[] buf)
        {
            for (int i = 0; i < buf.Length; i++)
            {
                float x = buf[i];
                float y = _lpfPrevOut + _lpfAlpha * (x - _lpfPrevOut);
                _lpfPrevOut = y;
                buf[i] = y;
            }
        }

        private void ApplyAGC(float[] buf)
        {
            float rms = GetRMS(buf);
            float targetGain = 1f;
            
            // Se houver áudio útil, tentar equalizar para um RMS de 0.05
            if (rms > 0.001f)
            {
                float targetRms = 0.05f;
                targetGain = targetRms / rms;
                targetGain = Mathf.Clamp(targetGain, 0.2f, 4.0f); // Boost máximo 4x, Redução máxima 5x
            }
            
            // Transição absurdamente suave (0.001) para não parecer que o ganho está tremendo (pumping)
            for (int i = 0; i < buf.Length; i++)
            {
                _agcCurrentGain = Mathf.Lerp(_agcCurrentGain, targetGain, 0.001f);
                buf[i] *= _agcCurrentGain;
            }
        }

        private void ApplyLimiter(float[] buf)
        {
            // Soft clipping simple & robusto (limite em 0.98f para evitar estouro total em 1.0f)
            for (int i = 0; i < buf.Length; i++)
            {
                if (buf[i] > 0.98f) buf[i] = 0.98f;
                else if (buf[i] < -0.98f) buf[i] = -0.98f;
            }
        }

        private void ApplyNoiseGate(float[] buf)
        {
            float rms = GetRMS(buf);

            if (rms >= OpenThreshold) { _gateOpen = true; _gateHoldTimer = HoldTime; }
            else if (_gateOpen)
            {
                _gateHoldTimer -= _frameDuration;
                if (_gateHoldTimer <= 0f) _gateOpen = false;
            }

            float targetGain = _gateOpen ? 1f : 0f;
            float attackRatePerSample = (1f / 0.005f) / 48000f;
            float releaseRatePerSample = (1f / 0.080f) / 48000f;
            float ratePerSample = _gateOpen ? attackRatePerSample : releaseRatePerSample;

            for (int i = 0; i < buf.Length; i++)
            {
                _gateGain = Mathf.MoveTowards(_gateGain, targetGain, ratePerSample);
                buf[i] *= _gateGain;
            }
        }

        private static float GetRMS(float[] buf)
        {
            float sum = 0f;
            for (int i = 0; i < buf.Length; i++) sum += buf[i] * buf[i];
            return Mathf.Sqrt(sum / buf.Length);
        }

        public void Reset(int latency)
        {
            _hpfPrevIn = _hpfPrevOut = 0f;
            _lpfPrevOut = 0f;
            _agcCurrentGain = 1f;
            _gateGain  = 0f; _gateHoldTimer = 0f; _gateOpen = false;
            _rnGateGain = 0f; _rnGateHoldTimer = 0f; _rnGateOpen = false;
            _inputWrite = _inputRead = _inputCount = 0;
            _outputWrite = _outputRead = _outputCount = 0;
            Array.Clear(_inputQueue, 0, _inputQueue.Length);
            Array.Clear(_outputQueue, 0, _outputQueue.Length);

            // Pre-preenche a fila de saída com silêncio para criar a latência inicial.
            for (int i = 0; i < latency; i++)
            {
                _outputQueue[_outputWrite] = 0f;
                _outputWrite = (_outputWrite + 1) % _outputQueue.Length;
            }
            _outputCount = latency;
        }

        public void Reset()
        {
            int lat = 2048;
            try
            {
                if (VoIPPlugin.RNNoiseLatency != null)
                    lat = VoIPPlugin.RNNoiseLatency.Value;
            }
            catch {}
            Reset(lat);
        }

        public void Dispose()
        {
            if (_rnAvailable && _rnState != IntPtr.Zero)
            {
                rnnoise_destroy(_rnState);
                _rnState    = IntPtr.Zero;
                _rnAvailable = false;
            }
        }
    }
}
