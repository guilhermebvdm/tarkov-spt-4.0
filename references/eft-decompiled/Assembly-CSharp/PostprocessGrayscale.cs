using UnityEngine;

public class PostprocessGrayscale : Posteffect
{
	private SSAAPropagator ssaapropagator_0;

	public void Awake()
	{
		ssaapropagator_0 = GetComponent<SSAAPropagator>();
	}

	public override void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.GetSourceDestination(out source, out destination);
		}
		base.OnRenderImage(source, destination);
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.ReleaseSourceDestination(source, destination);
		}
	}
}
