using WebSocketSharp;

public abstract class GClass653
{
	public const string SEND_MESSAGE_KEY_WORD = "MESSAGE";

	public const string CONFIRM_MESSAGE_KEY_WORD = "CONFIRM";

	public static string GetRequestText(GClass655 wsRequestJson)
	{
		return "MESSAGE " + wsRequestJson.Method + " " + JsonParserClass.ToJson(wsRequestJson);
	}

	public static string GetConfirmationText(string wsMessageId)
	{
		return "CONFIRM " + wsMessageId;
	}

	public static bool TryGetResponseText(MessageEventArgs e, out string wsMessageId, out string payload)
	{
		return smethod_0(e, out wsMessageId, out payload);
	}

	public static bool smethod_0(MessageEventArgs e, out string wsMessageId, out string payload)
	{
		wsMessageId = "";
		payload = "";
		string data = e.Data;
		GClass651.Logger.LogInfo("WebSocketSharp - message received: " + data);
		char[] separator = new char[1] { ' ' };
		string[] array = e.Data.Split(separator, 2);
		if (array.Length > 1)
		{
			wsMessageId = array[0];
			payload = array[1];
			return true;
		}
		return false;
	}
}
