using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using UnityEngine;

public class WeaponOverheatDebug : MonoBehaviour
{
	private GUIStyle guistyle_0;

	private GUIStyle guistyle_1;

	private GUIStyle guistyle_2;

	private Texture2D texture2D_0;

	private Player player_0;

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
			AmmoTemplate currentAmmoTemplate = item.CurrentAmmoTemplate;
			float lastShotOverheat = item.MalfState.LastShotOverheat;
			float lastShotTime = item.MalfState.LastShotTime;
			float num = GClass1891.PastTime - item.MalfState.LastShotTime;
			float heatFactorGun = item.Template.HeatFactorGun;
			float coolFactorGun = item.Template.CoolFactorGun;
			float num2 = currentAmmoTemplate?.HeatFactor ?? 1f;
			BackendConfigSettingsClass.GClass1739 overheat = Singleton<BackendConfigSettingsClass>.Instance.Overheat;
			float modHeatFactor = overheat.ModHeatFactor;
			float modCoolFactor = overheat.ModCoolFactor;
			float modsCoolFactor;
			float currentOverheat = item.GetCurrentOverheat(GClass1891.PastTime, overheat, out modsCoolFactor);
			float modsHeatFactor;
			float shotOverheat = item.GetShotOverheat(num2, modHeatFactor, out modsHeatFactor);
			if (!item.AllowOverheat)
			{
				GUI.Box(new Rect(1010f, 35f, 320f, 20f), "Ignore overheat (taxonomy, AllowOverheat parameter)", guistyle_0);
				return;
			}
			GUI.Box(new Rect(1010f, 35f, 240f, 20f), $"Overheat: {currentOverheat}", (currentOverheat < 100f) ? guistyle_2 : guistyle_1);
			GUI.Box(new Rect(1010f, 55f, 240f, 20f), $"Last shot overheat: {lastShotOverheat}", guistyle_0);
			GUI.Box(new Rect(1010f, 75f, 240f, 20f), $"Last shot time: {lastShotTime}", guistyle_0);
			GUI.Box(new Rect(1010f, 95f, 240f, 20f), $"Last shot deltaTime: {num}", guistyle_0);
			GUI.Box(new Rect(1010f, 115f, 240f, 20f), $"Next shot overheat: {shotOverheat}", guistyle_0);
			GUI.Box(new Rect(1010f, 135f, 240f, 20f), $"Weapon HeatFactor: {heatFactorGun}", guistyle_0);
			GUI.Box(new Rect(1010f, 155f, 240f, 20f), $"Weapon CoolFactor: {coolFactorGun}", guistyle_0);
			GUI.Box(new Rect(1010f, 175f, 240f, 20f), $"Ammo HeatFactor: {num2}", guistyle_0);
			GUI.Box(new Rect(1010f, 195f, 240f, 20f), $"Mods HeatFactor: {modsHeatFactor}", guistyle_0);
			GUI.Box(new Rect(1010f, 215f, 240f, 20f), $"Mods CoolFactor: {modsCoolFactor}", guistyle_0);
			GUI.Box(new Rect(1010f, 235f, 240f, 20f), $"Global Mod HeatFactor: {modHeatFactor}", guistyle_0);
			GUI.Box(new Rect(1010f, 255f, 240f, 20f), $"Global Mod CoolFactor: {modCoolFactor}", guistyle_0);
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
		Object.DestroyImmediate(texture2D_0);
	}
}
