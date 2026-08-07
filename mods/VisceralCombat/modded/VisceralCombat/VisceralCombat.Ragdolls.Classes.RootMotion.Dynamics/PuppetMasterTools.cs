using UnityEngine;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

public static class PuppetMasterTools
{
	public static void PositionRagdoll(PuppetMaster puppetMaster)
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		Rigidbody[] componentsInChildren = ((Component)((Component)puppetMaster).transform).GetComponentsInChildren<Rigidbody>();
		if (componentsInChildren.Length == 0)
		{
			return;
		}
		Muscle[] muscles = puppetMaster.muscles;
		foreach (Muscle muscle in muscles)
		{
			if ((Object)(object)muscle.joint == (Object)null || (Object)(object)muscle.target == (Object)null)
			{
				return;
			}
		}
		Vector3[] array = (Vector3[])(object)new Vector3[componentsInChildren.Length];
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			if (((Component)componentsInChildren[j]).transform.childCount == 1)
			{
				array[j] = ((Component)componentsInChildren[j]).transform.InverseTransformDirection(((Component)componentsInChildren[j]).transform.GetChild(0).position - ((Component)componentsInChildren[j]).transform.position);
			}
		}
		Rigidbody[] array2 = componentsInChildren;
		foreach (Rigidbody val in array2)
		{
			Muscle[] muscles2 = puppetMaster.muscles;
			foreach (Muscle muscle2 in muscles2)
			{
				if ((Object)(object)((Component)muscle2.joint).GetComponent<Rigidbody>() == (Object)(object)val)
				{
					((Component)val).transform.position = muscle2.target.position;
				}
			}
		}
		for (int m = 0; m < componentsInChildren.Length; m++)
		{
			if (((Component)componentsInChildren[m]).transform.childCount == 1)
			{
				Vector3 position = ((Component)componentsInChildren[m]).transform.GetChild(0).position;
				((Component)componentsInChildren[m]).transform.rotation = Quaternion.FromToRotation(((Component)componentsInChildren[m]).transform.rotation * array[m], position - ((Component)componentsInChildren[m]).transform.position) * ((Component)componentsInChildren[m]).transform.rotation;
				((Component)componentsInChildren[m]).transform.GetChild(0).position = position;
			}
		}
	}

	public static void RealignRagdoll(PuppetMaster puppetMaster)
	{
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		Muscle[] muscles = puppetMaster.muscles;
		foreach (Muscle muscle in muscles)
		{
			if ((Object)(object)muscle.joint == (Object)null || (Object)(object)((Component)muscle.joint).transform == (Object)null || (Object)(object)muscle.target == (Object)null)
			{
				return;
			}
		}
		Muscle[] muscles2 = puppetMaster.muscles;
		foreach (Muscle muscle2 in muscles2)
		{
			if ((Object)(object)muscle2.target != (Object)null)
			{
				Transform[] array = (Transform[])(object)new Transform[((Component)muscle2.joint).transform.childCount];
				for (int k = 0; k < array.Length; k++)
				{
					array[k] = ((Component)muscle2.joint).transform.GetChild(k);
				}
				Transform[] array2 = array;
				foreach (Transform val in array2)
				{
					val.parent = null;
				}
				BoxCollider component = ((Component)muscle2.joint).GetComponent<BoxCollider>();
				Vector3 val2 = Vector3.zero;
				Vector3 val3 = Vector3.zero;
				if ((Object)(object)component != (Object)null)
				{
					val2 = ((Component)component).transform.TransformVector(component.size);
					val3 = ((Component)component).transform.TransformVector(component.center);
				}
				CapsuleCollider component2 = ((Component)muscle2.joint).GetComponent<CapsuleCollider>();
				Vector3 val4 = Vector3.zero;
				Vector3 val5 = Vector3.zero;
				if ((Object)(object)component2 != (Object)null)
				{
					val4 = ((Component)component2).transform.TransformVector(component2.center);
					val5 = ((Component)component2).transform.TransformVector(DirectionIntToVector3(component2.direction));
				}
				SphereCollider component3 = ((Component)muscle2.joint).GetComponent<SphereCollider>();
				Vector3 val6 = Vector3.zero;
				if ((Object)(object)component3 != (Object)null)
				{
					val6 = ((Component)component3).transform.TransformVector(component3.center);
				}
				Vector3 val7 = ((Component)muscle2.joint).transform.TransformVector(((Joint)muscle2.joint).axis);
				Vector3 val8 = ((Component)muscle2.joint).transform.TransformVector(muscle2.joint.secondaryAxis);
				((Component)muscle2.joint).transform.rotation = muscle2.target.rotation;
				if ((Object)(object)component != (Object)null)
				{
					component.size = ((Component)component).transform.InverseTransformVector(val2);
					component.center = ((Component)component).transform.InverseTransformVector(val3);
				}
				if ((Object)(object)component2 != (Object)null)
				{
					component2.center = ((Component)component2).transform.InverseTransformVector(val4);
					Vector3 dir = ((Component)component2).transform.InverseTransformDirection(val5);
					component2.direction = DirectionVector3ToInt(dir);
				}
				if ((Object)(object)component3 != (Object)null)
				{
					component3.center = ((Component)component3).transform.InverseTransformVector(val6);
				}
				((Joint)muscle2.joint).axis = ((Component)muscle2.joint).transform.InverseTransformVector(val7);
				muscle2.joint.secondaryAxis = ((Component)muscle2.joint).transform.InverseTransformVector(val8);
				Transform[] array3 = array;
				foreach (Transform val9 in array3)
				{
					val9.parent = ((Component)muscle2.joint).transform;
				}
			}
		}
	}

	private static Vector3 DirectionIntToVector3(int dir)
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

	private static int DirectionVector3ToInt(Vector3 dir)
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
}
