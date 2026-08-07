using UnityEngine;
using Object = UnityEngine.Object;

public class BFX_BloodDecalLayers : MonoBehaviour
{
	public enum DecalLayersProperty
	{
		DrawToSelectedLayers,
		IgnoreSelectedLayers
	}

	public enum DepthMode
	{
		FullScreen,
		HalfScreen,
		QuarterScreen
	}

	public LayerMask DecalLayers = 1;

	public DecalLayersProperty DecalRenderingMode = DecalLayersProperty.DrawToSelectedLayers;

	public DepthMode LayerDepthResoulution = DepthMode.FullScreen;

	private DepthTextureMode defaultMode;

	private RenderTexture rt;

	private Camera depthCamera;

	private void OnEnable()
	{
		Camera component = GetComponent<Camera>();
		if (component != null)
		{
			defaultMode = component.depthTextureMode;
			if ((int)component.renderingPath == 1)
			{
				component.depthTextureMode = component.depthTextureMode | DepthTextureMode.Depth;
			}
			GameObject val = new GameObject("DecalLayersCamera");
			val.transform.parent = component.transform;
			val.transform.localPosition = Vector3.zero;
			val.transform.localRotation = Quaternion.identity;
			depthCamera = val.AddComponent<Camera>();
			depthCamera.CopyFrom(component);
			depthCamera.renderingPath = (RenderingPath)1;
			depthCamera.depth = component.depth - 1f;
			depthCamera.cullingMask = DecalLayers;
			CreateDepthTexture();
			depthCamera.targetTexture = rt;
			Shader.SetGlobalTexture("_LayerDecalDepthTexture", rt);
			Shader.EnableKeyword("USE_CUSTOM_DECAL_LAYERS");
			if (DecalRenderingMode == DecalLayersProperty.IgnoreSelectedLayers)
			{
				Shader.EnableKeyword("USE_CUSTOM_DECAL_LAYERS_IGNORE_MODE");
			}
		}
	}

	private void OnDisable()
	{
		Camera component = GetComponent<Camera>();
		if (component != null)
		{
			component.depthTextureMode = defaultMode;
		}
		RenderTexture.ReleaseTemporary(rt);
		Shader.DisableKeyword("USE_CUSTOM_DECAL_LAYERS");
		if (DecalRenderingMode == DecalLayersProperty.IgnoreSelectedLayers)
		{
			Shader.DisableKeyword("USE_CUSTOM_DECAL_LAYERS_IGNORE_MODE");
		}
	}

	private void CreateDepthTexture()
	{
		switch (LayerDepthResoulution)
		{
		case DepthMode.FullScreen:
			rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, (RenderTextureFormat)1);
			break;
		case DepthMode.HalfScreen:
			rt = RenderTexture.GetTemporary((int)((float)Screen.width * 0.5f), (int)((float)Screen.height * 0.5f), 24, (RenderTextureFormat)1);
			break;
		case DepthMode.QuarterScreen:
			rt = RenderTexture.GetTemporary((int)((float)Screen.width * 0.25f), (int)((float)Screen.height * 0.25f), 24, (RenderTextureFormat)1);
			break;
		}
	}
}
