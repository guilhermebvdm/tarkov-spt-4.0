using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class LocalDustParticlesParent : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	public class Class610
	{
		public static readonly Class610 class610_0 = new Class610();

		public static Func<LocalDustParticles, bool> func_0;

		public bool method_0(LocalDustParticles ldp)
		{
			return !ldp;
		}
	}

	public LocalDustParticles[] Childs;

	public float TimeSpeed;

	public float Size;

	public int QuadTileSetSide = 2;

	public float UvTilePadding;

	private Material material_0;

	private static Vector2 vector2_0;

	private Vector3 vector3_0 = new Vector3(41.234f, 50.43f, 60.753f);

	public bool Regenerate;

	private Mesh mesh_0;

	private float float_0;

	private static readonly int int_0 = Shader.PropertyToID("_SelfTime");

	public void Awake()
	{
		GenerateMesh();
	}

	public void OnValidate()
	{
		method_0();
		if (Regenerate)
		{
			Regenerate = false;
			GenerateMesh();
		}
	}

	public void method_0()
	{
		QuadTileSetSide = Mathf.Max(1, QuadTileSetSide);
		Size = Mathf.Max(0f, Size);
	}

	public void GenerateMesh()
	{
		method_0();
		QuadTileSetSide = Mathf.Max(1, QuadTileSetSide);
		Size = Mathf.Max(0f, Size);
		material_0 = GetComponent<Renderer>().sharedMaterial;
		vector2_0 = new Vector2(Size, Size);
		Vector2[][] uvs = smethod_3(QuadTileSetSide, QuadTileSetSide, UvTilePadding);
		Childs = smethod_4(Childs, (LocalDustParticles ldp) => !ldp);
		if (Childs.Length == 0)
		{
			return;
		}
		int num = 0;
		LocalDustParticles[] childs = Childs;
		foreach (LocalDustParticles localDustParticles in childs)
		{
			num += localDustParticles.Count;
		}
		Vector3[] array = new Vector3[num << 2];
		Vector4[] tangents = new Vector4[num << 2];
		Vector2[] array2 = new Vector2[num << 2];
		Vector2[] array3 = new Vector2[num << 2];
		Color32[] array4 = new Color32[num << 2];
		int[] triangles = new int[num * 6];
		int num3 = 0;
		Bounds bounds = new Bounds(Childs[0].transform.position, Vector3.zero);
		childs = Childs;
		foreach (LocalDustParticles localDustParticles2 in childs)
		{
			Transform transform = localDustParticles2.transform;
			Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;
			float sideSpeed = localDustParticles2.SideSpeed;
			Bounds bounds2 = new Bounds(transform.position, transform.localScale * (1.5f + sideSpeed));
			bounds.Encapsulate(bounds2);
			int count = localDustParticles2.Count;
			Vector2[][] shifts = smethod_2(vector2_0, 6, localDustParticles2.SizeRandomness);
			Color32[] rndColors = smethod_1(localDustParticles2.ParticlesColor, 16, localDustParticles2.AlphaRandomness);
			for (int num4 = 0; num4 < count; num4++)
			{
				smethod_0(num3++, triangles, array, array2, array3, tangents, array4, localToWorldMatrix, sideSpeed, uvs, shifts, rndColors);
			}
		}
		base.transform.position = Vector3.zero;
		if (mesh_0 != null)
		{
			UnityEngine.Object.DestroyImmediate(mesh_0);
		}
		mesh_0 = new Mesh
		{
			vertices = array,
			triangles = triangles,
			uv = array2,
			uv2 = array3,
			tangents = tangents,
			colors32 = array4,
			bounds = bounds,
			name = "LocalDustParticlesParent GenerateMesh"
		};
		GetComponent<MeshFilter>().mesh = mesh_0;
	}

	public static void smethod_0(int i, int[] triangles, Vector3[] positions, Vector2[] uv0, Vector2[] uv1, Vector4[] tangents, Color32[] colors, Matrix4x4 matrix, float sideSpeed, Vector2[][] uvs, Vector2[][] shifts, Color32[] rndColors)
	{
		int num = i << 2;
		int num2 = num++;
		int num3 = num++;
		int num4 = num++;
		int num5 = num;
		int num6 = i * 6;
		triangles[num6++] = num2;
		triangles[num6++] = num3;
		triangles[num6++] = num5;
		triangles[num6++] = num5;
		triangles[num6++] = num4;
		triangles[num6] = num2;
		Vector3 vector = matrix.MultiplyPoint(new Vector3(UnityEngine.Random.value - 0.5f, UnityEngine.Random.value - 0.5f, UnityEngine.Random.value - 0.5f));
		positions[num2] = (positions[num3] = (positions[num4] = (positions[num5] = vector)));
		tangents[num2] = (tangents[num3] = (tangents[num4] = (tangents[num5] = new Vector4(UnityEngine.Random.value * 3f, UnityEngine.Random.value * 3f, UnityEngine.Random.value * 3f, (0.1f + UnityEngine.Random.value * 0.2f) * sideSpeed))));
		colors[num2] = (colors[num3] = (colors[num4] = (colors[num5] = rndColors[UnityEngine.Random.Range(0, rndColors.Length - 1)])));
		Vector2[] array = uvs[UnityEngine.Random.Range(0, uvs.Length - 1)];
		uv0[num2] = array[0];
		uv0[num3] = array[1];
		uv0[num4] = array[2];
		uv0[num5] = array[3];
		Vector2[] array2 = shifts[UnityEngine.Random.Range(0, shifts.Length - 1)];
		uv1[num2] = array2[0];
		uv1[num3] = array2[1];
		uv1[num4] = array2[2];
		uv1[num5] = array2[3];
	}

	public static Color32[] smethod_1(Color color, int count, float randomnessAlpha)
	{
		float minInclusive = color.a - randomnessAlpha;
		float maxInclusive = color.a + randomnessAlpha;
		Color32[] array = new Color32[count];
		for (int i = 0; i < count; i++)
		{
			color.a = UnityEngine.Random.Range(minInclusive, maxInclusive);
			array[i] = color;
		}
		return array;
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.white;
		Bounds bounds = GetComponent<MeshFilter>().sharedMesh.bounds;
		Gizmos.DrawWireCube(bounds.center, bounds.size);
	}

	public void OnDrawGizmos()
	{
		float num = (Time.realtimeSinceStartup - float_0) * TimeSpeed;
		float_0 = Time.realtimeSinceStartup;
		float num2 = 0.5f * num;
		vector3_0 += new Vector3(num, num, num);
		vector3_0 += new Vector3(1f + Mathf.PerlinNoise(vector3_0.y, 0.5f), 1f + Mathf.PerlinNoise(vector3_0.z, 743f), 1f + Mathf.PerlinNoise(vector3_0.x, -424f)) * num2;
		material_0.SetVector(int_0, new Vector4(vector3_0.x, vector3_0.y, vector3_0.z, 0f));
	}

	public void Update()
	{
		float num = Time.deltaTime * TimeSpeed;
		float num2 = 0.5f * num;
		vector3_0 += new Vector3(num, num, num);
		vector3_0 += new Vector3(1f + Mathf.PerlinNoise(vector3_0.y, 0.5f), 1f + Mathf.PerlinNoise(vector3_0.z, 743f), 1f + Mathf.PerlinNoise(vector3_0.x, -424f)) * num2;
		material_0.SetVector(int_0, new Vector4(vector3_0.x, vector3_0.y, vector3_0.z, 0f));
	}

	public static Vector2[][] smethod_2(Vector2 size, int steps, float randomnessSize)
	{
		Vector2[][] array = new Vector2[steps][];
		float num = MathF.PI * 2f / (float)steps;
		float num2 = 0f;
		for (int i = 0; i < steps; i++)
		{
			float num3 = UnityEngine.Random.Range(1f - randomnessSize, 1f + randomnessSize);
			Vector2 vector = new Vector2(Mathf.Sin(num2), Mathf.Cos(num2));
			vector.x *= size.x * num3;
			vector.y *= size.y * num3;
			Vector2 vector2 = new Vector2(vector.y, 0f - vector.x);
			num2 += num;
			array[i] = new Vector2[4]
			{
				vector,
				-vector2,
				vector2,
				-vector
			};
		}
		return array;
	}

	public static Vector2[][] smethod_3(int width, int heigth, float padding = 0f)
	{
		Vector2[][] array = new Vector2[width * heigth][];
		float num = 1f / (float)width;
		float num2 = 1f / (float)heigth;
		float num3 = padding * num;
		float num4 = (1f - padding) * num;
		float num5 = padding * num2;
		float num6 = (1f - padding) * num2;
		int num7 = 0;
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < heigth; j++)
			{
				Vector2[] array2 = new Vector2[4];
				float x = (float)i * num + num3;
				float x2 = (float)i * num + num4;
				float y = (float)j * num2 + num5;
				float y2 = (float)j * num2 + num6;
				array2[0] = new Vector2(x, y);
				array2[1] = new Vector2(x, y2);
				array2[2] = new Vector2(x2, y);
				array2[3] = new Vector2(x2, y2);
				array[num7++] = array2;
			}
		}
		return array;
	}

	public static T[] smethod_4<T>(T[] array, Func<T, bool> needToRemove)
	{
		LinkedList<T> linkedList = new LinkedList<T>(array);
		for (LinkedListNode<T> linkedListNode = linkedList.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			if (needToRemove(linkedListNode.Value))
			{
				linkedList.Remove(linkedListNode);
			}
		}
		array = new T[linkedList.Count];
		linkedList.CopyTo(array, 0);
		return array;
	}

	public static Texture2D[] smethod_5(Texture3D tex)
	{
		Color[] pixels = tex.GetPixels();
		int width = tex.width;
		int height = tex.height;
		Texture2D[] array = new Texture2D[tex.depth];
		for (int i = 0; i < array.Length; i++)
		{
			Color[] array2 = new Color[width * height];
			int num = array2.Length * i;
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] = pixels[j + num];
			}
			array[i] = new Texture2D(width, height, TextureFormat.Alpha8, mipChain: false);
			array[i].SetPixels(array2);
			array[i].name = "LocalDustParticles";
			array[i].Apply();
		}
		return array;
	}

	public static Texture3D smethod_6(Texture2D[] textures)
	{
		int width = textures[0].width;
		int num = width * width;
		int num2 = textures.Length;
		Color[] array = new Color[num * num2];
		for (int i = 0; i < num2; i++)
		{
			Color[] pixels = textures[i].GetPixels();
			int num3 = i * num;
			for (int j = 0; j < num; j++)
			{
				array[num3 + j] = pixels[j];
			}
		}
		Texture3D texture3D = new Texture3D(width, width, num2, TextureFormat.Alpha8, mipChain: false);
		texture3D.SetPixels(array);
		texture3D.Apply();
		return texture3D;
	}

	public static Texture2D smethod_7(Texture2D tex, int width, int height, float xScale, float yScale, float xOffset, float yOffset)
	{
		Texture2D texture2D = new Texture2D(width, height, tex.format, mipChain: false);
		texture2D.name = "ResizedLocalDustParticles";
		float num = yScale / (float)height;
		float num2 = xScale / (float)width;
		float num3 = (1f - yScale) * 0.5f + yOffset;
		float num4 = (1f - xScale) * 0.5f + xOffset;
		float num5 = num3;
		for (int i = 0; i < height; i++)
		{
			float num6 = num4;
			for (int j = 0; j < width; j++)
			{
				Color pixelBilinear = tex.GetPixelBilinear(num6, num5);
				texture2D.SetPixel(j, i, pixelBilinear);
				num6 += num2;
			}
			num5 += num;
		}
		texture2D.Apply();
		return texture2D;
	}

	public static void smethod_8(Texture2D[] textures, int width, int height)
	{
		float num = 1f / (float)textures.Length;
		float num2 = 0f;
		for (int i = 0; i < textures.Length; i++)
		{
			float num3 = Mathf.Lerp(0.16f, 1f, num2);
			textures[i] = smethod_7(textures[i], width, height, num3, num3, 0.065f, -0.015f);
			num2 += num;
		}
	}

	public void OnDestroy()
	{
		if (mesh_0 != null)
		{
			UnityEngine.Object.DestroyImmediate(mesh_0);
		}
	}
}
