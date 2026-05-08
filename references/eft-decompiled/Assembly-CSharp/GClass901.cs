using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass901
{
	[NonSerialized]
	[CompilerGenerated]
	public Vector3 Vector3_0;

	[NonSerialized]
	[CompilerGenerated]
	public Vector3 Vector3_1;

	[NonSerialized]
	[CompilerGenerated]
	public Color Color_0;

	public Vector3 StartPoint
	{
		[CompilerGenerated]
		get
		{
			return Vector3_0;
		}
	}

	public Vector3 EndPoint
	{
		[CompilerGenerated]
		get
		{
			return Vector3_1;
		}
	}

	public Color Color
	{
		[CompilerGenerated]
		get
		{
			return Color_0;
		}
		[CompilerGenerated]
		set
		{
			Color_0 = value;
		}
	}

	public GClass901(float effect, Vector3 startPoint, Vector3 endPoint)
	{
		Vector3_0 = startPoint;
		Vector3_1 = endPoint;
		Color = smethod_0(effect);
	}

	public static Color smethod_0(float effect)
	{
		if (effect <= 0f)
		{
			return Color.green;
		}
		if (effect > 0f && effect < 1f)
		{
			return Color.yellow;
		}
		return Color.red;
	}
}
