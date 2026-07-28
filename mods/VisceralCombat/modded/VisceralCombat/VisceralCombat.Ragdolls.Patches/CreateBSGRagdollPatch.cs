using System;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using EFT.AssetsManager;
using EFT.Interactive;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace VisceralCombat.Ragdolls.Patches;

public class CreateBSGRagdollPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return typeof(Corpse).GetMethod("method_16");
	}

	[PatchPrefix]
	private static bool Prefix(Corpse __instance, bool forceStill = false)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		PlayerBody playerBody = ((Component)__instance).gameObject.GetComponent<Player>().PlayerBody;
		__instance.Ragdoll = new RagdollClass(Traverse.Create((object)__instance).Field<RigidbodySpawner[]>("rigidbodySpawner_0").Value, Traverse.Create((object)__instance).Field<CharacterJointSpawner[]>("characterJointSpawner_0").Value, Traverse.Create((object)__instance).Field<List<PlayerRigidbodySleepHierarchy>>("list_0").Value, Traverse.Create((object)__instance).Field<Vector3>("vector3_1").Value, EFTHardSettings.Instance.CorpseMaxDepenetrationVelocity, (CollisionDetectionMode)0, (MonoBehaviour)(object)__instance, (Func<bool, float, bool>)__instance.CheckCorpseIsStill, playerBody, (Func<bool>)playerBody.IsVisible, (Action)CounterDeleteRigidBody, forceStill, true);
		((LootItem)__instance).OnRigidbodyStarted();
		__instance.method_19();
		return false;
	}

	private static void CounterDeleteRigidBody()
	{
	}
}
