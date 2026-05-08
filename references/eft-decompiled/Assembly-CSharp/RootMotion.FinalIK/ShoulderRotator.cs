using System;
using UnityEngine;

namespace RootMotion.FinalIK;

public class ShoulderRotator : MonoBehaviour
{
	[Tooltip("Weight of shoulder rotation")]
	public float weight = 1.5f;

	[Tooltip("The greater the offset, the sooner the shoulder will start rotating")]
	public float offset = 0.2f;

	private FullBodyBipedIK fullBodyBipedIK_0;

	private bool bool_0;

	public void Start()
	{
		fullBodyBipedIK_0 = GetComponent<FullBodyBipedIK>();
		if (fullBodyBipedIK_0 == null)
		{
			fullBodyBipedIK_0 = GetComponentInChildren<FullBodyBipedIK>();
		}
		IKSolverFullBodyBiped solver = fullBodyBipedIK_0.solver;
		solver.OnPostUpdate = (IKSolver.GDelegate47)Delegate.Combine(solver.OnPostUpdate, new IKSolver.GDelegate47(method_0));
	}

	public void method_0()
	{
		if (!(fullBodyBipedIK_0 == null) && !(fullBodyBipedIK_0.solver.IKPositionWeight <= 0f))
		{
			if (bool_0)
			{
				bool_0 = false;
				return;
			}
			method_1(FullBodyBipedChain.LeftArm, weight, offset);
			method_1(FullBodyBipedChain.RightArm, weight, offset);
			bool_0 = true;
			fullBodyBipedIK_0.solver.Update();
		}
	}

	public void method_1(FullBodyBipedChain chain, float weight, float offset)
	{
		Quaternion b = Quaternion.FromToRotation(method_2(chain).swingDirection, fullBodyBipedIK_0.solver.GetEndEffector(chain).position - method_2(chain).transform.position);
		Vector3 vector = fullBodyBipedIK_0.solver.GetEndEffector(chain).position - fullBodyBipedIK_0.solver.GetLimbMapping(chain).bone1.position;
		float num = fullBodyBipedIK_0.solver.GetChain(chain).nodes[0].length + fullBodyBipedIK_0.solver.GetChain(chain).nodes[1].length;
		float num2 = vector.magnitude / num - 1f + offset;
		num2 = Mathf.Clamp(num2 * weight, 0f, 1f);
		Quaternion quaternion = Quaternion.Lerp(Quaternion.identity, b, num2 * fullBodyBipedIK_0.solver.GetEndEffector(chain).positionWeight * fullBodyBipedIK_0.solver.IKPositionWeight);
		fullBodyBipedIK_0.solver.GetLimbMapping(chain).parentBone.rotation = quaternion * fullBodyBipedIK_0.solver.GetLimbMapping(chain).parentBone.rotation;
	}

	public IKMapping.BoneMap method_2(FullBodyBipedChain chain)
	{
		return fullBodyBipedIK_0.solver.GetLimbMapping(chain).GetBoneMap(IKMappingLimb.BoneMapType.Parent);
	}

	public void OnDestroy()
	{
		if (fullBodyBipedIK_0 != null)
		{
			IKSolverFullBodyBiped solver = fullBodyBipedIK_0.solver;
			solver.OnPostUpdate = (IKSolver.GDelegate47)Delegate.Remove(solver.OnPostUpdate, new IKSolver.GDelegate47(method_0));
		}
	}
}
