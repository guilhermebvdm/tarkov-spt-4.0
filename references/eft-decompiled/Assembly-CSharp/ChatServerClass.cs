using System;
using Newtonsoft.Json;

public class ChatServerClass
{
	[JsonProperty("_id")]
	public string ServerId;

	public int RegistrationId;

	public string VersionId;

	public string Ip;

	public int Port;

	public DateTime DateTime;

	public GClass1051[] Chats;

	public string[] Regions;
}
