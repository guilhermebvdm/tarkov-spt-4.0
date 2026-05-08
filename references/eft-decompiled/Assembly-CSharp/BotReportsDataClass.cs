using EFT;
using UnityEngine;

public class BotReportsDataClass
{
	public IPlayer Witness;

	public IPlayer Enemy;

	public Vector3 PartPos;

	public EEnemyPartVisibleType VisibleOnlyBySence;

	public BotReportsDataClass(IPlayer witness, IPlayer enemy, Vector3 partPos, EEnemyPartVisibleType visibleOnlyBySence)
	{
		Witness = witness;
		Enemy = enemy;
		PartPos = partPos;
		VisibleOnlyBySence = visibleOnlyBySence;
	}
}
