namespace ombarella;

public static class Visualiser
{
	private static readonly string VIS_ZERO = "}           {";

	private static readonly string VIS_ONE = "} ]         {";

	private static readonly string VIS_TWO = "} ]]        {";

	private static readonly string VIS_THREE = "} ]]]       {";

	private static readonly string VIS_FOUR = "} ]]]]      {";

	private static readonly string VIS_FIVE = "} ]]]]]     {";

	private static readonly string VIS_SIX = "} ]]]]]]    {";

	private static readonly string VIS_SEVEN = "} ]]]]]]]   {";

	private static readonly string VIS_EIGHT = "} ]]]]]]]]  {";

	private static readonly string VIS_NINE = "} ]]]]]]]]] {";

	public static string GetLevelString(float averageLight, bool isDebug)
	{
		if (isDebug)
		{
			return string.Format($"debug value = {averageLight}");
		}
		if (float.IsNaN(averageLight))
		{
			string vIS_ZERO = VIS_ZERO;
		}
		if (averageLight < 0.1f)
		{
			return VIS_ZERO;
		}
		if (averageLight < 0.2f)
		{
			return VIS_ONE;
		}
		if (averageLight < 0.3f)
		{
			return VIS_TWO;
		}
		if (averageLight < 0.4f)
		{
			return VIS_THREE;
		}
		if (averageLight < 0.5f)
		{
			return VIS_FOUR;
		}
		if (averageLight < 0.6f)
		{
			return VIS_FIVE;
		}
		if (averageLight < 0.7f)
		{
			return VIS_SIX;
		}
		if (averageLight < 0.8f)
		{
			return VIS_SEVEN;
		}
		if (averageLight < 0.9f)
		{
			return VIS_EIGHT;
		}
		return VIS_NINE;
	}
}
