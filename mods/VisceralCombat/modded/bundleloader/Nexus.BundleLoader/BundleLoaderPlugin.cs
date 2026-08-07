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
		string moddedPath1 = Path.Combine(Environment.CurrentDirectory, "BepInEx", "plugins", "VisceralCombat", "ssh", "Bundles");
		string moddedPath2 = Path.Combine(Environment.CurrentDirectory, "BepInEx", "plugins", "VisceralCombat", "Bundles");
		string legacyPath = Path.Combine(Environment.CurrentDirectory, "BepInEx", "plugins", "ssh", "Bundles");

		string path = Directory.Exists(moddedPath1) ? moddedPath1 : (Directory.Exists(moddedPath2) ? moddedPath2 : legacyPath);
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
		Logger.LogInfo((object)$"Loaded {_loadedBundles.Count} bundles from {path}...");
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
		if (string.IsNullOrEmpty(bundleName)) return null;
		bundleName = bundleName.ToLower();

		if (_loadedBundles.TryGetValue(bundleName, out var value))
		{
			return value.assetBundle;
		}
		Logger.LogWarning((object)$"GetAssetBundle: Bundle '{bundleName}' not found in loaded bundles list.");
		return null;
	}

	public async Task<AssetBundle> GetAssetBundleAsync(string name, CancellationToken cancellationToken = default(CancellationToken))
	{
		name = name.ToLower();
		bool isFinished;
		while (!IsLoading(name, out isFinished))
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return null;
			}
			await Task.Yield();
		}
		while (!isFinished)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return null;
			}
			await Task.Yield();
		}
		return GetAssetBundle(name);
	}
}
