using System;
using System.Text;
using EFT;
using UnityEngine;

public class BotInfoDataPanel : MonoBehaviour
{
	public DebugTextPanelWithManualLayout TextPanel;

	private readonly string string_0 = GClass819.GetRichTextColor(new Color(0.8f, 0.8f, 0.8f));

	private readonly string string_1 = GClass819.GetRichTextColor(new Color(0.25f, 1f, 0.2f));

	private readonly StringBuilder stringBuilder_0 = new StringBuilder();

	private RectTransform rectTransform_0;

	public RectTransform RectTransform => rectTransform_0 ?? (rectTransform_0 = GClass949.RectTransform(base.transform));

	public void UpdateData(ActorDataStruct actorData, EBotInfoMode mode, bool showHint)
	{
		string text;
		try
		{
			text = GetInfoText(actorData, mode, showHint);
		}
		catch (Exception)
		{
			text = "error";
		}
		TextPanel.UpdateText(text);
	}

	public string GetInfoText(ActorDataStruct botData, EBotInfoMode mode, bool showHints)
	{
		return mode switch
		{
			EBotInfoMode.Behaviour => method_6(botData, showHints), 
			EBotInfoMode.BattleState => method_7(botData, showHints), 
			EBotInfoMode.Health => method_8(botData), 
			EBotInfoMode.Specials => method_5(botData, showHints), 
			EBotInfoMode.Custom => method_4(botData, showHints), 
			EBotInfoMode.TargetVisibility => method_0(botData, showHints), 
			_ => null, 
		};
	}

	public string method_0(ActorDataStruct botData, bool showHints)
	{
		return new StringBuilder().ToString();
	}

	public string method_1(ActorDataStruct actorDataStruct)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"Point:{actorDataStruct.BotData.PatrolPointStatus}");
		stringBuilder.AppendLine($"Way:{actorDataStruct.BotData.PatrolWayStatus}");
		return stringBuilder.ToString();
	}

	public string method_2(bool val)
	{
		if (!val)
		{
			return "";
		}
		return "(BL)";
	}

	public string method_3(bool val)
	{
		if (!val)
		{
			return "";
		}
		return "(Broken)";
	}

	public string method_4(ActorDataStruct actorDataStruct, bool showLabels)
	{
		BotDataStruct botData = actorDataStruct.BotData;
		stringBuilder_0.Clear();
		GClass1673.AppendLabeledValue(stringBuilder_0, "Layer", botData.LayerName, string_0, string_0, showLabels);
		stringBuilder_0.AppendLine(string.IsNullOrEmpty(botData.CustomData) ? "No Custom Data" : botData.CustomData);
		return stringBuilder_0.ToString();
	}

	public string method_5(ActorDataStruct actorDataStruct, bool showLabels)
	{
		BotDataStruct botData = actorDataStruct.BotData;
		stringBuilder_0.Clear();
		StringBuilder builder = stringBuilder_0;
		if (actorDataStruct.ProfileId.Length > 12)
		{
			GClass1673.AppendLabeledValue(builder, "Id", actorDataStruct.ProfileId.Substring(0, 12), Color.white, Color.white, showLabels);
			if (actorDataStruct.ProfileId.Length <= 24)
			{
				GClass1673.AppendLabeledValue(builder, "", actorDataStruct.ProfileId.Substring(12), Color.white, Color.white, labelEnabled: false);
			}
			else
			{
				GClass1673.AppendLabeledValue(builder, "", actorDataStruct.ProfileId.Substring(12, 12), Color.white, Color.white, labelEnabled: false);
				GClass1673.AppendLabeledValue(builder, "", actorDataStruct.ProfileId.Substring(24), Color.white, Color.white, labelEnabled: false);
			}
		}
		GClass1673.AppendLabeledValue(stringBuilder_0, "WeapSpawn", botData.IsInSpawnWeapon.ToString(), Color.white, Color.white, showLabels);
		GClass1673.AppendLabeledValue(stringBuilder_0, "AxeEnemy", botData.HaveAxeEnemy.ToString(), Color.white, Color.white, showLabels);
		WildSpawnType botRole = (WildSpawnType)botData.BotRole;
		string text = botRole.ToString();
		BotDifficulty botDifficulty = (BotDifficulty)botData.BotDifficulty;
		string data = text + "_" + botDifficulty;
		GClass1673.AppendLabeledValue(stringBuilder_0, "Role", data, Color.white, Color.white, showLabels);
		GClass1673.AppendLabeledValue(stringBuilder_0, "State", botData.PlayerState.ToString(), Color.white, Color.white, showLabels);
		GClass1673.AppendLabeledValue(stringBuilder_0, "PoseLvl", botData.PoseLevel.ToString("0.00"), Color.white, Color.white, showLabels);
		GClass1673.AppendLabeledValue(stringBuilder_0, "IsLay", botData.IsLay.ToString(), Color.white, Color.white, showLabels);
		GClass1673.AppendLabeledValue(stringBuilder_0, "IsInPronePos", botData.IsInPronePos.ToString(), Color.white, Color.white, showLabels);
		string data2;
		try
		{
			data2 = actorDataStruct.PlayerOwner.CurrentStataName.ToString();
		}
		catch (Exception)
		{
			data2 = "no data";
		}
		GClass1673.AppendLabeledValue(stringBuilder_0, "StateLc", data2, Color.white, Color.white, showLabels);
		GClass1673.AppendLabeledValue(stringBuilder_0, "Shoot", actorDataStruct.BotData.ToString(), Color.white, Color.white, showLabels);
		return stringBuilder_0.ToString();
	}

	public string method_6(ActorDataStruct actorDataStruct, bool showLabels)
	{
		BotDataStruct botData = actorDataStruct.BotData;
		stringBuilder_0.Clear();
		string strategyName = botData.StrategyName;
		GClass1673.AppendLabeledValue(stringBuilder_0, "Id, Strategy", $"{botData.Id} {strategyName}", Color.white, Color.white, showLabels);
		GClass1673.AppendLabeledValue(stringBuilder_0, "Layer", botData.LayerName, Color.yellow, Color.yellow, showLabels);
		string data = ((botData.NodeRepeats > 10) ? $"{botData.NodeName}_{botData.NodeRepeats}" : (botData.NodeName ?? ""));
		GClass1673.AppendLabeledValue(stringBuilder_0, "Node", data, Color.white, Color.white, showLabels);
		GClass1673.AppendLabeledValue(stringBuilder_0, "EnterBy", botData.Reason, string_1, string_1, showLabels);
		if (!string.IsNullOrEmpty(botData.PrevNodeName))
		{
			GClass1673.AppendLabeledValue(stringBuilder_0, "PrevNode", botData.PrevNodeName, string_0, string_0, showLabels);
			GClass1673.AppendLabeledValue(stringBuilder_0, "ExitBy", botData.PrevNodeExitReason, string_0, string_0, showLabels);
		}
		GClass1673.AppendLabeledValue(stringBuilder_0, "Group", actorDataStruct.BotData.GroupId, Color.white, Color.white, showLabels);
		return stringBuilder_0.ToString();
	}

	public string method_7(ActorDataStruct actorDataStruct, bool showLabels)
	{
		BotDataStruct botData = actorDataStruct.BotData;
		stringBuilder_0.Clear();
		try
		{
			if (actorDataStruct.PlayerOwner == null)
			{
				return "no battle data";
			}
			GClass1673.AppendLabeledValue(stringBuilder_0, "Hits", botData.HitsOnMe + "/" + botData.ShootsOnMe, Color.white, Color.white, showLabels);
			GClass1673.AppendLabeledValue(stringBuilder_0, "Reloading", botData.Reloading.ToString(), Color.white, Color.white, showLabels);
			GClass1673.AppendLabeledValue(stringBuilder_0, "CoverId", botData.CoverIndex.ToString(), Color.white, Color.white, showLabels);
		}
		catch (Exception)
		{
			Debug.LogError("Debug panel firearms error");
		}
		return stringBuilder_0.ToString();
	}

	public string method_8(ActorDataStruct actorDataStruct)
	{
		GStruct17 heathsData = actorDataStruct.HeathsData;
		stringBuilder_0.Clear();
		GClass1673.AppendLabeledValue(stringBuilder_0, "Head", heathsData.HealthHead + method_2(heathsData.HealthHeadBL) + method_3(heathsData.HealthHeadBroken), Color.white, Color.white);
		GClass1673.AppendLabeledValue(stringBuilder_0, "Chest", heathsData.HealthBody + method_2(heathsData.HealthBodyBL) + method_3(heathsData.HealthHeadBroken), Color.white, Color.white);
		GClass1673.AppendLabeledValue(stringBuilder_0, "Stomach", heathsData.HealthStomach + method_2(heathsData.HealthStomachBL), Color.white, Color.white);
		GClass1673.AppendLabeledValue(stringBuilder_0, "Arms", heathsData.HealthLeftArm + method_2(heathsData.HealthLeftArmBL) + method_3(heathsData.HealthLeftArmBroken) + " " + heathsData.HealthRightArm + method_2(heathsData.HealthRightArmBL) + method_3(heathsData.HealthRightArmBroken), Color.white, Color.white);
		GClass1673.AppendLabeledValue(stringBuilder_0, "Legs", heathsData.HealthLeftLeg + method_2(heathsData.HealthLeftLegBL) + method_3(heathsData.HealthLeftLegBroken) + " " + heathsData.HealthRightLeg + method_2(heathsData.HealthRightLegBL) + method_3(heathsData.HealthRightLegBroken), Color.white, Color.white);
		GClass1673.AppendLabeledValue(stringBuilder_0, "Energy", heathsData.Energy.ToString("F"), Color.white, Color.white);
		GClass1673.AppendLabeledValue(stringBuilder_0, "Hydration", heathsData.Hydration.ToString("F"), Color.white, Color.white);
		GClass1673.AppendLine(stringBuilder_0, "Effects:", Color.white);
		string[] array = heathsData.Effects.ToString("G").Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			if (i == 0)
			{
				GClass1673.Append(stringBuilder_0, array[i], Color.yellow);
			}
			else if (i % 2 == 0)
			{
				GClass1673.AppendLine(stringBuilder_0, array[i], Color.yellow);
			}
			else
			{
				GClass1673.Append(stringBuilder_0, array[i], Color.yellow);
			}
		}
		return stringBuilder_0.ToString();
	}
}
