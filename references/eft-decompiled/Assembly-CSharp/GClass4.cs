using EFT.Weather;

public class GClass4
{
	public static GInterface0 Instance;

	public static bool IsAvailable
	{
		get
		{
			if (!MonoBehaviourSingleton<TOD_Sky>.Instantiated)
			{
				return MonoBehaviourSingleton<TODSkySimple>.Instantiated;
			}
			return true;
		}
	}

	public static void RegisterInstance(GInterface0 instance)
	{
		Instance = instance;
	}

	public static void TryForceInitialize()
	{
		if (IsAvailable)
		{
			return;
		}
		TOD_Sky tOD_Sky = GClass870.FindUnityObjectOfType<TOD_Sky>();
		if (tOD_Sky != null)
		{
			tOD_Sky.Awake();
			return;
		}
		TODSkySimple tODSkySimple = GClass870.FindUnityObjectOfType<TODSkySimple>();
		if (tODSkySimple != null)
		{
			tODSkySimple.Awake();
		}
	}
}
