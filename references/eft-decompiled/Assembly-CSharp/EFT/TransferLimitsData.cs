using System;
using Newtonsoft.Json;

namespace EFT;

[Serializable]
public class TransferLimitsData
{
	[JsonProperty("nextResetTime")]
	public int nextResetTime;

	[JsonProperty("remainingLimit")]
	public int remainingLimit;

	[JsonProperty("totalLimit")]
	public int totalLimit;

	[JsonProperty("resetInterval")]
	public int resetInterval;
}
