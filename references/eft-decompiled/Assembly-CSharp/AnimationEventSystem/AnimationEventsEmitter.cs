using System;
using System.Collections.Generic;
using NLog;
using UnityEngine;

namespace AnimationEventSystem;

public class AnimationEventsEmitter : IEventsConsumer, IDisposable
{
	public class GClass715 : LoggerClass
	{
		[NonSerialized]
		public AnimationEventsEmitter AnimationEventsEmitter_0;

		[NonSerialized]
		public string String_0;

		[NonSerialized]
		public string String_1;

		public GClass715(AnimationEventsEmitter animationEventsEmitter, LoggerMode loggerMode, string distinctId = "undefined")
			: base("anim-events-emitter", loggerMode)
		{
			AnimationEventsEmitter_0 = animationEventsEmitter;
			String_0 = animationEventsEmitter.Animator.name;
			String_1 = distinctId;
		}

		public void WarnNoHandler(string functionName)
		{
			Log(LogLevel.Warn, "<color=red>There is no handler for event '{0}' in '{1}'</color>", functionName, String_0);
		}

		public void TraceEventTestPassed(AnimationEvent e, bool conditionsPassed, float normalizedTime, IAnimator animator, in AnimatorStateInfoWrapper stateInfo)
		{
			if (!IsEnabled(LogLevel.Trace))
			{
				return;
			}
			if (conditionsPassed)
			{
				Log("[{0}][Firing {1}::{2}({3}):{4}][nt:{5}][{6}][{7}]", "<b>[{0}]</b>[<color=blue>Firing <b>{1}::{2}({3}):{4}</b></color>][nt:{5}][{6}][{7}]", LogLevel.Trace, Time.frameCount, animator.GetStateName(in stateInfo), e.FunctionName, e.Parameter, e.Time, normalizedTime, animator, String_1);
				return;
			}
			if (!e.Enabled)
			{
				Log("[{0}][Disabled {1}::{2}({3}):{4}][nt:{5}][{6}][{7}]", "<b>[{0}]</b>[<color=red>Disabled {1}::{2}({3}):{4}</color>][nt:{5}][{6}][{7}]", LogLevel.Trace, Time.frameCount, animator.GetStateName(in stateInfo), e.FunctionName, e.Parameter, e.Time, normalizedTime, animator, String_1);
				return;
			}
			EventCondition eventCondition = null;
			for (int i = 0; i < e.EventConditions.Count; i++)
			{
				if (!e.EventConditions[i].IsSucceed(animator))
				{
					eventCondition = e.EventConditions[i];
					break;
				}
			}
			Log("[{0}][Condition failed:{1}::{2}({3}):{4} | condition:{5}][nt:{6}][{7}][{8}]", "<b>[{0}]</b>[<color=red>Condition failed:{1}::{2}({3}):{4} | condition:{5}</color>][nt:{6}][{7}][{8}]", LogLevel.Trace, Time.frameCount, animator.GetStateName(in stateInfo), e.FunctionName, e.Parameter, e.Time, eventCondition.ToString(animator), normalizedTime, animator, String_1);
		}

		public void EventPuttedToDelayedEventsQueue(AnimationEvent e, IAnimator animator, in AnimatorStateInfoWrapper stateInfo, float normalizedTime)
		{
			Log("[{0}][Delayed event: {1}::{2}({3})[{4}]][stinf.nt: {5}][{6}][{7}]", "<b>[{0}]</b>[<color=yellow>Delayed event:</color> <b>{1}::{2}({3})[{4}][stinf.nt: {5}]</b>][{6}][{7}]", LogLevel.Trace, Time.frameCount, AnimationEventsEmitter_0.Animator.GetStateName(in stateInfo), e.FunctionName, e.Parameter, e.Time, stateInfo.normalizedTime, String_0, String_1);
		}
	}

	public enum EEmitType
	{
		EmitOnAnimatorUpdate,
		EmitOnDemand
	}

	public struct Struct254
	{
		public readonly IAnimator Animator;

		public readonly float NormalizedTime;

		public readonly AnimationEvent AnimationEvent;

		public readonly AnimatorStateInfoWrapper StateInfo;

		public Struct254(AnimationEvent animationEvent, IAnimator animator, in AnimatorStateInfoWrapper stateInfo, float normalizedTime)
		{
			this = default(Struct254);
			Animator = animator;
			NormalizedTime = normalizedTime;
			AnimationEvent = animationEvent;
			StateInfo = stateInfo;
		}
	}

	[NonSerialized]
	public GClass715 Logger;

	[NonSerialized]
	public IAnimator Animator;

	[NonSerialized]
	public EEmitType EmitType;

	[NonSerialized]
	public Queue<Struct254> DelayedEvents = new Queue<Struct254>();

	[NonSerialized]
	public HashSet<GInterface137> AnimationEventsStateBehaviours = new HashSet<GInterface137>();

	[NonSerialized]
	public AnimationEventsSequenceData EventsSequenceData_1 = new AnimationEventsSequenceData();

	public AnimationEventsSequenceData EventsSequenceData => EventsSequenceData_1;

	public event Action<int, AnimationEventParameter> OnEventAction;

	public void SetAnimator(IAnimator animator, EEmitType emitType, string distinctId = "undefined", bool force = false)
	{
		if (Animator != animator || force)
		{
			RemoveBindedAnimator();
			Animator = animator;
			EmitType = emitType;
			if (Animator != null)
			{
				Logger = new GClass715(this, LoggerMode.Add, distinctId);
				method_0();
			}
		}
	}

	public void RemoveBindedAnimator()
	{
		if (Animator != null)
		{
			method_1();
		}
		Animator = null;
	}

	public void Dispose()
	{
		method_1();
	}

	public void SubscribeToAnimatorEvent(GInterface137 behaviour)
	{
		behaviour.EventsContainer.SetEventsConsumer(this);
		AnimationEventsStateBehaviours.Add(behaviour);
	}

	public void UnsubscribeToAnimatorEvent(GInterface137 behaviour)
	{
		behaviour.EventsContainer.UnsetEventsConsumer(this);
		AnimationEventsStateBehaviours.Remove(behaviour);
	}

	public void method_0()
	{
		GInterface137[] behaviours = Animator.GetBehaviours<GInterface137>();
		foreach (GInterface137 behaviour in behaviours)
		{
			SubscribeToAnimatorEvent(behaviour);
		}
	}

	public void method_1()
	{
		if (AnimationEventsStateBehaviours == null)
		{
			return;
		}
		foreach (GInterface137 animationEventsStateBehaviour in AnimationEventsStateBehaviours)
		{
			animationEventsStateBehaviour.EventsContainer.UnsetEventsConsumer(this);
		}
		AnimationEventsStateBehaviours.Clear();
	}

	public void ReceiveEvent(AnimationEvent animationEvent, IAnimator animator, in AnimatorStateInfoWrapper animatorStateInfo, float normalizedTime)
	{
		if (EmitType == EEmitType.EmitOnDemand)
		{
			Logger.EventPuttedToDelayedEventsQueue(animationEvent, animator, in animatorStateInfo, normalizedTime);
			DelayedEvents.Enqueue(new Struct254(animationEvent, animator, in animatorStateInfo, normalizedTime));
		}
		else
		{
			method_3(animationEvent, animator, in animatorStateInfo, normalizedTime);
		}
	}

	void IEventsConsumer.ReceiveEvent(AnimationEvent animationEvent, IAnimator animator, in AnimatorStateInfoWrapper animatorStateInfo, float normalizedTime)
	{
		//ILSpy generated this explicit interface implementation from .override directive in ReceiveEvent
		this.ReceiveEvent(animationEvent, animator, in animatorStateInfo, normalizedTime);
	}

	public void method_2(string functionName, int functionNameHash, AnimationEventParameter parameter, int stateShortNameHash)
	{
		EventsSequenceData_1.AddAnimationEventToDebugQueue(functionName, stateShortNameHash, conditionPassed: true);
		if (this.OnEventAction != null)
		{
			this.OnEventAction(functionNameHash, parameter);
		}
		else
		{
			Logger.WarnNoHandler(functionName);
		}
	}

	public void EmitEvents()
	{
		if (EmitType != EEmitType.EmitOnDemand)
		{
			Logger.LogError("Wrong AnimationEventsEmmiter using, it must have EmitType == EmitOnDemand");
		}
		while (DelayedEvents.Count > 0)
		{
			Struct254 @struct = DelayedEvents.Dequeue();
			AnimationEvent animationEvent = @struct.AnimationEvent;
			IAnimator animator = @struct.Animator;
			float normalizedTime = @struct.NormalizedTime;
			AnimatorStateInfoWrapper stateInfo = @struct.StateInfo;
			method_3(animationEvent, animator, in stateInfo, normalizedTime);
		}
	}

	public void method_3(AnimationEvent @event, IAnimator animator, in AnimatorStateInfoWrapper stateInfo, float normalizedTime)
	{
		bool flag = @event.IsConditionsSucceed(animator);
		Logger.TraceEventTestPassed(@event, flag, normalizedTime, animator, in stateInfo);
		if (flag)
		{
			method_2(@event.FunctionName, @event.FunctionNameHash, @event.Parameter, stateInfo.shortNameHash);
		}
		else
		{
			EventsSequenceData_1.AddAnimationEventToDebugQueue(@event.FunctionName, stateInfo.shortNameHash, conditionPassed: false);
		}
	}
}
