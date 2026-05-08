public class GClass789
{
	public ProfileStats Eft;

	public ProfileStats Arena;

	public GClass789(ProfileStatsClass descriptor)
	{
		if (descriptor.Eft != null)
		{
			Eft = new ProfileStats(descriptor.Eft);
		}
		if (descriptor.Arena != null)
		{
			Arena = new ProfileStats(descriptor.Arena);
		}
	}
}
