using System;
using UnityEngine;

public class GClass576
{
	public float Cos;

	public float Sin;

	public float Sin2;

	public float Tan;

	public float Ang;

	public GClass576(float ang)
	{
		float num = ang * (MathF.PI / 180f);
		Cos = Mathf.Cos(num);
		Sin = Mathf.Sin(num);
		Sin2 = Mathf.Sin(2f * num);
		Tan = Mathf.Tan(num);
		Ang = ang;
	}
}
