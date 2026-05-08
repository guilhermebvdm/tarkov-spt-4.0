using System;
using EFT;
using UnityEngine;

public class GClass510 : GClass429, GInterface10
{
	public const float BAD_DIST_TO_FOLLOWER = 3.2f;

	public GClass570 AllFollowersPatrolInfo = new GClass570();

	[NonSerialized]
	public PatrollingData PatrollingData_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	public static bool IsCome(BotOwner bot, Vector3? curTrg, bool extraTarget, out float dist)
	{
		if (!curTrg.HasValue)
		{
			dist = 0f;
			return true;
		}
		Vector3 vector = curTrg.Value - bot.Transform.position;
		if (extraTarget)
		{
			dist = vector.magnitude;
			return dist < 1f;
		}
		vector.y = ((Mathf.Abs(vector.y) > bot.Settings.FileSettings.Move.Y_APPROXIMATION) ? vector.y : 0f);
		dist = vector.magnitude;
		return dist < bot.Settings.FileSettings.Move.REACH_DIST;
	}

	public static Vector3 FrontPos(BotOwner boss, bool isFront, int totalCount, int index)
	{
		if (totalCount > 1)
		{
			float num = 3.2f;
			int num2 = 1 + index / 2;
			if (num2 >= 2)
			{
				num *= 0.8f;
			}
			Vector3 vector = boss.Position + (isFront ? 1f : (-0.7f)) * boss.Mover.NormDirCurPoint * num;
			Vector3 vector2 = GClass855.Rotate90(side: (!GClass855.IsOdd(index)) ? GClass855.SideTurn.right : GClass855.SideTurn.left, n: boss.Mover.NormDirCurPoint);
			return vector + vector2 * num2;
		}
		return boss.Position + (isFront ? 1f : (-0.7f)) * boss.Mover.NormDirCurPoint * 3.2f;
	}

	public GClass510(BotOwner owner, PatrollingData data)
		: base(owner)
	{
		PatrollingData_0 = data;
	}

	public virtual void Update()
	{
		BotOwner_0.MagazineChecker.ManualUpdate();
		BotOwner_0.Mover.SetTargetMoveSpeed(1f);
		if (PatrollingData_0.CurPatrolPoint != null)
		{
			bool num = method_0();
			if (!num && Float_0 < Time.time)
			{
				Float_0 = Time.time + 2.5f;
				PatrollingData_0.PatrolPathControl.RefreshGoToPoint();
			}
			if (num)
			{
				PatrollingData_0.ComeToPoint();
			}
		}
	}

	public virtual void DrawGizmosSelected()
	{
	}

	public bool method_0()
	{
		if (PatrollingData_0.Status == PatrolStatus.stay)
		{
			return true;
		}
		Vector3 vector = PatrollingData_0.CurTargetPoint - BotOwner_0.Transform.position;
		vector.y = ((Mathf.Abs(vector.y) > BotOwner_0.Settings.FileSettings.Move.Y_APPROXIMATION) ? vector.y : 0f);
		Float_1 = vector.magnitude;
		return Float_1 < BotOwner_0.Settings.FileSettings.Move.REACH_DIST;
	}

	public virtual void Dispose()
	{
	}
}
