using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class RainCondensator : MonoBehaviour
{
	private const float float_0 = 8f;

	private const float float_1 = 0.25f;

	private const string string_0 = "USERAIN";

	private const string string_1 = "SMap";

	private static readonly int int_0 = Shader.PropertyToID("_RippleTexScale");

	private static readonly int int_1 = Shader.PropertyToID("_SkinnedMeshMaterial");

	private static readonly int int_2 = Shader.PropertyToID("_LocalRain");

	private static readonly int int_3 = Shader.PropertyToID("_LocalRainAverage");

	private bool bool_0;

	private MaterialPropertyBlock materialPropertyBlock_0;

	private List<Material> list_0 = new List<Material>();

	private Renderer renderer_0;

	private Vector3 vector3_0;

	public void OnEnable()
	{
		if (!base.gameObject.name.Contains("Flare") && !base.gameObject.name.Contains("MuzzleJet"))
		{
			renderer_0 = GetComponent<Renderer>();
			bool_0 = renderer_0 is SkinnedMeshRenderer;
			materialPropertyBlock_0 = new MaterialPropertyBlock();
			Material[] materials = renderer_0.materials;
			foreach (Material material in materials)
			{
				if (material.shader.name.Contains("SMap"))
				{
					material.SetFloat(int_0, bool_0 ? 0.25f : 8f);
					material.SetFloat(int_1, Convert.ToInt32(bool_0));
					list_0.Add(material);
					material.EnableKeyword("USERAIN");
				}
			}
			RainController.smethod_0(this);
		}
		else
		{
			base.enabled = false;
		}
	}

	public void UpdateValues()
	{
		Vector3 vector = (RainController.IsCameraUnderRain ? RainController.FallingVectorV3 : (0.9f * vector3_0));
		vector3_0 = Vector3.Slerp(vector3_0, vector, 0.3f);
		Vector3 vector2 = -base.transform.InverseTransformDirection(vector);
		Vector3 vector3 = -base.transform.InverseTransformDirection(vector3_0);
		materialPropertyBlock_0.SetVector(int_2, vector2);
		materialPropertyBlock_0.SetVector(int_3, vector3);
		for (int i = 0; i < list_0.Count; i++)
		{
			renderer_0.SetPropertyBlock(materialPropertyBlock_0, i);
		}
	}

	public void OnDisable()
	{
		Material[] materials = renderer_0.materials;
		foreach (Material material in materials)
		{
			if (material.shader.name.Contains("SMap"))
			{
				material.DisableKeyword("USERAIN");
			}
		}
		list_0.Clear();
		RainController.smethod_1(this);
	}
}
