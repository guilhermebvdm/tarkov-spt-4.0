using System.Collections.Generic;
using System.Text;
using Comfort.Common;
using EFT;
using UnityEngine;

namespace CameraRotationMod
{
    /// <summary>
    /// [P-11.1] Instrumento de diagnóstico do teto de velocidade. Mostra QUAL causa está segurando o jogador —
    /// o EFT mantém um dicionário `MovementContext.SpeedLimits` (causa → fator 0..1) e o efetivo é o MENOR de
    /// todos (`MovementContext.method_4`, decompilado). O sintoma "anda devagar sem motivo e mirar destrava"
    /// não diz qual causa está presa; este overlay diz.
    ///
    /// ⚠️ Nota de mecânica, contra a hipótese antiga: `MovementContext.MaxSpeed` NÃO depende da pose — é
    /// `Evaluate(WalkSpeed, Strength/60)` (`MovementContext.cs:910`), função só do backend e da skill. Agachar
    /// ou levantar não muda esse valor, e `ProcessSpeedLimits` roda todo frame dentro de `ManualUpdate`
    /// (`MovementContext.cs:2499`), então o limite do mod não fica "stale". A causa do bug está em OUTRA
    /// entrada do dicionário — achá-la é o objetivo desta tela.
    ///
    /// Unidades: tudo aqui é FATOR normalizado (1 = sem limite), não m/s. Com a config do servidor, o limite do
    /// mod cai por volta de 0,5 — esse é o teto intencional da Stance 0, não o bug (ver P-8.3).
    /// </summary>
    public class SpeedLimitDebugUI : MonoBehaviour
    {
        private GUIStyle _style;
        private readonly StringBuilder _sb = new StringBuilder(256);
        private string _cached = string.Empty;
        private float _nextRefresh;

        // Log de transição: só escreve quando a causa vencedora muda, para o usuário não precisar estar
        // olhando a tela no instante do travamento.
        private int _lastWinnerCause = int.MinValue;
        private float _lastWinnerValue = -1f;

        private const float RefreshInterval = 0.25f;

        private void OnGUI()
        {
            if (Plugin._DebugSpeedLimits == null || !Plugin._DebugSpeedLimits.Value) return;

            var mc = Singleton<GameWorld>.Instance?.MainPlayer?.MovementContext;
            if (mc == null) return;

            if (Time.unscaledTime >= _nextRefresh)
            {
                _nextRefresh = Time.unscaledTime + RefreshInterval;
                Rebuild(mc);
            }

            if (_cached.Length == 0) return;

            if (_style == null)
                _style = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold, normal = { textColor = Color.green } };

            GUI.Label(new Rect(20f, 76f, 900f, 52f), _cached, _style);
        }

        private void Rebuild(MovementContext mc)
        {
            _sb.Length = 0;
            _sb.Append("SPEED LIMIT: state ").Append(mc.StateSpeedLimit.ToString("F3"))
               .Append("  | max ").Append(mc.MaxSpeed.ToString("F3"))
               .Append("  | clamped ").Append(mc.ClampedSpeed.ToString("F3"))
               .Append("  | sprintLimit ").Append(mc.StateSprintSpeedLimit.ToString("F3"))
               .Append('\n');

            int winnerCause = int.MinValue;
            float winnerValue = float.MaxValue;

            var limits = mc.SpeedLimits;
            if (limits == null || limits.Count == 0)
            {
                _sb.Append("  (nenhuma causa ativa — nada segurando)");
            }
            else
            {
                foreach (KeyValuePair<Player.ESpeedLimit, float> kv in limits)
                {
                    if (kv.Value < winnerValue)
                    {
                        winnerValue = kv.Value;
                        winnerCause = (int)kv.Key;
                    }
                }

                bool first = true;
                foreach (KeyValuePair<Player.ESpeedLimit, float> kv in limits)
                {
                    if (!first) _sb.Append("  ·  ");
                    first = false;
                    if ((int)kv.Key == winnerCause) _sb.Append('>');
                    _sb.Append(CauseName(kv.Key)).Append(' ').Append(kv.Value.ToString("F3"));
                }
            }

            _cached = _sb.ToString();

            if (winnerCause != _lastWinnerCause || Mathf.Abs(winnerValue - _lastWinnerValue) > 0.001f)
            {
                _lastWinnerCause = winnerCause;
                _lastWinnerValue = winnerValue;
                Plugin.Logger.LogInfo(
                    $"[SpeedLimit] vencedora agora: {(winnerCause == int.MinValue ? "nenhuma" : CauseName((Player.ESpeedLimit)winnerCause))} " +
                    $"= {(winnerValue == float.MaxValue ? 1f : winnerValue):F3} | state={mc.StateSpeedLimit:F3} | {_cached.Replace('\n', ' ')}");
            }
        }

        /// <summary>O cause do mod (9001) está fora do enum do EFT — sem isto sairia só "9001" na tela.</summary>
        private static string CauseName(Player.ESpeedLimit cause)
            => (int)cause == Plugin.StanceSpeedLimitCauseId ? "Stance(mod)" : cause.ToString();
    }
}
