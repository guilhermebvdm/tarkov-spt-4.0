using System;
using EFT;
using UnityEngine;

public class GClass595 : BotRequest
{
	[NonSerialized]
	public const float Float_0 = 12f;

	[NonSerialized]
	public const float Float_1 = 25f;

	public Vector3? Direction;

	public override EBotRequestMode RequestMode => EBotRequestMode.Fight;

	public GClass595(Player requester, Vector3 dir)
		: base(requester, BotRequestType.hold)
	{
		EndTimeBeforeTaken = Time.time + LocalBotSettingsProviderClass.Core.HOLD_REQUEST_TIME_SEC;
		Direction = dir;
	}

	public override void PlayerDestroy(Player player)
	{
	}

	public override void Take(BotOwner executor)
	{
		base.Take(executor);
		SetEndTimeOnTake(LocalBotSettingsProviderClass.Core.HOLD_REQUEST_TIME_SEC);
		if (Direction.HasValue)
		{
			SetDirection(Direction.Value, Requester.PlayerBones.Head.position);
		}
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		return default(AICoreActionEndStruct);
	}

	public override bool CanProceed()
	{
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

	public override bool CanRequest(BotOwner requester)
	{
		return true;
	}

	public void SetDirection(Vector3 dir, Vector3 headPos)
	{
		dir.y = 0f;
		Vector3 position = Executor.GetPlayer.PlayerBones.Head.position;
		dir = GClass855.NormalizeFastSelf(dir);
		RaycastHit hitInfo;
		Vector3 direction = ((!Physics.Raycast(new Ray(headPos, dir), out hitInfo, 12f, LayerMaskClass.HighPolyWithTerrainMaskAI)) ? (headPos + dir * 25f - position) : (hitInfo.point - position));
		if (Physics.Raycast(new Ray(position, direction), out hitInfo, 2f, LayerMaskClass.HighPolyWithTerrainMaskAI))
		{
			Vector3 p = headPos + dir * 12f;
			Vector3 dir2 = GClass369.Test4Sides(GClass369.GetProjectionPoint(position, headPos, p) - position, position);
			method_0(dir2);
		}
		else
		{
			Vector3 dir3 = GClass369.Test4Sides(dir, position);
			method_0(dir3);
		}
	}

	public void method_0(Vector3 dir)
	{
		SetEndTimeOnTake(LocalBotSettingsProviderClass.Core.HOLD_REQUEST_TIME_SEC);
		Executor.BotTalk.TrySay(EPhraseTrigger.Roger, withGroupDelay: true);
		Direction = dir;
		Executor.Memory.botObserveData.SetVectorToLook(Direction.Value);
	}
}
