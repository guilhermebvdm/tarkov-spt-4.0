using UnityEngine;

namespace VisceralCombat.Ragdolls.Classes.RootMotion;

public static class BakerUtilities
{
	public static void ReduceKeyframes(AnimationCurve curve, float maxError)
	{
		if (!(maxError <= 0f))
		{
			curve.keys = GetReducedKeyframes(curve, maxError);
		}
	}

	public static Keyframe[] GetReducedKeyframes(AnimationCurve curve, float maxError)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		Keyframe[] array = curve.keys;
		int num = 1;
		while (num < array.Length - 1 && array.Length > 2)
		{
			Keyframe[] array2 = (Keyframe[])(object)new Keyframe[array.Length - 1];
			int num2 = 0;
			for (int i = 0; i < array.Length; i++)
			{
				if (num != i)
				{
					array2[num2] = new Keyframe(((Keyframe)(ref array[i])).time, ((Keyframe)(ref array[i])).value, ((Keyframe)(ref array[i])).inTangent, ((Keyframe)(ref array[i])).outTangent);
					num2++;
				}
			}
			AnimationCurve val = new AnimationCurve();
			val.keys = array2;
			float num3 = Mathf.Abs(val.Evaluate(((Keyframe)(ref array[num])).time) - ((Keyframe)(ref array[num])).value);
			float num4 = ((Keyframe)(ref array[num])).time + (((Keyframe)(ref array[num - 1])).time - ((Keyframe)(ref array[num])).time) * 0.5f;
			float num5 = ((Keyframe)(ref array[num])).time + (((Keyframe)(ref array[num + 1])).time - ((Keyframe)(ref array[num])).time) * 0.5f;
			float num6 = Mathf.Abs(val.Evaluate(num4) - curve.Evaluate(num4));
			float num7 = Mathf.Abs(val.Evaluate(num5) - curve.Evaluate(num5));
			if (num3 < maxError && num6 < maxError && num7 < maxError)
			{
				array = array2;
			}
			else
			{
				num++;
			}
		}
		return array;
	}

	public static void SetLoopFrame(float time, AnimationCurve curve)
	{
		Keyframe[] keys = curve.keys;
		((Keyframe)(ref keys[^1])).value = ((Keyframe)(ref keys[0])).value;
		float inTangent = Mathf.Lerp(((Keyframe)(ref keys[0])).inTangent, ((Keyframe)(ref keys[^1])).inTangent, 0.5f);
		((Keyframe)(ref keys[0])).inTangent = inTangent;
		((Keyframe)(ref keys[^1])).inTangent = inTangent;
		float outTangent = Mathf.Lerp(((Keyframe)(ref keys[0])).outTangent, ((Keyframe)(ref keys[^1])).outTangent, 0.5f);
		((Keyframe)(ref keys[0])).outTangent = outTangent;
		((Keyframe)(ref keys[^1])).outTangent = outTangent;
		((Keyframe)(ref keys[^1])).time = time;
		curve.keys = keys;
	}

	public static void SetTangentMode(AnimationCurve curve)
	{
	}

	public static Quaternion EnsureQuaternionContinuity(Quaternion lastQ, Quaternion q)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = default(Quaternion);
		((Quaternion)(ref val))._002Ector(0f - q.x, 0f - q.y, 0f - q.z, 0f - q.w);
		Quaternion val2 = default(Quaternion);
		((Quaternion)(ref val2))._002Ector(Mathf.Lerp(lastQ.x, q.x, 0.5f), Mathf.Lerp(lastQ.y, q.y, 0.5f), Mathf.Lerp(lastQ.z, q.z, 0.5f), Mathf.Lerp(lastQ.w, q.w, 0.5f));
		Quaternion val3 = default(Quaternion);
		((Quaternion)(ref val3))._002Ector(Mathf.Lerp(lastQ.x, val.x, 0.5f), Mathf.Lerp(lastQ.y, val.y, 0.5f), Mathf.Lerp(lastQ.z, val.z, 0.5f), Mathf.Lerp(lastQ.w, val.w, 0.5f));
		float num = Quaternion.Angle(lastQ, val2);
		float num2 = Quaternion.Angle(lastQ, val3);
		return (num2 < num) ? val : q;
	}
}
