using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FracturePointsPlacer : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	public class Class661
	{
		public static readonly Class661 class661_0 = new Class661();

		public static Comparison<Vector3> comparison_0;

		public int method_0(Vector3 a, Vector3 b)
		{
			return a.y.CompareTo(b.y);
		}
	}

	public static void PlacePoints(Transform hitTransform, Vector3 hitPoint, Vector3[] verts)
	{
		Vector3 vector = hitTransform.InverseTransformPoint(hitPoint);
		float num = 0.5f;
		float num2 = 0.02f;
		verts[0] = vector;
		verts[1] = new Vector3(-0.5f, -0.5f);
		verts[2] = new Vector3(-0.5f, num);
		verts[3] = new Vector3(num, -0.5f);
		verts[4] = new Vector3(num, num);
		for (int i = 5; i < verts.Length; i++)
		{
			Vector3 vector2 = UnityEngine.Random.insideUnitCircle;
			vector2 *= num2;
			vector2.x += UnityEngine.Random.value * num2 * 0.001f;
			vector2.y += UnityEngine.Random.value * num2 * 0.001f;
			vector2 += vector;
			if (Mathf.Abs(vector2.x) > num)
			{
				vector2.x %= num;
			}
			if (Mathf.Abs(vector2.y) > num)
			{
				vector2.y %= num;
			}
			if (Mathf.Abs(vector2.z) > num)
			{
				vector2.z %= num;
			}
			if (i % UnityEngine.Random.Range(3, 5) == 0)
			{
				num2 *= 2f;
			}
			verts[i] = vector2;
		}
		Array.Sort(verts, (Vector3 a, Vector3 b) => a.y.CompareTo(b.y));
		for (int num3 = 0; num3 < verts.Length; num3++)
		{
			verts[num3] = hitTransform.TransformPoint(verts[num3]);
		}
	}
}
