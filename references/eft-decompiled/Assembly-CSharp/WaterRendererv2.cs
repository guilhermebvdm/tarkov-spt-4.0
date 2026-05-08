using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
[GAttribute8(19000)]
public class WaterRendererv2 : MonoBehaviour
{
	private readonly GClass1001 gclass1001_0 = new GClass1001("Draw Water v2", CameraEvent.BeforeReflections);

	private List<WaterForSSRv2> list_0 = new List<WaterForSSRv2>();

	public void OnEnable()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(method_2));
		WaterForSSRv2.OnAdd += method_1;
		WaterForSSRv2.OnRemove += method_3;
		method_0();
	}

	public void OnDisable()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(method_2));
		WaterForSSRv2.OnAdd -= method_1;
		WaterForSSRv2.OnRemove -= method_3;
	}

	public void method_0()
	{
		WaterForSSRv2[] array = GClass870.FindUnityObjectsOfType<WaterForSSRv2>();
		foreach (WaterForSSRv2 waterForSSRv in array)
		{
			if (waterForSSRv.enabled && waterForSSRv.gameObject.activeSelf)
			{
				method_1(waterForSSRv);
			}
		}
	}

	public void method_1(WaterForSSRv2 water)
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

	public void method_3(WaterForSSRv2 water)
	{
		if (list_0.Contains(water))
		{
			list_0.Remove(water);
		}
	}
}
