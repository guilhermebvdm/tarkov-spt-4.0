using System;
using Newtonsoft.Json;

namespace EFT;

[Serializable]
public class TraderData
{
	[JsonProperty("salesSum")]
	public long? SalesSum;

	[JsonProperty("standing")]
	public double? Standing;

	[JsonProperty("loyalty")]
	public float? LoyaltyLevel;

	[JsonProperty("unlocked")]
	public bool? Unlocked;

	[JsonProperty("disabled")]
	public bool? Disabled;

	public void Apply(Profile.TraderInfo traderInfo)
	{
		if (SalesSum.HasValue)
		{
			traderInfo.SetSalesSum(SalesSum.Value);
		}
		if (Standing.HasValue)
		{
			traderInfo.SetStanding(Standing.Value);
		}
		if (Unlocked.HasValue)
		{
			traderInfo.SetUnlocked(Unlocked.Value);
		}
		if (Disabled.HasValue)
		{
			traderInfo.SetDisabled(Disabled.Value);
		}
	}
}
