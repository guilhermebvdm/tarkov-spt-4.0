using System;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion;

public class AvatarUtility
{
	public static Quaternion GetPostRotation(Avatar avatar, AvatarIKGoal avatarIKGoal)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected I4, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)HumanIDFromAvatarIKGoal(avatarIKGoal);
		if (num == 55)
		{
			throw new InvalidOperationException("Invalid human id.");
		}
		MethodInfo method = typeof(Avatar).GetMethod("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
		if (method == null)
		{
			throw new InvalidOperationException("Cannot find GetPostRotation method.");
		}
		return (Quaternion)method.Invoke(avatar, new object[1] { num });
	}

	public static TQ GetIKGoalTQ(Avatar avatar, float humanScale, AvatarIKGoal avatarIKGoal, TQ bodyPositionRotation, TQ boneTQ)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected I4, but got Unknown
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Invalid comparison between Unknown and I4
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)HumanIDFromAvatarIKGoal(avatarIKGoal);
		if (num == 55)
		{
			throw new InvalidOperationException("Invalid human id.");
		}
		MethodInfo method = typeof(Avatar).GetMethod("GetAxisLength", BindingFlags.Instance | BindingFlags.NonPublic);
		if (method == null)
		{
			throw new InvalidOperationException("Cannot find GetAxisLength method.");
		}
		MethodInfo method2 = typeof(Avatar).GetMethod("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
		if (method2 == null)
		{
			throw new InvalidOperationException("Cannot find GetPostRotation method.");
		}
		Quaternion val = (Quaternion)method2.Invoke(avatar, new object[1] { num });
		TQ tQ = new TQ(boneTQ.t, boneTQ.q * val);
		if ((int)avatarIKGoal == 0 || (int)avatarIKGoal == 1)
		{
			float num2 = (float)method.Invoke(avatar, new object[1] { num });
			Vector3 val2 = new Vector3(num2, 0f, 0f);
			tQ.t += tQ.q * val2;
		}
		Quaternion val3 = Quaternion.Inverse(bodyPositionRotation.q);
		tQ.t = val3 * (tQ.t - bodyPositionRotation.t);
		tQ.q = val3 * tQ.q;
		tQ.t /= humanScale;
		tQ.q = Quaternion.LookRotation(tQ.q * Vector3.forward, tQ.q * Vector3.up);
		return tQ;
	}

	public static TQ WorldSpaceIKGoalToBone(TQ goalTQ, Avatar avatar, AvatarIKGoal avatarIKGoal)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected I4, but got Unknown
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Invalid comparison between Unknown and I4
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)HumanIDFromAvatarIKGoal(avatarIKGoal);
		if (num == 55)
		{
			throw new InvalidOperationException("Invalid human id.");
		}
		MethodInfo method = typeof(Avatar).GetMethod("GetAxisLength", BindingFlags.Instance | BindingFlags.NonPublic);
		if (method == null)
		{
			throw new InvalidOperationException("Cannot find GetAxisLength method.");
		}
		MethodInfo method2 = typeof(Avatar).GetMethod("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
		if (method2 == null)
		{
			throw new InvalidOperationException("Cannot find GetPostRotation method.");
		}
		Quaternion val = (Quaternion)method2.Invoke(avatar, new object[1] { num });
		if ((int)avatarIKGoal == 0 || (int)avatarIKGoal == 1)
		{
			float num2 = (float)method.Invoke(avatar, new object[1] { num });
			Vector3 val2 = new Vector3(num2, 0f, 0f);
			goalTQ.t -= goalTQ.q * val2;
		}
		return new TQ(goalTQ.t, goalTQ.q * Quaternion.Inverse(val));
	}

	public static TQ GetWorldSpaceIKGoal(BakerHumanoidQT ikQT, BakerHumanoidQT rootQT, float time, float humanScale)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		TQ tQ = ikQT.Evaluate(time);
		TQ tQ2 = rootQT.Evaluate(time);
		tQ.q = tQ2.q * tQ.q;
		tQ.t = tQ2.t + tQ2.q * tQ.t;
		tQ.t *= humanScale;
		return tQ;
	}

	public static HumanBodyBones HumanIDFromAvatarIKGoal(AvatarIKGoal avatarIKGoal)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected I4, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		return (HumanBodyBones)((int)avatarIKGoal switch
		{
			0 => 5, 
			1 => 6, 
			2 => 17, 
			3 => 18, 
			_ => 55, 
		});
	}
}
