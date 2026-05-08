using Comfort.Common;
using EFT.InventoryLogic;

public interface IHandsThrowController : GInterface199, IHandsController, GInterface197
{
	new ThrowWeapItemClass Item { get; }

	Weapon.EFireMode CurrentFireMode { get; }

	bool WaitingForHighThrow { get; }

	bool WaitingForLowThrow { get; }

	void ExamineWeapon();

	void PullRingForHighThrow();

	void HighThrow();

	void PullRingForLowThrow();

	void LowThrow();

	void SetOnUsedCallback(Callback<IHandsThrowController> callback);

	bool CanThrow();

	bool CanChangeFireMode(Weapon.EFireMode fireMode);

	void ChangeFireMode(Weapon.EFireMode fireMode);

	void HandleFireInput();

	void HandleAltFireInput();
}
