using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Seasons/WinterMaterialSpeedTreeEft")]
public class WinterMaterialSpeedTreeEft : ScriptableObject, GInterface46, GInterface42
{
	private static readonly int _base = Shader.PropertyToID("_MainTex");

	private static readonly int _normalMap = Shader.PropertyToID("_BumpMap");

	private static readonly int _detail = Shader.PropertyToID("_DetailTex");

	private static readonly int _hueVariation = Shader.PropertyToID("_HueVariation");

	private static readonly int _color = Shader.PropertyToID("_Color");

	public Material Material;

	public Texture Base;

	public Texture NormalMap;

	public Texture Detail;

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
		Material.SetColor(_hueVariation, HueVariation);
		Material.SetColor(_color, Color);
		Logger.LogTrace("SpeedTreeEFTMaterial material fixed", this);
	}
}
