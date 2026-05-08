using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public abstract class TransferItemsControllerAbstractClass
{
	[CompilerGenerated]
	public class Class1172
	{
		public string profileId;

		public bool method_0(StashItemClass container)
		{
			return container.Id == profileId;
		}
	}

	[CompilerGenerated]
	public class Class1173
	{
		public string profileId;

		public bool method_0(StashItemClass container)
		{
			return container.Id == profileId;
		}
	}

	[CompilerGenerated]
	public class Class1174
	{
		public Player player;

		public bool method_0(StashItemClass grid)
		{
			return grid.Id == player.ProfileId;
		}

		public bool method_1(StashGridClass grid)
		{
			return grid.ID == player.ProfileId;
		}
	}

	[CompilerGenerated]
	public class Class1175
	{
		public string profileId;

		public bool method_0(StashItemClass grid)
		{
			return grid.Id == profileId;
		}
	}

	[NonSerialized]
	public TraderControllerClass TraderControllerClass;

	[NonSerialized]
	public List<StashItemClass> List_0 = new List<StashItemClass>();

	[NonSerialized]
	public List<Item> List_1 = new List<Item>();

	[NonSerialized]
	[CompilerGenerated]
	public StashItemClass StashItemClass;

	[NonSerialized]
	[CompilerGenerated]
	public ETraderServiceType EtraderServiceType_0;

	public Action OnCautionState;

	[NonSerialized]
	public GameWorld GameWorld_0;

	[NonSerialized]
	public bool Bool_0;

	public StashItemClass Stash
	{
		[CompilerGenerated]
		get
		{
			return StashItemClass;
		}
		[CompilerGenerated]
		set
		{
			StashItemClass = value;
		}
	}

	public ETraderServiceType ServiceType
	{
		[CompilerGenerated]
		get
		{
			return EtraderServiceType_0;
		}
		[CompilerGenerated]
		set
		{
			EtraderServiceType_0 = value;
		}
	}

	public TransferItemsControllerAbstractClass(GameWorld gameWorld, bool isOfflineGame, ETraderServiceType serviceType)
	{
		GameWorld_0 = gameWorld;
		Bool_0 = isOfflineGame;
		ServiceType = serviceType;
	}

	public bool HasNonEmptyTransferContainer(string profileId)
	{
		return List_0.FirstOrDefault((StashItemClass container) => container.Id == profileId)?.Grid.Items.Any() ?? false;
	}

	public void InitTransferContainer(StashItemClass newContainer, string name)
	{
		string id = newContainer.Id;
		if (!HasNonEmptyTransferContainer(id))
		{
			List_0.Add(newContainer);
			TraderControllerClass key = new TraderControllerClass(newContainer, id, name, canBeLocalized: false);
			GameWorld_0.ItemOwners.Add(key, new GameWorld.GStruct162
			{
				DoNotPerformPickUpValidation = true
			});
			return;
		}
		StashItemClass orAddTransferContainer = GetOrAddTransferContainer(id);
		StashGridClass grid = orAddTransferContainer.Grid;
		StashGridClass grid2 = newContainer.Grid;
		TraderControllerClass itemController = (TraderControllerClass)orAddTransferContainer.Owner;
		GStruct154<GClass3411> gStruct;
		do
		{
			if (newContainer.Grid.Items.Any())
			{
				Item item = grid2.Items.First();
				LocationInGrid itemLocation = grid2.GetItemLocation(item);
				GClass3393 to = grid.CreateItemAddress(itemLocation);
				gStruct = InteractionsHandlerClass.Move(item, to, itemController);
				continue;
			}
			return;
		}
		while (!gStruct.Failed);
		throw new Exception(gStruct.Error.ToString());
	}

	public void InitPlayerStash(Player player)
	{
		StashItemClass orAddTransferContainer = GetOrAddTransferContainer(player.ProfileId);
		if (orAddTransferContainer.Grid.ItemCollection.Any())
		{
			orAddTransferContainer.Grid.RemoveAll();
		}
		StashGridClass[] array = new StashGridClass[Stash.Grids.Length + 1];
		Stash.Grids.CopyTo(array, 0);
		array[^1] = new StashGridClass(player.ProfileId, 10, 10, canStretchVertically: true, canStretchHorizontally: false, Array.Empty<ItemFilter>(), Stash);
		Stash.Grids = array;
	}

	public StashItemClass GetOrAddTransferContainer(string profileId)
	{
		StashItemClass stashItemClass = List_0.FirstOrDefault((StashItemClass container) => container.Id == profileId);
		if (stashItemClass != null)
		{
			XYCellSizeStruct xYCellSizeStruct = method_0(profileId);
			if (stashItemClass.Grid.GridWidth != xYCellSizeStruct.X || stashItemClass.Grid.GridHeight != xYCellSizeStruct.Y)
			{
				stashItemClass.Grid.ClampSize(xYCellSizeStruct.X, xYCellSizeStruct.Y, force: true);
			}
			return stashItemClass;
		}
		StashItemClass stashItemClass2 = Singleton<ItemFactoryClass>.Instance.CreateFakeStash(profileId);
		string iD = stashItemClass2.Grid.ID;
		XYCellSizeStruct xYCellSizeStruct2 = method_0(profileId);
		StashGridClass stashGridClass = new StashGridClass(iD, xYCellSizeStruct2.X, xYCellSizeStruct2.Y, canStretchVertically: false, canStretchHorizontally: false, Array.Empty<ItemFilter>(), stashItemClass2);
		stashItemClass2.Grids = new StashGridClass[1] { stashGridClass };
		TraderControllerClass key = new TraderControllerClass(stashItemClass2, "656f0f98d80a697f855d34b1", "BTR", canBeLocalized: false);
		GameWorld_0.ItemOwners.Add(key, new GameWorld.GStruct162
		{
			DoNotPerformPickUpValidation = true
		});
		List_0.Add(stashItemClass2);
		return stashItemClass2;
	}

	public XYCellSizeStruct method_0(string profileId)
	{
		Player alivePlayerByProfileID = GameWorld_0.GetAlivePlayerByProfileID(profileId);
		if (alivePlayerByProfileID == null)
		{
			Debug.LogError("Cant find player with id " + profileId);
			return new XYCellSizeStruct(0, 0);
		}
		Vector3 gridSize = GetGridSize(alivePlayerByProfileID.Profile);
		return new XYCellSizeStruct((int)gridSize.x, (int)gridSize.y);
	}

	public void InitItemControllerServer(MongoID traderId, string name)
	{
		Stash = Singleton<ItemFactoryClass>.Instance.CreateFakeStash(traderId);
		Stash.Grids = Array.Empty<StashGridClass>();
		TraderControllerClass = new TraderControllerClass(Stash, name + " Extraction Controller", name, canBeLocalized: false);
		GameWorld_0.ItemOwners.Add(TraderControllerClass, new GameWorld.GStruct162
		{
			DoNotPerformPickUpValidation = true
		});
	}

	public void MoveItemsFromTempStashToTransferStash(Player player)
	{
		if (!Bool_0)
		{
			return;
		}
		StashItemClass stashItemClass = List_0.FirstOrDefault((StashItemClass grid) => grid.Id == player.ProfileId);
		StashGridClass stashGridClass = Stash.Grids.FirstOrDefault((StashGridClass grid) => grid.ID == player.ProfileId);
		if (stashItemClass != null && stashGridClass != null)
		{
			foreach (KeyValuePair<Item, LocationInGrid> item2 in stashItemClass.Grids[0].ItemCollection.ToList())
			{
				item2.Deconstruct(out var key, out var _);
				Item item = key;
				GClass3393 to = stashGridClass.FindLocationForItem(item);
				InteractionsHandlerClass.Move(item, to, TraderControllerClass);
			}
			return;
		}
		Debug.LogError($"tempGrid is null: {stashItemClass == null} transferGrid is null: {stashGridClass == null}");
	}

	public bool method_1(string profileId, out StashItemClass playerContainer)
	{
		playerContainer = List_0.FirstOrDefault((StashItemClass grid) => grid.Id == profileId);
		if (playerContainer == null)
		{
			Debug.LogError("playerGrid is null for profile: " + profileId);
			return false;
		}
		return true;
	}

	public int method_2(StashItemClass stashContainer, ETraderServiceType traderServiceType, float deliveryPrice, float modDeliveryCost)
	{
		StashGridClass grid = stashContainer.Grid;
		int num = 0;
		GClass3120 itemCollection = grid.ItemCollection;
		if (GClass856.IsNullOrEmpty(itemCollection))
		{
			return num;
		}
		HandbookClass instance = Singleton<HandbookClass>.Instance;
		List_1.Clear();
		IEnumerable<Item> allItemsFromGridItemCollectionNonAlloc = GClass3380.GetAllItemsFromGridItemCollectionNonAlloc(grid.ItemCollection, List_1);
		int num2 = 0;
		foreach (KeyValuePair<Item, LocationInGrid> item in itemCollection)
		{
			item.Deconstruct(out var key, out var _);
			XYCellSizeStruct xYCellSizeStruct = key.CalculateCellSize();
			num2 += xYCellSizeStruct.X * xYCellSizeStruct.Y;
		}
		foreach (Item item2 in allItemsFromGridItemCollectionNonAlloc)
		{
			double basePrice = instance.GetBasePrice(item2.TemplateId);
			if (basePrice != 0.0)
			{
				double num3 = basePrice * (double)item2.StackObjectsCount;
				num += (int)num3;
			}
		}
		int basePrice2 = (int)(deliveryPrice * (float)num2 + (float)num * modDeliveryCost);
		return CalculateServicePrice(stashContainer.Id, traderServiceType, basePrice2);
	}

	public abstract Vector3 GetGridSize(Profile profile);

	public abstract int GetGridItemsPrice(StashItemClass stashContainer);

	public abstract double GetDeliveryPriceForProfile(string profileId);

	public abstract int CalculateServicePrice(string profileId, ETraderServiceType serviceType, int basePrice);
}
