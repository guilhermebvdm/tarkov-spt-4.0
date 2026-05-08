using System;
using System.Threading.Tasks;
using Comfort.Common;
using Diz.Resources;
using UnityEngine;

public abstract class GClass1805
{
	public const string BUNDLE_EXTENSION = ".bundle";

	public const string STREAMING_ASSETS_DIR_NAME = "StreamingAssets";

	[NonSerialized]
	public const string String_0 = "Assets/Scenes/";

	[NonSerialized]
	public const string String_1 = null;

	public static async Task<IEasyAssets> CreateEasyAssets(GameObject gameObject, Func<string, bool> shouldExclude, IBundleLock bundleLock, string platformName, Func<string, Task> bundlesCheck)
	{
		return await EasyAssets.Create(gameObject, bundleLock, "doge", GetRelativePath(), platformName, shouldExclude, bundlesCheck);
	}

	public static Operation<IAssetsManager> CreateAssetsManager(bool updateBundles = false)
	{
		string platformName = GetPlatformName(Application.platform);
		Debug.Log("Using real bundles");
		return GClass3962.Run(new GStruct453
		{
			DownloadingUrl = GetDownloadingUrl(platformName),
			ManifestBundleName = platformName,
			ManifestAssetName = "AssetBundleManifest",
			UpdateUrl = (updateBundles ? smethod_0(platformName) : null)
		});
	}

	public static string GetDownloadingUrl(string platformName)
	{
		string relativePath = GetRelativePath();
		return $"{relativePath}/{platformName}/";
	}

	public static string smethod_0(string platformName)
	{
		return GetDownloadingUrl(platformName);
	}

	public static string GetRelativePath()
	{
		if (Application.isEditor)
		{
			return "file:///" + Environment.CurrentDirectory.Replace("\\", "/") + "/StreamingAssets";
		}
		if (!Application.isMobilePlatform && !Application.isConsolePlatform)
		{
			return "file:///" + Application.streamingAssetsPath;
		}
		return Application.streamingAssetsPath;
	}

	public static string GetPlatformName(this RuntimePlatform platform)
	{
		return platform switch
		{
			RuntimePlatform.WindowsPlayer => "Windows", 
			RuntimePlatform.OSXPlayer => "OSX", 
			RuntimePlatform.Android => "Android", 
			RuntimePlatform.IPhonePlayer => "iOS", 
			_ => null, 
		};
	}
}
