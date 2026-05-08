using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.Game.Spawning;
using EFT.InventoryLogic;
using UnityEngine;

public abstract class GClass168 : GClass167
{
	[Serializable]
	[CompilerGenerated]
	public class Class228
	{
		public static readonly Class228 class228_0 = new Class228();

		public static Func<BodyPartCollider, bool> func_0;

		public bool method_0(BodyPartCollider x)
		{
			return x.BodyPartType == EBodyPart.Head;
		}
	}

	[NonSerialized]
	public const int Int_1 = 4;

	[NonSerialized]
	public const int Int_2 = 4;

	[NonSerialized]
	public AIPlaceInfoLogicZryachiy AiplaceInfoLogicZryachiy_0;

	[NonSerialized]
	public Dictionary<int, int> Dictionary_0 = new Dictionary<int, int>();

	[NonSerialized]
	[CompilerGenerated]
	public int? Nullable_1;

	public new int? Nullable_0
	{
		[CompilerGenerated]
		get
		{
			return Nullable_1;
		}
		[CompilerGenerated]
		set
		{
			Nullable_1 = value;
		}
	}

	public float SECONDS_CAN_TELERORT => BotOwner_0.Settings.FileSettings.Boss.BOSS_ZRYACHIY_TELEPORT_CAN_SECONDS_FROM_START;

	public float Single_0 => BotOwner_0.Settings.FileSettings.Boss.BOSS_ZRYACHIY_POSSIBLE_FOG;

	public GClass168(BotOwner bot, int priority)
		: base(bot, priority)
	{
		BotOwner_0.ShootData.OnTriggerPressed += method_17;
		List<AIPlaceInfo> places = BotOwner_0.BotsGroup.BotGame.BotsController.CoversData.AIPlaceInfoHolder.Places;
		if (bot.Boss.IamBoss)
		{
			foreach (AIPlaceInfo item in places)
			{
				if (item.InfoLogicAllEnemy is AIPlaceInfoLogicZryachiy { BossPositions: not false } aIPlaceInfoLogicZryachiy && aIPlaceInfoLogicZryachiy.CanTakeBy(BotOwner_0))
				{
					method_18(aIPlaceInfoLogicZryachiy, item);
					break;
				}
			}
			return;
		}
		foreach (AIPlaceInfo item2 in places)
		{
			if (item2.InfoLogicAllEnemy is AIPlaceInfoLogicZryachiy { FollowersPositions: not false } aIPlaceInfoLogicZryachiy2 && aIPlaceInfoLogicZryachiy2.CanTakeBy(BotOwner_0))
			{
				method_18(aIPlaceInfoLogicZryachiy2, item2);
				break;
			}
		}
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		data.PlaceInfo = Nullable_0;
		return base.FindPoint(data, p, checkCurrent);
	}

	public void method_14(Player player, bool lethal)
	{
		if (player == null)
		{
			return;
		}
		BodyPartCollider bodyPartCollider = player.GetPlayer.PlayerBones.BodyPartColliders.FirstOrDefault((BodyPartCollider x) => x.BodyPartType == EBodyPart.Head);
		if (bodyPartCollider == null)
		{
			bodyPartCollider = player.GetPlayer.PlayerBones.BodyPartColliders.FirstOrDefault();
		}
		if (!(bodyPartCollider == null))
		{
			AmmoTemplate currentAmmoTemplate = BotOwner_0.WeaponManager.CurrentWeapon.CurrentAmmoTemplate;
			if (currentAmmoTemplate != null)
			{
				DamageInfoStruct damageInfo = new DamageInfoStruct
				{
					DamageType = EDamageType.Bullet,
					Damage = currentAmmoTemplate.Damage,
					ArmorDamage = (float)currentAmmoTemplate.ArmorDamage / 100f,
					StaminaBurnRate = currentAmmoTemplate.StaminaBurnPerDamage,
					PenetrationPower = (lethal ? 100 : currentAmmoTemplate.PenetrationPower),
					Direction = UnityEngine.Random.onUnitSphere,
					HitCollider = bodyPartCollider.Collider,
					HitNormal = Vector3.zero,
					HitPoint = bodyPartCollider.Collider.transform.position,
					HittedBallisticCollider = bodyPartCollider,
					Player = Singleton<GameWorld>.Instance.GetEverExistedBridgeByProfileID(BotOwner_0.GetPlayer.ProfileId),
					IsForwardHit = true
				};
				bodyPartCollider.ApplyHit(damageInfo, ShotIdStruct.EMPTY_SHOT_ID);
			}
		}
	}

	public bool method_15()
	{
		float bOSS_ZRYACHIY_MIN_DIST_TO_TELEPORT = BotOwner_0.Settings.FileSettings.Boss.BOSS_ZRYACHIY_MIN_DIST_TO_TELEPORT;
		IPlayersCollection players = BotOwner_0.BotsGroup.BotGame.BotsController.Players;
		float num = bOSS_ZRYACHIY_MIN_DIST_TO_TELEPORT * bOSS_ZRYACHIY_MIN_DIST_TO_TELEPORT;
		foreach (IPlayer item in players)
		{
			if (!item.IsAI && !((item.Position - BotOwner_0.Position).sqrMagnitude >= num))
			{
				return false;
			}
		}
		if (BotOwner_0.BotFollower.HaveBoss)
		{
			if (Time.time - BotOwner_0.BotFollower.BossToFollow.Player().AIData.BotOwner.ActivateTime > SECONDS_CAN_TELERORT)
			{
				return false;
			}
			EnemyInfo enemyInfo = BotOwner_0.BotFollower.BossToFollow.CurEnemy();
			if (enemyInfo == null)
			{
				return true;
			}
			if (enemyInfo.Distance < bOSS_ZRYACHIY_MIN_DIST_TO_TELEPORT && method_16())
			{
				return false;
			}
			return true;
		}
		if (BotOwner_0.Boss.IamBoss)
		{
			if (Time.time - BotOwner_0.ActivateTime > SECONDS_CAN_TELERORT)
			{
				return false;
			}
			EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
			if (goalEnemy == null)
			{
				return true;
			}
			if (goalEnemy.Distance < bOSS_ZRYACHIY_MIN_DIST_TO_TELEPORT && method_16())
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public bool method_16()
	{
		return BotOwner_0.BotsGroup.BotGame.WeatherCurve.Fog >= Single_0;
	}

	public void method_17()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && !(goalEnemy.Distance < 4f))
		{
			int id = goalEnemy.Person.Id;
			int num = 1;
			if (Dictionary_0.ContainsKey(id))
			{
				num = Dictionary_0[id] + 1;
				Dictionary_0[id] = num;
			}
			else
			{
				Dictionary_0.Add(id, num);
			}
			if (num > 4)
			{
				method_14(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(goalEnemy.Person.ProfileId), lethal: true);
			}
		}
	}

	public override void Dispose()
	{
		BotOwner_0.ShootData.OnTriggerPressed -= method_17;
		if (AiplaceInfoLogicZryachiy_0 != null)
		{
			AiplaceInfoLogicZryachiy_0.ReleaseBy(BotOwner_0);
		}
		base.Dispose();
	}

	[CompilerGenerated]
	public bool method_18(AIPlaceInfoLogicZryachiy infoLogicZryachiy, AIPlaceInfo aiPlaceInfo)
	{
		AiplaceInfoLogicZryachiy_0 = infoLogicZryachiy;
		infoLogicZryachiy.TakeBy(BotOwner_0);
		Nullable_0 = aiPlaceInfo.AreaId;
		return true;
	}
}
