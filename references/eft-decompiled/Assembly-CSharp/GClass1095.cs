using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

public class GClass1095<T> : GInterface63<T> where T : GClass1081<T>, new()
{
	[NonSerialized]
	public const string String_0 = ".ini";

	[NonSerialized]
	public List<GClass1080<T>> List_0;

	[NonSerialized]
	public string String_1;

	[NonSerialized]
	public GClass1087[] Gclass1087_0;

	[NonSerialized]
	public GClass1087.GInterface66 Ginterface66_0;

	public IReadOnlyList<GClass1080<T>> Presets => List_0;

	public GClass1095(string folderPath, [NotNull] GClass1087.GInterface66 serializer, params GClass1087[] fallbackProviders)
	{
		String_1 = folderPath;
		Ginterface66_0 = serializer;
		Gclass1087_0 = fallbackProviders;
	}

	public Task RefreshPresetList()
	{
		if (!Directory.Exists(String_1))
		{
			Directory.CreateDirectory(String_1);
			List_0 = new List<GClass1080<T>>();
			return Task.CompletedTask;
		}
		List_0 = Directory.GetFiles(String_1, "*.ini").Select(method_0).ToList();
		return Task.CompletedTask;
	}

	public async Task<GClass1080<T>> CreateNewPreset(string name, T settingsGroup)
	{
		GClass1080<T> gClass = method_1(method_3(name), name);
		await gClass.Save(settingsGroup);
		return gClass;
	}

	public Task RemovePreset(GClass1080<T> preset)
	{
		if (List_0.Remove(preset))
		{
			File.Delete(method_3(preset.Name));
		}
		return Task.CompletedTask;
	}

	public GClass1080<T> method_0(string path)
	{
		return method_1(path, method_2(path));
	}

	public GClass1080<T> method_1(string path, string name)
	{
		GClass1089 provider = new GClass1089(path, Ginterface66_0, Gclass1087_0);
		return new GClass1080<T>(name, provider);
	}

	public string method_2(string path)
	{
		return path.Replace(String_1, string.Empty).Replace(".ini", string.Empty);
	}

	public string method_3(string name)
	{
		return Path.Combine(String_1, name + ".ini");
	}
}
