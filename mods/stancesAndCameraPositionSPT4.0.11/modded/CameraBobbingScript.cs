using UnityEngine;
using EFT;
using CameraRotationMod.Patches;

namespace CameraRotationMod
{
    [DefaultExecutionOrder(9999)]
    public class CameraBobbingScript : MonoBehaviour
    {
        private void Update()
        {
            if (EFTHardSettings.Instance != null && Plugin._LeanSpeedMultiplier != null)
            {
                // 10f é a velocidade padrão do Tarkov
                EFTHardSettings.Instance.TILT_CHANGING_SPEED = 10f * Plugin._LeanSpeedMultiplier.Value;
            }
        }

        private void LateUpdate()
        {
            if (Camera.main == null) return;

            float bobbingMult = Plugin._CameraBobbingMultiplier?.Value ?? 1f;
            if (bobbingMult <= 0.01f) return;

            Vector3 cameraWiggle = SpringGetPatch.CurrentCurveRotation * bobbingMult;
            if (cameraWiggle == Vector3.zero) return;

            Camera.main.transform.localRotation *= Quaternion.Euler(cameraWiggle);
        }
    }
}
