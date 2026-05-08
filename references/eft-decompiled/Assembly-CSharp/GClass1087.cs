using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using UnityEngine;

public abstract class GClass1087 : GInterface64
{
	public interface GInterface66
	{
		[NotNull]
		T Populate<T>([CanBeNull] JObject json, [NotNull] T item, out bool needReSave);

		[NotNull]
		JObject PrepareToSave<T>([NotNull] T item);
	}

	[NonSerialized]
	public GInterface66 Ginterface66_0;

	[NonSerialized]
	public GClass1087[] Gclass1087_0;

	public GClass1087([NotNull] GInterface66 serializer, params GClass1087[] fallbackProviders)
	{
		Ginterface66_0 = serializer;
		Gclass1087_0 = fallbackProviders;
	}

	public async Task<T> Load<T>(T settingsGroup) where T : GClass1081<T>
	{
		JObject jObject = await LoadJson();
		bool flag = jObject != null;
		if (!flag)
		{
			jObject = await method_0();
		}
		if (jObject == null)
		{
			Debug.LogWarning("Fallback to default " + typeof(T).Name);
		}
		bool needReSave;
		T val = Ginterface66_0.Populate(jObject, settingsGroup, out needReSave);
		if (!flag || needReSave)
		{
			await Save(val);
		}
		return val;
	}

	public async Task Save<T>(T settingsGroup) where T : GClass1081<T>
	{
		await SaveJson(Ginterface66_0.PrepareToSave(settingsGroup));
	}

	public abstract Task<JObject> LoadJson();

	public abstract Task SaveJson([NotNull] JObject json);

	public async Task<JObject> method_0()
	{
		if (Gclass1087_0 == null)
		{
			return null;
		}
		JObject jObject = null;
		GClass1087[] gclass1087_ = Gclass1087_0;
		foreach (GClass1087 gClass in gclass1087_)
		{
			Debug.LogWarning($"Fallback to {gClass}");
			jObject = await gClass.LoadJson();
			if (jObject != null)
			{
				break;
			}
		}
		return jObject;
	}
}
