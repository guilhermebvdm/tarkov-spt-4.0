using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct GStruct33 : IJobParallelFor
{
	public class GClass741
	{
		public int Count;

		public GStruct30[] Commands = Array.Empty<GStruct30>();

		public GClass739[] Triggers = Array.Empty<GClass739>();

		public int[] Result = Array.Empty<int>();
	}

	public static readonly GClass741[] JobParams = new GClass741[64];

	[ReadOnly]
	public int Index;

	public int GetExecutionLength()
	{
		return JobParams[Index].Count;
	}

	public void Execute(int executeIndex)
	{
		GClass741 pars = JobParams[Index];
		method_0(pars, executeIndex);
	}

	public void method_0(GClass741 pars, int index)
	{
		GStruct30 gStruct = pars.Commands[index];
		GStruct32 a = new GStruct32(gStruct.Center, gStruct.Size, gStruct.Orientation);
		int num = 0;
		for (int i = 0; i < pars.Triggers.Length; i++)
		{
			GClass739 gClass = pars.Triggers[i];
			if ((!gClass.IsTrigger || gStruct.QueryTriggerInteraction != QueryTriggerInteraction.Ignore) && (!gClass.IsTrigger || gStruct.QueryTriggerInteraction != QueryTriggerInteraction.UseGlobal || Physics.queriesHitTriggers) && gStruct.Mask == (gStruct.Mask | (1 << gClass.Layer)) && !((gClass.Obb.Center - a.Center).magnitude > (a.SizeDistance + gClass.Obb.SizeDistance) * 2f) && GClass753.Intersects(a, gClass.Obb))
			{
				GClass832.Set2D(pars.Result, pars.Count, index, num++, gClass.Id);
			}
		}
	}
}
