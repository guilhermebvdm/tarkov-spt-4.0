using System;
using UnityEngine;

public class RoadsGeneratorFixed : MonoBehaviour
{
	[Serializable]
	public class RoadPoint
	{
		public Transform Point;

		public float Width;

		[HideInInspector]
		public Vector3[] Points;

		[HideInInspector]
		public Vector3[] Normals;
	}

	[SerializeField]
	private Material _material;

	[SerializeField]
	private float _textureLength = 0.5f;

	[SerializeField]
	private float _offsetAmount = 0.1f;

	[SerializeField]
	private float _halfRaycastLength = 10f;

	[SerializeField]
	public int OneSideAdditionalPointsAmount;

	[SerializeField]
	private LayerMask _mask;

	[SerializeField]
	private RoadPoint[] _roadPoints = new RoadPoint[0];

	private int int_0;

	public void Awake()
	{
		int_0 = OneSideAdditionalPointsAmount * 2 + 1;
		method_0(_roadPoints);
		RoadPoint[] roadPoints = _roadPoints;
		foreach (RoadPoint roadPoint in roadPoints)
		{
			method_1(roadPoint, roadPoint.Point.right);
			Vector3[] points = roadPoint.Points;
			for (int j = 0; j < points.Length; j++)
			{
				Debug.DrawRay(points[j], Vector3.up, Color.green, 900f);
			}
		}
		Mesh mesh = new Mesh
		{
			name = "RoadsGeneratorFixed Mesh"
		};
		mesh.vertices = method_2(_roadPoints);
		mesh.triangles = method_3(mesh.vertices);
		mesh.normals = method_4(_roadPoints);
		roadPoints = _roadPoints;
		foreach (RoadPoint roadPoint2 in roadPoints)
		{
			for (int k = 0; k < roadPoint2.Points.Length; k++)
			{
				Debug.DrawRay(roadPoint2.Points[k], roadPoint2.Normals[k], Color.yellow, 900f);
			}
		}
		mesh.uv = method_5(_roadPoints, mesh.vertexCount);
		GameObject obj = new GameObject("road");
		obj.transform.position = Vector3.zero;
		obj.transform.rotation = Quaternion.identity;
		obj.AddComponent<MeshFilter>().sharedMesh = mesh;
		obj.AddComponent<MeshRenderer>().material = _material;
	}

	public void method_0(RoadPoint[] points)
	{
	}

	public void method_1(RoadPoint roadPoint, Vector3 direction)
	{
		float num = roadPoint.Width / (float)(2 * OneSideAdditionalPointsAmount);
		roadPoint.Points = new Vector3[OneSideAdditionalPointsAmount * 2 + 1];
		roadPoint.Normals = new Vector3[roadPoint.Points.Length];
		roadPoint.Points[roadPoint.Points.Length / 2 + 1] = roadPoint.Point.position;
		for (int i = 0; i < OneSideAdditionalPointsAmount * 2 + 1; i++)
		{
			if (Physics.Raycast(roadPoint.Point.position + direction * num * (i - OneSideAdditionalPointsAmount) + new Vector3(0f, _halfRaycastLength), Vector3.down, out var hitInfo, _halfRaycastLength * 2f, _mask))
			{
				roadPoint.Points[i] = hitInfo.point + new Vector3(0f, _offsetAmount);
				roadPoint.Normals[i] = hitInfo.normal;
				if (i > 0)
				{
					Debug.DrawLine(roadPoint.Points[i - 1], roadPoint.Points[i], Color.red, 150f);
				}
			}
		}
	}

	public Vector3[] method_2(RoadPoint[] roadPoints)
	{
		Vector3[] array = new Vector3[_roadPoints.Length * OneSideAdditionalPointsAmount * 2 + _roadPoints.Length];
		int num = OneSideAdditionalPointsAmount * 2 + 1;
		for (int i = 0; i < roadPoints.Length; i++)
		{
			for (int j = 0; j < num; j++)
			{
				array[i * num + j] = roadPoints[i].Points[j];
			}
		}
		return array;
	}

	public int[] method_3(Vector3[] vert)
	{
		int[] array = new int[(vert.Length - int_0) * 6];
		for (int i = 0; i < vert.Length - int_0; i++)
		{
			if ((i + 1) % int_0 != 0 || i == 0)
			{
				array[i * 6] = i;
				array[i * 6 + 1] = i + int_0;
				array[i * 6 + 2] = i + 1;
				array[i * 6 + 3] = i + 1;
				array[i * 6 + 4] = i + int_0;
				array[i * 6 + 5] = i + int_0 + 1;
			}
		}
		return array;
	}

	public Vector3[] method_4(RoadPoint[] roadPoints)
	{
		Vector3[] array = new Vector3[_roadPoints.Length * OneSideAdditionalPointsAmount * 2 + _roadPoints.Length];
		int num = OneSideAdditionalPointsAmount * 2 + 1;
		for (int i = 0; i < roadPoints.Length; i++)
		{
			for (int j = 0; j < num; j++)
			{
				array[i * num + j] = roadPoints[i].Normals[j];
			}
		}
		return array;
	}

	public Vector2[] method_5(RoadPoint[] points, int vertLen)
	{
		int num = 0;
		float x = 0f;
		float num2 = points[0].Width / (float)(int_0 - 1) / _textureLength;
		Vector2[] array = new Vector2[vertLen];
		int num3 = 0;
		for (int i = 0; i < vertLen; i++)
		{
			if (i % int_0 == 0 && i != 0)
			{
				num3 = 0;
				num++;
				num2 = points[num].Width / (float)(int_0 - 1) / _textureLength;
				if (num < points.Length && i != 0)
				{
					x = (points[num - 1].Point.position - points[num].Point.position).magnitude / _textureLength;
				}
			}
			array[i] = new Vector2(x, num2 * (float)num3);
			num3++;
		}
		return array;
	}
}
