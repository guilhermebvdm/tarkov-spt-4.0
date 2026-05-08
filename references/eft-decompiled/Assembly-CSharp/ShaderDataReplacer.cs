using UnityEngine;

public class ShaderDataReplacer : MonoBehaviour
{
	[SerializeField]
	public string _shaderNameToReplaceData = "p0/Reflective/Bumped Specular SMap";

	[SerializeField]
	public Cubemap _cubemap;

	[SerializeField]
	public Color _reflectColor = new Color(1f, 1f, 1f, 0.5f);

	[SerializeField]
	[Range(0.01f, 10f)]
	public float _specularness = 5f / 64f;

	[SerializeField]
	[Range(0.01f, 10f)]
	public float _glossness = 1f;
}
