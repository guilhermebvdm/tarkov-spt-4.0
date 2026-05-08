using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;

public class GClass937
{
	public struct GStruct62 : IEquatable<GStruct62>
	{
		public ECurrencyType CurrencyToAutoExchange;

		public int ExistingAmount;

		public int AmountToAutoExchange;

		public int PriceToAutoExchange;

		public TraderClass ExchangeTrader;

		public bool Equals(GStruct62 other)
		{
			if (CurrencyToAutoExchange == other.CurrencyToAutoExchange && AmountToAutoExchange == other.AmountToAutoExchange && PriceToAutoExchange == other.PriceToAutoExchange && object.Equals(ExchangeTrader, other.ExchangeTrader))
			{
				return ExistingAmount == other.ExistingAmount;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is GStruct62 other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine((int)CurrencyToAutoExchange, AmountToAutoExchange, PriceToAutoExchange, ExchangeTrader, ExistingAmount);
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class583
	{
		public static readonly Class583 class583_0 = new Class583();

		public static Func<KeyValuePair<Item, LocationInGrid>, IEnumerable<Item>> func_0;

		public static Func<Item, bool> func_1;

		public IEnumerable<Item> method_0(KeyValuePair<Item, LocationInGrid> x)
		{
			return GClass3380.GetAllItems(x.Key);
		}

		public bool method_1(Item item)
		{
			return GClass3750.IsExchangeable(item);
		}
	}

	[NonSerialized]
	public InventoryController InventoryController_0;

	[NonSerialized]
	public TraderClass TraderClass;

	[NonSerialized]
	public TraderClass TraderClass_1;

	[NonSerialized]
	public List<TraderClass> List_0;

	public GClass937(InventoryController stashController, IEnumerable<TraderClass> traders)
	{
		InventoryController_0 = stashController ?? throw new ArgumentNullException("stashController");
		object obj;
		if (traders == null)
		{
			obj = null;
		}
		else
		{
			obj = traders.ToList();
			if (obj != null)
			{
				List_0 = (List<TraderClass>)obj;
				foreach (TraderClass item in List_0)
				{
					if (item.Id.Equals("5935c25fb3acc3127c3d8cd9"))
					{
						TraderClass = item;
					}
					if (item.Id.Equals("58330581ace78e27b8b10cee"))
					{
						TraderClass_1 = item;
					}
				}
				method_0(TraderClass);
				method_0(TraderClass_1);
				return;
			}
		}
		throw new ArgumentNullException("traders");
	}

	public void method_0(TraderClass trader)
	{
		if (trader != null && trader.Info.Unlocked && !trader.AssortmentLoading && !trader.AssortmentUpdateTimeout)
		{
			trader.RefreshAssortment(createIfNotExists: true, ignoreTimeout: false).HandleExceptions();
		}
	}

	public async Task<bool> ApproveAutoExchange(GStruct62 exchangeData)
	{
		if (exchangeData.Equals(default(GStruct62)))
		{
			return false;
		}
		if (exchangeData.ExchangeTrader == null)
		{
			return false;
		}
		if (!exchangeData.ExchangeTrader.Info.Unlocked)
		{
			return false;
		}
		ItemUiContext instance = ItemUiContext.Instance;
		if (instance == null)
		{
			return false;
		}
		string text = GClass2348.FormatSeparate(exchangeData.PriceToAutoExchange);
		string currencyChar = GClass3130.GetCurrencyChar(ECurrencyType.RUB);
		string description = string.Format(GClass2348.Localized("Autoexchange/Description"), exchangeData.ExchangeTrader.LocalizedName, currencyChar + " " + text);
		string caption = GClass2348.Localized("Autoexchange/Caption");
		GClass3834 windowContext;
		return await instance.ShowMessageWindow(out windowContext, description, caption);
	}

	public bool method_1(ECurrencyType fromCurrency, ECurrencyType toCurrency, out BarterVariant barterVariant, out TraderAssortmentControllerClass assortment, out Item targetCurrencyItem, out TraderClass outTrader)
	{
		barterVariant = null;
		assortment = null;
		targetCurrencyItem = null;
		outTrader = null;
		if (!GClass3130.CurrencyIndex.TryGetValue(fromCurrency, out var value))
		{
			return false;
		}
		if (string.IsNullOrEmpty(value.Id))
		{
			return false;
		}
		MongoID id = value.Id;
		if (!GClass3130.CurrencyIndex.TryGetValue(toCurrency, out var value2))
		{
			return false;
		}
		if (string.IsNullOrEmpty(value2.Id))
		{
			return false;
		}
		MongoID id2 = value2.Id;
		foreach (TraderClass item in List_0)
		{
			if (item == null || !item.Info.Unlocked || item.CurrentAssortment == null)
			{
				continue;
			}
			assortment = item.CurrentAssortment;
			IEnumerable<Item> allItems = GClass3380.GetAllItems(assortment.TraderController.RootItem);
			List<Item> list = new List<Item>();
			foreach (Item item2 in allItems)
			{
				if (!(item2.TemplateId != id2))
				{
					list.Add(item2);
				}
			}
			foreach (Item item3 in list)
			{
				BarterScheme schemeForItem = assortment.GetSchemeForItem(item3);
				if (schemeForItem == null)
				{
					continue;
				}
				foreach (BarterVariant item4 in schemeForItem)
				{
					if (item4.Count == 1 && !((MongoID)item4[0]._tpl != id))
					{
						barterVariant = item4;
						targetCurrencyItem = item3;
						outTrader = item;
						return true;
					}
				}
			}
		}
		return false;
	}

	public async Task<bool> ExchangeCurrency(ECurrencyType targetCurrency, int amountToBuy)
	{
		if (amountToBuy <= 0)
		{
			return false;
		}
		if (!method_1(ECurrencyType.RUB, targetCurrency, out var barterVariant, out var assortment, out var targetCurrencyItem, out var _))
		{
			return false;
		}
		int num = (int)Math.Ceiling(barterVariant[0].count * (double)amountToBuy);
		if (method_3(ECurrencyType.RUB) < num)
		{
			return false;
		}
		Item selectedItem = assortment.SelectedItem;
		int currentQuantity = assortment.CurrentQuantity;
		assortment.SelectItem(targetCurrencyItem);
		if (assortment.CurrentRequisites.Count != 1)
		{
			return false;
		}
		assortment.CurrentQuantity = amountToBuy;
		assortment.SetValidity();
		List<Item> source = InventoryController_0.Inventory.Stash.Grid.ContainedItems.SelectMany((KeyValuePair<Item, LocationInGrid> x) => GClass3380.GetAllItems(x.Key)).ToList();
		List<GClass2571> currentRequisites = assortment.CurrentRequisites;
		if (currentRequisites != null)
		{
			GClass3750.AutoFill(currentRequisites, source.Where((Item item) => GClass3750.IsExchangeable(item)));
		}
		if (!assortment.IsValid)
		{
			return false;
		}
		IResult result = await assortment.Purchase();
		if (selectedItem == null)
		{
			return result.Succeed;
		}
		assortment.SelectItem(selectedItem);
		assortment.CurrentQuantity = currentQuantity;
		return result.Succeed;
	}

	public bool CanPurchaseWithAutoExchange(Dictionary<ECurrencyType, int> requirements, out GStruct62 autoExchangeData)
	{
		autoExchangeData = default(GStruct62);
		if (requirements == null)
		{
			return false;
		}
		if (requirements.TryGetValue(ECurrencyType.RUB, out var value) && value > 0 && method_3(ECurrencyType.RUB) < value)
		{
			return false;
		}
		int num = 0;
		ECurrencyType[] array = new ECurrencyType[2]
		{
			ECurrencyType.USD,
			ECurrencyType.EUR
		};
		foreach (ECurrencyType eCurrencyType in array)
		{
			if (requirements.TryGetValue(eCurrencyType, out var value2) && value2 > 0 && method_3(eCurrencyType) < value2)
			{
				num++;
			}
		}
		if (num > 1)
		{
			return false;
		}
		array = new ECurrencyType[2]
		{
			ECurrencyType.USD,
			ECurrencyType.EUR
		};
		int i = 0;
		ECurrencyType eCurrencyType2;
		int value3;
		int num2;
		while (true)
		{
			if (i < array.Length)
			{
				eCurrencyType2 = array[i];
				if (requirements.TryGetValue(eCurrencyType2, out value3) && value3 > 0)
				{
					num2 = method_3(eCurrencyType2);
					if (num2 < value3)
					{
						break;
					}
				}
				i++;
				continue;
			}
			return true;
		}
		int num3 = value3 - num2;
		if (!method_2(eCurrencyType2, num3, out var priceAmount, out var exchangeTrader))
		{
			return false;
		}
		autoExchangeData = new GStruct62
		{
			CurrencyToAutoExchange = eCurrencyType2,
			AmountToAutoExchange = num3,
			PriceToAutoExchange = priceAmount,
			ExchangeTrader = exchangeTrader,
			ExistingAmount = num2
		};
		return true;
	}

	public bool CanPurchaseAssortmentWithAutoExchange(TraderAssortmentControllerClass purchaseAssortment, out GStruct62 autoExchangeData)
	{
		autoExchangeData = default(GStruct62);
		if (purchaseAssortment == null)
		{
			return false;
		}
		if (purchaseAssortment.IsValid)
		{
			return true;
		}
		if (purchaseAssortment.CurrentRequisites.Count != 1)
		{
			return false;
		}
		GClass2571 gClass = purchaseAssortment.CurrentRequisites[0];
		if (!GClass3130.TryGetCurrencyType(gClass.RequiredItem.TemplateId, out var type))
		{
			return false;
		}
		if (type != ECurrencyType.USD && type != ECurrencyType.EUR)
		{
			return false;
		}
		int requiredItemsCount = gClass.RequiredItemsCount;
		int num = method_3(type);
		if (num >= requiredItemsCount)
		{
			return false;
		}
		int num2 = requiredItemsCount - num;
		if (!method_2(type, num2, out var priceAmount, out var exchangeTrader))
		{
			return false;
		}
		autoExchangeData = new GStruct62
		{
			CurrencyToAutoExchange = type,
			AmountToAutoExchange = num2,
			PriceToAutoExchange = priceAmount,
			ExchangeTrader = exchangeTrader,
			ExistingAmount = num
		};
		return true;
	}

	public bool method_2(ECurrencyType targetCurrency, int amountToExchange, out int priceAmount, out TraderClass exchangeTrader)
	{
		priceAmount = 0;
		exchangeTrader = null;
		if (amountToExchange <= 0)
		{
			return false;
		}
		if (!method_1(ECurrencyType.RUB, targetCurrency, out var barterVariant, out var _, out var targetCurrencyItem, out exchangeTrader))
		{
			return false;
		}
		int num = (int)Math.Ceiling(barterVariant[0].count * (double)amountToExchange);
		if (method_3(ECurrencyType.RUB) < num)
		{
			return false;
		}
		if (!targetCurrencyItem.UnlimitedCount && targetCurrencyItem.StackObjectsCount < amountToExchange)
		{
			return false;
		}
		if (targetCurrencyItem.BuyRestrictionMax > 0 && targetCurrencyItem.BuyRestrictionCurrent + amountToExchange > targetCurrencyItem.BuyRestrictionMax)
		{
			return false;
		}
		priceAmount = num;
		return true;
	}

	public int method_3(ECurrencyType currencyType)
	{
		if (InventoryController_0?.Inventory == null)
		{
			return 0;
		}
		return GClass3373.GetMoneySums(InventoryController_0.Inventory.Stash.Grid.ContainedItems.Keys).GetValueOrDefault(currencyType, 0);
	}
}
