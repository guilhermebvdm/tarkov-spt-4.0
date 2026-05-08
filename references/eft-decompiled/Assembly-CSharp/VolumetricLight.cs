using System;
using System.Runtime.CompilerServices;
using Comfort.Common;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Light))]
[ExecuteInEditMode]
public class VolumetricLight : MonoBehaviour
{
	[CompilerGenerated]
	private bool bool_0;

	private Light light_0;

	private Shader shader_0;

	private Material material_0;

	private CommandBuffer commandBuffer_0;

	private CommandBuffer commandBuffer_1;

	[Range(1f, 64f)]
	public int SampleCount = 8;

	[Range(0f, 1f)]
	public float ScatteringCoef = 0.5f;

	[Range(0f, 0.1f)]
	public float ExtinctionCoef = 0.01f;

	[Range(0f, 1f)]
	public float SkyboxExtinctionCoef = 0.9f;

	[Range(0f, 0.999f)]
	public float MieG = 0.1f;

	public bool HeightFog;

	[Range(0f, 0.5f)]
	public float HeightScale = 0.1f;

	public float GroundLevel;

	public bool Noise;

	public float NoiseScale = 0.015f;

	public float NoiseIntensity = 1f;

	public float NoiseIntensityOffset = 0.3f;

	public Vector2 NoiseVelocity = new Vector2(3f, 3f);

	[Tooltip("")]
	public float MaxRayLength = 400f;

	private Vector4[] vector4_0 = new Vector4[4];

	private bool bool_1;

	private bool bool_2;

	private Vector3 vector3_0;

	private Quaternion quaternion_0;

	private float float_0;

	private float float_1;

	private Matrix4x4 matrix4x4_0;

	private Matrix4x4 matrix4x4_1;

	private Matrix4x4 matrix4x4_2;

	private Matrix4x4 matrix4x4_3;

	private Matrix4x4 matrix4x4_4;

	private Matrix4x4 matrix4x4_5;

	private Matrix4x4 matrix4x4_6;

	private Matrix4x4 matrix4x4_7;

	private Vector3 vector3_1;

	private Vector3 vector3_2;

	private Mesh mesh_0;

	private static readonly int int_0 = Shader.PropertyToID("_SampleCount");

	private static readonly int int_1 = Shader.PropertyToID("_NoiseVelocity");

	private static readonly int int_2 = Shader.PropertyToID("_NoiseData");

	private static readonly int int_3 = Shader.PropertyToID("_MieG");

	private static readonly int int_4 = Shader.PropertyToID("_VolumetricLight");

	private static readonly int int_5 = Shader.PropertyToID("_ZTest");

	private static readonly int int_6 = Shader.PropertyToID("_HeightFog");

	private static readonly int int_7 = Shader.PropertyToID("_CameraForward");

	private static readonly int int_8 = Shader.PropertyToID("_CameraDepthTexture");

	private static readonly int int_9 = Shader.PropertyToID("_LightTexture0");

	private static readonly int int_10 = Shader.PropertyToID("_LightPos");

	private static readonly int int_11 = Shader.PropertyToID("_MyLightMatrix0");

	private static readonly int int_12 = Shader.PropertyToID("_WorldViewProj");

	private static readonly int int_13 = Shader.PropertyToID("_WorldView");

	private static readonly int int_14 = Shader.PropertyToID("_LightColor");

	private static readonly int int_15 = Shader.PropertyToID("_CosAngle");

	private static readonly int int_16 = Shader.PropertyToID("_PlaneD");

	private static readonly int int_17 = Shader.PropertyToID("_ConeApex");

	private static readonly int int_18 = Shader.PropertyToID("_ConeAxis");

	private static readonly int int_19 = Shader.PropertyToID("_MyWorld2Shadow");

	private static readonly int int_20 = Shader.PropertyToID("_MaxRayLength");

	private static readonly int int_21 = Shader.PropertyToID("_LightDir");

	private static readonly int int_22 = Shader.PropertyToID("_FrustumCorners");

	private static readonly int int_23 = Shader.PropertyToID("_ShadowStrength");

	public bool NeedToPrerender
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

	public Light Light => light_0;

	public Material VolumetricMaterial => material_0;

	public static void RefreshAll()
	{
		VolumetricLight[] array = UnityEngine.Object.FindObjectsByType<VolumetricLight>(FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnEnable();
		}
	}

	public void Awake()
	{
		SharedGameSettingsClass instance = Singleton<SharedGameSettingsClass>.Instance;
		if (instance != null && !instance.Graphics.Settings.VolumetricLight.Value)
		{
			GClass972.SafeDestroy(this);
			return;
		}
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D11 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D12 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal || SystemInfo.graphicsDeviceType == GraphicsDeviceType.PlayStation4 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan || SystemInfo.graphicsDeviceType == GraphicsDeviceType.XboxOne)
		{
			bool_1 = true;
		}
		light_0 = GetComponent<Light>();
		method_1();
		method_0();
		method_2();
		method_3();
		method_4();
		method_5();
	}

	public void OnEnable()
	{
		CheckIntensity();
		if (NeedToPrerender)
		{
			VolumetricLightRenderer.AddLight(this);
		}
	}

	public void OnDisable()
	{
		if (commandBuffer_0 != null)
		{
			commandBuffer_0.Clear();
		}
		VolumetricLightRenderer.RemoveLight(this);
	}

	public void OnDestroy()
	{
		VolumetricLightRenderer.RemoveLight(this);
		UnityEngine.Object.Destroy(material_0);
	}

	public void SetWeaponFlashlight()
	{
		bool_2 = true;
	}

	public void CheckIntensity()
	{
		if (!(light_0 != null) || !(light_0.gameObject != null) || !base.isActiveAndEnabled)
		{
			return;
		}
		if (light_0.intensity < 0.001f)
		{
			NeedToPrerender = false;
			return;
		}
		if (!NeedToPrerender)
		{
			VolumetricLightRenderer.AddLight(this);
		}
		NeedToPrerender = true;
	}

	public void method_0()
	{
		commandBuffer_0 = new CommandBuffer
		{
			name = "Light Command Buffer"
		};
		commandBuffer_1 = new CommandBuffer
		{
			name = "Dir Light Command Buffer"
		};
		commandBuffer_1.SetGlobalTexture("_CascadeShadowMapTexture", new RenderTargetIdentifier(BuiltinRenderTextureType.CurrentActive));
		if (light_0.type == LightType.Directional)
		{
			light_0.AddCommandBuffer(LightEvent.BeforeScreenspaceMask, commandBuffer_0);
			light_0.AddCommandBuffer(LightEvent.AfterShadowMap, commandBuffer_1);
		}
		else
		{
			light_0.AddCommandBuffer(LightEvent.AfterShadowMap, commandBuffer_0);
		}
	}

	public void method_1()
	{
		if (commandBuffer_0 != null)
		{
			if (light_0.type == LightType.Directional)
			{
				light_0.RemoveCommandBuffer(LightEvent.BeforeScreenspaceMask, commandBuffer_0);
				light_0.RemoveCommandBuffer(LightEvent.AfterShadowMap, commandBuffer_1);
			}
			else
			{
				light_0.RemoveCommandBuffer(LightEvent.AfterShadowMap, commandBuffer_0);
			}
		}
	}

	public void method_2()
	{
		if (shader_0 == null)
		{
			shader_0 = GClass872.Find("Sandbox/VolumetricLight");
		}
		material_0 = new Material(shader_0);
	}

	public void method_3()
	{
		material_0.SetInt(int_0, SampleCount);
		material_0.SetVector(int_1, new Vector4(NoiseVelocity.x, NoiseVelocity.y) * NoiseScale);
		material_0.SetVector(int_2, new Vector4(NoiseScale, NoiseIntensity, NoiseIntensityOffset));
		material_0.SetVector(int_3, new Vector4(1f - MieG * MieG, 1f + MieG * MieG, 2f * MieG, 1f / (4f * MathF.PI)));
		material_0.SetVector(int_4, new Vector4(ScatteringCoef, ExtinctionCoef, light_0.range, 1f - SkyboxExtinctionCoef));
		material_0.SetFloat(int_5, 8f);
		if (HeightFog)
		{
			material_0.EnableKeyword("HEIGHT_FOG");
			material_0.SetVector(int_6, new Vector4(GroundLevel, HeightScale));
		}
		else
		{
			material_0.DisableKeyword("HEIGHT_FOG");
		}
	}

	public void method_4()
	{
		switch (light_0.type)
		{
		case LightType.Spot:
			method_9();
			break;
		case LightType.Directional:
			method_12();
			break;
		case LightType.Point:
			method_6();
			break;
		}
	}

	public void method_5()
	{
		switch (light_0.type)
		{
		case LightType.Spot:
			method_10();
			break;
		case LightType.Directional:
			method_13();
			break;
		case LightType.Point:
			method_7();
			break;
		}
	}

	public void VolumetricLightPreRender(VolumetricLightRenderer renderer, Matrix4x4 viewProj)
	{
		if (!NeedToPrerender)
		{
			return;
		}
		if (commandBuffer_0 == null)
		{
			Awake();
		}
		if (!(light_0 == null) && !(light_0.gameObject == null))
		{
			material_0.SetVector(int_7, Camera.current.transform.forward);
			material_0.SetTexture(int_8, renderer.GetVolumeLightDepthBuffer());
			switch (light_0.type)
			{
			case LightType.Spot:
				method_11(renderer, viewProj);
				break;
			case LightType.Directional:
				method_14(renderer, viewProj);
				break;
			case LightType.Point:
				method_8(renderer, viewProj);
				break;
			}
		}
		else
		{
			VolumetricLightRenderer.RemoveLight(this);
		}
	}

	public void method_6()
	{
		float_0 = light_0.range * 2f;
		if (Noise)
		{
			material_0.EnableKeyword("NOISE");
		}
		else
		{
			material_0.DisableKeyword("NOISE");
		}
		if (light_0.cookie == null)
		{
			material_0.EnableKeyword("POINT");
			material_0.DisableKeyword("POINT_COOKIE");
		}
		else
		{
			material_0.EnableKeyword("POINT_COOKIE");
			material_0.DisableKeyword("POINT");
			material_0.SetTexture(int_9, light_0.cookie);
		}
	}

	public void method_7()
	{
		matrix4x4_0 = Matrix4x4.TRS(base.transform.position, light_0.transform.rotation, new Vector3(float_0, float_0, float_0));
		material_0.SetVector(int_10, new Vector4(light_0.transform.position.x, light_0.transform.position.y, light_0.transform.position.z, 1f / (light_0.range * light_0.range)));
		if (light_0.cookie != null)
		{
			Matrix4x4 inverse = Matrix4x4.TRS(light_0.transform.position, light_0.transform.rotation, Vector3.one).inverse;
			material_0.SetMatrix(int_11, inverse);
		}
	}

	public void method_8(VolumetricLightRenderer renderer, Matrix4x4 viewProj)
	{
		commandBuffer_0.Clear();
		int num = 0;
		if (!method_15())
		{
			num = 2;
		}
		material_0.SetPass(num);
		if (!vector3_0.Equals(base.transform.position) || quaternion_0 != base.transform.rotation)
		{
			method_7();
			vector3_0 = base.transform.position;
			quaternion_0 = base.transform.rotation;
		}
		mesh_0 = VolumetricLightRenderer.GetPointLightMesh();
		material_0.SetMatrix(int_12, viewProj * matrix4x4_0);
		material_0.SetMatrix(int_13, Camera.current.worldToCameraMatrix * matrix4x4_0);
		material_0.SetColor(int_14, light_0.color * light_0.intensity);
		bool flag = (light_0.transform.position - Camera.current.transform.position).magnitude >= QualitySettings.shadowDistance;
		if (light_0.shadows != LightShadows.None && !flag)
		{
			material_0.EnableKeyword("SHADOWS_CUBE");
			commandBuffer_0.SetGlobalTexture("_ShadowMapTexture", BuiltinRenderTextureType.CurrentActive);
			commandBuffer_0.SetRenderTarget(renderer.GetVolumeLightBuffer());
			commandBuffer_0.DrawMesh(mesh_0, matrix4x4_0, material_0, 0, num);
		}
		else
		{
			material_0.DisableKeyword("SHADOWS_CUBE");
			renderer.GlobalCommandBuffer.DrawMesh(mesh_0, matrix4x4_0, material_0, 0, num);
		}
	}

	public void method_9()
	{
		float_0 = light_0.range;
		float_1 = Mathf.Tan((light_0.spotAngle + 1f) * 0.5f * (MathF.PI / 180f)) * light_0.range;
		matrix4x4_2 = Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0f), Quaternion.identity, new Vector3(-0.5f, -0.5f, 1f));
		matrix4x4_4 = Matrix4x4.Perspective(light_0.spotAngle, 1f, 0f, 1f);
		material_0.SetFloat(int_15, Mathf.Cos((light_0.spotAngle + 1f) * 0.5f * (MathF.PI / 180f)));
		material_0.EnableKeyword("SPOT");
		if (Noise)
		{
			material_0.EnableKeyword("NOISE");
		}
		else
		{
			material_0.DisableKeyword("NOISE");
		}
		if (light_0.cookie == null)
		{
			material_0.SetTexture(int_9, VolumetricLightRenderer.GetDefaultSpotCookie());
		}
		else
		{
			material_0.SetTexture(int_9, light_0.cookie);
		}
		if (bool_1)
		{
			matrix4x4_5 = Matrix4x4.Perspective(light_0.spotAngle, 1f, light_0.range, light_0.shadowNearPlane);
		}
		else
		{
			matrix4x4_5 = Matrix4x4.Perspective(light_0.spotAngle, 1f, light_0.shadowNearPlane, light_0.range);
		}
		matrix4x4_3 = Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, new Vector3(0.5f, 0.5f, 0.5f));
		matrix4x4_6 = matrix4x4_3 * matrix4x4_5;
		matrix4x4_6[0, 2] *= -1f;
		matrix4x4_6[1, 2] *= -1f;
		matrix4x4_6[2, 2] *= -1f;
		matrix4x4_6[3, 2] *= -1f;
	}

	public void method_10()
	{
		matrix4x4_0 = Matrix4x4.TRS(base.transform.position, base.transform.rotation, new Vector3(float_1, float_1, float_0));
		matrix4x4_1 = Matrix4x4.TRS(light_0.transform.position, light_0.transform.rotation, Vector3.one).inverse;
		matrix4x4_7 = matrix4x4_6 * matrix4x4_1;
		material_0.SetMatrix(int_11, matrix4x4_2 * matrix4x4_4 * matrix4x4_1);
		material_0.SetVector(int_10, new Vector4(light_0.transform.position.x, light_0.transform.position.y, light_0.transform.position.z, 1f / (light_0.range * light_0.range)));
		vector3_1 = base.transform.position;
		vector3_2 = base.transform.forward;
		float value = 0f - Vector3.Dot(vector3_1 + vector3_2 * light_0.range, vector3_2);
		material_0.SetFloat(int_16, value);
		material_0.SetVector(int_17, new Vector4(vector3_1.x, vector3_1.y, vector3_1.z));
		material_0.SetVector(int_18, new Vector4(vector3_2.x, vector3_2.y, vector3_2.z));
	}

	public void method_11(VolumetricLightRenderer renderer, Matrix4x4 viewProj)
	{
		commandBuffer_0.Clear();
		int shaderPass = 1;
		if (!method_16())
		{
			shaderPass = 3;
		}
		if (Math.Abs(Vector3.SqrMagnitude(vector3_0 - base.transform.position)) > 0.01f || quaternion_0 != base.transform.rotation)
		{
			method_10();
			vector3_0 = base.transform.position;
			quaternion_0 = base.transform.rotation;
		}
		mesh_0 = VolumetricLightRenderer.GetSpotLightMesh();
		material_0.SetMatrix(int_12, viewProj * matrix4x4_0);
		material_0.SetVector(int_14, light_0.color * light_0.intensity);
		bool flag = !bool_2 && (light_0.transform.position - Camera.current.transform.position).magnitude >= QualitySettings.shadowDistance;
		if (light_0.shadows != LightShadows.None && !flag)
		{
			material_0.SetMatrix(int_19, matrix4x4_7);
			material_0.SetMatrix(int_13, matrix4x4_7);
			material_0.SetFloat(int_23, light_0.shadowStrength);
			material_0.EnableKeyword("SHADOWS_DEPTH");
			commandBuffer_0.SetGlobalTexture("_ShadowMapTexture", BuiltinRenderTextureType.CurrentActive);
			commandBuffer_0.SetRenderTarget(renderer.GetVolumeLightBuffer());
			commandBuffer_0.DrawMesh(mesh_0, matrix4x4_0, material_0, 0, shaderPass);
		}
		else
		{
			material_0.DisableKeyword("SHADOWS_DEPTH");
			renderer.GlobalCommandBuffer.DrawMesh(mesh_0, matrix4x4_0, material_0, 0, shaderPass);
		}
	}

	public void method_12()
	{
		if (Noise)
		{
			material_0.EnableKeyword("NOISE");
		}
		else
		{
			material_0.DisableKeyword("NOISE");
		}
		material_0.SetFloat(int_20, MaxRayLength);
		if (light_0.cookie == null)
		{
			material_0.EnableKeyword("DIRECTIONAL");
			material_0.DisableKeyword("DIRECTIONAL_COOKIE");
		}
		else
		{
			material_0.EnableKeyword("DIRECTIONAL_COOKIE");
			material_0.DisableKeyword("DIRECTIONAL");
			material_0.SetTexture(int_9, light_0.cookie);
		}
	}

	public void method_13()
	{
		material_0.SetVector(int_21, new Vector4(light_0.transform.forward.x, light_0.transform.forward.y, light_0.transform.forward.z, 1f / (light_0.range * light_0.range)));
	}

	public void method_14(VolumetricLightRenderer renderer, Matrix4x4 viewProj)
	{
		commandBuffer_0.Clear();
		int pass = 4;
		material_0.SetPass(4);
		if (!vector3_0.Equals(base.transform.position) || quaternion_0 != base.transform.rotation)
		{
			method_13();
			vector3_0 = base.transform.position;
			quaternion_0 = base.transform.rotation;
		}
		material_0.SetVector(int_14, light_0.color * light_0.intensity);
		vector4_0[0] = Camera.current.ViewportToWorldPoint(new Vector3(0f, 0f, Camera.current.farClipPlane));
		vector4_0[2] = Camera.current.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.current.farClipPlane));
		vector4_0[3] = Camera.current.ViewportToWorldPoint(new Vector3(1f, 1f, Camera.current.farClipPlane));
		vector4_0[1] = Camera.current.ViewportToWorldPoint(new Vector3(1f, 0f, Camera.current.farClipPlane));
		material_0.SetVectorArray(int_22, vector4_0);
		Texture source = null;
		if (light_0.shadows != LightShadows.None)
		{
			material_0.EnableKeyword("SHADOWS_DEPTH");
			commandBuffer_0.Blit(source, renderer.GetVolumeLightBuffer(), material_0, pass);
		}
		else
		{
			material_0.DisableKeyword("SHADOWS_DEPTH");
			renderer.GlobalCommandBuffer.Blit(source, renderer.GetVolumeLightBuffer(), material_0, pass);
		}
	}

	public bool method_15()
	{
		float sqrMagnitude = (light_0.transform.position - Camera.current.transform.position).sqrMagnitude;
		float num = light_0.range + 1f;
		return sqrMagnitude < num * num;
	}

	public bool method_16()
	{
		float num = Vector3.Dot(light_0.transform.forward, Camera.current.transform.position - light_0.transform.position);
		float num2 = light_0.range + 1f;
		if (num > num2)
		{
			return false;
		}
		if (Mathf.Acos(Vector3.Dot(base.transform.forward, (Camera.current.transform.position - light_0.transform.position).normalized)) * 57.29578f > (light_0.spotAngle + 3f) * 0.5f)
		{
			return false;
		}
		return true;
	}
}
