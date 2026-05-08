using EFT;

public class GClass228 : GClass200<GClass31>
{
	public bool CanChangeDecision = true;

	public virtual bool UseZigZag => false;

	public GClass228(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass31 data)
	{
		bool flag = method_0() != DoorInteractionStatus.OpeningDoor;
		BotOwner_0.BotLight.TurnOff();
		ShootPointClass shootPointClass = null;
		bool flag2;
		if (flag2 = BotOwner_0.Tactic.IsCurTactic(BotsGroup.BotCurrentTactic.Attack) || BotOwner_0.Tactic.IsCurTactic(BotsGroup.BotCurrentTactic.Protect))
		{
			shootPointClass = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		}
		if (shootPointClass == null)
		{
			flag2 = false;
		}
		if (BotOwner_0.Memory.GoalEnemy != null && BotOwner_0.Memory.GoalEnemy.CanShoot && BotOwner_0.Memory.GoalEnemy.Distance < BotOwner_0.Settings.FileSettings.Mind.DIST_TO_STOP_RUN_ENEMY)
		{
			BotOwner_0.DangerPointsData.SetAllDangerNull();
		}
		CoverShootType shootType = ((!flag2) ? CoverShootType.hide : CoverShootType.shoot);
		if (data != null)
		{
			BotOwner_0.SetPose(1f);
			BotOwner_0.SetTargetMoveSpeed(1f);
			BotOwner_0.Steering.LookToMovingDirection();
			BotOwner_0.Memory.SetCoverPoints(data.Point);
			BotOwner_0.GoToPoint(data.Point);
			BotOwner_0.Sprint(!flag);
			if (BotOwner_0.Mover.IsComeTo(0.6f, onCover: true, data.Point))
			{
				BotOwner_0.Sprint(val: false, withDebugCallback: false);
				BotOwner_0.Memory.ComeToPoint();
			}
		}
		else
		{
			BotOwner_0.BotRun.Run(shootType, shootPointClass, BotOwner_0.Settings.FileSettings.Cover.CHANGE_RUN_TO_COVER_SEC, flag, CanChangeDecision, UseZigZag);
		}
	}
}
