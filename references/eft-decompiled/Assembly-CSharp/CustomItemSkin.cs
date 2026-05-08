using System.Runtime.CompilerServices;
using Diz.Skinning;
using EFT;
using EFT.InventoryLogic;
using EFT.Visual;
using UnityEngine;

public class CustomItemSkin : CustomSkin<CustomItemData>
{
	[CompilerGenerated]
	public class Class566
	{
		public Slot currentSlot;

		public CustomItemSkin customItemSkin_0;

		public void method_0()
		{
			currentSlot.OnAddOrRemoveItem -= customItemSkin_0.method_0;
		}
	}

	[SerializeField]
	private EClippingCustoms _itemType;

	private PlayerBody playerBody_0;

	private CompositeDisposableClass compositeDisposableClass;

	public void Init(Skeleton skeleton, PlayerBody playerBody)
	{
		playerBody_0 = playerBody;
		compositeDisposableClass?.Dispose();
		compositeDisposableClass = new CompositeDisposableClass();
		Skin.Init(skeleton);
		if (!CustomItem.InterestSlots.TryGetValue(_itemType, out var value))
		{
			Debug.LogError($"Unknown custom item type: {_itemType}");
			return;
		}
		EquipmentSlot[] array = value;
		foreach (EquipmentSlot equipmentSlot in array)
		{
			if (playerBody.SlotViews.GetByKey(equipmentSlot).ContainedItem != null)
			{
				Slot currentSlot = playerBody.Equipment.GetSlot(equipmentSlot);
				currentSlot.OnAddOrRemoveItem += method_0;
				compositeDisposableClass.AddDisposable(delegate
				{
					currentSlot.OnAddOrRemoveItem -= method_0;
				});
				compositeDisposableClass.AddDisposable(playerBody.SlotViews.GetByKey(equipmentSlot).ContainedItem.Bind(method_0));
			}
		}
	}

	public void method_0(Item item)
	{
		if (!(playerBody_0 == null))
		{
			EClippingCustoms eClippingCustoms = GClass922.CalculateRules(playerBody_0.Equipment);
			eClippingCustoms |= playerBody_0.CustomizationClipping;
			method_2(method_1(eClippingCustoms));
		}
	}

	public bool method_1(EClippingCustoms customs)
	{
		return customs.HasFlag(_itemType);
	}

	public void method_2(bool isCustom)
	{
		if (!(Data.Base == null) && !(Data.Custom == null))
		{
			Skin.SkinnedMeshRenderer.sharedMesh = (isCustom ? Data.Custom : Data.Base);
		}
	}

	public void OnDestroy()
	{
		compositeDisposableClass?.Dispose();
		compositeDisposableClass = null;
	}

	public override void Unskin()
	{
		Unsubscribe();
		base.Unskin();
	}
}
