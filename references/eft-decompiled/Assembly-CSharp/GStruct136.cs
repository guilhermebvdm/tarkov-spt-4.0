using BitPacking;

public struct GStruct136(float min, float max, float resolution, BitPackingTag tag)
{
	public readonly float Min = min;

	public readonly float Max = max;

	public readonly float Resolution = resolution;

	public readonly BitPackingTag Tag = tag;
}
