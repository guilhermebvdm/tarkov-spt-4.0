using EFT;

[GAttribute29]
public class GClass1945
{
	public string Id;

	public ClassVector3 Position;

	public ClassVector3 Rotation;

	public InventoryDescriptorClass Item;

	[GAttribute30]
	public MongoID[] ValidProfiles;

	public bool IsContainer;

	public bool UseGravity;

	public bool RandomRotation;

	public ClassVector3 Shift = new ClassVector3();

	public short PlatformId;
}
