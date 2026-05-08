using System.Collections.Generic;
using UnityEngine;

public class CameraLightSwitcher : MonoBehaviour
{
	public List<Light> Lights;

	public virtual void OnPreCull()
	{
		TurnLights(on: true);
	}

	public virtual void OnPostRender()
	{
		TurnLights(on: false);
	}

	public virtual void OnEnable()
	{
		TurnLights(on: false);
	}

	public void TurnLights(bool on)
	{
		for (int i = 0; i < Lights.Count; i++)
		{
			Lights[i].enabled = on;
		}
	}
}
