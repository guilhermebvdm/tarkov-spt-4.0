using UnityEngine;

public class GAttribute1 : PropertyAttribute
{
	public float max;

	public GAttribute1(float max)
	{
		this.max = max;
	}
}
