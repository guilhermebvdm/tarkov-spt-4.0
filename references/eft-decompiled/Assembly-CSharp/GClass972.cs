using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public abstract class GClass972
{
	[NonSerialized]
	public static Material Material_0_1;

	[NonSerialized]
	public static Material Material_1_1;

	[NonSerialized]
	public static Material Material_2_1;

	[NonSerialized]
	public static Material Material_3_1;

	[NonSerialized]
	public static Material Material_4_1;

	[NonSerialized]
	public static Material Material_5_1;

	[NonSerialized]
	public static int Int_0;

	[NonSerialized]
	public static int Int_1;

	[NonSerialized]
	public static int Int_2;

	[NonSerialized]
	public static int Int_3;

	[NonSerialized]
	public static int Int_4;

	[NonSerialized]
	public static int Int_5;

	[NonSerialized]
	public static int Int_6;

	[NonSerialized]
	public static int Int_7;

	[NonSerialized]
	public static int Int_8;

	[NonSerialized]
	public static int Int_9;

	[NonSerialized]
	public static int Int_10;

	[NonSerialized]
	public static int Int_11;

	[NonSerialized]
	public static int Int_12;

	[NonSerialized]
	public static int Int_13;

	[NonSerialized]
	public static bool Bool_0;

	public static Material Material_0
	{
		get
		{
			if (Material_0_1 == null)
			{
				Material_0_1 = new Material(GClass872.Find("Hidden/Blur"));
			}
			return Material_0_1;
		}
	}

	public static Material Material_1
	{
		get
		{
			if (Material_1_1 == null)
			{
				Material_1_1 = new Material(GClass872.Find("Hidden/Lerp"));
			}
			return Material_1_1;
		}
	}

	public static Material Material_2
	{
		get
		{
			if (Material_2_1 == null)
			{
				Material_2_1 = new Material(GClass872.Find("Hidden/Transform"));
			}
			return Material_2_1;
		}
	}

	public static Material Material_3
	{
		get
		{
			if (Material_3_1 == null)
			{
				Material_3_1 = new Material(GClass872.Find("Hidden/ColorBlend"));
			}
			return Material_3_1;
		}
	}

	public static Material Material_4
	{
		get
		{
			if (Material_4_1 == null)
			{
				Material_4_1 = new Material(GClass872.Find("Hidden/Outline"));
			}
			return Material_4_1;
		}
	}

	public static Material Material_5
	{
		get
		{
			if (Material_5_1 == null)
			{
				Material_5_1 = new Material(GClass872.Find("Hidden/GetChannel"));
			}
			return Material_5_1;
		}
	}

	public static void smethod_0()
	{
		if (!Bool_0)
		{
			Int_0 = Shader.PropertyToID("_Color");
			Int_1 = Shader.PropertyToID("_SrcFactor");
			Int_2 = Shader.PropertyToID("_DstFactor");
			Int_3 = Shader.PropertyToID("_Operation");
			Int_4 = Shader.PropertyToID("_Intensity");
			Int_5 = Shader.PropertyToID("_BlurOffsets0");
			Int_6 = Shader.PropertyToID("_TexA");
			Int_7 = Shader.PropertyToID("_TexB");
			Int_8 = Shader.PropertyToID("_T");
			Int_9 = Shader.PropertyToID("_MaskColor");
			Int_10 = Shader.PropertyToID("_AddColor");
			Int_11 = Shader.PropertyToID("_MultColor");
			Int_12 = Shader.PropertyToID("_Mask");
			Int_13 = Shader.PropertyToID("_Offsets");
			Bool_0 = true;
		}
	}

	public static void MakeBlur(RenderTexture source, RenderTexture temp, float blurInPixels, float intensity = 1f)
	{
		MakeBlur(source, temp, new Vector2(blurInPixels / (float)source.width, blurInPixels / (float)source.height), intensity);
	}

	public static void MakeBlur(RenderTexture source, RenderTexture temp, Vector2 blur, float intensity = 1f)
	{
		smethod_0();
		Material material_ = Material_0;
		material_.SetFloat(Int_4, intensity);
		material_.mainTexture = source;
		material_.SetVector(Int_5, new Vector2(blur.x, 0f));
		DrawQuad(material_, temp);
		material_.mainTexture = temp;
		material_.SetVector(Int_5, new Vector2(0f, blur.y));
		DrawQuad(material_, source);
	}

	public static void LerpTextures(Texture textureA, Texture textureB, float t, RenderTexture destination)
	{
		smethod_0();
		Material material_ = Material_1;
		material_.SetTexture(Int_6, textureA);
		material_.SetTexture(Int_7, textureB);
		material_.SetFloat(Int_8, t);
		DrawQuad(material_, destination);
	}

	public static void GetChannel(Texture texture, int channel, RenderTexture destination)
	{
		Color mask = default(Color);
		if (channel < 0 || channel > 3)
		{
			throw new IndexOutOfRangeException("Invalid channel");
		}
		mask[channel] = 1f;
		GetChannel(texture, mask, destination);
	}

	public static void GetChannel(Texture texture, Color mask, RenderTexture destination)
	{
		smethod_0();
		Material material_ = Material_5;
		material_.mainTexture = texture;
		material_.SetColor(Int_9, mask);
		DrawQuad(material_, destination);
	}

	public static void TransformTexture(Texture texture, Color add, Color multiply, RenderTexture destination)
	{
		smethod_0();
		Material material_ = Material_2;
		material_.mainTexture = texture;
		material_.SetColor(Int_10, add);
		material_.SetColor(Int_11, multiply);
		DrawQuad(material_, destination);
	}

	public static void Treshold(Texture texture, float min, float max, RenderTexture destination)
	{
		float num = 1f / (max - min);
		float num2 = (0f - min) * num;
		TransformTexture(texture, new Color(num2, num2, num2, num2), new Color(num, num, num, num), destination);
	}

	public static void ColorBlend(Color color, RenderTexture destination, BlendMode srcFactor = BlendMode.SrcAlpha, BlendMode dstFactor = BlendMode.OneMinusSrcAlpha, BlendOp operation = BlendOp.Add)
	{
		smethod_0();
		Material material_ = Material_3;
		material_.SetColor(Int_0, color);
		material_.SetInt(Int_1, (int)srcFactor);
		material_.SetInt(Int_2, (int)dstFactor);
		material_.SetInt(Int_3, (int)operation);
		DrawQuad(material_, destination);
	}

	public static void ColorBlend(CommandBuffer commandBuffer, Color color, RenderTexture destination, BlendMode srcFactor = BlendMode.SrcAlpha, BlendMode dstFactor = BlendMode.OneMinusSrcAlpha, BlendOp operation = BlendOp.Add)
	{
		smethod_0();
		Material material_ = Material_3;
		material_.SetColor(Int_0, color);
		material_.SetInt(Int_1, (int)srcFactor);
		material_.SetInt(Int_2, (int)dstFactor);
		material_.SetInt(Int_3, (int)operation);
		DrawFullscreenTriangle(commandBuffer, material_, destination);
	}

	public static void TextureBlend(Texture texture, Color multiply, RenderTexture destination, BlendMode srcFactor = BlendMode.SrcAlpha, BlendMode dstFactor = BlendMode.OneMinusSrcAlpha, BlendOp operation = BlendOp.Add)
	{
		smethod_0();
		Material material_ = Material_3;
		material_.mainTexture = texture;
		material_.SetColor(Int_0, multiply);
		material_.SetInt(Int_1, (int)srcFactor);
		material_.SetInt(Int_2, (int)dstFactor);
		material_.SetInt(Int_3, (int)operation);
		DrawQuad(material_, destination, 1);
	}

	public static void Outline(Texture texture, Color mask, Color color, float outlineWidthInPixels, RenderTexture destination)
	{
		smethod_0();
		Material material_ = Material_4;
		material_.mainTexture = texture;
		material_.SetColor(Int_12, mask);
		material_.SetColor(Int_0, color);
		material_.SetVector(Int_13, new Vector4(outlineWidthInPixels / (float)texture.width, outlineWidthInPixels / (float)texture.height));
		DrawQuad(material_, destination);
	}

	public static void DrawQuad(Material material, RenderTexture destination, int pass = 0)
	{
		if (destination != null)
		{
			Graphics.SetRenderTarget(destination);
		}
		material.SetPass(pass);
		GL.Begin(7);
		GL.TexCoord2(0f, 0f);
		GL.Vertex3(-1f, -1f, 0f);
		GL.TexCoord2(0f, 1f);
		GL.Vertex3(-1f, 1f, 0f);
		GL.TexCoord2(1f, 1f);
		GL.Vertex3(1f, 1f, 0f);
		GL.TexCoord2(1f, 0f);
		GL.Vertex3(1f, -1f, 0f);
		GL.End();
	}

	public static void DrawFullscreenTriangle(CommandBuffer commandBuffer, Material material, RenderTexture destination, int pass = 0)
	{
		if (destination != null)
		{
			commandBuffer.SetRenderTarget(destination);
		}
		commandBuffer.DrawMesh(RuntimeUtilities.fullscreenTriangle, Matrix4x4.identity, material, 0, pass);
	}

	public static void SafeDestroy(UnityEngine.Object obj)
	{
		if (!(obj == null) && (bool)obj)
		{
			UnityEngine.Object.Destroy(obj);
		}
	}
}
