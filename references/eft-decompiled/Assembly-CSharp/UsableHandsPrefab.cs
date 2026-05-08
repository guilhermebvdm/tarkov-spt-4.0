using UnityEngine;

[DisallowMultipleComponent]
public class UsableHandsPrefab : WeaponPrefab
{
	[Header("Bone name for item")]
	public string UsableItemBoneName = "item_position";

	public Transform ItemSpawnTransform;

	public override void CacheInternalObjects()
	{
		base.CacheInternalObjects();
		ItemSpawnTransform = TransformHelperClass.FindTransformRecursive(base.transform, UsableItemBoneName);
	}
}
