using Newtonsoft.Json;

namespace EFT;

[JsonConverter(typeof(GClass1866<ETraderServiceType>))]
public enum ETraderServiceType
{
	[JsonProperty("None")]
	None = -1,
	[JsonProperty("ExUsecLoyalty")]
	ExUsecLoyalty,
	[JsonProperty("ZryachiyAid")]
	ZryachiyAid,
	[JsonProperty("CultistsAid")]
	CultistsAid,
	[JsonProperty("BtrItemsDelivery")]
	BtrItemsDelivery,
	[JsonProperty("PlayerTaxi")]
	PlayerTaxi,
	[JsonProperty("BtrBotCover")]
	BtrBotCover,
	[JsonProperty("TransitItemsDelivery")]
	TransitItemsDelivery
}
