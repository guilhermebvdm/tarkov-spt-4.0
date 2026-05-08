using System;
using System.Linq;
using Audio.SpatialSystem;
using Unity.Jobs;
using UnityEngine;

public class GClass1129 : GClass1128
{
	[NonSerialized]
	public GClass1180 Gclass1180_0;

	[NonSerialized]
	public float Float_2 = 1f;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public uint Uint_0;

	[NonSerialized]
	public bool Bool_1 = true;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public GClass1160 Gclass1160_0;

	[NonSerialized]
	public bool Bool_3;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public GClass1137 Gclass1137_0;

	public JobHandle LastHandle => Gclass1137_0.LastHandle;

	public GClass1129(SpatialAudioSystem system)
		: base(system)
	{
		Gclass1137_0 = new GClass1137(system.LocationInfo, system.OcclusionSettings.propagationSettings);
		Gclass1160_0 = new GClass1160();
		Gclass1160_0.SetThreshold(1f);
		Float_4 = system.OcclusionSettings.propagationSettings.GetCompressionFactorByQuality();
		Gclass1180_0 = new GClass1180();
	}

	public override void OnWarmUp()
	{
		GClass1138 data = GClass3670.GetData<GClass1138>();
		RoomPair value = data.RoomPairsByID.FirstOrDefault().Value;
		if (value != null && data.RoomsByID.TryGetValue(value.FirstRoomID, out var value2) && data.RoomsByID.TryGetValue(value.SecondRoomID, out var value3))
		{
			Gclass1137_0.PrepareData(value, isGlobalDataNeedUpdate: true);
			Gclass1137_0.Schedule(value2.Position, value3.Position, isReversed: false);
			Gclass1137_0.Complete();
		}
		base.OnWarmUp();
	}

	public override void OnUpdate(SourceContainerClass sourceContainer, Vector3 listenerPosition, bool forceUpdate = false)
	{
		Bool_3 = false;
		Bool_2 = forceUpdate;
		Bool_1 = Gclass1160_0.IsPositionChanged(sourceContainer.CurrentPosition, listenerPosition);
		if (!Bool_1 && !forceUpdate)
		{
			return;
		}
		if (sourceContainer.UseQualityCompression)
		{
			float currentDistanceSquared = Vector3.SqrMagnitude(sourceContainer.CurrentPosition - listenerPosition);
			Float_2 = Gclass1180_0.CalculateQualityFactorSq(currentDistanceSquared, sourceContainer.SqrMaxDistance);
			Float_2 = Mathf.Min(Float_2, sourceContainer.MaxOcclusionQualityFactor);
		}
		else
		{
			Float_2 = 1f;
		}
		Float_2 = Mathf.Min(Float_4, Float_2);
		Vector3 sourcePosition = sourceContainer.CurrentPosition;
		bool flag = SpatialAudioSystem_0.IsSourceInListenerRoom(sourceContainer);
		RoomPair roomPair;
		bool isReversed;
		if (SpatialAudioSystem_0.IsSourceAndListenerInCommonOutdoor(sourceContainer.CurrentAudioRoom))
		{
			Float_3 = 0f;
			Bool_3 = true;
		}
		else if (flag)
		{
			Float_3 = 0f;
			Bool_3 = true;
		}
		else if (Vector3.SqrMagnitude(listenerPosition - sourcePosition) > sourceContainer.SqrMaxDistance)
		{
			Float_3 = 1f;
			Bool_3 = true;
		}
		else if (SpatialAudioSystem_0.TryGetMatchingRoomPair(sourceContainer.CurrentAudioRoom, out roomPair, out isReversed) && roomPair.Routes.Length != 0)
		{
			bool isGlobalDataNeedUpdate;
			if ((isGlobalDataNeedUpdate = Uint_0 != roomPair.ID || Gclass1137_0.DataDisposed) || forceUpdate)
			{
				Uint_0 = roomPair.ID;
				Gclass1137_0.PrepareData(roomPair, isGlobalDataNeedUpdate);
			}
			Gclass1137_0.Schedule(in sourcePosition, in listenerPosition, isReversed, Float_2);
			base.OnUpdate(sourceContainer, listenerPosition, forceUpdate);
		}
		else
		{
			Float_3 = 1f;
			Bool_3 = true;
		}
	}

	public override void OnLateUpdate()
	{
		if (Bool_1 || Bool_2)
		{
			Bool_2 = false;
			if (!Bool_3 && Gclass1137_0.LastHandle.IsCompleted)
			{
				Gclass1137_0.Complete();
				Float_3 = Gclass1137_0.PropagationScore;
				base.OnLateUpdate();
			}
		}
	}

	public override float GetScore()
	{
		return Float_3;
	}

	public override void OnReset()
	{
		base.OnReset();
		Uint_0 = uint.MaxValue;
		Bool_3 = true;
	}

	public override void OnDispose()
	{
		Gclass1137_0?.Dispose();
		Gclass1137_0 = null;
		base.OnDispose();
	}
}
