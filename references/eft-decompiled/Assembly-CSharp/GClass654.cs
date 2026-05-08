using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Diz.Utils;

public class GClass654 : IDisposable
{
	public enum WsRequestStatus
	{
		NonInitialized,
		InProgress,
		FinishiedWithResponse,
		FinishedWithException
	}

	[NonSerialized]
	public GClass655 Gclass655_0;

	public int _timeoutSeconds;

	public DateTime _creationTime;

	[NonSerialized]
	[CompilerGenerated]
	public WsRequestStatus WsRequestStatus_0;

	[NonSerialized]
	[CompilerGenerated]
	public GClass657 Gclass657_0;

	[NonSerialized]
	[CompilerGenerated]
	public Exception Exception_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public string String_0;

	public WsRequestStatus Status
	{
		[CompilerGenerated]
		get
		{
			return WsRequestStatus_0;
		}
		[CompilerGenerated]
		set
		{
			WsRequestStatus_0 = value;
		}
	}

	public GClass657 ResultResponse
	{
		[CompilerGenerated]
		get
		{
			return Gclass657_0;
		}
		[CompilerGenerated]
		set
		{
			Gclass657_0 = value;
		}
	}

	public Exception ResultException
	{
		[CompilerGenerated]
		get
		{
			return Exception_0;
		}
		[CompilerGenerated]
		set
		{
			Exception_0 = value;
		}
	}

	public bool ReceivingConfirmed
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public bool IsFinished
	{
		get
		{
			if (Status != WsRequestStatus.FinishiedWithResponse)
			{
				return Status == WsRequestStatus.FinishedWithException;
			}
			return true;
		}
	}

	public string RequestJsonText
	{
		[CompilerGenerated]
		get
		{
			return String_0;
		}
		[CompilerGenerated]
		set
		{
			String_0 = value;
		}
	}

	public string RequestId => Gclass655_0.Id;

	public void Initialize(GClass655 json, int timeoutSec)
	{
		Gclass655_0 = json;
		_timeoutSeconds = timeoutSec;
		_creationTime = DateTime.Now;
		Status = WsRequestStatus.InProgress;
	}

	public void Dispose()
	{
		Gclass655_0 = null;
		_timeoutSeconds = 0;
		_creationTime = default(DateTime);
		Status = WsRequestStatus.NonInitialized;
	}

	public async Task SerializeDataToTextAsync()
	{
		RequestJsonText = await AsyncWorker.RunOnBackgroundThread(() => GClass653.GetRequestText(Gclass655_0));
	}

	public bool CheckTimedOut()
	{
		return (DateTime.Now - _creationTime).TotalSeconds > (double)_timeoutSeconds;
	}

	public void ConfirmReceiveFromServer()
	{
		ReceivingConfirmed = true;
	}

	public void FinishWithResponse(GClass657 responseJson)
	{
		ResultResponse = responseJson;
		Status = WsRequestStatus.FinishiedWithResponse;
	}

	public void FinishWithException(Exception exc)
	{
		ResultException = exc;
		Status = WsRequestStatus.FinishedWithException;
	}

	public void FinishWithTimeout()
	{
		if (Gclass655_0 == null)
		{
			GClass651.Logger.LogError("\nTrying to finish request by timeout with null requestJson!\nRequest message: " + RequestJsonText + $"\nReceivingConfirmed: {ReceivingConfirmed}" + "\nResultResponse id: " + ResultResponse?.ID);
			if (ReceivingConfirmed)
			{
				return;
			}
		}
		FinishWithException(new GException7("WsException - Timeout for request [id = " + RequestId + "]"));
	}

	[CompilerGenerated]
	public string method_0()
	{
		return GClass653.GetRequestText(Gclass655_0);
	}
}
