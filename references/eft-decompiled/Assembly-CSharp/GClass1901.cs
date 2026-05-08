using System;
using EFT;
using UnityEngine;

public class GClass1901 : TransferItemsControllerAbstractClass
{
	[NonSerialized]
	public BackendConfigSettingsClass.TransitSettingsClass TransitSettingsClass;

	public GClass1901(GameWorld gameWorld, BackendConfigSettingsClass.TransitSettingsClass settings, bool isOfflineGame)
		: base(gameWorld, isOfflineGame, ETraderServiceType.TransitItemsDelivery)
	{
		TransitSettingsClass = settings;
	}

	public override Vector3 GetGridSize(Profile profile)
	{
		return profile.FenceInfo.FenceLoyalty.TransitGridSize;
	}

	public override int GetGridItemsPrice(StashItemClass stashContainer)
	{
		return method_2(stashContainer, ETraderServiceType.TransitItemsDelivery, TransitSettingsClass.DeliveryPrice, TransitSettingsClass.ModDeliveryCost);
	}

	public override double GetDeliveryPriceForProfile(string profileId)
	{
		if (method_1(profileId, out var playerContainer))
		{
			return method_2(playerContainer, ETraderServiceType.TransitItemsDelivery, TransitSettingsClass.DeliveryPrice, TransitSettingsClass.ModDeliveryCost);
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
			EPlayerSide.Usec => TransitSettingsClass.UsecPriceMod, 
			EPlayerSide.Bear => TransitSettingsClass.BearPriceMod, 
			EPlayerSide.Savage => TransitSettingsClass.ScavPriceMod, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		int level = alivePlayerByProfileID.Profile.Skills.Charisma.Level;
		float coefficientDiscountCharisma = TransitSettingsClass.CoefficientDiscountCharisma;
		if (serviceType == ETraderServiceType.TransitItemsDelivery)
		{
			float priceModDelivery = alivePlayerByProfileID.Profile.FenceInfo.FenceLoyalty.PriceModDelivery;
			int b = (int)((float)basePrice * num2 * (1f - (float)level * coefficientDiscountCharisma) - priceModDelivery);
			num = Mathf.Max(TransitSettingsClass.DeliveryMinPrice, b);
		}
		return Mathf.CeilToInt((float)num * (flag ? markOfUnknown.TradersDiscount : 1f));
	}
}
