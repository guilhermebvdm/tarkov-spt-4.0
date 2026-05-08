public interface IActorEvents
{
	void OnAddAmmoInChamber();

	void OnAddAmmoInMag();

	void OnArm();

	void OnCook();

	void OnDelAmmoChamber();

	void OnDelAmmoFromMag();

	void OnDisarm();

	void OnFireEnd();

	void OnFiringBullet();

	void OnFoldOff();

	void OnFoldOn();

	void OnIdleStart();

	void OnLauncherAppeared();

	void OnLauncherDisappeared();

	void OnMagHide();

	void OnMagIn();

	void OnMagOut();

	void OnMagShow();

	void OnMessageName();

	void OnMalfunctionOff();

	void OnModChanged();

	void OnOffBoltCatch();

	void OnOnBoltCatch();

	void OnPutMagToRig();

	void OnRemoveShell();

	void OnReplaceSecondMag();

	void OnShellEject();

	void OnShowAmmo(bool BoolParam);

	void OnShowMag();

	void OnSliderOut();

	void OnSound(string StringParam);

	void OnSoundAtPoint(string StringParam);

	void OnStartUtilityOperation();

	void OnThirdAction(int IntParam);

	void OnUseProp(bool BoolParam);

	void OnUseSecondMagForReload();

	void OnWeapIn();

	void OnWeapOut();

	void OnBackpackDrop();

	void OnComboPlanning();

	void OnCurrentAnimStateEnded();

	void OnSetActiveObject(int objectID);

	void OnDeactivateObject(int objectID);

	void ReloadTest();

	void BipodOpen();

	void BipodClose();

	void OutUse();

	void AimReady();

	void IdleReady();

	void DropWeapon();
}
