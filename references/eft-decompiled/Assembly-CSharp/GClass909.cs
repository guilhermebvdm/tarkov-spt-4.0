using System;
using System.Collections.Generic;
using EFT;
using EFT.Animations;
using UnityEngine;

public class GClass909 : GInterface38
{
	public struct Struct106
	{
		public Transform StoredTransform;

		public Vector3 Position;

		public Quaternion Rotation;
	}

	[NonSerialized]
	public List<Struct106> List_0 = new List<Struct106>();

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
			if ((pwa.Mask & EProceduralAnimationMask.Breathing) != 0)
			{
				pwa.Breath.Process(deltaTime);
			}
			if ((pwa.Mask & EProceduralAnimationMask.Walking) != 0)
			{
				pwa.Walk.Process(deltaTime);
			}
			if ((pwa.Mask & EProceduralAnimationMask.HandShake) != 0)
			{
				pwa.HandShakeEffector.Process(deltaTime);
			}
			if ((pwa.Mask & EProceduralAnimationMask.DrawDown) == 0 || pwa.ActiveBlends.Count > 0)
			{
				pwa.TurnAway.OverlapDepth = 0f;
			}
			pwa.TurnAway.LeftStance = pwa.LeftStance;
			pwa.TurnAway.InMountState = pwa.IsMountedState;
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
		pwa.ZeroAdjustments();
		pwa.UpdateAimWeight(dt);
		pwa.BlendAnimatorPose(dt);
		pwa.ApplyPosition();
		pwa.ApplyComplexRotation(dt);
		pwa.ApplyTacticalReloadTransformations();
		pwa.AvoidObstacles();
	}

	public void ApplyCameraTransformations(ProceduralWeaponAnimation pwa, float dt)
	{
		pwa.UpdateCurrentScopeBoneTarget();
		pwa.CalculateCameraPosition();
		pwa.LerpCamera(dt);
	}

	public void LateTransformations(ProceduralWeaponAnimation pwa, float dt)
	{
	}

	public void ApplyFovAdjustments(ProceduralWeaponAnimation pwa, Player player)
	{
		pwa.PreCalibrate();
		player.RibcageScaleCurrent = ((Mathf.Abs(player.RibcageScaleCurrent - player.RibcageScaleCurrentTarget) > 0.0025f) ? Mathf.Lerp(player.RibcageScaleCurrent, player.RibcageScaleCurrentTarget, Time.deltaTime * 2f) : player.RibcageScaleCurrentTarget);
		if (Math.Abs(player.RibcageScaleCurrent - player.PlayerBones.Ribcage.Original.localScale.z) > Mathf.Epsilon)
		{
			Transform[] fovSpecialTransforms = player.PlayerBones.FovSpecialTransforms;
			foreach (Transform transform in fovSpecialTransforms)
			{
				List_0.Add(new Struct106
				{
					StoredTransform = transform,
					Position = transform.position,
					Rotation = transform.rotation
				});
			}
			player.PlayerBones.Ribcage.Original.rotation = player.HandsRotation;
			foreach (Struct106 item in List_0)
			{
				item.StoredTransform.position = item.Position;
				item.StoredTransform.rotation = item.Rotation;
			}
			List_0.Clear();
			player.PlayerBones.Ribcage.Original.localScale = new Vector3(1f, 1f, player.RibcageScaleCurrent);
			player.HandsController.HandsHierarchy.Self.localScale = new Vector3(1f, 1f, player.RibcageScaleCurrent);
			player.PlayerBones.Weapon_Root_Third.localScale = new Vector3(1f, 1f, player.RibcageScaleCurrent);
		}
		pwa.Calibrate();
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
		proceduralWeaponAnimation.OpticCalibrationSwitch(calibrate);
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
