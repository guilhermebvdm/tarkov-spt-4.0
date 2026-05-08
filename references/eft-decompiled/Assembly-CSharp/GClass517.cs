using EFT;
using UnityEngine;

public class GClass517 : AbstractSuppressStationary
{
	public override bool Usable => true;

	public GClass517(BotSuppressStationary suppressStationary, BotOwner owner)
		: base(suppressStationary, owner)
	{
	}

	public override bool CanStartSuppressAt(Vector3 enemyPos)
	{
		if (!IsAtSector(enemyPos))
		{
			return false;
		}
		return true;
	}

	public override bool CanStartSupressEnemy(EnemyInfo enemy)
	{
		return true;
	}

	public override void StopExternal(Vector3 badPos)
	{
		Stop();
	}
}
