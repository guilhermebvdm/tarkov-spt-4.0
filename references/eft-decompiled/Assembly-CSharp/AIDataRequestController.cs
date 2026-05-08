using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AIDataRequestController
{
	[CompilerGenerated]
	public class Class278
	{
		public BotRequestType type;

		public bool method_0(BotRequest x)
		{
			return x.BotRequestType == type;
		}
	}

	[CompilerGenerated]
	public class Class279
	{
		public BotRequest exeptThis;

		public bool method_0(BotRequest x)
		{
			return x != exeptThis;
		}
	}

	[NonSerialized]
	public List<BotRequest> CurrentRequests = new List<BotRequest>();

	[NonSerialized]
	public float NextCheckRequestsToEnd;

	[NonSerialized]
	public PlayerAIDataClass AiData;

	public int RequestsCount => CurrentRequests.Count;

	[field: NonSerialized]
	public BotRequest ActivatedRequest { get; set; }

	public AIDataRequestController(PlayerAIDataClass aiData)
	{
		AiData = aiData;
		NextCheckRequestsToEnd = Time.time + GClass856.Random(0f, 4f);
	}

	public bool TryAdd(BotRequest request, BotGroupRequestController groupRequestController)
	{
		if (!CanAddByCount(request.BotRequestType))
		{
			return false;
		}
		if (!request.SetGroup(groupRequestController))
		{
			return false;
		}
		if (AiData.IsAI && request.Requester.Id != AiData.BotOwner.GetPlayer.Id)
		{
			Debug.LogError($"wrong requester data: + request.Requester.Id:{request.Requester.Id}   _aiData.BotOwner.GetPlayer.Id:{AiData.BotOwner.GetPlayer.Id}");
		}
		CurrentRequests.Add(request);
		return true;
	}

	public bool CanAddByCount(BotRequestType type)
	{
		if (AiData.IAmBoss)
		{
			return true;
		}
		int num = LocalBotSettingsProviderClass.Core.MAX_BASE_REQUESTS_PER_PLAYER;
		switch (type)
		{
		case BotRequestType.wait:
			num = LocalBotSettingsProviderClass.Core.MAX_WAIT_REQUESTS_PER_PLAYER;
			break;
		case BotRequestType.getInCover:
			num = LocalBotSettingsProviderClass.Core.MAX_GET_IN_COVER_REQUESTS_PER_PLAYER;
			break;
		case BotRequestType.followMe:
			num = LocalBotSettingsProviderClass.Core.MAX_COME_WITH_ME_REQUESTS_PER_PLAYER;
			break;
		case BotRequestType.hold:
			num = LocalBotSettingsProviderClass.Core.MAX_HOLD_REQUESTS_PER_PLAYER;
			break;
		case BotRequestType.goToPoint:
			num = LocalBotSettingsProviderClass.Core.MAX_GO_TO_REQUESTS_PER_PLAYER;
			break;
		}
		return CurrentRequests.Count((BotRequest x) => x.BotRequestType == type) < num;
	}

	public void MyRequestTaken(BotRequest request)
	{
		if (request.Requester.AIData.IsAI && request.DisposeOtherRequestsWhenTaken)
		{
			DisposeAll(request);
		}
		ActivatedRequest = request;
	}

	public void CheckOnEndByRequester()
	{
		if (!(NextCheckRequestsToEnd < Time.time))
		{
			return;
		}
		NextCheckRequestsToEnd = Time.time + 3.5f;
		if (CurrentRequests.Count != 0)
		{
			BotRequest[] array = CurrentRequests.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].CheckIsEndBeforeTake();
			}
		}
	}

	public void Dispose(BotRequest request)
	{
		if (!CurrentRequests.Remove(request))
		{
			if (AiData.IsAI && request.Removed)
			{
			}
		}
		else
		{
			request.Removed = true;
		}
		if (ActivatedRequest != null)
		{
			ActivatedRequest = null;
		}
	}

	public void DisposeAll(BotRequest exeptThis = null)
	{
		BotRequest[] array = ((exeptThis != null) ? CurrentRequests.Where((BotRequest x) => x != exeptThis).ToArray() : CurrentRequests.ToArray());
		for (int num = 0; num < array.Length; num++)
		{
			array[num].Dispose();
		}
	}

	public void Dispose()
	{
		DisposeAll();
	}
}
