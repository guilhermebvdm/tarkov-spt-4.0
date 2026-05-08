using System.Collections.Generic;
using UnityEngine;

public class ConvexHull : MonoBehaviour
{
	private static List<GClass982> list_0;

	private static List<GClass982> list_1;

	private static List<GClass982> list_2;

	public static int[] ComputeIncremental(Vector3[] points)
	{
		if (points.Length < 3)
		{
			return null;
		}
		GClass982.Points = points;
		GClass982 gClass = new GClass982(0, 1, 2);
		Vector3 p = smethod_0(points, 3, gClass);
		if (gClass.IsVisible(p))
		{
			gClass.Flip();
		}
		GClass982 gClass2 = new GClass982(3, gClass.I0, gClass.I1);
		if (gClass2.IsVisible(p))
		{
			gClass2.Flip();
		}
		GClass982 gClass3 = new GClass982(3, gClass.I1, gClass.I2);
		if (gClass3.IsVisible(p))
		{
			gClass3.Flip();
		}
		GClass982 gClass4 = new GClass982(3, gClass.I2, gClass.I0);
		if (gClass4.IsVisible(p))
		{
			gClass4.Flip();
		}
		list_0 = new List<GClass982> { gClass, gClass2, gClass3, gClass4 };
		list_1 = new List<GClass982>(points.Length);
		list_2 = new List<GClass982>(points.Length);
		int num = 4;
		while (true)
		{
			if (num < points.Length)
			{
				Vector3 p2 = points[num];
				list_1.Clear();
				for (int i = 0; i < list_0.Count; i++)
				{
					if (list_0[i].IsVisible(p2))
					{
						list_1.Add(list_0[i]);
					}
				}
				if (list_1.Count != 0)
				{
					for (int j = 0; j < list_1.Count; j++)
					{
						list_0.Remove(list_1[j]);
					}
					if (list_1.Count == 1)
					{
						GClass982 gClass5 = list_1[0];
						list_0.Add(new GClass982(num, gClass5.I0, gClass5.I1));
						list_0.Add(new GClass982(num, gClass5.I1, gClass5.I2));
						list_0.Add(new GClass982(num, gClass5.I2, gClass5.I0));
					}
					else
					{
						if (list_1.Count > 2000)
						{
							Debug.LogWarning("Visible faces is too big, " + list_1.Count + " cancelling operation.");
							return new int[0];
						}
						list_2.Clear();
						for (int k = 0; k < list_1.Count; k++)
						{
							list_2.Add(new GClass982(num, list_1[k].I0, list_1[k].I1));
							list_2.Add(new GClass982(num, list_1[k].I1, list_1[k].I2));
							list_2.Add(new GClass982(num, list_1[k].I2, list_1[k].I0));
						}
						if (list_2.Count > 8000)
						{
							break;
						}
						for (int l = 0; l < list_2.Count; l++)
						{
							GClass982 gClass6 = list_2[l];
							for (int m = 0; m < list_2.Count; m++)
							{
								if (gClass6 != list_2[m] && gClass6.IsVisible(list_2[m].Centroid))
								{
									gClass6 = null;
									break;
								}
							}
							if (gClass6 != null)
							{
								list_0.Add(gClass6);
							}
						}
					}
				}
				num++;
				continue;
			}
			int[] array = new int[list_0.Count * 3];
			int num2 = 0;
			for (int n = 0; n < list_0.Count; n++)
			{
				int[] array2 = array;
				int num3 = num2;
				int num4 = num3 + 1;
				int i2 = list_0[n].I0;
				array2[num3] = i2;
				int[] array3 = array;
				int num5 = num4;
				int num6 = num5 + 1;
				int i3 = list_0[n].I1;
				array3[num5] = i3;
				int[] array4 = array;
				int num7 = num6;
				num2 = num7 + 1;
				int i4 = list_0[n].I2;
				array4[num7] = i4;
			}
			return array;
		}
		Debug.LogWarning("Temp faces is too big, " + list_2.Count + " cancelling operation.");
		return new int[0];
	}

	public static Vector3 smethod_0(Vector3[] points, int index, GClass982 face)
	{
		return (points[index] + points[face.I0] + points[face.I1] + points[face.I2]) / 4f;
	}
}
