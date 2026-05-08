using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Comfort.Common;
using JetBrains.Annotations;
using UnityEngine;

namespace EFT;

public class BotsSmokesVisionSystem
{
	public class Class1093 : IEquatable<Class1093>
	{
		public readonly int Id;

		public float WorldRadius;

		public float WorldRadiusSqr;

		public Vector3 WorldPosition;

		public Class1093(int id, Vector3 worldPosition)
		{
			Id = id;
			float worldRadius = 0f;
			WorldRadiusSqr = 0f;
			WorldRadius = worldRadius;
			WorldPosition = worldPosition;
		}

		public bool Equals(Class1093 other)
		{
			if (other != null)
			{
				return Id == other.Id;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is Class1093 other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Id;
		}
	}

	[CompilerGenerated]
	public class Class1094
	{
		public int id;

		public bool method_0(Class1093 data)
		{
			return data.Id == id;
		}
	}

	[NonSerialized]
	public const float UPDATE_INTERVAL = 1f;

	[NonSerialized]
	public BotsClass BotsList;

	[NonSerialized]
	public AICoversData CoversData;

	[NonSerialized]
	public List<Class1093> ActiveSmokes = new List<Class1093>();

	[NonSerialized]
	public Dictionary<Vector3Int, HashSet<Class1093>> AffectedVoxelsMap = new Dictionary<Vector3Int, HashSet<Class1093>>();

	[NonSerialized]
	public Dictionary<int, HashSet<int>> BotsInSmokes = new Dictionary<int, HashSet<int>>();

	[NonSerialized]
	public List<Class1093> SmokeGrenadesAlongRayBuffer = new List<Class1093>();

	[NonSerialized]
	public Vector3[] BoundsExtendsBuffer = new Vector3[6];

	[NonSerialized]
	public float NextUpdateTime;

	public BotsSmokesVisionSystem(BotsClass botsList, AICoversData coversData)
	{
		BotsList = botsList;
		CoversData = coversData;
	}

	public void TryAddGrenade(int id, Vector3 position)
	{
		if (!ActiveSmokes.Exists((Class1093 data) => data.Id == id) && Singleton<GameWorld>.Instance.Grenades.TryGetByKey(id, out var value))
		{
			SmokeGrenade obj = (SmokeGrenade)value;
			obj.EmissionEnd = (Action<Grenade>)Delegate.Combine(obj.EmissionEnd, new Action<Grenade>(method_0));
			ActiveSmokes.Add(new Class1093(id, position));
		}
	}

	public void method_0(Grenade obj)
	{
		SmokeGrenade obj2 = (SmokeGrenade)obj;
		obj2.EmissionEnd = (Action<Grenade>)Delegate.Remove(obj2.EmissionEnd, new Action<Grenade>(method_0));
		for (int num = ActiveSmokes.Count - 1; num >= 0; num--)
		{
			if (ActiveSmokes[num].Id == obj.Id)
			{
				ActiveSmokes.RemoveAt(num);
			}
		}
	}

	public bool IsRayIntersectAnySmoke(Vector3 from, Vector3 to, [CanBeNull] out SphereCollider hitCollider, out Vector3 intersectionPoint, out int grenadesAlongWayCount)
	{
		hitCollider = null;
		List<Class1093> list = method_2(from, to);
		grenadesAlongWayCount = list.Count;
		if (list.Count == 0)
		{
			intersectionPoint = Vector3.zero;
			return false;
		}
		foreach (Class1093 item in list)
		{
			if (smethod_0(from, to, item.WorldPosition, item.WorldRadiusSqr, out intersectionPoint))
			{
				if (Singleton<GameWorld>.Instance.Grenades.TryGetByKey(item.Id, out var value))
				{
					hitCollider = ((SmokeGrenade)value).Area;
					return true;
				}
				return false;
			}
		}
		intersectionPoint = Vector3.zero;
		return false;
	}

	public void Update()
	{
		if (Time.time < NextUpdateTime)
		{
			return;
		}
		NextUpdateTime = Time.time + 1f - UnityEngine.Random.Range(0f, 0.2f);
		if (ActiveSmokes.Count == 0)
		{
			AffectedVoxelsMap.Clear();
			foreach (BotOwner botOwner in BotsList.BotOwners)
			{
				int id = botOwner.Id;
				if (!BotsInSmokes.TryGetValue(id, out var value))
				{
					continue;
				}
				foreach (int item in value)
				{
					botOwner.SmokeGrenade.SmokeExit(item);
				}
				if (value.Count == 0)
				{
					BotsInSmokes.Remove(id);
				}
			}
		}
		AffectedVoxelsMap.Clear();
		for (int num = ActiveSmokes.Count - 1; num >= 0; num--)
		{
			Class1093 @class = ActiveSmokes[num];
			if (!Singleton<GameWorld>.Instance.Grenades.TryGetByKey(@class.Id, out var value2))
			{
				ActiveSmokes.RemoveAt(num);
				foreach (KeyValuePair<Vector3Int, HashSet<Class1093>> item2 in AffectedVoxelsMap)
				{
					item2.Value.Remove(@class);
				}
			}
			else
			{
				SphereCollider area = ((SmokeGrenade)value2).Area;
				Vector3 lossyScale = area.transform.lossyScale;
				@class.WorldPosition = area.bounds.center;
				@class.WorldRadius = area.radius * Mathf.Abs(Mathf.Max(lossyScale.x, lossyScale.y, lossyScale.z));
				@class.WorldRadiusSqr = @class.WorldRadius * @class.WorldRadius;
				method_3(@class, area.bounds);
				method_1(@class);
			}
		}
	}

	public void method_1(Class1093 grenadeData)
	{
		foreach (BotOwner botOwner in BotsList.BotOwners)
		{
			int id = botOwner.Id;
			HashSet<int> value2;
			if (GClass856.SqrDistance(botOwner.Position, grenadeData.WorldPosition) <= grenadeData.WorldRadiusSqr)
			{
				if (!BotsInSmokes.TryGetValue(id, out var value))
				{
					value = new HashSet<int>();
					BotsInSmokes[id] = value;
				}
				if (value.Add(grenadeData.Id))
				{
					botOwner.SmokeGrenade.SmokeEnter(grenadeData.Id);
				}
			}
			else if (BotsInSmokes.TryGetValue(id, out value2) && value2.Remove(grenadeData.Id))
			{
				botOwner.SmokeGrenade.SmokeExit(grenadeData.Id);
				if (value2.Count == 0)
				{
					BotsInSmokes.Remove(id);
				}
			}
		}
	}

	public static bool smethod_0(Vector3 segmentStart, Vector3 segmentEnd, Vector3 sphereCenter, float sphereRadiusSqr, out Vector3 hitPoint)
	{
		Vector3 vector = segmentEnd - segmentStart;
		Vector3 lhs = sphereCenter - segmentStart;
		float sqrMagnitude = vector.sqrMagnitude;
		if (sqrMagnitude < Mathf.Epsilon)
		{
			hitPoint = segmentStart;
			return (segmentStart - sphereCenter).sqrMagnitude <= sphereRadiusSqr;
		}
		float value = Vector3.Dot(lhs, vector) / sqrMagnitude;
		value = Mathf.Clamp01(value);
		return ((hitPoint = segmentStart + vector * value) - sphereCenter).sqrMagnitude <= sphereRadiusSqr;
	}

	public List<Class1093> method_2(Vector3 from, Vector3 to)
	{
		SmokeGrenadesAlongRayBuffer.Clear();
		if (AffectedVoxelsMap.Count == 0)
		{
			return SmokeGrenadesAlongRayBuffer;
		}
		Vector3 v = to - from;
		float magnitude = v.magnitude;
		GClass855.NormalizeFastSelf(ref v);
		Vector3Int indexes = CoversData.GetIndexes(from);
		Vector3Int indexes2 = CoversData.GetIndexes(to);
		if (indexes == indexes2)
		{
			if (AffectedVoxelsMap.TryGetValue(indexes, out var value))
			{
				SmokeGrenadesAlongRayBuffer.AddRange(value);
			}
			return SmokeGrenadesAlongRayBuffer;
		}
		List<NavGraphVoxelSimple> voxelesExtended = CoversData.GetVoxelesExtended(indexes.x, indexes.y, indexes.z, 1, onlyWithPoints: false);
		List<NavGraphVoxelSimple> voxelesExtended2 = CoversData.GetVoxelesExtended(indexes2.x, indexes2.y, indexes2.z, 1, onlyWithPoints: false);
		Vector3Int key = default(Vector3Int);
		for (int i = 0; i < voxelesExtended.Count; i++)
		{
			NavGraphVoxelSimple navGraphVoxelSimple = voxelesExtended[i];
			indexes.Set(navGraphVoxelSimple.IndexX, navGraphVoxelSimple.IndexY, navGraphVoxelSimple.IndexZ);
			if (AffectedVoxelsMap.TryGetValue(key, out var value2))
			{
				SmokeGrenadesAlongRayBuffer.AddRange(value2);
			}
		}
		for (int j = 0; j < voxelesExtended2.Count; j++)
		{
			NavGraphVoxelSimple navGraphVoxelSimple2 = voxelesExtended2[j];
			indexes.Set(navGraphVoxelSimple2.IndexX, navGraphVoxelSimple2.IndexY, navGraphVoxelSimple2.IndexZ);
			if (AffectedVoxelsMap.TryGetValue(indexes, out var value3))
			{
				SmokeGrenadesAlongRayBuffer.AddRange(value3);
			}
		}
		for (float num = 0f; num <= magnitude + Mathf.Epsilon; num += 10f)
		{
			indexes = CoversData.GetIndexes(from + v * num);
			if (AffectedVoxelsMap.TryGetValue(indexes, out var value4))
			{
				SmokeGrenadesAlongRayBuffer.AddRange(value4);
			}
		}
		return SmokeGrenadesAlongRayBuffer;
	}

	public void method_3(Class1093 grenadeData, Bounds bounds)
	{
		if (bounds.extents == Vector3.zero)
		{
			bounds.size = Vector3.one * grenadeData.WorldRadius;
		}
		Vector3Int indexes = CoversData.GetIndexes(bounds.center);
		method_4(grenadeData, indexes);
		Vector3[] array = method_5(bounds);
		for (int i = 0; i < array.Length; i++)
		{
			method_4(grenadeData, CoversData.GetIndexes(array[i]));
		}
	}

	public void method_4(Class1093 grenadeData, Vector3Int affectedVoxelIdx)
	{
		if (!AffectedVoxelsMap.TryGetValue(affectedVoxelIdx, out var value))
		{
			value = new HashSet<Class1093>();
			AffectedVoxelsMap[affectedVoxelIdx] = value;
		}
		value.Add(grenadeData);
	}

	public Vector3[] method_5(Bounds bounds)
	{
		Vector3 center = bounds.center;
		Vector3 extents = bounds.extents;
		BoundsExtendsBuffer[0] = center + new Vector3(0f, extents.y, 0f);
		BoundsExtendsBuffer[1] = center + new Vector3(0f, 0f - extents.y, 0f);
		BoundsExtendsBuffer[2] = center + new Vector3(0f, 0f, extents.z);
		BoundsExtendsBuffer[3] = center + new Vector3(0f, 0f, 0f - extents.z);
		BoundsExtendsBuffer[4] = center + new Vector3(0f - extents.x, 0f, 0f);
		BoundsExtendsBuffer[5] = center + new Vector3(extents.x, 0f, 0f);
		return BoundsExtendsBuffer;
	}
}
