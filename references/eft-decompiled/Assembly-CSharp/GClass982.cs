using UnityEngine;

public class GClass982
{
	public static Vector3[] Points;

	public int I0;

	public int I1;

	public int I2;

	public float A;

	public float B;

	public float C;

	public float D;

	public Vector3 Centroid
	{
		get
		{
			Vector3 vector = Points[I0];
			Vector3 vector2 = Points[I1];
			Vector3 vector3 = Points[I2];
			return new Vector3(vector.x + vector2.x + vector3.x, vector.y + vector2.y + vector3.y, vector.z + vector2.z + vector3.z) / 3f;
		}
	}

	public GClass982(int i0, int i1, int i2)
	{
		I0 = i0;
		I1 = i1;
		I2 = i2;
		method_0();
	}

	public void method_0()
	{
		Vector3 vector = Points[I0];
		Vector3 vector2 = Points[I1];
		Vector3 vector3 = Points[I2];
		A = vector.y * (vector2.z - vector3.z) + vector2.y * (vector3.z - vector.z) + vector3.y * (vector.z - vector2.z);
		B = vector.z * (vector2.x - vector3.x) + vector2.z * (vector3.x - vector.x) + vector3.z * (vector.x - vector2.x);
		C = vector.x * (vector2.y - vector3.y) + vector2.x * (vector3.y - vector.y) + vector3.x * (vector.y - vector2.y);
		D = 0f - (vector.x * (vector2.y * vector3.z - vector3.y * vector2.z) + vector2.x * (vector3.y * vector.z - vector.y * vector3.z) + vector3.x * (vector.y * vector2.z - vector2.y * vector.z));
	}

	public bool IsVisible(Vector3 p)
	{
		return (double)A * (double)p.x + (double)B * (double)p.y + (double)C * (double)p.z + (double)D >= 0.0;
	}

	public void Flip()
	{
		int i = I0;
		I0 = I1;
		I1 = i;
		method_0();
	}
}
