using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace EFT.Rendering.Clouds;

public class CloudController : MonoBehaviour
{
	private GClass2383 gclass2383_0;

	private Class1821 class1821_0;

	private GClass1001 gclass1001_0;

	private GClass1001 gclass1001_1;

	private GClass2381 gclass2381_0;

	[SerializeField]
	private Gradient _ambientColor = new Gradient
	{
		colorKeys = new GradientColorKey[2]
		{
			new GradientColorKey(Color.black, 0f),
			new GradientColorKey(Color.black, 1f)
		}
	};

	[SerializeField]
	private Light _sun;

	[SerializeField]
	private CloudLayer _settings;

	[CompilerGenerated]
	private bool bool_0;

	public CloudLayer Settings
	{
		get
		{
			return _settings;
		}
		set
		{
			_settings = value;
		}
	}

	public bool IsInitialized
	{
		[CompilerGenerated]
		get
		{
			return bool_0;
		}
		[CompilerGenerated]
		set
		{
			bool_0 = value;
		}
	}

	public void OnEnable()
	{
		if (IsInitialized)
		{
			OnDisable();
		}
		class1821_0 = new Class1821();
		gclass2383_0 = new GClass2383();
		gclass2381_0 = new GClass2381();
		gclass1001_1 = new GClass1001("Cloud Main Buffer", CameraEvent.AfterForwardOpaque);
		gclass1001_0 = new GClass1001("Cloud Shadow Buffer", CameraEvent.BeforeGBuffer);
		class1821_0.Build();
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(OnPreRenderCamera));
		IsInitialized = true;
	}

	public void OnDisable()
	{
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(OnPreRenderCamera));
		class1821_0?.Cleanup();
		class1821_0 = null;
		gclass2381_0?.Dispose();
		gclass2381_0 = null;
		gclass1001_1?.DestroyBuffers();
		gclass1001_1 = null;
		gclass1001_0?.DestroyBuffers();
		gclass1001_0 = null;
		IsInitialized = false;
	}

	public void OnPreRenderCamera(Camera cam)
	{
		AmbientLight component;
		if (!Application.isPlaying && !base.gameObject.activeInHierarchy)
		{
			OnDisable();
		}
		else if (IsInitialized && !(_settings == null) && (cam.CompareTag("MainCamera") || cam.CompareTag("OpticCamera") || cam.TryGetComponent<AmbientLight>(out component)))
		{
			gclass1001_0.UpdateOnRenderObject(out var buffer);
			gclass1001_1.UpdateOnRenderObject(out var buffer2);
			buffer.Clear();
			buffer2.Clear();
			method_1(gclass2383_0, cam, buffer2);
			if (!gclass2383_0.IsAnimate)
			{
				_settings.LayerA.ScrollFactor = 0f;
			}
			if (class1821_0.vmethod_0(gclass2383_0))
			{
				class1821_0.UpdateTexture(gclass2383_0);
			}
			method_0(buffer);
			class1821_0.RenderClouds(gclass2383_0, renderForCubemap: false);
		}
	}

	public void UpdateAmbient(SphericalHarmonicsL2 sh)
	{
		float time = MonoBehaviourSingleton<TOD_Sky>.Instance.SunDirection.y * 0.5f + 0.5f;
		Color color = _ambientColor.Evaluate(time);
		sh.AddAmbientLight(color);
		gclass2381_0.SetData(sh);
	}

	public void method_0(CommandBuffer shadowBuffer)
	{
		if (!(gclass2383_0.SunLight == null))
		{
			if (class1821_0.GetSunLightCookieParameters(_settings, gclass2383_0, out var cookieParams))
			{
				GStruct291 builtinParams = new GStruct291
				{
					SunLight = _sun,
					CommandBuffer = shadowBuffer,
					CloudSettings = _settings,
					FrameIndex = gclass2383_0.FrameIndex
				};
				class1821_0.RenderSunLightCookie(builtinParams);
			}
			_sun.cookie = cookieParams.Texture;
			float num = Mathf.Max(cookieParams.Size.x, cookieParams.Size.y);
			if (!Mathf.Approximately(_sun.cookieSize, num))
			{
				_sun.cookieSize = num;
			}
		}
	}

	public void method_1(GClass2383 parameters, Camera cam, CommandBuffer buffer)
	{
		int pixelWidth = cam.pixelWidth;
		int pixelHeight = cam.pixelHeight;
		Matrix4x4 projectionMatrix = cam.projectionMatrix;
		Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
		Matrix4x4 matrix = GL.GetGPUProjectionMatrix(projectionMatrix, renderIntoTexture: true);
		Matrix4x4 view = worldToCameraMatrix;
		float aspect = GClass2385.ProjectionMatrixAspect(in matrix);
		Vector4 vector = new Vector4(pixelWidth, pixelHeight, 1f / (float)pixelWidth, 1f / (float)pixelHeight);
		Matrix4x4 pixelCoordToViewDirMatrix = GClass2385.ComputePixelCoordToWorldSpaceViewDirectionMatrix(matrix, view, cam, vector, aspect);
		parameters.Time = Time.time;
		parameters.IsAnimate = true;
		parameters.PixelCoordToViewDirMatrix = pixelCoordToViewDirMatrix;
		parameters.WorldSpaceCameraPos = cam.transform.position;
		parameters.ViewMatrix = worldToCameraMatrix;
		parameters.ScreenSize = vector;
		parameters.SunLight = _sun;
		parameters.FrameIndex = Time.frameCount;
		parameters.CloudSettings = _settings;
		parameters.CubemapFace = CubemapFace.Unknown;
		parameters.CloudOpacity = default(RenderTargetIdentifier);
		parameters.ExposureMultiplier = 1f;
		parameters.CloudAmbientProbe = gclass2381_0;
		parameters.CommandBuffer = buffer;
		parameters.ColorBuffer = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
	}
}
