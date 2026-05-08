using System;
using Audio.SpatialSystem;
using UnityEngine;

public class GClass1132 : GClass1130
{
	[NonSerialized]
	public float Float_2;

	public GClass1132(SpatialAudioSystem audioSystem)
		: base(audioSystem)
	{
	}

	public override float GetScore()
	{
		return Float_2;
	}

	public override float CalculateScore(Vector3 listenerPosition, SourceContainerClass sourceContainer)
	{
		Vector3 currentPosition = sourceContainer.CurrentPosition;
		if (Vector3.SqrMagnitude(currentPosition - listenerPosition) > sourceContainer.SqrMaxDistance)
		{
			return 1f;
		}
		Float_2 = 0f;
		Vector3 end = GClass1130.smethod_0(listenerPosition, currentPosition, 1f, positive: true);
		Vector3 end2 = GClass1130.smethod_0(listenerPosition, currentPosition, 1f, positive: false);
		Vector3 end3 = GClass1130.smethod_0(currentPosition, listenerPosition, 1f, positive: true);
		Vector3 end4 = GClass1130.smethod_0(currentPosition, listenerPosition, 1f, positive: false);
		Float_2 += method_1(currentPosition, listenerPosition);
		Float_2 += method_1(currentPosition, end);
		Float_2 += method_1(currentPosition, end2);
		Float_2 += method_1(listenerPosition, end3);
		Float_2 += method_1(listenerPosition, end4);
		method_0();
		return Float_2 / 5f;
	}

	public float method_1(Vector3 origin, Vector3 end)
	{
		if (!GClass1162.SimpleOcclusionTest(origin, end, GClass1130.Int32_0))
		{
			return 0f;
		}
		return 1f;
	}

	public override void OnDispose()
	{
		Float_2 = 0f;
		base.OnDispose();
	}
}
