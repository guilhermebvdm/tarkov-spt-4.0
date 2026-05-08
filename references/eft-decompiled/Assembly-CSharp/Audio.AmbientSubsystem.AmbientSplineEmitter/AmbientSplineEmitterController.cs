using System;
using UnityEngine;

namespace Audio.AmbientSubsystem.AmbientSplineEmitter;

public class AmbientSplineEmitterController : MonoBehaviour
{
	private GInterface109[] ginterface109_0 = Array.Empty<GInterface109>();

	private bool bool_0;

	public void Tick()
	{
		if (bool_0)
		{
			GInterface109[] array = ginterface109_0;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Tick();
			}
		}
	}

	public void LateTick()
	{
		if (bool_0)
		{
			GInterface109[] array = ginterface109_0;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].LateTick(Time.deltaTime);
			}
		}
	}

	public void Init(Transform target)
	{
		ginterface109_0 = GetComponentsInChildren<GInterface109>();
		GInterface109[] array = ginterface109_0;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Init(target);
		}
		bool_0 = true;
	}
}
