using EFT.InventoryLogic;

public class CultistAmuletComponent : GClass3379
{
	[GAttribute25]
	public int NumberOfUsages;

	public CultistAmuletComponent(Item item, CultistAmuletTemplateClass template)
		: base(item)
	{
		NumberOfUsages = template.MaxUsages;
	}
}
