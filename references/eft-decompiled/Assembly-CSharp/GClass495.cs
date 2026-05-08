using EFT;
using UnityEngine;

public class GClass495 : BotMover
{
	public GClass495(BotOwner owner, Player player, AICoversData covers)
		: base(owner, player, covers)
	{
	}

	public override void SetLastContex(float remainDist, Vector3 direction, Vector3 realDestPoint)
	{
	}

	public override bool CheckIsOnPoint(Vector3 position)
	{
		return true;
	}

	public override bool CheckCornerIndexByReachDist(float distCur, Vector3 position)
	{
		if (distCur < base.ActualPathController.CurPath.ReachDist)
		{
			if (base.ActualPathController.IsLast())
			{
				Player.EnableSprint(enable: false);
				Stop();
				return false;
			}
			base.ActualPathController.IncCornerIndex();
			CurrentTargetPoint = base.ActualPathController.CurPath.CurrentCorner();
			Vector3 dirCurPoint_ = CurrentTargetPoint - position;
			if (base.ActualPathController.CurPath == null)
			{
				base.IsMoving = false;
				Player.EnableSprint(enable: false);
				Stop();
				return false;
			}
			if (dirCurPoint_.sqrMagnitude > 0.01f)
			{
				DirCurPoint_1 = dirCurPoint_;
			}
			BotOwner_0.Memory.botObserveData.SetVector(noTimeCheck: true);
		}
		return true;
	}
}
