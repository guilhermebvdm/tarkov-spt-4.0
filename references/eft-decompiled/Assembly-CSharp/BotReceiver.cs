using System;
using Comfort.Common;
using EFT;
using UnityEngine;

public class BotReceiver : GClass429
{
	[NonSerialized]
	public const float BIG_SDIST = 10000f;

	[NonSerialized]
	public const float DELAY_FollowMe = 120f;

	[NonSerialized]
	public const float DELAY_GetInCover = 120f;

	[NonSerialized]
	public const float DELAY_NeedHelp = 120f;

	[NonSerialized]
	public const float DELAY_Silence = 120f;

	[NonSerialized]
	public const float DELAY_Spreadout = 120f;

	[NonSerialized]
	public const float DELAY_Stop = 120f;

	[NonSerialized]
	public const float MED_SDIST = 625f;

	[NonSerialized]
	public const float SMALL_SDIST = 100f;

	[NonSerialized]
	public const float STANDART_DELAY = 120f;

	[NonSerialized]
	public const float TALK_DELAY = 30f;

	[NonSerialized]
	public GClass624<int, EPhraseTrigger, float> NextPossibleUse = new GClass624<int, EPhraseTrigger, float>();

	[NonSerialized]
	public GClass624<int, EPhraseTrigger, bool> ForeverBlock = new GClass624<int, EPhraseTrigger, bool>();

	public static bool CanReceiveFromPoint(Vector3 p, Vector3 from, Vector3 dir, float cos)
	{
		Vector3 vector = p - from;
		if (!(dir == Vector3.zero) && !(vector == Vector3.zero))
		{
			return GClass855.IsAngLessNormalized(dir, GClass855.NormalizeFastSelf(vector), cos);
		}
		return false;
	}

	public BotReceiver(BotOwner owner)
		: base(owner)
	{
	}

	public void Init()
	{
		Singleton<BotEventHandler>.Instance.OnQETilt += method_4;
		Singleton<BotEventHandler>.Instance.OnGestusShow += method_6;
		Singleton<BotEventHandler>.Instance.OnPhraseSay += method_0;
		Singleton<BotEventHandler>.Instance.OnHardAimDelegate += method_3;
		_ = (EPhraseTrigger[])Enum.GetValues(typeof(EPhraseTrigger));
	}

	public void method_0(BotEventHandler.GClass692 info)
	{
		if (!method_5(info.PlayerRequester) || (NextPossibleUse.TryGet(info.PlayerRequester.Id, info.phrase, out var val) && val > Time.time) || (ForeverBlock.TryGet(info.PlayerRequester.Id, info.phrase, out var val2) && val2))
		{
			return;
		}
		float num = 0f;
		switch (info.phrase)
		{
		default:
			return;
		case EPhraseTrigger.Silence:
		case EPhraseTrigger.NeedHelp:
			num = 10000f;
			break;
		case EPhraseTrigger.FollowMe:
		case EPhraseTrigger.Stop:
		case EPhraseTrigger.GetInCover:
			num = 100f;
			break;
		case EPhraseTrigger.Spreadout:
			num = 625f;
			break;
		}
		if ((BotOwner_0.Position - info.PlayerRequester.Transform.position).sqrMagnitude > num)
		{
			return;
		}
		switch (info.phrase)
		{
		case EPhraseTrigger.Silence:
			if (method_2(info))
			{
				method_1(info, 15f, 30f);
				BotOwner_0.BotTalk.SetSilence(30f);
			}
			NextPossibleUse.Set(info.PlayerRequester.Id, info.phrase, Time.time + 120f);
			break;
		case EPhraseTrigger.Stop:
			if (method_2(info))
			{
				BotOwner_0.BotsGroup.RequestsController.TryActivateWait(info.PlayerRequester, BotOwner_0);
			}
			else
			{
				GClass426 request = new GClass426(info.PlayerRequester, EInteraction.GetOffGesture, new GClass425(EInteraction.GetOffGesture));
				BotOwner_0.Gesture.TryAddRequest(request, force: false, withCheckDist: true);
				ForeverBlock.Set(info.PlayerRequester.Id, info.phrase, val: true);
			}
			NextPossibleUse.Set(info.PlayerRequester.Id, info.phrase, Time.time + 120f);
			break;
		case EPhraseTrigger.FollowMe:
			if (method_2(info))
			{
				BotOwner_0.BotsGroup.RequestsController.TryAskFollowMeRequest(info.PlayerRequester, BotOwner_0);
			}
			NextPossibleUse.Set(info.PlayerRequester.Id, info.phrase, Time.time + 120f);
			break;
		default:
			NextPossibleUse.Set(info.PlayerRequester.Id, info.phrase, Time.time + 120f);
			break;
		case EPhraseTrigger.NeedHelp:
			if (method_2(info))
			{
				method_1(info, 15f, 30f);
			}
			NextPossibleUse.Set(info.PlayerRequester.Id, info.phrase, Time.time + 120f);
			break;
		case EPhraseTrigger.GetInCover:
			if (method_2(info))
			{
				method_1(info, 5f, 30f);
			}
			else
			{
				GClass426 request2 = new GClass426(info.PlayerRequester, EInteraction.GetOffGesture, new GClass425(EInteraction.GetOffGesture));
				BotOwner_0.Gesture.TryAddRequest(request2, force: false, withCheckDist: true);
				ForeverBlock.Set(info.PlayerRequester.Id, info.phrase, val: true);
			}
			NextPossibleUse.Set(info.PlayerRequester.Id, info.phrase, Time.time + 120f);
			break;
		case EPhraseTrigger.Spreadout:
			if (method_2(info))
			{
				method_1(info, 10f, 30f);
			}
			NextPossibleUse.Set(info.PlayerRequester.Id, info.phrase, Time.time + 120f);
			break;
		}
	}

	public void method_1(BotEventHandler.GClass692 info, float distToCenter, float period)
	{
		int num = info.UsedTimes;
		if (GClass369.BaseDirections.Count <= num)
		{
			int num2 = num / GClass369.BaseDirections.Count;
			num = Mathf.Clamp(num - num2 * GClass369.BaseDirections.Count, 0, info.UsedTimes - 1);
		}
		Vector3 value = GClass369.BaseDirections[num] * distToCenter + info.PlayerRequester.Position;
		BotOwner_0.BotsGroup.RequestsController.TryActivateGetInCover(info.PlayerRequester, BotOwner_0, value, period);
	}

	public bool method_2(BotEventHandler.GClass692 info)
	{
		if (BotOwner_0.Settings.FileSettings.Mind.REACT_ADD_DRUNK_ENEMY)
		{
			if (info.PlayerRequester.AIData.IsDrunk)
			{
				return true;
			}
			return false;
		}
		if (info.CalcChanceByLoyalty())
		{
			info.UsedTimes++;
			return true;
		}
		return false;
	}

	public void method_3(IPlayer player, bool status)
	{
		if (method_5(player) && status)
		{
			BotOwner_0.PeaceHardAim.TryAddRequest(player);
		}
	}

	public void method_4(IPlayer player)
	{
		if (method_5(player))
		{
			BotOwner_0.FriendlyTilt.TryAddRequest(player);
		}
	}

	public bool method_5(IPlayer player)
	{
		if (player.AIData == null)
		{
			return false;
		}
		if (player.AIData.IsAI)
		{
			return false;
		}
		if (BotOwner_0.BotsGroup.IsEnemy(player))
		{
			return false;
		}
		if (Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.ProfileId) == BotOwner_0.GetPlayer)
		{
			return false;
		}
		if (BotOwner_0.Settings.FileSettings.Mind.REACT_ADD_DRUNK_ENEMY)
		{
			if (player.AIData.IsDrunk)
			{
				return true;
			}
			return false;
		}
		if (BotOwner_0.Settings.FileSettings.Mind.CAN_RECEIVE_PLAYER_REQUESTS_SAVAGE && player.Side == EPlayerSide.Savage)
		{
			return true;
		}
		if (BotOwner_0.Settings.FileSettings.Mind.CAN_RECEIVE_PLAYER_REQUESTS_BEAR && player.Side == EPlayerSide.Bear)
		{
			return true;
		}
		if (BotOwner_0.Settings.FileSettings.Mind.CAN_RECEIVE_PLAYER_REQUESTS_USEC && player.Side == EPlayerSide.Usec)
		{
			return true;
		}
		return false;
	}

	public void method_6(GClass532 data)
	{
		IPlayer player = data.Player;
		EInteraction gesture = data.Gesture;
		if (!method_5(player))
		{
			return;
		}
		bool flag = false;
		if (BotOwner_0.BotFollower.HaveBoss && BotOwner_0.BotFollower.BossToFollow.IsMe(player))
		{
			flag = true;
		}
		if ((!flag && (BotOwner_0.Position - player.Transform.position).sqrMagnitude > LocalBotSettingsProviderClass.Core.GESTUS_DIST_ANSWERS_SQRT) || (gesture == EInteraction.HoldGesture && BotOwner_0.BotRequestController.TryStopCurrent(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.ProfileId))))
		{
			return;
		}
		switch (gesture)
		{
		default:
		{
			GClass426 request = new GClass426(player, gesture, GClass425.Create(gesture));
			BotOwner_0.Gesture.TryAddRequest(request, force: false, withCheckDist: true);
			break;
		}
		case EInteraction.ComeWithMeGesture:
			BotOwner_0.BotsGroup.RequestsController.TryAskFollowMeRequest(player, BotOwner_0);
			break;
		case EInteraction.ThereGesture:
			if (flag)
			{
				BotOwner_0.BotsGroup.RequestsController.TryActivateSuppressionRequest(player, BotOwner_0);
			}
			else
			{
				method_8(player);
			}
			break;
		}
	}

	public bool method_7(bool comandFromMyBoss, bool addVal)
	{
		if (comandFromMyBoss)
		{
			BotOwner firstFollower = BotOwner_0.BotFollower.BossToFollow.GetFirstFollower(addVal);
			if (firstFollower != null && firstFollower == BotOwner_0)
			{
				return true;
			}
		}
		return false;
	}

	public void method_8(IPlayer player)
	{
		if (BotOwner_0.Memory.GoalEnemy == null)
		{
			BotOwner_0.BotsGroup.RequestsController.TryAskHoldRequest(player, BotOwner_0);
		}
		else
		{
			BotOwner_0.BotsGroup.RequestsController.TryActivateSuppressionRequest(player, BotOwner_0);
		}
	}

	public void Dispose()
	{
		Singleton<BotEventHandler>.Instance.OnHardAimDelegate -= method_3;
		Singleton<BotEventHandler>.Instance.OnQETilt -= method_4;
		Singleton<BotEventHandler>.Instance.OnGestusShow -= method_6;
		Singleton<BotEventHandler>.Instance.OnPhraseSay -= method_0;
	}
}
