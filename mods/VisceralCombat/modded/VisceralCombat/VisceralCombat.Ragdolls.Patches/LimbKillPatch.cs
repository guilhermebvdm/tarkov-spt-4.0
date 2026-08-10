using System;
using System.Linq;
using System.Reflection;
using EFT;
using EFT.Ballistics;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using UnityEngine;
using Object = UnityEngine.Object;
using VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;
using VisceralCombat.Ragdolls.Classes;

namespace VisceralCombat.Ragdolls.Patches;

public class LimbKillPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return typeof(BallisticsCalculator).GetMethods(BindingFlags.Instance | BindingFlags.Public).First((MethodInfo m) => m.Name == "Shoot" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(EftBulletClass));
	}

	[PatchPostfix]
	private static void Postfix(EftBulletClass shot)
	{
		if (shot == null) return;
		((MonoBehaviour)StaticManager.Instance).StartCoroutine(WatchShot(shot));
	}

	private static System.Collections.IEnumerator WatchShot(EftBulletClass shot)
	{
		if (shot == null) yield break;

		float timeout = 3.0f;
		while (!shot.IsShotFinished && timeout > 0f)
		{
			timeout -= Time.deltaTime;
			yield return null;
		}

		if (shot != null && shot.IsShotFinished && shot.HitCollider != null)
		{
			ProcessLimbKill(shot);
		}
	}

	public static void ProcessLimbKill(EftBulletClass shot)
	{
		if (shot == null || shot.HitCollider == null || shot.Ammo == null) return;

		Collider hitCollider = shot.HitCollider;
		Rigidbody rb = hitCollider.attachedRigidbody;
		if (rb == null) return;

		// --- 1. Resolve the Player via BodyPartCollider (precise, no string matching) ---
		BodyPartCollider bpc = hitCollider.GetComponent<BodyPartCollider>();
		if (bpc == null) bpc = hitCollider.GetComponentInParent<BodyPartCollider>();

		Player player = null;
		if (bpc?.Player is Player bpcPlayer)
		{
			player = bpcPlayer;
		}

		// Fallback: walk the hierarchy
		if (player == null)
		{
			GameObject rootGO = VisceralCombat.Dismemberment.Classes.Utils.GetRootGameObject(rb.gameObject);
			if (rootGO != null) player = rootGO.GetComponentInChildren<Player>(true);
		}
		if (player == null) player = rb.gameObject.GetComponentInParent<Player>();
		if (player == null) return;

		// Only process dead players
		bool isDead = (player.HealthController == null || !player.HealthController.IsAlive);
		if (!isDead) return;

		// --- 2. PuppetMaster: agony interruption (only relevant while pm is active) ---
		GameObject rootForPm = VisceralCombat.Dismemberment.Classes.Utils.GetRootGameObject(rb.gameObject);
		PuppetMaster pm = rootForPm?.GetComponentInChildren<PuppetMaster>(true);

		if (pm != null && pm.muscles != null)
		{
			string rbName = rb.gameObject.name;
			foreach (Muscle muscle in pm.muscles)
			{
				if (muscle != null && muscle.name != null && muscle.name.Contains(rbName))
				{
					if (rbName.Contains("Head") && pm.mappingWeight > 0.05f)
					{
						pm.stateSettings.killDuration = 0f;
						pm.state = PuppetMaster.State.Dead;
					}
					muscle.props.muscleWeight *= 0.5f;
				}
			}

			// If agony animation is active, collapse the bot from its current floor pose
			if (pm.mappingWeight > 0.05f)
			{
				RagdollHelperClass.InterruptAgony(player, pm);
				if (rb != null && !rb.isKinematic)
				{
					rb.AddForceAtPosition(shot.Direction * (shot.Speed * 0.15f), shot.HitPoint, ForceMode.Impulse);
				}
			}
		}

		// --- 3. Post-mortem dismemberment using EBodyPartColliderType (exact, no string guessing) ---
		if (VisceralEntry.Instance == null || !VisceralEntry.Instance.EnableDismemberment.Value) return;
		if (bpc == null) return; // Can only dismember if we have a typed collider

		EBodyPart? dismemberPart = null;
		string boneName = null;
		string capAsset = null;
		string[] extraAssets = Array.Empty<string>();

		switch (bpc.BodyPartColliderType)
		{
			case EBodyPartColliderType.LeftUpperArm:
			case EBodyPartColliderType.LeftForearm:
				dismemberPart = (EBodyPart)3;
				boneName = "lforearm1";
				capAsset = "Arm_LeftCap";
				extraAssets = new[] { "Arm_L_1", "Arm_L_2" };
				break;
			case EBodyPartColliderType.RightUpperArm:
			case EBodyPartColliderType.RightForearm:
				dismemberPart = (EBodyPart)4;
				boneName = "rforearm1";
				capAsset = "Arm_RightCap";
				extraAssets = new[] { "Arm_R_1", "Arm_R_2" };
				break;
			case EBodyPartColliderType.LeftThigh:
			case EBodyPartColliderType.LeftCalf:
				dismemberPart = (EBodyPart)5;
				boneName = "lthigh1";
				capAsset = "Leg_LeftCap";
				extraAssets = new[] { "gore_leg_torn01" };
				break;
			case EBodyPartColliderType.RightThigh:
			case EBodyPartColliderType.RightCalf:
				dismemberPart = (EBodyPart)6;
				boneName = "rthigh1";
				capAsset = "Leg_RightCap";
				extraAssets = new[] { "gore_leg_torn02" };
				break;
			// Head and torso excluded: scaling Head bone would collapse body mesh
		}

		if (!dismemberPart.HasValue || boneName == null) return;

		float chance = 0.5f;
		if (shot.Ammo is AmmoItemClass ammo && !string.IsNullOrEmpty(ammo.Caliber))
		{
			if (!VisceralCombat.Combined.Patches.KillPatch.calibers.TryGetValue(ammo.Caliber, out chance))
			{
				string clean = ammo.Caliber.StartsWith("Caliber") ? ammo.Caliber.Substring(7) : ammo.Caliber;
				VisceralCombat.Combined.Patches.KillPatch.calibers.TryGetValue(clean, out chance);
			}
		}

		if (UnityEngine.Random.value <= chance)
		{
			Transform[] dummyLimbs;
			VisceralCombat.Combined.Patches.KillPatch.DismemberLimb(player, shot.Direction, dismemberPart.Value, boneName, capAsset, extraAssets, out dummyLimbs);
		}
	}
}
