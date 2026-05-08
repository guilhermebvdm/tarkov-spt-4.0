using System;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using UnityEngine;

public class WeaponDurabilityDebug : MonoBehaviour
{
	private GUIStyle guistyle_0;

	private GUIStyle guistyle_1;

	private GUIStyle guistyle_2;

	private Texture2D texture2D_0;

	private Player player_0;

	private float float_0;

	private float float_1;

	private float float_2;

	public void SetDebugObject(Player toDebug)
	{
		player_0 = toDebug;
		method_0();
	}

	public void OnGUI()
	{
		if (player_0 == null || MonoBehaviourSingleton<PreloaderUI>.Instance.Console.IsConsoleVisible)
		{
			return;
		}
		Player.FirearmController firearmController = player_0.HandsController as Player.FirearmController;
		if (!(firearmController == null))
		{
			Weapon item = firearmController.Item;
			double num = Math.Round(item.Repairable.Durability, 3);
			double num2 = Math.Round(item.Repairable.MaxDurability, 3);
			float operatingResource = item.Template.OperatingResource;
			int templateDurability = item.Repairable.TemplateDurability;
			double num3 = num / (double)((float)templateDurability / operatingResource);
			AmmoTemplate currentAmmoTemplate = item.CurrentAmmoTemplate;
			float modsBurnRatio = 1f;
			BackendConfigSettingsClass.GClass1739 overheat = Singleton<BackendConfigSettingsClass>.Instance.Overheat;
			float modsCoolFactor;
			float overheatFactor = Mathf.Clamp(item.GetCurrentOverheat(GClass1891.PastTime, overheat, out modsCoolFactor) / 100f, 1f, 2f);
			float num4 = currentAmmoTemplate?.DurabilityBurnModificator ?? 1f;
			float value = player_0.Skills.WeaponDurabilityLosOnShotReduce.Value;
			float num5 = ((currentAmmoTemplate == null) ? 0f : item.GetDurabilityLossOnShot(currentAmmoTemplate.DurabilityBurnModificator, overheatFactor, value, out modsBurnRatio));
			if (Time.time - float_0 > 1f)
			{
				float_0 = Time.time;
				float_1 = num5;
				float_2 = num5 / ((float)templateDurability / operatingResource);
			}
			GUI.Box(new Rect(450f, 35f, 240f, 20f), $"Durability: {num} / {num2} (Tmpl: {templateDurability})", guistyle_0);
			GUI.Box(new Rect(450f, 55f, 240f, 20f), $"OperatingResource: {num3} / {operatingResource}", guistyle_0);
			GUI.Box(new Rect(450f, 75f, 240f, 20f), "WeaponBurnRatio: " + item.DurabilityBurnRatio, guistyle_0);
			GUI.Box(new Rect(450f, 95f, 240f, 20f), "AmmoBurnRatio: " + num4, guistyle_0);
			GUI.Box(new Rect(450f, 115f, 240f, 20f), "ModsBurnRatio: " + modsBurnRatio, guistyle_0);
			GUI.Box(new Rect(450f, 135f, 240f, 20f), "OverheatFactor: " + overheatFactor, guistyle_0);
			GUI.Box(new Rect(450f, 155f, 240f, 20f), "WeaponTreatment Factor: " + value, guistyle_0);
			GUI.Box(new Rect(450f, 175f, 240f, 20f), $"NextShot Resource Los: {float_2} ({float_1}%)", guistyle_0);
		}
	}

	public void method_0()
	{
		if (guistyle_0 == null)
		{
			guistyle_0 = new GUIStyle
			{
				alignment = TextAnchor.UpperLeft
			};
			texture2D_0 = method_1(2, 2, new Color(0.2f, 0.2f, 0.3f, 0.9f));
			guistyle_0.normal.background = texture2D_0;
			guistyle_0.normal.textColor = Color.white;
		}
		if (guistyle_1 == null)
		{
			guistyle_1 = new GUIStyle
			{
				alignment = TextAnchor.UpperLeft
			};
			guistyle_1.normal.background = texture2D_0;
			guistyle_1.normal.textColor = Color.red;
		}
		if (guistyle_2 == null)
		{
			guistyle_2 = new GUIStyle
			{
				alignment = TextAnchor.UpperLeft
			};
			guistyle_2.normal.background = texture2D_0;
			guistyle_2.normal.textColor = Color.green;
		}
	}

	public Texture2D method_1(int width, int height, Color col)
	{
		Color[] array = new Color[width * height];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = col;
		}
		Texture2D texture2D = new Texture2D(width, height);
		texture2D.SetPixels(array);
		texture2D.Apply();
		return texture2D;
	}

	public void OnDestroy()
	{
		UnityEngine.Object.DestroyImmediate(texture2D_0);
	}
}
