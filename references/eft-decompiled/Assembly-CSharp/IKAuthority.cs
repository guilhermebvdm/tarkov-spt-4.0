using UnityEngine;

public class IKAuthority : Authority
{
	public bool Exit = true;

	public float Speed = 4f;

	private bool bool_0;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		PlayerBones.IKTransit(PlayerBones.ETransitionType.In, Speed);
		bool_0 = false;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!bool_0 && Exit)
		{
			int shortNameHash = animator.GetNextAnimatorStateInfo(0).shortNameHash;
			if (shortNameHash != 0 && shortNameHash != stateInfo.shortNameHash)
			{
				PlayerBones.IKTransit(PlayerBones.ETransitionType.Out, Speed);
				bool_0 = true;
			}
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!bool_0 && Exit)
		{
			PlayerBones.IKTransit(PlayerBones.ETransitionType.Out, Speed);
			bool_0 = true;
		}
	}
}
