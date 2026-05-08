using UnityEngine;

public class GClass803 : GClass797<Vector3>
{
	public GClass803(int count)
		: base(count)
	{
	}

	public override Vector3 Difference(Vector3 minuend, Vector3 subtrahend)
	{
		return minuend - subtrahend;
	}

	public override Vector3 Summation(Vector3 summandL, Vector3 summandR)
	{
		return summandL + summandR;
	}

	public override Vector3 Division(Vector3 dividend, int divisor)
	{
		return dividend / divisor;
	}
}
