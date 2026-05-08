using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

[DisallowMultipleComponent]
public class CrossSceneCullingGroupPreProcess : PerfectCullingCrossSceneGroupPreProcess
{
	[Serializable]
	[CompilerGenerated]
	public class Class845
	{
		public static readonly Class845 class845_0 = new Class845();

		public static Comparison<PerfectCullingCrossSceneContentMeshes> comparison_0;

		public int method_0(PerfectCullingCrossSceneContentMeshes x, PerfectCullingCrossSceneContentMeshes y)
		{
			if (x.ContentGroupId >= y.ContentGroupId)
			{
				return 1;
			}
			return -1;
		}
	}

	[SerializeField]
	private int _numBakeGroups;

	[SerializeField]
	private GuidReference[] _cullingGroups;

	[SerializeField]
	private int _bakeHash;

	public GuidReference[] CullingGroups => _cullingGroups;

	public bool Contains(GuidReference guid)
	{
		GuidReference[] cullingGroups = CullingGroups;
		int num = 0;
		while (true)
		{
			if (num < cullingGroups.Length)
			{
				if (cullingGroups[num] == guid)
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		return true;
	}

	public void method_0(GuidReference[] newCullingGroups)
	{
		_cullingGroups = newCullingGroups;
	}

	public override int GetBakeHash()
	{
		return _bakeHash;
	}

	public override void OnEndContentCollect()
	{
		method_1();
	}

	public override PerfectCullingBakeGroup[] CollectBakeGroups()
	{
		return Array.Empty<PerfectCullingBakeGroup>();
	}

	public override BakeBatch[] CreateBakeBatches(PerfectCullingBakeGroup[] inputGroups)
	{
		return Array.Empty<BakeBatch>();
	}

	public override BakeBatch[] GetBakeBatches()
	{
		BakeBatch bakeBatch = new BakeBatch();
		bakeBatch.Groups = GetBakeGroups();
		return new BakeBatch[1] { bakeBatch };
	}

	public override PerfectCullingBakeGroup[] PrepareRuntimeContent()
	{
		GuidReference[] cullingGroups = _cullingGroups;
		foreach (GuidReference obj in cullingGroups)
		{
			PerfectCullingCrossSceneGroup component = GetComponent<PerfectCullingCrossSceneGroup>();
			if (component == null)
			{
				GClass712.Throw("No culling group found");
			}
			obj.GetComponent<PerfectCullingCrossSceneContentMeshes>().RuntimeCullingGroup = component;
		}
		return GetBakeGroups();
	}

	public override LODGroup[] GetLODGroups()
	{
		List<LODGroup> list = new List<LODGroup>();
		GuidReference[] cullingGroups = _cullingGroups;
		foreach (GuidReference guidReference in cullingGroups)
		{
			list.AddRange(guidReference.gameObject.GetComponentsInChildren<LODGroup>());
		}
		return list.ToArray();
	}

	public void method_1()
	{
	}

	public PerfectCullingBakeGroup[] GetBakeGroups()
	{
		List<PerfectCullingCrossSceneContentMeshes> list = new List<PerfectCullingCrossSceneContentMeshes>();
		GuidReference[] cullingGroups = _cullingGroups;
		foreach (GuidReference guidReference in cullingGroups)
		{
			list.Add(guidReference.gameObject.GetComponent<PerfectCullingCrossSceneContentMeshes>());
		}
		list.Sort((PerfectCullingCrossSceneContentMeshes x, PerfectCullingCrossSceneContentMeshes y) => (x.ContentGroupId >= y.ContentGroupId) ? 1 : (-1));
		List<PerfectCullingBakeGroup> list2 = new List<PerfectCullingBakeGroup>();
		_numBakeGroups = 0;
		foreach (PerfectCullingCrossSceneContentMeshes item in list)
		{
			list2.AddRange(item.BakeGroups);
			_numBakeGroups += item.BakeGroups.Length;
		}
		return list2.ToArray();
	}

	public override SharedOccluder GenerateSharedOccluder()
	{
		return GClass1239.GenerateSharedOccluder(null, base.Group, GetBakeGroups());
	}
}
