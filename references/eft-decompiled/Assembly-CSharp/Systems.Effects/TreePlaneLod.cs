using System.Collections.Generic;
using UnityEngine;

namespace Systems.Effects;

public class TreePlaneLod : MonoBehaviour
{
	private static LinkedList<TreePlaneLod> linkedList_0 = new LinkedList<TreePlaneLod>();

	private static TreePlaneLod[] treePlaneLod_0;

	private static Vector3[] vector3_0;

	private static Vector3[] vector3_1;

	private static Vector4[] vector4_0;

	private static Vector2[] vector2_0;

	private static Vector2[] vector2_1;

	private static int[] int_0;

	private static Mesh mesh_0;

	private static Vector3 vector3_2 = Vector3.zero;

	private static Vector3 vector3_3 = Vector3.zero;

	private static bool bool_0 = false;

	public float Height;

	public float Width;

	public Material Mat;

	private int int_1;

	public void Awake()
	{
		linkedList_0.AddLast(this);
	}

	public void Start()
	{
		if (bool_0)
		{
			return;
		}
		bool_0 = true;
		int count = linkedList_0.Count;
		treePlaneLod_0 = new TreePlaneLod[count];
		int num = 0;
		foreach (TreePlaneLod item in linkedList_0)
		{
			treePlaneLod_0[num] = item;
			item.int_1 = num++;
		}
		int num2 = count << 2;
		vector3_0 = new Vector3[num2];
		vector3_1 = new Vector3[num2];
		vector4_0 = new Vector4[num2];
		vector2_0 = new Vector2[num2];
		vector2_1 = new Vector2[num2];
		int_0 = new int[count * 6];
		int i = 0;
		int num3 = 0;
		for (; i < count; i++)
		{
			int num4 = i << 2;
			int_0[num3++] = num4;
			int_0[num3++] = num4 + 1;
			int_0[num3++] = num4 + 2;
			int_0[num3++] = num4 + 2;
			int_0[num3++] = num4 + 3;
			int_0[num3++] = num4;
		}
		int j = 0;
		int num5 = 0;
		for (; j < count; j++)
		{
			TreePlaneLod treePlaneLod = treePlaneLod_0[j];
			int num6 = num5++;
			int num7 = num5++;
			int num8 = num5++;
			int num9 = num5++;
			vector2_0[num6] = new Vector2(0f, 0f);
			vector2_0[num7] = new Vector2(0f, 1f);
			vector2_0[num8] = new Vector2(1f, 1f);
			vector2_0[num9] = new Vector2(1f, 0f);
			float y = (treePlaneLod.GetComponent<Renderer>().isVisible ? 0f : 1f);
			vector2_1[num6] = new Vector2(0f - treePlaneLod.Width, y);
			vector2_1[num7] = new Vector2(0f - treePlaneLod.Width, y);
			vector2_1[num8] = new Vector2(treePlaneLod.Width, y);
			vector2_1[num9] = new Vector2(treePlaneLod.Width, y);
			Vector3 position = treePlaneLod.transform.position;
			Vector3 vector = new Vector3(position.x, position.y + treePlaneLod.Height, position.z);
			vector3_0[num6] = position;
			vector3_0[num7] = vector;
			vector3_0[num8] = vector;
			vector3_0[num9] = position;
			CalcBounds(position);
		}
		mesh_0 = new Mesh
		{
			vertices = vector3_0,
			normals = vector3_1,
			tangents = vector4_0,
			uv = vector2_0,
			uv2 = vector2_1,
			triangles = int_0,
			name = "TreePlaneLod _mesh"
		};
		mesh_0.MarkDynamic();
		mesh_0.bounds = new Bounds
		{
			min = vector3_2 - new Vector3(50f, 50f, 50f),
			max = vector3_3 + new Vector3(50f, 50f, 50f)
		};
		GameObject obj = new GameObject("TreesMesh")
		{
			hideFlags = HideFlags.HideAndDontSave
		};
		MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
		meshFilter.mesh = mesh_0;
		meshRenderer.sharedMaterial = Mat;
	}

	public static void CalcBounds(Vector3 position)
	{
		if (position.x < vector3_2.x)
		{
			vector3_2.x = position.x;
		}
		if (position.y < vector3_2.y)
		{
			vector3_2.y = position.y;
		}
		if (position.z < vector3_2.z)
		{
			vector3_2.z = position.z;
		}
		if (position.x > vector3_3.x)
		{
			vector3_3.x = position.x;
		}
		if (position.y > vector3_3.y)
		{
			vector3_3.y = position.y;
		}
		if (position.z > vector3_3.z)
		{
			vector3_3.z = position.z;
		}
	}

	public static void UpdateMesh()
	{
		mesh_0.uv2 = vector2_1;
	}

	public void OnBecameVisible()
	{
		int num = int_1 << 2;
		vector2_1[num++] = new Vector2(0f - Width, 0f);
		vector2_1[num++] = new Vector2(0f - Width, 0f);
		vector2_1[num++] = new Vector2(Width, 0f);
		vector2_1[num] = new Vector2(Width, 0f);
		UpdateMesh();
	}

	public void OnBecameInvisible()
	{
		int num = int_1 << 2;
		vector2_1[num++] = new Vector2(0f - Width, 1f);
		vector2_1[num++] = new Vector2(0f - Width, 1f);
		vector2_1[num++] = new Vector2(Width, 1f);
		vector2_1[num] = new Vector2(Width, 1f);
		UpdateMesh();
	}
}
