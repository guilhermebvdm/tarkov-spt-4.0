using System.Collections.Generic;
using System.Linq;
using Audio.SpatialSystem;
using EFT;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio.AmbientSubsystem;

public class TriggerSoundBankPlayer : MonoBehaviour
{
	public struct Struct220
	{
		public float NextPLayingTime;

		public Vector3 Position;
	}

	[SerializeField]
	private AudioTriggerArea _audioTriggerArea;

	[SerializeField]
	private SoundBank _soundBank;

	[SerializeField]
	private float _volume = 1f;

	[SerializeField]
	private int _rolloff = 1;

	[SerializeField]
	private EnvironmentType _environmentType;

	[SerializeField]
	private BetterAudio.AudioSourceGroupType _groupType;

	[SerializeField]
	private AudioMixerGroup _mixerGroup;

	[SerializeField]
	private float _cooldown = 0.1f;

	private readonly Dictionary<int, Struct220> dictionary_0 = new Dictionary<int, Struct220>();

	private readonly List<IPlayer> list_0 = new List<IPlayer>();

	public void Awake()
	{
		_audioTriggerArea.OnTriggerArea += method_0;
	}

	public void method_0(object sender, GEventArgs0 e)
	{
		IPlayer player = e.Player;
		if (e.Player != null)
		{
			if (e.TriggerEventType == GEventArgs0.ETriggerEventType.TriggerEnter)
			{
				dictionary_0[player.Id] = new Struct220
				{
					NextPLayingTime = Time.realtimeSinceStartup,
					Position = player.Position
				};
				list_0.Add(player);
				player.OnIPlayerDeadOrUnspawn += method_1;
			}
			else if (e.TriggerEventType == GEventArgs0.ETriggerEventType.TriggerExit)
			{
				player.OnIPlayerDeadOrUnspawn -= method_1;
				dictionary_0.Remove(player.Id);
				list_0.Remove(player);
			}
		}
	}

	public void method_1(IPlayer player)
	{
		player.OnIPlayerDeadOrUnspawn -= method_1;
		list_0.Remove(player);
		dictionary_0.Remove(player.Id);
	}

	public void Update()
	{
		if (list_0.Count == 0)
		{
			return;
		}
		for (int num = list_0.Count - 1; num >= 0; num--)
		{
			IPlayer player = list_0[num];
			if (player != null && player.HealthController.IsAlive)
			{
				Struct220 @struct = dictionary_0[player.Id];
				if (!(Time.realtimeSinceStartup < @struct.NextPLayingTime) && !GClass940.IsPositionIdentical(@struct.Position, player.Position, 0.2f))
				{
					dictionary_0[player.Id] = new Struct220
					{
						NextPLayingTime = Time.realtimeSinceStartup + _cooldown,
						Position = player.Position
					};
					method_2(player);
				}
			}
			else
			{
				dictionary_0.Remove(dictionary_0.ElementAt(num).Key);
				list_0.RemoveAt(num);
			}
		}
	}

	public void method_2(IPlayer player)
	{
		AudioClip clip = _soundBank.PickSingleClip((int)_environmentType);
		if (MonoBehaviourSingleton<BetterAudio>.Instance.TryPlayAtPoint(out var source, player.Position, clip, _groupType, _rolloff, _volume, EOcclusionTest.None, _mixerGroup, !player.IsYourPlayer) && !player.IsYourPlayer && MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
		{
			MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(player, source);
		}
	}

	public void OnDestroy()
	{
		if (_audioTriggerArea != null)
		{
			_audioTriggerArea.OnTriggerArea -= method_0;
		}
		for (int i = 0; i < list_0.Count; i++)
		{
			IPlayer player = list_0[i];
			if (player != null)
			{
				player.OnIPlayerDeadOrUnspawn -= method_1;
			}
		}
		list_0.Clear();
		dictionary_0.Clear();
	}
}
