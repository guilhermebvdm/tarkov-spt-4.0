using EFT.BlitDebug;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Cross Stitch")]
public class CC_CrossStitch : CC_Base
{
	[Range(1f, 128f)]
	public int size = 8;

	public float brightness = 1.5f;

	public bool invert;

	public bool pixelize = true;

	protected Camera m_Camera;

	protected SSAA m_SSAA;

	private static readonly int int_0 = Shader.PropertyToID("_StitchSize");

	private static readonly int int_1 = Shader.PropertyToID("_Brightness");

	private static readonly int int_2 = Shader.PropertyToID("_Scale");

	private static readonly int int_3 = Shader.PropertyToID("_Ratio");

	public override void Start()
	{
		base.Start();
		m_Camera = GetComponent<Camera>();
		m_SSAA = GetComponent<SSAA>();
	}

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetFloat(int_0, size);
		base.material.SetFloat(int_1, brightness);
		int num = (invert ? 1 : 0);
		if (pixelize)
		{
			int num2 = (m_SSAA ? m_SSAA.GetInputWidth() : m_Camera.pixelWidth);
			int num3 = (m_SSAA ? m_SSAA.GetInputHeight() : m_Camera.pixelHeight);
			num += 2;
			base.material.SetFloat(int_2, num2 / size);
			base.material.SetFloat(int_3, num2 / num3);
		}
		DebugGraphics.Blit(source, destination, base.material, num);
	}
}
