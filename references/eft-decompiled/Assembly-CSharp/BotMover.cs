using System;
using System.Collections.Generic;
using EFT;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

public abstract class BotMover : GClass429
{
	public const float MaxSprintSpeed = 2f;

	public const float SlowEndDist = 0.5f;

	public const float SlowPeriod = 0.5f;

	[NonSerialized]
	public const float MAX_DIST_OUT_OF_NAVMESH = 7f;

	[NonSerialized]
	public const float MAX_Y_DELTA_PER_STEP = 0.5f;

	public GClass492 LocalAvoidance;

	[NonSerialized]
	public Player Player;

	[NonSerialized]
	public Vector3 PositionOnWay_1 = Vector3.zero;

	[NonSerialized]
	public float LastError;

	[NonSerialized]
	public float LastTimePosChanged;

	[NonSerialized]
	public Vector3 LastPos;

	[NonSerialized]
	public Vector3 DirCurPoint_1 = Vector3.one;

	[NonSerialized]
	public Vector3 CurrentTargetPoint = Vector3.one;

	[NonSerialized]
	public float EndSlowTime;

	[NonSerialized]
	public bool StartSlowActivated;

	[NonSerialized]
	public bool FirstTurnComplete_1;

	[NonSerialized]
	public bool FirstTurnBigSpeed_1;

	[NonSerialized]
	public GClass548 MoverStateMachine;

	[NonSerialized]
	public GClass496 DebugerWay;

	[NonSerialized]
	public GClass496 Debuger;

	[NonSerialized]
	public Vector3 OffsetCastUp = Vector3.up * 0.2f;

	[NonSerialized]
	public float PoseSpeedChangeFactor = 0.2f;

	[NonSerialized]
	public float PauseEndTime;

	[NonSerialized]
	public float SprintStopEnd;

	[NonSerialized]
	public float NextCheckVoxel;

	[NonSerialized]
	public bool LinkedToNavmeshInitially;

	[NonSerialized]
	public Vector3 PrevSuccessLinkedFrom_1;

	[NonSerialized]
	public float PrevPosLinkedTime_1 = -1f;

	[NonSerialized]
	public BotPathFinderClass PathFinder;

	[NonSerialized]
	public Vector3 PrevSuccsesLinkedFrom;

	[NonSerialized]
	public Vector3 PrevLinkPos;

	[NonSerialized]
	public BotMoverDebugGameObject DebugHolder;

	[NonSerialized]
	public Vector3 LastGoodCastPoint;

	[NonSerialized]
	public float LastGoodCastPointTime;

	[NonSerialized]
	public NavGraphVoxelSimple LastVoxel;

	[NonSerialized]
	public Vector3 PositionOnWayCasted;

	[NonSerialized]
	public Vector3 PrevFramePos;

	[NonSerialized]
	public float PrevFrameTime;

	[NonSerialized]
	public float LastTimeCheckNeedMove;

	[NonSerialized]
	public Vector3 PrevOffsetGoodCasted;

	[NonSerialized]
	public float PrevOffsetGoodCastedTime;

	[NonSerialized]
	public const float RELINK_SDIST = 0.0025000002f;

	[NonSerialized]
	public float Next;

	[NonSerialized]
	public bool AllowTeleport = true;

	public PathControllerClass ActualPathController => MoverStateMachine.CurrentMoverState.PathController;

	public BotPathFinderClass ActualPathFinder => MoverStateMachine.CurrentMoverState.PathFinder;

	public EBotMoverState CurrentState => MoverStateMachine.CurrentMoverState.State;

	public string CallstackPathComeFrom => ActualPathController.CallstackPathComeFrom;

	public Vector3 PrevSuccessLinkedFrom => PrevSuccessLinkedFrom_1;

	public float PrevPosLinkedTime => PrevPosLinkedTime_1;

	public string String_0 => ActualPathController.PathStack;

	[field: NonSerialized]
	public bool Sprinting { get; set; }

	[field: NonSerialized]
	public float TargetPose { get; set; }

	[field: NonSerialized]
	public bool IsMoving { get; set; }

	public Vector3? TargetPoint => ActualPathController.TargetPoint;

	public bool HasPathAndNoComplete => ActualPathController.HavePath;

	public Action<int, Vector3[]> onPathCornerChanged
	{
		get
		{
			return ActualPathController.onPathCornerChanged;
		}
		set
		{
			ActualPathController.onPathCornerChanged = value;
		}
	}

	public Vector3 DirCurPoint => DirCurPoint_1;

	public Vector3 PositionOnWay => PositionOnWayInner;

	public float DistDestination => ActualPathController.PlayerRemainingDist;

	[field: NonSerialized]
	public float SDistDestination { get; set; }

	[field: NonSerialized]
	public Vector3 NormDirCurPoint { get; set; } = Vector3.one;

	public Vector3 RealDestPoint => CurrentTargetPoint;

	[field: NonSerialized]
	public bool Pause { get; set; }

	public float RemainPause => PauseEndTime - Time.time;

	[field: NonSerialized]
	public float DestMoveSpeed { get; set; }

	public virtual bool IsImpostorWorks => false;

	public virtual bool Blocked => false;

	public bool NoSprint => SprintStopEnd > Time.time;

	public Vector3 CurrentCornerPoint => ActualPathController.CurrentCornerPoint;

	public bool ShallSlowAtStart => StartSlowActivated;

	public bool FirstTurnComplete => FirstTurnComplete_1;

	public bool FirstTurnBigSpeed => FirstTurnBigSpeed_1;

	public float LastPathSetTime => ActualPathController.LastPathSetTime;

	public float LastEndPathTime => ActualPathController.LastEndPathTime;

	public Vector3 PositionOnWayInner
	{
		get
		{
			return PositionOnWay_1;
		}
		set
		{
			PositionOnWay_1 = value;
		}
	}

	public event Action<float> OnPoseChange;

	public event Action<bool> OnWayChanged;

	public static BotMover Create(BotOwner owner, AICoversData covers)
	{
		Player getPlayer = owner.GetPlayer;
		if (owner.Profile.Info.Settings.Role == WildSpawnType.shooterBTR)
		{
			return new GClass493(owner, getPlayer, covers);
		}
		return new GClass494(owner, getPlayer, covers);
	}

	public BotMover(BotOwner owner, Player player, AICoversData covers)
		: base(owner)
	{
		LocalAvoidance = new GClass492(owner);
		Debuger = new GClass496(owner);
		DebugerWay = new GClass496(owner);
		Player = player;
		MoverStateMachine = new GClass548(owner, this, covers);
		BotOwner_0.GetPlayer.MovementContext.CanSlope = false;
		MoverStateMachine.CurrentMoverState.PathController.OnWayChanged += WayChanged;
	}

	public virtual void WayChanged(bool obj)
	{
		this.OnWayChanged?.Invoke(obj);
	}

	public Vector3 LastDestination()
	{
		return ActualPathController.LastDestination();
	}

	public void SetReachDist(float reachDist)
	{
		ActualPathController.SetReachDist(reachDist);
	}

	public bool IsComeTo(float dist2check, bool onCover, CustomNavigationPoint p = null)
	{
		float num = dist2check * dist2check;
		if (p == null)
		{
			p = BotOwner_0.Memory.CurCustomCoverPoint;
		}
		if (onCover && p != null)
		{
			Vector3 vector = BotOwner_0.Position - p.Position;
			if (Mathf.Abs(vector.y) < BotOwner_0.Settings.FileSettings.Move.Y_APPROXIMATION)
			{
				vector.y = 0f;
			}
			SDistDestination = vector.sqrMagnitude;
			return SDistDestination < num;
		}
		return DistDestination < num;
	}

	public bool IsPointOnCurrentWay(Vector3 point, float maxDist)
	{
		return ActualPathController.IsPointOnCurrentWay(point, maxDist);
	}

	public virtual void Activate()
	{
		BotOwner_0.DoorOpener.OnEndInteract += method_13;
		method_11(BotOwner_0.VoxelesPersonalData.GetVoxelSafe(BotOwner_0.Position));
		PositionOnWayInner = BotOwner_0.Position;
	}

	public virtual void ManualUpdate()
	{
		method_19();
		method_18();
		method_14();
		LocalAvoidance.ManualUpdate();
		if (DebugBotData.UseDebugData && DebugBotData.Instance.CheckStuck)
		{
			method_2();
		}
	}

	public void method_0()
	{
	}

	public void method_1()
	{
	}

	public void method_2()
	{
		Vector3 position = BotOwner_0.Position;
		if ((LastPos - position).sqrMagnitude > 0.1f)
		{
			LastPos = position;
			LastTimePosChanged = Time.time;
		}
		if (Time.time - LastTimePosChanged > 560f && Time.time - LastError > 30f)
		{
			LastError = Time.time;
			Debug.LogError($"bot:{BotOwner_0.Id} looksLike stuck at:{position}");
		}
	}

	public void ManualFixedUpdate()
	{
		method_16();
		method_12();
	}

	public void method_3()
	{
		if (!HasPathAndNoComplete && Time.time - LastTimeCheckNeedMove > 0.2f)
		{
			float magnitude = (BotOwner_0.Position - ActualPathController.StopPos).magnitude;
			if (magnitude > 0.2f)
			{
				LastTimeCheckNeedMove = Time.time;
				Debug.DrawRay(ActualPathController.StopPos, Vector3.up * 20f, Color.red);
				Debug.DrawRay(BotOwner_0.Position, Vector3.up * 20f, Color.green);
				Debug.LogError("move after stop:" + magnitude.ToString("0.00"));
			}
		}
	}

	public void LateFixedUpdate()
	{
	}

	public Vector3 ModifVectorByWallSide(Vector3 directionLook, Vector3 toWallVector)
	{
		if (GClass855.IsAngLessNormalized(GClass855.NormalizeFastSelf(directionLook), toWallVector, 0.5f))
		{
			int side = GClass856.RandomSing();
			return BotOwner_0.LookData.RotateWallBySide(BotOwner_0.Memory.CurCustomCoverPoint, side);
		}
		return directionLook;
	}

	public void GoToPointNoWay(Vector3? v)
	{
		SetTargetMoveSpeed(1f);
		MoverStateMachine.DefaultMoverState.PathController.GoToPointNoWay(v);
	}

	public virtual void GoToByWay(Vector3[] way, float reachDist)
	{
		MoverStateMachine.DefaultMoverState.PathController.GoToByWay(way, reachDist);
	}

	public void DoProne(bool val)
	{
		if (val)
		{
			Stop();
		}
		BotOwner_0.GetPlayer.MovementContext.IsInPronePose = val;
	}

	public void MovementPause(float pauseTime, bool pauseOnlyIfNot = true)
	{
		if (!(Pause && pauseOnlyIfNot))
		{
			Pause = true;
			PauseEndTime = Time.time + pauseTime;
		}
	}

	public void MovementResume()
	{
		Pause = false;
	}

	public void Stop()
	{
		IsMoving = false;
		ActualPathController.Stop();
	}

	public void SprintPause(float period)
	{
		SprintStopEnd = Time.time + period;
	}

	public float ComputePathLengthToPoint(Vector3 targetPoint)
	{
		float num = GClass371.CalculatePathLength(CalcPath(targetPoint));
		if (!(num < 0.001f))
		{
			return num;
		}
		return float.MaxValue;
	}

	public virtual void SetPose(float targetPose)
	{
		TargetPose = targetPose;
	}

	public virtual void Sprint(bool val, bool withDebugCallback = true)
	{
		if (Sprinting != val)
		{
			Sprinting = val;
			if (Sprinting)
			{
				BotOwner_0.SetTargetMoveSpeed(1f);
			}
			if (NoSprint)
			{
				Player.EnableSprint(enable: false);
			}
			else
			{
				Player.EnableSprint(val);
			}
		}
	}

	public void SetTargetMoveSpeed(float target)
	{
		DestMoveSpeed = target;
	}

	public Vector3[] CalcPath(Vector3 position)
	{
		NavMeshPath navMeshPath = new NavMeshPath();
		if (NavMesh.CalculatePath(BotOwner_0.Transform.position, position, -1, navMeshPath))
		{
			return navMeshPath.corners;
		}
		if (NavMesh.SamplePosition(BotOwner_0.Transform.position, out var hit, 7f, -1))
		{
			_ = new Vector3[2]
			{
				BotOwner_0.Transform.position,
				hit.position
			};
			return navMeshPath.corners;
		}
		return null;
	}

	public virtual void OnDrawGizmosPrevWay()
	{
		Debuger.OnDrawGizmosPrevWay();
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(BotOwner_0.Position, 0.13f);
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(PositionOnWay_1, 0.15f);
	}

	public void DrawVisionLines()
	{
		GClass810.DrawArrow(BotOwner_0.MyHead.position, GClass855.NormalizeFastSelf(BotOwner_0.Steering.LookDirection) * 1.6f, Color.white);
		GClass810.DrawArrow(BotOwner_0.MyHead.position - Vector3.up * 0.05f, GClass855.NormalizeFastSelf(NormDirCurPoint) * 1.6f, Color.green);
	}

	public NavMeshPathStatus GoToPoint(CustomNavigationPoint targetPoint, bool slowAtTheEnd = false, bool getUpWithCheck = false)
	{
		return MoverStateMachine.DefaultMoverState.PathFinder.GoToPosition(targetPoint, slowAtTheEnd, 0.5f, getUpWithCheck);
	}

	public NavMeshPathStatus GoToPoint(Vector3 pos, bool slowAtTheEnd, float reachDist, bool getUpWithCheck = false, bool mustHaveWay = true, bool onlyShortTrie = false, bool force = false)
	{
		return MoverStateMachine.DefaultMoverState.PathFinder.GoToPosition(pos, slowAtTheEnd, reachDist, getUpWithCheck, mustHaveWay, onlyShortTrie, force);
	}

	public NavMeshPathStatus CurrentStateGoToPoint(Vector3 pos, bool slowAtTheEnd, float reachDist, bool getUpWithCheck = false, bool mustHaveWay = true, bool onlyShortTrie = false, bool force = false)
	{
		return MoverStateMachine.CurrentMoverState.PathFinder.GoToPosition(pos, slowAtTheEnd, reachDist, getUpWithCheck, mustHaveWay, onlyShortTrie, force);
	}

	public void SetMoverState(EBotMoverState state)
	{
		MoverStateMachine.SetState(state);
	}

	public Vector3 LastTargetPoint(float distCoef)
	{
		return ActualPathController.LastTargetPoint(distCoef);
	}

	public Vector3 PrevCorner()
	{
		return ActualPathController.PrevCorner;
	}

	public void DrawSelectedGizmosWay()
	{
		Gizmos.color = Color.magenta;
		Gizmos.DrawLine(BotOwner_0.Transform.position, PositionOnWay);
		ActualPathController.DrawSelectedGizmosWay();
		ActualPathController.DrawSelectedGizmosLastWay();
	}

	public void DebugDrawLastPath()
	{
		ActualPathController.DebugDrawLastWay();
	}

	public void RecalcWay()
	{
		if (ActualPathController.CurPath != null)
		{
			GInterface9 targetPoint = ActualPathController.CurPath.TargetPoint;
			GoToPoint(targetPoint.Position, slowAtTheEnd: false, 1f);
		}
	}

	public virtual void CantFindWayErrorTraceLog()
	{
		Debuger.CantFindWayErrorTraceLog();
		DebugerWay.CantFindWayErrorTraceLog();
	}

	public abstract bool CheckCornerIndexByReachDist(float distCur, Vector3 position);

	public abstract void SetLastContex(float remainDist, Vector3 directionMove, Vector3 targetPos);

	public abstract bool CheckIsOnPoint(Vector3 position);

	public EBotLinkResult SetPlayerToNavMesh(Vector3 castPoint)
	{
		EBotLinkResult debugType = EBotLinkResult.fail;
		if (Time.time - LastGoodCastPointTime < 1f)
		{
			castPoint = new Vector3(castPoint.x, LastGoodCastPoint.y, castPoint.z);
		}
		if (method_8(castPoint, 0.4f, out var hitPos))
		{
			LastGoodCastPoint = hitPos.Value;
			LastGoodCastPointTime = Time.time;
			debugType = method_7(hitPos);
		}
		if (debugType != EBotLinkResult.complete)
		{
			bool num = method_6(castPoint, out debugType);
			GClass810.DebugPoint(castPoint, Color.red, 0.3f, 1f);
			if (num)
			{
				debugType = EBotLinkResult.extraConnect;
				PrevLinkPos = castPoint;
				goto IL_0257;
			}
			debugType = EBotLinkResult.superFail;
			_ = BotOwner_0.Position;
			GClass369.DebugDrawArc(castPoint, PositionOnWay, 1.3f, 4f, Color.cyan);
			_ = (castPoint - PositionOnWay).magnitude;
			method_9(castPoint);
			PositionOnWayInner = method_4(castPoint);
			BotOwner_0.Mover.AllowTeleport();
			BotOwner_0.Mover.Teleport(PositionOnWayInner);
			BotOwner_0.Mover.method_9(PositionOnWayInner);
			BotOwner_0.Brain.BaseBrain.CalcActionNextFrame();
			BotOwner_0.Mover.LocalAvoidance.DropOffset();
			BotOwner_0.Mover.InertionUpdateOnCoef(0f);
			PrevLinkPos = castPoint;
		}
		else
		{
			PrevLinkPos = castPoint;
			switch (debugType)
			{
			case EBotLinkResult.fail:
			case EBotLinkResult.superFail:
				break;
			case EBotLinkResult.complete:
			case EBotLinkResult.extraConnect:
				goto IL_0257;
			default:
				goto IL_0270;
			}
		}
		if (PrevPosLinkedTime_1 > 0.1f)
		{
			_ = (PrevSuccessLinkedFrom_1 - castPoint).magnitude;
			if (DebugBotData.Instance != null)
			{
				_ = DebugBotData.UseDebugData;
			}
			if (BotOwner_0.Mover.ActualPathController.TargetPoint.HasValue)
			{
				BotOwner_0.Mover.GoToPoint(BotOwner_0.Mover.ActualPathController.TargetPoint.Value, slowAtTheEnd: false, -1f);
			}
			GClass810.DebugPoint(castPoint, 0.1f, 1.2f, depthTest: true, Color.red);
			GClass810.DebugPoint(PrevSuccessLinkedFrom_1, 0.15f, 1.2f, depthTest: true, Color.white);
		}
		goto IL_0270;
		IL_0270:
		return debugType;
		IL_0257:
		LinkedToNavmeshInitially = true;
		PrevSuccessLinkedFrom_1 = castPoint;
		PrevPosLinkedTime_1 = Time.time;
		goto IL_0270;
	}

	public Vector3 method_4(Vector3 castPoint)
	{
		if (PrevSuccessLinkedFrom_1.sqrMagnitude > 0.1f)
		{
			Vector3 normalized = (BotOwner_0.Mover.ActualPathController.CurrentCornerPoint - BotOwner_0.Mover.ActualPathController.PrevCorner).normalized;
			if (NavMesh.SamplePosition(PrevSuccessLinkedFrom_1 + normalized, out var _, 100f, -1))
			{
				return PrevSuccessLinkedFrom_1 + normalized;
			}
			if (NavMesh.SamplePosition(PrevSuccessLinkedFrom_1 + normalized * 2f, out var _, 100f, -1))
			{
				return PrevSuccessLinkedFrom_1 + normalized * 2f;
			}
			if (NavMesh.SamplePosition(castPoint, out var hit3, 100f, -1))
			{
				return hit3.position;
			}
			if (NavMesh.SamplePosition(castPoint + Vector3.up * 50f, out var hit4, 100f, -1))
			{
				return hit4.position;
			}
		}
		else
		{
			if (NavMesh.SamplePosition(castPoint, out var hit5, 100f, -1))
			{
				return hit5.position;
			}
			if (NavMesh.SamplePosition(castPoint + Vector3.up * 50f, out var hit6, 100f, -1))
			{
				return hit6.position;
			}
		}
		return PositionOnWayInner;
	}

	public virtual Vector3 InertionOffset()
	{
		return Vector3.zero;
	}

	public float UseFuncAcceleration(float x)
	{
		return BotOwner_0.Settings.Curv.MoveStartCurv.Evaluate(x);
	}

	public void method_5(Vector3 obj)
	{
	}

	public bool method_6(Vector3 castPoint, out EBotLinkResult debugType)
	{
		if (Time.time - LastGoodCastPointTime < 1f && method_8(new Vector3(castPoint.x, LastGoodCastPoint.y, castPoint.z), 2f, out var hitPos))
		{
			LastGoodCastPoint = hitPos.Value;
			LastGoodCastPointTime = Time.time;
			debugType = method_7(hitPos);
			return true;
		}
		debugType = EBotLinkResult.superFail;
		return false;
	}

	public EBotLinkResult method_7(Vector3? hitPos)
	{
		return method_9(hitPos.Value);
	}

	public bool method_8(Vector3 castPoint, float sampleRad, out Vector3? hitPos)
	{
		bool flag = false;
		flag = NavMesh.SamplePosition(castPoint, out var hit, sampleRad, -1);
		if (hit.position.y - castPoint.y > 0.5f)
		{
			flag = false;
		}
		hitPos = hit.position;
		return flag;
	}

	public EBotLinkResult method_9(Vector3 pos)
	{
		PositionOnWayCasted = pos;
		EBotLinkResult eBotLinkResult = EBotLinkResult.fail;
		Vector3 zero = Vector3.zero;
		zero = InertionOffset() + LocalAvoidance.NotLinkedOffset;
		Vector3 posiblePos = PositionOnWayCasted + zero;
		eBotLinkResult = method_10(posiblePos, withExtra: false);
		if (eBotLinkResult != EBotLinkResult.complete)
		{
			posiblePos = ((!(Time.time - PrevOffsetGoodCastedTime < 1f)) ? PositionOnWayCasted : (PositionOnWayCasted + PrevOffsetGoodCasted));
			eBotLinkResult = method_10(posiblePos, withExtra: true);
		}
		else
		{
			PrevOffsetGoodCasted = zero;
			PrevOffsetGoodCastedTime = Time.time;
		}
		float magnitude = zero.magnitude;
		float magnitude2 = (PositionOnWayCasted - BotOwner_0.Transform.position).magnitude;
		if (magnitude > 0f && eBotLinkResult == EBotLinkResult.complete)
		{
			float coef = magnitude2 / magnitude;
			LocalAvoidance.UpdateTotal(coef);
			InertionUpdateOnCoef(coef);
		}
		else
		{
			LocalAvoidance.UpdateTotal(0f);
			InertionUpdateOnCoef(0f);
			PrevOffsetGoodCasted = Vector3.zero;
		}
		NavGraphVoxelSimple voxelSafe = BotOwner_0.VoxelesPersonalData.GetVoxelSafe(PositionOnWayInner);
		if (voxelSafe != LastVoxel)
		{
			LastVoxel.BotsInside.Remove(BotOwner_0);
			method_11(voxelSafe);
		}
		return eBotLinkResult;
	}

	public virtual void InertionUpdateOnCoef(float coef)
	{
	}

	public EBotLinkResult method_10(Vector3 posiblePos, bool withExtra)
	{
		if (GClass858.IsAnyComponentNaN(posiblePos))
		{
			Debug.LogError("smt is nan");
			return EBotLinkResult.superFail;
		}
		if (NavMesh.SamplePosition(posiblePos, out var hit, 0.2f, -1))
		{
			if (Physics.Raycast(new Ray(hit.position, Vector3.down), out var hitInfo, 0.2f, LayerMaskClass.PlayerCollisionsMask))
			{
				BotOwner_0.Transform.position = hitInfo.point;
				return EBotLinkResult.complete;
			}
			if (withExtra)
			{
				BotOwner_0.Transform.position = hit.position;
				Debug.DrawLine(hit.position, PositionOnWay_1, Color.blue, 5f);
				return EBotLinkResult.extraConnect;
			}
		}
		return EBotLinkResult.fail;
	}

	public void method_11(NavGraphVoxelSimple centerVoxel)
	{
		LastVoxel = centerVoxel;
		LastVoxel.BotsInside.Add(BotOwner_0);
	}

	public void AllowTeleport()
	{
		this.AllowTeleport = true;
	}

	public void method_12()
	{
		Vector3 position = BotOwner_0.Position;
		Vector3 vector = PrevSuccessLinkedFrom_1 - position;
		float sqrMagnitude = vector.sqrMagnitude;
		float num = 0.09f;
		if (HasPathAndNoComplete && !this.AllowTeleport && LinkedToNavmeshInitially)
		{
			if (vector.sqrMagnitude > num)
			{
				PositionOnWayInner = PrevSuccessLinkedFrom_1;
				BotOwner_0.Mover.method_9(PrevSuccessLinkedFrom_1);
			}
			if (sqrMagnitude > num * 4f && LinkedToNavmeshInitially)
			{
				PositionOnWayInner = PrevSuccessLinkedFrom_1;
				BotOwner_0.Mover.method_9(PrevSuccessLinkedFrom_1);
			}
		}
		else if (sqrMagnitude > 0.01f && Next < Time.time)
		{
			Next = Time.time + 0.3f;
			EBotLinkResult eBotLinkResult = SetPlayerToNavMesh(position);
			if (eBotLinkResult != EBotLinkResult.complete && eBotLinkResult != EBotLinkResult.extraConnect)
			{
				Next = Time.time + 2f;
			}
			else
			{
				LinkedToNavmeshInitially = true;
				PositionOnWayInner = BotOwner_0.Position;
				BotOwner_0.Mover.LocalAvoidance.DropOffset();
			}
			this.AllowTeleport = false;
		}
	}

	public void method_13()
	{
		ActualPathController.OnEndInteract();
	}

	public void method_14()
	{
		if (Pause)
		{
			Pause = PauseEndTime > Time.time;
		}
	}

	public void method_15(float val)
	{
		Player.MovementContext.SprintSpeed = val;
	}

	public void method_16()
	{
		if (!Pause && ActualPathController.HavePath)
		{
			if (!ActualPathController.CheckShouldMove())
			{
				IsMoving = false;
				BotOwner_0.AimingManager.CurrentAiming.Move();
				return;
			}
			if (Sprinting && !NoSprint)
			{
				Player.EnableSprint(enable: true);
			}
			CheckForRecalcPath();
			Vector3 vector = ActualPathController.CurrentCorner();
			Vector3 v = vector - PositionOnWayInner;
			StartSlowActivated = StartSlowActivated && Time.time < EndSlowTime && Sprinting;
			if (StartSlowActivated)
			{
				float x = 1f - (EndSlowTime - Time.time) / 0.5f;
				float num = UseFuncAcceleration(x);
				method_15(num * 2f);
			}
			else
			{
				method_15(2f);
			}
			v.y = 0f;
			if (v.magnitude < 0.01f)
			{
				ActualPathController.IncCornerIndex();
				if (!ActualPathController.HavePath || ActualPathController.IsLast())
				{
					IsMoving = false;
					return;
				}
				vector = ActualPathController.CurrentCorner();
			}
			CurrentTargetPoint = vector;
			DirCurPoint_1 = CurrentTargetPoint - PositionOnWayInner;
			if (StartSlowActivated && !FirstTurnComplete_1)
			{
				if (!GClass855.IsAngLessNormalized(GClass855.NormalizeFastSelf(v), BotOwner_0.LookDirection, 0.7071068f))
				{
					return;
				}
				FirstTurnComplete_1 = true;
				FirstTurnBigSpeed_1 = GClass855.IsAngLessNormalized(GClass855.NormalizeFastSelf(v), BotOwner_0.LookDirection, 0f);
			}
			if (!CheckIsOnPoint(PositionOnWayInner))
			{
				return;
			}
			float num2 = ((!(DirCurPoint_1.y < 0.5f)) ? Mathf.Sqrt(DirCurPoint_1.x * DirCurPoint_1.x + DirCurPoint_1.y * DirCurPoint_1.y + DirCurPoint_1.z * DirCurPoint_1.z) : Mathf.Sqrt(DirCurPoint_1.x * DirCurPoint_1.x + DirCurPoint_1.z * DirCurPoint_1.z));
			if (!CheckCornerIndexByReachDist(num2, PositionOnWayInner))
			{
				return;
			}
			float playerRemainingDist = ActualPathController.PlayerRemainingDist;
			float speed = Player.Speed;
			Vector3 v2 = GClass855.NormalizeFastSelf(DirCurPoint_1);
			NormDirCurPoint = GClass855.NormalizeFastSelf(v2);
			BotOwner_0.Memory.botObserveData.SetVector();
			if (Sprinting)
			{
				if (!StartSlowActivated)
				{
					if (playerRemainingDist < 0.5f)
					{
						float time = playerRemainingDist / 0.5f;
						float num3 = BotOwner_0.Settings.Curv.MoveStartCurv.Evaluate(time);
						method_15(num3 * 2f);
					}
					else
					{
						Player.MovementContext.SprintSpeed = 2f;
					}
				}
			}
			else
			{
				float num4 = ((!ActualPathFinder.SlowAtTheEnd) ? ((playerRemainingDist > BotOwner_0.Settings.FileSettings.Move.BASESTART_SLOW_DIST) ? Player.Speed : (playerRemainingDist / BotOwner_0.Settings.FileSettings.Move.SLOW_COEF)) : ((playerRemainingDist > BotOwner_0.Settings.FileSettings.Move.START_SLOW_DIST) ? Player.Speed : (playerRemainingDist / BotOwner_0.Settings.FileSettings.Move.SLOW_COEF)));
				float speedDelta = num4 - speed;
				Player.ChangeSpeed(speedDelta);
			}
			BotOwner_0.AimingManager.CurrentAiming.Move(speed);
			IsMoving = true;
			Player.CharacterController.SetSteerDirection(NormDirCurPoint);
			SetLastContex(num2, DirCurPoint_1, CurrentTargetPoint);
			if (NextCheckVoxel < Time.time)
			{
				NextCheckVoxel = Time.time + 0.5f;
				BotOwner_0.AIData.SetPosToVoxel(PositionOnWayInner);
			}
			Vector2 direction = method_17();
			Player.Move(direction);
		}
		else
		{
			IsMoving = false;
			BotOwner_0.AimingManager.CurrentAiming.Move();
		}
	}

	public virtual void CheckForRecalcPath()
	{
	}

	public Vector2 method_17()
	{
		Vector2 vector = new Vector2(NormDirCurPoint.x, NormDirCurPoint.z);
		Vector3 v = Quaternion.Euler(0f, 0f, Player.Rotation.x) * vector;
		v = GClass855.NormalizeFastSelf(v);
		return new Vector2(v.x, v.y);
	}

	public void method_18()
	{
		if (!BotOwner_0.BotLay.IsLay)
		{
			float num = Math.Abs(TargetPose - Player.PoseLevel);
			if (num >= float.Epsilon)
			{
				this.OnPoseChange?.Invoke(num);
				Player.ChangePose(PoseSpeedChangeFactor * (TargetPose - Player.PoseLevel));
			}
		}
	}

	public void method_19()
	{
		if (BotOwner_0.DoorOpener.NearDoor)
		{
			DestMoveSpeed = 0.5f;
		}
		float num = DestMoveSpeed - Player.Speed;
		if (Math.Abs(num) >= float.Epsilon)
		{
			Player.ChangeSpeed(num);
		}
	}

	public virtual void Dispose()
	{
		LastVoxel.BotsInside.Remove(BotOwner_0);
		MoverStateMachine.CurrentMoverState.PathController.OnWayChanged -= WayChanged;
		BotOwner_0.DoorOpener.OnEndInteract -= method_13;
	}

	public void SetPointOnWay(Vector3 point)
	{
		PositionOnWay_1 = point;
	}

	public Vector3[] GetWayPoints(int maxWayPoints)
	{
		return ActualPathController.GetWayPoints(maxWayPoints);
	}

	public bool TryRelacePathAround(Vector3 point, [CanBeNull] List<Vector3> subAvoids, out BotCurrentPathAbstractClass curPath)
	{
		return ActualPathController.TryReplacePathAround(point, subAvoids, out curPath);
	}

	public Vector3? FindPointFarThan(float dist, out int index)
	{
		return ActualPathController.FindPointFarThan(dist, out index);
	}

	public List<Vector3> GetCornersFromIndex(int index)
	{
		return ActualPathController.GetCornersFromIndex(index);
	}

	public void Teleport(Vector3 rPosition)
	{
		AllowTeleport();
		BotOwner_0.GetPlayer.Teleport(rPosition);
		PositionOnWayInner = rPosition;
	}
}
