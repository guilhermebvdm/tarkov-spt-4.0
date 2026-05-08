using UnityEngine;

public abstract class GClass935
{
	public static void UpdateHueVariation(this Material material, EHueVariationEnabledState state, string hueVariationEffect, int hueVariationId, Color hueVariationColor)
	{
		bool enable = state != EHueVariationEnabledState.Disabled;
		GClass1002.SetKeyword(material, hueVariationEffect, enable);
		if (state == EHueVariationEnabledState.EnabledWithColorModification)
		{
			material.SetColor(hueVariationId, hueVariationColor);
		}
	}
}
