using System;
using System.Collections.Generic;
using System.Text;
using EFT;

public class GClass540 : EnemyInfo
{
	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public HashSet<AIPlaceInfoLogicBoar> HashSet_0 = new HashSet<AIPlaceInfoLogicBoar>();

	public bool CanAttack => base.HaveSeenPersonal;

	public override EEnemyPartVisibleType VisibleType
	{
		get
		{
			if (!CanAttack)
			{
				return base.VisibleType;
			}
			return EEnemyPartVisibleType.Visible;
		}
	}

	public bool Boolean_0 => GClass398.Instance.IsTraceEnable();

	public GClass540(BotsGroup botsGroup, IPlayer enemy, BotOwner owner, BotSettingsClass groupInfo)
		: base(botsGroup, enemy, owner, groupInfo)
	{
		base.Person.AIData.OnComeToPlace += method_24;
		base.Person.AIData.OnLeavePlace += method_23;
		method_22();
	}

	public void method_22()
	{
		foreach (AIPlaceInfo currentZone in base.Person.AIData.CurrentZones)
		{
			if (currentZone.InfoLogicAllEnemy != null && currentZone.InfoLogicAllEnemy is AIPlaceInfoLogicBoar item)
			{
				HashSet_0.Add(item);
				_ = Boolean_0;
			}
		}
		method_25(HashSet_0);
	}

	public override string GetDebugInfo()
	{
		return $" [V:{IsVisible} origin:{base.IsVisible}]  [S:{CanShoot}(origin:{CanShoot_1})]" + $" VisibleType:{VisibleType}  VisibleDist:{base.Owner.LookSensor.VisibleDist} EnemyDistance:{base.Distance}  ";
	}

	public void method_23(AIPlaceInfo place)
	{
		if (place.InfoLogicAllEnemy != null && place.InfoLogicAllEnemy is AIPlaceInfoLogicBoar item)
		{
			HashSet_0.Remove(item);
			_ = Boolean_0;
		}
		method_25(HashSet_0);
	}

	public void method_24(AIPlaceInfo place)
	{
		if (place.InfoLogicAllEnemy != null && place.InfoLogicAllEnemy is AIPlaceInfoLogicBoar item)
		{
			HashSet_0.Add(item);
			_ = Boolean_0;
		}
		method_25(HashSet_0);
	}

	public void method_25(HashSet<AIPlaceInfoLogicBoar> current)
	{
		bool bool_ = false;
		foreach (AIPlaceInfoLogicBoar item in current)
		{
			if (item.PermanentVision)
			{
				bool_ = true;
			}
		}
		Bool_0 = bool_;
		if (!Boolean_0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder($"Recheck boar. Count:{current.Count}  botId:{base.Owner.Id}  enemy:{base.Person.Profile.Nickname} ");
		stringBuilder.AppendLine(" Recheck:");
		foreach (AIPlaceInfoLogicBoar item2 in current)
		{
			stringBuilder.Append($" {item2.name} logic.PermanentVision:{item2.PermanentVision} ");
		}
		stringBuilder.AppendLine("");
		stringBuilder.Append($"RESULT: _isPermanentVision:{Bool_0}");
	}

	public override void Dispose()
	{
		base.Person.AIData.OnComeToPlace -= method_24;
		base.Person.AIData.OnLeavePlace -= method_23;
		base.Dispose();
	}
}
