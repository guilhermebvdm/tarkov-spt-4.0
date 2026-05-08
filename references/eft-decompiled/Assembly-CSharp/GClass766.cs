public class GClass766
{
	public EPlayerState StateName;

	public virtual GClass766 Clone()
	{
		return new GClass766
		{
			StateName = StateName
		};
	}
}
