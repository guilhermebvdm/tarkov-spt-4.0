using System;
using UnityEngine;

[Serializable]
public class BotLocationModifier
{
	public float AccuracySpeed = 1f;

	public float Scattering = 1f;

	public float GainSight = 1f;

	public float MarksmanAccuratyCoef = 1f;

	public float VisibleDistance = 1f;

	public float DistToSleep = 240f;

	public float DistToActivate = 220f;

	public float LeaveDist = 300f;

	public float DistToPersueAxemanCoef = 1f;

	public float MinExfiltrationTime;

	public float MaxExfiltrationTime;

	public AdditionalHostilitySettings[] AdditionalHostilitySettings;

	public float LockSpawnStartTime = 10f;

	public float LockSpawnStartTimePvE = 10f;

	public float LockSpawnStepTime = 50f;

	public float LockSpawnStepTimePvE = 50f;

	public float LockSpawnCheckRadius = 150f;

	public float LockSpawnCheckRadiusPvE = 150f;

	public int NonWaveSpawnBotsLimitPerPlayer = 10;

	public int NonWaveSpawnBotsLimitPerPlayerPvE = 15;

	public float FogVisibilitySpeedCoef;

	public float FogVisibilityDistanceCoef;

	public float RainVisibilitySpeedCoef;

	public float RainVisibilityDistanceCoef;

	public void Validate()
	{
		method_0(ref AccuracySpeed);
		method_0(ref Scattering);
		method_0(ref GainSight);
		method_0(ref MarksmanAccuratyCoef);
		method_0(ref VisibleDistance);
		method_0(ref DistToSleep);
		method_0(ref DistToActivate);
	}

	public void method_0(ref float val)
	{
		if (val <= 0.001f)
		{
			Debug.LogError("Wrong location map modification parameter in taxonomy. fix it manually!");
			val = 1f;
		}
	}
}
