using System;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT;
using UnityEngine;
using UnityEngine.Timeline;

namespace Cutscene;

public class BaseCutsceneTrigger : MonoBehaviour
{
	[CompilerGenerated]
	private Action<BaseCutsceneTrigger, Player> action_0;

	[CompilerGenerated]
	private Action<Player> action_1;

	[GAttribute16]
	[SerializeField]
	private int _cutsceneID = -1;

	[GAttribute16]
	[SerializeField]
	private Vector3 _startPosition;

	[GAttribute16]
	[SerializeField]
	private Vector3 _startViewDirection;

	[GAttribute16]
	[SerializeField]
	private float _startPlayerPosLevel;

	[GAttribute16]
	[SerializeField]
	private bool _needToProneAtStart;

	[GAttribute16]
	[SerializeField]
	private Vector3 _cutsceneEndPlayerPosition;

	public CutsceneFakePlayerSteps fakePlayerSteps;

	[SerializeField]
	private TimelineAsset _timelineAsset;

	[SerializeField]
	private bool _callServerTeleportOnEnd;

	private Player player_0;

	public int CutsceneID => _cutsceneID;

	public Vector3 StartPosition => _startPosition;

	public Vector3 StartViewDirection => _startViewDirection;

	public float StartPlayerPosLevel => _startPlayerPosLevel;

	public bool NeedToProneAtStart => _needToProneAtStart;

	public Vector3 CutsceneEndPlayerPosition => _cutsceneEndPlayerPosition;

	public TimelineAsset TimeLineAsset => _timelineAsset;

	public event Action<BaseCutsceneTrigger, Player> OnPlayerCausesCutscene
	{
		[CompilerGenerated]
		add
		{
			Action<BaseCutsceneTrigger, Player> action = action_0;
			Action<BaseCutsceneTrigger, Player> action2;
			do
			{
				action2 = action;
				Action<BaseCutsceneTrigger, Player> value2 = (Action<BaseCutsceneTrigger, Player>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<BaseCutsceneTrigger, Player> action = action_0;
			Action<BaseCutsceneTrigger, Player> action2;
			do
			{
				action2 = action;
				Action<BaseCutsceneTrigger, Player> value2 = (Action<BaseCutsceneTrigger, Player>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<Player> OnCutsceneEnded
	{
		[CompilerGenerated]
		add
		{
			Action<Player> action = action_1;
			Action<Player> action2;
			do
			{
				action2 = action;
				Action<Player> value2 = (Action<Player>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<Player> action = action_1;
			Action<Player> action2;
			do
			{
				action2 = action;
				Action<Player> value2 = (Action<Player>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public virtual void Awake()
	{
		GClass1048.Instance.AddActiveCutsceneTrigger(this);
		CutsceneTriggerStartInfoSO.StartPlayerValues playerStartInfo = CutsceneTriggerStartInfoSO.Instance.GetPlayerStartInfo(base.gameObject.scene.name, _cutsceneID);
		if (playerStartInfo != null)
		{
			_startPosition = playerStartInfo.startPosition;
			_startViewDirection = playerStartInfo.startViewDirection;
			_startPlayerPosLevel = playerStartInfo.startPlayerPosLevel;
			_needToProneAtStart = playerStartInfo.needToProneAtStart;
			_cutsceneEndPlayerPosition = playerStartInfo.cutsceneEndPlayerPos;
		}
	}

	public void CallStartCutscene(IPlayer player)
	{
		if (player.IsYourPlayer)
		{
			player_0 = GamePlayerOwner.MyPlayer;
			action_0?.Invoke(this, GamePlayerOwner.MyPlayer);
		}
	}

	public void CallEndCutscene()
	{
		action_1?.Invoke(player_0);
		player_0 = null;
		if (_callServerTeleportOnEnd)
		{
			GlobalEventHandlerClass.Instance.CreateCommonEvent<GClass3563>().Invoke(base.gameObject.scene.name, CutsceneID);
		}
	}
}
