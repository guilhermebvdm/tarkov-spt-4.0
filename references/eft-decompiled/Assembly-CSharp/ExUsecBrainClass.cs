using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using UnityEngine;

public class ExUsecBrainClass : GClass326
{
	[CompilerGenerated]
	public class Class233
	{
		public EnemyInfo enemy;

		public Vector3 method_0()
		{
			return enemy.CurrPosition;
		}
	}

	[CompilerGenerated]
	public class Class234
	{
		public KeyValuePair<IPlayer, EnemyInfo> enemyInfo;

		public Vector3 method_0()
		{
			return enemyInfo.Value.CurrPosition;
		}
	}

	[NonSerialized]
	public const float Float_0 = 2500f;

	[NonSerialized]
	public const float Float_1 = 45f;

	[NonSerialized]
	public const float Float_2 = 60f;

	[NonSerialized]
	public const float Float_3 = 50f;

	[NonSerialized]
	public const float Float_4 = 3f;

	[NonSerialized]
	public const float Float_5 = 10f;

	[NonSerialized]
	public const float Float_6 = 250f;

	[NonSerialized]
	public const int Int_0 = 1;

	[NonSerialized]
	public const int Int_1 = 2;

	[NonSerialized]
	public const int Int_2 = 3;

	[NonSerialized]
	public const int Int_3 = 4;

	[NonSerialized]
	public const int Int_4 = 5;

	[NonSerialized]
	public const int Int_5 = 6;

	[NonSerialized]
	public const int Int_6 = 7;

	[NonSerialized]
	public const int Int_7 = 8;

	[NonSerialized]
	public const int Int_8 = 9;

	[NonSerialized]
	public const int Int_9 = 10;

	[NonSerialized]
	public const int Int_10 = 11;

	[NonSerialized]
	public const int Int_11 = 12;

	[NonSerialized]
	public const int Int_12 = 13;

	[NonSerialized]
	public const int Int_13 = 15;

	[NonSerialized]
	public const int Int_14 = 17;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public float Float_8;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public bool Bool_3;

	[NonSerialized]
	public StationaryWeaponLink StationaryWeaponLink_0;

	[NonSerialized]
	public HashSet<IPlayer> HashSet_1 = new HashSet<IPlayer>();

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public bool Bool_5;

	[NonSerialized]
	public float Float_9;

	public AbstractSuppressStationary AbstractSuppressStationary_0 => Owner.SuppressStationary.CurUsingLogic;

	public ExUsecBrainClass(BotOwner owner)
		: base(owner)
	{
		Bool_0 = false;
		method_6();
		if (Owner.WeaponManager.Stationary.TryFindClosests(Owner.Position, withSetTarget: true, out StationaryWeaponLink_0))
		{
			Bool_0 = true;
			Bool_1 = true;
			Owner.GetPlayer.BeingHitAction += method_26;
			BotHearingSensor hearingSensor = Owner.HearingSensor;
			hearingSensor.OnEnemySounHearded = (GDelegate7)Delegate.Combine(hearingSensor.OnEnemySounHearded, new GDelegate7(method_18));
			Bool_2 = StationaryWeaponLink_0.IsGrenade();
			Bool_3 = true;
		}
		if (Singleton<BotEventHandler>.Instantiated)
		{
			Singleton<BotEventHandler>.Instance.OnBeingHit += method_17;
		}
		method_11();
		GClass48 layer = new GClass48(owner, 100);
		method_0(5, layer, activeOnStart: true);
		GClass88 layer2 = new GClass88(owner, 95);
		method_0(17, layer2, activeOnStart: false);
		GClass83 layer3 = new GClass83(owner, 80);
		method_0(15, layer3, activeOnStart: true);
		GClass81 layer4 = new GClass81(owner, 75);
		method_0(10, layer4, Bool_0);
		GClass118 layer5 = new GClass118(owner, 73);
		method_0(11, layer5, activeOnStart: true);
		GClass84 layer6 = new GClass84(owner, 70);
		method_0(9, layer6, activeOnStart: true);
		GClass166 layer7 = new GClass166(owner, 65);
		method_0(12, layer7, activeOnStart: true);
		GClass142 layer8 = new GClass142(owner, 60);
		method_0(1, layer8, activeOnStart: true);
		GClass39 layer9 = new GClass39(owner, 50);
		method_0(6, layer9, activeOnStart: true);
		GClass139 layer10 = new GClass139(owner, 30);
		method_0(4, layer10, activeOnStart: true);
		Class99 layer11 = new Class99(owner, 9, withSearch: false);
		method_0(3, layer11, activeOnStart: true);
		GClass132 layer12 = new GClass132(owner, 3);
		method_0(8, layer12, activeOnStart: true);
		GClass86 layer13 = new GClass86(owner, 2);
		method_0(7, layer13, activeOnStart: true);
		GClass133 layer14 = new GClass133(owner, 0);
		method_0(2, layer14, activeOnStart: true);
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, 85, 45, 86, 27);
	}

	public override void ManualUpdate()
	{
		base.ManualUpdate();
		method_16();
		method_19();
		method_23();
		method_20();
	}

	public override string ShortName()
	{
		return "ExUsec";
	}

	public void method_11()
	{
		foreach (AIPlaceInfo place in Owner.BotsGroup.BotGame.BotsController.CoversData.AIPlaceInfoHolder.Places)
		{
			if (place.IsInside && place.InfoLogicAllEnemy is AIPlaceInfoLogicExUsecAttack aIPlaceInfoLogicExUsecAttack && aIPlaceInfoLogicExUsecAttack.ConnectedZone != null && aIPlaceInfoLogicExUsecAttack.ConnectedZone.NameZone.Equals(Owner.BotsGroup.BotZone.NameZone))
			{
				place.OnPlayerEnter += method_15;
				place.OnPlayerExit += method_13;
			}
		}
		Owner.BotsGroup.OnEnemyRemove += method_12;
	}

	public void method_12(IPlayer obj)
	{
		method_14(obj);
	}

	public void method_13(Player obj)
	{
		method_14(obj);
	}

	public void method_14(IPlayer person)
	{
		HashSet_1.Remove(person);
		if (HashSet_1.Count == 0)
		{
			Bool_4 = true;
			Float_9 = Time.time + 15f;
		}
	}

	public void method_15(Player obj)
	{
		if (!obj.AIData.IsAI)
		{
			Bool_4 = false;
			HashSet_1.Add(obj);
			method_1(17);
			CallEnemyTargetInfo trg = new CallEnemyTargetInfo(obj.Position, obj.AIData.EnvironmentId);
			Owner.CalledData.TryCall(trg, Owner);
			UnityEngine.Debug.DrawRay(obj.Position, Vector3.up * 100f, Color.yellow, 3f);
			Bool_5 = true;
			if (Owner.EnemiesController.EnemyInfos.TryGetValue(obj, out var value))
			{
				value.SetVisible(value: true);
			}
		}
	}

	public void method_16()
	{
		if (Bool_4 && Float_9 < Time.time)
		{
			Bool_4 = false;
			method_3(17);
			Bool_5 = false;
		}
	}

	public void method_17(DamageInfoStruct damageinfo, Player victim)
	{
		IPlayerOwner player = damageinfo.Player;
		if (player == null)
		{
			return;
		}
		if (victim.IsAI && victim.Profile.Info.Settings.Role == WildSpawnType.exUsec)
		{
			Owner.BotsGroup.CheckAndAddEnemy(player.iPlayer, ignoreAI: true);
		}
		else
		{
			if (Owner.BotsController.BotTradersServices.LighthouseKeeperServices.IsPlayerExUsecFriendly(player.iPlayer) || victim.Profile.Side != EPlayerSide.Usec || player.iPlayer.Profile.Side == EPlayerSide.Usec)
			{
				return;
			}
			Player alivePlayerByProfileID = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.iPlayer.ProfileId);
			if (!player.AIData.IsAI || (alivePlayerByProfileID != null && alivePlayerByProfileID.Profile.Info.Settings.Role == WildSpawnType.assault))
			{
				if ((victim.Position - Owner.Position).sqrMagnitude < 2500f)
				{
					Owner.BotsGroup.CheckAndAddEnemy(player.iPlayer, ignoreAI: true);
				}
				else if ((player.iPlayer.Position - Owner.Position).sqrMagnitude < 2500f)
				{
					Owner.BotsGroup.CheckAndAddEnemy(player.iPlayer, ignoreAI: true);
				}
			}
		}
	}

	public void method_18(Vector3 pos, float dist, AISoundType type)
	{
		if (dist < 10f)
		{
			method_25();
		}
	}

	public void method_19()
	{
		EnemyInfo goalEnemy = Owner.Memory.GoalEnemy;
		if (goalEnemy != null && goalEnemy.IsVisible)
		{
			float num = Time.time - goalEnemy.LastChangeVisionTime;
			if (goalEnemy.Distance < 50f && num > 3f)
			{
				method_24(val: false);
			}
		}
	}

	public void method_20()
	{
		if (Bool_3 && Float_8 < Time.time)
		{
			Float_8 = Time.time + 7f;
			if (Bool_2)
			{
				method_22();
			}
			else
			{
				method_21();
			}
		}
	}

	public void method_21()
	{
		EnemyInfo enemy = Owner.Memory.GoalEnemy;
		if (enemy == null || enemy.CanShoot || AbstractSuppressStationary_0.IsReady() || !AbstractSuppressStationary_0.IsAtSector(enemy.CurrPosition))
		{
			return;
		}
		float num = Time.time - enemy.PersonalLastSeenTime;
		if (num < 7f && num > 3f)
		{
			AbstractSuppressStationary_0.SetPeriod(2f);
			Owner.SuppressStationary.SetTargetGetter(() => enemy.CurrPosition);
		}
	}

	public void method_22()
	{
		EnemyInfo goalEnemy = Owner.Memory.GoalEnemy;
		if ((goalEnemy != null && goalEnemy.CanShoot) || AbstractSuppressStationary_0.IsReady())
		{
			return;
		}
		foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in Owner.EnemiesController.EnemyInfos)
		{
			if (enemyInfo.Value.Distance > 250f)
			{
				continue;
			}
			IAIData aIData = enemyInfo.Value.Person.AIData;
			if (aIData.PlaceInfo == null || !aIData.PlaceInfo.AtrZone)
			{
				continue;
			}
			Vector3 currPosition = enemyInfo.Value.CurrPosition;
			if (AbstractSuppressStationary_0.CanStartSuppressAt(currPosition))
			{
				AbstractSuppressStationary_0.SetPeriod(10f);
				Owner.SuppressStationary.SetTargetGetter(() => enemyInfo.Value.CurrPosition, enemyInfo.Value);
				break;
			}
		}
	}

	public void method_23()
	{
		if (Owner.Brain.LastDecision.HasValue && Owner.Brain.LastDecision.Value == BotLogicDecision.warnPlayer)
		{
			Bool_1 = false;
			Float_7 = 0f;
		}
		else if (Bool_0 && !Bool_1)
		{
			float num = Time.time - Float_7;
			EnemyInfo goalEnemy = Owner.Memory.GoalEnemy;
			if (num > 45f && goalEnemy == null)
			{
				method_24(val: true);
			}
			else if (num > 60f && !goalEnemy.IsVisible && Time.time - goalEnemy.TimeLastSeen > 60f)
			{
				method_24(val: true);
			}
		}
	}

	public void method_24(bool val)
	{
		if (val == Bool_1)
		{
			return;
		}
		Bool_1 = val;
		if (val)
		{
			method_1(10);
			if (Owner.WeaponManager.Stationary.CurLink == null && StationaryWeaponLink_0.IsFree(Owner.Id))
			{
				Owner.WeaponManager.Stationary.SetTargetStationary(StationaryWeaponLink_0);
			}
		}
		else
		{
			Float_7 = Time.time;
			method_3(10);
			Owner.Brain.BaseBrain.CalcActionNextFrame();
		}
	}

	public void method_25()
	{
		EnemyInfo goalEnemy = Owner.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			method_24(val: false);
		}
		else if (Owner.WeaponManager.Stationary != null && Owner.WeaponManager.Stationary.CheckeEnemyOutOfSector(goalEnemy))
		{
			method_24(val: false);
		}
	}

	public void method_26(DamageInfoStruct damageInfo, EBodyPart arg2, float arg3)
	{
		method_25();
	}

	public override void Dispose()
	{
		method_27();
		Singleton<BotEventHandler>.Instance.OnBeingHit -= method_17;
		Owner.GetPlayer.BeingHitAction -= method_26;
		BotHearingSensor hearingSensor = Owner.HearingSensor;
		hearingSensor.OnEnemySounHearded = (GDelegate7)Delegate.Remove(hearingSensor.OnEnemySounHearded, new GDelegate7(method_18));
		base.Dispose();
	}

	public void method_27()
	{
		foreach (AIPlaceInfo place in Owner.BotsGroup.BotGame.BotsController.CoversData.AIPlaceInfoHolder.Places)
		{
			if (place.IsInside && place.InfoLogicAllEnemy is AIPlaceInfoLogicExUsecAttack aIPlaceInfoLogicExUsecAttack && aIPlaceInfoLogicExUsecAttack.ConnectedZone != null && aIPlaceInfoLogicExUsecAttack.ConnectedZone.NameZone.Equals(Owner.BotsGroup.BotZone.NameZone))
			{
				place.OnPlayerEnter += method_15;
				place.OnPlayerExit += method_13;
			}
		}
		Owner.BotsGroup.OnEnemyRemove -= method_12;
	}
}
