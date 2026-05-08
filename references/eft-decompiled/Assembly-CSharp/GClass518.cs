using EFT;
using UnityEngine;

public class GClass518 : AbstractSuppressStationary
{
	public override bool Usable => false;

	public GClass518(BotSuppressStationary suppressStationary, BotOwner owner)
		: base(suppressStationary, owner)
	{
	}

	public override bool CanStartSuppressAt(Vector3 enemyPos)
	{
		return false;
	}

	public override bool CanStartSupressEnemy(EnemyInfo memoryGoalEnemy)
	{
		return false;
	}

	public override void StopExternal(Vector3 badPos)
	{
	}

	public override bool IsReady()
	{
		return false;
	}
}
