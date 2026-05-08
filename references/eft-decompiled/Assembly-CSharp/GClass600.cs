using System;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class GClass600 : GClass598
{
	[Serializable]
	[CompilerGenerated]
	public class Class283
	{
		public static readonly Class283 class283_0 = new Class283();

		public static Func<ThrowGrenadePlace, bool> func_0;

		public bool method_0(ThrowGrenadePlace x)
		{
			return x.IsNowValid();
		}
	}

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public Action Action_0;

	[NonSerialized]
	[CompilerGenerated]
	public Player Player_0_1;

	public Player Player_0
	{
		[CompilerGenerated]
		get
		{
			return Player_0_1;
		}
	}

	public override Vector3 PlaceToThrow => Vector3_0;

	[CanBeNull]
	public static ThrowGrenadePlace FindPlace(Player playerToThrow)
	{
		ThrowGrenadePlace[] array = playerToThrow.AIData.PlaceInfo.GrenadePlaces.Where((ThrowGrenadePlace x) => x.IsNowValid()).ToArray();
		if (array.Length == 0)
		{
			return null;
		}
		Vector3 position = playerToThrow.Position;
		IPositionPoint[] points = array;
		int nearEntity = GClass369.GetNearEntity(position, points);
		if (nearEntity < 0)
		{
			return null;
		}
		return array[nearEntity];
	}

	public GClass600(Player requester, Player targetToThrow, bool onlyCached, Action callbackFinish)
		: base(requester)
	{
		Action_0 = callbackFinish;
		Bool_0 = onlyCached;
		base.CanExecuteByMyself = true;
		Player_0_1 = targetToThrow;
	}

	public override void Take(BotOwner executor)
	{
		if (Player_0.AIData.PlaceInfo != null && Player_0.AIData.PlaceInfo.GrenadePlaces.Length != 0)
		{
			ThrowGrenadePlace throwGrenadePlace = FindPlace(Player_0);
			Vector3_0 = Player_0.AIData.PlaceInfo.transform.position;
			base.ThrowFromPos = throwGrenadePlace.From.position;
			base.ThrowData = throwGrenadePlace.GetData();
			executor.WeaponManager.Grenades.OnGrenadeThrowComplete += base.method_0;
			base.Take(executor);
		}
		else if (!Bool_0)
		{
			Vector3 position = Player_0.Position;
			CoverSearchData data = new CoverSearchData(executor.Position, executor.CoverSearchInfo, CoverShootType.grenade, LocalBotSettingsProviderClass.Core.START_DIST_TO_COV, 0f, CoverSearchType.shoot_toCover_toBot_Distances, new ShootPointClass(position), null, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(executor.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL));
			CustomNavigationPoint coverPointMain = executor.BotsGroup.CoverPointMaster.GetCoverPointMain(data, checkCurrent: false);
			if (coverPointMain != null)
			{
				Vector3_0 = position;
				base.ThrowFromPos = coverPointMain.FirePosition;
				AIGreanageThrowData throwData = GClass577.CanThrowGrenade2(coverPointMain.FirePosition, position, executor.WeaponManager.Grenades, AIGreandeAng.ang45, executor.Settings.FileSettings.Grenade.MIN_THROW_GRENADE_DIST_SQRT);
				base.ThrowData = throwData;
				executor.WeaponManager.Grenades.OnGrenadeThrowComplete += base.method_0;
				base.Take(executor);
			}
			else
			{
				Dispose();
			}
		}
		else
		{
			Dispose();
		}
	}

	public override void Dispose()
	{
		if (Action_0 != null)
		{
			Action_0();
		}
		base.Dispose();
	}
}
