using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class ConstraintPositionOffset : Constraint
{
	public Vector3 offset;

	[NonSerialized]
	public Vector3 DefaultLocalPosition;

	[NonSerialized]
	public Vector3 LastLocalPosition;

	[NonSerialized]
	public bool Initiated;

	public bool Boolean_0 => transform.localPosition != LastLocalPosition;

	public override void UpdateConstraint()
	{
		if (!(weight <= 0f) && base.isValid)
		{
			if (!Initiated)
			{
				DefaultLocalPosition = transform.localPosition;
				LastLocalPosition = transform.localPosition;
				Initiated = true;
			}
			if (Boolean_0)
			{
				DefaultLocalPosition = transform.localPosition;
			}
			transform.localPosition = DefaultLocalPosition;
			transform.position += offset * weight;
			LastLocalPosition = transform.localPosition;
		}
	}

	public ConstraintPositionOffset()
	{
	}

	public ConstraintPositionOffset(Transform transform)
	{
		base.transform = transform;
	}
}
