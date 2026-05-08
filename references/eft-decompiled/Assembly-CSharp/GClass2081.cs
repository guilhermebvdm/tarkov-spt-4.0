using System;
using EFT;
using EFT.InventoryLogic;

public abstract class GClass2081
{
	public static EShotType ToShotType(this Weapon.EMalfunctionState malfunctionState)
	{
		return malfunctionState switch
		{
			Weapon.EMalfunctionState.None => EShotType.RegularShot, 
			Weapon.EMalfunctionState.Misfire => EShotType.Misfire, 
			Weapon.EMalfunctionState.Feed => EShotType.Feed, 
			Weapon.EMalfunctionState.Jam => EShotType.JamedShot, 
			Weapon.EMalfunctionState.SoftSlide => EShotType.SoftSlidedShot, 
			Weapon.EMalfunctionState.HardSlide => EShotType.HardSlidedShot, 
			_ => throw new NotImplementedException("ToShotType failed"), 
		};
	}

	public static Weapon.EMalfunctionState ToWeaponMalfunctionState(this EShotType eShotType)
	{
		switch (eShotType)
		{
		case EShotType.Misfire:
			return Weapon.EMalfunctionState.Misfire;
		case EShotType.Feed:
			return Weapon.EMalfunctionState.Feed;
		case EShotType.JamedShot:
			return Weapon.EMalfunctionState.Jam;
		case EShotType.SoftSlidedShot:
			return Weapon.EMalfunctionState.SoftSlide;
		case EShotType.HardSlidedShot:
			return Weapon.EMalfunctionState.HardSlide;
		default:
			throw new NotImplementedException("ToWeaponMalfunctionState failed");
		case EShotType.RegularShot:
		case EShotType.DryFire:
			return Weapon.EMalfunctionState.None;
		}
	}
}
