using System;
using Audio.SpatialSystem;
using UnityEngine;

public class GClass1140 : IDisposable
{
	[NonSerialized]
	public SpatialAudioSystem SpatialAudioSystem_0;

	[NonSerialized]
	public Action Action_0;

	public GClass1140(SpatialAudioSystem audioSystem)
	{
		SpatialAudioSystem_0 = audioSystem;
	}

	public void method_0(GClass3567 requestEvent)
	{
		string iD = requestEvent.ID;
		if (string.IsNullOrEmpty(iD) || requestEvent.Source == null || !GClass3670.TryGetData<GClass1138>(out var dataContainer))
		{
			return;
		}
		ISpatialAudioRoom listenerCurrentRoom = SpatialAudioSystem_0.ListenerCurrentRoom;
		if (listenerCurrentRoom.IsValid && dataContainer.TryGetInteractivePortalById(iD, out var portal) && portal.FrontRoom.ID != listenerCurrentRoom.ID && portal.BackRoom.ID != listenerCurrentRoom.ID)
		{
			RoomPair roomPair;
			bool flag = SpatialAudioSystem_0.TryGetMatchingRoomPair(portal.FrontRoom, out roomPair);
			RoomPair roomPair2;
			bool flag2 = SpatialAudioSystem_0.TryGetMatchingRoomPair(portal.BackRoom, out roomPair2);
			if (!flag && !flag2)
			{
				SpatialAudioSystem_0.ProcessSourceOcclusion(requestEvent.Source, requestEvent.OcclusionTest);
				return;
			}
			Vector3 normalized = (((!flag || (flag2 && roomPair.ShortestRouteLength > roomPair2.ShortestRouteLength)) ? portal.BackRoom : portal.FrontRoom).Position - requestEvent.PlayingPosition).normalized;
			SpatialAudioSystem_0.ProcessSourceOcclusion(requestEvent.Source, requestEvent.OcclusionTest, normalized);
		}
	}

	public bool CheckOcclusion(string targetID, BetterSource source, Vector3 position, EOcclusionTest test)
	{
		if (!string.IsNullOrEmpty(targetID) && !(source == null))
		{
			if (!GClass3670.TryGetData<GClass1138>(out var dataContainer))
			{
				return false;
			}
			ISpatialAudioRoom listenerCurrentRoom = SpatialAudioSystem_0.ListenerCurrentRoom;
			if (!listenerCurrentRoom.IsValid)
			{
				return false;
			}
			if (!dataContainer.TryGetInteractivePortalById(targetID, out var portal))
			{
				SpatialAudioSystem_0.ProcessSourceOcclusion(source, test);
				return false;
			}
			if (portal.FrontRoom.ID != listenerCurrentRoom.ID && portal.BackRoom.ID != listenerCurrentRoom.ID)
			{
				RoomPair roomPair;
				bool flag = SpatialAudioSystem_0.TryGetMatchingRoomPair(portal.FrontRoom, out roomPair);
				RoomPair roomPair2;
				bool flag2 = SpatialAudioSystem_0.TryGetMatchingRoomPair(portal.BackRoom, out roomPair2);
				if (!flag && !flag2)
				{
					SpatialAudioSystem_0.ProcessSourceOcclusion(source, test);
					return false;
				}
				Vector3 normalized = (((!flag || (flag2 && roomPair.ShortestRouteLength > roomPair2.ShortestRouteLength)) ? portal.BackRoom : portal.FrontRoom).Position - position).normalized;
				SpatialAudioSystem_0.ProcessSourceOcclusion(source, test, normalized);
				return true;
			}
			return false;
		}
		return false;
	}

	public void Dispose()
	{
		Action_0?.Invoke();
		Action_0 = null;
	}
}
