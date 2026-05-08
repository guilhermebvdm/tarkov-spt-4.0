using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[HelpURL("http://www.root-motion.com/finalikdox/html/page12.html")]
[AddComponentMenu("Scripts/RootMotion.FinalIK/Rotation Limits/Rotation Limit Polygonal")]
public class RotationLimitPolygonal : RotationLimit
{
	[Serializable]
	public class ReachCone
	{
		public Vector3[] tetrahedron;

		public float volume;

		public Vector3 S;

		public Vector3 B;

		public Vector3 o => tetrahedron[0];

		public Vector3 a => tetrahedron[1];

		public Vector3 b => tetrahedron[2];

		public Vector3 c => tetrahedron[3];

		public bool isValid => volume > 0f;

		public ReachCone(Vector3 _o, Vector3 _a, Vector3 _b, Vector3 _c)
		{
			tetrahedron = new Vector3[4];
			tetrahedron[0] = _o;
			tetrahedron[1] = _a;
			tetrahedron[2] = _b;
			tetrahedron[3] = _c;
			volume = 0f;
			S = Vector3.zero;
			B = Vector3.zero;
		}

		public void Calculate()
		{
			Vector3 lhs = Vector3.Cross(a, b);
			volume = Vector3.Dot(lhs, c) / 6f;
			S = Vector3.Cross(a, b).normalized;
			B = Vector3.Cross(b, c).normalized;
		}
	}

	[Serializable]
	public class LimitPoint
	{
		public Vector3 point;

		public float tangentWeight;

		public LimitPoint()
		{
			point = Vector3.forward;
			tangentWeight = 1f;
		}
	}

	[Range(0f, 180f)]
	public float twistLimit = 180f;

	[Range(0f, 3f)]
	public int smoothIterations;

	[SerializeField]
	[HideInInspector]
	public LimitPoint[] points;

	[SerializeField]
	[HideInInspector]
	public Vector3[] P;

	[SerializeField]
	[HideInInspector]
	public ReachCone[] reachCones = new ReachCone[0];

	[ContextMenu("User Manual")]
	public void method_0()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/page12.html");
	}

	[ContextMenu("Scrpt Reference")]
	public void method_1()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_rotation_limit_polygonal.html");
	}

	[ContextMenu("Support Group")]
	public void method_2()
	{
		Application.OpenURL("https://groups.google.com/forum/#!forum/final-ik");
	}

	[ContextMenu("Asset Store Thread")]
	public void method_3()
	{
		Application.OpenURL("http://forum.unity3d.com/threads/final-ik-full-body-ik-aim-look-at-fabrik-ccd-ik-1-0-released.222685/");
	}

	public void SetLimitPoints(LimitPoint[] points)
	{
		if (points.Length < 3)
		{
			LogWarning("The polygon must have at least 3 Limit Points.");
			return;
		}
		this.points = points;
		BuildReachCones();
	}

	public override Quaternion LimitRotation(Quaternion rotation)
	{
		if (reachCones.Length == 0)
		{
			Start();
		}
		return RotationLimit.LimitTwist(method_8(rotation), axis, base.secondaryAxis, twistLimit);
	}

	public void Start()
	{
		if (points.Length < 3)
		{
			ResetToDefault();
		}
		for (int i = 0; i < reachCones.Length; i++)
		{
			if (!reachCones[i].isValid)
			{
				if (smoothIterations <= 0)
				{
					int num = 0;
					num = ((i < reachCones.Length - 1) ? (i + 1) : 0);
					LogWarning("Reach Cone {point " + i + ", point " + num + ", Origin} has negative volume. Make sure Axis vector is in the reachable area and the polygon is convex.");
				}
				else
				{
					LogWarning("One of the Reach Cones in the polygon has negative volume. Make sure Axis vector is in the reachable area and the polygon is convex.");
				}
			}
		}
		axis = axis.normalized;
	}

	public void ResetToDefault()
	{
		points = new LimitPoint[4];
		for (int i = 0; i < points.Length; i++)
		{
			points[i] = new LimitPoint();
		}
		Quaternion quaternion = Quaternion.AngleAxis(45f, Vector3.right);
		Quaternion quaternion2 = Quaternion.AngleAxis(45f, Vector3.up);
		points[0].point = quaternion * quaternion2 * axis;
		points[1].point = Quaternion.Inverse(quaternion) * quaternion2 * axis;
		points[2].point = Quaternion.Inverse(quaternion) * Quaternion.Inverse(quaternion2) * axis;
		points[3].point = quaternion * Quaternion.Inverse(quaternion2) * axis;
		BuildReachCones();
	}

	public void BuildReachCones()
	{
		smoothIterations = Mathf.Clamp(smoothIterations, 0, 3);
		P = new Vector3[points.Length];
		for (int i = 0; i < points.Length; i++)
		{
			P[i] = points[i].point.normalized;
		}
		for (int j = 0; j < smoothIterations; j++)
		{
			P = method_4();
		}
		reachCones = new ReachCone[P.Length];
		for (int k = 0; k < reachCones.Length - 1; k++)
		{
			reachCones[k] = new ReachCone(Vector3.zero, axis.normalized, P[k], P[k + 1]);
		}
		reachCones[P.Length - 1] = new ReachCone(Vector3.zero, axis.normalized, P[P.Length - 1], P[0]);
		for (int l = 0; l < reachCones.Length; l++)
		{
			reachCones[l].Calculate();
		}
	}

	public Vector3[] method_4()
	{
		Vector3[] array = new Vector3[P.Length * 2];
		float num = method_5(P.Length);
		for (int i = 0; i < array.Length; i += 2)
		{
			array[i] = method_6(P[i / 2], 1f);
		}
		for (int j = 1; j < array.Length; j += 2)
		{
			Vector3 vector = Vector3.zero;
			Vector3 zero = Vector3.zero;
			Vector3 vector2 = Vector3.zero;
			if (j > 1 && j < array.Length - 2)
			{
				vector = array[j - 2];
				vector2 = array[j + 1];
			}
			else if (j == 1)
			{
				vector = array[^2];
				vector2 = array[j + 1];
			}
			else if (j == array.Length - 1)
			{
				vector = array[j - 2];
				vector2 = array[0];
			}
			zero = ((j >= array.Length - 1) ? array[0] : array[j + 1]);
			int num2 = array.Length / points.Length;
			array[j] = 0.5f * (array[j - 1] + zero) + num * points[j / num2].tangentWeight * (zero - vector) + num * points[j / num2].tangentWeight * (array[j - 1] - vector2);
		}
		for (int k = 0; k < array.Length; k++)
		{
			array[k] = method_7(array[k], 1f);
		}
		return array;
	}

	public float method_5(int k)
	{
		if (k <= 3)
		{
			return 0.1667f;
		}
		return k switch
		{
			4 => 0.1036f, 
			5 => 0.085f, 
			6 => 0.0773f, 
			7 => 0.07f, 
			_ => 0.0625f, 
		};
	}

	public Vector3 method_6(Vector3 p, float r)
	{
		float num = Vector3.Dot(axis, p);
		float num2 = 2f * r * r / (r * r + num);
		return num2 * p + (1f - num2) * -axis;
	}

	public Vector3 method_7(Vector3 q, float r)
	{
		float num = Vector3.Dot(q - axis, q - axis);
		float num2 = 4f * r * r / (4f * r * r + num);
		return num2 * q + (1f - num2) * -axis;
	}

	public Quaternion method_8(Quaternion rotation)
	{
		if (rotation == Quaternion.identity)
		{
			return rotation;
		}
		Vector3 vector = rotation * axis;
		int num = method_9(vector);
		if (num == -1)
		{
			if (!GClass1465.logged)
			{
				LogWarning("RotationLimitPolygonal reach cones are invalid.");
			}
			return rotation;
		}
		if (Vector3.Dot(reachCones[num].B, vector) > 0f)
		{
			return rotation;
		}
		Vector3 rhs = Vector3.Cross(axis, vector);
		vector = Vector3.Cross(-reachCones[num].B, rhs);
		return Quaternion.FromToRotation(rotation * axis, vector) * rotation;
	}

	public int method_9(Vector3 L)
	{
		float num = Vector3.Dot(reachCones[0].S, L);
		int num2 = 0;
		while (true)
		{
			if (num2 < reachCones.Length)
			{
				float num3 = num;
				num = ((num2 >= reachCones.Length - 1) ? Vector3.Dot(reachCones[0].S, L) : Vector3.Dot(reachCones[num2 + 1].S, L));
				if (num3 >= 0f && !(num >= 0f))
				{
					break;
				}
				num2++;
				continue;
			}
			return -1;
		}
		return num2;
	}
}
