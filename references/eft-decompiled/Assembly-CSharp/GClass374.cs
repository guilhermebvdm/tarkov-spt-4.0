using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass374
{
	public NavPoint P1;

	public NavPoint P2;

	[NonSerialized]
	[CompilerGenerated]
	public GClass374 Gclass374_0;

	[NonSerialized]
	[CompilerGenerated]
	public GClass374 Gclass374_1;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_1;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_2;

	[NonSerialized]
	public NavEdge NavEdge_0;

	[NonSerialized]
	public Func<GClass392, NavEdge, bool> Func_0;

	public int CorePointId;

	public GClass374 Left
	{
		[CompilerGenerated]
		get
		{
			return Gclass374_0;
		}
		[CompilerGenerated]
		set
		{
			Gclass374_0 = value;
		}
	}

	public GClass374 Right
	{
		[CompilerGenerated]
		get
		{
			return Gclass374_1;
		}
		[CompilerGenerated]
		set
		{
			Gclass374_1 = value;
		}
	}

	public float LeftAng
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

	public float RightAng
	{
		[CompilerGenerated]
		get
		{
			return Float_1;
		}
		[CompilerGenerated]
		set
		{
			Float_1 = value;
		}
	}

	public int GroupId
	{
		[CompilerGenerated]
		get
		{
			return Int_0;
		}
		[CompilerGenerated]
		set
		{
			Int_0 = value;
		}
	}

	public float Magnitude
	{
		[CompilerGenerated]
		get
		{
			return Float_2;
		}
		[CompilerGenerated]
		set
		{
			Float_2 = value;
		}
	}

	public GClass374(NavEdge edge, Func<GClass392, NavEdge, bool> cutEdge, int corePointId)
	{
		CorePointId = corePointId;
		Func_0 = cutEdge;
		P1 = edge.Point1;
		P2 = edge.Point2;
		NavEdge_0 = edge;
		Magnitude = (P1.Pos - P2.Pos).magnitude;
	}

	public void SetLeft(GClass374 segment, float ang)
	{
		Left = segment;
		LeftAng = ang;
	}

	public void SetRight(GClass374 segment, float ang)
	{
		Right = segment;
		LeftAng = ang;
	}

	public NavPoint GetOther(NavPoint prevPoint)
	{
		if (P1 == prevPoint)
		{
			return P2;
		}
		return P1;
	}

	public Vector3 OrtNoraml()
	{
		return GClass855.Rotate90(GClass855.NormalizeFastSelf(P1.Pos - P2.Pos), GClass855.SideTurn.left);
	}

	public void DebugDraw(Vector3 offset)
	{
		Debug.DrawLine(P1.Pos + offset, P2.Pos + offset, Color.cyan, 10f);
	}

	public bool CreatedPoint(GClass392 np1)
	{
		return Func_0(np1, NavEdge_0);
	}
}
