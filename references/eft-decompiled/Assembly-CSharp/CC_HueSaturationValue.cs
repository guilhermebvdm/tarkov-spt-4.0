using EFT.BlitDebug;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Hue, Saturation, Value")]
public class CC_HueSaturationValue : CC_Base
{
	[Range(-180f, 180f)]
	public float masterHue;

	[Range(-100f, 100f)]
	public float masterSaturation;

	[Range(-100f, 100f)]
	public float masterValue;

	[Range(-180f, 180f)]
	public float redsHue;

	[Range(-100f, 100f)]
	public float redsSaturation;

	[Range(-100f, 100f)]
	public float redsValue;

	[Range(-180f, 180f)]
	public float yellowsHue;

	[Range(-100f, 100f)]
	public float yellowsSaturation;

	[Range(-100f, 100f)]
	public float yellowsValue;

	[Range(-180f, 180f)]
	public float greensHue;

	[Range(-100f, 100f)]
	public float greensSaturation;

	[Range(-100f, 100f)]
	public float greensValue;

	[Range(-180f, 180f)]
	public float cyansHue;

	[Range(-100f, 100f)]
	public float cyansSaturation;

	[Range(-100f, 100f)]
	public float cyansValue;

	[Range(-180f, 180f)]
	public float bluesHue;

	[Range(-100f, 100f)]
	public float bluesSaturation;

	[Range(-100f, 100f)]
	public float bluesValue;

	[Range(-180f, 180f)]
	public float magentasHue;

	[Range(-100f, 100f)]
	public float magentasSaturation;

	[Range(-100f, 100f)]
	public float magentasValue;

	public bool advanced;

	public int currentChannel;

	private static readonly int int_0 = Shader.PropertyToID("_Master");

	private static readonly int int_1 = Shader.PropertyToID("_Reds");

	private static readonly int int_2 = Shader.PropertyToID("_Yellows");

	private static readonly int int_3 = Shader.PropertyToID("_Greens");

	private static readonly int int_4 = Shader.PropertyToID("_Cyans");

	private static readonly int int_5 = Shader.PropertyToID("_Blues");

	private static readonly int int_6 = Shader.PropertyToID("_Magentas");

	public float hue
	{
		get
		{
			return masterHue;
		}
		set
		{
			masterHue = value;
		}
	}

	public float saturation
	{
		get
		{
			return masterSaturation;
		}
		set
		{
			masterSaturation = value;
		}
	}

	public float value
	{
		get
		{
			return masterValue;
		}
		set
		{
			masterValue = value;
		}
	}

	[ImageEffectTransformsToLDR]
	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetVector(int_0, new Vector3(masterHue / 360f, masterSaturation * 0.01f, masterValue * 0.01f));
		if (advanced)
		{
			base.material.SetVector(int_1, new Vector3(redsHue / 360f, redsSaturation * 0.01f, redsValue * 0.01f));
			base.material.SetVector(int_2, new Vector3(yellowsHue / 360f, yellowsSaturation * 0.01f, yellowsValue * 0.01f));
			base.material.SetVector(int_3, new Vector3(greensHue / 360f, greensSaturation * 0.01f, greensValue * 0.01f));
			base.material.SetVector(int_4, new Vector3(cyansHue / 360f, cyansSaturation * 0.01f, cyansValue * 0.01f));
			base.material.SetVector(int_5, new Vector3(bluesHue / 360f, bluesSaturation * 0.01f, bluesValue * 0.01f));
			base.material.SetVector(int_6, new Vector3(magentasHue / 360f, magentasSaturation * 0.01f, magentasValue * 0.01f));
			DebugGraphics.Blit(source, destination, base.material, 1);
		}
		else
		{
			DebugGraphics.Blit(source, destination, base.material, 0);
		}
	}
}
