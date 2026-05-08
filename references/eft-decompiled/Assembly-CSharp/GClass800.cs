public class GClass800 : GClass798<float>
{
	public GClass800(int count)
		: base(count)
	{
	}

	public override float Difference(float minuend, float subtrahend)
	{
		return minuend - subtrahend;
	}

	public override float Summation(float summandL, float summandR)
	{
		return summandL + summandR;
	}

	public override float Division(float dividend, int divisor)
	{
		return dividend / (float)divisor;
	}

	public override bool IsGreaterOrEqual(float rhs, float lhs)
	{
		return rhs >= lhs;
	}

	public override bool IsLessOrEqual(float rhs, float lhs)
	{
		return rhs <= lhs;
	}
}
