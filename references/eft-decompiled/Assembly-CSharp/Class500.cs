using System;
using UnityEngine;
using UnityEngine.Rendering;

public class Class500
{
	[NonSerialized]
	public Texture2D Texture2D_0;

	[NonSerialized]
	public Material Material_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public static int Int_2 = Shader.PropertyToID("_MainTex");

	[NonSerialized]
	public static int Int_3 = Shader.PropertyToID("_FlareProj");

	[NonSerialized]
	public static int Int_4 = Shader.PropertyToID("_BrightTexture");

	[NonSerialized]
	public static int Int_5 = Shader.PropertyToID("_Intensity");

	public void RebuildMeshIfNeeded(int width, int height, float spriteRelativeScaleX, float spriteRelativeScaleY, ref Mesh[] meshes)
	{
		if (Int_0 == width && Int_1 == height && Float_0 == spriteRelativeScaleX && Float_1 == spriteRelativeScaleY && meshes != null)
		{
			return;
		}
		if (meshes != null)
		{
			Mesh[] array = meshes;
			for (int i = 0; i < array.Length; i++)
			{
				UnityEngine.Object.DestroyImmediate(array[i], allowDestroyingAssets: true);
			}
		}
		meshes = null;
		BuildMeshes(width, height, spriteRelativeScaleX, spriteRelativeScaleY, ref meshes);
	}

	public void BuildMeshes(int width, int height, float spriteRelativeScaleX, float spriteRelativeScaleY, ref Mesh[] meshes)
	{
		int num = 10833;
		int num2 = width * height;
		int num3 = Mathf.CeilToInt(1f * (float)num2 / 10833f);
		meshes = new Mesh[num3];
		int num4 = num2;
		Int_0 = width;
		Int_1 = height;
		Float_0 = spriteRelativeScaleX;
		Float_1 = spriteRelativeScaleY;
		int num5 = 0;
		for (int i = 0; i < num3; i++)
		{
			Mesh mesh = new Mesh();
			mesh.hideFlags = HideFlags.HideAndDontSave;
			int num6 = num4;
			if (num4 > num)
			{
				num6 = num;
			}
			num4 -= num6;
			Vector3[] vertices = new Vector3[num6 * 4];
			int[] triangles = new int[num6 * 6];
			Vector2[] array = new Vector2[num6 * 4];
			Vector2[] array2 = new Vector2[num6 * 4];
			Vector3[] normals = new Vector3[num6 * 4];
			Color[] colors = new Color[num6 * 4];
			float num7 = Float_0 * (float)width;
			float num8 = Float_1 * (float)height;
			for (int j = 0; j < num6; j++)
			{
				int num9 = num5 % width;
				int num10 = (num5 - num9) / width;
				SetupSprite(j, num9, num10, vertices, triangles, array, array2, normals, colors, new Vector2((float)num9 / (float)width, 1f - (float)num10 / (float)height), num7 * 0.5f, num8 * 0.5f);
				num5++;
			}
			mesh.vertices = vertices;
			mesh.triangles = triangles;
			mesh.colors = colors;
			mesh.uv = array;
			mesh.uv2 = array2;
			mesh.normals = normals;
			mesh.RecalculateBounds();
			mesh.UploadMeshData(markNoLongerReadable: true);
			meshes[i] = mesh;
		}
	}

	public void Clear(ref Mesh[] meshes)
	{
		if (meshes != null)
		{
			Mesh[] array = meshes;
			for (int i = 0; i < array.Length; i++)
			{
				UnityEngine.Object.DestroyImmediate(array[i], allowDestroyingAssets: true);
			}
		}
		meshes = null;
	}

	public void SetTexture(Texture2D texture)
	{
		Texture2D_0 = texture;
		Material_0.SetTexture(Int_2, Texture2D_0);
	}

	public void SetMaterial(Material flareMaterial)
	{
		Material_0 = flareMaterial;
	}

	public void RenderFlare(CommandBuffer _cmdBuf, RenderTexture brightPixels, RenderTexture destination, float intensity, ref Mesh[] meshes)
	{
		RenderTexture active = RenderTexture.active;
		if (_cmdBuf != null)
		{
			_cmdBuf.SetRenderTarget(destination);
			_cmdBuf.ClearRenderTarget(clearDepth: true, clearColor: true, Color.black);
		}
		else
		{
			RenderTexture.active = destination;
			GL.Clear(clearDepth: true, clearColor: true, Color.black);
		}
		Matrix4x4 value = Matrix4x4.Ortho(0f, Int_0, 0f, Int_1, -1f, 1f);
		Material_0.SetMatrix(Int_3, value);
		Material_0.SetTexture(Int_4, brightPixels);
		Material_0.SetFloat(Int_5, intensity);
		if (Material_0.SetPass(0))
		{
			for (int i = 0; i < meshes.Length; i++)
			{
				if (_cmdBuf != null)
				{
					_cmdBuf.DrawMesh(meshes[i], Matrix4x4.identity, Material_0);
				}
				else
				{
					Graphics.DrawMeshNow(meshes[i], Matrix4x4.identity);
				}
			}
		}
		else
		{
			Debug.LogError("Can't render flare mesh");
		}
		RenderTexture.active = active;
		_cmdBuf?.SetRenderTarget(active);
	}

	public void SetupSprite(int idx, int x, int y, Vector3[] vertices, int[] triangles, Vector2[] uv0, Vector2[] uv1, Vector3[] normals, Color[] colors, Vector2 targetPixelUV, float halfWidth, float halfHeight)
	{
		int num = idx * 4;
		int num2 = idx * 6;
		triangles[num2] = num;
		triangles[num2 + 1] = num + 2;
		triangles[num2 + 2] = num + 1;
		triangles[num2 + 3] = num + 2;
		triangles[num2 + 4] = num + 3;
		triangles[num2 + 5] = num + 1;
		vertices[num] = new Vector3(0f - halfWidth + (float)x, 0f - halfHeight + (float)y, 0f);
		vertices[num + 1] = new Vector3(halfWidth + (float)x, 0f - halfHeight + (float)y, 0f);
		vertices[num + 2] = new Vector3(0f - halfWidth + (float)x, halfHeight + (float)y, 0f);
		vertices[num + 3] = new Vector3(halfWidth + (float)x, halfHeight + (float)y, 0f);
		Vector2 vector = targetPixelUV;
		colors[num] = new Color((0f - halfWidth) / (float)Int_0 + vector.x, (0f - halfHeight) * -1f / (float)Int_1 + vector.y, 0f, 0f);
		colors[num + 1] = new Color(halfWidth / (float)Int_0 + vector.x, (0f - halfHeight) * -1f / (float)Int_1 + vector.y, 0f, 0f);
		colors[num + 2] = new Color((0f - halfWidth) / (float)Int_0 + vector.x, halfHeight * -1f / (float)Int_1 + vector.y, 0f, 0f);
		colors[num + 3] = new Color(halfWidth / (float)Int_0 + vector.x, halfHeight * -1f / (float)Int_1 + vector.y, 0f, 0f);
		normals[num] = -Vector3.forward;
		normals[num + 1] = -Vector3.forward;
		normals[num + 2] = -Vector3.forward;
		normals[num + 3] = -Vector3.forward;
		uv0[num] = new Vector2(0f, 0f);
		uv0[num + 1] = new Vector2(1f, 0f);
		uv0[num + 2] = new Vector2(0f, 1f);
		uv0[num + 3] = new Vector2(1f, 1f);
		uv1[num] = targetPixelUV;
		uv1[num + 1] = targetPixelUV;
		uv1[num + 2] = targetPixelUV;
		uv1[num + 3] = targetPixelUV;
	}
}
