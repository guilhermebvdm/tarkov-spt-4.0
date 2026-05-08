public class GClass1206 : GInterface113
{
	public GInterface112 CreateSampleSet()
	{
		return new GClass1205();
	}

	public GInterface112 CreateSampleSet(GStruct109 result)
	{
		return new GClass1205(result);
	}

	public GInterface114 CreateSamplingTaskResult(int numCells)
	{
		return new GClass1207(numCells);
	}
}
