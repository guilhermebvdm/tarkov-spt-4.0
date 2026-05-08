using System.Collections.Generic;
using UnityEngine;

public struct GStruct232
{
	public bool HasData;

	public int Id;

	public KeyValuePair<int, Vector3>[] fp;

	public override string ToString()
	{
		return $"HasData: {HasData}, Id: {Id}, Fp: {((fp != null) ? fp.Length : 0)}";
	}

	public static void Serialize(GStruct232 info, GInterface131 writer)
	{
		writer.Write(info.HasData);
		if (info.HasData)
		{
			writer.Write(info.Id);
			writer.Write((info.fp != null) ? info.fp.Length : 0);
			for (int i = 0; i < info.fp.Length; i++)
			{
				KeyValuePair<int, Vector3> keyValuePair = info.fp[i];
				writer.Write(keyValuePair.Key);
				writer.Write(keyValuePair.Value);
			}
		}
	}

	public static GStruct232 Deserialize(IDataReader reader)
	{
		if (reader.ReadBool())
		{
			int id = reader.ReadInt32();
			int num = reader.ReadInt32();
			KeyValuePair<int, Vector3>[] array = new KeyValuePair<int, Vector3>[num];
			for (int i = 0; i < num; i++)
			{
				int key = reader.ReadInt32();
				Vector3 value = reader.ReadVector3();
				array[i] = new KeyValuePair<int, Vector3>(key, value);
			}
			return new GStruct232
			{
				HasData = true,
				fp = array,
				Id = id
			};
		}
		return default(GStruct232);
	}
}
