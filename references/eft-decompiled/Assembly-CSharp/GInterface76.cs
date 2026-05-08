using UnityEngine;

public interface GInterface76
{
	float PlayingTimeThresholdSeconds { get; }

	void SetEnvironment(EnvironmentType environment);

	AudioClip GetImpactClip();

	float CalculateVolume(float forceMagnitude);

	float GetRandomPitch();

	bool IsOutOfRange(Vector3 position, int sqrRolloff);
}
