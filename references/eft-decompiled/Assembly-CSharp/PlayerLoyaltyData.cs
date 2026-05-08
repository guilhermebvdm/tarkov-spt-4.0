using System;
using System.Collections.Generic;
using EFT;
using EFT.Counters;
using UnityEngine;

public class PlayerLoyaltyData
{
	[NonSerialized]
	public const float AGGRESSOR_PERIOD = 180f;

	[NonSerialized]
	public double EndAggressor;

	[NonSerialized]
	public Player Player;

	[NonSerialized]
	public Profile.FenceTraderInfo LoyaltySetting;

	[NonSerialized]
	public int RegularScavKills;

	[NonSerialized]
	public HashSet<string> BotPmcDamagedScavHashSet = new HashSet<string>();

	[NonSerialized]
	public float BotPmcAggressorStreak_1;

	[NonSerialized]
	public (double standing, EFenceStandingSource source) AggressorBonus;

	[NonSerialized]
	public Dictionary<string, bool> Enemies = new Dictionary<string, bool>();

	public float BotPmcAggressorStreak => BotPmcAggressorStreak_1;

	public bool ReactOnMarkOnUnknowns => LoyaltySetting?.FenceLoyalty.ReactOnMarkOnUnknowns ?? false;

	public bool HostileScavs => LoyaltySetting?.FenceLoyalty.HostileScavs ?? false;

	public bool BossNoAttack
	{
		get
		{
			Profile.FenceTraderInfo loyaltySetting = LoyaltySetting;
			if (loyaltySetting == null)
			{
				return false;
			}
			return !loyaltySetting.FenceLoyalty.HostileBosses;
		}
	}

	public float ChanceApplyStopRequest => LoyaltySetting?.FenceLoyalty.BotStopChance ?? 0f;

	public float ChanceApplyGetInCoverRequest => LoyaltySetting?.FenceLoyalty.BotGetInCoverChance ?? 0f;

	public float ChanceApplySilenceRequest => LoyaltySetting?.FenceLoyalty.BotApplySilenceChance ?? 0f;

	public float ChanceApplySpreadoutRequest => LoyaltySetting?.FenceLoyalty.BotSpreadoutChance ?? 0f;

	public float ChanceApplyHelpRequest => LoyaltySetting?.FenceLoyalty.BotHelpChance ?? 0f;

	public float ChanceApplyFollowRequest => LoyaltySetting?.FenceLoyalty.BotFollowChance ?? 0f;

	public bool ScavAttackSupport => LoyaltySetting?.FenceLoyalty.ScavAttackSupport ?? false;

	public bool IsAggressor => EndAggressor > EFTDateTimeClass.UtcNowUnix;

	[field: NonSerialized]
	public bool CanBeFreeKilled { get; set; }

	public PlayerLoyaltyData(Player player)
	{
		Player = player;
		LoyaltySetting = player.Profile.FenceInfo;
		CanBeFreeKilled = HostileScavs;
	}

	public void KillEnemy(Player victim)
	{
	}

	public void MarkAsAggressor(IPlayer victim)
	{
	}

	public void TryAddDamagedScavToHashSet(IPlayer victim)
	{
		if (BotPmcDamagedScavHashSet.Add(victim.ProfileId))
		{
			BotPmcAggressorStreak_1 += (float)victim.Profile.Info.Settings.AggressorBonus;
			_ = $"Added victim with id: {victim.Id}, agressorBonus: {victim.Profile.Info.Settings.AggressorBonus} to hashSet. New total aggressor bonus for player with id: {Player.Id} is now {BotPmcAggressorStreak}";
		}
	}

	public bool WasAttackedBy(IPlayer person)
	{
		bool value;
		return Enemies.TryGetValue(person.ProfileId, out value) && value;
	}

	public void MarkAsCanBeFreeKilled()
	{
		CanBeFreeKilled = true;
	}

	public void method_0(Player victim, string title, double standingDelta)
	{
		InfoClass info = victim.Profile.Info;
		FenceLoyaltyLevel fenceLoyalty = LoyaltySetting.FenceLoyalty;
		double standing = LoyaltySetting.Standing;
		IAIData aIData = victim.AIData;
		PlayerLoyaltyData loyalty = victim.Loyalty;
		_ = info.Settings;
		string message = $" {title}  VictimId: {victim.Id}  VictimName: {info.Nickname}  StandingDelta: {standingDelta}  AddStandingTo: {Player.Profile.Nickname}  InitialStanding: {standing}  HasMaxFenceLevel: {fenceLoyalty.IsMaxLevel}  HostileScavs: {fenceLoyalty.HostileScavs} victim.CanBeFreeKilled:{loyalty.CanBeFreeKilled}  victim.IsAI:{aIData.IsAI}  WasAttackedBy:{WasAttackedBy(victim)}";
		Player.ShowStringNotification(message);
	}

	public void method_1(Player victim)
	{
		InfoClass info = victim.Profile.Info;
		FenceLoyaltyLevel fenceLoyalty = LoyaltySetting.FenceLoyalty;
		double standing = LoyaltySetting.Standing;
		IAIData aIData = victim.AIData;
		PlayerLoyaltyData loyalty = victim.Loyalty;
		ProfileInfoSettingsClass settings = info.Settings;
		double num = settings.StandingForKill;
		if (BotSettingsRepoClass.IsPmcBot(victim.Profile.Info.Settings.Role))
		{
			method_5(loyalty);
			method_0(victim, "KillAsScav  PMC AGGRESSOR", loyalty.BotPmcAggressorStreak);
			return;
		}
		if (info.Side != EPlayerSide.Savage)
		{
			if (loyalty.IsAggressor)
			{
				method_6(loyalty);
				method_0(victim, "KillAsScav  PMC AGGRESSOR", loyalty.AggressorBonus.standing);
			}
			return;
		}
		if (BotSettingsRepoClass.IsHostileToEverybody(settings.Role))
		{
			method_0(victim, "Kill for free  CULTIST OR RAIDER", 0.0);
			return;
		}
		string groupId = Player.GroupId;
		if (!string.IsNullOrEmpty(groupId) && groupId == victim.GroupId)
		{
			method_0(victim, "Kill for free  TEAM KILL", 0.0);
			return;
		}
		if (!aIData.IsAI && loyalty.CanBeFreeKilled)
		{
			method_3(settings.AggressorBonus, EFenceStandingSource.TraitorKill);
			method_0(victim, "KillAsScav  BAD PLAYER_SCAV", settings.AggressorBonus);
			return;
		}
		if (fenceLoyalty.IsMaxLevel && BotSettingsRepoClass.IsBossOrFollower(settings.Role) && settings.Role != WildSpawnType.assaultGroup)
		{
			if (settings.StandingForKill != 0.0)
			{
				double standingDelta = num - (standing - fenceLoyalty.StandingRequired);
				method_3(standingDelta, EFenceStandingSource.BossKill);
				CanBeFreeKilled = true;
				method_0(victim, "KillAsScav  FRIENDLY BOSS", standingDelta);
			}
			return;
		}
		int num2;
		int num3;
		if (!WasAttackedBy(victim) && !loyalty.CanBeFreeKilled)
		{
			if (BotSettingsRepoClass.IsBossOrFollower(settings.Role))
			{
				num2 = ((settings.Role != WildSpawnType.assaultGroup) ? 1 : 0);
				if (num2 != 0)
				{
					num3 = 3;
					goto IL_01de;
				}
			}
			else
			{
				num2 = 0;
			}
			num3 = 2;
			goto IL_01de;
		}
		method_0(victim, "Kill for free", 0.0);
		return;
		IL_01de:
		EFenceStandingSource standingSource = (EFenceStandingSource)num3;
		if (num2 == 0)
		{
			RegularScavKills++;
		}
		double standingRequired = fenceLoyalty.StandingRequired;
		if (fenceLoyalty.IsMaxLevel)
		{
			double val = standing - standingRequired;
			float num4 = Mathf.Pow(2f, RegularScavKills);
			num -= Math.Min(val, num4);
		}
		method_3(num, standingSource);
		CanBeFreeKilled = true;
		method_0(victim, "KillAsScav  REGULAR BOSS OR SAVAGE OR PLAYER_SCAV", num);
	}

	public void method_2(Player victim)
	{
		InfoClass info = victim.Profile.Info;
		FenceLoyaltyLevel fenceLoyalty = LoyaltySetting.FenceLoyalty;
		double standing = LoyaltySetting.Standing;
		ProfileInfoSettingsClass settings = info.Settings;
		double num = settings.StandingForKill;
		bool num2 = BotSettingsRepoClass.IsBossOrFollower(settings.Role);
		EFenceStandingSource standingSource = (num2 ? EFenceStandingSource.BossKill : EFenceStandingSource.ScavKill);
		if (!num2)
		{
			RegularScavKills++;
		}
		double standingRequired = fenceLoyalty.StandingRequired;
		if (fenceLoyalty.IsMaxLevel)
		{
			double val = standing - standingRequired;
			float num3 = Mathf.Pow(2f, RegularScavKills);
			num -= Math.Min(val, num3);
		}
		method_3(num, standingSource);
		CanBeFreeKilled = true;
		method_0(victim, "KillAsScav  REGULAR BOSS OR SAVAGE OR PLAYER_SCAV", num);
	}

	public void method_3(double standingDelta, EFenceStandingSource standingSource)
	{
		if (LoyaltySetting != null)
		{
			LoyaltySetting.AddStanding(standingDelta, standingSource);
		}
	}

	public void method_4(ProfileInfoSettingsClass victimSettings)
	{
		double aggressorBonus = victimSettings.AggressorBonus;
		if (!(Math.Abs(aggressorBonus) < 1.0000000116860974E-07))
		{
			if (aggressorBonus > AggressorBonus.standing || !IsAggressor)
			{
				EFenceStandingSource item = ((!BotSettingsRepoClass.IsBossOrFollower(victimSettings.Role)) ? EFenceStandingSource.ScavHelp : EFenceStandingSource.BossHelp);
				AggressorBonus = (standing: aggressorBonus, source: item);
			}
			EndAggressor = EFTDateTimeClass.UtcNowUnix + 180.0;
		}
	}

	public void method_5(PlayerLoyaltyData loyaltyData)
	{
		if (LoyaltySetting != null && loyaltyData.BotPmcAggressorStreak > 0f)
		{
			LoyaltySetting.AddStanding(loyaltyData.BotPmcAggressorStreak, EFenceStandingSource.PmcBotKill);
		}
	}

	public void method_6(PlayerLoyaltyData loyaltyData)
	{
		if (LoyaltySetting != null)
		{
			var (num, standingSource) = loyaltyData.AggressorBonus;
			if (loyaltyData.IsAggressor && !GClass855.IsZero(num))
			{
				LoyaltySetting.AddStanding(num, standingSource);
			}
		}
	}
}
