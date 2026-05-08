using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public abstract class GClass236 : GClass200<GClass26>
{
	[Serializable]
	[CompilerGenerated]
	public class Class118
	{
		public static readonly Class118 class118_0 = new Class118();

		public static Action action_0;

		public void method_0()
		{
		}
	}

	[NonSerialized]
	public const float Float_0 = 100f;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public ETacticalMove EtacticalMove_0;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public float Float_3 = -1f;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public NavMeshPath NavMeshPath_0 = new NavMeshPath();

	[NonSerialized]
	public NavMeshPath NavMeshPath_1 = new NavMeshPath();

	[NonSerialized]
	public NavMeshPath NavMeshPath_2 = new NavMeshPath();

	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public float Float_8;

	[NonSerialized]
	public float Float_9;

	[NonSerialized]
	public ushort Ushort_0;

	[NonSerialized]
	public bool Bool_0 = true;

	[NonSerialized]
	public GroupPoint GroupPoint_0;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public HashSet<int> HashSet_0 = new HashSet<int>();

	[NonSerialized]
	public BifacialTransform BifacialTransform_0;

	[NonSerialized]
	public const float Float_10 = 0.1f;

	[NonSerialized]
	public const float Float_11 = 0.010000001f;

	[NonSerialized]
	public NavMeshPath NavMeshPath_3 = new NavMeshPath();

	[NonSerialized]
	public float Float_12 = 80f;

	[NonSerialized]
	public float Float_13 = 60f;

	[NonSerialized]
	public Vector3[] Vector3_0;

	public virtual float SDIST_TOO_CLOSE_TO_FINAL => 64f;

	public virtual float NEXT_COVER_LONG => 10f;

	public virtual float NEXT_COVER_SHORT => 4f;

	public virtual float STAY_AT_COVER_WITH_ENEMY => 4f;

	public virtual float ENEMY_VISIBLE_RECENTLY => 6f;

	public virtual float STAY_AT_COVER => 9f;

	public virtual float CHANCE_TO_SPRINT => 50f;

	public virtual float DIST_TO_SPRINT => 3f;

	public virtual float DIST_TO_COVER_SPRINT_MIN => 3f;

	public virtual float DIST_TO_COVER_SPRINT_MAX_OR_HIGHER => 7f;

	public virtual float CHANCE_TO_COVER_SPRINT_ON_MIN_DIST_OR_LOWER => 0f;

	public virtual float CHANCE_TO_COVER_SPRINT_ON_MAX_DIST_OR_HIGHER => 60f;

	public virtual float DIST_TO_SPRINT_SQR_MIN => 9f;

	public virtual float DIST_TO_SPRINT_SQR_MAX_OR_HIGHER => 100f;

	public virtual float CHANCE_TO_SPRINT_SQR_ON_MIN_DIST_OR_LOWER => 0f;

	public virtual float CHANCE_TO_SPRINT_SQR_ON_MAX_DIST_OR_HIGHER => 100f;

	public virtual float CHANCE_TO_STOP_SPRINT_SQR_ON_MIN_DIST_OR_LOWER => 20f;

	public virtual float CHANCE_TO_STOP_SPRINT_SQR_ON_MAX_DIST_OR_HIGHER => 0f;

	public virtual Vector3 MainTargetPosition => BotOwner_0.Memory.CurCustomCoverPoint.Position;

	public GClass236(BotOwner bot, float chanceToShortStay, float chanceToNextStopShort)
		: base(bot)
	{
		BifacialTransform_0 = bot.Transform;
		Float_12 = chanceToShortStay;
		Float_13 = chanceToNextStopShort;
		BotMover mover = bot.Mover;
		mover.onPathCornerChanged = (Action<int, Vector3[]>)Delegate.Combine(mover.onPathCornerChanged, new Action<int, Vector3[]>(method_5));
	}

	public void method_5(int pathCornerIndex, Vector3[] path)
	{
		float lengthSqr = 0f;
		if (pathCornerIndex < path.Length - 1)
		{
			lengthSqr = GClass856.SqrDistance(path[pathCornerIndex], path[pathCornerIndex + 1]);
		}
		if (BotOwner_0.Mover.Sprinting)
		{
			if (GClass856.IsTrue100(method_12(lengthSqr)))
			{
				BotOwner_0.Sprint(val: false);
			}
		}
		else
		{
			float v = method_11(lengthSqr);
			BotOwner_0.Sprint(GClass856.IsTrue100(v));
		}
	}

	public virtual void MoverToMainTarget(CustomNavigationPoint navigationPoint)
	{
		if (BotOwner_0.Memory.HaveEnemy)
		{
			BotOwner_0.BotTalk.TrySay(EPhraseTrigger.OnFight, withGroupDelay: true);
		}
		else
		{
			BotOwner_0.BotTalk.TrySay(EPhraseTrigger.OnMutter);
		}
		BotOwner_0.Memory.SetCoverPoints(navigationPoint);
		if (navigationPoint == null || !method_6(navigationPoint.Position))
		{
			BotOwner_0.GoToPoint(navigationPoint);
		}
		BotOwner_0.Sprint(val: false);
	}

	public bool method_6(Vector3 navigationPoint)
	{
		bool result = false;
		if (Vector3_0 != null)
		{
			Vector3 vector = Vector3_0[0];
			Vector3 vector2 = Vector3_0[Vector3_0.Length - 1];
			float sqrMagnitude = (vector - BotOwner_0.Position).sqrMagnitude;
			float sqrMagnitude2 = (vector2 - navigationPoint).sqrMagnitude;
			if (sqrMagnitude > 0.25f && NavMesh.CalculatePath(BotOwner_0.Position, vector, -1, NavMeshPath_2) && NavMeshPath_2.status == NavMeshPathStatus.PathComplete)
			{
				Vector3_0 = BotPathFinderClass.LinkWays(new List<Vector3[]> { NavMeshPath_2.corners, Vector3_0 });
				vector = Vector3_0[0];
				vector2 = Vector3_0[Vector3_0.Length - 1];
				sqrMagnitude = (vector - BotOwner_0.Position).sqrMagnitude;
				sqrMagnitude2 = (vector2 - navigationPoint).sqrMagnitude;
			}
			if (sqrMagnitude < 3f && sqrMagnitude2 < 3f)
			{
				result = true;
				sqrMagnitude = (vector - BotOwner_0.Position).magnitude;
				BotOwner_0.GoToByWay(Vector3_0);
			}
			Vector3_0 = null;
		}
		return result;
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_0();
		BotOwner_0.SetTargetMoveSpeed(1f);
		BotOwner_0.Mover.SetPose(1f);
		BotOwner_0.WeaponManager.TryReloadWeaponOrUnderbarrelLauncher();
		switch (EtacticalMove_0)
		{
		case ETacticalMove.moveToTarget:
			if (Float_1 < Time.time)
			{
				Float_1 = Time.time + 2f;
				method_17();
			}
			MoveToMainTarget();
			method_9();
			LookSimple();
			method_7();
			break;
		case ETacticalMove.moveToSub:
			method_15();
			LookSimple();
			method_7();
			if (Time.time - Float_3 > 30f && Float_3 > -1f)
			{
				method_14(ETacticalMove.stayAtSub);
			}
			break;
		case ETacticalMove.stayAtSub:
			method_16();
			LookStaingAtSub();
			break;
		}
	}

	public void method_7()
	{
		if (BotOwner_0.Mover.DistDestination < 2f)
		{
			BotOwner_0.Mover.Sprint(val: false);
		}
	}

	public abstract void LookSimple();

	public virtual void LookStaingAtSub()
	{
		if (Bool_0)
		{
			UpdateLookFromCover();
		}
		else
		{
			method_8();
		}
	}

	public void method_8()
	{
		if (GroupPoint_0 != null && CustomNavigationPoint_0 != null)
		{
			BotOwner_0.LookData.SetLookPointByHearing(CustomNavigationPoint_0);
		}
		else
		{
			BotOwner_0.LookData.SetLookPointByHearing();
		}
	}

	public virtual void UpdateLookFromCover()
	{
		GroupPoint groupPoint_ = GroupPoint_0;
		if (groupPoint_ != null)
		{
			if (GClass855.SqrDistHorizontal(groupPoint_.FirePosition, groupPoint_.Position) > 0.1f)
			{
				Vector3 wallDirection = groupPoint_.WallDirection;
				BotOwner_0.Tilt.Set(groupPoint_.TiltType);
				BotOwner_0.Steering.LookToDirection(wallDirection);
			}
			else
			{
				method_8();
			}
			switch (groupPoint_.CoverLevel)
			{
			case CoverLevel.Sit:
			case CoverLevel.Lay:
				BotOwner_0.SetPose(0.1f);
				break;
			case CoverLevel.Stay:
				BotOwner_0.SetPose(1f);
				break;
			}
		}
	}

	public virtual void MoveToMainTarget()
	{
		bool num = BotOwner_0.Mover.IsComeTo(BotOwner_0.Settings.FileSettings.Move.REACH_DIST, onCover: true);
		BotOwner_0.SetPose(1f);
		if (num && !BotOwner_0.Memory.ComeToPoint())
		{
			BotOwner_0.BotAttackManager.UpdateNextTick();
		}
		if (BotOwner_0.HasPathAndNotComplete || BotOwner_0.Memory.CurCustomCoverPoint == null || BotOwner_0.Memory.CurCustomCoverPoint.Owner != BotOwner_0.Id)
		{
			return;
		}
		if ((BotOwner_0.Position - BotOwner_0.Memory.CurCustomCoverPoint.Position).sqrMagnitude > 1f)
		{
			if (BotOwner_0.Memory.CurCustomCoverPoint == null || !method_6(BotOwner_0.Memory.CurCustomCoverPoint.Position))
			{
				BotOwner_0.GoToPoint(BotOwner_0.Memory.CurCustomCoverPoint);
			}
		}
		else if (!BotOwner_0.Memory.ComeToPoint())
		{
			BotOwner_0.BotAttackManager.UpdateNextTick();
		}
	}

	public void method_9()
	{
		if (Float_5 > Time.time)
		{
			return;
		}
		Float_5 = Time.time + 2f;
		if (Time.time - Float_4 < Float_9 || Time.time - BotOwner_0.Mover.LastPathSetTime < Float_9)
		{
			return;
		}
		int index;
		Vector3? vector = BotOwner_0.Mover.FindPointFarThan(15f, out index);
		if (!vector.HasValue || (vector.Value - BotOwner_0.Position).sqrMagnitude < SDIST_TOO_CLOSE_TO_FINAL)
		{
			return;
		}
		NavGraphVoxelSimple curVoxel = BotOwner_0.VoxelesPersonalData.CurVoxel;
		List<GroupPoint> points = curVoxel.Points;
		if (Ushort_0 == curVoxel.Id)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		foreach (GroupPoint item in points)
		{
			if (HashSet_0.Contains(item.Id))
			{
				continue;
			}
			if (NavMesh.CalculatePath(BotOwner_0.Position, item.Position, -1, NavMeshPath_0) && NavMeshPath_0.status == NavMeshPathStatus.PathComplete)
			{
				if (!item.IsFreeById(BotOwner_0.Id))
				{
					continue;
				}
				if (NavMesh.CalculatePath(item.Position, vector.Value, -1, NavMeshPath_1) && NavMeshPath_1.status == NavMeshPathStatus.PathComplete)
				{
					float num3 = GClass371.CalculatePathLength(NavMeshPath_1);
					if (num3 > 20f)
					{
						num2++;
						continue;
					}
					EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
					Vector3 mainTargetPosition = MainTargetPosition;
					if (GClass856.SqrDistance(BotOwner_0.Position, mainTargetPosition) > GClass856.SqrDistance(item.Position, mainTargetPosition))
					{
						continue;
					}
					float num4 = method_13(BotOwner_0.Position, mainTargetPosition);
					if (!(method_13(item.Position, mainTargetPosition) > num4 + 100f))
					{
						if (HashSet_0.Count > 15)
						{
							HashSet_0.Clear();
						}
						float v = method_10(num3);
						if ((goalEnemy == null || !goalEnemy.IsVisible) && num3 > DIST_TO_COVER_SPRINT_MIN)
						{
							bool val = GClass856.IsTrue100(v);
							BotOwner_0.Sprint(val);
						}
						Vector3[] corners = NavMeshPath_1.corners;
						List<Vector3> cornersFromIndex = BotOwner_0.Mover.GetCornersFromIndex(index);
						Vector3_0 = BotPathFinderClass.LinkWays(new List<Vector3[]>
						{
							corners,
							cornersFromIndex.ToArray()
						});
						Ushort_0 = curVoxel.Id;
						CustomNavigationPoint_0 = item.GetById(BotOwner_0.Id);
						HashSet_0.Add(item.Id);
						BotOwner_0.GoToByWay(NavMeshPath_0.corners);
						GroupPoint_0 = item;
						method_14(ETacticalMove.moveToSub);
						break;
					}
				}
				else
				{
					num2++;
				}
			}
			else
			{
				num++;
			}
		}
	}

	public float method_10(float length)
	{
		float num = Mathf.Clamp(length, DIST_TO_COVER_SPRINT_MIN, DIST_TO_COVER_SPRINT_MAX_OR_HIGHER);
		float t = 1f;
		if (Mathf.Abs(DIST_TO_COVER_SPRINT_MAX_OR_HIGHER - DIST_TO_COVER_SPRINT_MIN) > 0.001f)
		{
			t = (num - DIST_TO_COVER_SPRINT_MIN) / (DIST_TO_COVER_SPRINT_MAX_OR_HIGHER - DIST_TO_COVER_SPRINT_MIN);
		}
		return Mathf.Lerp(CHANCE_TO_COVER_SPRINT_ON_MIN_DIST_OR_LOWER, CHANCE_TO_COVER_SPRINT_ON_MAX_DIST_OR_HIGHER, t);
	}

	public float method_11(float lengthSqr)
	{
		float num = Mathf.Clamp(lengthSqr, DIST_TO_SPRINT_SQR_MIN, DIST_TO_SPRINT_SQR_MAX_OR_HIGHER);
		float t = 1f;
		if (Mathf.Abs(DIST_TO_SPRINT_SQR_MIN - DIST_TO_SPRINT_SQR_MAX_OR_HIGHER) > 0.001f)
		{
			t = (num - DIST_TO_SPRINT_SQR_MIN) / (DIST_TO_SPRINT_SQR_MAX_OR_HIGHER - DIST_TO_SPRINT_SQR_MIN);
		}
		return Mathf.Lerp(CHANCE_TO_SPRINT_SQR_ON_MIN_DIST_OR_LOWER, CHANCE_TO_SPRINT_SQR_ON_MAX_DIST_OR_HIGHER, t);
	}

	public float method_12(float lengthSqr)
	{
		float num = Mathf.Clamp(lengthSqr, DIST_TO_SPRINT_SQR_MIN, DIST_TO_SPRINT_SQR_MAX_OR_HIGHER);
		float t = 1f;
		if (Mathf.Abs(DIST_TO_SPRINT_SQR_MIN - DIST_TO_SPRINT_SQR_MAX_OR_HIGHER) > 0.001f)
		{
			t = (num - DIST_TO_SPRINT_SQR_MIN) / (DIST_TO_SPRINT_SQR_MAX_OR_HIGHER - DIST_TO_SPRINT_SQR_MIN);
		}
		return Mathf.Lerp(CHANCE_TO_STOP_SPRINT_SQR_ON_MIN_DIST_OR_LOWER, CHANCE_TO_STOP_SPRINT_SQR_ON_MAX_DIST_OR_HIGHER, t);
	}

	public float method_13(Vector3 from, Vector3 to)
	{
		if (NavMesh.CalculatePath(from, to, -1, NavMeshPath_3))
		{
			if (NavMeshPath_3.status != NavMeshPathStatus.PathComplete)
			{
				return 999999f;
			}
			return GClass371.CalculatePathLength(NavMeshPath_3);
		}
		return float.MaxValue;
	}

	public void method_14(ETacticalMove st)
	{
		if (EtacticalMove_0 != st)
		{
			if (EtacticalMove_0 == ETacticalMove.stayAtSub)
			{
				Float_4 = Time.time;
			}
			if (EtacticalMove_0 == ETacticalMove.moveToSub)
			{
				Float_3 = Time.time;
			}
			EtacticalMove_0 = st;
			BotOwner_0.LookData.ResetUpdateTime();
		}
	}

	public void method_15()
	{
		BotOwner_0.SetPose(1f);
		if (!BotOwner_0.HasPathAndNotComplete)
		{
			Float_2 = Time.time;
			if (!GClass856.IsTrue100(Float_12) && (!BotOwner_0.Memory.HaveEnemy || Time.time - BotOwner_0.Memory.GoalEnemy.TimeLastSeenReal >= ENEMY_VISIBLE_RECENTLY))
			{
				Float_7 = GClass856.GreateRandom(STAY_AT_COVER);
			}
			else
			{
				Float_7 = GClass856.GreateRandom(STAY_AT_COVER_WITH_ENEMY);
			}
			if (GClass856.IsTrue100(Float_13))
			{
				Float_9 = GClass856.GreateRandom(NEXT_COVER_SHORT);
			}
			else
			{
				Float_9 = GClass856.GreateRandom(NEXT_COVER_LONG);
			}
			Float_8 = Time.time;
			BotOwner_0.StopMove();
			Bool_2 = true;
			Bool_1 = true;
			if (GroupPoint_0 != null)
			{
				float num = GClass855.SqrDistHorizontal(GroupPoint_0.FirePosition, GroupPoint_0.Position);
				Bool_0 = num > 0.1f;
			}
			else
			{
				Bool_0 = false;
			}
			BotObserveDataClass.SetVectorToLook(-GroupPoint_0.WallDirection);
			method_14(ETacticalMove.stayAtSub);
		}
	}

	public void method_16()
	{
		GroupPoint groupPoint_ = GroupPoint_0;
		if (Bool_2)
		{
			BotOwner_0.StopMove();
			Bool_2 = false;
		}
		if (groupPoint_ != null && GClass855.SqrDistHorizontal(groupPoint_.FirePosition, groupPoint_.Position) > 0.1f)
		{
			BotOwner_0.SetPose(1f);
			Vector3 vector = Vector3.Lerp(groupPoint_.FirePosition, groupPoint_.Position, 0.3f);
			vector.y = groupPoint_.Position.y;
			float num = GClass855.SqrDistHorizontal(BotOwner_0.Position, vector);
			if (num > 0.010100001f && Float_6 < Time.time)
			{
				BotOwner_0.Sprint(val: false);
				Bool_1 = false;
				Float_6 = Time.time + 5f;
				NavMeshPathStatus num2 = BotOwner_0.GoToPoint(vector);
				BotOwner_0.Mover.SetReachDist(0.1f);
				if (num2 == NavMeshPathStatus.PathComplete)
				{
					Bool_2 = false;
				}
				else
				{
					Bool_2 = true;
				}
			}
			if (num < 0.1f)
			{
				Bool_2 = true;
			}
		}
		if (Time.time - Float_8 > Float_7)
		{
			method_14(ETacticalMove.moveToTarget);
			BotOwner_0.Tilt.Stop();
			method_17();
		}
	}

	public void method_17()
	{
		bool canRunNoAmmo;
		Vector3 centerPos = ((!BotOwner_0.Memory.IsInCover || BotOwner_0.Memory.GoalEnemy == null || BotOwner_0.LookSensor.EnoughDistToShoot(out canRunNoAmmo)) ? BotOwner_0.Transform.position : ((BifacialTransform_0.position + BotOwner_0.Memory.GoalEnemy.EnemyLastPosition) / 2f));
		if (BotOwner_0.Memory.CurCustomCoverPoint != null)
		{
			MoverToMainTarget(BotOwner_0.Memory.CurCustomCoverPoint);
		}
		else
		{
			BotOwner_0.BotAttackManager.TryPointGetting(centerPos, CoverShootType.shoot, LocalBotSettingsProviderClass.Core.START_DIST_TO_COV, CoverSearchType.distToBot, BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true), MoverToMainTarget, delegate
			{
			});
		}
	}
}
