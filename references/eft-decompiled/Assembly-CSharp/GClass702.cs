using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass702
{
	[Serializable]
	[CompilerGenerated]
	public class Class357
	{
		public static readonly Class357 class357_0 = new Class357();

		public static Func<ISyncAble, ulong> func_0;

		public static Func<ISyncAble, string> func_1;

		public static Func<ISyncAble, ulong> func_2;

		public static Func<ISyncAble, ISyncAble> func_3;

		public ulong method_0(ISyncAble so)
		{
			return so.GetSceneId();
		}

		public string method_1(ISyncAble gr)
		{
			return $"{GClass6.GetFullPath(gr.transform)} - {gr.GetSceneId()}";
		}

		public ulong method_2(ISyncAble target)
		{
			return target.GetSceneId();
		}

		public ISyncAble method_3(ISyncAble target)
		{
			return target;
		}
	}

	[NonSerialized]
	public Dictionary<ulong, ISyncAble> Dictionary_0;

	public GClass702()
	{
		ISyncAble[] source = LocationScene.GetAllObjects<ISyncAble>(withDisabled: true).ToArray();
		foreach (IGrouping<ulong, ISyncAble> item in from so in source
			group so by so.GetSceneId())
		{
			if (item.Count() > 1)
			{
				Debug.LogError("[Ask technical art department support to resolve id conflicts] " + string.Join("\n", item.Select((ISyncAble gr) => $"{GClass6.GetFullPath(gr.transform)} - {gr.GetSceneId()}")));
			}
		}
		Dictionary_0 = source.ToDictionary((ISyncAble target) => target.GetSceneId(), (ISyncAble target) => target);
	}

	public void Serialize(GInterface131 writerStream)
	{
		writerStream.Write(Dictionary_0.Count);
		foreach (var (value, syncAble2) in Dictionary_0)
		{
			writerStream.Write(value);
			syncAble2.Serialize(writerStream);
		}
	}

	public void Deserialize(IDataReader readerStream)
	{
		int num = readerStream.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			ulong key = readerStream.ReadUInt64();
			Dictionary_0[key].Deserialize(readerStream);
		}
	}
}
