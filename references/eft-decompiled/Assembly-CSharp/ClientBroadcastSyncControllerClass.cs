using System;
using System.Collections.Generic;
using System.Linq;
using Audio.AudioCulling;
using Audio.RadioSystem;
using Comfort.Common;
using CommonAssets.Scripts.Audio.RadioSystem;
using EFT;
using EFT.GlobalEvents.AudioEvents;
using UnityEngine;

public class ClientBroadcastSyncControllerClass : GInterface79, IDisposable
{
	[NonSerialized]
	public GInterface93 Ginterface93_0;

	[NonSerialized]
	public Dictionary<ERadioStation, AudioClip> Dictionary_0 = new Dictionary<ERadioStation, AudioClip>();

	[NonSerialized]
	public Dictionary<ERadioStation, double> Dictionary_1 = new Dictionary<ERadioStation, double>();

	[NonSerialized]
	public Dictionary<ERadioStation, List<GInterface80>> Dictionary_2 = new Dictionary<ERadioStation, List<GInterface80>>();

	[NonSerialized]
	public ClientBroadcastPlayer[] ClientBroadcastPlayer_0;

	[NonSerialized]
	public Action Action_0;

	public ClientBroadcastSyncControllerClass()
	{
		GlobalEventHandlerClass instance = GlobalEventHandlerClass.Instance;
		Action_0 = (Action)Delegate.Combine(Action_0, instance.SubscribeOnEvent<BroadcastItemChangedEvent>(method_0));
		Action_0 = (Action)Delegate.Combine(Action_0, instance.SubscribeOnEvent<RadioStationsSyncEvent>(method_1));
		Transform transform = Singleton<AudioListenerConsistencyManager>.Instance.transform;
		Ginterface93_0 = new GClass1182(transform);
		ClientBroadcastPlayer_0 = LocationScene.GetAllObjects<ClientBroadcastPlayer>(withDisabled: true).ToArray();
		ClientBroadcastPlayer[] clientBroadcastPlayer_ = ClientBroadcastPlayer_0;
		foreach (ClientBroadcastPlayer clientBroadcastPlayer in clientBroadcastPlayer_)
		{
			RegisterBroadcastPlayer(clientBroadcastPlayer);
			Ginterface93_0.Register(clientBroadcastPlayer);
			clientBroadcastPlayer.AudibleStateChanged += method_2;
		}
	}

	public bool TryGetClipAndBroadcastTime(ERadioStation station, out AudioClip clip, out double broadcastTime)
	{
		clip = null;
		broadcastTime = 0.0;
		if (!Dictionary_1.TryGetValue(station, out var value))
		{
			return false;
		}
		if (AudioSettings.dspTime > value)
		{
			return false;
		}
		if (!Dictionary_0.TryGetValue(station, out clip))
		{
			return false;
		}
		broadcastTime = (double)clip.length - (value - AudioSettings.dspTime);
		return true;
	}

	public void method_0(BroadcastItemChangedEvent changedEvent)
	{
		if (!MonoBehaviourSingleton<RadioBroadcastController>.Instantiated)
		{
			return;
		}
		ERadioStation radioStation = changedEvent.RadioStation;
		if (!MonoBehaviourSingleton<RadioBroadcastController>.Instance.TryGetConcreteStationClipByIndex(radioStation, changedEvent.ItemType, changedEvent.ClipIndex, out var clip))
		{
			return;
		}
		if (Dictionary_2.TryGetValue(radioStation, out var value))
		{
			foreach (GInterface80 item in value)
			{
				item.SyncBroadcast(clip, 0.0, changedEvent.CrossfadeDuration);
			}
		}
		Dictionary_0[radioStation] = clip;
		Dictionary_1[radioStation] = AudioSettings.dspTime + (double)clip.length;
	}

	public void method_1(RadioStationsSyncEvent syncEvent)
	{
		List<BroadcastDataPacket> broadcastData = syncEvent.BroadcastData;
		if (broadcastData != null && broadcastData.Count != 0)
		{
			if (GamePlayerOwner.MyPlayer == null || syncEvent.ProfileID != GamePlayerOwner.MyPlayer.ProfileId)
			{
				return;
			}
			{
				foreach (BroadcastDataPacket item in broadcastData)
				{
					ERadioStation radioStation = (ERadioStation)item.radioStation;
					EBroadcastItemType broadcastItemType = (EBroadcastItemType)item.broadcastItemType;
					float currentClipTime = item.currentClipTime;
					if (!MonoBehaviourSingleton<RadioBroadcastController>.Instance.TryGetConcreteStationClipByIndex(radioStation, broadcastItemType, item.broadcastItemIndex, out var clip))
					{
						break;
					}
					if (Dictionary_2.TryGetValue(radioStation, out var value))
					{
						foreach (GInterface80 item2 in value)
						{
							item2.SyncBroadcast(clip, currentClipTime);
						}
					}
					Dictionary_0[radioStation] = clip;
					Dictionary_1[radioStation] = AudioSettings.dspTime + (double)Mathf.Max(0f, clip.length - item.currentClipTime);
				}
				return;
			}
		}
		Debug.LogError("broadcast data failed");
	}

	public void RegisterBroadcastPlayer(GInterface80 player)
	{
		ERadioStation radioStation = player.RadioStation;
		if (Dictionary_2.TryGetValue(radioStation, out var value))
		{
			value.Add(player);
		}
		else
		{
			Dictionary_2[radioStation] = new List<GInterface80> { player };
		}
		player.Initialize();
		method_3(player);
	}

	public void ChangePlayerStation(GInterface80 player, ERadioStation newStation)
	{
		if (Dictionary_2.TryGetValue(player.RadioStation, out var value))
		{
			value.Remove(player);
		}
		player.ChangeStation(newStation);
		RegisterBroadcastPlayer(player);
	}

	public void method_2(EAudibleState state)
	{
		if (state == EAudibleState.Mute)
		{
			return;
		}
		for (int i = 0; i < ClientBroadcastPlayer_0.Length; i++)
		{
			ClientBroadcastPlayer clientBroadcastPlayer = ClientBroadcastPlayer_0[i];
			if (!(clientBroadcastPlayer == null) && clientBroadcastPlayer.CurrentAudibleState != EAudibleState.Mute)
			{
				method_3(clientBroadcastPlayer);
			}
		}
	}

	public void method_3(GInterface80 player)
	{
		if (TryGetClipAndBroadcastTime(player.RadioStation, out var clip, out var broadcastTime))
		{
			player.SyncBroadcast(clip, broadcastTime);
		}
	}

	public void Dispose()
	{
		ClientBroadcastPlayer[] clientBroadcastPlayer_ = ClientBroadcastPlayer_0;
		foreach (ClientBroadcastPlayer clientBroadcastPlayer in clientBroadcastPlayer_)
		{
			if (!(clientBroadcastPlayer == null))
			{
				clientBroadcastPlayer.Stop();
				clientBroadcastPlayer.AudibleStateChanged -= method_2;
			}
		}
		Ginterface93_0?.Dispose();
		Action_0?.Invoke();
		Action_0 = null;
	}
}
