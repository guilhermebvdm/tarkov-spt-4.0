using System.Text;
using Comfort.Common;
using EFT;
using UnityEngine;

namespace CameraRotationMod
{
    /// <summary>
    /// Item 018 (spy, 2026-09-09) — instrumento de diagnóstico pro bug "prone-run não acelera de
    /// verdade" que sobreviveu ao fix da 2.20.2 (chamada de PlayerAnimatorEnableSprint). Mostra, ao
    /// vivo, todos os valores que a teoria (baseada em Assembly, não em teste real) diz que deveriam
    /// mudar quando o boost liga — se algum não mudar, é ali que a teoria está errada.
    ///
    /// Unidades: MaxSpeed/CharacterMovementSpeed/ClampedSpeed/SmoothedCharacterMovementSpeed/
    /// StateSpeedLimit são todos FATOR normalizado (1 = base), não m/s — mesmo padrão documentado em
    /// SpeedLimitDebugUI (P-11.1).
    /// </summary>
    public class ProneRunDebugUI : MonoBehaviour
    {
        private GUIStyle _style;
        private readonly StringBuilder _sb = new StringBuilder(512);
        private string _cached = string.Empty;
        private float _nextRefresh;

        private const float RefreshInterval = 0.1f;

        private void OnGUI()
        {
            if (Plugin._DebugProneRun == null || !Plugin._DebugProneRun.Value) return;

            Player player = Singleton<GameWorld>.Instance?.MainPlayer;
            MovementContext mc = player?.MovementContext;
            if (player == null || mc == null) return;

            if (Time.unscaledTime >= _nextRefresh)
            {
                _nextRefresh = Time.unscaledTime + RefreshInterval;
                Rebuild(player, mc);
            }

            if (_cached.Length == 0) return;

            if (_style == null)
                _style = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold, normal = { textColor = Color.cyan } };

            GUI.Label(new Rect(20f, 130f, 900f, 160f), _cached, _style);
        }

        private void Rebuild(Player player, MovementContext mc)
        {
            bool featureOn = Plugin._EnableProneRun?.Value ?? false;
            bool held = Patches.ProneRunSprintIntent.Held;
            bool moving = mc.MovementDirection.sqrMagnitude > 0.0001f;
            bool wantsBoost = featureOn && mc.IsInPronePose && held && moving;

            string stateName = player.CurrentManagedState?.GetType().Name ?? "(null)";
            string bool5 = (player.CurrentManagedState is ProneMoveStateClass pm) ? pm.Bool_5.ToString() : "n/a (não é ProneMoveStateClass)";

            float animatorSpeedParam = mc.PlayerAnimator?.GetCharacterMovementSpeed() ?? -1f;

            _sb.Length = 0;
            _sb.Append("PRONE-RUN SPY | featureOn=").Append(featureOn)
               .Append(" held=").Append(held)
               .Append(" moving=").Append(moving)
               .Append(" wantsBoost=").Append(wantsBoost)
               .Append('\n');
            _sb.Append("CurrentManagedState=").Append(stateName)
               .Append(" | Bool_5(nativo)=").Append(bool5)
               .Append('\n');
            _sb.Append("IsInPronePose=").Append(mc.IsInPronePose)
               .Append(" PoseLevel=").Append(mc.PoseLevel.ToString("F3"))
               .Append(" MovementDirection=").Append(mc.MovementDirection.ToString("F2"))
               .Append('\n');
            _sb.Append("MaxSpeed(pós-boost)=").Append(mc.MaxSpeed.ToString("F3"))
               .Append(" StateSpeedLimit=").Append(mc.StateSpeedLimit.ToString("F3"))
               .Append('\n');
            _sb.Append("CharacterMovementSpeed=").Append(mc.CharacterMovementSpeed.ToString("F3"))
               .Append(" ClampedSpeed=").Append(mc.ClampedSpeed.ToString("F3"))
               .Append(" SmoothedCharacterMovementSpeed=").Append(mc.SmoothedCharacterMovementSpeed.ToString("F3"))
               .Append('\n');
            _sb.Append("Animator \"Speed\" param (blend, NÃO controla playback)=").Append(animatorSpeedParam.ToString("F3"))
               .Append('\n');
            _sb.Append("Animator.speed (multiplicador GLOBAL de playback — o novo lever)=").Append((mc.PlayerAnimator?.Animator?.speed ?? -1f).ToString("F3"))
               .Append('\n');
            _sb.Append("IsSprintEnabled(Physical.Sprinting)=").Append(mc.IsSprintEnabled);

            _cached = _sb.ToString();
        }
    }
}
