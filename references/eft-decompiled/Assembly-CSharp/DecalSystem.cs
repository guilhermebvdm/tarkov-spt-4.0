using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class DecalSystem : MonoBehaviour
{
	public class Class624
	{
		public int Pos4;

		public Vector3[] Vertices = new Vector3[4];

		public Vector3 Normal;

		public Vector4 Tangent;

		public Vector2[] Uv;

		public void FastCalcWithoutRotation(Vector3 position, Vector3 normal, float radius)
		{
			Vector3 vector;
			Vector3 vector2;
			float num;
			if (normal.x < 0.95f && normal.x > -0.95f)
			{
				vector = new Vector3(0f, normal.z, 0f - normal.y);
				Debug.DrawRay(position, normal, Color.green, 5f);
				Debug.DrawRay(position, vector, Color.red, 5f);
				num = radius / Mathf.Sqrt(vector.y * vector.y + vector.z * vector.z);
				vector2 = new Vector3(normal.y * (0f - normal.y) - normal.z * normal.z, (0f - normal.x) * (0f - normal.y), normal.x * normal.z);
			}
			else
			{
				vector = new Vector3(0f - normal.z, 0f, normal.x);
				num = radius / Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z);
				vector2 = new Vector3(normal.y * vector.z, normal.z * vector.x - normal.x * vector.z, (0f - normal.y) * vector.x);
			}
			vector *= num;
			vector2 *= num;
			Vertices[0] = position - vector;
			Vertices[1] = position - vector2;
			Vertices[2] = position + vector;
			Vertices[3] = position + vector2;
			num = 0.7071f / radius;
			Tangent = new Vector4((Vertices[3].x - Vertices[0].x) * num, (Vertices[3].y - Vertices[0].y) * num, (Vertices[3].z - Vertices[0].z) * num, -1f);
			Normal = normal;
		}

		public void Calc(Vector3 position, Vector3 normal, float radius)
		{
			Vector3 vector = Vector3.Cross(normal, GClass2608.VectorNormalized());
			vector *= radius / vector.magnitude;
			Vector3 vector2 = Vector3.Cross(normal, vector);
			Vertices[0] = position - vector;
			Vertices[1] = position - vector2;
			Vertices[2] = position + vector;
			Vertices[3] = position + vector2;
			float num = 0.7071f / radius;
			Tangent = new Vector4((Vertices[3].x - Vertices[0].x) * num, (Vertices[3].y - Vertices[0].y) * num, (Vertices[3].z - Vertices[0].z) * num, -1f);
			Normal = normal;
		}

		public void Update(Vector3[] vertices, Vector3[] normals, Vector4[] tangents)
		{
			for (int i = 0; i < 4; i++)
			{
				int num = Pos4 + i;
				vertices[num] = Vertices[i];
				normals[num] = Normal;
				tangents[num] = Tangent;
			}
		}

		public void Update(Vector3[] vertices, Vector3[] normals, Vector4[] tangents, Vector2[] uv)
		{
			for (int i = 0; i < 4; i++)
			{
				int num = Pos4 + i;
				vertices[num] = Vertices[i];
				normals[num] = Normal;
				tangents[num] = Tangent;
				uv[num] = Uv[i];
			}
		}
	}

	public int Count = 2048;

	public float SizeMin = 1f;

	public float SizeRandomRange;

	private bool bool_0;

	public int TileSheetRows = 1;

	public int TileSheetColumns = 1;

	private Vector2[][] vector2_0;

	private int int_0 = 1;

	private bool bool_1;

	private LinkedList<Class624> linkedList_0;

	private int int_1;

	private Class624[] class624_0;

	private Vector3[] vector3_0;

	private Vector3[] vector3_1;

	private Vector4[] vector4_0;

	private Vector2[] vector2_1;

	private Vector2[] vector2_2;

	private int[] int_2;

	private Mesh mesh_0;

	private Vector3 vector3_2 = Vector3.zero;

	private Vector3 vector3_3 = Vector3.zero;

	public void Awake()
	{
		bool_0 = SizeRandomRange >= float.Epsilon;
		linkedList_0 = new LinkedList<Class624>();
		class624_0 = new Class624[Count];
		for (int i = 0; i < Count; i++)
		{
			class624_0[i] = new Class624();
			class624_0[i].Pos4 = i << 2;
		}
		int num = Count << 2;
		vector3_0 = new Vector3[num];
		vector3_1 = new Vector3[num];
		vector4_0 = new Vector4[num];
		vector2_1 = new Vector2[num];
		vector2_2 = new Vector2[num];
		int_2 = new int[Count * 6];
		int j = 0;
		int num2 = 0;
		for (; j < Count; j++)
		{
			int num3 = j << 2;
			int_2[num2++] = num3;
			int_2[num2++] = num3 + 1;
			int_2[num2++] = num3 + 2;
			int_2[num2++] = num3 + 2;
			int_2[num2++] = num3 + 3;
			int_2[num2++] = num3;
		}
		int k = 0;
		int num4 = 0;
		for (; k < Count; k++)
		{
			vector2_1[num4++] = new Vector2(0f, 0f);
			vector2_1[num4++] = new Vector2(0f, 1f);
			vector2_1[num4++] = new Vector2(1f, 1f);
			vector2_1[num4++] = new Vector2(1f, 0f);
		}
		int_0 = TileSheetRows * TileSheetColumns;
		bool_1 = int_0 > 1;
		if (bool_1)
		{
			vector2_0 = new Vector2[int_0][];
			float num5 = 1f / (float)TileSheetRows;
			float num6 = 1f / (float)TileSheetColumns;
			int l = 0;
			int num7 = 0;
			int num8 = 0;
			for (; l < int_0; l++)
			{
				float x = num5 * (float)num7;
				float x2 = num5 * (float)(num7 + 1);
				float y = num6 * (float)num8;
				float y2 = num6 * (float)(num8 + 1);
				vector2_0[l] = new Vector2[4];
				vector2_0[l][0] = new Vector2(x, y);
				vector2_0[l][1] = new Vector2(x, y2);
				vector2_0[l][2] = new Vector2(x2, y2);
				vector2_0[l][3] = new Vector2(x2, y);
				num7++;
				if (num7 >= TileSheetRows)
				{
					num8++;
					num7 = 0;
				}
			}
		}
		mesh_0 = new Mesh
		{
			vertices = vector3_0,
			normals = vector3_1,
			tangents = vector4_0,
			uv = vector2_1,
			uv2 = vector2_2,
			triangles = int_2,
			name = "DecalSystem _mesh"
		};
		mesh_0.MarkDynamic();
		mesh_0.bounds = new Bounds(Vector3.zero, Vector3.zero);
		GetComponent<MeshFilter>().mesh = mesh_0;
	}

	public void LateUpdate()
	{
		if (linkedList_0.Count == 0)
		{
			return;
		}
		if (bool_1)
		{
			foreach (Class624 item in linkedList_0)
			{
				item.Update(vector3_0, vector3_1, vector4_0, vector2_1);
			}
		}
		else
		{
			foreach (Class624 item2 in linkedList_0)
			{
				item2.Update(vector3_0, vector3_1, vector4_0);
			}
		}
		linkedList_0.Clear();
		mesh_0.vertices = vector3_0;
		mesh_0.normals = vector3_1;
		if (bool_1)
		{
			mesh_0.uv = vector2_1;
		}
		mesh_0.tangents = vector4_0;
	}

	public void Clear()
	{
		vector3_0 = new Vector3[Count << 2];
		mesh_0.vertices = vector3_0;
		mesh_0.bounds = new Bounds(Vector3.zero, Vector3.zero);
		vector3_2 = Vector3.zero;
		vector3_3 = Vector3.zero;
		linkedList_0.Clear();
	}

	public void Add(Vector3 position, Vector3 normal)
	{
		float radius = (bool_0 ? (SizeMin + GClass2608.Float() * SizeRandomRange) : SizeMin);
		Class624 @class = class624_0[int_1];
		if (++int_1 >= Count)
		{
			int_1 = 0;
		}
		position = base.transform.InverseTransformPoint(position);
		normal = base.transform.InverseTransformDirection(normal);
		linkedList_0.AddLast(@class);
		@class.Calc(position, normal, radius);
		if (bool_1)
		{
			@class.Uv = vector2_0[GClass2608.Int(int_0)];
		}
		method_0(position);
	}

	public void Add(Vector3 position, Vector3 normal, int tile)
	{
		float radius = (bool_0 ? (SizeMin + GClass2608.Float() * SizeRandomRange) : SizeMin);
		Class624 @class = class624_0[int_1];
		if (++int_1 >= Count)
		{
			int_1 = 0;
		}
		position = base.transform.InverseTransformPoint(position);
		normal = base.transform.InverseTransformDirection(normal);
		linkedList_0.AddLast(@class);
		@class.Calc(position, normal, radius);
		@class.Uv = vector2_0[tile];
		method_0(position);
	}

	public void method_0(Vector3 position)
	{
		if (!(position.x > vector3_2.x) || !(position.y > vector3_2.y) || !(position.z > vector3_2.z) || !(position.x < vector3_3.x) || !(position.y < vector3_3.y) || !(position.z < vector3_3.z))
		{
			if (position.x < vector3_2.x)
			{
				vector3_2.x = position.x - 50f;
			}
			if (position.y < vector3_2.y)
			{
				vector3_2.y = position.y - 50f;
			}
			if (position.z < vector3_2.z)
			{
				vector3_2.z = position.z - 50f;
			}
			if (position.x > vector3_3.x)
			{
				vector3_3.x = position.x + 50f;
			}
			if (position.y > vector3_3.y)
			{
				vector3_3.y = position.y + 50f;
			}
			if (position.z > vector3_3.z)
			{
				vector3_3.z = position.z + 50f;
			}
			mesh_0.bounds = new Bounds
			{
				min = vector3_2,
				max = vector3_3
			};
		}
	}
}
