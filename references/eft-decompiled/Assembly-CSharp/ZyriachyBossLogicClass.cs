using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.Game.Spawning;
using EFT.InventoryLogic;
using UnityEngine;

public class ZyriachyBossLogicClass : ABossLogic
{
	[CompilerGenerated]
	public class Class177
	{
		public ZyriachyBossLogicClass ZyriachyBossLogicClass;

		public Player player;

		public RadioTransmitterRecodableComponent trRec;

		public IPlayersCollection playersCollection;

		public IPlayer person;

		public void method_0(Player obj)
		{
			// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
			if (!ZyriachyBossLogicClass.method_6(player))
			{
				trRec.Handler.OnStatusChanged -= method_0;
				IPlayer[] groupPlayersToENemies = GClass3585.GroupPlayers(playersCollection, person.GroupId).ToArray();
				ZyriachyBossLogicClass.method_11(groupPlayersToENemies);
				ZyriachyBossLogicClass.method_4(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(person.ProfileId));
			}
		}
	}

	public const int MAX_RESPAWNS = 3;

	[NonSerialized]
	public const float Float_0 = 600f;

	[NonSerialized]
	public AIPlaceInfo AiplaceInfo_0;

	[NonSerialized]
	public HashSet<IPlayer> HashSet_0 = new HashSet<IPlayer>();

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public HashSet<int> HashSet_1 = new HashSet<int>();

	[NonSerialized]
	public HashSet<int> HashSet_2 = new HashSet<int>();

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2 = 49f;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	public bool IsManyEnemy
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public ZyriachyBossLogicClass(BotOwner owner, BotBoss bossLogic)
		: base(owner, bossLogic)
	{
	}

	public override void SetPatrolMode()
	{
	}

	public override void Activate()
	{
		Float_1 = Time.time + 5f;
		List<AIPlaceInfo> places = BotOwner_0.BotsGroup.BotGame.BotsController.CoversData.AIPlaceInfoHolder.Places;
		BotOwner_0.GetPlayer.BeingHitAction += method_2;
		BotOwner_0.GetPlayer.OnPlayerDead += method_17;
		Singleton<BotEventHandler>.Instance.OnSoundPlayed += method_1;
		Singleton<BotEventHandler>.Instance.OnBeingHit += method_0;
		foreach (BotOwner follower in BotOwner_0.Boss.Followers)
		{
			method_8(follower);
		}
		foreach (AIPlaceInfo item in places)
		{
			if (item.InfoLogicAllEnemy is AIPlaceInfoLogicZryachiy { ControllableZone: not false })
			{
				AiplaceInfo_0 = item;
				break;
			}
		}
		if (AiplaceInfo_0 == null)
		{
			Debug.LogError("Zryachiy don't have controllable zone FIX it. Create aiplace with AIPlaceInfoLogicZryachiy");
		}
		else
		{
			AiplaceInfo_0.OnPlayerEnter += method_13;
			AiplaceInfo_0.OnPlayerExit += method_12;
		}
		BotOwner_0.Boss.OnFollowerStatusChange += method_7;
	}

	public void AddEnemy(IPlayer person, EBotEnemyCause cause = EBotEnemyCause.zryachiyLogic)
	{
		HashSet_1.Add(person.Id);
		BotOwner_0.BotsGroup.AddEnemy(person, cause);
		for (int i = 0; i < BotOwner_0.BotsGroup.MembersCount; i++)
		{
			if (BotOwner_0.BotsGroup.Member(i).EnemiesController.EnemyInfos.TryGetValue(person, out var value) && value is GClass541 gClass)
			{
				gClass.SetGetHitFrom();
			}
		}
	}

	public override void BossLogicUpdate()
	{
		if (Float_1 > Time.time)
		{
			return;
		}
		float num = 3f;
		Float_1 = Time.time + num;
		foreach (BotOwner follower in BotOwner_0.Boss.Followers)
		{
			foreach (KeyValuePair<IPlayer, BotSettingsClass> neutral in BotOwner_0.BotsGroup.Neutrals)
			{
				method_18(neutral.Key, follower);
			}
		}
		foreach (KeyValuePair<IPlayer, BotSettingsClass> neutral2 in BotOwner_0.BotsGroup.Neutrals)
		{
			method_18(neutral2.Key, BotOwner_0);
		}
	}

	public bool IsEnemyNow(IPlayer person)
	{
		if (HashSet_1.Contains(person.Id))
		{
			return true;
		}
		if (method_6(person))
		{
			IPlayersCollection instance = Singleton<GameWorld>.Instance;
			IPlayer[] array = GClass3585.GroupPlayers(instance, person.GroupId).ToArray();
			if (array.Length != 0)
			{
				IPlayer[] array2 = array;
				foreach (IPlayer player in array2)
				{
					if (!method_6(player))
					{
						return true;
					}
				}
			}
			method_10(person, instance);
			return false;
		}
		return true;
	}

	public bool HaveHitFrom(int personId)
	{
		return HashSet_1.Contains(personId);
	}

	public bool IsMeAttackFirst(int playerId)
	{
		return HashSet_2.Contains(playerId);
	}

	public void MarkEnemyAsFirstAttacker(int personId)
	{
		HashSet_2.Add(personId);
	}

	public void method_0(DamageInfoStruct damageinfo, Player victim)
	{
		if (damageinfo.Player == null)
		{
			return;
		}
		IPlayer iPlayer = damageinfo.Player.iPlayer;
		if (iPlayer == null || HashSet_1.Contains(iPlayer.Id))
		{
			return;
		}
		foreach (AIPlaceInfo currentZone in iPlayer.AIData.CurrentZones)
		{
			if (currentZone.InfoLogicAllEnemy is AIPlaceInfoLogicZryachiy { CloseZone: not false } && HashSet_1.Contains(victim.Id))
			{
				HashSet_1.Add(iPlayer.Id);
				BotOwner_0.BotsGroup.AddEnemy(iPlayer, EBotEnemyCause.zryachiyLogic);
				break;
			}
		}
	}

	public void method_1(IPlayer player, Vector3 position, float power, AISoundType type)
	{
		if ((uint)(type - 1) > 1u || !(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.ProfileId).HandsController is IFirearmHandsController firearmHandsController) || !firearmHandsController.Item.IsGrenadeLauncher)
		{
			return;
		}
		Vector3 v = BotOwner_0.Position - player.Position;
		if (!(v.magnitude > 600f) && GClass855.IsAngLessNormalized(GClass855.NormalizeFastSelf(v), GClass855.NormalizeFastSelf(player.LookDirection), 0.9396926f))
		{
			HashSet_1.Add(player.Id);
			BotOwner_0.BotsGroup.AddEnemy(player, EBotEnemyCause.zryachiyLogic);
			if (BotOwner_0.EnemiesController.EnemyInfos.TryGetValue(player, out var value) && value is GClass541 gClass)
			{
				gClass.SetGetHitFrom();
			}
		}
	}

	public void method_2(DamageInfoStruct damage, EBodyPart arg2, float arg3)
	{
		method_3(damage);
	}

	public void method_3(DamageInfoStruct damageinfo)
	{
		IPlayerOwner player = damageinfo.Player;
		if (player != null && HashSet_1.Add(player.iPlayer.Id))
		{
			method_4(player.iPlayer);
			IPlayer[] groupPlayersToENemies = GClass3585.GroupPlayers(Singleton<GameWorld>.Instance, player.iPlayer.GroupId).ToArray();
			method_11(groupPlayersToENemies);
		}
	}

	public void method_4(IPlayer aggressor)
	{
		if (!aggressor.IsAI)
		{
			HashSet_1.Add(aggressor.Id);
			BotOwner_0.BotsGroup.AddEnemy(aggressor, EBotEnemyCause.zryachiyLogic);
			if (BotOwner_0.EnemiesController.EnemyInfos.TryGetValue(aggressor, out var value) && value is GClass541 gClass)
			{
				gClass.SetGetHitFrom();
			}
		}
	}

	public void method_5(DamageInfoStruct damage, EBodyPart part, float val)
	{
		method_3(damage);
	}

	public bool method_6(IPlayer player)
	{
		RadioTransmitterRecodableComponent radioTransmitterRecodableComponent = player.FindRadioTransmitter();
		if (radioTransmitterRecodableComponent != null)
		{
			return radioTransmitterRecodableComponent.Handler.Status == RadioTransmitterStatus.Green;
		}
		return false;
	}

	public void method_7(BotOwner follower, FollowerStatusChange st)
	{
		switch (st)
		{
		case FollowerStatusChange.Remove:
			follower.GetPlayer.BeingHitAction -= method_5;
			follower.GetPlayer.OnPlayerDead -= method_15;
			method_9();
			break;
		case FollowerStatusChange.Add:
			method_8(follower);
			break;
		}
	}

	public void method_8(BotOwner follower)
	{
		follower.GetPlayer.BeingHitAction += method_5;
		follower.GetPlayer.OnPlayerDead += method_15;
	}

	public void method_9()
	{
		if (Int_0 < 3)
		{
			Int_0++;
			BotOwner_0.AITaskManager.RegisterDelayedTask(BotOwner_0, 5f, delegate
			{
				BotWaveDataClass wave = new BotWaveDataClass
				{
					BotsCount = 1,
					Difficulty = BotDifficulty.normal,
					SpawnAreaName = BotOwner_0.BotsGroup.BotZone.name,
					Time = 61f,
					IsPlayers = false,
					WildSpawnType = WildSpawnType.followerZryachiy,
					Side = EPlayerSide.Savage,
					WithCheckMinMax = false
				};
				BotOwner_0.BotsGroup.BotGame.BotsController.BotSpawner.ActivateBotsByWave(wave).HandleExceptions();
			});
		}
	}

	public void method_10(IPlayer person, IPlayersCollection playersCollection)
	{
		Class177 CS_0024_003C_003E8__locals19 = new Class177();
		CS_0024_003C_003E8__locals19.ZyriachyBossLogicClass = this;
		CS_0024_003C_003E8__locals19.playersCollection = playersCollection;
		CS_0024_003C_003E8__locals19.person = person;
		CS_0024_003C_003E8__locals19.player = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(CS_0024_003C_003E8__locals19.person.ProfileId);
		CS_0024_003C_003E8__locals19.trRec = CS_0024_003C_003E8__locals19.player.RecodableItemsHandler.GetRecodableComponent<RadioTransmitterRecodableComponent>();
		if (CS_0024_003C_003E8__locals19.trRec == null || CS_0024_003C_003E8__locals19.trRec.Handler == null)
		{
			return;
		}
		CS_0024_003C_003E8__locals19.trRec.Handler.OnStatusChanged += delegate
		{
			// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
			if (!CS_0024_003C_003E8__locals19.ZyriachyBossLogicClass.method_6(CS_0024_003C_003E8__locals19.player))
			{
				CS_0024_003C_003E8__locals19.trRec.Handler.OnStatusChanged -= CS_0024_003C_003E8__locals19.method_0;
				IPlayer[] groupPlayersToENemies = GClass3585.GroupPlayers(CS_0024_003C_003E8__locals19.playersCollection, CS_0024_003C_003E8__locals19.person.GroupId).ToArray();
				CS_0024_003C_003E8__locals19.ZyriachyBossLogicClass.method_11(groupPlayersToENemies);
				CS_0024_003C_003E8__locals19.ZyriachyBossLogicClass.method_4(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(CS_0024_003C_003E8__locals19.person.ProfileId));
			}
		};
	}

	public void method_11(IPlayer[] groupPlayersToENemies)
	{
		if (!GClass398.Instance.IsTraceEnable() || groupPlayersToENemies == null)
		{
		}
		if (groupPlayersToENemies != null)
		{
			foreach (IPlayer player in groupPlayersToENemies)
			{
				method_4(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.ProfileId));
			}
		}
	}

	public void method_12(Player obj)
	{
		if (BotOwner_0.EnemiesController.IsEnemy(obj))
		{
			method_14();
		}
	}

	public void method_13(Player obj)
	{
		if (BotOwner_0.EnemiesController.IsEnemy(obj))
		{
			method_14();
		}
	}

	public void method_14()
	{
		int count = AiplaceInfo_0.GetCount((Player player) => BotOwner_0.EnemiesController.IsEnemy(player));
		IsManyEnemy = count > 1;
	}

	public void method_15(Player player, IPlayer lastaggressor, DamageInfoStruct damageinfo, EBodyPart part)
	{
		method_16(player, lastaggressor, damageinfo, part);
	}

	public void method_16(Player player, IPlayer lastaggressor, DamageInfoStruct damageinfo, EBodyPart part)
	{
		if (damageinfo.Player != null)
		{
			method_3(damageinfo);
		}
	}

	public void method_17(Player player, IPlayer lastaggressor, DamageInfoStruct damageinfo, EBodyPart part)
	{
		method_16(player, lastaggressor, damageinfo, part);
	}

	public override void Dispose()
	{
		Singleton<BotEventHandler>.Instance.OnBeingHit -= method_0;
		Singleton<BotEventHandler>.Instance.OnSoundPlayed -= method_1;
		BotOwner_0.Boss.OnFollowerStatusChange -= method_7;
		AiplaceInfo_0.OnPlayerEnter -= method_13;
		AiplaceInfo_0.OnPlayerExit -= method_12;
		BotOwner_0.GetPlayer.BeingHitAction -= method_2;
		BotOwner_0.GetPlayer.OnPlayerDead -= method_17;
		foreach (BotOwner follower in BotOwner_0.Boss.Followers)
		{
			follower.GetPlayer.BeingHitAction -= method_5;
			follower.GetPlayer.OnPlayerDead -= method_15;
		}
	}

	[CompilerGenerated]
	public void method_18(IPlayer info, BotOwner closestBot)
	{
		if ((closestBot.Position - info.Position).sqrMagnitude < Float_2)
		{
			AddEnemy(info);
		}
	}

	[CompilerGenerated]
	public void method_19()
	{
		BotWaveDataClass wave = new BotWaveDataClass
		{
			BotsCount = 1,
			Difficulty = BotDifficulty.normal,
			SpawnAreaName = BotOwner_0.BotsGroup.BotZone.name,
			Time = 61f,
			IsPlayers = false,
			WildSpawnType = WildSpawnType.followerZryachiy,
			Side = EPlayerSide.Savage,
			WithCheckMinMax = false
		};
		BotOwner_0.BotsGroup.BotGame.BotsController.BotSpawner.ActivateBotsByWave(wave).HandleExceptions();
	}

	[CompilerGenerated]
	public bool method_20(Player player)
	{
		return BotOwner_0.EnemiesController.IsEnemy(player);
	}
}
