using System;
using System.Runtime.CompilerServices;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using UnityEngine;

public class DoorInteractionStateClass : MovementState
{
	[CompilerGenerated]
	public class Class1395
	{
		public DoorInteractionStateClass DoorInteractionStateClass;

		public GClass2107 endState;

		public GClass2105 method_0()
		{
			if (!(DoorInteractionStateClass.MovementContext.GetHandProgress() > 0.99f) && !(Time.realtimeSinceStartup - DoorInteractionStateClass.Float_0 > 1.6f) && (!DoorInteractionStateClass.MovementContext.UsedSimplifiedSkeleton || !(DoorInteractionStateClass.NormalizedTime > 0.35f)))
			{
				return null;
			}
			return endState;
		}

		public GClass2105 method_1()
		{
			if (!(DoorInteractionStateClass.MovementContext.GetHandProgress() < 0f) && !(Time.realtimeSinceStartup - DoorInteractionStateClass.Float_0 > 3f) && DoorInteractionStateClass.Bool_1 && (!DoorInteractionStateClass.MovementContext.UsedSimplifiedSkeleton || !(DoorInteractionStateClass.NormalizedTime > 0.7f)))
			{
				return null;
			}
			return new GClass2105(DoorInteractionStateClass);
		}
	}

	[NonSerialized]
	public float Float_0;

	public WorldInteractiveObject Door;

	[NonSerialized]
	public GripPose GripPose_0;

	[NonSerialized]
	public EDoorState EdoorState_0;

	[NonSerialized]
	public SmartGrip SmartGrip_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public const float Float_2 = 1f;

	[NonSerialized]
	public GClass2105 Gclass2105_0;

	public DoorInteractionStateClass(MovementContext movementContext)
		: base(movementContext)
	{
	}

	public override void Enter(bool isFromSameState)
	{
		base.Enter(isFromSameState);
		MovementContext.StateLocksInventory = true;
		Bool_0 = false;
		MovementContext.SetTilt(0f);
		MovementContext.FovCompensationOn(on: false);
		MovementContext.IK.Value = 0f;
		Float_0 = Time.realtimeSinceStartup;
		Door = MovementContext.InteractionInfo.WorldInteractiveObject;
		WorldInteractiveObject.GStruct436 interactionParameters = MovementContext.InteractionParameters;
		GripPose_0 = interactionParameters.Grip;
		Int_0 = interactionParameters.AnimationId;
		Bool_1 = interactionParameters.Snap || GripPose_0 != null;
		if (GripPose_0 != null)
		{
			SmartGrip_0 = GripPose_0.GetComponent<SmartGrip>();
		}
		EdoorState_0 = ((Door.DoorState == EDoorState.Interacting) ? Door.FallbackState : Door.DoorState);
		StartStates();
	}

	public override void Exit(bool toSameState)
	{
		base.Exit(toSameState);
		MovementContext.StateLocksInventory = false;
		MovementContext.IgnoreInteractionCollision(null, ignore: false);
		MovementContext.FovCompensationOn(on: true);
		if (EdoorState_0 == EDoorState.Locked && Door != null && Door.DoorState != EDoorState.Locked)
		{
			MovementContext.LockedDoorOpened(breached: false);
		}
		Door = null;
		GripPose_0 = null;
		SmartGrip_0 = null;
		MovementContext.SetIKInteraction(null);
		MovementContext.ResetInventory();
	}

	public void StartInteractionAnimation()
	{
		MovementContext.CancelInventoryAnimation();
		MovementContext.SetInteractInHands((EInteraction)Int_0);
		MovementContext.SetIKInteraction(GripPose_0);
	}

	public virtual void ExecuteInteraction()
	{
		MovementContext.ExecuteInteraction(Door, MovementContext.InteractionInfo.Result);
	}

	public override void ExecuteDoorInteraction(WorldInteractiveObject door, InteractionResult interactionResult, Action callback, Player user)
	{
		door.SetUser(user);
		door.Interact(interactionResult);
	}

	public void StopInteractionAnimation()
	{
		MovementContext.SetIKInteraction(null);
		if (SmartGrip_0 != null)
		{
			SmartGrip_0.Reset();
		}
		if (Bool_0)
		{
			MovementContext.RemoveKeyFromHand();
		}
		MovementContext.SetInteractInHands(EInteraction.None);
		MovementContext.StopAnyInteractions();
	}

	public void BaseUpdate()
	{
		MovementContext.IK.Value = MovementContext.GetHandProgress();
		if (!Bool_0 && MovementContext.InteractionInfo.Result != null && MovementContext.InteractionInfo.Result.InteractionType == EInteractionType.Unlock && Time.realtimeSinceStartup - Float_0 > 1f)
		{
			KeyComponent key = ((KeyInteractionResultClass)MovementContext.InteractionInfo.Result).Key;
			MovementContext.SpawnKeyInHands(key.Item, "Base HumanLDigit21");
			Bool_0 = true;
		}
	}

	public void StartStates()
	{
		GClass2106 gClass = new GClass2106(this);
		GClass2107 endState = new GClass2107(this);
		gClass.Transition = () => (!(MovementContext.GetHandProgress() > 0.99f) && !(Time.realtimeSinceStartup - Float_0 > 1.6f) && (!MovementContext.UsedSimplifiedSkeleton || !(NormalizedTime > 0.35f))) ? null : endState;
		endState.Transition = () => (!(MovementContext.GetHandProgress() < 0f) && !(Time.realtimeSinceStartup - Float_0 > 3f) && Bool_1 && (!MovementContext.UsedSimplifiedSkeleton || !(NormalizedTime > 0.7f))) ? null : new GClass2105(this);
		Gclass2105_0 = gClass;
		Gclass2105_0.OnEnter();
	}

	public override void ManualAnimatorMoveUpdate(float deltaTime)
	{
		if (Gclass2105_0 == null)
		{
			Debug.LogError("That was rather unexpected. Commence DoorInteractState's termination.");
			StopInteractionAnimation();
			return;
		}
		BaseUpdate();
		GClass2105 getNextState = Gclass2105_0.GetNextState;
		if (getNextState != null)
		{
			Gclass2105_0.OnExit();
			getNextState.OnEnter();
			Gclass2105_0 = getNextState;
		}
	}
}
