using EFT;
using UnityEngine;

public class GClass2128 : JumpStateClass
{
	public GClass2128(MovementContext movementContext)
		: base(movementContext)
	{
	}

	public override void Enter(bool isFromSameState)
	{
		base.Enter(isFromSameState);
		EjumpState_0 = EJumpState.Jump;
		Vector3_1 = Vector3.zero;
	}

	public override void Exit(bool toSameState)
	{
		base.Exit(toSameState);
		MovementContext.PlayerAnimatorEnableFallingDown(enabled: false);
	}
}
