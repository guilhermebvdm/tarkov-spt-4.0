using System;
using System.Net;
using ComponentAce.Compression.Libs.zlib;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class GClass845 : WebClient
{
	public int Timeout = 30000;

	public bool ZipTraffic = true;

	public GClass845()
	{
		base.Headers.Add("Content-Type", "application/json");
	}

	public override WebRequest GetWebRequest(Uri address)
	{
		WebRequest webRequest = base.GetWebRequest(address);
		webRequest.Timeout = Timeout;
		return webRequest;
	}

	public new string UploadString(string address, string data, string method = "POST")
	{
		try
		{
			if (ZipTraffic)
			{
				return SimpleZlib.Decompress(UploadData(address, method, SimpleZlib.CompressToBytes(data, 9)));
			}
			return base.UploadString(address, data);
		}
		catch (Exception ex)
		{
			Debug.LogErrorFormat("Can't upload string to '{0}', exception occured: {1}", address, ex.Message);
			throw;
		}
	}

	public new string DownloadString(string address)
	{
		try
		{
			if (ZipTraffic)
			{
				byte[] array = DownloadData(address);
				return Pooled9LevelZLib.DecompressNonAlloc(array, array.Length);
			}
			return base.DownloadString(address);
		}
		catch (Exception ex)
		{
			Debug.LogErrorFormat("Can't Download String from '{0}', exception occured: {1}", address, ex.Message);
			throw;
		}
	}

	public T ParseDataFromJson<T>(string json)
	{
		JToken jToken = JObject.Parse(json)["data"];
		if (jToken == null)
		{
			Debug.LogError("Can't parse data from json");
			return default(T);
		}
		return jToken.ToObject<T>();
	}

	public int ParseErrorCodeFromJson(string json)
	{
		JObject jObject = JObject.Parse(json);
		if (jObject["err"] == null)
		{
			Debug.LogError("Can't parse error code from json");
			return -1;
		}
		return jObject["err"].Value<int>();
	}
}
