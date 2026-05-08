using UnityEngine;

namespace EFT;

public class BeltDetachablePartSpawner : MonoBehaviour
{
	[SerializeField]
	private BeltDetachablePart _beltDetachablePart;

	[SerializeField]
	private Transform _positionForSpawn;

	public void SpawnBeltDetachablePart()
	{
		BeltDetachablePart beltDetachablePart = Object.Instantiate(_beltDetachablePart);
		beltDetachablePart.transform.position = _positionForSpawn.position;
		beltDetachablePart.transform.rotation = _positionForSpawn.rotation;
		beltDetachablePart.transform.localRotation *= Quaternion.Euler(90f, 0f, 0f);
		beltDetachablePart.transform.SetParent(_positionForSpawn, worldPositionStays: true);
		beltDetachablePart.transform.parent = null;
		beltDetachablePart.Spawn();
	}
}
