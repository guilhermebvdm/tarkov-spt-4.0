using System;
using System.Collections;
using System.Runtime.CompilerServices;
using CommonAssets.Scripts.Audio.RadioSystem;
using EFT.GlobalEvents.AudioEvents;
using UnityEngine;

public class GClass1504 : GInterface148
{
	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public BroadcastGrid BroadcastGrid_0;

	[NonSerialized]
	public MonoBehaviour MonoBehaviour_0;

	[NonSerialized]
	public BroadcastRule BroadcastRule_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1 = 1f;

	[NonSerialized]
	public Coroutine Coroutine_0;

	[NonSerialized]
	[CompilerGenerated]
	public ERadioStation EradioStation_0;

	[NonSerialized]
	[CompilerGenerated]
	public BroadcastItemData BroadcastItemData_0 = new BroadcastItemData();

	[NonSerialized]
	[CompilerGenerated]
	public float Float_2;

	[NonSerialized]
	[CompilerGenerated]
	public EBroadcastState EbroadcastState_0;

	public ERadioStation Station
	{
		[CompilerGenerated]
		get
		{
			return EradioStation_0;
		}
	}

	public BroadcastItemData CurrentBroadcastItem
	{
		[CompilerGenerated]
		get
		{
			return BroadcastItemData_0;
		}
		[CompilerGenerated]
		set
		{
			BroadcastItemData_0 = value;
		}
	}

	public float CurrentBroadcastTime
	{
		[CompilerGenerated]
		get
		{
			return Float_2;
		}
		[CompilerGenerated]
		set
		{
			Float_2 = value;
		}
	}

	public EBroadcastState BroadcastState
	{
		[CompilerGenerated]
		get
		{
			return EbroadcastState_0;
		}
		[CompilerGenerated]
		set
		{
			EbroadcastState_0 = value;
		}
	}

	public GClass1504(ERadioStation station, BroadcastItemsContainer container, BroadcastRule rule, MonoBehaviour coroutineBehaviour)
	{
		EradioStation_0 = station;
		BroadcastRule_0 = rule;
		BroadcastGrid_0 = new BroadcastGrid(container, rule);
		Bool_0 = rule.restartBroadcastAfterFinished;
		MonoBehaviour_0 = coroutineBehaviour;
	}

	public void StartBroadcast()
	{
		BroadcastState = EBroadcastState.Started;
		GClass7.TryStopCoroutine(MonoBehaviour_0, ref Coroutine_0);
		Coroutine_0 = MonoBehaviour_0.StartCoroutine(method_0());
	}

	public void Pause()
	{
		BroadcastState = EBroadcastState.Paused;
	}

	public IEnumerator method_0()
	{
		do
		{
			BroadcastGrid_0.RebuildGrid();
			while (BroadcastGrid_0.Count > 0)
			{
				if (BroadcastState == EBroadcastState.Started)
				{
					if (CurrentBroadcastTime >= CurrentBroadcastItem.clipLength - Float_1 && BroadcastGrid_0.TryGetBroadcastItemData(out var item))
					{
						if (item.itemType != EBroadcastItemType.None)
						{
							GlobalEventHandlerClass.CreateEvent<BroadcastItemChangedEvent>().Invoke(Station, item, Float_1);
						}
						CurrentBroadcastItem = item;
						CurrentBroadcastTime = 0f;
						Float_1 = BroadcastRule_0.GetCrossfadeDuration(CurrentBroadcastItem.itemType);
						Float_0 = Time.realtimeSinceStartup;
					}
					if (CurrentBroadcastItem.clipLength > 0f)
					{
						CurrentBroadcastTime = Time.realtimeSinceStartup - Float_0;
					}
				}
				yield return null;
			}
			yield return null;
		}
		while (BroadcastState == EBroadcastState.Started && Bool_0);
	}

	public void StopBroadcast()
	{
		BroadcastState = EBroadcastState.Stopped;
		if (MonoBehaviour_0 != null)
		{
			GClass7.TryStopCoroutine(MonoBehaviour_0, ref Coroutine_0);
		}
	}
}
