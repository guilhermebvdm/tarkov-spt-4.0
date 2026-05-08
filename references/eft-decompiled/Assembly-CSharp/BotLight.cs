using System;
using System.Collections.Generic;
using System.Linq;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class BotLight : GClass429
{
	[NonSerialized]
	public const float DELAY = 3f;

	[NonSerialized]
	public bool HaveLight;

	[NonSerialized]
	public bool CanUseNow;

	[NonSerialized]
	public bool IsInDarkPlace_1;

	[NonSerialized]
	public float CurLightDist;

	[NonSerialized]
	public Player.FirearmController FirearmController;

	[NonSerialized]
	public float NextTimeUpdaDist;

	[NonSerialized]
	public LightComponent LightMod;

	[NonSerialized]
	public float NextCanToggleTime;

	[NonSerialized]
	public BotStroboscopeLight Stroboscope_1;

	public BotStroboscopeLight Stroboscope => Stroboscope_1;

	public bool IsEnable
	{
		get
		{
			if (LightMod == null)
			{
				HaveLight = false;
				return false;
			}
			return LightMod.IsActive;
		}
	}

	public Player.FirearmController FirearmController_0
	{
		get
		{
			if (FirearmController == null)
			{
				FirearmController = BotOwner_0.GetPlayer.HandsController as Player.FirearmController;
			}
			return FirearmController;
		}
	}

	public bool IsInDarkPlace
	{
		set
		{
			IsInDarkPlace_1 = value;
			if (IsInDarkPlace_1)
			{
				TurnOn(anyTime: true);
			}
			else
			{
				method_0();
			}
		}
	}

	public bool CanUseBySettings => BotOwner_0.Settings.FileSettings.Look.CAN_USE_LIGHT;

	public void UpdateStrope()
	{
		Stroboscope_1.ManualUpdate();
	}

	public BotLight(BotOwner owner)
		: base(owner)
	{
		Stroboscope_1 = new BotStroboscopeLight(owner, this);
	}

	public float UpdateLightEnable(float curLightDist)
	{
		if (BotOwner_0.FlashGrenade.IsFlashed)
		{
			return curLightDist;
		}
		if (!HaveLight)
		{
			return curLightDist;
		}
		CurLightDist = curLightDist;
		bool canUseNow;
		if ((canUseNow = BotOwner_0.Settings.FileSettings.Look.LightOnVisionDistance > curLightDist) != CanUseNow && !BotOwner_0.FlashGrenade.IsFlashed)
		{
			CanUseNow = canUseNow;
			if (IsEnable && !CanUseNow)
			{
				TurnOff();
			}
			if (CanUseNow && BotOwner_0.Memory.IsPeace)
			{
				TurnOn();
			}
		}
		return method_1(curLightDist);
	}

	public void FindLight()
	{
		HaveLight = false;
		if (BotOwner_0.WeaponManager.CurrentWeapon != null)
		{
			List<LightComponent> list = GClass3380.GetComponents<LightComponent>(BotOwner_0.WeaponManager.CurrentWeapon.Mods).ToList();
			HaveLight = list.Count > 0;
			if (HaveLight)
			{
				LightMod = list.First();
			}
		}
	}

	public void TurnOff(bool dependceOnEnemy = true, bool anyTime = false)
	{
		if (!IsInDarkPlace_1 && (BotOwner_0.Memory.GoalEnemy == null || !BotOwner_0.Memory.GoalEnemy.IsVisible || !dependceOnEnemy) && HaveLight && IsEnable && (NextCanToggleTime < Time.time || anyTime))
		{
			NextCanToggleTime = Time.time + 3f;
			if (FirearmController_0 != null)
			{
				FirearmController_0.SetLightsState(new FirearmLightStateStruct[1]
				{
					new FirearmLightStateStruct
					{
						Id = LightMod.Item.Id,
						IsActive = false,
						LightMode = LightMod.SelectedMode
					}
				});
			}
		}
	}

	public void TurnOn(bool anyTime = false)
	{
		if (CanUseBySettings && (anyTime || CanUseNow) && HaveLight && !IsEnable && (NextCanToggleTime < Time.time || anyTime))
		{
			NextCanToggleTime = Time.time + 3f;
			if (FirearmController_0 != null)
			{
				FirearmController_0.SetLightsState(new FirearmLightStateStruct[1]
				{
					new FirearmLightStateStruct
					{
						Id = LightMod.Item.Id,
						IsActive = true,
						LightMode = LightMod.SelectedMode
					}
				});
			}
		}
	}

	public void method_0()
	{
		if (IsEnable && BotOwner_0.Settings.FileSettings.Look.LightOnVisionDistance < CurLightDist)
		{
			TurnOff(dependceOnEnemy: false, anyTime: true);
		}
	}

	public float method_1(float botVisibleDist)
	{
		if (IsEnable)
		{
			return BotOwner_0.Settings.FileSettings.Look.VISIBLE_DISNACE_WITH_LIGHT;
		}
		return botVisibleDist;
	}
}
