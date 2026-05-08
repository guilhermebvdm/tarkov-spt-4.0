public class GClass1220 : GInterface113
{
	public GInterface112 CreateSampleSet()
	{
		return new GClass1218();
	}

	public GInterface112 CreateSampleSet(GStruct109 result)
	{
		return new GClass1218(result);
	}

	public GInterface114 CreateSamplingTaskResult(int numCells)
	{
		return new GClass1219(numCells);
	}
}
