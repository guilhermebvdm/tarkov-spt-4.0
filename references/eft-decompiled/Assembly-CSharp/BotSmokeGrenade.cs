using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class BotSmokeGrenade : GClass429
{
	[NonSerialized]
	public HashSet<int> AffectedSmokes = new HashSet<int>();

	[NonSerialized]
	public BotLastBlindEffectModifierClass SmokeModif;

	[NonSerialized]
	public GClass578 LastPlace;

	[NonSerialized]
	public bool WillShootToSmoke;

	[NonSerialized]
	public float NextCheckTime;

	[NonSerialized]
	public float SmokeEndTime;

	[field: NonSerialized]
	public bool IsInSmoke { get; set; }

	public Vector3 PlaceToShoot => LastPlace.Position + Vector3.up * 1.3f;

	public BotSmokeGrenade(BotOwner owner)
		: base(owner)
	{
	}

	public void SmokeEnter(int throwableId)
	{
		AffectedSmokes.Add(throwableId);
		method_0();
	}

	public void SmokeExit(int throwableId)
	{
		AffectedSmokes.Remove(throwableId);
		method_0();
	}

	public bool ShallShoot()
	{
		if (!WillShootToSmoke)
		{
			return false;
		}
		if (NextCheckTime > Time.time)
		{
			return WillShootToSmoke;
		}
		NextCheckTime = Time.time + BotOwner_0.Settings.FileSettings.Grenade.SMOKE_CHECK_DELTA;
		if (LastPlace == null)
		{
			WillShootToSmoke = false;
			return WillShootToSmoke;
		}
		Vector3 placeToShoot = PlaceToShoot;
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		bool flag = false;
		if (goalEnemy != null)
		{
			bool flag2 = (goalEnemy.EnemyLastPosition - placeToShoot).sqrMagnitude < BotOwner_0.Settings.FileSettings.Grenade.CLOSE_TO_SMOKE_TO_SHOOT_SQRT;
			flag = Time.time - goalEnemy.TimeLastSeen < BotOwner_0.Settings.FileSettings.Grenade.CLOSE_TO_SMOKE_TIME_DELTA && flag2;
		}
		if (!flag)
		{
			PlaceForCheck placeForCheck = BotOwner_0.BotsGroup.YoungestFastPlace(BotOwner_0, 50f, 15f);
			if (placeForCheck != null)
			{
				bool flag3 = (placeForCheck.Position - placeToShoot).sqrMagnitude < BotOwner_0.Settings.FileSettings.Grenade.CLOSE_TO_SMOKE_TO_SHOOT_SQRT;
				flag = Time.time - placeForCheck.CreatedTime < BotOwner_0.Settings.FileSettings.Grenade.CLOSE_TO_SMOKE_TIME_DELTA && flag3;
			}
		}
		if (!flag)
		{
			WillShootToSmoke = false;
			return WillShootToSmoke;
		}
		Vector3 direction = placeToShoot - BotOwner_0.WeaponRoot.position;
		float num = direction.magnitude * 0.9f;
		Ray ray = new Ray(BotOwner_0.WeaponRoot.position, direction);
		if (Physics.Raycast(ray, num, LayerMaskClass.HighPolyWithTerrainMask))
		{
			Debug.DrawRay(ray.origin, ray.direction * num, Color.red, 0.1f);
			WillShootToSmoke = false;
			return WillShootToSmoke;
		}
		Debug.DrawRay(ray.origin, ray.direction * num, Color.green, 0.1f);
		WillShootToSmoke = true;
		return WillShootToSmoke;
	}

	public void AddSmokeGrenadeData(GClass578 place)
	{
		if (GClass856.IsTrue100(BotOwner_0.Settings.FileSettings.Grenade.AMBUSH_IF_SMOKE_IN_ZONE_100))
		{
			BotOwner_0.Tactic.SetTactic(BotsGroup.BotCurrentTactic.Ambush, shallAutoReturnToAttack: true, BotOwner_0.Settings.FileSettings.Grenade.AMBUSH_IF_SMOKE_RETURN_TO_ATTACK_SEC);
		}
		LastPlace = place;
		WillShootToSmoke = GClass856.IsTrue100(BotOwner_0.Settings.FileSettings.Grenade.SHOOT_TO_SMOKE_CHANCE_100);
		float radius = place.Radius;
		float duration = place.Duration;
		BotOwner botOwner_ = BotOwner_0;
		if ((place.Position - botOwner_.Position).magnitude < radius * BotOwner_0.Settings.FileSettings.Grenade.BE_ATTENTION_COEF)
		{
			List<CustomNavigationPoint> closePoints = botOwner_.BotsGroup.CoverPointMaster.GetClosePoints(place.Position, botOwner_, radius * BotOwner_0.Settings.FileSettings.Grenade.SIZE_SPOTTED_COEF);
			for (int i = 0; i < closePoints.Count; i++)
			{
				closePoints[i].Spotted(duration);
			}
		}
	}

	public void method_0()
	{
		bool isInSmoke = IsInSmoke;
		IsInSmoke = AffectedSmokes.Count > 0;
		if (isInSmoke == IsInSmoke)
		{
			return;
		}
		if (IsInSmoke)
		{
			if (SmokeModif != null)
			{
				BotOwner_0.Settings.Current.Dismiss(SmokeModif);
			}
			SmokeModif = new BotLastBlindEffectModifierClass(BotOwner_0.Settings.FileSettings.Change.SMOKE_PRECICING, BotOwner_0.Settings.FileSettings.Change.SMOKE_ACCURATY, BotOwner_0.Settings.FileSettings.Change.SMOKE_LAY_CHANCE, BotOwner_0.Settings.FileSettings.Change.SMOKE_VISION_DIST, BotOwner_0.Settings.FileSettings.Change.SMOKE_GAIN_SIGHT, BotOwner_0.Settings.FileSettings.Change.SMOKE_SCATTERING, BotOwner_0.Settings.FileSettings.Change.SMOKE_HEARING, BotOwner_0.Settings.FileSettings.Change.SMOKE_SCATTERING, 1f);
			BotOwner_0.Settings.Current.Apply(SmokeModif);
		}
		else
		{
			BotOwner_0.Settings.Current.Dismiss(SmokeModif);
		}
	}
}
