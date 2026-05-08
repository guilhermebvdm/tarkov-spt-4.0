using System;
using System.Collections.Generic;

namespace AnimationEventSystem;

public class AnimationEventsSequenceData
{
	public readonly struct GStruct142(string eventName, int stateHash, bool condsPassed)
	{
		public readonly string EventName = eventName;

		public readonly int StateNameShortHash = stateHash;

		public readonly bool ConditionPassed = condsPassed;
	}

	[NonSerialized]
	public const int MAX_QUEUE_SIZE = 15;

	public readonly Queue<GStruct142> AnimationEventsDebugQueue = new Queue<GStruct142>(15);

	public void AddAnimationEventToDebugQueue(string eventName, int stateShortNameHash, bool conditionPassed)
	{
		if (AnimationEventsDebugQueue.Count > 15)
		{
			AnimationEventsDebugQueue.Dequeue();
		}
		AnimationEventsDebugQueue.Enqueue(new GStruct142(eventName, stateShortNameHash, conditionPassed));
	}
}
