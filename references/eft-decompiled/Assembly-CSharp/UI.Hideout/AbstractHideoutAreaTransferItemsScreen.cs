using System.Runtime.CompilerServices;
using EFT.Hideout;
using EFT.InputSystem;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using UnityEngine;

namespace UI.Hideout;

public abstract class AbstractHideoutAreaTransferItemsScreen<TController, TScreen> : BaseHideoutAreaTransferItemsScreen<TController, TScreen> where TController : BaseHideoutAreaTransferItemsScreen<TController, TScreen>.GClass3901 where TScreen : BaseHideoutAreaTransferItemsScreen<TController, TScreen>
{
	[CompilerGenerated]
	public class Class725
	{
		public TemplatedGridsView templatedGridsView;

		public AbstractHideoutAreaTransferItemsScreen<TController, TScreen> abstractHideoutAreaTransferItemsScreen_0;

		public void method_0()
		{
			if (templatedGridsView != null)
			{
				templatedGridsView.OnItemFilter -= abstractHideoutAreaTransferItemsScreen_0.method_7;
				templatedGridsView.OnAllItemFilter -= abstractHideoutAreaTransferItemsScreen_0.method_8;
			}
		}
	}

	[SerializeField]
	protected Transform _parent;

	[SerializeField]
	private HandoverHideoutAreaStashItemsWindow _handoverItemsWindow;

	protected ContainedGridsView _containedGridsView;

	private GClass3829 gclass3829_0;

	private bool bool_1;

	private GClass3129 gclass3129_0;

	public override bool SetAsLastSiblingTabGroup => false;

	public void Show(AreaData areaData, InventoryController controller, bool canAddToFavorites, ISession session)
	{
		_backEndSession = session;
		_mainAreaData = areaData;
		_inventoryController = controller;
		_playerStash = _inventoryController.Inventory.Stash;
		_itemUiContext = ItemUiContext.Instance;
		bool_1 = canAddToFavorites;
		_inventoryItemContext = new GClass3459(_playerStash, GClass3459.EItemType.Inventory, _inventoryController.Inventory.FavoriteItemsStorage, bool_1);
		Show(_inventoryController.Inventory.HideoutAreaStashes[_mainAreaData.Template.Type]);
	}

	public override void UpdateTabGroup(int selectedTabIndex)
	{
		_tabGroup?.Close();
		if (_linkedAreaDatas.Count <= 1)
		{
			Tab[] tabs = _tabs;
			for (int i = 0; i < tabs.Length; i++)
			{
				tabs[i].gameObject.SetActive(value: false);
			}
			return;
		}
		Tab selectedTab = null;
		for (int j = 0; j < _linkedAreaDatas.Count; j++)
		{
			Tab tab = _tabs[j];
			if (_containedGridsView is TemplatedGridsView templatedGridsView)
			{
				tab.transform.SetParent(templatedGridsView.SwitchZoneTabsPosition);
			}
			tab.Init(new TransferScreenSwitcherTabController(_linkedAreaDatas[j], _inventoryController.Inventory.HideoutAreaStashes[_linkedAreaDatas[j].Template.Type], this));
			tab.gameObject.SetActive(value: true);
			if (_linkedAreaDatas[j] == _selectedAreaData)
			{
				selectedTab = tab;
			}
		}
		CreateTabGroupAndShow(selectedTab);
	}

	public override void UpdateAreaStashView(AreaData areaData, CompoundItem stash)
	{
		method_9();
		_areaStash = stash;
		_selectedAreaData = areaData;
		_containedGridsView = ContainedGridsView.CreateGrids(_areaStash, null);
		_containedGridsView.transform.SetParent(_parent, worldPositionStays: false);
		GClass949.RectTransform(_containedGridsView).anchoredPosition = Vector2.zero;
		_containedGridsView.Show(_areaStash, new GClass3459(_areaStash, GClass3459.EItemType.AreaStash, _inventoryController.Inventory.FavoriteItemsStorage, bool_1), _inventoryController, null, _itemUiContext);
		ContainedGridsView containedGridsView = _containedGridsView;
		TemplatedGridsView templatedGridsView = containedGridsView as TemplatedGridsView;
		if ((object)templatedGridsView != null)
		{
			templatedGridsView.OnItemFilter += method_7;
			templatedGridsView.OnAllItemFilter += method_8;
			UI.AddDisposable(delegate
			{
				if (templatedGridsView != null)
				{
					templatedGridsView.OnItemFilter -= method_7;
					templatedGridsView.OnAllItemFilter -= method_8;
				}
			});
		}
		_itemUiContext.Configure(_inventoryController, _backEndSession.Profile, _backEndSession, _backEndSession.InsuranceCompany, null, null, new CompoundItem[1] { _playerStash }, EItemUiContextType.TransferItemsScreen, ECursorResult.ShowCursor, _areaStash, _inventoryController.Inventory.Equipment);
		UpdateTabGroup(0);
	}

	public void method_7(EFT.InventoryLogic.IContainer container, string localizationKey)
	{
		if (_handoverItemsWindow.isActiveAndEnabled && _handoverItemsWindow.Container == container)
		{
			gclass3829_0?.Close();
			return;
		}
		gclass3829_0?.Close();
		gclass3829_0 = _handoverItemsWindow.Show(container, _inventoryController, localizationKey);
	}

	public void method_8(string localizationKey)
	{
		gclass3129_0 = gclass3129_0 ?? new GClass3129(_areaStash, _areaStash.Containers);
		method_7(gclass3129_0, localizationKey);
	}

	public void method_9()
	{
		if (!(_containedGridsView == null))
		{
			Tab[] tabs = _tabs;
			for (int i = 0; i < tabs.Length; i++)
			{
				tabs[i].transform.SetParent(_tabsRoot);
			}
			_containedGridsView.Dispose();
			Object.Destroy(_containedGridsView.gameObject);
		}
	}

	public override ETranslateResult TranslateCommand(ECommand command)
	{
		if (command == ECommand.Escape)
		{
			if (_handoverItemsWindow != null && _handoverItemsWindow.isActiveAndEnabled)
			{
				gclass3829_0.Close();
				return ETranslateResult.Block;
			}
			ScreenController.CloseScreen();
			return ETranslateResult.Block;
		}
		return InputNode.GetDefaultBlockResult(command);
	}

	public override void Close()
	{
		ScreenController.Session.SetFavoriteItems(_inventoryController.Inventory.FavoriteItemsStorage);
		ScreenController.Session.FlushOperationQueue();
		gclass3829_0?.Close();
		method_9();
		gclass3129_0 = null;
		base.Close();
	}

	public AbstractHideoutAreaTransferItemsScreen()
	{
	}
}
