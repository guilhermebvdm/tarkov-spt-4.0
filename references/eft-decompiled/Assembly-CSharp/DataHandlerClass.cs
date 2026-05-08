using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Comfort.Common;
using ComponentAce.Compression.Libs.zlib;
using Diz.Utils;
using EFT;
using JetBrains.Annotations;
using Microsoft.Extensions.ObjectPool;
using NLog;
using Newtonsoft.Json;
using UnityEngine;

public class DataHandlerClass
{
	public class Class314 : IPooledObjectPolicy<byte[]>
	{
		public byte[] Create()
		{
			return new byte[16];
		}

		public bool Return(byte[] obj)
		{
			Array.Clear(obj, 0, 16);
			return true;
		}
	}

	public struct Struct29<T>
	{
		public T ParsedObject;

		public string ResponseText;
	}

	[Serializable]
	[CompilerGenerated]
	public class Class315
	{
		public static readonly Class315 class315_0 = new Class315();

		public static Action action_0;

		public void method_0()
		{
			if (Singleton<Class2723>.Instantiated)
			{
				Singleton<Class2723>.Instance.method_0(CancellationToken.None).HandleExceptions();
			}
			else
			{
				GClass651.Logger.LogInfo("ConnectionDiagnostics not started");
			}
		}
	}

	[CompilerGenerated]
	public class Class316<T>
	{
		public DataHandlerClass DataHandlerClass;

		public Class312 backRequest;

		public ERequestErrorHandlingResult method_0(int code, string message)
		{
			try
			{
				DataHandlerClass.action_0?.Invoke(GClass866<EBackendErrorCode>.GetValue(code), message);
			}
			catch (Exception e)
			{
				GClass651.Logger.LogException(e);
			}
			ERequestErrorHandlingResult eRequestErrorHandlingResult = ERequestErrorHandlingResult.Default;
			bool abortOnFail = backRequest.HandlingMode == ERequestHandlingMode.AbortOnFail;
			if (backRequest.HandlingMode != ERequestHandlingMode.Ignore)
			{
				eRequestErrorHandlingResult = DataHandlerClass.Gdelegate8_0(code, message, abortOnFail);
				try
				{
					DataHandlerClass.action_1?.Invoke(eRequestErrorHandlingResult);
				}
				catch (Exception e2)
				{
					GClass651.Logger.LogException(e2);
				}
				return eRequestErrorHandlingResult;
			}
			return ERequestErrorHandlingResult.Default;
		}

		public ERequestErrorHandlingResult method_1(GException4 netwExc)
		{
			if (netwExc.Code >= 501 && netwExc.Code <= 506 && netwExc.Code != 505)
			{
				return method_0(netwExc.Code, netwExc.Message);
			}
			return ERequestErrorHandlingResult.Default;
		}

		public void method_2()
		{
			backRequest.SetupDataBlock(DataHandlerClass.Bool_0);
		}
	}

	[CompilerGenerated]
	public class Class317
	{
		public string separator;

		public string method_0(string current, KeyValuePair<string, string> item)
		{
			return current + "{" + item.Key + ":" + item.Value + "}" + separator;
		}
	}

	[CompilerGenerated]
	public class Class318<T>
	{
		public Class312 backRequest;

		public DataHandlerClass DataHandlerClass;

		public GClass650 responseData;

		public string responseText;

		public T method_0()
		{
			return JsonParserClass.ParseJsonTo<T>(backRequest.CachedResponse.data, Array.Empty<JsonConverter>());
		}

		public string method_1()
		{
			return DataHandlerClass.method_5(backRequest, responseData);
		}

		public T method_2()
		{
			return DataHandlerClass.method_7<T>(backRequest, responseText);
		}
	}

	[NonSerialized]
	public const int Int_0 = 16;

	[NonSerialized]
	public static byte[] Byte_0;

	[NonSerialized]
	public static ObjectPool<byte[]> ObjectPool_0;

	[NonSerialized]
	public DateTime DateTime_0;

	[CompilerGenerated]
	private Action<EBackendErrorCode, string> action_0;

	[CompilerGenerated]
	private Action<ERequestErrorHandlingResult> action_1;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public GDelegate8 Gdelegate8_0;

	[NonSerialized]
	public StringBuilder StringBuilder_0 = new StringBuilder(256);

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	public DateTime LastSucceedResponse
	{
		get
		{
			lock (this)
			{
				return DateTime_0;
			}
		}
		set
		{
			lock (this)
			{
				DateTime_0 = value;
			}
		}
	}

	public Interface2 Interface2_0 => GClass651.Interface2_0;

	public GClass651.GClass711 GClass711_0 => GClass651.Logger;

	public bool WebDiagnosticsEnabled
	{
		[CompilerGenerated]
		get
		{
			return Bool_1;
		}
		[CompilerGenerated]
		set
		{
			Bool_1 = value;
		}
	}

	public event Action<EBackendErrorCode, string> OnSessionError
	{
		[CompilerGenerated]
		add
		{
			Action<EBackendErrorCode, string> action = action_0;
			Action<EBackendErrorCode, string> action2;
			do
			{
				action2 = action;
				Action<EBackendErrorCode, string> value2 = (Action<EBackendErrorCode, string>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<EBackendErrorCode, string> action = action_0;
			Action<EBackendErrorCode, string> action2;
			do
			{
				action2 = action;
				Action<EBackendErrorCode, string> value2 = (Action<EBackendErrorCode, string>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<ERequestErrorHandlingResult> OnErrorHandledWithResult
	{
		[CompilerGenerated]
		add
		{
			Action<ERequestErrorHandlingResult> action = action_1;
			Action<ERequestErrorHandlingResult> action2;
			do
			{
				action2 = action;
				Action<ERequestErrorHandlingResult> value2 = (Action<ERequestErrorHandlingResult>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<ERequestErrorHandlingResult> action = action_1;
			Action<ERequestErrorHandlingResult> action2;
			do
			{
				action2 = action;
				Action<ERequestErrorHandlingResult> value2 = (Action<ERequestErrorHandlingResult>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	static DataHandlerClass()
	{
		Byte_0 = Encoding.UTF8.GetBytes("A@l*vx*0ZRL9@IZDfRXShD9J");
		int num = Byte_0.Length % 8;
		if (num != 0)
		{
			Array.Resize(ref Byte_0, Byte_0.Length + (8 - num));
		}
		ObjectPool_0 = new DefaultObjectPool<byte[]>(new Class314());
		ObjectPool_0.Preallocate(2);
	}

	public static string smethod_0(IDictionary<string, string> responseHeaders)
	{
		if (responseHeaders.TryGetValue("X-Encryption", out var value))
		{
			return value;
		}
		return string.Empty;
	}

	public byte[] method_0(byte[] data, string encryption)
	{
		if (string.IsNullOrEmpty(encryption))
		{
			throw new ArgumentException("Decryption type exception: decryption type is null or empty");
		}
		if (encryption == "aes")
		{
			byte[] array = ObjectPool_0.Get();
			int num = data.Length - array.Length;
			byte[] array2 = ArrayPool<byte>.Shared.Rent(num);
			Array.Copy(data, array.Length, array2, 0, num);
			Array.Copy(data, array, array.Length);
			byte[] result;
			try
			{
				result = smethod_1(array2, num, Byte_0, array);
			}
			catch
			{
				ObjectPool_0.Return(array);
				ArrayPool<byte>.Shared.Return(array2, clearArray: true);
				throw;
			}
			ObjectPool_0.Return(array);
			ArrayPool<byte>.Shared.Return(array2, clearArray: true);
			return result;
		}
		throw new ArgumentException("Unknown encryption type: " + encryption);
	}

	public static byte[] smethod_1(byte[] cipherText, int cipherBytesLength, byte[] Key, byte[] IV)
	{
		if (cipherText != null && cipherText.Length != 0)
		{
			if (Key != null && Key.Length != 0)
			{
				if (IV != null && IV.Length != 0)
				{
					byte[] result = null;
					using Aes aes = Aes.Create();
					aes.Padding = PaddingMode.Zeros;
					aes.Key = Key;
					aes.IV = IV;
					ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);
					byte[] array = ArrayPool<byte>.Shared.Rent(cipherBytesLength);
					try
					{
						using MemoryStream memoryStream = new MemoryStream(array);
						using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
						{
							cryptoStream.Write(cipherText, 0, cipherBytesLength);
							cryptoStream.Close();
						}
						result = memoryStream.ToArray();
					}
					catch
					{
						ArrayPool<byte>.Shared.Return(array, clearArray: true);
						throw;
					}
					ArrayPool<byte>.Shared.Return(array, clearArray: true);
					return result;
				}
				throw new ArgumentNullException("IV");
			}
			throw new ArgumentNullException("Key");
		}
		throw new ArgumentNullException("cipherText");
	}

	public DataHandlerClass(string appVersion, [CanBeNull] string phpSessionId = null, bool zipped = false, GDelegate8 failHandler = null)
	{
		Bool_0 = zipped;
		Gdelegate8_0 = failHandler;
		GClass651.PhpSessionId = phpSessionId;
		GClass651.AppVersion = appVersion;
	}

	public async Task<T> SendAndHandleRetries<T>(Class312 backRequest)
	{
		Class316<T> CS_0024_003C_003E8__locals23 = new Class316<T>();
		CS_0024_003C_003E8__locals23.DataHandlerClass = this;
		CS_0024_003C_003E8__locals23.backRequest = backRequest;
		if (!CS_0024_003C_003E8__locals23.backRequest.RunInMainThread)
		{
			await AsyncWorker.RunOnBackgroundThread(delegate
			{
				CS_0024_003C_003E8__locals23.backRequest.SetupDataBlock(CS_0024_003C_003E8__locals23.DataHandlerClass.Bool_0);
			});
		}
		else
		{
			CS_0024_003C_003E8__locals23.backRequest.SetupDataBlock(Bool_0);
		}
		StringBuilder_0.Clear();
		StringBuilder_0.Append("---> Request HTTPS, id [").Append(CS_0024_003C_003E8__locals23.backRequest.RequestId).Append("]: ");
		StringBuilder_0.Append("URL: ").Append(CS_0024_003C_003E8__locals23.backRequest.CurSelectedURLFull).Append(", ");
		StringBuilder_0.Append("crc: ").Append(CS_0024_003C_003E8__locals23.backRequest.Crc);
		StringBuilder_0.Append(". ");
		GClass711_0.LogInfo(StringBuilder_0.ToString());
		Struct29<T> @struct = default(Struct29<T>);
		GException0 gException = null;
		bool flag = false;
		bool flag2 = false;
		try
		{
			@struct = await method_1<T>(CS_0024_003C_003E8__locals23.backRequest);
		}
		catch (GException4 gException2)
		{
			gException = gException2;
			ERequestErrorHandlingResult eRequestErrorHandlingResult = CS_0024_003C_003E8__locals23.method_1(gException2);
			flag = eRequestErrorHandlingResult == ERequestErrorHandlingResult.Default;
			flag2 = eRequestErrorHandlingResult == ERequestErrorHandlingResult.Expected;
		}
		catch (GException2 gException3)
		{
			gException = gException3;
			ERequestErrorHandlingResult eRequestErrorHandlingResult2 = CS_0024_003C_003E8__locals23.method_0(gException3.Code, gException3.Message);
			flag = eRequestErrorHandlingResult2 == ERequestErrorHandlingResult.Default;
			flag2 = eRequestErrorHandlingResult2 == ERequestErrorHandlingResult.Expected;
		}
		catch (Exception)
		{
			throw;
		}
		if (gException != null)
		{
			int num = CS_0024_003C_003E8__locals23.backRequest.Retries ?? 0;
			for (int num2 = 1; num2 <= num && flag; num2++)
			{
				CS_0024_003C_003E8__locals23.backRequest.IncreaseRetryIndex();
				float num3 = CS_0024_003C_003E8__locals23.backRequest.CurrentRetryCycle switch
				{
					1 => LegacyParamsStruct.SecondCycleDelaySeconds, 
					0 => LegacyParamsStruct.FirstCycleDelaySeconds, 
					_ => LegacyParamsStruct.NextCycleDelaySeconds, 
				} + UnityEngine.Random.Range(0f, LegacyParamsStruct.AdditionalRandomDelaySeconds);
				GClass711_0.LogWarn($"Request {CS_0024_003C_003E8__locals23.backRequest.MainURLFull} will be retried after {num3} sec, " + $"retry:{num2} from retries:{num} url:{CS_0024_003C_003E8__locals23.backRequest.CurSelectedURLFull} " + "error:" + gException.Message + ", ");
				await Task.Delay(TimeSpan.FromSeconds(num3));
				gException = null;
				flag2 = false;
				try
				{
					@struct = await method_1<T>(CS_0024_003C_003E8__locals23.backRequest);
				}
				catch (GException4 gException4)
				{
					gException = gException4;
					ERequestErrorHandlingResult eRequestErrorHandlingResult3 = CS_0024_003C_003E8__locals23.method_1(gException4);
					flag = eRequestErrorHandlingResult3 == ERequestErrorHandlingResult.Default;
					flag2 = false;
				}
				catch (GException2 gException5)
				{
					gException = gException5;
					ERequestErrorHandlingResult eRequestErrorHandlingResult4 = CS_0024_003C_003E8__locals23.method_0(gException5.Code, gException5.Message);
					flag = eRequestErrorHandlingResult4 == ERequestErrorHandlingResult.Default;
					flag2 = eRequestErrorHandlingResult4 == ERequestErrorHandlingResult.Expected;
				}
				catch (Exception)
				{
					throw;
				}
				if (gException == null || !flag)
				{
					break;
				}
				if (num2 == num && gException.Message.Contains("Failed to receive data"))
				{
					method_2();
				}
			}
		}
		StringBuilder_0.Clear();
		StringBuilder_0.Append("<--- Response HTTPS, id [").Append(CS_0024_003C_003E8__locals23.backRequest.RequestId).Append("]: ");
		if (gException != null)
		{
			StringBuilder_0.Append("Exception occured: ").Append(gException.ToString()).Append(", ");
		}
		StringBuilder_0.Append("URL: ").Append(CS_0024_003C_003E8__locals23.backRequest.CurSelectedURLFull).Append(", ");
		StringBuilder_0.Append("crc: ").Append(CS_0024_003C_003E8__locals23.backRequest.Crc).Append(", ");
		StringBuilder_0.Append("responseText: ");
		StringBuilder_0.Append(".");
		if (gException == null || flag2)
		{
			GClass711_0.LogInfo(StringBuilder_0.ToString());
		}
		else
		{
			GClass711_0.LogError(StringBuilder_0.ToString());
		}
		if (gException != null)
		{
			throw gException;
		}
		return @struct.ParsedObject;
	}

	public static string ToString(IDictionary<string, string> responseHeaders, string separator = "\n")
	{
		string text = string.Empty;
		if (responseHeaders != null)
		{
			text = responseHeaders.Aggregate(text, (string current, KeyValuePair<string, string> item) => current + "{" + item.Key + ":" + item.Value + "}" + separator);
		}
		return text;
	}

	public async Task<Struct29<T>> method_1<T>(Class312 backRequest)
	{
		StringBuilder_0.Clear();
		StringBuilder_0.Append("  |---> HTTPS Request execution, id [").Append(backRequest.RequestId).Append("]: ");
		StringBuilder_0.Append("URL: ").Append(backRequest.CurSelectedURLFull).Append(", ");
		StringBuilder_0.Append("crc: ").Append(backRequest.Crc);
		StringBuilder_0.Append(". ");
		GClass651.Logger.LogTrace(StringBuilder_0.ToString());
		T parsedObject = default(T);
		Exception ex = null;
		GClass650 responseData = null;
		string responseText = "";
		StringBuilder_0.Clear();
		StringBuilder_0.Append("  |<--- HTTPS Request hanlded, id [").Append(backRequest.RequestId).Append("]: ");
		try
		{
			Dictionary<string, string> headers = method_3(backRequest);
			responseData = await Interface2_0.WaitResponse(backRequest.CurSelectedURLFull, backRequest.Data, headers, backRequest.TimeoutSeconds);
			if (backRequest.CacheValidationConfigured && responseData.responseCode == 304)
			{
				StringBuilder_0.Append("Cache matched, ");
				T val;
				if (backRequest.DefaultObjectInCaseOfCache != null)
				{
					val = (T)backRequest.DefaultObjectInCaseOfCache;
				}
				else
				{
					T val2 = ((!backRequest.RunInMainThread) ? (await AsyncWorker.RunOnBackgroundThread(() => JsonParserClass.ParseJsonTo<T>(backRequest.CachedResponse.data, Array.Empty<JsonConverter>()))) : JsonParserClass.ParseJsonTo<T>(backRequest.CachedResponse.data, Array.Empty<JsonConverter>()));
					val = val2;
				}
				parsedObject = val;
			}
			else
			{
				if (backRequest.CacheValidationConfigured)
				{
					StringBuilder_0.Append("Cache mis-matched, ");
				}
				if (responseData.responseCode != 200 || !GClass856.IsNullOrEmpty(responseData.errorText))
				{
					throw new GException4(responseData.responseCode, responseData.errorText);
				}
				LastSucceedResponse = EFTDateTimeClass.UtcNow;
				if (backRequest.RunInMainThread)
				{
					responseText = method_5(backRequest, responseData);
				}
				else
				{
					responseText = await AsyncWorker.RunOnBackgroundThread(() => method_5(backRequest, responseData));
				}
				parsedObject = ((!backRequest.RunInMainThread) ? (await AsyncWorker.RunOnBackgroundThread(() => method_7<T>(backRequest, responseText))) : method_7<T>(backRequest, responseText));
			}
		}
		catch (Exception ex2)
		{
			ex = ex2;
		}
		if (responseData != null)
		{
			StringBuilder_0.Append("code: ").Append(responseData.responseCode).Append(", ");
		}
		if (ex != null)
		{
			StringBuilder_0.Append("Exception occured: ").Append(ex.ToString()).Append(", ");
		}
		StringBuilder_0.Append("URL: ").Append(backRequest.CurSelectedURLFull).Append(", ");
		StringBuilder_0.Append("crc: ").Append(backRequest.Crc).Append(", ");
		if (responseData != null)
		{
			StringBuilder_0.Append("responseHeaders: ").Append(ToString(responseData.responseHeaders)).Append(", ");
			if (!string.IsNullOrEmpty(responseText))
			{
				StringBuilder_0.Append("responseText: ");
				StringBuilder_0.Append(", ");
			}
		}
		GClass651.Logger.LogTrace(StringBuilder_0.ToString());
		if (ex != null)
		{
			throw ex;
		}
		return new Struct29<T>
		{
			ParsedObject = parsedObject,
			ResponseText = responseText
		};
	}

	public void method_2()
	{
		if (!WebDiagnosticsEnabled)
		{
			return;
		}
		GClass651.Logger.LogInfo("try run connection diagnostics");
		AsyncWorker.RunInMainTread(delegate
		{
			if (Singleton<Class2723>.Instantiated)
			{
				Singleton<Class2723>.Instance.method_0(CancellationToken.None).HandleExceptions();
			}
			else
			{
				GClass651.Logger.LogInfo("ConnectionDiagnostics not started");
			}
		});
	}

	public Dictionary<string, string> method_3(Class312 bRequest)
	{
		string value = bRequest.RequestId.ToString();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("App-Version", GClass651.AppVersion);
		if (!string.IsNullOrEmpty(GClass651.PhpSessionId))
		{
			dictionary.Add("Cookie", "PHPSESSID=" + GClass651.PhpSessionId);
		}
		dictionary.Add("GClient-RequestId", value);
		if (bRequest.CacheValidationConfigured)
		{
			dictionary.Add("If-None-Match", $"\"{bRequest.Crc}\"");
		}
		return dictionary;
	}

	public T method_4<T>(Class312 backRequest, GClass650 backResponse)
	{
		string responseJsonText = method_5(backRequest, backResponse);
		return method_7<T>(backRequest, responseJsonText);
	}

	public string method_5(Class312 backRequest, GClass650 bResponse)
	{
		byte[] array = bResponse.responseData;
		int responseDataLength = bResponse.responseDataLength;
		string responseText = bResponse.responseText;
		long num = bResponse.responseCode;
		Dictionary<string, string> responseHeaders = bResponse.responseHeaders;
		string text = smethod_0(responseHeaders);
		if (array != null && (array.Length != 0 || responseDataLength != 0 || !string.IsNullOrEmpty(responseText) || num != 0L || (responseHeaders != null && responseHeaders.Count != 0)))
		{
			string text2;
			if (Bool_0)
			{
				try
				{
					if (!string.IsNullOrEmpty(text))
					{
						GClass651.Logger.LogTrace("{0} encrypted with {1}", backRequest.CurSelectedURLFull, text);
						array = method_0(array, text);
					}
					text2 = Pooled9LevelZLib.DecompressNonAlloc(array, responseDataLength);
					if (string.IsNullOrEmpty(text2))
					{
						throw new GException5("Empty result of DecompressNonAlloc");
					}
				}
				catch (Exception innerException)
				{
					ToString(responseHeaders);
					string text3 = "Decompress error: " + responseText + ". Data length:" + ((array == null) ? "null" : "0") + ". Text:" + (responseText ?? "null") + ".";
					GClass651.Logger.LogTrace(text3);
					method_2();
					throw new GException5(text3, innerException);
				}
			}
			else
			{
				if (string.IsNullOrEmpty(responseText))
				{
					long result = num;
					if (responseHeaders != null && responseHeaders.TryGetValue("STATUS", out var value))
					{
						long.TryParse(value, out result);
					}
					ToString(responseHeaders);
					string text4 = string.Format("Empty text received: (parsed:{0}). Data length:{1}. Text length:{2}.", result, (array == null) ? "null" : responseDataLength.ToString(), (responseText == null) ? "null" : "0");
					GClass651.Logger.LogTrace(text4);
					method_2();
					throw new GException5(text4);
				}
				text2 = responseText;
				if (!string.IsNullOrEmpty(text))
				{
					GClass651.Logger.LogTrace("{0} encrypted with {1}", backRequest.CurSelectedURLFull, text);
					array = method_0(array, text);
					text2 = Encoding.UTF8.GetString(array);
					GClass651.Logger.LogTrace(text2);
				}
			}
			if (responseHeaders != null)
			{
				method_6(responseHeaders);
			}
			float num2 = (float)bResponse.stopwatch.ElapsedMilliseconds / 1000f;
			if (GClass651.Logger.IsEnabled(LogLevel.Trace))
			{
				GClass651.Logger.LogTrace("ParseResponseToText: id [{0}]: {1}, time: {2} seconds\r\n responseHeaders:{3} \r\n{4}", backRequest.RequestId, backRequest.CurSelectedURLFull, num2, ToString(responseHeaders), text2);
			}
			return text2;
		}
		method_2();
		throw new GException5(string.Format("Empty response received. Zipped:{0}. Data length:{1}", Bool_0, (array == null) ? "null" : "0"));
	}

	public void method_6(IDictionary<string, string> responseHeaders)
	{
		foreach (var (text3, text4) in responseHeaders)
		{
			if (text3.ToUpper() != "SET-COOKIE" && !text3.Contains("PHPSESSID="))
			{
				continue;
			}
			string[] array = text4.Split(';');
			foreach (string text5 in array)
			{
				int num = text5.IndexOf("PHPSESSID=", StringComparison.Ordinal);
				if (num >= 0)
				{
					GClass651.PhpSessionId = text5.Substring(num).Substring("PHPSESSID=".Length);
					break;
				}
			}
		}
	}

	public T method_7<T>(Class312 backRequest, string responseJsonText)
	{
		global::GenericParsedJsonResponseClass<T> genericParsedJsonResponseClass;
		try
		{
			GClass648 gClass = JsonParserClass.ParseJsonTo<GClass648>(responseJsonText, Array.Empty<JsonConverter>());
			genericParsedJsonResponseClass = new global::GenericParsedJsonResponseClass<T>
			{
				err = gClass.err,
				errmsg = gClass.errmsg,
				crc = gClass.crc,
				data = (GClass652.IsNotNull(gClass.data) ? JsonParserClass.ParseJsonTo<T>(gClass.data, Array.Empty<JsonConverter>()) : ((backRequest.CachedResponse == null || !GClass652.IsNotNull(backRequest.CachedResponse.data)) ? default(T) : ((backRequest.DefaultObjectInCaseOfCache != null) ? ((T)backRequest.DefaultObjectInCaseOfCache) : JsonParserClass.ParseJsonTo<T>(backRequest.CachedResponse.data, Array.Empty<JsonConverter>()))))
			};
		}
		catch (JsonSerializationException ex)
		{
			JsonParserClass.LogJsonDeserializationError<T>(responseJsonText);
			string text = ((ex.InnerException != null) ? ("\nInner exception: " + ex.InnerException.Message) : string.Empty);
			GClass651.Logger.LogError("JSON parsing error in response to " + backRequest.CurSelectedURLFull + "\r\nJSON parsing into " + typeof(T).FullName + " failed on response:\r\n" + responseJsonText + "\r\nError details: " + ex.Message + text);
			GClass2634.LogExceptionWithFullStackTrace(ex, "JSON parsing error in response to " + backRequest.CurSelectedURLFull);
			throw new GException5($"JSON parsing error in response to {backRequest.CurSelectedURLFull} at line {ex.LineNumber}, position {ex.LinePosition}: {ex.Message}{text}", ex);
		}
		catch (Exception ex2)
		{
			GClass651.Logger.LogError("JSON parsing into " + typeof(T).FullName + " failed on response:\r\n" + responseJsonText);
			GClass2634.LogExceptionWithFullStackTrace(ex2, "JSON parsing error in response to " + backRequest.CurSelectedURLFull);
			throw new GException5("In response to " + backRequest.CurSelectedURLFull + ": " + ex2.Message, ex2);
		}
		if (genericParsedJsonResponseClass.err == 0 && genericParsedJsonResponseClass.errmsg == null)
		{
			uint num = backRequest.CachedResponse?.crc ?? 0;
			GClass651.Logger.LogDebug("crc: " + genericParsedJsonResponseClass.crc + " requestCacheConfigured: " + backRequest.CacheValidationConfigured + " url: " + backRequest.CurSelectedURLFull);
			if (backRequest.ShouldUseCache && genericParsedJsonResponseClass.crc != 0 && genericParsedJsonResponseClass.crc != num)
			{
				backRequest.SaveResponseToCache(responseJsonText);
			}
			return genericParsedJsonResponseClass.data;
		}
		throw new GException2(genericParsedJsonResponseClass.err, genericParsedJsonResponseClass.errmsg);
	}
}
