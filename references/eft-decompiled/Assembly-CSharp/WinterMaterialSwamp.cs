using UnityEngine;

[CreateAssetMenu(menuName = "Seasons/WinterMaterialSwamp")]
public class WinterMaterialSwamp : ScriptableObject, GInterface46, GInterface42
{
	private static readonly int _color0 = Shader.PropertyToID("_Color0");

	private static readonly int _mainTex0 = Shader.PropertyToID("_MainTex0");

	private static readonly int _bumpMap0 = Shader.PropertyToID("_BumpMap0");

	private static readonly int _color1 = Shader.PropertyToID("_Color1");

	private static readonly int _mainTex1 = Shader.PropertyToID("_MainTex1");

	private static readonly int _bumpMap1 = Shader.PropertyToID("_BumpMap1");

	private static readonly int _color2 = Shader.PropertyToID("_Color2");

	private static readonly int _mainTex2 = Shader.PropertyToID("_MainTex2");

	private static readonly int _bumpMap2 = Shader.PropertyToID("_BumpMap2");

	public Material Material;

	public Color Color0;

	public Texture Main0;

	public Texture BumpMap0;

	public Color Color1;

	public Texture Main1;

	public Texture BumpMap1;

	public Color Color2;

	public Texture Main2;

	public Texture BumpMap2;

	public static LoggerClass Logger => Class443.Logger;

	public void Fix()
	{
		if (Material == null)
		{
			Logger.LogError("Material is null", this);
			return;
		}
		Material.SetColor(_color0, Color0);
		if (Main0 != null)
		{
			Material.SetTexture(_mainTex0, Main0);
		}
		else
		{
			Logger.LogWarn("Main0 is null", this);
		}
		if (BumpMap0 != null)
		{
			Material.SetTexture(_bumpMap0, BumpMap0);
		}
		else
		{
			Logger.LogWarn("BumpMap0 is null", this);
		}
		Material.SetColor(_color1, Color1);
		if (Main1 != null)
		{
			Material.SetTexture(_mainTex1, Main1);
		}
		else
		{
			Logger.LogWarn("Main1 is null", this);
		}
		if (BumpMap1 != null)
		{
			Material.SetTexture(_bumpMap1, BumpMap1);
		}
		else
		{
			Logger.LogWarn("BumpMap1 is null", this);
		}
		Material.SetColor(_color2, Color2);
		if (Main2 != null)
		{
			Material.SetTexture(_mainTex2, Main2);
		}
		else
		{
			Logger.LogWarn("Main2 is null", this);
		}
		if (BumpMap2 != null)
		{
			Material.SetTexture(_bumpMap2, BumpMap2);
		}
		else
		{
			Logger.LogWarn("BumpMap2 is null", this);
		}
		Logger.LogTrace("Swamp material fixed", this);
	}
}
