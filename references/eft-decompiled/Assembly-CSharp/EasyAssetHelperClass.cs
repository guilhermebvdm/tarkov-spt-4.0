using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Diz.DependencyManager;
using Diz.Jobs;
using UnityEngine;
using UnityEngine.Build.Pipeline;

public class EasyAssetHelperClass : IEasyBundle, GInterface161
{
	[NonSerialized]
	public string String_0;

	[NonSerialized]
	public IBundleLock IBundleLock;

	[NonSerialized]
	public string String_1;

	[NonSerialized]
	[CompilerGenerated]
	public IEnumerable<string> Ienumerable_0;

	[NonSerialized]
	[CompilerGenerated]
	public string String_2;

	[NonSerialized]
	public AssetBundle AssetBundle_0;

	[NonSerialized]
	public AssetBundleRequest AssetBundleRequest_0;

	[NonSerialized]
	[CompilerGenerated]
	public UnityEngine.Object[] Object_0;

	[NonSerialized]
	[CompilerGenerated]
	public UnityEngine.Object Object_1;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_0;

	[NonSerialized]
	[CompilerGenerated]
	public global::BindableStateClass<ELoadState> Gclass1643_0;

	[NonSerialized]
	public Task Task_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public Func<string, Task> Func_0;

	public IEnumerable<string> DependencyKeys
	{
		[CompilerGenerated]
		get
		{
			return Ienumerable_0;
		}
		[CompilerGenerated]
		set
		{
			Ienumerable_0 = value;
		}
	}

	public string Key
	{
		[CompilerGenerated]
		get
		{
			return String_2;
		}
		[CompilerGenerated]
		set
		{
			String_2 = value;
		}
	}

	public UnityEngine.Object[] Assets
	{
		[CompilerGenerated]
		get
		{
			return Object_0;
		}
		[CompilerGenerated]
		set
		{
			Object_0 = value;
		}
	}

	public UnityEngine.Object SameNameAsset
	{
		[CompilerGenerated]
		get
		{
			return Object_1;
		}
		[CompilerGenerated]
		set
		{
			Object_1 = value;
		}
	}

	public float Progress
	{
		[CompilerGenerated]
		get
		{
			return Float_0;
		}
		[CompilerGenerated]
		set
		{
			Float_0 = value;
		}
	}

	public global::BindableStateClass<ELoadState> LoadState
	{
		[CompilerGenerated]
		get
		{
			return Gclass1643_0;
		}
		[CompilerGenerated]
		set
		{
			Gclass1643_0 = value;
		}
	}

	public EasyAssetHelperClass(string key, string rootPath, CompatibilityAssetBundleManifest manifest, IBundleLock bundleLock, Func<string, Task> bundleCheck)
	{
		Key = key;
		String_1 = rootPath + key;
		String_0 = Path.GetFileNameWithoutExtension(key);
		DependencyKeys = manifest.GetDirectDependencies(key);
		LoadState = new global::BindableStateClass<ELoadState>(ELoadState.Unloaded);
		IBundleLock = bundleLock;
		Func_0 = bundleCheck;
	}

	public void Load()
	{
		if (LoadState.Value != ELoadState.Unloaded && LoadState.Value != ELoadState.Failed)
		{
			throw new InvalidOperationException("Bundle is already in " + LoadState.Value.ToString() + " state");
		}
		Bool_0 = true;
		Progress = 0f;
		LoadState.Value = ELoadState.Loading;
		if (Task_0 == null)
		{
			Task_0 = method_0();
		}
	}

	public async Task method_0()
	{
		while (true)
		{
			if (IBundleLock.IsLocked)
			{
				if (!Bool_0)
				{
					break;
				}
				await Task.Yield();
				continue;
			}
			if (!Bool_0)
			{
				Task_0 = null;
				return;
			}
			IBundleLock.Lock();
			if (Func_0 != null)
			{
				await Func_0(Key);
			}
			AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(String_1);
			if (assetBundleCreateRequest == null)
			{
				GClass716.Instance.LogError("Error: could not load bundle: " + Key);
				LoadState.Value = ELoadState.Failed;
				IBundleLock.Unlock();
				Task_0 = null;
				return;
			}
			while (!assetBundleCreateRequest.isDone)
			{
				Progress = assetBundleCreateRequest.progress * 0.5f;
				await JobScheduler.Yield();
			}
			AssetBundle assetBundle = assetBundleCreateRequest.assetBundle;
			if (assetBundle == null)
			{
				GClass716.Instance.LogError("Error: could not load bundle: " + Key);
				LoadState.Value = ELoadState.Failed;
				IBundleLock.Unlock();
				Progress = 0f;
				Task_0 = null;
				return;
			}
			if (!Bool_0)
			{
				assetBundle.Unload(unloadAllLoadedObjects: true);
				LoadState.Value = ELoadState.Unloaded;
				IBundleLock.Unlock();
				Progress = 0f;
				Task_0 = null;
				return;
			}
			AssetBundle_0 = assetBundle;
			Progress = 0.5f;
			AssetBundleRequest_0 = AssetBundle_0.LoadAllAssetsAsync();
			while (!AssetBundleRequest_0.isDone)
			{
				Progress = 0.5f + AssetBundleRequest_0.progress * 0.4f;
				await JobScheduler.Yield();
			}
			AssetBundleRequest assetBundleRequest_ = AssetBundleRequest_0;
			AssetBundleRequest_0 = null;
			if (!Bool_0)
			{
				IBundleLock.Unlock();
				AssetBundle_0.Unload(unloadAllLoadedObjects: true);
				LoadState.Value = ELoadState.Unloaded;
				AssetBundle_0 = null;
				Progress = 0f;
				Task_0 = null;
				return;
			}
			Progress = 0.9f;
			Assets = assetBundleRequest_.allAssets;
			for (int i = 0; i < Assets.Length; i++)
			{
				if (Assets[i].name == String_0)
				{
					SameNameAsset = Assets[i];
					break;
				}
				await JobScheduler.Yield();
				if (!Bool_0)
				{
					IBundleLock.Unlock();
					Progress = 0f;
					Task_0 = null;
					return;
				}
			}
			LoadState.Value = ELoadState.Loaded;
			IBundleLock.Unlock();
			Progress = 1f;
			Task_0 = null;
			return;
		}
		Task_0 = null;
	}

	public void Unload()
	{
		if (!Bool_0)
		{
			throw new InvalidOperationException("Bundle is already in " + LoadState.Value.ToString() + " state");
		}
		Bool_0 = false;
		Progress = 0f;
		SameNameAsset = null;
		if (AssetBundleRequest_0 == null)
		{
			if (AssetBundle_0 != null)
			{
				AssetBundle_0.Unload(unloadAllLoadedObjects: true);
				AssetBundle_0 = null;
			}
			LoadState.Value = ELoadState.Unloaded;
		}
		else
		{
			LoadState.Value = ELoadState.Unloading;
		}
		Assets = null;
	}
}
