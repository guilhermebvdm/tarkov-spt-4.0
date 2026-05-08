using UnityEngine;

public class ParticleGroup : MonoBehaviour
{
	private Vector3[] vector3_0;

	private Vector3[] vector3_1;

	private float[] float_0;

	private MeshRenderer meshRenderer_0;

	private MeshFilter meshFilter_0;

	public void Awake()
	{
		method_0();
	}

	public void OnDrawGizmos()
	{
		method_0();
	}

	public void method_0()
	{
		if (meshRenderer_0 == null)
		{
			meshRenderer_0 = GetComponent<MeshRenderer>() ?? base.gameObject.AddComponent<MeshRenderer>();
		}
		if (meshFilter_0 == null)
		{
			meshFilter_0 = GetComponent<MeshFilter>() ?? base.gameObject.AddComponent<MeshFilter>();
		}
		if (vector3_0 == null || vector3_0.Length != base.transform.childCount)
		{
			vector3_0 = new Vector3[base.transform.childCount];
			vector3_1 = new Vector3[base.transform.childCount];
			float_0 = new float[base.transform.childCount];
		}
		int num = 0;
		bool flag = false;
		foreach (Transform item in base.transform)
		{
			if (vector3_0[num] != item.localPosition || vector3_1[num] != item.localScale || float_0[num] != item.eulerAngles.x)
			{
				vector3_0[num] = item.localPosition;
				vector3_1[num] = item.localScale;
				float_0[num] = item.eulerAngles.x;
				flag = true;
			}
			num++;
		}
		if (flag)
		{
			meshFilter_0.mesh = method_1();
		}
	}

	public Mesh method_1()
	{
		int num = vector3_0.Length;
		int num2 = num << 2;
		Vector3[] array = new Vector3[num2];
		Vector2[] array2 = new Vector2[num2];
		Vector2[] array3 = new Vector2[num2];
		int[] array4 = new int[num * 6];
		int i = 0;
		int num3 = 0;
		for (; i < num; i++)
		{
			int num4 = i << 2;
			array4[num3++] = num4;
			array4[num3++] = num4 + 1;
			array4[num3++] = num4 + 2;
			array4[num3++] = num4 + 2;
			array4[num3++] = num4 + 3;
			array4[num3++] = num4;
		}
		int j = 0;
		int num5 = 0;
		for (; j < num; j++)
		{
			int num6 = num5++;
			int num7 = num5++;
			int num8 = num5++;
			int num9 = num5++;
			array2[num6] = new Vector2(0f, 1f);
			array2[num7] = new Vector2(0f, 0f);
			array2[num8] = new Vector2(1f, 0f);
			array2[num9] = new Vector2(1f, 1f);
			Vector2 vector = vector3_1[j];
			float f = float_0[j];
			float num10 = Mathf.Cos(f);
			float num11 = Mathf.Sin(f);
			float num12 = vector.x * num10;
			float num13 = vector.y * (0f - num11);
			float num14 = vector.x * num11;
			float num15 = vector.y * num10;
			array3[num6] = new Vector2(0f - num12 - num13, 0f - num14 - num15);
			array3[num7] = new Vector2(0f - num12 + num13, 0f - num14 + num15);
			array3[num8] = new Vector2(num12 + num13, num14 + num15);
			array3[num9] = new Vector2(num12 - num13, num14 - num15);
			array[num6] = (array[num7] = (array[num8] = (array[num9] = vector3_0[j])));
		}
		return new Mesh
		{
			vertices = array,
			uv = array2,
			uv2 = array3,
			triangles = array4,
			name = "ParticleGroup GetMesh"
		};
	}
}
