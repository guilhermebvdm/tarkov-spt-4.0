using System;
using System.Globalization;
using System.IO;
using Comfort.Common;
using EFT;
using TRLCoreSight.Configuration;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TRLCoreSight.Core
{
    /// <summary>
    /// Registra telemetria de frametimes e calcula métricas de estabilidade (média e 1% low)
    /// ao término de cada raid para validação empírica do impacto das estratégias de afinidade de CPU.
    /// </summary>
    public class FrametimeBenchmark : MonoBehaviour
    {
        public static FrametimeBenchmark Instance { get; private set; }

        // ref: CR-01-01
        private const int MAX_SAMPLES = 60000;
        private readonly float[] _samples = new float[MAX_SAMPLES];
        private int _sampleCount = 0;
        private bool _isInitialized = false;
        private bool _truncationWarned = false;

        private void Awake()
        {
            Instance = this;
        }

        public void Initialize()
        {
            _sampleCount = 0;
            _isInitialized = true;
            _truncationWarned = false;
            if (ModConfig.EnableFrametimeBenchmark != null && ModConfig.EnableFrametimeBenchmark.Value)
            {
                Plugin.LogSource?.LogInfo("[TRL-CoreSight][Benchmark] Gravador de frametimes ativado para esta raid.");
            }
        }

        public void OnUpdate()
        {
            if (!_isInitialized) return;
            if (ModConfig.EnableFrametimeBenchmark == null || !ModConfig.EnableFrametimeBenchmark.Value) return;

            float dt = Time.unscaledDeltaTime;
            // Descarta deltas espúrios (travamentos de carregamento ou freeze de janela)
            if (dt <= 0f || dt > 1.0f) return;

            if (_sampleCount < MAX_SAMPLES)
            {
                _samples[_sampleCount++] = dt;
            }
            else if (!_truncationWarned)
            {
                _truncationWarned = true;
                Plugin.LogSource?.LogWarning($"[TRL-CoreSight][Benchmark] Buffer de frametimes atingiu o limite de {MAX_SAMPLES} amostras (~{MAX_SAMPLES / 3600.0:F0} min a 60fps). O restante da raid não será incluído no relatório — CSV marcado como Truncated=true.");
            }
        }

        public void Cleanup()
        {
            if (_isInitialized && ModConfig.EnableFrametimeBenchmark != null && ModConfig.EnableFrametimeBenchmark.Value)
            {
                SaveBenchmarkReport();
            }

            _sampleCount = 0;
            _isInitialized = false;
            Instance = null;
        }

        private void SaveBenchmarkReport()
        {
            if (_sampleCount < 100)
            {
                Plugin.LogSource?.LogInfo($"[TRL-CoreSight][Benchmark] Amostras insuficientes ({_sampleCount} frames) para gerar relatório de benchmark.");
                return;
            }

            try
            {
                // 1. Cálculo da média de frametime e FPS
                double sumDt = 0;
                for (int i = 0; i < _sampleCount; i++)
                {
                    sumDt += _samples[i];
                }
                double avgDt = sumDt / _sampleCount;
                double avgFrametimeMs = avgDt * 1000.0;
                double avgFps = avgDt > 0.00001 ? 1.0 / avgDt : 0.0;

                // 2. Cálculo dos 1% lows (piores 1% de frames com maior delta)
                float[] sorted = new float[_sampleCount];
                Array.Copy(_samples, sorted, _sampleCount);
                Array.Sort(sorted);

                int onePercentCount = Math.Max(1, _sampleCount / 100);
                int startIndex = _sampleCount - onePercentCount;
                double worstSumDt = 0;
                for (int i = startIndex; i < _sampleCount; i++)
                {
                    worstSumDt += sorted[i];
                }
                double worstAvgDt = worstSumDt / onePercentCount;
                double onePercentLowMs = worstAvgDt * 1000.0;
                double onePercentLowFps = worstAvgDt > 0.00001 ? 1.0 / worstAvgDt : 0.0;

                bool truncated = _sampleCount >= MAX_SAMPLES;
                string mode = ModConfig.CpuAffinityMode != null ? ModConfig.CpuAffinityMode.Value.ToString() : "Unknown";
                string mapName = "Unknown";
                try
                {
                    mapName = SceneManager.GetActiveScene().name;
                }
                catch { }

                // 3. Persistência em arquivo CSV
                string baseDir = Path.Combine(BepInEx.Paths.PluginPath, "TRL-CoreSight");
                if (!Directory.Exists(baseDir))
                {
                    Directory.CreateDirectory(baseDir);
                }

                string csvPath = Path.Combine(baseDir, "benchmark_frametimes.csv");
                bool fileExists = File.Exists(csvPath);

                using (StreamWriter sw = new StreamWriter(csvPath, true, System.Text.Encoding.UTF8))
                {
                    if (!fileExists)
                    {
                        sw.WriteLine("Timestamp,Map,CpuAffinityMode,TotalFrames,AvgFrametimeMs,AvgFPS,OnePercentLowMs,OnePercentLowFPS,Truncated");
                    }

                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string line = string.Format(
                        CultureInfo.InvariantCulture,
                        "{0},{1},{2},{3},{4:F2},{5:F1},{6:F2},{7:F1},{8}",
                        timestamp,
                        mapName,
                        mode,
                        _sampleCount,
                        avgFrametimeMs,
                        avgFps,
                        onePercentLowMs,
                        onePercentLowFps,
                        truncated
                    );
                    sw.WriteLine(line);
                }

                Plugin.LogSource?.LogInfo($"[TRL-CoreSight][Benchmark] Benchmark gravado com sucesso em '{csvPath}': Modo={mode}, Frames={_sampleCount}{(truncated ? " (TRUNCADO)" : "")}, Avg={avgFrametimeMs:F2}ms ({avgFps:F1} FPS), 1% Low={onePercentLowMs:F2}ms ({onePercentLowFps:F1} FPS).");
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight][Benchmark] Falha ao calcular e salvar benchmark de frametimes: {ex.Message}");
            }
        }

        private void OnDestroy()
        {
            Cleanup();
        }
    }
}
