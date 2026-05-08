using EFT;
using Newtonsoft.Json;

[GAttribute29]
public class TraderInfoClass
{
	[JsonProperty("unlocked")]
	public bool Unlocked;

	[JsonProperty("loyaltyLevel")]
	public int LoyaltyLevel;

	[JsonProperty("salesSum")]
	public long SalesSum;

	[JsonProperty("standing")]
	public double Standing;

	[JsonProperty("nextResupply")]
	public int NextResupply;

	[JsonProperty("disabled")]
	public bool Disabled;

	public TraderInfoClass()
	{
	}

	public TraderInfoClass(Profile.TraderInfo traderInfo)
	{
		LoyaltyLevel = traderInfo.LoyaltyLevel;
		SalesSum = traderInfo.SalesSum;
		Standing = traderInfo.Standing;
		NextResupply = traderInfo.NextResupply;
		Unlocked = traderInfo.Unlocked;
		Disabled = traderInfo.Disabled;
	}
}
