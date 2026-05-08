using System;
using Newtonsoft.Json;

public class RagfairSettingsClass
{
	public class GClass1793
	{
		public float from;

		public float to;

		public int count;

		public int countForSpecialEditions;

		public GClass1793(float from, float to, int count, int countForSpecialEditions)
		{
			this.from = from;
			this.to = to;
			this.count = count;
			this.countForSpecialEditions = countForSpecialEditions;
		}
	}

	public bool enabled;

	public int minUserLevel;

	public float communityItemTax;

	public float communityRequirementTax;

	public float offerDurationTimeInHour;

	public float offerDurationTimeInHourAfterRemove;

	public int delaySinceOfferAdd;

	public double RagfairTurnOnTimestamp;

	public float renewCommunityTax;

	public float renewPricePerHour;

	public bool isOnlyFoundInRaidAllowed;

	public GClass1793[] maxActiveOfferCount;

	[JsonProperty("sellInOnePiece")]
	public int SellInOnePiece;

	[JsonProperty("ItemRestrictions")]
	public GClass1794[] ItemRestrictions = Array.Empty<GClass1794>();

	public TimeSpan DefaultDuration => TimeSpan.FromHours(offerDurationTimeInHour);
}
