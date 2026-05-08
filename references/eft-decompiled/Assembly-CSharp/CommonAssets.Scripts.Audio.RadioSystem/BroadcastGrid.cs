using System;
using System.Collections.Generic;

namespace CommonAssets.Scripts.Audio.RadioSystem;

[Serializable]
public class BroadcastGrid
{
	[NonSerialized]
	public Queue<BroadcastItemData> BroadcastQueue = new Queue<BroadcastItemData>();

	[NonSerialized]
	public BroadcastItemsContainer ItemsContainer;

	[NonSerialized]
	public BroadcastRule Rule;

	public int Count => BroadcastQueue.Count;

	public BroadcastGrid(BroadcastItemsContainer itemsContainer, BroadcastRule rule)
	{
		ItemsContainer = itemsContainer;
		Rule = rule;
		method_0();
	}

	public void method_0()
	{
		BroadcastQueue.Clear();
		int num = 0;
		int num2 = 0;
		float num3 = TransformHelperClass.RandomInt(Rule.tracksBeforeJingle);
		float num4 = TransformHelperClass.RandomInt(Rule.adsAfterTracks);
		float num5 = TransformHelperClass.RandomInt(Rule.showsCount);
		float num6 = TransformHelperClass.RandomInt(Rule.showAfterTracks);
		for (int i = 0; i < Rule.broadcastItemsCount; i++)
		{
			if (method_1(EBroadcastItemType.Music))
			{
				num++;
				if ((float)num % num3 == 0f)
				{
					method_1(EBroadcastItemType.Jingle);
				}
				if ((float)num % num4 == 0f)
				{
					method_1(EBroadcastItemType.Ad);
				}
				if (Rule.includeShow && (float)num2 < num5 && (float)num % num6 == 0f && method_1(EBroadcastItemType.Show))
				{
					num2++;
				}
			}
		}
	}

	public bool method_1(EBroadcastItemType itemType)
	{
		if (!ItemsContainer.TryGetRandomBroadcastData(itemType, out var data))
		{
			return false;
		}
		BroadcastQueue.Enqueue(data);
		return true;
	}

	public bool TryGetBroadcastItemData(out BroadcastItemData item)
	{
		item = null;
		if (BroadcastQueue.Count == 0)
		{
			return false;
		}
		item = BroadcastQueue.Dequeue();
		return true;
	}

	public void RebuildGrid()
	{
		method_0();
	}
}
