using BitPacking;

public struct GStruct137(int min, int max, BitPackingTag tag)
{
	public readonly int Min = min;

	public readonly int Max = max;

	public readonly BitPackingTag Tag = tag;
}
