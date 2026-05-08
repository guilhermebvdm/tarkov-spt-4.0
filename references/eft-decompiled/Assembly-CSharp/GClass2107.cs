public class GClass2107 : GClass2105
{
	public GClass2107(DoorInteractionStateClass baseState)
		: base(baseState)
	{
	}

	public override void OnExit()
	{
		DoorInteractionStateClass.StopInteractionAnimation();
	}
}
