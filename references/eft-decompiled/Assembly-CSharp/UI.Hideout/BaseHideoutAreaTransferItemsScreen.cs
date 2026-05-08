using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DG.Tweening;
using EFT.Hideout;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.Screens;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Hideout;

public abstract class BaseHideoutAreaTransferItemsScreen<TController, TScreen> : EftScreen<TController, TScreen> where TController : BaseHideoutAreaTransferItemsScreen<TController, TScreen>.GClass3901 where TScreen : BaseHideoutAreaTransferItemsScreen<TController, TScreen>
{
	public class TransferScreenSwitcherTabController : GClass3799
	{
		[NonSerialized]
		public BaseHideoutAreaTransferItemsScreen<TController, TScreen> BaseHideoutAreaTransferItemsScreen_0;

		[NonSerialized]
		public AreaData AreaData_0;

		[NonSerialized]
		public CompoundItem CompoundItem_0;

		public TransferScreenSwitcherTabController(AreaData areaData, CompoundItem stash, BaseHideoutAreaTransferItemsScreen<TController, TScreen> screen)
		{
			BaseHideoutAreaTransferItemsScreen_0 = screen;
			AreaData_0 = areaData;
			CompoundItem_0 = stash;
		}

		public override void Show()
		{
			AreaData_0.SetSelectedStatus(isSelected: true);
			BaseHideoutAreaTransferItemsScreen_0.UpdateAreaStashView(AreaData_0, CompoundItem_0);
		}
	}

	public abstract class GClass3901 : GClass3900<TController, TScreen>
	{
		[NonSerialized]
		[CompilerGenerated]
		public AreaData AreaData_0;

		public readonly InventoryController InventoryController;

		public readonly ISession Session;

		public override EEftScreenType ScreenType => EEftScreenType.HideoutAreaItemsTransfer;

		public override EStateSwitcher EnvironmentOverlay => EStateSwitcher.Disabled;

		public AreaData AreaData
		{
			[CompilerGenerated]
			get
			{
				return AreaData_0;
			}
			[CompilerGenerated]
			set
			{
				AreaData_0 = value;
			}
		}

		public virtual bool ShowSortingTable => false;

		public GClass3901(AreaData areaData, InventoryController inventoryController, ISession session)
		{
			AreaData = areaData;
			InventoryController = inventoryController;
			Session = session;
		}

		public override async Task<bool> CloseScreenInterruption(bool moveForward)
		{
			bool flag;
			if (flag = Gparam_0 != null)
			{
				flag = !(await Gparam_0._stashPanel.TryClose());
			}
			if (flag)
			{
				return false;
			}
			return await base.CloseScreenInterruption(moveForward);
		}

		[CompilerGenerated]
		[DebuggerHidden]
		public Task<bool> method_11(bool moveForward)
		{
			return base.CloseScreenInterruption(moveForward);
		}
	}

	protected InventoryController _inventoryController;

	protected CompoundItem _playerStash;

	protected CompoundItem _areaStash;

	protected AreaData _mainAreaData;

	protected AreaData _selectedAreaData;

	protected ItemUiContext _itemUiContext;

	protected ItemContextAbstractClass _inventoryItemContext;

	protected ISession _backEndSession;

	[SerializeField]
	protected Toggle _uiHiddenToggle;

	[SerializeField]
	protected Tab[] _tabs;

	[SerializeField]
	protected Transform _tabsRoot;

	[SerializeField]
	protected CanvasGroup _areaStashCanvasGroup;

	[SerializeField]
	protected CanvasGroup _inventoryCanvasGroup;

	[SerializeField]
	protected CanvasGroup _handoverItemsWindowCanvasGroup;

	[SerializeField]
	protected SimpleStashPanel _stashPanel;

	[SerializeField]
	protected DefaultUIButton _closeButton;

	protected GClass3808 _tabGroup;

	protected List<AreaData> _linkedAreaDatas = new List<AreaData>();

	public virtual bool SetAsLastSiblingTabGroup => false;

	public virtual void Awake()
	{
		_closeButton.OnClick.AddListener(delegate
		{
			ScreenController.CloseScreen();
		});
		if (_uiHiddenToggle != null)
		{
			_uiHiddenToggle.onValueChanged.AddListener(method_3);
		}
	}

	public void method_3(bool hide)
	{
		_areaStashCanvasGroup.interactable = !hide;
		_inventoryCanvasGroup.interactable = !hide;
		_handoverItemsWindowCanvasGroup.interactable = !hide;
		_areaStashCanvasGroup.blocksRaycasts = !hide;
		_inventoryCanvasGroup.blocksRaycasts = !hide;
		_handoverItemsWindowCanvasGroup.blocksRaycasts = !hide;
		_areaStashCanvasGroup.DOFade((!hide) ? 1 : 0, 1f);
		_inventoryCanvasGroup.DOFade((!hide) ? 1 : 0, 1f);
		_handoverItemsWindowCanvasGroup.DOFade((!hide) ? 1 : 0, 1f);
	}

	public void Show(CompoundItem stashToShow, int selectedTabIndex = 0)
	{
		ShowGameObject();
		_stashPanel.Show(_playerStash, _inventoryController, _inventoryItemContext, inRaid: false, ScreenController.ShowSortingTable ? _inventoryController.Inventory.SortingTable : null, SimpleStashPanel.EStashSearchAvailability.Unavailable);
		UI.AddDisposable(_stashPanel);
		method_4();
		UpdateAreaStashView(_mainAreaData, stashToShow);
		UpdateTabGroup(selectedTabIndex);
		if (_uiHiddenToggle != null)
		{
			_uiHiddenToggle.isOn = false;
		}
	}

	public void method_4()
	{
		List<AreaData> list = new List<AreaData>();
		AreaData areaData = _mainAreaData.ParentAreaData ?? _mainAreaData;
		list.Add(areaData);
		list.AddRange(areaData.ChildAreasData);
		_linkedAreaDatas = list;
	}

	public void CreateTabGroupAndShow(Tab selectedTab)
	{
		_tabGroup = new GClass3808(_tabs, selectedTab, SetAsLastSiblingTabGroup);
		_tabGroup.Show(selectedTab, sendCallback: false);
		UI.AddDisposable(delegate
		{
			_tabGroup.Close();
		});
	}

	public virtual void UpdateTabGroup(int selectedTabIndex)
	{
		CreateTabGroupAndShow(_tabs[selectedTabIndex]);
	}

	public virtual void UpdateAreaStashView(AreaData areaData, CompoundItem stash)
	{
	}

	public BaseHideoutAreaTransferItemsScreen()
	{
	}

	[CompilerGenerated]
	public void method_5()
	{
		ScreenController.CloseScreen();
	}

	[CompilerGenerated]
	public void method_6()
	{
		_tabGroup.Close();
	}
}
