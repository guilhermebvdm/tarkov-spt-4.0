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

    }
}
