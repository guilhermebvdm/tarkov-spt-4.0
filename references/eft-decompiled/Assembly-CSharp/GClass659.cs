using Comfort.Common;
using UnityEngine;

public abstract class GClass659
{
	public static ArmorResistanceStruct RealResistance(float durability, float templateDurability, int armorClass, float penetrationPower)
	{
		float num = durability / templateDurability * 100f;
		int resistance = Singleton<BackendConfigSettingsClass>.Instance.Armor.GetArmorClass(armorClass).Resistance;
		float num2 = (121f - 5000f / (45f + num * 2f)) * (float)resistance * 0.01f;
		return new ArmorResistanceStruct
		{
			RealResistance = num2,
			ArmorClassResistance = resistance,
			CF = Mathf.Clamp(penetrationPower / (num2 + 12f), 0.6f, 1f)
		};
	}
}
