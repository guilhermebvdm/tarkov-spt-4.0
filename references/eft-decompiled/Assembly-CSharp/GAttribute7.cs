using UnityEngine;

public class GAttribute7 : PropertyAttribute
{
	public readonly string Data;

	public readonly bool WithLable;

	public readonly int Lines;

	public GAttribute7(string data, bool withLable = true)
	{
		Data = data;
		WithLable = withLable;
		Lines = data.Split('\n').Length;
	}
}
