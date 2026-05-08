using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace CW2.Animations;

[Serializable]
public class SmoothRandom
{
	[FormerlySerializedAs("Amlitude")]
	[Range(0f, 5f)]
	public float Amplitude = 0.1f;

	[GAttribute6(0f, 1f, -1f)]
	public Vector2 MinMaxDelay = new Vector2(0.3f, 0.6f);

	[Range(0f, 2f)]
	public float Hardness = 0.1f;

	[NonSerialized]
	public float CountDown;

	[NonSerialized]
	public float CurrentVal;

	[NonSerialized]
	public float Increase;

	public float GetValue(float deltaTime)
	{
		CountDown -= deltaTime;
		if (CountDown < 0f)
		{
			float num = (CountDown = UnityEngine.Random.Range(MinMaxDelay.x, MinMaxDelay.y));
			float num2 = (UnityEngine.Random.Range(0f - Amplitude, Amplitude) - CurrentVal) / num;
			Increase += (num2 - Increase) * Hardness;
		}
		CurrentVal += Increase * deltaTime;
		return CurrentVal;
	}
}
