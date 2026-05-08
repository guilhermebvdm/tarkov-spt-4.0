using System;
using System.IO;
using System.Threading;
using Comfort.Common;
using EFT;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class LoadSceneClass
{
	public static T Instantiate<T>(this IAssetsManager assetsManager, string assetName) where T : UnityEngine.Object
	{
		T asset = assetsManager.GetAsset<T>(assetName);
		if (asset != null)
		{
			T val = UnityEngine.Object.Instantiate(asset);
			if (val != null)
			{
				return val;
			}
		}
		Debug.LogErrorFormat("Can't instantiate assetName:{0} as:{1}", assetName, typeof(T));
		return null;
	}

	public static T Instantiate<T>(this IAssetsManager assetsManager, string assetName, Vector3 position, Quaternion rotation) where T : UnityEngine.Object
	{
		T asset = assetsManager.GetAsset<T>(assetName);
		if (asset != null)
		{
			T val = UnityEngine.Object.Instantiate(asset, position, rotation);
			if (val != null)
			{
				return val;
			}
		}
		Debug.LogErrorFormat("Can't instantiate assetName:{0} as:{1}", assetName, typeof(T));
		return null;
	}

	public static T Instantiate<T>(this IAssetsManager assetsManager, ResourceKey resourceKey) where T : UnityEngine.Object
	{
		string assetName = assetsManager.GetAssetName(resourceKey);
		if (!string.IsNullOrEmpty(assetName))
		{
			return Instantiate<T>(assetsManager, assetName);
		}
		return UnityEngine.Object.Instantiate(GClass861.Load<T>("Doge"));
	}

	public static T Instantiate<T>(this IAssetsManager assetsManager, ResourceKey resourceKey, Vector3 position, Quaternion rotation) where T : UnityEngine.Object
	{
		string assetName = assetsManager.GetAssetName(resourceKey);
		if (!string.IsNullOrEmpty(assetName))
		{
			return Instantiate<T>(assetsManager, assetName, position, rotation);
		}
		return UnityEngine.Object.Instantiate(GClass861.Load<T>("Doge"));
	}

	public static GClass2287 LoadScenesFromPreset(this IAssetsManager assetsManager, ServerScenesDataStruct preset, bool loadFirstAsSingle, bool loadInParallelMode, bool allowSceneActivation, CancellationToken cancellationToken = default(CancellationToken), IProgress<float> progress = null)
	{
		return GClass2287.smethod_0(preset, assetsManager, loadFirstAsSingle, loadInParallelMode, allowSceneActivation, cancellationToken, progress);
	}

	public static GClass2287 LoadScenesFromPreset(this IAssetsManager assetsManager, ScenesPreset preset, bool loadFirstAsSingle, bool loadInParallelMode, bool allowSceneActivation, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GClass2287.smethod_1(preset, assetsManager, loadFirstAsSingle, loadInParallelMode, allowSceneActivation, cancellationToken);
	}

	public static GClass3968 LoadScene(this IAssetsManager manager, ResourceKey resourceKey, LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool allowSceneActivation = true, Action<float> progressCallback = null)
	{
		string sceneName = resourceKey.ToAssetName();
		string path = resourceKey.path;
		return manager.LoadScene(path, sceneName, loadSceneMode, allowSceneActivation, progressCallback);
	}

	public static IOperation LoadSceneByReference(this IAssetsManager manager, string reference, LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool allowSceneActivation = true, Action<float> progressCallback = null)
	{
		if (string.IsNullOrEmpty(reference))
		{
			return Operation.FailedOperation("invalid argument");
		}
		ResourceKey resourceKey = smethod_0(reference);
		if (resourceKey != null)
		{
			return LoadScene(manager, resourceKey, loadSceneMode, allowSceneActivation, progressCallback);
		}
		SceneManager.LoadScene(Path.GetFileNameWithoutExtension(reference), loadSceneMode);
		return Operation.SucceedOperation();
	}

	public static ResourceKey smethod_0(string reference)
	{
		if (reference.Contains(".bundle"))
		{
			return new ResourceKey
			{
				path = reference
			};
		}
		return null;
	}
}
