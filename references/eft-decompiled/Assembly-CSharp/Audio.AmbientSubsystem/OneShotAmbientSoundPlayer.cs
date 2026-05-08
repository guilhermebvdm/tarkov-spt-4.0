using EFT;
using UnityEngine;

namespace Audio.AmbientSubsystem;

public class OneShotAmbientSoundPlayer : BaseAmbientSoundPlayer
{
	private const float float_0 = 0.1f;

	[SerializeField]
	private SoundBank _soundBank;

	private bool bool_2;

	private float float_1;

	public override void OnPlay(float dt)
	{
		if (bool_2 && Time.realtimeSinceStartup > float_1)
		{
			Stop();
		}
		else if (!bool_2)
		{
			AudioClip clip = GetClip();
			base.Source.volume = GetBaseVolume();
			base.Source.clip = clip;
			base.Source.Play();
			base.OnPlay(dt);
			bool_2 = true;
			float_1 = Time.realtimeSinceStartup + clip.length + 0.1f;
		}
	}

	public override void OnStop()
	{
		base.OnStop();
		bool_2 = false;
	}

	public override AudioClip GetClip()
	{
		return _soundBank.PickSingleClip(0);
	}
}
