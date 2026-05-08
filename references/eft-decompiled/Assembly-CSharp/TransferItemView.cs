using System;
using System.Runtime.CompilerServices;
using Comfort.Common;
using Diz.Binding;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using UnityEngine;
using UnityEngine.EventSystems;

public class TransferItemView : GridItemView
{
	[Serializable]
	[CompilerGenerated]
	public class Class580
	{
		public static readonly Class580 class580_0 = new Class580();

		public static Func<float, bool, bool, float> func_0;

		public float method_0(float baseTransparent, bool isSelectedForTransfer, bool isTransferAllowed)
		{
			if (!isSelectedForTransfer && isTransferAllowed)
			{
				return baseTransparent;
			}
			return 0.25f;
		}
	}

	[CompilerGenerated]
	public class Class581
	{
		public TransferItemView transferItemView_0;

		public GClass3461 sourceContext;

		public void method_0(bool x)
		{
			transferItemView_0._stashSelectedImage.SetActive(x && sourceContext.IsPlayerStashView);
		}
	}

	[SerializeField]
	private GameObject _stashSelectedImage;

	[SerializeField]
	private GameObject _transferSelectedImage;

	private readonly global::BindableStateClass<bool> gclass1643_12 = new global::BindableStateClass<bool>();

	private readonly global::BindableStateClass<bool> gclass1643_13 = new global::BindableStateClass<bool>();

	private GClass3455 gclass3455_1;

	public override IBindable<float> Transparency => GClass1641.Combine(base.Transparency, gclass1643_12, gclass1643_13, (float baseTransparent, bool isSelectedForTransfer, bool isTransferAllowed) => (!isSelectedForTransfer && isTransferAllowed) ? baseTransparent : 0.25f);

	public static TransferItemView Create(Item item, GClass3461 sourceContext, ItemRotation rotation, TraderControllerClass inventoryController, IItemOwner itemOwner, FilterPanel filterPanel, IContainer container, ItemUiContext itemUiContext, InsuranceCompanyClass insurance)
	{
		return ItemViewFactory.CreateFromPool<TransferItemView>("arena_etf_transfer_layout").method_37(item, sourceContext, rotation, inventoryController, itemOwner, filterPanel, container, itemUiContext, insurance);
	}

	public TransferItemView method_37(Item item, GClass3461 sourceContext, ItemRotation rotation, TraderControllerClass itemController, IItemOwner itemOwner, FilterPanel filterPanel, IContainer container, ItemUiContext itemUiContext, InsuranceCompanyClass insurance)
	{
		gclass3455_1 = sourceContext.CreateSelectableChild(item);
		NewGridItemView(item, sourceContext, rotation, itemController, itemOwner, filterPanel, container, itemUiContext, insurance);
		if (!sourceContext.IsPlayerStashView)
		{
			gclass1643_12.Value = true;
			_transferSelectedImage.gameObject.SetActive(value: true);
			_stashSelectedImage.gameObject.SetActive(value: false);
		}
		Init();
		CompositeDisposable.BindState(gclass1643_12, delegate(bool x)
		{
			_stashSelectedImage.SetActive(x && sourceContext.IsPlayerStashView);
		});
		if (sourceContext.IsSelected && sourceContext.IsPlayerStashView)
		{
			gclass1643_12.Value = true;
		}
		return this;
	}

	public override ItemContextAbstractClass CreateNewItemContext(ItemContextAbstractClass sourceContext)
	{
		return gclass3455_1;
	}

	public override void ShowTooltip()
	{
		string text = (Examined ? Singleton<ItemFactoryClass>.Instance.BriefItemName(base.Item, GClass2348.Localized(base.Item.Name)) : GClass2348.Localized("Unknown item"));
		if (gclass1643_13.Value)
		{
			ItemUiContext.Tooltip.Show(text);
			return;
		}
		gclass3455_1.IsActive(out var tooltip);
		text = text + "\n" + GClass1673.SetColor(GClass2348.Localized(tooltip), Color.red);
		ItemUiContext.Tooltip.Show(text);
	}

	public void method_38(GEventArgs1 eventArgs)
	{
		if (eventArgs.Status == CommandStatus.Succeed && eventArgs.Item != base.Item && GClass3380.IsChildOf(eventArgs.Location, base.Item))
		{
			method_39();
		}
	}

	public override void OnItemAdded(GEventArgs2 eventArgs)
	{
		base.OnItemAdded(eventArgs);
		method_38(eventArgs);
	}

	public override void OnItemRemoved(GEventArgs3 eventArgs)
	{
		base.OnItemRemoved(eventArgs);
		method_38(eventArgs);
	}

	public override void UpdateStaticInfo()
	{
		base.UpdateStaticInfo();
		method_39();
	}

	public void method_39()
	{
		string tooltip;
		bool value = gclass3455_1.IsActive(out tooltip);
		gclass1643_13.Value = value;
	}

	public override void ChangeSelectedStatus(bool selected)
	{
		base.ChangeSelectedStatus(selected);
		gclass1643_12.Value = selected;
	}

	public override void OnClick(PointerEventData.InputButton button, Vector2 position, bool doubleClick)
	{
		switch (button)
		{
		case PointerEventData.InputButton.Left:
			if (base.ItemContext is GClass3455 gClass && gclass1643_13.Value)
			{
				gClass.ToggleSelection();
			}
			break;
		case PointerEventData.InputButton.Right:
			ShowContextMenu(position);
			break;
		case PointerEventData.InputButton.Middle:
			ExecuteMiddleClick();
			break;
		}
	}

	public override void Kill()
	{
		gclass1643_12.Value = false;
		_transferSelectedImage.gameObject.SetActive(value: false);
		_stashSelectedImage.gameObject.SetActive(value: false);
		base.Kill();
	}
}
