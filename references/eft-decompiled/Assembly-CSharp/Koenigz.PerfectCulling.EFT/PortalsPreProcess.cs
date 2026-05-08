using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

public class PortalsPreProcess : PerfectCullingCrossSceneGroupPreProcess
{
	[Serializable]
	[CompilerGenerated]
	public class Class849
	{
		public static readonly Class849 class849_0 = new Class849();

		public static Comparison<CrossSceneContentPortals> comparison_0;

		public int method_0(CrossSceneContentPortals x, CrossSceneContentPortals y)
		{
			if (x.ContentGroupId >= y.ContentGroupId)
			{
				return 1;
			}
			return -1;
		}
	}

	public static readonly LODGroup[] EMPTY_LOD_GROUPS = new LODGroup[0];

	[SerializeField]
	private int _numBakeGroups;

	[SerializeField]
	private GuidReference[] _cullingGroups;

	[SerializeField]
	private int _bakeHash;

	public GuidReference[] CullingGroups => _cullingGroups;

	public override LODGroup[] GetLODGroups()
	{
		return EMPTY_LOD_GROUPS;
	}

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
		List<CrossSceneContentPortals> list = new List<CrossSceneContentPortals>();
		GuidReference[] cullingGroups = _cullingGroups;
		foreach (GuidReference guidReference in cullingGroups)
		{
			list.Add(guidReference.gameObject.GetComponent<CrossSceneContentPortals>());
		}
		list.Sort((CrossSceneContentPortals x, CrossSceneContentPortals y) => (x.ContentGroupId >= y.ContentGroupId) ? 1 : (-1));
		List<PerfectCullingBakeGroup> list2 = new List<PerfectCullingBakeGroup>();
		_numBakeGroups = 0;
		foreach (CrossSceneContentPortals item in list)
		{
			list2.AddRange(item.BakeGroups);
			_numBakeGroups += item.BakeGroups.Length;
		}
		return list2.ToArray();
	}
}
