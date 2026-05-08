using Comfort.Common;
using EFT.InventoryLogic;

public interface IHandsController
{
	Item Item { get; }

	bool Destroyed { get; }

	bool IsAiming { get; }

	float AimingSensitivity { get; }

	TransformLinks HandsHierarchy { get; }

	FirearmsAnimator FirearmsAnimator { get; }

	bool CanExecute(GInterface438 operation);

	void Execute(GInterface438 operation, Callback callback);

	void Pickup(bool p);

	bool SupportPickup();

	void Interact(bool isInteracting, int actionIndex);

	void Loot(bool b);

	bool CanRemove();

	bool IsHandsProcessing();

	bool IsPlacingBeacon();

	bool CanInteract();

	bool IsInInteraction();

	bool IsInInteractionStrictCheck();

	float GetAnimatorFloatParam(int hash);

	void ManualLateUpdate(float deltaTime);

	void SetInventoryOpened(bool opened);

	void OnPlayerDead();

	void OnGameSessionEnd();

	void Destroy();

	void ShowGesture(EInteraction gesture);

	void BlindFire(int b);

	bool InCanNotBeInterruptedOperation();
}
