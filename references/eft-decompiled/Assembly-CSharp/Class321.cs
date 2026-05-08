using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class Class321
{
	[NonSerialized]
	public WsWebSender WsWebSender_0 = new WsWebSender();

	public WsWebSender Sender => WsWebSender_0;

	public async Task EstablishConnectionToUrl(Struct36 connParams)
	{
		await WsWebSender_0.EstablishConnection(connParams);
	}

	public async Task CloseConnection()
	{
		await WsWebSender_0.CloseConnection();
	}

	public async Task<T> SendRequest<T>(Class312 reqParams)
	{
		long requestId = reqParams.RequestId;
		GClass655 gClass = new GClass655
		{
			Method = reqParams.BackendMethod,
			@params = reqParams.Params,
			Id = requestId.ToString()
		};
		GClass651.Logger.LogInfo($"---> Request WSS, id [{gClass.Id}]: {gClass}");
		GClass657 gClass2 = await WsWebSender_0.SendRequest(gClass, reqParams.TimeoutSeconds);
		GClass651.Logger.LogInfo($"<--- Response WSS, id [{gClass2.ID}]: {gClass2}");
		if (gClass2.Error != null)
		{
			throw new GException2(GClass856.ParseToInt(gClass2.Error.Code), gClass2.Error.Message);
		}
		return GClass652.IsNull(gClass2.Result) ? default(T) : JsonParserClass.ParseJsonTo<T>(gClass2.Result, Array.Empty<JsonConverter>());
	}

	public void AddNotificationToHandle(string notificationId, Action<string> action)
	{
		WsWebSender_0.AddNotificationToHandle(notificationId, action);
	}

	public void RemoveHandlingNotification(string notificationId)
	{
		WsWebSender_0.RemoveHandlingNotification(notificationId);
	}

	public void SendNotification<T>(Class312 reqParams)
	{
		GClass655 requestJson = new GClass655
		{
			Method = reqParams.BackendMethod,
			@params = reqParams.Params,
			Id = "0"
		};
		WsWebSender_0.SendNotification(requestJson);
	}
}
