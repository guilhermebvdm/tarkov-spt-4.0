using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class InteractionLookAt
{
	[Tooltip("(Optional) reference to the LookAtIK component that will be used to make the character look at the objects that it is interacting with.")]
	public LookAtIK ik;

	[Tooltip("Interpolation speed of the LookAtIK target.")]
	public float lerpSpeed = 5f;

	[Tooltip("Interpolation speed of the LookAtIK weight.")]
	public float weightSpeed = 1f;

	[HideInInspector]
	public bool isPaused;

	[NonSerialized]
	public Transform LookAtTarget;

	[NonSerialized]
	public float StopLookTime;

	[NonSerialized]
	public float Weight;

	[NonSerialized]
	public bool FirstFBBIKSolve;

	public void Look(Transform target, float time)
	{
		if (!(ik == null))
		{
			if (ik.solver.IKPositionWeight <= 0f)
			{
				ik.solver.IKPosition = ik.solver.GetRoot().position + ik.solver.GetRoot().forward * 3f;
			}
			LookAtTarget = target;
			StopLookTime = time;
		}
	}

	public void Update()
	{
		if (ik == null)
		{
			return;
		}
		if (ik.enabled)
		{
			ik.enabled = false;
		}
		if (!(LookAtTarget == null))
		{
			if (isPaused)
			{
				StopLookTime += Time.deltaTime;
			}
			float num = ((Time.time < StopLookTime) ? weightSpeed : (0f - weightSpeed));
			Weight = Mathf.Clamp(Weight + num * Time.deltaTime, 0f, 1f);
			ik.solver.IKPositionWeight = GClass1462.Float(Weight, InterpolationMode.InOutQuintic);
			ik.solver.IKPosition = Vector3.Lerp(ik.solver.IKPosition, LookAtTarget.position, lerpSpeed * Time.deltaTime);
			if (Weight <= 0f)
			{
				LookAtTarget = null;
			}
			FirstFBBIKSolve = true;
		}
	}

	public void SolveSpine()
	{
		if (!(ik == null) && FirstFBBIKSolve)
		{
			float headWeight = ik.solver.headWeight;
			float eyesWeight = ik.solver.eyesWeight;
			ik.solver.headWeight = 0f;
			ik.solver.eyesWeight = 0f;
			ik.solver.Update();
			ik.solver.headWeight = headWeight;
			ik.solver.eyesWeight = eyesWeight;
		}
	}

	public void SolveHead()
	{
		if (!(ik == null) && FirstFBBIKSolve)
		{
			float bodyWeight = ik.solver.bodyWeight;
			ik.solver.bodyWeight = 0f;
			ik.solver.Update();
			ik.solver.bodyWeight = bodyWeight;
			FirstFBBIKSolve = false;
		}
	}
}
