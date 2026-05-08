using Koenigz.PerfectCulling.EFT;
using UnityEngine;

public abstract class GClass1236
{
	public static bool NotBatchedByWindowsManager(Renderer rend)
	{
		if (PerfectCullingMaterialSettings.Instance.IgnoreBatchedWindowsFilter)
		{
			return true;
		}
		return !WindowsManager.IsManagedByWindowsManager(rend);
	}
}
