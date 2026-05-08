using System.Collections.Generic;
using UnityEngine;

public class AIDangerPlacesHolder : MonoBehaviour
{
	[SerializeField]
	public List<AIDangerPlace> PlacesToAvoid;

	public bool AlwaysDraw;

	public void InGameRestore(AICoversData data)
	{
		if (PlacesToAvoid == null || PlacesToAvoid.Count == 0)
		{
			return;
		}
		foreach (AIDangerPlace item in PlacesToAvoid)
		{
			NavGraphVoxelSimple voxelSafe = data.GetVoxelSafe(item.Position);
			foreach (NavGraphVoxelSimple item2 in data.GetVoxelesExtended(voxelSafe.IndexX, voxelSafe.IndexY, voxelSafe.IndexZ, 1, onlyWithPoints: false))
			{
				item2.AddPlaceToAvoid(item);
			}
		}
	}

	public void OnDrawGizmosSelected()
	{
		if (!AlwaysDraw)
		{
			method_0();
		}
	}

	public void OnDrawGizmos()
	{
		if (AlwaysDraw)
		{
			method_0();
		}
	}

	public void method_0()
	{
		foreach (AIDangerPlace item in PlacesToAvoid)
		{
			Gizmos.color = Color.yellow;
			GClass850.DrawArrow(item.Position, Vector3.up * 3f);
			Gizmos.DrawWireSphere(item.Position, 0.3f);
		}
	}
}
