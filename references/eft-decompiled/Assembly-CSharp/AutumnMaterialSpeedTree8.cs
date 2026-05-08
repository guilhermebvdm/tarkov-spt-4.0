using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Seasons/AutumnMaterialSpeedTree8")]
public class AutumnMaterialSpeedTree8 : ScriptableObject, GInterface41, GInterface42
{
	private static readonly int _mainTex = Shader.PropertyToID("_MainTex");

	private static readonly int _normalMap = Shader.PropertyToID("_BumpMap");

	private static readonly int _color = Shader.PropertyToID("_Color");

	private const string HUE_VARIATION_EFFECT = "EFFECT_HUE_VARIATION";

	private static readonly int _hueVariationColor = Shader.PropertyToID("_HueVariationColor");

	public Material Material;

	public Texture MainTex;

	public Texture NormalMap;

	public Color Color;

	public EHueVariationEnabledState HueVariationEnabledState;

	public Color HueVariation;

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
		Material.SetColor(_color, Color);
		GClass935.UpdateHueVariation(Material, HueVariationEnabledState, "EFFECT_HUE_VARIATION", _hueVariationColor, HueVariation);
		Logger.LogTrace("AutumnMaterialSpeedTree8 material fixed", this);
	}

	public void Unload()
	{
	}
}
