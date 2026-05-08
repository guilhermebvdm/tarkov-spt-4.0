using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT.Weather;
using UnityEngine;
using WaterSSR;

public class RainController : MonoBehaviour
{
	public enum ERainIntensity
	{
		None,
		Low,
		Med,
		High
	}

	public enum ERainControllerStatus : byte
	{
		Summer,
		WinterStorm,
		WinterStormReconnect,
		Winter,
		Spring,
		SpringEarly,
		Autumn,
		AutumnLate
	}

	public abstract class Class668 : IDisposable
	{
		[NonSerialized]
		[CompilerGenerated]
		public RainController RainController_0_1;

		[NonSerialized]
		[CompilerGenerated]
		public ERainControllerStatus ErainControllerStatus_0;

		public RainController RainController_0
		{
			[CompilerGenerated]
			get
			{
				return RainController_0_1;
			}
			[CompilerGenerated]
			set
			{
				RainController_0_1 = value;
			}
		}

		public ERainControllerStatus ERainControllerStatus_0
		{
			[CompilerGenerated]
			get
			{
				return ErainControllerStatus_0;
			}
		}

		public Class668(RainController controller, ERainControllerStatus status)
		{
			RainController_0 = controller;
			ErainControllerStatus_0 = status;
		}

		public void Dispose()
		{
			if (RainController_0 != null && RainController_0.class668_0 == this)
			{
				RainController_0.class668_0 = null;
			}
			RainController_0 = null;
		}

		public virtual void vmethod_0()
		{
			LoggerClass.LogError($"Invalid switch from Status:{ERainControllerStatus_0} to {ERainControllerStatus.Summer}");
		}

		public virtual void vmethod_1()
		{
			LoggerClass.LogError($"Invalid switch from Status:{ERainControllerStatus_0} to {ERainControllerStatus.Spring}");
		}

		public virtual void vmethod_2(Vector3 springSnowFactor)
		{
			LoggerClass.LogError($"Invalid switch from Status:{ERainControllerStatus_0} to {ERainControllerStatus.SpringEarly}");
		}

		public virtual void vmethod_3()
		{
			LoggerClass.LogError($"Invalid switch from Status:{ERainControllerStatus_0} to {ERainControllerStatus.Autumn}");
		}

		public virtual void vmethod_4()
		{
			LoggerClass.LogError($"Invalid switch from Status:{ERainControllerStatus_0} to {ERainControllerStatus.AutumnLate}");
		}

		public virtual void vmethod_5()
		{
			LoggerClass.LogError($"Invalid switch from Status:{ERainControllerStatus_0} to {ERainControllerStatus.Winter}");
		}

		public virtual void vmethod_6()
		{
			LoggerClass.LogError($"Invalid switch from Status:{ERainControllerStatus_0} to {ERainControllerStatus.WinterStorm}");
		}

		public virtual void vmethod_7()
		{
			LoggerClass.LogError($"Invalid switch from Status:{ERainControllerStatus_0} to {ERainControllerStatus.WinterStormReconnect}");
		}

		public virtual void vmethod_8(CameraClass cameraManager)
		{
			RainController_0.transform_0 = cameraManager.Camera.transform;
			RainController_0._depthPhotograper.Render();
		}

		public abstract void vmethod_9(bool debug);

		public abstract void vmethod_10(float wetting, float fogginess, Vector2 mainWind, float t01);

		public virtual void vmethod_11(float wetting)
		{
			Wetting = wetting;
			action_0?.Invoke(Wetting);
		}

		public virtual void vmethod_12()
		{
			vmethod_13(0f);
			Shader.DisableKeyword("Rain");
		}

		public virtual void vmethod_13(float value)
		{
			Intensity = value;
			Shader.SetGlobalFloat(int_1, Intensity);
			if (Mathf.Abs(Intensity) < Mathf.Epsilon)
			{
				Shader.DisableKeyword("Rain");
			}
			else
			{
				Shader.EnableKeyword("Rain");
			}
			if (GlobalEventHandlerClass.Instance != null)
			{
				GlobalEventHandlerClass.CreateEvent<GClass3543>().Invoke(IntensityType);
			}
		}

		public virtual void vmethod_14(float opaqueness)
		{
			Opaqueness = Mathf.Clamp01(opaqueness);
		}

		public virtual void vmethod_15(in Vector3 rainDir, float rain, float laingWater, float cloudness, float hour)
		{
			FallingVectorV3 = rainDir;
			Vector2 puddleWaterLevel = RainController_0.PuddleWaterLevel;
			Shader.SetGlobalFloat(int_0, Mathf.LerpUnclamped(puddleWaterLevel.x, puddleWaterLevel.y, laingWater));
			vmethod_13(rain);
		}
	}

	public abstract class Class669 : Class668
	{
		public Class669(RainController controller, ERainControllerStatus status)
			: base(controller, status)
		{
			base.RainController_0._rainFallDrops.Init();
			if (WeatherController.Instance != null)
			{
				Wetting = WeatherController.Instance.WeatherCurve.Rain;
			}
		}

		public override void vmethod_8(CameraClass cameraManager)
		{
			base.vmethod_8(cameraManager);
			base.RainController_0.screenWater_0 = cameraManager.EffectsController.ScreenWater;
			base.RainController_0.rainScreenDrops_0 = cameraManager.EffectsController.RainScreenDrops;
			base.RainController_0.rainScreenDrops_0.Init();
		}

		public override void vmethod_9(bool debug)
		{
			base.RainController_0._wetRenderer.SetDebugRain(debug);
		}

		public override void vmethod_13(float value)
		{
			base.vmethod_13(value);
			base.RainController_0._rippleController.Intensity = value;
			base.RainController_0._rainFallDrops.Intensity = value;
			base.RainController_0._rainSplashController.Intensity = value;
			if (base.RainController_0.rainScreenDrops_0 != null)
			{
				base.RainController_0.rainScreenDrops_0.SetIntensity(value);
			}
			if (base.RainController_0.screenWater_0 != null)
			{
				base.RainController_0.screenWater_0.SetIntensity(value);
			}
		}

		public override void vmethod_10(float wetting, float fogginess, Vector2 mainWind, float t01)
		{
			float deltaTime = Time.deltaTime;
			float num = ((wetting > 0f) ? (wetting * deltaTime / 60f) : ((0f - t01) * (1f + 0.1f * (1f - fogginess) + 0.3f * mainWind.magnitude) * deltaTime / 180f));
			wetting = Mathf.Clamp01(Wetting + num);
			base.RainController_0._wetRenderer.Intensity = wetting;
			vmethod_11(wetting);
		}

		public override void vmethod_12()
		{
			base.vmethod_12();
			if (base.RainController_0.rainScreenDrops_0 != null)
			{
				base.RainController_0.rainScreenDrops_0.enabled = false;
			}
			if (base.RainController_0.screenWater_0 != null)
			{
				base.RainController_0.screenWater_0.enabled = false;
			}
		}

		public override void vmethod_15(in Vector3 rainDir, float rain, float laingWater, float cloudness, float hour)
		{
			base.vmethod_15(in rainDir, rain, laingWater, cloudness, hour);
			cloudness = Mathf.InverseLerp(0f, 1f, cloudness);
			float b = Mathf.InverseLerp(0f, 12f, Mathf.Abs(hour - 12f));
			float num = Mathf.InverseLerp(12f, 0f, Mathf.Abs(hour - 12f));
			float num2 = Mathf.Max(cloudness, b);
			float minAmbientAdditionCoef = (rain + num2) / 2f * (num / 2f + 0.5f);
			base.RainController_0._rainFallDrops.MinAmbientAdditionCoef = minAmbientAdditionCoef;
		}
	}

	public class Class670 : Class669
	{
		public Class670(RainController controller)
			: base(controller, ERainControllerStatus.Summer)
		{
			WaterRendererv3.DisableSnowMask(disable: true);
		}

		public override void vmethod_0()
		{
			LoggerClass.LogTrace("StateSummer.SetupSummer");
		}

		public override void vmethod_5()
		{
			LoggerClass.LogTrace("StateSummer.SetupWinter");
			base.RainController_0.class668_0 = new Class676(base.RainController_0);
			Dispose();
		}

		public override void vmethod_6()
		{
			LoggerClass.LogTrace("StateSummer.SetupWinterStorm");
			base.RainController_0.class668_0 = new Class677(base.RainController_0);
			Dispose();
		}

		public override void vmethod_7()
		{
			LoggerClass.LogTrace("StateSummer.SetupWinterStormReconnect");
			base.RainController_0.class668_0 = new Class678(base.RainController_0);
			Dispose();
		}

		public override void vmethod_2(Vector3 springSnowFactor)
		{
			LoggerClass.LogTrace("StateSummer.SetupSpringEarly");
			base.RainController_0.class668_0 = new Class671(base.RainController_0, springSnowFactor);
			Dispose();
		}

		public override void vmethod_1()
		{
			LoggerClass.LogTrace("StateSummer.SetupSpring");
			base.RainController_0.class668_0 = new Class672(base.RainController_0);
			Dispose();
		}

		public override void vmethod_3()
		{
			LoggerClass.LogTrace("StateSummer.SetupAutumn");
			base.RainController_0.class668_0 = new Class673(base.RainController_0);
			Dispose();
		}

		public override void vmethod_4()
		{
			LoggerClass.LogTrace("StateSummer.SetupAutumnLate");
			base.RainController_0.class668_0 = new Class674(base.RainController_0);
			Dispose();
		}
	}

	public class Class671 : Class669
	{
		public Class671(RainController controller, Vector3 springSnowFactor)
			: base(controller, ERainControllerStatus.SpringEarly)
		{
			WaterRendererv3.DisableSnowMask(disable: false);
			base.RainController_0._snowWetRenderer.Wetting = 0.74f;
			base.RainController_0._snowWetRenderer.Opaqueness = 3.6f;
			base.RainController_0._snowWetRenderer.SpringSnowFactor = springSnowFactor;
			base.RainController_0._snowWetRenderer.WinterShow = false;
			base.RainController_0._snowWetRenderer.enabled = true;
			WetRenderer.WetOffMaskUsed = true;
			base.RainController_0._wetRenderer.enabled = true;
		}
	}

	public class Class672 : Class669
	{
		public Class672(RainController controller)
			: base(controller, ERainControllerStatus.Summer)
		{
			WaterRendererv3.DisableSnowMask(disable: true);
		}
	}

	public class Class673 : Class669
	{
		public Class673(RainController controller)
			: base(controller, ERainControllerStatus.Autumn)
		{
			WaterRendererv3.DisableSnowMask(disable: true);
		}
	}

	public class Class674 : Class669
	{
		public Class674(RainController controller)
			: base(controller, ERainControllerStatus.AutumnLate)
		{
			WaterRendererv3.DisableSnowMask(disable: true);
		}
	}

	public abstract class Class675 : Class668
	{
		public Class675(RainController controller, ERainControllerStatus status)
			: base(controller, status)
		{
			base.RainController_0._wetRenderer.enabled = false;
			base.RainController_0._rippleController.enabled = false;
			base.RainController_0._rainFallDrops.gameObject.SetActive(value: false);
			base.RainController_0._rainSplashController.gameObject.SetActive(value: false);
			base.RainController_0._snowWetRenderer.WinterShow = true;
			base.RainController_0._snowWetRenderer.enabled = true;
			base.RainController_0._snowRippleController.enabled = true;
			base.RainController_0._snowFlakes.gameObject.SetActive(value: true);
		}

		public override void vmethod_10(float wetting, float fogginess, Vector2 mainWind, float t01)
		{
			vmethod_11(wetting);
		}

		public override void vmethod_12()
		{
			base.vmethod_12();
			base.RainController_0._snowWetRenderer.Wetting = 0f;
		}

		public override void vmethod_13(float value)
		{
			base.vmethod_13(value);
			base.RainController_0._snowFlakes.Intensity = value;
			base.RainController_0._snowRippleController.Intensity = value;
		}

		public override void vmethod_9(bool debug)
		{
			base.RainController_0._snowWetRenderer.SetDebugRain(debug);
		}

		public override void vmethod_14(float opaqueness)
		{
			base.vmethod_14(opaqueness);
			base.RainController_0._snowWetRenderer.Opaqueness = Opaqueness;
		}
	}

	public class Class676 : Class675
	{
		public Class676(RainController controller)
			: base(controller, ERainControllerStatus.Winter)
		{
			base.RainController_0._snowWetRenderer.Wetting = 1f;
			vmethod_11(1f);
			WaterRendererv3.DisableSnowMask(disable: false);
		}
	}

	public class Class677 : Class675
	{
		public Class677(RainController controller)
			: base(controller, ERainControllerStatus.WinterStorm)
		{
			base.RainController_0._snowWetRenderer.StormEnabled = true;
			base.RainController_0._snowFlakes.StormFactor = 1f;
		}

		public override void vmethod_11(float wetting)
		{
			base.RainController_0._snowWetRenderer.Wetting = wetting;
			base.vmethod_11(wetting);
		}
	}

	public class Class678 : Class675
	{
		public Class678(RainController controller)
			: base(controller, ERainControllerStatus.WinterStormReconnect)
		{
			base.RainController_0._snowWetRenderer.StormEnabled = true;
			base.RainController_0._snowWetRenderer.Wetting = 1f;
			base.RainController_0._snowFlakes.StormFactor = 1f;
			vmethod_11(1f);
		}
	}

	[GAttribute6(0f, 1f, -1f)]
	public Vector2 PuddleWaterLevel = new Vector2(0f, 0.95f);

	[SerializeField]
	private DepthPhotograper _depthPhotograper;

	[SerializeField]
	private RainSplashController _rainSplashController;

	[SerializeField]
	private RainFallDrops _rainFallDrops;

	[SerializeField]
	private WetRenderer _wetRenderer;

	[SerializeField]
	private RippleController _rippleController;

	[SerializeField]
	private SnowFlakes _snowFlakes;

	[SerializeField]
	private SnowWetRenderer _snowWetRenderer;

	[SerializeField]
	private SnowRippleController _snowRippleController;

	private Class668 class668_0;

	[CompilerGenerated]
	private static bool bool_0;

	[CompilerGenerated]
	private static float float_0;

	[CompilerGenerated]
	private static float float_1;

	[CompilerGenerated]
	private static float float_2;

	[CompilerGenerated]
	private static Action<float> action_0;

	[CompilerGenerated]
	private static Vector3 vector3_0;

	private RainScreenDrops rainScreenDrops_0;

	private ScreenWater screenWater_0;

	private Transform transform_0;

	private static readonly List<RainCondensator> list_0 = new List<RainCondensator>();

	private static readonly int int_0 = Shader.PropertyToID("_WaterLevel");

	private static readonly int int_1 = Shader.PropertyToID("_RainIntensity");

	public static LoggerClass LoggerClass => Class443.Logger;

	public Class668 Class668_0 => class668_0 ?? (class668_0 = new Class670(this));

	public float StormSideSpeed
	{
		get
		{
			return _snowFlakes.StormSideSpeed;
		}
		set
		{
			_snowFlakes.StormSideSpeed = value;
		}
	}

	public static bool IsCameraUnderRain
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

	public static float Intensity
	{
		[CompilerGenerated]
		get
		{
			return float_0;
		}
		[CompilerGenerated]
		set
		{
			float_0 = value;
		}
	}

	public static float Opaqueness
	{
		[CompilerGenerated]
		get
		{
			return float_1;
		}
		[CompilerGenerated]
		set
		{
			float_1 = value;
		}
	}

	public static float Wetting
	{
		[CompilerGenerated]
		get
		{
			return float_2;
		}
		[CompilerGenerated]
		set
		{
			float_2 = value;
		}
	}

	public static ERainIntensity IntensityType
	{
		get
		{
			if (Intensity > 0.7f)
			{
				return ERainIntensity.High;
			}
			if (Intensity > 0.2f)
			{
				return ERainIntensity.Med;
			}
			if (Intensity > 0.01f)
			{
				return ERainIntensity.Low;
			}
			return ERainIntensity.None;
		}
	}

	public static Vector3 FallingVectorV3
	{
		[CompilerGenerated]
		get
		{
			return vector3_0;
		}
		[CompilerGenerated]
		set
		{
			vector3_0 = value;
		}
	}

	public static event Action<float> WettingChangedEvent
	{
		[CompilerGenerated]
		add
		{
			Action<float> action = action_0;
			Action<float> action2;
			do
			{
				action2 = action;
				Action<float> value2 = (Action<float>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<float> action = action_0;
			Action<float> action2;
			do
			{
				action2 = action;
				Action<float> value2 = (Action<float>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void Awake()
	{
		CameraClass.Instance.OnCameraChanged += method_0;
	}

	public void Update()
	{
		IsCameraUnderRain = _depthPhotograper.CheckIfCameraUnderRain(transform_0);
		method_15();
	}

	public void OnDestroy()
	{
		Class668_0.vmethod_12();
		CameraClass.Instance.OnCameraChanged -= method_0;
	}

	public void method_0()
	{
		Class668_0.vmethod_8(CameraClass.Instance);
	}

	public void method_1()
	{
		Class668_0.vmethod_12();
	}

	public void method_2()
	{
		Class668_0.vmethod_0();
	}

	public void method_3()
	{
		Class668_0.vmethod_1();
	}

	public void method_4(Vector3 springSnowFactor)
	{
		Class668_0.vmethod_2(springSnowFactor);
	}

	public void method_5()
	{
		Class668_0.vmethod_3();
	}

	public void method_6()
	{
		Class668_0.vmethod_4();
	}

	public void method_7()
	{
		Class668_0.vmethod_5();
	}

	public void method_8()
	{
		Class668_0.vmethod_6();
	}

	public void method_9()
	{
		Class668_0.vmethod_7();
	}

	public void method_10(float wetting)
	{
		Class668_0.vmethod_11(wetting);
	}

	public void method_11(float wetting, float fogginess, Vector2 mainWind, float t01)
	{
		Class668_0.vmethod_10(wetting, fogginess, mainWind, t01);
	}

	public void method_12(bool debug)
	{
		Class668_0.vmethod_9(debug);
	}

	public void method_13(float opaqueness)
	{
		Class668_0.vmethod_14(opaqueness);
	}

	public void method_14(in Vector3 rainDir, float rain, float laingWater, float cloudiness, float hour)
	{
		Class668_0.vmethod_15(in rainDir, rain, laingWater, cloudiness, hour);
	}

	public static void smethod_0(RainCondensator rainCondensator)
	{
		if (!list_0.Contains(rainCondensator))
		{
			list_0.Add(rainCondensator);
		}
	}

	public static void smethod_1(RainCondensator rainCondensator)
	{
		if (list_0.Contains(rainCondensator))
		{
			list_0.Remove(rainCondensator);
		}
	}

	public void method_15()
	{
		if (!(Intensity <= 0f))
		{
			for (int i = 0; i < list_0.Count; i++)
			{
				list_0[i].UpdateValues();
			}
		}
	}
}
