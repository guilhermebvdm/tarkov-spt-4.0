using EFT.Communications;

public struct GStruct87
{
	public string MessageId;

	public string EventId;

	public static GStruct87 Create(ProfileChangeEvent profileEvent)
	{
		return new GStruct87
		{
			EventId = profileEvent.Id,
			MessageId = profileEvent.MessageId
		};
	}
}
