using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT.Interactive;
using EFT.UI;
using EFT.UI.BattleTimer;
using UnityEngine;

namespace EFT;

[GAttribute8(-500)]
public abstract class AbstractGame : MonoBehaviour, IGame, IDisposable
{
	protected const string HALLOWEEN_PREFAB_PATH = "Prefabs/HALLOWEEN_CONTROLLER";

	private bool bool_0 = true;

	private readonly WaitForFixedUpdate waitForFixedUpdate_0 = new WaitForFixedUpdate();

	protected readonly CompositeDisposableClass CompositeDisposable = new CompositeDisposableClass();

	[CompilerGenerated]
	private GameStatus gameStatus_0;

	[CompilerGenerated]
	private GameTimerClass gameTimerClass;

	[CompilerGenerated]
	private EGameType egameType_0;

	private float float_0;

	private bool bool_1;

	[CompilerGenerated]
	private EUpdateQueue eupdateQueue_0;

	[CompilerGenerated]
	private static Action<EGameType> action_0;

	public const float MAX_FIXED_DELTA_TIME = 0.05f;

	[CompilerGenerated]
	private Action<string, float?> action_1;

	public GameStatus Status
	{
		[CompilerGenerated]
		get
		{
			return gameStatus_0;
		}
		[CompilerGenerated]
		set
		{
			gameStatus_0 = value;
		}
	}

	public GameTimerClass GameTimer
	{
		[CompilerGenerated]
		get
		{
			return gameTimerClass;
		}
		[CompilerGenerated]
		set
		{
			gameTimerClass = value;
		}
	}

	public EGameType GameType
	{
		[CompilerGenerated]
		get
		{
			return egameType_0;
		}
		[CompilerGenerated]
		set
		{
			egameType_0 = value;
		}
	}

	public float PastTime
	{
		get
		{
			if (!bool_1)
			{
				float_0 = GClass1893.PastTimeSeconds(GameTimer);
				bool_1 = true;
			}
			return float_0;
		}
	}

	public EUpdateQueue UpdateQueue
	{
		[CompilerGenerated]
		get
		{
			return eupdateQueue_0;
		}
		[CompilerGenerated]
		set
		{
			eupdateQueue_0 = value;
		}
	}

	public float FixedDeltaTime
	{
		get
		{
			return Time.fixedDeltaTime;
		}
		set
		{
			if (value <= 0.05f)
			{
				Time.fixedDeltaTime = value;
				Debug.LogFormat("Fixed DT = {0}", FixedDeltaTime);
			}
			else
			{
				Debug.LogErrorFormat("Attemption to set to low fixed dt '{0}'", value);
			}
		}
	}

	public virtual float LastServerTimeStamp => 0f;

	public virtual int LastServerFrameId => 0;

	public abstract string LocationObjectId { get; }

	public bool InRaid
	{
		get
		{
			if (!(this is Interface13))
			{
				return this is LocalGame;
			}
			return true;
		}
	}

	public abstract GameUI GameUi { get; }

	public abstract string ProfileId { get; }

	GameObject IGame.gameObject => base.gameObject;

	public static event Action<EGameType> OnGameTypeSetted
	{
		[CompilerGenerated]
		add
		{
			Action<EGameType> action = action_0;
			Action<EGameType> action2;
			do
			{
				action2 = action;
				Action<EGameType> value2 = (Action<EGameType>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<EGameType> action = action_0;
			Action<EGameType> action2;
			do
			{
				action2 = action;
				Action<EGameType> value2 = (Action<EGameType>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<string, float?> OnMatchingStatusChanged
	{
		[CompilerGenerated]
		add
		{
			Action<string, float?> action = action_1;
			Action<string, float?> action2;
			do
			{
				action2 = action;
				Action<string, float?> value2 = (Action<string, float?>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<string, float?> action = action_1;
			Action<string, float?> action2;
			do
			{
				action2 = action;
				Action<string, float?> value2 = (Action<string, float?>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public static TGame Create<TGame>(EUpdateQueue updateQueue, TimeSpan? sessionTime = null, string goName = "GAME") where TGame : AbstractGame
	{
		TGame val = new GameObject(goName).AddComponent<TGame>();
		val.GameTimer = new GameTimerClass(sessionTime);
		val.UpdateQueue = updateQueue;
		EGameType gameType = ((val is HideoutGame) ? EGameType.Hideout : ((!(val is LocalGame)) ? ((val is EftNetworkGame) ? EGameType.Online : ((val is NarrateGame) ? EGameType.Narrate : val.GameType)) : EGameType.Offline));
		val.GameType = gameType;
		action_0?.Invoke(val.GameType);
		val.StartCoroutine(val.method_0());
		return val;
	}

	public virtual void Dispose()
	{
		CompositeDisposable.Dispose();
		UnityEngine.Object.DestroyImmediate(base.gameObject);
	}

	public IEnumerator method_0()
	{
		while (true)
		{
			yield return waitForFixedUpdate_0;
			LateFixedUpdate();
			bool_0 = true;
		}
	}

	public virtual void FixedUpdate()
	{
		if (bool_0)
		{
			bool_0 = false;
		}
		else
		{
			StartCoroutine(method_0());
		}
	}

	public virtual void LateFixedUpdate()
	{
	}

	public virtual void LateUpdate()
	{
		bool_1 = false;
	}

	public void ShowNewSecretExit(ExfiltrationPoint point, EExfiltrationStatus prevStatus)
	{
		if (prevStatus == EExfiltrationStatus.Hidden)
		{
			point.OnStatusChanged -= ShowNewSecretExit;
			GameUi.TimerPanel.SwitchTimers(point.Settings.Name, showOnePoint: true);
			GameUi.TimerPanel.ShowTimer(showExits: true);
		}
	}

	public void UpdateExfiltrationUi(ExfiltrationPoint point, bool contains, bool initial = false)
	{
		ExitTriggerSettings settings = point.Settings;
		bool flag = false;
		if (contains)
		{
			flag = point.HasMetRequirements(ProfileId);
		}
		float num = ((point.Status == EExfiltrationStatus.Countdown) ? (settings.ExfiltrationTime - ((point.ExfiltrationStartTime > 0f) ? (PastTime - point.ExfiltrationStartTime) : 0f)) : (flag ? settings.ExfiltrationTime : (0f - PastTime)));
		if (contains && point.Status != EExfiltrationStatus.NotPresent)
		{
			if (settings.PlayersCount > 0)
			{
				GameUi.BattleUiPmcCount.Show(settings.PlayersCount, point.QueuedPlayers.Count);
			}
			if (flag)
			{
				GameUi.BattleUiPanelExitTrigger.Show(num);
			}
			else
			{
				GameUi.BattleUiPanelExitTrigger.Close();
			}
		}
		GameUi.TimerPanel.UpdateExfiltrationTimers(point, flag, contains, initial, num, point.Status);
		GameUi.TimerPanel.SetMainTimerState(point.Settings.Name, flag ? EMainTimerState.StayInEpInside : EMainTimerState.FindEp);
	}

	public void OnEpInteraction(ExfiltrationPoint point, bool entered)
	{
		ExitTriggerSettings settings = point.Settings;
		bool flag = point.QueuedPlayers.Contains(ProfileId);
		if (entered)
		{
			UpdateExfiltrationUi(point, point.Entered.Any((Player x) => x.ProfileId == ProfileId));
			GameUi.TimerPanel.SwitchTimers(point.Settings.Name, showOnePoint: true);
			return;
		}
		GameUi.BattleUiPmcCount.Close();
		GameUi.BattleUiPanelExitTrigger.Close();
		if (flag)
		{
			GameUi.TimerPanel.ShowTimer(showExits: true);
		}
		if (string.IsNullOrEmpty(settings.Name))
		{
			return;
		}
		if (point.Status == EExfiltrationStatus.RegularMode)
		{
			GameUi.TimerPanel.UpdateExfiltrationTimers(point, availability: true, point.Entered.Any((Player x) => x.ProfileId == ProfileId), initial: false, settings.MaxTime * 60f - PastTime, point.Status);
		}
		GameUi.TimerPanel.SetMainTimerState(point.Settings.Name, flag ? EMainTimerState.StayInEpOutside : EMainTimerState.FindEp);
	}

	public virtual void SetMatchmakerStatus(string status, float? progress = null)
	{
	}

	public void InvokeMatchingStatusChanged(string status, float? progress = null)
	{
		action_1?.Invoke(status, progress);
	}

	public AbstractGame()
	{
	}

	[CompilerGenerated]
	public bool method_1(Player x)
	{
		return x.ProfileId == ProfileId;
	}

	[CompilerGenerated]
	public bool method_2(Player x)
	{
		return x.ProfileId == ProfileId;
	}
}
