using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class HairRenderer : MonoBehaviour
{
	public enum DebugMode
	{
		DBG_NONE,
		DBG_OCCLUSION,
		DBG_GRAYMASK,
		DBG_MASKEDALBEDO,
		DBG_SPECULAR,
		DBG_LIGHTING,
		DBG_FLOW
	}

	public enum Mode
	{
		Original,
		StaticHeightBased,
		DynamicRadialDistance
	}

	public class Class690
	{
		public class Class691
		{
			public int i0;

			public int i1;

			public int i2;

			public Class691(int i0, int i1, int i2)
			{
				this.i0 = i0;
				this.i1 = i1;
				this.i2 = i2;
			}
		}

		public class Class692
		{
			public Class690 owner;

			public HashSet<int> indices;

			public HashSet<Class691> triangles;

			public Class692(Class690 owner, Class691 t)
			{
				this.owner = owner;
				indices = new HashSet<int>();
				triangles = new HashSet<Class691>();
				if (t != null)
				{
					AddTriangle(t);
				}
			}

			public bool AddTriangle(Class691 t)
			{
				if (triangles.Contains(t))
				{
					return false;
				}
				triangles.Add(t);
				indices.Add(t.i0);
				indices.Add(t.i1);
				indices.Add(t.i2);
				return true;
			}

			public bool Merge(Class692 p)
			{
				int num = 0;
				foreach (int index in p.indices)
				{
					if (indices.Contains(index))
					{
						if (++num >= 1)
						{
							break;
						}
						continue;
					}
					Vector3 vector = owner.Vector3_0[index];
					foreach (int index2 in indices)
					{
						Vector3 vector2 = owner.Vector3_0[index2];
						if (Vector3.SqrMagnitude(vector - vector2) <= 1.0000001E-06f && ++num >= 1)
						{
							goto end_IL_00ad;
						}
					}
					continue;
					end_IL_00ad:
					break;
				}
				if (num >= 1)
				{
					foreach (Class691 triangle in p.triangles)
					{
						AddTriangle(triangle);
					}
					return true;
				}
				return false;
			}
		}

		public struct Struct162(int[] i, Vector3 c, float l)
		{
			public Vector3 centroid = c;

			public float layer = l;

			public int[] indices = i;
		}

		[NonSerialized]
		public Vector3[] Vector3_0;

		[NonSerialized]
		public Vector2[] Vector2_0;

		[NonSerialized]
		public Color[] Color_0;

		[NonSerialized]
		public Struct162[] Struct162_0;

		[NonSerialized]
		public uint[] Uint_0;

		public Color[] staticPatchColors;

		public int[] staticPatchIndices;

		public Class690(Vector3[] vertices, Vector2[] uvs, Color[] colors, int[] indices, Transform space, Transform[] spheres)
		{
			Vector3_0 = vertices;
			Vector2_0 = uvs;
			Color_0 = colors;
			List<Class692> list = new List<Class692>
			{
				new Class692(this, new Class691(indices[0], indices[1], indices[2]))
			};
			Class692 @class = list[0];
			int i = 3;
			for (int num = indices.Length; i < num; i += 3)
			{
				Class692 class2 = new Class692(this, new Class691(indices[i], indices[i + 1], indices[i + 2]));
				if (!@class.Merge(class2))
				{
					list.Add(class2);
					@class = class2;
				}
			}
			staticPatchColors = new Color[vertices.Length];
			List<int> list2 = new List<int>();
			int num2 = 0;
			foreach (Class692 item in list)
			{
				foreach (Class691 triangle in item.triangles)
				{
					list2.Add(triangle.i0);
					list2.Add(triangle.i1);
					list2.Add(triangle.i2);
				}
				num2++;
			}
			staticPatchIndices = list2.ToArray();
			Uint_0 = new uint[list.Count];
			Struct162_0 = new Struct162[list.Count];
			int j = 0;
			for (int count = list.Count; j < count; j++)
			{
				Class692 class3 = list[j];
				Vector3 zero = Vector3.zero;
				float num3 = float.MaxValue;
				foreach (int index in class3.indices)
				{
					Vector3 vector = vertices[index];
					zero += vector;
					num3 = Mathf.Min(num3, space.TransformPoint(vector).y - space.position.y);
				}
				zero /= (float)class3.indices.Count;
				int[] array = new int[class3.triangles.Count * 3];
				int num4 = 0;
				foreach (Class691 triangle2 in class3.triangles)
				{
					array[num4++] = triangle2.i0;
					array[num4++] = triangle2.i1;
					array[num4++] = triangle2.i2;
				}
				Struct162_0[j] = new Struct162(array, zero, num3);
			}
		}

		public void method_0(int patchCount, int patchIdx, int offset, int[] indices)
		{
			float num = Mathf.Ceil((float)patchCount / 3f);
			float num2 = 2f * num;
			float num3 = patchIdx;
			Color color = default(Color);
			if (num3 <= num)
			{
				color.r = Mathf.Clamp01(num3 / num);
			}
			else if (num3 - num <= num)
			{
				color.g = Mathf.Clamp01((num3 - num) / num);
			}
			else
			{
				color.b = Mathf.Clamp01((num3 - num2) / num);
			}
			if (indices != null)
			{
				int i = 0;
				for (int num4 = indices.Length; i < num4; i++)
				{
					staticPatchColors[indices[i]] = color;
				}
			}
			else
			{
				staticPatchColors[offset] = color;
			}
		}

		public void BuildNormalizedPatches()
		{
			int i = 0;
			for (int num = Struct162_0.Length; i < num; i++)
			{
				int[] indices = Struct162_0[i].indices;
				float num2 = 1f;
				float num3 = 0f;
				int j = 0;
				for (int num4 = indices.Length; j < num4; j++)
				{
					Vector2 vector = Vector2_0[indices[j]];
					num2 = Mathf.Min(num2, vector.y);
					num3 = Mathf.Max(num3, vector.y);
				}
				float num5 = 1f / (num3 - num2);
				float num6 = num2;
				int k = 0;
				for (int num7 = indices.Length; k < num7; k++)
				{
					int num8 = indices[k];
					float num9 = Mathf.Clamp01((Vector2_0[num8].y - num6) * num5);
					float num10 = num9 * num9;
					float num11 = num10 * num9;
					float a = ((Color_0.Length != 0) ? Color_0[num8].a : 1f);
					staticPatchColors[num8] = new Color(1f - num9, 1f - num10, 1f - num11, a);
				}
			}
		}

		public void SortIndices(Vector3 eye, int[] indices, float distScale)
		{
			int i = 0;
			for (int num = Uint_0.Length; i < num; i++)
			{
				Struct162 @struct = Struct162_0[i];
				int num2 = Mathf.RoundToInt(Mathf.Clamp((@struct.layer + Vector3.SqrMagnitude(eye - @struct.centroid) * distScale) * 100f, 0f, 1048575f));
				Uint_0[i] = (uint)(((num2 & 0xFFFFF) << 20) | (i & 0xFFF));
			}
			Array.Sort(Uint_0);
			int j = 0;
			int num3 = Uint_0.Length;
			int num4 = 0;
			for (; j < num3; j++)
			{
				uint num5 = Uint_0[j] & 0xFFF;
				int[] indices2 = Struct162_0[num5].indices;
				Array.Copy(indices2, 0, indices, num4, indices2.Length);
				num4 += indices2.Length;
			}
		}
	}

	public Renderer sourceRenderer;

	public Mode mode = Mode.StaticHeightBased;

	public bool useOpaquePass = true;

	public float opaqueAlphaRef = 0.8f;

	public bool frontWriteDepth;

	public float frontBackAlphaRef = 0.01f;

	public DebugMode debugMode;

	[HideInInspector]
	public Transform[] headSpheres;

	[HideInInspector]
	public Transform headShell;

	[HideInInspector]
	public float sortDistanceScale = 1f;

	private Mesh mesh_0;

	private Mesh mesh_1;

	private int[] int_0;

	private MeshFilter meshFilter_0;

	private Class690 class690_0;

	private Material material_0;

	private Material material_1;

	private Material material_2;

	private static readonly int int_1 = Shader.PropertyToID("_SrcBlend");

	private static readonly int int_2 = Shader.PropertyToID("_DstBlend");

	private static readonly int int_3 = Shader.PropertyToID("_ZWrite");

	private static readonly int int_4 = Shader.PropertyToID("_ZTest");

	private static readonly int int_5 = Shader.PropertyToID("_Cull");

	private static readonly int int_6 = Shader.PropertyToID("_Cutoff");

	public void Awake()
	{
		debugMode = DebugMode.DBG_NONE;
		base.gameObject.layer = sourceRenderer.gameObject.layer;
		if (sourceRenderer is MeshRenderer)
		{
			mesh_0 = ((MeshRenderer)sourceRenderer).GetComponent<MeshFilter>().sharedMesh;
		}
		else if (sourceRenderer is SkinnedMeshRenderer)
		{
			mesh_0 = ((SkinnedMeshRenderer)sourceRenderer).sharedMesh;
		}
		else
		{
			Debug.LogError("Invalid source renderer type");
		}
		method_0();
		method_1();
		method_2();
		method_3();
		method_5();
	}

	public void method_0()
	{
		Vector3[] vertices = mesh_0.vertices;
		Vector2[] uv = mesh_0.uv;
		Color[] colors = mesh_0.colors;
		int[] triangles = mesh_0.triangles;
		int_0 = new int[triangles.Length];
		class690_0 = new Class690(vertices, uv, colors, triangles, sourceRenderer.transform, headSpheres);
		class690_0.BuildNormalizedPatches();
		class690_0.SortIndices(Vector3.zero, int_0, 0f);
		mesh_1 = new Mesh
		{
			name = "HairRenderer m_sortedMesh"
		};
		mesh_1.vertices = vertices;
		mesh_1.uv = uv;
		mesh_1.normals = mesh_0.normals;
		mesh_1.tangents = mesh_0.tangents;
		mesh_1.colors = class690_0.staticPatchColors;
		mesh_1.triangles = int_0;
		mesh_1.bindposes = mesh_0.bindposes;
		mesh_1.boneWeights = mesh_0.boneWeights;
		mesh_0.RecalculateBounds();
	}

	public void method_1()
	{
		Material sharedMaterial = sourceRenderer.sharedMaterial;
		material_0 = new Material(sharedMaterial);
		material_0.name = "HairOpaque";
		material_0.renderQueue = 2450;
		material_0.shaderKeywords = sharedMaterial.shaderKeywords;
		material_0.DisableKeyword("_ALPHABLEND_ON");
		material_0.EnableKeyword("_ALPHATEST_ON");
		material_0.SetInt(int_1, 1);
		material_0.SetInt(int_2, 0);
		material_0.SetInt(int_3, 1);
		material_0.SetInt(int_4, 4);
		material_0.SetInt(int_5, 0);
		material_1 = new Material(sharedMaterial);
		material_1.name = "HairBack";
		material_1.renderQueue = 2521;
		material_1.shaderKeywords = sharedMaterial.shaderKeywords;
		material_1.EnableKeyword("_ALPHABLEND_ON");
		material_1.SetInt(int_1, 5);
		material_1.SetInt(int_2, 10);
		material_1.SetInt(int_4, 2);
		material_1.SetInt(int_5, 1);
		material_2 = new Material(sharedMaterial);
		material_2.name = "HairFront";
		material_2.renderQueue = 2522;
		material_2.shaderKeywords = sharedMaterial.shaderKeywords;
		material_2.EnableKeyword("_ALPHABLEND_ON");
		material_2.SetInt(int_1, 5);
		material_2.SetInt(int_2, 10);
		material_2.SetInt(int_4, 2);
		material_2.SetInt(int_5, 2);
	}

	public void method_2()
	{
		material_0.SetFloat(int_6, opaqueAlphaRef);
		material_2.SetInt(int_3, frontWriteDepth ? 1 : 0);
		material_1.SetInt(int_3, 0);
		material_2.SetFloat(int_6, frontBackAlphaRef);
		material_1.SetFloat(int_6, frontBackAlphaRef);
		if (frontBackAlphaRef > 0f)
		{
			material_2.EnableKeyword("_ALPHATEST_ON");
			material_1.EnableKeyword("_ALPHATEST_ON");
		}
		else
		{
			material_2.DisableKeyword("_ALPHATEST_ON");
			material_1.DisableKeyword("_ALPHATEST_ON");
		}
	}

	public void method_3()
	{
		Material[] array = new Material[3] { material_0, material_1, material_2 };
		foreach (Material material in array)
		{
			string[] names = Enum.GetNames(typeof(DebugMode));
			foreach (string keyword in names)
			{
				material.DisableKeyword(keyword);
			}
			material.EnableKeyword(debugMode.ToString());
		}
	}

	public Material method_4(Material m)
	{
		GameObject gameObject = new GameObject(m.name);
		gameObject.layer = base.gameObject.layer;
		gameObject.transform.parent = base.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		Material material = m;
		if (sourceRenderer is MeshRenderer)
		{
			gameObject.AddComponent<MeshFilter>().sharedMesh = mesh_1;
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
			meshRenderer.material = m;
			material = meshRenderer.material;
			meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
			meshRenderer.receiveShadows = true;
		}
		else
		{
			SkinnedMeshRenderer skinnedMeshRenderer = gameObject.AddComponent<SkinnedMeshRenderer>();
			skinnedMeshRenderer.rootBone = (sourceRenderer as SkinnedMeshRenderer).rootBone;
			skinnedMeshRenderer.bones = (sourceRenderer as SkinnedMeshRenderer).bones;
			skinnedMeshRenderer.material = m;
			material = skinnedMeshRenderer.material;
			skinnedMeshRenderer.sharedMesh = mesh_1;
		}
		return material;
	}

	public void method_5()
	{
		foreach (object item in base.transform)
		{
			UnityEngine.Object.Destroy((item as Transform).gameObject);
		}
		sourceRenderer.enabled = false;
		if (useOpaquePass)
		{
			material_0 = method_4(material_0);
		}
		material_1 = method_4(material_1);
		material_2 = method_4(material_2);
	}

	public void OnValidate()
	{
		if (mode == Mode.DynamicRadialDistance)
		{
			Debug.LogWarning("DynamicRadialDistance is not really supported. Falling back to StaticHeightBased. (change code if you really want to try this)");
			mode = Mode.StaticHeightBased;
		}
		if ((bool)mesh_0)
		{
			sortDistanceScale = Mathf.Clamp01(sortDistanceScale);
			opaqueAlphaRef = Mathf.Clamp(opaqueAlphaRef, 0f, 1.001f);
			frontBackAlphaRef = Mathf.Clamp(frontBackAlphaRef, 0f, 1.001f);
			method_2();
			method_3();
			method_5();
		}
	}

	public void LateUpdate()
	{
		if (!(Camera.current == null) && mode == Mode.DynamicRadialDistance)
		{
			Vector3 position = Camera.current.transform.position;
			Vector3 direction = headShell.transform.position - position;
			Ray ray = new Ray(position, direction);
			if (headShell.GetComponent<SphereCollider>().Raycast(ray, out var hitInfo, direction.magnitude))
			{
				class690_0.SortIndices(base.transform.InverseTransformPoint(hitInfo.point), int_0, sortDistanceScale);
				Debug.DrawLine(position, hitInfo.point, Color.red, 3f);
				mesh_1.triangles = int_0;
				mesh_1.colors = class690_0.staticPatchColors;
			}
			else
			{
				Debug.LogWarning("Failed to find head ray.. inside shell?");
			}
		}
	}
}
