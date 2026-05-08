using System;
using System.Collections.Generic;
using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

[RequireComponent(typeof(PerfectCullingCrossSceneGroup))]
[DisallowMultipleComponent]
public abstract class PerfectCullingCrossSceneGroupPreProcess : MonoBehaviour
{
	public enum RenderMode
	{
		TriangleFill,
		Wireframe
	}

	public PerfectCullingCrossSceneGroup Group => GetComponent<PerfectCullingCrossSceneGroup>();

	public virtual int GetBakeResolution()
	{
		return PerfectCullingSettings.Instance.bakeCameraResolutionWidth;
	}

	public virtual RenderMode GetGroupRenderMode()
	{
		return RenderMode.TriangleFill;
	}

	public virtual PerfectCullingBakeGroup[] CollectBakeGroups()
	{
		return Array.Empty<PerfectCullingBakeGroup>();
	}

	public virtual BakeBatch[] CreateBakeBatches(PerfectCullingBakeGroup[] inputGroups)
	{
		return new BakeBatch[1]
		{
			new BakeBatch
			{
				Groups = inputGroups
			}
		};
	}

	public virtual PerfectCullingBakeGroup[] PrebakeTransform(BakeBatch batch, ICollection<GameObject> tempObjects, out GClass1221.Mode writeMode)
	{
		writeMode = GClass1221.Mode.Overwrite;
		return batch.Groups;
	}

	public virtual int GetBakeHash()
	{
		return Group.GetBakeHashDefault();
	}

	public virtual BakeBatch[] GetBakeBatches()
	{
		return Group.bakeBatches;
	}

	public virtual void OnBeginContentCollect()
	{
	}

	public virtual void OnEndContentCollect()
	{
	}

	public virtual PerfectCullingBakeGroup[] PrepareRuntimeContent()
	{
		return Group.bakeGroups;
	}

	public virtual void RemapBatchResult(BakeBatch sourceBatch, GClass1221 task)
	{
	}

	public virtual SharedOccluder GenerateSharedOccluder()
	{
		return GClass1239.GenerateSharedOccluder(null, Group);
	}

	public virtual LODGroup[] GetLODGroups()
	{
		return Group.GroupRoot.GetComponentsInChildren<LODGroup>();
	}

	public PerfectCullingCrossSceneGroupPreProcess()
	{
	}
}
