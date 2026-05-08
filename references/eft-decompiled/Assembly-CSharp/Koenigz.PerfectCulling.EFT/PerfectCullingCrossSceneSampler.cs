using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Koenigz.PerfectCulling.EFT;

public class PerfectCullingCrossSceneSampler : MonoBehaviour
{
	public class Class854
	{
		public PerfectCullingCamera camera;

		public int threadId;

		public int numThreads;
	}

	public class Class855
	{
		public HashSet<int> VolumeIds = new HashSet<int>();

		public HashSet<int> GroupIds = new HashSet<int>();

		public ManualResetEventSlim WorkThreadWaitHandle = new ManualResetEventSlim(initialState: false);

		public object LockUpdate = new object();

		public long WorkTime;

		public void Clear()
		{
			VolumeIds.Clear();
			GroupIds.Clear();
		}
	}

	public abstract class Class856
	{
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		public struct Struct242 : IJobParallelFor
		{
			public void Execute(int index)
			{
				if (!TerminateToken.IsCancellationRequested)
				{
					smethod_4(TerminateToken.Token, JobParams.camera.ObservePosition, VolumeIds[index], List_1[index], List_0[index]);
				}
			}
		}

		[StructLayout(LayoutKind.Sequential, Size = 1)]
		public struct Struct243 : IJobParallelFor
		{
			public void Execute(int index)
			{
				if (!TerminateToken.IsCancellationRequested)
				{
					smethod_5(TerminateToken.Token, JobParams.camera, GroupIds[index]);
				}
			}
		}

		public static Class854 JobParams;

		public static CancellationTokenSource TerminateToken;

		public static readonly HashSet<int>[] VolumeIds = new HashSet<int>[2]
		{
			new HashSet<int>(),
			new HashSet<int>()
		};

		public static readonly HashSet<int>[] GroupIds = new HashSet<int>[2]
		{
			new HashSet<int>(),
			new HashSet<int>()
		};

		[NonSerialized]
		public static List<int>[] List_0 = new List<int>[2]
		{
			new List<int>(),
			new List<int>()
		};

		[NonSerialized]
		public static List<int>[] List_1 = new List<int>[2]
		{
			new List<int>(),
			new List<int>()
		};
	}

	public class Class857
	{
		public PerfectCullingAdaptiveGrid grid;
	}

	public class Class858
	{
		public bool IsActive;

		public Vector3 Position;

		public float FieldOfView;

		public Func<Camera> CameraGetter;

		public Class858(Func<Camera> cam)
		{
			CameraGetter = cam;
			Update();
		}

		public void Update()
		{
			Camera camera = CameraGetter();
			if (camera != null)
			{
				IsActive = camera.gameObject.activeInHierarchy;
				Position = camera.transform.position;
				FieldOfView = camera.fieldOfView;
			}
			else
			{
				IsActive = false;
			}
		}
	}

	public struct Struct244 : IJobParallelFor
	{
		[ReadOnly]
		public NativeArray<RaycastHit> SnapRaycastResults;

		public void Execute(int index)
		{
			if (index < int_5)
			{
				bool flag = GClass1255.GetColliderID(SnapRaycastResults[index]) > 0;
				bool_3[index] = bool_2[index] || !flag;
			}
		}
	}

	public struct Struct245 : IJobParallelFor
	{
		public LayerMask Mask;

		public Vector3 CamPos;

		[WriteOnly]
		public NativeArray<RaycastCommand> SnapRaycastCommands;

		[WriteOnly]
		public NativeArray<RaycastHit> SnapRaycastResults;

		public void Execute(int index)
		{
			if (index < int_5)
			{
				OrientedBounds cellBounds = cullingCellData_1[index].CellBounds;
				bool_3[index] = false;
				bool isPointInside;
				Vector3 vector = cellBounds.ClosestPointOriented(CamPos, out isPointInside);
				bool_2[index] = isPointInside;
				SnapRaycastResults[index] = default(RaycastHit);
				vector3_0[index] = vector;
				SnapRaycastCommands[index] = new RaycastCommand
				{
					from = CamPos,
					direction = (vector - CamPos).normalized,
					distance = (vector - CamPos).magnitude,
					layerMask = Mask,
					maxHits = 1
				};
			}
		}
	}

	public struct Struct246 : IJob
	{
		public Vector3 ObservePosition;

		public void Execute()
		{
			Bounds bounds = new Bounds(ObservePosition, Vector3.one * float_3);
			int_5 = PerfectCullingAdaptiveGrid.Instance.RuntimeBVH.OverlapNoAlloc(bounds, cullingCellData_1);
		}
	}

	public struct Struct247 : IJob
	{
		public Vector3 ObservePosition;

		public void method_0(GStruct113 e)
		{
			PerfectCullingAdaptiveGrid.Instance.RuntimeGroupMapping[e.CullingGroupId].method_10(e.Indices);
		}

		public static CullingCellData smethod_0(CullingCellData[] buffer, int numElementsInBuffer, Vector3 point, out int bestCell)
		{
			bestCell = -1;
			if (numElementsInBuffer == 0)
			{
				return null;
			}
			float num = -1f;
			int num2 = 0;
			while (true)
			{
				if (num2 < numElementsInBuffer)
				{
					if (bool_3[num2])
					{
						bool isPointInside;
						Vector3 vector = buffer[num2].ClosestPointOriented(point, out isPointInside);
						if (isPointInside)
						{
							break;
						}
						float sqrMagnitude = (vector - point).sqrMagnitude;
						if (num < 0f || sqrMagnitude < num)
						{
							num = sqrMagnitude;
							bestCell = num2;
						}
					}
					num2++;
					continue;
				}
				if (bestCell < 0)
				{
					return null;
				}
				return buffer[bestCell];
			}
			bestCell = num2;
			return buffer[num2];
		}

		public void Execute()
		{
			try
			{
				PerfectCullingAdaptiveGrid instance = PerfectCullingAdaptiveGrid.Instance;
				if (instance == null || Instance == null || !Instance.bool_0 || disableWorkThread)
				{
					return;
				}
				smethod_7();
				int bestCell;
				CullingCellData cullingCellData = smethod_0(cullingCellData_1, int_5, ObservePosition, out bestCell);
				if (cullingCellData_0 != cullingCellData)
				{
					CullingCellData cullingCellData2 = smethod_6(cullingCellData);
					if (cullingCellData2 != null)
					{
						int_1 = cullingCellData2.RuntimeCellId;
						Instance.bounds_0 = cullingCellData2.SpatialItemBounds;
						instance.IndexDecompressor.DecompressCullingCell(cullingCellData2.RuntimeCellId, method_0);
					}
				}
			}
			catch (Exception)
			{
			}
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class859
	{
		public static readonly Class859 class859_0 = new Class859();

		public static Func<Camera> func_0;

		public static Func<Camera> func_1;

		public Camera method_0()
		{
			return CameraClass.Instance?.Camera;
		}

		public Camera method_1()
		{
			return CameraClass.Instance?.OpticCameraManager.Camera;
		}
	}

	[CompilerGenerated]
	private static PerfectCullingCrossSceneSampler perfectCullingCrossSceneSampler_0;

	[Tooltip("Current culling camera.")]
	[SerializeField]
	private PerfectCullingCamera _cullingCamera;

	[Tooltip("Number of work threads for presampling data. Typically 2 threads is enough. More threads may lead to poor performance.")]
	[SerializeField]
	private int _numWorkThreads = 2;

	[SerializeField]
	private PerfectCullingCrossSceneVolume[] _forceInitializeVolumes;

	private Thread[] thread_0;

	private float float_0;

	private float float_1;

	private int int_0;

	private float float_2;

	private volatile bool bool_0;

	private JobHandle? nullable_0;

	private static CancellationTokenSource cancellationTokenSource_0;

	private static CancellationToken cancellationToken_0;

	public static bool disableWorkThread;

	public static bool disableReuse;

	public static bool disableSampling;

	[CompilerGenerated]
	private bool bool_1;

	private static AnalyticSource[] analyticSource_0;

	private static readonly Class855[] class855_0 = Array.Empty<Class855>();

	private static GClass1238 gclass1238_0;

	private static GClass1237 gclass1237_0;

	private Task task_0;

	private static int int_1;

	private Bounds bounds_0;

	internal static float float_3 = 5f;

	private const int int_2 = 1;

	private const int int_3 = 100;

	private static CullingCellData cullingCellData_0;

	private static float float_4 = 2f;

	private static object object_0 = new object();

	private static readonly List<Class858> list_0 = new List<Class858>();

	private JobHandle jobHandle_0;

	private static int int_4;

	private static Vector3[] vector3_0 = new Vector3[512];

	private static bool[] bool_2 = new bool[512];

	private static NativeArray<RaycastCommand> nativeArray_0;

	private static NativeArray<RaycastHit> nativeArray_1;

	public const int MAX_OVERLAP_BUFFER_SIZE = 512;

	private static int int_5;

	private static bool[] bool_3 = new bool[512];

	private static CullingCellData[] cullingCellData_1 = new CullingCellData[512];

	private static LayerMask layerMask_0;

	private const string string_0 = "LowPolyCollider";

	public static PerfectCullingCrossSceneSampler Instance
	{
		[CompilerGenerated]
		get
		{
			return perfectCullingCrossSceneSampler_0;
		}
		[CompilerGenerated]
		set
		{
			perfectCullingCrossSceneSampler_0 = value;
		}
	}

	public static bool Exists => Instance != null;

	public PerfectCullingCamera CullingCamera
	{
		get
		{
			return _cullingCamera;
		}
		set
		{
			_cullingCamera = value;
		}
	}

	public bool ShowStats
	{
		[CompilerGenerated]
		get
		{
			return bool_1;
		}
		[CompilerGenerated]
		set
		{
			bool_1 = value;
		}
	}

	public bool IsUpdatingVisibility => bool_0;

	public static bool IsAutocullingExists
	{
		get
		{
			IReadOnlyCollection<PerfectCullingCrossSceneVolume> allCrossSceneVolumes = PerfectCullingCrossSceneVolume.AllCrossSceneVolumes;
			List<PerfectCullingCrossSceneGroup> allCrossGroups = PerfectCullingCrossSceneGroup.AllCrossGroups;
			if (allCrossSceneVolumes != null && allCrossGroups != null)
			{
				if (allCrossSceneVolumes != null && allCrossSceneVolumes.Count > 0)
				{
					if (allCrossGroups == null)
					{
						return false;
					}
					return allCrossGroups.Count > 0;
				}
				return false;
			}
			return false;
		}
	}

	public static int Int32_0 => int_1;

	public void Start()
	{
		float_0 = Time.time;
		method_0();
	}

	public void OnDestroy()
	{
		Shutdown();
	}

	public void Update()
	{
		method_4();
	}

	public void LateUpdate()
	{
		method_5();
	}

	public void Shutdown()
	{
		analyticSource_0 = null;
		GClass712.Debug("Async sampler shutdown. Waiting for work threads to finish");
		if (cancellationTokenSource_0 != null)
		{
			cancellationTokenSource_0.Cancel();
		}
		AwaitWorkJobFinish();
		Class856.JobParams = null;
		if (thread_0 != null)
		{
			Thread[] array = thread_0;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Join();
			}
		}
		if (!jobHandle_0.IsCompleted)
		{
			jobHandle_0.Complete();
		}
		thread_0 = null;
		if (cancellationTokenSource_0 != null)
		{
			cancellationTokenSource_0.Dispose();
			cancellationTokenSource_0 = null;
		}
		if (gclass1237_0 != null)
		{
			gclass1237_0.Dispose();
		}
		gclass1237_0 = null;
		if (gclass1238_0 != null)
		{
			gclass1238_0.Dispose();
		}
		gclass1238_0 = null;
		DeinitializeAutoCulling();
		if (Instance == this)
		{
			Instance = null;
		}
		GClass712.Debug("Async sampler terminated");
	}

	public static async Task InitializeAutoCulling(CancellationToken token, IProgress<float> progress = null)
	{
		GClass712.Debug("Initializing autoculling");
		if (PerfectCullingCrossSceneVolume.AllRuntimeCrossGroupVolumes.Count > 0)
		{
			GClass712.Throw("Previous autoculling was not cleared properly");
		}
		foreach (PerfectCullingCrossSceneGroup allCrossGroup in PerfectCullingCrossSceneGroup.AllCrossGroups)
		{
			allCrossGroup.OnPostLevelLoaded();
		}
		PerfectCullingAdaptiveGrid instance = PerfectCullingAdaptiveGrid.Instance;
		if (instance != null)
		{
			await instance.method_0(token, progress);
		}
		if (!token.IsCancellationRequested)
		{
			smethod_9();
			ScreenDistanceSwitcher.Boolean_0 = true;
			analyticSource_0 = UnityEngine.Object.FindObjectsOfType<AnalyticSource>();
		}
	}

	public static void smethod_0(Scene prev, Scene newScene)
	{
	}

	public static void smethod_1()
	{
		Instance.CullingCamera.method_3();
		foreach (PerfectCullingCrossSceneGroup allCrossGroup in PerfectCullingCrossSceneGroup.AllCrossGroups)
		{
			allCrossGroup.UpdateSwitchQueuesMT();
		}
		Instance.method_4();
	}

	public static int GetRuntimeMemoryUsageMb()
	{
		int num = 0;
		foreach (PerfectCullingCrossSceneVolume.GClass1254 allRuntimeCrossGroupVolume in PerfectCullingCrossSceneVolume.AllRuntimeCrossGroupVolumes)
		{
			num += allRuntimeCrossGroupVolume.streamer.GetRuntimeMemorySizeBytes();
		}
		num += GClass1245.RuntimeMemoryUsage;
		return num / 1048576;
	}

	public static void DeinitializeAutoCulling()
	{
		PerfectCullingBakeGroup.numPersistentShadowLods = 0;
		ScreenDistanceSwitcher.Boolean_0 = false;
	}

	public void method_0()
	{
		GClass712.Debug("Initializing culling async sampler " + base.gameObject.name);
		if (Instance != null)
		{
			GClass712.Throw("Autocull sampler exists " + Instance.gameObject.name);
		}
		Instance = this;
		smethod_10();
		layerMask_0 = LayerMask.GetMask("LowPolyCollider");
		bool_0 = true;
		if (_cullingCamera == null)
		{
			_cullingCamera = GetComponent<PerfectCullingCamera>();
		}
		if (thread_0 != null)
		{
			return;
		}
		if (_cullingCamera == null)
		{
			_cullingCamera = UnityEngine.Object.FindObjectOfType<PerfectCullingCamera>();
		}
		cancellationTokenSource_0 = new CancellationTokenSource();
		cancellationToken_0 = cancellationTokenSource_0.Token;
		if (IsAutocullingExists)
		{
			Class856.JobParams = new Class854
			{
				camera = _cullingCamera
			};
			CullingGridPreProcess cullingGridPreProcess = UnityEngine.Object.FindObjectOfType<CullingGridPreProcess>();
			if (cullingGridPreProcess != null)
			{
				gclass1238_0 = new GClass1238(PerfectCullingAdaptiveGrid.Instance, cullingGridPreProcess);
				gclass1237_0 = new GClass1237(gclass1238_0, _cullingCamera);
			}
		}
	}

	public void method_1()
	{
		if (!(Time.time - float_0 >= 1f))
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		float_0 = Time.time;
		foreach (PerfectCullingCrossSceneVolume.GClass1254 allRuntimeCrossGroupVolume in PerfectCullingCrossSceneVolume.AllRuntimeCrossGroupVolumes)
		{
			num += allRuntimeCrossGroupVolume.streamer.bytesRead;
			num2 += allRuntimeCrossGroupVolume.streamer.readOps;
			allRuntimeCrossGroupVolume.streamer.bytesRead = 0;
			allRuntimeCrossGroupVolume.streamer.readOps = 0;
		}
		float_1 = num;
		int_0 = num2;
		if (float_1 > float_2)
		{
			float_2 = float_1;
		}
	}

	public void method_2()
	{
		if (!ShowStats)
		{
			return;
		}
		GUILayout.BeginArea(new Rect(0f, 0f, 400f, Screen.height));
		GUILayout.BeginVertical();
		if (gclass1238_0 != null)
		{
			GUILayout.Label($"Culling grid group id: {gclass1238_0.GroupId}");
		}
		if (gclass1237_0 != null)
		{
			GUILayout.Label($"LCQ: {GClass1237.LastQuery.IsEmpty}");
			GUILayout.Label($"Dynamic grid objects: {GClass1237.NumDynamicVisibleLastFrame} / {GClass1237.NumDynamicObjects}");
			GUILayout.Label($"SwitchQueueSizes: {GClass1237.SwitchQueueSizes.activate} | {GClass1237.SwitchQueueSizes.deactivate}");
		}
		GUILayout.Label($"Num overlap cells: {int_5}");
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < int_5; i++)
		{
			stringBuilder.Append($"{GClass1255.GetColliderID(nativeArray_1[i])} ");
		}
		GUILayout.Label("Collider IDs: " + stringBuilder.ToString());
		GUILayout.Label($"Culling job time: {int_4}ms");
		GUILayout.Label($"Culling cell ID: {int_1}");
		GUILayout.Label("Streaming: " + (float_1 / 1048576f).ToString("0.00") + " MB/s | Max: " + (float_2 / 1048576f).ToString("0.00") + " MB/s");
		GUILayout.Label($"Mem alloc: {GetRuntimeMemoryUsageMb()} MB");
		GUILayout.Label($"Buffers alloc: {GClass1245.NumDebugAllocs}");
		GUILayout.Label($"Buffers Free: {GClass1245.NumFreeBuffers}");
		GUILayout.Label($"Read ops: {int_0}/s");
		GUILayout.Label($"Persistent shadow renderers: {PerfectCullingBakeGroup.numPersistentShadowLods}");
		GUILayout.Space(10f);
		GUILayout.Label("Workthread load");
		int num = 0;
		Class855[] array = class855_0;
		foreach (Class855 @class in array)
		{
			GUILayout.Label($" {num + 1} = {@class.WorkTime}ms");
			num++;
		}
		if (analyticSource_0 != null)
		{
			int num2 = 0;
			AnalyticSource[] array2 = analyticSource_0;
			for (int j = 0; j < array2.Length; j++)
			{
				if (array2[j].IsAutocullVisible)
				{
					num2++;
				}
			}
			GUILayout.Label($"AnalSources: {num2}/{analyticSource_0.Length}");
		}
		if (PerfectCullingCrossSceneGroup.AllCrossGroups != null && PerfectCullingCrossSceneGroup.AllCrossGroups.Count != 0)
		{
			foreach (PerfectCullingCrossSceneGroup allCrossGroup in PerfectCullingCrossSceneGroup.AllCrossGroups)
			{
				if (allCrossGroup.AllowAdaptiveGridMapping)
				{
					(int, int) switchStats = allCrossGroup.SwitchStats;
					GUILayout.Label($"{allCrossGroup.gameObject.name} - A:{switchStats.Item1} | D:{switchStats.Item2}");
				}
			}
		}
		else
		{
			GUILayout.Label("No culling groups");
		}
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

	public void ScheduleUpdate(global::VisibilityIndicesClass<int> inputRuntimeVolumeIds, global::VisibilityIndicesClass<int> inputGroupIds)
	{
		lock (class855_0[0].LockUpdate)
		{
			for (int i = 0; i < inputRuntimeVolumeIds.Count; i++)
			{
				class855_0[0].VolumeIds.Add(inputRuntimeVolumeIds.Buffer[i]);
			}
			for (int j = 0; j < inputGroupIds.Count; j++)
			{
				class855_0[0].GroupIds.Add(inputGroupIds.Buffer[j]);
			}
			class855_0[0].WorkThreadWaitHandle.Set();
		}
	}

	public JobHandle? ScheduleJob(global::VisibilityIndicesClass<int> inputRuntimeVolumeIds, global::VisibilityIndicesClass<int> inputGroupIds)
	{
		if (cancellationTokenSource_0 != null && !cancellationTokenSource_0.IsCancellationRequested && Class856.JobParams != null)
		{
			Class856.VolumeIds[0].Clear();
			Class856.VolumeIds[1].Clear();
			Class856.GroupIds[0].Clear();
			Class856.GroupIds[1].Clear();
			int num = inputRuntimeVolumeIds.Count / 2;
			for (int i = 0; i < inputRuntimeVolumeIds.Count; i++)
			{
				int num2 = ((i > num) ? 1 : 0);
				Class856.VolumeIds[num2].Add(inputRuntimeVolumeIds.Buffer[i]);
			}
			int num3 = inputGroupIds.Count / 2;
			for (int j = 0; j < inputGroupIds.Count; j++)
			{
				int num4 = ((j > num3) ? 1 : 0);
				Class856.GroupIds[num4].Add(inputGroupIds.Buffer[j]);
			}
			if (Class856.TerminateToken != null)
			{
				Class856.TerminateToken.Dispose();
			}
			Class856.TerminateToken = new CancellationTokenSource();
			Class856.Struct243 jobData = default(Class856.Struct243);
			JobHandle dependsOn = IJobParallelForExtensions.Schedule(default(Class856.Struct242), 2, 1);
			JobHandle value = IJobParallelForExtensions.Schedule(jobData, 2, 1, dependsOn);
			nullable_0 = value;
			return nullable_0;
		}
		return null;
	}

	public bool FinishJob(JobHandle handle)
	{
		CancellationTokenSource terminateToken = Class856.TerminateToken;
		bool result = handle.IsCompleted && !terminateToken.IsCancellationRequested;
		if (!handle.IsCompleted)
		{
			terminateToken.Cancel();
		}
		handle.Complete();
		return result;
	}

	public void AwaitWorkJobFinish()
	{
		JobHandle? jobHandle = nullable_0;
		if (jobHandle.HasValue)
		{
			JobHandle valueOrDefault = jobHandle.GetValueOrDefault();
			FinishJob(valueOrDefault);
		}
		if (Class856.TerminateToken != null)
		{
			Class856.TerminateToken.Dispose();
			Class856.TerminateToken = null;
		}
		nullable_0 = null;
	}

	public static void smethod_2(object p)
	{
		Class854 obj = p as Class854;
		PerfectCullingCamera camera = obj.camera;
		int threadId = obj.threadId;
		List<int> sortedIndices = new List<int>();
		List<int> removeCells = new List<int>();
		Class855 @class = class855_0[threadId];
		Stopwatch stopwatch = Stopwatch.StartNew();
		while (!cancellationToken_0.IsCancellationRequested)
		{
			try
			{
				@class.WorkThreadWaitHandle.Wait(cancellationToken_0);
				if (cancellationToken_0.IsCancellationRequested)
				{
					break;
				}
				stopwatch.Restart();
				lock (@class.LockUpdate)
				{
					Vector3 observePosition = camera.ObservePosition;
					if (disableWorkThread)
					{
						@class.WorkTime = stopwatch.ElapsedMilliseconds;
						@class.Clear();
						@class.WorkThreadWaitHandle.Reset();
						continue;
					}
					if (smethod_3(cancellationToken_0, camera, observePosition, @class.VolumeIds, @class.GroupIds, removeCells, sortedIndices))
					{
						Thread.Yield();
					}
					@class.WorkTime = stopwatch.ElapsedMilliseconds;
					@class.Clear();
					@class.WorkThreadWaitHandle.Reset();
				}
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception ex2)
			{
				GClass712.Error(ex2.Message + " | " + ex2.StackTrace + " " + ex2.InnerException?.Message);
			}
		}
	}

	public static bool smethod_3(CancellationToken terminateToken, PerfectCullingCamera camera, Vector3 worldPos, HashSet<int> volumeIds, HashSet<int> groupIds, List<int> removeCells, List<int> sortedIndices)
	{
		smethod_4(terminateToken, worldPos, volumeIds, removeCells, sortedIndices);
		if (terminateToken.IsCancellationRequested)
		{
			return false;
		}
		return smethod_5(terminateToken, camera, groupIds);
	}

	public static void smethod_4(CancellationToken terminateToken, Vector3 worldPos, HashSet<int> volumeIds, List<int> removeCells, List<int> sortedIndices)
	{
		foreach (int volumeId in volumeIds)
		{
			PerfectCullingCrossSceneVolume.GClass1254 gClass = PerfectCullingCrossSceneVolume.AllRuntimeCrossGroupVolumes[volumeId];
			if (gClass == null)
			{
				GClass712.Throw("Null volume found");
			}
			if (terminateToken.IsCancellationRequested)
			{
				break;
			}
			try
			{
				if (!Monitor.TryEnter(gClass.lockSampleQueue))
				{
					continue;
				}
				bool isOutOfBounds;
				int indexForWorldPosNoOrientationMT = gClass.GetIndexForWorldPosNoOrientationMT(worldPos, out isOutOfBounds);
				if (isOutOfBounds || gClass.lastCellIndex == indexForWorldPosNoOrientationMT)
				{
					continue;
				}
				if (!disableReuse)
				{
					if (gClass.runtimeBounds.SqrDistance(worldPos) > gClass.reuseDistance2)
					{
						foreach (KeyValuePair<int, global::VisibilityIndicesClass<ushort>> syncSample in gClass.syncSamples)
						{
							if (syncSample.Value != global::VisibilityIndicesClass<ushort>.Empty)
							{
								GClass1245.ReuseCellArray(syncSample.Value);
							}
						}
						gClass.syncSamples.Clear();
					}
					else
					{
						removeCells.Clear();
						foreach (KeyValuePair<int, global::VisibilityIndicesClass<ushort>> syncSample2 in gClass.syncSamples)
						{
							if (syncSample2.Value != global::VisibilityIndicesClass<ushort>.Empty && (gClass.GetSamplingPositionAtMT(syncSample2.Key) - worldPos).sqrMagnitude > gClass.reuseDistance2)
							{
								removeCells.Add(syncSample2.Key);
							}
						}
						foreach (int removeCell in removeCells)
						{
							GClass1245.ReuseCellArray(gClass.syncSamples[removeCell]);
							gClass.syncSamples.Remove(removeCell);
						}
					}
					Thread.Yield();
				}
				if (!disableSampling)
				{
					sortedIndices.Clear();
					Vector3[] visibilityOffsets = gClass.visibilityOffsets;
					foreach (Vector3 vector in visibilityOffsets)
					{
						Vector3 pos = worldPos + vector;
						bool isOutOfBounds2;
						int indexForWorldPosMT = gClass.GetIndexForWorldPosMT(pos, out isOutOfBounds2);
						if (!isOutOfBounds2 && !gClass.syncSamples.ContainsKey(indexForWorldPosMT))
						{
							if (gClass.streamer.StreamData.indices[indexForWorldPosMT].dataLength == 0)
							{
								gClass.syncSamples[indexForWorldPosMT] = global::VisibilityIndicesClass<ushort>.Empty;
							}
							else
							{
								sortedIndices.Add(indexForWorldPosMT);
							}
						}
					}
					sortedIndices.Sort();
					foreach (int sortedIndex in sortedIndices)
					{
						global::VisibilityIndicesClass<ushort> visibilityIndicesClass = GClass1245.AllocateCellArray();
						if (gClass.streamer.SampleByIndexFromDisk(sortedIndex, visibilityIndicesClass) != 0)
						{
							if (!gClass.syncSamples.ContainsKey(sortedIndex))
							{
								gClass.syncSamples[sortedIndex] = visibilityIndicesClass;
							}
							else
							{
								GClass1245.ReuseCellArray(visibilityIndicesClass);
							}
							continue;
						}
						throw new Exception("Attempt to sample zero data");
					}
					gClass.lastCellIndex = indexForWorldPosNoOrientationMT;
				}
				Thread.Yield();
			}
			catch (Exception ex)
			{
				GClass712.Error(ex.Message + " | " + ex.StackTrace + "\n" + ex?.InnerException?.StackTrace);
			}
			finally
			{
				if (Monitor.IsEntered(gClass.lockSampleQueue))
				{
					Monitor.Exit(gClass.lockSampleQueue);
				}
			}
		}
	}

	public static bool smethod_5(CancellationToken terminateToken, PerfectCullingCamera camera, HashSet<int> groupIds)
	{
		if (!Instance.bool_0)
		{
			return false;
		}
		foreach (int groupId in groupIds)
		{
			if (!terminateToken.IsCancellationRequested)
			{
				PerfectCullingCrossSceneGroup perfectCullingCrossSceneGroup = PerfectCullingCrossSceneGroup.AllCrossGroups[groupId];
				if (perfectCullingCrossSceneGroup.disableOnRuntime)
				{
					continue;
				}
				int counterMainThread = perfectCullingCrossSceneGroup.counterMainThread;
				try
				{
					if (counterMainThread != perfectCullingCrossSceneGroup.counterWorkThread)
					{
						Monitor.Enter(perfectCullingCrossSceneGroup.lockUpdateVisibilityQueues);
						if (perfectCullingCrossSceneGroup.UpdateVisibleSetsMT(camera))
						{
							perfectCullingCrossSceneGroup.counterWorkThread = counterMainThread;
						}
					}
				}
				catch (Exception ex)
				{
					throw ex;
				}
				finally
				{
					if (Monitor.IsEntered(perfectCullingCrossSceneGroup.lockUpdateVisibilityQueues))
					{
						Monitor.Exit(perfectCullingCrossSceneGroup.lockUpdateVisibilityQueues);
					}
				}
				continue;
			}
			return false;
		}
		return true;
	}

	public static void UpdateBakedLODVisibility(PerfectCullingCrossSceneGroup group, ScreenDistanceSwitcher lod)
	{
		smethod_6(null);
	}

	public static CullingCellData smethod_6(CullingCellData newCell)
	{
		lock (object_0)
		{
			cullingCellData_0 = newCell;
			return cullingCellData_0;
		}
	}

	public static void smethod_7()
	{
		List<ScreenDistanceSwitcher> list = ScreenDistanceSwitcher.list_0;
		if (list == null || list.Count == 0)
		{
			return;
		}
		foreach (ScreenDistanceSwitcher item in list)
		{
			item.IsAutocullVisible = smethod_8(item, list_0);
		}
	}

	public static bool smethod_8(ScreenDistanceSwitcher sw, List<Class858> cameras)
	{
		foreach (Class858 camera in cameras)
		{
			if (camera.IsActive && sw.IsBakedAutocullVisible && GClass1244.IsVisibleByCamera(camera.Position, sw.RuntimeCenter, camera.FieldOfView, sw.RuntimeSize, sw.RuntimeRelativeScreenSwitchHeight, float_4))
			{
				return true;
			}
		}
		return false;
	}

	public static void smethod_9()
	{
		list_0.Clear();
		list_0.Add(new Class858(() => CameraClass.Instance?.Camera));
		list_0.Add(new Class858(() => CameraClass.Instance?.OpticCameraManager.Camera));
	}

	public void method_3()
	{
		foreach (Class858 item in list_0)
		{
			item.Update();
		}
		float_4 = QualitySettings.lodBias;
	}

	public static void smethod_10()
	{
		if (nativeArray_0.Length == 0 || nativeArray_1.Length == 0)
		{
			nativeArray_0 = new NativeArray<RaycastCommand>(512, Allocator.Persistent);
			nativeArray_1 = new NativeArray<RaycastHit>(512, Allocator.Persistent);
		}
	}

	public void method_4()
	{
		if (!(PerfectCullingAdaptiveGrid.Instance == null) && PerfectCullingAdaptiveGrid.Instance.RuntimeBVH != null)
		{
			if (gclass1237_0 != null)
			{
				gclass1237_0.Update();
			}
			method_3();
			Struct246 jobData = default(Struct246);
			Vector3 vector = (jobData.ObservePosition = _cullingCamera.ObservePosition);
			JobHandle dependsOn = jobData.Schedule();
			Struct245 jobData2 = default(Struct245);
			jobData2.Mask = layerMask_0;
			jobData2.CamPos = vector;
			jobData2.SnapRaycastCommands = nativeArray_0;
			jobData2.SnapRaycastResults = nativeArray_1;
			Struct244 jobData3 = default(Struct244);
			jobData3.SnapRaycastResults = nativeArray_1;
			JobHandle dependsOn2 = IJobParallelForExtensions.Schedule(jobData2, int_5, 1, dependsOn);
			JobHandle dependsOn3 = RaycastCommand.ScheduleBatch(nativeArray_0, nativeArray_1, 4, dependsOn2);
			JobHandle dependsOn4 = IJobParallelForExtensions.Schedule(jobData3, int_5, 1, dependsOn3);
			Struct247 jobData4 = default(Struct247);
			jobData4.ObservePosition = vector;
			jobHandle_0 = jobData4.Schedule(dependsOn4);
			JobHandle.ScheduleBatchedJobs();
		}
	}

	public void method_5()
	{
		jobHandle_0.Complete();
		gclass1237_0?.LateUpdate();
	}
}
