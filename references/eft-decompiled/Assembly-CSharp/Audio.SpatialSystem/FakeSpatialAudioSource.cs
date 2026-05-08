using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

namespace Audio.SpatialSystem;

public class FakeSpatialAudioSource : BaseSpatialAudioSource
{
	[CompilerGenerated]
	private bool bool_0;

	[CompilerGenerated]
	private float float_0;

	[CompilerGenerated]
	private float float_1;

	[CompilerGenerated]
	private bool bool_1;

	[CompilerGenerated]
	private bool bool_2;

	[CompilerGenerated]
	private float float_2;

	[CompilerGenerated]
	private float float_3;

	[CompilerGenerated]
	private float float_4;

	[CompilerGenerated]
	private float float_5;

	[CompilerGenerated]
	private bool bool_3;

	public override bool EnableSpatialization
	{
		[CompilerGenerated]
		get
		{
			return bool_0;
		}
		[CompilerGenerated]
		set
		{
			bool_0 = value;
		}
	}

	public override float HrtfIntensity
	{
		[CompilerGenerated]
		get
		{
			return float_0;
		}
		[CompilerGenerated]
		set
		{
			float_0 = value;
		}
	}

	public override float DirectivityIntensity
	{
		[CompilerGenerated]
		get
		{
			return float_1;
		}
		[CompilerGenerated]
		set
		{
			float_1 = value;
		}
	}

	public override bool EnableReverb
	{
		[CompilerGenerated]
		get
		{
			return bool_1;
		}
		[CompilerGenerated]
		set
		{
			bool_1 = value;
		}
	}

	public override bool EnableDirectSound
	{
		[CompilerGenerated]
		get
		{
			return bool_2;
		}
		[CompilerGenerated]
		set
		{
			bool_2 = value;
		}
	}

	public override float ReverbSendDB
	{
		[CompilerGenerated]
		get
		{
			return float_2;
		}
		[CompilerGenerated]
		set
		{
			float_2 = value;
		}
	}

	public override float EarlyReflectionsSendDB
	{
		[CompilerGenerated]
		get
		{
			return float_3;
		}
		[CompilerGenerated]
		set
		{
			float_3 = value;
		}
	}

	public override float ReverbReach
	{
		[CompilerGenerated]
		get
		{
			return float_4;
		}
		[CompilerGenerated]
		set
		{
			float_4 = value;
		}
	}

	public override float VolumetricRadius
	{
		[CompilerGenerated]
		get
		{
			return float_5;
		}
		[CompilerGenerated]
		set
		{
			float_5 = value;
		}
	}

	public override bool DirectSoundEnabled
	{
		[CompilerGenerated]
		get
		{
			return bool_3;
		}
		[CompilerGenerated]
		set
		{
			bool_3 = value;
		}
	}

	public override void Awake()
	{
		base.Awake();
		Debug.LogWarning("FakeSpatialAudioSource::Awake");
	}

	public override void SetParameters(AudioGroupPreset preset)
	{
	}

	public override void UpdateParameters()
	{
	}

	public override void ManualUpdate()
	{
	}

	public override void SetActive(bool active)
	{
		base.enabled = active;
	}
}
