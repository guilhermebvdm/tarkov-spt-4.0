using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AbsolutDecals;
using UnityEngine;

public class DecalMeshClass
{
	[Serializable]
	[CompilerGenerated]
	public class Class722
	{
		public static readonly Class722 class722_0 = new Class722();

		public static Comparison<GStruct80> comparison_0;

		public static Comparison<GStruct80> comparison_1;

		public int method_0(GStruct80 x, GStruct80 y)
		{
			return x.Dot.CompareTo(y.Dot);
		}

		public int method_1(GStruct80 x, GStruct80 y)
		{
			return y.Dot.CompareTo(x.Dot);
		}
	}

	public Vector3[] _vertices;

	public int[] Triangles;

	public Vector3[] Normals;

	public Vector2[] UV;

	public bool IsSkinned;

	public Matrix4x4[] BindPoses;

	public BoneWeight[] BoneWeights;

	[NonSerialized]
	public static Vector2 Vector2_0 = new Vector2(0.5f, 0.5f);

	public Transform[] Bones;

	public Transform DecalTransform;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	public Vector3[] WorldVertices;

	public Vector3[] WorldNormals;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	public bool IsFromStatic
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public Vector3[] Vertices
	{
		get
		{
			return _vertices;
		}
		set
		{
			if (value != null)
			{
				VertexCount = value.Length;
			}
			_vertices = value;
		}
	}

	public int VertexCount
	{
		[CompilerGenerated]
		get
		{
			return Int_0;
		}
		[CompilerGenerated]
		set
		{
			Int_0 = value;
		}
	}

	public DecalMeshClass(Vector3[] vertices, int[] triangles, Vector3[] normals, Vector2[] uv, Vector3[] worldVertices = null, Vector3[] worldNormals = null)
	{
		Vertices = vertices;
		Triangles = triangles;
		Normals = normals;
		UV = uv;
		VertexCount = vertices.Length;
		if (worldVertices != null)
		{
			IsFromStatic = true;
			WorldVertices = worldVertices;
			WorldNormals = worldNormals;
		}
	}

	public DecalMeshClass(Mesh mesh, bool isFromStatic = false, Transform objectTransform = null)
	{
		if (mesh == null)
		{
			return;
		}
		Vertices = mesh.vertices;
		Triangles = mesh.triangles;
		Normals = mesh.normals;
		UV = mesh.uv;
		VertexCount = Vertices.Length;
		IsFromStatic = isFromStatic;
		if (isFromStatic && objectTransform != null)
		{
			WorldVertices = new Vector3[VertexCount];
			WorldNormals = new Vector3[VertexCount];
			Matrix4x4 transpose = objectTransform.worldToLocalMatrix.inverse.transpose;
			for (int i = 0; i < VertexCount; i++)
			{
				WorldVertices[i] = objectTransform.TransformPoint(Vertices[i]);
				WorldNormals[i] = transpose.MultiplyVector(Normals[i]);
			}
		}
	}

	public static DecalMeshClass operator +(DecalMeshClass lhs, DecalMeshClass rhs)
	{
		if (lhs == null && rhs == null)
		{
			return null;
		}
		if (lhs == null)
		{
			return rhs;
		}
		if (rhs == null)
		{
			return lhs;
		}
		int num = lhs.VertexCount + rhs.VertexCount;
		Vector3[] array = new Vector3[num];
		Vector3[] array2 = new Vector3[num];
		Vector2[] array3 = new Vector2[num];
		int[] array4 = new int[lhs.Triangles.Length + rhs.Triangles.Length];
		for (int i = 0; i < num; i++)
		{
			if (i < lhs.VertexCount)
			{
				array[i] = lhs.Vertices[i];
				array2[i] = lhs.Normals[i];
				array3[i] = lhs.UV[i];
			}
			else
			{
				int num2 = i - lhs.VertexCount;
				array[i] = rhs.Vertices[num2];
				array2[i] = rhs.Normals[num2];
				array3[i] = rhs.UV[num2];
			}
		}
		for (int j = 0; j < array4.Length; j++)
		{
			if (j < lhs.Triangles.Length)
			{
				array4[j] = lhs.Triangles[j];
			}
			else
			{
				array4[j] = rhs.Triangles[j - lhs.Triangles.Length] + lhs.VertexCount;
			}
		}
		DecalMeshClass decalMeshClass = new DecalMeshClass(array, array4, array2, array3);
		if (rhs.Bones != null)
		{
			decalMeshClass.BindPoses = rhs.BindPoses;
			decalMeshClass.Bones = rhs.Bones;
		}
		else if (lhs.Bones != null)
		{
			decalMeshClass.BindPoses = lhs.BindPoses;
			decalMeshClass.Bones = lhs.Bones;
		}
		int num3 = 0;
		int num4 = 0;
		if (lhs.BoneWeights != null)
		{
			num3 = lhs.BoneWeights.Length;
		}
		if (rhs.BoneWeights != null)
		{
			num4 = rhs.BoneWeights.Length;
		}
		BoneWeight[] array5 = new BoneWeight[num3 + num4];
		for (int k = 0; k < array5.Length; k++)
		{
			if (k < num3)
			{
				array5[k] = lhs.BoneWeights[k];
			}
			else
			{
				array5[k] = rhs.BoneWeights[k - num3];
			}
		}
		decalMeshClass.BoneWeights = array5;
		return decalMeshClass;
	}

	public static Mesh operator +(Mesh lhs, DecalMeshClass rhs)
	{
		if (lhs == null && rhs == null)
		{
			return null;
		}
		if (lhs == null)
		{
			return rhs.ToMesh();
		}
		if (rhs == null)
		{
			return lhs;
		}
		int num = lhs.vertexCount + rhs.VertexCount;
		Vector3[] array = new Vector3[num];
		Vector3[] array2 = new Vector3[num];
		Vector2[] array3 = new Vector2[num];
		int[] array4 = new int[lhs.triangles.Length + rhs.Triangles.Length];
		Vector3[] vertices = lhs.vertices;
		Vector2[] uv = lhs.uv;
		Vector3[] normals = lhs.normals;
		int[] triangles = lhs.triangles;
		int vertexCount = lhs.vertexCount;
		int num2 = triangles.Length;
		for (int i = 0; i < num; i++)
		{
			if (i < vertexCount)
			{
				array[i] = vertices[i];
				array2[i] = normals[i];
				array3[i] = uv[i];
			}
			else
			{
				int num3 = i - vertexCount;
				array[i] = rhs.Vertices[num3];
				array2[i] = rhs.Normals[num3];
				array3[i] = rhs.UV[num3];
			}
		}
		for (int j = 0; j < array4.Length; j++)
		{
			if (j < triangles.Length)
			{
				array4[j] = triangles[j];
			}
			else
			{
				array4[j] = rhs.Triangles[j - num2] + vertexCount;
			}
		}
		Mesh mesh = new Mesh
		{
			vertices = array,
			triangles = array4,
			normals = array2,
			uv = array3,
			name = "TemporaryMesh operator+"
		};
		mesh.MarkDynamic();
		if (rhs.IsSkinned)
		{
			if (lhs.bindposes == null)
			{
				mesh.bindposes = rhs.BindPoses;
			}
			else
			{
				mesh.bindposes = lhs.bindposes;
			}
			BoneWeight[] array5 = new BoneWeight[lhs.boneWeights.Length + rhs.BoneWeights.Length];
			int num4 = lhs.boneWeights.Length;
			for (int k = 0; k < array5.Length; k++)
			{
				if (k < num4)
				{
					array5[k] = lhs.boneWeights[k];
				}
				else
				{
					array5[k] = rhs.BoneWeights[k - num4];
				}
			}
			mesh.boneWeights = array5;
		}
		return mesh;
	}

	public static DecalMeshClass MakeMesh(List<Vector3> verticesList, List<Vector3> projectorVertsList, Vector3 normal, DecalProjector.ProjectionDirections projectionDirection, Vector3 projectionDirectionVector, Vector2[] decalUv, float offset, bool isInProjectorSpace, List<BoneWeight> boneWeights = null, Transform relativeTransform = null)
	{
		Vector3[] array = new Vector3[verticesList.Count()];
		for (int i = 0; i < verticesList.Count; i++)
		{
			array[i] = verticesList[i];
		}
		Vector3 vector = -projectionDirectionVector;
		Vector3 vector2 = array[0];
		Vector3 vector3 = projectorVertsList[0];
		Vector3[] array2 = new Vector3[array.Length - 1];
		List<GStruct80> list = new List<GStruct80>(array.Length);
		List<GStruct80> list2 = new List<GStruct80>(array.Length);
		array2[0] = (array[1] - vector2).normalized;
		for (int j = 1; j < array.Length; j++)
		{
			if (Vector3.Dot(Vector3.Cross(array[j] - vector2, array2[0]).normalized, normal) >= 0f)
			{
				list.Add(new GStruct80
				{
					Vertex = array[j],
					ProjectorSpaceVertex = projectorVertsList[j],
					Dot = Vector3.Dot((array[j] - vector2).normalized, array2[0])
				});
			}
			else
			{
				list2.Add(new GStruct80
				{
					Vertex = array[j],
					ProjectorSpaceVertex = projectorVertsList[j],
					Dot = Vector3.Dot((array[j] - vector2).normalized, array2[0])
				});
			}
		}
		list.Sort((GStruct80 x, GStruct80 y) => x.Dot.CompareTo(y.Dot));
		list2.Sort((GStruct80 x, GStruct80 y) => y.Dot.CompareTo(x.Dot));
		Vector3[] array3 = new Vector3[list.Count + list2.Count + 1];
		Vector3[] array4 = new Vector3[list.Count + list2.Count + 1];
		Vector2[] array5 = new Vector2[array.Length];
		Vector3[] array6 = new Vector3[array.Length];
		Vector3 vector4 = Vector3.up * offset;
		Vector3 vector5 = vector * offset;
		array3[0] = vector2 + vector5;
		array4[0] = vector3 + vector4;
		array5[0] = GetUv(vector3, projectionDirection, decalUv);
		array6[0] = normal;
		for (int num = 0; num < list.Count + list2.Count; num++)
		{
			if (num < list.Count)
			{
				array3[num + 1] = list[num].Vertex + vector5;
				array4[num + 1] = list[num].ProjectorSpaceVertex + vector4;
				array5[num + 1] = GetUv(list[num].ProjectorSpaceVertex, projectionDirection, decalUv);
				array6[num + 1] = normal;
			}
			else
			{
				array3[num + 1] = list2[num - list.Count].Vertex + vector5;
				array4[num + 1] = list2[num - list.Count].ProjectorSpaceVertex + vector4;
				array5[num + 1] = GetUv(list2[num - list.Count].ProjectorSpaceVertex, projectionDirection, decalUv);
				array6[num + 1] = normal;
			}
		}
		int[] array7 = new int[(array.Length - 2) * 3];
		for (int num2 = 0; num2 < array.Length - 2; num2++)
		{
			array7[num2 * 3] = 0;
			array7[num2 * 3 + 1] = num2 + 1;
			array7[num2 * 3 + 2] = num2 + 2;
		}
		array6[array.Length - 2] = normal;
		array6[array.Length - 1] = normal;
		DecalMeshClass decalMeshClass;
		if (!(relativeTransform != null))
		{
			decalMeshClass = (isInProjectorSpace ? new DecalMeshClass(array4, array7, array6, array5) : new DecalMeshClass(array3, array7, array6, array5));
		}
		else
		{
			for (int num3 = 0; num3 < array3.Length; num3++)
			{
				array3[num3] = relativeTransform.InverseTransformPoint(array3[num3]);
			}
			decalMeshClass = new DecalMeshClass(array3, array7, array6, array5);
			decalMeshClass.BoneWeights = boneWeights.ToArray();
		}
		return decalMeshClass;
	}

	public static Vector2 GetUv(Vector3 projectorLocalVertices, DecalProjector.ProjectionDirections direction, Vector2[] decalUv)
	{
		switch (direction)
		{
		default:
			Debug.LogWarning("bad projection direction");
			return Vector2.zero;
		case DecalProjector.ProjectionDirections.PositiveX:
			return new Vector2(0f - projectorLocalVertices.z, 0f - projectorLocalVertices.y) + Vector2_0;
		case DecalProjector.ProjectionDirections.NegativeX:
			return new Vector2(projectorLocalVertices.z, projectorLocalVertices.y) + Vector2_0;
		case DecalProjector.ProjectionDirections.PositiveY:
			return new Vector2(0f - projectorLocalVertices.x, 0f - projectorLocalVertices.z) + Vector2_0;
		case DecalProjector.ProjectionDirections.NegativeY:
		{
			Vector2 b = new Vector2(projectorLocalVertices.x, projectorLocalVertices.z) + Vector2_0;
			return Vector2.Scale(decalUv[2] - decalUv[0], b) + decalUv[0];
		}
		case DecalProjector.ProjectionDirections.PositiveZ:
			return new Vector2(0f - projectorLocalVertices.x, 0f - projectorLocalVertices.y) + Vector2_0;
		case DecalProjector.ProjectionDirections.NegativeZ:
			return new Vector2(projectorLocalVertices.x, projectorLocalVertices.y) + Vector2_0;
		}
	}

	public static DecalMeshClass TerrainToMesh(TerrainData terrain, Transform terrainTransform, int terrainResolutionPow = 3)
	{
		Vector3 a = terrain.size;
		int num;
		Vector2 b;
		float[,] heights;
		int heightmapResolution;
		int heightmapResolution2;
		do
		{
			num = (int)Mathf.Pow(2f, terrainResolutionPow);
			heightmapResolution = terrain.heightmapResolution;
			heightmapResolution2 = terrain.heightmapResolution;
			a = new Vector3(a.x / (float)(heightmapResolution - 1) * (float)num, a.y, a.z / (float)(heightmapResolution2 - 1) * (float)num);
			b = new Vector2(1f / (float)(heightmapResolution - 1), 1f / (float)(heightmapResolution2 - 1));
			heights = terrain.GetHeights(0, 0, heightmapResolution, heightmapResolution2);
			heightmapResolution = (heightmapResolution - 1) / num + 1;
			heightmapResolution2 = (heightmapResolution2 - 1) / num + 1;
			terrainResolutionPow++;
			if (terrainResolutionPow >= 10)
			{
				Debug.LogWarning("Cant save terrain mesh");
				return null;
			}
		}
		while (heightmapResolution * heightmapResolution2 >= 65000);
		Vector3[] array = new Vector3[heightmapResolution * heightmapResolution2];
		Vector3[] array2 = new Vector3[array.Length];
		Vector2[] array3 = new Vector2[heightmapResolution * heightmapResolution2];
		int[] array4 = new int[(heightmapResolution - 1) * (heightmapResolution2 - 1) * 6];
		Vector3[] array5 = new Vector3[heightmapResolution * heightmapResolution2];
		Vector3[] array6 = new Vector3[array5.Length];
		Matrix4x4 transpose = terrainTransform.worldToLocalMatrix.inverse.transpose;
		for (int i = 0; i < heightmapResolution2; i++)
		{
			for (int j = 0; j < heightmapResolution; j++)
			{
				array[i * heightmapResolution + j] = Vector3.Scale(a, new Vector3(i, heights[j * num, i * num], j));
				array2[i * heightmapResolution + j] = terrainTransform.TransformPoint(array[i * heightmapResolution + j]);
				array3[i * heightmapResolution + j] = Vector2.Scale(new Vector2(j * num, i * num), b);
				array5[i * heightmapResolution + j] = Vector3.up;
				array6[i * heightmapResolution + j] = transpose.MultiplyVector(Vector3.up);
			}
		}
		int num2 = 0;
		for (int k = 0; k < heightmapResolution2 - 1; k++)
		{
			for (int l = 0; l < heightmapResolution - 1; l++)
			{
				array4[num2++] = k * heightmapResolution + l;
				array4[num2++] = k * heightmapResolution + l + 1;
				array4[num2++] = (k + 1) * heightmapResolution + l;
				array4[num2++] = (k + 1) * heightmapResolution + l;
				array4[num2++] = k * heightmapResolution + l + 1;
				array4[num2++] = (k + 1) * heightmapResolution + l + 1;
			}
		}
		return new DecalMeshClass(array, array4, array5, array3, array2, array6);
	}

	public Mesh ToMesh()
	{
		Mesh mesh = new Mesh
		{
			vertices = Vertices,
			triangles = Triangles,
			normals = Normals,
			uv = UV,
			name = "TemporaryMesh ToMesh"
		};
		mesh.MarkDynamic();
		if (IsSkinned)
		{
			mesh.bindposes = BindPoses;
			mesh.boneWeights = BoneWeights;
		}
		return mesh;
	}
}
