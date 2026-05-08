using EFT;
using UnityEngine;

namespace Audio.AmbientSubsystem;

public class EventLoopPlayer : BaseAmbientSoundPlayer, GInterface104
{
	[SerializeField]
	[HideInInspector]
	private EventAudioClipChanger.EventAudioClips _soundClips;

	private AudioClip audioClip_1;

	private bool bool_2;

	public override AudioClip GetClip()
	{
		return audioClip_1;
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
		if (!base.Source.isPlaying && bool_2)
		{
			return base.CanPlay();
		}
		return false;
	}

	public void ChangeSoundContent(EEventType eventType)
	{
		if (!_soundClips.TryGetValue(eventType, out var value))
		{
			Debug.Log($"No proposed clip for event: {eventType}");
			return;
		}
		bool isPlaying = base.IsPlaying;
		if (isPlaying)
		{
			Stop(force: true);
		}
		audioClip_1 = value;
		if (isPlaying)
		{
			Play();
		}
		bool_2 = true;
	}
}
