using System;
using System.Collections.Generic;
using System.Text;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class GClass380 : GClass379
{
	[NonSerialized]
	public HashSet<int> HashSet_0 = new HashSet<int>();

	[NonSerialized]
	public GClass368<GroupPointSearchData> Gclass368_0 = new GClass368<GroupPointSearchData>();

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public GroupPoint GroupPoint_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public const float Float_1 = 49f;

	[NonSerialized]
	public StringBuilder StringBuilder_0;

	[NonSerialized]
	public int Int_0;

	public static GroupPoint GetStartPoint(IBot bot, Vector3 pos, out string replaceInfo)
	{
		CustomNavigationPoint lastCover = bot.LastCover;
		replaceInfo = "";
		if (lastCover != null)
		{
			Vector3 vector = lastCover.Position - pos;
			if (Mathf.Abs(vector.y) < 1f && vector.sqrMagnitude < 16f)
			{
				return lastCover.GroupPoint;
			}
		}
		NavGraphVoxelSimple voxelSafe = bot.GetVoxelSafe(pos);
		int connectionGroupId = bot.ConnectionGroupId;
		bool withFoliage = bot.Type != WildSpawnType.assault && bot.Type != WildSpawnType.assaultGroup;
		GroupPoint closestsPointVoxelesExtended = bot.GetClosestsPointVoxelesExtended(pos, 1, connectionGroupId, withFoliage);
		if (closestsPointVoxelesExtended != null)
		{
			return closestsPointVoxelesExtended;
		}
		GroupPoint groupPoint = voxelSafe.PointStartSearch;
		if (groupPoint == null || groupPoint.ConnectionGroup != connectionGroupId)
		{
			if (lastCover != null && lastCover.GroupPoint.ConnectionGroup == connectionGroupId)
			{
				groupPoint = lastCover.GroupPoint;
			}
			if (groupPoint == null || groupPoint.ConnectionGroup != connectionGroupId)
			{
				using List<GroupPoint>.Enumerator enumerator = voxelSafe.Points.GetEnumerator();
				while (enumerator.MoveNext() && enumerator.Current.ConnectionGroup != connectionGroupId)
				{
				}
			}
			if (groupPoint == null || groupPoint.ConnectionGroup != connectionGroupId)
			{
				groupPoint = FoundStartPointExtended(bot, pos);
			}
			if (groupPoint == null || groupPoint.ConnectionGroup != connectionGroupId)
			{
				groupPoint = bot.GetClosest(pos, connectionGroupId);
			}
			if (groupPoint != null)
			{
			}
		}
		return groupPoint;
	}

	public CustomNavigationPoint GetClosestPoint(BotOwner bot, Vector3 pos, bool noRestrictions, [CanBeNull] Func<GroupPoint, bool> goodFunc, bool printErrorLogsIfFail, int maxIterations = 1000)
	{
		_ = bot.StartCorePoint.ConnectionGroupId;
		string replaceInfo;
		GroupPoint startPoint = GetStartPoint(bot.CoverSearchInfo, pos, out replaceInfo);
		bool flag = GClass398.Instance.IsTraceEnable();
		Gclass368_0.Clear();
		HashSet_0.Clear();
		GroupPoint_0 = null;
		Float_0 = float.MaxValue;
		Int_0 = 0;
		int deepCheck = 0;
		Bool_0 = GClass398.Instance.IsTraceEnable() && DebugBotData.UseDebugData && DebugBotData.Instance.DebugCoverLogs;
		StringBuilder_0 = new StringBuilder();
		if (Bool_0)
		{
			method_7($"botid:{bot.Id}  {bot.Profile.Info.Settings.Role.ToString()}  at start pos:{pos}  botPos:{bot.Position} ");
		}
		if (goodFunc == null)
		{
			noRestrictions = true;
		}
		if (startPoint != null)
		{
			if (printErrorLogsIfFail || flag)
			{
				_ = $"Start point id:{startPoint.Id}  pos:{startPoint.Position}";
			}
			if (startPoint.IsFreeById(bot.Id) && !startPoint.IsSpotted && (noRestrictions || goodFunc(startPoint)))
			{
				CustomNavigationPoint byId = startPoint.GetById(bot.Id);
				if (flag)
				{
					method_5(pos, bot, byId);
				}
				return byId;
			}
			if (method_8(bot, pos, noRestrictions: false, goodFunc, startPoint, maxIterations, out var customNavigationPoint, ref deepCheck))
			{
				if (flag)
				{
					method_5(pos, bot, customNavigationPoint);
				}
				return customNavigationPoint;
			}
		}
		if (GroupPoint_0 != null)
		{
			return GroupPoint_0.GetById(bot.Id);
		}
		if (!noRestrictions)
		{
			CustomNavigationPoint customNavigationPoint = GetClosestPoint(bot, pos, noRestrictions: true, null, printErrorLogsIfFail: false);
			if (customNavigationPoint != null)
			{
				if (flag)
				{
					method_5(pos, bot, customNavigationPoint);
				}
				return customNavigationPoint;
			}
		}
		return null;
	}

	public static GroupPoint FoundStartPointExtended(IBot bot, Vector3 pos)
	{
		for (int i = 2; i < 10; i++)
		{
			foreach (NavGraphVoxelSimple item in bot.GetVoxelesExtended(pos, i))
			{
				foreach (GroupPoint point in item.Points)
				{
					if (point.ConnectionGroup == bot.ConnectionGroupId)
					{
						return item.PointStartSearch;
					}
				}
			}
		}
		return null;
	}

	public void method_5(Vector3 startPos, BotOwner bot, CustomNavigationPoint taken)
	{
		bot?.CoverSearchInfo.SetLastSearchClosestsDebug(Gclass368_0, startPos, taken);
	}

	public GroupPointSearchData method_6(GroupPoint groupP, Vector3 targetPos)
	{
		if (HashSet_0.Contains(groupP.Id))
		{
			return null;
		}
		float power = 1f / (groupP.Position - targetPos).sqrMagnitude;
		GroupPointSearchData groupPointSearchData = new GroupPointSearchData(groupP, power);
		Gclass368_0.Add(groupPointSearchData);
		if (Bool_0)
		{
			method_7($" add:{groupP.Id} ");
		}
		return groupPointSearchData;
	}

	public void method_7(string info, bool withLine = false)
	{
		try
		{
			if (withLine)
			{
				StringBuilder_0.AppendLine(info);
			}
			else
			{
				StringBuilder_0.Append(info);
			}
		}
		catch (Exception)
		{
			Bool_0 = false;
		}
	}

	public bool method_8(BotOwner bot, Vector3 pos, bool noRestrictions, Func<GroupPoint, bool> goodFunc, GroupPoint curUsingPoint, int maxIterations, out CustomNavigationPoint customNavigationPoint, ref int deepCheck)
	{
		if (Bool_0)
		{
			method_7("", withLine: true);
		}
		Int_0++;
		if (Int_0 <= maxIterations && Int_0 <= 1000)
		{
			deepCheck++;
			if (method_6(curUsingPoint, pos) == null)
			{
				if (Gclass368_0.Count > 0)
				{
					GroupPointSearchData groupPointSearchData = Gclass368_0.TakeFirstAndRemove();
					return method_8(bot, pos, noRestrictions, goodFunc, groupPointSearchData.Point, maxIterations, out customNavigationPoint, ref deepCheck);
				}
				if (Bool_0)
				{
					method_7($" dataToCheck == null. _list:{Gclass368_0.Count}");
				}
				customNavigationPoint = null;
				return false;
			}
			if (curUsingPoint == null)
			{
				customNavigationPoint = null;
				return false;
			}
			if (noRestrictions || goodFunc == null || goodFunc(curUsingPoint))
			{
				if (noRestrictions)
				{
					customNavigationPoint = curUsingPoint.GetById(bot.Id);
					return true;
				}
				if (curUsingPoint.IsFreeById(bot.Id))
				{
					customNavigationPoint = curUsingPoint.GetById(bot.Id);
					return true;
				}
			}
			if (!curUsingPoint.IsSpotted && curUsingPoint.IsFreeById(bot.Id))
			{
				float num = Mathf.Abs(49f - (curUsingPoint.Position - pos).sqrMagnitude);
				if (num < Float_0)
				{
					Float_0 = num;
					GroupPoint_0 = curUsingPoint;
				}
			}
			HashSet_0.Add(curUsingPoint.Id);
			if (Bool_0)
			{
				method_7($" add from Neigh:{curUsingPoint.NeighbourhoodsWays.Count}(");
			}
			foreach (GroupPointWay neighbourhoodsWay in curUsingPoint.NeighbourhoodsWays)
			{
				GroupPoint target = neighbourhoodsWay.Target;
				method_6(target, pos);
			}
			if (Bool_0)
			{
				method_7($" ) _list:{Gclass368_0.Count}");
			}
			if (Gclass368_0.Count == 0)
			{
				customNavigationPoint = null;
				return false;
			}
			GroupPointSearchData groupPointSearchData2 = Gclass368_0.TakeFirstAndRemove();
			if (Bool_0)
			{
				method_7($" take from:{groupPointSearchData2.Point.Id} ");
			}
			return method_8(bot, pos, noRestrictions, goodFunc, groupPointSearchData2.Point, maxIterations, out customNavigationPoint, ref deepCheck);
		}
		if (Bool_0)
		{
			method_7(" tooMuch iterations");
		}
		customNavigationPoint = null;
		return false;
	}
}
