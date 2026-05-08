using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using ComponentAce.Compression.Libs.zlib;
using UnityEngine;

public class Class310 : GInterface17, IDisposable
{
	public class Class387 : LoggerClass
	{
		public Class387()
			: base("backendCache", LoggerMode.Add)
		{
		}
	}

	[NonSerialized]
	[CompilerGenerated]
	public Class387 Class387_0_1;

	[NonSerialized]
	public string String_0;

	public Class387 Class387_0
	{
		[CompilerGenerated]
		get
		{
			return Class387_0_1;
		}
	}

	public Class310(string dir)
	{
		String_0 = dir;
		Class387_0_1 = new Class387();
	}

	public string method_0(string url)
	{
		string md5Hash = Class304.GetMd5Hash(url);
		return String_0 + md5Hash;
	}

	public bool Exists(string url)
	{
		try
		{
			return File.Exists(method_0(url));
		}
		catch (Exception e)
		{
			Class387_0.LogException(e);
		}
		return false;
	}

	public string Load(string url)
	{
		try
		{
			string text = method_0(url);
			Class387_0.LogInfo("BackendCache.Load File name: {0}, URL: {1}", text, url);
			if (File.Exists(text))
			{
				Class387_0.LogInfo("BackendCache.Load File name: {0} - exists", text);
				byte[] array = File.ReadAllBytes(text);
				Class387_0.LogInfo("BackendCache.Load File name: {0} , read bytes: {1}", text, array.Length);
				for (int i = 0; i < array.Length; i++)
				{
					array[i] ^= 0xD;
				}
				Class387_0.LogInfo("BackendCache.Load File name: {0} , bytes decoded: {1}", text, array.Length);
				string text2 = Pooled9LevelZLib.DecompressNonAlloc(array, array.Length);
				Class387_0.LogInfo("BackendCache.Load File name: {0} , decompressed, string length: {1}", text, text2.Length);
				return text2;
			}
			Class387_0.LogInfo("BackendCache.Load File name: {0} - NOT exists", text);
		}
		catch (Exception e)
		{
			Class387_0.LogException(e);
		}
		return null;
	}

	public void Save(string url, string data)
	{
		try
		{
			if (!Directory.Exists(String_0))
			{
				Directory.CreateDirectory(String_0);
			}
			string text = method_0(url);
			Class387_0.LogInfo("BackendCache.Save File name: {0}, URL: {1}", text, url);
			if (File.Exists(text))
			{
				File.Delete(text);
			}
			if (string.IsNullOrEmpty(data))
			{
				Debug.LogError("Attempting to save empty cash file url: " + url);
				return;
			}
			byte[] array = ArrayPool<byte>.Shared.Rent(data.Length + 16);
			int num = Pooled9LevelZLib.CompressToBytesNonAlloc(data, array);
			Class387_0.LogInfo("BackendCache.Compressed File name: {0}, URL: {1} Bytes to write {2}", text, url, num);
			for (int i = 0; i < num; i++)
			{
				array[i] ^= 0xD;
			}
			using (FileStream fileStream = File.Open(text, FileMode.OpenOrCreate))
			{
				fileStream.Write(array, 0, num);
			}
			ArrayPool<byte>.Shared.Return(array);
		}
		catch (Exception e)
		{
			Class387_0.LogException(e);
		}
	}

	public void Dispose()
	{
		Class387_0?.Dispose();
	}
}
