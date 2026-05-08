using EFT;
using JetBrains.Annotations;

public class GClass472 : BotEnemiesController
{
	public GClass472([NotNull] BotOwner owner)
		: base(owner)
	{
	}

	public override EnemyInfo AddNew(BotsGroup botsGroup, IPlayer enemy, BotSettingsClass groupInfo)
	{
		return new GClass540(botsGroup, enemy, BotOwner_0, groupInfo);
	}
}
