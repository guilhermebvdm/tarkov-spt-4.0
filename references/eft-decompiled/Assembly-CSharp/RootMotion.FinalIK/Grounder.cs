using UnityEngine;

namespace RootMotion.FinalIK;

public abstract class Grounder : MonoBehaviour
{
	public delegate void GDelegate46();

	[Tooltip("The master weight. Use this to fade in/out the grounding effect.")]
	[Range(0f, 1f)]
	public float weight = 1f;

	[Tooltip("The Grounding solver. Not to confuse with IK solvers.")]
	public Grounding solver = new Grounding();

	public GDelegate46 OnPreGrounder;

	public GDelegate46 OnPostGrounder;

	protected bool initiated;

	public abstract void ResetPosition();

	public Vector3 GetSpineOffsetTarget()
	{
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < solver.legs.Length; i++)
		{
			zero += method_0(solver.legs[i]);
		}
		return zero;
	}

	public void LogWarning(string message)
	{
		GClass1465.Log(message, base.transform);
	}

	public Vector3 method_0(Grounding.Leg leg)
	{
		Vector3 vector = method_1(leg);
		float num = (Vector3.Dot(solver.root.forward, vector.normalized) + 1f) * 0.5f;
		float magnitude = (leg.IKPosition - leg.transform.position).magnitude;
		return vector * magnitude * num;
	}

	public Vector3 method_1(Grounding.Leg leg)
	{
		Vector3 tangent = leg.transform.position - solver.root.position;
		if (solver.rotateSolver && !(solver.root.up == Vector3.up))
		{
			Vector3 normal = solver.root.up;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			return tangent;
		}
		return new Vector3(tangent.x, 0f, tangent.z);
	}

	public abstract void OpenUserManual();

	public abstract void OpenScriptReference();

	public Grounder()
	{
	}
}
