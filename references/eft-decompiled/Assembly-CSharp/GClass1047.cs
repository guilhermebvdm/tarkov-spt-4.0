using System;
using Cutscene;
using UnityEngine;
using uLipSync;

public class GClass1047
{
	public enum ESyncTime
	{
		Animation,
		AudioSystem
	}

	[NonSerialized]
	public Cutscene.AnimationEvent[] AnimationEvent_0;

	[NonSerialized]
	public Cutscene.AnimationEvent AnimationEvent_1;

	[NonSerialized]
	public Cutscene.AnimationEvent AnimationEvent_2;

	[NonSerialized]
	public ESyncTime EsyncTime_0 = ESyncTime.AudioSystem;

	[NonSerialized]
	public LipSyncPlayer LipSyncPlayer_0;

	public GClass1047(GameObject target, ESyncTime syncTime, Cutscene.AnimationEvent[] events)
	{
		EsyncTime_0 = syncTime;
		AnimationEvent_0 = events;
		LipSyncPlayer_0 = LipSyncPlayer.GetPlayer(target);
	}

	public void OnStateExit(float normalizedTime, float stateDuration)
	{
		if (LipSyncPlayer_0.IsPlaying && EsyncTime_0 == ESyncTime.Animation)
		{
			LipSyncPlayer_0.ContinueDetachedPlay();
		}
		AnimationEvent_1 = null;
	}

	public void OnStateUpdate(float normalizedTime, float stateDuration)
	{
		method_0(normalizedTime, stateDuration);
	}

	public void method_0(float normalizedTime, float stateDuration)
	{
		bool flag = false;
		if (EsyncTime_0 == ESyncTime.Animation && AnimationEvent_1 != null)
		{
			float num = method_1(AnimationEvent_1, normalizedTime, stateDuration);
			if (num >= 0f && num < 1f)
			{
				LipSyncPlayer_0.UpdatePlay(num);
			}
			else
			{
				LipSyncPlayer_0.StopPlay();
				AnimationEvent_1 = null;
			}
		}
		else if (EsyncTime_0 == ESyncTime.AudioSystem && AnimationEvent_1 != null && !LipSyncPlayer_0.IsPlaying)
		{
			AnimationEvent_1 = null;
			flag = true;
		}
		if (flag)
		{
			return;
		}
		int num2 = AnimationEvent_0.Length;
		for (int i = 0; i < num2; i++)
		{
			Cutscene.AnimationEvent animationEvent = AnimationEvent_0[i];
			if (animationEvent == null || AnimationEvent_1 == animationEvent)
			{
				continue;
			}
			float num3 = method_1(animationEvent, normalizedTime, stateDuration);
			if ((!(num3 >= 0.5f) || animationEvent != AnimationEvent_2) && num3 >= 0f && num3 < 1f)
			{
				AnimationEvent_1 = animationEvent;
				AnimationEvent_2 = animationEvent;
				BakedData backedDataForPlay = animationEvent.fire.GetBackedDataForPlay();
				LipSyncPlayer_0.StartPlay(backedDataForPlay);
				if (EsyncTime_0 == ESyncTime.AudioSystem)
				{
					LipSyncPlayer_0.ContinueDetachedPlay();
				}
			}
		}
	}

	public float method_1(Cutscene.AnimationEvent @event, float normalizedTime, float stateDuration)
	{
		return (normalizedTime - @event.time) * stateDuration / @event.duration;
	}
}
