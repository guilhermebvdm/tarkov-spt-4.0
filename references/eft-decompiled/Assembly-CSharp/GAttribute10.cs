using System;
using UnityEngine;

public class GAttribute10 : PropertyAttribute
{
	public Type propType;

	public GAttribute10(Type aType)
	{
		propType = aType;
	}
}
