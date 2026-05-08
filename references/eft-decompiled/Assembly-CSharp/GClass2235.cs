using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.InventoryLogic.Operations;

public abstract class GClass2235 : GClass2234, IPlayerSearchController, ISearchController
{
	[CompilerGenerated]
	public class Class1423
	{
		public GClass2235 gclass2235_0;

		public SearchContentOperation searchContentOperation;

		public void method_0(IResult error)
		{
			gclass2235_0.Gclass1628_0.Remove(searchContentOperation);
		}
	}

	[CompilerGenerated]
	public class Class1424
	{
		public MongoID itemId;

		public bool method_0(SearchContentOperation x)
		{
			return (MongoID)x.Item.Id == itemId;
		}
	}

	[NonSerialized]
	public Profile Profile_0;

	[NonSerialized]
	public Player.PlayerInventoryController PlayerInventoryController_0;

	[NonSerialized]
	public HashSet<SearchableItemItemClass> HashSet_0 = new HashSet<SearchableItemItemClass>();

	[NonSerialized]
	public GClass1628<SearchContentOperation> Gclass1628_0 = new GClass1628<SearchContentOperation>
	{
		IsExceptionSafe = true
	};

	public GInterface155<SearchContentOperation> SearchOperations => Gclass1628_0;

	public MongoID ProfileId => Profile_0.Id;

	public override bool CanSearch => true;

	public GClass2235(Profile profile, Player.PlayerInventoryController inventoryController)
	{
		Profile_0 = profile;
		PlayerInventoryController_0 = inventoryController;
	}

	public override void SearchContents(SearchableItemItemClass searchableItem)
	{
		SearchContentOperation searchContentOperation = PlayerInventoryController_0.vmethod_2(searchableItem);
		Gclass1628_0.Add(searchContentOperation);
		PlayerInventoryController_0.vmethod_1(searchContentOperation, delegate
		{
			Gclass1628_0.Remove(searchContentOperation);
		});
	}

	public override void StopSearching(MongoID itemId)
	{
		SearchContentOperation searchContentOperation = Gclass1628_0.FirstOrDefault((SearchContentOperation x) => (MongoID)x.Item.Id == itemId);
		if (searchContentOperation != null)
		{
			StopSearchOperation(searchContentOperation);
		}
	}

	public override bool CanStartNewSearchOperation()
	{
		if (Profile_0.SkillsInfo == null || !Profile_0.SkillsInfo.IsSearchDouble)
		{
			return Gclass1628_0.Count < 1;
		}
		return Gclass1628_0.Count < 2;
	}

	public virtual void SetItemAsSearched<TItem>(TItem item) where TItem : SearchableItemItemClass, GInterface171
	{
		if (HashSet_0.Add(item))
		{
			method_2(item);
		}
	}

	public override bool IsSearched(SearchableItemItemClass item)
	{
		return HashSet_0.Contains(item);
	}

	public virtual void UncoverItem<TItem>(TItem item) where TItem : SearchableItemItemClass, GInterface171
	{
		HashSet_0.Add(item);
	}

	public virtual void StopSearchOperation(SearchContentOperation operation)
	{
		operation.Terminate();
	}

	public override bool ContainsUnknownItems(SearchableItemItemClass item)
	{
		foreach (EFT.InventoryLogic.IContainer container in item.Containers)
		{
			if (!(container is GInterface215 gInterface))
			{
				continue;
			}
			foreach (Item item2 in gInterface.Items)
			{
				if (!IsItemKnown(item2))
				{
					return true;
				}
			}
		}
		return false;
	}

	public abstract void SetItemAsKnown(Item item, bool raiseEvents);
}
