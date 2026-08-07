using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion;

[Serializable]
public class BakerTransform
{
	public Transform transform;

	public AnimationCurve posX;

	public AnimationCurve posY;

	public AnimationCurve posZ;

	public AnimationCurve rotX;

	public AnimationCurve rotY;

	public AnimationCurve rotZ;

	public AnimationCurve rotW;

	private string relativePath;

	private bool recordPosition;

	private Vector3 relativePosition;

	private bool isRootNode;

	private Quaternion relativeRotation;

	public BakerTransform(Transform transform, Transform root, bool recordPosition, bool isRootNode)
	{
		this.transform = transform;
		this.recordPosition = recordPosition || isRootNode;
		this.isRootNode = isRootNode;
		relativePath = string.Empty;
		Reset();
	}

	public void SetRelativeSpace(Vector3 position, Quaternion rotation)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		relativePosition = position;
		relativeRotation = rotation;
	}

	public void SetCurves(ref AnimationClip clip)
	{
		if (recordPosition)
		{
			clip.SetCurve(relativePath, typeof(Transform), "localPosition.x", posX);
			clip.SetCurve(relativePath, typeof(Transform), "localPosition.y", posY);
			clip.SetCurve(relativePath, typeof(Transform), "localPosition.z", posZ);
		}
		clip.SetCurve(relativePath, typeof(Transform), "localRotation.x", rotX);
		clip.SetCurve(relativePath, typeof(Transform), "localRotation.y", rotY);
		clip.SetCurve(relativePath, typeof(Transform), "localRotation.z", rotZ);
		clip.SetCurve(relativePath, typeof(Transform), "localRotation.w", rotW);
		if (isRootNode)
		{
			AddRootMotionCurves(ref clip);
		}
		clip.EnsureQuaternionContinuity();
	}

	private void AddRootMotionCurves(ref AnimationClip clip)
	{
		if (recordPosition)
		{
			clip.SetCurve("", typeof(Animator), "MotionT.x", posX);
			clip.SetCurve("", typeof(Animator), "MotionT.y", posY);
			clip.SetCurve("", typeof(Animator), "MotionT.z", posZ);
		}
		clip.SetCurve("", typeof(Animator), "MotionQ.x", rotX);
		clip.SetCurve("", typeof(Animator), "MotionQ.y", rotY);
		clip.SetCurve("", typeof(Animator), "MotionQ.z", rotZ);
		clip.SetCurve("", typeof(Animator), "MotionQ.w", rotW);
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
		posX = new AnimationCurve();
		posY = new AnimationCurve();
		posZ = new AnimationCurve();
		rotX = new AnimationCurve();
		rotY = new AnimationCurve();
		rotZ = new AnimationCurve();
		rotW = new AnimationCurve();
	}

	public void ReduceKeyframes(float maxError)
	{
		BakerUtilities.ReduceKeyframes(rotX, maxError);
		BakerUtilities.ReduceKeyframes(rotY, maxError);
		BakerUtilities.ReduceKeyframes(rotZ, maxError);
		BakerUtilities.ReduceKeyframes(rotW, maxError);
		BakerUtilities.ReduceKeyframes(posX, maxError);
		BakerUtilities.ReduceKeyframes(posY, maxError);
		BakerUtilities.ReduceKeyframes(posZ, maxError);
	}

	public void SetKeyframes(float time)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (recordPosition)
		{
			Vector3 val = transform.localPosition;
			if (isRootNode)
			{
				val = transform.position - relativePosition;
			}
			posX.AddKey(time, val.x);
			posY.AddKey(time, val.y);
			posZ.AddKey(time, val.z);
		}
		Quaternion val2 = transform.localRotation;
		if (isRootNode)
		{
			val2 = Quaternion.Inverse(relativeRotation) * transform.rotation;
		}
		rotX.AddKey(time, val2.x);
		rotY.AddKey(time, val2.y);
		rotZ.AddKey(time, val2.z);
		rotW.AddKey(time, val2.w);
	}

	public void AddLoopFrame(float time)
	{
		if (recordPosition && !isRootNode)
		{
			posX.AddKey(time, posX.keys[0].value);
			posY.AddKey(time, posY.keys[0].value);
			posZ.AddKey(time, posZ.keys[0].value);
		}
		rotX.AddKey(time, rotX.keys[0].value);
		rotY.AddKey(time, rotY.keys[0].value);
		rotZ.AddKey(time, rotZ.keys[0].value);
		rotW.AddKey(time, rotW.keys[0].value);
	}
}
