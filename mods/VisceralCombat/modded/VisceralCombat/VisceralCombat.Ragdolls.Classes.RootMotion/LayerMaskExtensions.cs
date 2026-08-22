using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion;

public static class LayerMaskExtensions
{
	public static bool Contains(LayerMask mask, int layer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return mask == (mask | (1 << layer));
	}

	public static LayerMask Create(params string[] layerNames)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return NamesToMask(layerNames);
	}

	public static LayerMask Create(params int[] layerNumbers)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return LayerNumbersToMask(layerNumbers);
	}

	public static LayerMask NamesToMask(params string[] layerNames)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		LayerMask val = 0;
		foreach (string text in layerNames)
		{
			val = val | (1 << LayerMask.NameToLayer(text));
		}
		return val;
	}

	public static LayerMask LayerNumbersToMask(params int[] layerNumbers)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		LayerMask val = 0;
		foreach (int num in layerNumbers)
		{
			val = val | (1 << num);
		}
		return val;
	}

	public static LayerMask Inverse(this LayerMask original)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return ~original;
	}

	public static LayerMask AddToMask(this LayerMask original, params string[] layerNames)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		return original | NamesToMask(layerNames);
	}

	public static LayerMask RemoveFromMask(this LayerMask original, params string[] layerNames)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		LayerMask val = ~original;
		return ~(val | NamesToMask(layerNames));
	}

	public static string[] MaskToNames(this LayerMask original)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		List<string> list = new List<string>();
		for (int i = 0; i < 32; i++)
		{
			int num = 1 << i;
			if ((original & num) == num)
			{
				string text = LayerMask.LayerToName(i);
				if (!string.IsNullOrEmpty(text))
				{
					list.Add(text);
				}
			}
		}
		return list.ToArray();
	}

	public static int[] MaskToNumbers(this LayerMask original)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		List<int> list = new List<int>();
		for (int i = 0; i < 32; i++)
		{
			int num = 1 << i;
			if ((original & num) == num)
			{
				list.Add(i);
			}
		}
		return list.ToArray();
	}

	public static string MaskToString(this LayerMask original)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return original.MaskToString(", ");
	}

	public static string MaskToString(this LayerMask original, string delimiter)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return string.Join(delimiter, original.MaskToNames());
	}
}
