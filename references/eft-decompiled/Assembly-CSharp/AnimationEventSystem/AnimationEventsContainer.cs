using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NLog;
using UnityEngine;

namespace AnimationEventSystem;

[Serializable]
public class AnimationEventsContainer : ICloneable
{
	public enum EUpdateType
	{
		OnEnter,
		OnUpdate,
		OnExit
	}

	public class Class392 : LoggerClass
	{
		public struct Struct253
		{
			public int stateHash;

			public EUpdateType UpdateType;
		}

		[NonSerialized]
		public Struct253 Struct253_0 = new Struct253
		{
			stateHash = 0
		};

		public Class392()
			: base("anim-events-container", LoggerMode.Add)
		{
		}

		public void LogEventsCheck(IAnimator animator, in AnimatorStateInfoWrapper stateInfo, int layerIndex, EUpdateType updateType, float previousNormalizedTime)
		{
			if (method_3(in stateInfo, updateType))
			{
				Struct253_0.stateHash = stateInfo.fullPathHash;
				Struct253_0.UpdateType = updateType;
				LogLevel trace = LogLevel.Trace;
				string stateName = animator.GetStateName(in stateInfo);
				if (animator.IsInTransition(layerIndex))
				{
					AnimatorStateInfoWrapper stateInfo2 = animator.GetCurrentAnimatorStateInfo(layerIndex);
					AnimatorStateInfoWrapper stateInfo3 = animator.GetNextAnimatorStateInfo(layerIndex);
					Log("[{0}][{1}::{2}:({7}:{3})][{4} -> {5}][{6}]", "<b>[{0}][{1}::{2}:({7}:{3})]</b><color=cyan>[{4} -> {5}]</color>[{6}]", trace, Time.frameCount, stateName, updateType, stateInfo.normalizedTime, animator.GetStateName(in stateInfo2), animator.GetStateName(in stateInfo3), animator, previousNormalizedTime);
				}
				else
				{
					Log("[{0}][{1}::{2}:({3}:{4})][{5}]", "<b>[{0}][{1}::{2}:({3}:{4})]</b>[{5}]", trace, Time.frameCount, stateName, updateType, previousNormalizedTime, stateInfo.normalizedTime, animator);
				}
			}
		}

		public bool method_3(in AnimatorStateInfoWrapper stateInfo, EUpdateType updateType)
		{
			if (IsEnabled(LogLevel.Trace))
			{
				return true;
			}
			if (IsEnabled(LogLevel.Debug) && (Struct253_0.stateHash != stateInfo.fullPathHash || Struct253_0.UpdateType != updateType))
			{
				return true;
			}
			return false;
		}

		public void DebugAnimatorsSwitch(IAnimator previousAnimator, IAnimator animator, in AnimatorStateInfoWrapper stateInfo, int layerIndex, EUpdateType updateType)
		{
			if (IsEnabled(LogLevel.Debug))
			{
				string stateName = animator.GetStateName(in stateInfo);
				Log("[{0}][{1}:{2}][Animator switched: {3} -> {4}][{4}]", "[{0}][{1}:{2}][Animator switched: {3} -> {4}][{4}]", LogLevel.Debug, Time.frameCount, stateName, updateType, previousAnimator, animator);
			}
		}

		public void LogEventTimeCheck(AnimationEvent animationEvent, float previousNormalizedTime, float normalizedTime)
		{
			if (IsEnabled(LogLevel.Debug))
			{
				Log(LogLevel.Trace, "[events container][time-check][{0}] {1} [{2}:{3}]", Time.frameCount, animationEvent, previousNormalizedTime, normalizedTime);
			}
		}
	}

	[NonSerialized]
	public const float ALMOST_ONE = 0.99999f;

	[NonSerialized]
	public const float A_BIT_GREATER_THAN_ONE = 1.00001f;

	[NonSerialized]
	public const float MIN_DIFF = 1E-05f;

	[NonSerialized]
	public static Class392 Logger = new Class392();

	[NonSerialized]
	public bool PreviousLoop;

	[NonSerialized]
	public float PreviousNormalizedTime;

	[NonSerialized]
	public IAnimator PreviousAnimator;

	[NonSerialized]
	public IEventsConsumer EventsConsumer;

	[NonSerialized]
	public bool HasEventsAndConsumerCache;

	public virtual void ResetCache()
	{
		PreviousLoop = false;
		PreviousNormalizedTime = 0f;
	}

	public void SetEventsConsumer([NotNull] IEventsConsumer consumer)
	{
		if (EventsConsumer != null)
		{
			Logger.LogError("Attempting to set events consumer '{0}' while it's '{1}' already set", consumer, EventsConsumer);
		}
		EventsConsumer = consumer;
		HasEventsAndConsumerCache = method_1();
	}

	public void UnsetEventsConsumer([NotNull] IEventsConsumer consumer)
	{
		if (EventsConsumer != consumer)
		{
			Logger.LogError("Attempting to unset events consumer '{0}', while it's not equal to new one'{1}'", consumer, EventsConsumer);
		}
		EventsConsumer = null;
		HasEventsAndConsumerCache = method_1();
	}

	public object Clone()
	{
		return new AnimationEventsContainer();
	}

	public void OnStateEnter(IAnimator animator, in AnimatorStateInfoWrapper stateInfo, int layerIndex, List<AnimationEvent> animationEvents)
	{
		method_0(animator, in stateInfo, layerIndex, EUpdateType.OnEnter, animationEvents);
	}

	public void OnStateUpdate(IAnimator animator, in AnimatorStateInfoWrapper stateInfo, int layerIndex, List<AnimationEvent> animationEvents)
	{
		method_0(animator, in stateInfo, layerIndex, EUpdateType.OnUpdate, animationEvents);
	}

	public void OnStateExit(IAnimator animator, in AnimatorStateInfoWrapper stateInfo, int layerIndex, List<AnimationEvent> animationEvents)
	{
		method_0(animator, in stateInfo, layerIndex, EUpdateType.OnExit, animationEvents);
	}

	public void method_0(IAnimator animator, in AnimatorStateInfoWrapper stateInfo, int layerIndex, EUpdateType updateType, List<AnimationEvent> animationEvents)
	{
		if (updateType == EUpdateType.OnExit && PreviousAnimator != animator && PreviousAnimator != null)
		{
			Logger.DebugAnimatorsSwitch(PreviousAnimator, animator, in stateInfo, layerIndex, updateType);
			PreviousAnimator = animator;
			return;
		}
		PreviousAnimator = animator;
		Logger.LogEventsCheck(animator, in stateInfo, layerIndex, updateType, PreviousNormalizedTime);
		if (!HasEventsAndConsumerCache)
		{
			return;
		}
		int num = (int)PreviousNormalizedTime;
		int num2 = (int)stateInfo.normalizedTime;
		float previousNormalizedTime = ((PreviousLoop || num <= 0) ? (PreviousNormalizedTime - (float)num) : 1f);
		float normalizedTime = stateInfo.normalizedTime - (float)num2;
		if (animator.IsInTransition(layerIndex) && animator.GetNextAnimatorStateInfo(layerIndex).fullPathHash != stateInfo.fullPathHash)
		{
			method_2(animator, in stateInfo, previousNormalizedTime, 0.99999f, animationEvents);
			PreviousNormalizedTime = 0.99999f;
			return;
		}
		if (stateInfo.loop)
		{
			if (updateType == EUpdateType.OnExit && num2 == 1 && (num2 < num || PreviousNormalizedTime > stateInfo.normalizedTime))
			{
				method_2(animator, in stateInfo, previousNormalizedTime, 1.00001f, animationEvents);
			}
			else if (num == num2)
			{
				method_2(animator, in stateInfo, previousNormalizedTime, normalizedTime, animationEvents);
			}
			else if (num < num2)
			{
				method_2(animator, in stateInfo, previousNormalizedTime, 1.00001f, animationEvents);
				if (updateType != EUpdateType.OnExit)
				{
					method_2(animator, in stateInfo, 0f, normalizedTime, animationEvents);
				}
			}
			PreviousNormalizedTime = stateInfo.normalizedTime;
		}
		else
		{
			float num3 = Mathf.Clamp01(PreviousNormalizedTime);
			float num4 = Mathf.Clamp01(stateInfo.normalizedTime);
			if (num == 0 && num2 == 0)
			{
				if (num3 != num4)
				{
					method_2(animator, in stateInfo, num3, num4, animationEvents);
				}
			}
			else if (num == 0 && num2 > 0 && num3 < 1f)
			{
				method_2(animator, in stateInfo, previousNormalizedTime, 1.00001f, animationEvents);
			}
			PreviousNormalizedTime = Mathf.Clamp(stateInfo.normalizedTime, 0f, 0.99999f);
			if (updateType == EUpdateType.OnExit)
			{
				PreviousNormalizedTime = 0f;
			}
		}
		PreviousLoop = stateInfo.loop;
	}

	public bool method_1()
	{
		if (EventsConsumer == null)
		{
			return false;
		}
		return true;
	}

	public void method_2(IAnimator animator, in AnimatorStateInfoWrapper stateInfo, float previousNormalizedTime, float normalizedTime, List<AnimationEvent> animationEvents)
	{
		if (animator == null)
		{
			Debug.LogError("Can't check events time animator is null");
			return;
		}
		if (animationEvents == null)
		{
			Debug.LogError(string.Format("Can't check events time {0} is null, {1}:{2}", "animationEvents", "animator", animator));
			return;
		}
		int count = animationEvents.Count;
		for (int i = 0; i < count; i++)
		{
			AnimationEvent animationEvent = animationEvents[i];
			if (animationEvent == null)
			{
				Debug.LogError(string.Format("Can't check events time {0}[{1}] is null, {2}:{3}", "animationEvent", i, "animator", animator));
			}
			else if (animationEvent.IsTimeToFire(previousNormalizedTime, normalizedTime))
			{
				Logger.LogEventTimeCheck(animationEvent, previousNormalizedTime, normalizedTime);
				method_3(animationEvent, animator, in stateInfo, normalizedTime);
			}
		}
	}

	public void method_3(AnimationEvent @event, IAnimator animator, in AnimatorStateInfoWrapper stateInfo, float normalizedTime)
	{
		if (EventsConsumer != null)
		{
			EventsConsumer.ReceiveEvent(@event, animator, in stateInfo, normalizedTime);
		}
	}
}
