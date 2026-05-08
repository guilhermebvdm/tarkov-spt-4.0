using EFT.BlitDebug;
using UnityEngine;

public class SpecialBlur : MonoBehaviour
{
	public Shader Shader;

	public RenderTexture From;

	public RenderTexture To;

	public RectTransform Target;

	public int BlurIterations = 1;

	public float Snrength = 1f;

	public float Aspect = 1f;

	private Material material_0;

	private static readonly int int_0 = Shader.PropertyToID("offsets");

	public void Start()
	{
		OnValidate();
	}

	public void OnValidate()
	{
		GetComponent<Camera>().aspect = Aspect;
		method_1(From, To);
	}

	public Material method_0()
	{
		if (material_0 != null)
		{
			return material_0;
		}
		return material_0 = new Material(Shader);
	}

	public void method_1(RenderTexture from, RenderTexture to)
	{
		if (Target != null)
		{
			int num = (int)Target.rect.width;
			int num2 = (int)Target.rect.height;
			if (num != from.width || num2 != from.height)
			{
				from.Release();
				from.width = num;
				from.height = num2;
				from.Create();
				to.Release();
				to.width = num;
				to.height = num2;
				to.Create();
			}
		}
	}

	public void method_2(RenderTexture from, RenderTexture to)
	{
		method_1(from, to);
		int num = to.width >> 1;
		int num2 = to.height >> 1;
		RenderTexture temporary = RenderTexture.GetTemporary(num, num2, 0, RenderTextureFormat.ARGB32);
		temporary.name = "SpecialBlur RT";
		Graphics.Blit(from, to);
		Material material = method_0();
		for (int i = 0; i < BlurIterations; i++)
		{
			float num3 = 1 << i;
			material.SetVector(int_0, new Vector4(Snrength * num3 / (float)num, 0f, 0f, 0f));
			DebugGraphics.Blit(to, temporary, material, 0);
			material.SetVector(int_0, new Vector4(0f, Snrength * num3 / (float)num2, 0f, 0f));
			DebugGraphics.Blit(temporary, to, material, 0);
		}
		RenderTexture.ReleaseTemporary(temporary);
	}

	public void OnPostRender()
	{
		method_2(From, To);
	}
}
