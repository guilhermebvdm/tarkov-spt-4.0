using EFT.ZombieEventsConsumers;

namespace EFT;

public interface IPlayerAnimatorEvents
{
	IVaultingSoundsEvents VaultingSoundsEvents { get; }

	IVaultingSoundsEvents SprintVaultSoundsEvents { get; }

	IVaultingSoundsEvents ClimbSoundsEvents { get; }

	IDropBackPackEvents DropBackpackEvents { get; }

	IBipodToggleEvents BipodToggleEvents { get; }

	IZombieFireBulletEvents ZombieFireBulletEvents { get; }

	IZombieFireEndEvents ZombieFireEndEvents { get; }

	ILeftHandInteractionEvents LeftHandInteractionEvents { get; }
}
