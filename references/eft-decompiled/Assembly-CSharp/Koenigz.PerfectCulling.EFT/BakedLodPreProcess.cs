using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

public class BakedLodPreProcess : PerfectCullingCrossSceneGroupPreProcess
{
	[Serializable]
	[CompilerGenerated]
	public class Class844
	{
		public static readonly Class844 class844_0 = new Class844();

		public static Comparison<BakedLodContent> comparison_0;

		public int method_0(BakedLodContent x, BakedLodContent y)
		{
			if (x.ContentGroupId >= y.ContentGroupId)
			{
				return 1;
			}
			return -1;
		}
	}

	[SerializeField]
	private GuidReference[] _cullingGroups;

	[SerializeField]
	private int _bakeHash;

	public GuidReference[] CullingGroups => _cullingGroups;

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
		List<BakedLodContent> list = new List<BakedLodContent>();
		GuidReference[] cullingGroups = _cullingGroups;
		foreach (GuidReference guidReference in cullingGroups)
		{
			list.Add(guidReference.gameObject.GetComponent<BakedLodContent>());
		}
		list.Sort((BakedLodContent x, BakedLodContent y) => (x.ContentGroupId >= y.ContentGroupId) ? 1 : (-1));
		List<PerfectCullingBakeGroup> list2 = new List<PerfectCullingBakeGroup>();
		foreach (BakedLodContent item in list)
		{
			list2.AddRange(item.BakeGroups);
		}
		return list2.ToArray();
	}
}
