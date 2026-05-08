using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public abstract class BotCurrentPathAbstractClass
{
	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public Vector3[] Vector3_0;

	[NonSerialized]
	public Vector3 Vector3_1;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_0 = 0.5f;

	public Action<int, Vector3[]> onPathCornerChanged;

	public float ReachDist
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

	public abstract GInterface9 TargetPoint { get; }

	public int Length => Vector3_0.Length;

	public int CurIndex => Int_0;

	public string DebugInfo => "TODO. Not implemented";

	public BotCurrentPathAbstractClass(Vector3[] path)
	{
		Vector3_1 = path[0];
		Vector3_0 = path;
	}

	public void SetReachDist(float reachDist)
	{
		ReachDist = reachDist;
	}

	public Vector3 GetReturnPoint()
	{
		if (Vector3_0.Length > Int_0)
		{
			return Vector3_0[Int_0];
		}
		if (Vector3_0.Length != 0)
		{
			return Vector3_0[Vector3_0.Length - 1];
		}
		return default(Vector3);
	}

	public Vector3 GetPoint(int index)
	{
		return Vector3_0[index];
	}

	public abstract void DrawSelectedGizmosWay(float upCoef, Color color);

	public abstract void DrawDebugWay(float upCoef);

	public Vector3 CurrentCorner()
	{
		return Vector3_0[Int_0];
	}

	public Vector3 LastCorner()
	{
		return Vector3_0[Vector3_0.Length - 1];
	}

	public bool IsLast()
	{
		return Int_0 >= Length;
	}

	public abstract void Complete();

	public void IncCornerIndex()
	{
		Int_0++;
		onPathCornerChanged?.Invoke(Int_0, Vector3_0);
		if (IsLast())
		{
			Complete();
		}
	}

	public void CurPathChanged()
	{
		onPathCornerChanged?.Invoke(Int_0, Vector3_0);
	}

	public void CheckIsFinished(BotOwner bot)
	{
		Vector3 vector = bot.Position - TargetPoint.Position;
		float sqrMagnitude = vector.sqrMagnitude;
		if (!(vector.y > 0.4f) && !(sqrMagnitude > ReachDist * ReachDist))
		{
			Complete();
		}
	}

	public abstract bool IsSame(Vector3 target, Vector3 ownerPosition);

	public abstract bool IsSame(IAICorePointLink target, Vector3 ownerPosition);

	public bool IsPointOnWay(Vector3 p, float distToBeClose)
	{
		return GClass369.IsPointOnWay(Vector3_0, p, distToBeClose, CurIndex);
	}

	public List<Vector3> GetCornersFromIndex(int startIndex)
	{
		List<Vector3> list = new List<Vector3>();
		for (int i = startIndex; i < Vector3_0.Length; i++)
		{
			list.Add(Vector3_0[i]);
		}
		return list;
	}
}
