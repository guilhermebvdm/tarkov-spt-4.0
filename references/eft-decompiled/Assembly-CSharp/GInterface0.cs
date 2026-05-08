using UnityEngine;

public interface GInterface0
{
	Light LightObject { get; }

	TOD_CycleParameters Cycle { get; }

	TOD_Time CurrentTime { get; }

	TOD_AtmosphereParameters Atmosphere { get; }

	Vector3 SunDirection { get; }

	float UpdateInterval { get; set; }

	float ForceIndoor { get; }

	float SunVisibility { get; set; }

	float ClearSky { get; set; }

	bool IsDay { get; set; }

	bool IsNight { get; set; }

	void Initialize();

	Vector3 LightDirectionExtrapolated(float addTime);
}
