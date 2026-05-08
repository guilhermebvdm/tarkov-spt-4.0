using UnityEngine;

namespace BezierSplineTools;

public class SplineWalker : MonoBehaviour
{
	public BezierSpline spline;

	public float duration;

	public bool lookForward;

	public SplineWalkerMode mode;

	private float float_0;

	private bool bool_0 = true;

	public void Update()
	{
		if (bool_0)
		{
			float_0 += Time.deltaTime / duration;
			if (float_0 > 1f)
			{
				if (mode == SplineWalkerMode.Once)
				{
					float_0 = 1f;
				}
				else if (mode == SplineWalkerMode.Loop)
				{
					float_0 -= 1f;
				}
				else
				{
					float_0 = 2f - float_0;
					bool_0 = false;
				}
			}
		}
		else
		{
			float_0 -= Time.deltaTime / duration;
			if (float_0 < 0f)
			{
				float_0 = 0f - float_0;
				bool_0 = true;
			}
		}
		Vector3 point = spline.GetPoint(float_0);
		base.transform.localPosition = point;
		if (lookForward)
		{
			base.transform.LookAt(point + spline.GetDirection(float_0));
		}
	}
}
