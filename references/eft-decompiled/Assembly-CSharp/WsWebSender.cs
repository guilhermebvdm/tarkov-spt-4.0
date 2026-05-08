using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Diz.Utils;
using Newtonsoft.Json;
using UnityEngine;
using WebSocketSharp;

public class WsWebSender : MonoBehaviour
{
	[CompilerGenerated]
	public class Class322
	{
		public WsWebSender wsWebSender_0;

		public DateTime startTime;

		public void method_0()
		{
			wsWebSender_0.webSocket_0.Connect();
		}

		public bool method_1()
		{
			return (DateTime.Now - startTime).TotalSeconds < 30.0;
		}
	}

	[CompilerGenerated]
	public class Class323
	{
		public GClass654 reqToSend;

		public Func<bool> func_0;

		public bool method_0()
		{
			return !reqToSend.CheckTimedOut();
		}
	}

	[CompilerGenerated]
	public class Class324
	{
		public WsWebSender wsWebSender_0;

		public GClass654 requestToSend;

		public void method_0()
		{
			wsWebSender_0.webSocket_0.Send(requestToSend.RequestJsonText);
		}
	}

	[CompilerGenerated]
	public class Class325
	{
		public WsWebSender wsWebSender_0;

		public string payload;

		public string wsMessageId;

		public GClass657 method_0()
		{
			return JsonParserClass.ParseJsonTo<GClass657>(payload, Array.Empty<JsonConverter>());
		}

		public void method_1()
		{
			wsWebSender_0.webSocket_0.Send(GClass653.GetConfirmationText(wsMessageId));
		}
	}

	[CompilerGenerated]
	public class Class326
	{
		public Action<string> action;

		public string notificationParams;

		public void method_0()
		{
			action(notificationParams);
		}
	}

	private const int int_0 = 1000;

	private const int int_1 = 2000;

	private const float float_0 = 1f;

	private const float float_1 = 1f;

	private readonly float[] float_2 = new float[7] { 0.2f, 0.4f, 0.8f, 1.6f, 3.2f, 6.4f, 12.8f };

	private const float float_3 = 30f;

	private WsConnectionStatus wsConnectionStatus_0;

	private CloseStatusCode closeStatusCode_0;

	private CancellationTokenSource cancellationTokenSource_0;

	private bool bool_0;

	private DateTime dateTime_0;

	private float float_4 = 1f;

	private float[] float_5;

	private bool bool_1;

	private int int_2;

	private float float_6;

	private WebSocket webSocket_0;

	private readonly Dictionary<string, Action<string>> dictionary_0 = new Dictionary<string, Action<string>>();

	private readonly Stack<GClass654> stack_0 = new Stack<GClass654>();

	private readonly GClass656 gclass656_0 = new GClass656();

	private readonly GClass656 gclass656_1 = new GClass656();

	private readonly Dictionary<string, GClass654> dictionary_1 = new Dictionary<string, GClass654>();

	private readonly List<string> list_0 = new List<string>();

	private readonly float[] float_7 = new float[6] { 1f, 1f, 1f, 3f, 5f, 10f };

	private Action action_0;

	public WsConnectionStatus ConnectionStatus => wsConnectionStatus_0;

	public CloseStatusCode CloseStatusCode => closeStatusCode_0;

	public bool DebugLastPingResult => bool_0;

	public bool DebugIsPingResultOutdated => (DateTime.Now - dateTime_0).TotalSeconds >= (double)float_4;

	public int DebugCurrRunningRequestsCount => gclass656_0.Count + gclass656_1.Count + dictionary_1.Count;

	public int DebugCurrSerializingReqs => gclass656_0.Count;

	public int DebugCurrSendingReqs => gclass656_1.Count;

	public int DebugCurrWaitingResponseReqs => dictionary_1.Count;

	public float DebugCurrReconnectIntervalTimer => float_6;

	public bool DebugReqConfirmInProgress => bool_1;

	public bool Boolean_0
	{
		get
		{
			if (float_5 != null)
			{
				return float_5.Length != 0;
			}
			return false;
		}
	}

	public async Task EstablishConnection(Struct36 connParams)
	{
		if (webSocket_0 == null)
		{
			webSocket_0 = new WebSocket(connParams.EndpointUrl);
			webSocket_0.Log.Level = LogLevel.Trace;
			WebSocketSharp.Logger log = webSocket_0.Log;
			log.Output = (Action<LogData, string>)Delegate.Combine(log.Output, new Action<LogData, string>(method_23));
			webSocket_0.OnOpen += method_19;
			webSocket_0.OnClose += method_20;
			webSocket_0.OnMessage += method_21;
			webSocket_0.OnError += method_22;
			webSocket_0.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls12;
			webSocket_0.SetToken(connParams.TokenB64, preAuth: true);
			action_0 = connParams.ActionOnReconnect;
			if (connParams.KeepAliveInterval == 0f)
			{
				GClass651.Logger.LogWarn($"WsWebSender - KeepAliveInterval is 0f, set fallback value ({1f})");
				float_4 = 1f;
			}
			else
			{
				float_4 = connParams.KeepAliveInterval;
			}
			float_5 = connParams.ConfirmationTimeouts;
			DateTime startTime = DateTime.Now;
			cancellationTokenSource_0 = new CancellationTokenSource();
			method_2(cancellationTokenSource_0.Token).HandleExceptions();
			method_1(cancellationTokenSource_0.Token).HandleExceptions();
			method_3(cancellationTokenSource_0.Token).HandleExceptions();
			method_4(cancellationTokenSource_0.Token).HandleExceptions();
			method_5(cancellationTokenSource_0.Token).HandleExceptions();
			await AsyncWorker.RunOnBackgroundThread(delegate
			{
				webSocket_0.Connect();
			});
			await method_12(() => (DateTime.Now - startTime).TotalSeconds < 30.0, cancellationTokenSource_0.Token);
			if ((DateTime.Now - startTime).TotalSeconds >= 30.0)
			{
				cancellationTokenSource_0.Cancel();
				throw new GException6("Initial WS connect timed out");
			}
		}
	}

	public async Task CloseConnection()
	{
		if (ConnectionStatus == WsConnectionStatus.InitialNotConnected)
		{
			GClass651.Logger.LogError("WsWebSender - Trying to close not opened connection");
			return;
		}
		cancellationTokenSource_0.Cancel();
		cancellationTokenSource_0.Dispose();
		if (ConnectionStatus == WsConnectionStatus.Connected)
		{
			webSocket_0.Close();
		}
		WebSocketSharp.Logger log = webSocket_0.Log;
		log.Output = (Action<LogData, string>)Delegate.Remove(log.Output, new Action<LogData, string>(method_23));
		webSocket_0.OnOpen -= method_19;
		webSocket_0.OnClose -= method_20;
		webSocket_0.OnMessage -= method_21;
		webSocket_0.OnError -= method_22;
		webSocket_0 = null;
		bool_1 = false;
	}

	public bool method_0(WsConnectionStatus connectionStatus)
	{
		if (connectionStatus != WsConnectionStatus.Connected)
		{
			return connectionStatus == WsConnectionStatus.AwaitingReconnect;
		}
		return true;
	}

	public async Task<GClass657> SendRequest(GClass655 requestJson, int timeoutSeconds)
	{
		if (requestJson.Id == "0")
		{
			throw new GException6("Request/response logic shouldn't handle request [id = 0], use SendNotification instead.");
		}
		if (!method_0(ConnectionStatus))
		{
			return new GClass657
			{
				ID = requestJson.Id,
				Status = "Connection is not active, request finished with empty response without sending it"
			};
		}
		GClass654 gClass = method_14();
		gClass.Initialize(requestJson, timeoutSeconds);
		gclass656_0.Enqueue(gClass);
		while (gClass.Status == GClass654.WsRequestStatus.InProgress)
		{
			await Task.Yield();
		}
		switch (gClass.Status)
		{
		default:
			method_15(gClass);
			throw new GException6("WsSender - Unknown request routine result");
		case GClass654.WsRequestStatus.FinishedWithException:
			method_15(gClass);
			throw gClass.ResultException;
		case GClass654.WsRequestStatus.FinishiedWithResponse:
		{
			GClass657 obj = gClass.ResultResponse ?? throw new GException6("WsException -  Null response received for request [id = " + requestJson.Id + "]!");
			obj.Status = "Request finished by response message with data from receiver";
			method_15(gClass);
			return obj;
		}
		}
	}

	public void AddNotificationToHandle(string notificationId, Action<string> action)
	{
		if (notificationId == "0")
		{
			throw new GException6("Request/response logic shouldn't handle request [id = 0]");
		}
		dictionary_0.Add(notificationId, action);
	}

	public void RemoveHandlingNotification(string notificationId)
	{
		if (dictionary_0.ContainsKey(notificationId))
		{
			dictionary_0.Remove(notificationId);
		}
	}

	public async Task SendNotification(GClass655 requestJson)
	{
		throw new NotImplementedException();
	}

	public async Task method_1(CancellationToken cancellationToken)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (ConnectionStatus == WsConnectionStatus.Connected && !((DateTime.Now - dateTime_0).TotalSeconds < (double)float_4))
			{
				await method_17();
			}
			await Task.Yield();
		}
	}

	public async Task method_2(CancellationToken cancellationToken)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (ConnectionStatus == WsConnectionStatus.AwaitingReconnect)
			{
				GClass651.Logger.LogInfo($"WsWebSender - Automatic reconnect for socket [{webSocket_0.Url}] try");
				try
				{
					await AsyncWorker.RunOnBackgroundThread(delegate
					{
						webSocket_0.Connect();
					});
					if (await method_17())
					{
						GClass651.Logger.LogInfo("WsWebSender - Reconnected successfully");
						method_13();
						action_0?.Invoke();
					}
					else
					{
						float num = float_7[int_2];
						int_2++;
						if (int_2 >= float_7.Length)
						{
							int_2 = float_7.Length - 1;
						}
						GClass651.Logger.LogInfo($"WsWebSender - Reconnect failed, waiting interval {num} s");
						float_6 = num;
						while (!(float_6 <= 0f))
						{
							await Task.Yield();
							float_6 -= Time.unscaledDeltaTime;
						}
						float_6 = 0f;
					}
				}
				catch (Exception e)
				{
					GClass651.Logger.LogException(e);
				}
			}
			await Task.Yield();
		}
	}

	public async Task method_3(CancellationToken cancellationToken)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			while (gclass656_0.Count > 0)
			{
				gclass656_0.FinishTimedOutRequests();
				GClass654 gClass = gclass656_0.Dequeue();
				await gClass.SerializeDataToTextAsync();
				gclass656_1.Enqueue(gClass);
			}
			await Task.Yield();
		}
	}

	public async Task method_4(CancellationToken cancellationToken)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			while (gclass656_1.Count > 0)
			{
				gclass656_1.FinishTimedOutRequests();
				await method_12(delegate
				{
					gclass656_1.FinishTimedOutRequests();
					return gclass656_1.Count > 0;
				}, cancellationToken);
				cancellationToken.ThrowIfCancellationRequested();
				if (gclass656_1.Count > 0)
				{
					GClass654 reqToSend = gclass656_1.Dequeue();
					bool_1 = true;
					await method_6(reqToSend, cancellationToken);
					bool_1 = false;
				}
			}
			await Task.Yield();
		}
	}

	public async Task method_5(CancellationToken cancellationToken)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			foreach (var (item, gClass2) in dictionary_1)
			{
				if (gClass2.CheckTimedOut())
				{
					gClass2.FinishWithTimeout();
					list_0.Add(item);
				}
			}
			foreach (string item2 in list_0)
			{
				dictionary_1.Remove(item2);
			}
			list_0.Clear();
			await Task.Yield();
		}
	}

	public async Task method_6(GClass654 reqToSend, CancellationToken cancellationToken)
	{
		dictionary_1.Add(reqToSend.RequestId, reqToSend);
		if (!(await method_9(reqToSend)))
		{
			dictionary_1.Remove(reqToSend.RequestId);
			return;
		}
		cancellationToken.ThrowIfCancellationRequested();
		if (Boolean_0)
		{
			await method_8(reqToSend, cancellationToken);
		}
		else
		{
			await method_7(reqToSend, cancellationToken);
		}
	}

	public async Task method_7(GClass654 reqToSend, CancellationToken cancellationToken)
	{
		do
		{
			if (!method_10(reqToSend))
			{
				cancellationToken.ThrowIfCancellationRequested();
				continue;
			}
			return;
		}
		while (!reqToSend.CheckTimedOut());
		reqToSend.FinishWithTimeout();
		dictionary_1.Remove(reqToSend.RequestId);
	}

	public async Task method_8(GClass654 reqToSend, CancellationToken cancellationToken)
	{
		int num = 0;
		while (true)
		{
			if (method_10(reqToSend))
			{
				return;
			}
			DateTime now = DateTime.Now;
			float num2 = float_5[num];
			while (!method_10(reqToSend) && !((DateTime.Now - now).TotalSeconds >= (double)num2))
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (reqToSend.CheckTimedOut())
				{
					reqToSend.FinishWithTimeout();
					dictionary_1.Remove(reqToSend.RequestId);
					return;
				}
				await Task.Yield();
			}
			if (!method_10(reqToSend))
			{
				await method_12(() => !reqToSend.CheckTimedOut(), cancellationToken);
				if (reqToSend.CheckTimedOut())
				{
					break;
				}
				if (!(await method_9(reqToSend)))
				{
					dictionary_1.Remove(reqToSend.RequestId);
					return;
				}
				if (num < float_5.Length - 1)
				{
					num++;
				}
			}
		}
		reqToSend.FinishWithTimeout();
		dictionary_1.Remove(reqToSend.RequestId);
	}

	public async Task<bool> method_9(GClass654 requestToSend)
	{
		bool flag = false;
		try
		{
			await AsyncWorker.RunOnBackgroundThread(delegate
			{
				webSocket_0.Send(requestToSend.RequestJsonText);
			});
		}
		catch (Exception exc)
		{
			flag = true;
			requestToSend.FinishWithException(exc);
		}
		return !flag;
	}

	public bool method_10(GClass654 req)
	{
		if (!req.ReceivingConfirmed)
		{
			return req.IsFinished;
		}
		return true;
	}

	public async void method_11(object sender, MessageEventArgs e)
	{
		try
		{
			if (!GClass653.TryGetResponseText(e, out var wsMessageId, out var payload))
			{
				throw new NotImplementedException("WsWebSender - received message has wrong backend contract format!");
			}
			GClass657 gClass = await AsyncWorker.RunOnBackgroundThread(() => JsonParserClass.ParseJsonTo<GClass657>(payload, Array.Empty<JsonConverter>()));
			if (gClass.ID == "0")
			{
				throw new NotImplementedException("WsWebSender - message with id 0 received! Notifications are not implemented!");
			}
			Action<string> action;
			if (dictionary_1.TryGetValue(gClass.ID, out var value))
			{
				value.ConfirmReceiveFromServer();
				value.FinishWithResponse(gClass);
				if (dictionary_1.ContainsKey(gClass.ID))
				{
					dictionary_1.Remove(gClass.ID);
				}
				else
				{
					GClass651.Logger.LogError("Received response with id " + gClass.ID + " is not is not awaited by _requestsToAwaitResult list!");
				}
			}
			else if (gClass.Method != null && dictionary_0.TryGetValue(gClass.Method, out action))
			{
				string notificationParams = JsonParserClass.ParseJsonTo<Dictionary<string, object>>(payload, Array.Empty<JsonConverter>())["params"].ToString();
				AsyncWorker.RunInMainTread(delegate
				{
					action(notificationParams);
				});
			}
			else
			{
				GClass651.Logger.LogError("WsWebSender - Non awaited message received with id=" + gClass.ID + "!");
			}
			await AsyncWorker.RunOnBackgroundThread(delegate
			{
				webSocket_0.Send(GClass653.GetConfirmationText(wsMessageId));
			});
		}
		catch (Exception e2)
		{
			GClass651.Logger.LogException(e2);
		}
	}

	public async Task method_12(Func<bool> updateContinueCondition, CancellationToken cancellationToken)
	{
		if (ConnectionStatus != WsConnectionStatus.Connected)
		{
			method_13();
		}
		else
		{
			await method_17();
		}
		while (true)
		{
			bool flag;
			if (!(flag = ConnectionStatus != WsConnectionStatus.Connected))
			{
				flag = !(await method_18(1f));
			}
			if (!flag)
			{
				break;
			}
			if (ConnectionStatus != WsConnectionStatus.Connected)
			{
				while (ConnectionStatus == WsConnectionStatus.AwaitingReconnect)
				{
					cancellationToken.ThrowIfCancellationRequested();
					if (!updateContinueCondition())
					{
						return;
					}
					await Task.Yield();
				}
				if (ConnectionStatus != WsConnectionStatus.Connected)
				{
					throw new GException6("Trying to wait for reconnect while no reconnection logic is performed.");
				}
			}
			else
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (!updateContinueCondition())
				{
					break;
				}
				await Task.Yield();
			}
		}
	}

	public void method_13()
	{
		float_6 = 0f;
		int_2 = 0;
	}

	public GClass654 method_14()
	{
		if (stack_0.Count != 0)
		{
			return stack_0.Pop();
		}
		return new GClass654();
	}

	public void method_15(GClass654 req)
	{
		req.Dispose();
		stack_0.Push(req);
	}

	public bool method_16()
	{
		try
		{
			bool_0 = webSocket_0 != null && webSocket_0.Ping();
		}
		catch (Exception e)
		{
			GClass651.Logger.LogException(e);
			bool_0 = false;
		}
		finally
		{
			dateTime_0 = DateTime.Now;
		}
		return bool_0;
	}

	public async Task<bool> method_17()
	{
		return await AsyncWorker.RunOnBackgroundThread((Func<bool>)method_16);
	}

	public async Task<bool> method_18(float outdateIntervalSec)
	{
		if ((DateTime.Now - dateTime_0).TotalSeconds > (double)outdateIntervalSec)
		{
			await method_17();
		}
		return bool_0;
	}

	public void method_19(object sender, EventArgs e)
	{
		wsConnectionStatus_0 = WsConnectionStatus.Connected;
		GClass651.Logger.LogInfo($"WsWebSender - Connection opened on url [{webSocket_0.Url}]");
	}

	public void method_20(object sender, CloseEventArgs e)
	{
		closeStatusCode_0 = (CloseStatusCode)e.Code;
		wsConnectionStatus_0 = (e.WasClean ? WsConnectionStatus.Disconnected : WsConnectionStatus.AwaitingReconnect);
		if (e.WasClean)
		{
			GClass651.Logger.LogInfo($"WsWebSender - Connection closed cleanly on [{webSocket_0.Url}], " + $"code: [{e.Code}] {(CloseStatusCode)e.Code}, reason: {e.Reason}, wasClean: {e.WasClean}");
		}
		else
		{
			GClass651.Logger.LogError($"WsWebSender - Connection terminated abnormal on [{webSocket_0.Url}], " + $"code: [{e.Code}] {(CloseStatusCode)e.Code}, reason: {e.Reason}, wasClean: {e.WasClean}");
		}
	}

	public void method_21(object sender, MessageEventArgs e)
	{
		try
		{
			method_11(sender, e);
		}
		catch (Exception e2)
		{
			GClass651.Logger.LogException(e2);
		}
	}

	public void method_22(object sender, ErrorEventArgs e)
	{
		GClass651.Logger.LogError($"WebSocketSharp internal error - {e}");
	}

	public void method_23(LogData data, string message)
	{
		GClass651.Logger.LogTrace("WebSocketSharp internal log - " + data);
	}

	public async Task<bool> DebugPingManuallyAsync()
	{
		return await method_17();
	}

	[CompilerGenerated]
	public void method_24()
	{
		webSocket_0.Connect();
	}

	[CompilerGenerated]
	public bool method_25()
	{
		gclass656_1.FinishTimedOutRequests();
		return gclass656_1.Count > 0;
	}
}
