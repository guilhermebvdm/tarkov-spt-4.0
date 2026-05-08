using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Audio.SpatialSystem;
using Comfort.Common;
using CommonAssets.Scripts.ArtilleryShelling;
using EFT;
using EFT.GlobalEvents;
using EFT.GlobalEvents.ArtilleryShellingEcents;
using UnityEngine;

public class GClass1508 : IDisposable
{
	[Serializable]
	[CompilerGenerated]
	public class Class1002
	{
		public static readonly Class1002 class1002_0 = new Class1002();

		public static Func<ArtilleryShellingZone, bool> func_0;

		public bool method_0(ArtilleryShellingZone x)
		{
			return x.UseInCalledShelling;
		}
	}

	[NonSerialized]
	public const string String_0 = "Audio/item_mobile_phone_vibration_oneshot";

	[NonSerialized]
	public const int Int_0 = 15;

	[NonSerialized]
	public const float Float_0 = 0.3f;

	[NonSerialized]
	public const string String_1 = "ShellingWarningMessage";

	[NonSerialized]
	public const string String_2 = "ShellingBusy";

	[NonSerialized]
	public const float Float_1 = 1f;

	[NonSerialized]
	public AudioClip AudioClip_0;

	[NonSerialized]
	public ClientShellingControllerClass ClientShellingControllerClass;

	[NonSerialized]
	public Dictionary<int, GClass1509> Dictionary_0 = new Dictionary<int, GClass1509>();

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public Action Action_0;

	[NonSerialized]
	public Queue<string> Queue_0 = new Queue<string>();

	public void Init(ClientShellingControllerClass controller)
	{
		ClientShellingControllerClass = controller;
		method_0();
		AudioClip_0 = (AudioClip)Resources.Load("Audio/item_mobile_phone_vibration_oneshot");
	}

	public void OnUpdate()
	{
		method_4();
		method_5();
	}

	public void ReceiveShellingNotificationToPlayers(string zoneId, int[] playersToNotify)
	{
		if (playersToNotify.Contains(GamePlayerOwner.MyPlayer.PlayerId))
		{
			Queue_0.Enqueue(zoneId);
		}
	}

	public void ReceiveChangeAlertPlayerState(int playerId, string zoneId, Vector3 nearestPoint)
	{
		if (!Singleton<GameWorld>.Instance.TryGetAlivePlayerByID(playerId, out var player))
		{
			return;
		}
		bool flag;
		if (!(flag = method_8(zoneId, out var foundZone)))
		{
			foundZone = ClientShellingControllerClass.CurrentMapConfiguration.ShellingZones.FirstOrDefault((ArtilleryShellingZone x) => x.UseInCalledShelling);
			flag = !foundZone.Equals(null);
		}
		if (flag && !string.IsNullOrEmpty(zoneId))
		{
			if (player == null)
			{
				return;
			}
			float num = Vector3.Distance(player.Position, nearestPoint);
			GStruct150[] alarmStages = foundZone.AlarmStages;
			int num2 = 0;
			GStruct150 gStruct;
			while (true)
			{
				if (num2 < alarmStages.Length)
				{
					gStruct = alarmStages[num2];
					if (!(num > gStruct.Value.x))
					{
						break;
					}
					num2++;
					continue;
				}
				return;
			}
			Vector2 value = gStruct.Value;
			method_6(playerId, value, player, foundZone, nearestPoint);
		}
		else
		{
			method_7(playerId);
		}
	}

	public void method_0()
	{
		Action_0 = (Action)Delegate.Combine(Action_0, GlobalEventHandlerClass.Instance.SubscribeOnEvent<ShellingNotifyEvent>(method_2));
		Action_0 = (Action)Delegate.Combine(Action_0, GlobalEventHandlerClass.Instance.SubscribeOnEvent<ChangePlayerShellingAlertStateEvent>(method_3));
		Action_0 = (Action)Delegate.Combine(Action_0, GlobalEventHandlerClass.Instance.SubscribeOnEvent<NotifyEvent>(method_1));
	}

	public void method_1(NotifyEvent notifyEvent)
	{
		if (notifyEvent.ReceiverId.Equals(GamePlayerOwner.MyPlayer.PlayerId) && notifyEvent.MessageType == NotifyEvent.EMessageType.ArtilleryBusy)
		{
			NotificationManagerClass.DisplayMessageNotification(GClass2348.Localized("ShellingBusy"));
		}
	}

	public void method_2(ShellingNotifyEvent notifyEvent)
	{
		ReceiveShellingNotificationToPlayers(notifyEvent.ZoneId, notifyEvent.PlayersIDs);
	}

	public void method_3(ChangePlayerShellingAlertStateEvent changeStateEvent)
	{
		ReceiveChangeAlertPlayerState(changeStateEvent.PlayerId, changeStateEvent.ZoneId, changeStateEvent.NearestPoint);
	}

	public void method_4()
	{
		if (Queue_0.Count != 0)
		{
			Float_3 += Time.deltaTime;
			if (!(Float_3 < 1f))
			{
				string zoneId = Queue_0.Dequeue();
				method_10(zoneId);
				Float_3 = 0f;
			}
		}
	}

	public void method_5()
	{
		foreach (var (_, gClass2) in Dictionary_0)
		{
			if (!gClass2.AlarmOn)
			{
				continue;
			}
			Vector3 position = gClass2.Player.Position;
			position.y = gClass2.NearestShellingPoint.y;
			float num2 = Vector3.Distance(position, gClass2.NearestShellingPoint);
			GStruct150[] alarmStages = gClass2.Zone.AlarmStages;
			float x = alarmStages[^1].Value.x;
			if (num2 > x)
			{
				continue;
			}
			for (int i = 0; i < alarmStages.Length; i++)
			{
				if (!(num2 > alarmStages[i].Value.x))
				{
					gClass2.AlarmStage.x = alarmStages[i].Value.x;
					gClass2.AlarmStage.y = alarmStages[i].Value.y;
					break;
				}
			}
			gClass2.ElapsedTime += Time.deltaTime;
			float y = gClass2.AlarmStage.y;
			if (!(gClass2.ElapsedTime < y))
			{
				gClass2.ElapsedTime = 0f;
				method_9(gClass2.Player);
			}
		}
	}

	public void method_6(int playerId, Vector2 alarmStage, IPlayer player, ArtilleryShellingZone zone, Vector3 nearestPoint)
	{
		if (!Dictionary_0.ContainsKey(playerId))
		{
			Dictionary_0.Add(playerId, new GClass1509
			{
				Player = player,
				AlarmStage = alarmStage,
				Zone = zone,
				NearestShellingPoint = nearestPoint
			});
		}
		else
		{
			Dictionary_0[playerId].AlarmStage.x = alarmStage.x;
			Dictionary_0[playerId].AlarmStage.y = alarmStage.y;
			Dictionary_0[playerId].NearestShellingPoint = nearestPoint;
		}
		Dictionary_0[playerId].AlarmOn = true;
	}

	public void method_7(int playerId)
	{
		if (Dictionary_0.TryGetValue(playerId, out var value))
		{
			value.AlarmStage.x = 0f;
			value.AlarmStage.y = 0f;
			value.AlarmOn = false;
			value.NearestShellingPoint = Vector3.zero;
		}
	}

	public bool method_8(string zoneId, out ArtilleryShellingZone foundZone)
	{
		ArtilleryShellingZone[] shellingZones = ClientShellingControllerClass.CurrentMapConfiguration.ShellingZones;
		int num = 0;
		ArtilleryShellingZone artilleryShellingZone;
		while (true)
		{
			if (num < shellingZones.Length)
			{
				artilleryShellingZone = shellingZones[num];
				if (artilleryShellingZone.ID == zoneId)
				{
					break;
				}
				num++;
				continue;
			}
			foundZone = default(ArtilleryShellingZone);
			return false;
		}
		foundZone = artilleryShellingZone;
		return true;
	}

	public void method_9(IPlayer player)
	{
		if (GamePlayerOwner.MyPlayer == null || player == null || !player.HealthController.IsAlive)
		{
			return;
		}
		bool isYourPlayer = player.IsYourPlayer;
		Vector3 position = player.Position;
		if (!isYourPlayer && !GClass2313.IsInRange(position, 15f))
		{
			return;
		}
		BetterAudio.AudioSourceGroupType sourceGroup = BetterAudio.AudioSourceGroupType.Environment;
		if (isYourPlayer)
		{
			sourceGroup = BetterAudio.AudioSourceGroupType.Nonspatial;
		}
		if (MonoBehaviourSingleton<BetterAudio>.Instance.TryPlayAtPoint(out var source, player.Position, AudioClip_0, sourceGroup, 15, 0.3f, EOcclusionTest.None, null, !isYourPlayer))
		{
			if (isYourPlayer)
			{
				source.EnableStereo(stereo: true);
			}
			else if (MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
			{
				MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(player, source);
			}
		}
	}

	public void method_10(string zoneId)
	{
		string text = "ShellingWarningMessage/" + zoneId;
		string text2 = GClass2348.Localized(text);
		if (text2 == text)
		{
			text2 = GClass2348.Localized("ShellingWarningMessage");
		}
		NotificationManagerClass.DisplayMessageNotification(text2);
	}

	public void Dispose()
	{
		Action_0?.Invoke();
		Action_0 = null;
	}
}
