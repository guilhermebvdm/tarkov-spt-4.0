namespace Koenigz.PerfectCulling.EFT;

public class PerfectCullingLightGroupPreProcess : PerfectCullingCrossSceneGroupPreProcess
{
	public override int GetBakeHash()
	{
		PerfectCullingBakeGroup[] bakeGroups = base.Group.bakeGroups;
		int num = 13;
		num = 221 + bakeGroups.Length;
		for (int i = 0; i < bakeGroups.Length; i++)
		{
			num = (int)(num * 53 + bakeGroups[i].groupType);
			if (bakeGroups[i].cullingLightObjects != null)
			{
				num = num * 23 + bakeGroups[i].cullingLightObjects.Length;
			}
		}
		return num + GClass1228.HashStringInt32(base.gameObject.scene.name + base.gameObject.name);
	}
}
