using EFT;
using EFT.Vehicle;
using UnityEngine;

public class GClass493 : BotMover
{
	public GClass493(BotOwner owner, Player player, AICoversData covers)
		: base(owner, player, covers)
	{
	}

	public override bool CheckCornerIndexByReachDist(float distCur, Vector3 position)
	{
		return true;
	}

	public override void SetLastContex(float remainDist, Vector3 directionMove, Vector3 targetPos)
	{
	}

	public override void SetPose(float targetPose)
	{
	}

	public override void Sprint(bool val, bool withDebugCallback = true)
	{
	}

	public override void GoToByWay(Vector3[] way, float reachDist)
	{
	}

	public override void ManualUpdate()
	{
		BTRControllerClass instance = BTRControllerClass.Instance;
		if (instance != null && instance.BtrVehicle != null)
		{
			BTRVehicle btrVehicle = instance.BtrVehicle;
			BotOwner_0.Mover.Teleport(btrVehicle.BTRTurret.transform.position);
			if (BotOwner_0.BotBtrData.CanShoot())
			{
				EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
				if (goalEnemy != null)
				{
					BotOwner_0.BotBtrData.SyncBotRotation(goalEnemy.Person.PlayerBones.Head.position - btrVehicle.BTRTurret.machineGunLaunchPoint.position);
				}
			}
			else
			{
				BotOwner_0.BotBtrData.SyncBotRotation(btrVehicle.BTRTurret.machineGunRoot.forward);
			}
		}
		BotOwner_0.SetPose(1f);
	}

	public override bool CheckIsOnPoint(Vector3 position)
	{
		return true;
	}
}
