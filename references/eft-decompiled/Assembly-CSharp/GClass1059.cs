using ChatShared;
using Newtonsoft.Json;

public class GClass1059
{
	[JsonProperty("_id")]
	public string Id;

	public float deleteTime;

	public int type;

	public ChatMessageClass message;

	public int @new;

	public int attachmentsNew;

	public bool pinned;

	public string Owner;

	public string Name;

	public UpdatableChatMember[] Users;
}
