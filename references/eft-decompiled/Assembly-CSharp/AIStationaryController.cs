using System;
using System.Collections.Generic;
using UnityEngine;

public class AIStationaryController : MonoBehaviour
{
	public StationaryWeaponLink[] Weapons = Array.Empty<StationaryWeaponLink>();

	public void Init(AICorePointHolder corePointHolder)
	{
		StationaryWeaponLink[] weapons = Weapons;
		foreach (StationaryWeaponLink obj in weapons)
		{
			obj.Init();
			obj.Restore(corePointHolder);
		}
	}

	public void SetList(List<StationaryWeaponLink> list)
	{
		Weapons = list.ToArray();
	}

	public int GetCount()
	{
		if (Weapons == null)
		{
			return 0;
		}
		return Weapons.Length;
	}

	public void Clear()
	{
		StationaryWeaponLink[] weapons = Weapons;
		for (int i = 0; i < weapons.Length; i++)
		{
			UnityEngine.Object.DestroyImmediate(weapons[i].gameObject);
		}
		Weapons = Array.Empty<StationaryWeaponLink>();
	}
}
