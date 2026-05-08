using EFT;

public abstract class GClass2282
{
	public abstract InteractPacketStruct GetInteractPacket(int objectId, EventObject.EInteraction interaction);

	public abstract void InteractWithEventObject(Player player, InteractPacketStruct packet);

	public GClass2282()
	{
	}
}
