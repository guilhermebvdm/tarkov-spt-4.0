using System;
using System.Runtime.CompilerServices;
using BSG.CameraEffects;
using EFT.InventoryLogic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ThermalVision : MonoBehaviour, GInterface57
{
	public bool On;

	public bool IsNoisy;

	public bool IsFpsStuck;

	public bool IsMotionBlurred;

	public bool IsGlitch;

	public bool IsPixelated;

	public ThermalVisionUtilities ThermalVisionUtilities;

	public StuckFPSUtilities StuckFpsUtilities;

	public MotionBlurUtilities MotionBlurUtilities;

	public GlitchUtilities GlitchUtilities;

	public PixelationUtilities PixelationUtilities;

	public TextureMask TextureMask;

	public MonoBehaviour[] SwitchComponentsOn;

	public MonoBehaviour[] SwitchComponentsOff;

	public float ChromaticAberrationThermalShift = 0.013f;

	public AnimationCurve BlackFlashGoingToOn;

	public AnimationCurve BlackFlashGoingToOff;

	public AudioClip SwitchOn;

	public AudioClip SwitchOff;

	[Header("Unsharp Mask")]
	public float UnsharpRadiusBlur = 5f;

	public float UnsharpBias = 2f;

	private Class687 class687_0;

	private bool? nullable_0;

	private bool bool_0 = true;

	private CommandBuffer commandBuffer_0;

	private CommandBuffer commandBuffer_1;

	private Material material_0;

	private Camera camera_0;

	private GClass980 gclass980_0;

	private GClass978 gclass978_0;

	private GClass977 gclass977_0;

	private GClass979 gclass979_0;

	private VolumetricLightRenderer volumetricLightRenderer_0;

	private SSAA ssaa_0;

	private MBOIT_Scattering mboit_Scattering_0;

	private ChromaticAberration chromaticAberration_0;

	private float float_0;

	private float float_1;

	private static readonly int int_0 = Shader.PropertyToID("_FinalRT");

	private static readonly int int_1 = Shader.PropertyToID("_RampTex");

	private static readonly int int_2 = Shader.PropertyToID("_MainTexColorCoef");

	private static readonly int int_3 = Shader.PropertyToID("_MinimumTemperatureValue");

	private static readonly int int_4 = Shader.PropertyToID("_Noise");

	private static readonly int int_5 = Shader.PropertyToID("_NoiseIntensity");

	private static readonly int int_6 = Shader.PropertyToID("_DepthFade");

	private static readonly int int_7 = Shader.PropertyToID("_RampShift");

	private static readonly int int_8 = Shader.PropertyToID("_ThermalVisionOn");

	private static readonly int int_9 = Shader.PropertyToID("_RadiusBlur");

	private static readonly int int_10 = Shader.PropertyToID("_Bias");

	public bool InProcessSwitching => class687_0.InProcess;

	public void Awake()
	{
		method_0();
	}

	public void method_0()
	{
		class687_0 = new Class687
		{
			SwitchingOn = BlackFlashGoingToOn,
			SwitchingOff = BlackFlashGoingToOff
		};
		class687_0.AddCase(smethod_0(class687_0.SwitchingOn), delegate
		{
			method_1(on: true);
		}, switchingOn: true);
		class687_0.AddCase(smethod_0(class687_0.SwitchingOff), delegate
		{
			method_1(on: false);
		}, switchingOn: false);
		camera_0 = GetComponent<Camera>();
		gclass980_0 = new GClass980(this);
		gclass978_0 = new GClass978(this);
		gclass977_0 = new GClass977(this);
		gclass979_0 = new GClass979(this, camera_0);
		ssaa_0 = GetComponent<SSAA>();
		mboit_Scattering_0 = GetComponent<MBOIT_Scattering>();
		volumetricLightRenderer_0 = camera_0.GetComponent<VolumetricLightRenderer>();
		chromaticAberration_0 = camera_0.GetComponent<ChromaticAberration>();
		float_0 = chromaticAberration_0.Shift;
		Shader shader = GClass872.Find("Hidden/ThermalVision");
		material_0 = new Material(shader);
		if (commandBuffer_0 == null)
		{
			commandBuffer_0 = new CommandBuffer
			{
				name = "Thermal Vision+01"
			};
		}
		camera_0.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer_0);
		camera_0.AddCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer_0);
		if (commandBuffer_1 == null)
		{
			commandBuffer_1 = new CommandBuffer
			{
				name = "CopyGBuffer0CB"
			};
		}
		camera_0.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, commandBuffer_1);
		camera_0.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, commandBuffer_1);
	}

	public void Update()
	{
		if (camera_0.enabled)
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
		if (mboit_Scattering_0 != null)
		{
			mboit_Scattering_0.ThermalVisionIsEnabled = false;
		}
		volumetricLightRenderer_0.IsOn = true;
		commandBuffer_0?.Clear();
		commandBuffer_1?.Clear();
		Shader.SetGlobalFloat(int_8, 0f);
	}

	public void OnPreCull()
	{
		if (volumetricLightRenderer_0.IsOn == On)
		{
			volumetricLightRenderer_0.IsOn = !On;
		}
		if (mboit_Scattering_0 != null)
		{
			mboit_Scattering_0.ThermalVisionIsEnabled = On;
		}
		if (!On)
		{
			gclass978_0.IsNeedClear = true;
			if (ssaa_0 != null)
			{
				ssaa_0.SetThermalVisionOn(on: false, null);
			}
			return;
		}
		RenderTargetIdentifier renderTargetIdentifier = (ssaa_0 ? ssaa_0.GetRTIdentifier() : ((RenderTargetIdentifier)BuiltinRenderTextureType.CameraTarget));
		commandBuffer_1.Blit(BuiltinRenderTextureType.GBuffer0, BuiltinRenderTextureType.CameraTarget);
		Shader.SetGlobalFloat(int_8, 1f);
		float_1 = QualitySettings.shadowDistance;
		QualitySettings.shadowDistance = 0f;
		SetMaterialProperties();
		commandBuffer_0.GetTemporaryRT(int_0, -1, -1);
		commandBuffer_0.SetRenderTarget(BuiltinRenderTextureType.CurrentActive, BuiltinRenderTextureType.CurrentActive);
		if (ssaa_0 != null)
		{
			ssaa_0.SetThermalVisionOn(on: true, material_0);
		}
		commandBuffer_0.Blit(renderTargetIdentifier, int_0);
		commandBuffer_0.Blit(int_0, renderTargetIdentifier, material_0, 2);
		commandBuffer_0.Blit(renderTargetIdentifier, int_0, material_0, 0);
		commandBuffer_0.Blit(int_0, renderTargetIdentifier);
		commandBuffer_0.ReleaseTemporaryRT(int_0);
		if (IsFpsStuck)
		{
			gclass980_0.UpdateTexture(commandBuffer_0, camera_0);
		}
		if (IsMotionBlurred)
		{
			gclass978_0.UpdateTexture(commandBuffer_0, camera_0, ssaa_0, renderTargetIdentifier);
		}
		if (IsGlitch)
		{
			gclass977_0.UpdateTexture(commandBuffer_0);
		}
		if (IsPixelated)
		{
			gclass979_0.UpdateTexture(commandBuffer_0);
		}
	}

	public void OnPostRender()
	{
		Shader.SetGlobalFloat(int_8, 0f);
		commandBuffer_1.Clear();
		commandBuffer_0.Clear();
		if (On)
		{
			QualitySettings.shadowDistance = float_1;
		}
	}

	public void SetMask(NightVisionComponent.EMask mask)
	{
		switch (mask)
		{
		default:
			throw new ArgumentOutOfRangeException("mask", mask, null);
		case NightVisionComponent.EMask.Thermal:
			ThermalVisionUtilities.MaskDescription.Mask = ThermalVisionUtilities.MaskDescription.ThermalMaskTexture;
			break;
		case NightVisionComponent.EMask.Anvis:
			ThermalVisionUtilities.MaskDescription.Mask = ThermalVisionUtilities.MaskDescription.AnvisMaskTexture;
			break;
		case NightVisionComponent.EMask.Binocular:
			ThermalVisionUtilities.MaskDescription.Mask = ThermalVisionUtilities.MaskDescription.BinocularMaskTexture;
			break;
		case NightVisionComponent.EMask.GasMask:
			ThermalVisionUtilities.MaskDescription.Mask = ThermalVisionUtilities.MaskDescription.GasMaskTexture;
			break;
		case NightVisionComponent.EMask.OldMonocular:
			ThermalVisionUtilities.MaskDescription.Mask = ThermalVisionUtilities.MaskDescription.OldMonocularMaskTexture;
			break;
		}
	}

	public void method_1(bool on)
	{
		On = on;
		if (on)
		{
			float_0 = chromaticAberration_0.Shift;
		}
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
		chromaticAberration_0.Shift = (on ? ChromaticAberrationThermalShift : float_0);
	}

	public void Toggle()
	{
		StartSwitch(!class687_0.On);
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

	public void SetMaterialProperties()
	{
		material_0.SetTexture(int_1, method_2());
		material_0.SetFloat(int_2, ThermalVisionUtilities.ValuesCoefs.MainTexColorCoef);
		material_0.SetFloat(int_3, ThermalVisionUtilities.ValuesCoefs.MinimumTemperatureValue);
		material_0.SetTexture(int_4, ThermalVisionUtilities.NoiseParameters.NoiseTex);
		material_0.SetFloat(int_5, IsNoisy ? ThermalVisionUtilities.NoiseParameters.NoiseIntensity : 0f);
		material_0.SetFloat(int_6, ThermalVisionUtilities.DepthFade);
		material_0.SetFloat(int_7, ThermalVisionUtilities.ValuesCoefs.RampShift);
		material_0.SetFloat(int_9, UnsharpRadiusBlur);
		material_0.SetFloat(int_10, UnsharpBias);
		if (TextureMask != null)
		{
			TextureMask.Mask = ThermalVisionUtilities.MaskDescription.Mask;
			TextureMask.Size = ThermalVisionUtilities.MaskDescription.MaskSize;
			TextureMask.ApplySettings();
		}
	}

	public Texture method_2()
	{
		int num = 0;
		RampTexPalletteConnector rampTexPalletteConnector;
		while (true)
		{
			if (num < ThermalVisionUtilities.RampTexPalletteConnectors.Count)
			{
				rampTexPalletteConnector = ThermalVisionUtilities.RampTexPalletteConnectors[num];
				if (rampTexPalletteConnector.SelectablePalette == ThermalVisionUtilities.CurrentRampPalette)
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		return rampTexPalletteConnector.Texture;
	}

	public void OnDestroy()
	{
		if (commandBuffer_0 != null)
		{
			camera_0.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer_0);
		}
		if (commandBuffer_1 != null)
		{
			camera_0.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, commandBuffer_1);
		}
		gclass980_0?.Destroy();
		gclass978_0?.Dispose();
		gclass980_0 = null;
		gclass978_0 = null;
		Shader.SetGlobalFloat(int_8, 0f);
	}

	public static float smethod_0(AnimationCurve curve)
	{
		float result = 0f;
		float num = float.MinValue;
		Keyframe[] keys = curve.keys;
		for (int i = 0; i < keys.Length; i++)
		{
			Keyframe keyframe = keys[i];
			if (keyframe.value > num)
			{
				result = keyframe.time;
				num = keyframe.value;
			}
		}
		return result;
	}

	[CompilerGenerated]
	public void method_3()
	{
		method_1(on: true);
	}

	[CompilerGenerated]
	public void method_4()
	{
		method_1(on: false);
	}
}
