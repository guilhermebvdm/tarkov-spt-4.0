public class GClass1252
{
	public ushort numIndices;

	public byte[] compressedIndexArray;

	public long Size => 2 + ((numIndices > 0) ? compressedIndexArray.Length : 0);
}
