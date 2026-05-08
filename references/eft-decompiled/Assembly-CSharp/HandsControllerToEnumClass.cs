using EFT;
using UnityEngine;

public abstract class HandsControllerToEnumClass
{
	public static EHandsControllerType FromController(IHandsController handsController)
	{
		if (handsController == null)
		{
			return EHandsControllerType.Empty;
		}
		if (!(handsController is GInterface198))
		{
			if (!(handsController is IFirearmHandsController))
			{
				if (!(handsController is IHandsThrowController))
				{
					if (!(handsController is IKnifeController))
					{
						if (!(handsController is GInterface203))
						{
							if (!(handsController is GInterface206))
							{
								if (!(handsController is GInterface207))
								{
									if (!(handsController is GInterface202))
									{
										if (!(handsController is IOnHandsUseCallback))
										{
											Debug.LogError("Unknown controller type '{0}'");
											return EHandsControllerType.None;
										}
										return EHandsControllerType.QuickUseItem;
									}
									return EHandsControllerType.UsableItem;
								}
								return EHandsControllerType.QuickKnife;
							}
							return EHandsControllerType.QuickGrenade;
						}
						return EHandsControllerType.Meds;
					}
					return EHandsControllerType.Knife;
				}
				return EHandsControllerType.Grenade;
			}
			return EHandsControllerType.Firearm;
		}
		return EHandsControllerType.Empty;
	}
}
