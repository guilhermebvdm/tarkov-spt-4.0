using System.Reflection;
using EFT.Animations;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace CameraRotationMod.Patches
{
    /// <summary>
    /// Item 017 (F3) — compressão da velocidade de ADS. O EFT deriva `_aimingSpeed` do peso + ergonomia da arma
    /// (armas leves miram muito rápido, pesadas devagar). Isto puxa os extremos em direção a um pivô, em
    /// log-space (natural para velocidade, que é multiplicativa):
    ///
    ///     aimSpeed' = pivot * (native / pivot) ^ k,   k = 1 - compressão
    ///
    /// k=1 (compressão 0%) não muda nada; k=0 (100%) põe TODAS as armas na velocidade do pivô; 0.5 puxa cada uma
    /// "metade do caminho" (em log). Aplicado no Postfix de UpdateWeaponVariables (onde o EFT acabou de calcular
    /// o `_aimingSpeed` nativo) — persiste por arma até o próximo evento de arma. O gate da F1 salva/restaura o
    /// valor JÁ comprimido; ver ApplyComplexRotationPatch.OnBaseAimSpeedChanged para a coordenação.
    /// </summary>
    public class AdsSpeedCompressionPatch : ModulePatch
    {
        private static FieldInfo _aimingSpeedField;
        private static FieldInfo _firearmControllerField;

        // Para reaplicar ao mexer no slider (calibração reflete na hora, sem re-sacar a arma).
        private static ProceduralWeaponAnimation _lastPwa;
        private static float _lastNative;

        protected override MethodBase GetTargetMethod()
        {
            _aimingSpeedField = AccessTools.Field(typeof(ProceduralWeaponAnimation), "_aimingSpeed");
            _firearmControllerField = AccessTools.Field(typeof(ProceduralWeaponAnimation), "_firearmController");
            return AccessTools.Method(typeof(ProceduralWeaponAnimation), nameof(ProceduralWeaponAnimation.UpdateWeaponVariables));
        }

        [PatchPostfix]
        private static void Postfix(ProceduralWeaponAnimation __instance)
        {
            if (_aimingSpeedField == null) return;

            // code-review F3 #1: UpdateWeaponVariables também roda SEM arma em mãos (ex.: trocar colete/mochila
            // com faca/granada) — nesse caso o EFT NÃO recalcula o _aimingSpeed nativo, então ler o campo aqui
            // pegaria o valor JÁ comprimido e comprimiria de novo. Só agir com arma (mesmo guard do EFT).
            if (_firearmControllerField != null && _firearmControllerField.GetValue(__instance) == null) return;

            // O EFT acabou de escrever o _aimingSpeed nativo (peso+ergo). Guarda o nativo e comprime.
            float native = (float)_aimingSpeedField.GetValue(__instance);
            _lastPwa = __instance;
            _lastNative = native;
            Apply(__instance, native);
        }

        /// <summary>Reset de raid (StanceManager.ResetState) — esquece o último PWA, igual aos resets irmãos.</summary>
        public static void Reset()
        {
            _lastPwa = null;
            _lastNative = 0f;
        }

        /// <summary>Reaplica com a config atual (chamado pelos SettingChanged dos sliders — calibração ao vivo).</summary>
        public static void Reapply()
        {
            if (_lastPwa != null && _aimingSpeedField != null)
                Apply(_lastPwa, _lastNative);
        }

        private static void Apply(ProceduralWeaponAnimation pwa, float native)
        {
            float compressed = Compress(native);
            _aimingSpeedField.SetValue(pwa, compressed);
            // Coordena com o gate da F1: se estiver segurando este PWA, o valor a restaurar passa a ser o novo.
            ApplyComplexRotationPatch.OnBaseAimSpeedChanged(pwa, compressed);
        }

        /// <summary>native → comprimido. Sem efeito quando compressão = 0% ou entradas inválidas.</summary>
        public static float Compress(float native)
        {
            int pct = Plugin._AdsSpeedCompression?.Value ?? 0;
            if (pct <= 0) return native;
            float pivot = Plugin._AdsSpeedPivot?.Value ?? 1.5f;
            if (native <= 0f || pivot <= 0f) return native; // Pow de base ≤ 0 é indefinido — não mexe
            float k = 1f - (pct / 100f);
            return pivot * Mathf.Pow(native / pivot, k);
        }
    }
}
