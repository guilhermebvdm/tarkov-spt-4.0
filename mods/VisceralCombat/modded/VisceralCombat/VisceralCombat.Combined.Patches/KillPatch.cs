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
		{ (EBodyPart)0, "base humanhead" },
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
		if (!deadPlayers.ContainsKey(__instance))
		{
			deadPlayers.Add(__instance, 0);
		}

		if ((int)damageInfo.DamageType != 2048 && (int)damageInfo.DamageType != 4 && (int)damageInfo.DamageType != 32 && (int)damageInfo.DamageType != 8 && (int)damageInfo.DamageType != 16 && (int)damageInfo.DamageType != 8192)
		{
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

			float dismemberChance = 1.0f; // Default to 100% if caliber cannot be checked
			if (!string.IsNullOrEmpty(caliber) && calibers.TryGetValue(caliber, out var chance))
			{
				dismemberChance = chance;
			}

			if (Random.value > dismemberChance)
			{
				if (VisceralEntry.Instance.UseActiveRagdolls.Value && (FikaBackendUtils.IsServer || FikaBackendUtils.IsSinglePlayer))
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
		if ((int)damageInfo.DamageType == 2048 || (int)damageInfo.DamageType == 4)
		{
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
		else if (bodyparts.TryGetValue(bodyPartType, out value3))
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
				DismemberLimb(__instance, damageInfo.Direction, bodyPartType, value3, $"Head_{Random.Range(1, 4)}", Array.Empty<string>(), out affectedLimbs);
				break;
			case 1:
			case 2:
				break;
			}
		}
	}

	internal static void DismemberLimb(Player player, Vector3 Direction, EBodyPart bodyPartType, string bone, string capAssetName, string[] assetNames, out Transform[] affectedLimbs)
	{
		affectedLimbs = (from t in VisceralCombat.Ragdolls.Classes.Utils.EnumerateHierarchyCore(player.Transform.Original)
			where ((Object)t).name.ToLower().Contains(bone) || (((int)bodyPartType == 0 || bone.Contains("head")) && ((Object)t).name.ToLower().Contains("head")) && !VisceralCombat.Dismemberment.Classes.Utils.ParentContains(t, "weapon_holster")
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

			if (!VisceralEntry.Instance.dismemberedPlayers.Contains(player))
			{
				VisceralEntry.Instance.dismemberedPlayers.Add(player);
			}
			val.localScale = RagdollHelperClass.limbSize;
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
			if ((int)bodyPartType == 0 && Random.value >= 0.5f)
			{
				int index = Random.Range(0, VisceralEntry.Instance.effectContainer.bloodSFX.Count);
				GameObject val8 = Object.Instantiate<GameObject>(VisceralEntry.Instance.effectContainer.bloodSFX[index]);
				val8.transform.position = val.position;
			}
			SpawnOldVolumetricBlood(val, Direction, 1f);
			SpawnArterialSprays(val, Direction);
		}
		if (player.IsYourPlayer && (int)bodyPartType == 0)
		{
			VisceralEntry.Instance.effectContainer.blood3dFxEffects[0].SetActive(true);
		}
		if (VisceralEntry.Instance.UseActiveRagdolls.Value && (FikaBackendUtils.IsServer || FikaBackendUtils.IsSinglePlayer))
		{
			DeathSetup(player, bodyPartType, 10);
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
				QuickLogger.Log(ELogType.Warn, "DeathSetup: No PuppetMaster found in player's hierarchy!");
				return;
			}

			componentInChildren.pinWeight = 0.25f;
			componentInChildren.stateSettings.enableAngularLimitsOnKill = true;
			componentInChildren.stateSettings.deadMuscleWeight = 0.01f;
			componentInChildren.muscleSpring = 175f;
			componentInChildren.muscleDamper = 1.5f;
			((Behaviour)componentInChildren).enabled = true;
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
			((MonoBehaviour)p).StartCoroutine(RagdollHelperClass.LerpMappingWeight(componentInChildren, 0f, 1f, 0.8f));
			componentInChildren.state = PuppetMaster.State.Dead;
			GClass855.WaitSeconds((MonoBehaviour)(object)StaticManager.Instance, 0.1f, (Action)delegate
			{
				if ((Object)(object)p != (Object)null && p.BodyAnimatorCommon != null)
				{
					p.BodyAnimatorCommon.enabled = true;
				}
			});
		}
		catch (Exception ex)
		{
			QuickLogger.Log(ELogType.Error, $"Error in DeathSetup: {ex.Message}");
		}
	}

	internal static void SpawnOldVolumetricBlood(Transform val, Vector3 Direction, float mul)
	{
		if (VisceralEntry.Instance.EnableBloodEffects.Value && (Object)(object)VisceralEntry.Instance.effectContainer.limbSquirter != (Object)null)
		{
			GameObject val2 = GoreObjectPool.Instance.Spawn(VisceralEntry.Instance.effectContainer.limbSquirter, val.position, Quaternion.LookRotation(Direction));
			ParticleSystem component = val2.GetComponent<ParticleSystem>();
			if ((Object)(object)component != (Object)null)
			{
				ParticleSystem.MainModule main = component.main;
				main.startSizeMultiplier *= mul;
				main.startLifetimeMultiplier *= mul;
			}
			GClass855.WaitSeconds((MonoBehaviour)(object)StaticManager.Instance, 30f, (Action)delegate
			{
				if (GoreObjectPool.Instance != null && val2 != null)
				{
					GoreObjectPool.Instance.Recycle(val2);
				}
			});
		}
	}

	private static void SpawnArterialSprays(Transform val, Vector3 Direction)
	{
		if (!VisceralEntry.Instance.ArterySpray.Value || !VisceralEntry.Instance.EnableBloodEffects.Value || (Object)(object)VisceralEntry.Instance.effectContainer.squirtEffect1 == (Object)null || (Object)(object)VisceralEntry.Instance.effectContainer.squirtEffect2 == (Object)null)
		{
			return;
		}
		GameObject val2 = GoreObjectPool.Instance.Spawn((Random.value > 0.5f) ? VisceralEntry.Instance.effectContainer.squirtEffect1 : VisceralEntry.Instance.effectContainer.squirtEffect2, val.position, Quaternion.LookRotation(-Direction));
		val2.transform.parent = val;
		ParticleSystem component = val2.GetComponent<ParticleSystem>();
		if ((Object)(object)component != (Object)null)
		{
			ParticleSystem.MainModule main = component.main;
			main.loop = false;
		}
		GClass855.WaitSeconds((MonoBehaviour)(object)StaticManager.Instance, Random.Range(VisceralEntry.Instance.ArterySprayMin.Value, VisceralEntry.Instance.ArterySprayMax.Value), (Action)delegate
		{
			if ((Object)(object)component != (Object)null)
			{
				ParticleSystem.MainModule main2 = component.main;
				main2.loop = false;
			}
			GClass855.WaitSeconds((MonoBehaviour)(object)StaticManager.Instance, 5f, (Action)delegate
			{
				if (GoreObjectPool.Instance != null && val2 != null)
				{
					GoreObjectPool.Instance.Recycle(val2);
				}
			});
		});
	}
}
