using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SceneLights : MonoBehaviour
{
	public static List<Light> AllLights = new List<Light>();

	[SerializeField]
	private Light[] _lights;

	public void Awake()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		Light[] lights = _lights;
		foreach (Light light in lights)
		{
			if (light != null)
			{
				AllLights.Add(light);
			}
		}
	}

	public void RecacheLights()
	{
		_lights = (from x in GClass870.FindUnityObjectsOfType<Light>()
			where x.gameObject.scene == base.gameObject.scene
			select x).ToArray();
	}

	[CompilerGenerated]
	public bool method_0(Light x)
	{
		return x.gameObject.scene == base.gameObject.scene;
	}
}
