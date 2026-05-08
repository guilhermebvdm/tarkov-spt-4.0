public class GameObjectStateSync : ISyncAble
{
	public override void Serialize(GInterface131 writerStream)
	{
		byte value = (byte)(base.gameObject.activeSelf ? 1 : 0);
		writerStream.Write(value);
	}

	public override void Deserialize(IDataReader readerStream)
	{
		byte b = readerStream.ReadByte();
		base.gameObject.SetActive(b == 1);
	}
}
