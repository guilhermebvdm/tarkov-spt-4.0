using EFT.Hideout;
using EFT.InventoryLogic;
using EFT.UI.Screens;

namespace UI.Hideout;

public class HideoutAreaTransferItemsScreen : AbstractHideoutAreaTransferItemsScreen<HideoutAreaTransferItemsScreen.GClass3902, HideoutAreaTransferItemsScreen>
{
	public class GClass3902 : GClass3901
	{
		public override EStateSwitcher MenuChatBarVisibility => EStateSwitcher.Enabled;

		public override EStateSwitcher TaskBarButtonsAvailability => EStateSwitcher.Enabled;

		public GClass3902(AreaData areaData, InventoryController inventoryController, ISession session)
			: base(areaData, inventoryController, session)
		{
		}

		public override void CloseAction(bool moveForward)
		{
			base.AreaData = Gparam_0._selectedAreaData;
			base.CloseAction(moveForward);
		}
	}

	public override void Show(GClass3902 controller)
	{
		Show(controller.AreaData, controller.InventoryController, canAddToFavorites: true, controller.Session);
	}
}
