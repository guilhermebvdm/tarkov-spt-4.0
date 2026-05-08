using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace AnimationEventSystem;

[GAttribute19(typeof(GClass1334))]
public class AnimationEventsStateBehaviour : StateMachineBehaviour, GInterface137, IStateBehaviour
{
	public AnimationEventsContainer EventsContainer;

	public string FullName;

	public int FullNameHash;

	public AnimatorControllerStaticData EventsData;

	public int EventsListId = -1;

	private GClass1446 gclass1446_0;

	[CanBeNull]
	public List<AnimationEvent> AnimationEvents => EventsData.GetEventsByIndex(EventsListId);

	AnimationEventsContainer GInterface137.EventsContainer
	{
		get
		{
			return EventsContainer;
		}
		set
		{
			EventsContainer = value;
		}
	}

	int GInterface137.FullNameHash => FullNameHash;

	public void method_0(Animator animator)
	{
		if (gclass1446_0 == null)
		{
			gclass1446_0 = GClass1446.Create();
		}
		if ((object)animator != gclass1446_0.Animator)
		{
			gclass1446_0.SetAnimator(animator);
		}
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		method_0(animator);
		EventsContainer.OnStateEnter(gclass1446_0, GClass1446.CreateAnimatorStateInfoWrapper(stateInfo), layerIndex, AnimationEvents);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		method_0(animator);
		EventsContainer.OnStateUpdate(gclass1446_0, GClass1446.CreateAnimatorStateInfoWrapper(stateInfo), layerIndex, AnimationEvents);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		method_0(animator);
		EventsContainer.OnStateExit(gclass1446_0, GClass1446.CreateAnimatorStateInfoWrapper(stateInfo), layerIndex, AnimationEvents);
	}
}
