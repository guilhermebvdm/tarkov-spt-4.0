using EFT;

[GAttribute29]
public class GClass1949
{
	[GAttribute30]
	public MongoID? ParentId;

	public string ContainerId;

	public GClass1949()
	{
	}

	public GClass1949(string parentId, string containerId)
	{
		ParentId = parentId;
		ContainerId = containerId;
	}

	public override string ToString()
	{
		return $"ParentId: {ParentId}, ContainerId: {ContainerId}";
	}
}
