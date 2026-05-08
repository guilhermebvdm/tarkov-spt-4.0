using System;
using UnityEngine;

namespace RootMotion.FinalIK;

public class Recoil : OffsetModifier
{
	[Serializable]
	public class RecoilOffset
	{
		[Serializable]
		public class EffectorLink
		{
			[Tooltip("Type of the FBBIK effector to use")]
			public FullBodyBipedEffector effector;

			[Tooltip("Weight of using this effector")]
			public float weight;
		}

		[Tooltip("Offset vector for the associated effector when doing recoil.")]
		public Vector3 offset;

		[Tooltip("When firing before the last recoil has faded, how much of the current recoil offset will be maintained?")]
		[Range(0f, 1f)]
		public float additivity = 1f;

		[Tooltip("Max additive recoil for automatic fire.")]
		public float maxAdditiveOffsetMag = 0.2f;

		[Tooltip("Linking this recoil offset to FBBIK effectors.")]
		public EffectorLink[] effectorLinks;

		[NonSerialized]
		public Vector3 AdditiveOffset;

		[NonSerialized]
		public Vector3 LastOffset;

		public void Start()
		{
			if (!(additivity <= 0f))
			{
				AdditiveOffset = Vector3.ClampMagnitude(LastOffset * additivity, maxAdditiveOffsetMag);
			}
		}

		public void Apply(IKSolverFullBodyBiped solver, Quaternion rotation, float masterWeight, float length, float timeLeft)
		{
			AdditiveOffset = Vector3.Lerp(Vector3.zero, AdditiveOffset, timeLeft / length);
			LastOffset = rotation * (offset * masterWeight) + rotation * AdditiveOffset;
			EffectorLink[] array = effectorLinks;
			foreach (EffectorLink effectorLink in array)
			{
				solver.GetEffector(effectorLink.effector).positionOffset += LastOffset * effectorLink.weight;
			}
		}
	}

	[Serializable]
	public enum Handedness
	{
		Right,
		Left
	}

	[Tooltip("Reference to the AimIK component. Optional, only used to getting the aiming direction.")]
	public AimIK aimIK;

	[Tooltip("Which hand is holding the weapon?")]
	public Handedness handedness;

	[Tooltip("Check for 2-handed weapons.")]
	public bool twoHanded = true;

	[Tooltip("Weight curve for the recoil offsets. Recoil procedure is as long as this curve.")]
	public AnimationCurve recoilWeight;

	[Tooltip("How much is the magnitude randomized each time Recoil is called?")]
	public float magnitudeRandom = 0.1f;

	[Tooltip("How much is the rotation randomized each time Recoil is called?")]
	public Vector3 rotationRandom;

	[Tooltip("Rotating the primary hand bone for the recoil (in local space).")]
	public Vector3 handRotationOffset;

	[Tooltip("Time of blending in another recoil when doing automatic fire.")]
	public float blendTime;

	[Space(10f)]
	[Tooltip("FBBIK effector position offsets for the recoil (in aiming direction space).")]
	public RecoilOffset[] offsets;

	[HideInInspector]
	public Quaternion rotationOffset = Quaternion.identity;

	private float float_1 = 1f;

	private float float_2 = -1f;

	private Quaternion quaternion_0;

	private Quaternion quaternion_1;

	private Quaternion quaternion_2;

	private float float_3 = 1f;

	private bool bool_0;

	private float float_4;

	private float float_5;

	private Quaternion quaternion_3 = Quaternion.identity;

	private bool bool_1;

	public IKEffector IKEffector_0
	{
		get
		{
			if (handedness == Handedness.Right)
			{
				return ik.solver.rightHandEffector;
			}
			return ik.solver.leftHandEffector;
		}
	}

	public IKEffector IKEffector_1
	{
		get
		{
			if (handedness == Handedness.Right)
			{
				return ik.solver.leftHandEffector;
			}
			return ik.solver.rightHandEffector;
		}
	}

	public Transform Transform_0 => IKEffector_0.bone;

	public Transform Transform_1 => IKEffector_1.bone;

	public void SetHandRotations(Quaternion leftHandRotation, Quaternion rightHandRotation)
	{
		if (handedness == Handedness.Left)
		{
			quaternion_3 = leftHandRotation;
		}
		else
		{
			quaternion_3 = rightHandRotation;
		}
		bool_1 = true;
	}

	public void Fire(float magnitude)
	{
		float num = magnitude * UnityEngine.Random.value * magnitudeRandom;
		float_1 = magnitude + num;
		quaternion_2 = Quaternion.Euler(rotationRandom * UnityEngine.Random.value);
		RecoilOffset[] array = offsets;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Start();
		}
		if (Time.time < float_2)
		{
			float_4 = 0f;
		}
		else
		{
			float_4 = 1f;
		}
		float_3 = recoilWeight.keys[^1].time;
		float_2 = Time.time + float_3;
	}

	public override void OnModifyOffset()
	{
		if (Time.time >= float_2)
		{
			rotationOffset = Quaternion.identity;
			return;
		}
		if (!bool_0 && ik != null)
		{
			bool_0 = true;
			IKSolverFullBodyBiped solver = ik.solver;
			solver.OnPostUpdate = (IKSolver.GDelegate47)Delegate.Combine(solver.OnPostUpdate, new IKSolver.GDelegate47(method_2));
		}
		blendTime = Mathf.Max(blendTime, 0f);
		if (blendTime > 0f)
		{
			float_4 = Mathf.Min(float_4 + Time.deltaTime * (1f / blendTime), 1f);
		}
		else
		{
			float_4 = 1f;
		}
		float b = recoilWeight.Evaluate(float_3 - (float_2 - Time.time)) * float_1;
		float_5 = Mathf.Lerp(float_5, b, float_4);
		Quaternion quaternion = ((aimIK != null) ? Quaternion.LookRotation(aimIK.solver.IKPosition - aimIK.solver.transform.position, ik.references.root.up) : ik.references.root.rotation);
		quaternion = quaternion_2 * quaternion;
		RecoilOffset[] array = offsets;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Apply(ik.solver, quaternion, float_5, float_3, float_2 - Time.time);
		}
		if (!bool_1)
		{
			quaternion_3 = Transform_0.rotation;
		}
		bool_1 = false;
		rotationOffset = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(quaternion_2 * quaternion_3 * handRotationOffset), float_5);
		quaternion_0 = rotationOffset * quaternion_3;
		if (twoHanded)
		{
			Vector3 vector = Quaternion.Inverse(Transform_0.rotation) * (Transform_1.position - Transform_0.position);
			quaternion_1 = Quaternion.Inverse(Transform_0.rotation) * Transform_1.rotation;
			Vector3 vector2 = Transform_0.position + IKEffector_0.positionOffset + quaternion_0 * vector;
			IKEffector_1.positionOffset += vector2 - (Transform_1.position + IKEffector_1.positionOffset);
		}
	}

	public void method_2()
	{
		if (!(Time.time >= float_2))
		{
			Transform_0.rotation = quaternion_0;
			if (twoHanded)
			{
				Transform_1.rotation = Transform_0.rotation * quaternion_1;
			}
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (ik != null && bool_0)
		{
			IKSolverFullBodyBiped solver = ik.solver;
			solver.OnPostUpdate = (IKSolver.GDelegate47)Delegate.Remove(solver.OnPostUpdate, new IKSolver.GDelegate47(method_2));
		}
	}
}
