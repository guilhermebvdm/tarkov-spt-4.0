using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

[RequireComponent(typeof(GuidComponent))]
public class PerfectCullingCrossSceneGroup : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	public class Class853
	{
		public static readonly Class853 class853_0 = new Class853();

		public static Predicate<Renderer> predicate_0;

		public bool method_0(Renderer rend)
		{
			return !RendererFilter(rend);
		}
	}

	public static readonly List<PerfectCullingCrossSceneGroup> AllCrossGroups = new List<PerfectCullingCrossSceneGroup>();

	[SerializeField]
	[HideInInspector]
	public PerfectCullingBakeGroup[] bakeGroups = Array.Empty<PerfectCullingBakeGroup>();

	[SerializeField]
	[HideInInspector]
	public GameObject[] sharedOccluders = Array.Empty<GameObject>();

	[SerializeField]
	[HideInInspector]
	public GameObject[] sharedOccludeeOccluders = Array.Empty<GameObject>();

	[SerializeField]
	public SharedOccluder sharedOccluder;

	[SerializeField]
	public bool debugDrawVisibilityLines;

	[SerializeField]
	public bool lightsOnly;

	[SerializeField]
	public bool disableOnRuntime;

	[SerializeField]
	public BakeBatch[] bakeBatches = Array.Empty<BakeBatch>();

	[SerializeField]
	public bool useGroundSuperSampling;

	[SerializeField]
	public bool disableGroupOnPointSample;

	[SerializeField]
	public bool allowGroupCulling;

	[SerializeField]
	public Transform groupRoot;

	[SerializeField]
	private Bounds _groupBoundingBox;

	[SerializeField]
	public bool AllowAdaptiveGridMapping;

	private global::VisibilityIndicesClass<ushort> gclass1222_0;

	private global::VisibilityIndicesClass<ushort> gclass1222_1;

	private (int, int) valueTuple_0;

	private volatile int int_0;

	public volatile int counterMainThread;

	public volatile int counterWorkThread;

	public object lockUpdateVisibilityQueues = new object();

	public List<PerfectCullingCrossSceneVolume.GClass1254> _runtimeSharedVolumes = new List<PerfectCullingCrossSceneVolume.GClass1254>();

	public string runtimeGroupName;

	private List<Vector2Int> list_0 = new List<Vector2Int>();

	public Transform GroupRoot
	{
		get
		{
			if (!(groupRoot != null))
			{
				return base.transform;
			}
			return groupRoot;
		}
	}

	public Bounds GroupBoundingBox => _groupBoundingBox;

	public bool Boolean_0
	{
		get
		{
			global::VisibilityIndicesClass<ushort> visibilityIndicesClass = gclass1222_0;
			if (visibilityIndicesClass != null && visibilityIndicesClass.Count == 0)
			{
				global::VisibilityIndicesClass<ushort> visibilityIndicesClass2 = gclass1222_1;
				if (visibilityIndicesClass2 != null && visibilityIndicesClass2.Count == 0)
				{
					return counterMainThread == counterWorkThread;
				}
			}
			return false;
		}
	}

	public (int, int) SwitchStats => valueTuple_0;

	public LODGroup[] GetLODGroups()
	{
		PerfectCullingCrossSceneGroupPreProcess component = GetComponent<PerfectCullingCrossSceneGroupPreProcess>();
		if (component != null)
		{
			return component.GetLODGroups();
		}
		return GroupRoot.GetComponentsInChildren<LODGroup>();
	}

	public void method_0()
	{
		int num = 0;
		_groupBoundingBox = default(Bounds);
		if (bakeGroups != null && bakeGroups.Length != 0)
		{
			PerfectCullingBakeGroup[] array = bakeGroups;
			foreach (PerfectCullingBakeGroup perfectCullingBakeGroup in array)
			{
				if (perfectCullingBakeGroup != null)
				{
					(bool, Bounds) groupBounds = perfectCullingBakeGroup.GetGroupBounds();
					if (groupBounds.Item1)
					{
						_groupBoundingBox = groupBounds.Item2;
						break;
					}
					num++;
				}
			}
			array = bakeGroups;
			for (int i = 0; i < array.Length; i++)
			{
				(bool, Bounds) groupBounds2 = array[i].GetGroupBounds();
				if (groupBounds2.Item1)
				{
					_groupBoundingBox.Encapsulate(groupBounds2.Item2);
				}
				else
				{
					num++;
				}
			}
		}
		if (num > 0)
		{
			Debug.LogWarning("Failed computing group bounding box due to invalid renderers/meshes " + base.gameObject.name, base.gameObject);
		}
	}

	public float GetDistanceToNearestRendererSquared(Vector3 point)
	{
		float num = -1f;
		PerfectCullingBakeGroup[] array = bakeGroups;
		for (int i = 0; i < array.Length; i++)
		{
			Renderer[] renderers = array[i].renderers;
			foreach (Renderer renderer in renderers)
			{
				if (renderer != null)
				{
					float sqrMagnitude = (renderer.transform.position - point).sqrMagnitude;
					if (num < 0f || sqrMagnitude < num)
					{
						num = sqrMagnitude;
					}
				}
			}
		}
		return num;
	}

	public int GetBakeHash()
	{
		PerfectCullingCrossSceneGroupPreProcess component = GetComponent<PerfectCullingCrossSceneGroupPreProcess>();
		if (component != null)
		{
			return component.GetBakeHash();
		}
		return GetBakeHashDefault();
	}

	public int GetBakeHashDefault()
	{
		int num = 13;
		num = 221 + bakeGroups.Length;
		for (int i = 0; i < bakeGroups.Length; i++)
		{
			num = (int)(num * 53 + bakeGroups[i].groupType);
			if (bakeGroups[i].renderers != null)
			{
				num = num * 23 + bakeGroups[i].renderers.Length;
			}
		}
		return num + GClass1228.HashStringInt32(base.gameObject.scene.name + base.gameObject.name);
	}

	public static void RefreshBakeGroups(PerfectCullingCrossSceneGroup group)
	{
	}

	public static void RefreshSharedOccluders(PerfectCullingCrossSceneGroup group)
	{
	}

	public HashSet<Renderer> GetSharedOccluderRenderers()
	{
		if (sharedOccluder == null)
		{
			return new HashSet<Renderer>();
		}
		return new HashSet<Renderer>(sharedOccluder?.GetRenderers());
	}

	public static HashSet<Renderer> GetGroupRenderers(Transform groupRoot, bool applyTypeFiltering = true, bool includeInactiveRenderers = true)
	{
		HashSet<Renderer> hashSet = new HashSet<Renderer>();
		for (int i = 0; i < groupRoot.childCount; i++)
		{
			Transform child = groupRoot.GetChild(i);
			MultisceneSharedOccluder component = child.GetComponent<MultisceneSharedOccluder>();
			if (component == null || component.OccludeMode == EOccludeMode.SharedOccluder)
			{
				continue;
			}
			List<Renderer> list = new List<Renderer>();
			child.GetComponentsInChildren(includeInactiveRenderers, list);
			if (applyTypeFiltering)
			{
				list.RemoveAll((Renderer rend) => !RendererFilter(rend));
			}
			foreach (Renderer item in list)
			{
				hashSet.Add(item);
			}
		}
		return hashSet;
	}

	public static bool RendererFilter(Renderer renderer)
	{
		if (renderer == null)
		{
			return false;
		}
		if (renderer.enabled && renderer.gameObject.activeInHierarchy)
		{
			MeshRenderer meshRenderer = renderer as MeshRenderer;
			if (meshRenderer != null)
			{
				MeshFilter component = meshRenderer.GetComponent<MeshFilter>();
				if (component != null && component.sharedMesh == null)
				{
					return false;
				}
			}
			if (!GClass1224.SupportedRendererTypes.Contains(renderer.GetType()))
			{
				return false;
			}
			if (!GClass1228.ShouldProcessRenderer(renderer))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public void OnPostLevelLoaded()
	{
		_runtimeSharedVolumes.Sort(smethod_0);
	}

	public static int smethod_0(PerfectCullingCrossSceneVolume.GClass1254 a, PerfectCullingCrossSceneVolume.GClass1254 b)
	{
		if ((a.IsFineVolume && b.IsFineVolume) || (!a.IsFineVolume && !b.IsFineVolume))
		{
			return 0;
		}
		if (a.IsFineVolume && !b.IsFineVolume)
		{
			return -1;
		}
		if (!a.IsFineVolume && b.IsFineVolume)
		{
			return 1;
		}
		return 0;
	}

	public bool UpdateVisibleSetsMT(PerfectCullingCamera cam)
	{
		if (disableOnRuntime)
		{
			return true;
		}
		int_0++;
		bool flag = false;
		foreach (PerfectCullingCrossSceneVolume.GClass1254 runtimeSharedVolume in _runtimeSharedVolumes)
		{
			if (runtimeSharedVolume.UpdateVisibleRenderersAtPointMT(cam.ObservePosition, int_0) == PerfectCullingCrossSceneVolume.GClass1254.EUpdateResult.Ok)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return false;
		}
		gclass1222_0.Clear();
		gclass1222_1.Clear();
		PerfectCullingBakeGroup[] array = bakeGroups;
		foreach (PerfectCullingBakeGroup perfectCullingBakeGroup in array)
		{
			if (perfectCullingBakeGroup.updateCounter >= int_0)
			{
				if (!perfectCullingBakeGroup.isGroupEnabled)
				{
					gclass1222_0.Add(perfectCullingBakeGroup.runtimeGroupIndex);
				}
			}
			else if (perfectCullingBakeGroup.isGroupEnabled)
			{
				gclass1222_1.Add(perfectCullingBakeGroup.runtimeGroupIndex);
			}
		}
		valueTuple_0 = (gclass1222_0.Count, gclass1222_1.Count);
		return true;
	}

	public void UpdateSwitchQueuesMT()
	{
		if ((gclass1222_1.Count > 0 || gclass1222_0.Count > 0) && Monitor.TryEnter(lockUpdateVisibilityQueues))
		{
			method_1();
			Monitor.Exit(lockUpdateVisibilityQueues);
		}
	}

	public void method_1()
	{
		int b = PerfectCullingSettings.Instance.numActivationsPerVolume / 2;
		int num = Mathf.Min(gclass1222_1.Count, b);
		int num2 = Mathf.Min(gclass1222_0.Count, b);
		PerfectCullingBakeGroup[] array = bakeGroups;
		for (int i = 0; i < num; i++)
		{
			array[gclass1222_1.Dequeue()].IsEnabled = false;
		}
		for (int j = 0; j < num2; j++)
		{
			array[gclass1222_0.Dequeue()].IsEnabled = true;
		}
	}

	public void Awake()
	{
		method_5();
	}

	public void OnDestroy()
	{
		method_7();
	}

	public void Start()
	{
		method_6();
		runtimeGroupName = base.gameObject.name;
	}

	public void Update()
	{
		UpdateSwitchQueuesMT();
	}

	public void OnDrawGizmos()
	{
		if (Application.isPlaying && debugDrawVisibilityLines)
		{
			method_3();
		}
	}

	public void method_2()
	{
		Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
		Gizmos.DrawCube(GroupBoundingBox.center, GroupBoundingBox.size);
		Gizmos.color = Color.white;
		Gizmos.DrawWireCube(GroupBoundingBox.center, GroupBoundingBox.size);
	}

	public void method_3()
	{
		if (PerfectCullingCrossSceneSampler.Instance == null || PerfectCullingCrossSceneSampler.Instance.CullingCamera == null)
		{
			return;
		}
		Vector3 position = PerfectCullingCrossSceneSampler.Instance.CullingCamera.transform.position;
		Gizmos.color = Color.red;
		PerfectCullingBakeGroup[] array = bakeGroups;
		foreach (PerfectCullingBakeGroup perfectCullingBakeGroup in array)
		{
			IEnumerator<Vector3> enumerateCenters = perfectCullingBakeGroup.EnumerateCenters;
			while (enumerateCenters.MoveNext())
			{
				Gizmos.color = (perfectCullingBakeGroup.IsEnabled ? Color.green : Color.red);
				Gizmos.DrawLine(position, enumerateCenters.Current);
			}
		}
	}

	public void method_4()
	{
		PerfectCullingCrossSceneGroupPreProcess component = GetComponent<PerfectCullingCrossSceneGroupPreProcess>();
		if ((bool)component)
		{
			bakeGroups = component.PrepareRuntimeContent();
		}
	}

	public void method_5()
	{
		AllCrossGroups.Add(this);
		gclass1222_0 = new global::VisibilityIndicesClass<ushort>(65535);
		gclass1222_1 = new global::VisibilityIndicesClass<ushort>(65535);
	}

	public void method_6()
	{
		method_4();
		ushort num = 0;
		PerfectCullingBakeGroup[] array = bakeGroups;
		foreach (PerfectCullingBakeGroup obj in array)
		{
			obj.Init();
			obj.runtimeGroupIndex = num;
			num++;
		}
		runtimeGroupName = base.gameObject.name;
	}

	public void method_7()
	{
		AllCrossGroups.Remove(this);
		_runtimeSharedVolumes?.Clear();
		gclass1222_0?.Clear();
		gclass1222_1?.Clear();
		gclass1222_0 = null;
		gclass1222_1 = null;
	}

	public void CreateRuntimeProxies()
	{
		PerfectCullingBakeGroup[] array = bakeGroups;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].CreateRuntimeProxies();
		}
	}

	public void DeleteRuntimeProxies()
	{
		PerfectCullingBakeGroup[] array = bakeGroups;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].DeleteRuntimeProxies();
		}
	}

	public void method_8()
	{
		PerfectCullingBakeGroup[] array = bakeGroups;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].method_0();
		}
	}

	public void method_9(ushort[] indices, int newCounter)
	{
		PerfectCullingBakeGroup[] array = bakeGroups;
		int num = indices.Length;
		for (int i = 0; i < num; i++)
		{
			array[indices[i]].updateCounter = newCounter;
		}
	}

	public void method_10(global::VisibilityIndicesClass<ushort> visibilityIndices)
	{
		lock (lockUpdateVisibilityQueues)
		{
			int_0++;
			try
			{
				if (visibilityIndices != null && visibilityIndices.Count > 0)
				{
					int count = visibilityIndices.Count;
					ushort[] buffer = visibilityIndices.Buffer;
					for (int i = 0; i < count; i++)
					{
						bakeGroups[buffer[i]].updateCounter = int_0;
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(runtimeGroupName + " - " + ex.Message);
			}
			method_12();
		}
	}

	public void method_11(CullingGroupData data)
	{
		lock (lockUpdateVisibilityQueues)
		{
			int_0++;
			try
			{
				if (data != null && data.Indices != null)
				{
					method_9(data.Indices, int_0);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(runtimeGroupName + " - " + ex.Message);
			}
			method_12();
		}
	}

	public void method_12()
	{
		gclass1222_0.Clear();
		gclass1222_1.Clear();
		PerfectCullingBakeGroup[] array = bakeGroups;
		foreach (PerfectCullingBakeGroup perfectCullingBakeGroup in array)
		{
			bool flag = false;
			if (perfectCullingBakeGroup.updateCounter == int_0)
			{
				if (!perfectCullingBakeGroup.isGroupEnabled && !flag)
				{
					gclass1222_0.Add(perfectCullingBakeGroup.runtimeGroupIndex);
				}
			}
			else if (perfectCullingBakeGroup.isGroupEnabled)
			{
				gclass1222_1.Add(perfectCullingBakeGroup.runtimeGroupIndex);
			}
		}
		valueTuple_0 = (gclass1222_0.Count, gclass1222_1.Count);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AddHiddenIndex(Vector2Int v)
	{
		list_0.Add(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void RemoveHiddenIndex(Vector2Int v)
	{
		list_0.Remove(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool method_13(ushort index)
	{
		if (list_0.Count == 0)
		{
			return false;
		}
		foreach (Vector2Int item in list_0)
		{
			if (index >= item.x && index <= item.y)
			{
				return true;
			}
		}
		return false;
	}
}
