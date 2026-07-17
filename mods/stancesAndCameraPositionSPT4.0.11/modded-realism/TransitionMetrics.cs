using UnityEngine;

namespace CameraRotationMod
{
    /// <summary>
    /// Item 016 (F0) — instrumentação `Debug Transition Metrics`: mede cada transição de pose (stance↔stance,
    /// stance↔ADS) e loga UMA linha ao fim com pico de excursão além do alvo, cruzamentos de sinal do erro e
    /// tempo de assentamento. É a régua do baseline da mola legacy e dos critérios de aceite do fork
    /// (bug (b): "a arma sobe ~5 cm além da mira"). Ferramenta permanente — não é log temporário.
    ///
    /// Uma instância POR CORPO (local + cada observado Fika); a origem vai no rótulo de cada linha, exigência
    /// do gate de paridade da F4 (parear duração local × observado por amostra).
    ///
    /// Decisões do review técnico 01 (ver 016-...-02-spec-tech-F0.md):
    /// - Feed recebe stance/isAiming CRUS; strings só nascem dentro, após o gate da flag (custo zero com off).
    /// - Rotação logada com nome POR EXTENSO (pitch/roll/yaw ← .x/.y/.z LOCAIS da arma — não é a ordem Unity;
    ///   abreviar rótulo já reintroduziu o bug MP-01-02 uma vez).
    /// - `cross` = SOMA dos cruzamentos dos 6 canais.
    /// - Assentamento por TEMPO (0.15 s), não frames — FPS-independente.
    /// - Canal que já parte no alvo (|start−target|&lt;ε): excluído de pico/cross (sem direção de referência).
    /// - Debounce: nova transição só abre com alvo ESTÁVEL por 3 frames (drag de slider no F12 muda o alvo a
    ///   cada tick e viraria spam).
    /// - ×100 (m→cm) só na formatação.
    /// </summary>
    public class TransitionMetrics
    {
        private const float TargetEpsPos = 0.0005f;   // m — "o alvo mudou?"
        private const float TargetEpsRot = 0.05f;     // graus
        private const float SettleEpsPos = 0.001f;    // m — "assentou?"
        private const float SettleEpsRot = 0.25f;     // graus
        private const float SettleHoldSeconds = 0.15f;
        private const float TimeoutSeconds = 5f;
        private const int   StableFramesToOpen = 3;

        private struct Channel
        {
            public float Target;
            public bool  Excluded;      // partiu já no alvo — sem direção de referência
            public bool  Reached;       // alcançou/cruzou o alvo pela 1ª vez
            public int   InitialSign;   // sinal do erro na partida
            public int   LastSign;      // último sinal do erro (para contar cruzamentos)
            public float PeakBeyond;    // máx |erro| do lado OPOSTO à partida, após o 1º alcance
            public int   Crossings;
        }

        private readonly string _origin;
        private readonly Channel[] _ch = new Channel[6]; // 0..2 pos XYZ · 3..5 rot pitch(x)/roll(y)/yaw(z)

        private bool _measuring;
        private Vector3 _activeTargetPos, _activeTargetEuler;
        private Vector3 _pendingTargetPos, _pendingTargetEuler;
        private int _pendingStableFrames;
        private float _elapsed, _settledTime;
        private int _frames;
        private string _route = "";
        private string _lastToken = "S0";

        public TransitionMetrics(string origin) { _origin = origin; }

        // CR-2 do code-review 01: primeiro Feed após ligar a flag (ou reset) só ADOTA o alvo/token vigentes,
        // sem abrir medição — senão ligar a flag já em stance loga uma amostra falsa "S0->S2 settle 0.00s".
        private bool _primed;

        /// <summary>Chamar todo frame do corpo instrumentado, APÓS a pose do frame ser calculada.</summary>
        public void Feed(Vector3 tgtPos, Vector3 tgtEuler, Vector3 curPos, Vector3 curEuler, float dt,
                         Stance stance, bool isAiming, bool inStance)
        {
            if (!(Plugin._DebugTransitionMetrics?.Value ?? false)) { _primed = false; return; }

            if (!_primed)
            {
                _primed = true;
                _activeTargetPos = tgtPos; _activeTargetEuler = tgtEuler;
                _lastToken = Token(stance, isAiming, inStance);
                return;
            }

            bool targetChanged = Differs(tgtPos, _activeTargetPos, TargetEpsPos)
                              || DiffersAngular(tgtEuler, _activeTargetEuler, TargetEpsRot);

            if (targetChanged)
            {
                // Debounce (review 🟡6): só abre com o alvo estável há N frames — drag de slider no F12
                // muda o alvo a cada tick e viraria uma "transição" por frame.
                bool samePending = !Differs(tgtPos, _pendingTargetPos, TargetEpsPos)
                                && !DiffersAngular(tgtEuler, _pendingTargetEuler, TargetEpsRot);
                if (samePending) _pendingStableFrames++;
                else { _pendingTargetPos = tgtPos; _pendingTargetEuler = tgtEuler; _pendingStableFrames = 1; }

                if (_pendingStableFrames < StableFramesToOpen) return;

                if (_measuring) Finish("(interrupted)");
                Open(tgtPos, tgtEuler, curPos, curEuler, stance, isAiming, inStance, dt);
                return;
            }
            _pendingStableFrames = 0;

            if (!_measuring) return;

            _elapsed += dt; _frames++;
            if (_elapsed >= TimeoutSeconds) { Finish("(timeout)"); return; }

            bool allSettled = true;
            for (int i = 0; i < 6; i++)
            {
                bool rot = i >= 3;
                float cur = rot ? Axis(curEuler, i - 3) : Axis(curPos, i);
                float err = rot ? Mathf.DeltaAngle(_ch[i].Target, cur) : cur - _ch[i].Target;
                float settleEps = rot ? SettleEpsRot : SettleEpsPos;

                if (Mathf.Abs(err) >= settleEps) allSettled = false;
                if (_ch[i].Excluded)
                {
                    // CR-3: partiu no alvo, mas a velocidade residual da mola o tirou de lá (transição
                    // interrompida) — promove a canal medido, senão pico/cross ficam invisíveis.
                    if (Mathf.Abs(err) >= 2f * settleEps)
                    {
                        _ch[i].Excluded = false;
                        _ch[i].InitialSign = err > 0f ? 1 : -1;
                        _ch[i].LastSign = _ch[i].InitialSign;
                        _ch[i].Reached = false;
                    }
                    else continue;
                }

                int sign = err > 0f ? 1 : (err < 0f ? -1 : 0);
                // CR-4: regra ÚNICA de cruzamento (independe de "alcançou por deadband ou por flip") —
                // sem ela, a mesma trajetória física logava cross 1 ou 2 conforme o sorteio de frame.
                if (sign != 0 && sign != _ch[i].LastSign && Mathf.Abs(err) >= settleEps)
                { _ch[i].Crossings++; _ch[i].LastSign = sign; }

                if (!_ch[i].Reached && (sign != _ch[i].InitialSign || Mathf.Abs(err) < settleEps))
                    _ch[i].Reached = true;

                // pico "além do alvo" = excursão do lado oposto ao da partida
                if (_ch[i].Reached && sign == -_ch[i].InitialSign)
                    _ch[i].PeakBeyond = Mathf.Max(_ch[i].PeakBeyond, Mathf.Abs(err));
            }

            if (allSettled)
            {
                _settledTime += dt;
                if (_settledTime >= SettleHoldSeconds) Finish("");
            }
            else _settledTime = 0f;
        }

        private void Open(Vector3 tgtPos, Vector3 tgtEuler, Vector3 curPos, Vector3 curEuler,
                          Stance stance, bool isAiming, bool inStance, float dt)
        {
            _activeTargetPos = tgtPos; _activeTargetEuler = tgtEuler;
            // CR-5: o debounce consumiu (N-1) frames em que a mola já andava — repõe no relógio.
            _measuring = true; _elapsed = (StableFramesToOpen - 1) * dt; _settledTime = 0f; _frames = 0;

            for (int i = 0; i < 6; i++)
            {
                bool rot = i >= 3;
                float cur = rot ? Axis(curEuler, i - 3) : Axis(curPos, i);
                float tgt = rot ? Axis(tgtEuler, i - 3) : Axis(tgtPos, i);
                float err = rot ? Mathf.DeltaAngle(tgt, cur) : cur - tgt;
                float eps = rot ? SettleEpsRot : SettleEpsPos;

                int sign0 = err > 0f ? 1 : -1;
                _ch[i] = new Channel
                {
                    Target = tgt,
                    // review 🟡5: canal que já parte no alvo não tem direção de referência — fora de pico/cross
                    Excluded = Mathf.Abs(err) < eps,
                    InitialSign = sign0,
                    LastSign = sign0, // CR-4: cruzamentos contados desde a PARTIDA, com regra única
                };
            }

            string to = Token(stance, isAiming, inStance);
            _route = _lastToken + "->" + to;
            _lastToken = to;
        }

        // review 🟢9 + CR-1: 1 token por lado. Fora de stance o alvo real é a pose default ("S0"), mesmo
        // mirando — e "ADS" só quando a mira realmente troca o alvo (_ResetOnADS).
        private static string Token(Stance stance, bool isAiming, bool inStance)
        {
            if (!inStance) return "S0";
            if (isAiming && (Plugin._ResetOnADS?.Value ?? false)) return "ADS";
            return "S" + (int)stance;
        }

        private void Finish(string suffix)
        {
            _measuring = false;
            if (_frames == 0) return;

            int cross = 0;
            for (int i = 0; i < 6; i++) if (!_ch[i].Excluded) cross += _ch[i].Crossings;
            float avgDtMs = (_elapsed / _frames) * 1000f;

            // ×100 = m→cm SÓ aqui (review 🟡7). Rotação por extenso, mapeada aos eixos LOCAIS da arma
            // (pitch=.x, roll=.y, yaw=.z) — NÃO abreviar (review 🔴2 / bug histórico MP-01-02).
            Plugin.Logger.LogInfo(string.Format(
                "[METRICS] {0} | {1} | posPeak cm lateral {2:F2} longitudinal {3:F2} vertical {4:F2} | " +
                "rotPeak deg pitch {5:F2} roll {6:F2} yaw {7:F2} | cross {8} | settle {9:F2}s | avgDt {10:F1}ms {11}",
                _origin, _route,
                _ch[0].PeakBeyond * 100f, _ch[1].PeakBeyond * 100f, _ch[2].PeakBeyond * 100f,
                _ch[3].PeakBeyond, _ch[4].PeakBeyond, _ch[5].PeakBeyond,
                cross, _elapsed - (suffix.Length == 0 ? SettleHoldSeconds : 0f), avgDtMs, suffix));
        }

        /// <summary>Troca de raid: esquece medição em andamento e o token de rota anterior.</summary>
        public void Reset()
        {
            _measuring = false; _pendingStableFrames = 0; _primed = false;
            _activeTargetPos = _activeTargetEuler = Vector3.zero;
            _lastToken = "S0";
        }

        private static float Axis(Vector3 v, int i) => i == 0 ? v.x : (i == 1 ? v.y : v.z);

        private static bool Differs(Vector3 a, Vector3 b, float eps)
            => Mathf.Abs(a.x - b.x) > eps || Mathf.Abs(a.y - b.y) > eps || Mathf.Abs(a.z - b.z) > eps;

        private static bool DiffersAngular(Vector3 a, Vector3 b, float eps)
            => Mathf.Abs(Mathf.DeltaAngle(a.x, b.x)) > eps
            || Mathf.Abs(Mathf.DeltaAngle(a.y, b.y)) > eps
            || Mathf.Abs(Mathf.DeltaAngle(a.z, b.z)) > eps;
    }
}
