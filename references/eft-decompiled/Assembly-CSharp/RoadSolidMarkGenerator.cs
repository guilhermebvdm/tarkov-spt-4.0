using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class RoadSolidMarkGenerator : MonoBehaviour
{
	public RoadSplineGenerator Parent;

	public int Road;

	public float Shift;

	public float Width = 0.1f;

	[Range(0f, 1f)]
	public float Start;

	[Range(0f, 1f)]
	public float End = 1f;

	public Rect UvRect = new Rect(0f, 0f, 1f, 1f);

	public float RoadOffset = 0.01f;

	public int LodParts;

	[Range(0f, 100f)]
	public float CullingRate;

	private MeshFilter meshFilter_0;

	private float float_0;

	public string Culled;

	public LayerMask Mask = -1;

	private float float_1;

	private bool bool_0;

	public void Awake()
	{
		float_1 = Start;
	}

	public void OnValidate()
	{
		if (Parent != null)
		{
			Road = Mathf.Clamp(Road, 0, Parent.Roads.Length);
		}
		LodParts = Math.Max(0, LodParts);
		float_0 = 1f - CullingRate * 0.01f;
		Culled = float_0.ToString("P0");
		bool_0 = !Mathf.Approximately(float_1, Start);
		float_1 = Start;
		method_0();
	}

	public void method_0()
	{
		if (Parent == null || Parent.Roads == null || Parent.Roads.Length == 0)
		{
			return;
		}
		Parent.OnGenerate -= OnValidate;
		if (!this)
		{
			return;
		}
		Parent.OnGenerate += OnValidate;
		base.transform.position = Parent.transform.position + new Vector3(0f, RoadOffset, 0f);
		Vector3[] array = GeneratePoints(Parent.Roads[Road].Optimized, Start, End, Shift);
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = FixOnGround(array[i]);
		}
		if (LodParts > 1)
		{
			method_1(array, LodParts, float_0);
			GetComponent<Renderer>().enabled = false;
			return;
		}
		GetComponent<Renderer>().enabled = true;
		if (meshFilter_0 == null)
		{
			meshFilter_0 = GetComponent<MeshFilter>() ?? base.gameObject.AddComponent<MeshFilter>();
		}
		float uvY = UvRect.y;
		meshFilter_0.sharedMesh = method_2(array, GetForwards(array), ref uvY, meshFilter_0.sharedMesh);
	}

	public void method_1(Vector3[] points, int parts, float cullingRate)
	{
		Vector3[] forwards = GetForwards(points);
		smethod_0(points, forwards, parts, out var lodsPoints, out var lodsForwards);
		MeshRenderer component = GetComponent<MeshRenderer>();
		float uvY = UvRect.y;
		for (int i = 0; i < parts; i++)
		{
			Mesh mesh = null;
			string n = "lod[" + i + "]";
			Transform transform = base.transform.Find(n);
			if (transform == null)
			{
				GameObject obj = new GameObject(n, typeof(MeshFilter), typeof(MeshRenderer), typeof(LODGroup))
				{
					isStatic = base.gameObject.isStatic
				};
				transform = obj.transform;
				transform.parent = base.transform;
				transform.localPosition = Vector3.zero;
				MeshRenderer component2 = obj.GetComponent<MeshRenderer>();
				component2.shadowCastingMode = component.shadowCastingMode;
				component2.receiveShadows = component.receiveShadows;
				component2.sharedMaterials = component.sharedMaterials;
				component2.lightProbeUsage = component.lightProbeUsage;
				component2.reflectionProbeUsage = component.reflectionProbeUsage;
				component2.probeAnchor = component.probeAnchor;
			}
			else
			{
				mesh = transform.GetComponent<MeshFilter>().sharedMesh;
			}
			transform.GetComponent<MeshFilter>().sharedMesh = method_2(lodsPoints[i], lodsForwards[i], ref uvY, mesh);
			transform.GetComponent<LODGroup>().SetLODs(new LOD[1]
			{
				new LOD(cullingRate, new Renderer[1] { transform.GetComponent<MeshRenderer>() })
			});
		}
	}

	public Vector3 FixOnGround(Vector3 point)
	{
		Vector3 origin = point;
		origin.y += 100f;
		origin += base.transform.position;
		if (Physics.Raycast(origin, Vector3.down, out var hitInfo, 200f, Mask))
		{
			point = hitInfo.point;
			point.y += RoadOffset;
			point -= base.transform.position;
		}
		return point;
	}

	public Mesh method_2(Vector3[] points, Vector3[] forwards, ref float uvY, Mesh mesh = null)
	{
		int num = points.Length * 2;
		Vector3[] array = new Vector3[num];
		Vector3[] normals = new Vector3[num];
		Vector4[] tangents = new Vector4[num];
		Vector2[] uv = new Vector2[num];
		int[] array2 = new int[(points.Length - 1) * 6];
		Color[] colors = new Color[num];
		Color[] array3 = null;
		if (mesh != null)
		{
			array3 = mesh.colors;
		}
		smethod_1(points, forwards, array2, array, normals, tangents, uv, UvRect, Width, ref uvY, colors, array3, bool_0);
		if (mesh == null)
		{
			mesh = new Mesh
			{
				vertices = array,
				normals = normals,
				tangents = tangents,
				triangles = array2,
				uv = uv,
				name = "RoadSolidMarkGenerator GetMesh"
			};
		}
		else
		{
			mesh.Clear();
			mesh.vertices = array;
			mesh.normals = normals;
			mesh.tangents = tangents;
			mesh.triangles = array2;
			mesh.uv = uv;
			if (array3 != null)
			{
				mesh.colors = colors;
			}
		}
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		return mesh;
	}

	public static void smethod_0(Vector3[] points, Vector3[] forwards, int parts, out Vector3[][] lodsPoints, out Vector3[][] lodsForwards)
	{
		int num = points.Length;
		float[] array = new float[num];
		lodsPoints = new Vector3[parts][];
		lodsForwards = new Vector3[parts][];
		float num2 = 0f;
		array[0] = 0f;
		for (int i = 1; i < num; i++)
		{
			num2 = (array[i] = num2 + Vector3.Distance(points[i - 1], points[i]));
		}
		float num3 = num2 / (float)parts;
		float num4 = num3;
		int num5 = 1;
		int num6 = 0;
		int num7 = 0;
		while (true)
		{
			if (num5 >= num)
			{
				return;
			}
			if (!(array[num5] < num4))
			{
				num4 += num3;
				lodsPoints[num6] = new Vector3[num5 - num7 + 1];
				lodsForwards[num6] = new Vector3[lodsPoints[num6].Length];
				Array.Copy(points, num7, lodsPoints[num6], 0, lodsPoints[num6].Length);
				Array.Copy(forwards, num7, lodsForwards[num6], 0, lodsForwards[num6].Length);
				num7 = num5;
				num6++;
				if (num6 == parts - 1)
				{
					break;
				}
			}
			num5++;
		}
		lodsPoints[num6] = new Vector3[num - num7];
		lodsForwards[num6] = new Vector3[lodsPoints[num6].Length];
		Array.Copy(points, num7, lodsPoints[num6], 0, lodsPoints[num6].Length);
		Array.Copy(forwards, num7, lodsForwards[num6], 0, lodsForwards[num6].Length);
	}

	public static void smethod_1(Vector3[] points, Vector3[] forwards, int[] tris, Vector3[] verts, Vector3[] normals, Vector4[] tangents, Vector2[] uv, Rect uvRect, float width, ref float uvY, Color[] colors, Color[] oldColors, bool copyColorsInReverseOrder)
	{
		width *= 0.5f;
		Vector3 up = Vector3.up;
		float xMin = uvRect.xMin;
		float xMax = uvRect.xMax;
		float height = uvRect.height;
		int i = 0;
		int num = 0;
		for (; i < points.Length; i++)
		{
			if (i != 0)
			{
				uvY += Vector3.Distance(points[i], points[i - 1]) * height;
			}
			Vector3 vector = points[i];
			Vector3 normalized = RoadSplineGenerator.CrossUp(forwards[i]).normalized;
			Vector3 vector2 = normalized * width;
			Vector4 vector3 = new Vector4(normalized.x, normalized.y, normalized.z, 1f);
			int num2 = num++;
			int num3 = num++;
			verts[num2] = vector - vector2;
			verts[num3] = vector + vector2;
			normals[num2] = up;
			normals[num3] = up;
			tangents[num2] = vector3;
			tangents[num3] = vector3;
			uv[num2] = new Vector2(xMin, uvY);
			uv[num3] = new Vector2(xMax, uvY);
			if (oldColors != null && oldColors.Length > num3 && !copyColorsInReverseOrder)
			{
				colors[num2] = oldColors[num2];
				colors[num3] = oldColors[num3];
			}
		}
		if (oldColors != null && copyColorsInReverseOrder)
		{
			for (int j = 0; j < oldColors.Length && colors.Length - 1 - j >= 0; j++)
			{
				colors[colors.Length - 1 - j] = oldColors[oldColors.Length - 1 - j];
			}
		}
		int num4 = points.Length - 1;
		int k = 0;
		int num5 = 0;
		for (; k < num4; k++)
		{
			int num6 = k << 1;
			int num7 = num6 + 1;
			int num8 = num6 + 2;
			int num9 = num6 + 3;
			tris[num5++] = num6;
			tris[num5++] = num7;
			tris[num5++] = num9;
			tris[num5++] = num9;
			tris[num5++] = num8;
			tris[num5++] = num6;
		}
	}

	public static Vector3[] GeneratePoints(Vector3[] positions, float start, float end, float shift)
	{
		int num = positions.Length;
		float[] array = new float[num];
		float num2 = 0f;
		array[0] = 0f;
		for (int i = 1; i < num; i++)
		{
			num2 = (array[i] = num2 + Vector3.Distance(positions[i - 1], positions[i]));
		}
		start *= num2;
		end *= num2;
		int num3 = -1;
		int num4 = -1;
		Vector3 vector = default(Vector3);
		Vector3 vector2 = default(Vector3);
		for (int j = 1; j < num; j++)
		{
			if (array[j] >= start)
			{
				float t = (start - array[j - 1]) / (array[j] - array[j - 1]);
				vector = Lerp(positions[j - 1], positions[j], t);
				num3 = j - 1;
				break;
			}
		}
		for (int k = num3; k < num; k++)
		{
			if (array[k] >= end)
			{
				float t2 = (end - array[k - 1]) / (array[k] - array[k - 1]);
				vector2 = Lerp(positions[k - 1], positions[k], t2);
				num4 = k + 1;
				break;
			}
		}
		num = num4 - num3;
		Vector3[] array2 = new Vector3[num];
		for (int l = 0; l < num; l++)
		{
			array2[l] = positions[l + num3];
		}
		array2[0] = vector;
		array2[num - 1] = vector2;
		Vector3[] array3 = new Vector3[num];
		Vector3[] array4 = new Vector3[num];
		int num5 = num - 1;
		array3[0] = array2[1] - array2[0];
		array3[num5] = array2[num5] - array2[num5 - 1];
		for (int m = 1; m < num5; m++)
		{
			array3[m] = array2[m + 1] - array2[m - 1];
		}
		for (int n = 0; n < num; n++)
		{
			array4[n] = RoadSplineGenerator.CrossUp(array3[n]).normalized;
		}
		for (int num6 = 0; num6 < num; num6++)
		{
			array2[num6] += array4[num6] * shift;
		}
		return array2;
	}

	public static Vector3[] GetForwards(Vector3[] points)
	{
		Vector3[] array = new Vector3[points.Length];
		int num = points.Length - 1;
		array[0] = points[1] - points[0];
		array[num] = points[num] - points[num - 1];
		for (int i = 1; i < num; i++)
		{
			array[i] = points[i + 1] - points[i - 1];
		}
		return array;
	}

	public static Vector3 Lerp(Vector3 from, Vector3 to, float t)
	{
		return new Vector3(from.x + (to.x - from.x) * t, from.y + (to.y - from.y) * t, from.z + (to.z - from.z) * t);
	}
}
