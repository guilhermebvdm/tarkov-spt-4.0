using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass1111 : GInterface72
{
	[NonSerialized]
	public Transform Transform_0;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public Vector3 Vector3_1;

	public bool IsPositionTracked
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

	public Vector3 Position
	{
		[CompilerGenerated]
		get
		{
			return Vector3_1;
		}
		[CompilerGenerated]
		set
		{
			Vector3_1 = value;
		}
	}

	public void StartTracking(Transform tracker, Vector3 positionOffset = default(Vector3))
	{
		Transform_0 = tracker;
		Vector3_0 = positionOffset;
		IsPositionTracked = true;
	}

	public void StopTracking()
	{
		IsPositionTracked = false;
		Transform_0 = null;
		Vector3_0 = Vector3.zero;
	}

	public void UpdatePosition()
	{
		if (IsPositionTracked && Transform_0 != null)
		{
			Position = Transform_0.position + Vector3_0;
		}
	}
}
