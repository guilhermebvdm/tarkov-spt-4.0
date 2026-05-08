using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class CustomItem : MonoBehaviour
{
	[CompilerGenerated]
	public class Class565
	{
		public Slot currentSlot;

		public CustomItem customItem_0;

		public void method_0()
		{
			currentSlot.OnAddOrRemoveItem -= customItem_0.method_0;
		}
	}

	[SerializeField]
	private MeshFilter _meshFilter;

	[SerializeField]
	private CustomItemData _data;

	[SerializeField]
	private EClippingCustoms _itemType;

	private CompositeDisposableClass compositeDisposableClass;

	public static Dictionary<EClippingCustoms, EquipmentSlot[]> InterestSlots = new Dictionary<EClippingCustoms, EquipmentSlot[]>
	{
		{
			EClippingCustoms.Helmet,
			new EquipmentSlot[1] { EquipmentSlot.FaceCover }
		},
		{
			EClippingCustoms.Hat,
			new EquipmentSlot[3]
			{
				EquipmentSlot.FaceCover,
				EquipmentSlot.Earpiece,
				EquipmentSlot.Eyewear
			}
		},
		{
			EClippingCustoms.Visor,
			new EquipmentSlot[1] { EquipmentSlot.FaceCover }
		},
		{
			EClippingCustoms.FaceCover,
			new EquipmentSlot[2]
			{
				EquipmentSlot.Headwear,
				EquipmentSlot.Eyewear
			}
		},
		{
			EClippingCustoms.Headphones,
			new EquipmentSlot[1] { EquipmentSlot.Headwear }
		}
	};

	private PlayerBody playerBody_0;

	public void Init(PlayerBody playerBody)
	{
		playerBody_0 = playerBody;
		compositeDisposableClass?.Dispose();
		compositeDisposableClass = new CompositeDisposableClass();
		if (!InterestSlots.TryGetValue(_itemType, out var value))
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
			method_3(method_2(eClippingCustoms));
		}
	}

	public void OnDestroy()
	{
		method_1();
	}

	public void method_1()
	{
		compositeDisposableClass?.Dispose();
		compositeDisposableClass = null;
	}

	public void OnValidate()
	{
		if (_meshFilter == null)
		{
			_meshFilter = GetComponent<MeshFilter>();
		}
	}

	public bool method_2(EClippingCustoms customs)
	{
		return customs.HasFlag(_itemType);
	}

	public void method_3(bool isCustom)
	{
		if (!(_data.Base == null) && !(_data.Custom == null))
		{
			_meshFilter.sharedMesh = (isCustom ? _data.Custom : _data.Base);
		}
	}
}
