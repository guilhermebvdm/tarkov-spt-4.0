using System;
using Newtonsoft.Json;

namespace EFT.Hideout;

[Serializable]
public class HideoutCustomizationSlotOffer : HideoutCustomizationOffer
{
	[JsonProperty("slotId")]
	public string SlotId;

	[JsonProperty("areaTypeId")]
	public EAreaType AreaType;
}
