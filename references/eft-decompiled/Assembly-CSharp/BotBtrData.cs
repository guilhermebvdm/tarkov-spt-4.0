using EFT;
using EFT.Vehicle;
using UnityEngine;

public class BotBtrData : GClass429
{
	public float Single_0 => BotOwner_0.Settings.FileSettings.Mind.ANGLE_TO_SHOOT_BTR;

	public float Single_1 => BotOwner_0.Settings.FileSettings.Mind.ROTATION_SPEED_BTR;

	public Vector3 TurretMachineGunShootPoint => BTRControllerClass.Instance.BtrVehicle.BTRTurret.machineGunLaunchPoint.position;

	public BotBtrData(BotOwner owner)
		: base(owner)
	{
		if (owner.Profile.Info.Settings.Role == WildSpawnType.shooterBTR)
		{
			owner.BotsController.BotTradersServices.BTRServices.RegisterBotBTR(owner);
		}
	}

	public void SyncBotRotation(Vector3 btrLookDirection)
	{
		Vector3 lookDirection = BotOwner_0.LookDirection;
		Vector3 vector = btrLookDirection;
		float num = Vector3.Angle(vector, new Vector3(vector.x, 0f, vector.z));
		float num2 = Mathf.Sign(vector.y);
		float num3 = Vector3.Angle(lookDirection, new Vector3(lookDirection.x, 0f, lookDirection.z));
		float num4 = Mathf.Sign(lookDirection.y);
		lookDirection.y = 0f;
		vector.y = 0f;
		float num5 = Vector3.SignedAngle(lookDirection, vector, Vector3.up);
		float num6 = num3 * num4 - num * num2;
		BotOwner_0.ShootData.SetCanShootByState(Mathf.Abs(num5) + Mathf.Abs(num6) < Single_0);
		if (Mathf.Abs(num5) < 2f * Single_1)
		{
			if (Mathf.Abs(num5) < 0.01f)
			{
				num5 = 0f;
			}
		}
		else
		{
			num5 = Mathf.Sign(num5) * Single_1;
		}
		if (Mathf.Abs(num6) < 2f * Single_1)
		{
			if (Mathf.Abs(num6) < 0.01f)
			{
				num6 = 0f;
			}
		}
		else
		{
			num6 = Mathf.Sign(num6) * Single_1;
		}
		Vector2 deltaRotation = new Vector2(num5, num6);
		BotOwner_0.GetPlayer.Rotate(deltaRotation);
	}

	public bool CanShoot()
	{
		BTRTurretServer bTRTurret = BTRControllerClass.Instance.BtrVehicle.BTRTurret;
		if (bTRTurret.MachineGunAngleToTarget <= Single_0)
		{
			return bTRTurret.IsCanShoot;
		}
		return false;
	}
}
