using System;
using Audio.SpatialSystem;
using UnityEngine;

public class Class804
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2;

	public float SmoothedEffect => Float_2;

	public bool Boolean_0 => Float_2 > Float_1;

	public Class804(float smoothingFactor, float threshold = 0.001f)
	{
		Float_0 = smoothingFactor;
		Float_1 = threshold;
		Float_2 = 0f;
	}

	public void Update(bool isInStairs, float rawHeight, float deltaTime)
	{
		float b = (isInStairs ? rawHeight : 0f);
		float t = Mathf.Clamp01(Float_0 * deltaTime);
		Float_2 = Mathf.Lerp(Float_2, b, t);
	}

	public bool TryUpdate(SpatialAudioSystem audioSystem, SourceContainerClass container, Transform listenerTransform)
	{
		if (!container.IsValid)
		{
			return false;
		}
		Vector3 currentPosition = container.CurrentPosition;
		bool flag = audioSystem.IsListenerAndSourceInSameRoom(container.CurrentAudioRoom, EAudioRoomTypeMask.IndoorStairs);
		float rawHeight = 0f;
		if (flag && container.Count > 0)
		{
			Vector3 position = listenerTransform.position;
			AudioGroupOcclusionSettings spatialSettings = container[0].SpatialSettings;
			float num = ((audioSystem.OcclusionSettings.commonSettings.floorHeight > 0f) ? audioSystem.OcclusionSettings.commonSettings.floorHeight : 3f);
			float time = Mathf.Abs(currentPosition.y - position.y) / num;
			float num2 = spatialSettings.stairsHeightCurve.Evaluate(time);
			rawHeight = Mathf.Clamp01(1f - num2);
		}
		Update(flag, rawHeight, Time.deltaTime);
		return Boolean_0;
	}
}
