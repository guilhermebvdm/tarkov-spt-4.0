using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using EFT.Interactive;
using Koenigz.PerfectCulling;
using Koenigz.PerfectCulling.EFT;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public abstract class GClass1228
{
	[Serializable]
	[CompilerGenerated]
	public class Class832
	{
		public static readonly Class832 class832_0 = new Class832();

		public static Func<Vertex, Vector3> func_0;

		public static Predicate<LODGroup> predicate_0;

		public static Comparison<LOD> comparison_0;

		public Vector3 method_0(Vertex o)
		{
			return new Vector3
			{
				x = o.position.x,
				y = o.position.y,
				z = o.position.z
			};
		}

		public bool method_1(LODGroup lodGroup)
		{
			if (lodGroup.enabled)
			{
				return !lodGroup.gameObject.activeInHierarchy;
			}
			return true;
		}

		public int method_2(LOD x, LOD y)
		{
			if (!(x.screenRelativeTransitionHeight < y.screenRelativeTransitionHeight))
			{
				return -1;
			}
			return 1;
		}
	}

	[CompilerGenerated]
	public class Class833
	{
		public List<int> indices;

		public void method_0(Face o)
		{
			indices.AddRange(o.indexes.ToList());
		}
	}

	[CompilerGenerated]
	public class Class834
	{
		public Func<Renderer, bool> filter;

		public bool method_0(Renderer rend)
		{
			if (filter != null)
			{
				return !filter(rend);
			}
			return false;
		}
	}

	public const string CULLING_DATA_PATH = "Culling_Data";

	public static readonly Type[] SHARED_OCCLUDER_IGNORE_TYPES = new Type[7]
	{
		typeof(Door),
		typeof(WindowBreaker),
		typeof(StencilShadow),
		typeof(AreaLight),
		typeof(FloatingObject),
		typeof(LampController),
		typeof(Trunk)
	};

	public static readonly Type[] IGNORE_TYPES = new Type[5]
	{
		typeof(Door),
		typeof(StencilShadow),
		typeof(AreaLight),
		typeof(FloatingObject),
		typeof(Trunk)
	};

	[NonSerialized]
	public static string[] String_0 = new string[2] { "_ALPHATEST_ON", "ALPHACLIPPING_ON" };

	[NonSerialized]
	public const string String_1 = "SHADOW";

	[NonSerialized]
	public const string String_2 = "STENCIL";

	public static void SetActiveScene(GameObject obj)
	{
		SceneManager.SetActiveScene(obj.scene);
	}

	public static GClass877.GStruct59[] PrepareNativeMeshRendererData(List<Renderer> inputRenderers, Dictionary<Renderer, int> groupMappingDict)
	{
		Dictionary<Mesh, List<Renderer>> dictionary = new Dictionary<Mesh, List<Renderer>>();
		foreach (Renderer inputRenderer in inputRenderers)
		{
			MeshFilter component = inputRenderer.GetComponent<MeshFilter>();
			if (!(component == null) && !(component.sharedMesh == null))
			{
				if (dictionary.TryGetValue(component.sharedMesh, out var value))
				{
					value.Add(inputRenderer);
					continue;
				}
				dictionary.Add(component.sharedMesh, new List<Renderer> { inputRenderer });
			}
		}
		List<GClass877.GStruct59> list = new List<GClass877.GStruct59>();
		foreach (KeyValuePair<Mesh, List<Renderer>> item3 in dictionary)
		{
			List<Renderer> value2 = item3.Value;
			Mesh key = item3.Key;
			Vector3[] vertices = key.vertices;
			int[] indices = key.GetIndices(0);
			Color[] colors = key.colors;
			GClass877.GStruct57 meshData = new GClass877.GStruct57
			{
				vertCount = vertices.Length,
				verts = vertices,
				indCount = indices.Length,
				indices = indices,
				colorCount = colors.Length,
				colors = colors
			};
			List<GClass877.GStruct58> list2 = new List<GClass877.GStruct58>(value2.Count);
			List<int> list3 = new List<int>();
			foreach (Renderer item4 in value2)
			{
				Matrix4x4 localToWorldMatrix = item4.localToWorldMatrix;
				Bounds bounds = item4.bounds;
				GClass877.GStruct58 item = new GClass877.GStruct58
				{
					boundsCenter = bounds.center,
					boundsSize = bounds.size,
					mat4x4 = localToWorldMatrix
				};
				list2.Add(item);
				list3.Add(0);
			}
			GClass877.GStruct59 item2 = new GClass877.GStruct59
			{
				meshData = meshData,
				transformations = list2.ToArray(),
				transformationCount = list2.Count,
				renderMode = 0
			};
			list.Add(item2);
		}
		return list.ToArray();
	}

	public static GameObject CreateLightProxy(PerfectCullingBakeGroup light, Material replacementMaterial, Color c, Transform root = null)
	{
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		gameObject.transform.localScale = light.lightBakeSize * Vector3.one;
		gameObject.transform.position = light.lightBakePosition;
		UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<Collider>());
		gameObject.name = light.cullingLightObjects[0].gameObject.name;
		if (root != null)
		{
			gameObject.transform.SetParent(root.transform, worldPositionStays: true);
		}
		MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		c.a = 150f;
		materialPropertyBlock.SetColor("_Color", c);
		component.SetPropertyBlock(materialPropertyBlock);
		component.sharedMaterial = replacementMaterial;
		return gameObject;
	}

	public static GameObject CreateLightProxy(Light light, Material replacementMaterial, Color c, Transform root = null)
	{
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		gameObject.transform.localScale = light.range * Vector3.one;
		gameObject.transform.position = light.transform.position;
		gameObject.transform.rotation = light.transform.rotation;
		gameObject.name = light.gameObject.name;
		if (root != null)
		{
			gameObject.transform.SetParent(root.transform, worldPositionStays: true);
		}
		MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		c.a = 50f;
		materialPropertyBlock.SetColor("_Color", c);
		component.SetPropertyBlock(materialPropertyBlock);
		component.sharedMaterial = replacementMaterial;
		return gameObject;
	}

	public static string GetEditorCullingDataFolder(Scene scene)
	{
		return Path.GetDirectoryName(scene.path) + "\\Culling_Data";
	}

	public static void CreateCullingDataFolder(GameObject cullingVolume)
	{
		string editorCullingDataFolder = GetEditorCullingDataFolder(cullingVolume.gameObject.scene);
		if (!Directory.Exists(editorCullingDataFolder))
		{
			Directory.CreateDirectory(editorCullingDataFolder);
		}
	}

	public static List<Vector3> GenerateOffsets(int numOffsetsByAxis, Vector3 cellSize)
	{
		List<Vector3> list = new List<Vector3>();
		for (int i = -numOffsetsByAxis; i <= numOffsetsByAxis; i++)
		{
			for (int j = -numOffsetsByAxis; j <= numOffsetsByAxis; j++)
			{
				for (int k = -numOffsetsByAxis; k <= numOffsetsByAxis; k++)
				{
					list.Add(new Vector3((float)i * cellSize.x, (float)j * cellSize.y, (float)k * cellSize.z));
				}
			}
		}
		return list;
	}

	public static uint FromString(string str)
	{
		return FromBytes(Encoding.ASCII.GetBytes(str));
	}

	public static uint FromBytes(byte[] data)
	{
		return FromBytes(data, 0u);
	}

	public static uint FromBytes(byte[] data, uint seed)
	{
		uint num = seed;
		foreach (byte b in data)
		{
			num += b;
			num += num << 10;
			num ^= num >> 6;
		}
		num += num << 3;
		num ^= num >> 11;
		return num + (num << 15);
	}

	public static long HashString(string str)
	{
		return (long)FromString64(str);
	}

	public static int HashStringInt32(string str)
	{
		return (int)FromString(str);
	}

	public static ulong FromString64(string str)
	{
		return FromBytes64(Encoding.ASCII.GetBytes(str));
	}

	public static ulong FromBytes64(byte[] data)
	{
		uint num = FromBytes(data, 0u);
		uint num2 = FromBytes(data, num);
		return ((ulong)num << 32) + num2;
	}

	public static Vector3 Snap(Vector3 p, float s)
	{
		Vector3 result = default(Vector3);
		result.x = (float)(int)(p.x / s) * s;
		result.y = (float)(int)(p.y / s) * s;
		result.z = (float)(int)(p.z / s) * s;
		return result;
	}

	public static bool ContainsTransparentMaterial(Material[] sharedMaterials)
	{
		int num = 0;
		while (true)
		{
			if (num < sharedMaterials.Length)
			{
				if (IsMaterialTransparent(sharedMaterials[num]))
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		return true;
	}

	public static List<T> GetByTypeInActiveScene<T>() where T : Component
	{
		int buildIndex = SceneManager.GetActiveScene().buildIndex;
		List<T> list = new List<T>();
		T[] array = UnityEngine.Object.FindObjectsOfType<T>();
		foreach (T val in array)
		{
			if (val.gameObject.scene.buildIndex == buildIndex)
			{
				list.Add(val);
			}
		}
		return list;
	}

	public static T ContainsComponentOnParentInHierarchy<T>(Transform obj) where T : Component
	{
		Transform parent = obj.parent;
		T component;
		while (true)
		{
			if (parent != null)
			{
				component = parent.GetComponent<T>();
				if (component != null)
				{
					break;
				}
				parent = parent.parent;
				continue;
			}
			return null;
		}
		return component;
	}

	public static bool IsSharedOccluderHierarchy(Transform t)
	{
		MultisceneSharedOccluder multisceneSharedOccluder = ContainsComponentOnParentInHierarchy<MultisceneSharedOccluder>(t);
		if (multisceneSharedOccluder == null)
		{
			return false;
		}
		if (multisceneSharedOccluder.OccludeMode != EOccludeMode.SharedOccludeeOccluder)
		{
			return multisceneSharedOccluder.OccludeMode == EOccludeMode.SharedOccluder;
		}
		return true;
	}

	public static bool ContainsComponentInHierarchy(Transform obj, Type[] types, Transform maxTop = null)
	{
		Transform transform = obj;
		while (transform != null)
		{
			foreach (Type type in types)
			{
				if (transform.GetComponent(type) != null)
				{
					return true;
				}
			}
			transform = transform.parent;
			if (maxTop != null && transform == maxTop)
			{
				break;
			}
		}
		return false;
	}

	public static LODGroup TryGetParentLODGroup(Transform source)
	{
		LODGroup lODGroup = null;
		Transform transform = source;
		while (true)
		{
			if (transform != null)
			{
				lODGroup = transform.GetComponent<LODGroup>();
				if (lODGroup != null)
				{
					break;
				}
				transform = transform.parent;
				continue;
			}
			return lODGroup;
		}
		return lODGroup;
	}

	public static bool ContainsComponentOnParentInHierarchyEx<T1, T2>(Transform obj, Transform maxTop = null) where T1 : Component where T2 : Component
	{
		Transform transform = obj;
		while (true)
		{
			if (transform != null)
			{
				if (transform.GetComponent<T1>() != null || transform.GetComponent<T2>() != null)
				{
					break;
				}
				transform = transform.parent;
				if (!(maxTop != null) || !(transform == maxTop))
				{
					continue;
				}
			}
			return false;
		}
		return true;
	}

	public static Vector3 GetTransformWorldScale(Transform tr)
	{
		return tr.lossyScale;
	}

	public static MeshFilter ConvertFromProbuilder(GameObject targetObject, ProBuilderMesh pbMesh)
	{
		Debug.Log("using probuilder mesh for " + targetObject.name);
		Mesh mesh = new Mesh();
		mesh.SetVertices((from o in pbMesh.GetVertices()
			select new Vector3
			{
				x = o.position.x,
				y = o.position.y,
				z = o.position.z
			}).ToList());
		List<int> indices = new List<int>();
		pbMesh.faces.ToList().ForEach(delegate(Face o)
		{
			indices.AddRange(o.indexes.ToList());
		});
		mesh.SetIndices(indices, MeshTopology.Triangles, 0);
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		MeshFilter meshFilter = targetObject.GetComponent<MeshFilter>();
		if (meshFilter == null)
		{
			meshFilter = targetObject.AddComponent<MeshFilter>();
		}
		meshFilter.sharedMesh = mesh;
		return meshFilter;
	}

	public static MeshRenderer CloneRenderer(MeshRenderer renderer, Material replacementMaterial)
	{
		GameObject gameObject = new GameObject();
		gameObject.name = renderer.name;
		gameObject.transform.position = renderer.transform.position;
		gameObject.transform.rotation = renderer.transform.rotation;
		gameObject.transform.localScale = GetTransformWorldScale(renderer.transform);
		gameObject.layer = renderer.gameObject.layer;
		renderer.GetComponent<ProBuilderMesh>();
		gameObject.AddComponent<MeshFilter>().sharedMesh = renderer.GetComponent<MeshFilter>().sharedMesh;
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.sharedMaterials = MakeReplacementMaterials(renderer.sharedMaterials.Length, replacementMaterial);
		return meshRenderer;
	}

	public static Material[] MakeReplacementMaterials(int num, Material replacement)
	{
		Material[] array = new Material[num];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = replacement;
		}
		return array;
	}

	public static string FormatSeconds(double seconds)
	{
		return TimeSpan.FromSeconds(seconds).ToString("hh\\:mm\\:ss");
	}

	public static HashSet<PerfectCullingBakeGroup> CreateBakeGroupsForRenderersEFT(List<Renderer> inputRenderers, Func<Renderer, bool> filter, Transform sourceGroup)
	{
		inputRenderers.RemoveAll((Renderer rend) => filter != null && !filter(rend));
		HashSet<Renderer> hashSet = new HashSet<Renderer>(inputRenderers);
		HashSet<LODGroup> hashSet2 = new HashSet<LODGroup>(sourceGroup.GetComponentsInChildren<LODGroup>());
		hashSet2.RemoveWhere((LODGroup lodGroup) => !lodGroup.enabled || !lodGroup.gameObject.activeInHierarchy);
		HashSet<PerfectCullingBakeGroup> hashSet3 = new HashSet<PerfectCullingBakeGroup>(new GClass1213());
		foreach (LODGroup item in hashSet2)
		{
			HashSet<Renderer> hashSet4 = new HashSet<Renderer>();
			LOD[] lODs = item.GetLODs();
			for (int num = 0; num < lODs.Length; num++)
			{
				Renderer[] renderers = lODs[num].renderers;
				foreach (Renderer renderer in renderers)
				{
					hashSet.Remove(renderer);
					if (renderer == null)
					{
						Debug.LogWarning("Found null renderer in LODGroup: " + item.name + ". Selecting the LODGroup might remove the invalid renderer(s) for you.", item.gameObject);
					}
					else if (renderer.enabled && renderer.gameObject.activeInHierarchy && !IsShadowRenderer(renderer))
					{
						if (GClass1229.HasNullMesh(renderer))
						{
							Debug.LogWarning("Null mesh in: " + renderer.name + ".", renderer.gameObject);
						}
						else if (IsValidRendererType(renderer))
						{
							hashSet4.Add(renderer);
						}
					}
				}
			}
			if (hashSet4.Count != 0 && ShouldProcessLODGroup(item))
			{
				hashSet3.Add(new PerfectCullingBakeGroup
				{
					renderers = hashSet4.ToArray(),
					groupType = PerfectCullingBakeGroup.GroupType.LOD
				});
			}
		}
		foreach (Renderer item2 in hashSet)
		{
			if (item2 != null && !ContainsComponentInHierarchy(item2.transform, IGNORE_TYPES, item2.transform) && !IsShadowRenderer(item2))
			{
				hashSet3.Add(new PerfectCullingBakeGroup
				{
					renderers = new Renderer[1] { item2 },
					groupType = PerfectCullingBakeGroup.GroupType.Other
				});
			}
		}
		foreach (PerfectCullingBakeGroup item3 in hashSet3)
		{
			if (item3.ContainsRenderers())
			{
				MultisceneSharedOccluder multisceneSharedOccluder = ContainsComponentOnParentInHierarchy<MultisceneSharedOccluder>(item3.renderers[0].transform);
				EOccludeMode occludeMode = ((multisceneSharedOccluder != null) ? multisceneSharedOccluder.OccludeMode : EOccludeMode.OccludeeOccluder);
				item3.occludeMode = occludeMode;
			}
		}
		return hashSet3;
	}

	public static void CheckRendererTypes(IEnumerable<Renderer> rends)
	{
		foreach (Renderer rend in rends)
		{
			if (!GClass1224.SupportedRendererTypes.Contains(rend.GetType()))
			{
				Debug.LogError("Invalid renderer type " + rend.GetType().Name, rend.gameObject);
			}
		}
	}

	public static bool IsValidRendererType(Renderer r)
	{
		return GClass1224.SupportedRendererTypes.Contains(r.GetType());
	}

	public static bool ShouldProcessRenderer(Renderer rend)
	{
		Type[] iGNORE_TYPES = IGNORE_TYPES;
		int num = 0;
		while (true)
		{
			if (num < iGNORE_TYPES.Length)
			{
				Type type = iGNORE_TYPES[num];
				if (rend.GetComponent(type) != null)
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

	public static bool ShouldProcessLODGroup(LODGroup lodGroup)
	{
		LOD[] lODs = lodGroup.GetLODs();
		for (int i = 0; i < lODs.Length; i++)
		{
			Renderer[] renderers = lODs[i].renderers;
			foreach (Renderer renderer in renderers)
			{
				if (!(renderer == null) && ContainsComponentInHierarchy(renderer.transform, IGNORE_TYPES, lodGroup.transform))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static bool IsMaterialTransparent(Material mat)
	{
		if (mat == null)
		{
			return false;
		}
		if (PerfectCullingMaterialSettings.IsMaterialForcedTransparent(mat))
		{
			return true;
		}
		if (PerfectCullingMaterialSettings.IsMaterialForcedOpaque(mat))
		{
			return false;
		}
		string text = mat.name.ToLower();
		if (text.Contains("pc_trans"))
		{
			return true;
		}
		if (text.Contains("pc_opaque"))
		{
			return false;
		}
		string[] string_ = String_0;
		int num = 0;
		while (true)
		{
			if (num < string_.Length)
			{
				string keyword = string_[num];
				if (mat.IsKeywordEnabled(keyword))
				{
					break;
				}
				num++;
				continue;
			}
			Shader[] validTransparentShaders = PerfectCullingResourcesLocator.Instance.ValidTransparentShaders;
			num = 0;
			while (true)
			{
				if (num < validTransparentShaders.Length)
				{
					Shader shader = validTransparentShaders[num];
					if (mat.shader == shader)
					{
						break;
					}
					num++;
					continue;
				}
				return mat.renderQueue >= 2450;
			}
			return true;
		}
		return true;
	}

	public static bool IsDecalRenderer(Renderer renderer)
	{
		if (renderer.gameObject.name.ToUpper().Contains("DECAL"))
		{
			return true;
		}
		return false;
	}

	public static bool IsShadowRenderer(Renderer rend)
	{
		if (rend.shadowCastingMode == ShadowCastingMode.ShadowsOnly)
		{
			return true;
		}
		string text = rend.gameObject.name.ToUpper();
		if (!text.Contains("SHADOW") && !text.Contains("STENCIL"))
		{
			return false;
		}
		return true;
	}

	public static Dictionary<Renderer, int> GetFirstLODs(PerfectCullingCrossSceneGroup group)
	{
		PerfectCullingCrossSceneGroupPreProcess component = group.GetComponent<PerfectCullingCrossSceneGroupPreProcess>();
		PerfectCullingBakeGroup[] array = ((component == null) ? group.bakeGroups : component.PrepareRuntimeContent());
		List<Renderer> list = new List<Renderer>();
		Dictionary<Renderer, int> dictionary = new Dictionary<Renderer, int>();
		for (int i = 0; i < array.Length; i++)
		{
			Renderer[] renderers = array[i].renderers;
			foreach (Renderer renderer in renderers)
			{
				int value;
				if (renderer == null)
				{
					Debug.LogError("Group contains null renderer " + group.gameObject.name);
				}
				else if (dictionary.TryGetValue(renderer, out value))
				{
					if (i != value)
					{
						Debug.LogError($"Duplicated renderer {renderer.name} (group {i}, current {value}), ignoring", renderer);
					}
				}
				else if (!IsShadowRenderer(renderer))
				{
					list.Add(renderer);
					dictionary.Add(renderer, i);
				}
			}
		}
		LODGroup[] lODGroups = group.GetLODGroups();
		HashSet<Renderer> hashSet = new HashSet<Renderer>(list);
		HashSet<Renderer> hashSet2 = new HashSet<Renderer>();
		HashSet<Renderer> hashSet3 = new HashSet<Renderer>();
		LODGroup[] array2 = lODGroups;
		for (int j = 0; j < array2.Length; j++)
		{
			LOD[] lODs = array2[j].GetLODs();
			for (int k = 0; k < lODs.Length; k++)
			{
				Renderer[] renderers = lODs[k].renderers;
				foreach (Renderer renderer2 in renderers)
				{
					if (!(renderer2 == null) && renderer2.enabled && renderer2.gameObject.activeInHierarchy)
					{
						if (k == 0)
						{
							hashSet2.Add(renderer2);
						}
						else
						{
							hashSet3.Add(renderer2);
						}
					}
					else
					{
						hashSet3.Add(renderer2);
					}
				}
			}
		}
		foreach (Renderer item in hashSet3)
		{
			if (!hashSet2.Contains(item) && hashSet3.Contains(item) && hashSet.Contains(item))
			{
				dictionary.Remove(item);
			}
		}
		return dictionary;
	}

	public static void smethod_0(Renderer rend, Material opaque, Material transparent, GameObject root = null, ICollection<Renderer> outLst = null)
	{
		if (rend.shadowCastingMode != ShadowCastingMode.ShadowsOnly && rend != null && rend is MeshRenderer)
		{
			MeshRenderer item = CloneRenderer(rend as MeshRenderer, opaque);
			outLst.Add(item);
		}
	}

	public static List<Renderer> smethod_1(LODGroup original, Material opaque, Material transparent, GameObject root = null)
	{
		List<Renderer> list = new List<Renderer>();
		GameObject gameObject = new GameObject();
		gameObject.name = original.gameObject.name;
		gameObject.layer = 30;
		LODGroup lODGroup = gameObject.AddComponent<LODGroup>();
		List<Renderer> list2 = new List<Renderer>();
		List<LOD> list3 = new List<LOD>();
		LOD[] lODs = original.GetLODs();
		LOD item = default(LOD);
		for (int i = 0; i < lODs.Length; i++)
		{
			LOD lOD = lODs[i];
			Renderer[] renderers = lOD.renderers;
			foreach (Renderer renderer in renderers)
			{
				if (!(renderer == null))
				{
					smethod_0(renderer, opaque, transparent, root, list2);
				}
			}
			foreach (Renderer item2 in list2)
			{
				item2.transform.SetParent(lODGroup.transform);
				item2.gameObject.layer = 30;
			}
			Renderer[] array = list2.ToArray();
			list.AddRange(array);
			item.renderers = array;
			item.screenRelativeTransitionHeight = lOD.screenRelativeTransitionHeight;
			item.fadeTransitionWidth = lOD.fadeTransitionWidth;
			list3.Add(item);
			list2.Clear();
		}
		try
		{
			list3.Sort((LOD x, LOD y) => (x.screenRelativeTransitionHeight < y.screenRelativeTransitionHeight) ? 1 : (-1));
			lODGroup.SetLODs(list3.ToArray());
			lODGroup.RecalculateBounds();
		}
		catch (Exception)
		{
			Debug.LogError("LOD Error: " + original.gameObject.name);
		}
		if (root != null)
		{
			gameObject.transform.SetParent(root.transform, worldPositionStays: true);
		}
		return list;
	}

	public static void GenerateBakeVisuals(PerfectCullingCrossSceneGroup crossGroup)
	{
	}
}
