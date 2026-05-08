using System;
using Audio.AmbientSubsystem;
using Audio.SpatialSystem;
using Comfort.Common;
using EFT;
using UnityEngine;

public class GClass1188 : IDisposable
{
	[NonSerialized]
	public Transform Transform_0;

	[NonSerialized]
	public Bounds Bounds_0;

	[NonSerialized]
	public RoomAmbientData RoomAmbientData_0 = new RoomAmbientData();

	public Vector3 Vector3_0
	{
		get
		{
			if (!(Transform_0 == null))
			{
				return Transform_0.position;
			}
			return Vector3.zero;
		}
	}

	public GClass1188()
	{
		AudioListenerConsistencyManager instance = Singleton<AudioListenerConsistencyManager>.Instance;
		if (instance != null)
		{
			Transform_0 = instance.transform;
		}
	}

	public void UpdateRoom(ISpatialAudioRoom room)
	{
		if (room == null)
		{
			Dispose();
			return;
		}
		Bounds_0 = room.Bounds;
		RoomAmbientData_0 = room.RoomAmbientData;
	}

	public float CalculateNormalizedVolumeByRange()
	{
		float num = GClass2313.CalculateHeightPercentageRelativeToBounds(Vector3_0, in Bounds_0);
		Vector2 precipitationVolumeRange = RoomAmbientData_0.PrecipitationVolumeRange;
		return Mathf.Lerp(precipitationVolumeRange.x, precipitationVolumeRange.y, num / 100f);
	}

	public void Dispose()
	{
		Bounds_0 = default(Bounds);
		RoomAmbientData_0 = new RoomAmbientData();
	}
}
