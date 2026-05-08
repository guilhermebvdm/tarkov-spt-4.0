using System;
using UnityEngine;

public class GClass1026 : GClass1025
{
	[NonSerialized]
	public SSAA Ssaa_0;

	public GClass1026(Camera camera)
		: base(camera)
	{
		Ssaa_0 = camera.GetComponent<SSAA>();
	}

	public override void StartRendering()
	{
		base.StartRendering();
		if ((bool)Ssaa_0)
		{
			base.CommandBuffer_0.SetRenderTarget(Ssaa_0.GetRTIdentifier());
		}
	}
}
