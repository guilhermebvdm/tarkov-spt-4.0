using System;
using System.IO;
using JBooth.MicroSplat;
using UnityEngine;

public class GClass795
{
	public byte[,] data;

	public Vector3 size;

	public int alphamapWidth;

	public int alphamapHeight;

	[NonSerialized]
	public static float[] Float_0 = new float[20];

	public int GetTextureMix(Vector3 worldPos, Vector3 terrainWorldPosition)
	{
		int num = (int)((worldPos.x - terrainWorldPosition.x) / size.x * (float)alphamapWidth);
		int num2 = (int)((worldPos.z - terrainWorldPosition.z) / size.z * (float)alphamapHeight);
		if (num < 0 || num >= alphamapWidth || num2 < 0 || num2 >= alphamapHeight)
		{
			throw new IndexOutOfRangeException($"Try to access invalid indexes ({num}, {num2}, when array size is ({alphamapWidth}, {alphamapHeight}))");
		}
		return data[num, num2];
	}

	public static GClass795 LoadFromAsset(TextAsset asset)
	{
		BinaryReader reader = new BinaryReader(new MemoryStream(asset.bytes));
		GClass795 gClass = new GClass795();
		gClass.Deserialize(reader);
		return gClass;
	}

	public static void CreateTextureMixDataAsset(string path, MicroSplatTerrain terrain)
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(memoryStream);
		smethod_0(terrain).Serialize(writer);
		using FileStream fileStream = File.Open(path, FileMode.Create);
		memoryStream.WriteTo(fileStream);
		fileStream.Flush();
	}

	public static GClass795 smethod_0(MicroSplatTerrain terrain)
	{
		TerrainData terrainData = terrain.terrain.terrainData;
		int num = terrainData.alphamapWidth;
		int num2 = terrainData.alphamapHeight;
		byte[,] array = new byte[num, num2];
		float[] array2 = new float[10];
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				float[,,] alphamaps = terrainData.GetAlphamaps(i, j, 1, 1);
				int num3 = alphamaps.GetUpperBound(2) + 1;
				if (num3 > array2.Length)
				{
					array2 = new float[num3];
				}
				for (int k = 0; k < num3; k++)
				{
					array2[k] = alphamaps[0, 0, k];
				}
				float num4 = 0f;
				int num5 = 0;
				for (int l = 0; l < num3; l++)
				{
					if (array2[l] > num4)
					{
						num5 = l;
						num4 = array2[l];
					}
				}
				array[i, j] = (byte)num5;
			}
		}
		return new GClass795
		{
			data = array,
			size = terrainData.size,
			alphamapWidth = terrainData.alphamapWidth,
			alphamapHeight = terrainData.alphamapHeight
		};
	}

	public static int smethod_1(Vector3 worldPos, TerrainData terrainData, Vector3 terrainWorldPosition, ref float[] result)
	{
		int x = (int)((worldPos.x - terrainWorldPosition.x) / terrainData.size.x * (float)terrainData.alphamapWidth);
		int y = (int)((worldPos.z - terrainWorldPosition.z) / terrainData.size.z * (float)terrainData.alphamapHeight);
		float[,,] alphamaps = terrainData.GetAlphamaps(x, y, 1, 1);
		int num = alphamaps.GetUpperBound(2) + 1;
		if (num > result.Length)
		{
			Debug.LogError("Array for detecting texture layer is too small");
			Array.Resize(ref result, num);
		}
		for (int i = 0; i < num; i++)
		{
			result[i] = alphamaps[0, 0, i];
		}
		return num;
	}

	public static int GetMainTexture(Vector3 worldPos, TerrainData terrainData, Vector3 terrainWorldPosition)
	{
		int num = smethod_1(worldPos, terrainData, terrainWorldPosition, ref Float_0);
		float num2 = 0f;
		int result = 0;
		for (int i = 0; i < num; i++)
		{
			if (Float_0[i] > num2)
			{
				result = i;
				num2 = Float_0[i];
			}
		}
		return result;
	}

	public void Serialize(BinaryWriter writer)
	{
		writer.Write(alphamapWidth);
		writer.Write(alphamapHeight);
		writer.Write(size.x);
		writer.Write(size.y);
		writer.Write(size.z);
		for (int i = 0; i < alphamapWidth; i++)
		{
			for (int j = 0; j < alphamapHeight; j++)
			{
				writer.Write(data[i, j]);
			}
		}
	}

	public void Deserialize(BinaryReader reader)
	{
		alphamapWidth = reader.ReadInt32();
		alphamapHeight = reader.ReadInt32();
		float x = reader.ReadSingle();
		float y = reader.ReadSingle();
		float z = reader.ReadSingle();
		size = new Vector3(x, y, z);
		data = new byte[alphamapWidth, alphamapHeight];
		for (int i = 0; i < alphamapWidth; i++)
		{
			for (int j = 0; j < alphamapHeight; j++)
			{
				data[i, j] = reader.ReadByte();
			}
		}
	}
}
