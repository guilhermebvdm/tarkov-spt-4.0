using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Seasons/SpringMaterialRavine02")]
public class SpringMaterialRavine02 : ScriptableObject, GInterface44, GInterface42
{
	private static readonly int _base = Shader.PropertyToID("_BaseAlbedoASmoothness");

	private static readonly int _gloss = Shader.PropertyToID("_SpecMap");

	private static readonly int _normal = Shader.PropertyToID("_BaseNormalMap");

	private static readonly int _top = Shader.PropertyToID("_TopAlbedoASmoothness");

	private static readonly int _topNormal = Shader.PropertyToID("_TopNormalMap");

	private static readonly int _topColor = Shader.PropertyToID("_TopColor");

	public Material Material;

	public Texture Base;

	public Texture Gloss;

	public Texture Normal;

	public Texture Top;

	public Texture TopNormal;

	public Color TopColor = Color.white;

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
		if (Gloss != null)
		{
			Material.SetTexture(_gloss, Gloss);
		}
		else
		{
			Logger.LogWarn("Gloss texture is null", this);
		}
		if (Normal != null)
		{
			Material.SetTexture(_normal, Normal);
		}
		else
		{
			Logger.LogWarn("Normal texture is null", this);
		}
		if (Top != null)
		{
			Material.SetTexture(_top, Top);
		}
		else
		{
			Logger.LogWarn("Top texture is null", this);
		}
		if (TopNormal != null)
		{
			Material.SetTexture(_topNormal, TopNormal);
		}
		else
		{
			Logger.LogWarn("TopNormal texture is null", this);
		}
		Material.SetColor(_topColor, TopColor);
	}
}
