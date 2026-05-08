using System;
using EFT;
using UnityEngine;

public class GClass592(Player requester) : BotRequest(requester, BotRequestType.followMe)
{
	public const float EXECUTER_FAR_SQRT = 64f;

	public const float MAX_SQRT_DIST = 2500f;

	public const float MAX_SQRT_DIST_FROM_TAKE_POINT = 40000f;

	public Vector3 StartExecuterPosition;

	[NonSerialized]
	public float Float_0 = -1f;

	public override EBotRequestMode RequestMode => EBotRequestMode.Peace;

	public override bool CanStartExecute(BotOwner executor)
	{
		if (executor.WeaponManager.Stationary.Taken)
		{
			return false;
		}
		if (executor.Memory.GoalEnemy != null)
		{
			return false;
		}
		if (!executor.LoyaltyData.CanApply(Requester))
		{
			return false;
		}
		bool num = base.CanStartExecute(executor);
		if (num)
		{
			StartExecuterPosition = executor.Position;
			Float_0 = Time.time;
		}
		return num;
	}

	public override bool CanProceed()
	{
		EndIfCantExecute = false;
		if (!(Executor != null))
		{
			return true;
		}
		if (method_0())
		{
			EndIfCantExecute = true;
			return false;
		}
		if (IsExecuterTooFar())
		{
			EndIfCantExecute = true;
			return false;
		}
		if (Executor.Memory.GoalEnemy != null)
		{
			if (Executor.Memory.GoalEnemy.IsVisible)
			{
				return false;
			}
			if (Time.time - Executor.Memory.GoalEnemy.TimeLastSeen < 10f)
			{
				return false;
			}
			return true;
		}
		return true;
	}

	public override void PlayerDestroy(Player player)
	{
	}

	public Vector3 CurTragte()
	{
		if (Executor.Mover.HasPathAndNoComplete && Executor.Mover.TargetPoint.HasValue)
		{
			return Executor.Mover.TargetPoint.Value;
		}
		return Executor.Position;
	}

	public bool IsExecuterFar()
	{
		return (Requester.Position - CurTragte()).sqrMagnitude > 64f;
	}

	public bool IsExecuterTooFar()
	{
		return (Requester.Position - Executor.Position).sqrMagnitude > 2500f;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		return new AICoreActionEndStruct("foFollow");
	}

	public override bool CanRequest(BotOwner requester)
	{
		return true;
	}

	public bool method_0()
	{
		if (Float_0 < 0f)
		{
			return false;
		}
		if (Time.time - Float_0 < 5f)
		{
			return false;
		}
		if ((StartExecuterPosition - Executor.Position).sqrMagnitude > 40000f)
		{
			return true;
		}
		return false;
	}
}
