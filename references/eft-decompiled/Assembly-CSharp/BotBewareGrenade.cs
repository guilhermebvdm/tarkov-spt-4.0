using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class BotBewareGrenade(BotOwner owner) : GClass429(owner)
{
	[CompilerGenerated]
	public class Class160
	{
		public BotBewareGrenade botBewareGrenade_0;

		public Grenade grenade;

		public Vector3 danger;

		public void method_0()
		{
			if (botBewareGrenade_0.BotOwner_0.BotState == EBotState.Active && botBewareGrenade_0.BotOwner_0.HealthController.IsAlive && grenade != null)
			{
				botBewareGrenade_0.method_3(grenade.transform.position);
				botBewareGrenade_0.method_2(new GClass581(danger, grenade, botBewareGrenade_0.BotOwner_0, 0f));
			}
		}
	}

	[CompilerGenerated]
	public class Class161
	{
		public Grenade grenade;

		public Vector3? method_0()
		{
			if (grenade != null)
			{
				return grenade.transform.position;
			}
			return null;
		}
	}

	[CompilerGenerated]
	public class Class162
	{
		public Grenade grenade;

		public float distToGrenade;

		public BotBewareGrenade botBewareGrenade_0;

		public Vector3 danger;

		public bool isMyFlash;

		public void method_0()
		{
			if (grenade != null)
			{
				if (!(distToGrenade < botBewareGrenade_0.BotOwner_0.Settings.FileSettings.Grenade.RUN_AWAY) && distToGrenade >= botBewareGrenade_0.BotOwner_0.Settings.FileSettings.Grenade.ADD_GRENADE_AS_DANGER)
				{
					Debug.LogError($"grenade still is too far:  grenade:{grenade.transform.position}  _owner.Position:{botBewareGrenade_0.BotOwner_0.Position}  distToGrenade:{distToGrenade}");
				}
				else
				{
					botBewareGrenade_0.method_1(grenade, danger, isMyFlash);
				}
			}
		}
	}

	public GClass581 GrenadeDangerPoint;

	public Action<Grenade> OnBewareGrenade;

	[NonSerialized]
	public float EndPeriodOfIgnore;

	[NonSerialized]
	public float NextPosibleRunTime;

	[NonSerialized]
	public float DELTA_CALC_TIME = 2f;

	[NonSerialized]
	public float DeltaGoCheck = 2f;

	[NonSerialized]
	public bool DoExtraSearch;

	public void TurnAwayFlash(Grenade grenade)
	{
		BotOwner_0.BotTurnAwayLight.Activate(() => (grenade != null) ? new Vector3?(grenade.transform.position) : ((Vector3?)null), BotOwner_0.Settings.FileSettings.Grenade.WAIT_TIME_TURN_AWAY);
	}

	public void AddGrenadeDanger(Vector3 danger, Grenade grenade)
	{
		if (BotOwner_0.Settings.FileSettings.Mind.GRENADE_DAMAGE_IGNORE || GClass858.IsAnyComponentNaN(danger))
		{
			return;
		}
		bool isMyFlash = false;
		if (grenade == null)
		{
			return;
		}
		if (grenade.Player != null && grenade.Player.iPlayer != null && grenade.Player.iPlayer.Id == BotOwner_0.GetPlayer.Id)
		{
			if (BotOwner_0.Settings.FileSettings.Grenade.NO_RUN_FROM_AI_GRENADES)
			{
				return;
			}
			isMyFlash = grenade as StunGrenade != null;
			if (isMyFlash)
			{
				TurnAwayFlash(grenade);
			}
		}
		float magnitude = (BotOwner_0.Position - danger).magnitude;
		float distToGrenade = (BotOwner_0.Position - grenade.transform.position).magnitude;
		if (!(magnitude < BotOwner_0.Settings.FileSettings.Grenade.ADD_GRENADE_AS_DANGER) && !(distToGrenade < BotOwner_0.Settings.FileSettings.Grenade.ADD_GRENADE_AS_DANGER))
		{
			return;
		}
		BotOwner_0.BotsGroup.AddPointToSearch(danger, 140f, BotOwner_0, !(grenade is SmokeGrenade));
		if (magnitude < BotOwner_0.Settings.FileSettings.Grenade.RUN_AWAY_SQR)
		{
			method_1(grenade, danger, isMyFlash);
			return;
		}
		Debug.LogError($"danger still is too far TIMER ADDS:{danger} pos:{BotOwner_0.Position} dist:{magnitude}");
		StaticManager.Instance.TimerManager.MakeTimer(TimeSpan.FromSeconds(1.0)).OnTimer += delegate
		{
			if (grenade != null)
			{
				if (!(distToGrenade < BotOwner_0.Settings.FileSettings.Grenade.RUN_AWAY) && distToGrenade >= BotOwner_0.Settings.FileSettings.Grenade.ADD_GRENADE_AS_DANGER)
				{
					Debug.LogError($"grenade still is too far:  grenade:{grenade.transform.position}  _owner.Position:{BotOwner_0.Position}  distToGrenade:{distToGrenade}");
				}
				else
				{
					method_1(grenade, danger, isMyFlash);
				}
			}
		};
	}

	public bool ShallRunAway()
	{
		if (GrenadeDangerPoint == null)
		{
			return false;
		}
		if (method_4())
		{
			return false;
		}
		if (GrenadeDangerPoint.ShallDestroy())
		{
			GrenadeDangerPoint = null;
			return false;
		}
		if (!GrenadeDangerPoint.ShallRunAway())
		{
			return false;
		}
		if (method_0(GrenadeDangerPoint.Grenade))
		{
			return false;
		}
		return GrenadeDangerPoint.IsActive();
	}

	public void DrawGizmosSelected()
	{
		if (GrenadeDangerPoint != null)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(GrenadeDangerPoint.DangerPoint, LocalBotSettingsProviderClass.Core.DELTA_GRENADE_RUN_DIST);
		}
	}

	public void IgnoreGrenadesForSec(float sec)
	{
		EndPeriodOfIgnore = Time.time + sec;
	}

	public void UpdateByNode()
	{
		method_5();
		BotOwner_0.BotRun.RunAwayGrenade();
	}

	public bool method_0(Grenade grenade)
	{
		if (grenade is SmokeGrenade)
		{
			return BotOwner_0.Settings.FileSettings.Grenade.IGNORE_SMOKE_GRENADE;
		}
		return false;
	}

	public void method_1(Grenade grenade, Vector3 danger, bool isMyFlash)
	{
		if (!GClass856.IsTrue100(BotOwner_0.Settings.FileSettings.Grenade.CHANCE_TO_NOTIFY_ENEMY_GR_100))
		{
			return;
		}
		OnBewareGrenade?.Invoke(grenade);
		bool num = grenade as SmokeGrenade != null;
		bool flag = grenade as StunGrenade != null;
		if (!num && !flag)
		{
			method_3(danger);
			GClass641.IBotTimer botTimer = StaticManager.Instance.TimerManager.MakeTimer(TimeSpan.FromSeconds(1.399999976158142));
			GClass641.IBotTimer botTimer2 = StaticManager.Instance.TimerManager.MakeTimer(TimeSpan.FromSeconds(3.0));
			botTimer.OnTimer += delegate
			{
				if (BotOwner_0.BotState == EBotState.Active && BotOwner_0.HealthController.IsAlive && grenade != null)
				{
					method_3(grenade.transform.position);
					method_2(new GClass581(danger, grenade, BotOwner_0, 0f));
				}
			};
			botTimer2.OnTimer += delegate
			{
				if (BotOwner_0.BotState == EBotState.Active && BotOwner_0.HealthController.IsAlive && grenade != null)
				{
					method_3(grenade.transform.position);
					method_2(new GClass581(danger, grenade, BotOwner_0, 0f));
				}
			};
		}
		if (!isMyFlash)
		{
			BotOwner_0.BotTalk.TrySay(EPhraseTrigger.OnEnemyGrenade, withGroupDelay: true);
			method_2(new GClass581(danger, grenade, BotOwner_0, BotOwner_0.Settings.FileSettings.Grenade.DELTA_GRENADE_START_TIME));
		}
	}

	public void method_2(GClass581 gdp)
	{
		GrenadeDangerPoint = gdp;
	}

	public void method_3(Vector3 pos)
	{
		foreach (CustomNavigationPoint closePoint in BotOwner_0.BotsGroup.CoverPointMaster.GetClosePoints(pos, BotOwner_0, BotOwner_0.Settings.FileSettings.Cover.SPOTTED_GRENADE_RADIUS))
		{
			closePoint.Spotted(BotOwner_0.Settings.FileSettings.Cover.SPOTTED_GRENADE_TIME);
		}
	}

	public bool method_4()
	{
		return Time.time < EndPeriodOfIgnore;
	}

	public void method_5()
	{
		bool flag = false;
		if (BotOwner_0.Memory.BotCurrentCoverInfo.HaveCover && GrenadeDangerPoint.SafePlaceFound)
		{
			CustomNavigationPoint covPoint = BotOwner_0.Memory.BotCurrentCoverInfo.CovPoint;
			if (!(flag = !covPoint.IsFreeById(BotOwner_0.Id)) && GrenadeDangerPoint.IsCoverSafe(covPoint.Position))
			{
				if (!BotOwner_0.Mover.HasPathAndNoComplete || DeltaGoCheck < Time.time)
				{
					DeltaGoCheck = Time.time + 0.5f;
					BotOwner_0.GoToPoint(covPoint);
				}
				return;
			}
		}
		if (NextPosibleRunTime < Time.time || (!DoExtraSearch && flag))
		{
			CoverSearchData data = new CoverSearchData(BotOwner_0.Position, BotOwner_0.CoverSearchInfo, CoverShootType.hide, 100f, 0f, CoverSearchType.distToBot, null, null, null, ECheckSHootHide.hide, new CoverSearchDefenceDataClass(0f), PointsArrayType.allWithBush);
			NextPosibleRunTime = Time.time + ((!DoExtraSearch && flag) ? 0.5f : DELTA_CALC_TIME);
			CustomNavigationPoint customNavigationPoint = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(data, checkCurrent: false);
			DoExtraSearch = customNavigationPoint == null || !GrenadeDangerPoint.IsCoverSafe(customNavigationPoint.Position) || !customNavigationPoint.IsFreeById(BotOwner_0.Id);
			if (DoExtraSearch)
			{
				customNavigationPoint = method_7();
			}
			GrenadeDangerPoint.SafePlaceFound = true;
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(customNavigationPoint);
			BotOwner_0.GoToPoint(customNavigationPoint);
		}
	}

	public Vector3 method_6()
	{
		Vector3 zero = Vector3.zero;
		foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in BotOwner_0.EnemiesController.EnemyInfos)
		{
			if (enemyInfo.Value.Distance < 50f)
			{
				GClass369.DebugDrawArc(enemyInfo.Value.CurrPosition, BotOwner_0.Position, 20f, 2f, Color.yellow);
				zero += GClass855.NormalizeFastSelf(enemyInfo.Value.Direction);
			}
		}
		GClass369.DebugDrawArc(GrenadeDangerPoint.DangerPoint, BotOwner_0.Position, 20f, 2f, Color.red);
		Debug.DrawRay(GrenadeDangerPoint.DangerPoint, Vector3.up * 150f, Color.red, 20f);
		if (zero.sqrMagnitude > 0.1f)
		{
			Vector3 vector = BotOwner_0.Position - GClass855.NormalizeFastSelf(zero) * 5f;
			GClass369.DebugDrawArc(vector, BotOwner_0.Position, 25f, 2f, Color.green);
			return vector;
		}
		return BotOwner_0.Position;
	}

	public CustomNavigationPoint method_7()
	{
		Vector3 dangerPoint = GrenadeDangerPoint.DangerPoint;
		Vector3 vector = GClass855.NormalizeFastSelf(BotOwner_0.Position - dangerPoint);
		return BotOwner_0.Covers.FindHidePoint(BotOwner_0.Position + vector * 12f, -1f);
	}

	public bool SawGrenadeSoFar(float period)
	{
		if (GrenadeDangerPoint == null)
		{
			return false;
		}
		return Time.time - GrenadeDangerPoint.EndTime < period;
	}
}
