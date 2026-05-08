using Newtonsoft.Json;

[GAttribute29]
public class ProfileStatsClass
{
	[GAttribute30]
	[JsonProperty("Eft", NullValueHandling = NullValueHandling.Ignore)]
	public ProfileEftStatsClass Eft;

	[GAttribute30]
	[JsonProperty("Arena", NullValueHandling = NullValueHandling.Ignore)]
	public ProfileEftStatsClass Arena;

	public ProfileStatsClass()
	{
	}

	public ProfileStatsClass(GClass789 statsSeparator)
	{
		if (statsSeparator.Eft != null)
		{
			Eft = new ProfileEftStatsClass(statsSeparator.Eft);
		}
		if (statsSeparator.Arena != null)
		{
			Arena = new ProfileEftStatsClass(statsSeparator.Arena);
		}
	}
}
