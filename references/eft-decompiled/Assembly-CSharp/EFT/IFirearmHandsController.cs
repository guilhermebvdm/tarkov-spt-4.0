using System;
using Comfort.Common;
using EFT.InventoryLogic;
using JetBrains.Annotations;

namespace EFT;

public interface IFirearmHandsController : GInterface199, IHandsController, GInterface197
{
	bool IsOverlap { get; }

	new Weapon Item { get; }

	global::BindableStateClass<bool> CompassState { get; }

	void SetTriggerPressed(bool pressed);

	bool ChangeFireMode(Weapon.EFireMode fireMode);

	bool CheckFireMode();

	void ChangeAimingMode();

	void ToggleAim();

	void SetAim(bool value);

	void ReloadMag(MagazineItemClass foundItem, [CanBeNull] ItemAddress itemAddress, [CanBeNull] Callback callback);

	bool ExamineWeapon();

	void RollCylinder(bool rollToZeroCamora);

	void QuickReloadMag(MagazineItemClass foundItem, [CanBeNull] Callback callback);

	void ReloadWithAmmo(AmmoPackReloadingClass foundItem, [CanBeNull] Callback callback);

	void ReloadCylinderMagazine(AmmoPackReloadingClass ammoPack, [CanBeNull] Callback callback, bool quickReload);

	void ReloadBarrels(AmmoPackReloadingClass ammoPack, ItemAddress placeToPutContainedAmmoMagazine, [CanBeNull] Callback callback);

	void ReloadGrenadeLauncher(AmmoPackReloadingClass foundItem, [CanBeNull] Callback callback);

	bool CheckAmmo();

	bool CheckChamber();

	bool SetLightsState(FirearmLightStateStruct[] lightsStates, bool force = false, bool animated = true);

	bool ToggleLauncher(Action callback = null);

	bool CanStartReload();

	bool ShouldForceQuickReload();

	bool CanPressTrigger();

	bool IsInLauncherMode();

	void SetScopeMode(FirearmScopeStateStruct[] scopeStates);

	void ReloadMagNotFound();

	void OpticCalibrationSwitchUp(FirearmScopeStateStruct[] scopeStates);

	void OpticCalibrationSwitchDown(FirearmScopeStateStruct[] scopeStates);

	void UnderbarrelSightingRangeUp();

	void UnderbarrelSightingRangeDown();

	bool HasScopeAimBone(SightComponent sightComp);

	void ChangeLeftStance();

	bool ToggleBipod();
}
