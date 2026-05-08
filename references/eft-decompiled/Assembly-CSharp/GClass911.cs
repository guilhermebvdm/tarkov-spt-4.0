using EFT;
using EFT.Animations;
using UnityEngine;

public class GClass911 : GInterface38
{
	public void ApplyCameraTransformations(ProceduralWeaponAnimation pwa, float dt)
	{
	}

	public void ProcessEffectors(ProceduralWeaponAnimation pwa, float deltaTime, int nFixedFrames = 1)
	{
		if (nFixedFrames < 0 || pwa.HandsContainer.WeaponRootAnim == null || !pwa.enabled || Mathf.Approximately(deltaTime, 0f))
		{
			return;
		}
		deltaTime /= (float)nFixedFrames;
		for (int i = 0; i < nFixedFrames; i++)
		{
			if ((pwa.Mask & EProceduralAnimationMask.MotionReaction) != 0)
			{
				pwa.MotionReact.FixedTracking(deltaTime);
				pwa.MotionReact.Process(deltaTime);
			}
			if ((pwa.Mask & EProceduralAnimationMask.ForceReaction) != 0)
			{
				pwa.ForceReact.Process(deltaTime);
			}
			pwa.TurnAway.OverlapDepth = 0f;
			pwa.TurnAway.Process(deltaTime);
			pwa.HandsContainer.HandsPosition.FixedUpdate(deltaTime);
			pwa.HandsContainer.HandsRotation.FixedUpdate(deltaTime);
			pwa.HandsContainer.SwaySpring.Process(deltaTime);
			pwa.Shootingg.CurrentRecoilEffect.FixedUpdate(deltaTime);
			pwa.HandsContainer.CameraRotation.FixedUpdate(deltaTime);
			pwa.HandsContainer.CameraPosition.FixedUpdate(deltaTime);
		}
	}

	public void ApplyTransformations(ProceduralWeaponAnimation pwa, float dt)
	{
		pwa.ApplyStationaryWeaponPosition();
		pwa.ApplyComplexRotation(dt);
	}

	public void LateTransformations(ProceduralWeaponAnimation pwa, float dt)
	{
		if (pwa.PointOfView == EPointOfView.FirstPerson)
		{
			pwa.StationaryCamera(dt);
			pwa.LerpCamera(dt);
		}
	}

	public void ApplyFovAdjustments(ProceduralWeaponAnimation proceduralWeaponAnimation, Player player)
	{
		player.RibcageScaleCurrent = 1f;
	}

	public void ResetFovAdjustments(ProceduralWeaponAnimation proceduralWeaponAnimation, Player player)
	{
		if (!Mathf.Approximately(player.PlayerBones.Ribcage.Original.localScale.z, 1f))
		{
			player.PlayerBones.Ribcage.Original.localScale = Vector3.one;
			player.HandsController.HandsHierarchy.Self.localScale = Vector3.one;
		}
	}

	public void OpticCalibration(ProceduralWeaponAnimation proceduralWeaponAnimation, bool calibrate)
	{
		proceduralWeaponAnimation.AgsCalibrate(calibrate);
	}

	public float UpdatePossibleTilt(ProceduralWeaponAnimation proceduralWeaponAnimation, float smoothedCharacterMovementSpeed, float smoothedPoseLevel)
	{
		return 0f;
	}
}
