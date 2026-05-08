using EFT;
using JetBrains.Annotations;

public class GClass522 : BotSubTactic
{
	public GClass522([NotNull] BotOwner owner)
		: base(owner)
	{
	}

	public override CoverSearchType SearchRunToCover(CoverShootType shootType)
	{
		if (shootType == CoverShootType.hide)
		{
			return CoverSearchType.distToBot;
		}
		return CoverSearchType.distToToCenter;
	}
}
