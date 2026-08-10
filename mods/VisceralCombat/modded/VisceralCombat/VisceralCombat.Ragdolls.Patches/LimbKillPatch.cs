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
					// Fix 3: Only force agony-end state when agony is actually running (mappingWeight > 0).
					// For pure BSG-ragdoll corpses (mappingWeight ≈ 0) this would call
					// DisableLiveActiveRagdoll → SetActive(false) on the PM GameObject, making the body vanish.
					if (pm.mappingWeight > 0.05f)
					{
						pm.stateSettings.killDuration = 0f;
						pm.state = PuppetMaster.State.Dead;
					}
				}
				muscle.props.muscleWeight *= 0.5f;
			}
		}

		// Fix 4: If agony animation is active, interrupt it on any bullet hit so the bot
		// collapses into physical ragdoll immediately instead of continuing to writhe.
		if (pm.mappingWeight > 0.05f && player != null)
		{
			RagdollHelperClass.InterruptAgony(player, pm);
		}
	}
}
