public class GClass799 : GClass798<double>
{
	public GClass799(int count)
		: base(count)
	{
	}

	public override double Difference(double minuend, double subtrahend)
	{
		return minuend - subtrahend;
	}

	public override double Summation(double summandL, double summandR)
	{
		return summandL + summandR;
	}

	public override double Division(double dividend, int divisor)
	{
		return dividend / (double)divisor;
	}

	public override bool IsGreaterOrEqual(double rhs, double lhs)
	{
		return rhs >= lhs;
	}

	public override bool IsLessOrEqual(double rhs, double lhs)
	{
		return rhs <= lhs;
	}
}
