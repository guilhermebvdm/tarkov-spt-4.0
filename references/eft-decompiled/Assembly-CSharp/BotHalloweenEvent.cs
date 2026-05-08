using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.GlobalEvents;
using UnityEngine;

public class BotHalloweenEvent
{
	[Serializable]
	[CompilerGenerated]
	public class Class341
	{
		public static readonly Class341 class341_0 = new Class341();

		public static Func<BotOwner, bool> func_0;

		public static Func<BotOwner, bool> func_1;

		public bool method_0(BotOwner x)
		{
			return x.Profile.Info.Settings.Role == WildSpawnType.sectactPriestEvent;
		}

		public bool method_1(BotOwner x)
		{
			if (x.Profile.Info.Settings.Role == WildSpawnType.sectactPriestEvent)
			{
				return x.HealthController.IsAlive;
			}
			return false;
		}
	}

	public const string ZRYACHIY_PEAACE_KILL = "ZryachiyPeaceKill";

	public const string ZRYACHIY_SUMMON_COMPLETE = "SummonComplete";

	public const string ZRYACHIY_SUMMON_FAIL = "SummonFail";

	public const string FIRST_CONTACT = "HalloweenFirstContact";

	public const string FIRST_KILL = "HalloweenFirstKill";

	public const int CULTIST_ANIMATION_LAYER = 17;

	public int ToCircle = 3;

	public bool IsEventRaised;

	[NonSerialized]
	public BotsEventsController AllEvents;

	[NonSerialized]
	public BotSpawner Spawner;

	[NonSerialized]
	public AIPlaceInfoHolder PlacesInfo;

	[NonSerialized]
	public int EventTime = 120;

	[NonSerialized]
	public AIPlaceInfoHalloweenCircle TargetCircle_1;

	[NonSerialized]
	public bool RitualStarted;

	[NonSerialized]
	public float EndSummon;

	[NonSerialized]
	public bool FirstContact;

	[NonSerialized]
	public bool FirstKill;

	[NonSerialized]
	public int OwnerSectantGroup = -1;

	public HashSet<string> PlayersProfilesIdsToKill = new HashSet<string>();

	[field: NonSerialized]
	public HashSet<int> PlayersIdsToKill { get; } = new HashSet<int>();

	public AIPlaceInfoHalloweenCircle TargetCircle
	{
		get
		{
			if (TargetCircle_1 != null)
			{
				return TargetCircle_1;
			}
			List<AIPlaceInfoHalloweenCircle> list = new List<AIPlaceInfoHalloweenCircle>();
			foreach (AIPlaceInfo place in PlacesInfo.Places)
			{
				if (place.InfoLogicAllEnemy is AIPlaceInfoHalloweenCircle aIPlaceInfoHalloweenCircle && aIPlaceInfoHalloweenCircle.CastPoints.Count > 0)
				{
					list.Add(aIPlaceInfoHalloweenCircle);
				}
			}
			if (list.Count > 0)
			{
				TargetCircle_1 = GClass856.RandomElement(list);
				ToCircle = Mathf.Min(ToCircle, TargetCircle_1.CastPoints.Count);
				if (ToCircle <= 0)
				{
					return null;
				}
			}
			if (TargetCircle_1 == null)
			{
				return null;
			}
			return TargetCircle_1;
		}
	}

	public BotHalloweenEvent(BotsEventsController allEvents, BotSpawner spawner, AIPlaceInfoHolder placesInfo)
	{
		AllEvents = allEvents;
		Spawner = spawner;
		PlacesInfo = placesInfo;
		if (Singleton<BackendConfigSettingsClass>.Instantiated)
		{
			EventTime = Singleton<BackendConfigSettingsClass>.Instance.EventSettings.EventTime;
		}
		if (Spawner != null)
		{
			Spawner.BotGame.BotsController.Bots.OnBotRemove += method_0;
		}
	}

	public void method_0(BotOwner obj)
	{
		if (!IsEventRaised && obj.Profile.Info.Settings.Role == WildSpawnType.sectactPriestEvent)
		{
			CheckOnSectantsCount();
		}
	}

	public void StartRitual()
	{
		if (!RitualStarted)
		{
			RitualStarted = true;
			EndSummon = Time.time + (float)EventTime;
			Spawner.BotGame.BotsController.AiTaskManager.RegisterDelayedTask(EventTime, method_2);
			GlobalEventHandlerClass.Instance.CreateCommonEvent<HalloweenSummonStartedEvent>().Invoke(TargetCircle.CenterPoint.position);
		}
	}

	public void AddToKill(IPlayer player)
	{
		PlayersIdsToKill.Add(player.Id);
		PlayersProfilesIdsToKill.Add(player.ProfileId);
	}

	public void PeaceZryachiyKilled()
	{
		method_1();
	}

	public void method_1()
	{
		Singleton<BotEventHandler>.Instance.AnyEvent("ZryachiyPeaceKill");
		GlobalEventHandlerClass.Instance.CreateCommonEvent<HalloweenSyncStateEvent>().Invoke(EEventState.Started);
	}

	public void RitualCompleted()
	{
		Singleton<BotEventHandler>.Instance.AnyEvent("SummonComplete");
		GlobalEventHandlerClass.Instance.CreateCommonEvent<HalloweenSyncStateEvent>().Invoke(EEventState.SummonSuccess);
		Spawner.ActivateBotsWithoutWave(Spawner.MaxBots / 2, new BotProfileDataClass(EPlayerSide.Savage, WildSpawnType.assault, BotDifficulty.normal, 0f)).HandleExceptions();
		AllEvents.ForceAttackEvent.ExternalStart();
	}

	public void RitualFail()
	{
		Singleton<BotEventHandler>.Instance.AnyEvent("SummonFail");
		GlobalEventHandlerClass.Instance.CreateCommonEvent<HalloweenSyncStateEvent>().Invoke(EEventState.SummonFailed);
	}

	public void UpdateFromBoss()
	{
		if (RitualStarted && !IsEventRaised && EndSummon < Time.time)
		{
			method_2();
		}
	}

	public void method_2()
	{
		if (!IsEventRaised)
		{
			BotOwner botOwner = Spawner.BotGame.BotsController.Bots.BotOwners.FirstOrDefault((BotOwner x) => x.Profile.Info.Settings.Role == WildSpawnType.sectactPriestEvent);
			IsEventRaised = true;
			if (botOwner == null)
			{
				RitualFail();
			}
			else
			{
				RitualCompleted();
			}
		}
	}

	public void FirstContact()
	{
		if (!this.FirstContact)
		{
			this.FirstContact = true;
			Singleton<BotEventHandler>.Instance.AnyEvent("HalloweenFirstContact");
		}
	}

	public void KillFirstSectant()
	{
		if (!FirstKill)
		{
			FirstKill = true;
			Singleton<BotEventHandler>.Instance.AnyEvent("HalloweenFirstKill");
		}
	}

	public void SetupGroup(int ownerGroupId)
	{
		if (OwnerSectantGroup <= 0)
		{
			OwnerSectantGroup = ownerGroupId;
		}
	}

	public bool IsMyGroup(int ownerGroupId)
	{
		if (OwnerSectantGroup <= 0)
		{
			return true;
		}
		return OwnerSectantGroup == ownerGroupId;
	}

	public void CheckOnSectantsCount()
	{
		if (!(Spawner.BotGame.BotsController.Bots.BotOwners.FirstOrDefault((BotOwner x) => x.Profile.Info.Settings.Role == WildSpawnType.sectactPriestEvent && x.HealthController.IsAlive) != null))
		{
			IsEventRaised = true;
			RitualFail();
		}
	}

	public void Dispose()
	{
		Spawner.BotGame.BotsController.Bots.OnBotRemove -= method_0;
	}
}
