using System;
using EFT;

public interface GInterface28
{
	bool Exhausted { get; }

	bool CanSprint { get; }

	bool Sprinting { get; }

	bool HoldingBreath { get; }

	float CarryingWeightModifier { get; }

	float SprintAcceleration { get; }

	float SprintSpeed { get; }

	float SprintSensitivity { get; }

	bool CanJump { get; }

	bool CanMeleeHit { get; }

	BackendConfigSettingsClass.GClass1736 StaminaParameters { get; }

	bool BerserkRestorationFactor { set; }

	event Action<bool> OnSprintStateChangedEvent;

	event Action WeightRelatedValuesUpdated;

	void Init(Player player);

	void StandUp();

	void UpdatePose(int i);

	void UpdateWeightLimits();

	void OnWeightUpdated();

	void Sprint(bool target);

	void Aim(float mass = 0f);

	void HoldBreath(bool enable);

	void Update(float dt);

	void LateUpdate();

	void OnBreach();

	void InvokeInsufficient();

	void OnThrow(bool lowThrow);

	void ConsumeAsMelee(float expense);

	void OnJump(bool enabled);

	void BulletHit(float staminaBurnRate);
}
