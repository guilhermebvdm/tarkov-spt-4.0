using UnityEngine;

public abstract class Class688
{
	public static Mesh smethod_0(int count)
	{
		int num = count << 2;
		Vector3[] array = new Vector3[num];
		Vector4[] array2 = new Vector4[num];
		Vector3[] array3 = new Vector3[num];
		Vector2[] array4 = new Vector2[num];
		Vector2[] array5 = new Vector2[num];
		int i = 0;
		int num2 = 0;
		for (; i < count; i++)
		{
			int num3 = num2++;
			int num4 = num2++;
			int num5 = num2++;
			int num6 = num2++;
			array[num3] = (array[num4] = (array[num5] = (array[num6] = new Vector3(Random.value, Random.value, Random.value))));
			array4[num3] = new Vector2(0f, 1f);
			array4[num4] = new Vector2(0f, 0f);
			array4[num5] = new Vector2(1f, 0f);
			array4[num6] = new Vector2(1f, 1f);
			Vector2 vector = new Vector2(Random.value - 0.5f, 0f);
			float num7 = (Random.value - 0.5f) * 0.3f;
			vector.y = ((vector.x > 0f) ? (1f - vector.x) : (-1f - vector.x));
			Vector2 vector2 = new Vector2(0f - vector.y, vector.x + num7);
			array5[num3] = vector;
			array5[num4] = vector2;
			vector.y -= num7;
			array5[num5] = -vector;
			vector2.x += num7;
			array5[num6] = -vector2;
			float x = (Random.value - 0.5f) * 100f;
			float y = (Random.value - 0.5f) * 100f;
			float z = (Random.value - 0.5f) * 100f;
			float w = (Random.value - 0.5f) * 100f;
			array2[num3] = (array2[num4] = (array2[num5] = (array2[num6] = new Vector4(x, y, z, w))));
			array3[num3] = (array3[num4] = (array3[num5] = (array3[num6] = Vector3.zero)));
		}
		int[] triangles = smethod_1(count);
		Bounds bounds = new Bounds(Vector3.zero, Vector3.one * float.MaxValue);
		return new Mesh
		{
			name = "SnowFlakes GetMesh",
			vertices = array,
			normals = array3,
			tangents = array2,
			uv = array4,
			uv2 = array5,
			triangles = triangles,
			bounds = bounds
		};
	}

	public static int[] smethod_1(int count)
	{
		int[] array = new int[count * 6];
		int i = 0;
		int num = 0;
		for (; i < count; i++)
		{
			int num2 = i << 2;
			array[num++] = num2;
			array[num++] = num2 + 1;
			array[num++] = num2 + 2;
			array[num++] = num2 + 2;
			array[num++] = num2 + 3;
			array[num++] = num2;
		}
		return array;
	}
}
