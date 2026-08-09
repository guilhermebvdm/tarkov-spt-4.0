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

		if (shot.IsShotFinished)
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
					pm.stateSettings.killDuration = 0f;
					pm.state = PuppetMaster.State.Dead;
				}
				muscle.props.muscleWeight *= 0.5f;
			}
		}
	}
}
