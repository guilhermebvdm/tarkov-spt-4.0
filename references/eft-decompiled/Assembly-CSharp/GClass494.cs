using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass494 : BotMover
{
	[NonSerialized]
	public GClass545 Gclass545_0 = new GClass545();

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public CollisionFlags CollisionFlags_0;

	[NonSerialized]
	public GClass497 Gclass497_0;

	[NonSerialized]
	public const float Float_0 = 0.3f;

	public override bool IsImpostorWorks => Bool_0;

	public override bool Blocked => CollisionFlags_0 != CollisionFlags.None;

	public GClass494(BotOwner owner, Player player, AICoversData covers)
		: base(owner, player, covers)
	{
		Gclass497_0 = new GClass497(owner);
		GClass548 moverStateMachine = MoverStateMachine;
		moverStateMachine.OnStateChanged = (Action<GClass547>)Delegate.Combine(moverStateMachine.OnStateChanged, (Action<GClass547>)delegate
		{
			Gclass497_0.Activate(base.ActualPathController);
		});
	}

	public override void InertionUpdateOnCoef(float coef)
	{
		Gclass497_0.UpdateOnCoef(coef);
	}

	public override void Activate()
	{
		base.Activate();
		Gclass497_0.Activate(base.ActualPathController);
		Player.MovementContext.OnMotionApplied += method_21;
		base.ActualPathController.OnWayChanged += method_20;
	}

	public void method_20(bool havePrevWay)
	{
		if (base.ActualPathController.CurPath != null && !havePrevWay)
		{
			base.PositionOnWayInner = BotOwner_0.Position;
		}
	}

	public override void OnDrawGizmosPrevWay()
	{
		base.OnDrawGizmosPrevWay();
	}

	public override bool CheckCornerIndexByReachDist(float distCur, Vector3 position)
	{
		return true;
	}

	public override void SetLastContex(float remainDist, Vector3 directionMove, Vector3 targetPos)
	{
		Gclass545_0.DirectionMove = GClass855.NormalizeFastSelf(directionMove);
		Gclass545_0.RemainDist = remainDist;
		Gclass545_0.TargetPos = targetPos;
		if (!base.ActualPathController.IsLast())
		{
			Gclass545_0.NextTargetPath = base.ActualPathController.CurPath;
		}
		else
		{
			Gclass545_0.NextTargetPath = null;
		}
		Gclass545_0.PrevPosPlayer = BotOwner_0.Position;
		Gclass545_0.PrevPosWay = base.PositionOnWayInner;
		Gclass545_0.Used = false;
	}

	public override bool CheckIsOnPoint(Vector3 position)
	{
		if (DirCurPoint_1.x == 0f && DirCurPoint_1.z == 0f)
		{
			base.ActualPathController.IncCornerIndex();
			if (base.ActualPathController.IsLast())
			{
				return false;
			}
			CurrentTargetPoint = base.ActualPathController.CurrentCorner();
			DirCurPoint_1 = CurrentTargetPoint - position;
		}
		return true;
	}

	public void method_21(CollisionFlags flags, Vector3 deltaMove)
	{
		Bool_0 = false;
		CollisionFlags_0 = flags;
		if (!base.ActualPathController.HavePath)
		{
			return;
		}
		Bool_0 = true;
		float num = deltaMove.magnitude;
		if (num > 0.3f)
		{
			num = 0.3f;
		}
		Vector3 vector = base.PositionOnWayInner;
		if (!Gclass545_0.Used)
		{
			Gclass545_0.Used = true;
			if (Gclass545_0.RemainDist < num)
			{
				if (Gclass545_0.NextTargetPath != null)
				{
					int num2 = Gclass545_0.NextTargetPath.CurIndex + 1;
					if (num2 > 0)
					{
						float num3 = num - Gclass545_0.RemainDist;
						for (int i = num2; i < Gclass545_0.NextTargetPath.Length; i++)
						{
							Vector3 point = Gclass545_0.NextTargetPath.GetPoint(i);
							Vector3 point2 = Gclass545_0.NextTargetPath.GetPoint(i - 1);
							Vector3 v = point - point2;
							float magnitude = v.magnitude;
							base.ActualPathController.IncCornerIndex();
							if (num3 >= magnitude)
							{
								num3 -= magnitude;
								if (i == Gclass545_0.NextTargetPath.Length - 1)
								{
									vector = point;
									break;
								}
								continue;
							}
							float num4 = num3;
							Vector3 vector2 = GClass855.NormalizeFastSelf(v);
							Vector3 vector3 = num4 * vector2;
							vector = point2 + vector3;
							break;
						}
					}
					else if (!base.ActualPathController.IsLast())
					{
						base.ActualPathController.IncCornerIndex();
					}
				}
			}
			else
			{
				Vector3 vector4 = Gclass545_0.DirectionMove * num;
				vector = base.PositionOnWayInner + vector4;
			}
		}
		if (base.HasPathAndNoComplete)
		{
			Vector3 realDelta = vector - base.PositionOnWayInner;
			vector = Gclass497_0.AddToInertion(realDelta, base.PositionOnWayInner);
		}
		base.PositionOnWayInner = vector;
		EBotLinkResult debugType = SetPlayerToNavMesh(base.PositionOnWay);
		Vector3 startPos = base.PositionOnWayInner + Vector3.up * 0.3f;
		Debuger.AddDebugPos(debugType, startPos, BotOwner_0.Position, base.PositionOnWayInner);
	}

	public override Vector3 InertionOffset()
	{
		if (base.ActualPathController.HavePath)
		{
			return Gclass497_0.GetOffset();
		}
		return Vector3.zero;
	}

	public override void Dispose()
	{
		Gclass497_0.Dispose();
		base.ActualPathController.OnWayChanged -= method_20;
		Player.MovementContext.OnMotionApplied -= method_21;
		base.Dispose();
	}

	[CompilerGenerated]
	public void method_22(GClass547 x)
	{
		Gclass497_0.Activate(base.ActualPathController);
	}
}
