using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EFT.Interactive;
using UnityEngine;
using UnityEngine.Internal;
using UnityEngine.SceneManagement;

public abstract class EFTPhysicsClass
{
	public class ColliderSync : MonoBehaviour
	{
		private GClass742 gclass742_0;

		private BoxCollider boxCollider_0;

		private int? nullable_0;

		public void Assign(GClass742 searcher, BoxCollider collider)
		{
			gclass742_0 = searcher;
			boxCollider_0 = collider;
			if (base.gameObject.activeInHierarchy)
			{
				OnEnable();
			}
		}

		public void Sync()
		{
			int? num = nullable_0;
			if (num.HasValue)
			{
				int valueOrDefault = num.GetValueOrDefault();
				gclass742_0?.SyncCollider(valueOrDefault, boxCollider_0);
			}
		}

		public void OnEnable()
		{
			nullable_0 = gclass742_0?.RegisterCollider(boxCollider_0);
		}

		public void OnDisable()
		{
			gclass742_0?.RemoveCollider(boxCollider_0);
			nullable_0 = null;
		}
	}

	public abstract class Class421
	{
		[NonSerialized]
		public const long Long_0 = 30000L;

		[NonSerialized]
		public const float Float_0 = 5f;

		[NonSerialized]
		public const int Int_0 = 100;

		[NonSerialized]
		public static Stopwatch Stopwatch_0;

		[NonSerialized]
		public static long Long_1;

		[NonSerialized]
		public static float Float_1;

		[NonSerialized]
		public static int Int_1;

		[NonSerialized]
		[CompilerGenerated]
		public static bool Bool_0;

		public static bool RebuildingStaticCollidersDetected
		{
			[CompilerGenerated]
			get
			{
				return Bool_0;
			}
			[CompilerGenerated]
			set
			{
				Bool_0 = value;
			}
		}

		static Class421()
		{
			Stopwatch_0 = new Stopwatch();
			Stopwatch_0.Start();
		}

		public static void BeginOperation()
		{
			Long_1 = Stopwatch_0.ElapsedTicks;
		}

		public static void EndOperation()
		{
			if (Stopwatch_0.ElapsedTicks - Long_1 > 30000L)
			{
				Int_1++;
				if (Int_1 >= 100)
				{
					RebuildingStaticCollidersDetected = true;
				}
			}
		}

		public static void smethod_0(long passed)
		{
			if (Time.time - Float_1 >= 5f)
			{
				UnityEngine.Debug.Log("Possible physx is rebuilding static colliders tree: " + (float)passed / 10000f);
				Float_1 = Time.time;
			}
		}
	}

	public abstract class SyncTransformsClass
	{
		public enum UpdateModeType
		{
			FixedUpdate,
			Update,
			QualityControl,
			SmoothSimulate
		}

		[NonSerialized]
		public static bool Bool_0;

		public static UpdateModeType UpdateMode;

		[NonSerialized]
		public static float Float_0 = 1f;

		[NonSerialized]
		public static int Int_0 = 600;

		[NonSerialized]
		public static GClass828 Gclass828_0 = new GClass828(Int_0);

		public static bool UpdateControlledByUnity
		{
			get
			{
				return Physics.simulationMode != SimulationMode.Script;
			}
			set
			{
				Physics.simulationMode = ((!value) ? SimulationMode.Script : SimulationMode.FixedUpdate);
			}
		}

		public static bool SyncTransformsControlledByUnity
		{
			get
			{
				return Physics.autoSyncTransforms;
			}
			set
			{
				Physics.autoSyncTransforms = value;
			}
		}

		public static bool UpdateEnabled
		{
			get
			{
				return Bool_0;
			}
			set
			{
				Bool_0 = value;
			}
		}

		public static float Quality
		{
			get
			{
				return Float_0;
			}
			set
			{
				Float_0 = value;
			}
		}

		public static int SmoothSimulationSamplesCount
		{
			get
			{
				return Int_0;
			}
			set
			{
				Int_0 = value;
				Gclass828_0 = new GClass828(Int_0);
			}
		}

		public static double SimulationAVGDeltaTime => Gclass828_0.Average;

		public static bool Boolean_0
		{
			get
			{
				if (Physics.simulationMode == SimulationMode.Script)
				{
					return Bool_0;
				}
				return false;
			}
		}

		public static void smethod_0(float deltaTime)
		{
			if (Boolean_0)
			{
				Physics.Simulate(deltaTime);
			}
		}

		public static void smethod_1(float deltaTime)
		{
			Gclass828_0.AddValue(deltaTime);
			if (Boolean_0)
			{
				deltaTime = (float)Gclass828_0.Average;
				if (deltaTime > EFTHardSettings.Instance.MaxSmoothPhysicsDeltaTime)
				{
					deltaTime = EFTHardSettings.Instance.MaxSmoothPhysicsDeltaTime;
				}
				Physics.Simulate(deltaTime);
			}
		}

		public static void FixedUpdate()
		{
			if (UpdateMode == UpdateModeType.FixedUpdate)
			{
				smethod_0(Time.fixedDeltaTime);
			}
		}

		public static void Update()
		{
			if (UpdateMode == UpdateModeType.Update)
			{
				smethod_0(Time.deltaTime);
			}
			else if (UpdateMode == UpdateModeType.QualityControl)
			{
				smethod_2(Time.deltaTime);
			}
			else if (UpdateMode == UpdateModeType.SmoothSimulate)
			{
				smethod_1(Time.deltaTime);
			}
		}

		public static void SyncTransforms()
		{
			if (!Physics.autoSyncTransforms)
			{
				ForceSyncTransforms();
			}
		}

		public static void ForceSyncTransforms()
		{
			Physics.SyncTransforms();
		}

		public static void SetAVGSimulationDeltaTime(float time)
		{
			Gclass828_0.SetAll(time);
		}

		public static void smethod_2(float deltaTime)
		{
			float fixedDeltaTime = Time.fixedDeltaTime;
			int num = Mathf.RoundToInt(deltaTime / fixedDeltaTime * Float_0);
			if (num < 1)
			{
				num = 1;
			}
			float deltaTime2 = deltaTime / (float)num;
			for (int i = 0; i < num; i++)
			{
				smethod_0(deltaTime2);
			}
		}
	}

	public abstract class GClass745
	{
		public abstract class GClass746
		{
			public const float Low = 0f;

			public const float High = 1f;
		}

		public struct GStruct35
		{
			public Rigidbody rigidbody;

			public GClass833 visibilityChecker;
		}

		[NonSerialized]
		public const float Float_0 = 1f;

		[NonSerialized]
		public static List<GStruct35> List_0 = new List<GStruct35>(16);

		[NonSerialized]
		public static bool Bool_0;

		public static bool Enabled
		{
			get
			{
				return Bool_0;
			}
			set
			{
				Bool_0 = value;
				if (Bool_0)
				{
					SyncTransformsClass.UpdateControlledByUnity = false;
					SyncTransformsClass.UpdateEnabled = false;
					SyncTransformsClass.Quality = 1f;
					SyncTransformsClass.SyncTransformsControlledByUnity = false;
					SyncTransformsClass.UpdateMode = SyncTransformsClass.UpdateModeType.SmoothSimulate;
				}
			}
		}

		public static void SupportRigidbody(Rigidbody rigidbody, float quality = 1f, GClass833 visibilityChecker = null)
		{
			if (Bool_0)
			{
				if (quality > SyncTransformsClass.Quality)
				{
					SyncTransformsClass.Quality = quality;
				}
				GStruct35 item = new GStruct35
				{
					rigidbody = rigidbody,
					visibilityChecker = visibilityChecker
				};
				List_0.Add(item);
				smethod_0();
			}
		}

		public static void UnsupportRigidbody(Rigidbody rigidbody)
		{
			if (!Bool_0)
			{
				return;
			}
			for (int i = 0; i < List_0.Count; i++)
			{
				if (List_0[i].rigidbody == rigidbody)
				{
					List_0.RemoveAt(i);
					break;
				}
			}
			smethod_0();
		}

		public static void smethod_0()
		{
			int num = 0;
			for (int num2 = List_0.Count - 1; num2 >= 0; num2--)
			{
				GStruct35 gStruct = List_0[num2];
				if (!(gStruct.rigidbody == null) && gStruct.rigidbody.gameObject.activeInHierarchy)
				{
					if (gStruct.visibilityChecker == null || gStruct.visibilityChecker.IsVisible)
					{
						num++;
					}
				}
				else
				{
					List_0.RemoveAt(num2);
				}
			}
			smethod_1(num > 0);
			if (List_0.Count == 0)
			{
				SyncTransformsClass.Quality = 0f;
			}
		}

		public static void smethod_1(bool enabled)
		{
			if (SyncTransformsClass.UpdateEnabled != enabled)
			{
				SyncTransformsClass.UpdateEnabled = enabled;
			}
		}

		public static void Update()
		{
			if (Bool_0)
			{
				smethod_0();
			}
		}
	}

	public abstract class GClass747
	{
		[Flags]
		public enum EWorldType
		{
			Default = 2,
			VolumePropagationAndEnvironmentSwitcherTriggers = 4,
			DisablerCullingObjectTriggers = 8
		}

		public class GClass748
		{
			public static readonly GClass748 EmptyComplete = new GClass748
			{
				IsComplete = true,
				Count = 0
			};

			public Collider[] Results;

			public Collider[][] Buffers;

			public bool IsComplete;

			public int TotalWorldsCount;

			public int Count;

			[NonSerialized]
			public int Int_0;

			public void OneWorldCompleteCallback(int index, int count)
			{
				Int_0++;
				if (count > 0)
				{
					Array.Copy(Buffers[index], 0, Results, Count, count);
					Count += count;
				}
				if (Int_0 == TotalWorldsCount)
				{
					IsComplete = true;
				}
			}

			public void Reset()
			{
				TotalWorldsCount = 0;
				Int_0 = 0;
				Count = 0;
				IsComplete = false;
			}
		}

		public class GClass750 : GClass749
		{
			public GClass750()
				: base(EWorldType.Default)
			{
				base.IsCreated = true;
				PhysicsScene_0 = Physics.defaultPhysicsScene;
			}

			public override void Create()
			{
			}

			public override bool IsSupportingMask(int mask)
			{
				return true;
			}

			public override void ExtractGameObjectsToPhysicsScene()
			{
			}
		}

		public class GClass751 : GClass749
		{
			public GClass751()
				: base(EWorldType.DisablerCullingObjectTriggers)
			{
			}

			public override bool IsSupportingMask(int mask)
			{
				return (mask & LayerMaskClass.DisablerCullingObjectLayerMask) != 0;
			}

			public override void ExtractGameObjectsToPhysicsScene()
			{
				ColliderReporter[] array = GClass870.FindUnityObjectsOfType<ColliderReporter>();
				for (int i = 0; i < array.Length; i++)
				{
					GameObject gameObject = array[i].gameObject;
					if (gameObject.layer == LayerMaskClass.DisablerCullingObjectLayer)
					{
						method_1(gameObject);
					}
				}
				ref Scene scene_ = ref Scene_0;
				scene_.name = scene_.name + " (" + Scene_0.rootCount + ")";
			}
		}

		public class GClass752 : GClass749
		{
			public GClass752()
				: base(EWorldType.VolumePropagationAndEnvironmentSwitcherTriggers)
			{
			}

			public override bool IsSupportingMask(int mask)
			{
				return (mask & (int)LayerMaskClass.TriggersMask) != 0;
			}

			public override void ExtractGameObjectsToPhysicsScene()
			{
				method_2(GClass870.FindUnityObjectsOfType<MoveObjectsToAdditionalPhysSceneMarker>());
				method_2(GClass870.FindUnityObjectsOfType<SpatialAudioRoom>(), moveWithChild: true);
				ref Scene scene_ = ref Scene_0;
				scene_.name = scene_.name + " (" + Scene_0.rootCount + ")";
			}

			[CompilerGenerated]
			public void method_2(IEnumerable<Component> betterPropagationVolumes1, bool moveWithChild = false)
			{
				foreach (Component item in betterPropagationVolumes1)
				{
					method_1(item.gameObject, moveWithChild);
				}
			}
		}

		public abstract class GClass749
		{
			[NonSerialized]
			public Scene Scene_0;

			[NonSerialized]
			public PhysicsScene PhysicsScene_0;

			[NonSerialized]
			public List<GClass742> List_0 = new List<GClass742>();

			[NonSerialized]
			public int Int_0;

			[NonSerialized]
			[CompilerGenerated]
			public bool Bool_0;

			[NonSerialized]
			[CompilerGenerated]
			public EWorldType EworldType_0;

			[NonSerialized]
			[CompilerGenerated]
			public string String_0;

			public bool IsCreated
			{
				[CompilerGenerated]
				get
				{
					return Bool_0;
				}
				[CompilerGenerated]
				set
				{
					Bool_0 = value;
				}
			}

			public EWorldType WorldType
			{
				[CompilerGenerated]
				get
				{
					return EworldType_0;
				}
				[CompilerGenerated]
				set
				{
					EworldType_0 = value;
				}
			}

			public string Name
			{
				[CompilerGenerated]
				get
				{
					return String_0;
				}
				[CompilerGenerated]
				set
				{
					String_0 = value;
				}
			}

			public GClass749(EWorldType worldType)
			{
				WorldType = worldType;
				Name = WorldType.ToString();
			}

			public abstract bool IsSupportingMask(int mask);

			public bool CanBeUsed(EWorldType worldTypeMask, int castMask)
			{
				if (IsCreated && (worldTypeMask & WorldType) != 0)
				{
					return IsSupportingMask(castMask);
				}
				return false;
			}

			public virtual void Create()
			{
				List_0.Add(new GClass742());
				(Scene_0, PhysicsScene_0) = smethod_2("PhysicsWorld_" + Name);
				ExtractGameObjectsToPhysicsScene();
				SceneManager.sceneUnloaded += method_0;
				IsCreated = true;
			}

			public void method_0(Scene scene)
			{
				if (!(scene == Scene_0))
				{
					return;
				}
				IsCreated = false;
				foreach (GClass742 item in List_0)
				{
					item.Dispose();
				}
				List_0.Clear();
				SceneManager.sceneUnloaded -= method_0;
			}

			public abstract void ExtractGameObjectsToPhysicsScene();

			public void method_1(GameObject target, bool moveWithChild = false)
			{
				Transform parent = target.transform.parent;
				Scene scene = target.scene;
				if (scene == Scene_0)
				{
					return;
				}
				string fullPath = GClass6.GetFullPath(target.transform, withSceneName: true);
				target.transform.SetParent(null);
				SceneManager.MoveGameObjectToScene(target, Scene_0);
				GameObject gameObject = new GameObject(target.name + " (moved to " + Name + " physics world)");
				gameObject.transform.SetParent(null);
				SceneManager.MoveGameObjectToScene(gameObject, scene);
				gameObject.transform.parent = parent;
				gameObject.transform.SetPositionAndRotation(target.transform.position, target.transform.rotation);
				gameObject.transform.localScale = target.transform.localScale;
				for (int i = 0; i < target.transform.childCount; i++)
				{
					if (moveWithChild)
					{
						break;
					}
					Transform child = target.transform.GetChild(i);
					child.transform.SetParent(null);
					SceneManager.MoveGameObjectToScene(child.gameObject, scene);
					child.transform.parent = gameObject.transform;
				}
				target.name = fullPath;
				MirrorTransformChanges mirrorTransformChanges = null;
				if (target.CompareTag("DynamicCollider"))
				{
					mirrorTransformChanges = target.AddComponent<MirrorTransformChanges>();
					mirrorTransformChanges.RoleModel = gameObject.transform;
				}
				List<ColliderSync> list = new List<ColliderSync>();
				BoxCollider[] componentsInChildren = target.GetComponentsInChildren<BoxCollider>();
				foreach (BoxCollider boxCollider in componentsInChildren)
				{
					ColliderSync colliderSync = boxCollider.gameObject.AddComponent<ColliderSync>();
					if (List_0[Int_0].HaveCriticalTriggersCount)
					{
						List_0.Add(new GClass742());
						Int_0++;
					}
					colliderSync.Assign(List_0[Int_0], boxCollider);
					list.Add(colliderSync);
				}
				if (mirrorTransformChanges != null)
				{
					mirrorTransformChanges.colliderSyncInPhysics = list.ToArray();
				}
			}

			public int OverlapBoxNonAlloc(Vector3 center, Vector3 halfExtents, Collider[] results, Quaternion orientation, int mask, QueryTriggerInteraction queryTriggerInteraction)
			{
				if (!IsCreated)
				{
					UnityEngine.Debug.LogErrorFormat("World {0} not created", Name);
					return 0;
				}
				return smethod_0(PhysicsScene_0, center, halfExtents, results, orientation, mask, queryTriggerInteraction);
			}

			public void OverlapBoxNonAllocAsync(int index, Vector3 center, Vector3 halfExtents, Collider[] results, Quaternion orientation, int mask, QueryTriggerInteraction queryTriggerInteraction, GClass748 asyncData)
			{
				if (!IsCreated)
				{
					UnityEngine.Debug.LogErrorFormat("World {0} not created", Name);
					asyncData.OneWorldCompleteCallback(index, 0);
					return;
				}
				if (List_0.Count == 0)
				{
					int count = OverlapBoxNonAlloc(center, halfExtents, results, orientation, mask, queryTriggerInteraction);
					asyncData.OneWorldCompleteCallback(index, count);
					return;
				}
				foreach (GClass742 item in List_0)
				{
					item.BoxOverlap(index, center, halfExtents, orientation, results, mask, queryTriggerInteraction, asyncData);
				}
			}

			public int OverlapSphereNonAlloc(Vector3 position, float radius, Collider[] results, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
			{
				if (!IsCreated)
				{
					UnityEngine.Debug.LogErrorFormat("World {0} not created", Name);
					return 0;
				}
				return smethod_1(PhysicsScene_0, position, radius, results, layerMask, queryTriggerInteraction);
			}

			public int RaycastNonAlloc(Ray ray, RaycastHit[] results, [DefaultValue("Mathf.Infinity")] float maxDistance, [DefaultValue("DefaultRaycastLayers")] int layerMask, [DefaultValue("QueryTriggerInteraction.UseGlobal")] QueryTriggerInteraction queryTriggerInteraction)
			{
				return PhysicsScene_0.Raycast(ray.origin, ray.direction, results, maxDistance, layerMask, queryTriggerInteraction);
			}
		}

		public static GClass750 Default = new GClass750();

		public static GClass752 VolumePropagationAndEnvironmentSwitcherTriggers = new GClass752();

		public static GClass751 DisablerCullingObjectTriggers = new GClass751();

		[NonSerialized]
		public static List<GClass749> List_0 = new List<GClass749> { Default, VolumePropagationAndEnvironmentSwitcherTriggers, DisablerCullingObjectTriggers };

		public static int smethod_0(PhysicsScene physicsScene, Vector3 center, Vector3 halfExtents, Collider[] results, Quaternion orientation, int mask, QueryTriggerInteraction queryTriggerInteraction)
		{
			return physicsScene.OverlapBox(center, halfExtents, results, orientation, mask, queryTriggerInteraction);
		}

		public static int smethod_1(PhysicsScene physicsScene, Vector3 position, float radius, Collider[] results, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			return physicsScene.OverlapSphere(position, radius, results, layerMask, queryTriggerInteraction);
		}

		public static (Scene scene, PhysicsScene physicsScene) smethod_2(string sceneName)
		{
			CreateSceneParameters parameters = new CreateSceneParameters(LocalPhysicsMode.Physics3D);
			Scene scene = SceneManager.CreateScene(sceneName, parameters);
			PhysicsScene physicsScene = scene.GetPhysicsScene();
			return (scene: scene, physicsScene: physicsScene);
		}

		public static void Create()
		{
			foreach (GClass749 item in List_0)
			{
				item.Create();
			}
		}

		public static GClass748 OverlapBoxAsync(EWorldType worldTypeMask, Vector3 center, Vector3 halfExtents, Collider[][] buffers, Quaternion orientation, int mask, QueryTriggerInteraction queryTriggerInteraction, GClass748 complete)
		{
			if (List_0.Count == 0)
			{
				return GClass748.EmptyComplete;
			}
			complete.Reset();
			for (int i = 0; i < List_0.Count; i++)
			{
				GClass749 gClass = List_0[i];
				if (gClass.CanBeUsed(worldTypeMask, mask))
				{
					int totalWorldsCount = complete.TotalWorldsCount;
					complete.TotalWorldsCount++;
					gClass.OverlapBoxNonAllocAsync(totalWorldsCount, center, halfExtents, buffers[totalWorldsCount], orientation, mask, queryTriggerInteraction, complete);
				}
			}
			if (complete.TotalWorldsCount != 0)
			{
				return complete;
			}
			return GClass748.EmptyComplete;
		}

		public static int OverlapBoxNonAlloc(EWorldType worldTypeMask, Vector3 center, Vector3 halfExtents, Collider[] results, Collider[] tempBuffer, Quaternion orientation, int mask, QueryTriggerInteraction queryTriggerInteraction)
		{
			int num = 0;
			int num2 = 0;
			bool flag = worldTypeMask != 0 && (worldTypeMask & (worldTypeMask - 1)) == 0;
			for (int i = 0; i < List_0.Count; i++)
			{
				GClass749 gClass = List_0[i];
				if (!gClass.CanBeUsed(worldTypeMask, mask))
				{
					continue;
				}
				Collider[] results2 = (flag ? results : tempBuffer);
				int num3 = gClass.OverlapBoxNonAlloc(center, halfExtents, results2, orientation, mask, queryTriggerInteraction);
				num += num3;
				if (!flag)
				{
					for (int j = 0; j < num3; j++)
					{
						results[num2] = tempBuffer[j];
						num2++;
					}
				}
			}
			return num;
		}

		public static int OverlapSphereNonAlloc(EWorldType worldTypeMask, Vector3 position, float radius, Collider[] results, Collider[] tempBuffer, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			int num = 0;
			int num2 = 0;
			bool flag = worldTypeMask != 0 && (worldTypeMask & (worldTypeMask - 1)) == 0;
			for (int i = 0; i < List_0.Count; i++)
			{
				GClass749 gClass = List_0[i];
				if (!gClass.CanBeUsed(worldTypeMask, layerMask))
				{
					continue;
				}
				Collider[] results2 = (flag ? results : tempBuffer);
				int num3 = gClass.OverlapSphereNonAlloc(position, radius, results2, layerMask, queryTriggerInteraction);
				num += num3;
				if (!flag)
				{
					for (int j = 0; j < num3; j++)
					{
						results[num2] = tempBuffer[j];
						num2++;
					}
				}
			}
			return num;
		}
	}

	[NonSerialized]
	public static Dictionary<Collider, HashSet<Collider>> Dictionary_0 = new Dictionary<Collider, HashSet<Collider>>();

	[NonSerialized]
	public const int Int_0 = 8;

	[NonSerialized]
	public const float Float_0 = 0.001f;

	public static bool RebuildingStaticCollidersDetected => Class421.RebuildingStaticCollidersDetected;

	public static void IgnoreCollision(Collider collider1, Collider collider2, bool ignore = true)
	{
		Physics.IgnoreCollision(collider1, collider2, ignore);
		smethod_0(collider1, collider2, ignore);
		smethod_0(collider2, collider1, ignore);
	}

	public static void smethod_0(Collider collider1, Collider collider2, bool ignore)
	{
		HashSet<Collider> value = null;
		if (!Dictionary_0.TryGetValue(collider1, out value))
		{
			value = new HashSet<Collider>();
			Dictionary_0[collider1] = value;
		}
		if (ignore)
		{
			value.Add(collider2);
			return;
		}
		value.Remove(collider2);
		if (value.Count == 0)
		{
			Dictionary_0.Remove(collider1);
		}
	}

	public static bool GetIgnoreCollision(Collider collider1, Collider collider2)
	{
		HashSet<Collider> value = null;
		if (!Dictionary_0.TryGetValue(collider1, out value))
		{
			return false;
		}
		return value.Contains(collider2);
	}

	public static bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance, int layerMask)
	{
		Class421.BeginOperation();
		bool result = Physics.Raycast(ray, out hitInfo, maxDistance, layerMask);
		Class421.EndOperation();
		return result;
	}

	public static bool SphereCast(Ray ray, float radius, out RaycastHit hitInfo, float maxDistance, int layerMask)
	{
		Class421.BeginOperation();
		bool result = Physics.SphereCast(ray, radius, out hitInfo, maxDistance, layerMask);
		Class421.EndOperation();
		return result;
	}

	public static int OverlapSphereNonAlloc(Vector3 position, float radius, Collider[] results, [DefaultValue("AllLayers")] int layerMask, [DefaultValue("QueryTriggerInteraction.UseGlobal")] QueryTriggerInteraction queryTriggerInteraction)
	{
		Class421.BeginOperation();
		int result = Physics.OverlapSphereNonAlloc(position, radius, results, layerMask, queryTriggerInteraction);
		Class421.EndOperation();
		return result;
	}

	public static int OverlapBoxNonAlloc(Vector3 center, Vector3 halfExtents, Collider[] results, [DefaultValue("Quaternion.identity")] Quaternion orientation, [DefaultValue("AllLayers")] int layerMask, [DefaultValue("QueryTriggerInteraction.UseGlobal")] QueryTriggerInteraction queryTriggerInteraction)
	{
		Class421.BeginOperation();
		int result = Physics.OverlapBoxNonAlloc(center, halfExtents, results, orientation, layerMask, queryTriggerInteraction);
		Class421.EndOperation();
		return result;
	}

	public static void FixedUpdate()
	{
		SyncTransformsClass.FixedUpdate();
	}

	public static void Update()
	{
		SyncTransformsClass.Update();
		GClass745.Update();
	}

	public static bool LinecastInBothSides(Vector3 start, Vector3 end, out RaycastHit bestHit, out bool isForwardHit, LayerMask forwardLayerMask, LayerMask backwardLayerMask, RaycastHit[] raycastHits, Func<RaycastHit, bool> isHitIgnoredTest)
	{
		RaycastHit bestHit2 = default(RaycastHit);
		bool flag = false;
		if ((int)forwardLayerMask != 0)
		{
			flag = Linecast(start, end, out bestHit2, forwardLayerMask, reverseCheck: false, raycastHits, isHitIgnoredTest);
		}
		RaycastHit bestHit3 = default(RaycastHit);
		bool flag2 = false;
		if ((int)backwardLayerMask != 0)
		{
			flag2 = LinecastPrecise(end, start, out bestHit3, backwardLayerMask, reverseCheck: true, raycastHits, isHitIgnoredTest);
		}
		bool result = false;
		isForwardHit = true;
		bestHit = default(RaycastHit);
		if (flag && flag2)
		{
			result = true;
			float distance = bestHit2.distance;
			float magnitude = (bestHit3.point - start).magnitude;
			if (distance < magnitude)
			{
				isForwardHit = true;
				bestHit = bestHit2;
			}
			else
			{
				isForwardHit = false;
				bestHit = bestHit3;
			}
		}
		else if (flag)
		{
			result = true;
			isForwardHit = true;
			bestHit = bestHit2;
		}
		else if (flag2)
		{
			result = true;
			isForwardHit = false;
			bestHit = bestHit3;
		}
		return result;
	}

	public static bool LinecastPrecise(Vector3 start, Vector3 end, out RaycastHit bestHit, LayerMask layerMask, bool reverseCheck, RaycastHit[] raycastHits, Func<RaycastHit, bool> isHitIgnoredTest)
	{
		if (!reverseCheck)
		{
			return Linecast(start, end, out bestHit, layerMask, reverseCheck, raycastHits, isHitIgnoredTest);
		}
		bool result = false;
		bestHit = default(RaycastHit);
		Vector3 vector = default(Vector3);
		Vector3 start2 = start;
		for (int i = 0; i < 8; i++)
		{
			bool flag;
			if (!(flag = Linecast(start2, end, out var bestHit2, layerMask, reverseCheck, raycastHits, isHitIgnoredTest)))
			{
				break;
			}
			result = flag;
			bestHit = bestHit2;
			if (!(bestHit2.collider is MeshCollider))
			{
				break;
			}
			if (i == 0)
			{
				vector = (end - start).normalized * 0.001f;
			}
			start2 = bestHit2.point + vector;
		}
		return result;
	}

	public static bool Linecast(Vector3 start, Vector3 end, out RaycastHit bestHit, LayerMask layerMask, bool reverseCheck, RaycastHit[] raycastHits, Func<RaycastHit, bool> isHitIgnoredTest)
	{
		Vector3 direction = end - start;
		float magnitude = direction.magnitude;
		int num = Physics.RaycastNonAlloc(new Ray(start, direction), raycastHits, magnitude, layerMask);
		float num2 = (reverseCheck ? float.MinValue : float.MaxValue);
		bestHit = default(RaycastHit);
		bool result = false;
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = raycastHits[i];
			if ((isHitIgnoredTest == null || !isHitIgnoredTest(raycastHit)) && (reverseCheck ? (raycastHit.distance > num2) : (raycastHit.distance < num2)))
			{
				num2 = raycastHit.distance;
				bestHit = raycastHit;
				result = true;
			}
		}
		return result;
	}

	public static bool Linecast(Vector3 start, Vector3 end, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction, out RaycastHit hit)
	{
		Vector3 vector = end - start;
		return Physics.Raycast(new Ray(start, vector.normalized), out hit, vector.magnitude, layerMask, queryTriggerInteraction);
	}

	public static void SyncTransforms()
	{
		SyncTransformsClass.SyncTransforms();
	}

	public static void ForceSyncTransforms()
	{
		SyncTransformsClass.ForceSyncTransforms();
	}
}
