using System;
using EFT;
using UnityEngine;

public class BTRTransferItemsControllerClass : TransferItemsControllerAbstractClass
{
	[NonSerialized]
	public BackendConfigSettingsClass.BTRGlobalSettings BtrglobalSettings_0;

	public BTRTransferItemsControllerClass(GameWorld gameWorld, BackendConfigSettingsClass.BTRGlobalSettings settings, bool isOfflineGame)
		: base(gameWorld, isOfflineGame, ETraderServiceType.BtrItemsDelivery)
	{
		BtrglobalSettings_0 = settings;
	}

	public override Vector3 GetGridSize(Profile profile)
	{
		return profile.FenceInfo.FenceLoyalty.DeliveryGridSize;
	}

	public override int GetGridItemsPrice(StashItemClass stashContainer)
	{
		return method_2(stashContainer, ETraderServiceType.BtrItemsDelivery, BtrglobalSettings_0.DeliveryPrice, BtrglobalSettings_0.ModDeliveryCost);
	}

	public override double GetDeliveryPriceForProfile(string profileId)
	{
		if (method_1(profileId, out var playerContainer))
		{
			return method_2(playerContainer, ETraderServiceType.BtrItemsDelivery, BtrglobalSettings_0.DeliveryPrice, BtrglobalSettings_0.ModDeliveryCost);
		}
		return 0.0;
	}

	public override int CalculateServicePrice(string profileId, ETraderServiceType serviceType, int basePrice)
	{
		Player alivePlayerByProfileID = GameWorld_0.GetAlivePlayerByProfileID(profileId);
		MarkOfUnknownItemClass markOfUnknown;
		bool flag = alivePlayerByProfileID.HasMarkOfUnknown(out markOfUnknown);
		int num = 0;
		if (alivePlayerByProfileID == null)
		{
			Debug.LogError("Trying to calculate service price for null player with id " + profileId);
			return num;
		}
		float num2 = alivePlayerByProfileID.Side switch
		{
			EPlayerSide.Usec => BtrglobalSettings_0.UsecPriceMod, 
			EPlayerSide.Bear => BtrglobalSettings_0.BearPriceMod, 
			EPlayerSide.Savage => BtrglobalSettings_0.ScavPriceMod, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		int level = alivePlayerByProfileID.Profile.Skills.Charisma.Level;
		float coefficientDiscountCharisma = BtrglobalSettings_0.CoefficientDiscountCharisma;
		switch (serviceType)
		{
		case ETraderServiceType.BtrItemsDelivery:
		{
			float priceModDelivery = alivePlayerByProfileID.Profile.FenceInfo.FenceLoyalty.PriceModDelivery;
			int b3 = (int)((float)basePrice * num2 * (1f - (float)level * coefficientDiscountCharisma) - priceModDelivery);
			num = Mathf.Max(BtrglobalSettings_0.DeliveryMinPrice, b3);
			break;
		}
		case ETraderServiceType.PlayerTaxi:
		{
			float priceModTaxi = alivePlayerByProfileID.Profile.FenceInfo.FenceLoyalty.PriceModTaxi;
			int b2 = (int)((float)basePrice * num2 * (1f - (float)level * coefficientDiscountCharisma) - priceModTaxi);
			num = Mathf.Max(BtrglobalSettings_0.TaxiMinPrice, b2);
			break;
		}
		case ETraderServiceType.BtrBotCover:
		{
			float priceModCleanUp = alivePlayerByProfileID.Profile.FenceInfo.FenceLoyalty.PriceModCleanUp;
			int b = (int)((float)basePrice * num2 * (1f - (float)level * coefficientDiscountCharisma) - priceModCleanUp);
			num = Mathf.Max(BtrglobalSettings_0.BotCoverMinPrice, b);
			break;
		}
		}
		return Mathf.CeilToInt((float)num * (flag ? markOfUnknown.TradersDiscount : 1f));
	}
}
