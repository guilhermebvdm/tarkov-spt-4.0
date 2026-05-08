using UnityEngine;

public class AbstractMaterialBumpedSpecularSMap : ScriptableObject
{
	private static readonly int int_0 = Shader.PropertyToID("_MainTex");

	private static readonly int int_1 = Shader.PropertyToID("_BumpMap");

	private static readonly int int_2 = Shader.PropertyToID("_Color");

	private static readonly int int_3 = Shader.PropertyToID("_BaseTintColor");

	public Material Material;

	public Texture MainTex;

	public Texture BumpMap;

	public Color Color = Color.white;

	public Color BaseTintColor = Color.white;

	public static LoggerClass LoggerClass => Class443.Logger;

	public void Fix()
	{
		if (Material == null)
		{
			LoggerClass.LogError("Material is null", this);
			return;
		}
		if (MainTex != null)
		{
			Material.SetTexture(int_0, MainTex);
		}
		else
		{
			LoggerClass.LogWarn("MainTex texture is null", this);
		}
		if (BumpMap != null)
		{
			Material.SetTexture(int_1, BumpMap);
		}
		else
		{
			LoggerClass.LogWarn("BumpMap texture is null", this);
		}
		Material.SetColor(int_2, Color);
		Material.SetColor(int_3, BaseTintColor);
		LoggerClass.LogTrace("name material fixed", this);
	}
}
