using EFT;
using UnityEngine;

namespace Audio.SpatialSystem;

[RequireComponent(typeof(MetaXRAudioSource), typeof(MetaXRAudioSourceExperimentalFeatures))]
public class MetaSpatialAudioSource : BaseSpatialAudioSource
{
	[SerializeField]
	private MetaXRAudioSource _metaXRAudioSource;

	[SerializeField]
	private MetaXRAudioSourceExperimentalFeatures _metaXRAudioSourceExperimental;

	private bool bool_0 = true;

	public override bool EnableSpatialization
	{
		get
		{
			return _metaXRAudioSource.EnableSpatialization;
		}
		set
		{
			if (_metaXRAudioSource.EnableSpatialization != value)
			{
				_metaXRAudioSource.EnableSpatialization = value;
				base.ParentSource.spatialize = value;
			}
		}
	}

	public override float HrtfIntensity
	{
		get
		{
			return _metaXRAudioSourceExperimental.HrtfIntensity;
		}
		set
		{
			_metaXRAudioSourceExperimental.HrtfIntensity = value;
		}
	}

	public override float DirectivityIntensity
	{
		get
		{
			return _metaXRAudioSourceExperimental.DirectivityIntensity;
		}
		set
		{
			_metaXRAudioSourceExperimental.DirectivityIntensity = value;
		}
	}

	public override bool EnableReverb
	{
		get
		{
			return _metaXRAudioSource.EnableAcoustics;
		}
		set
		{
			_metaXRAudioSource.EnableAcoustics = value;
		}
	}

	public override bool EnableDirectSound
	{
		get
		{
			return _metaXRAudioSourceExperimental.DirectSoundEnabled;
		}
		set
		{
			_metaXRAudioSourceExperimental.DirectSoundEnabled = value;
		}
	}

	public override float ReverbSendDB
	{
		get
		{
			return _metaXRAudioSource.ReverbSendDb;
		}
		set
		{
			_metaXRAudioSource.ReverbSendDb = value;
		}
	}

	public override float EarlyReflectionsSendDB
	{
		get
		{
			return _metaXRAudioSourceExperimental.EarlyReflectionsSendDb;
		}
		set
		{
			_metaXRAudioSourceExperimental.EarlyReflectionsSendDb = value;
		}
	}

	public override float ReverbReach
	{
		get
		{
			return _metaXRAudioSourceExperimental.ReverbReach;
		}
		set
		{
			_metaXRAudioSourceExperimental.ReverbReach = value;
		}
	}

	public override float VolumetricRadius
	{
		get
		{
			return _metaXRAudioSourceExperimental.VolumetricRadius;
		}
		set
		{
			_metaXRAudioSourceExperimental.VolumetricRadius = value;
		}
	}

	public override bool DirectSoundEnabled
	{
		get
		{
			return _metaXRAudioSourceExperimental.DirectSoundEnabled;
		}
		set
		{
			_metaXRAudioSourceExperimental.DirectSoundEnabled = value;
		}
	}

	public override void UpdateParameters()
	{
		_metaXRAudioSource.UpdateParameters();
		_metaXRAudioSourceExperimental.UpdateParameters();
	}

	public override void Awake()
	{
		base.Awake();
		if ((object)_metaXRAudioSource == null)
		{
			_metaXRAudioSource = GetComponent<MetaXRAudioSource>();
		}
		if ((object)_metaXRAudioSourceExperimental == null)
		{
			_metaXRAudioSourceExperimental = GetComponent<MetaXRAudioSourceExperimentalFeatures>();
		}
		_metaXRAudioSource.Init();
		_metaXRAudioSourceExperimental.Init();
	}

	public override void SetActive(bool active)
	{
		if (bool_0 != active)
		{
			_metaXRAudioSource.enabled = active;
			_metaXRAudioSourceExperimental.enabled = active;
			if (active)
			{
				UpdateParameters();
			}
			bool_0 = active;
		}
	}

	public override void SetParameters(AudioGroupPreset preset)
	{
		if ((object)_metaXRAudioSource != null)
		{
			if (!_metaXRAudioSource.isActiveAndEnabled)
			{
				_metaXRAudioSource.enabled = true;
			}
			_metaXRAudioSource.EnableSpatialization = preset.SteamSpatialize;
			_metaXRAudioSourceExperimental.VolumetricRadius = preset.OcclusionSourceRadius;
			_metaXRAudioSourceExperimental.DirectivityPattern = preset.DirectivityPatternType;
			_metaXRAudioSourceExperimental.DirectivityIntensity = preset.DirectivityIntensity;
			_metaXRAudioSourceExperimental.HrtfIntensity = preset.HrtfIntensity;
			_metaXRAudioSource.EnableAcoustics = preset.acousticSettings.mono.enabledReverb;
			_metaXRAudioSource.ReverbSendDb = preset.acousticSettings.mono.reverbSendDb;
			_metaXRAudioSourceExperimental.EarlyReflectionsSendDb = preset.acousticSettings.mono.earlyReflectionsSendDb;
			_metaXRAudioSourceExperimental.ReverbReach = preset.acousticSettings.mono.reverbReach;
			_metaXRAudioSource.UpdateParameters();
		}
	}

	public override void ManualUpdate()
	{
		if (bool_0 && base.gameObject.activeInHierarchy)
		{
			_metaXRAudioSource.ManualUpdate();
			_metaXRAudioSourceExperimental.ManualUpdate();
		}
	}
}
