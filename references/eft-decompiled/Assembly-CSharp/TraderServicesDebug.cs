using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class TraderServicesDebug : MonoBehaviour
{
	private const int int_0 = 250;

	private Player player_0;

	private ClientPlayer clientPlayer_0;

	private GUIStyle guistyle_0;

	private Texture2D texture2D_0;

	public void SetDebugObject(Player player)
	{
		player_0 = player;
		if (player is ClientPlayer clientPlayer)
		{
			clientPlayer_0 = clientPlayer;
		}
		method_0();
	}

	public void method_0()
	{
		if (guistyle_0 == null)
		{
			guistyle_0 = new GUIStyle
			{
				alignment = TextAnchor.UpperLeft
			};
			texture2D_0 = method_2(2, 2, new Color(0.2f, 0.2f, 0.3f, 0.9f));
			guistyle_0.normal.background = texture2D_0;
			guistyle_0.normal.textColor = Color.white;
		}
	}

	public void OnGUI()
	{
		Dictionary<ETraderServiceType, BackendConfigSettingsClass.ServiceData> servicesData = Singleton<BackendConfigSettingsClass>.Instance.ServicesData;
		int num = 10;
		MarkOfUnknownItemClass markOfUnknown;
		bool flag = player_0.HasMarkOfUnknown(out markOfUnknown);
		foreach (KeyValuePair<ETraderServiceType, BackendConfigSettingsClass.ServiceData> item in servicesData)
		{
			item.Deconstruct(out var key, out var value);
			ETraderServiceType eTraderServiceType = key;
			BackendConfigSettingsClass.ServiceData serviceData = value;
			string text = $"Service type: {eTraderServiceType}\n";
			if (player_0.Profile.TradersInfo.TryGetValue("638f541a29ffd1183d187f57", out var value2))
			{
				text += $"Can afford service: {value2.IsServiceAvailableForPurchase(eTraderServiceType)}\n";
				text += $"Service purchased in this raid: {value2.IsServiceAlreadyPurchased(eTraderServiceType)}\n";
			}
			text += "Items to pay:\n";
			int i;
			foreach (KeyValuePair<string, int> item2 in serviceData.ServiceItemCost)
			{
				item2.Deconstruct(out var key2, out i);
				string text2 = key2;
				int num2 = i;
				int num3 = num2;
				string arg = "";
				if (flag && GClass3130.IsCurrencyId(text2))
				{
					num3 = Mathf.CeilToInt((float)num2 * markOfUnknown.TradersDiscount);
					arg = $" (-{markOfUnknown.TradersDiscount * 100f}%)";
				}
				text += string.Format("{0} : {1}{2}\n", GClass2348.Localized(text2 + " ShortName"), num3, arg);
			}
			text += "Items to get from service:\n";
			MongoID[] uniqueItems = serviceData.UniqueItems;
			for (i = 0; i < uniqueItems.Length; i++)
			{
				MongoID mongoID = uniqueItems[i];
				text = text + GClass2348.Localized(string.Concat(mongoID, " ShortName")) + "\n";
			}
			GUI.Box(new Rect(num, 10f, 250f, 120f), text, guistyle_0);
			if (GUI.Button(new Rect(num, 130f, 150f, 20f), "Try purchase service", guistyle_0) && clientPlayer_0 != null)
			{
				clientPlayer_0.InventoryController.TryPurchaseTraderService(eTraderServiceType, clientPlayer_0.AbstractQuestControllerClass).HandleExceptions();
			}
			num += 250;
		}
		if (player_0.Profile.TradersInfo.TryGetValue("638f541a29ffd1183d187f57", out var value3))
		{
			GUI.Box(new Rect(10f, 150f, 220f, 20f), $"Keeper standing: {value3.Standing}", guistyle_0);
		}
		CultistAmuletItemClass cultistAmuletItemClass = method_1();
		GUI.Box(new Rect(10f, 190f, 220f, 20f), (cultistAmuletItemClass != null) ? $"Has cultist amulet, charges left: {cultistAmuletItemClass.CultistAmuletComponent.NumberOfUsages}" : "No cultists amulet", guistyle_0);
		RadioTransmitterRecodableComponent radioTransmitterRecodableComponent = player_0.FindRadioTransmitter();
		string text3 = ((radioTransmitterRecodableComponent == null) ? "NONE" : radioTransmitterRecodableComponent.Status.ToString());
		GUI.Box(new Rect(10f, 210f, 220f, 20f), "Transmitter state: " + text3, guistyle_0);
		if (GUI.Button(new Rect(10f, 230f, 220f, 20f), "Get services", guistyle_0) && clientPlayer_0 != null)
		{
			clientPlayer_0.GetTraderServicesDataFromServer("638f541a29ffd1183d187f57");
		}
		if (GUI.Button(new Rect(10f, 250f, 220f, 20f), "Clear services", guistyle_0))
		{
			servicesData.Clear();
		}
		if (GUI.Button(new Rect(10f, 270f, 220f, 20f), "Close", guistyle_0))
		{
			Object.Destroy(this);
		}
	}

	public CultistAmuletItemClass method_1()
	{
		return player_0.InventoryController.Inventory.GetItemsInSlots(new EquipmentSlot[1] { EquipmentSlot.Pockets }).OfType<CultistAmuletItemClass>().FirstOrDefault();
	}

	public Texture2D method_2(int width, int height, Color col)
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
}
