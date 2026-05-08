using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LightManager<T> : MonoBehaviour
{
	private static LightManager<T> lightManager_0;

	private HashSet<T> hashSet_0 = new HashSet<T>();

	public static LightManager<T> LightManager_0
	{
		get
		{
			if (lightManager_0 != null)
			{
				return lightManager_0;
			}
			lightManager_0 = (LightManager<T>)GClass870.FindUnityObjectOfType(typeof(LightManager<T>));
			return lightManager_0;
		}
	}

	public static HashSet<T> Get()
	{
		LightManager<T> lightManager = LightManager_0;
		if (!(lightManager == null))
		{
			return lightManager.hashSet_0;
		}
		return new HashSet<T>();
	}

	public static bool Add(T t)
	{
		LightManager<T> lightManager = LightManager_0;
		if (lightManager == null)
		{
			return false;
		}
		lightManager.hashSet_0.Add(t);
		return true;
	}

	public static void Remove(T t)
	{
		LightManager<T> lightManager = LightManager_0;
		if (!(lightManager == null))
		{
			lightManager.hashSet_0.Remove(t);
		}
	}
}
[ExecuteInEditMode]
public class LightManager : OnRenderObjectConnector
{
	private static readonly HashSet<GInterface51> hashSet_0 = new HashSet<GInterface51>();

	private readonly GClass1001 gclass1001_0 = new GClass1001("Deferred custom lights", CameraEvent.AfterLighting);

	public override void ManualOnRenderObject(Camera currentCamera)
	{
		base.ManualOnRenderObject(currentCamera);
		CommandBuffer commandBuffer = gclass1001_0.OnRenderObject();
		if (commandBuffer == null)
		{
			return;
		}
		commandBuffer.Clear();
		Plane[] frustrumPlanes = GClass823.CalculateFrustumPlanesNonAlloc(currentCamera);
		foreach (GInterface51 item in hashSet_0)
		{
			item.Draw(commandBuffer, frustrumPlanes, currentCamera);
		}
	}

	public static void Add(GInterface51 renderer)
	{
		hashSet_0.Add(renderer);
	}

	public static void Remove(GInterface51 renderer)
	{
		hashSet_0.Remove(renderer);
	}
}
