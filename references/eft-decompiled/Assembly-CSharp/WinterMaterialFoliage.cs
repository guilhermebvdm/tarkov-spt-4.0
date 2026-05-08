using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Seasons/WinterMaterialFoliage")]
public class WinterMaterialFoliage : ScriptableObject, GInterface46, GInterface42
{
	private static readonly int _mainTexArray = Shader.PropertyToID("_MainTexArray");

	private static readonly int _normalMapArray = Shader.PropertyToID("_NormalMapArray");

	private static readonly int _dryColor = Shader.PropertyToID("_DryColor");

	private static readonly int _healthyColor = Shader.PropertyToID("_HealthyColor");

	private static readonly int _windWaveTintColor = Shader.PropertyToID("_WindWaveTintColor");

	public Material Material;

	public Texture2DArray MainTexArray;

	public Texture2DArray NormalMapArray;

	public Color DryColor;

	public Color HealthyColor;

	public Color WindWaveTintColor;

	public static LoggerClass Logger => Class443.Logger;

	public void Fix()
	{
		if (Material == null)
		{
			Logger.LogError("Material is null", this);
			return;
		}
		if (MainTexArray != null)
		{
			Material.SetTexture(_mainTexArray, MainTexArray);
		}
		else
		{
			Logger.LogWarn("MainTexArray texture is null", this);
		}
		if (NormalMapArray != null)
		{
			Material.SetTexture(_normalMapArray, NormalMapArray);
		}
		else
		{
			Logger.LogWarn("NormalMapArray texture is null", this);
		}
		Material.SetColor(_dryColor, DryColor);
		Material.SetColor(_healthyColor, HealthyColor);
		Material.SetColor(_windWaveTintColor, WindWaveTintColor);
		Logger.LogTrace("WinterMaterialFoliage material fixed", this);
	}
}
