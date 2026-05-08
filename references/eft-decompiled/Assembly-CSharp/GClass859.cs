using System;
using Comfort.Common;
using UnityEngine;

public abstract class GClass859
{
	[NonSerialized]
	public static float Float_0;

	public static float Single_0 => Singleton<BackendConfigSettingsClass>.Instance.Airdrop.ParachuteStartOpenHeight;

	public static float Single_1 => Singleton<BackendConfigSettingsClass>.Instance.Airdrop.ParachuteEndOpenHeight;

	public static float Single_2 => Singleton<BackendConfigSettingsClass>.Instance.Airdrop.ParachuteEndOpenHeight / Singleton<BackendConfigSettingsClass>.Instance.Airdrop.PlaneAirdropDuration;

	public static float Single_3 => Singleton<BackendConfigSettingsClass>.Instance.Airdrop.PlaneAirdropDuration;

	public static Vector3 GetPlaneOffsetDropPosition(Vector3 wind, float dropHeight)
	{
		return smethod_0(wind, dropHeight) + smethod_1(wind) + smethod_2(wind);
	}

	public static Vector3 smethod_0(Vector3 wind, float dropHeight)
	{
		float num = dropHeight - Single_0;
		float num2 = Mathf.Sqrt(2f * num / Mathf.Abs(Physics.gravity.y));
		Vector3 result = smethod_3(wind) * num2 * Mathf.InverseLerp(dropHeight, Single_0, EFTHardSettings.Instance.WindTurningOffHeight);
		Float_0 = num2 * Mathf.Abs(Physics.gravity.y);
		return result;
	}

	public static Vector3 smethod_1(Vector3 wind)
	{
		float num = (Float_0 - Single_2) * (Float_0 + Single_2) / (2f * (Single_0 - Single_1));
		_ = (Single_0 - Single_1) / (Float_0 + Single_2);
		float num2 = (Float_0 - Single_2) / num;
		return smethod_3(wind) * num2 * Mathf.InverseLerp(Single_0, Single_1, EFTHardSettings.Instance.WindTurningOffHeight);
	}

	public static Vector3 smethod_2(Vector3 wind)
	{
		return Single_3 * smethod_3(wind) * Mathf.InverseLerp(Single_1, 0f, EFTHardSettings.Instance.WindTurningOffHeight);
	}

	public static Vector3 smethod_3(Vector3 wind)
	{
		if (EFTHardSettings.Instance.WindFactor == 0f)
		{
			return Vector3.zero;
		}
		return EFTHardSettings.Instance.DebugWind + wind;
	}
}
