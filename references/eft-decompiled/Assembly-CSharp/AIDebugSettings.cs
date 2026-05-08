using UnityEngine;

public class AIDebugSettings : ScriptableObject
{
	private static AIDebugSettings aidebugSettings_0;

	public bool CoverPointDrawEnable;

	public Texture2D BearingPointIcon;

	public Texture2D SelectedBearingPointIcon;

	public static AIDebugSettings Instance
	{
		get
		{
			if (!aidebugSettings_0)
			{
				return aidebugSettings_0 = GClass861.Load<AIDebugSettings>("AIDebugSettings");
			}
			return aidebugSettings_0;
		}
	}
}
