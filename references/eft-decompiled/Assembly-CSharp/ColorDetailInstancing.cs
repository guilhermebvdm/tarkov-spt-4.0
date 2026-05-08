using System;
using UnityEngine;

public class ColorDetailInstancing : MonoBehaviour
{
	[Serializable]
	public struct MaterialLink(MeshRenderer meshRenderer, int materialIndex)
	{
		public MeshRenderer MeshRenderer = meshRenderer;

		public int MaterialIndex = materialIndex;
	}

	[Serializable]
	public struct MaterialControlls
	{
		public Material material;

		public Color Color;

		public int DetailTexIndex;

		public float DetailScale;

		[HideInInspector]
		public MaterialLink[] Links;
	}

	private static Shader shader_0;

	private static readonly int int_0 = Shader.PropertyToID("_DetailColor");

	private static readonly int int_1 = Shader.PropertyToID("_DetailValues");

	private static readonly int int_2 = Shader.PropertyToID("_DetailArray");

	private static MaterialPropertyBlock materialPropertyBlock_0;

	[SerializeField]
	private MaterialControlls[] _materials;

	public static void smethod_0(MaterialControlls[] controlls)
	{
		if (materialPropertyBlock_0 == null)
		{
			materialPropertyBlock_0 = new MaterialPropertyBlock();
		}
		for (int i = 0; i < controlls.Length; i++)
		{
			materialPropertyBlock_0.SetColor(int_0, controlls[i].Color);
			materialPropertyBlock_0.SetVector(int_1, new Vector4(controlls[i].DetailTexIndex, controlls[i].DetailScale));
			for (int j = 0; j < controlls[i].Links.Length; j++)
			{
				controlls[i].Links[j].MeshRenderer.SetPropertyBlock(materialPropertyBlock_0, controlls[i].Links[j].MaterialIndex);
			}
		}
	}

	public void Awake()
	{
		smethod_0(_materials);
	}
}
