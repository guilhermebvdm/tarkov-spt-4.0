public class GClass801 : GClass797<double>
{
	public GClass801(int count)
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
}
