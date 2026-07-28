using UnityEngine;

public class BFX_RenderDepth : MonoBehaviour
{
	private DepthTextureMode defaultMode;

	private void OnEnable()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Camera component = ((Component)this).GetComponent<Camera>();
		defaultMode = component.depthTextureMode;
		if ((int)component.renderingPath == 1)
		{
			component.depthTextureMode = (DepthTextureMode)(component.depthTextureMode | 1);
		}
	}

	private void OnDisable()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).GetComponent<Camera>().depthTextureMode = defaultMode;
	}
}
