using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio.AmbientSubsystem;

public class AmbientSoundPlayerGroup : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	public class Class814
	{
		public static readonly Class814 class814_0 = new Class814();

		public static Func<BaseAmbientSoundPlayer, bool> func_0;

		public bool method_0(BaseAmbientSoundPlayer x)
		{
			return x.IsPlaying;
		}
	}

	[SerializeField]
	private AudioMixerGroup _outputMixerGroup;

	[SerializeField]
	private List<BaseAmbientSoundPlayer> _soundPlayers = new List<BaseAmbientSoundPlayer>();

	public bool IsPlaying => _soundPlayers.Any((BaseAmbientSoundPlayer x) => x.IsPlaying);

	public void Awake()
	{
		if (_soundPlayers.Count == 0)
		{
			_soundPlayers = GetComponentsInChildren<BaseAmbientSoundPlayer>().ToList();
		}
	}

	public void Start()
	{
		method_0();
	}

	public void method_0()
	{
		foreach (BaseAmbientSoundPlayer soundPlayer in _soundPlayers)
		{
			soundPlayer.SetMixerGroup(_outputMixerGroup);
		}
	}

	public void Play()
	{
		foreach (BaseAmbientSoundPlayer soundPlayer in _soundPlayers)
		{
			soundPlayer.Play();
		}
	}

	public void Stop(bool force = false)
	{
		foreach (BaseAmbientSoundPlayer soundPlayer in _soundPlayers)
		{
			soundPlayer.Stop(force: true);
		}
	}
}
