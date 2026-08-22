using System;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using EFT.AssetsManager;
using EFT.Interactive;
using SPT.Reflection.Patching;
using UnityEngine;

namespace VisceralCombat.Ragdolls.Patches;

public class CreateBSGRagdollPatch : ModulePatch
{
	private static readonly FieldInfo _rbSpawnersField = typeof(Corpse).GetField("rigidbodySpawner_0", BindingFlags.Instance | BindingFlags.NonPublic);
	private static readonly FieldInfo _jointSpawnersField = typeof(Corpse).GetField("characterJointSpawner_0", BindingFlags.Instance | BindingFlags.NonPublic);
	private static readonly FieldInfo _sleepHierarchyField = typeof(Corpse).GetField("list_0", BindingFlags.Instance | BindingFlags.NonPublic);
	private static readonly FieldInfo _velocityField = typeof(Corpse).GetField("vector3_1", BindingFlags.Instance | BindingFlags.NonPublic);
	private static readonly FieldInfo _playerBodyField = typeof(Corpse).GetField("PlayerBody_0", BindingFlags.Instance | BindingFlags.NonPublic);

	protected override MethodBase GetTargetMethod()
	{
		return typeof(Corpse).GetMethod("method_16", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
	}

	[PatchPrefix]
	private static bool Prefix(Corpse __instance, bool forceStill = false)
	{
		PlayerBody playerBody = _playerBodyField?.GetValue(__instance) as PlayerBody ?? __instance.GetComponentInChildren<PlayerBody>();
		if (playerBody == null) return true; // fallback to EFT native logic if PlayerBody is unavailable

		var rbSpawners = _rbSpawnersField?.GetValue(__instance) as RigidbodySpawner[];
		var jointSpawners = _jointSpawnersField?.GetValue(__instance) as CharacterJointSpawner[];
		var sleepList = _sleepHierarchyField?.GetValue(__instance) as List<PlayerRigidbodySleepHierarchy>;
		Vector3 vel = _velocityField != null ? (Vector3)_velocityField.GetValue(__instance) : Vector3.zero;

		__instance.Ragdoll = new RagdollClass(
			rbSpawners,
			jointSpawners,
			sleepList,
			vel,
			EFTHardSettings.Instance.CorpseMaxDepenetrationVelocity,
			CollisionDetectionMode.Discrete,
			__instance,
			__instance.CheckCorpseIsStill,
			playerBody,
			playerBody.IsVisible,
			CounterDeleteRigidBody,
			forceStill,
			true
		);
		__instance.OnRigidbodyStarted();
		__instance.method_19();
		return false;
	}

	private static void CounterDeleteRigidBody()
	{
	}
}
