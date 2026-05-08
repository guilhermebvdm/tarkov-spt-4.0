using System;
using System.Collections;
using UnityEngine;

namespace RootMotion.FinalIK;

public abstract class OffsetModifier : MonoBehaviour
{
	[Serializable]
	public class OffsetLimits
	{
		[Tooltip("The effector type (this is just an enum)")]
		public FullBodyBipedEffector effector;

		[Tooltip("Spring force, if zero then this is a hard limit, if not, offset can exceed the limit.")]
		public float spring;

		[Tooltip("Which axes to limit the offset on?")]
		public bool x;

		[Tooltip("Which axes to limit the offset on?")]
		public bool y;

		[Tooltip("Which axes to limit the offset on?")]
		public bool z;

		[Tooltip("The limits")]
		public float minX;

		[Tooltip("The limits")]
		public float maxX;

		[Tooltip("The limits")]
		public float minY;

		[Tooltip("The limits")]
		public float maxY;

		[Tooltip("The limits")]
		public float minZ;

		[Tooltip("The limits")]
		public float maxZ;

		public void Apply(IKEffector e, Quaternion rootRotation)
		{
			Vector3 vector = Quaternion.Inverse(rootRotation) * e.positionOffset;
			if (spring <= 0f)
			{
				if (x)
				{
					vector.x = Mathf.Clamp(vector.x, minX, maxX);
				}
				if (y)
				{
					vector.y = Mathf.Clamp(vector.y, minY, maxY);
				}
				if (z)
				{
					vector.z = Mathf.Clamp(vector.z, minZ, maxZ);
				}
			}
			else
			{
				if (x)
				{
					vector.x = method_0(vector.x, minX, maxX);
				}
				if (y)
				{
					vector.y = method_0(vector.y, minY, maxY);
				}
				if (z)
				{
					vector.z = method_0(vector.z, minZ, maxZ);
				}
			}
			e.positionOffset = rootRotation * vector;
		}

		public float method_0(float value, float min, float max)
		{
			if (value > min && value < max)
			{
				return value;
			}
			if (value < min)
			{
				return method_1(value, min, negative: true);
			}
			return method_1(value, max, negative: false);
		}

		public float method_1(float value, float limit, bool negative)
		{
			float num = value - limit;
			float num2 = num * spring;
			if (negative)
			{
				return value + Mathf.Clamp(0f - num2, 0f, 0f - num);
			}
			return value - Mathf.Clamp(num2, 0f, num);
		}
	}

	[Tooltip("The master weight")]
	public float weight = 1f;

	[Tooltip("Reference to the FBBIK component")]
	[SerializeField]
	protected FullBodyBipedIK ik;

	private float float_0;

	public float deltaTime => Time.time - float_0;

	public abstract void OnModifyOffset();

	public virtual void Start()
	{
		StartCoroutine(method_0());
	}

	public IEnumerator method_0()
	{
		while (ik == null)
		{
			yield return null;
		}
		IKSolverFullBodyBiped solver = ik.solver;
		solver.OnPreUpdate = (IKSolver.GDelegate47)Delegate.Combine(solver.OnPreUpdate, new IKSolver.GDelegate47(method_1));
		float_0 = Time.time;
	}

	public void method_1()
	{
		if (base.enabled && !(weight <= 0f) && !(deltaTime <= 0f) && !(ik == null))
		{
			weight = Mathf.Clamp(weight, 0f, 1f);
			OnModifyOffset();
			float_0 = Time.time;
		}
	}

	public void ApplyLimits(OffsetLimits[] limits)
	{
		foreach (OffsetLimits offsetLimits in limits)
		{
			offsetLimits.Apply(ik.solver.GetEffector(offsetLimits.effector), base.transform.rotation);
		}
	}

	public virtual void OnDestroy()
	{
		if (ik != null)
		{
			IKSolverFullBodyBiped solver = ik.solver;
			solver.OnPreUpdate = (IKSolver.GDelegate47)Delegate.Remove(solver.OnPreUpdate, new IKSolver.GDelegate47(method_1));
		}
	}

	public OffsetModifier()
	{
	}
}
