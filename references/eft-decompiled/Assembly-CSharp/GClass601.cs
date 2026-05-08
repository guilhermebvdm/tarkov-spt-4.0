using System;
using System.Collections.Generic;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class GClass601 : GClass598
{
	[NonSerialized]
	public static List<AIGreandeAng> List_0 = new List<AIGreandeAng>
	{
		AIGreandeAng.ang25,
		AIGreandeAng.ang45
	};

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public ThrowWeapType? Nullable_1;

	public override Vector3 PlaceToThrow => base.ThrowData.Position;

	public static AIGreanageThrowData GetThrowTraectory(Vector3 placeToThrow, BotOwner executor, ThrowWeapType? throwType, Vector3? fromThrowPlace, List<AIGreandeAng> angsToTest = null)
	{
		if (angsToTest == null)
		{
			angsToTest = List_0;
		}
		AIGreanageThrowData aIGreanageThrowData = null;
		float maxThrowDistSqrt = executor.Settings.FileSettings.Grenade.MIN_THROW_GRENADE_DIST_SQRT;
		if (throwType.HasValue && throwType == ThrowWeapType.smoke_grenade)
		{
			maxThrowDistSqrt = 0.1f;
		}
		foreach (AIGreandeAng item in angsToTest)
		{
			aIGreanageThrowData = GClass577.CanThrowGrenade2(executor.Position + BotOwner.STAY_HEIGHT, placeToThrow, executor.WeaponManager.Grenades, item, maxThrowDistSqrt);
			if (aIGreanageThrowData != null && aIGreanageThrowData.CanThrow)
			{
				aIGreanageThrowData.GrenadeType = throwType;
				return aIGreanageThrowData;
			}
		}
		CoverSearchData data = new CoverSearchData(executor.Position, executor.CoverSearchInfo, CoverShootType.grenade, LocalBotSettingsProviderClass.Core.START_DIST_TO_COV, 0f, CoverSearchType.shoot_toCover_toBot_Distances, new ShootPointClass(placeToThrow), null, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(-1f));
		if (!fromThrowPlace.HasValue)
		{
			CustomNavigationPoint coverPointMain = executor.BotsGroup.CoverPointMaster.GetCoverPointMain(data, checkCurrent: false);
			if (coverPointMain != null)
			{
				fromThrowPlace = coverPointMain.FirePosition;
			}
		}
		else
		{
			fromThrowPlace = fromThrowPlace.Value + BotOwner.STAY_HEIGHT;
		}
		if (fromThrowPlace.HasValue)
		{
			foreach (AIGreandeAng item2 in angsToTest)
			{
				aIGreanageThrowData = GClass577.CanThrowGrenade2(fromThrowPlace.Value, placeToThrow, executor.WeaponManager.Grenades, item2, maxThrowDistSqrt);
				if (aIGreanageThrowData != null && aIGreanageThrowData.CanThrow)
				{
					aIGreanageThrowData.GrenadeType = throwType;
					return aIGreanageThrowData;
				}
			}
		}
		return aIGreanageThrowData;
	}

	public GClass601(IPlayer requester, Vector3 placeToThrow, ThrowWeapType? throwType = null)
		: base(requester)
	{
		Nullable_1 = throwType;
		base.CanExecuteByMyself = true;
		Vector3_0 = placeToThrow;
	}

	public GClass601(IPlayer requester, AIGreanageThrowData throwData)
		: base(requester)
	{
		Nullable_1 = throwData.GrenadeType;
		base.CanExecuteByMyself = true;
		base.ThrowData = throwData;
	}

	public override void Take(BotOwner executor)
	{
		if (base.ThrowData == null)
		{
			base.ThrowData = GetThrowTraectory(Vector3_0, executor, Nullable_1, null);
		}
		float duration = 10f;
		GClass810.DebugPoint(Vector3_0, Color.magenta, 1.4f, duration);
		GClass810.DebugPoint(base.ThrowData.Position, Color.yellow, 1.2f, duration);
		GClass810.DebugPoint(base.ThrowData.Target, Color.green, 1f, duration);
		if (base.ThrowData != null && base.ThrowData.CanThrow)
		{
			base.ThrowFromPos = base.ThrowData.Position;
			executor.WeaponManager.Grenades.SetThrowData(base.ThrowData);
			executor.WeaponManager.Grenades.OnGrenadeThrowComplete += base.method_0;
			base.Take(executor);
		}
		else
		{
			Dispose();
		}
	}
}
