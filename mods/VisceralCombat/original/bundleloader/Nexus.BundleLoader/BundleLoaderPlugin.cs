using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BepInEx;
using UnityEngine;

namespace Nexus.BundleLoader;

[BepInPlugin("com.pandahhcorp.bundleloader", "BundleLoader", "1.0.0")]
public class BundleLoaderPlugin : BaseUnityPlugin
{
	public Dictionary<string, AssetBundleCreateRequest> _loadedBundles;

	public static BundleLoaderPlugin Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		string path = Path.Combine(Environment.CurrentDirectory, "BepInEx", "plugins", "ssh", "Bundles");
		if (Directory.Exists(path))
		{
			_loadedBundles = (from f in Directory.GetFiles(path)
				where f.ToLower().EndsWith(".bundle") || f.ToLower().EndsWith(".servph")
				select f).ToDictionary((Func<string, string>)((string s) => Path.GetFileNameWithoutExtension(s).ToLower()), (Func<string, AssetBundleCreateRequest>)AssetBundle.LoadFromFileAsync);
		}
		else
		{
			Directory.CreateDirectory(path);
			_loadedBundles = new Dictionary<string, AssetBundleCreateRequest>();
		}
		((BaseUnityPlugin)this).Logger.LogInfo((object)$"Loaded {_loadedBundles.Count} bundles...");
	}

	public bool IsLoading(string bundleName, out bool isFinished)
	{
		isFinished = false;
		if (!_loadedBundles.TryGetValue(bundleName, out var value))
		{
			return false;
		}
		isFinished = ((AsyncOperation)value).isDone;
		return true;
	}

	public AssetBundle GetAssetBundle(string bundleName)
	{
		if (_loadedBundles.TryGetValue(bundleName, out var value) && ((AsyncOperation)value).isDone)
		{
			return value.assetBundle;
		}
		return null;
	}

	public async Task<AssetBundle> GetAssetBundleAsync(string bundleName, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_loadedBundles.TryGetValue(bundleName, out var request))
		{
			return null;
		}
		while (!((AsyncOperation)request).isDone && !cancellationToken.CanBeCanceled)
		{
			await Task.Yield();
		}
		return cancellationToken.IsCancellationRequested ? null : request.assetBundle;
	}

	public IEnumerator GetAssetBundleWaitForLoad(string bundleName)
	{
		if (_loadedBundles.TryGetValue(bundleName, out var request))
		{
			while (!((AsyncOperation)request).isDone)
			{
				yield return null;
			}
			yield return request.assetBundle;
		}
	}
}
