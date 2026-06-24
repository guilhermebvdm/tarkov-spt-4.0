using Comfort.Common;
using EFT;
using UnityEngine;

namespace CameraRotationMod
{
    /// <summary>
    /// Overlay de debug do cenário de stamina ativo (item 012). Adicionado via AddComponent no GameObject
    /// persistente do plugin (como OxygenUI/PassiveMountUI) — nunca via new GameObject no boot.
    /// </summary>
    public class StaminaDebugUI : MonoBehaviour
    {
        private GUIStyle _style;

        private void OnGUI()
        {
            if (Plugin._DebugStaminaState == null || !Plugin._DebugStaminaState.Value) return;

            var hands = Singleton<GameWorld>.Instance?.MainPlayer?.Physical?.HandsStamina;
            if (hands == null) return;

            if (_style == null)
                _style = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold, normal = { textColor = Color.yellow } };

            GUI.Label(new Rect(20f, 20f, 680f, 28f),
                $"STAMINA STATE: {StaminaController.CurrentLabel}  ({hands.Current:F0}/{(float)hands.TotalCapacity:F0})",
                _style);
        }
    }
}
