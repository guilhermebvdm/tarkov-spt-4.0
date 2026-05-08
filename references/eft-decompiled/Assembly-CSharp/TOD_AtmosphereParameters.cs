using System;
using UnityEngine;

[Serializable]
public class TOD_AtmosphereParameters
{
	[Tooltip("Intensity of the atmospheric Rayleigh scattering.")]
	[GAttribute0(0f)]
	public float RayleighMultiplier = 1f;

	[Tooltip("Intensity of the atmospheric Mie scattering.")]
	[GAttribute0(0f)]
	public float MieMultiplier = 1f;

	[Tooltip("Overall brightness of the atmosphere.")]
	[GAttribute0(0f)]
	public float Brightness = 1.5f;

	[Tooltip("Scattering brightness of the atmosphere.")]
	[GAttribute0(0f)]
	public float ScatteringBrightness = 1.5f;

	[Tooltip("Overall contrast of the atmosphere.")]
	[GAttribute0(0f)]
	public float Contrast = 1.5f;

	[Tooltip("Directionality factor that determines the size of the glow around the sun.")]
	[GAttribute2(0f, 1f)]
	public float Directionality = 0.7f;

	[Tooltip("Density of the fog covering the sky.")]
	[GAttribute2(0f, 1f)]
	public float Fogginess;
}
