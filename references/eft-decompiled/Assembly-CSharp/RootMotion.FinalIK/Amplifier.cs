using System;
using UnityEngine;

namespace RootMotion.FinalIK;

public class Amplifier : OffsetModifier
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

		[Tooltip("The Transform that's motion we are reading.")]
		public Transform transform;

		[Tooltip("Amplify the 'transform's' position relative to this Transform.")]
		public Transform relativeTo;

		[Tooltip("Linking the body to effectors. One Body can be used to offset more than one effector.")]
		public EffectorLink[] effectorLinks;

		[Tooltip("Amplification magnitude along the up axis of the character.")]
		public float verticalWeight = 1f;

		[Tooltip("Amplification magnitude along the horizontal axes of the character.")]
		public float horizontalWeight = 1f;

		[Tooltip("Speed of the amplifier. 0 means instant.")]
		public float speed = 3f;

		[NonSerialized]
		public Vector3 LastRelativePos;

		[NonSerialized]
		public Vector3 SmoothDelta;

		[NonSerialized]
		public bool FirstUpdate;

		public void Update(IKSolverFullBodyBiped solver, float w, float deltaTime)
		{
			if (!(transform == null) && !(relativeTo == null))
			{
				Vector3 vector = relativeTo.InverseTransformDirection(transform.position - relativeTo.position);
				if (FirstUpdate)
				{
					LastRelativePos = vector;
					FirstUpdate = false;
				}
				Vector3 vector2 = (vector - LastRelativePos) / deltaTime;
				SmoothDelta = ((speed <= 0f) ? vector2 : Vector3.Lerp(SmoothDelta, vector2, deltaTime * speed));
				Vector3 v = relativeTo.TransformDirection(SmoothDelta);
				Vector3 vector3 = GClass1464.ExtractVertical(v, solver.GetRoot().up, verticalWeight) + GClass1464.ExtractHorizontal(v, solver.GetRoot().up, horizontalWeight);
				for (int i = 0; i < effectorLinks.Length; i++)
				{
					solver.GetEffector(effectorLinks[i].effector).positionOffset += vector3 * w * effectorLinks[i].weight;
				}
				LastRelativePos = vector;
			}
		}

		public static Vector3 smethod_0(Vector3 v1, Vector3 v2)
		{
			v1.x *= v2.x;
			v1.y *= v2.y;
			v1.z *= v2.z;
			return v1;
		}
	}

	[Tooltip("The amplified bodies.")]
	public Body[] bodies;

	public override void OnModifyOffset()
	{
		if (!ik.fixTransforms)
		{
			if (!GClass1465.logged)
			{
				GClass1465.Log("Amplifier needs the Fix Transforms option of the FBBIK to be set to true. Otherwise it might amplify to infinity, should the animator of the character stop because of culling.", base.transform);
			}
			return;
		}
		Body[] array = bodies;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Update(ik.solver, weight, base.deltaTime);
		}
	}
}
