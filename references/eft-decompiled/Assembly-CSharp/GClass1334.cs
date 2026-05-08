using System.Collections.Generic;
using AnimationEventSystem;

public class GClass1334 : GClass1332<AnimationEventsStateBehaviour, GClass1330>
{
	public override GClass1330 InternalCreate(AnimationEventsStateBehaviour behaviour, GClass1312 state)
	{
		GClass1330 gClass = base.InternalCreate(behaviour, state);
		gClass.FullName = behaviour.FullName;
		gClass.FullNameHash = behaviour.FullNameHash;
		gClass.EventsContainer = behaviour.EventsContainer.Clone() as AnimationEventsContainer;
		gClass.AnimationEvents = new List<AnimationEvent>();
		foreach (AnimationEvent animationEvent in behaviour.AnimationEvents)
		{
			gClass.AnimationEvents.Add((AnimationEvent)animationEvent.Clone());
		}
		return gClass;
	}
}
