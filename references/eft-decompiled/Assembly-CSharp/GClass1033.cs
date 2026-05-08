using System;
using System.Collections.Generic;
using AbsolutDecals;
using UnityEngine;

public class GClass1033
{
	public static readonly Dictionary<DecalProjector.ProjectionDirections, Vector3> ProjcectionDirectionsDict = new Dictionary<DecalProjector.ProjectionDirections, Vector3>
	{
		{
			DecalProjector.ProjectionDirections.PositiveX,
			Vector3.right
		},
		{
			DecalProjector.ProjectionDirections.NegativeX,
			-Vector3.right
		},
		{
			DecalProjector.ProjectionDirections.PositiveY,
			Vector3.up
		},
		{
			DecalProjector.ProjectionDirections.NegativeY,
			-Vector3.up
		},
		{
			DecalProjector.ProjectionDirections.PositiveZ,
			Vector3.forward
		},
		{
			DecalProjector.ProjectionDirections.NegativeZ,
			-Vector3.forward
		}
	};

	[NonSerialized]
	public const float Float_0 = 0.0001f;

	public static Mesh RemoveFromMesh(Vector2 startEndVerticies, Vector2 startEndTrigs, Mesh mesh)
	{
		int num = (int)startEndVerticies.y - (int)startEndVerticies.x;
		int num2 = (int)startEndTrigs.y - (int)startEndTrigs.x;
		Vector3[] array = new Vector3[mesh.vertexCount - num];
		int[] array2 = new int[mesh.triangles.Length - num2];
		Vector3[] array3 = new Vector3[mesh.vertexCount - num];
		Vector2[] array4 = new Vector2[mesh.vertexCount - num];
		Vector3[] vertices = mesh.vertices;
		int[] triangles = mesh.triangles;
		Vector3[] normals = mesh.normals;
		Vector2[] uv = mesh.uv;
		int num3 = vertices.Length;
		int num4 = triangles.Length;
		for (int i = 0; (float)i < startEndVerticies.x; i++)
		{
			array[i] = vertices[i];
			array3[i] = normals[i];
			array4[i] = uv[i];
		}
		for (int j = (int)startEndVerticies.y; j < num3; j++)
		{
			array[(int)startEndVerticies.x + j - (int)startEndVerticies.y] = vertices[j];
			array3[(int)startEndVerticies.x + j - (int)startEndVerticies.y] = normals[j];
			array4[(int)startEndVerticies.x + j - (int)startEndVerticies.y] = uv[j];
		}
		for (int k = 0; (float)k < startEndTrigs.x; k++)
		{
			array2[k] = triangles[k];
		}
		for (int l = (int)startEndTrigs.y; l < num4; l++)
		{
			array2[(int)startEndTrigs.x + l - (int)startEndTrigs.y] = triangles[l] - num;
		}
		return new Mesh
		{
			vertices = array,
			triangles = array2,
			uv = array4,
			normals = array3,
			name = "DecalSystemUtils RemoveFromMesh"
		};
	}

	public static Vector3 CrossWithNormalization(Vector3 lhs1, Vector3 lhs2, Vector3 rhs1, Vector3 rhs2)
	{
		float num = lhs1.x - lhs2.x;
		float num2 = lhs1.y - lhs2.y;
		float num3 = lhs1.z - lhs2.z;
		float num4 = rhs1.x - rhs2.x;
		float num5 = rhs1.y - rhs2.y;
		float num6 = rhs1.z - rhs2.z;
		float num7 = num2 * num6 - num3 * num5;
		float num8 = num3 * num4 - num * num6;
		float num9 = num * num5 - num2 * num4;
		float num10 = 1f / Mathf.Sqrt(num7 * num7 + num8 * num8 + num9 * num9);
		return new Vector3(num7 * num10, num8 * num10, num9 * num10);
	}

	public static Vector3 ScaledVectorToWorld(Transform localSpace, Vector3 normal)
	{
		return localSpace.worldToLocalMatrix.transpose.MultiplyVector(normal);
	}

	public static List<Vector3> smethod_0(List<Vector3> vertices)
	{
		List<Vector3> list = new List<Vector3>();
		List<int> list2 = new List<int>();
		for (int i = 0; i < vertices.Count - 1; i++)
		{
			for (int j = i + 1; j < vertices.Count; j++)
			{
				if ((vertices[i] - vertices[j]).sqrMagnitude < 0.0001f)
				{
					list2.Add(i);
				}
			}
		}
		if (list2.Count != 0 && vertices.Count >= 3)
		{
			for (int k = 0; k < vertices.Count; k++)
			{
				if (!list2.Contains(k))
				{
					list.Add(vertices[k]);
				}
			}
			return smethod_0(list);
		}
		return vertices;
	}
}
