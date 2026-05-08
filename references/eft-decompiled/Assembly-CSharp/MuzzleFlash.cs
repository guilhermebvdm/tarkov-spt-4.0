using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MuzzleFlash : MonoBehaviour
{
	public int Count = 2048;

	public int TileSheetRows = 1;

	public int TileSheetColumns = 1;

	private Vector2[][] vector2_0;

	private Vector3[] vector3_0;

	private Vector2[] vector2_1;

	private Vector2[] vector2_2;

	private int[] int_0;

	private Mesh mesh_0;

	private int int_1;

	private bool bool_0;

	private int int_2;

	private GClass2608.GClass2609 gclass2609_0;

	public void Awake()
	{
		int num = Count << 2;
		vector3_0 = new Vector3[num];
		vector2_1 = new Vector2[num];
		vector2_2 = new Vector2[num];
		int_0 = new int[Count * 6];
		int i = 0;
		int num2 = 0;
		for (; i < Count; i++)
		{
			int num3 = i << 2;
			int_0[num2++] = num3;
			int_0[num2++] = num3 + 1;
			int_0[num2++] = num3 + 2;
			int_0[num2++] = num3 + 2;
			int_0[num2++] = num3 + 3;
			int_0[num2++] = num3;
		}
		int j = 0;
		int num4 = 0;
		for (; j < Count; j++)
		{
			vector2_1[num4++] = new Vector2(0f, 0f);
			vector2_1[num4++] = new Vector2(0f, 1f);
			vector2_1[num4++] = new Vector2(1f, 1f);
			vector2_1[num4++] = new Vector2(1f, 0f);
		}
		int_1 = TileSheetRows * TileSheetColumns;
		bool_0 = int_1 == 1;
		if (!bool_0)
		{
			gclass2609_0 = new GClass2608.GClass2609(int_1);
			vector2_0 = new Vector2[int_1][];
			float num5 = 1f / (float)TileSheetRows;
			float num6 = 1f / (float)TileSheetColumns;
			int k = 0;
			int num7 = 0;
			int num8 = 0;
			for (; k < int_1; k++)
			{
				float x = num5 * (float)num7;
				float x2 = num5 * (float)(num7 + 1);
				float y = num6 * (float)num8;
				float y2 = num6 * (float)(num8 + 1);
				vector2_0[k] = new Vector2[4];
				vector2_0[k][0] = new Vector2(x, y);
				vector2_0[k][1] = new Vector2(x, y2);
				vector2_0[k][2] = new Vector2(x2, y2);
				vector2_0[k][3] = new Vector2(x2, y);
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
			uv = vector2_1,
			uv2 = vector2_2,
			triangles = int_0,
			name = "MuzzleFlash _mesh"
		};
		mesh_0.bounds = new Bounds(Vector3.zero, new Vector3(2f, 2f, 2f));
		GetComponent<MeshFilter>().mesh = mesh_0;
	}

	public void AddFlash(float radius, float durationTime, float addTime, bool randomRotation)
	{
		Vector3 vector;
		Vector3 vector2;
		if (randomRotation)
		{
			vector = Random.insideUnitCircle * radius;
			vector2 = new Vector2(vector.y, 0f - vector.x);
		}
		else
		{
			vector = new Vector2(radius, radius);
			vector2 = new Vector2(radius, 0f - radius);
		}
		int num = int_2 << 2;
		int num2 = num++;
		int num3 = num++;
		int num4 = num++;
		int_2 = (int_2 + 1) % Count;
		vector3_0[num2] = -vector;
		vector3_0[num3] = -vector2;
		vector3_0[num4] = vector;
		vector3_0[num] = vector2;
		if (bool_0)
		{
			vector2_1[num2] = new Vector2(0f, 0f);
			vector2_1[num3] = new Vector2(0f, 1f);
			vector2_1[num4] = new Vector2(1f, 1f);
			vector2_1[num] = new Vector2(1f, 0f);
		}
		else
		{
			Vector2[] array = vector2_0[gclass2609_0.Get()];
			vector2_1[num2] = array[0];
			vector2_1[num3] = array[1];
			vector2_1[num4] = array[2];
			vector2_1[num] = array[3];
		}
		vector2_2[num2] = (vector2_2[num3] = (vector2_2[num4] = (vector2_2[num] = new Vector2(Time.time + durationTime + addTime, durationTime))));
		base.enabled = true;
	}

	public void LateUpdate()
	{
		mesh_0.vertices = vector3_0;
		mesh_0.uv = vector2_1;
		mesh_0.uv2 = vector2_2;
		base.enabled = false;
	}
}
