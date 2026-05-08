using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Seasons/SpringMaterialRavine01")]
public class SpringMaterialRavine01 : ScriptableObject, GInterface44, GInterface42
{
	private static readonly int _base = Shader.PropertyToID("_MainTex");

	private static readonly int _gloss = Shader.PropertyToID("_SpecMap");

	private static readonly int _normal = Shader.PropertyToID("_BumpMap");

	public Material Material;

	public Texture Base;

	public Texture Gloss;

	public Texture Normal;

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
			return;
		}
		Logger.LogWarn("Normal texture is null", this);
	}
}
