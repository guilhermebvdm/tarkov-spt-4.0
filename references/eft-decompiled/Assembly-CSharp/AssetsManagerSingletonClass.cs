using Comfort.Common;

public class AssetsManagerSingletonClass : Singleton<IAssetsManager>
{
	public static IAssetsManager Manager
	{
		get
		{
			return Singleton<IAssetsManager>.Instance;
		}
		set
		{
			Singleton<IAssetsManager>.Create(value);
		}
	}
}
