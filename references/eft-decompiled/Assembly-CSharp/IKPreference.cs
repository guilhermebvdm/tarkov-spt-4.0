using RootMotion.FinalIK;
using UnityEngine;

public class IKPreference : StateMachineBehaviour
{
	public LimbIK[] footIKs = new LimbIK[0];

	public bool IKEnabledEnter;

	public bool IKEnabledExit;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (footIKs.Length == 0)
		{
			method_0(animator);
		}
		LimbIK[] array = footIKs;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = IKEnabledEnter;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (footIKs.Length == 0)
		{
			method_0(animator);
		}
		LimbIK[] array = footIKs;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = IKEnabledExit;
		}
	}

	public void method_0(Animator animator)
	{
		footIKs = animator.gameObject.GetComponentsInChildren<LimbIK>();
	}
}
