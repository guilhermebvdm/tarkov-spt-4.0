using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
[GAttribute8(19000)]
public class WaterRenderer : MonoBehaviour
{
	private readonly GClass1001 gclass1001_0 = new GClass1001("Draw Water", CameraEvent.BeforeReflections);

	private List<WaterForSSR> list_0 = new List<WaterForSSR>();

	public void OnEnable()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(method_2));
		WaterForSSR.OnAdd += method_1;
		WaterForSSR.OnRemove += method_3;
		method_0();
	}

	public void OnDisable()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(method_2));
		WaterForSSR.OnAdd -= method_1;
		WaterForSSR.OnRemove -= method_3;
	}

	public void method_0()
	{
		WaterForSSR[] array = GClass870.FindUnityObjectsOfType<WaterForSSR>();
		foreach (WaterForSSR waterForSSR in array)
		{
			if (waterForSSR.enabled && waterForSSR.gameObject.activeSelf)
			{
				method_1(waterForSSR);
			}
		}
	}

	public void method_1(WaterForSSR water)
	{
		if (!list_0.Contains(water))
		{
			list_0.Add(water);
		}
		foreach (KeyValuePair<Camera, CommandBuffer> camera in gclass1001_0.Cameras)
		{
			if (camera.Key != null && water.IsCorrectLayer(camera.Key.cullingMask))
			{
				water.InitBuffer(camera.Value, camera.Key);
			}
		}
	}

	public void method_2(Camera currentCamera)
	{
		if ((!currentCamera.CompareTag("MainCamera") && !currentCamera.CompareTag("OpticCamera") && Application.isPlaying) || !gclass1001_0.UpdateOnPreCullRender(out var buffer))
		{
			return;
		}
		buffer.Clear();
		for (int i = 0; i < list_0.Count; i++)
		{
			if (list_0[i].IsCorrectLayer(currentCamera.cullingMask))
			{
				list_0[i].InitBuffer(buffer, currentCamera);
			}
		}
	}

	public void method_3(WaterForSSR water)
	{
		if (list_0.Contains(water))
		{
			list_0.Remove(water);
		}
	}
}
