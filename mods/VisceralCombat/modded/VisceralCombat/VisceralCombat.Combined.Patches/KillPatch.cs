using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using Diz.Skinning;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using Fika.Core.Main.Players;
using Fika.Core.Main.Utils;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using VisceralCombat.Dismemberment.Classes;
using VisceralCombat.Dismemberment.Classes.Packets;
using VisceralCombat.Ragdolls.Classes;
using VisceralCombat.Ragdolls.Classes.Packets;
using VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

namespace VisceralCombat.Combined.Patches;

public class KillPatch : ModulePatch
{
	public static Dictionary<string, float> calibers = new Dictionary<string, float>();

	private static Func<Player, InventoryController> _getInventoryController = delegate(Player player)
	{
		object? obj = typeof(Player).GetField("_inventoryController", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(player);
		return (InventoryController)((obj is InventoryController) ? obj : null);
	};

	private static Dictionary<EBodyPart, string> bodyparts = new Dictionary<EBodyPart, string>
	{
		{ (EBodyPart)0, "head" },
		{ (EBodyPart)3, "lforearm1" },
		{ (EBodyPart)4, "rforearm1" },
		{ (EBodyPart)5, "lthigh1" },
		{ (EBodyPart)6, "rthigh1" }
	};

	public static string[] limbNames { get; set; } = Array.Empty<string>();

	public int RandomNumberOutcome { get; set; }

	protected override MethodBase GetTargetMethod()
	{
		return typeof(Player).GetMethod("ApplyDamageInfo");
	}

	[PatchPostfix]
	internal static void Postfix(Player __instance, DamageInfoStruct damageInfo, EBodyPart bodyPartType)
	{
		Dictionary<Player, int> deadPlayers = VisceralEntry.Instance.deadPlayers;
		if (__instance.HealthController.IsAlive)
		{
			return;
		}
		bool isFirstDeath = !deadPlayers.ContainsKey(__instance);
		if (isFirstDeath)
		{
			deadPlayers.Add(__instance, 0);
		}

		string caliber = null;
		if (!string.IsNullOrEmpty(damageInfo.SourceId) && Singleton<ItemFactoryClass>.Instantiated && Singleton<ItemFactoryClass>.Instance.ItemTemplates != null)
		{
			if (((Dictionary<MongoID, ItemTemplate>)(object)Singleton<ItemFactoryClass>.Instance.ItemTemplates).TryGetValue((MongoID)damageInfo.SourceId, out ItemTemplate itemTemplate) && itemTemplate != null)
			{
				if (itemTemplate is AmmoTemplate ammoTemplate)
				{
					caliber = ammoTemplate.Caliber;
				}
			}
		}

		string cleanCaliber = caliber;
		if (!string.IsNullOrEmpty(cleanCaliber) && cleanCaliber.StartsWith("Caliber"))
		{
			cleanCaliber = cleanCaliber.Substring(7);
		}
		float dismemberChance = 0.5f; // Default to 50% if caliber cannot be checked
		if (!string.IsNullOrEmpty(caliber) && (calibers.TryGetValue(caliber, out var chance) || (!string.IsNullOrEmpty(cleanCaliber) && calibers.TryGetValue(cleanCaliber, out chance))))
		{
			dismemberChance = chance;
		}

		if ((int)damageInfo.DamageType != 2048 && (int)damageInfo.DamageType != 4 && (int)damageInfo.DamageType != 32 && (int)damageInfo.DamageType != 8 && (int)damageInfo.DamageType != 16 && (int)damageInfo.DamageType != 8192)
		{
			if (Random.value > dismemberChance)
			{
				if (isFirstDeath && VisceralEntry.Instance.UseActiveRagdolls.Value && (FikaBackendUtils.IsServer || FikaBackendUtils.IsSinglePlayer))
				{
					if (!VisceralEntry.Instance.OnlyPlayersCanActiveRagdollEnemies.Value || !damageInfo.Player.IsAI)
					{
						if (Vector3.Distance(damageInfo.Player.iPlayer.Position, __instance.Position) <= (float)VisceralEntry.Instance.RagdollMaxDistance.Value && RagdollHelperClass.ShouldRagdoll(bodyPartType))
						{
							int chance2 = Random.Range(0, 10);
							if (!VisceralEntry.Instance.dismemberedPlayers.Contains(__instance))
							{
								DeathSetup(__instance, bodyPartType, chance2);
							}
						}
					}
				}
				return;
			}
		}

		if (!VisceralEntry.Instance.EnableDismemberment.Value)
		{
			return;
		}

		Transform[] affectedLimbs = null;
		string value3;

		// If hit in specific dismemberable bodypart, dismember that bodypart
		if (bodyparts.TryGetValue(bodyPartType, out value3))
		{
			switch ((int)bodyPartType)
			{
			case 3:
				DismemberLimb(__instance, damageInfo.Direction, bodyPartType, value3, "Arm_LeftCap", new string[2] { "Arm_L_1", "Arm_L_2" }, out affectedLimbs);
				break;
			case 4:
				DismemberLimb(__instance, damageInfo.Direction, bodyPartType, value3, "Arm_RightCap", new string[2] { "Arm_R_1", "Arm_R_2" }, out affectedLimbs);
				break;
			case 5:
				DismemberLimb(__instance, damageInfo.Direction, bodyPartType, value3, "Leg_LeftCap", new string[1] { "gore_leg_torn01" }, out affectedLimbs);
				break;
			case 6:
				DismemberLimb(__instance, damageInfo.Direction, bodyPartType, value3, "Leg_RightCap", new string[1] { "gore_leg_torn02" }, out affectedLimbs);
				break;
			case 0:
				// Head dismemberment → direct ragdoll, no agony animation.
				DismemberLimb(__instance, damageInfo.Direction, bodyPartType, value3, $"Head_{Random.Range(1, 4)}", Array.Empty<string>(), out affectedLimbs);
				break;
			}

			// After dismembering an arm or leg, trigger agony animation on the remaining body.
			// Head (case 0) is intentionally excluded — agony with a 0.001f skull bone is not safe.
			bool isLimbDismember = (int)bodyPartType == 3 || (int)bodyPartType == 4
			                    || (int)bodyPartType == 5 || (int)bodyPartType == 6;
			if (isLimbDismember
			    && isFirstDeath
			    && VisceralEntry.Instance.UseActiveRagdolls.Value
			    && (FikaBackendUtils.IsServer || FikaBackendUtils.IsSinglePlayer)
			    && !VisceralEntry.Instance.dismemberedPlayers.Contains(__instance)
			    && (!VisceralEntry.Instance.OnlyPlayersCanActiveRagdollEnemies.Value || !damageInfo.Player.IsAI)
			    && Vector3.Distance(damageInfo.Player.iPlayer.Position, __instance.Position)
			           <= (float)VisceralEntry.Instance.RagdollMaxDistance.Value
			    && RagdollHelperClass.ShouldRagdoll(bodyPartType))
			{
				int dismemberDeathChance = Random.Range(0, 10);
				DeathSetup(__instance, bodyPartType, dismemberDeathChance);
			}
		}

		else if ((int)damageInfo.DamageType == 2048 || (int)damageInfo.DamageType == 4)
		{
			// Explosions or heavy damage dismember random limbs
			if (Random.Range(0, 3) == 0)
			{
				DismemberLimb(__instance, damageInfo.Direction, bodyPartType, "lthigh1", "Leg_LeftCap", new string[1] { "gore_leg_torn01" }, out affectedLimbs);
			}
			if (Random.Range(0, 3) == 0)
			{
				DismemberLimb(__instance, damageInfo.Direction, bodyPartType, "rthigh1", "Leg_RightCap", new string[1] { "gore_leg_torn02" }, out affectedLimbs);
			}
			if (Random.Range(0, 3) == 0)
			{
				DismemberLimb(__instance, damageInfo.Direction, bodyPartType, "lforearm1", "Arm_LeftCap", new string[2] { "Arm_L_1", "Arm_L_2" }, out affectedLimbs);
			}
			if (Random.Range(0, 3) == 0)
			{
				DismemberLimb(__instance, damageInfo.Direction, bodyPartType, "rforearm1", "Arm_RightCap", new string[2] { "Arm_R_1", "Arm_R_2" }, out affectedLimbs);
			}
		}
	}

	internal static void DismemberLimb(Player player, Vector3 Direction, EBodyPart bodyPartType, string bone, string capAssetName, string[] assetNames, out Transform[] affectedLimbs)
	{
		if (player != null && player.BodyAnimatorCommon != null)
		{
			player.BodyAnimatorCommon.enabled = false;
		}
		string boneLower = bone.ToLower();
		affectedLimbs = (from t in VisceralCombat.Ragdolls.Classes.Utils.EnumerateHierarchyCore(player.Transform.Original)
			where ((Object)t != null) &&
				  ((Object)t).name.ToLower().Contains(boneLower) &&
				  !VisceralCombat.Dismemberment.Classes.Utils.ParentContains(t, "weapon_holster")
			select t).ToArray();

		Transform[] array = affectedLimbs;
		foreach (Transform val in array)
		{
			if (val.localScale == RagdollHelperClass.limbSize)
			{
				continue;
			}

			// Only send Fika network packets if player is a FikaPlayer (Coop)
			if ((FikaBackendUtils.IsServer || FikaBackendUtils.IsHeadless) && FikaBackendUtils.IsClient)
			{
				Transform[] array2 = affectedLimbs;
				foreach (Transform val2 in array2)
				{
					limbNames = HarmonyLib.CollectionExtensions.AddItem<string>(limbNames, val2.name).ToArray();
				}
				if (player is FikaPlayer fikaPlayer && fikaPlayer != null && Singleton<FikaServer>.Instantiated && Singleton<FikaServer>.Instance != null)
				{
					DismembermentPacket dismembermentPacket = default(DismembermentPacket);
					dismembermentPacket.playerID = fikaPlayer.NetId;
					dismembermentPacket.Direction = Direction;
					dismembermentPacket.bodyPartType = bodyPartType;
					dismembermentPacket.bone = bone;
					dismembermentPacket.capAssetName = capAssetName;
					dismembermentPacket.assetNames = assetNames;
					DismembermentPacket dismembermentPacket2 = dismembermentPacket;
					QuickLogger.Log(ELogType.Log, string.Format("Dismemberment Packet Sent: {0}, {1}, {2}, {3}, {4}, {5}", dismembermentPacket2.playerID, dismembermentPacket2.Direction, dismembermentPacket2.bodyPartType, dismembermentPacket2.bone, dismembermentPacket2.capAssetName, string.Join(",", dismembermentPacket2.assetNames)));
					Singleton<FikaServer>.Instance.SendData<DismembermentPacket>(ref dismembermentPacket2, (DeliveryMethod)0, false);
				}
			}

			Transform[] allBranchTransforms = val.GetComponentsInChildren<Transform>(true);
			foreach (Transform tBranch in allBranchTransforms)
			{
				if (tBranch != null)
				{
					tBranch.localScale = RagdollHelperClass.limbSize;
					if (tBranch.gameObject.GetComponent<DismemberedLimbScaler>() == null)
					{
						tBranch.gameObject.AddComponent<DismemberedLimbScaler>();
					}
				}
			}

			// Permanently set all Rigidbodies under the dismembered limb to isKinematic = true to stop PhysX joint solver
			Rigidbody[] limbRbs = val.GetComponentsInChildren<Rigidbody>(true);
			foreach (Rigidbody rb in limbRbs)
			{
				if (rb != null)
				{
					rb.isKinematic = true;
					rb.detectCollisions = false;
				}
			}

			// Destroy PhysX Joint constraints on scaled limb transforms to prevent 1000x joint anchor scale explosion
			Joint[] limbJoints = val.GetComponentsInChildren<Joint>(true);
			foreach (Joint j in limbJoints)
			{
				if (j != null)
				{
					Object.Destroy(j);
				}
			}

			// Clear any joint in the entire player body that connects to this dismembered limb
			Joint[] allPlayerJoints = player.GetComponentsInChildren<Joint>(true);
			foreach (Joint j in allPlayerJoints)
			{
				if (j != null && j.connectedBody != null && (j.connectedBody.transform == val || j.connectedBody.transform.IsChildOf(val)))
				{
					j.connectedBody = null;
					Object.Destroy(j);
				}
			}

			PuppetMaster pm = player.GetComponentInChildren<PuppetMaster>();
			if (pm == null && player.gameObject.transform.parent != null)
			{
				pm = player.gameObject.transform.parent.GetComponentInChildren<PuppetMaster>();
			}
			if (pm != null && pm.muscles != null)
			{
				foreach (Muscle m in pm.muscles)
				{
					if (m == null) continue;

					bool isTargetMatch = m.target != null && (m.target == val || m.target.IsChildOf(val));
					bool isJointMatch  = m.joint != null && (m.joint.transform == val || m.joint.transform.IsChildOf(val));
					bool isRbMatch     = m.rigidbody != null && (m.rigidbody.transform == val || m.rigidbody.transform.IsChildOf(val));
					bool isNameMatch   = !string.IsNullOrEmpty(m.name) && !string.IsNullOrEmpty(val.name) &&
					                     m.name.Equals(val.name, StringComparison.OrdinalIgnoreCase);

					if (isTargetMatch || isJointMatch || isRbMatch || isNameMatch)
					{
						m.state.isDisconnected = true;
						m.props.muscleWeight = 0f;
						m.props.pinWeight = 0f;
						m.props.mappingWeight = 0f;
						m.state.muscleWeightMlp = 0f;
						m.state.pinWeightMlp = 0f;
						m.state.mappingWeightMlp = 0f;

						// Also scale ragdoll rigidbody transform to 0.001f if separate from animated target transform
						if (m.rigidbody != null && m.rigidbody.transform != null && m.rigidbody.transform != val)
						{
							m.rigidbody.transform.localScale = RagdollHelperClass.limbSize;
							if (m.rigidbody.transform.gameObject.GetComponent<DismemberedLimbScaler>() == null)
							{
								m.rigidbody.transform.gameObject.AddComponent<DismemberedLimbScaler>();
							}
							m.rigidbody.isKinematic = true;
							m.rigidbody.detectCollisions = false;
						}
					}
				}
			}

			if (VisceralEntry.Instance.effectContainer != null && VisceralEntry.Instance.effectContainer.goreCaps != null)
			{
				GameObject val4 = VisceralEntry.Instance.effectContainer.goreCaps.FirstOrDefault((GameObject cap) => (Object)(object)cap != (Object)null && ((Object)cap).name == capAssetName);
				if ((Object)(object)val4 == (Object)null)
				{
					QuickLogger.Log(ELogType.Warn, "Gore cap '" + capAssetName + "' not found in list.");
				}
				else
				{
					GameObject val5 = Object.Instantiate<GameObject>(val4);
					Skin componentInChildren = val5.GetComponentInChildren<Skin>();
					if ((Object)(object)componentInChildren != (Object)null)
					{
						componentInChildren.Init(player.PlayerBody.SkeletonRootJoint);
						((AbstractSkin)componentInChildren).ApplySkin();
					}
				}
				foreach (string assetName in assetNames)
				{
					GameObject val6 = VisceralEntry.Instance.effectContainer.goreCaps.FirstOrDefault((GameObject a) => (Object)(object)a != (Object)null && ((Object)a).name == assetName);
					if ((Object)(object)val6 == (Object)null)
					{
						QuickLogger.Log(ELogType.Error, "Dismemberment: DismemberLimb | [" + assetName + "] not found in gorecaps");
						continue;
					}
					GameObject val7 = Object.Instantiate<GameObject>(val6);
					val7.transform.position = val.position;
				}
			}

			if ((int)bodyPartType == 0 && Random.value >= 0.5f)
			{
				if (VisceralEntry.Instance.effectContainer != null && VisceralEntry.Instance.effectContainer.bloodSFX != null && VisceralEntry.Instance.effectContainer.bloodSFX.Count > 0)
				{
					int index = Random.Range(0, VisceralEntry.Instance.effectContainer.bloodSFX.Count);
					GameObject val8 = Object.Instantiate<GameObject>(VisceralEntry.Instance.effectContainer.bloodSFX[index]);
					val8.transform.position = val.position;
				}
			}
			SpawnOldVolumetricBlood(val, Direction, 1f);
			SpawnArterialSprays(val, Direction);
		}
		if (player.IsYourPlayer && (int)bodyPartType == 0)
		{
			if (VisceralEntry.Instance.effectContainer != null && VisceralEntry.Instance.effectContainer.blood3dFxEffects != null && VisceralEntry.Instance.effectContainer.blood3dFxEffects.Count > 0)
			{
				VisceralEntry.Instance.effectContainer.blood3dFxEffects[0].SetActive(true);
			}
		}
	}

	public static void DeathSetup(Player p, EBodyPart eBodyPart, int Chance)
	{
		try
		{
			if ((Object)(object)p == (Object)null || (Object)(object)((Component)p).gameObject == (Object)null)
			{
				QuickLogger.Log(ELogType.Error, "DeathSetup: Player or GameObject is Null!");
				return;
			}

			if (VisceralEntry.Instance != null && !VisceralEntry.Instance.dismemberedPlayers.Contains(p))
			{
				VisceralEntry.Instance.dismemberedPlayers.Add(p);
			}

			if (FikaBackendUtils.IsServer && FikaBackendUtils.IsClient)
			{
				if (p is FikaPlayer fikaPlayer && fikaPlayer != null && Singleton<FikaServer>.Instantiated && Singleton<FikaServer>.Instance != null)
				{
					RagdollSyncPacket ragdollSyncPacket = default(RagdollSyncPacket);
					ragdollSyncPacket.PlayerID = fikaPlayer.NetId;
					ragdollSyncPacket.BodyPart = eBodyPart;
					RagdollSyncPacket ragdollSyncPacket2 = ragdollSyncPacket;
					QuickLogger.Log(ELogType.Log, $"Ragdoll Packet Sent: {ragdollSyncPacket2.PlayerID}, {ragdollSyncPacket2.BodyPart}, {ragdollSyncPacket2.RandomChance}");
					Singleton<FikaServer>.Instance.SendData<RagdollSyncPacket>(ref ragdollSyncPacket2, (DeliveryMethod)0, false);
				}
			}

			RagdollHelperClass.limbsToCheck.Clear();

			PuppetMaster componentInChildren = null;
			if (((Component)p).gameObject.transform.parent != null)
			{
				componentInChildren = ((Component)((Component)p).gameObject.transform.parent).GetComponentInChildren<PuppetMaster>();
			}
			if ((Object)(object)componentInChildren == (Object)null)
			{
				componentInChildren = ((Component)p).GetComponentInChildren<PuppetMaster>();
			}
			if ((Object)(object)componentInChildren == (Object)null)
			{
				componentInChildren = ((Component)p).GetComponentInParent<PuppetMaster>();
			}

			if ((Object)(object)componentInChildren == (Object)null)
			{
				VisceralCombat.Ragdolls.Classes.Utils.SetupPuppetMaster(p);
				if (((Component)p).gameObject.transform.parent != null)
				{
					componentInChildren = ((Component)((Component)p).gameObject.transform.parent).GetComponentInChildren<PuppetMaster>();
				}
				if ((Object)(object)componentInChildren == (Object)null)
				{
					componentInChildren = ((Component)p).GetComponentInChildren<PuppetMaster>();
				}
			}

			if ((Object)(object)componentInChildren == (Object)null)
			{
				QuickLogger.Log(ELogType.Warn, $"DeathSetup: Still no PuppetMaster found for '{p.Profile?.Nickname}'!");
				return;
			}

			componentInChildren.pinWeight = 0.25f;
			componentInChildren.stateSettings.enableAngularLimitsOnKill = true;
			componentInChildren.stateSettings.deadMuscleWeight = 0.01f;
			componentInChildren.muscleSpring = 175f;
			componentInChildren.muscleDamper = 1.5f;
			((Behaviour)componentInChildren).enabled = true;
			// Zero muscle weights for the dismembered limb BEFORE the animator starts evaluating,
			// so LerpLayerWeight on layer 18 never drives a 0.001f-scaled bone (which causes gigantism).
			RagdollHelperClass.DisableDismemberedMuscles(componentInChildren, eBodyPart);
			if (p.BodyAnimatorCommon == null)
			{
				QuickLogger.Log(ELogType.Error, "Player's BodyAnimatorCommon is null!");
				return;
			}
			p.BodyAnimatorCommon.enabled = true;
			AnimatorOverrideController runtimeAnimatorController = new AnimatorOverrideController(p.BodyAnimatorCommon.runtimeAnimatorController);
			p.BodyAnimatorCommon.runtimeAnimatorController = (RuntimeAnimatorController)(object)runtimeAnimatorController;
			RagdollHelperClass.PlayDeathAnimation(p, componentInChildren, eBodyPart);
			if ((Object)(object)p.PlayerBones?.Pelvis?.Original == (Object)null)
			{
				QuickLogger.Log(ELogType.Error, "PlayerBones or Pelvis is null!");
				return;
			}
			TransformHelperClass.SetLayersRecursively(((Component)((Component)p.PlayerBones.Pelvis.Original).transform).gameObject, LayerMask.NameToLayer("Deadbody"));
			if ((Object)(object)p.PlayerBones.HolsterPrimary != (Object)null) ((Component)p.PlayerBones.HolsterPrimary).gameObject.SetActive(false);
			if ((Object)(object)p.PlayerBones.HolsterSecondary != (Object)null) ((Component)p.PlayerBones.HolsterSecondary).gameObject.SetActive(false);
			if ((Object)(object)p.PlayerBones.HolsterPrimaryAlternative != (Object)null) ((Component)p.PlayerBones.HolsterPrimaryAlternative).gameObject.SetActive(false);
			if ((Object)(object)p.PlayerBones.HolsterSecondaryAlternative != (Object)null) ((Component)p.PlayerBones.HolsterSecondaryAlternative).gameObject.SetActive(false);
			if ((Object)(object)p.PlayerBones.HolsterPistol != (Object)null) ((Component)p.PlayerBones.HolsterPistol).gameObject.SetActive(false);
			if ((Object)(object)p.PlayerBones.LeftLegHolsterPistol != (Object)null) ((Component)p.PlayerBones.LeftLegHolsterPistol).gameObject.SetActive(false);
			componentInChildren.Teleport(((Component)p).gameObject.transform.position, Quaternion.LookRotation(p.LookDirection), moveToTarget: true);
			((MonoBehaviour)p).StartCoroutine(RagdollHelperClass.LerpMappingWeight(componentInChildren, 0f, 1f, VisceralEntry.Instance.MappingWeightDuration.Value));
			componentInChildren.state = PuppetMaster.State.Dead;
			if ((int)eBodyPart > 0)
			{
				GClass855.WaitSeconds((MonoBehaviour)(object)StaticManager.Instance, 0.1f, (Action)delegate
				{
					if ((Object)(object)p != (Object)null && p.BodyAnimatorCommon != null)
					{
						p.BodyAnimatorCommon.enabled = true;
					}
				});
			}
		}
		catch (Exception ex)
		{
			QuickLogger.Log(ELogType.Error, $"Error in DeathSetup: {ex.Message}");
		}
	}

	internal static void SpawnOldVolumetricBlood(Transform target, Vector3 direction, float Scale)
	{
		if (!VisceralEntry.Instance.EnableBloodEffects.Value)
			return;
		if (VisceralEntry.Instance.effectContainer == null)
			return;

		List<GameObject> bloodParticles = VisceralEntry.Instance.effectContainer.bloodParticles;
		if (bloodParticles == null || bloodParticles.Count == 0)
		{
			QuickLogger.Log(ELogType.Warn, "bloodParticles list is empty or null!");
			return;
		}
		int num = Random.Range(0, bloodParticles.Count);
		GameObject val = bloodParticles[num];
		if ((Object)(object)val == (Object)null)
		{
			QuickLogger.Log(ELogType.Warn, $"bloodParticles[{num}] is null, aborting spawn.");
			return;
		}
		GameObject val2 = Object.Instantiate<GameObject>(val);
		GameObject brainParticles = VisceralEntry.Instance.effectContainer.brainParticles;
		if ((Object)(object)brainParticles == (Object)null)
		{
			QuickLogger.Log(ELogType.Warn, "brainParticles prefab is null, aborting spawn.");
			Object.Destroy((Object)(object)val2);
			return;
		}
		GameObject brainObject = Object.Instantiate<GameObject>(brainParticles);
		BFX_BloodSettings component = val2.GetComponent<BFX_BloodSettings>();
		val2.transform.position = target.position;
		float num2 = VisceralEntry.Instance.BloodSplatterSize.Value * Scale;
		val2.transform.localScale = new Vector3(num2, num2, num2);
		direction.y = 0f;
		Quaternion val3 = Quaternion.LookRotation(direction);
		val3 *= Quaternion.Euler(0f, 180f, 0f);
		val2.transform.rotation = val3;
		if (component != null)
		{
			if (VisceralEntry.Instance.UseOldBloodDecal.Value)
			{
				component.GroundHeight = target.position.y - 1.9f;
			}
			else
			{
				component.GroundHeight = -9999999f;
			}
			component.ClampDecalSideSurface = true;
		}
		brainObject.transform.position = target.position;
		Transform transform = brainObject.transform;
		Quaternion rotation = target.rotation;
		transform.rotation = Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);
		GClass855.WaitSeconds((MonoBehaviour)(object)StaticManager.Instance, 5f, (Action)delegate
		{
			Object.Destroy((Object)(object)brainObject);
		});
	}

	private static void SpawnArterialSprays(Transform target, Vector3 direction)
	{
		if (!VisceralEntry.Instance.ArterySpray.Value || !VisceralEntry.Instance.EnableBloodEffects.Value)
			return;
		if (VisceralEntry.Instance.effectContainer == null || (Object)(object)VisceralEntry.Instance.effectContainer.limbSquirter == (Object)null)
			return;

		GameObject bloodParticleObject = Object.Instantiate<GameObject>(VisceralEntry.Instance.effectContainer.limbSquirter);
		bloodParticleObject.AddComponent<ParticleFloorPainter>();
		if (((Component)target).transform.localScale == Vector3.zero)
		{
			bloodParticleObject.transform.parent = ((Component)target).transform.parent;
		}
		else
		{
			bloodParticleObject.transform.parent = ((Component)target).transform;
		}
		bloodParticleObject.transform.localPosition = new Vector3(0f, 0f, 0f);
		bloodParticleObject.transform.localRotation = new Quaternion(-0.0923f, 0.7011f, -0.0923f, -0.7011f);
		ParticleSystem[] componentsInChildren = bloodParticleObject.GetComponentsInChildren<ParticleSystem>();
		float num = Random.Range(VisceralEntry.Instance.ArterySprayMin.Value, VisceralEntry.Instance.ArterySprayMax.Value);
		ParticleSystem[] array = componentsInChildren;
		foreach (ParticleSystem val in array)
		{
			val.loop = false;
			var main = val.main;
			main.duration = num;
			var collision = val.collision;
			collision.sendCollisionMessages = true;
			((Component)val).gameObject.AddComponent<ParticleFloorPainter>();
			val.Play();
		}
		GClass855.WaitSeconds((MonoBehaviour)(object)StaticManager.Instance, num + 1f, (Action)delegate
		{
			Object.Destroy((Object)(object)bloodParticleObject);
		});
	}
}
