using System;
using UnityEngine;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

public abstract class RagdollCreator : MonoBehaviour
{
	[Serializable]
	public enum ColliderType
	{
		Box,
		Capsule
	}

	[Serializable]
	public enum JointType
	{
		Configurable,
		Character
	}

	[Serializable]
	public enum Direction
	{
		X,
		Y,
		Z
	}

	public struct CreateJointParams
	{
		public struct Limits
		{
			public float minSwing;

			public float maxSwing;

			public float swing2;

			public float twist;

			public Limits(float minSwing, float maxSwing, float swing2, float twist)
			{
				this.minSwing = minSwing;
				this.maxSwing = maxSwing;
				this.swing2 = swing2;
				this.twist = twist;
			}
		}

		public Rigidbody rigidbody;

		public Rigidbody connectedBody;

		public Transform child;

		public Vector3 worldSwingAxis;

		public Limits limits;

		public JointType type;

		public CreateJointParams(Rigidbody rigidbody, Rigidbody connectedBody, Transform child, Vector3 worldSwingAxis, Limits limits, JointType type)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			this.rigidbody = rigidbody;
			this.connectedBody = connectedBody;
			this.child = child;
			this.worldSwingAxis = worldSwingAxis;
			this.limits = limits;
			this.type = type;
		}
	}

	public static void ClearAll(Transform root)
	{
		if ((Object)(object)root == (Object)null)
		{
			return;
		}
		Transform val = root;
		Animator componentInChildren = ((Component)root).GetComponentInChildren<Animator>();
		if ((Object)(object)componentInChildren != (Object)null && componentInChildren.isHuman)
		{
			Transform boneTransform = componentInChildren.GetBoneTransform((HumanBodyBones)0);
			if ((Object)(object)boneTransform != (Object)null)
			{
				Transform[] componentsInChildren = ((Component)boneTransform).GetComponentsInChildren<Transform>();
				if (componentsInChildren.Length > 2)
				{
					val = boneTransform;
				}
			}
		}
		Transform[] componentsInChildren2 = ((Component)val).GetComponentsInChildren<Transform>();
		if (componentsInChildren2.Length >= 2)
		{
			for (int i = ((!((Object)(object)componentInChildren != (Object)null) || !componentInChildren.isHuman) ? 1 : 0); i < componentsInChildren2.Length; i++)
			{
				ClearTransform(componentsInChildren2[i]);
			}
		}
	}

	protected static void ClearTransform(Transform transform)
	{
		if ((Object)(object)transform == (Object)null)
		{
			return;
		}
		if (((Object)transform).name == "Foot Collider")
		{
			Object.DestroyImmediate((Object)(object)((Component)transform).gameObject);
			return;
		}
		Collider[] components = ((Component)transform).GetComponents<Collider>();
		Collider[] array = components;
		foreach (Collider val in array)
		{
			if ((Object)(object)val != (Object)null && !val.isTrigger)
			{
				Object.DestroyImmediate((Object)(object)val);
			}
		}
		Joint component = ((Component)transform).GetComponent<Joint>();
		if ((Object)(object)component != (Object)null)
		{
			Object.DestroyImmediate((Object)(object)component);
		}
		Rigidbody component2 = ((Component)transform).GetComponent<Rigidbody>();
		if ((Object)(object)component2 != (Object)null)
		{
			Object.DestroyImmediate((Object)(object)component2);
		}
	}

	protected static Collider CreateCollider(Transform t, Vector3 startPoint, Vector3 endPoint, ColliderType colliderType, float lengthOverlap, float width, Transform rigidbodyT = null)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rigidbodyT == (Object)null)
		{
			rigidbodyT = t;
		}
		Vector3 direction = endPoint - startPoint;
		float num = ((Vector3)(ref direction)).magnitude * (1f + lengthOverlap);
		Vector3 axisVectorToDirection = AxisTools.GetAxisVectorToDirection(t, direction);
		((Component)rigidbodyT).gameObject.AddComponent<Rigidbody>();
		float scaleF = GetScaleF(t);
		switch (colliderType)
		{
		case ColliderType.Capsule:
		{
			CapsuleCollider val3 = ((Component)t).gameObject.AddComponent<CapsuleCollider>();
			val3.height = Mathf.Abs(num / scaleF);
			val3.radius = Mathf.Abs(width * 0.75f / scaleF);
			val3.direction = DirectionVector3ToInt(axisVectorToDirection);
			val3.center = t.InverseTransformPoint(Vector3.Lerp(startPoint, endPoint, 0.5f));
			return (Collider)(object)val3;
		}
		case ColliderType.Box:
		{
			Vector3 val = Vector3.Scale(axisVectorToDirection, new Vector3(num, num, num));
			if (val.x == 0f)
			{
				val.x = width;
			}
			if (val.y == 0f)
			{
				val.y = width;
			}
			if (val.z == 0f)
			{
				val.z = width;
			}
			BoxCollider val2 = ((Component)t).gameObject.AddComponent<BoxCollider>();
			val2.size = val / scaleF;
			val2.size = new Vector3(Mathf.Abs(val2.size.x), Mathf.Abs(val2.size.y), Mathf.Abs(val2.size.z));
			val2.center = t.InverseTransformPoint(Vector3.Lerp(startPoint, endPoint, 0.5f));
			return (Collider)(object)val2;
		}
		default:
			return null;
		}
	}

	protected static void CreateCollider(Transform t, Vector3 startPoint, Vector3 endPoint, ColliderType colliderType, float lengthOverlap, float width, float proportionAspect, Vector3 widthDirection)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		if (colliderType == ColliderType.Capsule)
		{
			CreateCollider(t, startPoint, endPoint, colliderType, lengthOverlap, width * proportionAspect);
			return;
		}
		Vector3 direction = endPoint - startPoint;
		float num = ((Vector3)(ref direction)).magnitude * (1f + lengthOverlap);
		Vector3 axisVectorToDirection = AxisTools.GetAxisVectorToDirection(t, direction);
		Vector3 axisVectorToDirection2 = AxisTools.GetAxisVectorToDirection(t, widthDirection);
		if (axisVectorToDirection2 == axisVectorToDirection)
		{
			((Vector3)(ref axisVectorToDirection2))._002Ector(axisVectorToDirection.y, axisVectorToDirection.z, axisVectorToDirection.x);
		}
		((Component)t).gameObject.AddComponent<Rigidbody>();
		Vector3 val = Vector3.Scale(axisVectorToDirection, new Vector3(num, num, num));
		Vector3 val2 = Vector3.Scale(axisVectorToDirection2, new Vector3(width, width, width));
		Vector3 val3 = val + val2;
		if (val3.x == 0f)
		{
			val3.x = width * proportionAspect;
		}
		if (val3.y == 0f)
		{
			val3.y = width * proportionAspect;
		}
		if (val3.z == 0f)
		{
			val3.z = width * proportionAspect;
		}
		BoxCollider val4 = ((Component)t).gameObject.AddComponent<BoxCollider>();
		val4.size = val3 / GetScaleF(t);
		val4.center = t.InverseTransformPoint(Vector3.Lerp(startPoint, endPoint, 0.5f));
	}

	protected static float GetScaleF(Transform t)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Vector3 lossyScale = t.lossyScale;
		return (lossyScale.x + lossyScale.y + lossyScale.z) / 3f;
	}

	protected static Vector3 Abs(Vector3 v)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Vector3Abs(ref v);
		return v;
	}

	protected static void Vector3Abs(ref Vector3 v)
	{
		v.x = Mathf.Abs(v.x);
		v.y = Mathf.Abs(v.y);
		v.z = Mathf.Abs(v.z);
	}

	protected static Vector3 DirectionIntToVector3(int dir)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		return (Vector3)(dir switch
		{
			0 => Vector3.right, 
			1 => Vector3.up, 
			_ => Vector3.forward, 
		});
	}

	protected static Vector3 DirectionToVector3(Direction dir)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		return (Vector3)(dir switch
		{
			Direction.X => Vector3.right, 
			Direction.Y => Vector3.up, 
			_ => Vector3.forward, 
		});
	}

	protected static int DirectionVector3ToInt(Vector3 dir)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Dot(dir, Vector3.right);
		float num2 = Vector3.Dot(dir, Vector3.up);
		float num3 = Vector3.Dot(dir, Vector3.forward);
		float num4 = Mathf.Abs(num);
		float num5 = Mathf.Abs(num2);
		float num6 = Mathf.Abs(num3);
		int result = 0;
		if (num5 > num4 && num5 > num6)
		{
			result = 1;
		}
		if (num6 > num4 && num6 > num5)
		{
			result = 2;
		}
		return result;
	}

	protected static Vector3 GetLocalOrthoDirection(Transform transform, Vector3 worldDir)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		worldDir = ((Vector3)(ref worldDir)).normalized;
		float num = Vector3.Dot(worldDir, transform.right);
		float num2 = Vector3.Dot(worldDir, transform.up);
		float num3 = Vector3.Dot(worldDir, transform.forward);
		float num4 = Mathf.Abs(num);
		float num5 = Mathf.Abs(num2);
		float num6 = Mathf.Abs(num3);
		Vector3 val = Vector3.right;
		if (num5 > num4 && num5 > num6)
		{
			val = Vector3.up;
		}
		if (num6 > num4 && num6 > num5)
		{
			val = Vector3.forward;
		}
		if (Vector3.Dot(worldDir, transform.rotation * val) < 0f)
		{
			val = -val;
		}
		return val;
	}

	protected static Rigidbody GetConnectedBody(Transform bone, ref Transform[] bones)
	{
		if ((Object)(object)bone.parent == (Object)null)
		{
			return null;
		}
		Transform[] array = bones;
		foreach (Transform val in array)
		{
			if ((Object)(object)bone.parent == (Object)(object)val && (Object)(object)((Component)val).GetComponent<Rigidbody>() != (Object)null)
			{
				return ((Component)val).GetComponent<Rigidbody>();
			}
		}
		return GetConnectedBody(bone.parent, ref bones);
	}

	protected static void CreateJoint(CreateJointParams p)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localOrthoDirection = GetLocalOrthoDirection(((Component)p.rigidbody).transform, p.worldSwingAxis);
		Vector3 val = Vector3.forward;
		if ((Object)(object)p.child != (Object)null)
		{
			val = GetLocalOrthoDirection(((Component)p.rigidbody).transform, p.child.position - ((Component)p.rigidbody).transform.position);
		}
		else if ((Object)(object)p.connectedBody != (Object)null)
		{
			val = GetLocalOrthoDirection(((Component)p.rigidbody).transform, ((Component)p.rigidbody).transform.position - ((Component)p.connectedBody).transform.position);
		}
		Vector3 val2 = Vector3.Cross(localOrthoDirection, val);
		if (p.type == JointType.Configurable)
		{
			ConfigurableJoint val3 = ((Component)p.rigidbody).gameObject.AddComponent<ConfigurableJoint>();
			((Joint)val3).connectedBody = p.connectedBody;
			ConfigurableJointMotion val4 = (ConfigurableJointMotion)((!((Object)(object)p.connectedBody != (Object)null)) ? 2 : 0);
			ConfigurableJointMotion val5 = (ConfigurableJointMotion)(((Object)(object)p.connectedBody != (Object)null) ? 1 : 2);
			val3.xMotion = val4;
			val3.yMotion = val4;
			val3.zMotion = val4;
			val3.angularXMotion = val5;
			val3.angularYMotion = val5;
			val3.angularZMotion = val5;
			if ((Object)(object)p.connectedBody != (Object)null)
			{
				((Joint)val3).axis = localOrthoDirection;
				val3.secondaryAxis = val2;
				val3.lowAngularXLimit = ToSoftJointLimit(p.limits.minSwing);
				val3.highAngularXLimit = ToSoftJointLimit(p.limits.maxSwing);
				val3.angularYLimit = ToSoftJointLimit(p.limits.swing2);
				val3.angularZLimit = ToSoftJointLimit(p.limits.twist);
			}
			((Joint)val3).anchor = Vector3.zero;
		}
		else if (!((Object)(object)p.connectedBody == (Object)null))
		{
			CharacterJoint val6 = ((Component)p.rigidbody).gameObject.AddComponent<CharacterJoint>();
			((Joint)val6).connectedBody = p.connectedBody;
			((Joint)val6).axis = localOrthoDirection;
			val6.swingAxis = val2;
			val6.lowTwistLimit = ToSoftJointLimit(p.limits.minSwing);
			val6.highTwistLimit = ToSoftJointLimit(p.limits.maxSwing);
			val6.swing1Limit = ToSoftJointLimit(p.limits.swing2);
			val6.swing2Limit = ToSoftJointLimit(p.limits.twist);
			((Joint)val6).anchor = Vector3.zero;
		}
	}

	private static SoftJointLimit ToSoftJointLimit(float limit)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		SoftJointLimit result = default(SoftJointLimit);
		((SoftJointLimit)(ref result)).limit = limit;
		return result;
	}
}
