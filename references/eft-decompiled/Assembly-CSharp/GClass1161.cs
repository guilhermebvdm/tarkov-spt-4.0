using System;
using Audio.SpatialSystem;
using UnityEngine;

public class GClass1161
{
	[NonSerialized]
	public const float Float_0 = 0.1f;

	public void Compute(Vector3 listenerPos, Vector3 sourcePos, AudioGroupOcclusionSettings settings, bool isPropagationActive, out float lowPassResonance, out float highPassResonance)
	{
		if (!isPropagationActive)
		{
			lowPassResonance = 1f;
			highPassResonance = 1f;
			return;
		}
		float num = GClass1162.CalculateVerticalDot(listenerPos, sourcePos);
		float num2 = Mathf.Abs(num);
		float t = Mathf.Clamp01((num2 - 0.1f) / 0.9f);
		float b = settings.propagationEQPreset.lpfSettings.resonanceCurve.Evaluate(num2);
		lowPassResonance = Mathf.Lerp(1f, b, t);
		if (num > 0.1f)
		{
			float b2 = settings.propagationEQPreset.hpfSettings.resonanceCurve.Evaluate(num2);
			highPassResonance = Mathf.Lerp(1f, b2, t);
		}
		else
		{
			highPassResonance = 1f;
		}
	}
}
