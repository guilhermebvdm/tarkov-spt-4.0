namespace ombarella;

public static class PluginManager
{
	private static bool _isRaidLastFrame;

	public static void Update()
	{
		bool flag = Utils.IsInRaid();
		if (_isRaidLastFrame && !flag)
		{
			Plugin.Instance.CleanupRaid();
		}
		else if (!_isRaidLastFrame && flag)
		{
			Plugin.Instance.StartRaid();
		}
		_isRaidLastFrame = flag;
	}
}
