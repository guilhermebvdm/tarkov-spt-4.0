using UnityEngine;

namespace Audio.AmbientSubsystem;

public class LoopAmbientSoundPlayer : BaseAmbientSoundPlayer
{
	[SerializeField]
	private AudioClip _loopClip;

	public override void Awake()
	{
		base.Awake();
		base.Source.loop = true;
	}

	public override void OnPlay(float dt)
	{
		if (CanPlay())
		{
			base.OnPlay(dt);
			base.Source.loop = true;
			base.Source.pitch = GetPitch();
			base.Source.clip = GetClip();
			base.Source.Play();
		}
	}

	public override bool CanPlay()
	{
		if (!base.Source.isPlaying)
		{
			return base.CanPlay();
		}
		return false;
	}

	public override AudioClip GetClip()
	{
		return _loopClip;
	}
}
