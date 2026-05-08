using UnityEngine;

public class CullingLightObjectInverse : CullingLightObject
{
	public override void UpdateCullingObject(float multiplier, float shadowMultiplier)
	{
		CullDistance = GetFadEndDistance();
		float num = ((CullingManager.Instance.IsOpticEnabled() & Mathf.Approximately(multiplier, 0f)) ? 1f : multiplier);
		num *= base.IntensityMultiplier;
		if (GetFlicker() != null && GetFlicker().enabled)
		{
			GetFlicker().CullingCoef = 1f - num;
		}
		else
		{
			GetLight().intensity = getMaxIntensityFinal() * (1f - num);
		}
	}
}
