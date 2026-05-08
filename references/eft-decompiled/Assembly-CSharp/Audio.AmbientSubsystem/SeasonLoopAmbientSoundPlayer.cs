using System;
using UnityEngine;

namespace Audio.AmbientSubsystem;

public class SeasonLoopAmbientSoundPlayer : BaseAmbientSoundPlayer, GInterface98, GInterface102
{
	[Serializable]
	public class SeasonSoundClips : SerializableEnumDictionary<ESeasonStatus, AudioClip>
	{
	}

	[SerializeField]
	[HideInInspector]
	private SeasonSoundClips _seasonSoundClips;

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

	public void SetSeasonStatus(ESeasonStatus targetSeasonStatus)
	{
		if (!_seasonSoundClips.TryGetValue(targetSeasonStatus, out var value))
		{
			Debug.LogWarning($"sound bank is null on GO: {base.gameObject.name} for season: {targetSeasonStatus}");
			return;
		}
		ChangeAudioClip(value);
		bool_2 = true;
	}

	public void ChangeAudioClip(AudioClip newClip)
	{
		bool isPlaying = base.IsPlaying;
		if (isPlaying)
		{
			Stop(force: true);
		}
		audioClip_1 = newClip;
		if (isPlaying)
		{
			Play();
		}
	}
}
