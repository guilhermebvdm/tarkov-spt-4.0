using System;
using System.Collections;
using System.Collections.Generic;
using Koenigz.PerfectCulling;

public abstract class GClass1216
{
	[NonSerialized]
	public static IEnumerator Ienumerator_0 = null;

	[NonSerialized]
	public static PerfectCullingBakingBehaviour PerfectCullingBakingBehaviour_0 = null;

	[NonSerialized]
	public static Queue<GClass1204> Queue_0 = new Queue<GClass1204>();

	public static bool IsBaking => Ienumerator_0 != null;

	public static void ScheduleBake(GClass1204 bakeInformation)
	{
		Queue_0.Enqueue(bakeInformation);
	}

	public static void BakeAllScheduled()
	{
	}

	public static void smethod_0()
	{
	}
}
