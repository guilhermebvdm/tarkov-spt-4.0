using UnityEngine;
using Object = UnityEngine.Object;

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
		Keyframe[] array = curve.keys;
		int num = 1;
		while (num < array.Length - 1 && array.Length > 2)
		{
			Keyframe[] array2 = new Keyframe[array.Length - 1];
			int num2 = 0;
			for (int i = 0; i < array.Length; i++)
			{
				if (num != i)
				{
					array2[num2] = new Keyframe(array[i].time, array[i].value, array[i].inTangent, array[i].outTangent);
					num2++;
				}
			}
			AnimationCurve val = new AnimationCurve();
			val.keys = array2;
			float num3 = Mathf.Abs(val.Evaluate(array[num].time) - array[num].value);
			float num4 = array[num].time + (array[num - 1].time - array[num].time) * 0.5f;
			float num5 = array[num].time + (array[num + 1].time - array[num].time) * 0.5f;
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
		int last = keys.Length - 1;
		keys[last].value = keys[0].value;
		float inTangent = Mathf.Lerp(keys[0].inTangent, keys[last].inTangent, 0.5f);
		keys[0].inTangent = inTangent;
		keys[last].inTangent = inTangent;
		float outTangent = Mathf.Lerp(keys[0].outTangent, keys[last].outTangent, 0.5f);
		keys[0].outTangent = outTangent;
		keys[last].outTangent = outTangent;
		keys[last].time = time;
		curve.keys = keys;
	}

	public static void SetTangentMode(AnimationCurve curve)
	{
	}

	public static Quaternion EnsureQuaternionContinuity(Quaternion lastQ, Quaternion q)
	{
		Quaternion val = new Quaternion(-q.x, -q.y, -q.z, -q.w);
		Quaternion val2 = new Quaternion(Mathf.Lerp(lastQ.x, q.x, 0.5f), Mathf.Lerp(lastQ.y, q.y, 0.5f), Mathf.Lerp(lastQ.z, q.z, 0.5f), Mathf.Lerp(lastQ.w, q.w, 0.5f));
		Quaternion val3 = new Quaternion(Mathf.Lerp(lastQ.x, val.x, 0.5f), Mathf.Lerp(lastQ.y, val.y, 0.5f), Mathf.Lerp(lastQ.z, val.z, 0.5f), Mathf.Lerp(lastQ.w, val.w, 0.5f));
		float num = Quaternion.Angle(lastQ, val2);
		float num2 = Quaternion.Angle(lastQ, val3);
		return (num2 < num) ? val : q;
	}
}
