using System;
using EFT.InventoryLogic;
using UnityEngine;

public interface IBotAiming
{
	Vector3 EndTargetPoint { get; }

	bool IsReady { get; }

	bool AlwaysTurnOnLight { get; }

	Vector3 RealTargetPoint { get; }

	float LastDist2Target { get; }

	bool HardAim { get; }

	event Action<Vector3> OnSettingsTarget;

	void LoseTarget();

	void SetTarget(Vector3 trg);

	void SetNextAimingDelay(float nextAimingDelay);

	void TriggerPressedDone();

	void ShootDone(Weapon weapon);

	void NodeUpdate();

	void Activate();

	void GetHit(DamageInfoStruct damageInfo);

	void DrawGizmosSelected();

	void ManualUpdate();

	void RotateX(float angToRotate);

	void RotateY(float deltaAngle);

	void SetWeapon(Weapon weapon);

	void SetTracers(bool isTracers);

	void Move(float delta = 0f);

	void NextShotMiss(int missCount);

	void OnDrawGizmos();

	void DebugDraw();

	void Dispose();
}
