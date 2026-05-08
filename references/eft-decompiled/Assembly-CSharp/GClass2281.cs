using EFT;

public abstract class GClass2281
{
	public static bool IsNullOrEmpty(this ResourceKey key)
	{
		if (!(key == null))
		{
			return key.IsEmpty();
		}
		return true;
	}
}
