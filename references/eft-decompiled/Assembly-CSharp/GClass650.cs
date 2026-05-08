using System;
using System.Collections.Generic;
using System.Diagnostics;

public class GClass650 : IDisposable
{
	public int responseCode;

	public Dictionary<string, string> responseHeaders;

	public byte[] responseData;

	public int responseDataLength;

	public string responseText;

	public Stopwatch stopwatch;

	public string errorText;

	public GClass650(int code, string error, Dictionary<string, string> headers, byte[] data, int dataLength, string textResponse, Stopwatch watch)
	{
		responseCode = code;
		responseHeaders = headers;
		if (data != null)
		{
			responseData = new byte[data.Length];
			Array.Copy(data, 0, responseData, 0, data.Length);
		}
		responseText = textResponse;
		stopwatch = watch;
		errorText = error;
		responseDataLength = dataLength;
	}

	public GClass650(string error, int code = 0)
	{
		responseCode = code;
		errorText = error;
		responseHeaders = null;
		responseData = null;
		responseDataLength = 0;
		responseText = string.Empty;
		stopwatch = null;
	}

	public void Dispose()
	{
		errorText = string.Empty;
		responseText = string.Empty;
		responseCode = -1;
		stopwatch = null;
		responseData = null;
		responseDataLength = 0;
		if (responseHeaders != null)
		{
			responseHeaders.Clear();
			responseHeaders = null;
		}
	}
}
