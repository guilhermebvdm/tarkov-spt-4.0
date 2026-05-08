using EFT;
using EFT.Animations;

public interface GInterface38
{
	void ApplyCameraTransformations(ProceduralWeaponAnimation pwa, float dt);

	void ProcessEffectors(ProceduralWeaponAnimation pwa, float deltaTime, int nFixedFrames = 1);

	void ApplyTransformations(ProceduralWeaponAnimation pwa, float dt);

	void LateTransformations(ProceduralWeaponAnimation pwa, float dt);

	void ApplyFovAdjustments(ProceduralWeaponAnimation proceduralWeaponAnimation, Player player);

	void ResetFovAdjustments(ProceduralWeaponAnimation proceduralWeaponAnimation, Player player);

	void OpticCalibration(ProceduralWeaponAnimation proceduralWeaponAnimation, bool calibrate);

	float UpdatePossibleTilt(ProceduralWeaponAnimation proceduralWeaponAnimation, float smoothedCharacterMovementSpeed, float smoothedPoseLevel);
}
