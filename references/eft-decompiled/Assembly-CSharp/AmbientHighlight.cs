using System;
using UnityEngine;
using UnityEngine.Rendering;

[GAttribute8(19100)]
public class AmbientHighlight : OnRenderObjectConnector
{
	[Serializable]
	public struct AmbientDirectionalLight
	{
		public Transform LightTransform;

		public Color Color;

		public float Intensity;
	}

	public enum StencilType
	{
		Static,
		Characters,
		Hands
	}

	[Serializable]
	public class HighlightSettings
	{
		public float HighlightMinMultiplier = 0.2f;

		public float HighlightMaxMultiplier = 1.7f;

		public AnimationCurve HighlightIntensityCurve = new AnimationCurve(new Keyframe(-1f, 0f), new Keyframe(0f, 0f), new Keyframe(0.3f, 1f));

		public StencilType StencilTypeToUse = StencilType.Hands;
	}

	[SerializeField]
	private float AmbientBlur = 1f;

	[SerializeField]
	private Material AmbientMaterial;

	[SerializeField]
	private HighlightSettings[] _highlightSettings = new HighlightSettings[0];

	[SerializeField]
	private AmbientDirectionalLight[] _additionalLights;

	private GClass1001 gclass1001_0 = new GClass1001("Ambient Highlight", CameraEvent.AfterLighting);

	private static Mesh mesh_0;

	private static readonly Matrix4x4 matrix4x4_0 = Matrix4x4.identity;

	private static readonly int int_0 = Shader.PropertyToID("_SrcBlend");

	private static readonly int int_1 = Shader.PropertyToID("_DstBlend");

	private static readonly int[] int_2 = new int[3]
	{
		Shader.PropertyToID("_SHAr"),
		Shader.PropertyToID("_SHAg"),
		Shader.PropertyToID("_SHAb")
	};

	private static readonly int[] int_3 = new int[3]
	{
		Shader.PropertyToID("_SHBr"),
		Shader.PropertyToID("_SHBg"),
		Shader.PropertyToID("_SHBb")
	};

	private static readonly int int_4 = Shader.PropertyToID("_SHC");

	private static readonly int int_5 = Shader.PropertyToID("_StencilType");

	private static readonly int int_6 = Shader.PropertyToID("_HighlightMultiplier");

	private static readonly int int_7 = Shader.PropertyToID("_AmbientBlur");

	private MaterialPropertyBlock materialPropertyBlock_0;

	public override void Start()
	{
		base.Start();
		materialPropertyBlock_0 = new MaterialPropertyBlock();
	}

	public override void ManualOnRenderObject(Camera currentCamera)
	{
		base.ManualOnRenderObject(currentCamera);
		if (_highlightSettings != null && _highlightSettings.Length != 0 && !(currentCamera != CameraClass.Instance.Camera))
		{
			gclass1001_0.OnRenderObject(out var buffer);
			method_0(buffer, currentCamera);
		}
	}

	public void method_0(CommandBuffer ambientBuffer, Camera currentCamera)
	{
		if (GClass4.IsAvailable)
		{
			if (!mesh_0)
			{
				mesh_0 = GetQuadMesh();
			}
			if (materialPropertyBlock_0 == null)
			{
				materialPropertyBlock_0 = new MaterialPropertyBlock();
			}
			ambientBuffer.Clear();
			BlendMode blendMode = (currentCamera.allowHDR ? BlendMode.One : BlendMode.DstColor);
			BlendMode blendMode2 = (currentCamera.allowHDR ? BlendMode.One : BlendMode.Zero);
			ambientBuffer.SetGlobalFloat(int_0, (float)blendMode);
			ambientBuffer.SetGlobalFloat(int_1, (float)blendMode2);
			ambientBuffer.SetRenderTarget(BuiltinRenderTextureType.CurrentActive, BuiltinRenderTextureType.CurrentActive);
			float time = 0f - GClass4.Instance.LightObject.transform.forward.y;
			HighlightSettings[] highlightSettings = _highlightSettings;
			foreach (HighlightSettings highlightSettings2 in highlightSettings)
			{
				float t = highlightSettings2.HighlightIntensityCurve.Evaluate(time);
				float value = Mathf.Lerp(highlightSettings2.HighlightMinMultiplier, highlightSettings2.HighlightMaxMultiplier, t);
				materialPropertyBlock_0.SetFloat(int_7, 1f / AmbientBlur);
				materialPropertyBlock_0.SetFloat(int_6, value);
				AmbientMaterial.SetInt(int_5, (int)highlightSettings2.StencilTypeToUse);
				ambientBuffer.DrawMesh(mesh_0, matrix4x4_0, AmbientMaterial, 0, 0, materialPropertyBlock_0);
			}
		}
	}

	public new void OnDestroy()
	{
		gclass1001_0.DestroyBuffers();
	}

	public void SetSH(SphericalHarmonicsL2 sh)
	{
		if (materialPropertyBlock_0 == null)
		{
			materialPropertyBlock_0 = new MaterialPropertyBlock();
		}
		AmbientDirectionalLight[] additionalLights = _additionalLights;
		for (int i = 0; i < additionalLights.Length; i++)
		{
			AmbientDirectionalLight ambientDirectionalLight = additionalLights[i];
			if (!(ambientDirectionalLight.LightTransform == null))
			{
				sh.AddDirectionalLight(-ambientDirectionalLight.LightTransform.forward, ambientDirectionalLight.Color, ambientDirectionalLight.Intensity);
			}
		}
		materialPropertyBlock_0.SetVector(int_4, new Vector4(sh[0, 8], sh[1, 8], sh[2, 8], 1f));
		for (int j = 0; j < 3; j++)
		{
			materialPropertyBlock_0.SetVector(int_2[j], new Vector4(sh[j, 3], sh[j, 1], sh[j, 2], sh[j, 0] - sh[j, 6]));
			materialPropertyBlock_0.SetVector(int_3[j], new Vector4(sh[j, 4], sh[j, 5], sh[j, 6] * 3f, sh[j, 7]));
		}
	}

	public static Mesh GetQuadMesh()
	{
		float z = 0.1f;
		Vector3[] vertices = new Vector3[4]
		{
			new Vector3(-1f, -1f, z),
			new Vector3(1f, -1f, z),
			new Vector3(-1f, 1f, z),
			new Vector3(1f, 1f, z)
		};
		Vector2[] uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f)
		};
		int[] triangles = new int[6] { 0, 1, 3, 3, 2, 0 };
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = triangles;
		mesh.name = "AmbientHighlight GetQuadMesh";
		mesh.RecalculateBounds();
		mesh.name = "QuadMeshForAmbientLight";
		return mesh;
	}
}
