using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using EFT.Interactive;
using UnityEngine;

public class BotDoorOpener
{
	[CompilerGenerated]
	public class Class184
	{
		public BotDoorOpener botDoorOpener_0;

		public Vector3 pointLookForward;

		public Vector3 pointAfterDoor;

		public Vector3 firstLookDirection;

		public float magnitude;

		public Vector3 secondLookDirection;

		public Action action_0;

		public Action action_1;

		public Action action_2;

		public void method_0()
		{
			if (botDoorOpener_0.Owner.Memory.HaveEnemy)
			{
				botDoorOpener_0.Owner.Mover.SetMoverState(EBotMoverState.Default);
				botDoorOpener_0.EnteringDoorSequence = false;
				return;
			}
			botDoorOpener_0.Owner.Steering.LookToPoint(pointLookForward);
			botDoorOpener_0.Owner.Mover.CurrentStateGoToPoint(pointAfterDoor, slowAtTheEnd: false, botDoorOpener_0.Owner.Settings.FileSettings.Move.REACH_DIST, getUpWithCheck: false, mustHaveWay: false, onlyShortTrie: false, force: true);
			botDoorOpener_0.Owner.AITaskManager.RegisterDelayedTask(1.5f, delegate
			{
				if (!botDoorOpener_0.Owner.IsDead)
				{
					if (botDoorOpener_0.Owner.Memory.HaveEnemy)
					{
						botDoorOpener_0.Owner.Mover.SetMoverState(EBotMoverState.Default);
						botDoorOpener_0.EnteringDoorSequence = false;
					}
					else
					{
						if (botDoorOpener_0.Owner.Mover.IsComeTo(botDoorOpener_0.Owner.Settings.FileSettings.Move.REACH_DIST, onCover: false))
						{
							botDoorOpener_0.Owner.Mover.Stop();
						}
						Physics.Raycast(new Ray(pointAfterDoor, firstLookDirection - pointAfterDoor), out var hitInfo, magnitude, LayerMaskClass.HighPolyWithTerrainMask);
						float distance = hitInfo.distance;
						float delay = 0f;
						if (distance > 3f)
						{
							delay = 1.2f;
							botDoorOpener_0.Owner.Steering.LookToPoint(firstLookDirection);
						}
						botDoorOpener_0.Owner.AITaskManager.RegisterDelayedTask(delay, delegate
						{
							if (!botDoorOpener_0.Owner.IsDead)
							{
								if (botDoorOpener_0.Owner.Memory.HaveEnemy)
								{
									botDoorOpener_0.Owner.Mover.SetMoverState(EBotMoverState.Default);
									botDoorOpener_0.EnteringDoorSequence = false;
								}
								else
								{
									if (botDoorOpener_0.Owner.Mover.IsComeTo(botDoorOpener_0.Owner.Settings.FileSettings.Move.REACH_DIST, onCover: false))
									{
										botDoorOpener_0.Owner.Mover.Stop();
									}
									Physics.Raycast(new Ray(pointAfterDoor, secondLookDirection - pointAfterDoor), out var hitInfo2, magnitude, LayerMaskClass.HighPolyWithTerrainMask);
									float distance2 = hitInfo2.distance;
									float delay2 = 0f;
									if (distance2 > 3f)
									{
										delay2 = 1.2f;
										botDoorOpener_0.Owner.Steering.LookToPoint(secondLookDirection);
									}
									botDoorOpener_0.Owner.AITaskManager.RegisterDelayedTask(delay2, delegate
									{
										if (!botDoorOpener_0.Owner.IsDead)
										{
											botDoorOpener_0.EnteringDoorSequence = false;
											botDoorOpener_0.Owner.Mover.SetMoverState(EBotMoverState.Default);
											botDoorOpener_0.Owner.Steering.LookToMovingDirection();
										}
									});
								}
							}
						});
					}
				}
			});
		}

		public void method_1()
		{
			if (botDoorOpener_0.Owner.IsDead)
			{
				return;
			}
			if (botDoorOpener_0.Owner.Memory.HaveEnemy)
			{
				botDoorOpener_0.Owner.Mover.SetMoverState(EBotMoverState.Default);
				botDoorOpener_0.EnteringDoorSequence = false;
				return;
			}
			if (botDoorOpener_0.Owner.Mover.IsComeTo(botDoorOpener_0.Owner.Settings.FileSettings.Move.REACH_DIST, onCover: false))
			{
				botDoorOpener_0.Owner.Mover.Stop();
			}
			Physics.Raycast(new Ray(pointAfterDoor, firstLookDirection - pointAfterDoor), out var hitInfo, magnitude, LayerMaskClass.HighPolyWithTerrainMask);
			float distance = hitInfo.distance;
			float delay = 0f;
			if (distance > 3f)
			{
				delay = 1.2f;
				botDoorOpener_0.Owner.Steering.LookToPoint(firstLookDirection);
			}
			botDoorOpener_0.Owner.AITaskManager.RegisterDelayedTask(delay, delegate
			{
				if (!botDoorOpener_0.Owner.IsDead)
				{
					if (botDoorOpener_0.Owner.Memory.HaveEnemy)
					{
						botDoorOpener_0.Owner.Mover.SetMoverState(EBotMoverState.Default);
						botDoorOpener_0.EnteringDoorSequence = false;
					}
					else
					{
						if (botDoorOpener_0.Owner.Mover.IsComeTo(botDoorOpener_0.Owner.Settings.FileSettings.Move.REACH_DIST, onCover: false))
						{
							botDoorOpener_0.Owner.Mover.Stop();
						}
						Physics.Raycast(new Ray(pointAfterDoor, secondLookDirection - pointAfterDoor), out var hitInfo2, magnitude, LayerMaskClass.HighPolyWithTerrainMask);
						float distance2 = hitInfo2.distance;
						float delay2 = 0f;
						if (distance2 > 3f)
						{
							delay2 = 1.2f;
							botDoorOpener_0.Owner.Steering.LookToPoint(secondLookDirection);
						}
						botDoorOpener_0.Owner.AITaskManager.RegisterDelayedTask(delay2, delegate
						{
							if (!botDoorOpener_0.Owner.IsDead)
							{
								botDoorOpener_0.EnteringDoorSequence = false;
								botDoorOpener_0.Owner.Mover.SetMoverState(EBotMoverState.Default);
								botDoorOpener_0.Owner.Steering.LookToMovingDirection();
							}
						});
					}
				}
			});
		}

		public void method_2()
		{
			if (botDoorOpener_0.Owner.IsDead)
			{
				return;
			}
			if (botDoorOpener_0.Owner.Memory.HaveEnemy)
			{
				botDoorOpener_0.Owner.Mover.SetMoverState(EBotMoverState.Default);
				botDoorOpener_0.EnteringDoorSequence = false;
				return;
			}
			if (botDoorOpener_0.Owner.Mover.IsComeTo(botDoorOpener_0.Owner.Settings.FileSettings.Move.REACH_DIST, onCover: false))
			{
				botDoorOpener_0.Owner.Mover.Stop();
			}
			Physics.Raycast(new Ray(pointAfterDoor, secondLookDirection - pointAfterDoor), out var hitInfo, magnitude, LayerMaskClass.HighPolyWithTerrainMask);
			float distance = hitInfo.distance;
			float delay = 0f;
			if (distance > 3f)
			{
				delay = 1.2f;
				botDoorOpener_0.Owner.Steering.LookToPoint(secondLookDirection);
			}
			botDoorOpener_0.Owner.AITaskManager.RegisterDelayedTask(delay, delegate
			{
				if (!botDoorOpener_0.Owner.IsDead)
				{
					botDoorOpener_0.EnteringDoorSequence = false;
					botDoorOpener_0.Owner.Mover.SetMoverState(EBotMoverState.Default);
					botDoorOpener_0.Owner.Steering.LookToMovingDirection();
				}
			});
		}

		public void method_3()
		{
			if (!botDoorOpener_0.Owner.IsDead)
			{
				botDoorOpener_0.EnteringDoorSequence = false;
				botDoorOpener_0.Owner.Mover.SetMoverState(EBotMoverState.Default);
				botDoorOpener_0.Owner.Steering.LookToMovingDirection();
			}
		}
	}

	public const float DIR_SIDE_MAX_SQRT = 4f;

	public const float MAX_DIST_SQR = 4f;

	[NonSerialized]
	public const float CHECK_RUN_DIST_SQR = 27.039997f;

	[NonSerialized]
	public const float INTERACT_PERIOD_BREACH = 2.7f;

	[NonSerialized]
	public const float OPEN_DIST_SQR = 16f;

	[NonSerialized]
	public const float PERIOD_DOOR_OPEN = 3f;

	[NonSerialized]
	public const float PERIOD_FOR_DOOR_FORCED_STAY_OPENED_AFTER_OPEN_BY_BOT = 15f;

	[NonSerialized]
	public const float STOP_SPRINT_DIST_SQR = 27.039997f;

	[NonSerialized]
	public const float STOP_SPRINT_PERIOD = 4f;

	[NonSerialized]
	public const float DIST_TO_INTERACT = 0.3f;

	[NonSerialized]
	public const float DIST_SQR_TO_INTERACT = 0.09f;

	public bool NearDoor;

	public DoorInteractionStatus _lastFrameDoorInteractionStatus;

	[NonSerialized]
	public NavMeshDoorLink CurrentDoorLink;

	[NonSerialized]
	public BotOwner Owner;

	[NonSerialized]
	public float TraversingEnd;

	[NonSerialized]
	public float SearchCloseDoorTime;

	[NonSerialized]
	public float ComeToDoorLast;

	[NonSerialized]
	public float NextWaitInteractedDoor;

	[NonSerialized]
	public bool ShallStartInteract;

	[NonSerialized]
	public float NextStartStop;

	[NonSerialized]
	public float RefreshWayPeriod;

	[NonSerialized]
	public bool DoBreach;

	[NonSerialized]
	public bool BreachRolledForCurrentDoor;

	[NonSerialized]
	public Vector3 InteractPosition;

	[NonSerialized]
	public bool EnteringDoorSequence;

	[NonSerialized]
	public Door LastInteractingDoor;

	[NonSerialized]
	public EDoorState LastInteractingDoorState;

	[NonSerialized]
	public float PrevFrameSDistToDoorInteractionPoint;

	[NonSerialized]
	public float LastSDistToDoorInteractPointUpdateTime;

	[NonSerialized]
	public float NextWantPassCheck;

	public Vector3 LookPoint
	{
		get
		{
			Vector3 normalized = (CurrentDoorLink.transform.position - Owner.Position).normalized;
			return CurrentDoorLink.Door.GetInteractionPosition(Owner.Position + normalized * 2f);
		}
	}

	public bool HaveDoorToOpen => CurrentDoorLink != null;

	public float Single_0 => Owner.Settings.FileSettings.Move.WAIT_DOOR_OPEN_SEC;

	[field: NonSerialized]
	public bool Interacting { get; set; }

	public event Action OnEndInteract;

	public BotDoorOpener(BotOwner owner)
	{
		Owner = owner;
	}

	public void ManualUpdate()
	{
		if (Owner.Mover.CurrentState != EBotMoverState.NearDoor)
		{
			return;
		}
		if (Time.time > NextWantPassCheck)
		{
			NextWantPassCheck = Time.time + 1.5f;
			List<NavMeshDoorLink> cellDoorLinks = method_1();
			SearchCloseDoorTime = Time.time + 0.05f;
			method_3(cellDoorLinks, out var wantPass);
			if (!wantPass && !EnteringDoorSequence)
			{
				Owner.Mover.SetMoverState(EBotMoverState.Default);
				return;
			}
		}
		if ((CurrentDoorLink == null || CurrentDoorLink.Door.DoorState == EDoorState.Interacting || CurrentDoorLink.Door.DoorState == EDoorState.Breaching) && !EnteringDoorSequence)
		{
			Owner.Mover.SetMoverState(EBotMoverState.Default);
			return;
		}
		InteractPosition = CurrentDoorLink.Door.GetInteractionPosition(Owner.Position);
		if (DoBreach && CurrentDoorLink != null)
		{
			InteractPosition = CurrentDoorLink.Door.GetBreakInParameters(Owner.Position).InteractionPosition;
		}
		float num = Owner.SqrDistHorizontal(InteractPosition);
		if (num < PrevFrameSDistToDoorInteractionPoint)
		{
			PrevFrameSDistToDoorInteractionPoint = num;
			LastSDistToDoorInteractPointUpdateTime = Time.time;
		}
		if (Time.time - LastSDistToDoorInteractPointUpdateTime > 1f && !EnteringDoorSequence)
		{
			TraverseDoorLink();
		}
		else if (num > 0.09f)
		{
			NextWantPassCheck = Time.time + 1.5f;
		}
		else if (CurrentDoorLink.ShallInteract() && !EnteringDoorSequence)
		{
			if (!ShallStartInteract)
			{
				ShallStartInteract = true;
			}
			TraverseDoorLink();
		}
	}

	public DoorInteractionStatus UpdateDoorInteractionStatus()
	{
		if (Owner.Mover.CurrentState == EBotMoverState.NearDoor)
		{
			return DoorInteractionStatus.OpeningDoor;
		}
		List<NavMeshDoorLink> cellDoorLinks = method_1();
		if (Interacting)
		{
			Owner.Steering.SetYAngle(0f);
			if (TraversingEnd < Time.time)
			{
				Interacting = false;
				this.OnEndInteract?.Invoke();
			}
			return _lastFrameDoorInteractionStatus;
		}
		if (SearchCloseDoorTime < Time.time)
		{
			SearchCloseDoorTime = Time.time + 0.05f;
			if (!method_3(cellDoorLinks, out var wantPass))
			{
				return DoorInteractionStatus.CanRun;
			}
			if (wantPass)
			{
				method_2();
			}
		}
		return _lastFrameDoorInteractionStatus;
	}

	public bool DogFightHaveNearestDoor()
	{
		List<NavMeshDoorLink> list = method_1();
		int num = 0;
		while (true)
		{
			if (num < list.Count)
			{
				NavMeshDoorLink navMeshDoorLink = list[num];
				if (navMeshDoorLink.Door.DoorState == EDoorState.Shut && !(GClass856.SqrDistance(Owner.Position, navMeshDoorLink.MidOpen) >= 1.44f))
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		return true;
	}

	public void method_0()
	{
		if (CurrentDoorLink.Door.CurrentAngle <= 90f && Owner.Mover.TargetPoint.HasValue && Owner.SqrDistHorizontal(Owner.Mover.TargetPoint.Value) > 0.010000001f)
		{
			Owner.StopMove();
			Owner.GoToPoint(Owner.Position);
		}
	}

	public List<NavMeshDoorLink> method_1()
	{
		List<NavMeshDoorLink> result = Owner.NearDoorData.CurrentDoorLinks();
		if (CurrentDoorLink != null)
		{
			if (CurrentDoorLink.Door.DoorState == EDoorState.Interacting)
			{
				method_0();
				return result;
			}
		}
		else
		{
			Owner.Mover.SetMoverState(EBotMoverState.Default);
		}
		return result;
	}

	public bool method_2()
	{
		InteractPosition = CurrentDoorLink.Door.GetInteractionPosition(Owner.Position);
		if (!BreachRolledForCurrentDoor)
		{
			DoBreach = GClass856.IsTrue100(Owner.Settings.FileSettings.Move.BREACH_CHANCE_100);
			BreachRolledForCurrentDoor = true;
		}
		if (DoBreach && CurrentDoorLink != null)
		{
			InteractPosition = CurrentDoorLink.Door.GetBreakInParameters(Owner.Position).InteractionPosition;
		}
		float num = Owner.SqrDistHorizontal(InteractPosition);
		if (num > 0.09f)
		{
			if (CurrentDoorLink == null)
			{
				Debug.LogError("_currentDoorLink == null");
				return false;
			}
			PrevFrameSDistToDoorInteractionPoint = num;
			LastSDistToDoorInteractPointUpdateTime = Time.time;
			Owner.Mover.SetMoverState(EBotMoverState.NearDoor);
			NextWantPassCheck = Time.time + 1.5f;
			Owner.Mover.CurrentStateGoToPoint(InteractPosition, slowAtTheEnd: false, 0.3f, getUpWithCheck: false, mustHaveWay: false);
			return true;
		}
		return false;
	}

	public bool method_3(List<NavMeshDoorLink> cellDoorLinks, out bool wantPass)
	{
		NearDoor = false;
		CurrentDoorLink = method_5(cellDoorLinks, null);
		if (CurrentDoorLink != null && GClass856.SqrDistance(CurrentDoorLink.Door.transform.position, Owner.Position) > 108.15999f)
		{
			CurrentDoorLink = null;
		}
		if (CurrentDoorLink == null)
		{
			wantPass = false;
			return false;
		}
		bool flag = true;
		float num = method_4(CurrentDoorLink);
		if (num < 27.039997f)
		{
			flag = method_6(CurrentDoorLink);
		}
		_lastFrameDoorInteractionStatus = (flag ? DoorInteractionStatus.CanRun : DoorInteractionStatus.OpeningDoor);
		if (num < 16f)
		{
			wantPass = method_10(Owner.Mover.RealDestPoint, CurrentDoorLink);
			if (wantPass)
			{
				return true;
			}
			if (!wantPass && cellDoorLinks.Count > 1 && Owner.Mover.CurrentState != EBotMoverState.NearDoor)
			{
				NavMeshDoorLink navMeshDoorLink = method_5(cellDoorLinks, CurrentDoorLink);
				if (navMeshDoorLink == null)
				{
					return false;
				}
				CurrentDoorLink = navMeshDoorLink;
				wantPass = method_10(Owner.Mover.RealDestPoint, navMeshDoorLink);
				return true;
			}
		}
		wantPass = false;
		return false;
	}

	public void TraverseDoorLink()
	{
		BreachRolledForCurrentDoor = false;
		method_8();
	}

	public void Interact(Door door, EInteractionType Etype)
	{
		LastInteractingDoor = door;
		if (Time.time < CurrentDoorLink.LastInteractByBotTime + 1f)
		{
			Owner.Mover.SetMoverState(EBotMoverState.Default);
			return;
		}
		CurrentDoorLink.LastInteractByBotTime = Time.time;
		if (Etype == EInteractionType.Breach || Etype == EInteractionType.Open)
		{
			CurrentDoorLink.LastOpenByBotTime = Time.time;
			method_7();
		}
		switch (Etype)
		{
		case EInteractionType.Open:
			LastInteractingDoorState = EDoorState.Open;
			break;
		case EInteractionType.Close:
			LastInteractingDoorState = EDoorState.Shut;
			break;
		case EInteractionType.Breach:
			LastInteractingDoorState = EDoorState.Open;
			break;
		}
		Interacting = true;
		float num = 1f;
		num = ((Etype != EInteractionType.Breach) ? Single_0 : 2.7f);
		TraversingEnd = Time.time + num;
		InteractionResult interactionResult = new InteractionResult(Etype);
		Owner.GetPlayer.CurrentManagedState.StartDoorInteraction(door, interactionResult, null);
	}

	public float method_4(NavMeshDoorLink doorLink)
	{
		Vector3 vector = ((doorLink.Door.DoorState != EDoorState.Open) ? doorLink.MidOpen : doorLink.MidClose);
		return (vector - Owner.Transform.position).sqrMagnitude;
	}

	public NavMeshDoorLink method_5(List<NavMeshDoorLink> list, NavMeshDoorLink exclude)
	{
		float num = 4f;
		for (int i = 0; i < list.Count; i++)
		{
			NavMeshDoorLink navMeshDoorLink = list[i];
			if ((!(exclude != null) || navMeshDoorLink.Id != exclude.Id) && (!(LastInteractingDoor != null) || !(navMeshDoorLink.Door.Id == LastInteractingDoor.Id) || navMeshDoorLink.Door.DoorState != EDoorState.Open))
			{
				float num2 = method_4(navMeshDoorLink);
				if (num2 < num)
				{
					num = num2;
					CurrentDoorLink = navMeshDoorLink;
				}
			}
		}
		return CurrentDoorLink;
	}

	public bool method_6(NavMeshDoorLink link)
	{
		GClass365 gClass = null;
		switch (link.Door.DoorState)
		{
		case EDoorState.Open:
			gClass = link.SegmentClose;
			break;
		case EDoorState.Shut:
			gClass = link.SegmentOpen;
			break;
		}
		if (gClass == null)
		{
			return true;
		}
		Vector3 vector = GClass856.Rotate90(gClass.a - gClass.b, 1);
		if (Vector3.Dot(vector, Owner.LookDirection) < 0f)
		{
			vector = -vector;
		}
		if (GClass855.IsAngLessNormalized(GClass855.NormalizeFastSelf(Owner.LookDirection), GClass855.NormalizeFastSelf(vector), 0.9396926f))
		{
			return false;
		}
		if (GClass856.SqrDistance(link.Door.transform.position, Owner.Position) < 27.039997f)
		{
			Owner.Mover.SprintPause(4f);
			return false;
		}
		return true;
	}

	public void method_7()
	{
		if (CurrentDoorLink == null || EnteringDoorSequence || Owner.Memory.HaveEnemy)
		{
			return;
		}
		EnteringDoorSequence = true;
		Vector3 vector = GClass856.Rotate90(CurrentDoorLink.Open2 - CurrentDoorLink.Open1, GClass856.ESideTurn.Left).normalized * 0.75f;
		Vector3 vector2 = GClass856.Rotate90(CurrentDoorLink.Open2 - CurrentDoorLink.Open1, GClass856.ESideTurn.Right).normalized * 0.75f;
		Vector3 vector3 = CurrentDoorLink.MidOpen + vector;
		Vector3 vector4 = CurrentDoorLink.MidOpen + vector2;
		Vector3 pointAfterDoor;
		Vector3 vector5;
		if (GClass856.SqrDistance(Owner.Position, vector3) > GClass856.SqrDistance(Owner.Position, vector4))
		{
			pointAfterDoor = vector3;
			vector5 = vector;
		}
		else
		{
			pointAfterDoor = vector4;
			vector5 = vector2;
		}
		Vector3 pointLookForward = pointAfterDoor + vector5 * 100f;
		Vector3 vector6 = pointAfterDoor + vector5 * 5f + 30f * GClass856.Rotate90(vector5, GClass856.ESideTurn.Left);
		Vector3 vector7 = pointAfterDoor + vector5 * 5f + 30f * GClass856.Rotate90(vector5, GClass856.ESideTurn.Right);
		pointAfterDoor.y = CurrentDoorLink.MidOpen.y - 1f;
		vector7.y = CurrentDoorLink.MidOpen.y;
		vector6.y = CurrentDoorLink.MidOpen.y;
		pointLookForward.y = CurrentDoorLink.MidOpen.y;
		Owner.Steering.LookToPoint(pointLookForward);
		float magnitude = 10f;
		bool flag = GClass856.RandomBool();
		Vector3 firstLookDirection = (flag ? vector7 : vector6);
		Vector3 secondLookDirection = (flag ? vector6 : vector7);
		Owner.AITaskManager.RegisterDelayedTask(3f, delegate
		{
			if (Owner.Memory.HaveEnemy)
			{
				Owner.Mover.SetMoverState(EBotMoverState.Default);
				EnteringDoorSequence = false;
			}
			else
			{
				Owner.Steering.LookToPoint(pointLookForward);
				Owner.Mover.CurrentStateGoToPoint(pointAfterDoor, slowAtTheEnd: false, Owner.Settings.FileSettings.Move.REACH_DIST, getUpWithCheck: false, mustHaveWay: false, onlyShortTrie: false, force: true);
				Owner.AITaskManager.RegisterDelayedTask(1.5f, delegate
				{
					if (!Owner.IsDead)
					{
						if (Owner.Memory.HaveEnemy)
						{
							Owner.Mover.SetMoverState(EBotMoverState.Default);
							EnteringDoorSequence = false;
						}
						else
						{
							if (Owner.Mover.IsComeTo(Owner.Settings.FileSettings.Move.REACH_DIST, onCover: false))
							{
								Owner.Mover.Stop();
							}
							Physics.Raycast(new Ray(pointAfterDoor, firstLookDirection - pointAfterDoor), out var hitInfo, magnitude, LayerMaskClass.HighPolyWithTerrainMask);
							float distance = hitInfo.distance;
							float delay = 0f;
							if (distance > 3f)
							{
								delay = 1.2f;
								Owner.Steering.LookToPoint(firstLookDirection);
							}
							Owner.AITaskManager.RegisterDelayedTask(delay, delegate
							{
								if (!Owner.IsDead)
								{
									if (Owner.Memory.HaveEnemy)
									{
										Owner.Mover.SetMoverState(EBotMoverState.Default);
										EnteringDoorSequence = false;
									}
									else
									{
										if (Owner.Mover.IsComeTo(Owner.Settings.FileSettings.Move.REACH_DIST, onCover: false))
										{
											Owner.Mover.Stop();
										}
										Physics.Raycast(new Ray(pointAfterDoor, secondLookDirection - pointAfterDoor), out var hitInfo2, magnitude, LayerMaskClass.HighPolyWithTerrainMask);
										float distance2 = hitInfo2.distance;
										float delay2 = 0f;
										if (distance2 > 3f)
										{
											delay2 = 1.2f;
											Owner.Steering.LookToPoint(secondLookDirection);
										}
										Owner.AITaskManager.RegisterDelayedTask(delay2, delegate
										{
											if (!Owner.IsDead)
											{
												EnteringDoorSequence = false;
												Owner.Mover.SetMoverState(EBotMoverState.Default);
												Owner.Steering.LookToMovingDirection();
											}
										});
									}
								}
							});
						}
					}
				});
			}
		});
	}

	public void method_8()
	{
		float time = Time.time;
		float num = time - ComeToDoorLast;
		Owner.Mover.SprintPause(4f);
		Owner.MovementPause(Single_0);
		if (num > 3f)
		{
			ComeToDoorLast = time;
		}
		else
		{
			if (!CurrentDoorLink.Door.Operatable || CurrentDoorLink.Door.DoorState == EDoorState.Interacting)
			{
				return;
			}
			if (method_9(CurrentDoorLink))
			{
				Owner.Mover.SetMoverState(EBotMoverState.Default);
				return;
			}
			ShallStartInteract = false;
			if (CurrentDoorLink.Door.DoorState == EDoorState.Shut)
			{
				if (DoBreach)
				{
					WorldInteractiveObject.GStruct436 breakInParameters = CurrentDoorLink.Door.GetBreakInParameters(Owner.Position);
					DoBreach = CurrentDoorLink.Door.BreachSuccessRoll(breakInParameters.InteractionPosition);
				}
				if (DoBreach)
				{
					Interact(CurrentDoorLink.Door, EInteractionType.Breach);
				}
				else
				{
					Interact(CurrentDoorLink.Door, EInteractionType.Open);
				}
			}
			else if (CurrentDoorLink.Door.DoorState == EDoorState.Open)
			{
				Interact(CurrentDoorLink.Door, EInteractionType.Close);
			}
		}
	}

	public bool method_9(NavMeshDoorLink infoStruct)
	{
		if (Time.time < infoStruct.LastOpenByBotTime + 15f)
		{
			return infoStruct.Door.DoorState == EDoorState.Open;
		}
		return false;
	}

	public bool method_10(Vector3 goTo, NavMeshDoorLink infoStruct)
	{
		if (method_9(infoStruct))
		{
			return false;
		}
		Vector3 vector = Owner.Transform.position + Vector3.up;
		if (!(Mathf.Abs(vector.y - infoStruct.Open1.y) < 0.6f))
		{
			return false;
		}
		NearDoor = true;
		if (infoStruct.Door.DoorState == EDoorState.Interacting)
		{
			if (NextWaitInteractedDoor < Time.time)
			{
				NextWaitInteractedDoor = Time.time + 0.5f;
				Owner.Mover.SprintPause(2f);
			}
			return false;
		}
		GClass365 gClass = null;
		DoorActionType doorActionType = DoorActionType.none;
		if (infoStruct.Door.DoorState == EDoorState.Open)
		{
			gClass = infoStruct.SegmentClose;
			doorActionType = DoorActionType.wantClose;
			if ((infoStruct.MidClose - vector).sqrMagnitude > 4f)
			{
				return false;
			}
		}
		if (infoStruct.Door.DoorState == EDoorState.Shut)
		{
			float sqrMagnitude = (infoStruct.MidOpen - vector).sqrMagnitude;
			doorActionType = DoorActionType.wantOpen;
			gClass = infoStruct.SegmentOpen;
			if (sqrMagnitude > 4f)
			{
				return false;
			}
		}
		if (gClass == null)
		{
			return false;
		}
		DoorActionType doorActionType2 = DoorActionType.none;
		Vector3 v = vector - goTo;
		v.y = 0f;
		Vector3 vector2 = GClass855.NormalizeFastSelf(v);
		Vector3 vector3 = vector2 * 0.25f;
		Vector3 vector4 = vector2 * 0.5f;
		Vector3 a = vector + vector3;
		Vector3 b = goTo - vector4;
		if (GClass369.GetCrossPoint(new GClass365(a, b), gClass).HasValue)
		{
			doorActionType2 = doorActionType;
		}
		switch (doorActionType2)
		{
		case DoorActionType.wantOpen:
			if (infoStruct.Door.DoorState == EDoorState.Shut)
			{
				return true;
			}
			break;
		case DoorActionType.wantClose:
			if (infoStruct.Door.DoorState == EDoorState.Open)
			{
				return true;
			}
			break;
		}
		return false;
	}
}
