using System;
using UnityEngine;

namespace MultiFlare;

[Serializable]
public class FlareSettings
{
	[SerializeField]
	[GAttribute27]
	public ProFlareAtlas _atlas;

	[SerializeField]
	public BatchSettings _normalBatch;

	[SerializeField]
	public BatchSettings _shitBatch;

	[SerializeField]
	public FlareOverlapSettings _flareOverlapSettings;

	public BatchSettings NormalBatch => _normalBatch;

	public BatchSettings ShitBatch => _shitBatch;

	public FlareOverlapSettings FlareOverlapSettings => _flareOverlapSettings;

	public ProFlareAtlas Atlas => _atlas;

	public bool IsPopulated
	{
		get
		{
			if ((bool)_atlas && (bool)_normalBatch.Material && (bool)_shitBatch.Material)
			{
				return _flareOverlapSettings.Material;
			}
			return false;
		}
	}

	public bool IsNvEnabled
	{
		set
		{
			_normalBatch.IsNvEnabled = value;
			_shitBatch.IsNvEnabled = value;
			_flareOverlapSettings.IsNvEnabled = value;
		}
	}

	public void SetBlindnessProtectionFactor(float value)
	{
		_normalBatch.SetBlindnessProtectionFactor(value);
		_shitBatch.SetBlindnessProtectionFactor(value);
		_flareOverlapSettings.SetBlindnessProtectionFactor(value);
	}
}
