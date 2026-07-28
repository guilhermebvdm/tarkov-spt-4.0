using System;
using UnityEngine;

namespace VisceralCombat.Ragdolls.Classes.RootMotion;

[Serializable]
public class BakerHumanoidQT
{
	private Transform transform;

	private string Qx;

	private string Qy;

	private string Qz;

	private string Qw;

	private string Tx;

	private string Ty;

	private string Tz;

	public AnimationCurve rotX;

	public AnimationCurve rotY;

	public AnimationCurve rotZ;

	public AnimationCurve rotW;

	public AnimationCurve posX;

	public AnimationCurve posY;

	public AnimationCurve posZ;

	private AvatarIKGoal goal;

	private Quaternion lastQ;

	private bool lastQSet;

	public BakerHumanoidQT(string name)
	{
		Qx = name + "Q.x";
		Qy = name + "Q.y";
		Qz = name + "Q.z";
		Qw = name + "Q.w";
		Tx = name + "T.x";
		Ty = name + "T.y";
		Tz = name + "T.z";
		Reset();
	}

	public BakerHumanoidQT(Transform transform, AvatarIKGoal goal, string name)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		this.transform = transform;
		this.goal = goal;
		Qx = name + "Q.x";
		Qy = name + "Q.y";
		Qz = name + "Q.z";
		Qw = name + "Q.w";
		Tx = name + "T.x";
		Ty = name + "T.y";
		Tz = name + "T.z";
		Reset();
	}

	public Quaternion EvaluateRotation(float time)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		Quaternion result = default(Quaternion);
		((Quaternion)(ref result))._002Ector(rotX.Evaluate(time), rotY.Evaluate(time), rotZ.Evaluate(time), rotW.Evaluate(time));
		return result;
	}

	public Vector3 EvaluatePosition(float time)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(posX.Evaluate(time), posY.Evaluate(time), posZ.Evaluate(time));
	}

	public TQ Evaluate(float time)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new TQ(EvaluatePosition(time), EvaluateRotation(time));
	}

	public void GetCurvesFromClip(AnimationClip clip, Animator animator)
	{
	}

	public void Reset()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		rotX = new AnimationCurve();
		rotY = new AnimationCurve();
		rotZ = new AnimationCurve();
		rotW = new AnimationCurve();
		posX = new AnimationCurve();
		posY = new AnimationCurve();
		posZ = new AnimationCurve();
		lastQ = Quaternion.identity;
		lastQSet = false;
	}

	public void SetIKKeyframes(float time, Avatar avatar, Transform root, float humanScale, Vector3 bodyPosition, Quaternion bodyRotation)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = transform.position;
		Quaternion val2 = transform.rotation;
		if ((Object)(object)root.parent != (Object)null)
		{
			val = root.parent.InverseTransformPoint(val);
			val2 = Quaternion.Inverse(root.parent.rotation) * val2;
		}
		TQ iKGoalTQ = AvatarUtility.GetIKGoalTQ(avatar, humanScale, goal, new TQ(bodyPosition, bodyRotation), new TQ(val, val2));
		Quaternion val3 = iKGoalTQ.q;
		if (lastQSet)
		{
			val3 = BakerUtilities.EnsureQuaternionContinuity(lastQ, iKGoalTQ.q);
		}
		lastQ = val3;
		lastQSet = true;
		rotX.AddKey(time, val3.x);
		rotY.AddKey(time, val3.y);
		rotZ.AddKey(time, val3.z);
		rotW.AddKey(time, val3.w);
		Vector3 t = iKGoalTQ.t;
		posX.AddKey(time, t.x);
		posY.AddKey(time, t.y);
		posZ.AddKey(time, t.z);
	}

	public void SetKeyframes(float time, Vector3 pos, Quaternion rot)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		rotX.AddKey(time, rot.x);
		rotY.AddKey(time, rot.y);
		rotZ.AddKey(time, rot.z);
		rotW.AddKey(time, rot.w);
		posX.AddKey(time, pos.x);
		posY.AddKey(time, pos.y);
		posZ.AddKey(time, pos.z);
	}

	public void MoveLastKeyframes(float time)
	{
		MoveLastKeyframe(time, rotX);
		MoveLastKeyframe(time, rotY);
		MoveLastKeyframe(time, rotZ);
		MoveLastKeyframe(time, rotW);
		MoveLastKeyframe(time, posX);
		MoveLastKeyframe(time, posY);
		MoveLastKeyframe(time, posZ);
	}

	public void SetLoopFrame(float time)
	{
		BakerUtilities.SetLoopFrame(time, rotX);
		BakerUtilities.SetLoopFrame(time, rotY);
		BakerUtilities.SetLoopFrame(time, rotZ);
		BakerUtilities.SetLoopFrame(time, rotW);
		BakerUtilities.SetLoopFrame(time, posX);
		BakerUtilities.SetLoopFrame(time, posY);
		BakerUtilities.SetLoopFrame(time, posZ);
	}

	public void SetRootLoopFrame(float time)
	{
	}

	private void MoveLastKeyframe(float time, AnimationCurve curve)
	{
		Keyframe[] keys = curve.keys;
		((Keyframe)(ref keys[^1])).time = time;
		curve.keys = keys;
	}

	public void MultiplyLength(AnimationCurve curve, float mlp)
	{
		Keyframe[] keys = curve.keys;
		for (int i = 0; i < keys.Length; i++)
		{
			ref Keyframe reference = ref keys[i];
			((Keyframe)(ref reference)).time = ((Keyframe)(ref reference)).time * mlp;
		}
		curve.keys = keys;
	}

	public void SetCurves(ref AnimationClip clip, float maxError, float lengthMlp)
	{
		MultiplyLength(rotX, lengthMlp);
		MultiplyLength(rotY, lengthMlp);
		MultiplyLength(rotZ, lengthMlp);
		MultiplyLength(rotW, lengthMlp);
		MultiplyLength(posX, lengthMlp);
		MultiplyLength(posY, lengthMlp);
		MultiplyLength(posZ, lengthMlp);
		BakerUtilities.ReduceKeyframes(rotX, maxError);
		BakerUtilities.ReduceKeyframes(rotY, maxError);
		BakerUtilities.ReduceKeyframes(rotZ, maxError);
		BakerUtilities.ReduceKeyframes(rotW, maxError);
		BakerUtilities.ReduceKeyframes(posX, maxError);
		BakerUtilities.ReduceKeyframes(posY, maxError);
		BakerUtilities.ReduceKeyframes(posZ, maxError);
		BakerUtilities.SetTangentMode(rotX);
		BakerUtilities.SetTangentMode(rotY);
		BakerUtilities.SetTangentMode(rotZ);
		BakerUtilities.SetTangentMode(rotW);
		clip.SetCurve(string.Empty, typeof(Animator), Qx, rotX);
		clip.SetCurve(string.Empty, typeof(Animator), Qy, rotY);
		clip.SetCurve(string.Empty, typeof(Animator), Qz, rotZ);
		clip.SetCurve(string.Empty, typeof(Animator), Qw, rotW);
		clip.SetCurve(string.Empty, typeof(Animator), Tx, posX);
		clip.SetCurve(string.Empty, typeof(Animator), Ty, posY);
		clip.SetCurve(string.Empty, typeof(Animator), Tz, posZ);
	}
}
