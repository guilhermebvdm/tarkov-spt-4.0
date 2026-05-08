using Microsoft.Extensions.ObjectPool;

public class GClass2071 : PooledObjectPolicy<GStruct210[]>
{
	public const int CLIENT_INPUT_PACKETS_QUEUE_LENGTH = 5000;

	public override GStruct210[] Create()
	{
		return new GStruct210[5000];
	}

	public override bool Return(GStruct210[] obj)
	{
		return true;
	}
}
