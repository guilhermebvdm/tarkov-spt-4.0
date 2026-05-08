using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using UnityEngine;
using UnityEngine.AI;

public class BotGroupRequestController
{
	[Serializable]
	[CompilerGenerated]
	public class Class268
	{
		public static readonly Class268 class268_0 = new Class268();

		public static Action action_0;

		public void method_0()
		{
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class269<T> where T : BotRequest
	{
		public static readonly Class269<T> class269_0 = new Class269<T>();

		public static Func<BotRequest, bool> func_0;

		public bool method_0(BotRequest x)
		{
			return x is T;
		}
	}

	[CompilerGenerated]
	public class Class270
	{
		public Player requester;

		public bool method_0(BotRequest x)
		{
			return x.Requester == requester;
		}
	}

	[NonSerialized]
	public List<BotRequest> ListOfRequests = new List<BotRequest>();

	[NonSerialized]
	public BotsGroup BotGroup;

	public int RequestsCount => ListOfRequests.Count;

	public event Action<BotRequest> OnAddRequest;

	public BotGroupRequestController(BotsGroup botGroup)
	{
		BotGroup = botGroup;
	}

	public int GetRequestsCount<T>() where T : BotRequest
	{
		return ListOfRequests.Count((BotRequest x) => x is T);
	}

	public void FindForMe(BotOwner executer)
	{
		BotRequest botRequest = null;
		foreach (BotRequest listOfRequest in ListOfRequests)
		{
			if ((listOfRequest.CanExecuteByMyself || listOfRequest.Requester != executer.GetPlayer) && (!executer.Boss.IamBoss || executer.Boss.AllowRequestSelf || executer.GetPlayer.Id != listOfRequest.Requester.Id) && listOfRequest.CanStartExecute(executer))
			{
				botRequest = listOfRequest;
				break;
			}
		}
		if (botRequest != null)
		{
			botRequest.Take(executer);
			ListOfRequests.Remove(botRequest);
		}
	}

	public void RemoveAllRequestByRequester(Player requester)
	{
		BotRequest[] array = ListOfRequests.Where((BotRequest x) => x.Requester == requester).ToArray();
		for (int num = 0; num < array.Length; num++)
		{
			array[num].Dispose();
		}
	}

	public bool TryAddRequest(BotRequest request)
	{
		if (ListOfRequests.Count > LocalBotSettingsProviderClass.Core.MAX_REQUESTS__PER_GROUP)
		{
			return false;
		}
		ListOfRequests.Add(request);
		this.OnAddRequest?.Invoke(request);
		for (int i = 0; i < BotGroup.MembersCount; i++)
		{
			BotGroup.Member(i).BotRequestController.ResetTimer();
		}
		return true;
	}

	public bool TryActivateSuppressionRequest(IPlayer requester, EnemyInfo enemy)
	{
		GClass597 gClass = new GClass597(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(requester.ProfileId), enemy);
		if (gClass.CanRequest(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(requester.ProfileId)))
		{
			return requester.AIData.AskRequests.TryAdd(gClass, this);
		}
		return false;
	}

	public bool TryActivateThrowGrenadeRequest(IPlayer requester, Vector3 placeToThrow, ThrowWeapType? throwType, out GClass601 request)
	{
		request = new GClass601(requester, placeToThrow, throwType);
		if (request.CanRequest(requester))
		{
			return requester.AIData.AskRequests.TryAdd(request, this);
		}
		return false;
	}

	public bool TryActivateThrowGrenadeRequest(IPlayer requester, AIGreanageThrowData throwData, out GClass601 request)
	{
		request = new GClass601(requester, throwData);
		if (request.CanRequest(requester))
		{
			return requester.AIData.AskRequests.TryAdd(request, this);
		}
		return false;
	}

	public bool TryActivateGoToPointRequest(IPlayer requester, Vector3 point, Action completeCallback = null, Action disposeCallback = null)
	{
		NavMeshPath navMeshPath = new NavMeshPath();
		NavMesh.CalculatePath(requester.Position, point, -1, navMeshPath);
		if (navMeshPath.status == NavMeshPathStatus.PathComplete)
		{
			GClass593 gClass = new GClass593(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(requester.ProfileId), point, completeCallback, disposeCallback);
			if (gClass.CanRequest(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(requester.ProfileId)))
			{
				return requester.AIData.AskRequests.TryAdd(gClass, this);
			}
		}
		return false;
	}

	public bool TryActivateThrowGrenadeRequest(IPlayer requester, Player targetToThrow, bool onlyCached)
	{
		GClass600 gClass = new GClass600(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(requester.ProfileId), targetToThrow, onlyCached, delegate
		{
		});
		if (gClass.CanRequest(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(requester.ProfileId)))
		{
			return requester.AIData.AskRequests.TryAdd(gClass, this);
		}
		return false;
	}

	public bool TryAskSuppressionRequest(IPlayer requester, EnemyInfo player)
	{
		GClass597 gClass = new GClass597(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(requester.ProfileId), player);
		if (gClass.CanRequest(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(requester.ProfileId)))
		{
			return requester.AIData.AskRequests.TryAdd(gClass, this);
		}
		return false;
	}

	public bool TryAskHideRequest(IPlayer requester)
	{
		GClass594 gClass = new GClass594(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(requester.ProfileId));
		if (gClass.CanRequest(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(requester.ProfileId)))
		{
			return requester.AIData.AskRequests.TryAdd(gClass, this);
		}
		return false;
	}

	public void TryAskHideRequest(IPlayer player, BotOwner posibleExecuter)
	{
		if (posibleExecuter.BotRequestController.TryStopCurrent(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.ProfileId)))
		{
			GClass594 gClass = new GClass594(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.ProfileId));
			if (method_0(player, gClass, posibleExecuter))
			{
				gClass.AddPossibleExecutors(posibleExecuter);
			}
			else
			{
				posibleExecuter.BotRequestController.TrySayNegative(player, gClass.BotRequestType);
			}
		}
	}

	public bool TryAskFollowMeRequest(IPlayer player, BotOwner posibleExecuter)
	{
		if (posibleExecuter.BotRequestController.TryStopCurrent(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.ProfileId)))
		{
			GClass592 gClass = new GClass592(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.ProfileId));
			if (method_0(player, gClass, posibleExecuter))
			{
				gClass.AddPossibleExecutors(posibleExecuter);
				return true;
			}
			posibleExecuter.BotRequestController.TrySayNegative(player, gClass.BotRequestType);
		}
		return false;
	}

	public void TryActivateSuppressionRequest(IPlayer player, BotOwner posibleExecuter)
	{
		if (posibleExecuter.Memory.LastEnemy == null)
		{
			return;
		}
		Player alivePlayerByProfileID = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.ProfileId);
		if (posibleExecuter.BotRequestController.TryStopCurrent(alivePlayerByProfileID))
		{
			GClass597 gClass = new GClass597(alivePlayerByProfileID, posibleExecuter.Memory.LastEnemy);
			if (method_0(player, gClass, posibleExecuter))
			{
				gClass.AddPossibleExecutors(posibleExecuter);
			}
			else
			{
				posibleExecuter.BotRequestController.TrySayNegative(player, gClass.BotRequestType);
			}
		}
	}

	public bool TryActivateGetInCover(IPlayer player, BotOwner posibleExecuter, Vector3? nearestPoint, float period = -1f)
	{
		Player alivePlayerByProfileID = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.ProfileId);
		if (posibleExecuter.BotRequestController.TryStopCurrent(alivePlayerByProfileID))
		{
			Class280 @class = new Class280(alivePlayerByProfileID, nearestPoint, period);
			if (method_0(alivePlayerByProfileID, @class, posibleExecuter))
			{
				@class.AddPossibleExecutors(posibleExecuter);
				return true;
			}
			posibleExecuter.BotRequestController.TrySayNegative(alivePlayerByProfileID, @class.BotRequestType);
		}
		return false;
	}

	public bool TryActivateThrowGrenadeRequest(IPlayer requester, BotOwner posibleExecuter, ThrowWeapType? throwType = null)
	{
		if (posibleExecuter.Memory.LastEnemy != null)
		{
			Vector3 enemyLastPosition = posibleExecuter.Memory.LastEnemy.EnemyLastPosition;
			if (posibleExecuter.BotRequestController.TryStopCurrent(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(requester.ProfileId)))
			{
				GClass601 gClass = new GClass601(requester, enemyLastPosition, throwType);
				if (method_0(requester, gClass, posibleExecuter))
				{
					gClass.AddPossibleExecutors(posibleExecuter);
					return true;
				}
				posibleExecuter.BotRequestController.TrySayNegative(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(requester.ProfileId), gClass.BotRequestType);
			}
		}
		return false;
	}

	public void TryAskHoldRequest(IPlayer player, BotOwner posibleExecuter)
	{
		if (posibleExecuter.BotRequestController.CurRequest is GClass595 gClass)
		{
			Vector3 lookDirection = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.ProfileId).MovementContext.LookDirection;
			gClass.SetDirection(lookDirection, player.PlayerBones.Head.position);
		}
		else if (posibleExecuter.BotRequestController.TryStopCurrent(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.ProfileId)))
		{
			Vector3 lookDirection2 = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.ProfileId).MovementContext.LookDirection;
			GClass595 gClass2 = new GClass595(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.ProfileId), lookDirection2);
			if (method_0(player, gClass2, posibleExecuter))
			{
				gClass2.AddPossibleExecutors(posibleExecuter);
			}
		}
	}

	public void TryActivateGoToCheckRequest(IPlayer player, BotOwner posibleExecuter)
	{
		if (posibleExecuter.Memory.LastEnemy == null)
		{
			return;
		}
		Vector3 enemyLastPosition = posibleExecuter.Memory.LastEnemy.EnemyLastPosition;
		if (posibleExecuter.BotRequestController.TryStopCurrent(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.ProfileId)))
		{
			GClass593 gClass = new GClass593(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.ProfileId), enemyLastPosition, null, null);
			if (method_0(player, gClass, posibleExecuter))
			{
				gClass.AddPossibleExecutors(posibleExecuter);
			}
			else
			{
				posibleExecuter.BotRequestController.TrySayNegative(player, gClass.BotRequestType);
			}
		}
	}

	public void RemoveRequest(BotRequest botRequest)
	{
		ListOfRequests.Remove(botRequest);
	}

	public bool TryActivateThrowGrenadeRequest(IPlayer requester, Player targetToThrow, bool onlyCached = false, Action callbackFinish = null)
	{
		GClass600 gClass = new GClass600(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(requester.ProfileId), targetToThrow, onlyCached, callbackFinish);
		if (gClass.CanRequest(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(requester.ProfileId)))
		{
			return requester.AIData.AskRequests.TryAdd(gClass, this);
		}
		return false;
	}

	public bool TryActivateWait(IPlayer player, BotOwner posibleExecuter)
	{
		Player alivePlayerByProfileID = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.ProfileId);
		if (posibleExecuter.BotRequestController.TryStopCurrent(alivePlayerByProfileID.GetPlayer))
		{
			Class281 @class = new Class281(alivePlayerByProfileID.GetPlayer);
			if (@class.CanRequest(alivePlayerByProfileID.GetPlayer))
			{
				@class.AddPossibleExecutors(posibleExecuter);
				return alivePlayerByProfileID.AIData.AskRequests.TryAdd(@class, this);
			}
		}
		return false;
	}

	public bool TryActivateOpenDoorRequest(IPlayer requester, Door door, Action completeCallback)
	{
		GClass596 gClass = new GClass596(door, Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(requester.ProfileId), completeCallback);
		if (gClass.CanRequest(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(requester.ProfileId)))
		{
			return requester.AIData.AskRequests.TryAdd(gClass, this);
		}
		return false;
	}

	public bool TryActivateThrowGrenadePlaceRequest(IPlayer requester, Player targetToThrow, Action callbackFinish = null)
	{
		if (targetToThrow.AIData.PlaceInfo == null)
		{
			return false;
		}
		ThrowGrenadePlace throwGrenadePlace = GClass856.RandomElement(targetToThrow.AIData.PlaceInfo.GrenadePlaces);
		GClass596 gClass = null;
		if (throwGrenadePlace != null && throwGrenadePlace.HaveDoor && throwGrenadePlace.Door.DoorState != EDoorState.Open)
		{
			gClass = new GClass596(throwGrenadePlace.Door, Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(requester.ProfileId));
			if (!requester.AIData.AskRequests.TryAdd(gClass, this))
			{
				return false;
			}
		}
		GClass599 gClass2 = new GClass599(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(requester.ProfileId), throwGrenadePlace, gClass, callbackFinish);
		if (gClass2.CanRequest(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(requester.ProfileId)))
		{
			return requester.AIData.AskRequests.TryAdd(gClass2, this);
		}
		return false;
	}

	public bool method_0(IPlayer player, BotRequest request, BotOwner posibleExecuter)
	{
		if (posibleExecuter.ArtilleryDangerPlace.ShallRunAway())
		{
			return false;
		}
		if (posibleExecuter.BewareGrenade.ShallRunAway())
		{
			return false;
		}
		if (posibleExecuter.DangerPointsData.IsPanic)
		{
			return false;
		}
		if (posibleExecuter.BotRequestController.CurRequest == null && posibleExecuter.AIData.AskRequests.RequestsCount == 0)
		{
			return posibleExecuter.BotRequestController.IsAvailableByTime(request.BotRequestType) & player.AIData.AskRequests.TryAdd(request, this);
		}
		return false;
	}
}
