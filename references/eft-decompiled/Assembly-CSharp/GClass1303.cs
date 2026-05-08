public class GClass1303<T> : GClass1302<T, T>
{
	public GClass1303(GInterface120<T> interpolationStrategy, int queueSize)
		: base(interpolationStrategy, queueSize)
	{
	}

	public override T GetValueShortData(int index)
	{
		return GetTimeStampedValue(index).Value;
	}
}
