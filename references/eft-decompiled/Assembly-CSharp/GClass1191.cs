using System;
using System.Collections;
using Audio.AmbientSubsystem.PathMoverStrategy;
using UnityEngine;

public class GClass1191 : GClass1190
{
	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public Coroutine Coroutine_0;

	[NonSerialized]
	public float Float_4;

	public override EMovementStrategy MovementStrategy => EMovementStrategy.Backward;

	public GClass1191(MonoBehaviour behaviour, GInterface110 posTranslator, Vector3[] points, float[] segmentLengths, float totalLength)
		: base(behaviour, posTranslator, points, segmentLengths, totalLength)
	{
		Float_3 = 1f;
	}

	public override void OnExecute(Action onDone = null)
	{
		method_5();
		Coroutine_0 = MonoBehaviour_0.StartCoroutine(method_4(onDone));
	}

	public IEnumerator method_4(Action onDone = null)
	{
		Vector3 position = Vector3_0[Vector3_0.Length - 1];
		Ginterface110_0.Translate(position);
		Float_4 = Time.realtimeSinceStartup;
		float num = 1f / Float_1;
		float num2 = 0f;
		while (num2 < num)
		{
			num2 = Time.realtimeSinceStartup - Float_4;
			Float_3 = num2 / num;
			Float_3 = 1f - Float_3;
			float arcLength = Float_3 * Float_0;
			Vector3 position2 = method_2(arcLength);
			Ginterface110_0.Translate(position2);
			yield return null;
		}
		Float_3 = 0f;
		Vector3 position3 = Vector3_0[0];
		Ginterface110_0.Translate(position3);
		onDone?.Invoke();
	}

	public override void ResetParameters(bool resetPosition = false)
	{
		method_5();
		Float_3 = 1f;
		if (resetPosition)
		{
			Vector3 position = Vector3_0[Vector3_0.Length - 1];
			Ginterface110_0.Translate(position);
		}
	}

	public void method_5()
	{
		if (Coroutine_0 != null && !(MonoBehaviour_0 == null))
		{
			MonoBehaviour_0.StopCoroutine(Coroutine_0);
			Coroutine_0 = null;
		}
	}
}
