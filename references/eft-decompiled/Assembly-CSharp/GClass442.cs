using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.SynchronizableObjects;
using UnityEngine;

public class GClass442 : ABossLogic
{
	[Serializable]
	[CompilerGenerated]
	public class Class172
	{
		public static readonly Class172 class172_0 = new Class172();

		public static Func<BodyPartCollider, bool> func_0;

		public bool method_0(BodyPartCollider x)
		{
			return x.BodyPartType == EBodyPart.Head;
		}
	}

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public GClass25 Gclass25_0;

	[NonSerialized]
	public AIPlaceInfo AiplaceInfo_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public const float Float_1 = 90000f;

	[NonSerialized]
	public const float Float_2 = 120f;

	public const float SAVAGA_KARMRA_TO_SEARCH = 0.5f;

	[NonSerialized]
	public float Float_3 = 0.05f;

	[NonSerialized]
	public float Float_4 = 0.05f;

	[NonSerialized]
	public bool Bool_3;

	[NonSerialized]
	public const int Int_0 = 20;

	[NonSerialized]
	public const int Int_1 = 20;

	[NonSerialized]
	public const float Float_5 = 2500f;

	public const float SDIST_SAFE_PREWARM = 81f;

	public const float MINES_RADIUS = 50f;

	[NonSerialized]
	public List<AIMinePoint> List_0 = new List<AIMinePoint>();

	[NonSerialized]
	public EBossPartisanEnemyType EbossPartisanEnemyType_0;

	public EBossPartisanEnemyType PartisanEnemyType => EbossPartisanEnemyType_0;

	public GClass442(BotOwner owner, BotBoss bossLogic)
		: base(owner, bossLogic)
	{
		Gclass25_0 = new GClass25(10f, method_0);
		Bool_0 = GClass856.IsTrue100(120f);
	}

	public void method_0()
	{
		List<AIPlaceInfo> places = BotOwner_0.BotsGroup.BotGame.BotsController.CoversData.AIPlaceInfoHolder.Places;
		method_4();
		GameWorld instance = Singleton<GameWorld>.Instance;
		float num = float.MaxValue;
		bool flag = false;
		if (BotOwner_0.Memory.HaveEnemy && Time.time - BotOwner_0.Memory.GoalEnemy.PersonalLastSeenTime < 30f)
		{
			return;
		}
		foreach (AIPlaceInfo item in places)
		{
			if (!(item.InfoLogicAllEnemy is AIPlaceLogicPartisan aIPlaceLogicPartisan))
			{
				continue;
			}
			foreach (string idsOfVisitedPlayer in aIPlaceLogicPartisan.IdsOfVisitedPlayers)
			{
				float sqrMagnitude = (aIPlaceLogicPartisan.transform.position - BotOwner_0.Position).sqrMagnitude;
				Player alivePlayerByProfileID = instance.GetAlivePlayerByProfileID(idsOfVisitedPlayer);
				if (alivePlayerByProfileID != null && CanPersuit(alivePlayerByProfileID, BotOwner_0.Settings.FileSettings) && BotOwner_0.EnemiesController.EnemyInfos.TryGetValue(alivePlayerByProfileID, out var _))
				{
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						AiplaceInfo_0 = item;
					}
					BotOwner_0.BotsGroup.SetEnemyPos(alivePlayerByProfileID, alivePlayerByProfileID.Transform.position, alivePlayerByProfileID.WeaponRoot.position, EEnemyPartVisibleType.Sence);
					EbossPartisanEnemyType_0 = EBossPartisanEnemyType.PMCatMyZone;
					flag = true;
				}
			}
		}
		if (flag)
		{
			return;
		}
		IPlayer player = null;
		IPlayer player2 = null;
		float num2 = float.MaxValue;
		player = null;
		player2 = null;
		double num3 = 3.4028234663852886E+38;
		foreach (Player allAlivePlayers in instance.AllAlivePlayersList)
		{
			if (CanPersuit(allAlivePlayers, BotOwner_0.Settings.FileSettings) && allAlivePlayers.Profile.Side != EPlayerSide.Savage && allAlivePlayers.Profile.KarmaValue < 0.5f)
			{
				float num4 = BotOwner_0.SDistTo(allAlivePlayers.Position);
				if (!(num4 > 90000f) && num4 < num2)
				{
					num2 = num4;
					player = allAlivePlayers;
				}
			}
		}
		if (player != null)
		{
			if (!BotOwner_0.BotsGroup.IsEnemy(player))
			{
				BotOwner_0.BotsGroup.AddEnemy(player, EBotEnemyCause.partisanBadKarma);
			}
			EbossPartisanEnemyType_0 = EBossPartisanEnemyType.BadSavage;
			BotOwner_0.BotsGroup.SetEnemyPos(player, player.Transform.position, player.WeaponRoot.position, EEnemyPartVisibleType.Sence);
			return;
		}
		num2 = float.MaxValue;
		player = null;
		player2 = null;
		num3 = 3.4028234663852886E+38;
		foreach (Player allAlivePlayers2 in instance.AllAlivePlayersList)
		{
			if (CanPersuit(allAlivePlayers2, BotOwner_0.Settings.FileSettings) && allAlivePlayers2.Profile.Side == EPlayerSide.Savage && allAlivePlayers2.Profile.KarmaValue < 0.5f)
			{
				float num5 = BotOwner_0.SDistTo(allAlivePlayers2.Position);
				if ((double)allAlivePlayers2.Profile.KarmaValue < num3)
				{
					num3 = allAlivePlayers2.Profile.KarmaValue;
					player2 = allAlivePlayers2;
				}
				if (!(num5 > 90000f) && num5 < num2)
				{
					num2 = num5;
					player = allAlivePlayers2;
				}
			}
		}
		float float_ = Float_3;
		if (player2 != null && num3 < (double)float_)
		{
			if (!BotOwner_0.BotsGroup.IsEnemy(player2))
			{
				BotOwner_0.BotsGroup.AddEnemy(player2, EBotEnemyCause.partisanBadKarma);
			}
			Bool_1 = true;
			EbossPartisanEnemyType_0 = EBossPartisanEnemyType.ZeroSavage;
			BotOwner_0.BotsGroup.SetEnemyPos(player2, player2.Transform.position, player2.WeaponRoot.position, EEnemyPartVisibleType.Sence);
		}
		else if (player != null)
		{
			if (!BotOwner_0.BotsGroup.IsEnemy(player))
			{
				BotOwner_0.BotsGroup.AddEnemy(player, EBotEnemyCause.partisanBadKarma);
			}
			EbossPartisanEnemyType_0 = EBossPartisanEnemyType.BadSavage;
			BotOwner_0.BotsGroup.SetEnemyPos(player, player.Transform.position, player.WeaponRoot.position, EEnemyPartVisibleType.Sence);
		}
	}

	public static bool CanPersuit(IPlayer player, BotSettingsComponents bot)
	{
		if (player.IsAI)
		{
			if (bot == null)
			{
				return false;
			}
			if (bot.Mind.ENEMY_BOT_TYPES.Contains(player.Profile.Info.Settings.Role))
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public override void SetPatrolMode()
	{
	}

	public override void Activate()
	{
		Singleton<BotEventHandler>.Instance.OnPlantTripwire += method_5;
		Singleton<BotEventHandler>.Instance.OnTripwireChangeState += method_3;
		BotOwner_0.WeaponManager.Melee.OnEnemyHitted += method_1;
		_ = BotOwner_0.WeaponManager.Melee.HaveMelee;
	}

	public void method_1(BotOwner arg1, Player target)
	{
		method_2(target, lethal: false);
	}

	public void method_2(Player player, bool lethal)
	{
		if (player == null || Time.time - Float_4 < 1f)
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
				Float_4 = Time.time;
				DamageInfoStruct damageInfo = new DamageInfoStruct
				{
					DamageType = EDamageType.Undefined,
					Damage = 20f,
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

	public void method_3(TripwireSynchronizableObject obj)
	{
		if (obj.TripwireState == ETripwireState.Active && obj.PlacerPlayerId.Equals(BotOwner_0.GetPlayer.Profile.ProfileId))
		{
			BotOwner_0.BotTalk.Say(EPhraseTrigger.Mine, sayImmediately: true);
		}
	}

	public void method_4()
	{
		if (Bool_3)
		{
			return;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return;
		}
		Bool_3 = true;
		List<Player> allAlivePlayersList = Singleton<GameWorld>.Instance.AllAlivePlayersList;
		List<AIMinePoint> minesForRewarm = BotOwner_0.BotsGroup.BotGame.BotsController.CoversData.AIMinesPositions.MinesForRewarm;
		_ = BotOwner_0.BotsGroup.BotGame.BotsController.CoversData.AIMinesPositions.Mines;
		int num = 0;
		foreach (AIMinePoint item in minesForRewarm)
		{
			if (num > 20)
			{
				break;
			}
			if ((item.Position - goalEnemy.CurrPosition).sqrMagnitude > 2500f)
			{
				continue;
			}
			bool flag = true;
			foreach (Player item2 in allAlivePlayersList)
			{
				if (!item2.IsAI && !((item.Position - item2.Position).sqrMagnitude >= 81f))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				List_0.Add(item);
				num++;
			}
		}
	}

	public override void BossLogicUpdate()
	{
		Gclass25_0.Update();
		if (List_0.Count > 0)
		{
			AIMinePoint aIMinePoint = List_0[0];
			List_0.Remove(aIMinePoint);
			BotOwner_0.MinesData.PlantHereNow(aIMinePoint);
		}
	}

	public override void Dispose()
	{
		BotOwner_0.WeaponManager.Melee.OnEnemyHitted -= method_1;
		Singleton<BotEventHandler>.Instance.OnPlantTripwire -= method_5;
		Singleton<BotEventHandler>.Instance.OnTripwireChangeState -= method_3;
	}

	public void method_5(TripwireSynchronizableObject throwweap, Vector3 position)
	{
	}

	public int EnemiesAtClosestsZone()
	{
		if (AiplaceInfo_0 == null)
		{
			return 0;
		}
		return AiplaceInfo_0.GetCount();
	}
}
