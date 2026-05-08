using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT.Impostors;
using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

public class PerfectCullingTreePreProcess : PerfectCullingCrossSceneGroupPreProcess
{
	[Serializable]
	public class TreeGroup
	{
		public List<PerfectCullingBakeGroup> TreeBakeGroups = new List<PerfectCullingBakeGroup>();

		public void AddTree(PerfectCullingBakeGroup tree)
		{
			TreeBakeGroups.Add(tree);
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class848
	{
		public static readonly Class848 class848_0 = new Class848();

		public static Predicate<Renderer> predicate_0;

		public static Comparison<Renderer> comparison_0;

		public static Func<Transform, string> func_0;

		public bool method_0(Renderer x)
		{
			if (!(x == null) && x is MeshRenderer && x.enabled)
			{
				return !x.gameObject.activeInHierarchy;
			}
			return true;
		}

		public int method_1(Renderer x, Renderer y)
		{
			if (!(x.bounds.size.y < y.bounds.size.y))
			{
				return -1;
			}
			return 1;
		}

		public string method_2(Transform t)
		{
			return t.name;
		}
	}

	[SerializeField]
	private float m_BranchLength;

	[SerializeField]
	private float m_BranchSize;

	[SerializeField]
	private float m_CenterSize;

	[SerializeField]
	private float m_MergeRadius = 15f;

	[SerializeField]
	private int m_MaxCount;

	[HideInInspector]
	[SerializeField]
	private List<TreeGroup> _treeGroups = new List<TreeGroup>();

	public override PerfectCullingBakeGroup[] CollectBakeGroups()
	{
		_treeGroups.Clear();
		LODGroup[] componentsInChildren = base.Group.GetComponentsInChildren<LODGroup>();
		List<PerfectCullingBakeGroup> list = new List<PerfectCullingBakeGroup>();
		int num = 0;
		LODGroup[] array = componentsInChildren;
		foreach (LODGroup obj in array)
		{
			List<Renderer> list2 = smethod_0(obj);
			list2.RemoveAll((Renderer x) => x == null || !(x is MeshRenderer) || !x.enabled || !x.gameObject.activeInHierarchy);
			list2.Sort((Renderer x, Renderer renderer) => (x.bounds.size.y < renderer.bounds.size.y) ? 1 : (-1));
			Vector3 position = obj.transform.position;
			float y = list2[0].bounds.size.y;
			list.Add(new PerfectCullingBakeGroup
			{
				groupType = PerfectCullingBakeGroup.GroupType.Other,
				renderers = list2.ToArray(),
				lightBakePosition = position,
				lightBakeSize = y
			});
			num++;
			if (m_MaxCount > 0 && num == m_MaxCount)
			{
				break;
			}
		}
		if (m_MergeRadius > 0f)
		{
			HashSet<PerfectCullingBakeGroup> hashSet = new HashSet<PerfectCullingBakeGroup>();
			foreach (PerfectCullingBakeGroup item in list)
			{
				if (hashSet.Contains(item))
				{
					continue;
				}
				TreeGroup treeGroup = new TreeGroup();
				treeGroup.AddTree(item);
				hashSet.Add(item);
				foreach (PerfectCullingBakeGroup item2 in list)
				{
					if (item2 != item && !hashSet.Contains(item2) && !((item2.lightBakePosition - item.lightBakePosition).sqrMagnitude > m_MergeRadius * m_MergeRadius))
					{
						hashSet.Add(item2);
						treeGroup.AddTree(item2);
					}
				}
				_treeGroups.Add(treeGroup);
			}
			list.Clear();
			foreach (TreeGroup treeGroup2 in _treeGroups)
			{
				List<Renderer> list3 = new List<Renderer>();
				foreach (PerfectCullingBakeGroup treeBakeGroup in treeGroup2.TreeBakeGroups)
				{
					list3.AddRange(treeBakeGroup.renderers);
				}
				list.Add(new PerfectCullingBakeGroup
				{
					renderers = list3.ToArray(),
					groupType = PerfectCullingBakeGroup.GroupType.Other
				});
			}
		}
		return list.ToArray();
	}

	public static List<Renderer> smethod_0(LODGroup lodGroup)
	{
		List<Renderer> list = new List<Renderer>();
		LOD[] lODs = lodGroup.GetLODs();
		for (int i = 0; i < lODs.Length; i++)
		{
			Renderer[] renderers = lODs[i].renderers;
			if (renderers == null)
			{
				continue;
			}
			foreach (Renderer renderer in renderers)
			{
				if (renderer.gameObject.TryGetComponent<AmplifyImpostorsArrayElement>(out var _))
				{
					if (renderer.enabled)
					{
						renderer.enabled = false;
						string text = string.Join("/", (from t in renderer.gameObject.GetComponentsInParent<Transform>()
							select t.name).Reverse().ToArray());
						Debug.LogError("impostor renderer enabled: " + text, renderer.gameObject);
					}
				}
				else
				{
					list.Add(renderer);
				}
			}
		}
		return list;
	}

	public override PerfectCullingBakeGroup[] PrebakeTransform(BakeBatch batch, ICollection<GameObject> tempObjects, out GClass1221.Mode writeMode)
	{
		writeMode = GClass1221.Mode.Overwrite;
		int num = 0;
		if (m_MergeRadius > 0f)
		{
			List<PerfectCullingBakeGroup> list = new List<PerfectCullingBakeGroup>();
			foreach (TreeGroup treeGroup in _treeGroups)
			{
				List<Renderer> list2 = new List<Renderer>();
				foreach (PerfectCullingBakeGroup treeBakeGroup in treeGroup.TreeBakeGroups)
				{
					(MeshRenderer[], GameObject) tuple = GClass1241.Create(treeBakeGroup.lightBakeSize, m_CenterSize, m_BranchLength, m_BranchSize, 5);
					tuple.Item2.transform.position = treeBakeGroup.lightBakePosition;
					tempObjects?.Add(tuple.Item2);
					list2.AddRange(tuple.Item1);
				}
				list.Add(new PerfectCullingBakeGroup
				{
					renderers = list2.ToArray(),
					serializedGroupId = batch.Groups[num].serializedGroupId
				});
				num++;
			}
			return list.ToArray();
		}
		List<PerfectCullingBakeGroup> list3 = new List<PerfectCullingBakeGroup>();
		PerfectCullingBakeGroup[] groups = batch.Groups;
		foreach (PerfectCullingBakeGroup perfectCullingBakeGroup in groups)
		{
			(MeshRenderer[], GameObject) tuple2 = GClass1241.Create(perfectCullingBakeGroup.renderers[0].bounds.size.y, m_CenterSize, m_BranchLength, m_BranchSize, 5);
			tuple2.Item2.transform.position = perfectCullingBakeGroup.lightBakePosition;
			PerfectCullingBakeGroup obj = new PerfectCullingBakeGroup
			{
				groupType = PerfectCullingBakeGroup.GroupType.Other
			};
			Renderer[] item = tuple2.Item1;
			obj.renderers = item;
			obj.serializedGroupId = perfectCullingBakeGroup.serializedGroupId;
			list3.Add(obj);
			tempObjects?.Add(tuple2.Item2);
			num++;
		}
		return list3.ToArray();
	}

	[ContextMenu("Generate tree proxies")]
	public void method_0()
	{
		GameObject gameObject = new GameObject();
		gameObject.name = "TreeProxies";
		List<GameObject> list = new List<GameObject>();
		GClass1221.Mode writeMode;
		PerfectCullingBakeGroup[] array = PrebakeTransform(base.Group.bakeBatches[0], list, out writeMode);
		foreach (GameObject item in list)
		{
			item.transform.SetParent(gameObject.transform);
		}
		Color32[] colors = PerfectCullingResourcesLocator.Instance.ColorTable.Colors;
		int num = 0;
		PerfectCullingBakeGroup[] array2 = array;
		foreach (PerfectCullingBakeGroup obj in array2)
		{
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			materialPropertyBlock.SetColor("_Color", colors[num]);
			Renderer[] renderers = obj.renderers;
			for (int j = 0; j < renderers.Length; j++)
			{
				renderers[j].SetPropertyBlock(materialPropertyBlock);
			}
			num++;
		}
	}
}
