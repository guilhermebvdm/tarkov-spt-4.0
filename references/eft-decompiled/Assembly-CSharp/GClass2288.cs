using System;
using System.IO;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GClass2288
{
	public class GClass2289
	{
		public ScenesPreset ScenesPreset;

		public global::DependencyGraphClass<IEasyBundle>.GClass1661 Resources;
	}

	public static async Task<GClass2289> Load(ResourceKey preset)
	{
		global::DependencyGraphClass<IEasyBundle>.GClass1661 gClass = GClass1857.Retain(Singleton<IEasyAssets>.Instance, new string[1] { preset.path });
		await GClass1857.LoadBundles(gClass);
		ScenesPreset asset = GClass1857.GetAsset<ScenesPreset>(Singleton<IEasyAssets>.Instance, preset);
		SceneResourceKey[] scenesResourceKeys = asset.ScenesResourceKeys;
		for (int i = 0; i < scenesResourceKeys.Length; i++)
		{
			await LoadScene(scenesResourceKeys[i].ToAssetName(), LoadSceneMode.Additive);
		}
		return new GClass2289
		{
			ScenesPreset = asset,
			Resources = gClass
		};
	}

	public static async Task Unload(GClass2289 sceneToken)
	{
		SceneResourceKey[] scenesResourceKeys = sceneToken.ScenesPreset.ScenesResourceKeys;
		for (int i = 0; i < scenesResourceKeys.Length; i++)
		{
			await UnloadScene(scenesResourceKeys[i].ToAssetName());
		}
		sceneToken.Resources.Release();
		sceneToken.Resources = null;
	}

	public static async Task LoadScene(string sceneName, LoadSceneMode loadSceneMode, Action<float> progressCallback = null)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sceneName);
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(fileNameWithoutExtension, loadSceneMode);
		if (asyncOperation != null)
		{
			float num = 0f;
			while (!asyncOperation.isDone)
			{
				if (num < asyncOperation.progress)
				{
					num = asyncOperation.progress;
					progressCallback?.Invoke(num);
				}
				await Task.Yield();
			}
		}
		else
		{
			Debug.LogError("Can't load scene:" + sceneName);
		}
	}

	public static async Task UnloadScene(string sceneName, Action<float> progressCallback = null)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sceneName);
		AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(fileNameWithoutExtension);
		if (asyncOperation != null)
		{
			float num = 0f;
			while (!asyncOperation.isDone)
			{
				if (num < asyncOperation.progress)
				{
					num = asyncOperation.progress;
					progressCallback?.Invoke(num);
				}
				await Task.Yield();
			}
		}
		else
		{
			Debug.LogError("Can't unload scene:" + sceneName);
		}
	}
}
