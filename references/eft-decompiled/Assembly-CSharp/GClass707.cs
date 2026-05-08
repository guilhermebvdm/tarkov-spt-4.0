public abstract class GClass707
{
	public static XYCellSizeStruct Rotate(this ItemRotation rotation, XYCellSizeStruct size)
	{
		if (rotation != ItemRotation.Horizontal)
		{
			return new XYCellSizeStruct(size.Y, size.X);
		}
		return size;
	}
}
