using System;
using System.Collections.Generic;
using EFT;
using EFT.InventoryLogic;
using EFT.Weather;
using UnityEngine;

public class LookSensor : GInterface13
{
	public readonly float VISIBLE_ANGLE = -0.3420201f;

	public readonly float VISIBLE_ANGLE_LIGHT = 0.5f;

	public readonly float VISIBLE_ANGLE_NIGHTVISION = 0.5f;

	[NonSerialized]
	public BotOwner BotOwner;

	[NonSerialized]
	public BifacialTransform WeaponRootTransform;

	[NonSerialized]
	public BifacialTransform OwnerTransform;

	[NonSerialized]
	public Vector3 WeaponRootPoint;

	[NonSerialized]
	public GClass609 LookSensorShootPosition;

	public Vector3 HeadPoint;

	[NonSerialized]
	public float UpdateTimer;

	[NonSerialized]
	public int LookCount;

	[NonSerialized]
	public AnimationCurve VisionCurve;

	[NonSerialized]
	public bool IsTaskCompleted = true;

	[NonSerialized]
	public BotDifficultySettingsClass BotSettings;

	[NonSerialized]
	public float WeaponShootDistMaxSqr = 200f;

	[NonSerialized]
	public float PreferedFireDist = 30f;

	[NonSerialized]
	public float NextUpdateVisibleDist;

	[NonSerialized]
	public bool IsBossOrFollower;

	[NonSerialized]
	public float TaskMaxWaitPeriod = 0.5f;

	[NonSerialized]
	public List<EnemyInfo> CacheToLookEnemyInfos = new List<EnemyInfo>(16);

	[NonSerialized]
	public float TimeAddTask;

	[NonSerialized]
	public bool CanLookThroughGrassBySettings;

	[NonSerialized]
	public LookAllDataClass CheckEnemiesLookAllData;

	[NonSerialized]
	public bool LookingThroughGrassTemporary;

	[NonSerialized]
	public float EndLookingThroughGrassTemporary;

	[NonSerialized]
	public AITaskManager TaskManager;

	public bool ShootFromEyes;

	[NonSerialized]
	public bool IsFullSectorView_1;

	[NonSerialized]
	public Collider[] LookObstaclesBuffer = new Collider[1];

	public Vector3 ShootStartPos => LookSensorShootPosition.ShootStartPos;

	[field: NonSerialized]
	public LayerMask Mask { get; set; }

	public float PreferedShootDist
	{
		get
		{
			if (PreferedFireDist < 1f)
			{
				return 50f;
			}
			return PreferedFireDist;
		}
	}

	[field: NonSerialized]
	public float MaxShootDist { get; set; } = 200f;

	[field: NonSerialized]
	public float VisibleDist { get; set; }

	[field: NonSerialized]
	public float ClearVisibleDist { get; set; }

	[field: NonSerialized]
	public int HourServer { get; set; }

	[field: NonSerialized]
	public bool CurLookThroughGrass { get; set; }

	public bool IsFullSectorView => IsFullSectorView_1;

	public LookSensor(BotOwner botOwner)
	{
		BotOwner = botOwner;
		OwnerTransform = botOwner.Transform;
		WeaponRootTransform = BotOwner.Fireport;
		BotSettings = botOwner.Settings;
		VISIBLE_ANGLE = Mathf.Cos(BotSettings.FileSettings.Core.VisibleAngle * (MathF.PI / 180f));
		VISIBLE_ANGLE_LIGHT = Mathf.Cos(BotSettings.FileSettings.Look.VISIBLE_ANG_LIGHT * (MathF.PI / 180f));
		VISIBLE_ANGLE_NIGHTVISION = Mathf.Cos(BotSettings.FileSettings.Look.VISIBLE_ANG_NIGHTVISION * (MathF.PI / 180f));
		ShootFromEyes = BotSettings.FileSettings.Look.SHOOT_FROM_EYES;
		IsFullSectorView_1 = BotSettings.FileSettings.Look.FULL_SECTOR_VIEW;
		if (BotSettings.FileSettings.Look.SELF_NIGHTVISION)
		{
			VisionCurve = BotOwner.Settings.Curv.NightVisionSettings;
		}
		else
		{
			VisionCurve = BotOwner.Settings.Curv.StandartVisionSettings;
		}
		if (botOwner.Profile.Info.Settings.Role == WildSpawnType.shooterBTR)
		{
			LookSensorShootPosition = new GClass610(BotOwner);
		}
		else
		{
			LookSensorShootPosition = new GClass609(BotOwner);
		}
		CheckEnemiesLookAllData = new LookAllDataClass();
		TaskManager = BotOwner.BotsController.AiTaskManager;
	}

	public void UpdateZoneValue(BotZone botZone)
	{
		PreferedFireDist *= botZone.DistanceCoef;
	}

	public void Activate()
	{
		method_2();
		TaskMaxWaitPeriod = BotOwner.Settings.FileSettings.Look.POSIBLE_VISION_SPACE * 0.75f;
		IsBossOrFollower = GClass2190.IsBossOrFollower(BotOwner.Profile.Info.Settings);
		TaskManager.RegisterRegularTask(EAITaskGroupType.LookSensor, this);
	}

	public void Init()
	{
		CanLookThroughGrassBySettings = BotOwner.Settings.FileSettings.Look.LOOK_THROUGH_GRASS;
		if (!CanLookThroughGrassBySettings && BotOwner.Settings.FileSettings.Look.LOOK_THROUGH_PERIOD_BY_HIT > 0f)
		{
			BotOwner.GetPlayer.BeingHitAction += method_0;
		}
		method_1(CanLookThroughGrassBySettings);
		Weapon currentWeapon = BotOwner.WeaponManager.CurrentWeapon;
		int bEffDist = currentWeapon.Template.bEffDist;
		MaxShootDist = (float)bEffDist * BotSettings.FileSettings.Shoot.MAX_DIST_COEF;
		if (currentWeapon is PistolItemClass)
		{
			PreferedFireDist = BotSettings.FileSettings.Core.PistolFireDistancePref;
		}
		else if (currentWeapon is ShotgunItemClass)
		{
			PreferedFireDist = BotSettings.FileSettings.Core.ShotgunFireDistancePref;
		}
		else
		{
			PreferedFireDist = BotSettings.FileSettings.Core.RifleFireDistancePref;
		}
		if (MaxShootDist < 30f)
		{
			Debug.LogError("weapon.Template.bEffDist < 30. Name:" + currentWeapon.Template.Name);
			MaxShootDist = 30f;
		}
		if (PreferedFireDist <= 20f)
		{
			PreferedFireDist = 20f;
			string text = ((currentWeapon == null) ? "weapon null" : ("TemplateId" + currentWeapon.TemplateId));
			Debug.LogError("Bot shoot Preferred DIST is LESS  <= 20 meters!!!!!!!  Watafak!!!!! + _preferedFireDist <= 1f weapon:" + text);
		}
		MaxShootDist *= UnityEngine.Random.Range(0.9f, 1.1f);
		PreferedFireDist = GClass856.GreateRandom(PreferedFireDist);
		WeaponShootDistMaxSqr = MaxShootDist * MaxShootDist;
		WeaponShootDistMaxSqr = MaxShootDist * MaxShootDist;
	}

	public void UpdateGroupsValue(BotsGroup botsGroup)
	{
		PreferedFireDist *= botsGroup.BotZone.DistanceCoef;
	}

	public void ManualUpdate()
	{
		if (LookingThroughGrassTemporary && EndLookingThroughGrassTemporary < Time.time)
		{
			LookingThroughGrassTemporary = false;
			method_1(val: false);
		}
	}

	public bool CheckLookSimple(Player from, Player to)
	{
		EnemyPart enemyPart = from.MainParts[BodyPartType.head];
		Vector3 direction = to.MainParts[BodyPartType.head].Position - enemyPart.Position;
		float magnitude = direction.magnitude;
		RaycastHit hitInfo;
		return !Physics.Raycast(new Ray(enemyPart.Position, direction), out hitInfo, magnitude, LayerMaskClass.HighPolyWithTerrainMask);
	}

	public bool EnoughDistToShoot(out bool canRunNoAmmo)
	{
		if (BotOwner.Memory.GoalEnemy == null)
		{
			canRunNoAmmo = false;
			return false;
		}
		float sqrMagnitude = (BotOwner.Memory.GoalEnemy.CurrPosition - OwnerTransform.position).sqrMagnitude;
		canRunNoAmmo = sqrMagnitude > BotOwner.Settings.FileSettings.Shoot.RUN_DIST_NO_AMMO_SQRT;
		return sqrMagnitude < WeaponShootDistMaxSqr;
	}

	public void DrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(WeaponRootPoint, 0.3f);
		Gizmos.DrawWireSphere(WeaponRootPoint, 0.26f);
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(LookSensorShootPosition.ShootStartPos, 0.2f);
	}

	public void method_0(DamageInfoStruct arg1, EBodyPart arg2, float arg3)
	{
		if (!CanLookThroughGrassBySettings)
		{
			method_1(val: true);
			LookingThroughGrassTemporary = true;
			EndLookingThroughGrassTemporary = Time.time + BotOwner.Settings.FileSettings.Look.LOOK_THROUGH_PERIOD_BY_HIT;
		}
	}

	public void method_1(bool val)
	{
		CurLookThroughGrass = val;
		if (CurLookThroughGrass)
		{
			Mask = LayerMaskClass.HighPolyWithTerrainMask;
		}
		else
		{
			Mask = LayerMaskClass.HighPolyWithTerrainMaskAI;
		}
	}

	public void method_2()
	{
		if (NextUpdateVisibleDist < Time.time)
		{
			float currentVisibleDistance = BotOwner.Settings.Current.CurrentVisibleDistance;
			float num = 1f;
			if (BotOwner.GameDateTime != null && VisionCurve != null)
			{
				DateTime dateTime = BotOwner.GameDateTime.Calculate();
				float time = (float)dateTime.Hour + (float)dateTime.Minute / 60f;
				num = VisionCurve.Evaluate(time);
				HourServer = (short)dateTime.Hour;
			}
			BotGlobalLookData look = BotOwner.Settings.FileSettings.Look;
			IAIData aIData = BotOwner.AIData;
			float rainK;
			float fogK;
			float num2 = ((aIData == null || !aIData.IsInside) ? WeatherVisibilityK(look.RAIN_DEBUFF_MAXVISIBILITY_MULTIPLYER, look.FOG_DEBUFF_MAXVISIBILITY_MULTIPLYER, out rainK, out fogK) : 1f);
			ClearVisibleDist = Mathf.Clamp(currentVisibleDistance * num * num2, look.MINIMUM_VISIBLE_DIST, 9999f);
			VisibleDist = BotOwner.NightVision.UpdateVision(ClearVisibleDist);
			VisibleDist = BotOwner.BotLight.UpdateLightEnable(VisibleDist);
			NextUpdateVisibleDist = Time.time + (float)(BotOwner.FlashGrenade.IsFlashed ? 3 : 10);
		}
		BotOwner.BotLight.UpdateStrope();
	}

	public void AIPeriodicUpdate(float deltaTime)
	{
		method_3(deltaTime);
	}

	void GInterface13.AIPeriodicUpdate(float deltaTime)
	{
		//ILSpy generated this explicit interface implementation from .override directive in AIPeriodicUpdate
		this.AIPeriodicUpdate(deltaTime);
	}

	public void method_3(float deltaTime)
	{
		method_2();
		if (!(BotOwner == null) && !BotOwner.IsDead && BotOwner.LeaveData != null && !BotOwner.LeaveData.LeaveComplete)
		{
			WeaponRootPoint = WeaponRootTransform.position;
			LookSensorShootPosition.UpdateShootPosition(WeaponRootPoint);
			HeadPoint = BotOwner.MyHead.position;
			CheckEnemiesLookAllData.Reset();
			CheckAllEnemies(CheckEnemiesLookAllData, deltaTime);
			LookAllDataClass checkEnemiesLookAllData = CheckEnemiesLookAllData;
			if (CacheToLookEnemyInfos.Count > 0 && !IsBossOrFollower && !BotOwner.Tactic.IsCurTactic(BotsGroup.BotCurrentTactic.Protect))
			{
				BotsGroup.BotCurrentTactic tactic = ((checkEnemiesLookAllData.MinDistance > BotOwner.WeaponManager.AmbushDistance) ? BotsGroup.BotCurrentTactic.Ambush : BotsGroup.BotCurrentTactic.Attack);
				BotOwner.Tactic.SetTactic(tactic);
			}
			for (int i = 0; i < checkEnemiesLookAllData.ReportsData.Count; i++)
			{
				BotReportsDataClass botReportsDataClass = checkEnemiesLookAllData.ReportsData[i];
				BotOwner.BotsGroup.ReportAboutEnemy(botReportsDataClass.Enemy, botReportsDataClass.VisibleOnlyBySence, BotOwner);
			}
			if (checkEnemiesLookAllData.ReportsData.Count > 0)
			{
				BotOwner.Memory.SetLastTimeSeeEnemy();
			}
			if (checkEnemiesLookAllData.ShallRecalcGoal)
			{
				BotOwner.CalcGoal();
			}
			CheckEnemiesLookAllData.Reset();
		}
	}

	public void CheckAllEnemies(LookAllDataClass lookAll, float deltaTime)
	{
		CacheToLookEnemyInfos.Clear();
		CacheToLookEnemyInfos.AddRange(BotOwner.EnemiesController.EnemyInfos.Values);
		BotOwner.EnemiesController.BestObservedEnemy = null;
		foreach (EnemyInfo cacheToLookEnemyInfo in CacheToLookEnemyInfos)
		{
			try
			{
				cacheToLookEnemyInfo.CheckLookEnemy(lookAll, deltaTime);
			}
			catch (Exception ex)
			{
				BotOwner.BotsGroup.BotGame.BotsController.DebugLogsAboultRemoveEnemies();
				BotOwner.EnemiesController.EnemyInfos.Remove(cacheToLookEnemyInfo.Person);
				bool flag = cacheToLookEnemyInfo.Person.HealthController != null && cacheToLookEnemyInfo.Person.HealthController.IsAlive;
				Debug.LogError($" enemyInfo.ProfileId:{cacheToLookEnemyInfo.ProfileId} byBotId:{BotOwner.Id}   botProfileId:{BotOwner.Profile.Id}  isAlive:{flag} error:{ex}");
			}
			if (cacheToLookEnemyInfo.VisibilityLevel > 0f && !cacheToLookEnemyInfo.IsVisible)
			{
				if (BotOwner.EnemiesController.BestObservedEnemy == null)
				{
					BotOwner.EnemiesController.BestObservedEnemy = cacheToLookEnemyInfo;
				}
				else if (cacheToLookEnemyInfo.VisibilityLevel > BotOwner.EnemiesController.BestObservedEnemy.VisibilityLevel)
				{
					BotOwner.EnemiesController.BestObservedEnemy = cacheToLookEnemyInfo;
				}
			}
		}
	}

	public bool IsPointInVisibleSector(Vector3 position)
	{
		if (IsFullSectorView)
		{
			return true;
		}
		Vector3 v = position - BotOwner.Position;
		return GClass855.IsAngLessNormalized(cos: (!BotOwner.NightVision.UsingNow) ? (BotOwner.BotLight.IsEnable ? VISIBLE_ANGLE_LIGHT : VISIBLE_ANGLE) : VISIBLE_ANGLE_NIGHTVISION, a: BotOwner.LookDirection, b: GClass855.NormalizeFastSelf(v));
	}

	public void Dispose()
	{
		BotOwner.GetPlayer.BeingHitAction -= method_0;
		CacheToLookEnemyInfos.Clear();
		TaskManager.UnregisterRegularTask(this);
	}

	public float method_4()
	{
		if (WeatherController.Instance == null)
		{
			return 0f;
		}
		if (BotOwner?.BotsController?.BotGame?.WeatherCurve == null)
		{
			return 0f;
		}
		return BotOwner.BotsController.BotGame.WeatherCurve.Fog;
	}

	public float method_5()
	{
		if (WeatherController.Instance == null)
		{
			return 0f;
		}
		if (BotOwner?.BotsController?.BotGame?.WeatherCurve == null)
		{
			return 0f;
		}
		return BotOwner.BotsController.BotGame.WeatherCurve.Rain;
	}

	public float WeatherVisibilityK(float rainMaxInfluenceK, float fogMaxInfluenceK, out float rainK, out float fogK)
	{
		float t = method_5();
		float t2 = GClass2603.NormalizeFogValueToActualMinMax(method_4());
		rainK = Mathf.Lerp(1f, rainMaxInfluenceK, t);
		fogK = Mathf.Lerp(1f, fogMaxInfluenceK, t2);
		return rainK * fogK;
	}

	public bool HardToSeeFor(EnemyInfo enemy)
	{
		return Physics.OverlapCapsuleNonAlloc(HeadPoint, enemy.CurrPosition + Vector3.up, 1f, LookObstaclesBuffer, LayerMaskClass.Foliage) > 0;
	}
}
