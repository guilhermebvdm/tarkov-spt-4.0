using UnityEngine;

public class GAttribute15 : PropertyAttribute
{
	public readonly float max;

	public readonly float min;

	public GAttribute15(float min, float max)
	{
		this.min = min;
		this.max = max;
	}
}
