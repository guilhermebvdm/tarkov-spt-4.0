using UnityEngine;

namespace GPUInstancer;

[ExecuteInEditMode]
public class GPUInstancerTerrainProxy : MonoBehaviour
{
	public GPUInstancerDetailManager detailManager;

	public GPUInstancerDetailManager detailManagerOptic;

	public GPUInstancerTreeManager treeManager;

	public bool beingDestroyed;

	public int terrainSelectedToolIndex = -1;

	public void OnDestroy()
	{
		beingDestroyed = true;
	}
}
