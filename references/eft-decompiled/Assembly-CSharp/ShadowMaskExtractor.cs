using System;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Light))]
public class ShadowMaskExtractor : MonoBehaviour
{
	private CommandBuffer commandBuffer_0;

	private readonly int int_0 = Shader.PropertyToID("_SnowShadowMask");

	private readonly int int_1 = Shader.PropertyToID("_GlobalLightDir");

	private readonly int int_2 = Shader.PropertyToID("_GlobalLightIntensity");

	private Light light_0;

	public void Awake()
	{
		light_0 = GetComponent<Light>();
		commandBuffer_0 = new CommandBuffer
		{
			name = "ShadowMaskExtractor"
		};
		light_0.AddCommandBuffer(LightEvent.AfterScreenspaceMask, commandBuffer_0);
	}

	public void OnDestroy()
	{
		light_0.RemoveCommandBuffer(LightEvent.AfterScreenspaceMask, commandBuffer_0);
	}

	public void OnEnable()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(method_0));
	}

	public void OnDisable()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(method_0));
	}

	public void Update()
	{
		Shader.SetGlobalVector(int_1, light_0.transform.forward);
		Shader.SetGlobalFloat(int_2, light_0.intensity);
	}

	public void method_0(Camera currentCamera)
	{
		if (smethod_0(currentCamera))
		{
			method_1(currentCamera);
		}
	}

	public static bool smethod_0(Camera currentCamera)
	{
		CameraClass instance = CameraClass.Instance;
		if (!(currentCamera == instance.Camera))
		{
			return currentCamera == instance.OpticCameraManager.Camera;
		}
		return true;
	}

	public void method_1(Camera currentCamera)
	{
		Struct160 @struct = Class679.Class679_0.method_0(currentCamera, this);
		commandBuffer_0.Clear();
		commandBuffer_0.Blit(BuiltinRenderTextureType.CurrentActive, @struct.RenderTexture_0);
		commandBuffer_0.SetGlobalTexture(int_0, @struct.RenderTexture_0);
	}
}
