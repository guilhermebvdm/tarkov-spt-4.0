using UnityEngine;

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

	public LayerMask DecalLayers = LayerMask.op_Implicit(1);

	public DecalLayersProperty DecalRenderingMode = DecalLayersProperty.DrawToSelectedLayers;

	public DepthMode LayerDepthResoulution = DepthMode.FullScreen;

	private DepthTextureMode defaultMode;

	private RenderTexture rt;

	private Camera depthCamera;

	private void OnEnable()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Camera component = ((Component)this).GetComponent<Camera>();
		defaultMode = component.depthTextureMode;
		if ((int)component.renderingPath == 1)
		{
			component.depthTextureMode = (DepthTextureMode)(component.depthTextureMode | 1);
		}
		GameObject val = new GameObject("DecalLayersCamera");
		val.transform.parent = ((Component)component).transform;
		val.transform.localPosition = Vector3.zero;
		val.transform.localRotation = Quaternion.identity;
		depthCamera = val.AddComponent<Camera>();
		depthCamera.CopyFrom(component);
		depthCamera.renderingPath = (RenderingPath)1;
		depthCamera.depth = component.depth - 1f;
		depthCamera.cullingMask = LayerMask.op_Implicit(DecalLayers);
		CreateDepthTexture();
		depthCamera.targetTexture = rt;
		Shader.SetGlobalTexture("_LayerDecalDepthTexture", (Texture)(object)rt);
		Shader.EnableKeyword("USE_CUSTOM_DECAL_LAYERS");
		if (DecalRenderingMode == DecalLayersProperty.IgnoreSelectedLayers)
		{
			Shader.EnableKeyword("USE_CUSTOM_DECAL_LAYERS_IGNORE_MODE");
		}
	}

	private void OnDisable()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).GetComponent<Camera>().depthTextureMode = defaultMode;
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
