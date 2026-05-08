using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class BotPeacefulActions : GClass429
{
	[CompilerGenerated]
	public class Class263
	{
		public BotPeacefulActions botPeacefulActions_0;

		public EInteraction gestus;

		public GClass413 pairData;

		public void method_0()
		{
			botPeacefulActions_0.LastGesture = gestus;
			botPeacefulActions_0.method_0(pairData);
			botPeacefulActions_0.OnStartPeacefulMove?.Invoke(pairData);
		}
	}

	[NonSerialized]
	public const float STANDART_DELAY = 1f;

	[NonSerialized]
	public List<EInteraction> List = new List<EInteraction>
	{
		EInteraction.OkGesture,
		EInteraction.FriendlyGesture,
		EInteraction.ComeWithMeGesture,
		EInteraction.HoldGesture,
		EInteraction.ThereGesture,
		EInteraction.GetOffGesture
	};

	[NonSerialized]
	public float NextGusture;

	[NonSerialized]
	public GClass413 ActiveData;

	[NonSerialized]
	public EInteraction LastGesture = EInteraction.FriendlyGesture;

	[NonSerialized]
	public bool ErrorInited;

	public BotGlobalPatrolSettings BotGlobalPatrolSettings_0 => BotOwner_0.Settings.FileSettings.Patrol;

	public event Action<GClass413> OnStartPeacefulMove;

	public BotPeacefulActions(BotOwner owner)
		: base(owner)
	{
	}

	public void StartDo(GClass413 pairData)
	{
		float num = 1f;
		bool shallStop = false;
		EInteraction gestus = GClass856.RandomElement(List);
		bool flag = false;
		bool flag2 = false;
		if (pairData.Person1.AIData.IsAI)
		{
			if (GClass856.IsTrue100(pairData.Person1.AIData.BotOwner.Settings.FileSettings.Patrol.CHANCE_TO_PLAY_GESTURE_WHEN_CLOSE))
			{
				flag = true;
			}
			if (pairData.Person1.AIData.BotOwner.Settings.FileSettings.Patrol.FORCE_OPPONENT_TO_PEAEFUL)
			{
				flag2 = true;
			}
			if (GClass856.IsTrue100(pairData.Person1.AIData.BotOwner.Settings.FileSettings.Patrol.CHANCE_TO_PLAY_VOICE_WHEN_CLOSE))
			{
				pairData.Person1.AIData.BotOwner.BotTalk.TrySay(EPhraseTrigger.Roger, withGroupDelay: true);
			}
			float gESTURE_LENGTH = pairData.Person1.AIData.BotOwner.Settings.FileSettings.Patrol.GESTURE_LENGTH;
			if (pairData.Person1.AIData.BotOwner.Settings.FileSettings.Patrol.SHALL_STOP_IN_PEACEFUL_ACTION)
			{
				shallStop = true;
			}
			num = Mathf.Max(num, gESTURE_LENGTH);
		}
		if (pairData.Person2.AIData.IsAI)
		{
			if (GClass856.IsTrue100(pairData.Person2.AIData.BotOwner.Settings.FileSettings.Patrol.CHANCE_TO_PLAY_GESTURE_WHEN_CLOSE))
			{
				flag2 = true;
			}
			if (pairData.Person2.AIData.BotOwner.Settings.FileSettings.Patrol.FORCE_OPPONENT_TO_PEAEFUL)
			{
				flag = true;
			}
			if (GClass856.IsTrue100(pairData.Person2.AIData.BotOwner.Settings.FileSettings.Patrol.CHANCE_TO_PLAY_VOICE_WHEN_CLOSE))
			{
				pairData.Person2.AIData.BotOwner.BotTalk.TrySay(EPhraseTrigger.OnMutter, withGroupDelay: true);
			}
			float gESTURE_LENGTH2 = pairData.Person2.AIData.BotOwner.Settings.FileSettings.Patrol.GESTURE_LENGTH;
			if (pairData.Person2.AIData.BotOwner.Settings.FileSettings.Patrol.SHALL_STOP_IN_PEACEFUL_ACTION)
			{
				shallStop = true;
			}
			num = Mathf.Max(num, gESTURE_LENGTH2);
		}
		pairData.SetAction(EBotPairActionType.clap, num, shallStop);
		float delay = -1f;
		float delay2 = -1f;
		if (flag && flag2)
		{
			if (pairData.Person1.AIData.BotOwner.Boss.IamBoss)
			{
				delay2 = 1f;
			}
			else if (pairData.Person2.AIData.BotOwner.Boss.IamBoss)
			{
				delay = 1f;
			}
			else if (GClass856.IsTrue100(50f))
			{
				delay2 = 1f;
			}
			else
			{
				delay = 1f;
			}
		}
		if (flag)
		{
			pairData.Person1.AIData.BotOwner.PeacefulActions.SetActiveAction(pairData, gestus, delay);
		}
		if (flag2)
		{
			pairData.Person2.AIData.BotOwner.PeacefulActions.SetActiveAction(pairData, gestus, delay2);
		}
	}

	public void SetActiveAction(GClass413 pairData, EInteraction gestus, float delay = -1f)
	{
		Class263 CS_0024_003C_003E8__locals10 = new Class263();
		CS_0024_003C_003E8__locals10.botPeacefulActions_0 = this;
		CS_0024_003C_003E8__locals10.gestus = gestus;
		CS_0024_003C_003E8__locals10.pairData = pairData;
		if (delay > 0f)
		{
			BotOwner_0.AITaskManager.RegisterDelayedTask(BotOwner_0, delay, delegate
			{
				CS_0024_003C_003E8__locals10.botPeacefulActions_0.LastGesture = CS_0024_003C_003E8__locals10.gestus;
				CS_0024_003C_003E8__locals10.botPeacefulActions_0.method_0(CS_0024_003C_003E8__locals10.pairData);
				CS_0024_003C_003E8__locals10.botPeacefulActions_0.OnStartPeacefulMove?.Invoke(CS_0024_003C_003E8__locals10.pairData);
			});
		}
		else
		{
			CS_0024_003C_003E8__locals10.method_0();
		}
	}

	public bool HaveActions()
	{
		if (ActiveData != null && ActiveData.ActionType != EBotPairActionType.none)
		{
			return ActiveData.AllAlive();
		}
		return false;
	}

	public void UpdateAction()
	{
		try
		{
			if (ActiveData.ShallStop)
			{
				BotOwner_0.StopMove();
			}
			IPlayer another = ActiveData.GetAnother(BotOwner_0);
			BotOwner_0.Steering.LookToPoint(another.Position + Vector3.up * 1.5f);
			if (NextGusture < Time.time)
			{
				NextGusture = Time.time + 5f;
				BotOwner_0.Gesture.TryGestus(LastGesture, withAimingDelay: false);
			}
		}
		catch (Exception)
		{
			if (!ErrorInited)
			{
				ErrorInited = true;
			}
			method_0(null);
		}
	}

	public void RemovePlayer(IPlayer gamePerson)
	{
		if (ActiveData != null)
		{
			IPlayer another = ActiveData.GetAnother(BotOwner_0);
			if (another != null && another == gamePerson)
			{
				method_0(null);
			}
		}
	}

	public void method_0(GClass413 data)
	{
		if (ActiveData != null)
		{
			ActiveData.OnPairDataStops -= method_1;
		}
		ActiveData = data;
		if (ActiveData != null)
		{
			ActiveData.OnPairDataStops += method_1;
		}
	}

	public void method_1(GClass413 obj)
	{
		if (ActiveData != null)
		{
			ActiveData.OnPairDataStops -= method_1;
		}
		ActiveData = null;
	}

	public void Dispose()
	{
		this.OnStartPeacefulMove = null;
	}
}
