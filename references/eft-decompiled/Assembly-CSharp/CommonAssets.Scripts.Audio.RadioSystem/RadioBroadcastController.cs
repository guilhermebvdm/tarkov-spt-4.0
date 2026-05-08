using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.GlobalEvents.AudioEvents;
using UnityEngine;

namespace CommonAssets.Scripts.Audio.RadioSystem;

public class RadioBroadcastController : MonoBehaviourSingleton<RadioBroadcastController>, GInterface147, IDisposable
{
	[Serializable]
	public class RadioStationsGridData : SerializableEnumDictionary<ERadioStation, RadioStationGridData>
	{
	}

	[Serializable]
	public class RadioStationGridData
	{
		public BroadcastItemsContainer BroadcastItemsContainer;

		public BroadcastRule BroadcastRule;
	}

	[SerializeField]
	private RadioStationsGridData _radioStationsGridData;

	private readonly List<GInterface148> list_0 = new List<GInterface148>();

	private readonly List<BroadcastDataPacket> list_1 = new List<BroadcastDataPacket>();

	private bool bool_0;

	private EGameType egameType_0;

	private bool bool_1;

	private GClass1499.GClass1500 gclass1500_0;

	public override void Awake()
	{
		base.Awake();
		gclass1500_0 = Singleton<BackendConfigSettingsClass>.Instance.AudioSettings.RadioSettings;
		if (gclass1500_0.EnabledBroadcast && TarkovApplication.Exist(out var tarkovApplication) && (tarkovApplication.CurrentRaidSettings.IsPveOffline || tarkovApplication.CurrentRaidSettings.Local))
		{
			method_0();
			Singleton<GameWorld>.Instance.OnPersonAdd += method_1;
			bool_1 = true;
		}
	}

	public void method_0()
	{
		GClass1499.GClass1500 radioSettings = Singleton<BackendConfigSettingsClass>.Instance.AudioSettings.RadioSettings;
		foreach (var (station, radioStationGridData2) in _radioStationsGridData)
		{
			if (radioSettings.GetStationEnabledState(station))
			{
				GClass1504 item = new GClass1504(station, radioStationGridData2.BroadcastItemsContainer, radioStationGridData2.BroadcastRule, this);
				list_0.Add(item);
			}
		}
	}

	public void method_1(IPlayer player)
	{
		if (player.IsAI)
		{
			return;
		}
		list_1.Clear();
		foreach (GInterface148 item2 in list_0)
		{
			BroadcastDataPacket item = new BroadcastDataPacket
			{
				radioStation = (byte)item2.Station,
				broadcastItemType = (byte)item2.CurrentBroadcastItem.itemType,
				broadcastItemIndex = item2.CurrentBroadcastItem.clipIndex,
				currentClipTime = item2.CurrentBroadcastTime
			};
			list_1.Add(item);
		}
		GlobalEventHandlerClass.CreateEvent<RadioStationsSyncEvent>().Invoke(player.ProfileId, list_1);
	}

	public bool TryGetConcreteStationClipByIndex(ERadioStation station, EBroadcastItemType broadcastItemType, int index, out AudioClip clip)
	{
		clip = null;
		if (_radioStationsGridData != null && _radioStationsGridData.TryGetValue(station, out var value))
		{
			return value.BroadcastItemsContainer.TryGetContent(broadcastItemType, index, out clip);
		}
		return false;
	}

	public void StartBroadcast()
	{
		if (gclass1500_0 == null || !gclass1500_0.EnabledBroadcast)
		{
			return;
		}
		foreach (GInterface148 item in list_0)
		{
			if (gclass1500_0.GetStationEnabledState(item.Station))
			{
				item.StartBroadcast();
			}
		}
	}

	public void StopBroadcast()
	{
		foreach (GInterface148 item in list_0)
		{
			item.StopBroadcast();
		}
	}

	public void Dispose()
	{
		StopBroadcast();
		if (Singleton<GameWorld>.Instantiated)
		{
			Singleton<GameWorld>.Instance.OnPersonAdd -= method_1;
		}
	}

	public override void OnDestroy()
	{
		Dispose();
		base.OnDestroy();
	}
}
