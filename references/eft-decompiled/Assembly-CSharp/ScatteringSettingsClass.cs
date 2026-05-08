using System;
using System.Collections.Generic;
using EFT.InventoryLogic;
using UnityEngine;

public class ScatteringSettingsClass
{
	public GClass612 CurrentScattering;

	[NonSerialized]
	public Dictionary<string, GClass612> Dictionary_0 = new Dictionary<string, GClass612>();

	public ScatteringSettingsClass(GClass612[] scatterings)
	{
		CurrentScattering = new GClass612();
		if (scatterings == null)
		{
			Debug.LogError("Scattrings is NULL check backend");
			return;
		}
		for (int i = 0; i < scatterings.Length; i++)
		{
			GClass612 gClass = new GClass612(scatterings[i], null);
			Dictionary_0.Add(gClass.Name, gClass);
		}
	}

	public void Check(BotGlobalsScatteringSettings settings)
	{
		CurrentScattering.Check(settings);
		foreach (KeyValuePair<string, GClass612> item in Dictionary_0)
		{
			item.Value.Check(settings);
		}
	}

	public void SetWeapon(Weapon weapon)
	{
		if (Dictionary_0.TryGetValue(weapon.Template.weapClass, out var value))
		{
			CurrentScattering = value;
		}
		else
		{
			Debug.LogError("back end no json ype for scattering for weapon.Template.weapClass:" + weapon.Template.weapClass);
		}
	}
}
