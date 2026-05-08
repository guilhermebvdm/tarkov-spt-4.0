using System;
using System.Collections.Generic;
using Audio.AmbientSubsystem.Data;
using UnityEngine;

namespace Audio.AmbientSubsystem;

public class AmbientSoundPlayerGroupController : MonoBehaviour
{
	[SerializeField]
	private List<AmbientSoundPlayerGroup> _dayGroup;

	[SerializeField]
	private List<AmbientSoundPlayerGroup> _nightGroup;

	private readonly Dictionary<EDayTime, List<AmbientSoundPlayerGroup>> dictionary_0 = new Dictionary<EDayTime, List<AmbientSoundPlayerGroup>>(2);

	private Action action_0;

	public void Awake()
	{
		dictionary_0[EDayTime.Day] = _dayGroup;
		dictionary_0[EDayTime.Night] = _nightGroup;
		action_0 = (Action)Delegate.Combine(action_0, GlobalEventHandlerClass.Instance.SubscribeOnEvent<GClass3571>(method_0));
	}

	public void method_0(GClass3571 blendEvent)
	{
		EDayTimeBlendState dayTimeBlendState = blendEvent.DayTimeBlendState;
		method_1(EDayTime.Day, dayTimeBlendState);
		method_1(EDayTime.Night, dayTimeBlendState);
	}

	public void method_1(EDayTime dayTime, EDayTimeBlendState blendState)
	{
		bool flag = method_2(dayTime, blendState);
		if (!dictionary_0.TryGetValue(dayTime, out var value))
		{
			return;
		}
		foreach (AmbientSoundPlayerGroup item in value)
		{
			if (item != null)
			{
				item.gameObject.SetActive(flag);
				if (flag)
				{
					item.Play();
				}
				else
				{
					item.Stop();
				}
			}
		}
	}

	public bool method_2(EDayTime targetDayTime, EDayTimeBlendState blendState)
	{
		switch (blendState)
		{
		default:
			Debug.LogError($"can't recognised current blend state: {blendState}");
			return false;
		case EDayTimeBlendState.None:
			return false;
		case EDayTimeBlendState.DayBlendStart:
			return true;
		case EDayTimeBlendState.DayBlendEnd:
			return targetDayTime == EDayTime.Day;
		case EDayTimeBlendState.NightBlendStart:
			return true;
		case EDayTimeBlendState.NightBlendEnd:
			return targetDayTime == EDayTime.Night;
		}
	}

	public void OnDestroy()
	{
		action_0?.Invoke();
		action_0 = null;
	}
}
