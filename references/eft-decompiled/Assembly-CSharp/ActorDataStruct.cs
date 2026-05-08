using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BitPacking;
using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using UnityEngine;

public struct ActorDataStruct
{
	[CompilerGenerated]
	public class Class131
	{
		public ActorDataStruct cache;

		public IPlayerOwner player;

		public void method_0(IPlayer obj)
		{
			// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
			cache.IsLeavedOrDead = true;
			player.iPlayer.OnIPlayerDeadOrUnspawn -= method_0;
		}
	}

	public const int MAX_HEALTH = 16000;

	public const int MAX_WAY_POINTS = 8;

	public BotDataStruct BotData;

	public GStruct12 AIData;

	public GStruct17 HeathsData;

	public GStruct13 CoverStruct;

	public GStruct14 VisionData;

	public float Dist;

	public IPlayerOwner PlayerOwner;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	public bool CanLookThoughtGrass;

	public bool InBuildingTarget;

	public bool InBuildingMe;

	public bool CanShoot;

	public bool InitedBotData => BotData.Inited;

	public string Name => BotData.Name;

	public EBotState Status => BotData.Status;

	public int MainTactic => BotData.MainTactic;

	public string StrategyName => BotData.StrategyName;

	public int Ammo => BotData.Ammo;

	public string LookDirType => BotData.LookDirType;

	public string Target => BotData.Target;

	public float VisibleDist => BotData.VisibleDist;

	public float GreenDist => BotData.GreenDist;

	public int HourServer => BotData.HourServer;

	public bool InCover => BotData.InCover;

	public byte GreenType => BotData.GreenType;

	public int CoverIndex => BotData.CoverIndex;

	public bool UnderFire => BotData.UnderFire;

	public bool TriangleLoad => BotData.TriangleLoad;

	public bool IsInSpawnWeapon => BotData.IsInSpawnWeapon;

	public bool Reloading => BotData.Reloading;

	public bool HaveAxeEnemy => BotData.HaveAxeEnemy;

	public bool IsProblem => BotData.IsProblem;

	public int Request => BotData.Request;

	public int HealthHead => HeathsData.HealthHead;

	public int HealthBody => HeathsData.HealthBody;

	public int HealthStomach => HeathsData.HealthStomach;

	public int HealthLeftArm => HeathsData.HealthLeftArm;

	public int HealthRightArm => HeathsData.HealthRightArm;

	public int HealthLeftLeg => HeathsData.HealthLeftLeg;

	public int HealthRightLeg => HeathsData.HealthRightLeg;

	public int MaxHealthHead => HeathsData.MaxHealthHead;

	public int MaxHealthBody => HeathsData.MaxHealthBody;

	public int MaxHealthStomach => HeathsData.MaxHealthStomach;

	public int MaxHealthLeftArm => HeathsData.MaxHealthLeftArm;

	public int MaxHealthRightArm => HeathsData.MaxHealthRightArm;

	public int MaxHealthLeftLeg => HeathsData.MaxHealthLeftLeg;

	public int MaxHealthRightLeg => HeathsData.MaxHealthRightLeg;

	public bool HealthHeadBL => HeathsData.HealthHeadBL;

	public bool HealthBodyBL => HeathsData.HealthBodyBL;

	public bool HealthStomachBL => HeathsData.HealthStomachBL;

	public bool HealthLeftArmBL => HeathsData.HealthLeftArmBL;

	public bool HealthRightArmBL => HeathsData.HealthRightArmBL;

	public bool HealthLeftLegBL => HeathsData.HealthLeftLegBL;

	public bool HealthRightLegBL => HeathsData.HealthRightLegBL;

	public bool HealthRightLegBroken => HeathsData.HealthRightLegBroken;

	public bool HealthHeadBroken => HeathsData.HealthHeadBroken;

	public bool HealthBodyBroken => HeathsData.HealthBodyBroken;

	public bool HealthStomachBroken => HeathsData.HealthStomachBroken;

	public bool HealthLeftArmBroken => HeathsData.HealthLeftArmBroken;

	public bool HealthRightArmBroken => HeathsData.HealthRightArmBroken;

	public bool HealthLeftLegBroken => HeathsData.HealthLeftLegBroken;

	public bool Shooting => BotData.Shooting;

	public bool HaveGrenade => BotData.HaveGrenade;

	public bool HaveUnderbarrelLauncher => BotData.HaveUnderbarrelLauncher;

	public bool UnderbarrelLauncherActive => BotData.UnderbarrelLauncherActive;

	public bool UnderbarrelLauncherHasAmmoInChamber => BotData.UnderbarrelLauncherHasAmmoInChamber;

	public bool IsFlashed => BotData.IsFlashed;

	public bool IsParametesChanged => BotData.IsParametesChanged;

	public bool IsLay => BotData.IsLay;

	public bool IsInPronePos => BotData.IsInPronePos;

	public float PoseLevel => BotData.PoseLevel;

	public bool IsGoalEnemyVisible => BotData.IsGoalEnemyVisible;

	public float BaseVisibilityChangeSpeed => BotData.BaseVisibilityChangeSpeed;

	public float AccuratySpeed => BotData.AccuratySpeed;

	public float ScatteringPerMeter => BotData.ScatteringPerMeter;

	public float RuntimeVisionEffectK => BotData.RuntimeVisionEffectK;

	public float RuntimeVisionEffectKCur => BotData.RuntimeVisionEffectKCur;

	public float ScatteringPerMeterCur => BotData.ScatteringPerMeterCur;

	public bool ShallRunIfNoAmmo => BotData.ShallRunIfNoAmmo;

	public string LayerName => BotData.LayerName;

	public string NodeName => BotData.NodeName;

	public int NodeRepeats => BotData.NodeRepeats;

	public string PrevNodeName => BotData.PrevNodeName;

	public string Reason => BotData.Reason;

	public string PrevNodeExitReason => BotData.PrevNodeExitReason;

	public string CustomData => BotData.CustomData;

	public int BotRole => BotData.BotRole;

	public int BotDifficulty => BotData.BotDifficulty;

	public BotStandByType StandBy => BotData.StandBy;

	public bool UseMeds => BotData.UseMeds;

	public bool HaveMeds => BotData.HaveMeds;

	public bool Damaged => BotData.Damaged;

	public bool HaveStimulator => BotData.HaveStimulator;

	public bool UseStimulator => BotData.UseStimulator;

	public bool HaveSurgical => BotData.HaveSurgical;

	public bool UseSurgical => BotData.UseSurgical;

	public int HitsOnMe => BotData.HitsOnMe;

	public int ShootsOnMe => BotData.ShootsOnMe;

	public int Id => BotData.Id;

	public bool PatrolPointStatus => BotData.PatrolPointStatus;

	public bool PatrolWayStatus => BotData.PatrolWayStatus;

	public Vector3 TargetPoint => BotData.TargetPoint;

	public Vector3 PositionOnWay => BotData.PositionOnWay;

	public int WayPointsCount => BotData.WayPoints.Length;

	public Vector3[] WayPoints => BotData.WayPoints;

	public int BotMoverCurrentState => BotData.BotMoverCurrentState;

	public Vector3 DoorOpenerCurrentDoorPosition => BotData.DoorOpenerCurrentDoorPosition;

	public string ProfileId => PlayerOwner?.iPlayer.ProfileId;

	public bool AnyInited
	{
		get
		{
			if (!BotData.Inited && !AIData.Inited)
			{
				return HeathsData.Inited;
			}
			return true;
		}
	}

	public EPlayerState PlayerState => BotData.PlayerState;

	public bool IsLeavedOrDead
	{
		[CompilerGenerated]
		readonly get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public static void Serialize(ref ActorDataStruct data, ISerializer stream)
	{
		smethod_3(ref data, stream);
		smethod_4(ref data, stream);
		smethod_1(ref data, stream);
		smethod_2(ref data, stream);
		smethod_0(ref data, stream);
	}

	public static void smethod_0(ref ActorDataStruct data, ISerializer stream)
	{
		string value = ((data.PlayerOwner != null) ? data.ProfileId : "no id");
		stream.SerializeLimitedString(ref value, ' ', 'z', BitPackingTag.DebugBotStructProfileId, 1600u);
		if (stream.StreamMode != EBitStreamMode.Reading)
		{
			return;
		}
		GameWorld instance = Singleton<GameWorld>.Instance;
		if (!(instance != null))
		{
			return;
		}
		Class131 CS_0024_003C_003E8__locals8 = new Class131();
		CS_0024_003C_003E8__locals8.player = instance.GetAlivePlayerBridgeByProfileID(value);
		if (CS_0024_003C_003E8__locals8.player != null)
		{
			data.PlayerOwner = CS_0024_003C_003E8__locals8.player;
			CS_0024_003C_003E8__locals8.cache = data;
			CS_0024_003C_003E8__locals8.player.iPlayer.OnIPlayerDeadOrUnspawn += delegate
			{
				// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
				CS_0024_003C_003E8__locals8.cache.IsLeavedOrDead = true;
				CS_0024_003C_003E8__locals8.player.iPlayer.OnIPlayerDeadOrUnspawn -= CS_0024_003C_003E8__locals8.method_0;
			};
		}
	}

	public static void smethod_1(ref ActorDataStruct data, ISerializer stream)
	{
		stream.Serialize(ref data.AIData.Inited);
		stream.Serialize(ref data.AIData.PowerOfEquipment);
		stream.Serialize(ref data.AIData.PlaceInfoId);
		stream.Serialize(ref data.AIData.IsInTree);
		stream.Serialize(ref data.AIData.Nickname, 1600u);
		stream.Serialize(ref data.AIData.IsAggressor);
		stream.Serialize(ref data.AIData.BossNoAttack);
		stream.Serialize(ref data.AIData.ScavAttacker);
		stream.Serialize(ref data.AIData.CanBeFreeKilled);
		stream.Serialize(ref data.AIData.IsPursuitable);
		stream.Serialize(ref data.AIData.IsAxe);
		stream.SerializeLimitedString(ref data.AIData.shotNameWeapon, ' ', 'z', BitPackingTag.DebugBotStructShotNameWeapon, 1600u);
		stream.SerializeLimitedString(ref data.AIData.currentOpName, ' ', 'z', BitPackingTag.const_196, 1600u);
		stream.Serialize(ref data.AIData.hashName, 1600u);
		stream.Serialize(ref data.AIData.IsAI);
		stream.Serialize(ref data.AIData.HeadPosition);
	}

	public static void smethod_2(ref ActorDataStruct data, ISerializer stream)
	{
		stream.Serialize(ref data.CoverStruct.IdBot);
	}

	public static void smethod_3(ref ActorDataStruct data, ISerializer stream)
	{
		stream.Serialize(ref data.BotData.Inited);
		stream.SerializeLimitedString(ref data.BotData.Name, ' ', 'z', BitPackingTag.DebugBotStructName, 1600u);
		stream.Serialize(ref data.BotData.Status);
		stream.SerializeLimitedInt32(ref data.BotData.Request, 0, 8, BitPackingTag.DebugBotStruct2);
		stream.SerializeLimitedInt32(ref data.BotData.MainTactic, 0, 8, BitPackingTag.DebugBotStruct3);
		stream.Serialize(ref data.BotData.StrategyName, 1600u);
		stream.SerializeLimitedInt32(ref data.BotData.Ammo, -1, 255, BitPackingTag.DebugBotStruct4);
		stream.SerializeLimitedString(ref data.BotData.Target, ' ', 'z', BitPackingTag.DebugBotStructTarget, 1600u);
		stream.Serialize(ref data.BotData.InCover);
		stream.Serialize(ref data.BotData.GreenType);
		stream.Serialize(ref data.BotData.CoverIndex);
		stream.Serialize(ref data.BotData.UnderFire);
		stream.Serialize(ref data.BotData.IsInSpawnWeapon);
		stream.Serialize(ref data.BotData.Reloading);
		stream.Serialize(ref data.BotData.HaveAxeEnemy);
		stream.Serialize(ref data.BotData.TriangleLoad);
		stream.Serialize(ref data.BotData.Shooting);
		stream.Serialize(ref data.BotData.BotMoverCurrentState);
		stream.Serialize(ref data.BotData.DoorOpenerCurrentDoorPosition);
		stream.Serialize(ref data.BotData.TargetPoint);
		stream.Serialize(ref data.BotData.PositionOnWay);
		stream.SerializeLimitedInt32(ref data.BotData.WayPointsLength, 0, 9, BitPackingTag.DebugBotStruct18);
		if (data.BotData.WayPoints == null || data.BotData.WayPoints.Length < data.BotData.WayPointsLength)
		{
			data.BotData.WayPoints = new Vector3[data.BotData.WayPointsLength];
		}
		for (int i = 0; i < data.BotData.WayPointsLength; i++)
		{
			stream.Serialize(ref data.BotData.WayPoints[i]);
		}
		stream.Serialize(ref data.BotData.VisibleDist);
		stream.Serialize(ref data.BotData.GreenDist);
		stream.SerializeLimitedInt32(ref data.BotData.HourServer, 0, 30, BitPackingTag.DebugBotStruct16);
		stream.Serialize(ref data.BotData.HaveGrenade);
		stream.Serialize(ref data.BotData.IsGoalEnemyVisible);
		stream.Serialize(ref data.BotData.IsFlashed);
		stream.Serialize(ref data.BotData.IsParametesChanged);
		stream.Serialize(ref data.BotData.IsInPronePos);
		stream.Serialize(ref data.BotData.IsLay);
		stream.Serialize(ref data.BotData.PoseLevel);
		stream.Serialize(ref data.BotData.BaseVisibilityChangeSpeed);
		stream.Serialize(ref data.BotData.AccuratySpeed);
		stream.Serialize(ref data.BotData.ScatteringPerMeter);
		stream.Serialize(ref data.BotData.RuntimeVisionEffectK);
		stream.Serialize(ref data.BotData.RuntimeVisionEffectKCur);
		stream.Serialize(ref data.BotData.ScatteringPerMeterCur);
		stream.Serialize(ref data.BotData.ShallRunIfNoAmmo);
		stream.Serialize(ref data.BotData.StandBy);
		stream.SerializeLimitedString(ref data.BotData.FightType, ' ', 'z', BitPackingTag.DebugBotStructFightType, 1600u);
		stream.SerializeLimitedString(ref data.BotData.LayerName, ' ', 'z', BitPackingTag.DebugBotStructNodeInfo, 1600u);
		stream.SerializeLimitedString(ref data.BotData.NodeName, ' ', 'z', BitPackingTag.DebugBotStructNodeInfo, 1600u);
		stream.SerializeLimitedInt32(ref data.BotData.NodeRepeats, 0, 511, BitPackingTag.DebugBotStructNodeInfo);
		stream.SerializeLimitedInt32(ref data.BotData.HourServer, 0, 30, BitPackingTag.DebugBotStruct18);
		stream.SerializeLimitedString(ref data.BotData.PrevNodeName, ' ', 'z', BitPackingTag.DebugBotStructNodeInfo, 1600u);
		stream.SerializeLimitedString(ref data.BotData.Reason, ' ', 'z', BitPackingTag.DebugBotStructNodeReasonInfo, 1600u);
		stream.SerializeLimitedString(ref data.BotData.PrevNodeExitReason, ' ', 'z', BitPackingTag.DebugBotStructNodeReasonInfo, 1600u);
		stream.Serialize(ref data.BotData.CustomData, 1600u);
		stream.Serialize(ref data.BotData.BotRole);
		stream.Serialize(ref data.BotData.BotDifficulty);
		stream.Serialize(ref data.BotData.HasBossToFollow);
		stream.SerializeLimitedInt32(ref data.BotData.HitsOnMe, -1, 2045, BitPackingTag.DebugBotStruct7);
		stream.SerializeLimitedInt32(ref data.BotData.ShootsOnMe, -1, 2045, BitPackingTag.DebugBotStruct8);
		stream.SerializeLimitedInt32(ref data.BotData.Id, 0, 126, BitPackingTag.DebugBotStruct17);
		stream.Serialize(ref data.BotData.UseMeds);
		stream.Serialize(ref data.BotData.PatrolPointStatus);
		stream.Serialize(ref data.BotData.PlayerState);
		stream.Serialize(ref data.BotData.CanShootByState);
		stream.Serialize(ref data.BotData.PatrolWayStatus);
		stream.Serialize(ref data.BotData.UseStimulator);
		stream.Serialize(ref data.BotData.UseSurgical);
		stream.Serialize(ref data.BotData.Damaged);
		stream.Serialize(ref data.BotData.HaveMeds);
		stream.Serialize(ref data.BotData.HaveStimulator);
		stream.Serialize(ref data.BotData.HaveSurgical);
		stream.Serialize(ref data.BotData.HaveUnderbarrelLauncher);
		stream.Serialize(ref data.BotData.UnderbarrelLauncherActive);
		stream.Serialize(ref data.BotData.UnderbarrelLauncherHasAmmoInChamber);
		stream.Serialize(ref data.HeathsData.Inited);
		stream.SerializeLimitedInt32(ref data.HeathsData.HealthHead, 0, 16000, BitPackingTag.DebugBotStruct9);
		stream.SerializeLimitedInt32(ref data.HeathsData.HealthBody, 0, 16000, BitPackingTag.DebugBotStruct10);
		stream.SerializeLimitedInt32(ref data.HeathsData.HealthStomach, 0, 16000, BitPackingTag.DebugBotStruct11);
		stream.SerializeLimitedInt32(ref data.HeathsData.HealthLeftLeg, 0, 16000, BitPackingTag.DebugBotStruct12);
		stream.SerializeLimitedInt32(ref data.HeathsData.HealthRightLeg, 0, 16000, BitPackingTag.DebugBotStruct13);
		stream.SerializeLimitedInt32(ref data.HeathsData.HealthLeftArm, 0, 16000, BitPackingTag.DebugBotStruct14);
		stream.SerializeLimitedInt32(ref data.HeathsData.HealthRightArm, 0, 16000, BitPackingTag.DebugBotStruct15);
		stream.SerializeLimitedInt32(ref data.HeathsData.MaxHealthHead, 0, 16000, BitPackingTag.DebugBotStruct9);
		stream.SerializeLimitedInt32(ref data.HeathsData.MaxHealthBody, 0, 16000, BitPackingTag.DebugBotStruct10);
		stream.SerializeLimitedInt32(ref data.HeathsData.MaxHealthStomach, 0, 16000, BitPackingTag.DebugBotStruct11);
		stream.SerializeLimitedInt32(ref data.HeathsData.MaxHealthLeftLeg, 0, 16000, BitPackingTag.DebugBotStruct12);
		stream.SerializeLimitedInt32(ref data.HeathsData.MaxHealthRightLeg, 0, 16000, BitPackingTag.DebugBotStruct13);
		stream.SerializeLimitedInt32(ref data.HeathsData.MaxHealthLeftArm, 0, 16000, BitPackingTag.DebugBotStruct14);
		stream.SerializeLimitedInt32(ref data.HeathsData.MaxHealthRightArm, 0, 16000, BitPackingTag.DebugBotStruct15);
		stream.Serialize(ref data.HeathsData.HealthHeadBL);
		stream.Serialize(ref data.HeathsData.HealthBodyBL);
		stream.Serialize(ref data.HeathsData.HealthStomachBL);
		stream.Serialize(ref data.HeathsData.HealthLeftLegBL);
		stream.Serialize(ref data.HeathsData.HealthRightLegBL);
		stream.Serialize(ref data.HeathsData.HealthLeftArmBL);
		stream.Serialize(ref data.HeathsData.HealthRightArmBL);
		stream.Serialize(ref data.HeathsData.HealthHeadBroken);
		stream.Serialize(ref data.HeathsData.HealthBodyBroken);
		stream.Serialize(ref data.HeathsData.HealthStomachBroken);
		stream.Serialize(ref data.HeathsData.HealthLeftLegBroken);
		stream.Serialize(ref data.HeathsData.HealthRightLegBroken);
		stream.Serialize(ref data.HeathsData.HealthLeftArmBroken);
		stream.Serialize(ref data.HeathsData.HealthRightArmBroken);
		stream.SerializeBits(ref data.HeathsData.EffectsUint, 8);
		stream.SerializeLimitedString(ref data.BotData.GroupId, ' ', 'z', BitPackingTag.DebugbotStructGroupId, 1600u);
		stream.SerializeLimitedFloat(ref data.HeathsData.Energy, 0f, 400f, 0.1f, BitPackingTag.DebugBotEnergy);
		stream.SerializeLimitedFloat(ref data.HeathsData.Hydration, 0f, 400f, 0.1f, BitPackingTag.DebugBotHydration);
		stream.Serialize(ref data.VisionData.EnemiesCount);
		ref GStruct15[] enemiesVisions = ref data.VisionData.EnemiesVisions;
		if (enemiesVisions == null)
		{
			enemiesVisions = new GStruct15[data.VisionData.EnemiesCount];
		}
		for (int j = 0; j < data.VisionData.EnemiesVisions.Length; j++)
		{
			GStruct15 gStruct = data.VisionData.EnemiesVisions[j];
			stream.Serialize(ref gStruct.Id);
			stream.Serialize(ref gStruct.Visible);
			stream.Serialize(ref gStruct.VisibilityLevel);
			stream.Serialize(ref gStruct.VisibleType);
			stream.Serialize(ref gStruct.CanShoot);
			stream.Serialize(ref gStruct.PartsCount);
			ref GStruct16[] partVisions = ref gStruct.PartVisions;
			if (partVisions == null)
			{
				partVisions = new GStruct16[gStruct.PartsCount];
			}
			for (int k = 0; k < gStruct.PartVisions.Length; k++)
			{
				GStruct16 gStruct2 = gStruct.PartVisions[k];
				stream.Serialize(ref gStruct2.RaycastTargetPosition);
				stream.Serialize(ref gStruct2.Visible);
				stream.Serialize(ref gStruct2.VisibilityLevel);
				stream.Serialize(ref gStruct2.VisibilityDuration);
				stream.Serialize(ref gStruct2.VisibleType);
				stream.Serialize(ref gStruct2.HasLineOfSight);
				stream.Serialize(ref gStruct2.BotToTargetObstacleType);
				stream.Serialize(ref gStruct2.BotToTargetHitPosition);
				stream.Serialize(ref gStruct2.ObstaclePenetrationDepth);
				gStruct.PartVisions[k] = gStruct2;
			}
			data.VisionData.EnemiesVisions[j] = gStruct;
		}
	}

	public static void smethod_4(ref ActorDataStruct data, ISerializer stream)
	{
		stream.Serialize(ref data.CoverStruct.StartPos);
		stream.SerializeLimitedInt32(ref data.CoverStruct.CheckCoversLength, 0, 511, BitPackingTag.DebugBotStruct19);
		if (data.CoverStruct.CheckCovers == null)
		{
			data.CoverStruct.CheckCovers = new Vector3[data.CoverStruct.CheckCoversLength];
		}
		if (data.CoverStruct.Cause == null)
		{
			data.CoverStruct.Cause = new int[data.CoverStruct.CheckCoversLength];
		}
		for (int i = 0; i < data.CoverStruct.CheckCovers.Length; i++)
		{
			stream.Serialize(ref data.CoverStruct.CheckCovers[i]);
			stream.Serialize(ref data.CoverStruct.Cause[i]);
		}
	}

	public ActorDataStruct(IAIData aiData, float dist, IPlayer currentPlayer)
	{
		this = default(ActorDataStruct);
		method_0(aiData);
		Dist = dist;
		PlayerOwner = Singleton<GameWorld>.Instance.GetAlivePlayerBridgeByProfileID(currentPlayer.ProfileId);
	}

	public void method_0(IAIData aiData)
	{
		AIData.Inited = true;
		AIData.PowerOfEquipment = aiData.PowerOfEquipment;
		AIData.PlaceInfoId = ((aiData.PlaceInfo != null) ? aiData.PlaceInfo.AreaId : 0);
		AIData.IsInTree = aiData.IsInTree;
		AIData.IsAggressor = aiData.PlayerLoyaltyIsAggressor;
		AIData.BossNoAttack = aiData.PlayerLoyaltyBossNoAttack;
		AIData.ScavAttacker = aiData.IsScavAttacker;
		AIData.CanBeFreeKilled = aiData.PlayerLoyaltyCanBeFreeKilled;
		AIData.IsPursuitable = aiData.ShallPursuit;
		AIData.IsAxe = aiData.IsAxe;
		AIData.Nickname = aiData.PlayerProfileNickname;
		AIData.IsAI = aiData.IsAI;
		Player player = aiData.Player;
		if (player != null)
		{
			IHealthController healthController = player.HealthController;
			method_3(healthController);
			Player.FirearmController firearmController = player.HandsController as Player.FirearmController;
			if (firearmController == null)
			{
				return;
			}
			int shortNameHash = firearmController.FirearmsAnimator.Animator.GetCurrentAnimatorStateInfo(1).shortNameHash;
			string currentOpName = (player.HandsController as Player.ItemHandsController)?.CurrentHandsOperationName;
			string hashName = $"{GClass758.GetAnimStateByNameHash(shortNameHash)}|{shortNameHash}";
			AIData.currentOpName = currentOpName;
			AIData.hashName = hashName;
			if (firearmController.Item != null)
			{
				string shotNameWeapon = GClass2348.Localized(firearmController.Item.ShortName);
				AIData.shotNameWeapon = shotNameWeapon;
			}
			else
			{
				AIData.shotNameWeapon = "No weapon";
			}
		}
		AIData.HeadPosition = player.PlayerBones.Head.position;
	}

	public ActorDataStruct(BotOwner owner, float dist, IPlayer currentPlayer)
	{
		this = default(ActorDataStruct);
		if (owner.BotState != EBotState.Active)
		{
			return;
		}
		method_0(owner.AIData);
		PlayerOwner = Singleton<GameWorld>.Instance.GetAlivePlayerBridgeByProfileID(owner.ProfileId);
		BotData.Inited = true;
		BotData.Name = owner.name;
		BotData.Status = owner.BotState;
		Dist = dist;
		VisionData = default(GStruct14);
		int num = 0;
		int num2 = 0;
		Dictionary<string, GClass634> targetStats = owner.BotPersonalStats.targetStats;
		if (owner.BotPersonalStats.IsEnable)
		{
			if (targetStats != null && targetStats.TryGetValue(currentPlayer.Profile.Id, out var value))
			{
				foreach (KeyValuePair<int, GClass633> item in value.BotDistanceStat)
				{
					num += item.Value.hits;
					num2 += item.Value.shoots;
				}
			}
		}
		else
		{
			num = -1;
			num2 = -1;
		}
		BotData.Id = owner.Id;
		BotData.HitsOnMe = num;
		BotData.ShootsOnMe = num2;
		if (owner.Mover != null)
		{
			BotData.LookDirType = owner.Steering.SteeringMode.ToString();
		}
		Player.FirearmController firearmController = owner.GetPlayer.HandsController as Player.FirearmController;
		if (firearmController != null)
		{
			int chamberAmmoCount = firearmController.Item.ChamberAmmoCount;
			int currentMagazineCount = firearmController.Item.GetCurrentMagazineCount();
			BotData.Ammo = chamberAmmoCount + currentMagazineCount;
		}
		else
		{
			BotData.Ammo = -1;
		}
		BotData.Ammo = Mathf.Clamp(BotData.Ammo, 0, 255);
		BotData.BotRole = (int)owner.Profile.Info.Settings.Role;
		BotData.HasBossToFollow = owner.BotsGroup.HaveBoss(out var _);
		BotData.LayerName = owner.Brain.ActiveLayerName();
		BotData.NodeRepeats = Mathf.Clamp(owner.Brain.NodeRepeats(), 0, 511);
		BotData.NodeName = owner.Brain.GetLastNode();
		BotData.PrevNodeName = owner.Brain.Agent.PrevNode;
		BotData.Reason = owner.Brain.GetActiveNodeReason();
		BotData.PrevNodeExitReason = owner.Brain.Agent.EndReason;
		BotData.CustomData = owner.Brain.Agent.GetCustomData();
		BotData.GroupId = $"{owner.BotsGroup.Id} {owner.BotsGroup.Name}";
		method_9(owner);
		if (owner.StandBy.CanDoStandBy)
		{
			BotData.StandBy = owner.StandBy.StandByType;
		}
		else
		{
			BotData.StandBy = BotStandByType.none;
		}
		IHealthController healthController = owner.GetPlayer.HealthController;
		method_3(healthController);
		method_1(owner);
		if (owner.Memory != null)
		{
			BotData.VisibleDist = owner.LookSensor.VisibleDist;
			BotData.HourServer = owner.LookSensor.HourServer;
			BotData.MainTactic = (int)owner.Tactic.Tactic;
			BotData.StrategyName = owner.Brain.BaseBrain.ShortName();
			if (owner.Memory.GoalEnemy != null)
			{
				BotData.GreenDist = owner.Memory.GoalEnemy.GetBodyPartVision().ObstaclePenetrationDepth;
				BotData.GreenType = owner.Memory.GoalEnemy.GetBodyPartVision().ObstacleType;
				BotData.Target = owner.Memory.GoalEnemy.ToString();
			}
			else if (owner.Memory.GoalTarget.HavePlaceTarget())
			{
				BotData.Target = owner.Memory.GoalTarget.ToString();
			}
			else
			{
				BotData.Target = "Patrol->";
			}
			BotData.HaveSurgical = owner.Medecine.SurgicalKit.HaveSmth2Use;
			BotData.HaveMeds = owner.Medecine.FirstAid.HaveSmth2Use;
			BotData.Damaged = owner.Medecine.FirstAid.Damaged;
			BotData.HaveStimulator = owner.Medecine.Stimulators.HaveSmt;
			BotData.UseStimulator = owner.Medecine.Stimulators.Using;
			BotData.UseSurgical = owner.Medecine.SurgicalKit.Using;
			BotData.CanShootByState = owner.ShootData.CanShootByState;
			BotData.PlayerState = owner.GetPlayer.MovementContext.CurrentState.Name;
			BotData.UseMeds = owner.Medecine.Using;
			BotData.InCover = owner.Memory.IsInCover;
			BotData.CoverIndex = ((owner.Memory.BotCurrentCoverInfo.CovPoint != null) ? owner.Memory.BotCurrentCoverInfo.CovPoint.Id : (-1));
			BotData.UnderFire = owner.Memory.IsUnderFire;
			BotData.Reloading = owner.WeaponManager.Reload.Reloading;
			BotData.IsProblem = owner.Brain.IsProblem();
			BotData.HaveAxeEnemy = owner.EnemiesController.HavePursuitableEnemy;
			BotData.IsInSpawnWeapon = owner.WeaponManager.Stationary.IsInSpawnState;
			BotRequestController botRequestController = owner.BotRequestController;
			BotData.IsFlashed = owner.FlashGrenade.IsFlashed;
			BotData.IsLay = owner.BotLay.IsLay;
			BotData.IsInPronePos = owner.GetPlayer.MovementContext.IsInPronePose;
			BotData.IsParametesChanged = owner.Settings.Current.IsParametersChanged();
			BotData.PoseLevel = owner.GetPlayer.PoseLevel;
			BotData.Request = ((botRequestController.CurRequest != null) ? 2 : ((owner.AIData.AskRequests.ActivatedRequest != null) ? 1 : 0));
			BotData.IsGoalEnemyVisible = owner.Memory.GoalEnemy != null && owner.Memory.GoalEnemy.IsVisible;
			BotData.CanShoot = owner.Memory.GoalEnemy != null && owner.Memory.GoalEnemy.CanShoot;
			BotData.InBuildingMe = owner.AIData.EnvironmentId > 0;
			BotData.InBuildingTarget = owner.Memory.GoalEnemy != null && owner.Memory.GoalEnemy.Person.AIData.EnvironmentId > 0;
			BotData.IsGoalEnemyVisible = owner.Memory.GoalEnemy != null && owner.Memory.GoalEnemy.IsVisible;
			BotData.CanLookThoughtGrass = owner.LookSensor.CurLookThroughGrass;
			BotData.Shooting = owner.ShootData.Shooting;
			BotData.HaveGrenade = owner.WeaponManager.Grenades.HaveGrenade;
			if (owner.WeaponManager.UnderbarrelLauncherController.TryGetUnderbarrelWeapon(out var underbarrelWeapon))
			{
				BotData.HaveUnderbarrelLauncher = true;
				BotData.UnderbarrelLauncherActive = owner.WeaponManager.UnderbarrelLauncherController.IsActive;
				BotData.UnderbarrelLauncherHasAmmoInChamber = BotUnderbarrelLauncherController.HasAmmoInChamber(underbarrelWeapon);
			}
			else
			{
				BotData.HaveUnderbarrelLauncher = false;
				BotData.UnderbarrelLauncherActive = false;
				BotData.UnderbarrelLauncherHasAmmoInChamber = false;
			}
			if (owner.Mover != null)
			{
				BotData.BotMoverCurrentState = (int)owner.Mover.CurrentState;
			}
			if (owner.DoorOpener.HaveDoorToOpen)
			{
				BotData.DoorOpenerCurrentDoorPosition = owner.DoorOpener.LookPoint;
			}
			else
			{
				BotData.DoorOpenerCurrentDoorPosition = Vector3.zero;
			}
			BotData.TargetPoint = method_5(owner);
			BotData.PositionOnWay = owner.Mover.PositionOnWay;
			BotData.WayPoints = method_4(owner);
			BotData.WayPointsLength = BotData.WayPoints.Length;
			method_8(owner);
		}
		else
		{
			BotData.MainTactic = 0;
			BotData.Target = "";
			BotData.InCover = false;
		}
	}

	public void method_1(BotOwner owner)
	{
		DebugCoverFindGraphClosests lastDebugClosest = owner.CoverSearchInfo.LastDebugClosest;
		DebugCoverFindGraphSearchData lastDebugFunc = owner.CoverSearchInfo.LastDebugFunc;
		if (lastDebugFunc != null || lastDebugClosest != null)
		{
			if (lastDebugFunc == null)
			{
				method_2(lastDebugClosest);
			}
			else if (lastDebugClosest == null)
			{
				method_2(lastDebugFunc);
			}
			else if (!(lastDebugClosest.TimeSetted < lastDebugFunc.TimeSetted))
			{
				method_2(lastDebugFunc);
			}
		}
	}

	public void method_2(Interface0 info)
	{
		if (info != null)
		{
			CoverStruct.Time = Time.time;
			List<GroupPointSearchData> list = info.Checked;
			int num = Mathf.Min(list.Count, 15);
			CoverStruct.CheckCovers = new Vector3[num];
			CoverStruct.Cause = new int[num];
			CoverStruct.CheckCoversLength = num;
			CoverStruct.StartPos = info.PositionStart;
			for (int i = 0; i < num; i++)
			{
				GroupPointSearchData groupPointSearchData = list[i];
				CoverStruct.CheckCovers[i] = groupPointSearchData.Point.Position;
				CoverStruct.Cause[i] = (int)groupPointSearchData.Cause;
			}
		}
	}

	public void method_3(IHealthController healthController)
	{
		HeathsData.Inited = true;
		HeathsData.HealthHeadBL = method_6(healthController, EBodyPart.Head);
		HeathsData.HealthBodyBL = method_6(healthController, EBodyPart.Chest);
		HeathsData.HealthStomachBL = method_6(healthController, EBodyPart.Stomach);
		HeathsData.HealthLeftLegBL = method_6(healthController, EBodyPart.LeftLeg);
		HeathsData.HealthRightLegBL = method_6(healthController, EBodyPart.RightLeg);
		HeathsData.HealthLeftArmBL = method_6(healthController, EBodyPart.LeftArm);
		HeathsData.HealthRightArmBL = method_6(healthController, EBodyPart.RightArm);
		HeathsData.HealthHead = (int)healthController.GetBodyPartHealth(EBodyPart.Head).Current;
		HeathsData.HealthBody = (int)healthController.GetBodyPartHealth(EBodyPart.Chest).Current;
		HeathsData.HealthStomach = (int)healthController.GetBodyPartHealth(EBodyPart.Stomach).Current;
		HeathsData.HealthLeftArm = (int)healthController.GetBodyPartHealth(EBodyPart.LeftArm).Current;
		HeathsData.HealthRightArm = (int)healthController.GetBodyPartHealth(EBodyPart.RightArm).Current;
		HeathsData.HealthRightLeg = (int)healthController.GetBodyPartHealth(EBodyPart.RightLeg).Current;
		HeathsData.HealthLeftLeg = (int)healthController.GetBodyPartHealth(EBodyPart.LeftLeg).Current;
		HeathsData.MaxHealthHead = (int)healthController.GetBodyPartHealth(EBodyPart.Head).Maximum;
		HeathsData.MaxHealthBody = (int)healthController.GetBodyPartHealth(EBodyPart.Chest).Maximum;
		HeathsData.MaxHealthStomach = (int)healthController.GetBodyPartHealth(EBodyPart.Stomach).Maximum;
		HeathsData.MaxHealthLeftArm = (int)healthController.GetBodyPartHealth(EBodyPart.LeftArm).Maximum;
		HeathsData.MaxHealthRightArm = (int)healthController.GetBodyPartHealth(EBodyPart.RightArm).Maximum;
		HeathsData.MaxHealthRightLeg = (int)healthController.GetBodyPartHealth(EBodyPart.RightLeg).Maximum;
		HeathsData.MaxHealthLeftLeg = (int)healthController.GetBodyPartHealth(EBodyPart.LeftLeg).Maximum;
		HeathsData.Effects = smethod_5(healthController.GetAllActiveEffects());
		HeathsData.Energy = healthController.Energy.Current;
		HeathsData.Hydration = healthController.Hydration.Current;
	}

	public Vector3[] method_4(BotOwner owner)
	{
		return owner.Mover.GetWayPoints(8);
	}

	public Vector3 method_5(BotOwner owner)
	{
		if (!owner.Mover.TargetPoint.HasValue)
		{
			return owner.Position;
		}
		return owner.Mover.TargetPoint.Value;
	}

	public bool method_6(IHealthController owner, EBodyPart part)
	{
		return owner.FindActiveEffect<GInterface341>(part) != null;
	}

	public bool method_7(BotOwner owner, EBodyPart part)
	{
		return owner.GetPlayer.HealthController.IsBodyPartBroken(part);
	}

	public void method_8(BotOwner owner)
	{
		BotData.ShallRunIfNoAmmo = owner.Memory.ShallRunIfNoAmmo;
	}

	public void method_9(BotOwner owner)
	{
		BotData.BaseVisibilityChangeSpeed = owner.Settings.FileSettings.Look.VISIBILITY_CHANGE_SPEED;
		BotData.ScatteringPerMeter = owner.Settings.FileSettings.Core.ScatteringPerMeter;
		BotData.AccuratySpeed = owner.Settings.FileSettings.Core.AccuratySpeed;
		BotData.RuntimeVisionEffectKCur = owner.Settings.Current.RuntimeVisionEffectsK;
		BotData.RuntimeVisionEffectK = owner.Settings.Current.RuntimeVisionEffectsK;
		BotData.ScatteringPerMeterCur = owner.Settings.Current.CurrentScattering;
	}

	public static EDebugBotEffect smethod_5(IEnumerable<IEffect> activeEffects)
	{
		EDebugBotEffect eDebugBotEffect = EDebugBotEffect.None;
		foreach (IEffect activeEffect in activeEffects)
		{
			if (activeEffect is GInterface360)
			{
				eDebugBotEffect |= EDebugBotEffect.Stun;
			}
			if (activeEffect is GInterface354)
			{
				eDebugBotEffect |= EDebugBotEffect.BurnEyes;
			}
			if (activeEffect is GInterface341)
			{
				eDebugBotEffect |= EDebugBotEffect.Bleed;
			}
			if (activeEffect is GInterface342)
			{
				eDebugBotEffect |= EDebugBotEffect.Fracture;
			}
			if (activeEffect is GInterface358)
			{
				eDebugBotEffect |= EDebugBotEffect.Painkiller;
			}
			if (activeEffect is GInterface346)
			{
				eDebugBotEffect |= EDebugBotEffect.Intoxication;
			}
		}
		return eDebugBotEffect;
	}
}
