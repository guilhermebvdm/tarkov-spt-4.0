using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class BotGrenadeController : GClass429
{
	[Serializable]
	[CompilerGenerated]
	public class Class185
	{
		public static readonly Class185 class185_0 = new Class185();

		public static Func<Item, bool> func_0;

		public static Func<Item, bool> func_1;

		public bool method_0(Item x)
		{
			return x is ThrowWeapItemClass;
		}

		public bool method_1(Item x)
		{
			return x is ThrowWeapItemClass;
		}
	}

	public Vector3 LastThrowDirection;

	public float MaxThrowForce = 20f;

	public const float DEFAULT_GRENADE_WIEGHT = 0.5f;

	[NonSerialized]
	public const float WAIT_FOR_THROW_TIME = 0.4f;

	[NonSerialized]
	public ThrowWeapItemClass Grenade;

	[NonSerialized]
	public GrenadeActionType GrenadeActionType;

	[NonSerialized]
	public float ClearTime;

	[NonSerialized]
	public bool CheckStop;

	[NonSerialized]
	public float NextPosibleAttempt;

	[NonSerialized]
	public float LastThrowTime;

	[NonSerialized]
	public Vector3 PrecisionOffset_1;

	[NonSerialized]
	public bool InventoryUpdateDirty;

	[field: NonSerialized]
	public AIGreanageThrowData AIGreanageThrowData { get; set; }

	public Vector3 PrecisionOffset => PrecisionOffset_1;

	[field: NonSerialized]
	public bool ThrowindNow { get; set; }

	[field: NonSerialized]
	public float Mass { get; set; }

	public bool HaveGrenade => Grenade != null;

	public bool ReadyToThrow
	{
		get
		{
			if (AIGreanageThrowData != null && HaveGrenade)
			{
				return !AIGreanageThrowData.ThrowComplete;
			}
			return false;
		}
	}

	public Vector3 StartThrow => BotOwner_0.GetPlayer.WeaponRoot.position;

	public float MaxPower => BotOwner_0.Settings.FileSettings.Grenade.MAX_THROW_POWER / Mass;

	public bool NearLastThrow => Time.time - LastThrowTime < BotOwner_0.Settings.FileSettings.Grenade.NEAR_DELTA_THROW_TIME_SEC;

	public Vector3 ToThrowDirection => AIGreanageThrowData.Direction + PrecisionOffset_1;

	public event Action<ThrowWeapItemClass> OnGrenadeThrowComplete;

	public event Action OnGrenadeThrowStart;

	public BotGrenadeController(BotOwner owner)
		: base(owner)
	{
		BotOwner_0 = owner;
		LocalBotSettingsProviderClass.Core.G = Mathf.Abs(Physics.gravity.y);
		method_0();
	}

	public void Activate()
	{
		method_2();
	}

	public bool HaveGrenadeOfType(ThrowWeapType grenadeType)
	{
		foreach (Item item in from x in BotOwner_0.GetPlayer.InventoryController.Inventory.GetPlayerItems(EPlayerItems.Equipment)
			where x is ThrowWeapItemClass
			select x)
		{
			if ((item as ThrowWeapItemClass).ThrowType == grenadeType)
			{
				return true;
			}
		}
		return false;
	}

	public int GetGrenadeCount()
	{
		int num = 0;
		List<Item> list = BotOwner_0.GetPlayer.InventoryController.Inventory.GetPlayerItems(EPlayerItems.Equipment).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] is ThrowWeapItemClass)
			{
				num++;
			}
		}
		return num;
	}

	public bool CheckPeriodTime()
	{
		if (CheckStop && ClearTime > 0f && ClearTime < Time.time)
		{
			method_6();
			return true;
		}
		return false;
	}

	public bool DoThrow()
	{
		if (BotOwner_0.WeaponManager != null && BotOwner_0.WeaponManager.Selector.IsChanging)
		{
			return false;
		}
		if (BotOwner_0.WeaponManager != null && BotOwner_0.WeaponManager.Reload.Reloading)
		{
			return false;
		}
		if (BotOwner_0.Medecine.Using)
		{
			return false;
		}
		if (AIGreanageThrowData == null)
		{
			return false;
		}
		if (CheckPeriodTime())
		{
			return false;
		}
		if (!BotOwner_0.GetPlayer.HealthController.IsAlive)
		{
			return false;
		}
		if (BotOwner_0.Settings.FileSettings.Grenade.SHALL_GETUP)
		{
			BotOwner_0.BotLay.GetUp(withCheck: false);
		}
		BotOwner_0.Mover.SetPose(0f);
		if (BotOwner_0.Mover.HasPathAndNoComplete)
		{
			BotOwner_0.SetTargetMoveSpeed(0.2f);
		}
		else
		{
			BotOwner_0.SetTargetMoveSpeed(0f);
		}
		method_5();
		if (ThrowindNow)
		{
			return false;
		}
		switch (GrenadeActionType)
		{
		case GrenadeActionType.ready:
			CheckStop = true;
			ClearTime = Time.time + 5f;
			GrenadeActionType = GrenadeActionType.change2grenade;
			if (Grenade == null)
			{
				method_6();
				return false;
			}
			if (AIGreanageThrowData.GrenadeType.HasValue)
			{
				method_1(AIGreanageThrowData.GrenadeType.Value);
			}
			BotOwner_0.BotPersonalStats?.GrendateThrow(null);
			ThrowindNow = true;
			this.OnGrenadeThrowStart?.Invoke();
			BotOwner_0.BotTalk.Say(EPhraseTrigger.OnGrenade, sayImmediately: true);
			BotOwner_0.GetPlayer.SetInHands(Grenade, delegate(Result<IHandsThrowController> setInHandsResult)
			{
				ClearTime = Time.time + 5f;
				if (setInHandsResult.Value != null)
				{
					Player.GrenadeHandsController grenadeHandsController = BotOwner_0.GetPlayer.HandsController as Player.GrenadeHandsController;
					if (grenadeHandsController == null)
					{
						method_6();
					}
					else
					{
						GrenadeActionType = GrenadeActionType.grenadeReady;
						BotOwner_0.AITaskManager.RegisterDelayedTask(0.4f, delegate
						{
							Player.GrenadeHandsController grenadeHandsController2 = BotOwner_0.GetPlayer.HandsController as Player.GrenadeHandsController;
							if (grenadeHandsController2 == null)
							{
								method_6();
							}
							else
							{
								grenadeHandsController2.SetOnUsedCallback(delegate
								{
									method_6();
								});
								grenadeHandsController2.HandleFireInput();
								GrenadeActionType = GrenadeActionType.grenadeReady;
							}
						});
						grenadeHandsController.HandleFireInput();
					}
				}
				else
				{
					method_6();
				}
			});
			break;
		}
		return true;
	}

	public bool CanThrowGrenade(Vector3 trg)
	{
		return CanThrowGrenade(StartThrow, trg);
	}

	public bool CanThrowGrenade(Vector3 from, Vector3 trg)
	{
		if (!BotOwner_0.Settings.FileSettings.Grenade.CAN_LAY && BotOwner_0.BotLay.IsLay)
		{
			return false;
		}
		if (!BotOwner_0.BotsGroup.GroupGrenade.CanThrow())
		{
			return false;
		}
		if (BotOwner_0.AIData.PlaceInfo != null && BotOwner_0.AIData.PlaceInfo.BlockGrenade)
		{
			return false;
		}
		if (!BotOwner_0.Settings.FileSettings.Core.CanGrenade)
		{
			return false;
		}
		if (NextPosibleAttempt > Time.time)
		{
			return false;
		}
		method_7();
		if (!method_8(trg))
		{
			return false;
		}
		AIGreanageThrowData aIGreanageThrowData = GClass577.CanThrowGrenade2(greandeAng: BotOwner_0.Settings.FileSettings.Grenade.ANG_TYPE switch
		{
			2 => AIGreandeAng.ang25, 
			3 => AIGreandeAng.ang65, 
			4 => AIGreandeAng.ang15, 
			5 => AIGreandeAng.ang35, 
			6 => AIGreandeAng.ang55, 
			_ => AIGreandeAng.ang45, 
		}, from: from, trg: trg, maxPower: MaxPower, minThrowDistSqrt: BotOwner_0.Settings.FileSettings.Grenade.MIN_THROW_GRENADE_DIST_SQRT, maxPercent: BotOwner_0.Settings.FileSettings.Grenade.MIN_THROW_DIST_PERCENT_0_1);
		if (aIGreanageThrowData.CanThrow)
		{
			SetThrowData(aIGreanageThrowData);
			return true;
		}
		return false;
	}

	public bool SetThrowData(AIGreanageThrowData data)
	{
		AIGreanageThrowData = data;
		BotOwner_0.BotsGroup.GroupGrenade.ThrowGrenade(BotOwner_0);
		if (!data.CanThrow)
		{
			Debug.LogError("wrong throw data");
			return false;
		}
		return true;
	}

	public void UpdateCheck()
	{
		if (InventoryUpdateDirty)
		{
			InventoryUpdateDirty = false;
			method_2();
		}
	}

	public void SetDirty()
	{
		InventoryUpdateDirty = true;
	}

	public void NextThrowOffset(Vector3 nextOffset)
	{
		if (DebugBotData.UseDebugData && DebugBotData.Instance.NoGrenadeOffset)
		{
			PrecisionOffset_1 = Vector3.zero;
		}
		else
		{
			PrecisionOffset_1 = nextOffset;
		}
	}

	public void NextThrowNoOffset()
	{
		PrecisionOffset_1 = Vector3.zero;
	}

	public void method_0()
	{
		float grenadePrecision = BotOwner_0.Settings.FileSettings.Grenade.GrenadePrecision;
		if (DebugBotData.UseDebugData && DebugBotData.Instance.NoGrenadeOffset)
		{
			PrecisionOffset_1 = Vector3.zero;
		}
		else if (grenadePrecision > 0f)
		{
			float x = GClass856.Random(0f - grenadePrecision, grenadePrecision);
			float y = GClass856.Random(0f - grenadePrecision, grenadePrecision);
			float z = GClass856.Random(0f - grenadePrecision, grenadePrecision);
			PrecisionOffset_1 = new Vector3(x, y, z);
		}
		else
		{
			PrecisionOffset_1 = Vector3.zero;
		}
	}

	public void method_1(ThrowWeapType grenadeType)
	{
		foreach (Item item in from x in BotOwner_0.GetPlayer.InventoryController.Inventory.GetPlayerItems(EPlayerItems.Equipment)
			where x is ThrowWeapItemClass
			select x)
		{
			if (item is ThrowWeapItemClass throwWeapItemClass && throwWeapItemClass.ThrowType == grenadeType)
			{
				method_3(throwWeapItemClass);
				break;
			}
		}
	}

	public void method_2()
	{
		List<ThrowWeapItemClass> list = BotOwner_0.GetPlayer.InventoryController.Inventory.GetPlayerItems(EPlayerItems.Equipment).OfType<ThrowWeapItemClass>().ToList();
		foreach (ThrowWeapItemClass item in list)
		{
			if (item.ThrowType == ThrowWeapType.frag_grenade)
			{
				method_3(item);
				return;
			}
		}
		foreach (ThrowWeapItemClass item2 in list)
		{
			if (item2.ThrowType == ThrowWeapType.flash_grenade)
			{
				method_3(item2);
				return;
			}
		}
		if (list.Count > 0)
		{
			method_3(GClass856.RandomElement(list));
		}
		else
		{
			Grenade = null;
		}
	}

	public void method_3(ThrowWeapItemClass potentialGrenade)
	{
		Grenade = potentialGrenade;
		Mass = Grenade.TotalWeight;
		Mass = 0.5f;
	}

	public bool method_4()
	{
		if (AIGreanageThrowData != null)
		{
			if (AIGreanageThrowData.Force < 0.01f)
			{
				return true;
			}
			LastThrowDirection = AIGreanageThrowData.Direction + PrecisionOffset_1;
			BotOwner_0.Steering.LookToDirection(LastThrowDirection, 500f);
			LastThrowDirection.Normalize();
			Vector3 v = GClass855.NormalizeFastSelf(BotOwner_0.LookDirection);
			Vector3 lastThrowDirection = LastThrowDirection;
			float y = 0f;
			lastThrowDirection.y = 0f;
			v.y = y;
			v = GClass855.NormalizeFastSelf(v);
			lastThrowDirection = GClass855.NormalizeFastSelf(lastThrowDirection);
			if (Mathf.Abs(v.x - lastThrowDirection.x) < 0.1f && Mathf.Abs(v.z - lastThrowDirection.z) < 0.1f)
			{
				return true;
			}
		}
		return false;
	}

	public bool method_5()
	{
		return method_4();
	}

	public void method_6(ThrowWeapItemClass grenade = null)
	{
		if (BotOwner_0.BotState != EBotState.Active)
		{
			return;
		}
		ThrowindNow = false;
		method_0();
		ClearTime = 0f;
		CheckStop = false;
		GrenadeActionType = GrenadeActionType.ready;
		AIGreanageThrowData.ThrowComplete = true;
		method_2();
		method_7();
		if (!BotOwner_0.WeaponManager.Selector.TakePrevWeapon())
		{
			BotOwner_0.AITaskManager.RegisterDelayedTask(BotOwner_0, 2f, delegate
			{
				if (BotOwner_0.GetPlayer.HandsController is IHandsThrowController)
				{
					BotOwner_0.WeaponManager.Selector.TakePrevWeapon();
				}
			});
		}
		BotOwner_0.Steering.SetYAngle(0f);
		if (this.OnGrenadeThrowComplete != null && grenade != null)
		{
			this.OnGrenadeThrowComplete(grenade);
		}
		grenade = null;
	}

	public void method_7()
	{
		NextPosibleAttempt = BotOwner_0.Settings.FileSettings.Grenade.DELTA_NEXT_ATTEMPT + Time.time;
	}

	public bool method_8(Vector3 trg)
	{
		int num = 0;
		while (true)
		{
			if (num < BotOwner_0.BotsGroup.MembersCount)
			{
				if (!((BotOwner_0.BotsGroup.Member(num).Transform.position - trg).sqrMagnitude >= BotOwner_0.Settings.FileSettings.Grenade.MIN_DIST_NOT_TO_THROW_SQR))
				{
					break;
				}
				num++;
				continue;
			}
			return true;
		}
		return false;
	}

	public void Dispose()
	{
	}

	[CompilerGenerated]
	public void method_9(Result<IHandsThrowController> setInHandsResult)
	{
		ClearTime = Time.time + 5f;
		if (setInHandsResult.Value != null)
		{
			Player.GrenadeHandsController grenadeHandsController = BotOwner_0.GetPlayer.HandsController as Player.GrenadeHandsController;
			if (grenadeHandsController == null)
			{
				method_6();
				return;
			}
			GrenadeActionType = GrenadeActionType.grenadeReady;
			BotOwner_0.AITaskManager.RegisterDelayedTask(0.4f, delegate
			{
				Player.GrenadeHandsController grenadeHandsController2 = BotOwner_0.GetPlayer.HandsController as Player.GrenadeHandsController;
				if (grenadeHandsController2 == null)
				{
					method_6();
				}
				else
				{
					grenadeHandsController2.SetOnUsedCallback(delegate
					{
						method_6();
					});
					grenadeHandsController2.HandleFireInput();
					GrenadeActionType = GrenadeActionType.grenadeReady;
				}
			});
			grenadeHandsController.HandleFireInput();
		}
		else
		{
			method_6();
		}
	}

	[CompilerGenerated]
	public void method_10()
	{
		Player.GrenadeHandsController grenadeHandsController = BotOwner_0.GetPlayer.HandsController as Player.GrenadeHandsController;
		if (grenadeHandsController == null)
		{
			method_6();
			return;
		}
		grenadeHandsController.SetOnUsedCallback(delegate
		{
			method_6();
		});
		grenadeHandsController.HandleFireInput();
		GrenadeActionType = GrenadeActionType.grenadeReady;
	}

	[CompilerGenerated]
	public void method_11(Result<IHandsThrowController> throwResult)
	{
		method_6();
	}

	[CompilerGenerated]
	public void method_12()
	{
		if (BotOwner_0.GetPlayer.HandsController is IHandsThrowController)
		{
			BotOwner_0.WeaponManager.Selector.TakePrevWeapon();
		}
	}
}
