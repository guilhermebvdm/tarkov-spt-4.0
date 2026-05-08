using System.Collections.Generic;
using UnityEngine;

public abstract class GClass1458
{
	public static bool Contains(LayerMask mask, int layer)
	{
		return (int)mask == ((int)mask | (1 << layer));
	}

	public static LayerMask Create(params string[] layerNames)
	{
		return NamesToMask(layerNames);
	}

	public static LayerMask Create(params int[] layerNumbers)
	{
		return LayerNumbersToMask(layerNumbers);
	}

	public static LayerMask NamesToMask(params string[] layerNames)
	{
		LayerMask layerMask = 0;
		foreach (string layerName in layerNames)
		{
			layerMask = (int)layerMask | (1 << LayerMask.NameToLayer(layerName));
		}
		return layerMask;
	}

	public static LayerMask LayerNumbersToMask(params int[] layerNumbers)
	{
		LayerMask layerMask = 0;
		foreach (int num in layerNumbers)
		{
			layerMask = (int)layerMask | (1 << num);
		}
		return layerMask;
	}

	public static LayerMask Inverse(this LayerMask original)
	{
		return ~(int)original;
	}

	public static LayerMask AddToMask(this LayerMask original, params string[] layerNames)
	{
		return (int)original | (int)NamesToMask(layerNames);
	}

	public static LayerMask RemoveFromMask(this LayerMask original, params string[] layerNames)
	{
		return ~((int)(LayerMask)(~(int)original) | (int)NamesToMask(layerNames));
	}

	public static string[] MaskToNames(this LayerMask original)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < 32; i++)
		{
			int num = 1 << i;
			if (((int)original & num) == num)
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
		List<int> list = new List<int>();
		for (int i = 0; i < 32; i++)
		{
			int num = 1 << i;
			if (((int)original & num) == num)
			{
				list.Add(i);
			}
		}
		return list.ToArray();
	}

	public static string MaskToString(this LayerMask original)
	{
		return MaskToString(original, ", ");
	}

	public static string MaskToString(this LayerMask original, string delimiter)
	{
		return string.Join(delimiter, MaskToNames(original));
	}

	public static LayerMask GetAllCollisionsLayerMask(string layerCollision)
	{
		return GetAllCollisionsLayerMask(LayerMask.NameToLayer(layerCollision));
	}

	public static LayerMask GetAllCollisionsLayerMask(int layerCollision)
	{
		LayerMask layerMask = 0;
		for (int i = 0; i < 32; i++)
		{
			if (!Physics.GetIgnoreLayerCollision(i, layerCollision))
			{
				layerMask = (int)layerMask | (1 << i);
			}
		}
		return layerMask;
	}

	public static void IgnoreLayerCollision(int someLayer, LayerMask layersMask, bool ignore = true)
	{
		for (int i = 0; i < 32; i++)
		{
			int num = 1 << i;
			if (((int)layersMask & num) == num && Physics.GetIgnoreLayerCollision(i, someLayer) != ignore)
			{
				Physics.IgnoreLayerCollision(i, someLayer, ignore);
			}
		}
	}
}
