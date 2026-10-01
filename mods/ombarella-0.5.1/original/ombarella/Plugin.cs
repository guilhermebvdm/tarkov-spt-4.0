using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using EFT;
using SPT.Reflection.Patching;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace ombarella;

[BepInPlugin("Ombarella", "Ombarella", "0.5.1")]
public class Plugin : BaseUnityPlugin
{
	private enum LightMeterRenderPass
	{
		Luma,
		ColorBreadth
	}

	private class AsyncScoreBatch
	{
		public int Id;

		public int Pending;

		public int ScoreCount;

		public float ScoreSum;

		public ScoreSettings Settings;
	}

	private class AsyncScoreSample
	{
		public AsyncScoreBatch Batch;

		public int Pending;

		public bool LumaPassFailed;

		public bool ColorPassFailed;

		public RenderStats LumaStats;

		public RenderStats ColorStats;
	}

	private struct ScoreSettings
	{
		public float LumaCoef;

		public float RedLumaMulti;

		public float GreenLumaMulti;

		public float BlueLumaMulti;

		public float RedColorBreadthMulti;

		public float GreenColorBreadthMulti;

		public float BlueColorBreadthMulti;

		public float LumaColorDistribution;
	}

	private class RenderStats
	{
		public int SampleCount;

		public float RLow = 255f;

		public float GLow = 255f;

		public float BLow = 255f;

		public float RHigh;

		public float GHigh;

		public float BHigh;

		public float RLumaSum;

		public float GLumaSum;

		public float BLumaSum;

		public float Luma;

		public float ColorBreadth;
	}

	private sealed class MagnifiedOpticRenderScope : IDisposable
	{
		private struct RendererState
		{
			public Renderer Renderer;

			public bool ForceRenderingOff;
		}

		private readonly List<RendererState> _rendererStates = new List<RendererState>();

		private bool _disposed;

		public HashSet<Renderer> HiddenRenderers { get; private set; }

		public MagnifiedOpticRenderScope(HashSet<Renderer> renderers)
		{
			HiddenRenderers = renderers ?? new HashSet<Renderer>();
			foreach (Renderer hiddenRenderer in HiddenRenderers)
			{
				if (!((Object)(object)hiddenRenderer == (Object)null))
				{
					_rendererStates.Add(new RendererState
					{
						Renderer = hiddenRenderer,
						ForceRenderingOff = hiddenRenderer.forceRenderingOff
					});
					hiddenRenderer.forceRenderingOff = true;
				}
			}
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}
			for (int num = _rendererStates.Count - 1; num >= 0; num--)
			{
				RendererState rendererState = _rendererStates[num];
				if ((Object)(object)rendererState.Renderer != (Object)null)
				{
					rendererState.Renderer.forceRenderingOff = rendererState.ForceRenderingOff;
				}
			}
			_disposed = true;
		}
	}

	private sealed class FirstPersonBodyRenderScope : IDisposable
	{
		private struct RendererState
		{
			public Renderer Renderer;

			public ShadowCastingMode ShadowCastingMode;
		}

		private readonly List<RendererState> _rendererStates = new List<RendererState>();

		private bool _disposed;

		public FirstPersonBodyRenderScope(List<Renderer> renderers)
		{
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			if (renderers == null)
			{
				return;
			}
			for (int i = 0; i < renderers.Count; i++)
			{
				Renderer val = renderers[i];
				if (!((Object)(object)val == (Object)null))
				{
					_rendererStates.Add(new RendererState
					{
						Renderer = val,
						ShadowCastingMode = val.shadowCastingMode
					});
					val.shadowCastingMode = (ShadowCastingMode)1;
				}
			}
		}

		public void Dispose()
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			if (_disposed)
			{
				return;
			}
			for (int num = _rendererStates.Count - 1; num >= 0; num--)
			{
				RendererState rendererState = _rendererStates[num];
				if ((Object)(object)rendererState.Renderer != (Object)null)
				{
					rendererState.Renderer.shadowCastingMode = rendererState.ShadowCastingMode;
				}
			}
			_disposed = true;
		}
	}

	public static Plugin Instance;

	private const string modGUID = "Ombarella";

	private const string modName = "Ombarella";

	private const string modVersion = "0.5.1";

	private const string ConfigSectionGeneral = "a - General";

	private const string ConfigSectionCamera = "b - Camera";

	private const string ConfigSectionLuma = "c - Luma";

	private const string ConfigSectionColor = "d - Color";

	private const string ConfigSectionBlend = "e - Luma + Color Blend";

	private const string ConfigSectionDebug = "f - Debug";

	private Player _player;

	private Camera _lightCam;

	private Light _colorRenderFillLight;

	private RenderTexture _rt;

	private RenderTexture _colorRt;

	private int _texSize = 128;

	private bool _isDestroyed;

	private bool _asyncReadbackPending;

	private bool _asyncScoreReady;

	private bool _hasValidMeterSample;

	private bool _syncFallbackWarningLogged;

	private float _asyncScore = 0.01f;

	private int _asyncBatchId;

	private readonly Queue<float> _meterSamples = new Queue<float>();

	private float _meterSampleSum;

	public static ConfigEntry<bool> MeterViz;

	public static ConfigEntry<bool> MasterSwitch;

	public static ConfigEntry<bool> UseFikaPlayerAveraging;

	public static ConfigEntry<float> MeterAttenuationCoef;

	public static ConfigEntry<float> SamplesPerSec;

	public static ConfigEntry<float> MeterAverageSamples;

	public static ConfigEntry<float> AimNerf;

	public static ConfigEntry<float> CameraFOV;

	public static ConfigEntry<float> LumaCoef;

	public static ConfigEntry<float> RenderTextureResolution;

	public static ConfigEntry<bool> UseAsyncGPUReadback;

	public static ConfigEntry<float> RedLumaMulti;

	public static ConfigEntry<float> GreenLumaMulti;

	public static ConfigEntry<float> BlueLumaMulti;

	public static ConfigEntry<float> RedColorBreadthMulti;

	public static ConfigEntry<float> GreenColorBreadthMulti;

	public static ConfigEntry<float> BlueColorBreadthMulti;

	public static ConfigEntry<float> ColorRenderFillIntensity;

	public static ConfigEntry<float> LumaColorDistribution;

	public static ConfigEntry<float> CameraFocusHeightOffset;

	public static ConfigEntry<bool> UseOrbitCameraSampling;

	public static ConfigEntry<float> OrbitCameraRadius;

	public static ConfigEntry<float> OrbitCameraHeightOffset;

	public static ConfigEntry<bool> RejectOccludedSamples;

	public static ConfigEntry<float> DebugUpdateFreq;

	public static ConfigEntry<bool> IsDebug;

	public static ConfigEntry<bool> ShowRenderTexturePreview;

	public static ConfigEntry<float> RenderTexturePreviewSize;

	public static ConfigEntry<bool> UseFixedOrbitAngle;

	public static ConfigEntry<float> FixedOrbitAngle;

	private float updateTimer;

	private float debugScore;

	private float debugLumaScore;

	private float debugScore2;

	private ComputeShader _computeShader;

	private bool _computeShaderReady;

	private ComputeBuffer outputBuffer;

	private Color[] outputColors;

	private int _handleMain;

	private float _avgLightMeter = 1f;

	public float FinalLightMeter = 1f;

	private float _finalValueLerped = 1f;

	private GUIStyle efficiencyIndicatorStyle = new GUIStyle();

	public bool IsRaid { get; set; }

	private void Awake()
	{
		Instance = this;
		Initialize();
	}

	private void OnDestroy()
	{
		_isDestroyed = true;
		ReleaseRenderResources();
		DestroyColorRenderFillLight();
	}

	private void Initialize()
	{
		Utils.Logger = ((BaseUnityPlugin)this).Logger;
		LoadConfig();
		LoadPatches();
		PopulateShader();
		SetupRenderTexture();
	}

	private void LoadPatches()
	{
		TryLoadPatch((ModulePatch)(object)new Patch_VisionSpeed());
		TryLoadPatch((ModulePatch)(object)new Patch_AimOffset());
	}

	private void TryLoadPatch(ModulePatch patch)
	{
		try
		{
			patch.Enable();
		}
		catch (Exception arg2)
		{
			string arg = ((object)patch).ToString();
			((BaseUnityPlugin)this).Logger.LogError((object)$"Failed to load patch {arg}: {arg2}");
			throw;
		}
	}

	private void LoadConfig()
	{
		MasterSwitch = ConstructBoolConfig(true, "a - General", "1-Master switch", "Toggle all mod functions on/off", OldConfig("a - Toggles", "Master Switch"));
		MeterViz = ConstructBoolConfig(false, "a - General", "2-Enable light meter indicator", "Visual representation of how much you are being lit and how visible you are", OldConfig("a - Toggles", "Enable light meter indicator"));
		UseFikaPlayerAveraging = ConstructBoolConfig(false, "a - General", "3-Use Fika player averaging", "When enabled, target all real non-headless Fika client players and average each player's visibility from their nearest bot. Safe to leave disabled when Fika is not installed.", OldConfig("a - Toggles", "Use Fika player averaging"));
		SamplesPerSec = ConstructFloatConfig(20f, "a - General", "4-Light samples per second", "Main throttle of the mod; higher = more accurate reading / less perf", 1f, 60f, OldConfig("b - Main Settings", "1-Light samples per second"));
		MeterAverageSamples = ConstructFloatConfig(5f, "a - General", "5-Light meter average samples", "Number of valid light samples to average before applying the result. Higher values smooth noisy orbit sampling.", 1f, 600f, OldConfig("b - Main Settings", "2-Light meter average samples"));
		MeterAttenuationCoef = ConstructFloatConfig(1f, "a - General", "6-Light meter strength", "Determines how quickly bots can spot you per your visiblity level (100% = bots get full effect, slower recognition time)", 0f, 1f, OldConfig("b - Main Settings", "3-Light meter strength"));
		AimNerf = ConstructFloatConfig(0.03f, "a - General", "7-Bot aim handicap", "Determines how much bots' aim is affected by your visibility level (higher = bots' aim more nerfed by your viz level; zero = effect is removed", 0f, 0.1f, OldConfig("b - Main Settings", "4-Bot aim handicap"));
		UseAsyncGPUReadback = ConstructBoolConfig(true, "a - General", "8-Use async GPU readback", "Avoids blocking the main thread while reading the light camera texture. Disable to use the old synchronous compute readback path.", OldConfig("c - Advanced Settings", "Use async GPU readback"));
		CameraFOV = ConstructFloatConfig(30f, "b - Camera", "1-Camera FOV", "Size of light camera FOV", 10f, 170f, OldConfig("c - Advanced Settings", "CameraFOV"));
		RenderTextureResolution = ConstructFloatConfig(32f, "b - Camera", "2-Render texture resolution", "Resolution of each light camera render texture. Applied before raid start and rounded to the nearest multiple of 8.", 16f, 512f, OldConfig("c - Advanced Settings", "Render texture resolution"));
		CameraFocusHeightOffset = ConstructFloatConfig(-0.2f, "b - Camera", "3-Camera focus height offset", "Vertical offset from the player's ribcage bone. Negative values focus lower on the chest.", -1f, 1f, OldConfig("e - Camera Rig Settings", "Camera focus height offset"));
		UseOrbitCameraSampling = ConstructBoolConfig(true, "b - Camera", "4-Use orbit camera sampling", "Samples real human players from a random orbit around the chest instead of sampling from the nearest bot position", OldConfig("e - Camera Rig Settings", "Use orbit camera sampling"), OldConfig("b - Camera", "5-Use orbit camera sampling"));
		OrbitCameraRadius = ConstructFloatConfig(2.5f, "b - Camera", "5-Orbit camera radius", "Distance from the player's chest when orbit camera sampling is enabled", 0.25f, 12f, OldConfig("e - Camera Rig Settings", "Orbit camera radius"), OldConfig("b - Camera", "6-Orbit camera radius"));
		OrbitCameraHeightOffset = ConstructFloatConfig(1.5f, "b - Camera", "6-Orbit camera height offset", "Vertical offset above the player's chest when orbit camera sampling is enabled", -1f, 4f, OldConfig("e - Camera Rig Settings", "Orbit camera height offset"), OldConfig("b - Camera", "7-Orbit camera height offset"));
		RejectOccludedSamples = ConstructBoolConfig(true, "b - Camera", "7-Reject occluded samples", "Skips a light camera sample when world geometry blocks the ray from the light camera to the player's chest", OldConfig("e - Camera Rig Settings", "Reject occluded samples"), OldConfig("b - Camera", "8-Reject occluded samples"));
		LumaCoef = ConstructFloatConfig(20f, "c - Luma", "1-Luma coefficient", "Multiplies the luma result", 1f, 20f, OldConfig("c - Advanced Settings", "Luma coefficient"));
		RedLumaMulti = ConstructFloatConfig(0.5f, "c - Luma", "2-Red luma multi", "Red color in pixel analysis is multiplied by this to produce the luma calculation", 0f, 1f, OldConfig("d - Color Settings", "1-Red luma multi"));
		GreenLumaMulti = ConstructFloatConfig(0.75f, "c - Luma", "3-Green luma multi", "Green color in pixel analysis is multiplied by this to produce the luma calculation", 0f, 1f, OldConfig("d - Color Settings", "2-Green luma multi"));
		BlueLumaMulti = ConstructFloatConfig(1f, "c - Luma", "4-Blue luma multi", "Blue color in pixel analysis is multiplied by this to produce the luma calculation", 0f, 1f, OldConfig("d - Color Settings", "3-Blue luma multi"));
		RedColorBreadthMulti = ConstructFloatConfig(1f, "d - Color", "1-Red color breadth multi", "Red channel range in the color render is multiplied by this for the color breadth score", 0f, 1f, OldConfig("d - Color Settings", "4-Red color depth multi"), OldConfig("d - Color Settings", "4-Red breadth multi"));
		GreenColorBreadthMulti = ConstructFloatConfig(0.4f, "d - Color", "2-Green color breadth multi", "Green channel range in the color render is multiplied by this for the color breadth score", 0f, 1f, OldConfig("d - Color Settings", "5-Green color depth multi"), OldConfig("d - Color Settings", "5-Green breadth multi"));
		BlueColorBreadthMulti = ConstructFloatConfig(1f, "d - Color", "3-Blue color breadth multi", "Blue channel range in the color render is multiplied by this for the color breadth score", 0f, 1f, OldConfig("d - Color Settings", "6-Blue color depth multi"), OldConfig("d - Color Settings", "6-Blue breadth multi"));
		ColorRenderFillIntensity = ConstructFloatConfig(0.5f, "d - Color", "4-Color render fill intensity", "Temporary camera-aligned fill light intensity used only for the color-breadth render. Luma rendering is not filled.", 0f, 8f, OldConfig("d - Color", "1-Player profile fill intensity"), OldConfig("d - Color", "2-Environment profile fill intensity"), OldConfig("c - Advanced Settings", "Player profile fill intensity"), OldConfig("c - Advanced Settings", "Environment profile fill intensity"));
		LumaColorDistribution = ConstructFloatConfig(GetMigratedLumaColorDistribution(0.5f), "e - Luma + Color Blend", "1-Luma color distribution", "Normalized final score distribution. Zero is luma only; one is color breadth only.", 0f, 1f);
		IsDebug = ConstructBoolConfig(false, "f - Debug", "1-Enable debug logging", "", OldConfig("y - Debug", "1) Enable debug logging"));
		DebugUpdateFreq = ConstructFloatConfig(1f, "f - Debug", "2-Debug updates per second", "How frequently the debug logger updates per second", 1f, 10f, OldConfig("y - Debug", "2) Debug updates per second"));
		ShowRenderTexturePreview = ConstructBoolConfig(false, "f - Debug", "3-Show render texture preview", "Draws the light-meter render texture in the game window for debugging", OldConfig("y - Debug", "3) Show render texture preview"));
		RenderTexturePreviewSize = ConstructFloatConfig(256f, "f - Debug", "4-Render texture preview size", "Size of the render texture debug preview in pixels", 64f, 512f, OldConfig("y - Debug", "4) Render texture preview size"));
		UseFixedOrbitAngle = ConstructBoolConfig(false, "f - Debug", "5-Use fixed orbit angle", "Uses the configured orbit angle instead of a random orbit angle for the actual light-meter sample", OldConfig("y - Debug", "5) Use fixed orbit angle"));
		FixedOrbitAngle = ConstructFloatConfig(0f, "f - Debug", "6-Fixed orbit angle", "Camera angle around the sampled player's chest when fixed orbit sampling is enabled", 0f, 360f, OldConfig("y - Debug", "6) Fixed orbit angle"));
		RemoveObsoleteConfigEntries();
	}

	private void RemoveObsoleteConfigEntries()
	{
		if ((0u | (RemoveObsoleteConfigEntries("a - Toggles", "Master Switch", "Enable light meter indicator", "Use Fika player averaging", "Use Luma meter") ? 1u : 0u) | (RemoveObsoleteConfigEntries("b - Main Settings", "1-Light samples per second", "2-Light meter average samples", "3-Light meter strength", "4-Bot aim handicap") ? 1u : 0u) | (RemoveObsoleteConfigEntries("c - Advanced Settings", "CameraFOV", "Luma coefficient", "Render texture resolution", "Player profile fill intensity", "Environment profile fill intensity", "Use async GPU readback", "Ignore transparent pixels", "Analysis exposure multiplier", "Player render fill intensity", "Environment render fill intensity", "Use light camera fill light", "Light camera fill intensity", "Exclude sky from render texture") ? 1u : 0u) | (RemoveObsoleteConfigEntries("d - Color Settings", "1-Red luma multi", "2-Green luma multi", "3-Blue luma multi", "4-Color profile minimum visibility", "5-Color profile mismatch power", "6-Color profile influence", "4-Red breadth multi", "5-Green breadth multi", "6-Blue breadth multi", "4-Red color depth multi", "5-Green color depth multi", "6-Blue color depth multi") ? 1u : 0u) | (RemoveObsoleteConfigEntries("d - Color", "1-Player profile fill intensity", "2-Environment profile fill intensity", "3-Ignore transparent pixels") ? 1u : 0u) | (RemoveObsoleteConfigEntries("e - Luma + Color Blend", "1-Color profile minimum visibility", "2-Color profile mismatch power", "3-Color profile influence", "1-Luma blend weight", "2-Color breadth blend weight") ? 1u : 0u) | (RemoveObsoleteConfigEntries("e - Camera Rig Settings", "Camera horizontal offset", "Camera focus height offset", "Force target player renderers", "Use orbit camera sampling", "Orbit camera radius", "Orbit camera height offset", "Reject occluded samples", "Exclude optic renderers", "Render player only", "Exclude in-hands item renderers") ? 1u : 0u) | (RemoveObsoleteConfigEntries("b - Camera", "4-Camera horizontal offset", "5-Use orbit camera sampling", "6-Orbit camera radius", "7-Orbit camera height offset", "8-Reject occluded samples", "9-Force target player renderers", "10-Exclude optic renderers") ? 1u : 0u) | (RemoveObsoleteConfigEntries("y - Debug", "1) Enable debug logging", "2) Debug updates per second", "3) Show render texture preview", "4) Render texture preview size", "5) Use fixed orbit angle", "6) Fixed orbit angle") ? 1u : 0u) | (RemoveObsoleteConfigEntry("z - Dev", "dev1") ? 1u : 0u) | (RemoveObsoleteConfigEntry("z - Dev", "dev2") ? 1u : 0u)) != 0)
		{
			((BaseUnityPlugin)this).Config.Save();
		}
	}

	private bool RemoveObsoleteConfigEntries(string section, params string[] keys)
	{
		bool flag = false;
		if (keys == null)
		{
			return false;
		}
		for (int i = 0; i < keys.Length; i++)
		{
			flag |= RemoveObsoleteConfigEntry(section, keys[i]);
		}
		return flag;
	}

	private bool RemoveObsoleteConfigEntry(string section, string key)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		ConfigDefinition val = new ConfigDefinition(section, key);
		if (!((BaseUnityPlugin)this).Config.ContainsKey(val))
		{
			return false;
		}
		((BaseUnityPlugin)this).Config.Remove(val);
		return true;
	}

	private void Update()
	{
		if (MasterSwitch == null || !MasterSwitch.Value)
		{
			return;
		}
		PluginManager.Update();
		if (!IsRaid)
		{
			return;
		}
		if ((Object)(object)_player == (Object)null)
		{
			_player = Utils.GetMainPlayer();
		}
		if ((Object)(object)_player == (Object)null && !UseFikaPlayerAveraging.Value && !UseOrbitCameraSampling.Value)
		{
			Utils.LogError("Unable to return player, meter updates aborted");
			return;
		}
		updateTimer += Time.deltaTime;
		if (updateTimer > 1f / SamplesPerSec.Value)
		{
			updateTimer = 0f;
			UpdateLightMeter();
		}
	}

	public void CleanupRaid()
	{
		_player = null;
		ResetMeterAverage();
		_asyncReadbackPending = false;
		_asyncScoreReady = false;
		IsRaid = false;
	}

	public void StartRaid()
	{
		_player = Utils.GetMainPlayer();
		ResetMeterAverage();
		SetupRenderTexture();
		IsRaid = true;
	}

	private void UpdateLightMeter()
	{
		if ((Object)(object)_lightCam == (Object)null)
		{
			SetupRenderTexture();
		}
		if ((Object)(object)_lightCam == (Object)null)
		{
			return;
		}
		_lightCam.fieldOfView = CameraFOV.Value;
		List<Player> allPlayers = Utils.GetAllPlayers();
		if (allPlayers.Count != 0)
		{
			float score;
			if (UseAsyncGPUReadback.Value && SystemInfo.supportsAsyncGPUReadback)
			{
				ConsumeAsyncLightMeterScore();
				TryScheduleAsyncLightMeterScore(allPlayers);
			}
			else if (TryGetLightMeterScore(allPlayers, out score))
			{
				debugScore = score;
				RecalcMeterAverage(score);
			}
		}
	}

	private void ConsumeAsyncLightMeterScore()
	{
		if (_asyncScoreReady)
		{
			_asyncScoreReady = false;
			debugScore = _asyncScore;
			RecalcMeterAverage(_asyncScore);
		}
	}

	private void TryScheduleAsyncLightMeterScore(List<Player> playersList)
	{
		if (_asyncReadbackPending)
		{
			return;
		}
		List<Player> sampleTargetPlayers = GetSampleTargetPlayers(playersList);
		List<Player> sampleObserverPlayers = GetSampleObserverPlayers(playersList);
		if (sampleTargetPlayers.Count == 0)
		{
			return;
		}
		AsyncScoreBatch asyncScoreBatch = null;
		foreach (Player item in sampleTargetPlayers)
		{
			if (TryPrepareLightCameraSample(item, sampleObserverPlayers))
			{
				if (asyncScoreBatch == null)
				{
					asyncScoreBatch = CreateAsyncScoreBatch();
				}
				RenderAndRequestScoreReadback(item, asyncScoreBatch);
			}
		}
	}

	private bool TryGetLightMeterScore(List<Player> playersList, out float score)
	{
		score = 0f;
		if (!_computeShaderReady)
		{
			if (!_syncFallbackWarningLogged)
			{
				_syncFallbackWarningLogged = true;
				((BaseUnityPlugin)this).Logger.LogWarning((object)"Ombarella synchronous light meter path is disabled because the compute shader is unavailable.");
			}
			return false;
		}
		List<Player> sampleTargetPlayers = GetSampleTargetPlayers(playersList);
		List<Player> sampleObserverPlayers = GetSampleObserverPlayers(playersList);
		if (sampleTargetPlayers.Count == 0)
		{
			return false;
		}
		float num = 0f;
		int num2 = 0;
		foreach (Player item in sampleTargetPlayers)
		{
			if (TryPrepareLightCameraSample(item, sampleObserverPlayers) && TryRenderAndDispatchShader(item, out var score2))
			{
				num += score2;
				num2++;
			}
		}
		if (num2 == 0)
		{
			return false;
		}
		score = num / (float)num2;
		return true;
	}

	private List<Player> GetSampleTargetPlayers(List<Player> playersList)
	{
		if (UseOrbitCameraSampling.Value || UseFikaPlayerAveraging.Value)
		{
			List<Player> actualHumanPlayers = Utils.GetActualHumanPlayers(playersList);
			if (actualHumanPlayers.Count > 0)
			{
				return actualHumanPlayers;
			}
		}
		List<Player> list = new List<Player>();
		Player mainPlayer = Utils.GetMainPlayer();
		if (Utils.IsLightMeterUsablePlayer(mainPlayer))
		{
			list.Add(mainPlayer);
		}
		return list;
	}

	private List<Player> GetSampleObserverPlayers(List<Player> playersList)
	{
		if (UseOrbitCameraSampling.Value)
		{
			return null;
		}
		if (!UseFikaPlayerAveraging.Value)
		{
			return playersList;
		}
		return Utils.GetBotPlayers(playersList);
	}

	private bool TryPrepareLightCameraSample(Player targetPlayer, List<Player> observerPlayers)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (!(UseOrbitCameraSampling.Value ? CameraRig.TryRepositionCameraOnOrbit(targetPlayer, OrbitCameraRadius.Value, OrbitCameraHeightOffset.Value, GetSamplingOrbitAngle(), out var focusPoint) : CameraRig.TryRepositionCamera(targetPlayer, observerPlayers, out focusPoint)))
		{
			return false;
		}
		if (RejectOccludedSamples.Value)
		{
			return HasLineOfSightToFocus(focusPoint);
		}
		return true;
	}

	private float GetSamplingOrbitAngle()
	{
		if (UseFixedOrbitAngle.Value)
		{
			return FixedOrbitAngle.Value;
		}
		return Random.Range(0f, 360f);
	}

	private bool HasLineOfSightToFocus(Vector3 focusPoint)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_lightCam == (Object)null)
		{
			return false;
		}
		Vector3 position = ((Component)_lightCam).transform.position;
		Vector3 val = focusPoint - position;
		float magnitude = ((Vector3)(ref val)).magnitude;
		if (magnitude <= 0.001f || float.IsNaN(magnitude) || float.IsInfinity(magnitude))
		{
			return false;
		}
		RaycastHit val2 = default(RaycastHit);
		return !Physics.Raycast(position, val / magnitude, ref val2, magnitude, GetSampleOcclusionMask(), (QueryTriggerInteraction)1);
	}

	private int GetSampleOcclusionMask()
	{
		int mask = LayerMask.GetMask(new string[4] { "Terrain", "HighPolyCollider", "LowPolyCollider", "DoorLowPolyCollider" });
		if (mask == 0)
		{
			return -5 & ~GetPlayerLayerMask();
		}
		return mask;
	}

	private void RecalcMeterAverage(float meterThisFrame)
	{
		if (!float.IsNaN(meterThisFrame) && !float.IsInfinity(meterThisFrame))
		{
			int num = Mathf.Clamp(Mathf.RoundToInt(MeterAverageSamples.Value), 1, 600);
			_meterSamples.Enqueue(meterThisFrame);
			_meterSampleSum += meterThisFrame;
			while (_meterSamples.Count > num)
			{
				_meterSampleSum -= _meterSamples.Dequeue();
			}
			_avgLightMeter = ((_meterSamples.Count > 0) ? (_meterSampleSum / (float)_meterSamples.Count) : meterThisFrame);
			_avgLightMeter = Mathf.Clamp(_avgLightMeter, 0.01f, 1f);
			if (float.IsNaN(_avgLightMeter))
			{
				_avgLightMeter = 1f;
			}
			_hasValidMeterSample = true;
			ClampFinalValue();
		}
	}

	private void ResetMeterAverage()
	{
		_meterSamples.Clear();
		_meterSampleSum = 0f;
		_hasValidMeterSample = false;
		_avgLightMeter = 1f;
		_finalValueLerped = 1f;
		FinalLightMeter = 1f;
	}

	private ConfigEntry<float> ConstructFloatConfig(float defaultValue, string category, string descriptionShort, string descriptionFull, float min, float max, params ConfigDefinition[] migrateFrom)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		float migratedConfigValue = GetMigratedConfigValue(defaultValue, migrateFrom);
		return ((BaseUnityPlugin)this).Config.Bind<float>(category, descriptionShort, migratedConfigValue, new ConfigDescription(descriptionFull, (AcceptableValueBase)new AcceptableValueRange<float>(min, max), Array.Empty<object>()));
	}

	private ConfigEntry<bool> ConstructBoolConfig(bool defaultValue, string category, string descriptionShort, string descriptionFull, params ConfigDefinition[] migrateFrom)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		bool migratedConfigValue = GetMigratedConfigValue(defaultValue, migrateFrom);
		return ((BaseUnityPlugin)this).Config.Bind<bool>(category, descriptionShort, migratedConfigValue, new ConfigDescription(descriptionFull, (AcceptableValueBase)null, Array.Empty<object>()));
	}

	private ConfigDefinition OldConfig(string section, string key)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		return new ConfigDefinition(section, key);
	}

	private T GetMigratedConfigValue<T>(T defaultValue, ConfigDefinition[] migrateFrom)
	{
		if (migrateFrom == null)
		{
			return defaultValue;
		}
		foreach (ConfigDefinition val in migrateFrom)
		{
			if (!(val == (ConfigDefinition)null) && ((BaseUnityPlugin)this).Config.ContainsKey(val))
			{
				object boxedValue = ((BaseUnityPlugin)this).Config[val].BoxedValue;
				if (boxedValue is T)
				{
					return (T)boxedValue;
				}
				try
				{
					return (T)Convert.ChangeType(boxedValue, typeof(T));
				}
				catch
				{
					return defaultValue;
				}
			}
		}
		return defaultValue;
	}

	private float GetMigratedLumaColorDistribution(float defaultValue)
	{
		if (!TryGetConfigFloat(OldConfig("e - Luma + Color Blend", "1-Luma blend weight"), out var value) || !TryGetConfigFloat(OldConfig("e - Luma + Color Blend", "2-Color breadth blend weight"), out var value2))
		{
			return defaultValue;
		}
		value = Mathf.Max(0f, value);
		value2 = Mathf.Max(0f, value2);
		float num = value + value2;
		if (num <= 0.001f)
		{
			return defaultValue;
		}
		return Mathf.Clamp01(value2 / num);
	}

	private bool TryGetConfigFloat(ConfigDefinition definition, out float value)
	{
		value = 0f;
		if (definition == (ConfigDefinition)null || !((BaseUnityPlugin)this).Config.ContainsKey(definition))
		{
			return false;
		}
		object boxedValue = ((BaseUnityPlugin)this).Config[definition].BoxedValue;
		if (boxedValue is float num)
		{
			value = num;
			return true;
		}
		try
		{
			value = Convert.ToSingle(boxedValue);
			return true;
		}
		catch
		{
			return false;
		}
	}

	private void PopulateShader()
	{
		_computeShader = null;
		_computeShaderReady = false;
		AssetBundle val = LoadComputeShaderBundle();
		if ((Object)(object)val == (Object)null)
		{
			((BaseUnityPlugin)this).Logger.LogWarning((object)"Ombarella light meter compute shader bundle was not found. Async GPU readback can still work, but synchronous fallback is disabled.");
			return;
		}
		try
		{
			_computeShader = val.LoadAsset<ComputeShader>("GetAllPixelColors");
			_computeShaderReady = (Object)(object)_computeShader != (Object)null;
			string text = (_computeShaderReady ? "is loaded" : "is NULL");
			Debug.Log((object)("shader " + text));
		}
		finally
		{
			val.Unload(false);
		}
	}

	private AssetBundle LoadComputeShaderBundle()
	{
		string[] array = new string[2]
		{
			Path.Combine(Paths.PluginPath, "Ombarella", "shader"),
			Path.Combine(Paths.PluginPath, "Ombarella", "ombhistogram")
		};
		foreach (string text in array)
		{
			if (File.Exists(text))
			{
				AssetBundle val = AssetBundle.LoadFromFile(text);
				if ((Object)(object)val != (Object)null)
				{
					((BaseUnityPlugin)this).Logger.LogInfo((object)("Loaded light meter shader bundle: " + text));
					return val;
				}
				((BaseUnityPlugin)this).Logger.LogWarning((object)("Failed to load light meter shader bundle: " + text));
			}
		}
		return null;
	}

	private void SetupRenderTexture()
	{
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		int configuredTextureSize = GetConfiguredTextureSize();
		if ((Object)(object)_rt != (Object)null && (Object)(object)_colorRt != (Object)null && (!_computeShaderReady || outputBuffer != null) && configuredTextureSize == _texSize)
		{
			ApplyLightCameraSettings(_lightCam);
			return;
		}
		ReleaseRenderResources();
		_texSize = configuredTextureSize;
		_rt = CreateLightMeterRenderTexture();
		_colorRt = CreateLightMeterRenderTexture();
		if ((Object)(object)_lightCam == (Object)null)
		{
			_lightCam = ((Component)this).gameObject.AddComponent<Camera>();
		}
		_lightCam.targetTexture = _rt;
		((Behaviour)_lightCam).enabled = false;
		ApplyLightCameraSettings(_lightCam);
		CameraRig.Initialize(_lightCam);
		if (_computeShaderReady)
		{
			outputColors = (Color[])(object)new Color[_texSize * _texSize];
			outputBuffer = new ComputeBuffer(outputColors.Length, 16);
			_handleMain = _computeShader.FindKernel("CSMain");
			_computeShader.SetBuffer(_handleMain, "outputBuffer", outputBuffer);
		}
	}

	private RenderTexture CreateLightMeterRenderTexture()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		RenderTexture val = new RenderTexture(_texSize, _texSize, 16, (RenderTextureFormat)0)
		{
			enableRandomWrite = false,
			depth = 16,
			stencilFormat = (GraphicsFormat)0,
			dimension = (TextureDimension)2
		};
		val.Create();
		return val;
	}

	private int GetConfiguredTextureSize()
	{
		int num = Mathf.RoundToInt(RenderTextureResolution.Value);
		num = Mathf.Clamp(num, 16, 512);
		return Mathf.Max(8, Mathf.RoundToInt((float)num / 8f) * 8);
	}

	private void ReleaseRenderResources()
	{
		_asyncReadbackPending = false;
		_asyncScoreReady = false;
		_asyncBatchId++;
		if (outputBuffer != null)
		{
			outputBuffer.Release();
			outputBuffer = null;
		}
		if ((Object)(object)_rt != (Object)null)
		{
			_rt.Release();
			Object.Destroy((Object)(object)_rt);
			_rt = null;
		}
		if ((Object)(object)_colorRt != (Object)null)
		{
			_colorRt.Release();
			Object.Destroy((Object)(object)_colorRt);
			_colorRt = null;
		}
	}

	private int GetPlayerLayerMask()
	{
		int num = LayerMask.NameToLayer("Player");
		if (num < 0)
		{
			return 0;
		}
		return 1 << num;
	}

	private void ApplyLightCameraSettings(Camera lightCamera)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)lightCamera == (Object)null))
		{
			lightCamera.clearFlags = (CameraClearFlags)1;
			lightCamera.backgroundColor = Color.black;
			lightCamera.cullingMask = -1;
			lightCamera.renderingPath = (RenderingPath)(-1);
			lightCamera.allowHDR = false;
			lightCamera.allowMSAA = false;
			lightCamera.useOcclusionCulling = true;
			lightCamera.nearClipPlane = 0.01f;
			lightCamera.farClipPlane = 20f;
		}
	}

	private bool TryDispatchLumaStats(RenderTexture sourceTexture, ScoreSettings settings, out RenderStats stats)
	{
		stats = null;
		if (!TryDispatchRenderTexture(sourceTexture))
		{
			return false;
		}
		return TryCalculateLumaStats(outputColors, settings, out stats);
	}

	private bool TryDispatchColorBreadthStats(RenderTexture sourceTexture, ScoreSettings settings, out RenderStats stats)
	{
		stats = null;
		if (!TryDispatchRenderTexture(sourceTexture))
		{
			return false;
		}
		return TryCalculateColorBreadthStats(outputColors, settings, out stats);
	}

	private bool TryDispatchRenderTexture(RenderTexture sourceTexture)
	{
		if ((Object)(object)sourceTexture == (Object)null || !_computeShaderReady || outputBuffer == null)
		{
			return false;
		}
		_computeShader.SetTexture(_handleMain, "textureInput", (Texture)(object)sourceTexture);
		_computeShader.Dispatch(_handleMain, _texSize / 8, _texSize / 8, 1);
		outputBuffer.GetData((Array)outputColors);
		return true;
	}

	private bool TryRenderAndDispatchShader(Player targetPlayer, out float score)
	{
		score = 0f;
		ScoreSettings settings = CaptureScoreSettings();
		RenderLightCamera(_rt, enableColorFill: false, targetPlayer);
		if (!TryDispatchLumaStats(_rt, settings, out var stats))
		{
			return false;
		}
		RenderLightCamera(_colorRt, enableColorFill: true, targetPlayer);
		if (!TryDispatchColorBreadthStats(_colorRt, settings, out var stats2))
		{
			return false;
		}
		score = CombineTwoPassScore(stats, stats2, settings);
		return true;
	}

	public bool CanApplyBotVisibilityPatch()
	{
		if (MasterSwitch != null && MasterSwitch.Value && IsRaid)
		{
			return _hasValidMeterSample;
		}
		return false;
	}

	private AsyncScoreBatch CreateAsyncScoreBatch()
	{
		_asyncReadbackPending = true;
		_asyncBatchId++;
		return new AsyncScoreBatch
		{
			Id = _asyncBatchId,
			Pending = 0,
			Settings = CaptureScoreSettings()
		};
	}

	private void RenderAndRequestScoreReadback(Player targetPlayer, AsyncScoreBatch batch)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		AsyncScoreSample sample = new AsyncScoreSample
		{
			Batch = batch,
			Pending = 2
		};
		batch.Pending += sample.Pending;
		RenderLightCamera(_rt, enableColorFill: false, targetPlayer);
		AsyncGPUReadback.Request((Texture)(object)_rt, 0, (TextureFormat)4, (Action<AsyncGPUReadbackRequest>)delegate(AsyncGPUReadbackRequest request)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			HandleAsyncScoreReadback(request, sample, LightMeterRenderPass.Luma);
		});
		RenderLightCamera(_colorRt, enableColorFill: true, targetPlayer);
		AsyncGPUReadback.Request((Texture)(object)_colorRt, 0, (TextureFormat)4, (Action<AsyncGPUReadbackRequest>)delegate(AsyncGPUReadbackRequest request)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			HandleAsyncScoreReadback(request, sample, LightMeterRenderPass.ColorBreadth);
		});
	}

	private void RenderLightCamera(RenderTexture targetTexture, bool enableColorFill, Player targetPlayer)
	{
		ApplyLightCameraSettings(_lightCam);
		if (!enableColorFill)
		{
			DisableColorRenderFillLight();
		}
		RenderTexture targetTexture2 = _lightCam.targetTexture;
		_lightCam.targetTexture = targetTexture;
		bool flag = enableColorFill && EnableColorRenderFillLight(_lightCam);
		MagnifiedOpticRenderScope magnifiedOpticRenderScope = CreateMagnifiedOpticRenderScope(targetPlayer);
		FirstPersonBodyRenderScope firstPersonBodyRenderScope = CreateFirstPersonBodyRenderScope(targetPlayer, magnifiedOpticRenderScope?.HiddenRenderers);
		try
		{
			_lightCam.Render();
		}
		finally
		{
			firstPersonBodyRenderScope?.Dispose();
			magnifiedOpticRenderScope?.Dispose();
			if (flag)
			{
				DisableColorRenderFillLight();
			}
			_lightCam.targetTexture = targetTexture2;
		}
	}

	private MagnifiedOpticRenderScope CreateMagnifiedOpticRenderScope(Player targetPlayer)
	{
		if (!Utils.IsLightMeterUsablePlayer(targetPlayer))
		{
			return null;
		}
		GameObject handsControllerObject = GetHandsControllerObject(targetPlayer);
		if ((Object)(object)handsControllerObject == (Object)null)
		{
			return null;
		}
		HashSet<Renderer> magnifiedOpticRenderers = GetMagnifiedOpticRenderers(handsControllerObject);
		if (magnifiedOpticRenderers.Count <= 0)
		{
			return null;
		}
		return new MagnifiedOpticRenderScope(magnifiedOpticRenderers);
	}

	private GameObject GetHandsControllerObject(Player targetPlayer)
	{
		if ((Object)(object)targetPlayer == (Object)null || (Object)(object)targetPlayer.HandsController == (Object)null)
		{
			return null;
		}
		return targetPlayer.HandsController.ControllerGameObject;
	}

	private HashSet<Renderer> GetMagnifiedOpticRenderers(GameObject handsObject)
	{
		HashSet<Renderer> hashSet = new HashSet<Renderer>();
		if ((Object)(object)handsObject == (Object)null)
		{
			return hashSet;
		}
		SightModVisualControllers[] componentsInChildren = handsObject.GetComponentsInChildren<SightModVisualControllers>(true);
		foreach (SightModVisualControllers val in componentsInChildren)
		{
			if (IsMagnifiedOpticVisual(val))
			{
				AddRenderers(((Component)val).gameObject, hashSet);
			}
		}
		return hashSet;
	}

	private bool IsMagnifiedOpticVisual(SightModVisualControllers sightController)
	{
		if ((Object)(object)sightController == (Object)null)
		{
			return false;
		}
		ScopeZoomHandler val = default(ScopeZoomHandler);
		if (sightController.TryGetZoomHandler(ref val) && (Object)(object)val != (Object)null)
		{
			return true;
		}
		ScopePrefabCache component = ((Component)sightController).GetComponent<ScopePrefabCache>();
		if ((Object)(object)component != (Object)null)
		{
			return component.HasOptics;
		}
		return false;
	}

	private void AddRenderers(GameObject rootObject, HashSet<Renderer> renderers)
	{
		if ((Object)(object)rootObject == (Object)null || renderers == null)
		{
			return;
		}
		Renderer[] componentsInChildren = rootObject.GetComponentsInChildren<Renderer>(true);
		foreach (Renderer val in componentsInChildren)
		{
			if ((Object)(object)val != (Object)null)
			{
				renderers.Add(val);
			}
		}
	}

	private FirstPersonBodyRenderScope CreateFirstPersonBodyRenderScope(Player targetPlayer, HashSet<Renderer> excludedRenderers)
	{
		if (!Utils.IsLightMeterUsablePlayer(targetPlayer))
		{
			return null;
		}
		List<Renderer> list = new List<Renderer>();
		HashSet<Renderer> seenRenderers = new HashSet<Renderer>();
		AddShadowsOnlyRenderers(((Component)targetPlayer).gameObject, list, seenRenderers, excludedRenderers);
		if ((Object)(object)targetPlayer.HandsController != (Object)null)
		{
			AddShadowsOnlyRenderers(targetPlayer.HandsController.ControllerGameObject, list, seenRenderers, excludedRenderers);
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return new FirstPersonBodyRenderScope(list);
	}

	private void AddShadowsOnlyRenderers(GameObject rootObject, List<Renderer> renderers, HashSet<Renderer> seenRenderers, HashSet<Renderer> excludedRenderers)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Invalid comparison between Unknown and I4
		if ((Object)(object)rootObject == (Object)null)
		{
			return;
		}
		Renderer[] componentsInChildren = rootObject.GetComponentsInChildren<Renderer>(true);
		foreach (Renderer val in componentsInChildren)
		{
			if ((Object)(object)val != (Object)null && !IsExcludedRenderer(val, excludedRenderers) && (int)val.shadowCastingMode == 3 && seenRenderers.Add(val))
			{
				renderers.Add(val);
			}
		}
	}

	private bool IsExcludedRenderer(Renderer renderer, HashSet<Renderer> excludedRenderers)
	{
		if ((Object)(object)renderer != (Object)null && excludedRenderers != null)
		{
			return excludedRenderers.Contains(renderer);
		}
		return false;
	}

	private bool EnableColorRenderFillLight(Camera lightCamera)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)lightCamera == (Object)null || ColorRenderFillIntensity.Value <= 0f)
		{
			return false;
		}
		if (!EnsureColorRenderFillLight())
		{
			return false;
		}
		_colorRenderFillLight.type = (LightType)1;
		_colorRenderFillLight.color = Color.white;
		_colorRenderFillLight.intensity = ColorRenderFillIntensity.Value;
		_colorRenderFillLight.bounceIntensity = 0f;
		_colorRenderFillLight.shadows = (LightShadows)0;
		_colorRenderFillLight.renderMode = (LightRenderMode)1;
		_colorRenderFillLight.cullingMask = -1;
		((Component)_colorRenderFillLight).transform.position = ((Component)lightCamera).transform.position;
		((Component)_colorRenderFillLight).transform.rotation = ((Component)lightCamera).transform.rotation;
		((Behaviour)_colorRenderFillLight).enabled = true;
		return true;
	}

	private bool EnsureColorRenderFillLight()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		if ((Object)(object)_colorRenderFillLight != (Object)null)
		{
			return true;
		}
		GameObject val = new GameObject("Ombarella Color Render Fill Light");
		((Object)val).hideFlags = (HideFlags)61;
		val.transform.SetParent(((Component)this).transform, false);
		_colorRenderFillLight = val.AddComponent<Light>();
		((Behaviour)_colorRenderFillLight).enabled = false;
		return true;
	}

	private void DisableColorRenderFillLight()
	{
		if ((Object)(object)_colorRenderFillLight != (Object)null)
		{
			((Behaviour)_colorRenderFillLight).enabled = false;
		}
	}

	private void DestroyColorRenderFillLight()
	{
		if (!((Object)(object)_colorRenderFillLight == (Object)null))
		{
			Object.Destroy((Object)(object)((Component)_colorRenderFillLight).gameObject);
			_colorRenderFillLight = null;
		}
	}

	private void HandleAsyncScoreReadback(AsyncGPUReadbackRequest request, AsyncScoreSample sample, LightMeterRenderPass renderPass)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		AsyncScoreBatch batch = sample.Batch;
		if (_isDestroyed || batch == null || batch.Id != _asyncBatchId)
		{
			return;
		}
		try
		{
			if (!((AsyncGPUReadbackRequest)(ref request)).hasError)
			{
				if (renderPass == LightMeterRenderPass.Luma)
				{
					sample.LumaPassFailed = !TryCalculateLumaStats(((AsyncGPUReadbackRequest)(ref request)).GetData<Color32>(0), batch.Settings, out sample.LumaStats);
				}
				else
				{
					sample.ColorPassFailed = !TryCalculateColorBreadthStats(((AsyncGPUReadbackRequest)(ref request)).GetData<Color32>(0), batch.Settings, out sample.ColorStats);
				}
			}
			else if (renderPass == LightMeterRenderPass.Luma)
			{
				sample.LumaPassFailed = true;
			}
			else
			{
				sample.ColorPassFailed = true;
			}
		}
		catch (Exception arg)
		{
			Utils.LogError($"Ombarella async readback failed for {renderPass}: {arg}");
			if (renderPass == LightMeterRenderPass.Luma)
			{
				sample.LumaPassFailed = true;
			}
			else
			{
				sample.ColorPassFailed = true;
			}
		}
		sample.Pending--;
		batch.Pending--;
		if (sample.Pending == 0 && !sample.LumaPassFailed && !sample.ColorPassFailed && sample.LumaStats != null && sample.ColorStats != null)
		{
			batch.ScoreSum += CombineTwoPassScore(sample.LumaStats, sample.ColorStats, batch.Settings);
			batch.ScoreCount++;
		}
		if (batch.Pending <= 0)
		{
			_asyncReadbackPending = false;
			if (batch.ScoreCount != 0)
			{
				_asyncScore = batch.ScoreSum / (float)batch.ScoreCount;
				_asyncScoreReady = true;
			}
		}
	}

	private ScoreSettings CaptureScoreSettings()
	{
		ScoreSettings result = default(ScoreSettings);
		result.LumaCoef = LumaCoef.Value;
		result.RedLumaMulti = RedLumaMulti.Value;
		result.GreenLumaMulti = GreenLumaMulti.Value;
		result.BlueLumaMulti = BlueLumaMulti.Value;
		result.RedColorBreadthMulti = RedColorBreadthMulti.Value;
		result.GreenColorBreadthMulti = GreenColorBreadthMulti.Value;
		result.BlueColorBreadthMulti = BlueColorBreadthMulti.Value;
		result.LumaColorDistribution = LumaColorDistribution.Value;
		return result;
	}

	private bool TryCalculateLumaStats(NativeArray<Color32> pixels, ScoreSettings settings, out RenderStats stats)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		stats = new RenderStats();
		int length = pixels.Length;
		if (length == 0)
		{
			return false;
		}
		for (int i = 0; i < length; i++)
		{
			Color32 val = pixels[i];
			AccumulateLumaStats(stats, (int)val.r, (int)val.g, (int)val.b, settings);
		}
		return FinalizeLumaStats(stats);
	}

	private bool TryCalculateLumaStats(Color[] pixels, ScoreSettings settings, out RenderStats stats)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		stats = new RenderStats();
		if (pixels == null || pixels.Length == 0)
		{
			return false;
		}
		foreach (Color val in pixels)
		{
			AccumulateLumaStats(stats, val.r * 255f, val.g * 255f, val.b * 255f, settings);
		}
		return FinalizeLumaStats(stats);
	}

	private void AccumulateLumaStats(RenderStats stats, float r, float g, float b, ScoreSettings settings)
	{
		stats.SampleCount++;
		stats.RLumaSum += r * settings.RedLumaMulti;
		stats.GLumaSum += g * settings.GreenLumaMulti;
		stats.BLumaSum += b * settings.BlueLumaMulti;
	}

	private bool FinalizeLumaStats(RenderStats stats)
	{
		if (stats.SampleCount == 0)
		{
			return false;
		}
		stats.Luma = (stats.RLumaSum + stats.GLumaSum + stats.BLumaSum) / (255f * (float)stats.SampleCount);
		if (!float.IsNaN(stats.Luma))
		{
			return !float.IsInfinity(stats.Luma);
		}
		return false;
	}

	private bool TryCalculateColorBreadthStats(NativeArray<Color32> pixels, ScoreSettings settings, out RenderStats stats)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		stats = new RenderStats();
		int length = pixels.Length;
		if (length == 0)
		{
			return false;
		}
		for (int i = 0; i < length; i++)
		{
			Color32 val = pixels[i];
			AccumulateColorBreadthStats(stats, (int)val.r, (int)val.g, (int)val.b);
		}
		return FinalizeColorBreadthStats(stats, settings);
	}

	private bool TryCalculateColorBreadthStats(Color[] pixels, ScoreSettings settings, out RenderStats stats)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		stats = new RenderStats();
		if (pixels == null || pixels.Length == 0)
		{
			return false;
		}
		foreach (Color val in pixels)
		{
			AccumulateColorBreadthStats(stats, val.r * 255f, val.g * 255f, val.b * 255f);
		}
		return FinalizeColorBreadthStats(stats, settings);
	}

	private void AccumulateColorBreadthStats(RenderStats stats, float r, float g, float b)
	{
		stats.SampleCount++;
		if (r < stats.RLow)
		{
			stats.RLow = r;
		}
		if (g < stats.GLow)
		{
			stats.GLow = g;
		}
		if (b < stats.BLow)
		{
			stats.BLow = b;
		}
		if (r > stats.RHigh)
		{
			stats.RHigh = r;
		}
		if (g > stats.GHigh)
		{
			stats.GHigh = g;
		}
		if (b > stats.BHigh)
		{
			stats.BHigh = b;
		}
	}

	private bool FinalizeColorBreadthStats(RenderStats stats, ScoreSettings settings)
	{
		if (stats.SampleCount == 0)
		{
			return false;
		}
		float num = (stats.RHigh - stats.RLow) * settings.RedColorBreadthMulti;
		float num2 = (stats.GHigh - stats.GLow) * settings.GreenColorBreadthMulti;
		float num3 = (stats.BHigh - stats.BLow) * settings.BlueColorBreadthMulti;
		stats.ColorBreadth = Mathf.Clamp01((num + num2 + num3) / 765f);
		if (!float.IsNaN(stats.ColorBreadth))
		{
			return !float.IsInfinity(stats.ColorBreadth);
		}
		return false;
	}

	private float CombineTwoPassScore(RenderStats lumaStats, RenderStats colorStats, ScoreSettings settings)
	{
		float num = Mathf.Clamp01(lumaStats.Luma * settings.LumaCoef);
		float num2 = Mathf.Clamp01(colorStats.ColorBreadth);
		float num3 = Mathf.Clamp01(settings.LumaColorDistribution);
		float num4 = 1f - num3;
		float num5 = num * num4 + num2 * num3;
		debugLumaScore = num;
		debugScore2 = num2;
		return Mathf.Clamp(num5, 0.01f, 1f);
	}

	private void ClampFinalValue()
	{
		float finalValueLerped = Mathf.Clamp(_avgLightMeter, 0.01f, 1f);
		_finalValueLerped = finalValueLerped;
		if (float.IsNaN(_finalValueLerped))
		{
			_finalValueLerped = 1f;
		}
		float num = 1f - MeterAttenuationCoef.Value;
		FinalLightMeter = Mathf.Lerp(_finalValueLerped, 1f, num);
		Utils.Log($"_finalValueBeforeMod : {_finalValueLerped} // final light output : {FinalLightMeter}", oneTimeLog: false);
	}

	private void OnGUI()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		if (MasterSwitch != null && MasterSwitch.Value && Utils.IsInRaid())
		{
			if (MeterViz.Value)
			{
				efficiencyIndicatorStyle.normal.textColor = Color.grey;
				efficiencyIndicatorStyle.fontSize = 20;
				float num = 10f;
				string levelString = Visualiser.GetLevelString(_finalValueLerped, isDebug: false);
				GUI.Label(new Rect(20f, num, 40f, 40f), levelString, efficiencyIndicatorStyle);
			}
			if (IsDebug.Value && MeterViz.Value)
			{
				string text = string.Format($"score {debugScore}, luma {debugLumaScore}, color breadth {debugScore2}");
				GUI.Label(new Rect(20f, 50f, 40f, 40f), text, efficiencyIndicatorStyle);
			}
			DrawRenderTexturePreview();
		}
	}

	private void DrawRenderTexturePreview()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (ShowRenderTexturePreview.Value && !((Object)(object)_rt == (Object)null))
		{
			float value = RenderTexturePreviewSize.Value;
			Color color = GUI.color;
			DrawRenderTexturePreviewFrame(new Rect(20f, 80f, value, value), _rt);
			DrawRenderTexturePreviewFrame(new Rect(28f + value, 80f, value, value), _colorRt);
			GUI.color = color;
		}
	}

	private void DrawRenderTexturePreviewFrame(Rect previewRect, RenderTexture renderTexture)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)renderTexture == (Object)null))
		{
			Rect val = new Rect(((Rect)(ref previewRect)).x - 2f, ((Rect)(ref previewRect)).y - 2f, ((Rect)(ref previewRect)).width + 4f, ((Rect)(ref previewRect)).height + 4f);
			GUI.color = Color.black;
			GUI.DrawTexture(val, (Texture)(object)Texture2D.whiteTexture, (ScaleMode)0);
			GUI.color = Color.white;
			GUI.DrawTexture(previewRect, (Texture)(object)renderTexture, (ScaleMode)2, false);
		}
	}
}
