using UnityEngine;

public class CreateDepthTexture : MonoBehaviour
{
	private Material material_0;

	public RenderTexture _rt;

	private static readonly int int_0 = Shader.PropertyToID("_CameraDepthTextureDeferred");

	public void Start()
	{
		material_0 = new Material(GClass872.Find("Hidden/ShowDepth"));
		_rt = new RenderTexture(1024, 1024, 0)
		{
			name = "CreateDepthTexture _rt",
			wrapMode = TextureWrapMode.Clamp,
			filterMode = FilterMode.Point,
			format = RenderTextureFormat.RHalf
		};
	}

	public void OnRenderImage(RenderTexture s, RenderTexture d)
	{
		Graphics.Blit(s, _rt, material_0);
		Shader.SetGlobalTexture(int_0, _rt);
		Graphics.CopyTexture(s, d);
	}
}
