using System.Collections.Generic;
using UnityEngine;

public class LightSwitcherOverkill : CameraLightSwitcher
{
	private readonly List<Light> list_0 = new List<Light>();

	public override void OnPreCull()
	{
		list_0.Clear();
		for (int i = 0; i < SceneLights.AllLights.Count; i++)
		{
			Light light = SceneLights.AllLights[i];
			if (light != null && light.enabled)
			{
				list_0.Add(light);
				light.enabled = false;
			}
		}
		base.OnPreCull();
	}

	public override void OnPostRender()
	{
		foreach (Light item in list_0)
		{
			item.enabled = true;
		}
		base.OnPostRender();
	}
}
