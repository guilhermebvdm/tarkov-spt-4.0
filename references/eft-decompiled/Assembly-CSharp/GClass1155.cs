using System;
using Audio.Effects;
using UnityEngine;

public class GClass1155
{
	[NonSerialized]
	public const float Float_0 = 20f;

	[NonSerialized]
	public const float Float_1 = 20000f;

	[NonSerialized]
	public GClass1156 Gclass1156_0;

	[NonSerialized]
	public LoggerClass LoggerClass;

	public GClass1155(GClass1156 floorHeightCalculator, LoggerClass logger)
	{
		Gclass1156_0 = floorHeightCalculator;
		LoggerClass = logger;
	}

	public void Compute(AudioOcclusionEQPreset preset, float occlusion, GStruct89 influencedWeights, float heightDistance, bool isPropagationActive, float floorHeightSetting, float isolationFactor, bool isInDiffEvn, out float lowPassFreq, out float highPassFreq)
	{
		AudioOcclusionEQPreset.AudioFilterSettings hpfSettings = preset.hpfSettings;
		AudioOcclusionEQPreset.AudioFilterSettings lpfSettings = preset.lpfSettings;
		float time = Gclass1156_0.Compute(heightDistance, floorHeightSetting, LoggerClass);
		float num = (isPropagationActive ? lpfSettings.heightCurve.Evaluate(time) : 1f);
		float num2 = (isPropagationActive ? hpfSettings.heightCurve.Evaluate(time) : 1f);
		float num3 = (isPropagationActive ? occlusion : GClass2313.EaseInInterpolation(occlusion));
		if (isPropagationActive)
		{
			num = Mathf.Lerp(1f, num, GClass2313.LogarithmicInterpolation(num3));
			num2 = Mathf.Lerp(1f, num2, GClass2313.LogarithmicInterpolation(num3));
		}
		float time2 = num3 * num2;
		float time3 = num3 * num;
		float a = hpfSettings.frequencyCurve.Evaluate(time2);
		float a2 = lpfSettings.frequencyCurve.Evaluate(time3);
		AudioOcclusionEQPreset.PositionEQThresholds positionEqThresholds = hpfSettings.positionEqThresholds;
		AudioOcclusionEQPreset.PositionEQThresholds positionEqThresholds2 = lpfSettings.positionEqThresholds;
		float num4 = Mathf.Min(a, positionEqThresholds.levelFreq);
		float num5 = Mathf.Min(a, positionEqThresholds.aboveFreq);
		float num6 = Mathf.Min(a, positionEqThresholds.belowFreq);
		float num7 = Mathf.Min(a, positionEqThresholds.behindFreq);
		float num8 = Mathf.Max(a2, positionEqThresholds2.levelFreq);
		float num9 = Mathf.Max(a2, positionEqThresholds2.aboveFreq);
		float num10 = Mathf.Max(a2, positionEqThresholds2.belowFreq);
		float num11 = Mathf.Max(a2, positionEqThresholds2.behindFreq);
		float num12 = influencedWeights.Front * num4 + influencedWeights.Up * num5 + influencedWeights.Down * num6 + influencedWeights.Back * num7;
		float num13 = influencedWeights.Front * num8 + influencedWeights.Up * num9 + influencedWeights.Down * num10 + influencedWeights.Back * num11;
		if (isPropagationActive)
		{
			float b = (isInDiffEvn ? hpfSettings.environmentEqThresholds.diffEnvironmentIsolated : hpfSettings.environmentEqThresholds.indoorIsolated);
			float b2 = (isInDiffEvn ? lpfSettings.environmentEqThresholds.diffEnvironmentIsolated : lpfSettings.environmentEqThresholds.indoorIsolated);
			num12 = Mathf.Lerp(num12, b, isolationFactor);
			num13 = Mathf.Lerp(num13, b2, isolationFactor);
		}
		lowPassFreq = Mathf.Clamp(num13, 20f, 20000f);
		highPassFreq = Mathf.Clamp(num12, 20f, 20000f);
		if (highPassFreq > lowPassFreq)
		{
			LoggerClass.LogInfo($"FilterCutoffCalculator: HPF ({highPassFreq:F2}) > LPF ({lowPassFreq:F2}), correct");
			highPassFreq = lowPassFreq;
		}
	}
}
