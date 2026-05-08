using UnityEngine;

public class GAttribute2 : PropertyAttribute
{
	public float min;

	public float max;

	public GAttribute2(float min, float max)
	{
		this.min = min;
		this.max = max;
	}
}
