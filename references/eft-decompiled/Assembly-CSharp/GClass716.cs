public class GClass716 : LoggerClass
{
	public static GClass716 Instance;

	static GClass716()
	{
		Instance = new GClass716();
	}

	public GClass716()
		: base("assetBundle", LoggerMode.Add)
	{
	}
}
