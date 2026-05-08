using System;
using UnityEngine;

[Serializable]
public class GlitchUtilities
{
	public Shader Shader;

	public Texture2D DisplacementMap;

	[Header("Digital Mistake")]
	[Range(0f, 1f)]
	public float DigitalMistakeIntensity;

	[Range(0f, 1f)]
	public float DigitalMistakeFrequency = 1f;

	[Header("Vertical Flip")]
	[Range(0f, 1f)]
	public float VerticalFlipIntensity;

	[Range(0f, 1f)]
	public float VerticalFlipFrequency = 1f;

	[Header("Color Switch")]
	[Range(0f, 1f)]
	public float ColorSwitchIntensity;

	[Range(0f, 1f)]
	public float ColorSwitchFrequency = 1f;

	[Header("Vertical Jump")]
	[Range(0f, 1f)]
	public float VerticalJumpCoef;

	[Range(0f, 1f)]
	public float VerticalJumpFrequency = 1f;

	public float VerticalJumpTimeScale = 11.3f;

	[Header("Scan Line Jitter")]
	[Range(0f, 1f)]
	public float ScanLineJitterCoef;

	[Range(0f, 1f)]
	public float ScanLineJitterFrequency = 1f;

	[Header("Horizontal Shake")]
	[Range(0f, 1f)]
	public float HorizontalShake;

	[Range(0f, 1f)]
	public float HorizontalShakeFrequency = 1f;

	[Header("Color Drift")]
	[Range(0f, 1f)]
	public float ColorDriftCoef;

	[Range(0f, 1f)]
	public float ColorDriftFrequency = 1f;

	public float MaxColorDrift = 0.06f;
}
