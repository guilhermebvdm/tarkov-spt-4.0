using System;
using System.Collections.Generic;
using AbsolutDecals;
using UnityEngine;

public abstract class GClass1034
{
	public static DecalMeshClass GetDecalMesh(DecalProjector projector, bool inProjectorSpace, DecalMeshClass terrainMesh, List<Renderer> renderers, List<Transform> transforms, List<Mesh> meshes, List<DecalMeshClass> tmpMeshes, float offset, Vector2[] decalUv, bool includeTerrain)
	{
		Transform transform = projector.Transform;
		Vector3 vector = Vector3.one * 0.5f;
		Vector3[] array = new Vector3[8]
		{
			new Vector3(0f - vector.x, 0f - vector.y, 0f - vector.z),
			new Vector3(0f - vector.x, 0f - vector.y, vector.z),
			new Vector3(0f - vector.x, vector.y, 0f - vector.z),
			new Vector3(0f - vector.x, vector.y, vector.z),
			new Vector3(vector.x, 0f - vector.y, 0f - vector.z),
			new Vector3(vector.x, 0f - vector.y, vector.z),
			new Vector3(vector.x, vector.y, 0f - vector.z),
			new Vector3(vector.x, vector.y, vector.z)
		};
		Vector3[] array2 = new Vector3[8]
		{
			transform.TransformPoint(array[0]),
			transform.TransformPoint(array[1]),
			transform.TransformPoint(array[2]),
			transform.TransformPoint(array[3]),
			transform.TransformPoint(array[4]),
			transform.TransformPoint(array[5]),
			transform.TransformPoint(array[6]),
			transform.TransformPoint(array[7])
		};
		Ray[] array3 = new Ray[12]
		{
			new Ray(array2[0], array2[2] - array2[0]),
			new Ray(array2[1], array2[3] - array2[1]),
			new Ray(array2[5], array2[7] - array2[5]),
			new Ray(array2[4], array2[6] - array2[4]),
			new Ray(array2[7], array2[3] - array2[7]),
			new Ray(array2[6], array2[2] - array2[6]),
			new Ray(array2[7], array2[6] - array2[7]),
			new Ray(array2[3], array2[2] - array2[3]),
			new Ray(array2[5], array2[1] - array2[5]),
			new Ray(array2[4], array2[0] - array2[4]),
			new Ray(array2[5], array2[4] - array2[5]),
			new Ray(array2[1], array2[0] - array2[1])
		};
		float[] magnitudes = new float[12]
		{
			array3[0].direction.magnitude,
			array3[1].direction.magnitude,
			array3[2].direction.magnitude,
			array3[3].direction.magnitude,
			array3[4].direction.magnitude,
			array3[5].direction.magnitude,
			array3[6].direction.magnitude,
			array3[7].direction.magnitude,
			array3[8].direction.magnitude,
			array3[9].direction.magnitude,
			array3[10].direction.magnitude,
			array3[11].direction.magnitude
		};
		Vector3 vector2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		Vector3 vector3 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Ray[] projectorPlanesRaysLocal = new Ray[6]
		{
			new Ray(array[0], GClass1033.CrossWithNormalization(array[0], array[1], array[0], array[2])),
			new Ray(array[4], GClass1033.CrossWithNormalization(array[4], array[6], array[4], array[5])),
			new Ray(array[1], GClass1033.CrossWithNormalization(array[1], array[4], array[1], array[5])),
			new Ray(array[2], GClass1033.CrossWithNormalization(array[2], array[3], array[2], array[6])),
			new Ray(array[5], GClass1033.CrossWithNormalization(array[5], array[1], array[5], array[7])),
			new Ray(array[4], GClass1033.CrossWithNormalization(array[4], array[2], array[4], array[0]))
		};
		GStruct81[] projectorEdgesSegmentsLocal = new GStruct81[12]
		{
			new GStruct81(array[0], array[2]),
			new GStruct81(array[1], array[3]),
			new GStruct81(array[5], array[7]),
			new GStruct81(array[4], array[6]),
			new GStruct81(array[7], array[3]),
			new GStruct81(array[6], array[2]),
			new GStruct81(array[7], array[6]),
			new GStruct81(array[3], array[2]),
			new GStruct81(array[5], array[1]),
			new GStruct81(array[4], array[0]),
			new GStruct81(array[5], array[4]),
			new GStruct81(array[1], array[0])
		};
		foreach (Vector3 rhs in array2)
		{
			vector2 = Vector3.Max(vector2, rhs);
			vector3 = Vector3.Min(vector3, rhs);
		}
		Vector3 vector4 = vector2 - vector3;
		Bounds bounds = new Bounds(size: new Vector3(Mathf.Abs(vector4.x), Mathf.Abs(vector4.y), Mathf.Abs(vector4.z)), center: transform.position);
		DecalMeshClass result = null;
		if (includeTerrain && smethod_0(array3, magnitudes).Length != 0)
		{
			if (terrainMesh == null)
			{
				MonoBehaviourSingleton<AbsolutDecals.DecalSystem>.Instance.UpdateNonStaticSceneData();
			}
			result += smethod_1(Terrain.activeTerrain.transform, projectorPlanesRaysLocal, terrainMesh, projectorEdgesSegmentsLocal, projector, decalUv, inProjectorSpace, vector2, vector3, offset);
		}
		for (int j = 0; j < renderers.Count; j++)
		{
			if (!(renderers[j] == null) && renderers[j].bounds.Intersects(bounds))
			{
				result += smethod_1(transforms[j], projectorPlanesRaysLocal, tmpMeshes[j], projectorEdgesSegmentsLocal, projector, decalUv, inProjectorSpace, vector2, vector3, offset);
			}
		}
		return result;
	}

	public static Vector3[] smethod_0(Ray[] projectorEdgesWorld, float[] magnitudes)
	{
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i < 12; i++)
		{
			if (Physics.Raycast(projectorEdgesWorld[i], out var hitInfo, magnitudes[i], 1 << LayerMask.NameToLayer("terrain")))
			{
				list.Add(hitInfo.point);
			}
		}
		return list.ToArray();
	}

	public static DecalMeshClass smethod_1(Transform intersectedObjectTransform, Ray[] projectorPlanesRaysLocal, DecalMeshClass mesh, GStruct81[] projectorEdgesSegmentsLocal, DecalProjector projector, Vector2[] decalUv, bool isInProjectorSpace, Vector3 projectorMax, Vector3 projectorMin, float offset)
	{
		Vector3[] vertices = mesh.Vertices;
		int[] triangles = mesh.Triangles;
		DecalMeshClass result = null;
		Vector3 projectionDirectionVector = projector.ProjectionDirectionVector;
		Vector3 normalized = intersectedObjectTransform.InverseTransformDirection(projectionDirectionVector).normalized;
		projectorMin = intersectedObjectTransform.InverseTransformPoint(projectorMin);
		projectorMax = intersectedObjectTransform.InverseTransformPoint(projectorMax);
		Vector3 lhs = projectorMin;
		projectorMin = Vector3.Min(projectorMax, projectorMin);
		projectorMax = Vector3.Max(lhs, projectorMax);
		Matrix4x4 matrix4x = projector.transform.worldToLocalMatrix * intersectedObjectTransform.localToWorldMatrix;
		Matrix4x4 localToWorldMatrix = intersectedObjectTransform.localToWorldMatrix;
		int num = triangles.Length;
		for (int i = 0; i < num; i += 3)
		{
			Vector3 vector = GClass1033.CrossWithNormalization(vertices[triangles[i]], vertices[triangles[i + 1]], vertices[triangles[i]], vertices[triangles[i + 2]]);
			if (vector.x * normalized.x + vector.y * normalized.y + vector.z * normalized.z > -0.01f || !smethod_2(vertices[triangles[i]], vertices[triangles[i + 1]], vertices[triangles[i + 2]], projectorMin, projectorMax))
			{
				continue;
			}
			Vector3[] array = new Vector3[3]
			{
				matrix4x.MultiplyPoint(vertices[triangles[i]]),
				matrix4x.MultiplyPoint(vertices[triangles[i + 1]]),
				matrix4x.MultiplyPoint(vertices[triangles[i + 2]])
			};
			if (!smethod_3(array[0], array[1], array[2]))
			{
				continue;
			}
			Vector3[] array2 = new Vector3[3];
			if (mesh.IsFromStatic)
			{
				array2[0] = mesh.WorldVertices[triangles[i]];
				array2[1] = mesh.WorldVertices[triangles[i + 1]];
				array2[2] = mesh.WorldVertices[triangles[i + 2]];
			}
			else
			{
				array2[0] = localToWorldMatrix.MultiplyPoint(vertices[triangles[i]]);
				array2[1] = localToWorldMatrix.MultiplyPoint(vertices[triangles[i + 1]]);
				array2[2] = localToWorldMatrix.MultiplyPoint(vertices[triangles[i + 2]]);
			}
			Vector3 vector2 = (isInProjectorSpace ? GClass1033.CrossWithNormalization(array[0], array[1], array[0], array[2]) : ((!mesh.IsFromStatic) ? GClass1033.CrossWithNormalization(array2[0], array2[1], array2[0], array2[2]) : mesh.WorldNormals[triangles[i]]));
			Vector3[] normals = new Vector3[3] { vector2, vector2, vector2 };
			int[] triangleIndices = new int[3]
			{
				triangles[i],
				triangles[i + 1],
				triangles[i + 2]
			};
			DecalMeshClass decalMeshClass = smethod_4(array, array2, triangleIndices, normals, decalUv, projector, projectionDirectionVector, isInProjectorSpace, offset);
			if (decalMeshClass != null)
			{
				result += decalMeshClass;
				continue;
			}
			Vector3[] triangleEdges = new Vector3[3]
			{
				array[0] - array[1],
				array[1] - array[2],
				array[2] - array[0]
			};
			if (smethod_6(array, triangleEdges))
			{
				DecalMeshClass decalMeshClass2 = smethod_5(array, array2, projectorPlanesRaysLocal, projectorEdgesSegmentsLocal, projector, projectionDirectionVector, decalUv, isInProjectorSpace, offset);
				result += decalMeshClass2;
			}
		}
		return result;
	}

	public static bool smethod_2(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 min, Vector3 max)
	{
		if (v1.x > max.x && v2.x > max.x && v3.x > max.x)
		{
			return false;
		}
		if (v1.x < min.x && v2.x < min.x && v3.x < min.x)
		{
			return false;
		}
		if (v1.y > max.y && v2.y > max.y && v3.y > max.y)
		{
			return false;
		}
		if (v1.y < min.y && v2.y < min.y && v3.y < min.y)
		{
			return false;
		}
		if (v1.z > max.z && v2.z > max.z && v3.z > max.z)
		{
			return false;
		}
		if (v1.z < min.z && v2.z < min.z && v3.z < min.z)
		{
			return false;
		}
		return true;
	}

	public static bool smethod_3(Vector3 v1, Vector3 v2, Vector3 v3)
	{
		if (v1.x > 0.5f && v2.x > 0.5f && v3.x > 0.5f)
		{
			return false;
		}
		if (v1.x < -0.5f && v2.x < -0.5f && v3.x < -0.5f)
		{
			return false;
		}
		if (v1.y > 0.5f && v2.y > 0.5f && v3.y > 0.5f)
		{
			return false;
		}
		if (v1.y < -0.5f && v2.y < -0.5f && v3.y < -0.5f)
		{
			return false;
		}
		if (v1.z > 0.5f && v2.z > 0.5f && v3.z > 0.5f)
		{
			return false;
		}
		if (v1.z < -0.5f && v2.z < -0.5f && v3.z < -0.5f)
		{
			return false;
		}
		return true;
	}

	public static DecalMeshClass smethod_4(Vector3[] triangleProjectorSpace, Vector3[] triangleWorldSpace, int[] triangleIndices, Vector3[] normals, Vector2[] decalUv, DecalProjector projector, Vector3 projectionDirectionVector, bool isInProjectorSpace, float offset)
	{
		if (Mathf.Abs(triangleProjectorSpace[0].x) <= 0.5f && Mathf.Abs(triangleProjectorSpace[0].y) <= 0.5f && Mathf.Abs(triangleProjectorSpace[0].z) <= 0.5f && Mathf.Abs(triangleProjectorSpace[1].x) <= 0.5f && Mathf.Abs(triangleProjectorSpace[1].y) <= 0.5f && Mathf.Abs(triangleProjectorSpace[1].z) <= 0.5f && Mathf.Abs(triangleProjectorSpace[2].x) <= 0.5f && Mathf.Abs(triangleProjectorSpace[2].y) <= 0.5f && Mathf.Abs(triangleProjectorSpace[2].z) <= 0.5f)
		{
			Vector3[] array = new Vector3[3];
			if (!isInProjectorSpace)
			{
				Vector3 vector = -projectionDirectionVector;
				array[0] = triangleWorldSpace[0] + vector * offset;
				array[1] = triangleWorldSpace[1] + vector * offset;
				array[2] = triangleWorldSpace[2] + vector * offset;
			}
			else
			{
				Vector3 vector = -projector.ProjectionDirectionVectorLocal;
				array[0] = triangleProjectorSpace[0] + vector * offset;
				array[1] = triangleProjectorSpace[1] + vector * offset;
				array[2] = triangleProjectorSpace[2] + vector * offset;
			}
			triangleIndices[0] = 0;
			triangleIndices[1] = 1;
			triangleIndices[2] = 2;
			Vector2[] uv = new Vector2[3]
			{
				DecalMeshClass.GetUv(triangleProjectorSpace[0], projector.ProjectionDirection, decalUv),
				DecalMeshClass.GetUv(triangleProjectorSpace[1], projector.ProjectionDirection, decalUv),
				DecalMeshClass.GetUv(triangleProjectorSpace[2], projector.ProjectionDirection, decalUv)
			};
			return new DecalMeshClass(array, triangleIndices, normals, uv);
		}
		return null;
	}

	public static DecalMeshClass smethod_5(Vector3[] triangleProjectorSpace, Vector3[] triangleWorldSpace, Ray[] projectorPlanesRays, GStruct81[] projectorEdgesSegmentsLocal, DecalProjector projector, Vector3 projectionDirectionVector, Vector2[] decalUv, bool isInProjectorSpace, float offset)
	{
		List<Vector3> list = new List<Vector3>(16);
		List<Vector3> list2 = new List<Vector3>(16);
		for (int i = 0; i < 3; i++)
		{
			if (Mathf.Abs(triangleProjectorSpace[i].x) <= 0.5f && Mathf.Abs(triangleProjectorSpace[i].y) <= 0.5f && Mathf.Abs(triangleProjectorSpace[i].z) <= 0.5f)
			{
				list.Add(projector.Transform.TransformPoint(triangleProjectorSpace[i]));
				list2.Add(triangleProjectorSpace[i]);
			}
		}
		Vector3 normal = GClass1033.CrossWithNormalization(triangleWorldSpace[1], triangleWorldSpace[0], triangleWorldSpace[2], triangleWorldSpace[0]);
		GStruct81[] array = new GStruct81[3]
		{
			new GStruct81(triangleProjectorSpace[1], triangleProjectorSpace[0]),
			new GStruct81(triangleProjectorSpace[2], triangleProjectorSpace[1]),
			new GStruct81(triangleProjectorSpace[0], triangleProjectorSpace[2])
		};
		for (int j = 0; j < 6; j++)
		{
			for (int k = 0; k < 3; k++)
			{
				Vector3 direction = projectorPlanesRays[j].direction;
				Vector3 rhs = projectorPlanesRays[j].origin - array[k].StartPoint;
				float num = Vector3.Dot(direction, rhs);
				Vector3 vector = array[k].EndPoint - array[k].StartPoint;
				float num2 = Vector3.Dot(direction, vector);
				if (!(Math.Abs(num2) < Mathf.Epsilon) && !(Math.Abs(num) < Mathf.Epsilon))
				{
					Vector3 vector2 = array[k].StartPoint + vector * num / num2;
					if (Vector3.Dot(array[k].StartPoint - vector2, array[k].EndPoint - vector2) < 0f && Mathf.Abs(vector2.x) <= 0.50001f && Mathf.Abs(vector2.y) <= 0.50001f && Mathf.Abs(vector2.z) <= 0.50001f)
					{
						list.Add(projector.Transform.TransformPoint(vector2));
						list2.Add(vector2);
					}
				}
			}
		}
		smethod_7(triangleProjectorSpace, out var dot, out var dot2, out var dot3, out var v, out var v2, out var invDenom);
		Vector3 lhs = GClass1033.CrossWithNormalization(triangleProjectorSpace[0], triangleProjectorSpace[1], triangleProjectorSpace[0], triangleProjectorSpace[2]);
		for (int l = 0; l < 12; l++)
		{
			Vector3 rhs2 = triangleProjectorSpace[0] - projectorEdgesSegmentsLocal[l].StartPoint;
			float num3 = Vector3.Dot(lhs, rhs2);
			Vector3 vector3 = projectorEdgesSegmentsLocal[l].EndPoint - projectorEdgesSegmentsLocal[l].StartPoint;
			float num4 = Vector3.Dot(lhs, vector3);
			if (!(Math.Abs(num4) < Mathf.Epsilon) && !(Math.Abs(num3) < Mathf.Epsilon))
			{
				Vector3 vector4 = projectorEdgesSegmentsLocal[l].StartPoint + vector3 * num3 / num4;
				if (Vector3.Dot(projectorEdgesSegmentsLocal[l].StartPoint - vector4, projectorEdgesSegmentsLocal[l].EndPoint - vector4) < 0f)
				{
					smethod_8(vector4, triangleProjectorSpace, projector.Transform, dot, dot2, dot3, v, v2, invDenom, list, list2);
				}
			}
		}
		if (list.Count != list2.Count)
		{
			Debug.Log(list.Count + " __ " + list2.Count);
		}
		if (list.Count <= 2)
		{
			return null;
		}
		return DecalMeshClass.MakeMesh(list, list2, normal, projector.ProjectionDirection, projectionDirectionVector, decalUv, offset, isInProjectorSpace);
	}

	public static bool smethod_6(Vector3[] currentTriangleProjectorSpace, Vector3[] triangleEdges)
	{
		float[] array = new float[27]
		{
			triangleEdges[0].z,
			0f,
			0f - triangleEdges[0].x,
			triangleEdges[1].z,
			0f,
			0f - triangleEdges[1].x,
			triangleEdges[2].z,
			0f,
			0f - triangleEdges[2].x,
			0f,
			0f - triangleEdges[0].z,
			triangleEdges[0].y,
			0f,
			0f - triangleEdges[1].z,
			triangleEdges[1].y,
			0f,
			0f - triangleEdges[2].z,
			triangleEdges[2].y,
			0f - triangleEdges[0].y,
			triangleEdges[0].x,
			0f,
			0f - triangleEdges[0].y,
			triangleEdges[0].x,
			0f,
			0f - triangleEdges[2].y,
			triangleEdges[2].x,
			0f
		};
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				float[] array2 = new float[3]
				{
					array[j * 3 + 9 * i] * currentTriangleProjectorSpace[0].x + array[j * 3 + 9 * i + 1] * currentTriangleProjectorSpace[0].y + array[j * 3 + 9 * i + 2] * currentTriangleProjectorSpace[0].z,
					array[j * 3 + 9 * i] * currentTriangleProjectorSpace[1].x + array[j * 3 + 9 * i + 1] * currentTriangleProjectorSpace[1].y + array[j * 3 + 9 * i + 2] * currentTriangleProjectorSpace[1].z,
					array[j * 3 + 9 * i] * currentTriangleProjectorSpace[2].x + array[j * 3 + 9 * i + 1] * currentTriangleProjectorSpace[2].y + array[j * 3 + 9 * i + 2] * currentTriangleProjectorSpace[2].z
				};
				float num = ((array2[0] < array2[1]) ? array2[0] : array2[1]);
				num = ((num < array2[2]) ? num : array2[2]);
				float num2 = ((array2[0] > array2[1]) ? array2[0] : array2[1]);
				num2 = ((num2 > array2[2]) ? num2 : array2[2]);
				float num3 = 0.5f * Mathf.Abs(array[j * 3 + 9 * i]) + 0.5f * Mathf.Abs(array[j * 3 + 9 * i + 1]) + 0.5f * Mathf.Abs(array[j * 3 + 9 * i + 2]);
				if (num > num3 || !(num2 >= 0f - num3))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static void smethod_7(Vector3[] triangleProjSpace, out float dot00, out float dot01, out float dot11, out Vector3 v0, out Vector3 v1, out float invDenom)
	{
		v0 = triangleProjSpace[1] - triangleProjSpace[0];
		v1 = triangleProjSpace[2] - triangleProjSpace[0];
		dot00 = Vector3.Dot(v0, v0);
		dot01 = Vector3.Dot(v0, v1);
		dot11 = Vector3.Dot(v1, v1);
		invDenom = 1f / (dot00 * dot11 - dot01 * dot01);
	}

	public static void smethod_8(Vector3 point, Vector3[] triangleProjSpace, Transform proj, float dot00, float dot01, float dot11, Vector3 v0, Vector3 v1, float invDenom, List<Vector3> vertices, List<Vector3> projectorSpaceVerts)
	{
		Vector3 lhs = point - triangleProjSpace[0];
		float num = Vector3.Dot(lhs, v0);
		float num2 = Vector3.Dot(lhs, v1);
		float num3 = (dot11 * num - dot01 * num2) * invDenom;
		if (num3 < 0f)
		{
			return;
		}
		float num4 = (dot00 * num2 - dot01 * num) * invDenom;
		if (!(num4 < 0f))
		{
			float num5 = 1f - num3 - num4;
			if (!(num5 < 0f) && num5 + num3 <= 1f)
			{
				vertices.Add(proj.TransformPoint(point));
				projectorSpaceVerts.Add(point);
			}
		}
	}
}
