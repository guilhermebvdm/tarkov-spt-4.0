using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Seasons/SpringMaterialSpeedTree8")]
public class SpringMaterialSpeedTree8 : ScriptableObject, GInterface44, GInterface42
{
	private static readonly int _mainTex = Shader.PropertyToID("_MainTex");

	private static readonly int _normalMap = Shader.PropertyToID("_BumpMap");

	private static readonly int _hueVariationColor = Shader.PropertyToID("_HueVariationColor");

	private static readonly int _color = Shader.PropertyToID("_Color");

	public Material Material;

	public Texture MainTex;

	public Texture NormalMap;

	public Color HueVariation;

	public Color Color;

	public static LoggerClass Logger => Class443.Logger;

	public void Fix()
	{
		if (Material == null)
		{
			Logger.LogError("Material is null", this);
			return;
		}
		if (MainTex != null)
		{
			Material.SetTexture(_mainTex, MainTex);
		}
		else
		{
			Logger.LogWarn("Base texture is null", this);
		}
		if (NormalMap != null)
		{
			Material.SetTexture(_normalMap, NormalMap);
		}
		else
		{
			Logger.LogWarn("NormalMap texture is null", this);
		}
		Logger.LogTrace("SpringMaterialSpeedTree8 material fixed", this);
	}

	public void Unload()
	{
	}
}
