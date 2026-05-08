using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public abstract class BotRequest
{
	[NonSerialized]
	public AICoreActionEndStruct FinishNodeLogic = new AICoreActionEndStruct("Base logic");

	[NonSerialized]
	public AICoreActionEndStruct ContinueNodeLogic;

	public const float BASE_REQUEST_LIFETIME_SEC = 120f;

	public IPlayer Requester;

	public BotRequestType BotRequestType;

	public float EndTimeBeforeTaken;

	public float EndTimeByExecutor;

	public bool ShallEndByExecutor;

	public bool EndIfCantExecute;

	public bool DisposeOtherRequestsWhenTaken;

	[NonSerialized]
	public BotOwner Executor;

	[NonSerialized]
	public BotGroupRequestController GroupRequestController;

	[NonSerialized]
	public List<BotOwner> PossibleExecutors;

	[NonSerialized]
	public float TakenTime;

	[field: NonSerialized]
	public bool CanExecuteByMyself { get; set; }

	[field: NonSerialized]
	public bool Taken { get; set; }

	public abstract EBotRequestMode RequestMode { get; }

	public virtual float requestLifetime => 120f;

	[field: NonSerialized]
	public bool Removed { get; set; }

	public BotRequest(IPlayer requester, BotRequestType requestType)
	{
		BotRequestType = requestType;
		Requester = requester;
		EndTimeBeforeTaken = Time.time + 20f;
	}

	public virtual bool CanStartExecute(BotOwner executor)
	{
		if (executor.Settings.FileSettings.Mind.REACT_ADD_DRUNK_ENEMY)
		{
			if (Requester.AIData.IsDrunk)
			{
				return true;
			}
			return false;
		}
		if (!Requester.IsAI)
		{
			switch (Requester.Side)
			{
			case EPlayerSide.Usec:
				if (!executor.Settings.FileSettings.Mind.CAN_RECEIVE_PLAYER_REQUESTS_USEC)
				{
					return false;
				}
				break;
			case EPlayerSide.Bear:
				if (!executor.Settings.FileSettings.Mind.CAN_RECEIVE_PLAYER_REQUESTS_BEAR)
				{
					return false;
				}
				break;
			case EPlayerSide.Savage:
				if (!executor.Settings.FileSettings.Mind.CAN_RECEIVE_PLAYER_REQUESTS_SAVAGE)
				{
					return false;
				}
				break;
			}
		}
		if (executor.Medecine.Using)
		{
			return false;
		}
		if (PossibleExecutors == null)
		{
			return true;
		}
		if (!PossibleExecutors.Contains(executor))
		{
			return false;
		}
		return true;
	}

	public abstract bool CanRequest(BotOwner requester);

	public abstract bool CanProceed();

	public virtual void PlayerDestroy(Player player)
	{
	}

	public virtual void Take(BotOwner executor)
	{
		if (PossibleExecutors != null && PossibleExecutors.Contains(executor))
		{
			executor.Gesture.TryGestus(EInteraction.OkGesture, withAimingDelay: false);
		}
		Taken = true;
		float lastSetRequestTime = executor.BotRequestController.LastSetRequestTime;
		if (Time.time - lastSetRequestTime > 20f)
		{
			executor.BotTalk.Say(EPhraseTrigger.Roger);
			executor.Gesture.TryGestus(EInteraction.OkGesture, withAimingDelay: false);
		}
		executor.BotRequestController.SetCurrentRequest(this);
		Executor = executor;
		Requester.AIData.AskRequests.MyRequestTaken(this);
		executor.AIData.AskRequests.DisposeAll(this);
		Activate();
		TakenTime = Time.time;
	}

	public void SetEndTimeOnTake(float period)
	{
		ShallEndByExecutor = true;
		EndTimeByExecutor = Time.time + period;
	}

	public virtual void Activate()
	{
		EndTimeBeforeTaken = Time.time + requestLifetime;
	}

	public virtual bool CanRequest(IPlayer player)
	{
		return true;
	}

	public void CheckIsEndBeforeTake()
	{
		if (EndTimeBeforeTaken < Time.time)
		{
			GroupRequestController.RemoveRequest(this);
			Dispose();
		}
	}

	public virtual void Complete()
	{
		Dispose();
	}

	public void AddPossibleExecutors(BotOwner owner)
	{
		if (PossibleExecutors == null)
		{
			PossibleExecutors = new List<BotOwner>();
		}
		PossibleExecutors.Add(owner);
	}

	public void UseOnlySelf()
	{
		if (Requester.AIData.IsAI)
		{
			PossibleExecutors = new List<BotOwner>();
			PossibleExecutors.Add(Requester.AIData.BotOwner);
		}
	}

	public bool SetGroup(BotGroupRequestController groupRequestController)
	{
		GroupRequestController = groupRequestController;
		return GroupRequestController.TryAddRequest(this);
	}

	public void CheckSelfEnd()
	{
		if (ShallEndByExecutor && EndTimeByExecutor < Time.time)
		{
			Dispose();
		}
	}

	public string ExecutorInfo()
	{
		if (Executor != null)
		{
			return " id:" + Executor.Id + "Requester:" + Requester.Id;
		}
		return " no executor Requester:" + Requester.Id;
	}

	public virtual void Dispose()
	{
		if (Executor != null)
		{
			Executor.BotRequestController.CompleteRequest(this);
		}
		if (Requester != null)
		{
			Requester.AIData.AskRequests.Dispose(this);
		}
		GroupRequestController.RemoveRequest(this);
	}

	public virtual bool FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> func, bool checkCurrent, out CustomNavigationPoint customNavigationPoint)
	{
		customNavigationPoint = null;
		return false;
	}

	public virtual AICoreActionEndStruct EndHoldPosition()
	{
		return FinishNodeLogic;
	}
}
