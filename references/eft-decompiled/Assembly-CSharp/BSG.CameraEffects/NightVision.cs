using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

namespace BSG.CameraEffects;

[RequireComponent(typeof(Camera))]
public class NightVision : MonoBehaviour, GInterface57
{
	[CompilerGenerated]
	public class Class714
	{
		public float v;

		public Func<Keyframe, bool> func_0;

		public bool method_0(Keyframe key)
		{
			return key.value > v;
		}
	}

	public Shader Shader;

	public float Intensity;

	public Texture Noise;

	public Texture Mask;

	public float MaskSize = 1f;

	public float NoiseIntensity;

	public float NoiseScale;

	[ColorUsage(false)]
	public Color Color;

	public TextureMask TextureMask;

	public MonoBehaviour[] SwitchComponentsOn;

	public MonoBehaviour[] SwitchComponentsOff;

	public AnimationCurve BlackFlashGoingToOn;

	public AnimationCurve BlackFlashGoingToOff;

	public AudioClip SwitchOn;

	public AudioClip SwitchOff;

	public Texture ThermalMaskTexture;

	public Texture AnvisMaskTexture;

	public Texture BinocularMaskTexture;

	public Texture GasMaskTexture;

	public Texture OldMonocularMaskTexture;

	public float ambientFactor = 1.2f;

	[SerializeField]
	private bool _on;

	private static readonly int int_0 = Shader.PropertyToID("_Color");

	private static readonly int int_1 = Shader.PropertyToID("_Intensity");

	private static readonly int int_2 = Shader.PropertyToID("_Noise");

	private static readonly int int_3 = Shader.PropertyToID("_NoiseScale");

	private static readonly int int_4 = Shader.PropertyToID("_NoiseIntensity");

	private static readonly int int_5 = Shader.PropertyToID("_NightVisionOn");

	private static readonly int int_6 = Shader.PropertyToID("_FinalNvRT");

	private static readonly string string_0 = "NIGHT_VISION_NOISE";

	private Material material_0;

	private Material material_1;

	private int int_7;

	private int int_8;

	private Class687 class687_0;

	[CanBeNull]
	private SSAA ssaa_0;

	private SSAAPropagator ssaapropagator_0;

	private bool? nullable_0;

	private bool bool_0 = true;

	private CommandBuffer commandBuffer_0;

	private Camera camera_0;

	private bool bool_1;

	private Vector4 vector4_0;

	private Color color_0;

	public Material Material_0
	{
		get
		{
			if (!(material_0 == null))
			{
				return material_0;
			}
			return material_0 = new Material(Shader);
		}
	}

	public bool On
	{
		get
		{
			return _on;
		}
		set
		{
			_on = value;
		}
	}

	public bool InProcessSwitching => class687_0.InProcess;

	public Color Color_0 => Color * (1f - 2f * class687_0.Value);

	public void Awake()
	{
		method_0();
	}

	public void method_0()
	{
		_on = false;
		class687_0 = new Class687
		{
			SwitchingOn = BlackFlashGoingToOn,
			SwitchingOff = BlackFlashGoingToOff
		};
		ssaa_0 = GetComponent<SSAA>();
		material_0 = new Material(Shader);
		class687_0.AddCase(smethod_0(class687_0.SwitchingOn), delegate
		{
			method_1(on: true);
		}, switchingOn: true);
		class687_0.AddCase(smethod_0(class687_0.SwitchingOff), delegate
		{
			method_1(on: false);
		}, switchingOn: false);
		ssaapropagator_0 = GetComponent<SSAAPropagator>();
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.SetNightVisionMaterial(material_0);
		}
		bool_1 = ssaa_0 != null;
		if (!bool_1)
		{
			commandBuffer_0 = new CommandBuffer
			{
				name = "NightVisionCB+1"
			};
			camera_0 = GetComponent<Camera>();
			camera_0.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, commandBuffer_0);
			camera_0.AddCommandBuffer(CameraEvent.BeforeImageEffects, commandBuffer_0);
		}
		ApplySettings();
	}

	public void SetMask(NightVisionComponent.EMask mask)
	{
		switch (mask)
		{
		default:
			throw new ArgumentOutOfRangeException("mask", mask, null);
		case NightVisionComponent.EMask.Thermal:
			Mask = ThermalMaskTexture;
			break;
		case NightVisionComponent.EMask.Anvis:
			Mask = AnvisMaskTexture;
			break;
		case NightVisionComponent.EMask.Binocular:
			Mask = BinocularMaskTexture;
			break;
		case NightVisionComponent.EMask.GasMask:
			Mask = GasMaskTexture;
			break;
		case NightVisionComponent.EMask.OldMonocular:
			Mask = OldMonocularMaskTexture;
			break;
		}
	}

	public void ApplySettings()
	{
		float num = NoiseScale * (float)Screen.height / (float)Noise.height;
		vector4_0 = new Vector4(num * (float)Screen.width / (float)Screen.height, num, 0f, 0f);
		Material_0.SetColor(int_0, Color_0);
		Material_0.SetFloat(int_1, Intensity);
		Material_0.SetFloat(int_4, NoiseIntensity);
		Material_0.SetVector(int_3, vector4_0);
		Material_0.SetTexture(int_2, Noise);
		Material_0.EnableKeyword(string_0);
		if (bool_1)
		{
			Material_0.EnableKeyword(string_0);
		}
		if (!(TextureMask == null))
		{
			TextureMask.Mask = Mask;
			TextureMask.Size = MaskSize;
			TextureMask.ApplySettings();
		}
	}

	public void StartSwitch(bool on)
	{
		if (class687_0 == null)
		{
			Debug.LogError("[Switcher] is null in [NightVision].");
		}
		else if (bool_0)
		{
			bool_0 = false;
			method_1(on);
			class687_0.ForceSwitch(on);
		}
		else if (class687_0.InProcess)
		{
			nullable_0 = on;
		}
		else
		{
			class687_0.Switch(on);
		}
	}

	public void FastForwardSwitch()
	{
		if (class687_0.InProcess)
		{
			class687_0.Process(float.PositiveInfinity);
		}
		Update();
	}

	public void OnDisable()
	{
		if (commandBuffer_0 != null)
		{
			commandBuffer_0.Clear();
		}
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.SetNightVisionMaterial(null);
		}
		Shader.SetGlobalFloat(int_5, 0f);
		if (bool_1)
		{
			GClass1002.SetKeyword(Material_0, string_0, enable: true);
		}
	}

	public void Update()
	{
		if (class687_0.InProcess)
		{
			class687_0.Process(Time.deltaTime);
		}
		else if (nullable_0.HasValue)
		{
			class687_0.Switch(nullable_0.Value);
			nullable_0 = null;
		}
		Material_0.SetColor(int_0, Color_0);
		if (bool_1)
		{
			GClass1002.SetKeyword(Material_0, string_0, enable: true);
		}
	}

	public void OnPreCull()
	{
		Shader.SetGlobalFloat(int_5, On ? 1 : 0);
		if (commandBuffer_0 != null)
		{
			commandBuffer_0.Clear();
		}
		if (!On)
		{
			if (ssaapropagator_0 != null)
			{
				ssaapropagator_0.SetNightVisionMaterial(null);
			}
			return;
		}
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.SetNightVisionMaterial(Material_0);
		}
		RenderTargetIdentifier renderTargetIdentifier = (ssaa_0 ? ssaa_0.GetRTIdentifier() : ((RenderTargetIdentifier)BuiltinRenderTextureType.CameraTarget));
		Singleton<LevelSettings>.Instance.AmbientType = AmbientType.NightVision;
		if (commandBuffer_0 != null)
		{
			commandBuffer_0.GetTemporaryRT(int_6, -1, -1);
			commandBuffer_0.Blit(renderTargetIdentifier, int_6, Material_0);
			commandBuffer_0.Blit(int_6, renderTargetIdentifier);
			commandBuffer_0.ReleaseTemporaryRT(int_6);
		}
	}

	public void OnPostRender()
	{
		if ((bool)Singleton<LevelSettings>.Instance)
		{
			Singleton<LevelSettings>.Instance.AmbientType = AmbientType.Default;
		}
	}

	public void method_1(bool on)
	{
		_on = on;
		TextureMask.TryToEnable(this, on);
		MonoBehaviour[] switchComponentsOn = SwitchComponentsOn;
		for (int i = 0; i < switchComponentsOn.Length; i++)
		{
			switchComponentsOn[i].enabled = on;
		}
		switchComponentsOn = SwitchComponentsOff;
		for (int i = 0; i < switchComponentsOn.Length; i++)
		{
			switchComponentsOn[i].enabled = !on;
		}
	}

	public static float smethod_0(AnimationCurve curve)
	{
		float result = 0f;
		float v = float.MinValue;
		foreach (Keyframe item in curve.keys.Where((Keyframe key) => key.value > v))
		{
			result = item.time;
			v = item.value;
		}
		return result;
	}

	public void OnDestroy()
	{
		Shader.SetGlobalFloat(int_5, 0f);
	}

	[CompilerGenerated]
	public void method_2()
	{
		method_1(on: true);
	}

	[CompilerGenerated]
	public void method_3()
	{
		method_1(on: false);
	}
}
