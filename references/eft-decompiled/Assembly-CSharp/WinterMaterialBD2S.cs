using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Seasons/WinterMaterialBD2S")]
public class WinterMaterialBD2S : ScriptableObject, GInterface46, GInterface42
{
	private static readonly int _mainTex = Shader.PropertyToID("_MainTex");

	private static readonly int _bumpMap = Shader.PropertyToID("_BumpMap");

	private static readonly int _color = Shader.PropertyToID("_Color");

	public Material Material;

	public Texture MainTex;

	public Texture BumpMap;

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
			Logger.LogWarn("MainTex texture is null", this);
		}
		if (BumpMap != null)
		{
			Material.SetTexture(_bumpMap, BumpMap);
		}
		else
		{
			Logger.LogWarn("BumpMap texture is null", this);
		}
		Material.SetColor(_color, Color);
		Logger.LogTrace("WinterMaterialLeaves material fixed", this);
	}
}
