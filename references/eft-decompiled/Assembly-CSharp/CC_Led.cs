using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/LED")]
public class CC_Led : CC_Base
{
	[Range(1f, 255f)]
	public float scale = 80f;

	[Range(0f, 10f)]
	public float brightness = 1f;

	public bool automaticRatio;

	public float ratio = 1f;

	public int mode;

	protected Camera m_Camera;

	private static readonly int int_0 = Shader.PropertyToID("_Scale");

	private static readonly int int_1 = Shader.PropertyToID("_Ratio");

	private static readonly int int_2 = Shader.PropertyToID("_Brightness");

	public override void Start()
	{
		base.Start();
		m_Camera = GetComponent<Camera>();
	}

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		switch (mode)
		{
		default:
			base.material.SetFloat(int_0, (float)m_Camera.pixelWidth / scale);
			break;
		case 0:
			base.material.SetFloat(int_0, scale);
			break;
		}
		base.material.SetFloat(int_1, automaticRatio ? ((float)m_Camera.pixelWidth / (float)m_Camera.pixelHeight) : ratio);
		base.material.SetFloat(int_2, brightness);
		Graphics.Blit(source, destination, base.material);
	}
}
