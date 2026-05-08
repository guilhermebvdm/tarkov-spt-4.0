using UnityEngine;
using UnityStandardAssets.Water;

[ExecuteInEditMode]
[RequireComponent(typeof(WaterBase))]
public class WaterController : MonoBehaviour
{
	public Color WaterColor;

	public Color SpecularColor;

	[Range(0f, 500f)]
	public float Shininess;

	private WaterBase waterBase_0;

	private static readonly int int_0 = Shader.PropertyToID("_BaseColor");

	private static readonly int int_1 = Shader.PropertyToID("_WorldLightDir");

	private static readonly int int_2 = Shader.PropertyToID("_SpecularColor");

	private static readonly int int_3 = Shader.PropertyToID("_Shininess");

	public void Update()
	{
		if (!waterBase_0)
		{
			waterBase_0 = (WaterBase)base.gameObject.GetComponent(typeof(WaterBase));
		}
		if (waterBase_0 == null)
		{
			return;
		}
		TOD_Sky instance = MonoBehaviourSingleton<TOD_Sky>.Instance;
		if (!(instance == null))
		{
			Vector3 sunDirection = instance.SunDirection;
			Color sunLightColor = instance.SunLightColor;
			float num = Mathf.Clamp01(sunDirection.y * 4f);
			Material sharedMaterial = waterBase_0.sharedMaterial;
			if (!(sharedMaterial == null))
			{
				Color value = sunLightColor * WaterColor * num;
				value.a = WaterColor.a;
				sharedMaterial.SetColor(int_0, value);
				sharedMaterial.SetVector(int_1, -sunDirection);
				sharedMaterial.SetColor(int_2, SpecularColor * sunLightColor);
				sharedMaterial.SetFloat(int_3, Shininess);
			}
		}
	}
}
