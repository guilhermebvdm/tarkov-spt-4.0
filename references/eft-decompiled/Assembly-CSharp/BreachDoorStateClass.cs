using System;
using EFT;
using EFT.Interactive;
using UnityEngine;

public class BreachDoorStateClass(MovementContext movementContext) : MovementState(movementContext)
{
	[Range(0f, 1f)]
	public float KickTime = 0.45f;

	[NonSerialized]
	public Door Door_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public EDoorState EdoorState_0;

	public override void Enter(bool isFromSameState)
	{
		Bool_0 = false;
		base.Enter(isFromSameState);
		Door_0 = MovementContext.InteractionInfo.WorldInteractiveObject as Door;
		EdoorState_0 = ((Door_0.DoorState == EDoorState.Interacting) ? Door_0.FallbackState : Door_0.DoorState);
	}

	public override void Exit(bool toSameState)
	{
		base.Exit(toSameState);
		MovementContext.IgnoreInteractionCollision(null, ignore: false);
		MovementContext.StopAnyInteractions();
		if (EdoorState_0 == EDoorState.Locked && Door_0.DoorState != EDoorState.Locked)
		{
			MovementContext.LockedDoorOpened(breached: true);
		}
	}

	public override void ManualAnimatorMoveUpdate(float deltaTime)
	{
		if (NormalizedTime > 0.15f)
		{
			ProcessAnimatorMovement(deltaTime);
		}
		if (!(NormalizedTime < KickTime) && !Bool_0)
		{
			Bool_0 = true;
			if (MovementContext.NextBreachResult)
			{
				MovementContext.ExecuteInteraction(Door_0, MovementContext.InteractionInfo.Result);
			}
			else
			{
				Door_0.FailBreach(MovementContext.TransformPosition);
			}
			MovementContext.OnBreach();
		}
	}

	public override void ExecuteDoorInteraction(WorldInteractiveObject interactive, InteractionResult interactionResult, Action callback, Player user)
	{
		Door door = interactive as Door;
		if (door != null)
		{
			if (interactionResult.InteractionType == EInteractionType.Breach)
			{
				door.KickOpen(MovementContext.TransformPosition);
				return;
			}
			door.LockForInteraction();
			door.SetUser(user);
			door.Interact(interactionResult);
		}
	}
}
