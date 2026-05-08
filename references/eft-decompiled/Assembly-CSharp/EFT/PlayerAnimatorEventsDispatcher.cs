using System;
using AnimationEventSystem;

namespace EFT;

public class PlayerAnimatorEventsDispatcher
{
	[NonSerialized]
	public AnimationEventsEmitter AnimEventsEmitter;

	[NonSerialized]
	public GClass2096 EventsTable;

	public IPlayerAnimatorEvents PlayerAnimatorEvents => EventsTable;

	public PlayerAnimatorEventsDispatcher(IAnimator animator)
	{
		EventsTable = new GClass2096();
		AnimEventsEmitter = new AnimationEventsEmitter();
		AnimEventsEmitter.SetAnimator(animator, AnimationEventsEmitter.EEmitType.EmitOnDemand);
		AnimEventsEmitter.OnEventAction += method_0;
	}

	public void EmitEvents()
	{
		AnimEventsEmitter.EmitEvents();
	}

	public void Dispose()
	{
		AnimEventsEmitter.OnEventAction -= method_0;
	}

	public void method_0(int functionNameHash, AnimationEventParameter parameter)
	{
		EventsTable.DispatchEvents(functionNameHash, parameter);
	}
}
