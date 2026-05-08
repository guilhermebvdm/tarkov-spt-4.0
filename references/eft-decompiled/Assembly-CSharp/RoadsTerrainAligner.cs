using System;
using System.Collections.Generic;
using UnityEngine;

public class RoadsTerrainAligner : MonoBehaviour
{
	public class Class575
	{
		public Vector3[] HitWorldPoints = new Vector3[3];

		public List<Vector3[]> HeightmapPointsVariants = new List<Vector3[]>();

		public void ConvertToHeightmap(Vector3 terrainPosition, Vector3 terrainSize, int heightmapResolution)
		{
			Vector3[] array = new Vector3[3];
			Vector3[] array2 = new Vector3[3];
			Vector3[] array3 = new Vector3[3];
			Vector3[] array4 = new Vector3[3];
			for (int i = 0; i < 3; i++)
			{
				Vector3 vector = HitWorldPoints[i] - terrainPosition;
				vector.x /= terrainSize.x;
				vector.y /= terrainSize.y;
				vector.z /= terrainSize.z;
				vector.x *= heightmapResolution;
				vector.z *= heightmapResolution;
				float x = vector.x;
				float z = vector.z;
				vector.x = Mathf.CeilToInt(x);
				vector.z = Mathf.CeilToInt(z);
				array[i] = vector;
				vector.x = Mathf.FloorToInt(x);
				vector.z = Mathf.FloorToInt(z);
				array2[i] = vector;
				vector.x = Mathf.FloorToInt(x);
				vector.z = Mathf.CeilToInt(z);
				array3[i] = vector;
				vector.x = Mathf.CeilToInt(x);
				vector.z = Mathf.FloorToInt(z);
				array4[i] = vector;
			}
			HeightmapPointsVariants.Clear();
			HeightmapPointsVariants.Add(array);
			HeightmapPointsVariants.Add(array2);
			HeightmapPointsVariants.Add(array3);
			HeightmapPointsVariants.Add(array4);
			method_0();
		}

		public void method_0()
		{
			if (HeightmapPointsVariants[0][0].z > HeightmapPointsVariants[0][1].z)
			{
				for (int i = 0; i < HeightmapPointsVariants.Count; i++)
				{
					method_1(ref HeightmapPointsVariants[i][0], ref HeightmapPointsVariants[i][1]);
				}
				method_1(ref HitWorldPoints[0], ref HitWorldPoints[1]);
			}
			if (HeightmapPointsVariants[0][0].z > HeightmapPointsVariants[0][2].z)
			{
				for (int j = 0; j < HeightmapPointsVariants.Count; j++)
				{
					method_1(ref HeightmapPointsVariants[j][0], ref HeightmapPointsVariants[j][2]);
				}
				method_1(ref HitWorldPoints[0], ref HitWorldPoints[2]);
			}
			if (HeightmapPointsVariants[0][1].z > HeightmapPointsVariants[0][2].z)
			{
				for (int k = 0; k < HeightmapPointsVariants.Count; k++)
				{
					method_1(ref HeightmapPointsVariants[k][1], ref HeightmapPointsVariants[k][2]);
				}
				method_1(ref HitWorldPoints[1], ref HitWorldPoints[2]);
			}
		}

		public void method_1(ref Vector3 lhs, ref Vector3 rhs)
		{
			Vector3 vector = lhs;
			lhs = rhs;
			rhs = vector;
		}

		public override string ToString()
		{
			string text = "World points ";
			string text2 = "Heightmap points ";
			for (int i = 0; i < 3; i++)
			{
				text = text + " " + HitWorldPoints[i].ToString("0.0000");
			}
			return text + "\n" + text2;
		}
	}

	public Texture2D Heightmap;

	[SerializeField]
	private float _offsetFromMesh = 0.02f;

	[SerializeField]
	private float _raycastOffset = 10f;

	[SerializeField]
	private float _raycastDistance = 500f;

	private Terrain terrain_0;

	private TerrainData terrainData_0;

	private List<Class575> list_0 = new List<Class575>();

	public void method_0()
	{
		Mesh sharedMesh = GetComponent<MeshFilter>().sharedMesh;
		list_0.Clear();
		Vector3[] vertices = sharedMesh.vertices;
		int[] triangles = sharedMesh.triangles;
		for (int i = 0; i < triangles.Length; i += 3)
		{
			Class575 @class = new Class575();
			Vector3 vector = base.transform.TransformPoint(vertices[triangles[i]]);
			Vector3 vector2 = base.transform.TransformPoint(vertices[triangles[i + 1]]);
			Vector3 vector3 = base.transform.TransformPoint(vertices[triangles[i + 2]]);
			int layerMask = 1 << LayerMask.NameToLayer("Terrain");
			Vector3 vector4 = Vector3.up * _raycastOffset;
			if (!Physics.Raycast(vector + vector4, Vector3.down, out var hitInfo, _raycastDistance, layerMask) || !Physics.Raycast(vector2 + vector4, Vector3.down, out var hitInfo2, _raycastDistance, layerMask) || !Physics.Raycast(vector3 + vector4, Vector3.down, out var hitInfo3, _raycastDistance, layerMask))
			{
				continue;
			}
			if (terrainData_0 == null)
			{
				terrain_0 = hitInfo.collider.gameObject.GetComponent<Terrain>();
				if (terrain_0 != null)
				{
					terrainData_0 = terrain_0.terrainData;
				}
			}
			if (!(terrainData_0 == null) && !(terrain_0 == null))
			{
				@class.HitWorldPoints = new Vector3[3]
				{
					new Vector3(hitInfo.point.x, vector.y - _offsetFromMesh, hitInfo.point.z),
					new Vector3(hitInfo2.point.x, vector2.y - _offsetFromMesh, hitInfo2.point.z),
					new Vector3(hitInfo3.point.x, vector3.y - _offsetFromMesh, hitInfo3.point.z)
				};
				Color color = ((i == 0) ? Color.red : Color.green);
				Vector3 vector5 = new Vector3(0f, (float)i * 0f);
				Debug.DrawRay(@class.HitWorldPoints[0] + vector5, Vector3.up * 2f, color, 10f);
				Debug.DrawRay(@class.HitWorldPoints[1] + vector5, Vector3.up * 2f, color, 10f);
				Debug.DrawRay(@class.HitWorldPoints[2] + vector5, Vector3.up * 2f, color, 10f);
				@class.ConvertToHeightmap(terrain_0.transform.position, terrainData_0.size, terrainData_0.heightmapResolution);
				list_0.Add(@class);
			}
		}
		if (!(terrainData_0 == null) && !(terrain_0 == null))
		{
			method_1(list_0, terrainData_0, terrain_0);
			terrain_0.ApplyDelayedHeightmapModification();
			method_4();
		}
	}

	public void method_1(List<Class575> triangles, TerrainData terrainData, Terrain terrain)
	{
		foreach (Class575 triangle in triangles)
		{
			method_2(triangle, terrainData, terrain);
		}
	}

	public void method_2(Class575 triangle, TerrainData terrainData, Terrain terrain)
	{
		Vector3 t = triangle.HeightmapPointsVariants[0][0];
		Vector3 t2 = triangle.HeightmapPointsVariants[0][1];
		Vector3 t3 = triangle.HeightmapPointsVariants[0][2];
		smethod_0(terrainData, t, t2, t3);
		for (int i = 0; i < triangle.HeightmapPointsVariants.Count; i++)
		{
			t = triangle.HeightmapPointsVariants[i][0];
			t2 = triangle.HeightmapPointsVariants[i][1];
			t3 = triangle.HeightmapPointsVariants[i][2];
			terrainData.SetHeightsDelayLOD((int)t.x, (int)t.z, new float[1, 2] { { t.y, t.y } });
			terrainData.SetHeightsDelayLOD((int)t2.x, (int)t2.z, new float[1, 2] { { t2.y, t2.y } });
			terrainData.SetHeightsDelayLOD((int)t3.x, (int)t3.z, new float[1, 2] { { t3.y, t3.y } });
		}
	}

	public static void smethod_0(TerrainData terrainData, Vector3 t0, Vector3 t1, Vector3 t2)
	{
		terrainData.SetHeightsDelayLOD((int)t0.x, (int)t0.z, new float[1, 2] { { t0.y, t0.y } });
		terrainData.SetHeightsDelayLOD((int)t1.x, (int)t1.z, new float[1, 2] { { t1.y, t1.y } });
		terrainData.SetHeightsDelayLOD((int)t2.x, (int)t2.z, new float[1, 2] { { t2.y, t2.y } });
		float num = t2.z - t0.z;
		for (int i = 0; (float)i < num; i++)
		{
			bool flag;
			int num2 = ((flag = (float)i > t1.z - t0.z || Math.Abs(t1.z - t0.z) < Mathf.Epsilon) ? ((int)(t2.z - t1.z)) : ((int)(t1.z - t0.z)));
			float num3 = (float)i / num;
			float num4 = ((float)i - (flag ? (t1.z - t0.z) : 0f)) / (float)num2;
			Vector3 vector = t0 + (t2 - t0) * num3;
			Vector3 vector2 = (flag ? (t1 + (t2 - t1) * num4) : (t0 + (t1 - t0) * num4));
			if (vector.x > vector2.x)
			{
				Vector3 vector3 = vector;
				vector = vector2;
				vector2 = vector3;
			}
			float num5 = vector2.y - vector.y;
			float num6 = vector2.x - vector.x;
			for (int j = (int)vector.x; j <= (int)vector2.x; j++)
			{
				float num7 = vector.y + ((float)j - vector.x) * num5 / num6;
				terrainData.SetHeightsDelayLOD(j, (int)t0.z + i, new float[1, 2] { { num7, num7 } });
			}
		}
	}

	public void method_3()
	{
		method_0();
	}

	public void method_4()
	{
		if (terrainData_0 == null || terrain_0 == null)
		{
			return;
		}
		float[,] heights = terrainData_0.GetHeights(0, 0, terrainData_0.heightmapResolution, terrainData_0.heightmapResolution);
		Color[] array = new Color[terrainData_0.heightmapResolution * terrainData_0.heightmapResolution];
		float num = 40f;
		for (int i = 0; i < terrainData_0.heightmapResolution; i++)
		{
			for (int j = 0; j < terrainData_0.heightmapResolution; j++)
			{
				array[j + terrainData_0.heightmapResolution * i] = new Color(heights[i, j] * num, heights[i, j] * num, heights[i, j] * num);
			}
		}
		if (Heightmap == null)
		{
			Heightmap = new Texture2D(terrainData_0.heightmapResolution, terrainData_0.heightmapResolution);
			Heightmap.name = "RoadsTerrainHeightMap";
		}
		Heightmap.SetPixels(array);
		Heightmap.Apply();
	}
}
