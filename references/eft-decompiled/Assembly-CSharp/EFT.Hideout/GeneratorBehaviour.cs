using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using EFT.Communications;
using EFT.InventoryLogic;
using UnityEngine;

namespace EFT.Hideout;

public class GeneratorBehaviour : GClass2447, IHideoutConsumer, GInterface244, GInterface252, IDisposable, GInterface253
{
	[NonSerialized]
	public const int Int_0 = 5;

	public static readonly global::OnEventClass<ELightStatus> OnGeneratorInstalledHandler = new global::OnEventClass<ELightStatus>();

	[NonSerialized]
	[CompilerGenerated]
	public static bool Bool_0;

	[CompilerGenerated]
	private Action<Item, int> action_0;

	[CompilerGenerated]
	private Action<GInterface244, bool> action_1;

	[NonSerialized]
	public GClass2558 Gclass2558_0;

	[NonSerialized]
	public GClass2558 Gclass2558_1;

	[NonSerialized]
	public GClass2418 Gclass2418_0;

	[NonSerialized]
	public static GClass2448 Gclass2448_0;

	[NonSerialized]
	public bool Bool_1;

	public static bool IsGeneratorInstalled
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public GClass2418 ResourceConsumer => Gclass2418_0 ?? (Gclass2418_0 = new GClass2418());

	public static GClass2448 FuelDetails => Gclass2448_0 ?? (Gclass2448_0 = new GClass2448());

	public override bool HasError
	{
		get
		{
			if (base.Data.CurrentLevel != 0)
			{
				return !IsWorking;
			}
			return false;
		}
	}

	public bool Changeable => !base.Data.Template.TakeFromSlotLocked;

	public BarterItemItemClass[] UsingItems => ResourceConsumer.UsingItems;

	public float Consumption => ResourceConsumer.ResultConsumption;

	public float ConsumptionReduction => ResourceConsumer.ConsumptionReduction;

	public bool SwitchedOn => ResourceConsumer.IsOn;

	public bool IsWorking
	{
		get
		{
			return Bool_1;
		}
		set
		{
			if (Bool_1 != value)
			{
				Bool_1 = value;
				action_1?.Invoke(this, value);
				method_15();
			}
		}
	}

	public event Action<Item, int> OnConsumableItemChanged
	{
		[CompilerGenerated]
		add
		{
			Action<Item, int> action = action_0;
			Action<Item, int> action2;
			do
			{
				action2 = action;
				Action<Item, int> value2 = (Action<Item, int>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<Item, int> action = action_0;
			Action<Item, int> action2;
			do
			{
				action2 = action;
				Action<Item, int> value2 = (Action<Item, int>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<GInterface244, bool> OnWorkingStateChanged
	{
		[CompilerGenerated]
		add
		{
			Action<GInterface244, bool> action = action_1;
			Action<GInterface244, bool> action2;
			do
			{
				action2 = action;
				Action<GInterface244, bool> value2 = (Action<GInterface244, bool>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<GInterface244, bool> action = action_1;
			Action<GInterface244, bool> action2;
			do
			{
				action2 = action;
				Action<GInterface244, bool> value2 = (Action<GInterface244, bool>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public override void Init(AreaData area, GClass2204 profileInfo, InventoryController inventoryController, ISession session)
	{
		base.Init(area, profileInfo, inventoryController, session);
		method_13();
		BarterItemItemClass[] array = new BarterItemItemClass[base.Slots.NumberOfSlots];
		for (int i = 0; i < profileInfo.Slots.Length; i++)
		{
			GClass2426 gClass = profileInfo.Slots[i];
			if (gClass?.Items != null && gClass.Items.Length != 0)
			{
				Dictionary<string, Item> items = Singleton<ItemFactoryClass>.Instance.FlatItemsToTree(gClass.Items).Items;
				try
				{
					array[i] = items.FirstOrDefault().Value as FuelItemClass;
				}
				catch (IndexOutOfRangeException message)
				{
					Debug.LogError(message);
				}
			}
		}
		ResourceConsumer.StateChanged += method_11;
		ResourceConsumer.SetItems(array);
		IsWorking = ResourceConsumer.IsWorking;
		method_11();
		BonusController_0.OnBonusChanged += method_12;
	}

	public void method_11()
	{
		base.Data.SetProductionState(ResourceConsumer.IsWorking);
	}

	public override void Init(AreaData data)
	{
		base.Init(data);
		Gclass2558_0 = new GClass2559(EHideoutNotificationType.FuelIsLow, base.Data.Template, base.method_3);
		Gclass2558_1 = new GClass2559(EHideoutNotificationType.NoFuel, base.Data.Template, base.method_3);
		method_15();
	}

	public override void LevelUpdatedHandler()
	{
		base.LevelUpdatedHandler();
		FuelDetails.IsActive = base.Data.CurrentLevel > 0;
		method_14();
		method_13();
		if (base.Data.CurrentLevel > 0 && !IsGeneratorInstalled)
		{
			IsGeneratorInstalled = true;
			OnGeneratorInstalledHandler?.Invoke(SwitchedOn ? ELightStatus.Working : ELightStatus.OutOfFuel);
		}
	}

	public void method_12(bool isActive, EBonusType bonusType)
	{
		if (bonusType == EBonusType.FuelConsumption)
		{
			method_13();
		}
	}

	public void method_13()
	{
		HideoutControllerClass hideoutControllerClass = base.Data.CurrentStage.FuelSupply.Data?.FirstOrDefault();
		if (hideoutControllerClass == null)
		{
			HideoutClass.Class404.Instance.LogWarn("No consumption data for generator!");
			return;
		}
		ResourceConsumer.Consumption = (float)BonusController_0.Calculate(EBonusType.FuelConsumption, hideoutControllerClass.AmountPerSecond);
		HideoutClass.Class404.Instance.LogInfo("Consumption for generator: {0}", ResourceConsumer.Consumption);
		method_15();
	}

	public void method_14()
	{
		ResourceConsumer.UpdateSlotsData(base.Slots);
		method_15();
	}

	public void InstallConsumableItems(BarterItemItemClass[] installedSupplies)
	{
		if (UsingItems != null)
		{
			ResourceConsumer.SetItems(installedSupplies);
			PlaySound(EAreaActivityType.ItemInstall);
		}
	}

	public void Start(float profileDecayTime)
	{
		if (UsingItems == null || !ResourceConsumer.IsOn)
		{
			return;
		}
		float num = profileDecayTime * Consumption;
		BarterItemItemClass[] usingItems = UsingItems;
		for (int i = 0; i < usingItems.Length; i++)
		{
			ResourceComponent resourceComponent = usingItems[i]?.GetItemComponent<ResourceComponent>();
			if (resourceComponent != null && !(resourceComponent.Value < Mathf.Epsilon))
			{
				float num2 = Mathf.Min(num, resourceComponent.Value);
				resourceComponent.Value -= num2;
				num -= num2;
				if (num < Mathf.Epsilon)
				{
					break;
				}
			}
		}
	}

	public bool Work(float deltaTime)
	{
		IsWorking = ResourceConsumer.Work(deltaTime);
		if (IsWorking)
		{
			method_15();
		}
		return IsWorking;
	}

	public void SetSwitchedStatus(bool isOn)
	{
		ResourceConsumer.IsOn = isOn;
		method_15();
	}

	public void method_15()
	{
		GClass2448 fuelDetails = FuelDetails;
		if (!ResourceConsumer.IsOn)
		{
			fuelDetails.Type = EDetailsType.SwitchedOff;
		}
		else if (IsWorking)
		{
			fuelDetails.Type = EDetailsType.Fuel;
			if (ResourceConsumer.Consumption >= float.Epsilon)
			{
				int percentages = fuelDetails.Percentages;
				fuelDetails.Percentages = (int)Math.Ceiling(ResourceConsumer.TotalResourceCountLeft / ResourceConsumer.TotalResourceCount * 100.0);
				fuelDetails.Time = ResourceConsumer.Timer.TimeLeft;
				if (percentages >= 5 && fuelDetails.Percentages < 5)
				{
					Gclass2558_0.Show();
				}
				else if (percentages < 5 && fuelDetails.Percentages >= 5)
				{
					Gclass2558_0.Hide();
				}
				if (percentages > 0 && fuelDetails.Percentages == 0)
				{
					if (Gclass2558_0.Active)
					{
						Gclass2558_0.Hide();
					}
					Gclass2558_1.Show();
				}
				else if (percentages == 0 && fuelDetails.Percentages > 0)
				{
					Gclass2558_1.Hide();
				}
			}
			else
			{
				fuelDetails.Percentages = 100;
				fuelDetails.Time = 0;
			}
		}
		else
		{
			fuelDetails.Type = EDetailsType.NoFuel;
		}
	}

	public void Dispose()
	{
		BonusController_0.OnBonusChanged -= method_12;
		ResourceConsumer.StateChanged -= method_11;
	}
}
