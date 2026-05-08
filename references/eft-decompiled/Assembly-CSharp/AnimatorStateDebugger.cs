using UnityEngine;

[GAttribute19(typeof(GClass1333))]
public class AnimatorStateDebugger : StateMachineBehaviour
{
	public string Name;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		Debug.LogFormat("[frame:{0}] [name:{1}]-OnStateEnter [nt {2}]", Time.frameCount, Name, stateInfo.normalizedTime);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Debug.LogFormat("[frame:{0}] [name:{1}]-OnStateExit [nt {2}]", Time.frameCount, Name, stateInfo.normalizedTime);
	}

	public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
	{
		base.OnStateMachineEnter(animator, stateMachinePathHash);
		Debug.LogFormat("[frame:{0}] [name:{1}]-OnStateMachineEnter", Time.frameCount, Name);
	}

	public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
	{
		base.OnStateMachineExit(animator, stateMachinePathHash);
		Debug.LogFormat("[frame:{0}] [name:{1}]-OnStateMachineExit", Time.frameCount, Name);
	}
}
