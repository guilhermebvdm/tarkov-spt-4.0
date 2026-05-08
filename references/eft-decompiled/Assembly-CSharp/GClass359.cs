using EFT;

public abstract class GClass359 : GClass356
{
	public abstract BotOwner Bot { get; }

	public GClass359(BotOwner owner)
		: base(owner)
	{
	}

	public abstract void SetBoss(GClass448 sectantPriest);

	public abstract void SetAttackWithDelay(float minDelayToSupportSec);

	public abstract bool IsCurShootAndRun();

	public abstract bool TryChangeToMelee();
}
