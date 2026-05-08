using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Diz.DependencyManager;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public abstract class GClass1857
{
	[Serializable]
	[CompilerGenerated]
	public class Class1145
	{
		public static readonly Class1145 class1145_0 = new Class1145();

		public static Func<global::DependencyGraphClass<IEasyBundle>.GClass1661, Task> func_0;

		public Task method_0(global::DependencyGraphClass<IEasyBundle>.GClass1661 x)
		{
			return x.LoadingJob;
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class1146<T> where T : UnityEngine.Object
	{
		public static readonly Class1146<T> class1146_0 = new Class1146<T>();

		public static Func<UnityEngine.Object, bool> func_0;

		public bool method_0(UnityEngine.Object a)
		{
			if (!(a is T))
			{
				if (a is GameObject gameObject)
				{
					return gameObject.GetComponent<T>() != null;
				}
				return false;
			}
			return true;
		}
	}

	[CompilerGenerated]
	public class Class1147<T> where T : UnityEngine.Object
	{
		public string assetName;

		public bool method_0(UnityEngine.Object x)
		{
			return string.Equals(x.name, assetName, StringComparison.InvariantCultureIgnoreCase);
		}
	}

	public static T InstantiateAsset<T>(this IEasyAssets easyAssets, string key) where T : UnityEngine.Object
	{
		T val = UnityEngine.Object.Instantiate(GetAsset<T>(easyAssets, key));
		val.name = val.name.Replace("(Clone)", string.Empty);
		return val;
	}

	public static T InstantiateAsset<T>(this IEasyAssets easyAssets, ResourceKey key, Transform parent = null) where T : UnityEngine.Object
	{
		T val = UnityEngine.Object.Instantiate(GetAsset<T>(easyAssets, key.path, key.rcid), parent);
		val.name = val.name.Replace("(Clone)", string.Empty);
		return val;
	}

	[CanBeNull]
	public static T GetAsset<T>(this IEasyAssets easyAssets, ResourceKey key) where T : UnityEngine.Object
	{
		return GetAsset<T>(easyAssets, key.path, key.rcid);
	}

	[CanBeNull]
	public static UnityEngine.Object GetDefaultAsset(this IEasyAssets easyAssets)
	{
		return easyAssets.System.GetDefaultNode().Data.Assets.FirstOrDefault();
	}

	[CanBeNull]
	public static T GetAsset<T>(this IEasyAssets easyAssets, string key, string assetName = null) where T : UnityEngine.Object
	{
		IEasyBundle data = easyAssets.System.GetNode(key).Data;
		if (data.LoadState.Value == ELoadState.Failed)
		{
			return (T)easyAssets.System.GetDefaultNode().Data.Assets.FirstOrDefault();
		}
		if (data.LoadState.Value != ELoadState.Loaded)
		{
			throw new Exception(key + " is not loaded. You should load it first.");
		}
		UnityEngine.Object obj = ((!string.IsNullOrEmpty(assetName)) ? data.Assets.FirstOrDefault((UnityEngine.Object x) => string.Equals(x.name, assetName, StringComparison.InvariantCultureIgnoreCase)) : ((!(data.SameNameAsset != null) || !(data.SameNameAsset is T)) ? data.Assets.FirstOrDefault((UnityEngine.Object a) => a is T || (a is GameObject gameObject && gameObject.GetComponent<T>() != null)) : data.SameNameAsset));
		T obj2 = ((obj is T) ? ((T)obj) : ((obj is GameObject) ? ((GameObject)obj).GetComponent<T>() : null));
		if (obj2 == null)
		{
			Debug.LogError("Error: could not find component " + typeof(T)?.ToString() + " on object " + obj);
		}
		return obj2;
	}

	public static bool TryGetAsset<T>(this IEasyAssets easyAssets, out T asset, string key, string assetName = null) where T : UnityEngine.Object
	{
		asset = null;
		if (!IsAssetLoaded(easyAssets, key))
		{
			Debug.LogWarning("Bundle " + key + " not loaded");
			return false;
		}
		asset = GetAsset<T>(easyAssets, key, assetName);
		return asset != null;
	}

	public static global::DependencyGraphClass<IEasyBundle>.GClass1661 Retain(this IEasyAssets easyAssets, string key, [CanBeNull] Func<string, bool> dependencyPredicate, IProgress<float> progress)
	{
		return easyAssets.System.Retain(key, progress);
	}

	public static global::DependencyGraphClass<IEasyBundle>.GClass1661 Retain(this IEasyAssets easyAssets, IEnumerable<string> keys, IProgress<float> progress = null, CancellationToken ct = default(CancellationToken))
	{
		return easyAssets.System.Retain(keys, progress, ct);
	}

	public static global::DependencyGraphClass<IEasyBundle>.GClass1660 RetainSeparate(this IEasyAssets easyAssets, IEnumerable<string> keys, IProgress<float> progress = null, CancellationToken ct = default(CancellationToken))
	{
		return easyAssets.System.RetainSeparate(keys, progress, ct);
	}

	public static Task<global::DependencyGraphClass<IEasyBundle>.GClass1660> RetainSeparateTask(this IEasyAssets easyAssets, IEnumerable<string> keys, IProgress<float> progress = null, CancellationToken ct = default(CancellationToken))
	{
		return easyAssets.System.RetainSeparateAsync(keys, progress, ct);
	}

	public static async void WaitForAllBundles(global::DependencyGraphClass<IEasyBundle>.GClass1661[] bundles, Action onFinished, CancellationToken cancellationToken = default(CancellationToken), IProgress<float> progress = null)
	{
		await smethod_0(bundles, onFinished, cancellationToken, progress);
	}

	public static async void WaitForAllBundles(global::DependencyGraphClass<IEasyBundle>.GClass1661 bundles, Action onFinished, CancellationToken cancellationToken = default(CancellationToken), IProgress<float> progress = null)
	{
		await WaitForAllBundlesJob(bundles, onFinished, cancellationToken, progress);
	}

	public static async Task smethod_0(global::DependencyGraphClass<IEasyBundle>.GClass1661[] bundles, Action onFinished, CancellationToken cancellationToken = default(CancellationToken), IProgress<float> progress = null)
	{
		await LoadBundles(bundles);
		if (!cancellationToken.IsCancellationRequested)
		{
			onFinished();
		}
	}

	public static async Task WaitForAllBundlesJob(global::DependencyGraphClass<IEasyBundle>.GClass1661 bundles, Action onFinished, CancellationToken cancellationToken = default(CancellationToken), IProgress<float> progress = null)
	{
		await LoadBundles(bundles);
		if (!cancellationToken.IsCancellationRequested)
		{
			onFinished();
		}
	}

	public static Task LoadBundles(global::DependencyGraphClass<IEasyBundle>.GClass1661[] bundles)
	{
		return Task.WhenAll(bundles.Select((global::DependencyGraphClass<IEasyBundle>.GClass1661 x) => x.LoadingJob).ToArray());
	}

	public static Task LoadBundles(global::DependencyGraphClass<IEasyBundle>.GClass1661 bundles)
	{
		return bundles.LoadingJob;
	}

	public static bool IsAssetLoaded(this IEasyAssets easyAssets, string key)
	{
		return easyAssets.System.GetNode(key).Data.LoadState.Value == ELoadState.Loaded;
	}
}
