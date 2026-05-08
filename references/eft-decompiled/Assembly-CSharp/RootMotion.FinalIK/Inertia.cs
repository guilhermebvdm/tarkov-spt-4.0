using System;
using UnityEngine;

namespace RootMotion.FinalIK;

public class Inertia : OffsetModifier
{
	[Serializable]
	public class Body
	{
		[Serializable]
		public class EffectorLink
		{
			[Tooltip("Type of the FBBIK effector to use")]
			public FullBodyBipedEffector effector;

			[Tooltip("Weight of using this effector")]
			public float weight;
		}

		[Tooltip("The Transform to follow, can be any bone of the character")]
		public Transform transform;

		[Tooltip("Linking the body to effectors. One Body can be used to offset more than one effector")]
		public EffectorLink[] effectorLinks;

		[Tooltip("The speed to follow the Transform")]
		public float speed = 10f;

		[Tooltip("The acceleration, smaller values means lazyer following")]
		public float acceleration = 3f;

		[Tooltip("Matching target velocity")]
		[Range(0f, 1f)]
		public float matchVelocity;

		[Tooltip("gravity applied to the Body")]
		public float gravity;

		[NonSerialized]
		public Vector3 Delta;

		[NonSerialized]
		public Vector3 LazyPoint;

		[NonSerialized]
		public Vector3 Direction;

		[NonSerialized]
		public Vector3 LastPosition;

		[NonSerialized]
		public bool FirstUpdate = true;

		public void Reset()
		{
			if (!(transform == null))
			{
				LazyPoint = transform.position;
				LastPosition = transform.position;
				Direction = Vector3.zero;
			}
		}

		public void Update(IKSolverFullBodyBiped solver, float weight, float deltaTime)
		{
			if (!(transform == null))
			{
				if (FirstUpdate)
				{
					Reset();
					FirstUpdate = false;
				}
				Direction = Vector3.Lerp(Direction, (transform.position - LazyPoint) / deltaTime * 0.01f, deltaTime * acceleration);
				LazyPoint += Direction * deltaTime * speed;
				Delta = transform.position - LastPosition;
				LazyPoint += Delta * matchVelocity;
				LazyPoint.y += gravity * deltaTime;
				EffectorLink[] array = effectorLinks;
				foreach (EffectorLink effectorLink in array)
				{
					solver.GetEffector(effectorLink.effector).positionOffset += (LazyPoint - transform.position) * effectorLink.weight * weight;
				}
				LastPosition = transform.position;
			}
		}
	}

	[Tooltip("The array of Bodies")]
	public Body[] bodies;

	[Tooltip("The array of OffsetLimits")]
	public OffsetLimits[] limits;

	public void ResetBodies()
	{
		Body[] array = bodies;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Reset();
		}
	}

	public override void OnModifyOffset()
	{
		Body[] array = bodies;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Update(ik.solver, weight, base.deltaTime);
		}
		ApplyLimits(limits);
	}
}
