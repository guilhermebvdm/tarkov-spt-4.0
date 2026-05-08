using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using EFT.Game.Spawning;
using UnityEngine;

public class GClass696
{
	public class GClass697
	{
		[NonSerialized]
		public ISpawnPoint IspawnPoint_0;

		[NonSerialized]
		[CompilerGenerated]
		public bool Bool_0;

		[NonSerialized]
		[CompilerGenerated]
		public float Float_0;

		public bool NotOccupied
		{
			[CompilerGenerated]
			get
			{
				return Bool_0;
			}
			[CompilerGenerated]
			set
			{
				Bool_0 = value;
			}
		}

		public float SDist
		{
			[CompilerGenerated]
			get
			{
				return Float_0;
			}
			[CompilerGenerated]
			set
			{
				Float_0 = value;
			}
		}

		public GClass697(ISpawnPoint spawnPoint)
		{
			IspawnPoint_0 = spawnPoint;
		}

		public StringBuilder DebugInfo()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("_spawnPoint:" + IspawnPoint_0.Id);
			stringBuilder.Append($"NotOccupied:{NotOccupied}");
			stringBuilder.Append($"SDist:{SDist}");
			return stringBuilder;
		}
	}

	public bool Inited;

	[NonSerialized]
	public ISpawnPoint IspawnPoint_0;

	[NonSerialized]
	public Vector3? Nullable_0;

	[NonSerialized]
	public List<GClass697> List_0 = new List<GClass697>();

	[NonSerialized]
	public GClass697 Gclass697_0;

	[NonSerialized]
	public ISpawnPoint IspawnPoint_1;

	[NonSerialized]
	public float Float_0;

	public StringBuilder DebugInfo()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (IspawnPoint_0 == null)
		{
			stringBuilder.Append("_groupPlayerIsNullISpawnPoint == null");
			if (Nullable_0.HasValue)
			{
				stringBuilder.AppendLine($"_groupCenter:{Nullable_0.Value}");
			}
			stringBuilder.AppendLine("start checking...");
			foreach (GClass697 item in List_0)
			{
				stringBuilder.AppendLine(item.DebugInfo().ToString());
			}
			stringBuilder.AppendLine("end checking...");
			if (IspawnPoint_1 != null)
			{
				stringBuilder.AppendLine($"_closest:{IspawnPoint_1.Id} ClosestЕmpDist:{Float_0}");
			}
			return stringBuilder;
		}
		stringBuilder.Append("_groupPlayerIsNullISpawnPoint:" + IspawnPoint_0.Id);
		return stringBuilder;
	}

	public void GroupPlayerIsNull(ISpawnPoint spawnPoint)
	{
		IspawnPoint_0 = spawnPoint;
	}

	public void SetGroupCenter(Vector3 groupCenter)
	{
		Nullable_0 = groupCenter;
	}

	public void CheckValidPoints(ISpawnPoint validPoint)
	{
		GClass697 item = (Gclass697_0 = new GClass697(validPoint));
		List_0.Add(item);
	}

	public void CheckValidPointsIsOccupied(bool notOccupied)
	{
		Gclass697_0.NotOccupied = notOccupied;
	}

	public void CheckValidPointsSDist(float sDist)
	{
		Gclass697_0.SDist = sDist;
	}

	public void SetClosest(ISpawnPoint closest, float tmpDist)
	{
		IspawnPoint_1 = closest;
		Float_0 = tmpDist;
	}
}
