using UnityEngine;

public class GClass365
{
	public Vector3 a;

	public Vector3 b;

	public GClass365(Vector3 a, Vector3 b)
	{
		this.a = a;
		this.b = b;
	}

	public void DebugDrawSegment(Color color, bool onSameY = true)
	{
		Vector3 start = a;
		Vector3 end = b;
		if (onSameY)
		{
			start.y = end.y;
		}
		Debug.DrawLine(start, end, color, 0.2f);
	}

	public Vector3 Middle()
	{
		return (a + b) * 0.5f;
	}
}
