using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Seasons/AM_Rock_Moss_Material")]
public class AM_Rock_Moss_Material : ScriptableObject, GInterface41, GInterface42, GInterface44, GInterface46
{
	private static readonly int _TopAlbedoASmoothness = Shader.PropertyToID("_TopAlbedoASmoothness");

	public Material Material;

	public Texture Texture;

	public static LoggerClass Logger => Class443.Logger;

	public void Fix()
	{
		if (Material == null)
		{
			Logger.LogError("Material is null", this);
			return;
		}
		if (Texture != null)
		{
			Material.SetTexture(_TopAlbedoASmoothness, Texture);
		}
		else
		{
			Logger.LogWarn("Texture texture is null", this);
		}
		Logger.LogTrace("AM_Rock_Moss_Material material fixed", this);
	}
}
