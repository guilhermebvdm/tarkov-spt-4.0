using System;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT;
using EFT.InventoryLogic;
using EFT.InventoryLogic.Operations;

public class BotSearchControllerClass : IPlayerSearchController, ISearchController
{
	[CompilerGenerated]
	private Action<Item> action_0;

	[CompilerGenerated]
	private Action<SearchableItemItemClass> action_1;

	[CompilerGenerated]
	private Action action_2;

	[NonSerialized]
	[CompilerGenerated]
	public MongoID MongoID_0;

	[NonSerialized]
	[CompilerGenerated]
	public GInterface155<SearchContentOperation> Ginterface155_0 = new GClass1628<SearchContentOperation>();

	public MongoID ProfileId
	{
		[CompilerGenerated]
		get
		{
			return MongoID_0;
		}
	}

	public bool CanSearch => false;

	public GInterface155<SearchContentOperation> SearchOperations
	{
		[CompilerGenerated]
		get
		{
			return Ginterface155_0;
		}
	}

	public event Action<Item> OnItemFound
	{
		[CompilerGenerated]
		add
		{
			Action<Item> action = action_0;
			Action<Item> action2;
			do
			{
				action2 = action;
				Action<Item> value2 = (Action<Item>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<Item> action = action_0;
			Action<Item> action2;
			do
			{
				action2 = action;
				Action<Item> value2 = (Action<Item>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<SearchableItemItemClass> OnItemSearched
	{
		[CompilerGenerated]
		add
		{
			Action<SearchableItemItemClass> action = action_1;
			Action<SearchableItemItemClass> action2;
			do
			{
				action2 = action;
				Action<SearchableItemItemClass> value2 = (Action<SearchableItemItemClass>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<SearchableItemItemClass> action = action_1;
			Action<SearchableItemItemClass> action2;
			do
			{
				action2 = action;
				Action<SearchableItemItemClass> value2 = (Action<SearchableItemItemClass>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnItemFullySearchedEvent
	{
		[CompilerGenerated]
		add
		{
			Action action = action_2;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_2;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public BotSearchControllerClass(Profile profile)
	{
		MongoID_0 = profile.ProfileId;
	}

	public void SetItemAsSearched<TItem>(TItem item) where TItem : SearchableItemItemClass, GInterface171
	{
	}

	public void SetItemAsKnown(Item item, bool raiseEvents)
	{
	}

	public bool IsSearched(SearchableItemItemClass item)
	{
		return true;
	}

	public bool ContainsUnknownItems(SearchableItemItemClass item)
	{
		return false;
	}

	public bool CanStartNewSearchOperation()
	{
		return false;
	}

	public void StopSearching(MongoID itemId)
	{
	}

	public void SearchContents(SearchableItemItemClass searchableItem)
	{
		throw new NotImplementedException();
	}

	public void OnItemFullySearched()
	{
		action_2?.Invoke();
	}

	public bool IsItemKnown(Item item, ItemAddress itemAddress = null)
	{
		return true;
	}

	public bool TryFindChangedContainer(ItemAddress address, out GClass1802 changedContainer)
	{
		changedContainer = null;
		return false;
	}

	public EObserverItemState GetObserverItemState(Item item, ItemAddress address)
	{
		return EObserverItemState.Known;
	}
}
