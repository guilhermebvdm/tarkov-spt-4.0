using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class GClass1226
{
	public class Class828
	{
		public List<int> indices = new List<int>();

		public List<Vector3> verts = new List<Vector3>();

		public List<Color> colors = new List<Color>();

		public List<Class829> tris = new List<Class829>();

		public Mesh CreateMesh()
		{
			Mesh mesh = new Mesh();
			mesh.indexFormat = IndexFormat.UInt32;
			mesh.SetVertices(verts);
			mesh.SetTriangles(indices, 0);
			mesh.SetColors(colors);
			return mesh;
		}

		public Hash128 ComputeHash()
		{
			Hash128 hash = Hash128.Compute("xxx");
			Debug.Log(hash);
			hash.Append(verts);
			Debug.Log(hash);
			hash.Append(indices);
			Debug.Log(hash);
			hash.Append(colors);
			Debug.Log(hash);
			return hash;
		}
	}

	public class Class829
	{
		public Vector3 posA;

		public Vector3 posB;

		public Vector3 posC;

		public Color colA;

		public Vector3 center;
	}

	public class Class830 : IEqualityComparer<(Vector3, Color)>
	{
		public bool Equals((Vector3, Color) x, (Vector3, Color) y)
		{
			if (x.Item1 == y.Item1)
			{
				return x.Item2 == y.Item2;
			}
			return false;
		}

		public int GetHashCode((Vector3, Color) obj)
		{
			return obj.Item1.GetHashCode() ^ obj.Item2.GetHashCode();
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class831
	{
		public static readonly Class831 class831_0 = new Class831();

		public static Comparison<Class829> comparison_0;

		public static Comparison<Class829> comparison_1;

		public int method_0(Class829 a, Class829 b)
		{
			return a.center.x.CompareTo(b.center.x);
		}

		public int method_1(Class829 a, Class829 b)
		{
			return a.center.z.CompareTo(b.center.z);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	public struct Struct233
	{
		public Class828 splitMeshData;

		public List<Class828> meshes;

		public Dictionary<(Vector3, Color), int> spV;

		public int cIndex;
	}

	[NonSerialized]
	public const int Int_0 = 8000;

	[NonSerialized]
	public static Class830 Class830_0 = new Class830();

	public static List<Mesh> Split(Mesh mesh, Matrix4x4 transformMatrix)
	{
		List<Mesh> list = new List<Mesh>();
		Class828 @class = smethod_1(mesh, transformMatrix);
		Debug.Log("Split hash=" + @class.ComputeHash().ToString());
		@class.tris.Sort((Class829 a, Class829 b) => a.center.x.CompareTo(b.center.x));
		foreach (Class828 item in smethod_0(@class.tris, 64000))
		{
			item.tris.Sort((Class829 a, Class829 b) => a.center.z.CompareTo(b.center.z));
			foreach (Class828 item2 in smethod_0(item.tris, 8000))
			{
				list.Add(item2.CreateMesh());
			}
		}
		return list;
	}

	public static List<Class828> smethod_0(List<Class829> triangles, int splitCount)
	{
		Struct233 struct233_ = default(Struct233);
		struct233_.meshes = new List<Class828>();
		struct233_.cIndex = 0;
		struct233_.spV = new Dictionary<(Vector3, Color), int>(Class830_0);
		struct233_.splitMeshData = new Class828();
		foreach (Class829 triangle in triangles)
		{
			smethod_3(triangle.posA, triangle.colA, ref struct233_);
			smethod_3(triangle.posB, triangle.colA, ref struct233_);
			smethod_3(triangle.posC, triangle.colA, ref struct233_);
			if (struct233_.splitMeshData.verts.Count >= splitCount)
			{
				smethod_2(ref struct233_);
			}
		}
		smethod_2(ref struct233_);
		return struct233_.meshes;
	}

	public static Class828 smethod_1(Mesh mesh, Matrix4x4 transformMat)
	{
		Class828 @class = new Class828();
		Vector3[] vertices = mesh.vertices;
		int[] indices = mesh.GetIndices(0);
		Color[] colors = mesh.colors;
		bool flag = false;
		int num = indices.Length;
		for (int i = 0; i < num; i += 3)
		{
			Vector3 vector = (flag ? transformMat.MultiplyPoint3x4(vertices[indices[i]]) : vertices[indices[i]]);
			Vector3 vector2 = (flag ? transformMat.MultiplyPoint3x4(vertices[indices[i + 1]]) : vertices[indices[i + 1]]);
			Vector3 vector3 = (flag ? transformMat.MultiplyPoint3x4(vertices[indices[i + 2]]) : vertices[indices[i + 2]]);
			@class.tris.Add(new Class829
			{
				posA = vector,
				posB = vector2,
				posC = vector3,
				colA = colors[indices[i]],
				center = (vector + vector2 + vector3) / 3f
			});
		}
		@class.indices = indices.ToList();
		@class.verts = vertices.ToList();
		@class.colors = colors.ToList();
		return @class;
	}

	[CompilerGenerated]
	public static void smethod_2(ref Struct233 struct233_0)
	{
		int count = struct233_0.splitMeshData.indices.Count;
		for (int i = 0; i < count; i += 3)
		{
			Vector3 vector = struct233_0.splitMeshData.verts[struct233_0.splitMeshData.indices[i]];
			Vector3 vector2 = struct233_0.splitMeshData.verts[struct233_0.splitMeshData.indices[i + 1]];
			Vector3 vector3 = struct233_0.splitMeshData.verts[struct233_0.splitMeshData.indices[i + 2]];
			struct233_0.splitMeshData.tris.Add(new Class829
			{
				posA = vector,
				posB = vector2,
				posC = vector3,
				colA = struct233_0.splitMeshData.colors[struct233_0.splitMeshData.indices[i]],
				center = (vector + vector2 + vector3) / 3f
			});
		}
		struct233_0.meshes.Add(struct233_0.splitMeshData);
		struct233_0.splitMeshData = new Class828();
		struct233_0.spV.Clear();
		struct233_0.cIndex = 0;
	}

	[CompilerGenerated]
	public static void smethod_3(Vector3 pos, Color col, ref Struct233 struct233_0)
	{
		if (struct233_0.spV.TryGetValue((pos, col), out var value))
		{
			struct233_0.splitMeshData.indices.Add(value);
			return;
		}
		struct233_0.spV.Add((pos, col), struct233_0.cIndex);
		struct233_0.splitMeshData.verts.Add(pos);
		struct233_0.splitMeshData.indices.Add(struct233_0.cIndex);
		struct233_0.splitMeshData.colors.Add(col);
		int cIndex = struct233_0.cIndex + 1;
		struct233_0.cIndex = cIndex;
	}
}
