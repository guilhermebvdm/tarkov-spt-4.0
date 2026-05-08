using System;
using System.Collections.Generic;
using System.Text;
using EFT;
using UnityEngine;

public class GClass541 : EnemyInfo
{
	[NonSerialized]
	public const float Float_0 = 12f;

	public bool CloseZone;

	[NonSerialized]
	public HashSet<AIPlaceInfoLogicZryachiy> HashSet_0 = new HashSet<AIPlaceInfoLogicZryachiy>();

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public bool Bool_3;

	[NonSerialized]
	public float Float_2 = -1f;

	public override bool CanShoot
	{
		get
		{
			if (!Bool_3 && !Bool_1)
			{
				return false;
			}
			return CanShoot_1;
		}
	}

	public override bool IsVisible
	{
		get
		{
			if (!Bool_0)
			{
				return base.IsVisible;
			}
			return true;
		}
	}

	public override EEnemyPartVisibleType VisibleType
	{
		get
		{
			if (!Bool_0)
			{
				return base.VisibleType;
			}
			return EEnemyPartVisibleType.Visible;
		}
	}

	public override bool CheckPartCanShootOnlyIfVisible => false;

	public bool Boolean_0 => GClass398.Instance.IsTraceEnable();

	public GClass541(BotsGroup botsGroup, IPlayer enemy, BotOwner owner, BotSettingsClass groupInfo)
		: base(botsGroup, enemy, owner, groupInfo)
	{
		base.Person.AIData.OnComeToPlace += method_26;
		base.Person.AIData.OnLeavePlace += method_25;
		base.Owner.GetPlayer.BeingHitAction += method_23;
		method_22();
	}

	public void method_22()
	{
		foreach (AIPlaceInfo currentZone in base.Person.AIData.CurrentZones)
		{
			if (currentZone.InfoLogicAllEnemy != null && currentZone.InfoLogicAllEnemy is AIPlaceInfoLogicZryachiy item)
			{
				HashSet_0.Add(item);
				_ = Boolean_0;
			}
		}
		method_27(HashSet_0);
	}

	public bool CanShootCauseTime()
	{
		if (!Bool_2)
		{
			return false;
		}
		return Time.time - Float_1 > 12f;
	}

	public void method_23(DamageInfoStruct arg1, EBodyPart arg2, float arg3)
	{
		if (arg1.Player == null || arg1.Player.iPlayer.Id != base.Person.Id)
		{
			return;
		}
		SetGetHitFrom();
		if (base.GroupOwner.BossGroup != null)
		{
			BotOwner boss = base.GroupOwner.BossGroup.Boss;
			method_24(boss);
			foreach (BotOwner follower in boss.Boss.Followers)
			{
				method_24(follower);
			}
		}
		base.Owner.GetPlayer.BeingHitAction -= method_23;
	}

	public void method_24(BotOwner bot)
	{
		if (bot.EnemiesController.EnemyInfos.TryGetValue(base.Person, out var value) && value is GClass541 gClass)
		{
			gClass.Bool_1 = true;
		}
	}

	public override string GetDebugInfo()
	{
		return $" [V:{IsVisible} origin:{base.IsVisible}]  [S:{CanShoot}(origin:{CanShoot_1})] " + $" AttackPossible:{Bool_3}  GetHitFromThis:{Bool_1}  " + $"VisibleType:{VisibleType}  VisibleDist:{base.Owner.LookSensor.VisibleDist} EnemyDistance:{base.Distance}  ";
	}

	public void method_25(AIPlaceInfo place)
	{
		if (place.InfoLogicAllEnemy != null && place.InfoLogicAllEnemy is AIPlaceInfoLogicZryachiy item)
		{
			HashSet_0.Remove(item);
			_ = Boolean_0;
		}
		method_27(HashSet_0);
	}

	public void method_26(AIPlaceInfo place)
	{
		if (place.InfoLogicAllEnemy != null && place.InfoLogicAllEnemy is AIPlaceInfoLogicZryachiy item)
		{
			HashSet_0.Add(item);
			_ = Boolean_0;
		}
		method_27(HashSet_0);
	}

	public void method_27(HashSet<AIPlaceInfoLogicZryachiy> current)
	{
		bool bool_ = false;
		bool flag = false;
		bool closeZone = false;
		foreach (AIPlaceInfoLogicZryachiy item in current)
		{
			if (item.PermanentVision)
			{
				bool_ = true;
			}
			if (item.AttackPossible)
			{
				if (!Bool_2)
				{
					Float_1 = Time.time;
					Bool_2 = true;
				}
				flag = true;
			}
			if (item.CloseZone)
			{
				closeZone = true;
			}
		}
		Bool_0 = bool_;
		if (!Bool_3 && flag)
		{
			Float_2 = Time.time;
		}
		if (Bool_3 != flag)
		{
			Bool_3 = flag;
		}
		if (Boolean_0)
		{
			StringBuilder stringBuilder = new StringBuilder($"Recheck zryachiy. Count:{current.Count}  botId:{base.Owner.Id}  enemy:{base.Person.Profile.Nickname} ");
			stringBuilder.AppendLine(" Recheck:");
			foreach (AIPlaceInfoLogicZryachiy item2 in current)
			{
				stringBuilder.Append($" {item2.name} logic.PermanentVision:{item2.PermanentVision}  logic.AttackPossible:{item2.AttackPossible}");
			}
			stringBuilder.AppendLine("");
			stringBuilder.Append($"RESULT: _isPermanentVision:{Bool_0} _attackPossible:{Bool_3}");
		}
		CloseZone = closeZone;
	}

	public override void Dispose()
	{
		base.Person.AIData.OnComeToPlace -= method_26;
		base.Person.AIData.OnLeavePlace -= method_25;
		base.Owner.GetPlayer.BeingHitAction -= method_23;
		base.Dispose();
	}

	public bool SpendedAtAttackZone(float period)
	{
		if (Float_2 < 0f)
		{
			return false;
		}
		return Time.time - Float_2 > period;
	}

	public void SetGetHitFrom()
	{
		Bool_1 = true;
		if (base.FirstTimeShoot <= 0f && base.GroupOwner.BossGroup != null && base.GroupOwner.BossGroup.Boss.Boss.BossLogic is ZyriachyBossLogicClass zyriachyBossLogicClass)
		{
			zyriachyBossLogicClass.MarkEnemyAsFirstAttacker(base.Person.Id);
		}
	}
}
