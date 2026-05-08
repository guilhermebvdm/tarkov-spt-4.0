using EFT;
using EFT.Animations;
using UnityEngine;

public class GClass910 : GInterface38
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
			if (pwa.ActiveBlends.Count > 0 || (pwa.Mask & EProceduralAnimationMask.DrawDown) == 0)
			{
				pwa.TurnAway.OverlapDepth = 0f;
			}
			pwa.TurnAway.Process(deltaTime);
			if ((pwa.Mask & EProceduralAnimationMask.Aiming) != 0)
			{
				pwa.CustomEffector.Process(1f);
			}
			pwa.HandsContainer.HandsPosition.FixedUpdate(deltaTime);
			pwa.HandsContainer.HandsRotation.FixedUpdate(deltaTime);
			pwa.Shootingg.CurrentRecoilEffect.FixedUpdate(deltaTime);
		}
	}

	public void ApplyTransformations(ProceduralWeaponAnimation pwa, float dt)
	{
		pwa.ZeroAdjustments();
		pwa.UpdateAimWeight(dt);
		pwa.BlendAnimatorPose(dt);
		pwa.ApplyPosition();
		pwa.ApplySimpleRotation(dt);
		pwa.ApplyTacticalReloadTransformations();
		pwa.AvoidObstacles();
	}

	public void LateTransformations(ProceduralWeaponAnimation pwa, float dt)
	{
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
	}

	public float UpdatePossibleTilt(ProceduralWeaponAnimation proceduralWeaponAnimation, float smoothedCharacterMovementSpeed, float smoothedPoseLevel)
	{
		float value = proceduralWeaponAnimation.TiltBlender.Value;
		if (!(value >= 1f))
		{
			return Mathf.Max(value, ProceduralWeaponAnimation.GClass2791.GetValue(smoothedCharacterMovementSpeed, smoothedPoseLevel));
		}
		return value;
	}
}
