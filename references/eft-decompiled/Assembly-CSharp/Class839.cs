using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class Class839
{
	[NonSerialized]
	public const float Float_0 = 1f / 6f;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float CalculateTetrahedronVolume(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
	{
		return Mathf.Abs(Vector3.Dot(a - d, Vector3.Cross(b - d, c - d))) * (1f / 6f);
	}
}
