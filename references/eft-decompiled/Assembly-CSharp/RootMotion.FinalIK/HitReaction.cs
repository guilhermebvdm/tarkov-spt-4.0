using System;
using System.Collections.Generic;
using UnityEngine;

namespace RootMotion.FinalIK;

public class HitReaction : OffsetModifier
{
	[Serializable]
	public abstract class HitPoint
	{
		[Tooltip("Just for visual clarity, not used at all")]
		public string name;

		[Tooltip("Linking this hit point to a collider")]
		public Collider collider;

		public EBodyPartColliderType[] colliderTypes;

		public EBodyPart[] bodyParts;

		[Tooltip("Only used if this hit point gets hit when already processing another hit")]
		[SerializeField]
		public float crossFadeTime = 0.1f;

		[NonSerialized]
		public float Length;

		[NonSerialized]
		public float CrossFadeSpeed;

		[NonSerialized]
		public float LastTime;

		[field: NonSerialized]
		public float crossFader { get; set; }

		[field: NonSerialized]
		public float timer { get; set; }

		[field: NonSerialized]
		public Vector3 force { get; set; }

		[field: NonSerialized]
		public Vector3 point { get; set; }

		public void Hit(Vector3 force, Vector3 point)
		{
			if (Length == 0f)
			{
				Length = GetLength();
			}
			if (Length <= 0f)
			{
				Debug.LogError("Hit Point WeightCurve length is zero.");
				return;
			}
			if (timer < 1f)
			{
				crossFader = 0f;
			}
			CrossFadeSpeed = ((crossFadeTime > 0f) ? (1f / crossFadeTime) : 0f);
			CrossFadeStart();
			timer = 0f;
			this.force = force;
			this.point = point;
		}

		public void Apply(IKSolverFullBodyBiped solver, float weight)
		{
			float num = Time.time - LastTime;
			LastTime = Time.time;
			if (!(timer >= Length))
			{
				timer = Mathf.Clamp(timer + num, 0f, Length);
				if (CrossFadeSpeed > 0f)
				{
					crossFader = Mathf.Clamp(crossFader + num * CrossFadeSpeed, 0f, 1f);
				}
				else
				{
					crossFader = 1f;
				}
				OnApply(solver, weight);
			}
		}

		public abstract float GetLength();

		public abstract void CrossFadeStart();

		public abstract void OnApply(IKSolverFullBodyBiped solver, float weight);

		public HitPoint()
		{
		}
	}

	[Serializable]
	public class HitPointEffector : HitPoint
	{
		[Serializable]
		public class EffectorLink
		{
			[Tooltip("The FBBIK effector type")]
			public FullBodyBipedEffector effector;

			[Tooltip("The weight of this effector (could also be negative)")]
			public float weight;

			[NonSerialized]
			public Vector3 LastValue;

			[NonSerialized]
			public Vector3 Current;

			public void Apply(IKSolverFullBodyBiped solver, Vector3 offset, float crossFader)
			{
				Current = Vector3.Lerp(LastValue, offset * weight, crossFader);
				solver.GetEffector(effector).positionOffset += Current;
			}

			public void CrossFadeStart()
			{
				LastValue = Current;
			}
		}

		[Tooltip("Offset magnitude in the direction of the hit force")]
		public AnimationCurve offsetInForceDirection;

		[Tooltip("Offset magnitude in the direction of character.up")]
		public AnimationCurve offsetInUpDirection;

		[Tooltip("Linking this offset to the FBBIK effectors")]
		public EffectorLink[] effectorLinks;

		public override float GetLength()
		{
			int length = offsetInForceDirection.length;
			float num = ((length > 0) ? offsetInForceDirection[length - 1].time : 0f);
			int length2 = offsetInUpDirection.length;
			float min = ((length2 > 0) ? offsetInUpDirection[length2 - 1].time : 0f);
			return Mathf.Clamp(num, min, num);
		}

		public override void CrossFadeStart()
		{
			EffectorLink[] array = effectorLinks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].CrossFadeStart();
			}
		}

		public override void OnApply(IKSolverFullBodyBiped solver, float weight)
		{
			Vector3 vector = solver.GetRoot().up * base.force.magnitude;
			Vector3 offset = offsetInForceDirection.Evaluate(base.timer) * base.force + offsetInUpDirection.Evaluate(base.timer) * vector;
			offset *= weight;
			EffectorLink[] array = effectorLinks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Apply(solver, offset, base.crossFader);
			}
		}
	}

	[Serializable]
	public class HitPointBone : HitPoint
	{
		[Serializable]
		public class BoneLink
		{
			[Tooltip("Reference to the bone that this hit point rotates")]
			public Transform bone;

			[Tooltip("Weight of rotating the bone")]
			[Range(0f, 1f)]
			public float weight;

			[NonSerialized]
			public Quaternion LastValue;

			[NonSerialized]
			public Quaternion Current;

			public void Apply(IKSolverFullBodyBiped solver, Quaternion offset, float crossFader)
			{
				Current = Quaternion.Lerp(LastValue, Quaternion.Lerp(Quaternion.identity, offset, weight), crossFader);
				bone.rotation = Current * bone.rotation;
			}

			public void CrossFadeStart()
			{
				LastValue = Current;
			}
		}

		[Tooltip("The angle to rotate the bone around it's rigidbody's world center of mass")]
		public AnimationCurve aroundCenterOfMass;

		[Tooltip("Linking this hit point to bone(s)")]
		public BoneLink[] boneLinks;

		[NonSerialized]
		public Rigidbody Rigidbody;

		public override float GetLength()
		{
			if (aroundCenterOfMass.keys.Length == 0)
			{
				return 0f;
			}
			return aroundCenterOfMass.keys[aroundCenterOfMass.length - 1].time;
		}

		public override void CrossFadeStart()
		{
			BoneLink[] array = boneLinks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].CrossFadeStart();
			}
		}

		public override void OnApply(IKSolverFullBodyBiped solver, float weight)
		{
			if (Rigidbody == null)
			{
				Rigidbody = collider.GetComponent<Rigidbody>();
			}
			if (Rigidbody != null)
			{
				Vector3 axis = Vector3.Cross(base.force, base.point - Rigidbody.worldCenterOfMass);
				Quaternion offset = Quaternion.AngleAxis(aroundCenterOfMass.Evaluate(base.timer) * weight, axis);
				BoneLink[] array = boneLinks;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Apply(solver, offset, base.crossFader);
				}
			}
		}
	}

	public List<HitPoint> Recoil = new List<HitPoint>();

	[SerializeField]
	private HitPointEffector[] effectorHitPointsRecoil;

	[SerializeField]
	private HitPointBone[] boneHitPointsRecoil;

	[Tooltip("Hit points for the FBBIK effectors")]
	public HitPointEffector[] effectorHitPoints;

	[Tooltip(" Hit points for bones without an effector, such as the head")]
	public HitPointBone[] boneHitPoints;

	public override void OnModifyOffset()
	{
		HitPointEffector[] array = effectorHitPoints;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Apply(ik.solver, weight);
		}
		HitPointBone[] array2 = boneHitPoints;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].Apply(ik.solver, weight);
		}
		foreach (HitPoint item in Recoil)
		{
			item.Apply(ik.solver, weight);
		}
	}

	public override void Start()
	{
		base.Start();
		Recoil.Clear();
		Recoil.AddRange(effectorHitPointsRecoil);
		Recoil.AddRange(boneHitPointsRecoil);
	}

	public void Hit(EBodyPartColliderType colliderType, EBodyPart bodyPart, Vector3 force, Vector3 point, bool any = false)
	{
		if (ik == null)
		{
			Debug.LogError("No IK assigned in HitReaction");
			return;
		}
		if (any)
		{
			effectorHitPoints[UnityEngine.Random.Range(0, effectorHitPoints.Length)].Hit(force, point);
			return;
		}
		HitPointEffector[] array = effectorHitPoints;
		foreach (HitPointEffector hitPointEffector in array)
		{
			EBodyPart[] bodyParts = hitPointEffector.bodyParts;
			for (int j = 0; j < bodyParts.Length; j++)
			{
				if (bodyParts[j] == bodyPart)
				{
					hitPointEffector.Hit(force, point);
					break;
				}
			}
			EBodyPartColliderType[] colliderTypes = hitPointEffector.colliderTypes;
			for (int j = 0; j < colliderTypes.Length; j++)
			{
				if (colliderTypes[j] == colliderType)
				{
					hitPointEffector.Hit(force, point);
					break;
				}
			}
		}
		HitPointBone[] array2 = boneHitPoints;
		foreach (HitPointBone hitPointBone in array2)
		{
			EBodyPart[] bodyParts = hitPointBone.bodyParts;
			for (int j = 0; j < bodyParts.Length; j++)
			{
				if (bodyParts[j] == bodyPart)
				{
					hitPointBone.Hit(force, point);
					break;
				}
			}
			EBodyPartColliderType[] colliderTypes = hitPointBone.colliderTypes;
			for (int j = 0; j < colliderTypes.Length; j++)
			{
				if (colliderTypes[j] == colliderType)
				{
					hitPointBone.Hit(force, point);
					break;
				}
			}
		}
	}
}
