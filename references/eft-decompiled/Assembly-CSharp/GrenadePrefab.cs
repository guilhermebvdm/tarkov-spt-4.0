using EFT.PrefabSettings;
using UnityEngine;

[DisallowMultipleComponent]
public class GrenadePrefab : WeaponPrefab
{
	[Header("Element 0 defines grenade initial position!")]
	public string[] ThrowingParts;

	public GrenadeSettings GrenadeItself;

	public TripwireVisual TripwireItself;

	public override void ReturnToPool()
	{
		Transform transform = TransformHelperClass.FindTransformRecursive(base.transform, ThrowingParts[0]);
		if (transform != null)
		{
			transform.gameObject.SetActive(value: true);
		}
		base.ReturnToPool();
	}
}
