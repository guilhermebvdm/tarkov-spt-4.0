public class GClass2106 : GClass2105
{
	public GClass2106(DoorInteractionStateClass baseState)
		: base(baseState)
	{
	}

	public override void OnEnter()
	{
		DoorInteractionStateClass.StartInteractionAnimation();
	}

	public override void OnExit()
	{
		DoorInteractionStateClass.ExecuteInteraction();
	}
}
