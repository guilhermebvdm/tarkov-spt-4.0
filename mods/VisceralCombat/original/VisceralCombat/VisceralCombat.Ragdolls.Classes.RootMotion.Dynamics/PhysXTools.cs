using System;
using UnityEngine;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

public static class PhysXTools
{
	public static void Predict(Rigidbody r, int steps, out Vector3 position, out Quaternion rotation)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Predict(r, steps, out position, out rotation, Physics.gravity, r.drag, r.angularDrag);
	}

	public static void Predict(Rigidbody r, int steps, out Vector3 position, out Quaternion rotation, Vector3 gravity, float drag, float angularDrag)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		position = r.position;
		rotation = r.rotation;
		Vector3 velocity = r.velocity;
		Vector3 angularVelocity = r.angularVelocity;
		for (int i = 0; i < steps; i++)
		{
			Predict(ref position, ref rotation, ref velocity, ref angularVelocity, gravity, drag, angularDrag);
		}
	}

	public static void Predict(ref Vector3 position, ref Quaternion rotation, ref Vector3 velocity, ref Vector3 angularVelocity, Vector3 gravity, float drag, float angularDrag)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		velocity += gravity * Time.fixedDeltaTime;
		velocity -= velocity * drag * Time.fixedDeltaTime;
		angularVelocity -= angularVelocity * angularDrag * Time.fixedDeltaTime;
		Vector3 val = velocity * Time.fixedDeltaTime;
		Vector3 val2 = angularVelocity * Time.fixedDeltaTime * 57.29578f;
		position += val;
		rotation *= Quaternion.Euler(val2);
	}

	public static Vector3 GetCenterOfMass(PuppetMaster puppet)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		float num = 0f;
		for (int i = 0; i < puppet.muscles.Length; i++)
		{
			if (((Component)puppet.muscles[i].joint).gameObject.activeInHierarchy)
			{
				val += puppet.muscles[i].rigidbody.worldCenterOfMass * puppet.muscles[i].rigidbody.mass;
				num += puppet.muscles[i].rigidbody.mass;
			}
		}
		return val / num;
	}

	public static Vector3 GetCenterOfMass(Rigidbody[] rigidbodies)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		float num = 0f;
		for (int i = 0; i < rigidbodies.Length; i++)
		{
			if (((Component)rigidbodies[i]).gameObject.activeInHierarchy)
			{
				val += rigidbodies[i].worldCenterOfMass * rigidbodies[i].mass;
				num += rigidbodies[i].mass;
			}
		}
		return val / num;
	}

	public static Vector3 GetCenterOfMassVelocity(Rigidbody[] rigidbodies)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		float num = 0f;
		for (int i = 0; i < rigidbodies.Length; i++)
		{
			if (((Component)rigidbodies[i]).gameObject.activeInHierarchy)
			{
				val += rigidbodies[i].velocity * rigidbodies[i].mass;
				num += rigidbodies[i].mass;
			}
		}
		return val / num;
	}

	public static void DivByInertia(ref Vector3 v, Quaternion rotation, Vector3 inertiaTensor)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		v = rotation * Div(Quaternion.Inverse(rotation) * v, inertiaTensor);
	}

	public static void ScaleByInertia(ref Vector3 v, Quaternion rotation, Vector3 inertiaTensor)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		v = rotation * Vector3.Scale(Quaternion.Inverse(rotation) * v, inertiaTensor);
	}

	public static Vector3 GetAngularVelocity(Quaternion lastRotation, Quaternion rotation, float deltaTime)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = rotation * Quaternion.Inverse(lastRotation);
		float num = 0f;
		Vector3 zero = Vector3.zero;
		((Quaternion)(ref val)).ToAngleAxis(ref num, ref zero);
		if (float.IsNaN(zero.x))
		{
			return Vector3.zero;
		}
		if (float.IsInfinity(zero.x))
		{
			return Vector3.zero;
		}
		num *= MathF.PI / 180f;
		num /= deltaTime;
		num = QuaTools.ToBiPolar(num);
		return zero * num;
	}

	public static Vector3 GetFromToAcceleration(Vector3 fromV, Vector3 toV)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = Quaternion.FromToRotation(fromV, toV);
		float num = 0f;
		Vector3 zero = Vector3.zero;
		((Quaternion)(ref val)).ToAngleAxis(ref num, ref zero);
		Vector3 val2 = num * zero * (MathF.PI / 180f);
		return val2 / Time.fixedDeltaTime;
	}

	public static Vector3 GetAngularAcceleration(Quaternion fromR, Quaternion toR)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.Cross(fromR * Vector3.forward, toR * Vector3.forward);
		Vector3 val2 = Vector3.Cross(fromR * Vector3.up, toR * Vector3.up);
		float num = Quaternion.Angle(fromR, toR);
		Vector3 val3 = Vector3.Normalize(val + val2) * num * (MathF.PI / 180f);
		return val3 / Time.fixedDeltaTime;
	}

	public static void AddFromToTorque(Rigidbody r, Quaternion toR, ForceMode forceMode)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected I4, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 angularAcceleration = GetAngularAcceleration(r.rotation, toR);
		angularAcceleration -= r.angularVelocity;
		switch ((int)forceMode)
		{
		case 5:
			r.AddTorque(angularAcceleration / Time.fixedDeltaTime, forceMode);
			break;
		case 0:
		{
			Vector3 v2 = angularAcceleration / Time.fixedDeltaTime;
			ScaleByInertia(ref v2, r.rotation, r.inertiaTensor);
			r.AddTorque(v2, forceMode);
			break;
		}
		case 1:
		{
			Vector3 v = angularAcceleration;
			ScaleByInertia(ref v, r.rotation, r.inertiaTensor);
			r.AddTorque(v, forceMode);
			break;
		}
		case 2:
			r.AddTorque(angularAcceleration, forceMode);
			break;
		case 3:
		case 4:
			break;
		}
	}

	public static void AddFromToTorque(Rigidbody r, Vector3 fromV, Vector3 toV, ForceMode forceMode)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected I4, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		Vector3 fromToAcceleration = GetFromToAcceleration(fromV, toV);
		fromToAcceleration -= r.angularVelocity;
		switch ((int)forceMode)
		{
		case 5:
			r.AddTorque(fromToAcceleration / Time.fixedDeltaTime, forceMode);
			break;
		case 0:
		{
			Vector3 v2 = fromToAcceleration / Time.fixedDeltaTime;
			ScaleByInertia(ref v2, r.rotation, r.inertiaTensor);
			r.AddTorque(v2, forceMode);
			break;
		}
		case 1:
		{
			Vector3 v = fromToAcceleration;
			ScaleByInertia(ref v, r.rotation, r.inertiaTensor);
			r.AddTorque(v, forceMode);
			break;
		}
		case 2:
			r.AddTorque(fromToAcceleration, forceMode);
			break;
		case 3:
		case 4:
			break;
		}
	}

	public static void AddFromToForce(Rigidbody r, Vector3 fromV, Vector3 toV, ForceMode forceMode)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected I4, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		Vector3 linearAcceleration = GetLinearAcceleration(fromV, toV);
		linearAcceleration -= r.velocity;
		switch ((int)forceMode)
		{
		case 5:
			r.AddForce(linearAcceleration / Time.fixedDeltaTime, forceMode);
			break;
		case 0:
		{
			Vector3 val2 = linearAcceleration / Time.fixedDeltaTime;
			val2 *= r.mass;
			r.AddForce(val2, forceMode);
			break;
		}
		case 1:
		{
			Vector3 val = linearAcceleration;
			val *= r.mass;
			r.AddForce(val, forceMode);
			break;
		}
		case 2:
			r.AddForce(linearAcceleration, forceMode);
			break;
		case 3:
		case 4:
			break;
		}
	}

	public static Vector3 GetLinearAcceleration(Vector3 fromPoint, Vector3 toPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return (toPoint - fromPoint) / Time.fixedDeltaTime;
	}

	public static Quaternion ToJointSpace(ConfigurableJoint joint)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.Cross(((Joint)joint).axis, joint.secondaryAxis);
		Vector3 val2 = Vector3.Cross(val, ((Joint)joint).axis);
		return Quaternion.LookRotation(val, val2);
	}

	public static Vector3 CalculateInertiaTensorCuboid(Vector3 size, float mass)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		float num = size.x * size.x;
		float num2 = size.y * size.y;
		float num3 = size.z * size.z;
		float num4 = 1f / 12f * mass;
		return new Vector3(num4 * (num2 + num3), num4 * (num + num3), num4 * (num + num2));
	}

	public static Vector3 Div(Vector3 v, Vector3 v2)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(v.x / v2.x, v.y / v2.y, v.z / v2.z);
	}

	public static bool RayCapsuleIntersectUnscaled(Vector3 origin, Vector3 direction, CapsuleCollider capsule)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		return RayCapsuleIntersect(origin, direction, ((Component)capsule).transform.position, ((Component)capsule).transform.rotation, capsule.center, capsule.radius, capsule.height, capsule.direction, 1f);
	}

	public static bool RayCapsuleIntersect(Vector3 origin, Vector3 direction, CapsuleCollider capsule, float uniformScale)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		return RayCapsuleIntersect(origin, direction, ((Component)capsule).transform.position, ((Component)capsule).transform.rotation, capsule.center, capsule.radius, capsule.height, capsule.direction, uniformScale);
	}

	public static bool RayCapsuleIntersect(Vector3 origin, Vector3 direction, Vector3 capsuleTransformPos, Quaternion capsuleTransformRot, Vector3 capsuleCenter, float capsuleRadius, float capsuleHeight, int capsuleDir, float scale)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		float num = capsuleRadius;
		float num2 = Mathf.Max(num, capsuleHeight);
		num *= scale;
		num2 *= scale;
		Vector3 val = capsuleTransformRot * (Vector3)(capsuleDir switch
		{
			1 => Vector3.up, 
			0 => Vector3.right, 
			_ => Vector3.forward, 
		});
		float num3 = num2 * 0.5f - num;
		Vector3 val2 = val * num3;
		Vector3 val3 = capsuleTransformPos + capsuleTransformRot * capsuleCenter * scale;
		Vector3 c = val3 - val2;
		Vector3 c2 = val3 + val2;
		return RayCapsuleIntersect(origin, direction, c, c2, num);
	}

	public static bool RayCapsuleIntersect(Vector3 rayOrigin, Vector3 rayDir, Vector3 c1, Vector3 c2, float cRadius)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = c2 - c1;
		Vector3 val2 = rayOrigin - c1;
		float num = Vector3.Dot(val, val);
		float num2 = Vector3.Dot(val, rayDir);
		float num3 = Vector3.Dot(val, val2);
		float num4 = Vector3.Dot(rayDir, val2);
		float num5 = Vector3.Dot(val2, val2);
		float num6 = num - num2 * num2;
		float num7 = num * num4 - num3 * num2;
		float num8 = num * num5 - num3 * num3 - cRadius * cRadius * num;
		float num9 = num7 * num7 - num6 * num8;
		if ((double)num9 >= 0.0)
		{
			float num10 = (0f - num7 - Mathf.Sqrt(num9)) / num6;
			float num11 = num3 + num10 * num2;
			if (num11 > 0f && num11 < num)
			{
				return num10 > 0f;
			}
			Vector3 val3 = ((num11 <= 0f) ? val2 : (rayOrigin - c2));
			num7 = Vector3.Dot(rayDir, val3);
			num8 = Vector3.Dot(val3, val3) - cRadius * cRadius;
			num9 = num7 * num7 - num8;
			if (num9 > 0f)
			{
				return 0f - num7 - Mathf.Sqrt(num9) > 0f;
			}
		}
		return false;
	}
}
