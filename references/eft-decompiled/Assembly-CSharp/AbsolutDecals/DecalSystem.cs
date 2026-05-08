using System;
using System.Collections.Generic;
using EFT.Ballistics;
using UnityEngine;

namespace AbsolutDecals;

[Serializable]
[ExecuteInEditMode]
public class DecalSystem : MonoBehaviourSingleton<DecalSystem>
{
	public enum BakeResult
	{
		Unsuccess,
		BakedToSkinnedMesh,
		BakedToMesh
	}

	[Serializable]
	public class SingleDecal
	{
		public Material DecalMaterial;

		public int TileSheetRows = 1;

		public int TileSheetColumns = 1;

		public bool UseRandomScale = true;

		public Vector3 MaxScale = new Vector3(0.5f, 0.2f, 0.5f);

		public Vector3 MinScale = new Vector3(0.2f, 0.2f, 0.2f);

		public bool CanRotate = true;

		public MaterialType TypeOfMaterial;

		[NonSerialized]
		public bool IsPrecalculated;

		[NonSerialized]
		public Vector2[][] UvSet;

		public Vector3 GetScale()
		{
			if (!UseRandomScale)
			{
				return MaxScale;
			}
			return Vector3.Lerp(MinScale, MaxScale, UnityEngine.Random.Range(0f, 1f));
		}

		public Vector2[] GetUv()
		{
			if (TileSheetRows == 1 && TileSheetColumns == 1)
			{
				return new Vector2[4]
				{
					new Vector2(0f, 0f),
					new Vector2(0f, 1f),
					new Vector2(1f, 1f),
					new Vector2(1f, 0f)
				};
			}
			if (!IsPrecalculated)
			{
				method_0();
			}
			int num = UnityEngine.Random.Range(0, TileSheetColumns * TileSheetRows);
			return UvSet[num];
		}

		public void method_0()
		{
			int num = TileSheetRows * TileSheetColumns;
			UvSet = new Vector2[num][];
			float num2 = 1f / (float)TileSheetRows;
			float num3 = 1f / (float)TileSheetColumns;
			int i = 0;
			int num4 = 0;
			int num5 = 0;
			for (; i < num; i++)
			{
				float x = num2 * (float)num4;
				float x2 = num2 * (float)(num4 + 1);
				float y = num3 * (float)num5;
				float y2 = num3 * (float)(num5 + 1);
				UvSet[i] = new Vector2[4];
				UvSet[i][0] = new Vector2(x, y);
				UvSet[i][1] = new Vector2(x, y2);
				UvSet[i][2] = new Vector2(x2, y2);
				UvSet[i][3] = new Vector2(x2, y);
				num4++;
				if (num4 >= TileSheetRows)
				{
					num5++;
					num4 = 0;
				}
			}
			IsPrecalculated = true;
		}
	}

	public SingleDecal DefaultMaterial;

	public SingleDecal BloodOnWallsMaterial;

	public SingleDecal[] Decals;

	private List<Renderer> _renderers = new List<Renderer>();

	private List<Mesh> _meshes = new List<Mesh>();

	private List<DecalMeshClass> _tmpMeshes = new List<DecalMeshClass>();

	private List<Transform> _transforms = new List<Transform>();

	[SerializeField]
	private List<Renderer> _staticRenderers = new List<Renderer>();

	[SerializeField]
	private List<Mesh> _staticMeshes = new List<Mesh>();

	[SerializeField]
	private List<DecalMeshClass> _staticTmpMeshes = new List<DecalMeshClass>();

	[SerializeField]
	private List<Transform> _staticTransforms = new List<Transform>();

	public int count;

	private static List<SkinnedMeshRenderer> _skinnedMeshRenderers = new List<SkinnedMeshRenderer>();

	private static List<BoneWeight[]> _skinnedMeshBoneWeights = new List<BoneWeight[]>();

	private static List<Transform> _skinnedMeshTransforms = new List<Transform>();

	[SerializeField]
	private float _offset = 0.05f;

	[SerializeField]
	private string[] _ignoreProjectorTags;

	[SerializeField]
	private LayerMask _ingoreDecalsLayers = 0;

	[SerializeField]
	private int _maxDecalsOnSkinMesh = 3;

	private const string IgnoreProjectorTag = "DecalIgnore";

	private static DecalMeshClass _terrainMesh;

	private const int _maxUniqueDecalQueueCapasity = 20;

	private static GameObject _decalsParent;

	[SerializeField]
	private int MaxMeshVerticies = 60000;

	[SerializeField]
	private bool _includeTerrain = true;

	private static Dictionary<Material, List<MeshFilter>> _bakedMeshes = new Dictionary<Material, List<MeshFilter>>();

	private static Dictionary<Material, MeshFilter> _bakedReusableMeshes = new Dictionary<Material, MeshFilter>();

	private static Dictionary<MeshFilter, List<ProjectorHelperClass>> _reusableMeshesProjectors = new Dictionary<MeshFilter, List<ProjectorHelperClass>>();

	private static Dictionary<DecalProjector, ProjectorHelperClass> _projectors = new Dictionary<DecalProjector, ProjectorHelperClass>();

	private static Dictionary<DecalProjector, GameObject> _uniqueMeshes = new Dictionary<DecalProjector, GameObject>();

	private static Queue<GameObject> _uniqueMeshObjects = new Queue<GameObject>(20);

	private static Dictionary<SkinnedMeshRenderer, List<SkinnedMeshRenderer>> _skinnedDecals = new Dictionary<SkinnedMeshRenderer, List<SkinnedMeshRenderer>>();

	private static int _currentIndex = 1;

	public Action<List<Mesh>> OnAssetsReadyToSave;

	public static Action CallProjectors;

	private List<Mesh> _skinnedMeshes = new List<Mesh>();

	public GameObject ProjectorsParent { get; set; }

	public override void Awake()
	{
		base.Awake();
		count = _staticTmpMeshes.Count;
		UpdateNonStaticSceneData();
	}

	public void method_0()
	{
		OnDestroy(destroyProjectors: true);
	}

	public void Start()
	{
		FindProjectors();
	}

	public void UpdateSkinnedMeshSceneData()
	{
		method_9();
		SkinnedMeshRenderer[] array = GClass870.FindUnityObjectsOfType<SkinnedMeshRenderer>();
		for (int i = 0; i < array.Length; i++)
		{
			if (!(array[i].sharedMesh == null))
			{
				_skinnedMeshRenderers.Add(array[i]);
				_skinnedMeshBoneWeights.Add(array[i].sharedMesh.boneWeights);
				_skinnedMeshTransforms.Add(array[i].transform);
			}
		}
	}

	public void UpdateNonStaticSceneData()
	{
		UpdateSceneData(updateStatic: false);
	}

	public void UpdateSceneData(bool updateStatic)
	{
		Renderer[] array = GClass870.FindUnityObjectsOfType<Renderer>();
		LODGroup[] array2 = GClass870.FindUnityObjectsOfType<LODGroup>();
		List<int> list = new List<int>();
		method_9();
		method_8();
		for (int i = 0; i < array2.Length; i++)
		{
			LOD[] lODs = array2[i].GetLODs();
			bool flag = false;
			if (lODs.Length == 1)
			{
				for (int j = 0; j < lODs[0].renderers.Length; j++)
				{
					flag = method_1(lODs[0].renderers[j], updateStatic);
				}
			}
			else if (lODs.Length > 1)
			{
				for (int k = 0; k < lODs[1].renderers.Length; k++)
				{
					flag = method_1(lODs[1].renderers[k], updateStatic);
				}
			}
			if (!flag)
			{
				continue;
			}
			for (int l = 0; l < lODs.Length; l++)
			{
				for (int m = 0; m < lODs[l].renderers.Length; m++)
				{
					if (!(lODs[l].renderers[m] == null))
					{
						list.Add(lODs[l].renderers[m].gameObject.GetInstanceID());
					}
				}
			}
		}
		for (int n = 0; n < array.Length; n++)
		{
			if (array[n].enabled && !list.Contains(array[n].gameObject.GetInstanceID()))
			{
				method_1(array[n], updateStatic);
			}
		}
		Terrain activeTerrain = Terrain.activeTerrain;
		if (activeTerrain != null && activeTerrain.transform != null && activeTerrain.terrainData != null)
		{
			_terrainMesh = DecalMeshClass.TerrainToMesh(activeTerrain.terrainData, Terrain.activeTerrain.transform);
		}
		else
		{
			_includeTerrain = false;
		}
		GameObject gameObject = GameObject.Find("DecalsParent");
		if (gameObject == null)
		{
			_decalsParent = new GameObject("DecalsParent");
			_decalsParent.transform.position = Vector3.zero;
			_decalsParent.transform.rotation = Quaternion.identity;
		}
		else
		{
			_decalsParent = gameObject;
		}
		GameObject gameObject2 = GameObject.Find("ProjectorsParent");
		if (gameObject2 == null)
		{
			ProjectorsParent = new GameObject("ProjectorsParent");
			ProjectorsParent.transform.position = Vector3.zero;
			ProjectorsParent.transform.rotation = Quaternion.identity;
		}
		else
		{
			ProjectorsParent = gameObject2;
		}
		_transforms.AddRange(_staticTransforms);
		int num = _meshes.Count;
		_meshes.AddRange(_staticMeshes);
		_renderers.AddRange(_staticRenderers);
		for (int num2 = 0; num2 < _meshes.Count; num2++)
		{
			if (num2 < num)
			{
				_tmpMeshes.Add(new DecalMeshClass(_meshes[num2]));
			}
			else
			{
				_tmpMeshes.Add(new DecalMeshClass(_meshes[num2], isFromStatic: true, _transforms[num2]));
			}
		}
	}

	public void UpdateStaticSceneData()
	{
		UpdateSceneData(updateStatic: true);
	}

	public bool method_1(Renderer currentRenderer, bool forStatic)
	{
		int num = -1;
		if (currentRenderer == null)
		{
			return false;
		}
		if (_ignoreProjectorTags != null)
		{
			num = Array.IndexOf(_ignoreProjectorTags, currentRenderer.gameObject.tag);
		}
		if (num == -1 && !(currentRenderer.gameObject.tag == "DecalIgnore") && (currentRenderer.gameObject.layer & (int)_ingoreDecalsLayers) == 0)
		{
			SkinnedMeshRenderer component = currentRenderer.GetComponent<SkinnedMeshRenderer>();
			if (component == null)
			{
				MeshFilter component2 = currentRenderer.GetComponent<MeshFilter>();
				if (component2 == null)
				{
					return false;
				}
				if (currentRenderer.gameObject.isStatic)
				{
					if (!forStatic)
					{
						return false;
					}
					if (!Application.isPlaying)
					{
						_staticMeshes.Add(component2.sharedMesh);
					}
					else
					{
						_staticMeshes.Add(component2.mesh);
					}
					_staticRenderers.Add(currentRenderer.GetComponent<Renderer>());
					_staticTransforms.Add(currentRenderer.transform);
				}
				else
				{
					if (forStatic)
					{
						return false;
					}
					if (component2.sharedMesh == null)
					{
						return false;
					}
					_meshes.Add(component2.sharedMesh);
					_transforms.Add(currentRenderer.transform);
					_renderers.Add(currentRenderer.GetComponent<Renderer>());
				}
			}
			else
			{
				if (forStatic)
				{
					return false;
				}
				if (component.sharedMesh == null)
				{
					return false;
				}
				_skinnedMeshRenderers.Add(component);
				_skinnedMeshBoneWeights.Add(component.sharedMesh.boneWeights);
				_skinnedMeshTransforms.Add(component.transform);
				_skinnedMeshes.Add(component.sharedMesh);
			}
			return true;
		}
		return false;
	}

	public void RegisterRenderer(GameObject go)
	{
		Renderer component = go.GetComponent<Renderer>();
		if (!(component == null))
		{
			_renderers.Add(component);
			_meshes.Add(go.GetComponent<MeshFilter>().sharedMesh);
			_tmpMeshes.Add(new DecalMeshClass(_meshes[_meshes.Count - 1]));
			_transforms.Add(go.transform);
		}
	}

	public void BakeAndSaveAll()
	{
		foreach (KeyValuePair<DecalProjector, ProjectorHelperClass> projector in _projectors)
		{
			DecalProjector key = projector.Key;
			if (key.gameObject.activeSelf && key.CurrentState == DecalProjector.ProjectorState.Unbaked && !key.DontBakeInEditor && BakeDecal(key, bakeToReusableMesh: false) != BakeResult.Unsuccess)
			{
				key.CurrentState = DecalProjector.ProjectorState.WaitingForSaveOnDisc;
			}
		}
		GameObject gameObject = GameObject.Find("SavedDecalsParent");
		if (gameObject == null)
		{
			gameObject = new GameObject("SavedDecalsParent");
			gameObject.transform.position = Vector3.zero;
			gameObject.transform.rotation = Quaternion.identity;
		}
		List<Mesh> list = new List<Mesh>();
		foreach (KeyValuePair<Material, List<MeshFilter>> bakedMesh in _bakedMeshes)
		{
			foreach (MeshFilter item in bakedMesh.Value)
			{
				item.gameObject.transform.parent = gameObject.transform;
				list.Add(item.sharedMesh);
			}
		}
		foreach (KeyValuePair<Material, MeshFilter> bakedReusableMesh in _bakedReusableMeshes)
		{
			bakedReusableMesh.Value.gameObject.transform.parent = gameObject.transform;
			list.Add(bakedReusableMesh.Value.sharedMesh);
		}
		_bakedMeshes = new Dictionary<Material, List<MeshFilter>>();
		_bakedReusableMeshes = new Dictionary<Material, MeshFilter>();
		List<DecalProjector> list2 = new List<DecalProjector>();
		foreach (KeyValuePair<DecalProjector, ProjectorHelperClass> projector2 in _projectors)
		{
			if (projector2.Key.CurrentState == DecalProjector.ProjectorState.WaitingForSaveOnDisc)
			{
				projector2.Key.ResetState();
				list2.Add(projector2.Key);
				projector2.Key.gameObject.SetActive(value: false);
			}
		}
		if (OnAssetsReadyToSave != null)
		{
			OnAssetsReadyToSave(list);
		}
		foreach (DecalProjector item2 in list2)
		{
			UnregisterProjector(item2);
		}
		UpdateSceneData(updateStatic: false);
		if (CallProjectors != null)
		{
			CallProjectors();
		}
	}

	public SingleDecal GetDecal(MaterialType typeOfMaterial)
	{
		if (typeOfMaterial == MaterialType.None)
		{
			return DefaultMaterial;
		}
		int num = 0;
		while (true)
		{
			if (num < Decals.Length)
			{
				if (Decals[num].TypeOfMaterial == typeOfMaterial)
				{
					break;
				}
				num++;
				continue;
			}
			return DefaultMaterial;
		}
		return Decals[num];
	}

	public void RegisterProjector(DecalProjector projector)
	{
		if (!_projectors.ContainsKey(projector))
		{
			_projectors.Add(projector, new ProjectorHelperClass
			{
				DecalProjector = projector
			});
		}
	}

	public bool BakeUniqueDecal(DecalProjector projector)
	{
		DecalMeshClass decalMesh = GetDecalMesh(projector, inProjectorSpace: false);
		if (decalMesh != null && decalMesh.IsSkinned)
		{
			if (BakeToSkinnedMesh(decalMesh, projector) == BakeResult.BakedToSkinnedMesh)
			{
				return true;
			}
			return false;
		}
		if (decalMesh == null)
		{
			return false;
		}
		GameObject gameObject;
		MeshFilter meshFilter;
		MeshRenderer meshRenderer;
		if (_uniqueMeshObjects.Count > 0)
		{
			gameObject = _uniqueMeshObjects.Dequeue();
			meshFilter = gameObject.GetComponent<MeshFilter>();
			meshRenderer = gameObject.GetComponent<MeshRenderer>();
		}
		else
		{
			gameObject = new GameObject("DecalProjector" + projector.GetHashCode());
			gameObject.transform.parent = _decalsParent.transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.rotation = Quaternion.identity;
			gameObject.tag = "DecalIgnore";
			meshFilter = gameObject.AddComponent<MeshFilter>();
			meshRenderer = gameObject.AddComponent<MeshRenderer>();
		}
		meshFilter.sharedMesh = decalMesh.ToMesh();
		meshRenderer.sharedMaterial = projector.Decal.DecalMaterial;
		_uniqueMeshes.Add(projector, gameObject);
		gameObject.SetActive(value: true);
		return true;
	}

	public BakeResult BakeDecal(DecalProjector projector, bool bakeToReusableMesh)
	{
		Material decalMaterial = projector.Decal.DecalMaterial;
		ProjectorHelperClass projectorHelper = _projectors[projector];
		if (!bakeToReusableMesh)
		{
			return method_4(decalMaterial, projectorHelper, projector);
		}
		return method_2(decalMaterial, projectorHelper, projector);
	}

	public BakeResult method_2(Material projectorMaterial, ProjectorHelperClass projectorHelper, DecalProjector projector)
	{
		DecalMeshClass decalMesh = GetDecalMesh(projector, inProjectorSpace: false);
		if (decalMesh != null && decalMesh.IsSkinned)
		{
			return BakeToSkinnedMesh(decalMesh, projector);
		}
		if (!_bakedReusableMeshes.ContainsKey(projectorMaterial))
		{
			if (!method_6(projector, "ReusableMesh", projectorMaterial, projectorHelper, out var resultFilter, decalMesh))
			{
				return BakeResult.Unsuccess;
			}
			_bakedReusableMeshes.Add(projectorMaterial, resultFilter);
			_reusableMeshesProjectors.Add(resultFilter, new List<ProjectorHelperClass> { projectorHelper });
		}
		else
		{
			MeshFilter meshFilter = _bakedReusableMeshes[projectorMaterial];
			Mesh mesh = meshFilter.sharedMesh;
			if (decalMesh == null)
			{
				return BakeResult.Unsuccess;
			}
			int vertexCount = mesh.vertexCount;
			int vertexCount2 = decalMesh.VertexCount;
			if (vertexCount + vertexCount2 >= MaxMeshVerticies)
			{
				List<ProjectorHelperClass> list = _reusableMeshesProjectors[meshFilter];
				float num = 0f;
				int i;
				for (i = 0; i < list.Count; i++)
				{
					Vector2 vertsStartEnd = list[i].VertsStartEnd;
					num += vertsStartEnd.y - vertsStartEnd.x;
					if (num >= (float)vertexCount2)
					{
						break;
					}
				}
				if (i == list.Count)
				{
					return BakeResult.Unsuccess;
				}
				List<ProjectorHelperClass> list2 = new List<ProjectorHelperClass>();
				for (int num2 = i; num2 >= 0; num2--)
				{
					mesh = GClass1033.RemoveFromMesh(list[num2].VertsStartEnd, list[num2].TrigsStartEnd, mesh);
					list2.Add(list[num2]);
					list[num2].DecalProjector.gameObject.SetActive(value: false);
					smethod_0(list[num2], _projectors);
				}
				foreach (ProjectorHelperClass item in list2)
				{
					_projectors.Remove(item.DecalProjector);
					_reusableMeshesProjectors[meshFilter].Remove(item);
				}
			}
			method_5(projectorHelper, meshFilter, mesh, decalMesh);
			_reusableMeshesProjectors[meshFilter].Add(projectorHelper);
		}
		return BakeResult.BakedToMesh;
	}

	public BakeResult BakeToSkinnedMesh(DecalMeshClass meshToBake, DecalProjector projector)
	{
		SkinnedMeshRenderer component = meshToBake.DecalTransform.GetComponent<SkinnedMeshRenderer>();
		component.bones = meshToBake.Bones;
		component.material = projector.Decal.DecalMaterial;
		meshToBake.ToMesh().RecalculateBounds();
		component.sharedMesh = meshToBake.ToMesh();
		component.enabled = true;
		return BakeResult.BakedToSkinnedMesh;
	}

	public Transform GetRelativeTransform(SkinnedMeshRenderer doll, out bool success)
	{
		SkinnedMeshRenderer skinnedMeshRenderer;
		if (_skinnedDecals.ContainsKey(doll))
		{
			foreach (SkinnedMeshRenderer item in _skinnedDecals[doll])
			{
				if (!item.enabled)
				{
					success = true;
					return item.transform;
				}
			}
			if (_skinnedDecals[doll].Count >= _maxDecalsOnSkinMesh)
			{
				success = false;
				return null;
			}
			skinnedMeshRenderer = method_3(doll);
			_skinnedDecals[doll].Add(skinnedMeshRenderer);
			success = true;
			return skinnedMeshRenderer.transform;
		}
		skinnedMeshRenderer = method_3(doll);
		success = true;
		_skinnedDecals.Add(doll, new List<SkinnedMeshRenderer> { skinnedMeshRenderer });
		return skinnedMeshRenderer.transform;
	}

	public SkinnedMeshRenderer method_3(SkinnedMeshRenderer doll)
	{
		GameObject obj = new GameObject("SkinnedDecals" + doll.name)
		{
			tag = "DecalIgnore"
		};
		SkinnedMeshRenderer skinnedMeshRenderer = obj.AddComponent<SkinnedMeshRenderer>();
		obj.transform.parent = doll.transform.parent;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = Quaternion.identity;
		skinnedMeshRenderer.enabled = false;
		return skinnedMeshRenderer;
	}

	public BakeResult method_4(Material projectorMaterial, ProjectorHelperClass projectorHelper, DecalProjector projector)
	{
		DecalMeshClass decalMesh = GetDecalMesh(projector, inProjectorSpace: false);
		if (decalMesh != null && decalMesh.IsSkinned)
		{
			return BakeToSkinnedMesh(decalMesh, projector);
		}
		if (!_bakedMeshes.ContainsKey(projectorMaterial))
		{
			if (!method_6(projector, "ReusableMesh", projectorMaterial, projectorHelper, out var resultFilter, decalMesh))
			{
				return BakeResult.Unsuccess;
			}
			_bakedMeshes.Add(projectorMaterial, new List<MeshFilter> { resultFilter });
		}
		else
		{
			List<MeshFilter> list = _bakedMeshes[projectorMaterial];
			MeshFilter meshFilter = list[list.Count - 1];
			Mesh sharedMesh = meshFilter.sharedMesh;
			if (decalMesh == null)
			{
				return BakeResult.Unsuccess;
			}
			int vertexCount = sharedMesh.vertexCount;
			int vertexCount2 = decalMesh.VertexCount;
			if (vertexCount + vertexCount2 < MaxMeshVerticies)
			{
				method_5(projectorHelper, meshFilter, sharedMesh, decalMesh);
			}
			else
			{
				method_6(projector, "ReusableMesh" + _currentIndex, projectorMaterial, projectorHelper, out var resultFilter2, decalMesh);
				list.Add(resultFilter2);
			}
		}
		return BakeResult.BakedToMesh;
	}

	public void method_5(ProjectorHelperClass projectorHelper, MeshFilter filter, Mesh mesh, DecalMeshClass tmpMesh)
	{
		projectorHelper.ParentMesh = filter.sharedMesh;
		projectorHelper.ParentFilter = filter;
		int num = mesh.triangles.Length;
		int vertexCount = mesh.vertexCount;
		mesh += tmpMesh;
		projectorHelper.TrigsStartEnd = new Vector2(num, mesh.triangles.Length);
		projectorHelper.VertsStartEnd = new Vector2(vertexCount, mesh.vertexCount);
		filter.sharedMesh = mesh;
		projectorHelper.Index = _currentIndex;
		_currentIndex++;
	}

	public bool method_6(DecalProjector projector, string namePrefix, Material projectorMaterial, ProjectorHelperClass projectorHelper, out MeshFilter resultFilter, DecalMeshClass tmpMeshPrecalc = null)
	{
		if (tmpMeshPrecalc == null)
		{
			resultFilter = null;
			return false;
		}
		Mesh mesh = tmpMeshPrecalc.ToMesh();
		GameObject gameObject = new GameObject(namePrefix + projectorMaterial.name + projectorMaterial.GetHashCode());
		gameObject.transform.parent = _decalsParent.transform;
		gameObject.tag = "DecalIgnore";
		gameObject.transform.localPosition = Vector3.zero;
		resultFilter = gameObject.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		resultFilter.mesh = mesh;
		meshRenderer.sharedMaterial = projectorMaterial;
		projectorHelper.ParentMesh = mesh;
		projectorHelper.ParentFilter = resultFilter;
		projectorHelper.TrigsStartEnd = new Vector2(0f, mesh.triangles.Length);
		projectorHelper.VertsStartEnd = new Vector2(0f, mesh.vertexCount);
		projectorHelper.Index = _currentIndex;
		_currentIndex++;
		return true;
	}

	public void UnbakeDecal(DecalProjector projector)
	{
		if (projector.CurrentState == DecalProjector.ProjectorState.BakedToUniqueMesh)
		{
			GameObject gameObject = _uniqueMeshes[projector];
			_uniqueMeshes.Remove(projector);
			_uniqueMeshObjects.Enqueue(gameObject);
			gameObject.SetActive(value: false);
		}
		else if (projector.CurrentState != DecalProjector.ProjectorState.BakedInProjectorSpace && _projectors.ContainsKey(projector))
		{
			ProjectorHelperClass projectorHelperClass = _projectors[projector];
			MeshFilter parentFilter = projectorHelperClass.ParentFilter;
			if (parentFilter != null)
			{
				parentFilter.sharedMesh = GClass1033.RemoveFromMesh(projectorHelperClass.VertsStartEnd, projectorHelperClass.TrigsStartEnd, parentFilter.sharedMesh);
			}
			if (projector.BakedInReusableMesh)
			{
				_reusableMeshesProjectors[projectorHelperClass.ParentFilter].Remove(projectorHelperClass);
			}
			smethod_0(projectorHelperClass, _projectors);
		}
	}

	public DecalMeshClass GetDecalMesh(DecalProjector projector, bool inProjectorSpace)
	{
		DecalMeshClass decalMesh = GClass1036.GetDecalMesh(projector, _skinnedMeshRenderers, _skinnedMeshBoneWeights, _skinnedMeshTransforms, null, _skinnedMeshes, projector.Decal.GetUv(), _offset);
		if (decalMesh != null)
		{
			return decalMesh;
		}
		return GClass1034.GetDecalMesh(projector, inProjectorSpace, _terrainMesh, _renderers, _transforms, _meshes, _tmpMeshes, _offset, projector.Decal.GetUv(), _includeTerrain);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		OnDestroy(destroyProjectors: false);
	}

	public void method_7()
	{
		_bakedMeshes = new Dictionary<Material, List<MeshFilter>>();
		_bakedReusableMeshes = new Dictionary<Material, MeshFilter>();
		_reusableMeshesProjectors = new Dictionary<MeshFilter, List<ProjectorHelperClass>>();
		_projectors = new Dictionary<DecalProjector, ProjectorHelperClass>();
		_uniqueMeshes = new Dictionary<DecalProjector, GameObject>();
		_uniqueMeshObjects = new Queue<GameObject>();
		_projectors = new Dictionary<DecalProjector, ProjectorHelperClass>();
		if (Application.isPlaying)
		{
			ClearStaticLists();
		}
		method_8();
		method_9();
	}

	public void ClearStaticLists()
	{
		Debug.Log("++++++++++++++++clear static");
		_staticTransforms = new List<Transform>();
		_staticRenderers = new List<Renderer>();
		_staticMeshes = new List<Mesh>();
		_staticTmpMeshes = new List<DecalMeshClass>();
	}

	public void method_8()
	{
		_renderers = new List<Renderer>();
		_meshes = new List<Mesh>();
		_tmpMeshes = new List<DecalMeshClass>();
		_transforms = new List<Transform>();
	}

	public void method_9()
	{
		_skinnedMeshRenderers = new List<SkinnedMeshRenderer>();
		_skinnedMeshBoneWeights = new List<BoneWeight[]>();
		_skinnedMeshTransforms = new List<Transform>();
	}

	public void OnDestroy(bool destroyProjectors)
	{
		foreach (DecalProjector item in new List<DecalProjector>(_projectors.Keys))
		{
			if (item.CurrentState != DecalProjector.ProjectorState.Unbaked)
			{
				item.ResetState();
				UnregisterProjector(item, destroyMesh: true);
			}
			if (destroyProjectors)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
		method_7();
		if (_decalsParent != null)
		{
			UnityEngine.Object.Destroy(_decalsParent);
		}
		if (ProjectorsParent != null)
		{
			UnityEngine.Object.Destroy(ProjectorsParent);
		}
		GameObject gameObject = GameObject.Find("DecalsParent");
		if (gameObject != null)
		{
			UnityEngine.Object.Destroy(gameObject);
		}
		GameObject gameObject2 = GameObject.Find("ProjectorsParent");
		if (gameObject2 != null)
		{
			UnityEngine.Object.Destroy(gameObject2);
		}
		if (destroyProjectors && base.gameObject != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void UnregisterProjector(DecalProjector decalProjector, bool destroyMesh = false)
	{
		if (!_projectors.ContainsKey(decalProjector))
		{
			return;
		}
		if (decalProjector.CurrentState == DecalProjector.ProjectorState.BakedToCommonMesh)
		{
			ProjectorHelperClass projectorHelperClass = _projectors[decalProjector];
			if (destroyMesh)
			{
				Mesh sharedMesh = GClass1033.RemoveFromMesh(projectorHelperClass.VertsStartEnd, projectorHelperClass.TrigsStartEnd, projectorHelperClass.ParentFilter.sharedMesh);
				projectorHelperClass.ParentFilter.sharedMesh = sharedMesh;
				smethod_0(projectorHelperClass, _projectors);
			}
			_projectors.Remove(decalProjector);
		}
		else if (decalProjector.CurrentState == DecalProjector.ProjectorState.BakedToUniqueMesh)
		{
			if (_uniqueMeshes.ContainsKey(decalProjector))
			{
				GameObject gameObject = _uniqueMeshes[decalProjector];
				if (destroyMesh)
				{
					gameObject.GetComponent<MeshRenderer>().enabled = false;
				}
				_projectors.Remove(decalProjector);
			}
		}
		else
		{
			_projectors.Remove(decalProjector);
		}
	}

	public void FindProjectors()
	{
		DecalProjector[] array = GClass870.FindUnityObjectsOfType<DecalProjector>();
		foreach (DecalProjector projector in array)
		{
			RegisterProjector(projector);
		}
	}

	public static void smethod_0(ProjectorHelperClass projector, Dictionary<DecalProjector, ProjectorHelperClass> projectorHelpers)
	{
		Vector2 vector = new Vector2(projector.TrigsStartEnd.y - projector.TrigsStartEnd.x, projector.TrigsStartEnd.y - projector.TrigsStartEnd.x);
		Vector2 vector2 = new Vector2(projector.VertsStartEnd.y - projector.VertsStartEnd.x, projector.VertsStartEnd.y - projector.VertsStartEnd.x);
		MeshFilter parentFilter = projector.ParentFilter;
		foreach (ProjectorHelperClass value in projectorHelpers.Values)
		{
			if (value.Index > 0 && value.Index > projector.Index && value.ParentFilter == parentFilter)
			{
				value.TrigsStartEnd -= vector;
				value.VertsStartEnd -= vector2;
			}
		}
		projector.TrigsStartEnd = -Vector3.one;
		projector.VertsStartEnd = -Vector3.one;
		projector.ParentFilter = null;
		projector.ParentMesh = null;
		projector.Index = -1;
	}
}
