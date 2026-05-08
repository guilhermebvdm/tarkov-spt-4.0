using EFT.InventoryLogic;
using Newtonsoft.Json;

public class GClass2334
{
	[JsonProperty("Id")]
	public string Id;

	[JsonProperty("TemplateId")]
	public string TemplateId;

	[JsonProperty("Count")]
	public int Count;

	public GClass2334(Item item)
	{
		Id = item.Id;
		TemplateId = item.TemplateId;
		Count = item.StackObjectsCount;
	}
}
