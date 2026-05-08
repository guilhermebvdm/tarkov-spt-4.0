using System.Collections.Generic;
using UnityEngine;

public abstract class GClass976
{
	public static Shader SpeedTreeShader;

	public static Mesh Combine(ICollection<MeshFilter> meshFilters, Vector3 commonPosition, int subMeshCount)
	{
		return smethod_6(smethod_5(smethod_0(meshFilters, commonPosition), subMeshCount));
	}

	public static CombineInstance[] smethod_0(ICollection<MeshFilter> meshFilters, Vector3 commonPosition)
	{
		CombineInstance[] array = new CombineInstance[meshFilters.Count];
		int num = 0;
		foreach (MeshFilter meshFilter in meshFilters)
		{
			Mesh mesh = meshFilter.mesh;
			Transform transform = meshFilter.transform;
			smethod_1(mesh, transform.position - commonPosition, transform.rotation, transform.lossyScale);
			array[num++] = new CombineInstance
			{
				mesh = mesh
			};
		}
		return array;
	}

	public static void smethod_1(Mesh mesh, Vector3 position, Quaternion rotation, Vector3 scale)
	{
		Vector3[] vertices = mesh.vertices;
		Color32[] colors = mesh.colors32;
		Vector2[] uv = mesh.uv4;
		Vector3[] normals = mesh.normals;
		Vector4[] tangents = mesh.tangents;
		for (int i = 0; i < uv.Length; i++)
		{
			Vector3 vector = rotation * vertices[i];
			vector.Scale(scale);
			uv[i] = new Vector2(vector.x, vector.z);
			colors[i] = smethod_2((vector.y + 10f) * 0.02f, colors[i]);
			vertices[i] = position;
			normals[i] = rotation * normals[i];
			tangents[i] = rotation * tangents[i];
		}
		mesh.colors32 = colors;
		mesh.uv4 = uv;
		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.tangents = tangents;
	}

	public static Color32 smethod_2(float v, Color32 c)
	{
		float num = smethod_4(v);
		float num2 = smethod_4(v * 255f);
		float num3 = smethod_4(v * 65025f);
		float num4 = smethod_4(v * 16581375f);
		num -= num2 * 0.003921569f;
		num2 -= num3 * 0.003921569f;
		num3 -= num4 * 0.003921569f;
		return new Color32(c.r, smethod_3(num), smethod_3(num2), smethod_3(num3));
	}

	public static byte smethod_3(float f)
	{
		if (f < 0f)
		{
			f = 0f;
		}
		if (f > 1f)
		{
			f = 1f;
		}
		return (byte)(f * 255f);
	}

	public static float smethod_4(float t)
	{
		return t - Mathf.Floor(t);
	}

	public static Mesh[] smethod_5(CombineInstance[] instances, int subMeshCount)
	{
		Mesh[] array = new Mesh[subMeshCount];
		for (int i = 0; i < subMeshCount; i++)
		{
			for (int j = 0; j < instances.Length; j++)
			{
				instances[j].subMeshIndex = i;
			}
			array[i] = new Mesh
			{
				name = "SpeedTreeCombiner GetMeshes"
			};
			array[i].CombineMeshes(instances, mergeSubMeshes: true, useMatrices: false);
		}
		return array;
	}

	public static Mesh smethod_6(Mesh[] meshes)
	{
		int num = meshes.Length;
		Mesh mesh = meshes[0];
		mesh.subMeshCount = num;
		for (int i = 1; i < num; i++)
		{
			mesh.SetTriangles(meshes[i].GetTriangles(0), i);
		}
		mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 100f);
		return mesh;
	}
}
