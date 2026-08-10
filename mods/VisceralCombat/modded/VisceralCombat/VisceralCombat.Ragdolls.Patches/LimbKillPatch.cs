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

		GameObject rootGO = VisceralCombat.Dismemberment.Classes.Utils.GetRootGameObject(rb.gameObject);
		if (rootGO == null) return;

		PuppetMaster pm = rootGO.GetComponentInChildren<PuppetMaster>();
		if (pm == null || !pm.initiated) return;

		GameObject playerRoot = VisceralCombat.Dismemberment.Classes.Utils.GetRootGameObject(pm.gameObject);
		if (playerRoot == null) return;

		Player player = playerRoot.GetComponentInChildren<Player>();
		if (player == null || player.ActiveHealthController == null || player.ActiveHealthController.IsAlive) return;

		string rbName = rb.gameObject.name;
		if (pm.muscles == null) return;

		foreach (Muscle muscle in pm.muscles)
		{
			if (muscle != null && muscle.name != null && muscle.name.Contains(rbName))
			{
				if (rbName.Contains("Head"))
				{
					if (pm.mappingWeight > 0.05f)
					{
						pm.stateSettings.killDuration = 0f;
						pm.state = PuppetMaster.State.Dead;
					}
				}
				muscle.props.muscleWeight *= 0.5f;
			}
		}

		// If agony animation is active, interrupt it on any bullet hit so the bot
		// collapses into physical ragdoll immediately from its current floor pose.
		if (pm.mappingWeight > 0.05f && player != null)
		{
			RagdollHelperClass.InterruptAgony(player, pm);
			if (rb != null && !rb.isKinematic)
			{
				rb.AddForceAtPosition(shot.Direction * (shot.Speed * 0.15f), shot.HitPoint, ForceMode.Impulse);
			}
		}

		// Post-mortem dismemberment: Allow shooting off arms and legs on dead corpses
		if (VisceralEntry.Instance != null && VisceralEntry.Instance.EnableDismemberment.Value && player != null)
		{
			string rbLow = rbName.ToLower();
			EBodyPart? dismemberPart = null;
			string boneName = null;
			string capAsset = null;
			string[] extraAssets = Array.Empty<string>();

			if (rbLow.Contains("lupperarm") || rbLow.Contains("lforearm") || rbLow.Contains("larm") || rbLow.Contains("lhand") || rbLow.Contains("lpalm"))
			{
				dismemberPart = (EBodyPart)3;
				boneName = "lforearm1";
				capAsset = "Arm_LeftCap";
				extraAssets = new[] { "Arm_L_1", "Arm_L_2" };
			}
			else if (rbLow.Contains("rupperarm") || rbLow.Contains("rforearm") || rbLow.Contains("rarm") || rbLow.Contains("rhand") || rbLow.Contains("rpalm"))
			{
				dismemberPart = (EBodyPart)4;
				boneName = "rforearm1";
				capAsset = "Arm_RightCap";
				extraAssets = new[] { "Arm_R_1", "Arm_R_2" };
			}
			else if (rbLow.Contains("lthigh") || rbLow.Contains("lleg") || rbLow.Contains("lcalf") || rbLow.Contains("lfoot"))
			{
				dismemberPart = (EBodyPart)5;
				boneName = "lthigh1";
				capAsset = "Leg_LeftCap";
				extraAssets = new[] { "gore_leg_torn01" };
			}
			else if (rbLow.Contains("rthigh") || rbLow.Contains("rleg") || rbLow.Contains("rcalf") || rbLow.Contains("rfoot"))
			{
				dismemberPart = (EBodyPart)6;
				boneName = "rthigh1";
				capAsset = "Leg_RightCap";
				extraAssets = new[] { "gore_leg_torn02" };
			}

			if (dismemberPart.HasValue && boneName != null)
			{
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
	}
}
