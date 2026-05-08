using UnityEngine;

public class PainScreen : MonoBehaviour
{
	[Range(0f, 1f)]
	[SerializeField]
	private float _value;

	[SerializeField]
	private Material _mat;

	private static readonly int int_0 = Shader.PropertyToID("_Value");

	public void Start()
	{
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		_mat.SetFloat(int_0, _value);
		Graphics.Blit(source, destination, _mat);
	}
}
