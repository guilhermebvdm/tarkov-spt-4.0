using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Bsg.GameSettings;
using EFT.Settings.Graphics;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

public class GraphicsSettingsClass : GClass1081<GraphicsSettingsClass>
{
	[Serializable]
	[CompilerGenerated]
	public class Class1833
	{
		public static readonly Class1833 class1833_0 = new Class1833();

		public static Func<int, int> func_0;

		public static Func<int, int> func_1;

		public static Func<float, float> func_2;

		public static Func<int, int> func_3;

		public static Func<int, int> func_4;

		public static Func<float, float> func_5;

		public static Func<float, float> func_6;

		public static Func<ENvidiaReflexMode, ENvidiaReflexMode> func_7;

		public int method_0(int value)
		{
			return Mathf.Clamp(value, 0, 3);
		}

		public int method_1(int value)
		{
			return Mathf.Clamp(value, 0, 2);
		}

		public float method_2(float value)
		{
			return Mathf.Clamp(value, 400f, 3000f);
		}

		public int method_3(int value)
		{
			return Mathf.Clamp(value, 16, 64);
		}

		public int method_4(int value)
		{
			return Mathf.Clamp(value, 256, 4096);
		}

		public float method_5(float value)
		{
			return Mathf.Clamp(value, 2f, 4f);
		}

		public float method_6(float value)
		{
			return Mathf.Clamp(value, 0f, 5f);
		}

		public ENvidiaReflexMode method_7(ENvidiaReflexMode value)
		{
			if (!GClass3692.IsReflexAvailable())
			{
				return ENvidiaReflexMode.Off;
			}
			return value;
		}
	}

	[CompilerGenerated]
	public class Class1834
	{
		public byte displayIndex;

		public bool method_0(GClass2396 stored)
		{
			return stored.Index == displayIndex;
		}
	}

	[CompilerGenerated]
	public class Class1835
	{
		public GStruct293 displaySettings;

		public bool method_0(GClass2396 stored)
		{
			return stored.Index == displaySettings.Display;
		}
	}

	[NonSerialized]
	public const long Long_0 = 3686400L;

	[NonSerialized]
	public const long Long_1 = 8294400L;

	public readonly List<GClass2396> Stored;

	public readonly GameSetting<GStruct293> DisplaySettings;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	public readonly GameSetting<int?> GraphicsQuality;

	public readonly GameSetting<int> ShadowsQuality;

	public readonly GameSetting<int> TextureQuality;

	public readonly GameSetting<CloudsMode> CloudsQuality;

	public readonly GameSetting<bool> SDMode;

	public readonly GameSetting<bool> VSync;

	public readonly GameSetting<int> LobbyFramerate;

	public readonly GameSetting<int> GameFramerate;

	public readonly GameSetting<bool> DisableGameFramerateLimit;

	public readonly GameSetting<ESamplingMode> SuperSampling;

	public readonly GameSetting<AnisotropicFiltering> AnisotropicFiltering;

	public readonly GameSetting<float> OverallVisibility;

	public readonly GameSetting<float> LodBias;

	public readonly GameSetting<int> MipStreamingBufferSize;

	public readonly GameSetting<int> MipStreamingIOCount;

	public readonly GameSetting<ESSAOMode> Ssao;

	public readonly GameSetting<float> Sharpen;

	public readonly GameSetting<ESSRMode> SSR;

	public readonly GameSetting<EAntialiasingMode> AntiAliasing;

	public readonly GameSetting<ENvidiaReflexMode> NVidiaReflex;

	public readonly GameSetting<bool> HighQualityFog;

	public readonly GameSetting<bool> GrassShadow;

	public readonly GameSetting<bool> ChromaticAberrations;

	public readonly GameSetting<bool> Noise;

	public readonly GameSetting<bool> ZBlur;

	public readonly GameSetting<bool> HighQualityColor;

	public readonly GameSetting<bool> MipStreaming;

	public readonly GameSetting<bool> SdTarkovStreets;

	public readonly GameSetting<bool> VolumetricLight;

	public readonly GameSetting<EDLSSMode> DLSSMode;

	public readonly GameSetting<EFSR2Mode> FSR2Mode;

	public readonly GameSetting<EFSR3Mode> FSR3Mode;

	public readonly GameSetting<EDLSSPreset> DLSSPreset;

	[NonSerialized]
	[CanBeNull]
	public GameGraphicsClass GameGraphicsClass;

	[NonSerialized]
	[CompilerGenerated]
	public string String_0;

	public bool InApplyDisplaySettingsProcess
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public bool DLSSEnabled => (EDLSSMode)DLSSMode > EDLSSMode.Off;

	public bool FSR2Enabled => (EFSR2Mode)FSR2Mode > EFSR2Mode.Off;

	public bool FSR3Enabled => (EFSR3Mode)FSR3Mode > EFSR3Mode.Off;

	public float ShadowDistance => smethod_4(ShadowsQuality);

	public float SuperSamplingFactor => GetSuperSamplingFromQuality(SuperSampling);

	[JsonProperty]
	public string SystemMessage
	{
		set
		{
			SystemMessageLocale = value;
		}
	}

	[JsonIgnore]
	public string SystemMessageLocale
	{
		[CompilerGenerated]
		get
		{
			return String_0;
		}
		[CompilerGenerated]
		set
		{
			String_0 = value;
		}
	}

	public GraphicsSettingsClass([CanBeNull] GameGraphicsClass controller)
	{
		GameGraphicsClass = controller;
		Stored = new List<GClass2396>();
		DisplaySettings = method_3("Settings/Graphics/DisplaySettings", (controller != null) ? Class1826.smethod_17() : default(GStruct293), method_8);
		GraphicsQuality = method_4("Settings/Graphics/GraphicsQuality", (int?)0, (Func<int?, int?>)null);
		ShadowsQuality = method_2("Settings/Graphics/ShadowsQuality", 0, (int value) => Mathf.Clamp(value, 0, 3));
		TextureQuality = method_2("Settings/Graphics/TextureQuality", 0, (int value) => Mathf.Clamp(value, 0, 2));
		LobbyFramerate = method_2("Settings/Graphics/LobbyFramerate", 60);
		GameFramerate = method_2("Settings/Graphics/GameFramerate", 120);
		SuperSampling = method_4("Settings/Graphics/SuperSampling", ESamplingMode.Off);
		AnisotropicFiltering = method_4("Settings/Graphics/AnisotropicFiltering", UnityEngine.AnisotropicFiltering.Disable);
		OverallVisibility = method_2("Settings/Graphics/OverallVisibility", 400f, (float value) => Mathf.Clamp(value, 400f, 3000f));
		CloudsQuality = method_4("Settings/Graphics/CloudsQuality", CloudsMode.Low);
		MipStreamingBufferSize = method_2("Settings/Graphics/MipStreamingBufferSize", 32, (int value) => Mathf.Clamp(value, 16, 64));
		MipStreamingIOCount = method_2("Settings/Graphics/MipStreamingIOCount", 512, (int value) => Mathf.Clamp(value, 256, 4096));
		LodBias = method_2("Settings/Graphics/ObjectLodQuality", 2f, (float value) => Mathf.Clamp(value, 2f, 4f));
		Ssao = method_4("Settings/Graphics/Ssao", ESSAOMode.Off);
		Sharpen = method_2("Settings/Graphics/Sharpen", 0f, (float value) => Mathf.Clamp(value, 0f, 5f));
		SSR = method_4("Settings/Graphics/SSR", ESSRMode.Off);
		AntiAliasing = method_4("Settings/Graphics/AntiAliasing", EAntialiasingMode.None);
		NVidiaReflex = method_4("Settings/Graphics/NVidiaReflex", ENvidiaReflexMode.Off, (ENvidiaReflexMode value) => GClass3692.IsReflexAvailable() ? value : ENvidiaReflexMode.Off);
		VSync = method_2("Settings/Graphics/VSync", defaultValue: false);
		GrassShadow = method_2("Settings/Graphics/GrassShadow", defaultValue: false);
		ChromaticAberrations = method_2("Settings/Graphics/ChromaticAberrations", defaultValue: false);
		Noise = method_2("Settings/Graphics/Noise", defaultValue: false);
		ZBlur = method_2("Settings/Graphics/ZBlur", defaultValue: false);
		HighQualityFog = method_2("Settings/Graphics/HighQualityFog", defaultValue: false);
		HighQualityColor = method_2("Settings/Graphics/HighQualityColor", defaultValue: true);
		MipStreaming = method_2("Settings/Graphics/MipStreaming", defaultValue: false);
		SdTarkovStreets = method_2("Settings/Graphics/SdStreetsOfTarkov", defaultValue: false);
		VolumetricLight = method_2("Settings/Graphics/VolumetricLight", defaultValue: true);
		DisableGameFramerateLimit = method_2("Settings/Graphics/DisableGameFramerateLimit", defaultValue: false);
		DLSSMode = method_4("Settings/Graphics/DLSSMode", EDLSSMode.Off, method_12);
		FSR2Mode = method_4("Settings/Graphics/FSR2Mode", EFSR2Mode.Off, method_13);
		FSR3Mode = method_4("Settings/Graphics/FSR3Mode", EFSR3Mode.Off, method_14);
		DLSSPreset = method_4("Settings/Graphics/DLSSPreset", EDLSSPreset.Default);
	}

	public Task<GStruct293> method_8(GStruct293 displaySettings)
	{
		GameGraphicsClass gameGraphicsClass = GameGraphicsClass;
		if (gameGraphicsClass != null && gameGraphicsClass.IsBound)
		{
			InApplyDisplaySettingsProcess = true;
			return method_9(displaySettings);
		}
		return Task.FromResult(displaySettings);
	}

	public async Task<GStruct293> method_9(GStruct293 displaySettings)
	{
		if (GameGraphicsClass != null)
		{
			displaySettings = await GameGraphicsClass.ApplyDisplaySettings(displaySettings);
		}
		InApplyDisplaySettingsProcess = false;
		method_11(displaySettings);
		return displaySettings;
	}

	public GStruct293 GetStoredDisplaySettings(byte displayIndex)
	{
		GStruct293 displaySettings = DisplaySettings.Value;
		if (displaySettings.Display == displayIndex)
		{
			return displaySettings;
		}
		displaySettings.Display = displayIndex;
		method_10(ref displaySettings);
		return displaySettings;
	}

	public GStruct293 GetStoredDisplaySettings(FullScreenMode mode)
	{
		GStruct293 displaySettings = DisplaySettings.Value;
		if (displaySettings.FullScreenMode == mode)
		{
			return displaySettings;
		}
		displaySettings.FullScreenMode = mode;
		method_10(ref displaySettings);
		return displaySettings;
	}

	public void method_10(ref GStruct293 displaySettings)
	{
		byte displayIndex = displaySettings.Display;
		Stored.FirstOrDefault((GClass2396 stored) => stored.Index == displayIndex)?.ApplyTo(ref displaySettings);
		smethod_0(ref displaySettings);
	}

	public static void smethod_0(ref GStruct293 displaySettings)
	{
		if (!Class1826.smethod_16(displaySettings.Display, out var displayInfo))
		{
			Debug.LogError($"Cannot find display info for {displaySettings.Display}");
			return;
		}
		if (displaySettings.Resolution.Width > displayInfo.width)
		{
			displaySettings.Resolution.Width = displayInfo.width;
		}
		if (displaySettings.Resolution.Height > displayInfo.height)
		{
			displaySettings.Resolution.Height = displayInfo.height;
		}
	}

	public void method_11(GStruct293 displaySettings)
	{
		GClass2396 gClass = Stored.FirstOrDefault((GClass2396 stored) => stored.Index == displaySettings.Display);
		if (gClass == null)
		{
			gClass = GClass2396.Create(in displaySettings);
			Stored.Add(gClass);
		}
		else
		{
			gClass.Update(in displaySettings);
		}
	}

	public static int TextureQualityToMipBias(int quality)
	{
		return Mathf.Clamp(2 - quality + (GameGraphicsClass.ApplySDModeOnRuntime ? 1 : 0), 0, 2);
	}

	public float GetMipStreamingBias()
	{
		if (!MipStreaming.Value)
		{
			return 0f;
		}
		return TextureQualityToMipBias(TextureQuality);
	}

	public bool IsDLSSModeAllowed(EDLSSMode mode)
	{
		return mode == smethod_1(mode, DisplaySettings.Value.Resolution);
	}

	public EDLSSMode method_12(EDLSSMode mode)
	{
		return smethod_1(mode, DisplaySettings.Value.Resolution);
	}

	public static EDLSSMode smethod_1(EDLSSMode mode, EftResolution resolution)
	{
		if (!DLSSWrapper.IsDLSSSupported())
		{
			return EDLSSMode.Off;
		}
		long num = (long)resolution.Width * (long)resolution.Height;
		if (mode > EDLSSMode.Balanced && num < 3686400L)
		{
			return EDLSSMode.Balanced;
		}
		if (mode >= EDLSSMode.Performance && num < 8294400L)
		{
			return EDLSSMode.Performance;
		}
		return mode;
	}

	public bool IsFSR2ModeAllowed(EFSR2Mode mode)
	{
		return mode == smethod_2(mode, DisplaySettings.Value.Resolution);
	}

	public bool IsFSR3ModeAllowed(EFSR3Mode mode)
	{
		return mode == smethod_3(mode, DisplaySettings.Value.Resolution);
	}

	public EFSR2Mode method_13(EFSR2Mode mode)
	{
		return smethod_2(mode, DisplaySettings.Value.Resolution);
	}

	public EFSR3Mode method_14(EFSR3Mode mode)
	{
		return smethod_3(mode, DisplaySettings.Value.Resolution);
	}

	public static EFSR2Mode smethod_2(EFSR2Mode mode, EftResolution resolution)
	{
		return mode;
	}

	public static EFSR3Mode smethod_3(EFSR3Mode mode, EftResolution resolution)
	{
		return mode;
	}

	public static float smethod_4(int shadowQuality)
	{
		return shadowQuality switch
		{
			0 => 30f, 
			1 => 40f, 
			2 => 60f, 
			3 => 80f, 
			_ => 40f, 
		};
	}

	public static float GetSuperSamplingFromQuality(ESamplingMode quality)
	{
		return quality switch
		{
			ESamplingMode.DownX05 => Mathf.Sqrt(0.5f), 
			ESamplingMode.DownX075 => Mathf.Sqrt(0.75f), 
			ESamplingMode.SuperX2 => Mathf.Sqrt(2f), 
			ESamplingMode.SuperX4 => 2f, 
			_ => 1f, 
		};
	}

	public static bool Is1XSampling(ESamplingMode samplingQuality)
	{
		return GClass855.IsZero(GetSuperSamplingFromQuality(samplingQuality) - 1f);
	}

	public static int GetDLSSQuality(EDLSSMode mode)
	{
		switch (mode)
		{
		default:
			throw new ArgumentOutOfRangeException();
		case EDLSSMode.Quality:
			return 2;
		case EDLSSMode.Balanced:
			return 1;
		case EDLSSMode.Off:
		case EDLSSMode.Performance:
			return 0;
		case EDLSSMode.UltraPerformance:
			return 3;
		}
	}

	public bool IsTaaEnabled()
	{
		if ((EAntialiasingMode)AntiAliasing != EAntialiasingMode.TAA_Low)
		{
			return IsTaaHigh();
		}
		return true;
	}

	public bool IsTaaHigh()
	{
		return (EAntialiasingMode)AntiAliasing == EAntialiasingMode.TAA_High;
	}

	public bool IsDLSSEnabled()
	{
		return (EDLSSMode)DLSSMode > EDLSSMode.Off;
	}

	public bool IsFSR2Enabled()
	{
		return (EFSR2Mode)FSR2Mode > EFSR2Mode.Off;
	}

	public bool IsFSR3Enabled()
	{
		return (EFSR3Mode)FSR3Mode > EFSR3Mode.Off;
	}

	public override GraphicsSettingsClass Clone()
	{
		GraphicsSettingsClass graphicsSettingsClass = new GraphicsSettingsClass(null);
		graphicsSettingsClass.TakeSettingsFrom(this);
		return graphicsSettingsClass;
	}
}
