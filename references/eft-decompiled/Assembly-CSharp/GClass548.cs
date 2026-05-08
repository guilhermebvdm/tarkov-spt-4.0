using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;

public class GClass548
{
	[NonSerialized]
	public BotOwner BotOwner_0;

	[NonSerialized]
	[CompilerGenerated]
	public GClass547 Gclass547_0;

	[NonSerialized]
	public Dictionary<EBotMoverState, GClass547> Dictionary_0 = new Dictionary<EBotMoverState, GClass547>();

	public Action<GClass547> OnStateChanged;

	public GClass547 CurrentMoverState
	{
		[CompilerGenerated]
		get
		{
			return Gclass547_0;
		}
		[CompilerGenerated]
		set
		{
			Gclass547_0 = value;
		}
	}

	public GClass547 DefaultMoverState => Dictionary_0[EBotMoverState.Default];

	public GClass548(BotOwner owner, BotMover botMover, AICoversData covers)
	{
		BotOwner_0 = owner;
		method_0(owner, EBotMoverState.Default, botMover, covers);
		method_0(owner, EBotMoverState.NearDoor, botMover, covers);
		CurrentMoverState = Dictionary_0[EBotMoverState.Default];
	}

	public void method_0(BotOwner owner, EBotMoverState state, BotMover botMover, AICoversData covers)
	{
		Dictionary_0.Add(state, new GClass547(owner, state, botMover, covers));
	}

	public void SetState(EBotMoverState state)
	{
		if (state == CurrentMoverState.State)
		{
			return;
		}
		CurrentMoverState.LastTargetPoint = BotOwner_0.Mover.TargetPoint;
		CurrentMoverState = Dictionary_0[state];
		if (state == EBotMoverState.Default)
		{
			if (CurrentMoverState.PathController.TargetPoint.HasValue)
			{
				CurrentMoverState.PathFinder.GoToPosition(CurrentMoverState.PathController.TargetPoint.Value, slowAtTheEnd: true, 0.5f, getUpWithCheck: false, mustHaveWay: false, onlyShortTrie: false, force: true, slowCalcUsingNativeNavMesh: true);
			}
			else if (CurrentMoverState.LastTargetPoint.HasValue)
			{
				CurrentMoverState.PathFinder.GoToPosition(CurrentMoverState.LastTargetPoint.Value, slowAtTheEnd: true, 0.5f, getUpWithCheck: false, mustHaveWay: false, onlyShortTrie: false, force: true, slowCalcUsingNativeNavMesh: true);
			}
		}
		OnStateChanged?.Invoke(CurrentMoverState);
	}
}
