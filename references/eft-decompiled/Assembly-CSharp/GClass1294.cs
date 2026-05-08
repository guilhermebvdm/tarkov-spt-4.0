using System.Runtime.CompilerServices;

public abstract class GClass1294
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Elapsed(double time, double interval, ref double lastTime)
	{
		if (time >= lastTime + interval)
		{
			long num = (long)(time / interval);
			lastTime = (double)num * interval;
			return true;
		}
		return false;
	}
}
