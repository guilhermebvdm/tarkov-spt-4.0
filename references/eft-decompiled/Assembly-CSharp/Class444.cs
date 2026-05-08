using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Comfort.Common;
using EFT.GlobalEvents;
using EFT.Impostors;
using EFT.Weather;
using UnityEngine;

public class Class444 : Class443, GInterface29, IDisposable
{
	public interface Interface3 : IDisposable
	{
		ESeason Season { get; }

		ESeasonStatus Status { get; }

		Task Run();

		void StormStarted(GInterface49 visual);

		void SetupVisual(GInterface49 visual);

		Task HandleReconnect(GInterface49 visual, ESeasonStatus status, SeasonsSettingsClass seasonsSettings);
	}

	public abstract class Class445
	{
		[NonSerialized]
		[CompilerGenerated]
		public Class444 Class444_0_1;

		[NonSerialized]
		[CompilerGenerated]
		public ESeason Eseason_0;

		[NonSerialized]
		[CompilerGenerated]
		public ESeasonStatus EseasonStatus_0;

		public static RainController RainController_0 => WeatherController.Instance.RainController;

		public Class444 Class444_0
		{
			[CompilerGenerated]
			get
			{
				return Class444_0_1;
			}
			[CompilerGenerated]
			set
			{
				Class444_0_1 = value;
			}
		}

		public ESeason Season
		{
			[CompilerGenerated]
			get
			{
				return Eseason_0;
			}
		}

		public ESeasonStatus Status
		{
			[CompilerGenerated]
			get
			{
				return EseasonStatus_0;
			}
		}

		public Class445(Class444 controller, ESeasonStatus status, ESeason season)
		{
			Class444_0 = controller;
			Eseason_0 = season;
			EseasonStatus_0 = status;
			Class443.Logger.LogTrace($"{GetType().FullName} status:{status} created");
		}

		public void Dispose()
		{
			if (Class444_0 != null && Class444_0.Interface3_0 == this)
			{
				Class444_0.Interface3_0 = null;
			}
			Class444_0 = null;
		}

		public virtual Task HandleReconnect(GInterface49 visual, ESeasonStatus status, SeasonsSettingsClass seasonsSettings)
		{
			Class443.Logger.LogTrace($"HandleReconnect from Status:{Status} to status:{status}");
			return Task.CompletedTask;
		}
	}

	public class Class446 : Class445, Interface3, IDisposable
	{
		[CompilerGenerated]
		public class Class457
		{
			public Class446 class446_0;

			public GInterface49 visual;

			public async Task method_0(Interface3 state)
			{
				class446_0.Class444_0.Interface3_0 = state;
				class446_0.Dispose();
				class446_0.Class444_0.action_1?.Invoke(state.Status);
				await state.Run();
				state.SetupVisual(visual);
				Class443.Logger.LogTrace($"state:{state.Status} started");
			}
		}

		public Class446(Class444 controller)
			: base(controller, ESeasonStatus.Summer, ESeason.Summer)
		{
		}

		public async Task Run()
		{
			Class445.RainController_0.method_2();
			SeasonsMaterialsFixer instance = MonoBehaviourSingleton<SeasonsMaterialsFixer>.Instance;
			if (!(instance != null))
			{
				Class443.Logger.LogError("SeasonsMaterialsFixer.Instance is null");
			}
			else
			{
				await instance.method_0();
				instance.FixMaterials();
				instance.Unload();
			}
			ImpostorsRenderer instance2 = MonoBehaviourSingleton<ImpostorsRenderer>.Instance;
			if (instance2 != null)
			{
				instance2.Refresh();
			}
			else
			{
				Class443.Logger.LogError("ImpostorsRenderer.Instance is null");
			}
		}

		public void StormStarted(GInterface49 visual)
		{
			Class451 @class = new Class451(base.Class444_0);
			base.Class444_0.Interface3_0 = @class;
			Dispose();
			@class.Class444_0.action_1?.Invoke(@class.Status);
			@class.SetupVisual(visual);
			Class443.Logger.LogTrace("Storm started");
		}

		public void SetupVisual(GInterface49 visual)
		{
			WeatherObstacle winterWeatherObstacle = visual.WinterWeatherObstacle;
			if (winterWeatherObstacle == null)
			{
				Class443.Logger.LogError("StateSummer obstacle is null");
				return;
			}
			UnityEngine.Object.DestroyImmediate(winterWeatherObstacle.gameObject);
			DepthPhotograper instance = DepthPhotograper.Instance;
			if (instance != null)
			{
				instance.Render();
			}
			else
			{
				Class443.Logger.LogError("DepthPhotograper not found");
			}
		}

		public override Task HandleReconnect(GInterface49 visual, ESeasonStatus status, SeasonsSettingsClass seasonsSettings)
		{
			Class457 @class = new Class457();
			@class.class446_0 = this;
			@class.visual = visual;
			Class443.Logger.LogTrace($"SummerState.HandleReconnect status:{status}");
			switch (status)
			{
			default:
				throw new ArgumentOutOfRangeException("status", status, null);
			case ESeasonStatus.Summer:
				return Task.CompletedTask;
			case ESeasonStatus.Autumn:
			{
				Class449 state6 = new Class449(base.Class444_0);
				return @class.method_0(state6);
			}
			case ESeasonStatus.Winter:
			{
				Class453 state5 = new Class453(base.Class444_0);
				return @class.method_0(state5);
			}
			case ESeasonStatus.Spring:
			{
				Class447 state4 = new Class447(base.Class444_0);
				return @class.method_0(state4);
			}
			case ESeasonStatus.AutumnLate:
			{
				Class450 state3 = new Class450(base.Class444_0);
				return @class.method_0(state3);
			}
			case ESeasonStatus.SpringEarly:
			{
				Class448 state2 = new Class448(base.Class444_0, seasonsSettings.SpringSnowFactor);
				return @class.method_0(state2);
			}
			case ESeasonStatus.Storm:
			{
				Class452 state = new Class452(base.Class444_0);
				return @class.method_0(state);
			}
			}
		}
	}

	public class Class447 : Class445, Interface3, IDisposable
	{
		public Class447(Class444 controller)
			: base(controller, ESeasonStatus.Spring, ESeason.Spring)
		{
		}

		public async Task Run()
		{
			Class445.RainController_0.method_3();
			SeasonsMaterialsFixer instance = MonoBehaviourSingleton<SeasonsMaterialsFixer>.Instance;
			if (!(instance != null))
			{
				Class443.Logger.LogError("SeasonsMaterialsFixer.Instance is null");
			}
			else
			{
				await instance.method_3();
				instance.FixMaterials();
				instance.Unload();
			}
			ImpostorsRenderer instance2 = MonoBehaviourSingleton<ImpostorsRenderer>.Instance;
			if (instance2 != null)
			{
				instance2.Refresh();
			}
			else
			{
				Class443.Logger.LogError("ImpostorsRenderer.Instance is null");
			}
		}

		public void StormStarted(GInterface49 visual)
		{
			Class451 @class = new Class451(base.Class444_0);
			base.Class444_0.Interface3_0 = @class;
			Dispose();
			@class.Class444_0.action_1?.Invoke(@class.Status);
			@class.SetupVisual(visual);
			Class443.Logger.LogTrace("Storm started");
		}

		public void SetupVisual(GInterface49 visual)
		{
			WeatherObstacle winterWeatherObstacle = visual.WinterWeatherObstacle;
			if (winterWeatherObstacle == null)
			{
				Class443.Logger.LogError("StateSpring obstacle is null");
				return;
			}
			UnityEngine.Object.DestroyImmediate(winterWeatherObstacle.gameObject);
			DepthPhotograper instance = DepthPhotograper.Instance;
			if (instance != null)
			{
				instance.Render();
			}
			else
			{
				Class443.Logger.LogError("DepthPhotograper not found");
			}
		}
	}

	public class Class448 : Class445, Interface3, IDisposable
	{
		[NonSerialized]
		public Vector3 Vector3_0;

		public Class448(Class444 controller, Vector3 springSnowFactor)
			: base(controller, ESeasonStatus.SpringEarly, ESeason.SpringEarly)
		{
			Vector3_0 = springSnowFactor;
		}

		public async Task Run()
		{
			Class445.RainController_0.method_4(Vector3_0);
			SeasonsMaterialsFixer instance = MonoBehaviourSingleton<SeasonsMaterialsFixer>.Instance;
			if (!(instance != null))
			{
				Class443.Logger.LogError("SeasonsMaterialsFixer.Instance is null");
			}
			else
			{
				await instance.method_2();
				instance.FixMaterials();
				instance.Unload();
			}
			ImpostorsRenderer instance2 = MonoBehaviourSingleton<ImpostorsRenderer>.Instance;
			if (instance2 != null)
			{
				instance2.Refresh();
			}
			else
			{
				Class443.Logger.LogError("ImpostorsRenderer.Instance is null");
			}
		}

		public void StormStarted(GInterface49 visual)
		{
			Class451 @class = new Class451(base.Class444_0);
			base.Class444_0.Interface3_0 = @class;
			Dispose();
			@class.Class444_0.action_1?.Invoke(@class.Status);
			@class.SetupVisual(visual);
			Class443.Logger.LogTrace("Storm started");
		}

		public void SetupVisual(GInterface49 visual)
		{
		}
	}

	public class Class449 : Class445, Interface3, IDisposable
	{
		public Class449(Class444 controller)
			: base(controller, ESeasonStatus.Autumn, ESeason.Autumn)
		{
		}

		public async Task Run()
		{
			Class445.RainController_0.method_5();
			SeasonsMaterialsFixer instance = MonoBehaviourSingleton<SeasonsMaterialsFixer>.Instance;
			if (!(instance != null))
			{
				Class443.Logger.LogError("SeasonsMaterialsFixer.Instance is null");
			}
			else
			{
				await instance.method_4();
				instance.FixMaterials();
				instance.Unload();
			}
			ImpostorsRenderer instance2 = MonoBehaviourSingleton<ImpostorsRenderer>.Instance;
			if (instance2 != null)
			{
				instance2.Refresh();
			}
			else
			{
				Class443.Logger.LogError("ImpostorsRenderer.Instance is null");
			}
		}

		public void StormStarted(GInterface49 visual)
		{
			Class451 @class = new Class451(base.Class444_0);
			base.Class444_0.Interface3_0 = @class;
			Dispose();
			@class.Class444_0.action_1?.Invoke(@class.Status);
			@class.SetupVisual(visual);
			Class443.Logger.LogTrace("Storm started");
		}

		public void SetupVisual(GInterface49 visual)
		{
		}
	}

	public class Class450 : Class445, Interface3, IDisposable
	{
		public Class450(Class444 controller)
			: base(controller, ESeasonStatus.AutumnLate, ESeason.AutumnLate)
		{
		}

		public async Task Run()
		{
			Class445.RainController_0.method_6();
			SeasonsMaterialsFixer instance = MonoBehaviourSingleton<SeasonsMaterialsFixer>.Instance;
			if (!(instance != null))
			{
				Class443.Logger.LogError("SeasonsMaterialsFixer.Instance is null");
			}
			else
			{
				await instance.method_5();
				instance.FixMaterials();
				instance.Unload();
			}
			ImpostorsRenderer instance2 = MonoBehaviourSingleton<ImpostorsRenderer>.Instance;
			if (instance2 != null)
			{
				instance2.Refresh();
			}
			else
			{
				Class443.Logger.LogError("ImpostorsRenderer.Instance is null");
			}
		}

		public void StormStarted(GInterface49 visual)
		{
			Class451 @class = new Class451(base.Class444_0);
			base.Class444_0.Interface3_0 = @class;
			Dispose();
			@class.Class444_0.action_1?.Invoke(@class.Status);
			@class.SetupVisual(visual);
			Class443.Logger.LogTrace("Storm started");
		}

		public void SetupVisual(GInterface49 visual)
		{
		}
	}

	public class Class451 : Class445, Interface3, IDisposable
	{
		public Class451(Class444 controller)
			: base(controller, ESeasonStatus.Storm, ESeason.Summer)
		{
			Class445.RainController_0.method_8();
		}

		public Task Run()
		{
			return Task.CompletedTask;
		}

		public void StormStarted(GInterface49 visual)
		{
			Class443.Logger.LogError("Storm already started");
		}

		public void SetupVisual(GInterface49 visual)
		{
			visual.StormSetup();
		}
	}

	public class Class452 : Class445, Interface3, IDisposable
	{
		public Class452(Class444 controller)
			: base(controller, ESeasonStatus.Storm, ESeason.Summer)
		{
			Class445.RainController_0.method_9();
		}

		public Task Run()
		{
			return Task.CompletedTask;
		}

		public void StormStarted(GInterface49 visual)
		{
			Class443.Logger.LogError("Winter already started");
		}

		public void SetupVisual(GInterface49 visual)
		{
			visual.StormSetupOnReconnect();
		}
	}

	public class Class453 : Class445, Interface3, IDisposable
	{
		public Class453(Class444 controller)
			: base(controller, ESeasonStatus.Winter, ESeason.Winter)
		{
		}

		public async Task Run()
		{
			Class445.RainController_0.method_7();
			SeasonsMaterialsFixer instance = MonoBehaviourSingleton<SeasonsMaterialsFixer>.Instance;
			if (!(instance != null))
			{
				Class443.Logger.LogError("SeasonsMaterialsFixer.Instance is null");
			}
			else
			{
				await instance.method_1();
				instance.FixMaterials();
				instance.Unload();
			}
			ImpostorsRenderer instance2 = MonoBehaviourSingleton<ImpostorsRenderer>.Instance;
			if (instance2 != null)
			{
				instance2.Refresh();
			}
			else
			{
				Class443.Logger.LogError("ImpostorsRenderer.Instance is null");
			}
		}

		public void StormStarted(GInterface49 visual)
		{
			Class443.Logger.LogError("Winter already started");
		}

		public void SetupVisual(GInterface49 visual)
		{
			WeatherObstacle winterWeatherObstacle = visual.WinterWeatherObstacle;
			if (winterWeatherObstacle == null)
			{
				Class443.Logger.LogError("WinterWeatherObstacle not found");
				return;
			}
			WeatherObstacle instance = WeatherObstacle.Instance;
			if (instance != null)
			{
				UnityEngine.Object.DestroyImmediate(instance.gameObject);
			}
			WeatherObstacle.Instance = winterWeatherObstacle;
			winterWeatherObstacle.gameObject.SetActive(value: true);
			DepthPhotograper instance2 = DepthPhotograper.Instance;
			if (instance2 != null)
			{
				instance2.Render();
			}
			else
			{
				Class443.Logger.LogError("DepthPhotograper not found");
			}
		}
	}

	public class Class454 : Class445, Interface3, IDisposable
	{
		public Class454(Class444 controller)
			: base(controller, ESeasonStatus.Summer, ESeason.Summer)
		{
		}

		public Task Run()
		{
			Class443.Logger.LogTrace("StateUnknown.Run");
			return Task.CompletedTask;
		}

		public void StormStarted(GInterface49 visual)
		{
			Class443.Logger.LogTrace("StateUnknown.StormStarted");
		}

		public void SetupVisual(GInterface49 visual)
		{
			Class443.Logger.LogTrace("StateUnknown.SetupVisual");
		}
	}

	public class Class455 : Class445, Interface3, IDisposable
	{
		public Class455(Class444 controller, ESeasonStatus status, ESeason season)
			: base(controller, status, season)
		{
		}

		public Task Run()
		{
			Class443.Logger.LogTrace("StateFactoryDefault.Run");
			return Task.CompletedTask;
		}

		public void StormStarted(GInterface49 visual)
		{
			Class443.Logger.LogTrace("StateFactoryDefault.StormStarted");
		}

		public void SetupVisual(GInterface49 visual)
		{
			Class443.Logger.LogTrace("StateFactoryDefault.SetupVisual");
		}
	}

	public class Class456 : Class445, Interface3, IDisposable
	{
		public Class456(Class444 controller)
			: base(controller, ESeasonStatus.Winter, ESeason.Winter)
		{
		}

		public Task Run()
		{
			Class443.Logger.LogTrace("StateFactoryWinter.Run");
			FactoryWinterController.Instance.method_0();
			return Task.CompletedTask;
		}

		public void StormStarted(GInterface49 visual)
		{
			Class443.Logger.LogTrace("StateFactoryWinter.StormStarted");
		}

		public void SetupVisual(GInterface49 visual)
		{
			Class443.Logger.LogTrace("StateFactoryWinter.SetupVisual");
		}
	}

	[NonSerialized]
	public Interface3 Interface3_0;

	[CompilerGenerated]
	private Action<ESeasonStatus> action_1;

	[NonSerialized]
	public GInterface49 Ginterface49_0;

	[NonSerialized]
	public Action Action_2;

	public ESeason Season => Interface3_0?.Season ?? ESeason.Summer;

	public ESeasonStatus Status => Interface3_0?.Status ?? ESeasonStatus.Summer;

	public float SnowLevelOnTerrain => RainController.Wetting;

	public event Action<ESeasonStatus> StatusChangedEvent
	{
		[CompilerGenerated]
		add
		{
			Action<ESeasonStatus> action = action_1;
			Action<ESeasonStatus> action2;
			do
			{
				action2 = action;
				Action<ESeasonStatus> value2 = (Action<ESeasonStatus>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<ESeasonStatus> action = action_1;
			Action<ESeasonStatus> action2;
			do
			{
				action2 = action;
				Action<ESeasonStatus> value2 = (Action<ESeasonStatus>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<float> SnowLevelOnTerrainChangedEvent
	{
		add
		{
			RainController.WettingChangedEvent += value;
		}
		remove
		{
			RainController.WettingChangedEvent -= value;
		}
	}

	public void Dispose()
	{
		Interface3 @interface = Interlocked.Exchange(ref Interface3_0, null);
		if (@interface != null)
		{
			@interface.Dispose();
			Class443.Logger.LogTrace("SeasonsController disposed");
			Interlocked.Exchange(ref Action_2, null)?.Invoke();
		}
	}

	public async Task Run(ESeason season, SeasonsSettingsClass seasonsSettings)
	{
		Class443.Logger.LogTrace($"SeasonsController.Run begin season:{season}");
		await method_0(season, seasonsSettings);
		do
		{
			if (Class443.Controller != null && Singleton<BotEventHandler>.Instance != null && Singleton<BackendConfigSettingsClass>.Instance != null)
			{
				Action_2 = GlobalEventHandlerClass.Instance.SubscribeOnEvent<StormStartedEvent>(method_1);
				Class443.smethod_0(this);
				action_1?.Invoke(Interface3_0.Status);
				Class443.Logger.LogTrace("SeasonsController.Run complete");
				return;
			}
			await Task.Yield();
		}
		while (Interface3_0 != null);
		Class443.Logger.LogInfo("SeasonsController.Run aborted");
	}

	public Task method_0(ESeason season, SeasonsSettingsClass seasonsSettings)
	{
		if (WeatherController.Instance == null)
		{
			if (FactoryWinterController.Instance == null)
			{
				Interface3_0 = new Class454(this);
				return Task.CompletedTask;
			}
			ESeasonStatus eSeasonStatus;
			switch (season)
			{
			case ESeason.Winter:
				return ((Class456)(Interface3_0 = new Class456(this))).Run();
			case ESeason.Summer:
				eSeasonStatus = ESeasonStatus.Summer;
				break;
			case ESeason.Autumn:
				eSeasonStatus = ESeasonStatus.Autumn;
				break;
			default:
				throw new ArgumentOutOfRangeException("season", season, null);
			case ESeason.Spring:
				eSeasonStatus = ESeasonStatus.Spring;
				break;
			case ESeason.AutumnLate:
				eSeasonStatus = ESeasonStatus.AutumnLate;
				break;
			case ESeason.SpringEarly:
				eSeasonStatus = ESeasonStatus.SpringEarly;
				break;
			}
			ESeasonStatus status = eSeasonStatus;
			return ((Class455)(Interface3_0 = new Class455(this, status, season))).Run();
		}
		return season switch
		{
			ESeason.Summer => ((Class446)(Interface3_0 = new Class446(this))).Run(), 
			ESeason.Autumn => ((Class449)(Interface3_0 = new Class449(this))).Run(), 
			ESeason.Winter => ((Class453)(Interface3_0 = new Class453(this))).Run(), 
			ESeason.Spring => ((Class447)(Interface3_0 = new Class447(this))).Run(), 
			ESeason.AutumnLate => ((Class450)(Interface3_0 = new Class450(this))).Run(), 
			ESeason.SpringEarly => ((Class448)(Interface3_0 = new Class448(this, seasonsSettings.SpringSnowFactor))).Run(), 
			_ => throw new ArgumentOutOfRangeException("season", season, null), 
		};
	}

	public Task HandleReconnect(ESeasonStatus seasonStatus, SeasonsSettingsClass seasonsSettings)
	{
		Class443.Logger.LogTrace($"ReconnectHandler SeasonStatus:{seasonStatus}");
		return Interface3_0.HandleReconnect(Ginterface49_0, seasonStatus, seasonsSettings);
	}

	public void method_1(StormStartedEvent stormStartedEvent)
	{
		Class443.Logger.LogTrace("StormStartedHandler");
		Interface3_0.StormStarted(Ginterface49_0);
	}

	public void Register(GInterface49 winterVisual)
	{
		if (Ginterface49_0 != null)
		{
			Class443.Logger.LogError("WinterVisual double registration");
		}
		Ginterface49_0 = winterVisual;
		Class443.Logger.LogTrace("Register winterVisual complete");
		Interface3_0.SetupVisual(Ginterface49_0);
	}

	public void Unregister(GInterface49 winterVisual)
	{
		if (Ginterface49_0 == winterVisual)
		{
			Class443.Logger.LogTrace("Visual unregister succeed");
		}
		else
		{
			Class443.Logger.LogError("Visual unregister failed");
		}
		Ginterface49_0 = null;
	}
}
