using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.GlobalEvents;
using EFT.Interactive;
using EFT.UI;
using UnityEngine;

public class HalloweenEventControllerClass : IDisposable
{
	[NonSerialized]
	public CancellationTokenSource CancellationTokenSource_0 = new CancellationTokenSource();

	[NonSerialized]
	public int Int_0 = 65;

	[NonSerialized]
	public int Int_1 = 5;

	[NonSerialized]
	public GClass1908 Gclass1908_0;

	[NonSerialized]
	public GClass1908 Gclass1908_1;

	[NonSerialized]
	public GClass1908 Gclass1908_2;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public Action Action_0;

	[NonSerialized]
	public Action Action_1;

	[NonSerialized]
	public DateTime DateTime_0;

	[NonSerialized]
	public DateTime DateTime_1;

	[NonSerialized]
	public EEventState EeventState_0;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public string String_0;

	[NonSerialized]
	public HalloweenEventVisual HalloweenEventVisual_0;

	[NonSerialized]
	public Action Action_2;

	[CompilerGenerated]
	private static Action action_3;

	public static HalloweenEventControllerClass Instance
	{
		get
		{
			if (!(Singleton<GameWorld>.Instance == null))
			{
				return Singleton<GameWorld>.Instance.HalloweenEventController;
			}
			return null;
		}
	}

	public static event Action OnInitialized
	{
		[CompilerGenerated]
		add
		{
			Action action = action_3;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_3;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public HalloweenEventControllerClass()
	{
		method_0(CancellationTokenSource_0.Token).HandleExceptions();
	}

	public async Task method_0(CancellationToken cancellationToken)
	{
		if (Bool_0)
		{
			return;
		}
		Bool_0 = true;
		while (true)
		{
			if (Instance == null || Singleton<BotEventHandler>.Instance == null || Singleton<BackendConfigSettingsClass>.Instance == null)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					break;
				}
				await Task.Yield();
				continue;
			}
			Action_0 = GlobalEventHandlerClass.Instance.SubscribeOnEvent<HalloweenSummonStartedEvent>(method_5);
			Action_1 = GlobalEventHandlerClass.Instance.SubscribeOnEvent<HalloweenSyncStateEvent>(method_1);
			if (Singleton<BackendConfigSettingsClass>.Instantiated)
			{
				BackendConfigSettingsClass.GClass1746 eventSettings = Singleton<BackendConfigSettingsClass>.Instance.EventSettings;
				Gclass1908_1 = eventSettings.EventWeather;
				Gclass1908_0 = eventSettings.SummonSuccessWeather;
				Gclass1908_2 = eventSettings.SummonFailedWeather;
				Int_0 = eventSettings.EventTime;
				Int_1 = eventSettings.WeatherChangeTime;
			}
			Action_2 = GlobalEventHandlerClass.Instance.SubscribeOnEvent<HalloweenSyncExitsEvent>(method_4);
			action_3?.Invoke();
			break;
		}
	}

	public void method_1(HalloweenSyncStateEvent syncEvent)
	{
		SetEventState(syncEvent.EventState);
	}

	public void method_2(DateTime initialTime)
	{
		Gclass1908_2.Hour = initialTime.Hour;
		Gclass1908_2.Minute = initialTime.Minute;
		Bool_1 = true;
	}

	public void RegisterClientVisual(HalloweenEventVisual container)
	{
		method_0(CancellationTokenSource_0.Token).HandleExceptions();
		HalloweenEventVisual_0 = container;
	}

	public void HandleReconnect(SyncClientEventState syncEvent)
	{
		EeventState_0 = syncEvent.EventState;
		if (!GClass1673.IsNullOrEmpty(syncEvent.ExitName))
		{
			method_3(syncEvent.ExitName);
		}
		if (EeventState_0 != EEventState.NotStarted)
		{
			DateTime_1 = EFTDateTimeClass.UniversalDateTimeFromUnixTime(syncEvent.EventStartTimestamp);
			DateTime_0 = EFTDateTimeClass.UniversalDateTimeFromUnixTime(syncEvent.GameInitialTime);
			method_2(DateTime_0);
			Vector3_0 = syncEvent.RitualPosition;
			MonoBehaviourSingleton<GameUI>.Instance.TimerPanel.ForceUpdateExfiltrationPointsVisitedStatus();
			SetEventState(EeventState_0, force: true);
		}
	}

	public void method_3(string exitName)
	{
		String_0 = exitName;
		ExfiltrationPoint exfiltrationPoint = ExfiltrationControllerClass.Instance.ExfiltrationPoints.FirstOrDefault((ExfiltrationPoint ep) => ep.Settings.Name.Equals(String_0));
		if (!(exfiltrationPoint == null))
		{
			float exitTimeMultiplier = Singleton<BackendConfigSettingsClass>.Instance.EventSettings.ExitTimeMultiplier;
			exfiltrationPoint.Settings.ExfiltrationTime *= exitTimeMultiplier;
			MonoBehaviourSingleton<GameUI>.Instance.TimerPanel.ForceUpdateExfiltrationPointsVisitedStatus();
		}
	}

	public void method_4(HalloweenSyncExitsEvent exitsEvent)
	{
		method_3(exitsEvent.ExitName);
	}

	public void method_5(HalloweenSummonStartedEvent halloweenSummonStartedEvent)
	{
		Vector3_0 = halloweenSummonStartedEvent.PointPosition;
		SetEventState(EEventState.SummonStart);
	}

	public void SetEventState(EEventState eventState, bool force = false)
	{
		EeventState_0 = eventState;
		switch (eventState)
		{
		default:
			throw new ArgumentOutOfRangeException("eventState", eventState, null);
		case EEventState.NotStarted:
			method_9();
			break;
		case EEventState.Started:
			method_6(force);
			break;
		case EEventState.SummonStart:
			method_7(force);
			break;
		case EEventState.SummonSuccess:
			method_8(eventState, force);
			break;
		case EEventState.SummonFailed:
			method_8(eventState, force);
			break;
		}
	}

	public void method_6(bool force = false)
	{
		if (!Bool_1)
		{
			DateTime_0 = WeatherEventController.Instance.GameDateTime.Calculate();
		}
		method_2(DateTime_0);
		method_11(Gclass1908_1, force);
		method_10();
	}

	public void method_7(bool force = false)
	{
		if (force)
		{
			method_6(force: true);
		}
		else
		{
			DateTime_1 = EFTDateTimeClass.UtcNow;
		}
		MonoBehaviourSingleton<GameUI>.Instance.TimerPanel.SetEventTimerText(GClass2348.Localized(EEventState.SummonStart, EStringCase.None));
		MonoBehaviourSingleton<GameUI>.Instance.TimerPanel.ShowEventTimer(DateTime_1.AddSeconds(Int_0));
		Vector3[] positions = new Vector3[1] { Vector3_0 };
		HalloweenEventVisual_0.Initialize(positions);
		HalloweenEventVisual_0.StartEventVisual(Int_1, force);
	}

	public void method_8(EEventState state, bool force = false)
	{
		switch (state)
		{
		case EEventState.SummonFailed:
			method_11(Gclass1908_2, force);
			method_13();
			break;
		case EEventState.SummonSuccess:
			method_11(Gclass1908_0, force);
			break;
		}
		MonoBehaviourSingleton<GameUI>.Instance.TimerPanel.SetEventTimerText(GClass2348.Localized(state, EStringCase.None));
		MonoBehaviourSingleton<GameUI>.Instance.TimerPanel.ShowEventTimer(EFTDateTimeClass.UtcNow);
		HalloweenEventVisual_0.StopEventVisual(Int_1, force);
	}

	public void method_9()
	{
		if (EeventState_0 != EEventState.NotStarted)
		{
			HalloweenEventVisual_0.StopEventVisual(Int_1);
			method_12();
			method_13();
		}
	}

	public void method_10()
	{
		MonoBehaviourSingleton<GameUI>.Instance.TimerPanel.ForceUpdateExfiltrationPointsVisitedStatus();
	}

	public void method_11(GClass1908 desiredWeather, bool force = false)
	{
		method_13();
		if (desiredWeather == null)
		{
			desiredWeather = Gclass1908_1;
		}
		WeatherEventController.Instance.SecToChange = (force ? 2f : ((float)Int_1));
		WeatherEventController.Instance.SetWeather(desiredWeather);
		WeatherEventController.Instance.Activate(val: true, 0, force);
	}

	public void method_12()
	{
		WeatherEventController.Instance.SecToChange = Int_1;
		WeatherEventController.Instance.SetWeather(Gclass1908_2);
		WeatherEventController.Instance.Activate(val: true);
		method_13();
	}

	public void method_13()
	{
		WeatherEventController.Instance.EventActive = false;
		WeatherEventController.Instance.Activate(val: false);
		WeatherEventController.Instance.currentPercantage = 0f;
		GClass4.Instance.CurrentTime.LockCurrentTime = false;
	}

	public void DebugAllPlaces()
	{
		HalloweenEventVisual_0.DebugSpawnAllPlaces();
	}

	public void Dispose()
	{
		CancellationTokenSource_0.Cancel();
		Action_0?.Invoke();
		Action_1?.Invoke();
		Action_2?.Invoke();
	}

	[CompilerGenerated]
	public bool method_14(ExfiltrationPoint ep)
	{
		return ep.Settings.Name.Equals(String_0);
	}
}
