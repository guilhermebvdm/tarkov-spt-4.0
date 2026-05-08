using System.Collections.Generic;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class BotDangerPointsData : GClass429
{
	public PlaceForCheck place;

	public bool realDanger;

	public float ExpireTime;

	public PanicType PanicType = PanicType.none;

	public float PanicPower;

	public bool IsPanic
	{
		get
		{
			if (BotOwner_0.Tactic.IsCurTactic(BotsGroup.BotCurrentTactic.Protect) && !BotOwner_0.Settings.FileSettings.Mind.CAN_PANIC_IS_PROTECT)
			{
				return false;
			}
			bool result;
			if ((result = HaveDangePoints) && ExpireTime < Time.time)
			{
				result = false;
				method_0();
			}
			return result;
		}
	}

	public bool HaveDangePoints => PanicType != PanicType.none;

	public BotDangerPointsData([NotNull] BotOwner owner)
		: base(owner)
	{
	}

	public void SetAllDangerNull()
	{
		if (place != null)
		{
			method_0();
		}
	}

	public void AddPointOfDanger(PlaceForCheck placeForCheck, bool realDanger)
	{
		if (!BotOwner_0.Memory.IsPeace)
		{
			return;
		}
		bool flag = false;
		this.realDanger = realDanger;
		if (place == null)
		{
			PanicType = PanicType.run;
			float sqrMagnitude = (placeForCheck.Position - BotOwner_0.Transform.position).sqrMagnitude;
			float num = BotOwner_0.Settings.Current.CurrentLayChance;
			if (sqrMagnitude > BotOwner_0.Settings.FileSettings.Lay.MIN_CAN_LAY_DIST_SQRT && sqrMagnitude < BotOwner_0.Settings.FileSettings.Lay.MAX_CAN_LAY_DIST_SQRT)
			{
				CustomNavigationPoint closestPoint = BotOwner_0.Covers.GetClosestPoint(BotOwner_0.Position);
				if (closestPoint == null)
				{
					return;
				}
				if ((closestPoint.Position - BotOwner_0.Position).sqrMagnitude < 225f)
				{
					num /= 2f;
				}
			}
			bool flag2 = BotOwner_0.Boss.IamBoss || BotOwner_0.BotFollower.HaveBoss;
			float num2 = GClass856.Random(BotOwner_0.Settings.FileSettings.Mind.DANGER_EXPIRE_TIME_MIN, BotOwner_0.Settings.FileSettings.Mind.DANGER_EXPIRE_TIME_MAX);
			ExpireTime = Time.time + num2;
			GClass1881<PanicType> gClass = null;
			if (realDanger)
			{
				gClass = new GClass1881<PanicType>(new KeyValuePair<PanicType, float>(PanicType.lay, num), new KeyValuePair<PanicType, float>(PanicType.run, BotOwner_0.Settings.FileSettings.Mind.PANIC_RUN_WEIGHT), new KeyValuePair<PanicType, float>(PanicType.sit, BotOwner_0.Settings.FileSettings.Mind.PANIC_SIT_WEIGHT));
				if (!flag2)
				{
					PanicType panicType = gClass.Random();
					switch (panicType)
					{
					case PanicType.run:
						flag = true;
						break;
					case PanicType.lay:
						BotOwner_0.BotLay.TryLay();
						break;
					}
					PanicType = panicType;
				}
			}
			else if (!flag2)
			{
				gClass = new GClass1881<PanicType>(new KeyValuePair<PanicType, float>(PanicType.sit, BotOwner_0.Settings.FileSettings.Mind.PANIC_SIT_WEIGHT_PEACE), new KeyValuePair<PanicType, float>(PanicType.none, BotOwner_0.Settings.FileSettings.Mind.PANIC_NONE_WEIGHT));
				PanicType = gClass.Random();
			}
			PanicPower = GClass856.Random(0f, 1f);
		}
		place = placeForCheck;
		if (flag && !BotOwner_0.Memory.IsInCover)
		{
			BotOwner_0.Memory.TryGetInCoverSay();
		}
	}

	public void CheckDangerPoints(float maxLifetime)
	{
		if (HaveDangePoints && place != null && Time.time - place.CreatedTime > maxLifetime)
		{
			SetAllDangerNull();
		}
	}

	public void method_0()
	{
		PanicType = PanicType.none;
		place = null;
	}
}
