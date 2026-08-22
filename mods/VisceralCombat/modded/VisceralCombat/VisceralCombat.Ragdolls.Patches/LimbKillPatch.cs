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
	private static readonly System.Collections.Generic.HashSet<long> _evaluatedLivingVolleys = new System.Collections.Generic.HashSet<long>();

	public static void ClearLivingVolleys()
	{
		_evaluatedLivingVolleys.Clear();
	}

	protected override MethodBase GetTargetMethod()
	{
		return typeof(BallisticsCalculator).GetMethods(BindingFlags.Instance | BindingFlags.Public).First((MethodInfo m) => m.Name == "Shoot" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(EftBulletClass));
	}

	[PatchPostfix]
	private static void Postfix(EftBulletClass shot)
	{
		if (shot == null) return;
		VisceralCombat.Combined.Classes.VisceralShotProcessor.RegisterShot(shot);
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

		// Only process dead players OR living AI bots when VisceralCombat is present for all players
		bool isDead = (player.HealthController == null || !player.HealthController.IsAlive) && !RagdollHelperClass.IsPlayerDowned(player);
		if (!isDead)
		{
			if (!player.IsAI || !VisceralEntry.AllPlayersHaveVisceralCombat) return;
		}

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
			else if (rbLow.Contains("humanhead") || rbLow.Contains("humanskull"))
			{
				// Post-mortem head dismemberment using identical bone/cap parameters to KillPatch case 0.
				dismemberPart = (EBodyPart)0;
				boneName = "head";
				capAsset = $"Head_{UnityEngine.Random.Range(1, 4)}";
				extraAssets = Array.Empty<string>();
			}
		}

		if (!dismemberPart.HasValue || boneName == null) return;

		// --- Living bots/players branch ---
		// Can ONLY lose LEGS (LeftLeg=5 or RightLeg=6). Arms and Head are strictly dead-only!
		if (!isDead)
		{
			if (dismemberPart.Value != (EBodyPart)5 && dismemberPart.Value != (EBodyPart)6) return;

			// If the living bot already has a LivingDismembermentController, skip further leg dismemberment
			if (player.GetComponent<VisceralCombat.Dismemberment.Classes.LivingDismembermentController>() != null) return;

			// Buckshot protection: group all pellets from the same trigger pull/shot using (player.Id + shot.FireIndex)
			long volleyKey = ((long)player.Id << 32) | (uint)(shot.FireIndex & 0xFFFFFFFF);
			if (_evaluatedLivingVolleys.Contains(volleyKey))
			{
				return;
			}
			_evaluatedLivingVolleys.Add(volleyKey);
			if (_evaluatedLivingVolleys.Count > 1000) _evaluatedLivingVolleys.Clear();

			// Fixed 30% chance per hit/shot (counted once per shotgun volley)
			float livingChance = 0.30f;
			if (UnityEngine.Random.value <= livingChance)
			{
				Transform[] dummyLimbs;
				VisceralCombat.Combined.Patches.KillPatch.DismemberLimb(player, shot.Direction, dismemberPart.Value, boneName, capAsset, extraAssets, out dummyLimbs);
				VisceralCombat.Dismemberment.Classes.LivingDismembermentController.Attach(player, dismemberPart.Value);
				VisceralCombat.Combined.Classes.VisceralNetworkUtils.SendLivingDismemberment(player, dismemberPart.Value, shot.Direction, boneName, capAsset, extraAssets);
			}
			return;
		}

		// --- Dead corpses branch (uses caliber chance table) ---
		float chance = 0.5f;
		if (shot.Ammo is AmmoItemClass ammo && !string.IsNullOrEmpty(ammo.Caliber))
		{
			string calStr = ammo.Caliber;
			string cleanCalStr = calStr.StartsWith("Caliber") ? calStr.Substring(7) : calStr;
			if (VisceralCombat.Combined.Patches.KillPatch.calibers.TryGetValue(calStr, out float foundChance) ||
			    VisceralCombat.Combined.Patches.KillPatch.calibers.TryGetValue(cleanCalStr, out foundChance))
			{
				chance = foundChance;
			}
		}

		if (UnityEngine.Random.value <= chance)
		{
			Transform[] dummyLimbs;
			VisceralCombat.Combined.Patches.KillPatch.DismemberLimb(player, shot.Direction, dismemberPart.Value, boneName, capAsset, extraAssets, out dummyLimbs);
		}
	}
}
