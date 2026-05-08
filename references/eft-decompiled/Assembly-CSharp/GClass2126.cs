using EFT;

public class GClass2126 : JumpLandingStateClass
{
	public GClass2126(MovementContext movementContext)
		: base(movementContext)
	{
	}

	public override void Enter(bool isFromSameState)
	{
		base.Enter(isFromSameState);
	}

	public override void Exit(bool toSameState)
	{
		MovementContext.PlayerAnimator.SetIsVaulting(isVaulting: false);
		base.Exit(toSameState);
	}
}
