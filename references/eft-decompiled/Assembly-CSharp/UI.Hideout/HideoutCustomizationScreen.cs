using System;
using EFT.Hideout;
using EFT.InputSystem;
using EFT.UI.Screens;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Hideout;

public class HideoutCustomizationScreen : UIScreen
{
	public class Class3063 : GClass3799
	{
		[NonSerialized]
		public HideoutCustomizationScreen HideoutCustomizationScreen_0;

		[NonSerialized]
		public EHideoutCustomizationType EhideoutCustomizationType_0;

		public Class3063(HideoutCustomizationScreen screen, EHideoutCustomizationType customizationType)
		{
			HideoutCustomizationScreen_0 = screen;
			EhideoutCustomizationType_0 = customizationType;
		}

		public override void Show()
		{
			HideoutCustomizationScreen_0.method_2(EhideoutCustomizationType_0);
		}
	}

	private const string string_0 = "Wall";

	[SerializeField]
	private Tab _wallButton;

	[SerializeField]
	private Tab _floorButton;

	[SerializeField]
	private Tab _ceilingButton;

	[SerializeField]
	private Tab _lightButton;

	[SerializeField]
	private Tab _shootingRangeTargetsButton;

	[SerializeField]
	private Tab _postersButton;

	[SerializeField]
	private Tab _itemsButton;

	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private HideoutCustomizationOptionsWithSlotsPanel _optionWithSlotsPanel;

	[SerializeField]
	private HideoutCustomizationSimpleOptionsPanel _simpleOptionPanel;

	private GClass2421 gclass2421_0;

	private HideoutCustomizationItemModelSpawnPoint hideoutCustomizationItemModelSpawnPoint_0;

	private GClass3808 gclass3808_0;

	private Action action_0;

	public void Init(GClass2421 customizationController)
	{
		gclass2421_0 = customizationController;
		_wallButton.Init(new Class3063(this, EHideoutCustomizationType.Wall));
		_floorButton.Init(new Class3063(this, EHideoutCustomizationType.Floor));
		_ceilingButton.Init(new Class3063(this, EHideoutCustomizationType.Ceiling));
		_shootingRangeTargetsButton.Init(new Class3063(this, EHideoutCustomizationType.ShootingRangeMark));
		_postersButton.Init(new Class3063(this, EHideoutCustomizationType.PosterSlot));
		_itemsButton.Init(new Class3063(this, EHideoutCustomizationType.StatuetteSlot));
	}

	public void Show(Action closeAction = null)
	{
		action_0 = closeAction;
		gclass2421_0.TestOffersConditions();
		gclass2421_0.LoadAllBundles(method_3).HandleExceptions();
		_closeButton.onClick.AddListener(Close);
		UI.AddDisposable(_closeButton.onClick.RemoveAllListeners);
		ShowGameObject();
		HideoutCustomizationItemModelSpawnPoint spawnPoint = gclass2421_0.GetSpawnPoint("Wall");
		if (spawnPoint != null)
		{
			method_5(spawnPoint);
		}
	}

	public void method_2(EHideoutCustomizationType customizationType)
	{
		_simpleOptionPanel.Close();
		_optionWithSlotsPanel.Close();
		switch (customizationType)
		{
		default:
			throw new ArgumentOutOfRangeException("customizationType", customizationType, null);
		case EHideoutCustomizationType.PosterSlot:
		case EHideoutCustomizationType.StatuetteSlot:
			_optionWithSlotsPanel.Show(customizationType, gclass2421_0, method_4);
			break;
		case EHideoutCustomizationType.Floor:
		case EHideoutCustomizationType.Wall:
		case EHideoutCustomizationType.Ceiling:
		case EHideoutCustomizationType.Light:
		case EHideoutCustomizationType.ShootingRangeMark:
			_simpleOptionPanel.Show(customizationType, gclass2421_0.GetSelectedCustomizationItemId(customizationType).Value, gclass2421_0.GetGlobalOffers(customizationType), gclass2421_0.Icons, gclass2421_0.AvailableCustomizations, method_4);
			break;
		}
	}

	public void method_3()
	{
		gclass3808_0 = new GClass3808(new Tab[6] { _wallButton, _floorButton, _ceilingButton, _shootingRangeTargetsButton, _postersButton, _itemsButton }, _wallButton, setAsLastSibling: false);
		gclass3808_0.Show(_wallButton);
	}

	public void method_4(HideoutCustomizationOffer offer)
	{
		string id = ((offer is HideoutCustomizationSlotOffer hideoutCustomizationSlotOffer) ? hideoutCustomizationSlotOffer.SlotId : offer.Type.ToString());
		HideoutCustomizationItemModelSpawnPoint spawnPoint = gclass2421_0.GetSpawnPoint(id);
		if (spawnPoint != null)
		{
			method_5(spawnPoint);
		}
		gclass2421_0.InstallCustomization(offer);
	}

	public override ETranslateResult TranslateCommand(ECommand command)
	{
		if (command == ECommand.Escape)
		{
			Close();
			return ETranslateResult.Block;
		}
		return InputNode.GetDefaultBlockResult(command);
	}

	public void method_5(HideoutCustomizationItemModelSpawnPoint customizationItemModelSpawnPoint)
	{
		if ((bool)hideoutCustomizationItemModelSpawnPoint_0)
		{
			hideoutCustomizationItemModelSpawnPoint_0.FocusCamera(focus: false);
		}
		hideoutCustomizationItemModelSpawnPoint_0 = (customizationItemModelSpawnPoint ? customizationItemModelSpawnPoint : null);
		if ((bool)hideoutCustomizationItemModelSpawnPoint_0)
		{
			hideoutCustomizationItemModelSpawnPoint_0.FocusCamera(focus: true);
		}
	}

	public override void Close()
	{
		action_0?.Invoke();
		_simpleOptionPanel.Close();
		_optionWithSlotsPanel.Close();
		gclass3808_0.Close();
		gclass2421_0.Clear().HandleExceptions();
		method_5(null);
		base.Close();
	}
}
