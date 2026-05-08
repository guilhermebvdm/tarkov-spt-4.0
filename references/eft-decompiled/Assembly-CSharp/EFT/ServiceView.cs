using EFT.InventoryLogic;
using EFT.UI;

namespace EFT;

public abstract class ServiceView : UIElement
{
	public abstract bool CheckAvailable(TraderClass trader);

	public abstract void Show(TraderClass trader, Profile profile, InventoryController controller, HealthControllerClass healthController, QuestBookClass quests, GClass1638 achievements, ItemUiContext context, ISession session);

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public ServiceView()
	{
	}
}
