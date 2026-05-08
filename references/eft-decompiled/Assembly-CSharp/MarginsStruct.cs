using UnityEngine;

public struct MarginsStruct(float left, float right, float top, float bottom)
{
	public float Left = left;

	public float Right = right;

	public float Top = top;

	public float Bottom = bottom;

	public MarginsStruct Scale(Vector2 factor)
	{
		return new MarginsStruct(Left * factor.x, Right * factor.x, Top * factor.y, Bottom * factor.y);
	}
}
