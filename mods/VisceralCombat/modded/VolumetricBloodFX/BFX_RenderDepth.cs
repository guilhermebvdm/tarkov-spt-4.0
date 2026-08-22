using UnityEngine;
using Object = UnityEngine.Object;

public class BFX_RenderDepth : MonoBehaviour
{
	private DepthTextureMode defaultMode;

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
		}
	}

	private void OnDisable()
	{
		Camera component = GetComponent<Camera>();
		if (component != null)
		{
			component.depthTextureMode = defaultMode;
		}
	}
}
