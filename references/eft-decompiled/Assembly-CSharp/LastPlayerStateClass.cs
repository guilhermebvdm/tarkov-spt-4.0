using EFT.InventoryLogic;
using JetBrains.Annotations;

public class LastPlayerStateClass
{
	public readonly GClass1410 Info;

	[CanBeNull]
	public readonly GClass2197 Customization;

	[CanBeNull]
	public readonly InventoryEquipment Equipment;

	public LastPlayerStateClass(GClass1410 info, GClass2197 customization, InventoryEquipment equipment)
	{
		Info = info;
		Customization = customization;
		Equipment = equipment;
	}

	public LastPlayerStateClass(GClass2214 descriptor)
	{
		Info = descriptor.Info;
		Equipment = descriptor.Equipment?.ToEquipment();
		if (descriptor.Customization != null)
		{
			Customization = new GClass2197(descriptor.Customization);
		}
	}

	public bool IsEmpty()
	{
		if (Customization != null)
		{
			return Equipment == null;
		}
		return true;
	}
}
