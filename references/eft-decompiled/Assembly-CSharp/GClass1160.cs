using System;
using UnityEngine;

public class GClass1160
{
	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public Vector3 Vector3_1;

	[NonSerialized]
	public float Float_0 = 0.3f;

	public void SetThreshold(float threshold)
	{
		Float_0 = threshold;
	}

	public bool IsPositionChanged(Vector3 sourcePos, Vector3 listenerPos)
	{
		int num;
		if (GClass940.IsPositionIdentical(Vector3_0, listenerPos, Float_0))
		{
			num = ((!GClass940.IsPositionIdentical(Vector3_1, sourcePos, Float_0)) ? 1 : 0);
			if (num == 0)
			{
				goto IL_003d;
			}
		}
		else
		{
			num = 1;
		}
		Vector3_0 = listenerPos;
		Vector3_1 = sourcePos;
		goto IL_003d;
		IL_003d:
		return (byte)num != 0;
	}
}
