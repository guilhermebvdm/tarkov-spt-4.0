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

		// --- 1. Resolve Player ---
		// On live bots: BodyPartCollider is present with a direct Player reference.
		// On dead ragdolls: colliders are physics bones ("Base HumanRThigh1" etc.) —
		// no BodyPartCollider attached. Use hierarchy fallback.
		BodyPartCollider bpc = hitCollider.GetComponent<BodyPartCollider>();
		if (bpc == null) bpc = hitCollider.GetComponentInParent<BodyPartCollider>();

		Player player = null;
		if (bpc?.Player is Player bpcPlayer)
		{
			player = bpcPlayer;
		}

		if (player == null)
		{
			GameObject rootGO = VisceralCombat.Dismemberment.Classes.Utils.GetRootGameObject(rb.gameObject);
			if (rootGO != null) player = rootGO.GetComponentInChildren<Player>(true);
		}

		if (player == null) player = rb.gameObject.GetComponentInParent<Player>();
		if (player == null) return;

		// Only process dead players (ActiveHealthController is NOT reliable post-death)
		bool isDead = (player.HealthController == null || !player.HealthController.IsAlive);
		if (!isDead) return;

		// --- 2. PuppetMaster: agony interruption ---
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

			if (pm.mappingWeight > 0.05f)
			{
				RagdollHelperClass.InterruptAgony(player, pm);
				if (rb != null && !rb.isKinematic)
				{
					rb.AddForceAtPosition(shot.Direction * (shot.Speed * 0.15f), shot.HitPoint, ForceMode.Impulse);
				}
			}
		}

		// --- 3. Post-mortem dismemberment ---
		// Head and torso are intentionally excluded: "Base HumanHead" is the mesh root —
		// scaling it to 0.001f collapses the entire body model.
		if (VisceralEntry.Instance == null || !VisceralEntry.Instance.EnableDismemberment.Value) return;

		EBodyPart? dismemberPart = null;
		string boneName = null;
		string capAsset = null;
		string[] extraAssets = Array.Empty<string>();

		// Strategy A: typed BodyPartColliderType (live-bot shot-detection colliders)
		if (bpc != null)
		{
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
			}
		}

		// Strategy B: ragdoll physics bone names (dead corpses — EFT format: "Base Human[L/R][Part]")
		// Confirmed from log capture: "Base HumanRThigh1", "Base HumanLCalf", etc.
		// Head ("Base HumanHead") deliberately skipped — it is the mesh root.
		if (dismemberPart == null)
		{
			string rbLow = rb.gameObject.name.ToLower();

			if (rbLow.Contains("humanlupperarm") || rbLow.Contains("humanlforearm") ||
			    rbLow.Contains("humanlarm") || rbLow.Contains("humanl_arm"))
			{
				dismemberPart = (EBodyPart)3;
				boneName = "lforearm1";
				capAsset = "Arm_LeftCap";
				extraAssets = new[] { "Arm_L_1", "Arm_L_2" };
			}
			else if (rbLow.Contains("humanrupperarm") || rbLow.Contains("humanrforearm") ||
			         rbLow.Contains("humanrarm") || rbLow.Contains("humanr_arm"))
			{
				dismemberPart = (EBodyPart)4;
				boneName = "rforearm1";
				capAsset = "Arm_RightCap";
				extraAssets = new[] { "Arm_R_1", "Arm_R_2" };
			}
			else if (rbLow.Contains("humanlthigh") || rbLow.Contains("humanlcalf") ||
			         rbLow.Contains("humanlleg"))
			{
				dismemberPart = (EBodyPart)5;
				boneName = "lthigh1";
				capAsset = "Leg_LeftCap";
				extraAssets = new[] { "gore_leg_torn01" };
			}
			else if (rbLow.Contains("humanrthigh") || rbLow.Contains("humanrcalf") ||
			         rbLow.Contains("humanrleg"))
			{
				dismemberPart = (EBodyPart)6;
				boneName = "rthigh1";
				capAsset = "Leg_RightCap";
				extraAssets = new[] { "gore_leg_torn02" };
			}
		}

		if (!dismemberPart.HasValue || boneName == null) return;

		// --- Caliber-based dismemberment chance (same table as KillPatch) ---
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
