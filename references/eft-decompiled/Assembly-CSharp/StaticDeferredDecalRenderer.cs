using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Comfort.Common;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[GAttribute8(20010)]
public class StaticDeferredDecalRenderer : MonoBehaviour
{
	public class GClass970
	{
		public string Hash;

		public Material ParentMaterial;

		public Material OwnMaterial;

		public int DecalQueue;

		public List<StaticDeferredDecal> Decals;

		public Bounds Bounds;

		public GClass970(string hash, Material decalMaterial, int decalQueue, List<StaticDeferredDecal> decals, Bounds initialBounds)
		{
			Hash = hash;
			ParentMaterial = decalMaterial;
			OwnMaterial = new Material(ParentMaterial);
			DecalQueue = decalQueue;
			Decals = decals;
			Bounds = initialBounds;
		}
	}

	public class Class606
	{
		public CommandBuffer BeginCB;

		public CommandBuffer EndCB;

		public HashSet<CommandBuffer> DrawCBs = new HashSet<CommandBuffer>();
	}

	public int DecalsGridSize = 25;

	private GClass2380 gclass2380_0 = new GClass2380();

	public bool UseQuadTreeRenderInEditor;

	public bool UseQuadTreeRenderInPlayMode;

	public bool UseTexturesQuadTreeInEditor;

	public bool UseTexturesQuadTreeInPlayMode = true;

	public bool QuadTreeRecordPerFrame;

	public bool UseTreeAsync = true;

	private bool bool_0;

	private bool bool_1;

	public int TreeCmdBufMaxDepth = 5;

	public int TreeTexturesMaxDepth = 10;

	public int TreeThreadSleepTime = 100;

	private float float_0 = 1f;

	private float float_1 = 1f;

	[CompilerGenerated]
	private static StaticDeferredDecalRenderer staticDeferredDecalRenderer_0;

	private int int_0;

	private const int int_1 = 60000;

	private int int_2;

	private bool bool_2;

	private readonly GClass818<string, GClass970> gclass818_0 = new GClass818<string, GClass970>();

	private readonly SortedList<string, GClass968> sortedList_0 = new SortedList<string, GClass968>();

	private readonly Dictionary<Camera, Class606> dictionary_0 = new Dictionary<Camera, Class606>();

	private static readonly int int_3 = Shader.PropertyToID("_BumpMap");

	private static readonly int int_4 = Shader.PropertyToID("decalID");

	private Mesh mesh_0;

	private GClass966 gclass966_0;

	private Dictionary<Texture2D, GClass967> dictionary_1;

	private List<GClass967> list_0;

	public int UpdateTexturesPerFrame = 10;

	private int int_5 = -1;

	private static readonly int[] int_6 = new int[36]
	{
		3, 1, 0, 3, 2, 1, 7, 5, 4, 7,
		6, 5, 11, 9, 8, 11, 10, 9, 15, 13,
		12, 15, 14, 13, 19, 17, 16, 19, 18, 17,
		23, 21, 20, 23, 22, 21
	};

	private static readonly Vector3[] vector3_0 = new Vector3[24]
	{
		new Vector3(-0.5f, -0.5f, 0.5f),
		new Vector3(0.5f, -0.5f, 0.5f),
		new Vector3(0.5f, -0.5f, -0.5f),
		new Vector3(-0.5f, -0.5f, -0.5f),
		new Vector3(-0.5f, 0.5f, -0.5f),
		new Vector3(-0.5f, 0.5f, 0.5f),
		new Vector3(-0.5f, -0.5f, 0.5f),
		new Vector3(-0.5f, -0.5f, -0.5f),
		new Vector3(-0.5f, 0.5f, 0.5f),
		new Vector3(0.5f, 0.5f, 0.5f),
		new Vector3(0.5f, -0.5f, 0.5f),
		new Vector3(-0.5f, -0.5f, 0.5f),
		new Vector3(0.5f, 0.5f, -0.5f),
		new Vector3(-0.5f, 0.5f, -0.5f),
		new Vector3(-0.5f, -0.5f, -0.5f),
		new Vector3(0.5f, -0.5f, -0.5f),
		new Vector3(0.5f, 0.5f, 0.5f),
		new Vector3(0.5f, 0.5f, -0.5f),
		new Vector3(0.5f, -0.5f, -0.5f),
		new Vector3(0.5f, -0.5f, 0.5f),
		new Vector3(-0.5f, 0.5f, -0.5f),
		new Vector3(0.5f, 0.5f, -0.5f),
		new Vector3(0.5f, 0.5f, 0.5f),
		new Vector3(-0.5f, 0.5f, 0.5f)
	};

	private readonly RenderTargetIdentifier[] renderTargetIdentifier_0 = new RenderTargetIdentifier[4]
	{
		BuiltinRenderTextureType.GBuffer0,
		BuiltinRenderTextureType.GBuffer1,
		BuiltinRenderTextureType.GBuffer2,
		BuiltinRenderTextureType.CameraTarget
	};

	private static readonly int int_7 = Shader.PropertyToID("_NormalsCopy");

	private Vector4[] vector4_0 = new Vector4[6];

	private Plane[] plane_0 = new Plane[6];

	private Vector3[] vector3_1 = new Vector3[8];

	public static StaticDeferredDecalRenderer Instance
	{
		[CompilerGenerated]
		get
		{
			return staticDeferredDecalRenderer_0;
		}
		[CompilerGenerated]
		set
		{
			staticDeferredDecalRenderer_0 = value;
		}
	}

	public void Awake()
	{
		int_2 = 0;
		method_1();
		method_11();
		method_13();
		method_10();
		int_0 = DecalsGridSize;
		if (Application.isPlaying)
		{
			if (Singleton<SharedGameSettingsClass>.Instantiated)
			{
				bool flag = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.MipStreaming;
				UseTexturesQuadTreeInPlayMode &= flag;
			}
			bool_0 = UseQuadTreeRenderInPlayMode;
			bool_1 = UseTexturesQuadTreeInPlayMode;
		}
		else
		{
			bool_0 = UseQuadTreeRenderInEditor;
			bool_1 = UseTexturesQuadTreeInEditor;
		}
		Instance = this;
	}

	public void LateUpdate()
	{
		bool flag = (Application.isPlaying ? UseQuadTreeRenderInPlayMode : UseQuadTreeRenderInEditor);
		bool flag2 = (Application.isPlaying ? UseTexturesQuadTreeInPlayMode : UseTexturesQuadTreeInEditor);
		if (DecalsGridSize != int_0 || flag != bool_0 || flag2 != bool_1)
		{
			OnDestroy();
			Awake();
			Refresh();
		}
	}

	public void method_0()
	{
		gclass966_0?.Dispose();
		gclass966_0 = null;
		if (list_0 != null)
		{
			list_0.Clear();
			list_0 = null;
		}
		if (dictionary_1 == null)
		{
			return;
		}
		foreach (KeyValuePair<Texture2D, GClass967> item in dictionary_1)
		{
			item.Value.Dispose();
		}
		dictionary_1.Clear();
		dictionary_1 = null;
	}

	public void OnDestroy()
	{
		method_12();
		method_14();
		method_21();
		method_0();
		method_23();
		method_3();
	}

	public void method_1()
	{
		if (mesh_0 != null)
		{
			UnityEngine.Object.DestroyImmediate(mesh_0);
		}
		mesh_0 = new Mesh
		{
			name = "StaticDeferredDecal DecalMesh"
		};
		mesh_0.vertices = vector3_0;
		mesh_0.triangles = int_6;
	}

	public Mesh method_2()
	{
		if (mesh_0 == null)
		{
			method_1();
		}
		return mesh_0;
	}

	public void method_3()
	{
		if (mesh_0 != null)
		{
			UnityEngine.Object.DestroyImmediate(mesh_0);
		}
	}

	public void method_4(StaticDeferredDecal decal, bool updateInstances)
	{
		if (int_2 < 60000 && decal.enabled && decal.gameObject.activeInHierarchy)
		{
			method_8(decal, updateInstances);
		}
	}

	public void method_5(StaticDeferredDecal decal, bool updateInstances)
	{
		method_9(decal, updateInstances);
	}

	public string method_6(StaticDeferredDecal decal)
	{
		string text = decal.DecalQueue.ToString();
		Texture mainTexture = decal.DecalMaterial.mainTexture;
		if ((bool)mainTexture)
		{
			text = text + mainTexture.width + mainTexture.height + (mainTexture as Texture2D).format;
		}
		if (decal.DecalMaterial.HasProperty(int_3))
		{
			Texture texture = decal.DecalMaterial.GetTexture(int_3);
			if ((bool)texture)
			{
				text = text + texture.width + texture.height + (texture as Texture2D).format;
			}
		}
		return text;
	}

	public void method_7(StaticDeferredDecal decal, ref Vector3 minPoint, ref Vector3 maxPoint)
	{
		Vector3[] vertices = method_2().vertices;
		Matrix4x4 localToWorldMatrix = decal.transform.localToWorldMatrix;
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i] = localToWorldMatrix.MultiplyPoint(vertices[i]);
		}
		Vector3 vector = vertices[0];
		Vector3 vector2 = vertices[0];
		Vector3[] array = vertices;
		foreach (Vector3 rhs in array)
		{
			vector = Vector3.Min(vector, rhs);
			vector2 = Vector3.Max(vector2, rhs);
		}
		minPoint = vector;
		maxPoint = vector2;
	}

	public void method_8(StaticDeferredDecal decal, bool updateInstances)
	{
		string text = method_6(decal);
		Vector3 minPoint = Vector3.zero;
		Vector3 maxPoint = Vector3.zero;
		method_7(decal, ref minPoint, ref maxPoint);
		if (!gclass818_0.ContainsKey(text))
		{
			Vector3 center = (maxPoint + minPoint) * 0.5f;
			Vector3 size = maxPoint - minPoint;
			GClass970 value = new GClass970(text, decal.DecalMaterial, decal.DecalQueue, new List<StaticDeferredDecal>(), new Bounds(center, size));
			gclass818_0.Add(text, value);
		}
		GClass970 byKey = gclass818_0.GetByKey(text);
		List<StaticDeferredDecal> decals = byKey.Decals;
		if (!decals.Contains(decal))
		{
			decals.Add(decal);
			byKey.Bounds.Encapsulate(minPoint);
			byKey.Bounds.Encapsulate(maxPoint);
			int_2++;
			if (updateInstances)
			{
				bool_2 = true;
			}
		}
	}

	public void method_9(StaticDeferredDecal decal, bool updateInstances)
	{
		string key = method_6(decal);
		if (!gclass818_0.ContainsKey(key))
		{
			return;
		}
		GClass970 byKey = gclass818_0.GetByKey(key);
		List<StaticDeferredDecal> decals = gclass818_0.GetByKey(key).Decals;
		if (!decals.Contains(decal))
		{
			return;
		}
		decals.Remove(decal);
		int_2--;
		if (decals.Count == 0)
		{
			gclass818_0.Remove(key);
			if (sortedList_0.ContainsKey(key))
			{
				sortedList_0[key].DestroyResources();
				sortedList_0.Remove(key);
			}
			byKey.Bounds.SetMinMax(Vector3.zero, Vector3.zero);
		}
		else
		{
			Vector3 minPoint = Vector3.zero;
			Vector3 maxPoint = Vector3.zero;
			method_7(decals[0], ref minPoint, ref maxPoint);
			byKey.Bounds = new Bounds((maxPoint + minPoint) * 0.5f, maxPoint - minPoint);
			foreach (StaticDeferredDecal decal2 in byKey.Decals)
			{
				method_7(decal2, ref minPoint, ref maxPoint);
				byKey.Bounds.Encapsulate(minPoint);
				byKey.Bounds.Encapsulate(maxPoint);
			}
		}
		if (updateInstances)
		{
			bool_2 = true;
		}
	}

	public GClass968[] GetDecalDrawInstancesArray()
	{
		GClass968[] array = new GClass968[sortedList_0.Count];
		int num = 0;
		foreach (KeyValuePair<string, GClass968> item in sortedList_0)
		{
			array[num] = item.Value;
			num++;
		}
		return array;
	}

	public void method_10()
	{
		StaticDeferredDecal[] array = GClass870.FindUnityObjectsOfType<StaticDeferredDecal>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].DecalMaterial != null)
			{
				method_4(array[i], updateInstances: false);
			}
		}
	}

	public void method_11()
	{
		StaticDeferredDecal.OnDecalRegister += method_4;
		StaticDeferredDecal.OnDecalUnregister += method_5;
	}

	public void method_12()
	{
		StaticDeferredDecal.OnDecalRegister -= method_4;
		StaticDeferredDecal.OnDecalUnregister -= method_5;
	}

	public void method_13()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(method_15));
	}

	public void method_14()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(method_15));
	}

	public void method_15(Camera currentCamera)
	{
		if (sortedList_0.Count == 0 || currentCamera == null || !EFTHardSettings.Instance.STATIC_DEFERRED_DECALS_ENABLED)
		{
			return;
		}
		bool flag = currentCamera.CompareTag("OpticCamera");
		if (currentCamera.CompareTag("MainCamera") || flag || !Application.isPlaying)
		{
			method_22(currentCamera);
			if (!dictionary_0.ContainsKey(currentCamera))
			{
				method_16(currentCamera);
			}
			method_19(currentCamera);
		}
	}

	public void method_16(Camera camera)
	{
		Class606 @class = new Class606();
		@class.BeginCB = new CommandBuffer();
		@class.BeginCB.name = "Static decals begin";
		camera.AddCommandBuffer(CameraEvent.BeforeReflections, @class.BeginCB);
		@class.EndCB = new CommandBuffer();
		@class.EndCB.name = "Static decals end";
		camera.AddCommandBuffer(CameraEvent.BeforeReflections, @class.EndCB);
		dictionary_0.Add(camera, @class);
	}

	public void UpdateInstancesBuffers()
	{
		bool_2 = false;
		Mesh mesh = method_2();
		List<GClass968> list = new List<GClass968>();
		List<List<StaticDeferredDecal>> list2 = new List<List<StaticDeferredDecal>>();
		List<Bounds> list3 = new List<Bounds>();
		int num = 0;
		for (int i = 0; i < gclass818_0.Count; i++)
		{
			GClass970 byIndex = gclass818_0.GetByIndex(i);
			if (!sortedList_0.ContainsKey(byIndex.Hash))
			{
				GClass968 gClass = new GClass968();
				Material material = ((byIndex.DecalQueue == 0) ? byIndex.OwnMaterial : new Material(byIndex.OwnMaterial));
				gClass.InitInstance(material, byIndex.DecalQueue != 0, byIndex.Decals.Count, mesh);
				sortedList_0.Add(byIndex.Hash, gClass);
			}
			sortedList_0[byIndex.Hash].SetStartIndex(num);
			num += byIndex.Decals.Count;
			list.Add(sortedList_0[byIndex.Hash]);
			list2.Add(byIndex.Decals);
			list3.Add(byIndex.Bounds);
		}
		GClass968.InitGlobalBuffers(num);
		for (int j = 0; j < list.Count; j++)
		{
			list[j].UpdateBuffers(list2[j], mesh, list3[j]);
		}
		if (Application.isPlaying ? (UseQuadTreeRenderInPlayMode | UseTexturesQuadTreeInPlayMode) : (UseQuadTreeRenderInEditor | UseTexturesQuadTreeInEditor))
		{
			method_17();
		}
	}

	public void method_17()
	{
		if (gclass818_0.Count == 0)
		{
			return;
		}
		method_21();
		method_0();
		gclass966_0 = new GClass966();
		Bounds bounds = new Bounds(gclass818_0.GetByIndex(0).Bounds.center, gclass818_0.GetByIndex(0).Bounds.extents);
		for (int i = 1; i < gclass818_0.Count; i++)
		{
			GClass970 byIndex = gclass818_0.GetByIndex(i);
			bounds.Encapsulate(byIndex.Bounds);
		}
		gclass966_0.Bounds = new Rect(bounds.min.x, bounds.min.z, bounds.size.x, bounds.size.z);
		gclass966_0.SizeLimit = DecalsGridSize;
		Mesh mesh = method_2();
		for (int j = 0; j < gclass818_0.Count; j++)
		{
			foreach (StaticDeferredDecal decal in gclass818_0.GetByIndex(j).Decals)
			{
				gclass966_0.Insert(decal, mesh);
			}
		}
		dictionary_1 = gclass966_0.ConstructCommandBuffersAndTexturesTrees(mesh, method_20);
		list_0 = new List<GClass967>();
		foreach (KeyValuePair<Texture2D, GClass967> item in dictionary_1)
		{
			list_0.Add(item.Value);
		}
	}

	public bool method_18(GClass968 drawInstance)
	{
		vector3_1[0] = drawInstance.Bounds.center + new Vector3(drawInstance.Bounds.extents.x, drawInstance.Bounds.extents.y, drawInstance.Bounds.extents.z);
		vector3_1[1] = drawInstance.Bounds.center + new Vector3(drawInstance.Bounds.extents.x, drawInstance.Bounds.extents.y, 0f - drawInstance.Bounds.extents.z);
		vector3_1[2] = drawInstance.Bounds.center + new Vector3(drawInstance.Bounds.extents.x, 0f - drawInstance.Bounds.extents.y, drawInstance.Bounds.extents.z);
		vector3_1[3] = drawInstance.Bounds.center + new Vector3(drawInstance.Bounds.extents.x, 0f - drawInstance.Bounds.extents.y, 0f - drawInstance.Bounds.extents.z);
		vector3_1[4] = drawInstance.Bounds.center + new Vector3(0f - drawInstance.Bounds.extents.x, drawInstance.Bounds.extents.y, drawInstance.Bounds.extents.z);
		vector3_1[5] = drawInstance.Bounds.center + new Vector3(0f - drawInstance.Bounds.extents.x, drawInstance.Bounds.extents.y, 0f - drawInstance.Bounds.extents.z);
		vector3_1[6] = drawInstance.Bounds.center + new Vector3(0f - drawInstance.Bounds.extents.x, 0f - drawInstance.Bounds.extents.y, drawInstance.Bounds.extents.z);
		vector3_1[7] = drawInstance.Bounds.center + new Vector3(0f - drawInstance.Bounds.extents.x, 0f - drawInstance.Bounds.extents.y, 0f - drawInstance.Bounds.extents.z);
		int num = 0;
		while (true)
		{
			if (num < 6)
			{
				int num2 = 0;
				for (int i = 0; i < 8; i++)
				{
					if (Vector4.Dot(vector4_0[num], new Vector4(vector3_1[i].x, vector3_1[i].y, vector3_1[i].z, 1f)) < 0f)
					{
						num2++;
					}
				}
				if (num2 == 8)
				{
					break;
				}
				num++;
				continue;
			}
			return true;
		}
		return false;
	}

	public void method_19(Camera currentCamera)
	{
		Class606 @class = dictionary_0[currentCamera];
		@class.BeginCB.Clear();
		@class.EndCB.Clear();
		@class.BeginCB.GetTemporaryRT(int_7, -1, -1);
		@class.BeginCB.Blit(BuiltinRenderTextureType.GBuffer2, int_7);
		@class.BeginCB.SetRenderTarget(renderTargetIdentifier_0, BuiltinRenderTextureType.CameraTarget);
		currentCamera.AddCommandBuffer(CameraEvent.BeforeReflections, @class.BeginCB);
		Mesh mesh = method_2();
		gclass2380_0.Update(currentCamera);
		gclass2380_0.GetPlanes(plane_0);
		for (int i = 0; i < 6; i++)
		{
			vector4_0[i].x = plane_0[i].normal.x;
			vector4_0[i].y = plane_0[i].normal.y;
			vector4_0[i].z = plane_0[i].normal.z;
			vector4_0[i].w = plane_0[i].distance;
		}
		bool flag = (Application.isPlaying ? UseQuadTreeRenderInPlayMode : UseQuadTreeRenderInEditor);
		bool flag2 = (Application.isPlaying ? UseTexturesQuadTreeInPlayMode : UseTexturesQuadTreeInEditor);
		for (int j = 0; j < sortedList_0.Values.Count; j++)
		{
			GClass968 gClass = sortedList_0.Values[j];
			if (!flag)
			{
				gClass.Material.SetInt(int_4, gClass.StartIndex);
				@class.BeginCB.DrawMeshInstancedIndirect(mesh, 0, gClass.Material, -1, gClass.ArgsBuffer);
			}
		}
		GClass966.ThreadSkipJob = !flag;
		GClass966.ThreadSleepTime = TreeThreadSleepTime;
		GClass967.ThreadSkipJob = !flag2;
		GClass967.ThreadSleepTime = TreeThreadSleepTime;
		if (flag2 && dictionary_1 != null)
		{
			float val = (float)currentCamera.pixelWidth * currentCamera.nearClipPlane;
			float val2 = (float)currentCamera.pixelHeight * currentCamera.nearClipPlane;
			float_0 = Math.Max(float_0, val);
			float_1 = Math.Max(float_1, val2);
			if (list_0 != null)
			{
				for (int k = 0; k < UpdateTexturesPerFrame; k++)
				{
					int_5 = (int_5 + 1) % list_0.Count;
					list_0[int_5].SetThreadParams(currentCamera, gclass2380_0, float_0, float_1);
					list_0[int_5].UpdateTextureRequestedMipMapLevel();
				}
			}
		}
		if (flag && gclass966_0 != null)
		{
			gclass966_0.SetThreadParams(gclass2380_0, TreeCmdBufMaxDepth);
			if (UseTreeAsync && !QuadTreeRecordPerFrame)
			{
				List<CommandBuffer> commandBuffers = new List<CommandBuffer>();
				if (gclass966_0.GetThreadResults(ref commandBuffers))
				{
					foreach (CommandBuffer item in commandBuffers)
					{
						currentCamera.AddCommandBuffer(CameraEvent.BeforeReflections, item);
						@class.DrawCBs.Add(item);
					}
				}
			}
			else if (QuadTreeRecordPerFrame)
			{
				Mesh mesh2 = method_2();
				gclass966_0.RecordCommandBuffer(gclass2380_0, @class.EndCB, TreeCmdBufMaxDepth, method_20, mesh2);
			}
			else
			{
				List<CommandBuffer> commandBuffers2 = new List<CommandBuffer>();
				gclass966_0.GetCommandBuffers(gclass2380_0, ref commandBuffers2, TreeCmdBufMaxDepth);
				foreach (CommandBuffer item2 in commandBuffers2)
				{
					currentCamera.AddCommandBuffer(CameraEvent.BeforeReflections, item2);
					@class.DrawCBs.Add(item2);
				}
			}
		}
		currentCamera.AddCommandBuffer(CameraEvent.BeforeReflections, @class.EndCB);
		@class.EndCB.ReleaseTemporaryRT(int_7);
	}

	public void method_20(CommandBuffer cmdBuf)
	{
		cmdBuf.SetRenderTarget(renderTargetIdentifier_0, BuiltinRenderTextureType.CameraTarget);
	}

	public void method_21()
	{
		foreach (Camera key in dictionary_0.Keys)
		{
			if (key != null)
			{
				method_22(key);
			}
		}
		dictionary_0.Clear();
		float_0 = 1f;
		float_1 = 1f;
	}

	public void method_22(Camera cam)
	{
		if (!dictionary_0.TryGetValue(cam, out var value))
		{
			return;
		}
		cam.RemoveCommandBuffer(CameraEvent.BeforeReflections, value.BeginCB);
		cam.RemoveCommandBuffer(CameraEvent.BeforeReflections, value.EndCB);
		foreach (CommandBuffer drawCB in value.DrawCBs)
		{
			cam.RemoveCommandBuffer(CameraEvent.BeforeReflections, drawCB);
		}
		value.DrawCBs.Clear();
	}

	public void method_23()
	{
		gclass818_0.Clear();
		foreach (KeyValuePair<string, GClass968> item in sortedList_0)
		{
			item.Value.DestroyResources();
		}
		sortedList_0.Clear();
	}

	public void ReInitialize()
	{
		method_14();
		method_21();
		method_23();
		Awake();
		UpdateInstancesBuffers();
	}

	public void Refresh()
	{
	}

	public void method_24()
	{
	}

	public void OnDrawGizmosSelected()
	{
	}
}
