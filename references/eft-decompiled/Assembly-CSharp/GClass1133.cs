using System;
using System.Collections.Generic;
using Audio.SpatialSystem;
using UnityEngine;

public class GClass1133 : GClass1128
{
	[NonSerialized]
	public const int Int_0 = 0;

	[NonSerialized]
	public const int Int_1 = 1;

	[NonSerialized]
	public const float Float_2 = 0.05f;

	[NonSerialized]
	public Dictionary<int, GStruct88> Dictionary_0 = new Dictionary<int, GStruct88>();

	[NonSerialized]
	public List<GStruct88> List_0 = new List<GStruct88>();

	[NonSerialized]
	public uint Uint_0;

	[NonSerialized]
	public float Float_3;

	public GClass1133(uint maxPropagationDepths, SpatialAudioSystem audioSystem)
		: base(audioSystem)
	{
		Uint_0 = maxPropagationDepths;
	}

	public override float GetScore()
	{
		return Float_3;
	}

	public override float CalculateScore(Vector3 listenerPosition, SourceContainerClass sourceContainer)
	{
		SpatialAudioSystem instance = MonoBehaviourSingleton<SpatialAudioSystem>.Instance;
		Vector3 currentPosition = sourceContainer.CurrentPosition;
		bool flag = instance.IsSourceInListenerRoom(sourceContainer);
		if (instance.IsSourceAndListenerInCommonOutdoor(sourceContainer.CurrentAudioRoom))
		{
			return 0f;
		}
		if (flag)
		{
			return 0f;
		}
		if (Vector3.SqrMagnitude(listenerPosition - currentPosition) > sourceContainer.SqrMaxDistance)
		{
			return 1f;
		}
		if (instance.TryGetMatchingRoomPair(sourceContainer.CurrentAudioRoom, out var roomPair) && roomPair.Routes.Length != 0)
		{
			Dictionary_0.Clear();
			Float_3 = float.MaxValue;
			float num = float.MaxValue;
			RoomPair.Route[] routes = roomPair.Routes;
			for (int i = 0; i < routes.Length; i++)
			{
				RoomPair.Route route = routes[i];
				if (!(Float_3 < route.HeuristicCost))
				{
					method_3(out var cost, out var distance);
					if (cost < Float_3 - 0.01f || (Mathf.Abs(cost - Float_3) <= 0.01f && distance < num))
					{
						Float_3 = cost;
						num = distance;
					}
				}
			}
			return Float_3;
		}
		return 1f;
	}

	public void method_0(Vector3 listenerPosition, Vector3 sourcePosition, BaseSpatialAudioPortal portal)
	{
		short iD = portal.ID;
		if (!Dictionary_0.ContainsKey(iD))
		{
			Vector3 point = (sourcePosition + listenerPosition) / 2f;
			Vector3 vector = portal.portalCollider.bounds.ClosestPoint(point);
			GStruct88 value = new GStruct88(GStruct88.ENodeType.Portal, vector, portal.PortalClosureLevel, portal.traversalMaxCost, portal.Depth, portal.FrontRoom.WallOcclusion, portal.BackRoom.WallOcclusion, GClass1118.IsOutdoor(portal.FrontRoom.Type), GClass1118.IsOutdoor(portal.BackRoom.Type));
			Dictionary_0[iD] = value;
		}
	}

	public Vector3 method_1(Vector3 point, Collider portalCollider)
	{
		if (!portalCollider.bounds.Contains(point))
		{
			return portalCollider.ClosestPointOnBounds(point);
		}
		return point;
	}

	public void method_2(Vector3 sourcePosition, Vector3 listenerPosition, BaseSpatialAudioPortal[] portals)
	{
		List_0.Clear();
		List_0.Add(new GStruct88(GStruct88.ENodeType.Emitter, sourcePosition));
		foreach (BaseSpatialAudioPortal baseSpatialAudioPortal in portals)
		{
			if (Dictionary_0.TryGetValue(baseSpatialAudioPortal.ID, out var value))
			{
				GStruct88 item = value.Clone();
				List_0.Add(item);
			}
		}
		List_0.Add(new GStruct88(GStruct88.ENodeType.Listener, listenerPosition));
	}

	public void method_3(out float cost, out float distance)
	{
		float num = 0f;
		distance = 0f;
		for (int i = 0; i < List_0.Count - 2; i++)
		{
			GStruct88 gStruct = List_0[i];
			GStruct88 gStruct2 = List_0[i + 1];
			GStruct88 gStruct3 = List_0[i + 2];
			Vector3 vector = gStruct2.Position - gStruct.Position;
			Vector3 to = gStruct3.Position - gStruct2.Position;
			float magnitude = vector.magnitude;
			float magnitude2 = to.magnitude;
			if (i == List_0.Count - 3)
			{
				distance += magnitude + magnitude2;
			}
			else
			{
				distance += magnitude;
			}
			float num2 = ((magnitude < 0.05f || !(magnitude2 >= 0.05f)) ? 0f : Vector3.Angle(vector, to));
			float f = Mathf.Clamp01(num2 / 180f);
			f = Mathf.Pow(f, 0.5f);
			float value = ((!(magnitude < magnitude2)) ? (magnitude2 / (float)Uint_0) : (magnitude / (float)Uint_0));
			value = Mathf.Clamp01(value);
			float num3 = value * (f + gStruct2.PortalTraversalMaxCost) + gStruct2.PortalClosureLevel;
			float num4 = (((!gStruct2.IsFrontRoomOutdoor || gStruct2.IsBackRoomOutdoor) && (gStruct2.IsFrontRoomOutdoor || !gStruct2.IsBackRoomOutdoor)) ? ((gStruct2.FrontRoomWallOcclusion + gStruct2.BackRoomWallOcclusion) * 0.5f) : (gStruct2.IsFrontRoomOutdoor ? gStruct2.BackRoomWallOcclusion : gStruct2.FrontRoomWallOcclusion));
			num3 *= num4;
			num += num3;
		}
		num = Mathf.Clamp01(num);
		Vector3 a = List_0[0].Position;
		Vector3 b = List_0[List_0.Count - 1].Position;
		float num5 = Vector3.Distance(a, b);
		float num6 = ((distance > 0f) ? Mathf.Clamp01(num5 / distance) : 0f);
		float num7 = 1f - num6;
		cost = Mathf.Clamp01(num + num7 * 0.3f);
	}
}
