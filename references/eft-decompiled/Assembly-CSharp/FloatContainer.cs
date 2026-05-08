using System;
using UnityEngine;

public class FloatContainer : IComparable<FloatContainer>
{
	public FloatContainer PrevPoint;

	public NavGraphEditorPoint Owner;

	public float Val;

	public float Dist;

	public Vector3 Position => Owner.Position;

	public FloatContainer(float val, FloatContainer prevPoint, NavGraphEditorPoint owner)
	{
		Val = val;
		PrevPoint = prevPoint;
		Owner = owner;
	}

	public int CompareTo(FloatContainer other)
	{
		if (Owner.Id == other.Owner.Id)
		{
			return 0;
		}
		if (other.Val < Val)
		{
			return 1;
		}
		return -1;
	}
}
