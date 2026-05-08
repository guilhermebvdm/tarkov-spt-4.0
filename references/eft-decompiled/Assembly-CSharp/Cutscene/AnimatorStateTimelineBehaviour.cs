using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Cutscene;

public class AnimatorStateTimelineBehaviour : StateMachineBehaviour
{
	public int stateHash;

	public AnimatorStateTimelineData data;

	private GClass1047 gclass1047_0;

	private int int_0;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		method_1(animator);
		method_0(animator, stateInfo, layerIndex);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		method_1(animator);
		method_0(animator, stateInfo, layerIndex);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		method_1(animator);
		int_0 = 0;
		gclass1047_0.OnStateExit(-1f, stateInfo.length);
	}

	public void method_0(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		int num = (int)stateInfo.normalizedTime;
		float num2 = stateInfo.normalizedTime - (float)num;
		if (num != int_0)
		{
			if (num2 > 0f)
			{
				gclass1047_0.OnStateUpdate(0f, stateInfo.length);
			}
			int_0 = num;
		}
		gclass1047_0.OnStateUpdate(num2, stateInfo.length);
	}

	public void method_1(Animator animator)
	{
		if (gclass1047_0 == null)
		{
			gclass1047_0 = new GClass1047(animator.gameObject, GClass1047.ESyncTime.AudioSystem, data.events.Where((AnimationEvent e) => e.stateHash == stateHash).ToArray());
		}
	}

	[CompilerGenerated]
	public bool method_2(AnimationEvent e)
	{
		return e.stateHash == stateHash;
	}
}
