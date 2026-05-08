using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using UnityEngine;

public class MalfunctionDebug : MonoBehaviour
{
	private GUIStyle guistyle_0;

	private GUIStyle guistyle_1;

	private GUIStyle guistyle_2;

	private Texture2D texture2D_0;

	private Player player_0;

	private float float_0 = -1f;

	private int int_0;

	private List<GClass820<Weapon.EMalfunctionSource>.GStruct49<float, Weapon.EMalfunctionSource>> list_0 = new List<GClass820<Weapon.EMalfunctionSource>.GStruct49<float, Weapon.EMalfunctionSource>>(5);

	private Dictionary<Weapon, int> dictionary_0 = new Dictionary<Weapon, int>();

	private Dictionary<Weapon, int> dictionary_1 = new Dictionary<Weapon, int>();

	public void SetDebugObject(Player toDebug)
	{
		player_0 = toDebug;
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
			texture2D_0 = method_5(2, 2, new Color(0.2f, 0.2f, 0.3f, 0.9f));
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

	public void method_1(int value)
	{
		int_0 = value;
	}

	public int method_2(int incVal)
	{
		int result = int_0;
		int_0 += incVal;
		return result;
	}

	public void OnGUI()
	{
		if (player_0 == null || MonoBehaviourSingleton<PreloaderUI>.Instance.Console.IsConsoleVisible)
		{
			return;
		}
		Player.FirearmController firearmController = player_0.HandsController as Player.FirearmController;
		if (firearmController == null)
		{
			return;
		}
		bool isTriggerPressed = firearmController.IsTriggerPressed;
		Weapon item = firearmController.Item;
		bool flag = item.IsBoltCatch && WeaponAnimationSpeedControllerClass.GetBoolBoltCatch(firearmController.FirearmsAnimator.Animator);
		float floatAmmoInChamber = WeaponAnimationSpeedControllerClass.GetFloatAmmoInChamber(firearmController.FirearmsAnimator.Animator);
		float floatAmmoInMag = WeaponAnimationSpeedControllerClass.GetFloatAmmoInMag(firearmController.FirearmsAnimator.Animator);
		float modsCoolFactor;
		if (item.GetCurrentMagazine() is CylinderMagazineItemClass cylinderMagazineItemClass)
		{
			string text = "empty";
			int num = 0;
			float num2 = (item.CylinderHammerClosed ? (item.DoubleActionAccuracyPenalty * (1f - firearmController.BuffInfo.DoubleActionRecoilReduce) * item.StockDoubleActionAccuracyPenaltyMult) : 0f);
			Vector2 recoilRotationStrength = player_0.ProceduralWeaponAnimation.Shootingg.CurrentRecoilEffect.RecoilRotationStrength;
			method_1(420);
			GUI.Box(new Rect(10f, method_2(20), 320f, 20f), "Hammer closed: " + item.CylinderHammerClosed.ToString(CultureInfo.InvariantCulture), guistyle_2);
			GUI.Box(new Rect(10f, method_2(20), 320f, 20f), "DoubleActionAccuracyPenalty = " + num2.ToString(CultureInfo.InvariantCulture), guistyle_2);
			Rect position = new Rect(10f, method_2(20), 320f, 20f);
			Vector2 vector = recoilRotationStrength;
			GUI.Box(position, "RecoilOnXY = " + vector.ToString(), guistyle_2);
			Rect position2 = new Rect(10f, method_2(20), 320f, 20f);
			modsCoolFactor = player_0.ProceduralWeaponAnimation.AimingSpeed;
			GUI.Box(position2, "Aim speed = " + modsCoolFactor, guistyle_2);
			for (int i = 0; i < cylinderMagazineItemClass.Camoras.Length; i++)
			{
				num++;
				Rect position3 = new Rect(10f, 500 + num * 20, 320f, 20f);
				string text2 = ((cylinderMagazineItemClass.Camoras[i].ContainedItem != null) ? cylinderMagazineItemClass.Camoras[i].ContainedItem.Name : text);
				GUI.Box(position3, text2, (cylinderMagazineItemClass.CurrentCamoraIndex == i) ? guistyle_2 : guistyle_0);
			}
		}
		int shortNameHash = firearmController.FirearmsAnimator.Animator.GetCurrentAnimatorStateInfo(1).shortNameHash;
		string animStateByNameHash = GClass758.GetAnimStateByNameHash(shortNameHash);
		string currentHandsOperationName = (player_0.HandsController as Player.ItemHandsController).CurrentHandsOperationName;
		method_1(35);
		GUI.Box(new Rect(10f, method_2(20), 220f, 20f), GClass2348.Localized(item.ShortName), guistyle_0);
		GUI.Box(new Rect(10f, method_2(20), 220f, 20f), currentHandsOperationName, guistyle_0);
		GUI.Box(new Rect(10f, method_2(20), 220f, 20f), $"{animStateByNameHash} | {shortNameHash}", guistyle_0);
		if (!item.AllowMalfunction)
		{
			GUI.Box(new Rect(10f, 35f, 220f, 20f), "Ignore malfunctions (taxonomy, IgnoreMalfunctions parameter) ", guistyle_0);
			return;
		}
		BackendConfigSettingsClass instance = Singleton<BackendConfigSettingsClass>.Instance;
		float currentOverheat = item.GetCurrentOverheat(GClass1891.PastTime, instance.Overheat, out modsCoolFactor);
		AmmoItemClass ammoItemClass = item.Chambers[0].ContainedItem as AmmoItemClass;
		bool flag3;
		bool flag2 = (flag3 = item.GetCurrentMagazine() != null) && item.GetCurrentMagazine().Count > 0;
		bool isBoltCatch = item.IsBoltCatch;
		float nextMalfunctionRandom = firearmController.GetNextMalfunctionRandom();
		double durabilityMalfChance;
		float magMalfChance;
		float ammoMalfChance;
		float overheatMalfChance;
		float weaponDurability;
		float totalMalfunctionChance = firearmController.GetTotalMalfunctionChance(ammoItemClass, currentOverheat, out durabilityMalfChance, out magMalfChance, out ammoMalfChance, out overheatMalfChance, out weaponDurability);
		if (!dictionary_0.ContainsKey(item))
		{
			dictionary_0.Add(item, -1);
		}
		if (!dictionary_1.ContainsKey(item))
		{
			dictionary_1.Add(item, 0);
		}
		if (Math.Abs(float_0 - nextMalfunctionRandom) > Mathf.Epsilon)
		{
			dictionary_0[item]++;
			dictionary_1[item] = dictionary_0[item];
			if (item.MalfState.State != Weapon.EMalfunctionState.None)
			{
				dictionary_0[item] = 0;
			}
		}
		int num3 = ((item.MalfState.State == Weapon.EMalfunctionState.None) ? dictionary_0[item] : dictionary_1[item]);
		method_1(95);
		GUI.Box(new Rect(10f, method_2(20), 220f, 20f), "Shots since malfunction: " + num3, guistyle_0);
		GUI.Box(new Rect(10f, method_2(20), 220f, 20f), "Trigger pressed: " + isTriggerPressed, guistyle_0);
		GUI.Box(new Rect(10f, method_2(20), 220f, 20f), "BoltCatch: " + flag, guistyle_0);
		GUI.Box(new Rect(10f, method_2(20), 220f, 20f), "TotalChance: " + totalMalfunctionChance, guistyle_0);
		string text3 = ((ammoItemClass == null) ? "Empty chamber" : "chamber full");
		if (flag3)
		{
			MagazineItemClass currentMagazine = item.GetCurrentMagazine();
			GUI.Box(new Rect(10f, method_2(20), 220f, 20f), $"AmmoInMag: {currentMagazine.Count}/{currentMagazine.MaxCount}, {text3}", guistyle_0);
		}
		else
		{
			GUI.Box(new Rect(10f, method_2(20), 220f, 20f), text3 ?? "", guistyle_0);
		}
		if (flag3)
		{
			GUI.Box(new Rect(10f, method_2(20), 220f, 20f), $"ANIM AmmoInMag : {floatAmmoInMag}, Chamber {floatAmmoInChamber}", guistyle_0);
		}
		else
		{
			GUI.Box(new Rect(10f, method_2(20), 220f, 20f), $"ANIM Chamber {floatAmmoInChamber}", guistyle_0);
		}
		if (item.MalfState.State == Weapon.EMalfunctionState.None)
		{
			GUI.Box(new Rect(10f, method_2(20), 220f, 20f), "NextAmmoRandom " + nextMalfunctionRandom, guistyle_0);
		}
		else
		{
			GUI.Box(new Rect(10f, method_2(20), 220f, 20f), "CurrentAmmoRandom " + float_0, guistyle_0);
		}
		if (item.MalfState.State != Weapon.EMalfunctionState.None)
		{
			GUI.Box(new Rect(10f, method_2(20), 220f, 20f), "Malfunctioned: " + item.MalfState.State, guistyle_1);
			bool flag4 = item.MalfState.IsKnownMalfunction(player_0.Profile.Id);
			bool flag5 = item.MalfState.IsKnownMalfType(player_0.Profile.Id);
			GUI.Box(new Rect(10f, method_2(20), 220f, 20f), "Is known malfunction: " + flag4, guistyle_0);
			GUI.Box(new Rect(10f, method_2(20), 220f, 20f), "Is known malf type: " + flag5, guistyle_0);
			float lastMalfunctionTime = item.MalfState.LastMalfunctionTime;
			bool flag6 = item.CanQuickdrawPistolAfterMalf(GClass1891.PastTime, instance.Malfunction);
			GUI.Box(new Rect(10f, method_2(20), 220f, 20f), "Last Malf Time: " + lastMalfunctionTime, guistyle_0);
			GUI.Box(new Rect(10f, method_2(20), 220f, 20f), "Can quickdraw pistol: " + flag6, flag6 ? guistyle_2 : guistyle_1);
		}
		else if (item.MalfState.SlideOnOverheatReached && currentOverheat >= instance.Overheat.FixSlideOverheat)
		{
			GUI.Box(new Rect(10f, method_2(20), 220f, 20f), "Next shot is Overheat slide!", guistyle_1);
		}
		else if (nextMalfunctionRandom <= totalMalfunctionChance)
		{
			GUI.Box(new Rect(10f, method_2(20), 220f, 20f), "Next shot malfunctioned!", guistyle_1);
		}
		list_0.Clear();
		firearmController.GetMalfunctionSources(list_0, durabilityMalfChance, magMalfChance, ammoMalfChance, overheatMalfChance, flag2, flag3);
		if (nextMalfunctionRandom <= totalMalfunctionChance && item.MalfState.State == Weapon.EMalfunctionState.None)
		{
			Weapon.EMalfunctionSource eMalfunctionSource = GClass820<Weapon.EMalfunctionSource>.GenerateDrop(list_0, nextMalfunctionRandom);
			method_1(35);
			GUI.Box(new Rect(230f, method_2(20), 220f, 120f), "Active source: " + eMalfunctionSource, guistyle_2);
			GUI.Box(new Rect(230f, method_2(20), 220f, 120f), method_3(list_0), guistyle_0);
			bool shouldCheckJam = flag2 || !isBoltCatch || !flag3;
			List<GClass820<Weapon.EMalfunctionState>.GStruct49<float, Weapon.EMalfunctionState>> list = new List<GClass820<Weapon.EMalfunctionState>.GStruct49<float, Weapon.EMalfunctionState>>();
			firearmController.GetSpecificMalfunctionVariants(list, ammoItemClass, eMalfunctionSource, weaponDurability, flag2, flag3, shouldCheckJam);
			GUI.Box(new Rect(230f, 175f, 220f, 120f), method_4(eMalfunctionSource, list), guistyle_0);
			GUI.Box(new Rect(230f, 55f, 220f, 120f), method_3(list_0), guistyle_0);
		}
		else if (item.MalfState.State != Weapon.EMalfunctionState.None)
		{
			GUI.Box(new Rect(230f, 35f, 220f, 120f), "Active source: " + item.MalfState.Source, guistyle_2);
		}
		else
		{
			GUI.Box(new Rect(230f, 35f, 220f, 120f), method_3(list_0), guistyle_0);
		}
		float_0 = nextMalfunctionRandom;
	}

	public string method_3(List<GClass820<Weapon.EMalfunctionSource>.GStruct49<float, Weapon.EMalfunctionSource>> sources)
	{
		string text = "Malfunction sources:";
		foreach (GClass820<Weapon.EMalfunctionSource>.GStruct49<float, Weapon.EMalfunctionSource> source in sources)
		{
			string[] obj = new string[5]
			{
				text,
				"\n",
				source.Item2.ToString(),
				" ",
				null
			};
			float item = source.Item1;
			obj[4] = item.ToString();
			text = string.Concat(obj);
		}
		return text;
	}

	public string method_4(Weapon.EMalfunctionSource currentSource, List<GClass820<Weapon.EMalfunctionState>.GStruct49<float, Weapon.EMalfunctionState>> states)
	{
		string text = "Source variants: " + currentSource;
		foreach (GClass820<Weapon.EMalfunctionState>.GStruct49<float, Weapon.EMalfunctionState> state in states)
		{
			string[] obj = new string[5]
			{
				text,
				"\n",
				state.Item2.ToString(),
				" ",
				null
			};
			float item = state.Item1;
			obj[4] = item.ToString();
			text = string.Concat(obj);
		}
		return text;
	}

	public Texture2D method_5(int width, int height, Color col)
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

	public static void SimulateShotsAndGatherStats(int shotsCount, Player.FirearmController firearmController, string filePath = null)
	{
		float ammoMalfChanceMult = Singleton<BackendConfigSettingsClass>.Instance.Malfunction.AmmoMalfChanceMult;
		float magazineMalfChanceMult = Singleton<BackendConfigSettingsClass>.Instance.Malfunction.MagazineMalfChanceMult;
		Weapon item = firearmController.Item;
		AmmoItemClass ammoToFire = item.Chambers[0].ContainedItem as AmmoItemClass;
		bool isMagazineInserted;
		bool hasAmmoInMag = (isMagazineInserted = item.GetCurrentMagazine() != null) && item.GetCurrentMagazine().Count > 0;
		bool isBoltCatch = item.IsBoltCatch;
		int num = 0;
		int num2 = 0;
		List<int> list = new List<int>();
		Dictionary<Weapon.EMalfunctionState, int> dictionary = new Dictionary<Weapon.EMalfunctionState, int>();
		float weaponDurability = Mathf.Floor(item.Repairable.Durability / item.Repairable.MaxDurability * 100f);
		BackendConfigSettingsClass instance = Singleton<BackendConfigSettingsClass>.Instance;
		float modsCoolFactor;
		float currentOverheat = item.GetCurrentOverheat(GClass1891.PastTime, instance.Overheat, out modsCoolFactor);
		for (int i = 0; i < shotsCount; i++)
		{
			Weapon.EMalfunctionSource malfunctionSource;
			Weapon.EMalfunctionState malfunctionState = firearmController.GetMalfunctionState(ammoToFire, hasAmmoInMag, isBoltCatch, isMagazineInserted, currentOverheat, instance.Overheat.FixSlideOverheat, out malfunctionSource);
			if (malfunctionState == Weapon.EMalfunctionState.None)
			{
				num2++;
				continue;
			}
			if (dictionary.ContainsKey(malfunctionState))
			{
				dictionary[malfunctionState]++;
			}
			else
			{
				dictionary.Add(malfunctionState, 1);
			}
			list.Add(num2);
			num2 = 0;
			num++;
		}
		int num3 = 0;
		int b = int.MinValue;
		int b2 = int.MaxValue;
		double durabilityMalfChance;
		float overheatMalfChance;
		float magMalfChance;
		float ammoMalfChance;
		float totalMalfunctionChance = firearmController.GetTotalMalfunctionChance(ammoToFire, currentOverheat, out durabilityMalfChance, out magMalfChance, out ammoMalfChance, out overheatMalfChance, out weaponDurability);
		magMalfChance /= magazineMalfChanceMult;
		ammoMalfChance /= ammoMalfChanceMult;
		List<GClass820<Weapon.EMalfunctionSource>.GStruct49<float, Weapon.EMalfunctionSource>> list2 = new List<GClass820<Weapon.EMalfunctionSource>.GStruct49<float, Weapon.EMalfunctionSource>>();
		firearmController.GetMalfunctionSources(list2, durabilityMalfChance, magMalfChance, ammoMalfChance, overheatMalfChance, hasAmmoInMag, isMagazineInserted);
		foreach (int item3 in list)
		{
			num3 += item3;
			b = Mathf.Max(item3, b);
			b2 = Mathf.Min(item3, b2);
		}
		num3 = ((list.Count != 0) ? (num3 / list.Count) : 0);
		Debug.Log("test for " + shotsCount + " shots for weapon " + GClass2348.Localized(item.Name) + " results:");
		Debug.Log("taxonomy base chance " + item.BaseMalfunctionChance);
		Debug.Log("taxonomy magazine chance " + magMalfChance);
		Debug.Log("taxonomy global mag mod " + magazineMalfChanceMult);
		Debug.Log("taxonomy ammo chance " + ammoMalfChance);
		Debug.Log("taxonomy global ammo mod " + ammoMalfChanceMult);
		Debug.Log("weapon durability " + weaponDurability);
		Debug.Log("overheat malf chance " + overheatMalfChance);
		Debug.Log("total malfunction chance " + totalMalfunctionChance);
		foreach (GClass820<Weapon.EMalfunctionSource>.GStruct49<float, Weapon.EMalfunctionSource> item4 in list2)
		{
			string text = item4.Item2.ToString().ToLower();
			float item2 = item4.Item1;
			Debug.Log(text + " chance " + item2);
		}
		Debug.Log("malfunctions count: " + num);
		Debug.Log("max shots count: " + b);
		Debug.Log("min shots count: " + b2);
		Debug.Log("avg shots count: " + num3);
		foreach (KeyValuePair<Weapon.EMalfunctionState, int> item5 in dictionary)
		{
			Debug.Log(item5.Key.ToString().ToLower() + " occured " + item5.Value + " times.");
		}
		if (string.IsNullOrEmpty(filePath))
		{
			return;
		}
		string text2 = "test for " + shotsCount + " shots for weapon " + GClass2348.Localized(item.Name) + " results:";
		text2 = text2 + "\ntaxonomy base chance " + item.BaseMalfunctionChance;
		text2 = text2 + "\ntaxonomy magazine chance " + magMalfChance;
		text2 = text2 + "\ntaxonomy global mag mod " + magazineMalfChanceMult;
		text2 = text2 + "\ntaxonomy ammo chance " + ammoMalfChance;
		text2 = text2 + "\ntaxonomy global ammo mod " + ammoMalfChanceMult;
		text2 = text2 + "\nweapon durability " + weaponDurability;
		Debug.Log("overheat malf chance " + overheatMalfChance);
		text2 = text2 + "\ntotal malfunction chance " + totalMalfunctionChance;
		foreach (GClass820<Weapon.EMalfunctionSource>.GStruct49<float, Weapon.EMalfunctionSource> item6 in list2)
		{
			string[] obj = new string[5]
			{
				text2,
				"\n",
				item6.Item2.ToString().ToLower(),
				" chance ",
				null
			};
			float item2 = item6.Item1;
			obj[4] = item2.ToString();
			text2 = string.Concat(obj);
		}
		text2 = text2 + "\nmalfunctions count: " + num;
		text2 = text2 + "\nmax shots count: " + b;
		text2 = text2 + "\nmin shots count: " + b2;
		text2 = text2 + "\navg shots count: " + num3;
		foreach (KeyValuePair<Weapon.EMalfunctionState, int> item7 in dictionary)
		{
			text2 = text2 + "\n" + item7.Key.ToString().ToLower() + " occured " + item7.Value + " times.";
		}
		text2 += "\nshot counts between malfunction sequences:";
		foreach (int item8 in list)
		{
			text2 = text2 + "\n" + item8;
		}
		File.WriteAllText(filePath, text2);
	}
}
