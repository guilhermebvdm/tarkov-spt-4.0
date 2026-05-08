using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class BotGesture(BotOwner owner) : GClass429(owner)
{
	[CompilerGenerated]
	public class Class262
	{
		public GClass426 request;

		public bool method_0(EInteraction x)
		{
			return x == request.GestureByRequester;
		}
	}

	public GClass426 CurRequest;

	[NonSerialized]
	public float NextPosibleGestus;

	[NonSerialized]
	public List<EInteraction> ExecutedTypes = new List<EInteraction>();

	[NonSerialized]
	public Dictionary<string, List<EInteraction>> History = new Dictionary<string, List<EInteraction>>();

	public void StartExecuteCur()
	{
		CurRequest?.StartFirstExecuting();
	}

	public void StartSecondPart()
	{
		if (CurRequest.Answer.GetureAnswer.HasValue)
		{
			TryGestus(CurRequest.Answer.GetureAnswer.Value, withAimingDelay: false);
			if (CurRequest.GestureByRequester.HasValue)
			{
				ExecutedTypes.Add(CurRequest.GestureByRequester.Value);
			}
		}
		else
		{
			if (CurRequest.Answer.PhraseTrigger.HasValue)
			{
				BotOwner_0.BotTalk.TrySay(CurRequest.Answer.PhraseTrigger.Value);
			}
			if (CurRequest.GestureByRequester.HasValue)
			{
				ExecutedTypes.Add(CurRequest.GestureByRequester.Value);
			}
		}
		CurRequest.StartSecondExecuting();
	}

	public void TryGestus(EInteraction gesture, bool withAimingDelay)
	{
		if (BotOwner_0.Settings.FileSettings.Patrol.CAN_GESTUS && NextPosibleGestus < Time.time)
		{
			if (withAimingDelay)
			{
				BotOwner_0.AimingManager.CurrentAiming.SetNextAimingDelay(LocalBotSettingsProviderClass.Core.GESTUS_AIMING_DELAY);
			}
			NextPosibleGestus = Time.time + LocalBotSettingsProviderClass.Core.GESTUS_PERIOD_SEC;
			BotOwner_0.GetPlayer.MovementContext.SetInteractInHands(gesture);
		}
	}

	public bool HaveRequest()
	{
		if (CurRequest != null)
		{
			if (CurRequest.ShallDelete)
			{
				CurRequest = null;
				return false;
			}
			return true;
		}
		return false;
	}

	public bool CurRequestExecuting()
	{
		if (HaveRequest())
		{
			return CurRequest.Executing != AIRequestStage.none;
		}
		return false;
	}

	public void TryAddRequest(GClass426 request, bool force, bool withCheckDist)
	{
		if (CurRequest != null && CurRequest.Executing != AIRequestStage.none)
		{
			return;
		}
		if (force)
		{
			method_0(request);
			return;
		}
		bool flag = true;
		bool flag2 = false;
		if (History.TryGetValue(request.Requester.Profile.Id, out var value))
		{
			flag = (flag2 = value.Count((EInteraction x) => x == request.GestureByRequester) < LocalBotSettingsProviderClass.Core.GESTUS_MAX_ANSWERS) || GClass856.IsTrue100(LocalBotSettingsProviderClass.Core.GESTUS_ANYWAY_CHANCE);
		}
		float sqrMagnitude = (BotOwner_0.Transform.position - request.Requester.Transform.position).sqrMagnitude;
		bool flag3 = true;
		bool num = (flag3 = BotReceiver.CanReceiveFromPoint(request.Requester.Transform.position, BotOwner_0.Transform.position, BotOwner_0.LookDirection, BotOwner_0.LookSensor.VISIBLE_ANGLE));
		if (flag && (!withCheckDist || sqrMagnitude < LocalBotSettingsProviderClass.Core.GESTUS_DIST_ANSWERS_SQRT) && flag3)
		{
			method_0(request);
		}
		if (num && flag2 && GClass856.IsTrue100(50f))
		{
			BotOwner_0.BotTalk.TrySay(EPhraseTrigger.OnMutter, withGroupDelay: true);
		}
	}

	public void method_0(GClass426 request)
	{
		CurRequest = request;
		CurRequest.Executer = BotOwner_0;
		string id = request.Requester.Profile.Id;
		List<EInteraction> list;
		if (History.ContainsKey(id))
		{
			list = History[id];
		}
		else
		{
			list = new List<EInteraction>();
			History.Add(id, list);
		}
		if (request.GestureByRequester.HasValue)
		{
			list.Add(request.GestureByRequester.Value);
			if (request.GestureByRequester == EInteraction.GetOffGesture && list.Count > LocalBotSettingsProviderClass.Core.GESTUS_FUCK_TO_SHOOT)
			{
				BotOwner_0.BotTalk.TrySay(EPhraseTrigger.OnFight, withGroupDelay: true);
				BotOwner_0.BotsGroup.AddEnemy(request.Requester, EBotEnemyCause.fuckGestus);
			}
		}
	}
}
