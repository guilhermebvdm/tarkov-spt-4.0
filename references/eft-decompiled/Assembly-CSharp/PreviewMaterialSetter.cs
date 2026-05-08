using System.Collections.Generic;
using UnityEngine;

public class PreviewMaterialSetter : MonoBehaviour
{
	[SerializeField]
	private Color _availableColor = new Color(0f, 1f, 0f, 0.2f);

	[SerializeField]
	private Color _availableReflectionColor = new Color(0.25f, 1f, 0.25f, 1f);

	[SerializeField]
	private Color _unvailableColor = new Color(1f, 0f, 0f, 0.2f);

	[SerializeField]
	private Color _unvailableReflectionColor = new Color(1f, 0.25f, 0.25f, 1f);

	private Dictionary<MeshRenderer, Material[]> dictionary_0;

	private Material material_0;

	private static readonly int int_0 = Shader.PropertyToID("_Color");

	private static readonly int int_1 = Shader.PropertyToID("_ReflectColor");

	public void Awake()
	{
		method_0();
		method_2();
		foreach (KeyValuePair<MeshRenderer, Material[]> item in dictionary_0)
		{
			method_3(item.Key, item.Value);
		}
	}

	public void OnDestroy()
	{
		foreach (KeyValuePair<MeshRenderer, Material[]> item in dictionary_0)
		{
			item.Key.sharedMaterials = item.Value;
		}
		dictionary_0.Clear();
	}

	public void SetAvailable(bool isAvailable)
	{
		Color value = (isAvailable ? _availableColor : _unvailableColor);
		Color value2 = (isAvailable ? _availableReflectionColor : _unvailableReflectionColor);
		material_0.SetColor(int_0, value);
		material_0.SetColor(int_1, value2);
	}

	public void method_0()
	{
		dictionary_0 = new Dictionary<MeshRenderer, Material[]>();
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			dictionary_0.Add(componentsInChildren[i], method_1(componentsInChildren[i]));
		}
	}

	public Material[] method_1(MeshRenderer renderer)
	{
		Material[] array = new Material[renderer.sharedMaterials.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = renderer.sharedMaterials[i];
		}
		return array;
	}

	public void method_2()
	{
		material_0 = new Material(GClass872.Find("p0/Transparent/Reflective/Specular"));
		material_0.SetColor(int_0, _availableColor);
	}

	public void method_3(MeshRenderer renderer, Material[] savedMaterials)
	{
		Material[] array = new Material[renderer.sharedMaterials.Length];
		for (int i = 0; i < savedMaterials.Length; i++)
		{
			array[i] = material_0;
		}
		renderer.sharedMaterials = array;
	}
}
