using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion;

[Serializable]
public class BakerMuscle
{
	public AnimationCurve curve;

	private int muscleIndex = -1;

	private string propertyName;

	public BakerMuscle(int muscleIndex)
	{
		this.muscleIndex = muscleIndex;
		propertyName = MuscleNameToPropertyName(HumanTrait.MuscleName[muscleIndex]);
		Reset();
	}

	private string MuscleNameToPropertyName(string n)
	{
		return n switch
		{
			"Left Index 1 Stretched" => "LeftHand.Index.1 Stretched", 
			"Left Index 2 Stretched" => "LeftHand.Index.2 Stretched", 
			"Left Index 3 Stretched" => "LeftHand.Index.3 Stretched", 
			"Left Middle 1 Stretched" => "LeftHand.Middle.1 Stretched", 
			"Left Middle 2 Stretched" => "LeftHand.Middle.2 Stretched", 
			"Left Middle 3 Stretched" => "LeftHand.Middle.3 Stretched", 
			"Left Ring 1 Stretched" => "LeftHand.Ring.1 Stretched", 
			"Left Ring 2 Stretched" => "LeftHand.Ring.2 Stretched", 
			"Left Ring 3 Stretched" => "LeftHand.Ring.3 Stretched", 
			"Left Little 1 Stretched" => "LeftHand.Little.1 Stretched", 
			"Left Little 2 Stretched" => "LeftHand.Little.2 Stretched", 
			"Left Little 3 Stretched" => "LeftHand.Little.3 Stretched", 
			"Left Thumb 1 Stretched" => "LeftHand.Thumb.1 Stretched", 
			"Left Thumb 2 Stretched" => "LeftHand.Thumb.2 Stretched", 
			"Left Thumb 3 Stretched" => "LeftHand.Thumb.3 Stretched", 
			"Left Index Spread" => "LeftHand.Index.Spread", 
			"Left Middle Spread" => "LeftHand.Middle.Spread", 
			"Left Ring Spread" => "LeftHand.Ring.Spread", 
			"Left Little Spread" => "LeftHand.Little.Spread", 
			"Left Thumb Spread" => "LeftHand.Thumb.Spread", 
			"Right Index 1 Stretched" => "RightHand.Index.1 Stretched", 
			"Right Index 2 Stretched" => "RightHand.Index.2 Stretched", 
			"Right Index 3 Stretched" => "RightHand.Index.3 Stretched", 
			"Right Middle 1 Stretched" => "RightHand.Middle.1 Stretched", 
			"Right Middle 2 Stretched" => "RightHand.Middle.2 Stretched", 
			"Right Middle 3 Stretched" => "RightHand.Middle.3 Stretched", 
			"Right Ring 1 Stretched" => "RightHand.Ring.1 Stretched", 
			"Right Ring 2 Stretched" => "RightHand.Ring.2 Stretched", 
			"Right Ring 3 Stretched" => "RightHand.Ring.3 Stretched", 
			"Right Little 1 Stretched" => "RightHand.Little.1 Stretched", 
			"Right Little 2 Stretched" => "RightHand.Little.2 Stretched", 
			"Right Little 3 Stretched" => "RightHand.Little.3 Stretched", 
			"Right Thumb 1 Stretched" => "RightHand.Thumb.1 Stretched", 
			"Right Thumb 2 Stretched" => "RightHand.Thumb.2 Stretched", 
			"Right Thumb 3 Stretched" => "RightHand.Thumb.3 Stretched", 
			"Right Index Spread" => "RightHand.Index.Spread", 
			"Right Middle Spread" => "RightHand.Middle.Spread", 
			"Right Ring Spread" => "RightHand.Ring.Spread", 
			"Right Little Spread" => "RightHand.Little.Spread", 
			"Right Thumb Spread" => "RightHand.Thumb.Spread", 
			_ => n, 
		};
	}

	public void MultiplyLength(AnimationCurve curve, float mlp)
	{
		Keyframe[] keys = curve.keys;
		for (int i = 0; i < keys.Length; i++)
		{
			ref Keyframe reference = ref keys[i];
			reference.time = reference.time * mlp;
		}
		curve.keys = keys;
	}

	public void SetCurves(ref AnimationClip clip, float maxError, float lengthMlp)
	{
		MultiplyLength(curve, lengthMlp);
		BakerUtilities.ReduceKeyframes(curve, maxError);
		clip.SetCurve(string.Empty, typeof(Animator), propertyName, curve);
	}

	public void Reset()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		curve = new AnimationCurve();
	}

	public void SetKeyframe(float time, float[] muscles)
	{
		curve.AddKey(time, muscles[muscleIndex]);
	}

	public void SetLoopFrame(float time)
	{
		BakerUtilities.SetLoopFrame(time, curve);
	}
}
