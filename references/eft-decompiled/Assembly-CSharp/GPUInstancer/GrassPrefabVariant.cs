using UnityEngine;

namespace GPUInstancer;

public class GrassPrefabVariant : MonoBehaviour
{
	[SerializeField]
	public int ArrayIndex;

	[SerializeField]
	private Color DryColor = new Color(0.47f, 0.5f, 0.3f, 1f);

	[SerializeField]
	private Color HealthyColor = Color.white;

	private static readonly string string_0 = "_ArrayIndex";

	private static readonly string string_1 = "_DryColor";

	private static readonly string string_2 = "_HealthyColor";

	public void OnValidate()
	{
		if (!Application.isPlaying)
		{
			method_0();
		}
	}

	public void method_0()
	{
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		materialPropertyBlock.SetInt(string_0, ArrayIndex);
		materialPropertyBlock.SetColor(string_1, DryColor);
		materialPropertyBlock.SetColor(string_2, HealthyColor);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SetPropertyBlock(materialPropertyBlock);
		}
	}
}
