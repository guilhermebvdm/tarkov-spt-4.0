using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

[Serializable]
public class SubBehaviourCOM : SubBehaviourBase
{
	[Serializable]
	public enum Mode
	{
		FeetCentroid,
		CenterOfPressure
	}

	public Mode mode;

	public float velocityDamper = 1f;

	public float velocityLerpSpeed = 5f;

	public float velocityMax = 1f;

	public float centerOfPressureSpeed = 5f;

	public Vector3 offset;

	[HideInInspector]
	public bool[] groundContacts;

	[HideInInspector]
	public Vector3[] groundContactPoints;

	private LayerMask groundLayers;

	public Vector3 position { get; private set; }

	public Vector3 direction { get; private set; }

	public float angle { get; private set; }

	public Vector3 velocity { get; private set; }

	public Vector3 centerOfPressure { get; private set; }

	public Quaternion rotation { get; private set; }

	public Quaternion inverseRotation { get; private set; }

	public bool isGrounded { get; private set; }

	public float lastGroundedTime { get; private set; }

	public void Initiate(BehaviourBase behaviour, LayerMask groundLayers)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		base.behaviour = behaviour;
		this.groundLayers = groundLayers;
		rotation = Quaternion.identity;
		groundContacts = new bool[behaviour.puppetMaster.muscles.Length];
		groundContactPoints = (Vector3[])(object)new Vector3[groundContacts.Length];
		behaviour.OnPreActivate = (BehaviourBase.BehaviourDelegate)Delegate.Combine(behaviour.OnPreActivate, new BehaviourBase.BehaviourDelegate(OnPreActivate));
		behaviour.OnPreLateUpdate = (BehaviourBase.BehaviourUpdateDelegate)Delegate.Combine(behaviour.OnPreLateUpdate, new BehaviourBase.BehaviourUpdateDelegate(OnPreLateUpdate));
		behaviour.OnPreDeactivate = (BehaviourBase.BehaviourDelegate)Delegate.Combine(behaviour.OnPreDeactivate, new BehaviourBase.BehaviourDelegate(OnPreDeactivate));
		behaviour.OnPreMuscleCollision = (BehaviourBase.CollisionDelegate)Delegate.Combine(behaviour.OnPreMuscleCollision, new BehaviourBase.CollisionDelegate(OnPreMuscleCollision));
		behaviour.OnPreMuscleCollisionExit = (BehaviourBase.CollisionDelegate)Delegate.Combine(behaviour.OnPreMuscleCollisionExit, new BehaviourBase.CollisionDelegate(OnPreMuscleCollisionExit));
		behaviour.OnHierarchyChanged = (BehaviourBase.BehaviourDelegate)Delegate.Combine(behaviour.OnHierarchyChanged, new BehaviourBase.BehaviourDelegate(OnHierarchyChanged));
	}

	private void OnHierarchyChanged()
	{
		Array.Resize(ref groundContacts, behaviour.puppetMaster.muscles.Length);
		Array.Resize(ref groundContactPoints, behaviour.puppetMaster.muscles.Length);
	}

	private void OnPreMuscleCollision(MuscleCollision c)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if (LayerMaskExtensions.Contains(groundLayers, c.collision.gameObject.layer) && c.collision.contacts.Length != 0)
		{
			lastGroundedTime = Time.time;
			groundContacts[c.muscleIndex] = true;
			if (mode == Mode.CenterOfPressure)
			{
				groundContactPoints[c.muscleIndex] = GetCollisionCOP(c.collision);
			}
		}
	}

	private void OnPreMuscleCollisionExit(MuscleCollision c)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (LayerMaskExtensions.Contains(groundLayers, c.collision.gameObject.layer))
		{
			groundContacts[c.muscleIndex] = false;
			groundContactPoints[c.muscleIndex] = Vector3.zero;
		}
	}

	private void OnPreActivate()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		position = GetCenterOfMass();
		centerOfPressure = GetFeetCentroid();
		direction = position - centerOfPressure;
		angle = Vector3.Angle(direction, Vector3.up);
		velocity = Vector3.zero;
	}

	private void OnPreLateUpdate(float deltaTime)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		isGrounded = IsGrounded();
		if (mode == Mode.FeetCentroid || !isGrounded)
		{
			centerOfPressure = GetFeetCentroid();
		}
		else
		{
			Vector3 val = (isGrounded ? GetCenterOfPressure() : GetFeetCentroid());
			centerOfPressure = ((centerOfPressureSpeed <= 2f) ? val : Vector3.Lerp(centerOfPressure, val, deltaTime * centerOfPressureSpeed));
		}
		position = GetCenterOfMass();
		Vector3 centerOfMassVelocity = GetCenterOfMassVelocity();
		Vector3 val2 = centerOfMassVelocity - position;
		val2.y = 0f;
		val2 = Vector3.ClampMagnitude(val2, velocityMax);
		velocity = ((velocityLerpSpeed <= 0f) ? val2 : Vector3.Lerp(velocity, val2, deltaTime * velocityLerpSpeed));
		position += velocity * velocityDamper;
		position += behaviour.puppetMaster.targetRoot.rotation * offset;
		direction = position - centerOfPressure;
		rotation = Quaternion.FromToRotation(Vector3.up, direction);
		inverseRotation = Quaternion.Inverse(rotation);
		angle = Quaternion.Angle(Quaternion.identity, rotation);
	}

	private void OnPreDeactivate()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		velocity = Vector3.zero;
	}

	private Vector3 GetCollisionCOP(Collision collision)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		for (int i = 0; i < collision.contacts.Length; i++)
		{
			val += collision.contacts[i].point;
		}
		return val / (float)collision.contacts.Length;
	}

	private bool IsGrounded()
	{
		for (int i = 0; i < groundContacts.Length; i++)
		{
			if (groundContacts[i])
			{
				return true;
			}
		}
		return false;
	}

	private Vector3 GetCenterOfMass()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		float num = 0f;
		Muscle[] muscles = behaviour.puppetMaster.muscles;
		foreach (Muscle muscle in muscles)
		{
			val += muscle.rigidbody.worldCenterOfMass * muscle.rigidbody.mass;
			num += muscle.rigidbody.mass;
		}
		return val /= num;
	}

	private Vector3 GetCenterOfMassVelocity()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		float num = 0f;
		Muscle[] muscles = behaviour.puppetMaster.muscles;
		foreach (Muscle muscle in muscles)
		{
			val += muscle.rigidbody.worldCenterOfMass * muscle.rigidbody.mass;
			val += muscle.rigidbody.velocity * muscle.rigidbody.mass;
			num += muscle.rigidbody.mass;
		}
		return val /= num;
	}

	private Vector3 GetMomentum()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		for (int i = 0; i < behaviour.puppetMaster.muscles.Length; i++)
		{
			val += behaviour.puppetMaster.muscles[i].rigidbody.velocity * behaviour.puppetMaster.muscles[i].rigidbody.mass;
		}
		return val;
	}

	private Vector3 GetCenterOfPressure()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		int num = 0;
		for (int i = 0; i < groundContacts.Length; i++)
		{
			if (groundContacts[i])
			{
				val += groundContactPoints[i];
				num++;
			}
		}
		if (num != 0)
		{
			val /= (float)num;
		}
		return val;
	}

	private Vector3 GetFeetCentroid()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		int num = 0;
		for (int i = 0; i < behaviour.puppetMaster.muscles.Length; i++)
		{
			if (behaviour.puppetMaster.muscles[i].props.group == Muscle.Group.Foot)
			{
				val += behaviour.puppetMaster.muscles[i].rigidbody.worldCenterOfMass;
				num++;
			}
		}
		if (num != 0)
		{
			val /= (float)num;
		}
		return val;
	}
}
