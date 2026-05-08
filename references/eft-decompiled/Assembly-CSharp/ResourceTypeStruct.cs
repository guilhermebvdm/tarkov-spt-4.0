using EFT;
using EFT.InventoryLogic;
using JetBrains.Annotations;

public struct ResourceTypeStruct(ResourceType resourceType, ItemTemplate itemTemplate)
{
	public ResourceType ResourceType = resourceType;

	[CanBeNull]
	public ItemTemplate ItemTemplate = itemTemplate;

	public static ResourceTypeStruct Other => new ResourceTypeStruct
	{
		ResourceType = ResourceType.Other
	};

	public static ResourceTypeStruct Player => new ResourceTypeStruct
	{
		ResourceType = ResourceType.Player
	};

	public static ResourceTypeStruct SynchronizableObject => new ResourceTypeStruct
	{
		ResourceType = ResourceType.SynchronizableObject
	};
}
