using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using EFT;
using EFT.InventoryLogic;
using NLog;
using UnityEngine;

public class EnemyInfo
{
	public BotSettingsClass GroupInfo;

	public bool CanISearch;

	public Vector3 Direction;

	public float LastChangeVisionTime;

	[NonSerialized]
	public GClass543 EnemyVision = new GClass543(EEnemyPartVisibleType.NotVisible, canShoot: false, 0f);

	[NonSerialized]
	public EEnemyPartVisibleType VisibleType_1;

	[NonSerialized]
	public bool CanShoot_1;

	[NonSerialized]
	public bool ForceHeadCheck;

	[NonSerialized]
	public int PriorityIndex_1;

	[NonSerialized]
	public float NextPartRndTime;

	[NonSerialized]
	public EEnemyPriority Priority = EEnemyPriority.Low;

	[NonSerialized]
	public Dictionary<BodyPartType, EnemyPart> AllParts;

	[NonSerialized]
	public Dictionary<BodyPartType, GClass542> AllPartsVision = new Dictionary<BodyPartType, GClass542>();

	[NonSerialized]
	public HashSet<BodyPartType> ActiveParts = new HashSet<BodyPartType>();

	[NonSerialized]
	public HashSet<BodyPartType> Maxparts = new HashSet<BodyPartType>();

	[NonSerialized]
	public HashSet<BodyPartType> MiddleParts = new HashSet<BodyPartType>();

	[NonSerialized]
	public HashSet<BodyPartType> FarParts = new HashSet<BodyPartType>();

	[NonSerialized]
	public float Distance_1 = float.MaxValue;

	[NonSerialized]
	public bool LastCheckVision;

	[NonSerialized]
	public float VisibilityLevel_1;

	[NonSerialized]
	public static StringBuilder EnemyBuilder = new StringBuilder();

	[NonSerialized]
	public static StringBuilder PartBuilder = new StringBuilder();

	[NonSerialized]
	public static StringBuilder VisibilityKBuilder = new StringBuilder();

	public int PersonalShoot;

	[NonSerialized]
	public EEnemyPartVisibleType PreviousVisibleType;

	[NonSerialized]
	public float VisibilityChangeSpeedK;

	public int EnemyPlayerId => AllParts[BodyPartType.body].EnemyPlayer.Id;

	public virtual bool CheckPartCanShootOnlyIfVisible => true;

	[field: NonSerialized]
	public bool HaveSeenPersonal { get; set; }

	public virtual EEnemyPartVisibleType VisibleType => VisibleType_1;

	public virtual bool CanShoot => CanShoot_1;

	[field: NonSerialized]
	public virtual bool IsVisible { get; set; }

	[field: NonSerialized]
	public int SearchIndex { get; set; }

	[field: NonSerialized]
	public IPlayer Person { get; }

	public HashSet<BodyPartType> AllActiveParts
	{
		get
		{
			if (ActiveParts.Count != 0)
			{
				return ActiveParts;
			}
			return FarParts;
		}
	}

	[field: NonSerialized]
	public BotsGroup GroupOwner { get; }

	public float TimeLastSeen => GroupInfo.EnemyLastSeenTimeSense;

	public float TimeLastSeenReal => GroupInfo.EnemyLastSeenTimeReal;

	[field: NonSerialized]
	public float LastChangeVisionTypeTime { get; set; }

	[field: NonSerialized]
	public float PersonalSeenTime { get; set; }

	[field: NonSerialized]
	public float PersonalLastSeenTime { get; set; }

	[field: NonSerialized]
	public float PersonalLastShootTime { get; set; } = -1f;

	[field: NonSerialized]
	public Vector3 PersonalLastPos { get; set; }

	[field: NonSerialized]
	public float FirstTimeSeen { get; set; }

	[field: NonSerialized]
	public float AddTime { get; }

	[field: NonSerialized]
	public string ProfileId { get; }

	[field: NonSerialized]
	public EnemyPart LastPartToShoot { get; set; }

	public float Distance
	{
		get
		{
			return Distance_1;
		}
		set
		{
			Distance_1 = value;
			Owner.EnemiesController.UpdateFor(this);
		}
	}

	public Vector3 CurrPosition => Person.Transform.position;

	public int PriorityIndex
	{
		get
		{
			return PriorityIndex_1;
		}
		set
		{
			PriorityIndex_1 = value;
			if (PriorityIndex_1 < 6)
			{
				Priority = EEnemyPriority.High;
				method_2();
			}
			else if (PriorityIndex_1 < 8)
			{
				Priority = EEnemyPriority.Medium;
				method_1();
			}
			else
			{
				Priority = EEnemyPriority.Low;
				method_0();
			}
		}
	}

	[field: NonSerialized]
	public BotOwner Owner { get; }

	public bool HaveSeen => TimeLastSeen > 0f;

	public Vector3 EnemyLastPosition => GroupInfo.EnemyLastPosition;

	public Vector3 EnemyLastPositionReal => GroupInfo.EnemyLastVisiblePosition;

	public bool EnemyLastPositionCheck => GroupOwner.Enemies[Person].IsLastPositionChecked;

	public Vector3 EnemyWeaponRootLastPos => GroupOwner.Enemies[Person].EnemyWeaponRootLastPos;

	[field: NonSerialized]
	public bool IgnoreUntilAggression { get; set; }

	[field: NonSerialized]
	public float LastDoHitTime { get; set; } = -10f;

	[field: NonSerialized]
	public float LastGetHitTime { get; set; } = -10f;

	[field: NonSerialized]
	public float FirstTimeShoot { get; set; } = -10f;

	public string Nickname => Person.Profile.Nickname;

	public float VisibilityLevel => EnemyVision.VisibilityLevel;

	[field: NonSerialized]
	public int MissRemain { get; set; }

	public EnemyInfo(BotsGroup botsGroup, IPlayer enemy, BotOwner owner, BotSettingsClass groupInfo)
	{
		BotGlobalAimingSettings aiming = owner.Settings.FileSettings.Aiming;
		MissRemain = aiming.MISS_ON_START;
		AddTime = Time.time;
		GroupInfo = groupInfo;
		Owner = owner;
		ForceHeadCheck = owner.Settings.FileSettings.Look.CHECK_HEAD_ANY_DIST;
		GroupOwner = botsGroup;
		Person = enemy;
		ProfileId = enemy.Profile.Id;
		AllParts = enemy.MainParts;
		foreach (KeyValuePair<BodyPartType, EnemyPart> allPart in AllParts)
		{
			AllPartsVision[allPart.Key] = new GClass542();
		}
		if (owner.Settings.FileSettings.Look.MIDDLE_DIST_CAN_SHOOT_HEAD)
		{
			smethod_0(AllParts, MiddleParts, BodyPartType.head);
		}
		smethod_0(AllParts, MiddleParts, BodyPartType.leftArm);
		smethod_0(AllParts, MiddleParts, BodyPartType.rightArm);
		smethod_0(AllParts, FarParts, BodyPartType.body);
		smethod_0(AllParts, Maxparts, BodyPartType.leftArm);
		smethod_0(AllParts, Maxparts, BodyPartType.rightArm);
		smethod_0(AllParts, Maxparts, BodyPartType.leftLeg);
		smethod_0(AllParts, Maxparts, BodyPartType.rightLeg);
		if (!ForceHeadCheck || owner.Settings.FileSettings.Look.MIDDLE_DIST_CAN_SHOOT_HEAD)
		{
			smethod_0(AllParts, Maxparts, BodyPartType.head);
		}
		method_0();
	}

	public void SetIgnoreState(bool state = true)
	{
		IgnoreUntilAggression = state;
	}

	public float AdditionalWeightCauseHit()
	{
		if (!IsVisible || !CanShoot)
		{
			float num = Time.time - LastGetHitTime;
			if (num > 1f)
			{
				return Mathf.Clamp(1200f * num, 0f, 10000f);
			}
		}
		return 0f;
	}

	public void SetVisible(bool value)
	{
		bool isVisible = IsVisible;
		IsVisible = value;
		if (!value && isVisible)
		{
			if (GClass856.IsTrue100(35f))
			{
				Owner.BotTalk.TrySay(EPhraseTrigger.OnLostVisual, withGroupDelay: true);
			}
			if (Owner.Memory.GoalEnemy != null && Owner.Memory.GoalEnemy == this)
			{
				Owner.Memory.LoseVisionCurrentEnemy();
			}
			GroupOwner.LoseVision(Person);
		}
		if (!value || isVisible)
		{
			return;
		}
		bool haveSeenPersonal = HaveSeenPersonal;
		if (!HaveSeenPersonal)
		{
			HaveSeenPersonal = true;
			FirstTimeSeen = Time.time;
		}
		PersonalSeenTime = Time.time;
		EnemyInfo goalEnemy = Owner.Memory.GoalEnemy;
		ETagStatus? additionaMask = null;
		if (goalEnemy == null)
		{
			Owner.BotsGroup.CalcGoalForBot(Owner);
		}
		if (Distance < 25f && GClass856.IsTrue100(Owner.Settings.FileSettings.Mind.CHANCE_FUCK_YOU_ON_CONTACT_100))
		{
			Owner.Gesture.TryGestus(EInteraction.GetOffGesture, withAimingDelay: true);
		}
		if (goalEnemy != null)
		{
			switch (goalEnemy.Person.Profile.Info.Side)
			{
			case EPlayerSide.Usec:
				additionaMask = ETagStatus.Usec;
				break;
			case EPlayerSide.Bear:
				additionaMask = ETagStatus.Bear;
				break;
			case EPlayerSide.Savage:
				additionaMask = ETagStatus.Scav;
				break;
			}
		}
		int num = 0;
		foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in Owner.EnemiesController.EnemyInfos)
		{
			if (enemyInfo.Value.IsVisible)
			{
				num++;
			}
		}
		if (num > 3)
		{
			Owner.BotTalk.TrySay(EPhraseTrigger.HurtHeavy);
		}
		if (HaveSeen)
		{
			if (!method_19(haveSeenPersonal, out var _))
			{
				Owner.BotTalk.TrySay(EPhraseTrigger.OnRepeatedContact);
			}
		}
		else
		{
			if (GClass856.IsTrue100(Owner.Settings.FileSettings.Aiming.FIRST_CONTACT_ADD_CHANCE_100))
			{
				float nextAimingDelay = GClass856.GreateRandom(Owner.Settings.FileSettings.Aiming.FIRST_CONTACT_ADD_SEC);
				Owner.AimingManager.CurrentAiming.SetNextAimingDelay(nextAimingDelay);
			}
			if (!method_19(haveSeenPersonal, out var missShots2))
			{
				EPhraseTrigger type = EPhraseTrigger.OnFirstContact;
				if (Owner.BotsGroup.GroupTalk.CanSay(Owner, EPhraseTrigger.OnFirstContact))
				{
					Owner.BotTalk.TrySay(type, additionaMask, withGroupDelay: true);
				}
			}
			if (!missShots2)
			{
				method_18();
			}
		}
		Owner.BotPersonalStats.AddGetVision(Distance, VisibilityChangeSpeedK);
		GroupOwner.GetVision();
	}

	public bool IsFullDisappear(BotOwner owner)
	{
		return Time.time - TimeLastSeenReal > owner.Settings.FileSettings.Look.GOAL_TO_FULL_DISSAPEAR;
	}

	public bool IsFullDissapearGreen(BotOwner owner)
	{
		return Time.time - TimeLastSeenReal > owner.Settings.FileSettings.Look.GOAL_TO_FULL_DISSAPEAR_GREEN;
	}

	public bool ShallKnowEnemy()
	{
		return Time.time - TimeLastSeen < Owner.Settings.FileSettings.Mind.TIME_TO_FORGOR_ABOUT_ENEMY_SEC;
	}

	public bool ShallKnowEnemyLate()
	{
		float num = Time.time - TimeLastSeen;
		if (num < Owner.Settings.FileSettings.Mind.TIME_TO_FORGOR_ABOUT_ENEMY_SEC)
		{
			return num > Owner.Settings.FileSettings.Mind.TIME_TO_FIND_ENEMY;
		}
		return false;
	}

	public void SetSuppressEndTime(float supressedEndTime)
	{
		GroupInfo.SetSuppressEndTime(supressedEndTime);
	}

	public bool IsSuppressed()
	{
		return GroupInfo.IsSuppressed();
	}

	public bool ShallISuppress()
	{
		return GroupInfo.ShallISuppress();
	}

	public void method_0()
	{
		ActiveParts = FarParts;
	}

	public void method_1()
	{
		ActiveParts = MiddleParts;
	}

	public void method_2()
	{
		ActiveParts = Maxparts;
	}

	public GClass542 GetBodyPartVision()
	{
		return AllPartsVision[BodyPartType.body];
	}

	public Vector3 GetBodyPartPosition()
	{
		return AllParts[BodyPartType.body].Position;
	}

	public Vector3 GetPartToShoot()
	{
		switch (VisibleType)
		{
		default:
			if (!Owner.WeaponManager.UnderbarrelLauncherController.IsActive)
			{
				return GetBodyPartPosition();
			}
			return CurrPosition;
		case EEnemyPartVisibleType.Visible:
			return method_8();
		case EEnemyPartVisibleType.GreenSence:
		case EEnemyPartVisibleType.Sence:
			return method_7();
		}
	}

	public override string ToString()
	{
		return $"S:{CanShoot}  V:{IsVisible}";
	}

	public void method_3(bool p0)
	{
		if (LastCheckVision != p0)
		{
			LastChangeVisionTime = Time.time;
			GroupInfo.SetLastVisionChange(LastChangeVisionTime);
		}
		LastCheckVision = p0;
	}

	public bool HaveNightVision()
	{
		if (Person.AIData.Player.InventoryController.Inventory.Equipment.GetSlot(EquipmentSlot.Headwear).ContainedItem is CompoundItem thisItem)
		{
			return GClass3380.GetItemComponentsInChildren<NightVisionComponent>(thisItem).Any();
		}
		return false;
	}

	public void SetCanShoot(bool can)
	{
		CanShoot_1 = can;
	}

	public virtual string GetDebugInfo()
	{
		return $"V:{IsVisible} S:{CanShoot}  VisibleType:{VisibleType_1}";
	}

	public void CheckLookEnemy(LookAllDataClass lookAll, float deltaTime)
	{
		if (Person?.Transform?.Original == null)
		{
			return;
		}
		Direction = CurrPosition - Owner.Transform.position;
		Distance = Direction.magnitude;
		float num = Mathf.Min(Distance, Owner.Settings.FileSettings.Look.MAX_DIST_CLAMP_TO_SEEN_SPEED);
		float distanceToEnemyNormalized = num / Owner.Settings.FileSettings.Look.MAX_DIST_CLAMP_TO_SEEN_SPEED;
		VisibilityChangeSpeedK = method_9(Owner.Settings, Person.AIData, PersonalLastSeenTime, PersonalLastPos, distanceToEnemyNormalized, num, deltaTime);
		if (Distance < lookAll.MinDistance)
		{
			lookAll.MinDistance = Distance;
		}
		method_5();
		HashSet<BodyPartType> allActiveParts = AllActiveParts;
		float addSensorDistance = method_4(Owner);
		bool onSense = !IsFullDisappear(Owner);
		bool onSenceGreen = !IsFullDissapearGreen(Owner);
		if (Owner.FlashGrenade.IsFlashed)
		{
			onSense = false;
			addSensorDistance = -1f;
		}
		EnemyVision.VisibleType = EEnemyPartVisibleType.NotVisible;
		EnemyVision.CanShoot = false;
		LayerMask lookSensorMask = Owner.LookSensor.Mask;
		if (!Owner.Settings.FileSettings.Look.LOOK_THROUGH_GRASS)
		{
			if (num < Owner.Settings.FileSettings.Look.NO_GREEN_DIST)
			{
				lookSensorMask = LayerMaskClass.HighPolyWithTerrainMask;
			}
			else if (num < Owner.Settings.FileSettings.Look.NO_GRASS_DIST)
			{
				lookSensorMask = LayerMaskClass.HighPolyWithTerrainNoGrassMask;
			}
		}
		float num2 = 0f;
		if (!allActiveParts.Contains(BodyPartType.head) && ForceHeadCheck)
		{
			GClass542 gClass = AllPartsVision[BodyPartType.head];
			EnemyPart enemyPart = AllParts[BodyPartType.head];
			gClass.CalculatePartVisibility(Owner, enemyPart, lookSensorMask, onSense, onSenceGreen, addSensorDistance, VisibilityChangeSpeedK, deltaTime);
			enemyPart.CheckCanShoot(Owner, gClass, CheckPartCanShootOnlyIfVisible);
			smethod_1(gClass, enemyPart.CanShoot, EnemyVision);
			num2 = Mathf.Max(num2, gClass.VisibilityLevel);
		}
		foreach (BodyPartType item2 in allActiveParts)
		{
			GClass542 gClass2 = AllPartsVision[item2];
			EnemyPart enemyPart2 = AllParts[item2];
			gClass2.CalculatePartVisibility(Owner, enemyPart2, lookSensorMask, onSense, onSenceGreen, addSensorDistance, VisibilityChangeSpeedK, deltaTime);
			enemyPart2.CheckCanShoot(Owner, gClass2, CheckPartCanShootOnlyIfVisible);
			smethod_1(gClass2, enemyPart2.CanShoot, EnemyVision);
			num2 = Mathf.Max(num2, gClass2.VisibilityLevel);
		}
		EnemyVision.VisibilityLevel = num2;
		method_17(allActiveParts, EnemyVision.VisibilityLevel == 0f && num2 != 0f);
		SetCanShoot(EnemyVision.CanShoot);
		bool isVisible = IsVisible;
		SetVisible(EnemyVision.Visible);
		method_6(EnemyVision.VisibleType);
		if (EnemyVision.Visible)
		{
			if (VisibleType_1 == EEnemyPartVisibleType.Visible)
			{
				method_3(p0: true);
				PersonalLastSeenTime = Time.time;
				PersonalLastPos = CurrPosition;
			}
			else
			{
				method_3(p0: false);
			}
			BotReportsDataClass item = new BotReportsDataClass(Owner, Person, Person.Transform.position, VisibleType_1);
			lookAll.ReportsData.Add(item);
			if (!isVisible)
			{
				lookAll.ShallRecalcGoal = true;
			}
		}
		else
		{
			method_3(p0: false);
		}
	}

	public float method_4(BotOwner owner)
	{
		float result = 1f;
		if (Person.AIData.UsingLight && Owner.LookSensor.VisibleDist < owner.Settings.FileSettings.Look.ENEMY_LIGHT_START_DIST)
		{
			result = owner.Settings.FileSettings.Look.ENEMY_LIGHT_ADD;
		}
		return result;
	}

	public void method_5()
	{
		if (Distance > Owner.Settings.FileSettings.Look.FAR_DISTANCE)
		{
			method_0();
			return;
		}
		if (Distance > Owner.Settings.FileSettings.Look.MIDDLE_DIST)
		{
			if (Priority == EEnemyPriority.Low)
			{
				method_0();
			}
			else
			{
				method_1();
			}
			return;
		}
		switch (Priority)
		{
		case EEnemyPriority.High:
			method_2();
			break;
		case EEnemyPriority.Medium:
			method_1();
			break;
		case EEnemyPriority.Low:
			method_0();
			break;
		}
	}

	public float GetLastBodyHitDistance()
	{
		return AllPartsVision[BodyPartType.body].BotToTargetHit.distance;
	}

	public void SetLastShootTime()
	{
		PersonalShoot++;
		PersonalLastShootTime = Time.time;
		GroupInfo.SetLastShootTime();
	}

	public void method_6(EEnemyPartVisibleType totalCheckOnlySense)
	{
		if (VisibleType_1 != totalCheckOnlySense)
		{
			VisibleType_1 = totalCheckOnlySense;
			LastChangeVisionTypeTime = Time.time;
		}
	}

	public static void smethod_0(Dictionary<BodyPartType, EnemyPart> from, HashSet<BodyPartType> to, BodyPartType partType)
	{
		if (from.ContainsKey(partType))
		{
			to.Add(partType);
		}
	}

	public Vector3 method_7()
	{
		if (!Owner.WeaponManager.UnderbarrelLauncherController.IsActive)
		{
			return EnemyLastPositionReal + Vector3.up * 0.8f;
		}
		return EnemyLastPositionReal;
	}

	public Vector3 method_8()
	{
		if (Owner.WeaponManager.UnderbarrelLauncherController.IsActive)
		{
			return CurrPosition;
		}
		EnemyPart enemyPart = AllParts[BodyPartType.body];
		if (enemyPart.CanShoot)
		{
			LastPartToShoot = enemyPart;
			return LastPartToShoot.GetPartPositionWithOffset();
		}
		int aIMING_TYPE = Owner.Settings.FileSettings.Aiming.AIMING_TYPE;
		if (HaveSeenPersonal && Time.time - FirstTimeSeen > Owner.Settings.FileSettings.Aiming.ANY_PART_SHOOT_TIME)
		{
			aIMING_TYPE = 1;
			goto IL_00ce;
		}
		switch (aIMING_TYPE)
		{
		case 1:
			goto IL_00ce;
		case 2:
			goto IL_00db;
		case 3:
			goto IL_00e8;
		case 4:
			goto IL_0152;
		case 5:
			goto IL_015c;
		case 6:
			goto IL_0166;
		}
		LastPartToShoot = AllParts[BodyPartType.head];
		goto IL_01a8;
		IL_015c:
		method_16(withLegs: false, canBeHead: false);
		goto IL_01a8;
		IL_01a8:
		if (LastPartToShoot == null)
		{
			return Vector3.zero;
		}
		return LastPartToShoot.GetPartPositionWithOffset();
		IL_0166:
		if (AllParts[BodyPartType.head].CanShoot && AllPartsVision[BodyPartType.head].Visible)
		{
			LastPartToShoot = AllParts[BodyPartType.head];
		}
		else
		{
			method_16(withLegs: false, canBeHead: false);
		}
		goto IL_01a8;
		IL_00ce:
		method_16(withLegs: true, canBeHead: true);
		goto IL_01a8;
		IL_00db:
		method_16(withLegs: false, canBeHead: true);
		goto IL_01a8;
		IL_00e8:
		foreach (BodyPartType allActivePart in AllActiveParts)
		{
			if (AllParts[allActivePart].CanShoot)
			{
				LastPartToShoot = AllParts[allActivePart];
				return LastPartToShoot.GetPartPositionWithOffset();
			}
		}
		goto IL_01a8;
		IL_0152:
		method_16(withLegs: true, canBeHead: false);
		goto IL_01a8;
	}

	public static void smethod_1(GClass542 partVision, bool partCanShoot, GClass543 enemyVision)
	{
		if (partCanShoot)
		{
			enemyVision.CanShoot = true;
		}
		int visibleType = (int)partVision.VisibleType;
		int visibleType2 = (int)enemyVision.VisibleType;
		enemyVision.VisibleType = (EEnemyPartVisibleType)Mathf.Max(visibleType, visibleType2);
	}

	public float method_9(BotDifficultySettingsClass settings, IAIData enemyAiData, float personalLastSeenTime, Vector3 personalLastSeenPos, float distanceToEnemyNormalized, float clampedDistance, float deltaTime)
	{
		float num = method_13(distanceToEnemyNormalized);
		float flarePower = enemyAiData.FlarePower;
		float poseVisibilityCoef = enemyAiData.PoseVisibilityCoef;
		float num2 = method_15(personalLastSeenTime, personalLastSeenPos);
		float angleToEnemy;
		float num3 = method_14(settings, distanceToEnemyNormalized, out angleToEnemy);
		float num4 = method_12();
		float foliageIntersectionPercent;
		float num5 = method_10(out foliageIntersectionPercent);
		float rainK;
		float fogK;
		float num6 = method_11(enemyAiData.IsInside, out rainK, out fogK);
		float num7 = num * flarePower * poseVisibilityCoef * settings.Current.RuntimeVisionEffectsK * num2 * num3 * num4 * num5 * num6;
		VisibilityKBuilder.Clear();
		if (GClass398.Instance.IsVisionLogEnabled(LogLevel.Trace))
		{
			VisibilityKBuilder.Append("t2bVis:").Append(smethod_2(1f / (Owner.Settings.FileSettings.Look.VISIBILITY_CHANGE_SPEED * num7))).Append(" sec | ")
				.Append("main:")
				.Append(smethod_2(Owner.Settings.FileSettings.Look.VISIBILITY_CHANGE_SPEED))
				.Append(" | baseSpdK:")
				.Append(smethod_2(num7, "F6"))
				.Append(" | Δ:")
				.Append(smethod_2(deltaTime * num7, "F6"))
				.Append(" | dist:(")
				.Append(smethod_2(clampedDistance))
				.Append(")")
				.Append(smethod_2(num))
				.Append(" max:")
				.Append(smethod_2(Owner.Settings.FileSettings.Look.MAX_DIST_CLAMP_TO_SEEN_SPEED))
				.Append(" | pose:")
				.Append(smethod_2(poseVisibilityCoef))
				.Append(" | ang:(")
				.Append(smethod_2(angleToEnemy))
				.Append(")")
				.Append(smethod_2(num3, "F6"))
				.Append(" | flare:")
				.Append(smethod_2(flarePower))
				.Append(" | repeat:")
				.Append(smethod_2(num2))
				.Append(" | rtFx:")
				.Append(smethod_2(settings.Current.RuntimeVisionEffectsK))
				.Append(" | infect:")
				.Append(smethod_2(num4))
				.Append(" | foli:")
				.Append(smethod_2(num5))
				.Append(" (")
				.Append(foliageIntersectionPercent.ToString("P0", CultureInfo.InvariantCulture))
				.Append(")")
				.Append(" | wth:")
				.Append(smethod_2(num6))
				.Append("=rain:")
				.Append(smethod_2(rainK))
				.Append("*fog:")
				.Append(smethod_2(fogK));
		}
		else if (GClass398.Instance.IsVisionLogEnabled(LogLevel.Debug) || GClass398.Instance.IsVisionLogEnabled(LogLevel.Info))
		{
			VisibilityKBuilder.Append("t2bVis:").Append(smethod_2(1f / (Owner.Settings.FileSettings.Look.VISIBILITY_CHANGE_SPEED * num7))).Append(" sec | ")
				.Append("main:")
				.Append(smethod_2(Owner.Settings.FileSettings.Look.VISIBILITY_CHANGE_SPEED))
				.Append(" | run:")
				.Append(smethod_2(num7));
		}
		return num7;
	}

	public float method_10(out float foliageIntersectionPercent)
	{
		foliageIntersectionPercent = Owner.AIData.FoliageIntersectionPercent;
		if (Owner.AIData.IsInTree && !(Owner.AIData.FoliageIntersectionPercent <= 0.7f))
		{
			return Owner.Settings.FileSettings.Look.INSIDE_BUSH_COEF;
		}
		return 1f;
	}

	public float method_11(bool enemyIsInside, out float rainK, out float fogK)
	{
		if ((Owner.AIData?.IsInside ?? false) && enemyIsInside)
		{
			float num = 1f;
			fogK = 1f;
			float num2 = num;
			num = 1f;
			rainK = num2;
			return num;
		}
		return Owner.LookSensor.WeatherVisibilityK(Owner.Settings.FileSettings.Look.RAIN_DEBUFF_SEENCOEFF_MULTIPLYER, Owner.Settings.FileSettings.Look.FOG_DEBUFF_SEENCOEFF_MULTIPLYER, out rainK, out fogK);
	}

	public float method_12()
	{
		if (BotSettingsRepoClass.IsInfected(Owner.Profile.Info.Settings.Role) && Owner.BotsController.EventsController.BotHalloweenWithZombies != null)
		{
			return Owner.BotsController.EventsController.BotHalloweenWithZombies.ZombieLookCoeff;
		}
		return 1f;
	}

	public float method_13(float distanceToEnemyNormalized)
	{
		return Mathf.Lerp(Owner.Settings.FileSettings.Look.MIN_DISTANCE_VISIBILITY_CHANGE_SPEED_K, Owner.Settings.FileSettings.Look.MAX_DISTANCE_VISIBILITY_CHANGE_SPEED_K, Mathf.Pow(distanceToEnemyNormalized, 2f));
	}

	public float method_14(BotDifficultySettingsClass settings, float distanceToEnemyNormalized, out float angleToEnemy)
	{
		angleToEnemy = (Owner.LookSensor.IsFullSectorView ? 0.1f : Vector3.Angle(Owner.LookDirection, Direction));
		float b = settings.Curv.VisionAngCoef.Evaluate(angleToEnemy / 90f);
		return Mathf.Lerp(1f, b, 1f - Mathf.Pow(1f - distanceToEnemyNormalized, Owner.Settings.FileSettings.Look.ANGLE_VISION_COEF_FILTER));
	}

	public float method_15(float personalLastSeenTime, Vector3 personalLastSeenPos)
	{
		if (Time.time - personalLastSeenTime < Owner.Settings.FileSettings.Look.SEC_REPEATED_SEEN && (double)(personalLastSeenPos - CurrPosition).sqrMagnitude < Owner.Settings.FileSettings.Look.DIST_SQRT_REPEATED_SEEN)
		{
			return Owner.Settings.FileSettings.Look.COEF_REPEATED_SEEN;
		}
		return 1f;
	}

	public void method_16(bool withLegs, bool canBeHead)
	{
		HashSet<BodyPartType> allActiveParts = AllActiveParts;
		List<BodyPartType> list = new List<BodyPartType>();
		foreach (BodyPartType item in allActiveParts)
		{
			if ((AllParts[item].CanShoot || item == BodyPartType.body) && (withLegs || (item != BodyPartType.leftLeg && item != BodyPartType.rightLeg)) && (canBeHead || item != BodyPartType.head))
			{
				list.Add(item);
			}
		}
		BodyPartType key = GClass856.RandomElement(list);
		if (LastPartToShoot != null && allActiveParts.Contains(LastPartToShoot.BodyPartType) && !AllParts[LastPartToShoot.BodyPartType].CanShoot)
		{
			LastPartToShoot = AllParts[key];
		}
		else if (NextPartRndTime < Time.time)
		{
			NextPartRndTime = Time.time + LocalBotSettingsProviderClass.Core.SHOOT_TO_CHANGE_RND_PART_DELTA;
			LastPartToShoot = AllParts[key];
		}
		if (LastPartToShoot == null)
		{
			LastPartToShoot = AllParts[key] ?? AllParts[BodyPartType.body];
		}
	}

	public void method_17(HashSet<BodyPartType> activePartsTypes, bool startSeeing)
	{
		if (GClass398.Instance.IsVisionLogEnabled(LogLevel.Trace))
		{
			if (EnemyVision.VisibilityLevel > 0f)
			{
				EnemyBuilder.Append("botId: ").Append(Owner.Id).Append("\tenemyId:")
					.Append(Person.Id)
					.Append("\t")
					.Append(startSeeing ? "start " : string.Empty)
					.Append(EnemyVision);
				EnemyBuilder.AppendFormat("vis_k: {0}|", VisibilityKBuilder);
				EnemyBuilder.AppendJoin(string.Empty, activePartsTypes.Select(delegate(BodyPartType partType)
				{
					GClass542 gClass = AllPartsVision[partType];
					EnemyPart enemyPart = AllParts[partType];
					PartBuilder.Append(" p:").Append(GClass856.GetSymbol(partType)).Append(" | shoot:")
						.Append(enemyPart.CanShoot.ToString().PadRight(5))
						.Append(" | ");
					gClass.AppendLogInfo(PartBuilder, LogLevel.Trace);
					string result = PartBuilder.ToString();
					PartBuilder.Clear();
					return result;
				}));
				EnemyBuilder.Clear();
			}
		}
		else if (GClass398.Instance.IsVisionLogEnabled(LogLevel.Debug))
		{
			if (EnemyVision.VisibilityLevel > 0f)
			{
				EnemyBuilder.Append("botId: ").Append(Owner.Id).Append("\tenemyId: ")
					.Append(Person.Id)
					.Append("\t")
					.Append(startSeeing ? "start " : string.Empty)
					.Append(EnemyVision);
				EnemyBuilder.AppendFormat("vis_k: {0}|", VisibilityKBuilder);
				EnemyBuilder.AppendJoin(string.Empty, activePartsTypes.Select(delegate(BodyPartType partType)
				{
					GClass542 gClass = AllPartsVision[partType];
					EnemyPart enemyPart = AllParts[partType];
					PartBuilder.Append(" p:").Append(GClass856.GetSymbol(partType)).Append(" | shoot:")
						.Append(enemyPart.CanShoot.ToString().PadRight(5))
						.Append(" | ");
					gClass.AppendLogInfo(PartBuilder, LogLevel.Debug);
					string result = PartBuilder.ToString();
					PartBuilder.Clear();
					return result;
				}));
				EnemyBuilder.Clear();
			}
		}
		else if (GClass398.Instance.IsVisionLogEnabled(LogLevel.Info) && PreviousVisibleType != EnemyVision.VisibleType)
		{
			PreviousVisibleType = EnemyVision.VisibleType;
			EnemyBuilder.Append("botId: ").Append(Owner.Id).Append("\tenemyId: ")
				.Append(Person.Id)
				.Append("\t")
				.Append(EnemyVision)
				.Append(VisibilityKBuilder);
			EnemyBuilder.Clear();
		}
		VisibilityKBuilder.Clear();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string smethod_2(float value, string formatString = "F2")
	{
		return value.ToString(formatString, CultureInfo.InvariantCulture).PadLeft(6);
	}

	public virtual void Dispose()
	{
	}

	[CompilerGenerated]
	public void method_18()
	{
		if (Vector3.Dot(Owner.LookDirection, Person.LookDirection) > 0f && GClass856.IsTrue100(Owner.Settings.FileSettings.Aiming.NEXT_SHOT_MISS_CHANCE_100))
		{
			Owner.AimingManager.CurrentAiming.NextShotMiss(1);
		}
	}

	[CompilerGenerated]
	public bool method_19(bool haveSeenPersonal, out bool missShots)
	{
		missShots = false;
		if (haveSeenPersonal)
		{
			return false;
		}
		if (Person.IsAI)
		{
			return false;
		}
		if (!Owner.LookSensor.HardToSeeFor(this))
		{
			return false;
		}
		Owner.BotTalk.DropNextSayPeriod();
		Owner.BotTalk.Say(EPhraseTrigger.OnFirstContact, sayImmediately: true);
		float fIRST_CONTACT_HARD_TO_SEE_MISS_SHOOTS_DISTANCE = Owner.Settings.FileSettings.Aiming.FIRST_CONTACT_HARD_TO_SEE_MISS_SHOOTS_DISTANCE;
		if (Distance <= fIRST_CONTACT_HARD_TO_SEE_MISS_SHOOTS_DISTANCE)
		{
			return true;
		}
		missShots = true;
		Owner.AimingManager.CurrentAiming.NextShotMiss(Owner.Settings.FileSettings.Aiming.FIRST_CONTACT_HARD_TO_SEE_MISS_SHOOTS_COUNT);
		return true;
	}

	[CompilerGenerated]
	public string method_20(BodyPartType partType)
	{
		GClass542 gClass = AllPartsVision[partType];
		EnemyPart enemyPart = AllParts[partType];
		PartBuilder.Append(" p:").Append(GClass856.GetSymbol(partType)).Append(" | shoot:")
			.Append(enemyPart.CanShoot.ToString().PadRight(5))
			.Append(" | ");
		gClass.AppendLogInfo(PartBuilder, LogLevel.Trace);
		string result = PartBuilder.ToString();
		PartBuilder.Clear();
		return result;
	}

	[CompilerGenerated]
	public string method_21(BodyPartType partType)
	{
		GClass542 gClass = AllPartsVision[partType];
		EnemyPart enemyPart = AllParts[partType];
		PartBuilder.Append(" p:").Append(GClass856.GetSymbol(partType)).Append(" | shoot:")
			.Append(enemyPart.CanShoot.ToString().PadRight(5))
			.Append(" | ");
		gClass.AppendLogInfo(PartBuilder, LogLevel.Debug);
		string result = PartBuilder.ToString();
		PartBuilder.Clear();
		return result;
	}
}
