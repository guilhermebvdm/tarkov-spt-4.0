using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Koenigz.PerfectCulling.SamplingProviders;
using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

public class PerfectCullingCrossSceneVolume : PerfectCullingBakingBehaviour, GClass879.GInterface33
{
	[Serializable]
	public class BakeDescriptor
	{
		public enum LoadMode
		{
			Memory,
			Stream,
			AdaptiveGrid
		}

		public GuidReference crossSceneGroup;

		public int bakeCellSizeType;

		public int mergeIterations = 1;

		public StreamingVolumeBakeDataIndex streamingBakeData;

		[NonSerialized]
		public bool debugSamplingPoints;

		public LoadMode loadMode;

		public Vector3 bakeCellSize => Vector3.one * BAKE_CELL_SIZES[bakeCellSizeType];

		public PerfectCullingCrossSceneGroup GetGroup()
		{
			if ((bool)crossSceneGroup.gameObject)
			{
				return crossSceneGroup.gameObject.GetComponent<PerfectCullingCrossSceneGroup>();
			}
			return null;
		}

		public List<Renderer> GetAdditionalOccluders()
		{
			PerfectCullingCrossSceneGroup perfectCullingCrossSceneGroup = GetGroup();
			if (perfectCullingCrossSceneGroup == null)
			{
				return null;
			}
			if (perfectCullingCrossSceneGroup.sharedOccluder != null)
			{
				return perfectCullingCrossSceneGroup.sharedOccluder.GetRenderers();
			}
			return new List<Renderer>();
		}

		public void ValidateDescriptor()
		{
			if (GetGroup() == null)
			{
				throw new Exception("Group not loaded " + crossSceneGroup.ObjectGuid.ToString());
			}
			if (!streamingBakeData.IsValid)
			{
				throw new Exception("Invalid streaming index " + crossSceneGroup.ObjectGuid.ToString());
			}
		}
	}

	public class GClass1254
	{
		public enum EUpdateResult
		{
			OutOfBounds,
			Ok,
			NoData
		}

		public PerfectCullingCrossSceneGroup group;

		public int groupIndex;

		public StreamingVolumeBakeDataIndex volumeBakeData;

		public Vector3[] visibilityOffsets;

		public Vector3 volumeSize;

		public Vector3 volumePosition;

		public Quaternion volumeRotation;

		public string volumeName;

		public Bounds runtimeBounds;

		public int lastCellIndex;

		public float reuseDistance2;

		public object lockSampleQueue;

		public object lockRemoveCells;

		public object lockReuse;

		public GClass1247 streamer;

		public ExcludeInnerVolumeSamplingProvider excludeInnerVolumeSamplingProvider;

		public Dictionary<int, global::VisibilityIndicesClass<ushort>> syncSamples;

		public string parentVolumeName;

		public string groupName;

		public Vector3 runtimeCellCount;

		public bool IsFineVolume => excludeInnerVolumeSamplingProvider == null;

		public GClass1254(BakeDescriptor desc, PerfectCullingCrossSceneVolume parentVolume)
		{
			group = desc.GetGroup();
			group._runtimeSharedVolumes.Add(this);
			groupIndex = PerfectCullingCrossSceneGroup.AllCrossGroups.IndexOf(group);
			runtimeBounds = parentVolume.volumeBakeBounds;
			excludeInnerVolumeSamplingProvider = parentVolume.GetComponent<ExcludeInnerVolumeSamplingProvider>();
			streamer = new GClass1247(desc.streamingBakeData, StreamingVolumeBakeDataIndex.GetStreamingDataFilePath(parentVolume, desc), desc.loadMode);
			volumeName = parentVolume.name;
			volumePosition = parentVolume.transform.position;
			volumeRotation = parentVolume.transform.rotation;
			volumeSize = parentVolume.volumeSize;
			volumeBakeData = desc.streamingBakeData;
			lastCellIndex = int.MaxValue;
			lockSampleQueue = new object();
			lockRemoveCells = new object();
			syncSamples = new Dictionary<int, global::VisibilityIndicesClass<ushort>>();
			visibilityOffsets = GClass1228.GenerateOffsets(1, volumeBakeData.cellSize).ToArray();
			float num = volumeBakeData.cellSize.x * 2f;
			reuseDistance2 = num * num;
			runtimeCellCount = GClass1225.CalculateCellCount(volumeSize, volumeBakeData.cellSize);
			parentVolumeName = parentVolume.gameObject.name;
			groupName = group.gameObject.name;
		}

		public Vector3 GetSamplingPositionAtMT(int index)
		{
			Vector3 cellSize = volumeBakeData.cellSize;
			Vector3 cellCount = volumeBakeData.cellCount;
			Vector3 vector = cellCount * 0.5f;
			GClass1225.UnflattenToXYZ(index, out var x, out var y, out var z, cellCount);
			Vector3 vector2 = cellSize / 2f;
			vector2.x += (float)x * cellSize.x;
			vector2.y += (float)y * cellSize.y;
			vector2.z += (float)z * cellSize.z;
			vector2.x -= vector.x * cellSize.x;
			vector2.y -= vector.y * cellSize.y;
			vector2.z -= vector.z * cellSize.z;
			return volumePosition + volumeRotation * vector2;
		}

		public void method_0(global::VisibilityIndicesClass<ushort> indices, int newCounter)
		{
			PerfectCullingBakeGroup[] bakeGroups = group.bakeGroups;
			int count = indices.Count;
			ushort[] buffer = indices.Buffer;
			if (bakeGroups.Length < indices.Count)
			{
				GClass712.Error($"Group num doesnt match index num, groups={group.bakeGroups.Length}, indices={count}");
				return;
			}
			for (int i = 0; i < count; i++)
			{
				bakeGroups[buffer[i]].updateCounter = newCounter;
			}
		}

		public EUpdateResult UpdateVisibleRenderersAtPointMT(Vector3 pos, int newCounter)
		{
			if (!runtimeBounds.Contains(pos))
			{
				return EUpdateResult.OutOfBounds;
			}
			if (lastCellIndex == int.MaxValue)
			{
				return EUpdateResult.OutOfBounds;
			}
			global::VisibilityIndicesClass<ushort> precomputedIndices = GetPrecomputedIndices(lastCellIndex);
			if (precomputedIndices == global::VisibilityIndicesClass<ushort>.Empty)
			{
				return EUpdateResult.Ok;
			}
			if (precomputedIndices != null)
			{
				method_0(precomputedIndices, newCounter);
				return EUpdateResult.Ok;
			}
			GClass712.Warning("Not enough streaming data " + parentVolumeName + " -- " + groupName);
			return EUpdateResult.NoData;
		}

		public global::VisibilityIndicesClass<ushort> GetPrecomputedIndices(int index)
		{
			if (Monitor.TryEnter(lockSampleQueue))
			{
				syncSamples.TryGetValue(index, out var value);
				Monitor.Exit(lockSampleQueue);
				return value;
			}
			return null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetIndexForWorldPosNoOrientationMT(Vector3 pos, out bool isOutOfBounds)
		{
			return GClass1225.GetIndexForWorldPosNoOrientation(pos, volumePosition, volumeSize, runtimeCellCount, volumeBakeData.cellSize, out isOutOfBounds);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetIndexForWorldPosMT(Vector3 pos, out bool isOutOfBounds)
		{
			return GetIndexForWorldPosMT(pos, runtimeCellCount, volumeBakeData.cellSize, out isOutOfBounds);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetIndexForWorldPosMT(Vector3 pos, Vector3 cellCount, Vector3 cellSize, out bool isOutOfBounds)
		{
			Quaternion gridBakeOrientation = ((volumeBakeData == null) ? volumeRotation : volumeBakeData.orientation);
			return GClass1225.GetIndexForWorldPos(pos, volumePosition, volumeRotation, volumeSize, gridBakeOrientation, cellCount, cellSize, out isOutOfBounds);
		}
	}

	[CompilerGenerated]
	public class Class860
	{
		public GuidReference groupId;

		public bool method_0(BakeDescriptor x)
		{
			return x.crossSceneGroup == groupId;
		}
	}

	[CompilerGenerated]
	public class Class861
	{
		public string groupId;

		public bool method_0(BakeDescriptor x)
		{
			return x.crossSceneGroup.ObjectGuidString == groupId;
		}
	}

	public static readonly float[] BAKE_CELL_SIZES = new float[5] { 0.25f, 0.5f, 1f, 2f, 3f };

	public static readonly string[] BAKE_CELL_DISPLAY_VALUES = new string[5] { "0.25m", "0.5m", "1m", "2m", "3m" };

	public static readonly string[] BAKE_CELL_DISPLAY_FILTER_VALUES = new string[6] { "Any", "0.25m", "0.5m", "1m", "2m", "3m" };

	public static readonly string[] LOAD_MODES = new string[3] { "Memory", "Stream", "Adaptive" };

	private static readonly Color color_0 = new Color(0f, 1f, 0f, 0.25f);

	public const int MAX_RUNTIME_VOLUMES = 4096;

	public static readonly List<GClass1254> AllRuntimeCrossGroupVolumes = new List<GClass1254>();

	private static readonly List<PerfectCullingCrossSceneVolume> list_0 = new List<PerfectCullingCrossSceneVolume>();

	private static List<int> list_1;

	private List<GClass1254> list_2 = new List<GClass1254>();

	[SerializeField]
	private string _volumeGuid;

	[SerializeField]
	public Vector3 volumeSize = Vector3.one * 2f;

	public Bounds runtimeVolumeBounds;

	[SerializeField]
	public List<BakeDescriptor> bakeDescriptors = new List<BakeDescriptor>();

	private static BakeDescriptor bakeDescriptor_0;

	public static IReadOnlyCollection<PerfectCullingCrossSceneVolume> AllCrossSceneVolumes => list_0;

	public bool IsFineVolume => GetComponent<ExcludeInnerVolumeSamplingProvider>() == null;

	public string VolumeGuid => _volumeGuid;

	[SerializeField]
	public Bounds volumeBakeBounds
	{
		get
		{
			return new Bounds(base.transform.position, volumeSize);
		}
		set
		{
			base.transform.position = value.center;
			volumeSize = new Vector3(Mathf.Max(1f, value.size.x), Mathf.Max(1f, value.size.y), Mathf.Max(1f, value.size.z));
		}
	}

	public Vector3 HandleSized
	{
		get
		{
			return volumeBakeBounds.size;
		}
		set
		{
			volumeBakeBounds = new Bounds(base.transform.position, value);
		}
	}

	public float MaxCellSize
	{
		get
		{
			float num = 0f;
			foreach (BakeDescriptor bakeDescriptor in bakeDescriptors)
			{
				if (BAKE_CELL_SIZES[bakeDescriptor.bakeCellSizeType] > num)
				{
					num = BAKE_CELL_SIZES[bakeDescriptor.bakeCellSizeType];
				}
			}
			return num;
		}
	}

	public static BakeDescriptor ActiveBakeDescriptor => bakeDescriptor_0;

	public override PerfectCullingBakeData BakeData => PerfectCullingResourcesLocator.Instance.tempVolumeBakeData;

	public override PerfectCullingBakeGroup[] bakeGroups
	{
		get
		{
			return bakeDescriptor_0.GetGroup().bakeGroups;
		}
		set
		{
			throw new InvalidOperationException();
		}
	}

	public override List<Renderer> additionalOccluders
	{
		get
		{
			return bakeDescriptor_0.GetAdditionalOccluders();
		}
		set
		{
			throw new InvalidOperationException();
		}
	}

	public PerfectCullingVolumeBakeData volumeBakeData => PerfectCullingResourcesLocator.Instance.tempVolumeBakeData;

	public int CellCount => GClass1225.CalculateNumberOfCells(volumeSize, bakeDescriptor_0.bakeCellSize);

	public BakeDescriptor GetBakeDescriptor(PerfectCullingCrossSceneGroup group)
	{
		return GetBakeDescriptor(new GuidReference(group.GetComponent<GuidComponent>()));
	}

	public BakeDescriptor GetBakeDescriptor(GuidReference groupId)
	{
		return bakeDescriptors.Find((BakeDescriptor x) => x.crossSceneGroup == groupId);
	}

	public IReadOnlyCollection<int> GetUniqueBakeCellSizes()
	{
		HashSet<int> hashSet = new HashSet<int>();
		foreach (BakeDescriptor bakeDescriptor in bakeDescriptors)
		{
			hashSet.Add(bakeDescriptor.bakeCellSizeType);
		}
		return hashSet;
	}

	public static Vector3 ToBakeCellSize(int type)
	{
		return Vector3.one * BAKE_CELL_SIZES[type];
	}

	public BakeDescriptor GetBakeDescriptor(string groupId)
	{
		return bakeDescriptors.Find((BakeDescriptor x) => x.crossSceneGroup.ObjectGuidString == groupId);
	}

	public BakeDescriptor AddBakeDescriptor()
	{
		BakeDescriptor bakeDescriptor = new BakeDescriptor();
		bakeDescriptors.Add(bakeDescriptor);
		return bakeDescriptor;
	}

	public bool HasGroup(PerfectCullingCrossSceneGroup group)
	{
		foreach (BakeDescriptor bakeDescriptor in bakeDescriptors)
		{
			if (bakeDescriptor.crossSceneGroup != null && bakeDescriptor.GetGroup() == group)
			{
				return true;
			}
		}
		return false;
	}

	public void FixVolumeSize()
	{
		Bounds bounds = volumeBakeBounds;
		bounds.size = GClass1228.Snap(bounds.size, MaxCellSize);
		volumeBakeBounds = bounds;
	}

	public void ScheduleBake(List<int> bakeDescriptors, bool forceBakeAllNow = true)
	{
		if (bakeDescriptors.Count == 0)
		{
			bakeDescriptor_0 = null;
			return;
		}
		FixVolumeSize();
		list_1 = new List<int>(bakeDescriptors);
		for (int i = 0; i < list_1.Count; i++)
		{
			GClass1216.ScheduleBake(new GClass1204
			{
				BakingBehaviour = this,
				AdditionalOccluders = null
			});
		}
		if (forceBakeAllNow)
		{
			GClass1216.BakeAllScheduled();
		}
	}

	public void RemoteForceBakeSingle(int bakeDescId)
	{
		list_1 = new List<int>();
		list_1.Add(bakeDescId);
	}

	public override void SetBakeData(PerfectCullingBakeData bakeData)
	{
		throw new InvalidOperationException("Not allowed");
	}

	public override void InitializeBake()
	{
		if (list_1.Count == 0)
		{
			throw new InvalidOperationException("Bake ids empty.");
		}
		int index = list_1[0];
		bakeDescriptor_0 = bakeDescriptors[index];
		list_1.RemoveAt(0);
		Debug.Log("Starting bake " + base.gameObject.name + " - " + bakeDescriptor_0.crossSceneGroup.gameObject.name);
	}

	public override bool PreBake()
	{
		volumeSize = new Vector3((int)volumeSize.x, (int)volumeSize.y, (int)volumeSize.z);
		Vector3 bakeCellSize = bakeDescriptor_0.bakeCellSize;
		Vector3 cellCount = new Vector3(volumeSize.x / bakeCellSize.x, volumeSize.y / bakeCellSize.y, volumeSize.z / bakeCellSize.z);
		volumeBakeData.volumeGuid = _volumeGuid;
		volumeBakeData.SetVolumeBakeData(bakeCellSize, cellCount);
		if ((int)volumeBakeData.cellCount.x != 0 && (int)volumeBakeData.cellCount.y != 0 && (int)volumeBakeData.cellCount.z != 0)
		{
			return true;
		}
		return false;
	}

	public override void PostBake()
	{
		int mergeIterations = bakeDescriptor_0.mergeIterations;
		for (int i = 0; i < mergeIterations; i++)
		{
			volumeBakeData.MergeDownsample();
		}
		volumeBakeData.groupGuid = bakeDescriptor_0.crossSceneGroup.ObjectGuid.ToString("N");
	}

	public List<Vector3> method_0(Vector3 overrideBakeCellSize, Space space = Space.World, int maxPoints = 0)
	{
		InitializeAllSamplingProviders();
		int num = GClass1225.CalculateNumberOfCells(volumeSize, overrideBakeCellSize);
		int num2 = ((maxPoints > 0) ? maxPoints : num);
		List<Vector3> list = new List<Vector3>();
		int num3 = 0;
		for (int i = 0; i < num; i++)
		{
			if (num3 >= num2)
			{
				break;
			}
			Vector3 samplingPositionAt = GetSamplingPositionAt(i, overrideBakeCellSize, space);
			if (SamplingProvidersIsPositionActive(samplingPositionAt))
			{
				list.Add(samplingPositionAt);
				num3++;
			}
		}
		return list;
	}

	public override List<Vector3> GetSamplingPositions(Space space = Space.Self)
	{
		List<Vector3> list = new List<Vector3>(CellCount);
		for (int i = 0; i < CellCount; i++)
		{
			list.Add(GetSamplingPositionAt(i, bakeDescriptor_0.bakeCellSize, space));
		}
		return list;
	}

	public IEnumerable<Vector3?> GetSamplingPositionsEnum(Vector3 cellSize, Space space = Space.Self)
	{
		int num = GClass1225.CalculateNumberOfCells(volumeSize, cellSize);
		int num2 = 0;
		while (num2 < num)
		{
			yield return GetSamplingPositionAt(num2, cellSize, space);
			int num3 = num2 + 1;
			num2 = num3;
		}
		yield return null;
	}

	public List<Vector3> GetSamplingPositions(Vector3 cellSize, Space space = Space.Self)
	{
		int num = GClass1225.CalculateNumberOfCells(volumeSize, cellSize);
		List<Vector3> list = new List<Vector3>(num);
		for (int i = 0; i < num; i++)
		{
			list.Add(GetSamplingPositionAt(i, cellSize, space));
		}
		return list;
	}

	public override int GetIndexForWorldPos(Vector3 worldPos, out bool isOutOfBounds)
	{
		return GClass1225.GetIndexForWorldPos(worldPos, base.transform.position, base.transform.rotation, volumeSize, base.transform.rotation, GClass1225.CalculateCellCount(volumeSize, bakeDescriptor_0.bakeCellSize), bakeDescriptor_0.bakeCellSize, out isOutOfBounds);
	}

	public override Vector3 GetWorldPositionForIndex(int index)
	{
		return GetSamplingPositionAt(index, bakeDescriptor_0.bakeCellSize, Space.World);
	}

	public Vector3 GetSamplingPositionAt(int index, Vector3 cellSize, Space space = Space.Self)
	{
		Vector3 gridSize = new Vector3(volumeSize.x / cellSize.x, volumeSize.y / cellSize.y, volumeSize.z / cellSize.z);
		return GetSamplingPositionAt(index, gridSize, cellSize, space);
	}

	public Vector3 GetSamplingPositionAt(int index, Vector3 gridSize, Vector3 cellSize, Space space = Space.Self)
	{
		Vector3 vector = gridSize * 0.5f;
		GClass1225.UnflattenToXYZ(index, out var x, out var y, out var z, gridSize);
		Vector3 vector2 = cellSize / 2f + new Vector3((float)x * cellSize.x, (float)y * cellSize.y, (float)z * cellSize.z) - new Vector3(vector.x * cellSize.x, vector.y * cellSize.y, vector.z * cellSize.z);
		if (space == Space.World)
		{
			return base.transform.position + base.transform.rotation * vector2;
		}
		return vector2;
	}

	public static Vector3 GetSamplingPositionAtMT(int index, Vector3 cellSize, Vector3 volumeSize, Vector3 volumePosition, Quaternion volumeRotation, Space space = Space.Self)
	{
		Vector3 vector = new Vector3(volumeSize.x / cellSize.x, volumeSize.y / cellSize.y, volumeSize.z / cellSize.z);
		Vector3 vector2 = vector * 0.5f;
		GClass1225.UnflattenToXYZ(index, out var x, out var y, out var z, vector);
		Vector3 vector3 = cellSize / 2f + new Vector3((float)x * cellSize.x, (float)y * cellSize.y, (float)z * cellSize.z) - new Vector3(vector2.x * cellSize.x, vector2.y * cellSize.y, vector2.z * cellSize.z);
		if (space == Space.World)
		{
			return volumePosition + volumeRotation * vector3;
		}
		return vector3;
	}

	public override int GetBakeHash()
	{
		return bakeDescriptor_0.GetGroup().GetBakeHash();
	}

	public override void CullAdditionalOccluders(ref HashSet<Renderer> additionalOccluders)
	{
		if (additionalOccluders == null)
		{
			return;
		}
		Bounds bounds = new Bounds(base.transform.position, volumeSize);
		HashSet<Renderer> hashSet = new HashSet<Renderer>();
		foreach (Renderer additionalOccluder in additionalOccluders)
		{
			if (bounds.Intersects(additionalOccluder.bounds))
			{
				hashSet.Add(additionalOccluder);
			}
		}
		additionalOccluders = hashSet;
	}

	public override void PostProcessBakeData(PerfectCullingBakeData data)
	{
	}

	public void OnDestroy()
	{
		method_1();
	}

	public override void Start()
	{
		method_2();
	}

	public void method_1()
	{
		list_0.Remove(this);
		if (list_2 == null)
		{
			return;
		}
		if (PerfectCullingCrossSceneSampler.Instance != null)
		{
			PerfectCullingCrossSceneSampler.Instance.AwaitWorkJobFinish();
		}
		foreach (GClass1254 item in list_2)
		{
			AllRuntimeCrossGroupVolumes.Remove(item);
			foreach (global::VisibilityIndicesClass<ushort> value in item.syncSamples.Values)
			{
				if (value != global::VisibilityIndicesClass<ushort>.Empty)
				{
					GClass1245.ReuseCellArray(value);
				}
			}
			item.syncSamples.Clear();
			item.streamer.Dispose();
			item.streamer = null;
		}
		list_2.Clear();
		list_2 = null;
	}

	public void method_2()
	{
		list_0.Add(this);
		runtimeVolumeBounds = volumeBakeBounds;
	}

	public void ReplaceGroups(List<PerfectCullingCrossSceneGroup> newGroups)
	{
		bakeDescriptors = new List<BakeDescriptor>();
		foreach (PerfectCullingCrossSceneGroup newGroup in newGroups)
		{
			AddBakeDescriptor().crossSceneGroup = new GuidReference(newGroup.GetComponent<GuidComponent>());
		}
	}

	public static int GetFrameHashNoOrientation(Vector3 pos, global::VisibilityIndicesClass<int> runtimeVolumesToUpdate = null, global::VisibilityIndicesClass<int> runtimeGroupsToUpdate = null)
	{
		int num = 13;
		int num2 = 0;
		foreach (GClass1254 allRuntimeCrossGroupVolume in AllRuntimeCrossGroupVolumes)
		{
			bool isOutOfBounds;
			int indexForWorldPosNoOrientationMT = allRuntimeCrossGroupVolume.GetIndexForWorldPosNoOrientationMT(pos, out isOutOfBounds);
			if (!isOutOfBounds)
			{
				num = num * 17 + indexForWorldPosNoOrientationMT;
				runtimeVolumesToUpdate?.Add(num2);
				runtimeGroupsToUpdate?.Add(allRuntimeCrossGroupVolume.groupIndex);
			}
			num2++;
		}
		return num;
	}
}
