using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class GClass402
{
	public GClass409 DebugCoversChecker;

	public EditorsCoverPointDebugDataClass EditorsCoverPointDebugData;

	[NonSerialized]
	public GClass401 Gclass401_0;

	[NonSerialized]
	public BotOwner BotOwner_0;

	[NonSerialized]
	public HashSet<GClass367> HashSet_0 = new HashSet<GClass367>();

	public GClass402(BotOwner owner)
	{
		BotOwner_0 = owner;
	}

	public string GetLastNode()
	{
		return "zero";
	}

	public void Init()
	{
		DebugCoversChecker = new GClass409(BotOwner_0);
	}

	public void Stuck(Vector3 position)
	{
	}

	public void SetLastSearch(HashSet<CustomNavigationPoint> nearGroups, Vector3 CenterPos, CustomNavigationPoint betterPoint)
	{
	}

	public void TryDrawLastSearchData()
	{
		if (Gclass401_0 != null)
		{
			GClass369.DrawGroups(Gclass401_0.AllSavePoints, Gclass401_0.CenterSearch, Gclass401_0.MaxPoint, Gclass401_0._searchBeforeHide);
		}
	}

	public void SetLastSearchBeforeHide(HashSet<CustomNavigationPoint> nearGroups)
	{
	}

	public void Dispose()
	{
		DebugCoversChecker?.Dispose();
		EditorsCoverPointDebugData?.Dispose();
		HashSet_0?.Clear();
	}
}
