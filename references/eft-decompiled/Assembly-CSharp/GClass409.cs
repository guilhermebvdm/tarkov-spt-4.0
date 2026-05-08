using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;

public class GClass409
{
	[Serializable]
	[CompilerGenerated]
	public class Class132
	{
		public static readonly Class132 class132_0 = new Class132();

		public static Func<CustomNavigationPoint, bool> func_0;

		public bool method_0(CustomNavigationPoint x)
		{
			return true;
		}
	}

	public bool IsStuck;

	public bool InEdnCompete;

	[NonSerialized]
	public List<CustomNavigationPoint> List_0;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public BotOwner BotOwner_0;

	public GClass409(BotOwner owner)
	{
		BotOwner_0 = owner;
	}

	public CustomNavigationPoint GetPoint(bool closest, bool withRemove)
	{
		if (List_0 == null || List_0.Count == 0)
		{
			List_0 = BotOwner_0.VoxelesPersonalData.Points((CustomNavigationPoint x) => true).ToList();
		}
		CustomNavigationPoint customNavigationPoint = ((!closest) ? GClass856.RandomElement(List_0) : List_0.OrderBy((CustomNavigationPoint point) => (point.Position - BotOwner_0.Transform.position).sqrMagnitude).FirstOrDefault());
		if (withRemove)
		{
			List_0.Remove(customNavigationPoint);
		}
		return customNavigationPoint;
	}

	public void Stuck()
	{
		IsStuck = true;
	}

	public void SetNewPoint()
	{
		CustomNavigationPoint point = GetPoint(closest: false, withRemove: false);
		if (point != null)
		{
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(point);
		}
	}

	public void Dispose()
	{
		CustomNavigationPoint_0 = null;
	}

	[CompilerGenerated]
	public float method_0(CustomNavigationPoint point)
	{
		return (point.Position - BotOwner_0.Transform.position).sqrMagnitude;
	}
}
