using EFT;

public abstract class GClass2180
{
	public static bool IsScav(this EPlayerGroup playerGroup)
	{
		return playerGroup == EPlayerGroup.Scav;
	}

	public static EPlayerGroup ToPlayerGroup(this EPlayerSide playerSide)
	{
		if (playerSide != EPlayerSide.Savage)
		{
			return EPlayerGroup.Pmc;
		}
		return EPlayerGroup.Scav;
	}

	public static EPlayerGroup ToPlayerGroup(this ESideType playerSide)
	{
		if (playerSide != ESideType.Savage)
		{
			return EPlayerGroup.Pmc;
		}
		return EPlayerGroup.Scav;
	}
}
