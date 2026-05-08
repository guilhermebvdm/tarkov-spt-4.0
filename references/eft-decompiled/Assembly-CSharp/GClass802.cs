public class GClass802 : GClass797<float>
{
	public GClass802(int count)
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
}
