using System;
using System.Runtime.CompilerServices;
using EFT.InventoryLogic;
using EFT.UI;
using UnityEngine;

namespace EFT.Hideout;

public class AreaRequirementPanel : AreaPanel, GInterface258, GInterface489, IDisposable
{
	[Serializable]
	[CompilerGenerated]
	public class Class2000
	{
		public static readonly Class2000 class2000_0 = new Class2000();

		public static Action<AreaPanel> action_0;

		public void method_0(AreaPanel arg)
		{
			HideoutController hideoutController = arg.Data.Template.AreaBehaviour?.Controller;
			if (hideoutController != null)
			{
				hideoutController.Select(arg.Data, wait: true);
			}
		}
	}

	[SerializeField]
	private Color _fulfilledColor;

	[SerializeField]
	private Color _failedColor;

	[SerializeField]
	private RequirementFulfilledStatus _fulfilledStatus;

	private AreaRequirement areaRequirement_0;

	public void Show(ItemUiContext itemUiContext, InventoryController inventoryController, Requirement requirement, EAreaType areaType, bool ignoreFulfillment)
	{
		areaRequirement_0 = (AreaRequirement)requirement;
		AreaData areaData = areaRequirement_0.AreaData;
		if (areaData == null)
		{
			Debug.LogError($"Type {areaRequirement_0.AreaType} has not been found");
			return;
		}
		if (areaData.Template.Type == areaRequirement_0.AreaType && areaData.CurrentLevel >= areaRequirement_0.RequiredLevel)
		{
			Close();
			return;
		}
		Init(areaData, delegate(AreaPanel arg)
		{
			HideoutController hideoutController = arg.Data.Template.AreaBehaviour?.Controller;
			if (hideoutController != null)
			{
				hideoutController.Select(arg.Data, wait: true);
			}
		});
		if (base.AreaIconCreated == null)
		{
			base.AreaIconCreated = UnityEngine.Object.Instantiate(base.AreaIcon as AreaRequirementIcon, base.Container);
		}
		((AreaRequirementIcon)base.AreaIconCreated).Show(base.Data, areaRequirement_0);
		UI.AddDisposable(delegate
		{
			base.AreaIconCreated.Close();
			UnityEngine.Object.Destroy(base.AreaIconCreated.gameObject);
			base.AreaIconCreated = null;
		});
	}

	public override void SetInfo()
	{
		if (!(this == null))
		{
			bool fulfilled = areaRequirement_0.Fulfilled;
			base.AreaName.text = base.Data.Template.Name;
			base.AreaName.color = (fulfilled ? _fulfilledColor : _failedColor);
			_fulfilledStatus.Show(fulfilled);
		}
	}

	public override void Close()
	{
		_fulfilledStatus.Close();
		base.Close();
	}

	[CompilerGenerated]
	public void method_3()
	{
		base.AreaIconCreated.Close();
		UnityEngine.Object.Destroy(base.AreaIconCreated.gameObject);
		base.AreaIconCreated = null;
	}
}
