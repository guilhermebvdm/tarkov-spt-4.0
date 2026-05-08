using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SpeedTreeTerrainProcessor : MonoBehaviour
{
	public class Class652
	{
		public Class653[][] LodsRenderers;

		public float[] CullingRates;

		public Class652(LODGroup lodGroup)
		{
			LOD[] lODs = lodGroup.GetLODs();
			CullingRates = smethod_0(lODs);
			LodsRenderers = smethod_1(lODs);
			smethod_2(LodsRenderers, lODs);
		}

		public static float[] smethod_0(LOD[] lods)
		{
			float[] array = new float[lods.Length];
			for (int i = 0; i < lods.Length; i++)
			{
				array[i] = lods[i].screenRelativeTransitionHeight;
			}
			return array;
		}

		public static Class653[][] smethod_1(LOD[] lods)
		{
			Class653[][] array = new Class653[lods.Length][];
			for (int i = 0; i < lods.Length; i++)
			{
				LOD lOD = lods[i];
				array[i] = new Class653[lOD.renderers.Length];
				for (int j = 0; j < lOD.renderers.Length; j++)
				{
					array[i][j] = new Class653(lOD.renderers[j]);
				}
			}
			return array;
		}

		public static void smethod_2(Class653[][] lodsRenderers, LOD[] lods)
		{
			Dictionary<Renderer, int> dictionary = new Dictionary<Renderer, int>();
			for (int i = 0; i < lods.Length; i++)
			{
				LOD lOD = lods[i];
				for (int j = 0; j < lOD.renderers.Length; j++)
				{
					if (dictionary.TryGetValue(lOD.renderers[j], out var value))
					{
						lodsRenderers[i][j].SameRendererLodNum = value;
					}
					else
					{
						dictionary.Add(lOD.renderers[j], i);
					}
				}
			}
		}
	}

	public class Class653
	{
		public int VertexCount;

		public int SubMeshCount;

		public GameObject Prefab;

		public ObjType Type;

		public int SameRendererLodNum = -1;

		public Material[] Materials;

		[NonSerialized]
		public ShadowCastingMode ShadowCastingMode_0;

		[NonSerialized]
		public bool Bool_0;

		[NonSerialized]
		public LightProbeUsage LightProbeUsage_0;

		[NonSerialized]
		public ReflectionProbeUsage ReflectionProbeUsage_0;

		public Class653(Renderer renderer)
		{
			MeshFilter component = renderer.GetComponent<MeshFilter>();
			Prefab = smethod_0(renderer);
			if (component == null)
			{
				Type = ObjType.Billboard;
				return;
			}
			Mesh sharedMesh = component.sharedMesh;
			VertexCount = sharedMesh.vertexCount;
			SubMeshCount = sharedMesh.subMeshCount;
			Materials = renderer.materials;
			method_0(renderer);
			Type = ((!Materials[0].shader.name.Contains("SpeedTree")) ? ObjType.Mesh : ObjType.SpeedTree);
			if (Type == ObjType.SpeedTree)
			{
				Material[] materials = Materials;
				for (int i = 0; i < materials.Length; i++)
				{
					materials[i].shader = GClass976.SpeedTreeShader;
				}
			}
		}

		public void method_0(Renderer renderer)
		{
			ShadowCastingMode_0 = renderer.shadowCastingMode;
			Bool_0 = renderer.receiveShadows;
			LightProbeUsage_0 = renderer.lightProbeUsage;
			ReflectionProbeUsage_0 = renderer.reflectionProbeUsage;
		}

		public void SetRendererSettings(Renderer renderer)
		{
			renderer.sharedMaterials = Materials;
			renderer.shadowCastingMode = ShadowCastingMode_0;
			renderer.receiveShadows = Bool_0;
			renderer.lightProbeUsage = LightProbeUsage_0;
			renderer.reflectionProbeUsage = ReflectionProbeUsage_0;
		}

		public static GameObject smethod_0(Renderer renderer)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(renderer.gameObject);
			Component[] components = gameObject.GetComponents<Component>();
			foreach (Component component in components)
			{
				if (!(component is Transform) && !(component is Tree))
				{
					UnityEngine.Object.DestroyImmediate(component);
				}
			}
			return gameObject;
		}
	}

	public enum ObjType
	{
		SpeedTree,
		Billboard,
		Mesh
	}

	public Terrain Terrain;

	public Shader SpeedTreeShader;

	public bool SpawnTerrainPrefabs;

	public Transform SpawnTarget;

	public bool CombineOrdinaryMeshes;

	public float CellSize = 50f;

	public void OnDrawGizmosSelected()
	{
		if (!(Terrain == null))
		{
			Vector3 size = Terrain.terrainData.size;
			int num = (int)(size.x / CellSize);
			int num2 = (int)(size.z / CellSize);
			Vector2 vector = new Vector2(size.x / (float)num, size.z / (float)num2);
			Vector3 position = Terrain.GetPosition();
			Vector3 vector2 = position + size;
			Gizmos.color = Color.red;
			float num3 = position.x;
			for (int i = 0; i < num; i++)
			{
				Gizmos.DrawLine(new Vector3(num3, position.y, position.z), new Vector3(num3, position.y, vector2.z));
				num3 += vector.x;
			}
			float num4 = position.z;
			for (int j = 0; j < num; j++)
			{
				Gizmos.DrawLine(new Vector3(position.x, position.y, num4), new Vector3(vector2.x, position.y, num4));
				num4 += vector.y;
			}
		}
	}

	public static LODGroup[,][] smethod_0(Terrain terrain, IEnumerable<LODGroup> lodGroups, float cellSize)
	{
		Vector3 size = terrain.terrainData.size;
		int num = (int)(size.x / cellSize);
		int num2 = (int)(size.z / cellSize);
		Vector2 vector = new Vector2(size.x / (float)num, size.z / (float)num2);
		Vector2 vector2 = new Vector2(1f / vector.x, 1f / vector.y);
		List<LODGroup>[,] array = new List<LODGroup>[num, num2];
		foreach (LODGroup lodGroup in lodGroups)
		{
			Vector3 localPosition = lodGroup.transform.localPosition;
			int num3 = (int)(localPosition.x * vector2.x);
			int num4 = (int)(localPosition.z * vector2.y);
			(array[num3, num4] ?? (array[num3, num4] = new List<LODGroup>())).Add(lodGroup);
		}
		LODGroup[,][] array2 = new LODGroup[num, num2][];
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				if (array[j, i] != null)
				{
					array2[j, i] = array[j, i].ToArray();
				}
			}
		}
		return array2;
	}

	public static Dictionary<int, Class652> smethod_1(IEnumerable<LODGroup> lodGroups)
	{
		Dictionary<int, Class652> dictionary = new Dictionary<int, Class652>();
		foreach (LODGroup lodGroup in lodGroups)
		{
			int key = smethod_8(lodGroup);
			if (!dictionary.ContainsKey(key))
			{
				dictionary.Add(key, new Class652(lodGroup));
			}
		}
		return dictionary;
	}

	public static void UpdateTreesInteractiveParts(Terrain terrain)
	{
		Debug.LogWarning("UpdateAllTreesInteractiveParts is obsolete!");
	}

	public static void smethod_2(Terrain terrain, Transform spawnTarget)
	{
		TerrainData terrainData = terrain.terrainData;
		TreePrototype[] treePrototypes = terrainData.treePrototypes;
		TreeInstance[] treeInstances = terrainData.treeInstances;
		Vector3 size = terrainData.size;
		Vector3 position = terrain.GetPosition();
		TreeInstance[] array = treeInstances;
		for (int i = 0; i < array.Length; i++)
		{
			TreeInstance treeInstance = array[i];
			GameObject prefab = treePrototypes[treeInstance.prototypeIndex].prefab;
			Vector3 position2 = Vector3.Scale(treeInstance.position, size) + position;
			Quaternion rotation = Quaternion.AngleAxis(treeInstance.rotation * 57.29578f, Vector3.up);
			GameObject obj = UnityEngine.Object.Instantiate(prefab, position2, rotation);
			obj.transform.localScale = Vector3.one * treeInstance.heightScale;
			obj.transform.parent = spawnTarget;
		}
	}

	public static Dictionary<int, List<LODGroup>> smethod_3(LODGroup[] groups)
	{
		Dictionary<int, List<LODGroup>> dictionary = new Dictionary<int, List<LODGroup>>();
		foreach (LODGroup lODGroup in groups)
		{
			int key = smethod_8(lODGroup);
			if (!dictionary.TryGetValue(key, out var value))
			{
				dictionary.Add(key, value = new List<LODGroup>());
			}
			value.Add(lODGroup);
		}
		return dictionary;
	}

	public void method_0(LODGroup[,][] grid, Dictionary<int, Class652> prototypes)
	{
		int length = grid.GetLength(0);
		int length2 = grid.GetLength(1);
		for (int i = 0; i < length2; i++)
		{
			for (int j = 0; j < length; j++)
			{
				if (grid[j, i] != null)
				{
					method_1(grid[j, i], prototypes, "lod[" + j + ", " + i + "]");
				}
			}
		}
	}

	public void method_1(LODGroup[] cellGroups, Dictionary<int, Class652> prototypes, string name)
	{
		foreach (KeyValuePair<int, List<LODGroup>> item in smethod_3(cellGroups))
		{
			Class652 prototype = prototypes[item.Key];
			List<LODGroup> value = item.Value;
			LOD[][] array = new LOD[value.Count][];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = value[i].GetLODs();
			}
			List<Renderer>[] lodsRenderersList = method_2(prototype, array);
			foreach (LODGroup item2 in value)
			{
				UnityEngine.Object.DestroyImmediate(item2.gameObject);
			}
			method_6(lodsRenderersList, prototype, name);
		}
	}

	public List<Renderer>[] method_2(Class652 prototype, LOD[][] lods)
	{
		List<Renderer>[] array = new List<Renderer>[prototype.LodsRenderers.Length];
		for (int i = 0; i < prototype.LodsRenderers.Length; i++)
		{
			array[i] = new List<Renderer>();
			for (int j = 0; j < prototype.LodsRenderers[i].Length; j++)
			{
				Class653 @class = prototype.LodsRenderers[i][j];
				if (@class.SameRendererLodNum >= 0)
				{
					smethod_4(array, i, @class);
					continue;
				}
				switch (@class.Type)
				{
				case ObjType.SpeedTree:
					method_4(array, lods, i, j, @class);
					break;
				case ObjType.Billboard:
					smethod_6(array, lods, i, j, @class);
					break;
				case ObjType.Mesh:
					if (CombineOrdinaryMeshes)
					{
						method_3(array, lods, i, j, @class);
					}
					break;
				}
			}
		}
		return array;
	}

	public static void smethod_4(List<Renderer>[] lodsRenderersList, int lodNum, Class653 prototypeRenderer)
	{
		List<Renderer> obj = lodsRenderersList[prototypeRenderer.SameRendererLodNum];
		Material material = prototypeRenderer.Materials[0];
		foreach (Renderer item in obj)
		{
			if (item.sharedMaterial == material)
			{
				lodsRenderersList[lodNum].Add(item);
			}
		}
	}

	public static Mesh smethod_5(MeshFilter[] meshFilters, Vector3 commonPosition)
	{
		CombineInstance[] array = new CombineInstance[meshFilters.Length];
		int num = 0;
		foreach (MeshFilter meshFilter in meshFilters)
		{
			meshFilter.transform.position -= commonPosition;
			array[num++] = new CombineInstance
			{
				mesh = meshFilter.mesh,
				transform = meshFilter.transform.localToWorldMatrix
			};
		}
		Mesh mesh = new Mesh();
		mesh.name = "SpeedTreeTerrainProcessor CombineSimple";
		mesh.CombineMeshes(array, mergeSubMeshes: true, useMatrices: true);
		return mesh;
	}

	public void method_3(List<Renderer>[] lodsRenderersList, LOD[][] lods, int lodNum, int rendererNum, Class653 prototypeRenderer)
	{
		MeshFilter[] array = new MeshFilter[lods.Length];
		for (int i = 0; i < lods.Length; i++)
		{
			LOD[] array2 = lods[i];
			array[i] = array2[lodNum].renderers[rendererNum].GetComponent<MeshFilter>();
		}
		Vector3 zero = Vector3.zero;
		MeshFilter[] array3 = array;
		foreach (MeshFilter meshFilter in array3)
		{
			zero += meshFilter.transform.position;
		}
		zero /= (float)array.Length;
		Mesh sharedMesh = smethod_5(array, zero);
		GameObject obj = new GameObject("Mesh");
		obj.AddComponent<MeshFilter>().sharedMesh = sharedMesh;
		MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
		prototypeRenderer.SetRendererSettings(meshRenderer);
		obj.transform.position = zero;
		obj.transform.parent = Terrain.transform;
		array3 = array;
		for (int j = 0; j < array3.Length; j++)
		{
			UnityEngine.Object.Destroy(array3[j].gameObject);
		}
		lodsRenderersList[lodNum].Add(meshRenderer);
	}

	public static void smethod_6(List<Renderer>[] lodsRenderersList, LOD[][] lods, int lodNum, int rendererNum, Class653 prototypeRenderer)
	{
		for (int i = 0; i < lods.Length; i++)
		{
			Renderer renderer = lods[i][lodNum].renderers[rendererNum];
			renderer.transform.parent = null;
			lodsRenderersList[lodNum].Add(renderer);
		}
	}

	public void method_4(List<Renderer>[] lodsRenderersList, LOD[][] lods, int lodNum, int rendererNum, Class653 prototypeRenderer)
	{
		LinkedList<MeshFilter> linkedList = new LinkedList<MeshFilter>();
		int num = 0;
		foreach (LOD[] array in lods)
		{
			num += prototypeRenderer.VertexCount;
			if (num > 65535)
			{
				lodsRenderersList[lodNum].Add(method_5(linkedList, prototypeRenderer));
				linkedList.Clear();
				num = prototypeRenderer.VertexCount;
			}
			linkedList.AddLast(array[lodNum].renderers[rendererNum].GetComponent<MeshFilter>());
		}
		if (linkedList.Count > 0)
		{
			lodsRenderersList[lodNum].Add(method_5(linkedList, prototypeRenderer));
			linkedList.Clear();
		}
	}

	public MeshRenderer method_5(LinkedList<MeshFilter> meshFiltersList, Class653 prototypeRenderer)
	{
		Vector3 zero = Vector3.zero;
		foreach (MeshFilter meshFilters in meshFiltersList)
		{
			zero += meshFilters.transform.position;
		}
		zero /= (float)meshFiltersList.Count;
		Mesh sharedMesh = GClass976.Combine(meshFiltersList, zero, prototypeRenderer.SubMeshCount);
		GameObject obj = UnityEngine.Object.Instantiate(prototypeRenderer.Prefab);
		obj.AddComponent<MeshFilter>().sharedMesh = sharedMesh;
		MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
		prototypeRenderer.SetRendererSettings(meshRenderer);
		obj.transform.position = zero;
		obj.transform.parent = Terrain.transform;
		foreach (MeshFilter meshFilters2 in meshFiltersList)
		{
			UnityEngine.Object.Destroy(meshFilters2.gameObject);
		}
		return meshRenderer;
	}

	public void method_6(List<Renderer>[] lodsRenderersList, Class652 prototype, string name)
	{
		Vector3 zero = Vector3.zero;
		int num = 0;
		List<Renderer>[] array = lodsRenderersList;
		for (int i = 0; i < array.Length; i++)
		{
			foreach (Renderer item in array[i])
			{
				zero += item.transform.position;
				num++;
			}
		}
		zero /= (float)num;
		GameObject obj = new GameObject(name);
		LODGroup lodGroup = obj.AddComponent<LODGroup>();
		Transform transform = obj.transform;
		transform.position = zero;
		transform.SetParent(Terrain.transform);
		array = lodsRenderersList;
		for (int i = 0; i < array.Length; i++)
		{
			foreach (Renderer item2 in array[i])
			{
				item2.transform.SetParent(transform);
			}
		}
		smethod_7(lodGroup, lodsRenderersList, prototype.CullingRates);
	}

	public static void smethod_7(LODGroup lodGroup, List<Renderer>[] lodsRenderersList, float[] cullingRates)
	{
		LOD[] array = new LOD[lodsRenderersList.Length];
		for (int i = 0; i < array.Length; i++)
		{
			Renderer[] renderers = lodsRenderersList[i].ToArray();
			array[i] = new LOD(cullingRates[i], renderers);
		}
		lodGroup.SetLODs(array);
		lodGroup.fadeMode = LODFadeMode.CrossFade;
		lodGroup.animateCrossFading = true;
		lodGroup.size *= 0.3f;
	}

	public static int smethod_8(LODGroup lodGroup)
	{
		MeshFilter componentInChildren = lodGroup.GetComponentInChildren<MeshFilter>();
		if (componentInChildren == null)
		{
			return -1;
		}
		return componentInChildren.sharedMesh.GetInstanceID();
	}
}
