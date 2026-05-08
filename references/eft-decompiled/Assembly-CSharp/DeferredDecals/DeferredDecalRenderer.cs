using System;
using System.Collections.Generic;
using System.Linq;
using EFT.Ballistics;
using UnityEngine;
using UnityEngine.Rendering;

namespace DeferredDecals;

public class DeferredDecalRenderer : MonoBehaviour
{
	public class DeferredDecalMeshDataClass : IDisposable
	{
		public readonly int VertexCount;

		public readonly Mesh Mesh;

		public readonly Vector3[] Vertices;

		public readonly Vector3[] Normals;

		public readonly Vector4[] Tangents;

		public readonly List<Vector4> Uvs0;

		public readonly List<Vector4> Uvs1;

		public readonly List<Vector4> Uvs2;

		[NonSerialized]
		public static int[] Int_0;

		[NonSerialized]
		public static int Int_1;

		[NonSerialized]
		public const int Int_2 = 10000;

		public DeferredDecalMeshDataClass(int nDecals)
		{
			int num = nDecals * 24;
			Vertices = new Vector3[num];
			Normals = new Vector3[num];
			Tangents = new Vector4[num];
			Uvs0 = new List<Vector4>(num);
			Uvs1 = new List<Vector4>(num);
			Uvs2 = new List<Vector4>(num);
			for (int i = 0; i < num; i++)
			{
				Uvs0.Add(GClass842.Vector4Zero);
				Uvs1.Add(GClass842.Vector4Zero);
				Uvs2.Add(GClass842.Vector4Zero);
			}
			if (Int_0 == null || Int_1 != nDecals)
			{
				Int_0 = new int[nDecals * 36];
				DeferredDecalMeshHelper.GenerateTrigs(Int_0);
			}
			Int_1 = nDecals;
			Mesh = new Mesh
			{
				name = "DeferredDecalRenderer ManagedMesh"
			};
			Mesh.MarkDynamic();
			UpdateInnerMesh();
			Mesh.triangles = Int_0;
			Mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
		}

		public void UpdateInnerMesh()
		{
			Mesh.vertices = Vertices;
			Mesh.normals = Normals;
			Mesh.tangents = Tangents;
			Mesh.SetUVs(0, Uvs0);
			Mesh.SetUVs(1, Uvs1);
			Mesh.SetUVs(2, Uvs2);
		}

		public void PasteProjectorIntoMiddle(int startvIndex, Vector3[] newDecalVerts, Vector4[] newDecalTangents, Vector3[] newDecalNormals, Vector4[] decalUv0Right, Vector4[] decalUv1Up, Vector4[] decalUv2Fwd)
		{
			for (int i = startvIndex; i < startvIndex + 24; i++)
			{
				Vertices[i] = newDecalVerts[i - startvIndex];
				Tangents[i] = newDecalTangents[i - startvIndex];
				Normals[i] = newDecalNormals[i - startvIndex];
				Uvs0[i] = decalUv0Right[i - startvIndex];
				Uvs1[i] = decalUv1Up[i - startvIndex];
				Uvs2[i] = decalUv2Fwd[i - startvIndex];
			}
			UpdateInnerMesh();
		}

		public void Dispose()
		{
			UnityEngine.Object.DestroyImmediate(Mesh);
		}
	}

	[Serializable]
	public class SingleDecal
	{
		public bool RandomizeRotation;

		[GAttribute6(0f, 2f, -1f)]
		public Vector2 DecalSize = new Vector2(0.1f, 0.15f);

		[Range(1f, 10f)]
		public int TileSheetRows = 1;

		[Range(1f, 10f)]
		public int TileSheetColumns = 1;

		public MaterialType[] DecalMaterialType;

		public Material DecalMaterial;

		public Material DynamicDecalMaterial;

		[HideInInspector]
		public float TileUSize;

		[HideInInspector]
		public float TileVSize;

		[HideInInspector]
		public bool IsTiled;

		public void Init()
		{
			TileUSize = 1f / (float)TileSheetColumns;
			TileVSize = 1f / (float)TileSheetRows;
			IsTiled = TileSheetColumns != 1 || TileSheetRows != 1;
		}
	}

	public class DeferredDecalBufferClass
	{
		public CommandBuffer StaticDecalsBuffer;

		public CommandBuffer DynamicDecalsBuffer;

		public CullingGroup CullGroup;

		public bool IsStaticBufferDirty = true;

		public bool IsDynamicBufferDirty = true;
	}

	[SerializeField]
	[Range(0.001f, 3f)]
	private float _decalProjectorHeight = 0.5f;

	[SerializeField]
	[Range(0.001f, 3f)]
	private float _decalProjectorHeightForGrenade = 0.7f;

	[Range(1f, 1024f)]
	[SerializeField]
	private int _maxDecals = 500;

	[Range(1f, 1024f)]
	[SerializeField]
	private int _maxDynamicDecals = 500;

	[Range(1f, 1000f)]
	[SerializeField]
	private int _cullDistance = 100;

	[SerializeField]
	private SingleDecal[] _decals;

	[SerializeField]
	private SingleDecal _environmentBlood;

	[SerializeField]
	private SingleDecal _bleedingDecal;

	[SerializeField]
	private SingleDecal _grenadeDecal;

	[SerializeField]
	private SingleDecal _genericDecal;

	[SerializeField]
	private Mesh _cube;

	private readonly Queue<DeferredDecalMeshDataClass> queue_0 = new Queue<DeferredDecalMeshDataClass>();

	private readonly Dictionary<Material, DeferredDecalMeshDataClass> dictionary_0 = new Dictionary<Material, DeferredDecalMeshDataClass>();

	private readonly Dictionary<MaterialType, SingleDecal> dictionary_1 = GClass866<MaterialType>.GetDictWith<SingleDecal>();

	private int int_0;

	private int int_1;

	private int int_2;

	private Dictionary<Camera, DeferredDecalBufferClass> dictionary_2 = new Dictionary<Camera, DeferredDecalBufferClass>();

	private BoundingSphere[] boundingSphere_0;

	private int int_3;

	private List<DynamicDeferredDecalRenderer> list_0;

	private Vector4[] vector4_0;

	private Vector4[] vector4_1;

	private Vector4[] vector4_2;

	private Vector3[] vector3_0;

	private Vector3[] vector3_1;

	private Vector4[] vector4_3;

	private Transform transform_0;

	private const float float_0 = 0.4f;

	private CommandBuffer commandBuffer_0;

	private CommandBuffer commandBuffer_1;

	private bool bool_0;

	public void Awake()
	{
		int_1 = 24 * _maxDecals;
		int_2 = Shader.PropertyToID("_NormalsCopy");
		_environmentBlood.Init();
		_bleedingDecal.Init();
		_genericDecal.Init();
		_grenadeDecal.Init();
		SingleDecal[] decals = _decals;
		foreach (SingleDecal singleDecal in decals)
		{
			singleDecal.Init();
			MaterialType[] decalMaterialType = singleDecal.DecalMaterialType;
			foreach (MaterialType key in decalMaterialType)
			{
				dictionary_1.Add(key, singleDecal);
			}
		}
		list_0 = new List<DynamicDeferredDecalRenderer>(_maxDynamicDecals);
		boundingSphere_0 = new BoundingSphere[_maxDynamicDecals];
		int_3 = 0;
		vector4_0 = new Vector4[24];
		vector4_1 = new Vector4[24];
		vector4_2 = new Vector4[24];
		vector3_0 = new Vector3[24];
		vector3_1 = new Vector3[24];
		vector4_3 = new Vector4[24];
		transform_0 = new GameObject("dynamic_decals_parent").transform;
		transform_0.position = Vector3.zero;
		transform_0.rotation = Quaternion.identity;
		UnityEngine.Object.DontDestroyOnLoad(transform_0);
		method_1(dictionary_1.Count);
		method_3();
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(OnPreCullCameraRender));
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(OnPreCameraRender));
	}

	public void DrawDecal(RaycastHit hit, BallisticCollider hitCollider)
	{
		DrawDecal(hit.point, hit.normal, hitCollider, isGrenade: false);
	}

	public void SetMaxStaticDecals(int maxDecals)
	{
		_maxDecals = maxDecals;
		int_1 = 24 * _maxDecals;
		Material[] array = dictionary_0.Keys.ToArray();
		foreach (Material key in array)
		{
			dictionary_0[key] = new DeferredDecalMeshDataClass(int_1);
		}
	}

	public void SetMaxDynamicDecals(int maxDecals)
	{
		_maxDynamicDecals = maxDecals;
		for (int i = 0; i < list_0.Count; i++)
		{
			UnityEngine.Object.Destroy(list_0[i].gameObject);
		}
		method_4();
		method_3();
	}

	public void EmitBloodOnEnvironment(Vector3 position, Vector3 normal)
	{
		if (!dictionary_0.ContainsKey(_environmentBlood.DecalMaterial))
		{
			foreach (KeyValuePair<Camera, DeferredDecalBufferClass> item in dictionary_2)
			{
				item.Value.IsStaticBufferDirty = true;
			}
			method_7(_environmentBlood);
		}
		method_6(position, normal, dictionary_0[_environmentBlood.DecalMaterial], _environmentBlood, _decalProjectorHeight);
	}

	public void EmitBleeding(Vector3 position, Vector3 normal)
	{
		if (!dictionary_0.ContainsKey(_bleedingDecal.DecalMaterial))
		{
			foreach (KeyValuePair<Camera, DeferredDecalBufferClass> item in dictionary_2)
			{
				item.Value.IsStaticBufferDirty = true;
			}
			method_7(_bleedingDecal);
		}
		method_6(position, normal, dictionary_0[_bleedingDecal.DecalMaterial], _bleedingDecal, _decalProjectorHeight);
	}

	public void DrawDecal(Vector3 position, Vector3 normal, BallisticCollider hitCollider, bool isGrenade)
	{
		bool num = hitCollider == null || hitCollider.gameObject.isStatic;
		SingleDecal singleDecal = method_0(hitCollider, isGrenade);
		float projectorHeight = (isGrenade ? _decalProjectorHeightForGrenade : _decalProjectorHeight);
		if (num)
		{
			if (!dictionary_0.ContainsKey(singleDecal.DecalMaterial))
			{
				method_7(singleDecal);
				foreach (KeyValuePair<Camera, DeferredDecalBufferClass> item in dictionary_2)
				{
					item.Value.IsStaticBufferDirty = true;
				}
			}
			method_6(position, normal, dictionary_0[singleDecal.DecalMaterial], singleDecal, projectorHeight);
			return;
		}
		method_5(position, normal, hitCollider, singleDecal, singleDecal.DynamicDecalMaterial, projectorHeight);
		foreach (KeyValuePair<Camera, DeferredDecalBufferClass> item2 in dictionary_2)
		{
			item2.Value.IsDynamicBufferDirty = true;
		}
	}

	public SingleDecal method_0(BallisticCollider hitCollider, bool isGrenade)
	{
		if (isGrenade)
		{
			return _grenadeDecal;
		}
		MaterialType key = ((!(hitCollider == null)) ? hitCollider.TypeOfMaterial : MaterialType.None);
		if (!dictionary_1.ContainsKey(key))
		{
			return _genericDecal;
		}
		return dictionary_1[key];
	}

	public void method_1(int size)
	{
		for (int i = 0; i < size; i++)
		{
			queue_0.Enqueue(new DeferredDecalMeshDataClass(_maxDecals));
		}
	}

	public DeferredDecalMeshDataClass method_2()
	{
		if (queue_0.Count == 0)
		{
			queue_0.Enqueue(new DeferredDecalMeshDataClass(_maxDecals));
		}
		return queue_0.Dequeue();
	}

	public void method_3()
	{
		foreach (KeyValuePair<Camera, DeferredDecalBufferClass> item in dictionary_2)
		{
			item.Value.CullGroup.SetBoundingSphereCount(_maxDynamicDecals);
		}
		for (int i = 0; i < _maxDynamicDecals; i++)
		{
			GameObject obj = new GameObject
			{
				name = "DynamicDecal" + i
			};
			UnityEngine.Object.DontDestroyOnLoad(obj);
			DynamicDeferredDecalRenderer dynamicDeferredDecalRenderer = obj.AddComponent<DynamicDeferredDecalRenderer>();
			GameObject gameObject = new GameObject
			{
				name = "DecalHelper" + i
			};
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			dynamicDeferredDecalRenderer.enabled = false;
			dynamicDeferredDecalRenderer.GameObjectHelper = gameObject;
			dynamicDeferredDecalRenderer.TransformHelper = gameObject.transform;
			obj.transform.parent = transform_0;
			dynamicDeferredDecalRenderer.TransformHelper.parent = transform_0;
			list_0.Add(dynamicDeferredDecalRenderer);
		}
	}

	public void method_4()
	{
		if (list_0 == null)
		{
			return;
		}
		for (int i = 0; i < list_0.Count; i++)
		{
			if (!(list_0[i] == null))
			{
				if (list_0[i].gameObject != null)
				{
					UnityEngine.Object.DestroyImmediate(list_0[i].gameObject);
				}
				if (list_0[i].GameObjectHelper != null)
				{
					UnityEngine.Object.DestroyImmediate(list_0[i].GameObjectHelper);
				}
			}
		}
		list_0.Clear();
	}

	public void method_5(Vector3 position, Vector3 normal, BallisticCollider hitCollider, SingleDecal currentDecal, Material currentMaterial, float projectorHeight)
	{
		DynamicDeferredDecalRenderer dynamicDeferredDecalRenderer = list_0[int_3];
		GameObject gameObject = dynamicDeferredDecalRenderer.gameObject;
		Transform transformHelper = dynamicDeferredDecalRenderer.TransformHelper;
		int cullingGroupSphereIndex = dynamicDeferredDecalRenderer.CullingGroupSphereIndex;
		float num = UnityEngine.Random.Range(currentDecal.DecalSize.x, currentDecal.DecalSize.y);
		float num2 = num * 2f;
		float rad = Mathf.Sqrt(num * num + num * num);
		boundingSphere_0[cullingGroupSphereIndex] = new BoundingSphere(position, rad);
		if (gameObject != null)
		{
			gameObject.transform.localScale = new Vector3(num2, projectorHeight, num2);
			gameObject.transform.up = normal;
			gameObject.transform.position = position;
			if (currentDecal.RandomizeRotation)
			{
				gameObject.transform.Rotate(Vector3.up, UnityEngine.Random.Range(0f, 359f), Space.Self);
			}
			if (transformHelper != null)
			{
				transformHelper.position = position;
				transformHelper.rotation = gameObject.transform.rotation;
				transformHelper.parent = hitCollider.transform;
			}
		}
		int num3 = UnityEngine.Random.Range(0, currentDecal.TileSheetColumns);
		int num4 = UnityEngine.Random.Range(0, currentDecal.TileSheetRows);
		Vector4 uvStartEnd = new Vector4((float)num4 * currentDecal.TileUSize, (float)num3 * currentDecal.TileVSize, (float)num4 * currentDecal.TileUSize + currentDecal.TileUSize, (float)num3 * currentDecal.TileVSize + currentDecal.TileVSize);
		dynamicDeferredDecalRenderer.enabled = true;
		dynamicDeferredDecalRenderer.Init(currentMaterial, _cube, normal, uvStartEnd, currentDecal.IsTiled, cullingGroupSphereIndex);
		int_3 = (int_3 + 1) % _maxDynamicDecals;
	}

	public void method_6(Vector3 position, Vector3 normal, DeferredDecalMeshDataClass mesh, SingleDecal decal, float projectorHeight)
	{
		float value = UnityEngine.Random.Range(decal.DecalSize.x, decal.DecalSize.y);
		value = Mathf.Clamp(value, 0f, 0.4f);
		Vector3 vector = new Vector3(value, projectorHeight, value);
		Vector3 normalized = normal.normalized;
		float num = Mathf.Abs(Vector3.Dot(normalized, Vector3.up));
		Vector3 vector2 = Vector3.Cross(normalized, (num > 0.999f) ? Vector3.right : Vector3.up).normalized;
		Vector3 vector3 = Vector3.Cross(normalized, vector2).normalized;
		if (decal.RandomizeRotation)
		{
			Quaternion quaternion = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 359), normalized);
			vector2 = quaternion * vector2;
			vector3 = quaternion * vector3;
		}
		vector3_0 = DeferredDecalMeshHelper.GenerateVerts(vector3_0, position, vector, vector2, normalized, vector3);
		vector4_3 = DeferredDecalMeshHelper.GenerateTangents(vector4_3, decal);
		GClass842.FillWith(vector3_1, vector);
		GClass842.FillWith(vector4_0, new Vector4(vector2.x, vector2.y, vector2.z, position.x));
		GClass842.FillWith(vector4_1, new Vector4(normalized.x, normalized.y, normalized.z, position.y));
		GClass842.FillWith(vector4_2, new Vector4(vector3.x, vector3.y, vector3.z, position.z));
		mesh.PasteProjectorIntoMiddle(int_0 * 24, vector3_0, vector4_3, vector3_1, vector4_0, vector4_1, vector4_2);
		int_0 = (int_0 + 1) % _maxDecals;
	}

	public void method_7(SingleDecal decal)
	{
		decal.TileUSize = 1f / (float)decal.TileSheetColumns;
		decal.TileVSize = 1f / (float)decal.TileSheetRows;
		dictionary_0.Add(decal.DecalMaterial, method_2());
	}

	public void OnDisable()
	{
		method_8();
	}

	public void method_8()
	{
		foreach (KeyValuePair<Camera, DeferredDecalBufferClass> item in dictionary_2)
		{
			if (item.Key != null)
			{
				item.Key.RemoveCommandBuffer(CameraEvent.BeforeLighting, item.Value.StaticDecalsBuffer);
				item.Key.RemoveCommandBuffer(CameraEvent.BeforeLighting, item.Value.DynamicDecalsBuffer);
			}
			item.Value.CullGroup.Dispose();
			item.Value.CullGroup = null;
		}
		dictionary_2.Clear();
	}

	public void OnPreCullCameraRender(Camera currentCamera)
	{
		if (method_12(currentCamera))
		{
			commandBuffer_0 = null;
			commandBuffer_1 = null;
			bool_0 = false;
			if (dictionary_2.ContainsKey(currentCamera))
			{
				commandBuffer_0 = dictionary_2[currentCamera].StaticDecalsBuffer;
				commandBuffer_1 = dictionary_2[currentCamera].DynamicDecalsBuffer;
			}
			else
			{
				commandBuffer_0 = new CommandBuffer();
				commandBuffer_0.name = "Deferred decals Static";
				currentCamera.AddCommandBuffer(CameraEvent.BeforeLighting, commandBuffer_0);
				commandBuffer_1 = new CommandBuffer();
				commandBuffer_1.name = "Deferred decals Dynamic";
				currentCamera.AddCommandBuffer(CameraEvent.BeforeLighting, commandBuffer_1);
				bool_0 = true;
				dictionary_2.Add(currentCamera, new DeferredDecalBufferClass
				{
					StaticDecalsBuffer = commandBuffer_0,
					DynamicDecalsBuffer = commandBuffer_1,
					CullGroup = new CullingGroup()
				});
				dictionary_2[currentCamera].CullGroup.targetCamera = currentCamera;
				dictionary_2[currentCamera].CullGroup.SetBoundingSpheres(boundingSphere_0);
			}
			if (dictionary_2[currentCamera].IsStaticBufferDirty || bool_0)
			{
				dictionary_2[currentCamera].IsStaticBufferDirty = false;
				method_9(commandBuffer_0, currentCamera, method_11);
			}
		}
	}

	public void OnPreCameraRender(Camera currentCamera)
	{
		if (method_12(currentCamera) && dictionary_2.ContainsKey(currentCamera) && (dictionary_2[currentCamera].IsDynamicBufferDirty || bool_0))
		{
			dictionary_2[currentCamera].IsDynamicBufferDirty = false;
			method_9(commandBuffer_1, currentCamera, method_10);
		}
	}

	public void method_9(CommandBuffer buffer, Camera currentCamera, Action<Camera, CommandBuffer> drawFunc)
	{
		buffer.Clear();
		buffer.GetTemporaryRT(int_2, -1, -1);
		buffer.Blit(BuiltinRenderTextureType.GBuffer2, int_2);
		buffer.SetRenderTarget(new RenderTargetIdentifier[4]
		{
			BuiltinRenderTextureType.GBuffer0,
			BuiltinRenderTextureType.GBuffer1,
			BuiltinRenderTextureType.GBuffer2,
			currentCamera.allowHDR ? BuiltinRenderTextureType.CameraTarget : BuiltinRenderTextureType.GBuffer3
		}, BuiltinRenderTextureType.CameraTarget);
		drawFunc(currentCamera, buffer);
		buffer.ReleaseTemporaryRT(int_2);
	}

	public void Update()
	{
		bool flag = false;
		for (int i = 0; i < list_0.Count; i++)
		{
			if (list_0[i].enabled && list_0[i].ManualUpdate())
			{
				flag = true;
			}
		}
		if (flag)
		{
			method_13();
		}
	}

	public void method_10(Camera currentCamera, CommandBuffer buffer)
	{
		for (int i = 0; i < list_0.Count; i++)
		{
			if (list_0[i].enabled && dictionary_2[currentCamera].CullGroup.GetDistance(list_0[i].CullingGroupSphereIndex) < _cullDistance)
			{
				buffer.DrawMesh(_cube, list_0[i].ModelMatrix, list_0[i].DecalMaterial);
			}
		}
	}

	public void method_11(Camera currentCamera, CommandBuffer buffer)
	{
		foreach (KeyValuePair<Material, DeferredDecalMeshDataClass> item in dictionary_0)
		{
			buffer.DrawMesh(item.Value.Mesh, Matrix4x4.identity, item.Key);
		}
	}

	public bool method_12(Camera currentCamera)
	{
		if (!(currentCamera == null) && EFTHardSettings.Instance.DEFERRED_DECALS_ENABLED)
		{
			if (!currentCamera.isActiveAndEnabled)
			{
				return false;
			}
			bool flag = currentCamera.CompareTag("OpticCamera");
			if (currentCamera.renderingPath != RenderingPath.DeferredShading && !flag)
			{
				return false;
			}
			if (!((CameraClass.Instance.Camera != null && CameraClass.Instance.Camera == currentCamera) || flag))
			{
				return false;
			}
			if (dictionary_0.Count == 0 && list_0.Count == 0)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public void method_13()
	{
		foreach (KeyValuePair<Camera, DeferredDecalBufferClass> item in dictionary_2)
		{
			item.Value.IsDynamicBufferDirty = true;
		}
	}

	public void OnDestroy()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(OnPreCullCameraRender));
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(OnPreCameraRender));
		method_8();
		method_4();
		foreach (DeferredDecalMeshDataClass item in queue_0)
		{
			item.Dispose();
		}
		queue_0.Clear();
		foreach (DeferredDecalMeshDataClass value in dictionary_0.Values)
		{
			value.Dispose();
		}
		dictionary_0.Clear();
		if (transform_0 != null)
		{
			UnityEngine.Object.DestroyImmediate(transform_0.gameObject);
		}
	}

	public void Clear()
	{
		method_4();
		foreach (DeferredDecalMeshDataClass item in queue_0)
		{
			item.Dispose();
		}
		queue_0.Clear();
		foreach (DeferredDecalMeshDataClass value in dictionary_0.Values)
		{
			value.Dispose();
		}
		dictionary_0.Clear();
		int_3 = 0;
		method_3();
		method_13();
	}
}
