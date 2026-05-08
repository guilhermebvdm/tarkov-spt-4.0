using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Diz.Jobs;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Build.Pipeline;

namespace Diz.Resources;

public class EasyAssets : MonoBehaviour, IEasyAssets
{
	public struct Struct265
	{
		public string FileName;

		public uint Crc;

		public string[] Dependencies;
	}

	[Serializable]
	[CompilerGenerated]
	public class Class1050
	{
		public static readonly Class1050 class1050_0 = new Class1050();

		public static Func<KeyValuePair<string, Struct265>, string> func_0;

		public static Func<KeyValuePair<string, Struct265>, BundleDetails> func_1;

		public string method_0(KeyValuePair<string, Struct265> x)
		{
			return x.Key;
		}

		public BundleDetails method_1(KeyValuePair<string, Struct265> x)
		{
			return new BundleDetails
			{
				FileName = x.Value.FileName,
				Crc = x.Value.Crc,
				Dependencies = x.Value.Dependencies
			};
		}
	}

	public List<string> Log = new List<string>();

	[CompilerGenerated]
	private global::DependencyGraphClass<IEasyBundle> gclass1658_0;

	private EasyAssetHelperClass[] class1051_0;

	public CompatibilityAssetBundleManifest Manifest;

	public global::DependencyGraphClass<IEasyBundle> System
	{
		[CompilerGenerated]
		get
		{
			return gclass1658_0;
		}
		[CompilerGenerated]
		set
		{
			gclass1658_0 = value;
		}
	}

	public static async Task<EasyAssets> Create(GameObject gameObject, [CanBeNull] IBundleLock bundleLock, string defaultKey, string rootPath, string platformName, [CanBeNull] Func<string, bool> shouldExclude, [CanBeNull] Func<string, Task> bundleCheck)
	{
		EasyAssets easyAssets = gameObject.AddComponent<EasyAssets>();
		await easyAssets.method_0(bundleLock, defaultKey, rootPath, platformName, shouldExclude, bundleCheck);
		return easyAssets;
	}

	public async Task method_0([CanBeNull] IBundleLock bundleLock, string defaultKey, string rootPath, string platformName, [CanBeNull] Func<string, bool> shouldExclude, [CanBeNull] Func<string, Task> bundleCheck)
	{
		string text = rootPath.Replace("file:///", "").Replace("file://", "") + "/" + platformName + "/";
		Dictionary<string, BundleDetails> results = JsonConvert.DeserializeObject<Dictionary<string, Struct265>>(File.ReadAllText(text + platformName + ".json")).ToDictionary((KeyValuePair<string, Struct265> x) => x.Key, (KeyValuePair<string, Struct265> x) => new BundleDetails
		{
			FileName = x.Value.FileName,
			Crc = x.Value.Crc,
			Dependencies = x.Value.Dependencies
		});
		Manifest = ScriptableObject.CreateInstance<CompatibilityAssetBundleManifest>();
		Manifest.SetResults(results);
		string[] allAssetBundles = Manifest.GetAllAssetBundles();
		class1051_0 = new EasyAssetHelperClass[allAssetBundles.Length];
		if (bundleLock == null)
		{
			bundleLock = new BundleLockClass(int.MaxValue);
		}
		for (int num = 0; num < allAssetBundles.Length; num++)
		{
			class1051_0[num] = new EasyAssetHelperClass(allAssetBundles[num], text, Manifest, bundleLock, bundleCheck);
			await JobScheduler.Yield();
		}
		EasyAssets easyAssets = this;
		IEasyBundle[] loadables = class1051_0;
		easyAssets.System = new global::DependencyGraphClass<IEasyBundle>(loadables, defaultKey, shouldExclude);
	}

	public void Update()
	{
		System?.Update();
	}
}
