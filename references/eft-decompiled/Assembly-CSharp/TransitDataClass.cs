public class TransitDataClass
{
	public string raidId;

	public int count;

	public string[] maps;

	public bool events;

	public TransitDataClass(string raidId, int count, string[] maps, bool events)
	{
		this.raidId = raidId;
		this.count = count;
		this.maps = maps;
		this.events = events;
	}
}
