using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using EFT.AssetsManager;
using SPT.Reflection.Patching;
using UnityEngine;
using VisceralCombat.Ragdolls.Classes;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Patches;

public class RagdollClassPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return typeof(RagdollClass).GetMethod("Start");
	}

	[PatchPrefix]
	private static bool Prefix(RagdollClass __instance)
	{
		GClass4062.ReleaseBeginSample("CorpseRagdoll.SpawnRigidbodies", "Start");
		CharacterJointSpawner[] characterJointSpawner_ = __instance.CharacterJointSpawner_0;
		if (characterJointSpawner_ != null)
		{
			for (int i = 0; i < characterJointSpawner_.Length; i++)
			{
				Joint val = characterJointSpawner_[i].Create();
				val.enablePreprocessing = false;
				if (val is ConfigurableJoint configurableJoint)
				{
					configurableJoint.projectionMode = JointProjectionMode.PositionAndRotation;
				}
				else if (val is CharacterJoint characterJoint)
				{
					characterJoint.enableProjection = true;
				}
				val.massScale = val.connectedBody.mass / val.GetComponent<Rigidbody>().mass;
				val.connectedMassScale = 1f;
			}
		}

		RigidbodySpawner[] rigidbodySpawner_ = __instance.RigidbodySpawner_0;
		if (rigidbodySpawner_ != null)
		{
			for (int j = 0; j < rigidbodySpawner_.Length; j++)
			{
				Rigidbody val4 = rigidbodySpawner_[j].Create();
				Vector3 normalized = __instance.Vector3_0.normalized;
				__instance.Vector3_0 = (normalized.Equals(Vector3.up) ? normalized : Vector3.ClampMagnitude(__instance.Vector3_0, 2f));
				val4.isKinematic = false;
				val4.maxDepenetrationVelocity = __instance.Float_0;
				val4.velocity = __instance.Vector3_0;
				val4.collisionDetectionMode = __instance.CollisionDetectionMode_0;
				EFTPhysicsClass.GClass745.SupportRigidbody(val4, 0f);
			}
		}

		__instance.Bool_2 = false;
		if (__instance.Bool_1 && __instance.MonoBehaviour_0 != null)
		{
			__instance.MonoBehaviour_0.StartCoroutine(RagdollSleepHandler(__instance));
		}

		if (__instance.PlayerBody_0?.PlayerBones?.ArmorPlateColliders != null)
		{
			ArmorPlateCollider[] armorPlateColliders = __instance.PlayerBody_0.PlayerBones.ArmorPlateColliders;
			for (int k = 0; k < armorPlateColliders.Length; k++)
			{
				if (armorPlateColliders[k] != null)
				{
					armorPlateColliders[k].gameObject.SetActive(false);
				}
			}
		}

		return false;
	}

	public static IEnumerator RagdollSleepHandler(RagdollClass instance)
	{
		yield return null;
		instance.method_7();

		List<Rigidbody> rbsList = new List<Rigidbody>();
		if (instance.RigidbodySpawner_0 != null)
		{
			foreach (RigidbodySpawner spawner in instance.RigidbodySpawner_0)
			{
				if (spawner?.Rigidbody != null)
				{
					rbsList.Add(spawner.Rigidbody);
				}
			}
		}

		Rigidbody[] rbs = rbsList.ToArray();
		Player player = instance.PlayerBody_0?.GetComponentInParent<Player>();

		// Dynamically wait for agony animation to finish and body to come to a complete rest on the ground
		Transform root = instance.PlayerBody_0 != null ? ((Component)instance.PlayerBody_0).transform : null;
		yield return RagdollHelperClass.SleepCorpseWhenAtRest(root, rbs, player, 2.5f, 15.0f);

		instance.method_5();
		instance.Bool_2 = true;
		instance.Action_0?.Invoke();

		while (instance.PlayerBody_0 != null && instance.Func_1 != null && instance.Func_1())
		{
			yield return null;
		}

		if (instance.PlayerBody_0 == null)
		{
			yield break;
		}

		instance.method_2();
	}
}
