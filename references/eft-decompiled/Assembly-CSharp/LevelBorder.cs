using UnityEngine;

public class LevelBorder : MonoBehaviour
{
	[Space]
	public Vector3[] Points;

	public GClass916 CreateConcaveBorder()
	{
		if (Points != null && Points.Length >= 2)
		{
			Vector3 position = base.transform.position;
			Vector2[] array = new Vector2[Points.Length + 1];
			array[0] = new Vector2(position.x, position.z);
			for (int i = 0; i < Points.Length; i++)
			{
				Vector3 vector = Points[i];
				array[i + 1] = new Vector2(vector.x, vector.z);
			}
			return GClass916.smethod_0(array);
		}
		return null;
	}

	public int PointIndex(Vector3 position, float precision = 0f)
	{
		if (Points != null && Points.Length != 0)
		{
			int result = -1;
			float num = float.MaxValue;
			for (int i = 0; i < Points.Length; i++)
			{
				float num2 = Vector3.SqrMagnitude(Points[i] - position);
				if (num2 < num)
				{
					num = num2;
					result = i;
				}
			}
			if (num > precision)
			{
				result = -1;
			}
			return result;
		}
		return -1;
	}
}
