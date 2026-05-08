using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public abstract class GClass872
{
	[NonSerialized]
	public static Dictionary<string, Shader> Dictionary_0;

	[NonSerialized]
	public static bool Bool_0;

	static GClass872()
	{
		Dictionary_0 = new Dictionary<string, Shader>
		{
			{ "Custom/Additive_Icon", null },
			{ "Custom/OpticGlass_Icon", null },
			{ "Custom/XRayVisionObject", null },
			{ "CW FX/Collimator", null },
			{ "CW FX/Collimator_Icon", null },
			{ "CW FX/OpticSight_Icon", null },
			{ "CW FX/BackLens_Icon", null },
			{ "EFT/CollimatorGlass_Icon", null },
			{ "EFT/Glass_Icon", null },
			{ "Hidden/BilateralBlur", null },
			{ "Hidden/Black", null },
			{ "Hidden/BlitAdd", null },
			{ "Hidden/Blur", null },
			{ "Hidden/ClipAll", null },
			{ "Hidden/ColorBlend", null },
			{ "Hidden/CustomDepthSSRR", null },
			{ "Hidden/CustomDepthSSRRV3", null },
			{ "Hidden/FastBlur", null },
			{ "Hidden/GausFast", null },
			{ "Hidden/GausFastV3", null },
			{ "Hidden/GetChannel", null },
			{ "Hidden/GPUGraph", null },
			{ "Hidden/HotObject", null },
			{ "Hidden/Image Effects/Cinematic/AmbientOcclusion", null },
			{ "Hidden/Lerp", null },
			{ "Hidden/LumaComp", null },
			{ "Hidden/LumaCompV3", null },
			{ "Hidden/MapCreatorShader", null },
			{ "Hidden/Mirza Beig/Image Effects/Sharpen", null },
			{ "Hidden/NoiseAndGrainDX11", null },
			{ "Hidden/ScopeMask", null },
			{ "Hidden/Outline", null },
			{ "Hidden/PostEffects/ContactShadows", null },
			{ "Hidden/PreviewTexShader", null },
			{ "Hidden/PrismAmbientObscurance", null },
			{ "Hidden/PrismEffects", null },
			{ "Hidden/PrismEffectsSecondary", null },
			{ "Hidden/PrismEffectsTertiary", null },
			{ "Hidden/PrismKinoObscurance", null },
			{ "Hidden/SESSAO", null },
			{ "Hidden/ShowDepth", null },
			{ "Hidden/SSAOMask", null },
			{ "Hidden/TerrainEngine/Splatmap/Diffuse-AddPass", null },
			{ "Hidden/TerrainEngine/Splatmap/Lightmap-AddPass", null },
			{ "Hidden/TerrainEngine/Splatmap/Lightmap-FirstPass", null },
			{ "Hidden/ThermalMask", null },
			{ "Hidden/ThermalVision", null },
			{ "Hidden/Time of Day/God Rays", null },
			{ "Hidden/Time of Day/Scattering", null },
			{ "Hidden/Time of Day/Screen Clear", null },
			{ "Hidden/Toksvig", null },
			{ "Hidden/Toksvig3", null },
			{ "Hidden/Transform", null },
			{ "Hidden/Tree Billboard LOD", null },
			{ "Hidden/UBER_CopyPropsTexture", null },
			{ "Hidden/Ultimate/BloomCombine", null },
			{ "Hidden/Ultimate/BloomCombineFlareDirt", null },
			{ "Hidden/Ultimate/BloomMixer", null },
			{ "Hidden/Ultimate/BokehMisc", null },
			{ "Hidden/Ultimate/BokehTexture", null },
			{ "Hidden/Ultimate/BrightpassMask", null },
			{ "Hidden/Ultimate/FlareDouble", null },
			{ "Hidden/Ultimate/FlareMask", null },
			{ "Hidden/Ultimate/FlareMesh", null },
			{ "Hidden/Ultimate/FlareSingle", null },
			{ "Hidden/Ultimate/Sampling", null },
			{ "Hidden/Vertex Data", null },
			{ "Nature/Terrain/Diffuse", null },
			{ "p0/Bumped Specular_Icon", null },
			{ "p0/IconShadow", null },
			{ "p0/IconShadowBlit", null },
			{ "p0/OpticDeferredLinza_Icon", null },
			{ "p0/Pain", null },
			{ "p0/Reflective/Bumped Specular Mask_Icon", null },
			{ "p0/Reflective/Bumped Specular SMap_Decal_Icon", null },
			{ "p0/Reflective/Bumped Specular SMap_Icon", null },
			{ "p0/Reflective/Bumped Specular_Icon", null },
			{ "p0/Reflective/Specular_Icon", null },
			{ "p0/Transparent/Reflective/Specular", null },
			{ "Parallax Specular", null },
			{ "Particles/Additive", null },
			{ "Particles/Alpha Blended", null },
			{ "Rain/Bumped Specular SMap Wet", null },
			{ "Sandbox/VolumetricLight", null },
			{ "shadow", null },
			{ "Standard (Specular setup)_Icon", null },
			{ "Standard", null },
			{ "Transparent/DepthZwrite_Icon", null },
			{ "Transparent/DepthZwriteDithered_Icon", null },
			{ "UI Extensions/SoftMask", null },
			{ "p0/Transparent/Reflective/Specular_Icon", null },
			{ "Legacy Shaders/Transparent/Cutout/Bumped Specular_Icon", null },
			{ "Custom/Glass2_Icon", null },
			{ "RainyShaders/Reflect Bumped Specular Alpha Distort Rainy_Icon", null },
			{ "p0/Reflective/Bumped Emissive Specular SMap_Icon", null },
			{ "p0/Reflective/Bumped Animated Emissive Specular SMap_Icon", null },
			{ "Unlit/Color2_Icon", null },
			{ "p0/Cutout/Bumped Diffuse_Icon", null },
			{ "p0/Reflective/Bumped Specular_Decal_Icon", null },
			{ "Unlit/DrawNothing", null },
			{ "Unlit/HoboCullingDebugVision", null },
			{ "Hidden/OpticCullingMask", null },
			{ "Hidden/BicubicSampling", null },
			{ "VFX/Hologram_Icon", null },
			{ "Custom/Matcap_Icon", null }
		};
	}

	public static async Task InitShadersDictionary()
	{
		string[] array = Dictionary_0.Keys.ToArray();
		string[] array2 = array;
		foreach (string text in array2)
		{
			Dictionary_0[text] = Shader.Find(text);
			await Task.Yield();
		}
		Bool_0 = true;
	}

	public static Shader Find(string name)
	{
		if (!Application.isPlaying)
		{
			return Shader.Find(name);
		}
		if (Dictionary_0.TryGetValue(name, out var value))
		{
			if (!Bool_0 && value == null)
			{
				value = Shader.Find(name);
				Dictionary_0[name] = value;
			}
			return value;
		}
		Debug.LogWarningFormat("<color=red>Unregistered shader: {0}</color>", name);
		return Shader.Find(name);
	}
}
