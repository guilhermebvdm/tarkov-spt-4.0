using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using UnityEngine;

public class BotRequestController
{
	public const float PHRASE_DIST_SQRT = 25f;

	[NonSerialized]
	public const float DELTA_TIME_ACTIVATE_REQUEST_SEC = 10f;

	[NonSerialized]
	public const float DELTA_TIME_SEARCH_REQUEST_FO_SELF = 10f;

	public BotRequest CurRequest;

	[NonSerialized]
	public BotOwner Owner;

	[NonSerialized]
	public float NextFindTime;

	[NonSerialized]
	public float NextRequestTime;

	[NonSerialized]
	public float NextGoToPointRequestTime;

	[NonSerialized]
	public float NextThrowGrenadeRequestTime;

	[NonSerialized]
	public BotGroupRequestController GroupRequestController_1;

	[NonSerialized]
	public Dictionary<BotRequestType, float> BlockUntil = new Dictionary<BotRequestType, float>();

	[NonSerialized]
	public float LastSetRequestTime_1;

	public float LastSetRequestTime => LastSetRequestTime_1;

	public BotGroupRequestController GroupRequestController
	{
		get
		{
			if (GroupRequestController_1 == null)
			{
				GroupRequestController_1 = Owner.BotsGroup.RequestsController;
				GroupRequestController_1.OnAddRequest += method_0;
			}
			return GroupRequestController_1;
		}
	}

	public int GetRequestsCount<T>() where T : BotRequest
	{
		if (GroupRequestController_1 == null)
		{
			return 0;
		}
		return GroupRequestController_1.GetRequestsCount<T>();
	}

	public void method_0(BotRequest obj)
	{
		float num = (float)Owner.Id / 10f;
		float num2 = (float)(int)(num + 1f) - num;
		NextFindTime = Time.time + num2;
	}

	public BotRequestController(BotOwner owner)
	{
		Owner = owner;
		foreach (BotRequestType value in Enum.GetValues(typeof(BotRequestType)))
		{
			BlockUntil.Add(value, 0f);
		}
	}

	public bool TryThrowSuppressionRequest()
	{
		if (!method_1())
		{
			return false;
		}
		if (NextRequestTime < Time.time)
		{
			NextRequestTime = Time.time + 10f;
			return GroupRequestController.TryActivateSuppressionRequest(Owner, Owner.Memory.GoalEnemy);
		}
		return false;
	}

	public bool TryActivateGoToPointRequest(Vector3 point, Action completeCallback = null, Action disposeCallback = null)
	{
		if (method_1())
		{
			return GroupRequestController.TryActivateGoToPointRequest(Owner, point, completeCallback, disposeCallback);
		}
		return false;
	}

	public bool TryActivateThrowGrenadeRequest(Vector3 placeToThrow, ThrowWeapType? throwType, out GClass601 request)
	{
		if (method_1() && GroupRequestController != null && NextThrowGrenadeRequestTime < Time.time)
		{
			return GroupRequestController.TryActivateThrowGrenadeRequest(Owner, placeToThrow, throwType, out request);
		}
		request = null;
		return false;
	}

	public bool TryActivateOpenDoorRequest(Door door, Action completeCallback)
	{
		if (method_1())
		{
			return GroupRequestController.TryActivateOpenDoorRequest(Owner, door, completeCallback);
		}
		return false;
	}

	public bool TryActivateThrowGrenadeRequestToPlace(Player targetToThrow, Action callbackFinish = null)
	{
		IPlayer owner = Owner;
		if (method_1() && NextThrowGrenadeRequestTime < Time.time)
		{
			NextThrowGrenadeRequestTime = Time.time + 10f;
		}
		return GroupRequestController.TryActivateThrowGrenadePlaceRequest(owner, targetToThrow, callbackFinish);
	}

	public bool TryActivateThrowGrenadeRequest(Player targetToThrow, bool onlyCached = false, Action callbackFinish = null)
	{
		IPlayer owner = Owner;
		if (method_1() && NextThrowGrenadeRequestTime < Time.time)
		{
			NextThrowGrenadeRequestTime = Time.time + 10f;
			bool flag = !onlyCached;
			if (onlyCached && targetToThrow.AIData.PlaceInfo != null && targetToThrow.AIData.PlaceInfo.GrenadePlaces.Length != 0)
			{
				ThrowGrenadePlace throwGrenadePlace = GClass600.FindPlace(targetToThrow);
				if (throwGrenadePlace != null && throwGrenadePlace.GetData().CanThrow)
				{
					flag = true;
				}
			}
			if (flag)
			{
				return GroupRequestController.TryActivateThrowGrenadeRequest(owner, targetToThrow, onlyCached, callbackFinish);
			}
			return false;
		}
		return false;
	}

	public void TrySayNegative(IPlayer player, BotRequestType rejectedRequest)
	{
		if (CurRequest == null || CurRequest.Requester != Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.ProfileId) || CurRequest.BotRequestType != rejectedRequest)
		{
			Owner.Gesture.TryGestus(EInteraction.NoGesture, withAimingDelay: false);
			Owner.BotTalk.Say(EPhraseTrigger.Negative, sayImmediately: true);
		}
	}

	public void Update()
	{
		method_2();
		Owner.AIData.AskRequests.CheckOnEndByRequester();
	}

	public void TryToFind()
	{
		if (Owner.Settings.FileSettings.Mind.CAN_EXECUTE_REQUESTS && !(NextFindTime > Time.time) && (CurRequest == null || CurRequest.BotRequestType == BotRequestType.goToPoint))
		{
			NextFindTime = Time.time + 10f;
			GroupRequestController?.FindForMe(Owner);
		}
	}

	public void SetCurrentRequest(BotRequest possibleRequest)
	{
		CurRequest = possibleRequest;
		LastSetRequestTime_1 = Time.time;
	}

	public void CompleteRequest(BotRequest botRequest)
	{
		if (botRequest != CurRequest)
		{
			Debug.LogError("stop not active request!!  CurRequest:" + CurRequest?.ToString() + "    trying:" + botRequest?.ToString() + "  executer:" + botRequest.ExecutorInfo());
		}
		CurRequest = null;
		botRequest.Requester.AIData.AskRequests.Dispose(botRequest);
	}

	public void ResetTimer()
	{
		NextFindTime = 0f;
	}

	public void RemoveAllRequestByRequester(Player getPlayer)
	{
		if (CurRequest != null && CurRequest.Requester == getPlayer)
		{
			Dispose();
		}
	}

	public bool HaveActivatedRequests()
	{
		return CurRequest != null;
	}

	public bool TryStopCurrent(Player player, bool withTalk = true)
	{
		if (HaveActivatedRequests())
		{
			if (CurRequest.Requester != player && (CurRequest.Requester != Owner || !CurRequest.CanExecuteByMyself))
			{
				if (withTalk)
				{
					Owner.BotTalk.Say(EPhraseTrigger.Negative, sayImmediately: true);
					Owner.Gesture.TryGestus(EInteraction.NoGesture, withAimingDelay: false);
				}
				return false;
			}
			withTalk = CurRequest == null;
			CurRequest.Dispose();
			if (withTalk)
			{
				Owner.BotTalk.Say(EPhraseTrigger.Roger, sayImmediately: true);
				Owner.Gesture.TryGestus(EInteraction.OkGesture, withAimingDelay: false);
			}
			return true;
		}
		return true;
	}

	public void DoNotAcceptRequests(BotRequest lastRequestFailed)
	{
		BlockUntil[lastRequestFailed.BotRequestType] = Time.time + 30f;
	}

	public bool IsAvailableByTime(BotRequestType brt)
	{
		return BlockUntil[brt] < Time.time;
	}

	public bool method_1()
	{
		if (Owner.Settings.FileSettings.Mind.CAN_THROW_REQUESTS)
		{
			return CurRequest == null;
		}
		return false;
	}

	public void method_2()
	{
		CurRequest?.CheckSelfEnd();
	}

	public void Dispose()
	{
		if (GroupRequestController_1 != null)
		{
			GroupRequestController_1.OnAddRequest -= method_0;
		}
		CurRequest?.Dispose();
	}
}
