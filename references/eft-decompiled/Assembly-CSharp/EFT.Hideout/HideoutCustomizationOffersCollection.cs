using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace EFT.Hideout;

[Serializable]
public class HideoutCustomizationOffersCollection
{
	[JsonProperty("globals")]
	public HideoutCustomizationGlobalOffer[] GlobalOffers;

	[JsonProperty("slots")]
	public HideoutCustomizationSlotOffer[] SlotsOffers;

	[field: NonSerialized]
	public List<HideoutCustomizationOffer> AllOffers { get; set; }

	[OnDeserialized]
	public void method_0(StreamingContext context)
	{
		AllOffers = new List<HideoutCustomizationOffer>();
		AllOffers.AddRange(GlobalOffers);
		AllOffers.AddRange(SlotsOffers);
	}
}
