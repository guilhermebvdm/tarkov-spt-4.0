using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine.Networking;

public class Class320 : Interface2
{
	public async Task<GClass650> WaitResponse(string url, byte[] data, Dictionary<string, string> headers, int timeoutSeconds)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		using UnityWebRequest unityWebRequest = new UnityWebRequest(url, "POST");
		unityWebRequest.uploadHandler = new UploadHandlerRaw(data);
		unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
		unityWebRequest.certificateHandler = new SslCertPatchClass();
		unityWebRequest.timeout = timeoutSeconds;
		unityWebRequest.SetRequestHeader("Content-Type", "application/json");
		foreach (KeyValuePair<string, string> header in headers)
		{
			unityWebRequest.SetRequestHeader(header.Key, header.Value);
		}
		try
		{
			return await method_0(url, unityWebRequest, stopwatch);
		}
		catch (Exception ex)
		{
			return new GClass650(ex.Message);
		}
	}

	public async Task<GClass650> method_0(string url, UnityWebRequest request, Stopwatch stopwatch)
	{
		UnityWebRequestAsyncOperation unityWebRequestAsyncOperation = request.SendWebRequest();
		while (!unityWebRequestAsyncOperation.isDone)
		{
			await Task.Yield();
		}
		int num = (int)request.responseCode;
		if (!request.isNetworkError && !request.isHttpError)
		{
			Dictionary<string, string> responseHeaders = request.GetResponseHeaders();
			byte[] data = request.downloadHandler.data;
			string text = request.downloadHandler.text;
			return new GClass650(num, string.Empty, responseHeaders, data, data.Length, text, stopwatch);
		}
		string error = request.error;
		error = ((!(error == "Unknown Error")) ? (error + "\n" + request.downloadHandler.text) : "Certificate validation error");
		GClass651.Logger.LogError("<--- Error! HTTPS: {0}, isNetworkError:{1}, isHttpError:{2}, responseCode:{3}\n responseHeaders:{4}\nerror text: {5}", url, request.isNetworkError, request.isHttpError, num, DataHandlerClass.ToString(request.GetResponseHeaders()), error);
		return new GClass650("Backend error: " + error, num);
	}
}
