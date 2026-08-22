using CameraRotationMod.Patches;
using UnityEngine;

namespace CameraRotationMod
{
    /// <summary>
    /// Overlay de debug da compressão de ADS-speed (item 017 F3). Mostra o par nativo→comprimido da arma em
    /// mãos, para responder in-game "a compressão está aplicando?" e permitir calibrar o pivô olhando número,
    /// não sensação. Adicionado via AddComponent no GameObject persistente do plugin (padrão do StaminaDebugUI).
    ///
    /// A velocidade de mira do EFT é o INVERSO do tempo de mira: ~1.9 = pistola (rápida), ~1.0 = fuzil,
    /// ~0.6 = LMG (lenta). O pivô é o valor que a compressão deixa intacto — calibrar para o CENTRO da faixa
    /// das armas que se usa, senão a compressão puxa tudo para o mesmo lado em vez de aproximar os extremos.
    /// </summary>
    public class AdsSpeedDebugUI : MonoBehaviour
    {
        private GUIStyle _style;

        private void OnGUI()
        {
            if (Plugin._DebugAdsSpeed == null || !Plugin._DebugAdsSpeed.Value) return;

            float native = AdsSpeedCompressionPatch.LastNative;
            if (native <= 0f) return; // nenhuma arma medida ainda nesta raid

            if (_style == null)
                _style = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold, normal = { textColor = Color.cyan } };

            float compressed = AdsSpeedCompressionPatch.LastCompressed;
            int pct = Plugin._AdsSpeedCompression?.Value ?? 0;
            float pivot = Plugin._AdsSpeedPivot?.Value ?? 1.5f;
            // Tempo de mira aproximado (1/velocidade) — mais legível que a velocidade crua ao calibrar.
            string times = (native > 0f && compressed > 0f)
                ? $"  |  {1f / native:F2}s -> {1f / compressed:F2}s"
                : string.Empty;

            GUI.Label(new Rect(20f, 48f, 680f, 28f),
                $"ADS SPEED: {native:F2} -> {compressed:F2}{times}  (comp {pct}%, pivot {pivot:F2})",
                _style);
        }
    }
}
