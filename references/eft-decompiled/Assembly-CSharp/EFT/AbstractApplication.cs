using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Comfort.Common;
using Diz.Jobs;
using Diz.Utils;
using EFT.Animals;
using EFT.Interactive;
using EFT.Visual;
using MultiFlare;
using UnityEngine;
using UnityEngine.Assertions;

namespace EFT;

[GAttribute8(-3000)]
public abstract class AbstractApplication : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	public class Class1097
	{
		public static readonly Class1097 class1097_0 = new Class1097();

		public static RemoteCertificateValidationCallback remoteCertificateValidationCallback_0;

		public bool method_0(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}
	}

	public GClass718 Logger;

	private static bool bool_0;

	[CompilerGenerated]
	private static bool bool_1;

	[CompilerGenerated]
	private bool bool_2;

	public abstract EUpdateQueue PlayerUpdateQueue { get; }

	public static bool Initialized
	{
		[CompilerGenerated]
		get
		{
			return bool_1;
		}
		[CompilerGenerated]
		set
		{
			bool_1 = value;
		}
	}

	public bool Destroyed
	{
		[CompilerGenerated]
		get
		{
			return bool_2;
		}
		[CompilerGenerated]
		set
		{
			bool_2 = value;
		}
	}

	public virtual void Awake()
	{
		if (Initialized)
		{
			Debug.LogWarning("Application already instantiated");
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		Initialized = true;
		CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
		if (BackendConfigAbstractClass.Config == null)
		{
			BackendConfigAbstractClass.LoadApplicationConfig(new ApplicationConfigClass());
		}
		ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
		if (!Singleton<LogConfiguratorAbstractClass>.Instantiated)
		{
			Singleton<LogConfiguratorAbstractClass>.Create(CreateLogConfigurator());
		}
		Logger = new GClass718(LoggerMode.Add);
		MemoryControllerClass.Logger = delegate(string s)
		{
			Logger.LogDebug(s);
		};
		JobScheduler jobScheduler = base.gameObject.AddComponent<JobScheduler>();
		jobScheduler.SetTargetFrameRate(Application.targetFrameRate);
		if (BackendConfigAbstractClass.Config.LoadForceModeMultiplier > 0f)
		{
			jobScheduler.DefaultForceModeMultiplier = BackendConfigAbstractClass.Config.LoadForceModeMultiplier;
		}
		Singleton<JobScheduler>.Create(jobScheduler);
		jobScheduler.Init(BackendConfigAbstractClass.Config.Pools.ContinuationProfilerEnabled);
		Singleton<AsyncWorker>.Create(base.gameObject.AddComponent<AsyncWorker>());
		CreateTechnicalSystems();
		if (!Singleton<BotEventHandler>.Instantiated)
		{
			Singleton<BotEventHandler>.Create(new BotEventHandler());
		}
		GlobalEventHandlerClass.Instance.Initialize();
		new GClass3580();
		smethod_0();
		Logger.LogInfo("Application awaken, updateQueue:'{0}'", PlayerUpdateQueue);
		Logger.LogInfo("Assert.raiseExceptions:'{0}'", Assert.raiseExceptions);
		UnityEngine.Object.DontDestroyOnLoad(this);
		if (BackendConfigAbstractClass.Config.Physics.ManualUpdate)
		{
			EFTPhysicsClass.GClass745.Enabled = true;
		}
		if (GClass1801.Validate())
		{
			Logger.LogInfo("Application obfuscation succeed.");
		}
		else
		{
			Logger.LogError("Application obfuscation failed, validation class name not changed.");
		}
		GClass1800.Run();
	}

	public static void CreateTechnicalSystems()
	{
		if (bool_0)
		{
			return;
		}
		Dictionary<string, Action<GameObject>> obj = new Dictionary<string, Action<GameObject>>
		{
			{
				"muzzle",
				ComponentSystem<MuzzleManager, MuzzleSystem>.Register
			},
			{
				"light",
				ComponentSystem<BaseLight, BaseLightSystem>.Register
			},
			{
				"flicker",
				ComponentSystem<Flicker, FlickerSystem>.Register
			},
			{
				"lamp",
				ComponentSystem<LampController, LampSystem>.Register
			},
			{
				"overheat",
				ComponentSystem<WeaponPrefab, WeaponOverHeatSystem>.Register
			},
			{
				"bird",
				ComponentSystem<BirdCurveBrain, BirdCurveBrainSystem>.Register
			},
			{
				"hobo",
				ComponentSystem<DisablerCullingObjectBase, HoboCullingManager>.Register
			},
			{
				"floating",
				ComponentSystem<FloatingObject, FloatingObjectManager>.Register
			}
		};
		GameObject gameObject = new GameObject("[Technical Systems]");
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		foreach (KeyValuePair<string, Action<GameObject>> item in obj)
		{
			item.Value(gameObject);
		}
		gameObject.AddComponent<FlareScheduler>();
		bool_0 = true;
	}

	public abstract LogConfiguratorAbstractClass CreateLogConfigurator();

	public static void smethod_0()
	{
		Singleton<GClass1357>.Create(GClass1357.Create(LoggerMode.Add));
	}

	public virtual void OnDestroy()
	{
		if (Singleton<GClass1357>.Instantiated)
		{
			Singleton<GClass1357>.Release(Singleton<GClass1357>.Instance);
		}
		if (Singleton<LogConfiguratorAbstractClass>.Instantiated)
		{
			Singleton<LogConfiguratorAbstractClass>.Instance.Shutdown();
			Singleton<LogConfiguratorAbstractClass>.Release(Singleton<LogConfiguratorAbstractClass>.Instance);
		}
		Destroy();
	}

	public virtual void Destroy()
	{
		Destroyed = true;
	}

	public virtual void FixedUpdate()
	{
		EFTPhysicsClass.FixedUpdate();
	}

	public virtual void Update()
	{
		EFTPhysicsClass.Update();
	}

	public AbstractApplication()
	{
	}

	[CompilerGenerated]
	public void method_0(string s)
	{
		Logger.LogDebug(s);
	}
}
