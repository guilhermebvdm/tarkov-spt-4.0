using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

public class CrossSceneCullingTreePreProcess : PerfectCullingCrossSceneGroupPreProcess
{
	[Serializable]
	[CompilerGenerated]
	public class Class846
	{
		public static readonly Class846 class846_0 = new Class846();

		public static Comparison<PerfectCullingCrossSceneContentTrees> comparison_0;

		public int method_0(PerfectCullingCrossSceneContentTrees x, PerfectCullingCrossSceneContentTrees y)
		{
			if (x.ContentGroupId >= y.ContentGroupId)
			{
				return 1;
			}
			return -1;
		}
	}

	[Header("Content setup")]
	[SerializeField]
	private int _bakeHash;

	[SerializeField]
	private int _numBakeGroups;

	[SerializeField]
	private GuidReference[] _cullingGroups;

	public override int GetBakeHash()
	{
		return _bakeHash;
	}

	public override PerfectCullingBakeGroup[] PrepareRuntimeContent()
	{
		return GetBakeGroups();
	}

	public PerfectCullingBakeGroup[] GetBakeGroups()
	{
		List<PerfectCullingCrossSceneContentTrees> list = new List<PerfectCullingCrossSceneContentTrees>();
		GuidReference[] cullingGroups = _cullingGroups;
		foreach (GuidReference guidReference in cullingGroups)
		{
			list.Add(guidReference.gameObject.GetComponent<PerfectCullingCrossSceneContentTrees>());
		}
		list.Sort((PerfectCullingCrossSceneContentTrees x, PerfectCullingCrossSceneContentTrees y) => (x.ContentGroupId >= y.ContentGroupId) ? 1 : (-1));
		List<PerfectCullingBakeGroup> list2 = new List<PerfectCullingBakeGroup>();
		_numBakeGroups = 0;
		foreach (PerfectCullingCrossSceneContentTrees item in list)
		{
			list2.AddRange(item.BakeGroups);
			_numBakeGroups += item.BakeGroups.Length;
		}
		return list2.ToArray();
	}
}
