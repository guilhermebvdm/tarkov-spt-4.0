using UnityEngine;

public class AbstractMaterialSpeedVertPaintShaderSolid : ScriptableObject
{
	private static readonly int int_0 = Shader.PropertyToID("_MainTex0");

	private static readonly int int_1 = Shader.PropertyToID("_MainTex1");

	private static readonly int int_2 = Shader.PropertyToID("_MainTex2");

	private static readonly int int_3 = Shader.PropertyToID("_BumpMap0");

	private static readonly int int_4 = Shader.PropertyToID("_BumpMap1");

	private static readonly int int_5 = Shader.PropertyToID("_BumpMap2");

	private static readonly int int_6 = Shader.PropertyToID("_Color0");

	private static readonly int int_7 = Shader.PropertyToID("_Color1");

	private static readonly int int_8 = Shader.PropertyToID("_Color2");

	public Material Material;

	public Color Color0 = Color.white;

	public Texture MainTex0;

	public Texture BumpMap0;

	public Color Color1 = Color.white;

	public Texture MainTex1;

	public Texture BumpMap1;

	public Color Color2 = Color.white;

	public Texture MainTex2;

	public Texture BumpMap2;

	public static LoggerClass LoggerClass => Class443.Logger;

	public void Fix()
	{
		if (Material == null)
		{
			LoggerClass.LogError("Material is null", this);
			return;
		}
		if (MainTex0 != null)
		{
			Material.SetTexture(int_0, MainTex0);
		}
		else
		{
			LoggerClass.LogTrace("MainTex0 is null", this);
		}
		if (MainTex1 != null)
		{
			Material.SetTexture(int_1, MainTex1);
		}
		else
		{
			LoggerClass.LogTrace("MainTex1 is null", this);
		}
		if (MainTex2 != null)
		{
			Material.SetTexture(int_2, MainTex2);
		}
		else
		{
			LoggerClass.LogTrace("MainTex2 is null", this);
		}
		if (BumpMap0 != null)
		{
			Material.SetTexture(int_3, BumpMap0);
		}
		else
		{
			LoggerClass.LogTrace("BumpMap0 is null", this);
		}
		if (BumpMap1 != null)
		{
			Material.SetTexture(int_4, BumpMap1);
		}
		else
		{
			LoggerClass.LogTrace("BumpMap1 is null", this);
		}
		if (BumpMap2 != null)
		{
			Material.SetTexture(int_5, BumpMap2);
		}
		else
		{
			LoggerClass.LogTrace("BumpMap2 is null", this);
		}
		Material.SetColor(int_6, Color0);
		Material.SetColor(int_7, Color1);
		Material.SetColor(int_8, Color2);
		LoggerClass.LogTrace("name material fixed", this);
	}
}
