using UnityEngine;

public class GClass682
{
	public Vector3 position;

	public bool alreadyUsedToSpawn;

	public int CorePointId;

	public GClass682(Vector3 pos, int corePointId, bool isUsed)
	{
		position = pos;
		CorePointId = corePointId;
		alreadyUsedToSpawn = isUsed;
	}
}
