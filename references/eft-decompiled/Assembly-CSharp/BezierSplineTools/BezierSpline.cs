using System;
using System.Collections.Generic;
using System.Linq;
using EFT.Vehicle;
using UnityEngine;

namespace BezierSplineTools;

public class BezierSpline : MonoBehaviour
{
	public delegate void GDelegate41(PathSpline spline);

	public delegate void GDelegate42(PathSpline spline);

	public GDelegate41 SplineSelectedEvent;

	public GDelegate42 SplineDeselectedEvent;

	[SerializeField]
	private Vector3[] points;

	[SerializeField]
	private BezierControlPointMode[] modes;

	[SerializeField]
	private bool loop;

	private List<float> _curvesLengthCache = new List<float>();

	private float _totalCurvesLength;

	public Vector3[] Points => points;

	public bool Loop
	{
		get
		{
			return loop;
		}
		set
		{
			loop = value;
			if (value)
			{
				modes[modes.Length - 1] = modes[0];
				SetControlPoint(0, points[0]);
			}
		}
	}

	public int ControlPointCount => points.Length;

	public int CurveCount => (points.Length - 1) / 3;

	public Vector3 GetControlPoint(int index)
	{
		return points[index];
	}

	public float ClosestTimeOnBezier(Vector3 aP, float min, float step, float max)
	{
		float num = Vector3.Distance(GetPoint(min), aP);
		float result = min;
		for (float num2 = min + step; num2 <= max; num2 += step)
		{
			Vector3 point = GetPoint(num2);
			float num3 = Vector3.Distance(aP, point);
			if (num3 < num)
			{
				num = num3;
				result = num2;
			}
		}
		return result;
	}

	public void SetControlPoint(int index, Vector3 point)
	{
		if (index % 3 == 0)
		{
			Vector3 vector = point - points[index];
			if (loop)
			{
				if (index == 0)
				{
					points[1] += vector;
					points[points.Length - 2] += vector;
					points[points.Length - 1] = point;
				}
				else if (index == points.Length - 1)
				{
					points[0] = point;
					points[1] += vector;
					points[index - 1] += vector;
				}
				else
				{
					points[index - 1] += vector;
					points[index + 1] += vector;
				}
			}
			else
			{
				if (index > 0)
				{
					points[index - 1] += vector;
				}
				if (index + 1 < points.Length)
				{
					points[index + 1] += vector;
				}
			}
		}
		points[index] = point;
		method_0(index);
	}

	public BezierControlPointMode GetControlPointMode(int index)
	{
		return modes[(index + 1) / 3];
	}

	public void SetControlPointMode(int index, BezierControlPointMode mode)
	{
		int num = (index + 1) / 3;
		modes[num] = mode;
		if (loop)
		{
			if (num == 0)
			{
				modes[modes.Length - 1] = mode;
			}
			else if (num == modes.Length - 1)
			{
				modes[0] = mode;
			}
		}
		method_0(index);
	}

	public void method_0(int index)
	{
		int num = (index + 1) / 3;
		BezierControlPointMode bezierControlPointMode = modes[num];
		if (bezierControlPointMode == BezierControlPointMode.Free || (!loop && (num == 0 || num == modes.Length - 1)))
		{
			return;
		}
		int num2 = num * 3;
		int num3;
		int num4;
		if (index <= num2)
		{
			num3 = num2 - 1;
			if (num3 < 0)
			{
				num3 = points.Length - 2;
			}
			num4 = num2 + 1;
			if (num4 >= points.Length)
			{
				num4 = 1;
			}
		}
		else
		{
			num3 = num2 + 1;
			if (num3 >= points.Length)
			{
				num3 = 1;
			}
			num4 = num2 - 1;
			if (num4 < 0)
			{
				num4 = points.Length - 2;
			}
		}
		Vector3 vector = points[num2];
		Vector3 vector2 = vector - points[num3];
		if (bezierControlPointMode == BezierControlPointMode.Aligned)
		{
			vector2 = vector2.normalized * Vector3.Distance(vector, points[num4]);
		}
		points[num4] = vector + vector2;
	}

	public Vector3 GetPoint(float t)
	{
		int num;
		if (t >= 1f)
		{
			t = 1f;
			num = points.Length - 4;
		}
		else
		{
			t = Mathf.Clamp01(t) * (float)CurveCount;
			num = (int)t;
			t -= (float)num;
			num *= 3;
		}
		return base.transform.TransformPoint(GClass1280.GetPoint(points[num], points[num + 1], points[num + 2], points[num + 3], t));
	}

	public Vector3 GetPointByOneSpeed(float t)
	{
		int num;
		if (t >= 1f)
		{
			t = 1f;
			num = points.Length - 4;
		}
		else
		{
			t = Mathf.Clamp01(t) * (float)CurveCount;
			num = (int)t;
			t -= (float)num;
			num *= 3;
		}
		return base.transform.TransformPoint(GClass1280.GetPoint(points[num], points[num + 1], points[num + 2], points[num + 3], t));
	}

	public Vector3 GetVelocity(float t)
	{
		int num;
		if (t >= 1f)
		{
			t = 1f;
			num = points.Length - 4;
		}
		else
		{
			t = Mathf.Clamp01(t) * (float)CurveCount;
			num = (int)t;
			t -= (float)num;
			num *= 3;
		}
		return base.transform.TransformPoint(GClass1280.GetFirstDerivative(points[num], points[num + 1], points[num + 2], points[num + 3], t)) - base.transform.position;
	}

	public Tuple<Vector3, Vector3> GetPointAndDirection(float t)
	{
		int num;
		if (t >= 1f)
		{
			t = 1f;
			num = points.Length - 4;
		}
		else
		{
			t = Mathf.Clamp01(t) * (float)CurveCount;
			num = (int)t;
			t -= (float)num;
			num *= 3;
		}
		return new Tuple<Vector3, Vector3>(base.transform.TransformPoint(GClass1280.GetPoint(points[num], points[num + 1], points[num + 2], points[num + 3], t)), base.transform.TransformPoint(GClass1280.GetFirstDerivative(points[num], points[num + 1], points[num + 2], points[num + 3], t)) - base.transform.position);
	}

	public Vector3 GetDirection(float t)
	{
		return GetVelocity(t).normalized;
	}

	public void AddCurve()
	{
		Vector3 vector = points[points.Length - 1];
		Array.Resize(ref points, points.Length + 3);
		vector.x += 1f;
		points[points.Length - 3] = vector;
		vector.x += 1f;
		points[points.Length - 2] = vector;
		vector.x += 1f;
		points[points.Length - 1] = vector;
		Array.Resize(ref modes, modes.Length + 1);
		modes[modes.Length - 1] = modes[modes.Length - 2];
		method_0(points.Length - 4);
		if (loop)
		{
			points[points.Length - 1] = points[0];
			modes[modes.Length - 1] = modes[0];
			method_0(0);
		}
	}

	public void AddCurveNearPoint(int pointIndex)
	{
		List<Vector3> list = points.ToList();
		List<Vector3> list2 = new List<Vector3>();
		list2.AddRange(list.GetRange(0, pointIndex + 1));
		Vector3 item = points[pointIndex];
		item.x -= 1f;
		list2.Add(item);
		item.x -= 1f;
		list2.Add(item);
		item.x -= 1f;
		list2.Add(item);
		list2.AddRange(list.GetRange(pointIndex + 1, list.Count - (pointIndex + 1)));
		points = list2.ToArray();
		Array.Resize(ref modes, modes.Length + 1);
		modes[modes.Length - 1] = modes[modes.Length - 2];
		method_0(points.Length - 4);
		if (loop)
		{
			points[points.Length - 1] = points[0];
			modes[modes.Length - 1] = modes[0];
			method_0(0);
		}
	}

	public void AddCurveToStartLine()
	{
		Vector3 item = points[0];
		List<Vector3> list = new List<Vector3>();
		item.x -= 1f;
		list.Add(item);
		item.x -= 1f;
		list.Add(item);
		item.x -= 1f;
		list.Add(item);
		list.AddRange(points.ToList());
		points = list.ToArray();
		Array.Resize(ref modes, modes.Length + 1);
		modes[modes.Length - 1] = modes[modes.Length - 2];
		method_0(points.Length - 4);
		if (loop)
		{
			points[points.Length - 1] = points[0];
			modes[modes.Length - 1] = modes[0];
			method_0(0);
		}
	}

	public void AddCurve(Vector3 position, bool toEnd)
	{
		Vector3 vector = points[points.Length - 1];
		Array.Resize(ref points, points.Length + 3);
		vector.x += 1f;
		points[points.Length - 3] = vector;
		vector.x += 1f;
		points[points.Length - 2] = vector;
		vector.x += 1f;
		points[points.Length - 1] = vector;
		Array.Resize(ref modes, modes.Length + 1);
		modes[modes.Length - 1] = modes[modes.Length - 2];
		method_0(points.Length - 4);
		if (loop)
		{
			points[points.Length - 1] = points[0];
			modes[modes.Length - 1] = modes[0];
			method_0(0);
		}
	}

	public void Reset()
	{
		points = new Vector3[4]
		{
			new Vector3(1f, 0f, 0f),
			new Vector3(2f, 0f, 0f),
			new Vector3(3f, 0f, 0f),
			new Vector3(4f, 0f, 0f)
		};
		modes = new BezierControlPointMode[2];
	}

	public void CreateCurvesLengthCache(float sensitivity)
	{
		_totalCurvesLength = 0f;
		_curvesLengthCache.Clear();
		float deltaTime = 0.1f;
		for (int i = 0; i < CurveCount; i++)
		{
			float num = method_1(i, sensitivity, ref deltaTime);
			_curvesLengthCache.Add(num);
			_totalCurvesLength += num;
		}
	}

	public float method_1(int index, float sensitivity, ref float deltaTime)
	{
		float num = 0f;
		float num2 = 0f;
		Vector3 a = method_2(index, num2);
		do
		{
			num2 += deltaTime;
			if (num2 > 1f)
			{
				num2 = 1f;
			}
			Vector3 vector = method_2(index, num2);
			float num3 = Vector3.Distance(a, vector);
			if (num3 > sensitivity)
			{
				num2 -= deltaTime;
				deltaTime *= sensitivity / num3;
			}
			else
			{
				num += num3;
				a = vector;
			}
		}
		while (num2 < 1f);
		return num;
	}

	public float GetLengthApproximately(float sensitivity)
	{
		float num = 0f;
		float deltaTime = 0.1f;
		for (int i = 0; i < CurveCount; i++)
		{
			num += method_1(i, sensitivity, ref deltaTime);
		}
		return num;
	}

	public Vector3 method_2(int index, float t)
	{
		index *= 3;
		return base.transform.TransformPoint(GClass1280.GetPoint(points[index], points[index + 1], points[index + 2], points[index + 3], t));
	}

	public float GetLengthFromChache()
	{
		return _totalCurvesLength;
	}

	public void method_3(float t, out int index, out float curveTime)
	{
		t = Mathf.Clamp01(t);
		float num = _totalCurvesLength * t;
		float num2 = 0f;
		curveTime = 0f;
		index = CurveCount - 1;
		int num3 = 0;
		float num4;
		while (true)
		{
			if (num3 < CurveCount)
			{
				num4 = _curvesLengthCache[num3];
				num2 += num4;
				if (!(num2 < num))
				{
					break;
				}
				num3++;
				continue;
			}
			return;
		}
		index = num3;
		curveTime = ((num4 > 0f) ? (1f - (num2 - num) / num4) : 0f);
	}

	public Vector3 GetPointForBackwardsHomogeneousMovement(float currentDistance, float targetDistance, float precision, ref float currentTime)
	{
		float num = 0.02f;
		Vector3 b = GetPoint(currentTime);
		int num2 = 0;
		while (true)
		{
			if (++num2 <= 100)
			{
				currentTime -= num;
				Vector3 point = GetPoint(currentTime);
				float num3 = Vector3.Distance(point, b);
				currentDistance -= num3;
				float num4 = targetDistance - currentDistance;
				if (num4 < 0f - precision)
				{
					b = point;
				}
				else
				{
					if (!(num4 > precision))
					{
						break;
					}
					currentTime += num;
					num *= 1f - num4 / num3;
					currentDistance += num3;
				}
				if (currentDistance > targetDistance && !(currentTime <= 0f))
				{
					continue;
				}
			}
			else
			{
				Debug.Log($"Infinitive loop {currentDistance} => {targetDistance}, precision: {precision}, time: {currentTime}");
			}
			return GetPoint(currentTime);
		}
		return GetPoint(currentTime);
	}

	public Vector3 GetPointForHomogeneousMovement(float currentDistance, float targetDistance, float precision, ref float currentTime)
	{
		float num = targetDistance - currentDistance;
		precision = Mathf.Clamp(Mathf.Abs(num) / 5f, precision, 0.5f);
		currentTime = Mathf.Clamp01(currentTime);
		if (num < 0f)
		{
			return GetPointForBackwardsHomogeneousMovement(currentDistance, targetDistance, precision, ref currentTime);
		}
		float num2 = 0.02f;
		Vector3 b = GetPoint(currentTime);
		float num3 = currentDistance;
		int num4 = 0;
		while (true)
		{
			if (++num4 <= 100)
			{
				currentTime += num2;
				Vector3 point = GetPoint(currentTime);
				float num5 = Vector3.Distance(point, b);
				num3 += num5;
				float num6 = num3 - targetDistance;
				if (num6 < 0f - precision)
				{
					b = point;
				}
				else
				{
					if (!(num6 > precision))
					{
						break;
					}
					currentTime -= num2;
					num2 *= 1f - num6 / num5;
					num3 -= num5;
				}
				if (num3 < targetDistance && !(currentTime >= 1f))
				{
					continue;
				}
			}
			else
			{
				Debug.Log($"Infinitive loop {currentDistance} => {targetDistance}, precision: {precision}, time: {currentTime}");
			}
			return GetPoint(currentTime);
		}
		return GetPoint(currentTime);
	}

	public Vector3 GetPointWithCurvesLengthCache(float t)
	{
		method_3(t, out var index, out var curveTime);
		index *= 3;
		return base.transform.TransformPoint(GClass1280.GetPoint(points[index], points[index + 1], points[index + 2], points[index + 3], curveTime));
	}

	public Vector3 GetVelocityWithCurvesLengthCache(float t)
	{
		method_3(t, out var index, out var curveTime);
		index *= 3;
		return base.transform.TransformPoint(GClass1280.GetFirstDerivative(points[index], points[index + 1], points[index + 2], points[index + 3], curveTime)) - base.transform.position;
	}

	public Vector3 GetDirectionWithCurvesLengthCache(float t)
	{
		return GetVelocityWithCurvesLengthCache(t).normalized;
	}

	public void DeletePoint(int index)
	{
		List<Vector3> list = points.ToList();
		list.RemoveAt(index);
		points = list.ToArray();
	}
}
