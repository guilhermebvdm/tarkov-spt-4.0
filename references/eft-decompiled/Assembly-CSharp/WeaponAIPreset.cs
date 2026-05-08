using System;

public class WeaponAIPreset
{
	public float BaseShift;

	public float XZ_COEF;

	public Func<float> TriggerDownTime;

	public Func<float> HoldTriggerUpTime;

	public EWeaponAIPresetType WeaponAIPresetType;

	public WeaponAIPreset(float baseShift, float xzCoef, Func<float> triggerDownTime, Func<float> holdTriggerUpTime, EWeaponAIPresetType weaponAIPresetType)
	{
		BaseShift = baseShift;
		XZ_COEF = xzCoef;
		TriggerDownTime = triggerDownTime;
		HoldTriggerUpTime = holdTriggerUpTime;
		WeaponAIPresetType = weaponAIPresetType;
	}
}
