using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Class319 : Interface2
{
	public async Task<GClass650> WaitResponse(string url, byte[] data, Dictionary<string, string> headers, int timeoutSeconds)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
		httpWebRequest.Timeout = timeoutSeconds;
		httpWebRequest.Method = "POST";
		httpWebRequest.ContentLength = data.Length;
		httpWebRequest.ContentType = "application/json";
		httpWebRequest.Accept = "application/json";
		httpWebRequest.ServerCertificateValidationCallback = method_1;
		foreach (KeyValuePair<string, string> header in headers)
		{
			httpWebRequest.Headers.Add(header.Key, header.Value);
		}
		try
		{
			Stream stream = await httpWebRequest.GetRequestStreamAsync();
			await stream.WriteAsync(data, 0, data.Length);
			stream.Close();
		}
		catch (WebException ex)
		{
			return new GClass650(code: (int)((ex.Response is HttpWebResponse httpWebResponse) ? httpWebResponse.StatusCode : ((HttpStatusCode)526)), error: ex.Message);
		}
		try
		{
			return await method_0(url, httpWebRequest, stopwatch);
		}
		catch (WebException ex2)
		{
			return new GClass650(code: (int)((ex2.Response is HttpWebResponse httpWebResponse2) ? httpWebResponse2.StatusCode : ((HttpStatusCode)0)), error: ex2.Message);
		}
		catch (Exception ex3)
		{
			return new GClass650(ex3.Message);
		}
	}

	public async Task<GClass650> method_0(string url, HttpWebRequest request, Stopwatch stopwatch)
	{
		using HttpWebResponse httpWebResponse = (await request.GetResponseAsync()) as HttpWebResponse;
		string empty = string.Empty;
		HttpStatusCode statusCode = httpWebResponse.StatusCode;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		for (int i = 0; i < httpWebResponse.Headers.Count; i++)
		{
			dictionary.Add(httpWebResponse.GetResponseHeader(httpWebResponse.Headers.Keys[i]), httpWebResponse.Headers[i]);
		}
		StringBuilder stringBuilder = new StringBuilder();
		byte[] array = ArrayPool<byte>.Shared.Rent(1024);
		byte[] array2 = ArrayPool<byte>.Shared.Rent(1024);
		int num = 0;
		using (Stream stream = httpWebResponse.GetResponseStream())
		{
			int num2 = 10000;
			int num5;
			do
			{
				int num3 = await stream.ReadAsync(array, 0, array.Length);
				if (num3 == 0)
				{
					break;
				}
				int num4 = num3 + num;
				if (num4 > array2.Length)
				{
					byte[] array3 = ArrayPool<byte>.Shared.Rent(num4);
					Array.Copy(array2, array3, num);
					byte[] array4 = array2;
					array2 = array3;
					ArrayPool<byte>.Shared.Return(array4);
				}
				stringBuilder.Append(Encoding.UTF8.GetString(array, 0, num3));
				Array.Copy(array, 0, array2, num, num3);
				num += num3;
				if (!stream.CanRead)
				{
					break;
				}
				num5 = num2 - 1;
				num2 = num5;
			}
			while (num5 > 0);
			if (num2 <= 0)
			{
				UnityEngine.Debug.LogError("Sanity check failed");
			}
		}
		ArrayPool<byte>.Shared.Return(array);
		if (statusCode != HttpStatusCode.OK)
		{
			GClass651.Logger.LogError($"<--- Error! HTTPS: {url}, responseCode:{statusCode}");
			ArrayPool<byte>.Shared.Return(array2);
			return new GClass650(empty + " | " + stringBuilder.ToString(), (int)statusCode);
		}
		GClass650 result = new GClass650((int)statusCode, empty, dictionary, array2, num, stringBuilder.ToString(), stopwatch);
		ArrayPool<byte>.Shared.Return(array2);
		return result;
	}

	public bool method_1(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
	{
		if (sslpolicyerrors != SslPolicyErrors.None)
		{
			GClass651.Logger.LogError("SSL certificate error: {0}", GClass866<SslPolicyErrors>.GetName(sslpolicyerrors));
			return false;
		}
		bool num = SslCertPatchClass.ValidateCertificate(certificate);
		if (!num)
		{
			GClass651.Logger.LogError("Certificate validation error! Key " + certificate.GetPublicKeyString()?.ToLower() + " doesn't match to any related keys");
		}
		return num;
	}
}
