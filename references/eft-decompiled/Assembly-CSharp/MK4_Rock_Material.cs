using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Seasons/MK4_Rock_Material")]
public class MK4_Rock_Material : ScriptableObject, GInterface41, GInterface42, GInterface44, GInterface46
{
	private static readonly int _MaskRGBA = Shader.PropertyToID("_MaskRGBA");

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
			Material.SetTexture(_MaskRGBA, Texture);
		}
		else
		{
			Logger.LogWarn("Texture texture is null", this);
		}
		Logger.LogTrace("MK4_Rock_Material material fixed", this);
	}
}
