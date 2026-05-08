using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Seasons/AutumnMaterialSpeedTreeEft")]
public class AutumnMaterialSpeedTreeEft : ScriptableObject, GInterface41, GInterface42
{
	private static readonly int _base = Shader.PropertyToID("_MainTex");

	private static readonly int _normalMap = Shader.PropertyToID("_BumpMap");

	private static readonly int _detail = Shader.PropertyToID("_DetailTex");

	private const string HUE_VARIATION_EFFECT = "EFFECT_HUE_VARIATION";

	private static readonly int _hueVariation = Shader.PropertyToID("_HueVariation");

	private static readonly int _color = Shader.PropertyToID("_Color");

	public Material Material;

	public Texture Base;

	public Texture NormalMap;

	public Texture Detail;

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
		if (Base != null)
		{
			Material.SetTexture(_base, Base);
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
		if (Detail != null)
		{
			Material.SetTexture(_detail, Detail);
		}
		else
		{
			Logger.LogWarn("Detail texture is null", this);
		}
		Material.SetColor(_color, Color);
		GClass935.UpdateHueVariation(Material, HueVariationEnabledState, "EFFECT_HUE_VARIATION", _hueVariation, HueVariation);
		Logger.LogTrace("AutumnMaterialSpeedTreeEft material fixed", this);
	}
}
