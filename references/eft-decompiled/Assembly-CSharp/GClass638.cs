public abstract class GClass638
{
	public static bool IsExportInProgress;

	public static bool IsBuilding => false;

	public static bool IsBuildingOrExporting
	{
		get
		{
			if (!IsBuilding)
			{
				return IsExportInProgress;
			}
			return true;
		}
	}
}
