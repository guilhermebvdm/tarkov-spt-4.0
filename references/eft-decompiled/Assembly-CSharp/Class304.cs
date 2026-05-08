using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Comfort.Common;
using Diz.Utils;
using JetBrains.Annotations;
using NLog;

public class Class304
{
	public class Class386 : LoggerClass
	{
		public Class386(LoggerMode mode)
			: base(LogManager.GetLogger("backend"), mode)
		{
		}
	}

	public enum EBackEndCoreState
	{
		Free,
		Stopping,
		Stopped
	}

	[CompilerGenerated]
	public class Class309<T>
	{
		public Class312 backRequest;

		public Class304 class304_0;

		public void method_0()
		{
			backRequest.SetupCacheValidation(class304_0.Class310_0);
		}
	}

	[NonSerialized]
	public Class386 Class386_0;

	[NonSerialized]
	[CanBeNull]
	public Class310 Class310_0;

	[NonSerialized]
	public DataHandlerClass DataHandlerClass;

	[NonSerialized]
	public Class321 Class321_0;

	[NonSerialized]
	public string[] String_0_1;

	[NonSerialized]
	public string[] String_1_1;

	[NonSerialized]
	public EBackEndCoreState EbackEndCoreState_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public HashSet<string> HashSet_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	public GInterface17 Cache => Class310_0;

	public bool Destroyed
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

	public string String_0 => GClass651.AppVersion;

	[CanBeNull]
	public string String_1 => GClass651.PhpSessionId;

	public DateTime DateTime_0 => DataHandlerClass.LastSucceedResponse;

	public Class304(DataHandlerClass httpTransport, Class321 wsTransport, Class310 cache = null)
	{
		Class310_0 = cache;
		Class386_0 = new Class386(LoggerMode.Add);
		DataHandlerClass = httpTransport;
		Class321_0 = wsTransport;
	}

	public void method_0(bool webDiagnosticsEnabled)
	{
		DataHandlerClass.WebDiagnosticsEnabled = webDiagnosticsEnabled;
	}

	public async void method_1(string url, float delay, CancellationToken cts, bool inBackground = true, object @params = null)
	{
		while (!Destroyed)
		{
			if (Int_0 == 0)
			{
				DateTime lastSucceedResponse = DataHandlerClass.LastSucceedResponse;
				if ((EFTDateTimeClass.UtcNow - lastSucceedResponse).TotalSeconds > (double)delay)
				{
					Interlocked.Exchange(ref Int_0, 1);
					method_6(new LegacyParamsStruct
					{
						Url = url,
						Params = @params,
						Retries = LegacyParamsStruct.MaximumRetries,
						HandlingMode = ERequestHandlingMode.Default,
						ParseInBackground = inBackground
					}, delegate
					{
						Interlocked.Exchange(ref Int_0, 0);
					}).HandleExceptions();
				}
			}
			await Task.Yield();
			if (cts.IsCancellationRequested)
			{
				break;
			}
		}
	}

	public void AddNotification(string notificationType, Action<string> action)
	{
		try
		{
			Class321_0.AddNotificationToHandle(notificationType, action);
		}
		catch (Exception e)
		{
			GClass651.Logger.LogException(e);
			throw;
		}
	}

	public void RemoveNotificationHandling(string notificationType)
	{
		try
		{
			Class321_0.RemoveHandlingNotification(notificationType);
		}
		catch (Exception e)
		{
			GClass651.Logger.LogException(e);
			throw;
		}
	}

	public async Task<T> method_2<T>(Class312 backRequest)
	{
		if (EbackEndCoreState_0 != EBackEndCoreState.Free)
		{
			Class386_0.LogError("Session is destroyed");
			throw new Exception("session is destroyed");
		}
		long id = Interlocked.Increment(ref GClass651.NextRequestIndex);
		ETransportProtocolType eTransportProtocolType = method_10(backRequest.BackendMethod);
		backRequest.SetupIdAndProtocol(id, eTransportProtocolType);
		T result;
		try
		{
			switch (eTransportProtocolType)
			{
			default:
				throw new Exception("You add new protocol, but forgot to handle it in Backend.Send(..)");
			case ETransportProtocolType.HTTPS:
				if (!backRequest.RunInMainThread)
				{
					await AsyncWorker.RunOnBackgroundThread(delegate
					{
						backRequest.SetupCacheValidation(Class310_0);
					});
				}
				else
				{
					backRequest.SetupCacheValidation(Class310_0);
				}
				result = await DataHandlerClass.SendAndHandleRetries<T>(backRequest);
				break;
			case ETransportProtocolType.WSS:
				result = await Class321_0.SendRequest<T>(backRequest);
				break;
			}
		}
		catch (GException0)
		{
			throw;
		}
		catch (Exception e)
		{
			GClass651.Logger.LogException(e);
			throw;
		}
		if (EbackEndCoreState_0 == EBackEndCoreState.Stopping)
		{
			EbackEndCoreState_0 = EBackEndCoreState.Stopped;
		}
		return result;
	}

	public async Task<T> method_3<T>(LegacyParamsStruct request)
	{
		Result<T> result = await method_9<T>(request);
		if (result.Failed)
		{
			throw new Exception(result.Error);
		}
		return result.Value;
	}

	public async Task<IResult> method_4(LegacyParamsStruct request)
	{
		Result<object> result = await method_9<object>(request);
		IResult result2;
		if (!result.Failed)
		{
			result2 = SuccessfulResult.New;
		}
		else
		{
			IResult result3 = new FailedResult(result.Error, result.ErrorCode);
			result2 = result3;
		}
		return result2;
	}

	public async Task method_5<T>(LegacyParamsStruct request, Callback<T> onFinished)
	{
		Result<T> result = await method_9<T>(request);
		onFinished?.Invoke(result);
	}

	public async Task<IResult> method_6(LegacyParamsStruct request, Callback onFinished)
	{
		Result<object> sourceResult = await method_9<object>(request);
		onFinished?.Invoke(sourceResult);
		IResult result;
		if (!sourceResult.Failed)
		{
			result = SuccessfulResult.New;
		}
		else
		{
			IResult result2 = new FailedResult(sourceResult.Error, sourceResult.ErrorCode);
			result = result2;
		}
		return result;
	}

	public async Task method_7()
	{
		EbackEndCoreState_0 = EBackEndCoreState.Stopping;
		while (EbackEndCoreState_0 == EBackEndCoreState.Stopping)
		{
			await Task.Yield();
		}
		EbackEndCoreState_0 = EBackEndCoreState.Stopped;
	}

	public void method_8(IEnumerable<string> wssMethods)
	{
		if (wssMethods == null)
		{
			throw new Exception("RequestsMadeThroughLobby collection is null, so it's not presented in parsed data.");
		}
		HashSet_0 = new HashSet<string>(wssMethods);
	}

	public async Task<Result<T>> method_9<T>(LegacyParamsStruct request)
	{
		Class312 backRequest = Class312.CreateFromLegacyParams(request);
		T val;
		try
		{
			val = await method_2<T>(backRequest);
			TaskCompletionSource taskCompletionSource = new TaskCompletionSource();
			AsyncWorker.RunInMainTread(taskCompletionSource.Complete);
			await taskCompletionSource.Task;
		}
		catch (GException1 gException)
		{
			string url = request.Url;
			string error = ((!string.IsNullOrEmpty(gException.Message)) ? gException.Message : gException.Code.ToString());
			GClass651.Logger.LogTrace("We are still use error message handling like normal behaviour for " + url + ", exception:" + gException.ToString() + ".\nThis is legacy, remove that in future.");
			return new Result<T>
			{
				Error = error,
				ErrorCode = gException.Code
			};
		}
		catch (Exception ex)
		{
			return new Result<T>
			{
				Error = ex.Message
			};
		}
		return val;
	}

	public ETransportProtocolType method_10(string methodPath)
	{
		if (Bool_0 && HashSet_0 != null && HashSet_0.Contains(methodPath))
		{
			return ETransportProtocolType.WSS;
		}
		return ETransportProtocolType.HTTPS;
	}

	public static string GetMd5Hash(string input)
	{
		using MD5 mD = MD5.Create();
		byte[] array = mD.ComputeHash(Encoding.UTF8.GetBytes(input));
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("x2"));
		}
		return stringBuilder.ToString();
	}

	[CompilerGenerated]
	public void method_11(IResult result)
	{
		Interlocked.Exchange(ref Int_0, 0);
	}
}
