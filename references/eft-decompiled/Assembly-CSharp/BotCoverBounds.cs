using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BotCoverBounds : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	public class Class149
	{
		public static readonly Class149 class149_0 = new Class149();

		public static Func<BotCoverBounds, bool> func_0;

		public bool method_0(BotCoverBounds x)
		{
			return x.ExcludeCovers;
		}
	}

	[CompilerGenerated]
	public class Class150
	{
		public bool wall;

		public bool foliage;

		public bool snipe;

		public bool method_0(BotCoverBounds x)
		{
			if (!x.gameObject.activeInHierarchy)
			{
				return false;
			}
			if (wall && x.WallCovers)
			{
				return true;
			}
			if (foliage && x.FoliageCovers)
			{
				return true;
			}
			if (snipe && x.SnipeCovers)
			{
				return true;
			}
			return false;
		}
	}

	public BoxCollider Collider;

	public bool WallCovers = true;

	public bool FoliageCovers = true;

	public bool SnipeCovers;

	public bool ExcludeCovers;

	public int Id;

	public static List<BoxCollider> GetBoundsAsColliders(bool wall, bool foliage, bool snipe)
	{
		List<BotCoverBounds> bounds = GetBounds(wall, foliage, snipe);
		List<BoxCollider> list = new List<BoxCollider>();
		foreach (BotCoverBounds item in bounds)
		{
			list.Add(item.Collider);
		}
		return list;
	}

	public static List<BotCoverBounds> GetAllBounds()
	{
		return GClass870.FindObjectsOfTypeAll<BotCoverBounds>().ToList();
	}

	public static List<BotCoverBounds> GetBoundsExclude()
	{
		return (from x in GClass870.FindObjectsOfTypeAll<BotCoverBounds>()
			where x.ExcludeCovers
			select x).ToList();
	}

	public static List<BotCoverBounds> GetBounds(bool wall, bool foliage, bool snipe)
	{
		List<BotCoverBounds> list = GClass870.FindObjectsOfTypeAll<BotCoverBounds>().Where(delegate(BotCoverBounds x)
		{
			if (!x.gameObject.activeInHierarchy)
			{
				return false;
			}
			if (wall && x.WallCovers)
			{
				return true;
			}
			if (foliage && x.FoliageCovers)
			{
				return true;
			}
			return (snipe && x.SnipeCovers) ? true : false;
		}).ToList();
		_ = list.Count;
		return list;
	}

	public static void DisableAllCoilliders()
	{
		foreach (BotCoverBounds allBound in GetAllBounds())
		{
			allBound.Collider.enabled = false;
		}
	}
}
