using UnityEngine;

[AddComponentMenu("")]
[RequireComponent(typeof(Camera))]
public class AmplifyMotionPostProcess : MonoBehaviour
{
	private AmplifyMotionEffectBase amplifyMotionEffectBase_0;

	public AmplifyMotionEffectBase Instance
	{
		get
		{
			return amplifyMotionEffectBase_0;
		}
		set
		{
			amplifyMotionEffectBase_0 = value;
		}
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (amplifyMotionEffectBase_0 != null)
		{
			amplifyMotionEffectBase_0.PostProcess(source, destination);
		}
	}
}
