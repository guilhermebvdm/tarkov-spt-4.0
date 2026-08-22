using UnityEngine;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

public static class JointConverter
{
	public static void ToConfigurable(GameObject root)
	{
		int num = 0;
		CharacterJoint[] componentsInChildren = root.GetComponentsInChildren<CharacterJoint>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			CharacterToConfigurable(componentsInChildren[i]);
			num++;
		}
		HingeJoint[] componentsInChildren2 = root.GetComponentsInChildren<HingeJoint>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			HingeToConfigurable(componentsInChildren2[j]);
			num++;
		}
		FixedJoint[] componentsInChildren3 = root.GetComponentsInChildren<FixedJoint>();
		for (int k = 0; k < componentsInChildren3.Length; k++)
		{
			FixedToConfigurable(componentsInChildren3[k]);
			num++;
		}
		SpringJoint[] componentsInChildren4 = root.GetComponentsInChildren<SpringJoint>();
		for (int l = 0; l < componentsInChildren4.Length; l++)
		{
			SpringToConfigurable(componentsInChildren4[l]);
			num++;
		}
		if (num <= 0)
		{
		}
	}

	public static void HingeToConfigurable(HingeJoint src)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		ConfigurableJoint conf = ((Component)src).gameObject.AddComponent<ConfigurableJoint>();
		ConvertJoint(ref conf, (Joint)(object)src);
		conf.secondaryAxis = Vector3.zero;
		conf.xMotion = (ConfigurableJointMotion)0;
		conf.yMotion = (ConfigurableJointMotion)0;
		conf.zMotion = (ConfigurableJointMotion)0;
		conf.angularXMotion = (ConfigurableJointMotion)(src.useLimits ? 1 : 2);
		conf.angularYMotion = (ConfigurableJointMotion)0;
		conf.angularZMotion = (ConfigurableJointMotion)0;
		conf.highAngularXLimit = ConvertToHighSoftJointLimit(src.limits, src.spring, src.useSpring);
		conf.angularXLimitSpring = ConvertToSoftJointLimitSpring(src.limits, src.spring, src.useSpring);
		conf.lowAngularXLimit = ConvertToLowSoftJointLimit(src.limits, src.spring, src.useSpring);
		if (src.useMotor)
		{
		}
		Object.DestroyImmediate((Object)(object)src);
	}

	public static void FixedToConfigurable(FixedJoint src)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		ConfigurableJoint conf = ((Component)src).gameObject.AddComponent<ConfigurableJoint>();
		ConvertJoint(ref conf, (Joint)(object)src);
		conf.secondaryAxis = Vector3.zero;
		conf.xMotion = (ConfigurableJointMotion)0;
		conf.yMotion = (ConfigurableJointMotion)0;
		conf.zMotion = (ConfigurableJointMotion)0;
		conf.angularXMotion = (ConfigurableJointMotion)0;
		conf.angularYMotion = (ConfigurableJointMotion)0;
		conf.angularZMotion = (ConfigurableJointMotion)0;
		Object.DestroyImmediate((Object)(object)src);
	}

	public static void SpringToConfigurable(SpringJoint src)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		ConfigurableJoint conf = ((Component)src).gameObject.AddComponent<ConfigurableJoint>();
		ConvertJoint(ref conf, (Joint)(object)src);
		conf.xMotion = (ConfigurableJointMotion)1;
		conf.yMotion = (ConfigurableJointMotion)1;
		conf.zMotion = (ConfigurableJointMotion)1;
		conf.angularXMotion = (ConfigurableJointMotion)2;
		conf.angularYMotion = (ConfigurableJointMotion)2;
		conf.angularZMotion = (ConfigurableJointMotion)2;
		SoftJointLimit linearLimit = default(SoftJointLimit);
		linearLimit.bounciness = 0f;
		linearLimit.limit = src.maxDistance;
		conf.linearLimit = linearLimit;
		SoftJointLimitSpring linearLimitSpring = default(SoftJointLimitSpring);
		linearLimitSpring.damper = src.damper;
		linearLimitSpring.spring = src.spring;
		conf.linearLimitSpring = linearLimitSpring;
		Object.DestroyImmediate((Object)(object)src);
	}

	public static void CharacterToConfigurable(CharacterJoint src)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		ConfigurableJoint conf = ((Component)src).gameObject.AddComponent<ConfigurableJoint>();
		ConvertJoint(ref conf, (Joint)(object)src);
		conf.secondaryAxis = src.swingAxis;
		conf.xMotion = (ConfigurableJointMotion)0;
		conf.yMotion = (ConfigurableJointMotion)0;
		conf.zMotion = (ConfigurableJointMotion)0;
		conf.angularXMotion = (ConfigurableJointMotion)1;
		conf.angularYMotion = (ConfigurableJointMotion)1;
		conf.angularZMotion = (ConfigurableJointMotion)1;
		conf.highAngularXLimit = CopyLimit(src.highTwistLimit);
		conf.lowAngularXLimit = CopyLimit(src.lowTwistLimit);
		conf.angularYLimit = CopyLimit(src.swing1Limit);
		conf.angularZLimit = CopyLimit(src.swing2Limit);
		conf.angularXLimitSpring = CopyLimitSpring(src.twistLimitSpring);
		conf.angularYZLimitSpring = CopyLimitSpring(src.swingLimitSpring);
		((Joint)conf).enableCollision = ((Joint)src).enableCollision;
		conf.projectionMode = (JointProjectionMode)(src.enableProjection ? 1 : 0);
		conf.projectionAngle = src.projectionAngle;
		conf.projectionDistance = src.projectionDistance;
		Object.DestroyImmediate((Object)(object)src);
	}

	private static void ConvertJoint(ref ConfigurableJoint conf, Joint src)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		((Joint)conf).anchor = src.anchor;
		((Joint)conf).autoConfigureConnectedAnchor = src.autoConfigureConnectedAnchor;
		((Joint)conf).axis = src.axis;
		((Joint)conf).breakForce = src.breakForce;
		((Joint)conf).breakTorque = src.breakTorque;
		((Joint)conf).connectedAnchor = src.connectedAnchor;
		((Joint)conf).connectedBody = src.connectedBody;
		((Joint)conf).enableCollision = src.enableCollision;
	}

	private static SoftJointLimit ConvertToHighSoftJointLimit(JointLimits src, JointSpring spring, bool useSpring)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		SoftJointLimit result = default(SoftJointLimit);
		result.limit = 0f - src.max;
		result.bounciness = src.bounciness;
		return result;
	}

	private static SoftJointLimit ConvertToLowSoftJointLimit(JointLimits src, JointSpring spring, bool useSpring)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		SoftJointLimit result = default(SoftJointLimit);
		result.limit = 0f - src.min;
		result.bounciness = src.bounciness;
		return result;
	}

	private static SoftJointLimitSpring ConvertToSoftJointLimitSpring(JointLimits src, JointSpring spring, bool useSpring)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		SoftJointLimitSpring result = default(SoftJointLimitSpring);
		result.damper = (useSpring ? spring.damper : 0f);
		result.spring = (useSpring ? spring.spring : 0f);
		return result;
	}

	private static SoftJointLimit CopyLimit(SoftJointLimit src)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		SoftJointLimit result = default(SoftJointLimit);
		result.limit = src.limit;
		result.bounciness = src.bounciness;
		return result;
	}

	private static SoftJointLimitSpring CopyLimitSpring(SoftJointLimitSpring src)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		SoftJointLimitSpring result = default(SoftJointLimitSpring);
		result.damper = src.damper;
		result.spring = src.spring;
		return result;
	}
}
