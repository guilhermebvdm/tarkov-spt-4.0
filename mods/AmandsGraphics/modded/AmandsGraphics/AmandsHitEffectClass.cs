using UnityEngine;

namespace AmandsGraphics;

public sealed class AmandsHitEffectClass : MonoBehaviour
{
    public void Start()
    {
    }
    public void Update()
    {
        if (AmandsGraphicsClass.ChromaticAberrationAnimation > 0)
        {
            AmandsGraphicsClass.ChromaticAberrationAnimation -= Time.deltaTime / AmandsGraphicsPlugin.HitCASpeed.Value;
            if (AmandsGraphicsClass.FPSCameraChromaticAberration != null)
            {
                AmandsGraphicsClass.FPSCameraChromaticAberration.intensity.value = Mathf.Lerp(0f, AmandsGraphicsClass.ChromaticAberrationIntensity, AmandsGraphicsClass.ChromaticAberrationAnimation);
                AmandsGraphicsClass.FPSCameraChromaticAberration.enabled.value = AmandsGraphicsClass.ChromaticAberrationAnimation > 0.0f;
            }
        }
    }
}

