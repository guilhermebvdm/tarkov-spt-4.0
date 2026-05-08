using System;
using Audio.AmbientSubsystem.PathMoverStrategy;
using UnityEngine;

public class GClass1193 : GClass1190
{
	public override EMovementStrategy MovementStrategy => EMovementStrategy.Random;

	public GClass1193(MonoBehaviour behaviour, GInterface110 posTranslator, Vector3[] points, float[] segmentLengths, float totalLength)
		: base(behaviour, posTranslator, points, segmentLengths, totalLength)
	{
	}

	public override void OnExecute(Action onDone = null)
	{
		Vector3 position = method_1();
		Ginterface110_0.Translate(position);
		onDone?.Invoke();
	}

	public override void ResetParameters(bool resetPosition = false)
	{
		Vector3 position = Vector3_0[0];
		Ginterface110_0.Translate(position);
	}
}
