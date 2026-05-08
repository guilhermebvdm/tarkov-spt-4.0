using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class ConstraintRotationOffset : Constraint
{
	public Quaternion offset;

	[NonSerialized]
	public Quaternion DefaultRotation;

	[NonSerialized]
	public Quaternion DefaultLocalRotation;

	[NonSerialized]
	public Quaternion LastLocalRotation;

	[NonSerialized]
	public Quaternion DefaultTargetLocalRotation;

	[NonSerialized]
	public bool Initiated;

	public bool Boolean_0 => transform.localRotation != LastLocalRotation;

	public override void UpdateConstraint()
	{
		if (!(weight <= 0f) && base.isValid)
		{
			if (!Initiated)
			{
				DefaultLocalRotation = transform.localRotation;
				LastLocalRotation = transform.localRotation;
				Initiated = true;
			}
			if (Boolean_0)
			{
				DefaultLocalRotation = transform.localRotation;
			}
			transform.localRotation = DefaultLocalRotation;
			transform.rotation = Quaternion.Slerp(transform.rotation, offset, weight);
			LastLocalRotation = transform.localRotation;
		}
	}

	public ConstraintRotationOffset()
	{
	}

	public ConstraintRotationOffset(Transform transform)
	{
		base.transform = transform;
	}
}
