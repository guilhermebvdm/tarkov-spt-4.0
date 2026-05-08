using System;
using System.Collections.Generic;
using EFT.Hideout;
using EFT.InventoryLogic;
using EFT.UI;
using TMPro;
using UnityEngine;

namespace UI.Hideout;

public abstract class HideoutCustomizationOptionsPanel : UIElement
{
	public class Class3062 : GClass3799
	{
		[NonSerialized]
		public HideoutCustomizationOptionsPanel HideoutCustomizationOptionsPanel_0;

		[NonSerialized]
		public HideoutCustomizationCell HideoutCustomizationCell_0;

		public Class3062(HideoutCustomizationOptionsPanel screen, HideoutCustomizationCell selfCell)
		{
			HideoutCustomizationOptionsPanel_0 = screen;
			HideoutCustomizationCell_0 = selfCell;
		}

		public override void Show()
		{
			HideoutCustomizationOptionsPanel_0.OnCellSelected(HideoutCustomizationCell_0);
		}
	}

	private const int int_0 = 5;

	[SerializeField]
	private HideoutCustomizationCell _cellTemplate;

	[SerializeField]
	private GameObject _cellsRowContainerTemplate;

	[SerializeField]
	protected GameObject _allCellsContainer;

	[SerializeField]
	protected TMP_Text _header;

	protected HideoutCustomizationIcons _icons;

	private GClass3808 gclass3808_0;

	protected List<HideoutCustomizationCell> _cells = new List<HideoutCustomizationCell>();

	protected Action<HideoutCustomizationOffer> _slotClickHandler;

	private GClass2421 gclass2421_0;

	public GameObject CreateNewCellsRowIfNeeded(GameObject currentCellsRow)
	{
		if (currentCellsRow == null || currentCellsRow.transform.childCount >= 5)
		{
			currentCellsRow = UnityEngine.Object.Instantiate(_cellsRowContainerTemplate, _allCellsContainer.transform);
			currentCellsRow.SetActive(value: true);
		}
		return currentCellsRow;
	}

	public HideoutCustomizationCell CreateCell(Dictionary<HideoutCustomizationOffer, Slot> offerSlotAssociations, GameObject currentCellsRow, string spriteId, string areaName, bool isLocked, int? cellNumber = null)
	{
		Sprite sprite = _icons.GetSprite(spriteId);
		HideoutCustomizationCell hideoutCustomizationCell = UnityEngine.Object.Instantiate(_cellTemplate, currentCellsRow.transform);
		hideoutCustomizationCell.Init(new Class3062(this, hideoutCustomizationCell));
		hideoutCustomizationCell.Show(offerSlotAssociations, sprite, areaName, isLocked, cellNumber);
		_cells.Add(hideoutCustomizationCell);
		return hideoutCustomizationCell;
	}

	public virtual void OnCellSelected(HideoutCustomizationCell selectedCell)
	{
		foreach (HideoutCustomizationCell cell in _cells)
		{
			if (!(cell == selectedCell))
			{
				cell.SetShaded(shaded: true);
			}
		}
	}

	public void CreateTabGroup(Tab initialTab)
	{
		if (_cells.Count > 0)
		{
			Tab[] tabs = _cells.ToArray();
			gclass3808_0 = new GClass3808(tabs, initialTab, setAsLastSibling: false);
			gclass3808_0.Show(initialTab);
		}
	}

	public override void Close()
	{
		foreach (HideoutCustomizationCell cell in _cells)
		{
			cell?.Close();
		}
		GClass6.DestroyAllChildren(_allCellsContainer);
		base.Close();
	}

	public HideoutCustomizationOptionsPanel()
	{
	}
}
