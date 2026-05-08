using UnityEngine;

namespace DefaultNamespace;

[GAttribute18]
public class LayerWeightStateController : StateMachineBehaviour, IStateBehaviour
{
	public float StartValue;

	public float EndValue;

	public AnimationCurve LayerWeight;

	private GClass1446 gclass1446_0;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		method_0(animator);
		animator.SetLayerWeight(2, StartValue);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		method_0(animator);
		float weight = LayerWeight.Evaluate(stateInfo.normalizedTime);
		animator.SetLayerWeight(2, weight);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		method_0(animator);
		animator.SetLayerWeight(2, EndValue);
	}

	public void method_0(Animator animator)
	{
		if (gclass1446_0 == null)
		{
			gclass1446_0 = GClass1446.Create();
		}
		gclass1446_0.SetAnimator(animator);
	}
}
