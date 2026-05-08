using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GClass2287 : Operation
{
	public class Class395 : LoggerClass
	{
		public Class395()
			: base("scenes", LoggerMode.Add)
		{
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class1459
	{
		public static readonly Class1459 class1459_0 = new Class1459();

		public static Func<ScenesPreset, bool> func_0;

		public bool method_0(ScenesPreset child)
		{
			return child != null;
		}
	}

	[NonSerialized]
	public static Class395 Class395_0 = new Class395();

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	public IAssetsManager IAssetsManager;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public bool Bool_3;

	[NonSerialized]
	public CancellationToken CancellationToken_0;

	[NonSerialized]
	public IProgress<float> Iprogress_0;

	[NonSerialized]
	public List<GClass3968> List_0 = new List<GClass3968>();

	[NonSerialized]
	public List<GClass2287> List_1 = new List<GClass2287>();

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public int Int_0;

	public bool IsCanceled
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

	public static GClass2287 smethod_0(ServerScenesDataStruct preset, IAssetsManager assetsManager, bool loadFirstAsSingle, bool loadInParallelMode, bool allowSceneActivation, CancellationToken cancellationToken = default(CancellationToken), IProgress<float> progress = null)
	{
		GClass2287 obj = new GClass2287
		{
			IAssetsManager = assetsManager,
			Bool_1 = loadFirstAsSingle,
			Bool_2 = loadInParallelMode,
			Bool_3 = allowSceneActivation,
			CancellationToken_0 = cancellationToken,
			Iprogress_0 = progress
		};
		obj.method_0(preset).HandleExceptions();
		return obj;
	}

	public static GClass2287 smethod_1(ScenesPreset preset, IAssetsManager assetsManager, bool loadFirstAsSingle, bool loadInParallelMode, bool allowSceneActivation, CancellationToken cancellationToken = default(CancellationToken), IProgress<float> progress = null)
	{
		GClass2287 obj = new GClass2287
		{
			IAssetsManager = assetsManager,
			Bool_1 = loadFirstAsSingle,
			Bool_2 = loadInParallelMode,
			Bool_3 = allowSceneActivation,
			CancellationToken_0 = cancellationToken,
			Iprogress_0 = progress
		};
		obj.method_1(preset).HandleExceptions();
		return obj;
	}

	public async Task method_0(ServerScenesDataStruct preset)
	{
		Float_0 = 0f;
		Iprogress_0?.Report(0f);
		IOperation<object> operation = await AssetsManagerSingletonClass.Manager.LoadAssetAsync(preset.key);
		if (operation.Succeed)
		{
			ScenesPreset scenesPreset = operation.Result as ScenesPreset;
			if (scenesPreset != null)
			{
				scenesPreset.DisableServerScenes(preset.DisableServerScenes);
				Int_0 = scenesPreset.GetTotalSceneCount();
				Iprogress_0 = GClass1521.Select(Iprogress_0, delegate(float x)
				{
					Float_0 += x;
					Float_0 = Mathf.Clamp(Float_0, 0f, Int_0);
					return Float_0 / (float)Int_0;
				});
			}
			await method_1(scenesPreset);
		}
		else
		{
			Class395_0.LogError(operation.Error);
			Error = operation.Error;
		}
	}

	public async Task method_1(ScenesPreset preset)
	{
		GClass640.LogMemoryConsumption("ScenesPreset load {");
		if (preset == null)
		{
			method_6("Invalid preset");
			return;
		}
		if (CancellationToken_0.IsCancellationRequested)
		{
			method_7();
			return;
		}
		Class395_0.LogInfo("Begin loading preset: {0}", preset);
		int asyncUploadTimeSlice = QualitySettings.asyncUploadTimeSlice;
		int asyncUploadBufferSize = QualitySettings.asyncUploadBufferSize;
		QualitySettings.asyncUploadTimeSlice *= 2;
		QualitySettings.asyncUploadBufferSize *= 2;
		await method_2(preset.ScenesResourceKeys, Iprogress_0);
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		Class395_0.LogInfo("Begin scenes activation: {0}", preset);
		foreach (GClass3968 item in List_0)
		{
			try
			{
				await item;
			}
			catch (Exception)
			{
				Debug.LogError(item.Info);
			}
			if (!Bool_3)
			{
				continue;
			}
			try
			{
				await item.ActivateScene();
				if (Iprogress_0 != null)
				{
					Iprogress_0.Report(0.5f);
				}
			}
			catch (Exception)
			{
				Debug.LogError(item.Info);
				if (Iprogress_0 != null)
				{
					Iprogress_0.Report(0.5f);
				}
			}
		}
		QualitySettings.asyncUploadTimeSlice = asyncUploadTimeSlice;
		QualitySettings.asyncUploadBufferSize = asyncUploadBufferSize;
		Class395_0.LogInfo("Complete scenes activation: {0}, time: {1} s", preset, Time.realtimeSinceStartup - realtimeSinceStartup);
		foreach (ScenesPreset item2 in preset.ChildPresets.Where((ScenesPreset child) => child != null))
		{
			if (!CancellationToken_0.IsCancellationRequested)
			{
				GClass2287 gClass = smethod_1(item2, IAssetsManager, loadFirstAsSingle: false, Bool_2, Bool_3, CancellationToken_0, Iprogress_0);
				List_1.Add(gClass);
				await gClass;
				continue;
			}
			method_7();
			return;
		}
		Class395_0.LogInfo("Complete loading preset: {0}, time: {1} s", preset, Time.realtimeSinceStartup - realtimeSinceStartup);
		if (CancellationToken_0.IsCancellationRequested)
		{
			method_7();
			return;
		}
		int sceneCount = SceneManager.sceneCount;
		string text = "";
		Scene activeScene = SceneManager.GetActiveScene();
		for (int num = 0; num < sceneCount; num++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(num);
			text = text + sceneAt.name + ((activeScene == sceneAt) ? " - ACTIVE SCENE\n" : "\n");
		}
		Class395_0.LogInfo(text);
		if (!string.IsNullOrEmpty(preset.ActiveSceneName))
		{
			Scene sceneByName = SceneManager.GetSceneByName(preset.ActiveSceneName);
			if (sceneByName.IsValid())
			{
				SceneManager.SetActiveScene(sceneByName);
			}
		}
		string text2 = method_5();
		if (!string.IsNullOrEmpty(text2))
		{
			method_6(text2);
			return;
		}
		Succeed = true;
		GClass640.LogMemoryConsumption("ScenesPreset load }");
	}

	public async Task method_2(IList<ResourceKey> scenes, IProgress<float> progress = null)
	{
		if (scenes.Count != 0)
		{
			LoadSceneMode loadSceneMode = ((!Bool_1) ? LoadSceneMode.Additive : LoadSceneMode.Single);
			GClass3968 gClass = LoadSceneClass.LoadScene(IAssetsManager, scenes[0], loadSceneMode, Bool_1 || Bool_3);
			List_0.Add(gClass);
			await gClass;
			if (Iprogress_0 != null)
			{
				Iprogress_0.Report(0.5f);
			}
			if (Bool_2)
			{
				await method_3(scenes, progress);
			}
			else
			{
				await method_4(scenes, progress);
			}
		}
	}

	public async Task method_3(IList<ResourceKey> scenes, IProgress<float> progress = null)
	{
		for (int i = 1; i < scenes.Count; i++)
		{
			ResourceKey resourceKey = scenes[i];
			GClass3968 item = LoadSceneClass.LoadScene(IAssetsManager, resourceKey, LoadSceneMode.Additive, allowSceneActivation: false);
			List_0.Add(item);
		}
		for (int j = 1; j < List_0.Count; j++)
		{
			await List_0[j];
			if (progress != null && !CancellationToken_0.IsCancellationRequested)
			{
				progress.Report(Bool_3 ? 0.5f : 1f);
			}
		}
	}

	public async Task method_4(IList<ResourceKey> scenes, IProgress<float> progress = null)
	{
		for (int i = 1; i < scenes.Count; i++)
		{
			if (CancellationToken_0.IsCancellationRequested)
			{
				break;
			}
			ResourceKey resourceKey = scenes[i];
			GClass3968 gClass = LoadSceneClass.LoadScene(IAssetsManager, resourceKey, LoadSceneMode.Additive, allowSceneActivation: false);
			List_0.Add(gClass);
			await gClass;
			if (progress != null && !CancellationToken_0.IsCancellationRequested)
			{
				progress.Report(Bool_3 ? 0.5f : 1f);
			}
		}
	}

	public string method_5()
	{
		string text = "";
		for (int i = 0; i < List_0.Count; i++)
		{
			GClass3968 gClass = List_0[i];
			if (gClass.Failed)
			{
				text = text + gClass.Error + "; ";
			}
		}
		return text;
	}

	public void method_6(string error)
	{
		Class395_0.LogError(error);
		Error = error;
	}

	public void method_7()
	{
		Error = "Cancelled";
		IsCanceled = true;
		Class395_0.LogInfo(Error);
	}

	[CompilerGenerated]
	public float method_8(float x)
	{
		Float_0 += x;
		Float_0 = Mathf.Clamp(Float_0, 0f, Int_0);
		return Float_0 / (float)Int_0;
	}
}
