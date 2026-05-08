using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Serialization;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class VolumetricFog : MonoBehaviour
{
	public struct Struct154(int x, int y, int z)
	{
		public int x = x;

		public int y = y;

		public int z = z;
	}

	public struct Struct155
	{
		public Vector3 pos;

		public float range;

		public Vector3 color;

		[NonSerialized]
		public float Float_0;
	}

	public struct Struct156
	{
		public Vector3 start;

		public float range;

		public Vector3 end;

		public float radius;

		public Vector3 color;

		[NonSerialized]
		public float Float_0;
	}

	public struct Struct157
	{
		public Matrix4x4 mat;

		public Vector4 pos;

		public Vector3 color;

		public float bounded;
	}

	public struct Struct158
	{
		public Vector3 pos;

		public float radius;

		public Vector3 axis;

		public float stretch;

		public float density;

		public float noiseAmount;

		public float noiseSpeed;

		public float noiseScale;

		public float feather;

		public float blend;

		public float padding1;

		public float padding2;
	}

	private Material material_0;

	[HideInInspector]
	public Shader m_DebugShader;

	[HideInInspector]
	public Shader m_ShadowmapShader;

	[HideInInspector]
	public ComputeShader m_InjectLightingAndDensity;

	[HideInInspector]
	public ComputeShader m_Scatter;

	private Material material_1;

	[HideInInspector]
	public Shader m_ApplyToOpaqueShader;

	private Material material_2;

	[HideInInspector]
	public Shader m_BlurShadowmapShader;

	[HideInInspector]
	public Texture2D m_Noise;

	[HideInInspector]
	public bool m_Debug;

	[HideInInspector]
	[Range(0f, 1f)]
	public float m_Z = 1f;

	[Header("Size")]
	[GAttribute13(0.1f)]
	public float m_NearClip = 0.1f;

	[GAttribute13(0.1f)]
	public float m_FarClipMax = 100f;

	[Header("Fog Density")]
	[FormerlySerializedAs("m_Density")]
	public float m_GlobalDensityMult = 1f;

	private Struct154 struct154_0 = new Struct154(16, 2, 16);

	private Struct154 struct154_1 = new Struct154(32, 2, 1);

	private RenderTexture renderTexture_0;

	private RenderTexture renderTexture_1;

	private Struct154 struct154_2 = new Struct154(160, 90, 128);

	private Camera camera_0;

	public float m_ConstantFog;

	public float m_HeightFogAmount;

	public float m_HeightFogExponent;

	public float m_HeightFogOffset;

	[Tooltip("Noise multiplies with constant fog and height fog, but not with fog ellipsoids.")]
	[Range(0f, 1f)]
	public float m_NoiseFogAmount;

	public float m_NoiseFogScale = 1f;

	public Wind m_Wind;

	[Range(0f, 0.999f)]
	public float m_Anisotropy;

	[Header("Lights")]
	[FormerlySerializedAs("m_Intensity")]
	public float m_GlobalIntensityMult = 1f;

	[GAttribute13(0f)]
	public float m_AmbientLightIntensity;

	public Color m_AmbientLightColor = Color.white;

	private Struct155[] struct155_0;

	private ComputeBuffer computeBuffer_0;

	private Struct156[] struct156_0;

	private ComputeBuffer computeBuffer_1;

	private Struct157[] struct157_0;

	private ComputeBuffer computeBuffer_2;

	private Struct158[] struct158_0;

	private ComputeBuffer computeBuffer_3;

	private ComputeBuffer computeBuffer_4;

	private FogLight fogLight_0;

	private float[] float_0;

	private float[] float_1;

	private float[] float_2;

	private float[] float_3;

	private float[] float_4;

	private static readonly Vector2[] vector2_0 = new Vector2[4]
	{
		new Vector2(0f, 0f),
		new Vector2(1f, 0f),
		new Vector2(1f, 1f),
		new Vector2(0f, 1f)
	};

	private static float[] float_5 = new float[16];

	private static readonly int int_0 = Shader.PropertyToID("_VolumeInject");

	private static readonly int int_1 = Shader.PropertyToID("_VolumeScatter");

	private static readonly int int_2 = Shader.PropertyToID("_Z");

	private static readonly int int_3 = Shader.PropertyToID("_MainTex");

	private static readonly int int_4 = Shader.PropertyToID("_Screen_TexelSize");

	private static readonly int int_5 = Shader.PropertyToID("_VolumeScatter_TexelSize");

	private static readonly int int_6 = Shader.PropertyToID("_CameraFarOverMaxFar");

	private static readonly int int_7 = Shader.PropertyToID("_NearOverFarClip");

	public Camera Camera_0
	{
		get
		{
			if (camera_0 == null)
			{
				camera_0 = GetComponent<Camera>();
			}
			return camera_0;
		}
	}

	public float Single_0 => Mathf.Max(0f, m_NearClip);

	public float Single_1 => Mathf.Min(Camera_0.farClipPlane, m_FarClipMax);

	public void method_0(ref ComputeBuffer buffer)
	{
		if (buffer != null)
		{
			buffer.Release();
		}
		buffer = null;
	}

	public void OnDestroy()
	{
		method_1();
	}

	public void OnDisable()
	{
		method_1();
	}

	public void method_1()
	{
		UnityEngine.Object.DestroyImmediate(renderTexture_0);
		UnityEngine.Object.DestroyImmediate(renderTexture_1);
		method_0(ref computeBuffer_0);
		method_0(ref computeBuffer_1);
		method_0(ref computeBuffer_2);
		method_0(ref computeBuffer_3);
		method_0(ref computeBuffer_4);
		renderTexture_0 = null;
		renderTexture_1 = null;
	}

	public void method_2()
	{
		m_GlobalDensityMult = Mathf.Max(m_GlobalDensityMult, 0f);
		m_ConstantFog = Mathf.Max(m_ConstantFog, 0f);
		m_HeightFogAmount = Mathf.Max(m_HeightFogAmount, 0f);
	}

	public void method_3(int kernel)
	{
		int num = ((computeBuffer_0 != null) ? computeBuffer_0.count : 0);
		m_InjectLightingAndDensity.SetFloat("_PointLightsCount", num);
		if (num == 0)
		{
			m_InjectLightingAndDensity.SetBuffer(kernel, "_PointLights", computeBuffer_4);
			return;
		}
		if (struct155_0 == null || struct155_0.Length != num)
		{
			struct155_0 = new Struct155[num];
		}
		HashSet<FogLight> hashSet = LightManager<FogLight>.Get();
		int num2 = 0;
		HashSet<FogLight>.Enumerator enumerator = hashSet.GetEnumerator();
		while (enumerator.MoveNext())
		{
			FogLight current = enumerator.Current;
			if (!(current == null) && current.type == LightOverride.Type.Point && current.isOn)
			{
				Light light = current.light;
				struct155_0[num2].pos = light.transform.position;
				float num3 = light.range * current.m_RangeMult;
				struct155_0[num2].range = 1f / (num3 * num3);
				struct155_0[num2].color = new Vector3(light.color.r, light.color.g, light.color.b) * light.intensity * current.m_IntensityMult;
				num2++;
			}
		}
		computeBuffer_0.SetData(struct155_0);
		m_InjectLightingAndDensity.SetBuffer(kernel, "_PointLights", computeBuffer_0);
	}

	public void method_4(int kernel)
	{
		int num = ((computeBuffer_1 != null) ? computeBuffer_1.count : 0);
		m_InjectLightingAndDensity.SetFloat("_TubeLightsCount", num);
		if (num == 0)
		{
			m_InjectLightingAndDensity.SetBuffer(kernel, "_TubeLights", computeBuffer_4);
			return;
		}
		if (struct156_0 == null || struct156_0.Length != num)
		{
			struct156_0 = new Struct156[num];
		}
		HashSet<FogLight> hashSet = LightManager<FogLight>.Get();
		int num2 = 0;
		HashSet<FogLight>.Enumerator enumerator = hashSet.GetEnumerator();
		while (enumerator.MoveNext())
		{
			FogLight current = enumerator.Current;
			if (!(current == null) && current.type == LightOverride.Type.Tube && current.isOn)
			{
				TubeLight tubeLight = current.tubeLight;
				Transform transform = tubeLight.transform;
				Vector3 position = transform.position;
				Vector3 vector = 0.5f * transform.up * tubeLight.m_Length;
				struct156_0[num2].start = position + vector;
				struct156_0[num2].end = position - vector;
				float num3 = tubeLight.m_Range * current.m_RangeMult;
				struct156_0[num2].range = 1f / (num3 * num3);
				struct156_0[num2].color = new Vector3(tubeLight.m_Color.r, tubeLight.m_Color.g, tubeLight.m_Color.b) * tubeLight.m_Intensity * current.m_IntensityMult;
				struct156_0[num2].radius = tubeLight.m_Radius;
				num2++;
			}
		}
		computeBuffer_1.SetData(struct156_0);
		m_InjectLightingAndDensity.SetBuffer(kernel, "_TubeLights", computeBuffer_1);
	}

	public void method_5(int kernel)
	{
		int num = ((computeBuffer_2 != null) ? computeBuffer_2.count : 0);
		m_InjectLightingAndDensity.SetFloat("_AreaLightsCount", num);
		if (num == 0)
		{
			m_InjectLightingAndDensity.SetBuffer(kernel, "_AreaLights", computeBuffer_4);
			m_InjectLightingAndDensity.SetTexture(kernel, "_AreaLightShadowmap", Texture2D.whiteTexture);
			return;
		}
		if (struct157_0 == null || struct157_0.Length != num)
		{
			struct157_0 = new Struct157[num];
		}
		HashSet<FogLight> hashSet = LightManager<FogLight>.Get();
		int num2 = -1;
		int num3 = 0;
		HashSet<FogLight>.Enumerator enumerator = hashSet.GetEnumerator();
		while (enumerator.MoveNext())
		{
			FogLight current = enumerator.Current;
			if (current == null || current.type != LightOverride.Type.Area || !current.isOn)
			{
				continue;
			}
			AreaLight areaLight = current.areaLight;
			struct157_0[num3].mat = areaLight.GetProjectionMatrix(linearZ: true);
			struct157_0[num3].pos = areaLight.GetPosition();
			struct157_0[num3].color = new Vector3(areaLight.m_Color.r, areaLight.m_Color.g, areaLight.m_Color.b) * areaLight.m_Intensity * current.m_IntensityMult;
			struct157_0[num3].bounded = (current.m_Bounded ? 1 : 0);
			if (current.m_Shadows)
			{
				RenderTexture blurredShadowmap = areaLight.GetBlurredShadowmap();
				if (blurredShadowmap != null)
				{
					m_InjectLightingAndDensity.SetTexture(kernel, "_AreaLightShadowmap", blurredShadowmap);
					m_InjectLightingAndDensity.SetFloat("_ESMExponentAreaLight", current.m_ESMExponent);
					num2 = num3;
				}
			}
			num3++;
		}
		computeBuffer_2.SetData(struct157_0);
		m_InjectLightingAndDensity.SetBuffer(kernel, "_AreaLights", computeBuffer_2);
		m_InjectLightingAndDensity.SetFloat("_ShadowedAreaLightIndex", (num2 < 0) ? hashSet.Count : num2);
		if (num2 < 0)
		{
			m_InjectLightingAndDensity.SetTexture(kernel, "_AreaLightShadowmap", Texture2D.whiteTexture);
		}
	}

	public void method_6(int kernel)
	{
		int num = 0;
		HashSet<FogEllipsoid> hashSet = LightManager<FogEllipsoid>.Get();
		HashSet<FogEllipsoid>.Enumerator enumerator = hashSet.GetEnumerator();
		while (enumerator.MoveNext())
		{
			FogEllipsoid current = enumerator.Current;
			if (current != null && current.enabled && current.gameObject.activeSelf)
			{
				num++;
			}
		}
		m_InjectLightingAndDensity.SetFloat("_FogEllipsoidsCount", num);
		if (num == 0)
		{
			m_InjectLightingAndDensity.SetBuffer(kernel, "_FogEllipsoids", computeBuffer_4);
			return;
		}
		if (struct158_0 == null || struct158_0.Length != num)
		{
			struct158_0 = new Struct158[num];
		}
		int num2 = 0;
		HashSet<FogEllipsoid>.Enumerator enumerator2 = hashSet.GetEnumerator();
		while (enumerator2.MoveNext())
		{
			FogEllipsoid current2 = enumerator2.Current;
			if (!(current2 == null) && current2.enabled && current2.gameObject.activeSelf)
			{
				Transform transform = current2.transform;
				struct158_0[num2].pos = transform.position;
				struct158_0[num2].radius = current2.m_Radius * current2.m_Radius;
				struct158_0[num2].axis = -transform.up;
				struct158_0[num2].stretch = 1f / current2.m_Stretch - 1f;
				struct158_0[num2].density = current2.m_Density;
				struct158_0[num2].noiseAmount = current2.m_NoiseAmount;
				struct158_0[num2].noiseSpeed = current2.m_NoiseSpeed;
				struct158_0[num2].noiseScale = current2.m_NoiseScale;
				struct158_0[num2].feather = 1f - current2.m_Feather;
				struct158_0[num2].blend = ((current2.m_Blend != FogEllipsoid.Blend.Additive) ? 1 : 0);
				num2++;
			}
		}
		computeBuffer_3.SetData(struct158_0);
		m_InjectLightingAndDensity.SetBuffer(kernel, "_FogEllipsoids", computeBuffer_3);
	}

	public FogLight method_7()
	{
		HashSet<FogLight> hashSet = LightManager<FogLight>.Get();
		FogLight result = null;
		HashSet<FogLight>.Enumerator enumerator = hashSet.GetEnumerator();
		while (enumerator.MoveNext())
		{
			FogLight current = enumerator.Current;
			if (!(current == null) && current.type == LightOverride.Type.Directional && current.isOn)
			{
				result = current;
				break;
			}
		}
		return result;
	}

	public void OnPreRender()
	{
		fogLight_0 = method_7();
		if (fogLight_0 != null)
		{
			fogLight_0.UpdateDirectionalShadowmap();
		}
	}

	public void method_8(int kernel)
	{
		if (float_0 == null || float_0.Length != 3)
		{
			float_0 = new float[3];
		}
		if (float_1 == null || float_1.Length != 3)
		{
			float_1 = new float[3];
		}
		if (fogLight_0 == null)
		{
			float_0[0] = 0f;
			float_0[1] = 0f;
			float_0[2] = 0f;
			m_InjectLightingAndDensity.SetFloats("_DirLightColor", float_0);
			return;
		}
		fogLight_0.SetUpDirectionalShadowmapForSampling(fogLight_0.m_Shadows, m_InjectLightingAndDensity, kernel);
		Light light = fogLight_0.light;
		Vector4 vector = light.color;
		vector *= light.intensity * fogLight_0.m_IntensityMult;
		float_0[0] = vector.x;
		float_0[1] = vector.y;
		float_0[2] = vector.z;
		m_InjectLightingAndDensity.SetFloats("_DirLightColor", float_0);
		Vector3 forward = light.GetComponent<Transform>().forward;
		float_1[0] = forward.x;
		float_1[1] = forward.y;
		float_1[2] = forward.z;
		m_InjectLightingAndDensity.SetFloats("_DirLightDir", float_1);
	}

	public void method_9(int kernel)
	{
		method_2();
		method_18();
		method_15();
		float num = (Single_1 - Single_0) * 0.01f;
		m_InjectLightingAndDensity.SetFloat("_Density", m_GlobalDensityMult * 0.128f * num);
		m_InjectLightingAndDensity.SetFloat("_Intensity", m_GlobalIntensityMult);
		m_InjectLightingAndDensity.SetFloat("_Anisotropy", m_Anisotropy);
		m_InjectLightingAndDensity.SetTexture(kernel, "_VolumeInject", renderTexture_0);
		m_InjectLightingAndDensity.SetTexture(kernel, "_Noise", m_Noise);
		if (float_2 == null || float_2.Length != 4)
		{
			float_2 = new float[4];
		}
		if (float_3 == null || float_3.Length != 3)
		{
			float_3 = new float[3];
		}
		if (float_4 == null || float_4.Length != 3)
		{
			float_4 = new float[3];
		}
		float_2[0] = m_ConstantFog;
		float_2[1] = m_HeightFogExponent;
		float_2[2] = m_HeightFogOffset;
		float_2[3] = m_HeightFogAmount;
		m_InjectLightingAndDensity.SetFloats("_FogParams", float_2);
		m_InjectLightingAndDensity.SetFloat("_NoiseFogAmount", m_NoiseFogAmount);
		m_InjectLightingAndDensity.SetFloat("_NoiseFogScale", m_NoiseFogScale);
		m_InjectLightingAndDensity.SetFloat("_WindSpeed", (m_Wind == null) ? 0f : m_Wind.m_Speed);
		Vector3 vector = ((m_Wind == null) ? Vector3.forward : m_Wind.transform.forward);
		float_3[0] = vector.x;
		float_3[1] = vector.y;
		float_3[2] = vector.z;
		m_InjectLightingAndDensity.SetFloats("_WindDir", float_3);
		m_InjectLightingAndDensity.SetFloat("_Time", Time.time);
		m_InjectLightingAndDensity.SetFloat("_NearOverFarClip", Single_0 / Single_1);
		Color color = m_AmbientLightColor * m_AmbientLightIntensity * 0.1f;
		float_4[0] = color.r;
		float_4[1] = color.g;
		float_4[2] = color.b;
		m_InjectLightingAndDensity.SetFloats("_AmbientLight", float_4);
		method_3(kernel);
		method_4(kernel);
		method_5(kernel);
		method_8(kernel);
	}

	public void method_10()
	{
		method_9(0);
		m_InjectLightingAndDensity.Dispatch(0, struct154_2.x / struct154_0.x, struct154_2.y / struct154_0.y, struct154_2.z / struct154_0.z);
		m_Scatter.SetTexture(0, "_VolumeInject", renderTexture_0);
		m_Scatter.SetTexture(0, "_VolumeScatter", renderTexture_1);
		m_Scatter.Dispatch(0, struct154_2.x / struct154_1.x, struct154_2.y / struct154_1.y, 1);
	}

	public void method_11(RenderTexture src, RenderTexture dest)
	{
		method_20(ref material_0, m_DebugShader);
		material_0.SetTexture(int_0, renderTexture_0);
		material_0.SetTexture(int_1, renderTexture_1);
		material_0.SetFloat(int_2, m_Z);
		material_0.SetTexture(int_3, src);
		Graphics.Blit(src, dest, material_0);
	}

	public void method_12(int width, int height)
	{
		Shader.SetGlobalTexture(int_1, renderTexture_1);
		Shader.SetGlobalVector(int_4, new Vector4(1f / (float)width, 1f / (float)height, width, height));
		Shader.SetGlobalVector(int_5, new Vector4(1f / (float)struct154_2.x, 1f / (float)struct154_2.y, 1f / (float)struct154_2.z, 0f));
		Shader.SetGlobalFloat(int_6, Camera_0.farClipPlane / Single_1);
		Shader.SetGlobalFloat(int_7, Single_0 / Single_1);
	}

	public void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		if (!CheckSupport())
		{
			Debug.LogError(GetUnsupportedErrorMessage());
			GClass860.BlitOrCopy(src, dest);
			base.enabled = false;
			return;
		}
		if (m_Debug)
		{
			method_11(src, dest);
			return;
		}
		method_10();
		method_20(ref material_1, m_ApplyToOpaqueShader);
		material_1.SetTexture(int_3, src);
		method_12(src.width, src.height);
		Graphics.Blit(src, dest, material_1);
		method_13(enable: true);
	}

	public void OnPostRender()
	{
		method_13(enable: false);
	}

	public void method_13(bool enable)
	{
		if (enable)
		{
			Shader.EnableKeyword("VOLUMETRIC_FOG");
		}
		else
		{
			Shader.DisableKeyword("VOLUMETRIC_FOG");
		}
	}

	public Vector3 method_14(Camera c, Transform t, Vector3 p)
	{
		return t.InverseTransformPoint(c.ViewportToWorldPoint(p));
	}

	public void method_15()
	{
		float single_ = Single_1;
		Vector3 position = Camera_0.transform.position;
		Vector2[] array = vector2_0;
		for (int i = 0; i < 4; i++)
		{
			Vector3 vector = Camera_0.ViewportToWorldPoint(new Vector3(array[i].x, array[i].y, single_)) - position;
			float_5[i * 4] = vector.x;
			float_5[i * 4 + 1] = vector.y;
			float_5[i * 4 + 2] = vector.z;
			float_5[i * 4 + 3] = 0f;
		}
		m_InjectLightingAndDensity.SetVector("_CameraPos", position);
		m_InjectLightingAndDensity.SetFloats("_FrustumRays", float_5);
	}

	public void method_16(ref RenderTexture volume)
	{
		if (!volume)
		{
			volume = new RenderTexture(struct154_2.x, struct154_2.y, 0, RuntimeUtilities.defaultHDRRenderTextureFormat);
			volume.volumeDepth = struct154_2.z;
			volume.dimension = TextureDimension.Tex3D;
			volume.enableRandomWrite = true;
			volume.Create();
		}
	}

	public void method_17(ref ComputeBuffer buffer, int count, int stride)
	{
		if (buffer == null || buffer.count != count)
		{
			if (buffer != null)
			{
				buffer.Release();
				buffer = null;
			}
			if (count > 0)
			{
				buffer = new ComputeBuffer(count, stride);
			}
		}
	}

	public void method_18()
	{
		method_16(ref renderTexture_0);
		method_16(ref renderTexture_1);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		HashSet<FogLight>.Enumerator enumerator = LightManager<FogLight>.Get().GetEnumerator();
		while (enumerator.MoveNext())
		{
			FogLight current = enumerator.Current;
			if (current == null)
			{
				continue;
			}
			bool isOn = current.isOn;
			switch (current.type)
			{
			case LightOverride.Type.Point:
				if (isOn)
				{
					num++;
				}
				break;
			case LightOverride.Type.Tube:
				if (isOn)
				{
					num2++;
				}
				break;
			case LightOverride.Type.Area:
				if (isOn)
				{
					num3++;
				}
				break;
			}
		}
		method_17(ref computeBuffer_0, num, Marshal.SizeOf(typeof(Struct155)));
		method_17(ref computeBuffer_1, num2, Marshal.SizeOf(typeof(Struct156)));
		method_17(ref computeBuffer_2, num3, Marshal.SizeOf(typeof(Struct157)));
		method_17(ref computeBuffer_4, 1, 4);
	}

	public void method_19(ref RenderTexture rt)
	{
		if (!(rt == null))
		{
			RenderTexture.ReleaseTemporary(rt);
			rt = null;
		}
	}

	public void method_20(ref Material material, Shader shader)
	{
		if (!material)
		{
			if (!shader)
			{
				Debug.LogError("Missing shader");
				return;
			}
			material = new Material(shader);
			material.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawFrustum(Vector3.zero, Camera_0.fieldOfView, Single_1, Single_0, Camera_0.aspect);
	}

	public static bool CheckSupport()
	{
		return SystemInfo.supportsComputeShaders;
	}

	public static string GetUnsupportedErrorMessage()
	{
		return "Volumetric Fog requires compute shaders and this platform doesn't support them. Disabling. \nDetected device type: " + SystemInfo.graphicsDeviceType.ToString() + ", version: " + SystemInfo.graphicsDeviceVersion;
	}
}
