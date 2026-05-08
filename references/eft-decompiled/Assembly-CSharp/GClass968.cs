using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public class GClass968
{
	[NonSerialized]
	[CompilerGenerated]
	public CommandBuffer CommandBuffer_0;

	[NonSerialized]
	[CompilerGenerated]
	public Bounds Bounds_0;

	[NonSerialized]
	public Material Material_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public SortedList<int, Material> SortedList_0 = new SortedList<int, Material>();

	[NonSerialized]
	public uint[] Uint_0 = new uint[5];

	[NonSerialized]
	public ComputeBuffer ComputeBuffer_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public int Int_2;

	[NonSerialized]
	public TextureFormat TextureFormat_0;

	[NonSerialized]
	public static ComputeBuffer ComputeBuffer_1;

	[NonSerialized]
	public static ComputeBuffer ComputeBuffer_2;

	[NonSerialized]
	public static ComputeBuffer ComputeBuffer_3;

	[NonSerialized]
	public static ComputeBuffer ComputeBuffer_4;

	[NonSerialized]
	public static ComputeBuffer ComputeBuffer_5;

	[NonSerialized]
	public static ComputeBuffer ComputeBuffer_6;

	[NonSerialized]
	public static ComputeBuffer ComputeBuffer_7;

	[NonSerialized]
	public static ComputeBuffer ComputeBuffer_8;

	[NonSerialized]
	public static ComputeBuffer ComputeBuffer_9;

	[NonSerialized]
	public static ComputeBuffer ComputeBuffer_10;

	[NonSerialized]
	public static ComputeBuffer ComputeBuffer_11;

	[NonSerialized]
	public static ComputeBuffer ComputeBuffer_12;

	[NonSerialized]
	public static ComputeBuffer ComputeBuffer_13;

	[NonSerialized]
	public int Int_3;

	[NonSerialized]
	public static int Int_4 = Shader.PropertyToID("localToWorldMatrixBuffer");

	[NonSerialized]
	public static int Int_5 = Shader.PropertyToID("worldToLocalMatrixBuffer");

	[NonSerialized]
	public static int Int_6 = Shader.PropertyToID("uvStartEndBuffer");

	[NonSerialized]
	public static int Int_7 = Shader.PropertyToID("normalEdgeClipAndFadeoffBuffer");

	[NonSerialized]
	public static int Int_8 = Shader.PropertyToID("tintBuffer");

	[NonSerialized]
	public static int Int_9 = Shader.PropertyToID("textureArrayIndexBuffer");

	[NonSerialized]
	public static int Int_10 = Shader.PropertyToID("scaleOffsetBuffer");

	[NonSerialized]
	public static int Int_11 = Shader.PropertyToID("colorBuffer");

	[NonSerialized]
	public static int Int_12 = Shader.PropertyToID("normalPowerBuffer");

	[NonSerialized]
	public static int Int_13 = Shader.PropertyToID("specularColorBuffer");

	[NonSerialized]
	public static int Int_14 = Shader.PropertyToID("specSmoothnessBuffer");

	[NonSerialized]
	public static int Int_15 = Shader.PropertyToID("alphaMultiplierBuffer");

	[NonSerialized]
	public static int Int_16 = Shader.PropertyToID("temperatureBuffer");

	[NonSerialized]
	public static int Int_17 = Shader.PropertyToID("_Color");

	[NonSerialized]
	public static int Int_18 = Shader.PropertyToID("_NormalPower");

	[NonSerialized]
	public static int Int_19 = Shader.PropertyToID("_SpecularColor");

	[NonSerialized]
	public static int Int_20 = Shader.PropertyToID("_SpecSmoothness");

	[NonSerialized]
	public static int Int_21 = Shader.PropertyToID("_AlphaMultiplier");

	[NonSerialized]
	public static int Int_22 = Shader.PropertyToID("_Temperature");

	[NonSerialized]
	public static int Int_23 = Shader.PropertyToID("_MainTex");

	[NonSerialized]
	public static int Int_24 = Shader.PropertyToID("_BumpMap");

	[NonSerialized]
	public static int Int_25 = Shader.PropertyToID("_MainTexArray");

	[NonSerialized]
	public static int Int_26 = Shader.PropertyToID("_BumpMapArray");

	[NonSerialized]
	public Texture2DArray Texture2DArray_0;

	[NonSerialized]
	public Texture2DArray Texture2DArray_1;

	public ComputeBuffer ArgsBuffer => ComputeBuffer_0;

	public Material Material => Material_0;

	public CommandBuffer CommandBuffer
	{
		[CompilerGenerated]
		get
		{
			return CommandBuffer_0;
		}
		[CompilerGenerated]
		set
		{
			CommandBuffer_0 = value;
		}
	}

	public Bounds Bounds
	{
		[CompilerGenerated]
		get
		{
			return Bounds_0;
		}
		[CompilerGenerated]
		set
		{
			Bounds_0 = value;
		}
	}

	public int Count => Int_3;

	public int StartIndex => Int_0;

	public ComputeBuffer LocalToWorldMatrixBuffer => ComputeBuffer_1;

	public ComputeBuffer WorldToLocalMatrixBuffer => ComputeBuffer_2;

	public ComputeBuffer UVStartEndBuffer => ComputeBuffer_3;

	public ComputeBuffer NormalEdgeClipAndFadeoffBuffer => ComputeBuffer_4;

	public ComputeBuffer TintBuffer => ComputeBuffer_5;

	public static void InitGlobalBuffers(int decalsCount)
	{
		if (decalsCount != 0)
		{
			smethod_0<Matrix4x4>(ref ComputeBuffer_1, decalsCount);
			smethod_0<Matrix4x4>(ref ComputeBuffer_2, decalsCount);
			smethod_0<Vector4>(ref ComputeBuffer_3, decalsCount);
			smethod_0<Vector4>(ref ComputeBuffer_4, decalsCount);
			smethod_0<Vector4>(ref ComputeBuffer_5, decalsCount);
			smethod_0<int>(ref ComputeBuffer_6, decalsCount);
			smethod_0<Color>(ref ComputeBuffer_7, decalsCount);
			smethod_0<Color>(ref ComputeBuffer_8, decalsCount);
			smethod_0<float>(ref ComputeBuffer_9, decalsCount);
			smethod_0<Color>(ref ComputeBuffer_10, decalsCount);
			smethod_0<float>(ref ComputeBuffer_11, decalsCount);
			smethod_0<float>(ref ComputeBuffer_12, decalsCount);
			smethod_0<Vector4>(ref ComputeBuffer_13, decalsCount);
		}
	}

	public void InitInstance(Material material, bool isMaterialInstance, int decalsCount, Mesh mesh)
	{
		Material_0 = material;
		Bool_0 = isMaterialInstance;
		if (decalsCount != 0)
		{
			Int_3 = decalsCount;
			method_0(decalsCount, mesh);
		}
	}

	public void SetStartIndex(int startIndex)
	{
		Int_0 = startIndex;
	}

	public void method_0(int decalsCount, Mesh mesh)
	{
		Uint_0[0] = mesh.GetIndexCount(0);
		Uint_0[1] = (uint)decalsCount;
		Uint_0[2] = mesh.GetIndexStart(0);
		Uint_0[3] = mesh.GetBaseVertex(0);
		Uint_0[4] = 0u;
		if (ComputeBuffer_0 == null)
		{
			ComputeBuffer_0 = new ComputeBuffer(1, Uint_0.Length * 4, ComputeBufferType.DrawIndirect);
		}
		ComputeBuffer_0.SetData(Uint_0);
	}

	public static void smethod_0<[Attribute1] T>(ref ComputeBuffer computeBuffer, int decalsCount) where T : struct
	{
		if (computeBuffer != null)
		{
			computeBuffer.Release();
		}
		computeBuffer = new ComputeBuffer(decalsCount, Unsafe.SizeOf<T>());
	}

	public void UpdateBuffers(List<StaticDeferredDecal> decals, Mesh mesh, Bounds bounds)
	{
		if (decals.Count == 0)
		{
			return;
		}
		method_3(decals.Count, mesh);
		method_4(decals);
		method_5(decals);
		method_6(decals);
		method_7(decals);
		method_8(decals);
		if (CommandBuffer != null)
		{
			CommandBuffer.Dispose();
		}
		CommandBuffer = new CommandBuffer();
		CommandBuffer.name = "StaticDeferredDecalInstanceCB";
		Bounds = bounds;
		if (!method_9(decals))
		{
			return;
		}
		Texture2D texture2D = Material_0.mainTexture as Texture2D;
		if (texture2D.format != TextureFormat.DXT1Crunched && texture2D.format != TextureFormat.DXT5Crunched)
		{
			method_1(ref Texture2DArray_0, Material_0.mainTexture.width, Material_0.mainTexture.height, texture2D.format, Int_23);
			Material_0.SetTexture(Int_25, Texture2DArray_0);
			if (Int_1 != 0 && Int_2 != 0)
			{
				method_1(ref Texture2DArray_1, Int_1, Int_2, TextureFormat_0, Int_24);
				Material_0.EnableKeyword("BUMP_MAP");
				Material_0.SetTexture(Int_26, Texture2DArray_1);
			}
			else
			{
				Material_0.DisableKeyword("BUMP_MAP");
			}
		}
		else
		{
			Debug.LogError("Crunched texture! " + texture2D.name);
		}
	}

	public void method_1(ref Texture2DArray textureArray, int width, int height, TextureFormat format, int textureID)
	{
		method_12(ref textureArray);
		int num = Mathf.RoundToInt(Mathf.Log(width >> QualitySettings.globalTextureMipmapLimit, 2f)) + 1;
		int num2 = Mathf.RoundToInt(Mathf.Log(height >> QualitySettings.globalTextureMipmapLimit, 2f)) + 1;
		int num3 = Mathf.Max(num, num2);
		num3 -= Mathf.Abs(num - num2) + 2;
		num3 = Mathf.Max(num3, 1);
		textureArray = new Texture2DArray(width >> QualitySettings.globalTextureMipmapLimit, height >> QualitySettings.globalTextureMipmapLimit, SortedList_0.Count, format, num3, linear: true);
		int num4 = 0;
		foreach (KeyValuePair<int, Material> item in SortedList_0)
		{
			if (item.Value.GetTexture(textureID) == null)
			{
				num4++;
				continue;
			}
			Texture2D texture2D = item.Value.GetTexture(textureID) as Texture2D;
			if (QualitySettings.globalTextureMipmapLimit == 0 && texture2D.mipmapCount == textureArray.mipmapCount)
			{
				Graphics.CopyTexture(texture2D, 0, textureArray, num4);
			}
			else
			{
				method_2(texture2D, textureArray, num4, 0, Mathf.Min(texture2D.mipmapCount, textureArray.mipmapCount));
			}
			num4++;
		}
	}

	public void method_2(Texture2D texture, Texture2DArray textureArray, int slice, int startMip, int endMip)
	{
		for (int i = startMip; i < endMip; i++)
		{
			int srcWidth = texture.width >> i;
			int srcHeight = texture.height >> i;
			Graphics.CopyTexture(texture, 0, i, 0, 0, srcWidth, srcHeight, textureArray, slice, i, 0, 0);
		}
	}

	public void method_3(int decalsCount, Mesh mesh)
	{
		if (ComputeBuffer_0 == null)
		{
			method_0(decalsCount, mesh);
			return;
		}
		Uint_0[1] = (uint)decalsCount;
		ComputeBuffer_0.SetData(Uint_0);
	}

	public void method_4(List<StaticDeferredDecal> decals)
	{
		Matrix4x4[] array = new Matrix4x4[decals.Count];
		for (int i = 0; i < decals.Count; i++)
		{
			array[i] = decals[i].transform.localToWorldMatrix;
		}
		ComputeBuffer_1.SetData(array, 0, Int_0, decals.Count);
		Material_0.SetBuffer(Int_4, ComputeBuffer_1);
	}

	public void method_5(List<StaticDeferredDecal> decals)
	{
		Matrix4x4[] array = new Matrix4x4[decals.Count];
		for (int i = 0; i < decals.Count; i++)
		{
			array[i] = decals[i].transform.worldToLocalMatrix;
		}
		ComputeBuffer_2.SetData(array, 0, Int_0, decals.Count);
		Material_0.SetBuffer(Int_5, ComputeBuffer_2);
	}

	public void method_6(List<StaticDeferredDecal> decals)
	{
		Vector4[] array = new Vector4[decals.Count];
		for (int i = 0; i < decals.Count; i++)
		{
			array[i] = decals[i].GetUVStartEnd();
		}
		ComputeBuffer_3.SetData(array, 0, Int_0, decals.Count);
		Material_0.SetBuffer(Int_6, ComputeBuffer_3);
	}

	public void method_7(List<StaticDeferredDecal> decals)
	{
		Vector4[] array = new Vector4[decals.Count];
		for (int i = 0; i < decals.Count; i++)
		{
			array[i] = decals[i].GetNormalEdgeClipAndFadeoff();
		}
		ComputeBuffer_4.SetData(array, 0, Int_0, decals.Count);
		Material_0.SetBuffer(Int_7, ComputeBuffer_4);
	}

	public void method_8(List<StaticDeferredDecal> decals)
	{
		Vector4[] array = new Vector4[decals.Count];
		for (int i = 0; i < decals.Count; i++)
		{
			array[i] = decals[i].Tint;
		}
		ComputeBuffer_5.SetData(array, 0, Int_0, decals.Count);
		Material_0.SetBuffer(Int_8, ComputeBuffer_5);
	}

	public bool method_9(List<StaticDeferredDecal> decals)
	{
		int count = SortedList_0.Count;
		SortedList_0.Clear();
		for (int i = 0; i < decals.Count; i++)
		{
			if (SortedList_0.ContainsKey(decals[i].DecalMaterial.GetHashCode()))
			{
				continue;
			}
			SortedList_0.Add(decals[i].DecalMaterial.GetHashCode(), decals[i].DecalMaterial);
			if (decals[i].DecalMaterial.HasProperty(Int_24))
			{
				Texture texture = decals[i].DecalMaterial.GetTexture(Int_24);
				if ((bool)texture)
				{
					Int_1 = texture.width;
					Int_2 = texture.height;
					TextureFormat_0 = (texture as Texture2D).format;
				}
			}
		}
		int[] array = new int[decals.Count];
		Vector4[] array2 = new Vector4[decals.Count];
		Color[] array3 = new Color[decals.Count];
		float[] array4 = new float[decals.Count];
		Color[] array5 = new Color[decals.Count];
		float[] array6 = new float[decals.Count];
		float[] array7 = new float[decals.Count];
		Vector4[] array8 = new Vector4[decals.Count];
		for (int j = 0; j < decals.Count; j++)
		{
			Material decalMaterial = decals[j].DecalMaterial;
			array[j] = SortedList_0.IndexOfValue(decalMaterial);
			array2[j].Set(decalMaterial.mainTextureScale.x, decalMaterial.mainTextureScale.y, decalMaterial.mainTextureOffset.x, decalMaterial.mainTextureOffset.y);
			array3[j] = (decalMaterial.HasProperty(Int_17) ? decalMaterial.GetColor(Int_17) : Color.white);
			array4[j] = (decalMaterial.HasProperty(Int_18) ? decalMaterial.GetFloat(Int_18) : 1f);
			array5[j] = (decalMaterial.HasProperty(Int_19) ? decalMaterial.GetColor(Int_19) : Color.white);
			array6[j] = (decalMaterial.HasProperty(Int_20) ? decalMaterial.GetFloat(Int_20) : 1f);
			array7[j] = (decalMaterial.HasProperty(Int_21) ? decalMaterial.GetFloat(Int_21) : 1f);
			array8[j] = (decalMaterial.HasProperty(Int_22) ? decalMaterial.GetVector(Int_22) : Vector4.zero);
		}
		ComputeBuffer_6.SetData(array, 0, Int_0, decals.Count);
		Material_0.SetBuffer(Int_9, ComputeBuffer_6);
		ComputeBuffer_7.SetData(array2, 0, Int_0, decals.Count);
		Material_0.SetBuffer(Int_10, ComputeBuffer_7);
		ComputeBuffer_8.SetData(array3, 0, Int_0, decals.Count);
		Material_0.SetBuffer(Int_11, ComputeBuffer_8);
		ComputeBuffer_9.SetData(array4, 0, Int_0, decals.Count);
		Material_0.SetBuffer(Int_12, ComputeBuffer_9);
		ComputeBuffer_10.SetData(array5, 0, Int_0, decals.Count);
		Material_0.SetBuffer(Int_13, ComputeBuffer_10);
		ComputeBuffer_11.SetData(array6, 0, Int_0, decals.Count);
		Material_0.SetBuffer(Int_14, ComputeBuffer_11);
		ComputeBuffer_12.SetData(array7, 0, Int_0, decals.Count);
		Material_0.SetBuffer(Int_15, ComputeBuffer_12);
		ComputeBuffer_13.SetData(array8, 0, Int_0, decals.Count);
		Material_0.SetBuffer(Int_16, ComputeBuffer_13);
		return count != SortedList_0.Count;
	}

	public void DestroyResources()
	{
		if (Bool_0)
		{
			UnityEngine.Object.Destroy(Material_0);
		}
		method_11(ref ComputeBuffer_0);
		method_10();
		method_12(ref Texture2DArray_0);
		method_12(ref Texture2DArray_1);
		SortedList_0.Clear();
	}

	public void method_10()
	{
		method_11(ref ComputeBuffer_1);
		method_11(ref ComputeBuffer_2);
		method_11(ref ComputeBuffer_3);
		method_11(ref ComputeBuffer_4);
		method_11(ref ComputeBuffer_5);
		method_11(ref ComputeBuffer_6);
		method_11(ref ComputeBuffer_7);
		method_11(ref ComputeBuffer_8);
		method_11(ref ComputeBuffer_9);
		method_11(ref ComputeBuffer_10);
		method_11(ref ComputeBuffer_11);
		method_11(ref ComputeBuffer_12);
		method_11(ref ComputeBuffer_13);
	}

	public void method_11(ref ComputeBuffer computeBuffer)
	{
		if (computeBuffer != null)
		{
			computeBuffer.Release();
		}
		computeBuffer = null;
	}

	public void method_12(ref Texture2DArray textureArray)
	{
		if ((bool)textureArray)
		{
			UnityEngine.Object.Destroy(textureArray);
		}
		textureArray = null;
	}
}
