using System;
using UnityEngine;

namespace MultiFlare;

[Serializable]
public class BatchSettings
{
	[NonSerialized]
	public float AlphaMultiplier_1 = 1f;

	[NonSerialized]
	public float SizeMultiplier_1 = 1f;

	[SerializeField]
	public int _capacity = 300;

	[SerializeField]
	public Material _material;

	[SerializeField]
	[Range(0f, 1f)]
	public float _blindProtectionAlphaFactor = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	public float _blindProtectionSizeFactor = 1f;

	public int Capacity => _capacity;

	public Material Material => _material;

	public float AlphaMultiplier
	{
		get
		{
			if (!IsNvEnabled)
			{
				return AlphaMultiplier_1;
			}
			return 1f;
		}
	}

	public float SizeMultiplier
	{
		get
		{
			if (!IsNvEnabled)
			{
				return SizeMultiplier_1;
			}
			return 1f;
		}
	}

	[field: NonSerialized]
	public bool IsNvEnabled { get; set; }

	public void SetBlindnessProtectionFactor(float value)
	{
		value = Mathf.Clamp01(value);
		AlphaMultiplier_1 = 1f - _blindProtectionAlphaFactor * value;
		SizeMultiplier_1 = 1f - _blindProtectionSizeFactor * value;
	}
}
