using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Comfort.Common;
using UnityEngine;

public class GClass899
{
	[NonSerialized]
	public Dictionary<string, Voice> Dictionary_0 = new Dictionary<string, Voice>();

	[NonSerialized]
	public List<global::DependencyGraphClass<IEasyBundle>.GClass1661> List_0 = new List<global::DependencyGraphClass<IEasyBundle>.GClass1661>();

	[NonSerialized]
	public IEasyAssets IEasyAssets;

	public static async Task<GClass899> InstantiateSingleton()
	{
		if (!Singleton<GClass899>.Instantiated)
		{
			try
			{
				GClass899 gClass = new GClass899();
				gClass.method_0();
				Singleton<GClass899>.Create(gClass);
				return gClass;
			}
			catch (Exception message)
			{
				Debug.LogError(message);
				return null;
			}
		}
		Debug.LogError("PlayerVoiceLoader already instantiated!");
		return Singleton<GClass899>.Instance;
	}

	public void method_0()
	{
		IEasyAssets = Singleton<IEasyAssets>.Instance;
	}

	public async Task<Voice> LoadVoice(string name)
	{
		if (Dictionary_0.TryGetValue(name, out var value))
		{
			return value;
		}
		List_0.Add(GClass1857.Retain(IEasyAssets, new string[1] { ResourceKeyManagerAbstractClass.TakePhrasePath(name) }));
		await GClass1857.LoadBundles(List_0[List_0.Count - 1]);
		Voice asset = GClass1857.GetAsset<Voice>(Singleton<IEasyAssets>.Instance, ResourceKeyManagerAbstractClass.TakePhrasePath(name));
		if (asset == null)
		{
			Debug.LogError("Voice asset " + name + " not found! Voice won't be played on select.");
			return null;
		}
		Dictionary_0.Add(name, asset);
		return asset;
	}

	public async Task<TagBank> TakeVoice(string voiceName, EPhraseTrigger trigger = EPhraseTrigger.OnMutter)
	{
		if (!Dictionary_0.ContainsKey(voiceName))
		{
			await LoadVoice(voiceName);
		}
		return await method_1(voiceName, trigger);
	}

	public async Task<TagBank> method_1(string voiceName, EPhraseTrigger trigger)
	{
		TagBank tagBank = null;
		TagBank[] banks = Dictionary_0[voiceName].Banks;
		foreach (TagBank tagBank2 in banks)
		{
			if (tagBank2.Trigger == trigger)
			{
				tagBank = tagBank2;
				break;
			}
		}
		if (tagBank == null)
		{
			Debug.LogError($"No TagBank with EPhraseTrigger {GClass866<EPhraseTrigger>.GetName(trigger)} for voice {Dictionary_0[voiceName]}");
		}
		return tagBank;
	}

	public void UnloadVoices()
	{
		foreach (global::DependencyGraphClass<IEasyBundle>.GClass1661 item in List_0)
		{
			item.Release();
		}
		List_0.Clear();
		Dictionary_0.Clear();
	}
}
