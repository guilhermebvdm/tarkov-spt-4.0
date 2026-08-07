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
		{
			(EBodyPart)0,
			"base humanhead"
		},
		{
			(EBodyPart)3,
			"lforearm1"
		},
		{
			(EBodyPart)4,
			"rforearm1"
		},
		{
			(EBodyPart)5,
			"lthigh1"
		},
		{
			(EBodyPart)6,
			"rthigh1"
		}
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
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Invalid comparison between Unknown and I4
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Invalid comparison between Unknown and I4
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Invalid comparison between Unknown and I4
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Invalid comparison between Unknown and I4
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Invalid comparison between Unknown and I4
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Invalid comparison between Unknown and I4
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Invalid comparison between Unknown and I4
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Expected I4, but got Unknown
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_044f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0450: Unknown result type (might be due to invalid IL or missing references)
		//IL_0455: Unknown result type (might be due to invalid IL or missing references)
		//IL_0477: Unknown result type (might be due to invalid IL or missing references)
		//IL_0478: Unknown result type (might be due to invalid IL or missing references)
		//IL_047d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
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
			if (!((Dictionary<MongoID, ItemTemplate>)(object)Singleton<ItemFactoryClass>.Instance.ItemTemplates).TryGetValue((MongoID)damageInfo.SourceId, out ItemTemplate value))
			{
				return;
			}
			AmmoTemplate val = (AmmoTemplate)(object)((value is AmmoTemplate) ? value : null);
			if (val == null || !calibers.ContainsKey(val.Caliber))
			{
				return;
			}
			calibers.TryGetValue(val.Caliber, out var value2);
			if (Random.value > value2)
			{
				if (!VisceralEntry.Instance.UseActiveRagdolls.Value || !FikaBackendUtils.IsServer)
				{
					return;
				}
				if (!VisceralEntry.Instance.OnlyPlayersCanActiveRagdollEnemies.Value)
				{
					if (!damageInfo.Player.IsAI && Vector3.Distance(damageInfo.Player.iPlayer.Position, __instance.Position) <= (float)VisceralEntry.Instance.RagdollMaxDistance.Value && RagdollHelperClass.ShouldRagdoll(bodyPartType))
					{
						FikaPlayer val2 = (FikaPlayer)(object)((__instance is FikaPlayer) ? __instance : null);
						int chance = Random.Range(0, 10);
						if (!VisceralEntry.Instance.dismemberedPlayers.Contains(__instance))
						{
							DeathSetup(__instance, bodyPartType, chance);
						}
					}
				}
				else if (Vector3.Distance(damageInfo.Player.iPlayer.Position, __instance.Position) <= (float)VisceralEntry.Instance.RagdollMaxDistance.Value && RagdollHelperClass.ShouldRagdoll(bodyPartType))
				{
					FikaPlayer val3 = (FikaPlayer)(object)((__instance is FikaPlayer) ? __instance : null);
					int chance2 = Random.Range(0, 10);
					if (!VisceralEntry.Instance.dismemberedPlayers.Contains(__instance))
					{
						DeathSetup(__instance, bodyPartType, chance2);
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
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Invalid comparison between Unknown and I4
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_045a: Unknown result type (might be due to invalid IL or missing references)
		//IL_045c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0466: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0475: Unknown result type (might be due to invalid IL or missing references)
		//IL_047a: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		//IL_048e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0498: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		affectedLimbs = (from t in VisceralCombat.Ragdolls.Classes.Utils.EnumerateHierarchyCore(player.Transform.Original)
			where ((Object)t).name.ToLower().Contains(bone) && !VisceralCombat.Dismemberment.Classes.Utils.ParentContains(t, "weapon_holster")
			select t).ToArray();
		Transform[] array = affectedLimbs;
		foreach (Transform val in array)
		{
			if (val.localScale == RagdollHelperClass.limbSize)
			{
				continue;
			}
			if (FikaBackendUtils.IsServer || FikaBackendUtils.IsHeadless)
			{
				Transform[] array2 = affectedLimbs;
				foreach (Transform val2 in array2)
				{
					limbNames = HarmonyLib.CollectionExtensions.AddItem<string>(limbNames, val2.name).ToArray();
				}
				FikaPlayer val3 = (FikaPlayer)(object)((player is FikaPlayer) ? player : null);
				DismembermentPacket dismembermentPacket = default(DismembermentPacket);
				dismembermentPacket.playerID = val3.NetId;
				dismembermentPacket.Direction = Direction;
				dismembermentPacket.bodyPartType = bodyPartType;
				dismembermentPacket.bone = bone;
				dismembermentPacket.capAssetName = capAssetName;
				dismembermentPacket.assetNames = assetNames;
				DismembermentPacket dismembermentPacket2 = dismembermentPacket;
				QuickLogger.Log(ELogType.Log, string.Format("Dismemberment Packet Sent: {0}, {1}, {2}, {3}, {4}, {5}", dismembermentPacket2.playerID, dismembermentPacket2.Direction, dismembermentPacket2.bodyPartType, dismembermentPacket2.bone, dismembermentPacket2.capAssetName, string.Join(",", dismembermentPacket2.assetNames)));
				Singleton<FikaServer>.Instance.SendData<DismembermentPacket>(ref dismembermentPacket2, (DeliveryMethod)0, false);
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
			GameObject val9 = GameObject.CreatePrimitive((PrimitiveType)0);
			val9.GetComponent<SphereCollider>().radius = 0.1f;
			Object.Destroy((Object)(object)val9.GetComponent<Renderer>());
			Transform transform = ((Component)CameraClass.Instance.Camera).transform;
			Vector3 position = transform.position;
			Vector3 forward = transform.forward;
			float num = 1f;
			val9.transform.position = position - forward * num;
			transform.SetParent(val9.transform, true);
			Rigidbody val10 = val9.AddComponent<Rigidbody>();
			val10.mass = 0.1f;
			val10.drag = 0.5f;
			val10.angularDrag = 0.5f;
			Vector3 val11 = -forward * 10f + Random.insideUnitSphere * 2f;
			val10.AddForce(val11, (ForceMode)1);
			val10.AddTorque(Random.insideUnitSphere * 5f, (ForceMode)1);
			GClass855.WaitSeconds((MonoBehaviour)(object)StaticManager.Instance, 5f, (Action)delegate
			{
				if (val9 != null)
				{
					Object.Destroy((Object)(object)val9);
				}
			});
		}
	}

	internal static void SpawnOldVolumetricBlood(Transform target, Vector3 direction, float Scale)
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
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
		if (VisceralEntry.Instance.UseOldBloodDecal.Value)
		{
			component.GroundHeight = target.position.y - 1.9f;
		}
		else
		{
			component.GroundHeight = -9999999f;
		}
		component.ClampDecalSideSurface = true;
		brainObject.transform.position = target.position;
		Transform transform = brainObject.transform;
		Quaternion rotation = target.rotation;
		transform.rotation = Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);
		GClass855.WaitSeconds((MonoBehaviour)(object)StaticManager.Instance, 5f, (Action)delegate
		{
			Object.Destroy((Object)(object)brainObject);
		});
	}

	internal static void SpawnArterialSprays(Transform target, Vector3 direction)
	{
		GameObject bloodParticleObject = null;
		if (GoreObjectPool.Instance != null)
		{
			bloodParticleObject = GoreObjectPool.Instance.Spawn(VisceralEntry.Instance.effectContainer.limbSquirter, Vector3.zero, Quaternion.identity);
		}
		else
		{
			bloodParticleObject = Object.Instantiate<GameObject>(VisceralEntry.Instance.effectContainer.limbSquirter);
		}

		if (bloodParticleObject.GetComponent<ParticleFloorPainter>() == null)
		{
			bloodParticleObject.AddComponent<ParticleFloorPainter>();
		}

		if (((Component)target).transform.localScale == Vector3.zero)
		{
			bloodParticleObject.transform.parent = ((Component)target).transform.parent;
		}
		else
		{
			bloodParticleObject.transform.parent = ((Component)target).transform;
		}
		bloodParticleObject.transform.localPosition = Vector3.zero;
		bloodParticleObject.transform.localRotation = new Quaternion(-0.0923f, 0.7011f, -0.0923f, -0.7011f);
		ParticleSystem[] componentsInChildren = bloodParticleObject.GetComponentsInChildren<ParticleSystem>();
		float num = UnityEngine.Random.Range(VisceralEntry.Instance.ArterySprayMin.Value, VisceralEntry.Instance.ArterySprayMax.Value);
		ParticleSystem[] array = componentsInChildren;
		foreach (ParticleSystem val in array)
		{
			ParticleSystem.MainModule main = val.main;
			main.loop = false;
			main.duration = num;
			ParticleSystem.CollisionModule collision = val.collision;
			collision.sendCollisionMessages = true;
			if (((Component)val).gameObject.GetComponent<ParticleFloorPainter>() == null)
			{
				((Component)val).gameObject.AddComponent<ParticleFloorPainter>();
			}
			val.Play();
		}
		GClass855.WaitSeconds((MonoBehaviour)(object)StaticManager.Instance, num + 1f, (Action)delegate
		{
			if (bloodParticleObject != null && Singleton<GameWorld>.Instantiated)
			{
				if (GoreObjectPool.Instance != null)
				{
					GoreObjectPool.Instance.Recycle(bloodParticleObject);
				}
				else
				{
					Object.Destroy((Object)(object)bloodParticleObject);
				}
			}
		});
	}

	public static void DeathSetup(Player p, EBodyPart eBodyPart, int Chance)
	{
		if (FikaBackendUtils.IsServer)
		{
			Player obj = p;
			FikaPlayer val = (FikaPlayer)(object)((obj is FikaPlayer) ? obj : null);
			RagdollSyncPacket ragdollSyncPacket = default(RagdollSyncPacket);
			ragdollSyncPacket.PlayerID = val.NetId;
			ragdollSyncPacket.BodyPart = eBodyPart;
			RagdollSyncPacket ragdollSyncPacket2 = ragdollSyncPacket;
			QuickLogger.Log(ELogType.Log, $"Ragdoll Packet Sent: {ragdollSyncPacket2.PlayerID}, {ragdollSyncPacket2.BodyPart}, {ragdollSyncPacket2.RandomChance}");
			Singleton<FikaServer>.Instance.SendData<RagdollSyncPacket>(ref ragdollSyncPacket2, (DeliveryMethod)0, false);
		}

		RagdollHelperClass.limbsToCheck.Clear();
		if ((Object)(object)p == (Object)null || (Object)(object)((Component)p).gameObject == (Object)null || (Object)(object)((Component)p).gameObject.transform == (Object)null)
		{
			return;
		}

		PuppetMaster componentInChildren = ((Component)((Component)p).gameObject.transform.parent).GetComponentInChildren<PuppetMaster>();
		if ((Object)(object)componentInChildren == (Object)null)
		{
			QuickLogger.Log(ELogType.Error, "No PuppetMaster found in player's hierarchy!");
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

		// CR-01-03: Destruição do AnimatorOverrideController anterior para evitar vazamento de memória nativa
		if (p.BodyAnimatorCommon.runtimeAnimatorController is AnimatorOverrideController oldOverride)
		{
			UnityEngine.Object.Destroy(oldOverride);
		}

		AnimatorOverrideController runtimeAnimatorController = new AnimatorOverrideController(p.BodyAnimatorCommon.runtimeAnimatorController);
		p.BodyAnimatorCommon.runtimeAnimatorController = (RuntimeAnimatorController)(object)runtimeAnimatorController;
		RagdollHelperClass.PlayDeathAnimation(p, componentInChildren, eBodyPart);

		if (p.PlayerBones?.Pelvis?.Original != null)
		{
			TransformHelperClass.SetLayersRecursively(((Component)((Component)p.PlayerBones.Pelvis.Original).transform).gameObject, LayerMask.NameToLayer("Deadbody"));
		}

		if (p.PlayerBones != null)
		{
			if (p.PlayerBones.HolsterPrimary != null) ((Component)p.PlayerBones.HolsterPrimary).gameObject.SetActive(false);
			if (p.PlayerBones.HolsterSecondary != null) ((Component)p.PlayerBones.HolsterSecondary).gameObject.SetActive(false);
			if (p.PlayerBones.HolsterPrimaryAlternative != null) ((Component)p.PlayerBones.HolsterPrimaryAlternative).gameObject.SetActive(false);
			if (p.PlayerBones.HolsterSecondaryAlternative != null) ((Component)p.PlayerBones.HolsterSecondaryAlternative).gameObject.SetActive(false);
			if (p.PlayerBones.HolsterPistol != null) ((Component)p.PlayerBones.HolsterPistol).gameObject.SetActive(false);
			if (p.PlayerBones.LeftLegHolsterPistol != null) ((Component)p.PlayerBones.LeftLegHolsterPistol).gameObject.SetActive(false);
		}

		componentInChildren.Teleport(((Component)p).gameObject.transform.position, Quaternion.LookRotation(p.LookDirection), moveToTarget: true);

		float lerpDuration = (VisceralEntry.Instance != null && VisceralEntry.Instance.MappingWeightDuration != null) 
			? VisceralEntry.Instance.MappingWeightDuration.Value 
			: 0.8f;
		((MonoBehaviour)p).StartCoroutine(RagdollHelperClass.LerpMappingWeight(componentInChildren, 0f, 1f, lerpDuration));

		componentInChildren.state = PuppetMaster.State.Dead;
		GClass855.WaitSeconds((MonoBehaviour)(object)StaticManager.Instance, 0.1f, (Action)delegate
		{
			if (p != null && p.BodyAnimatorCommon != null && Singleton<GameWorld>.Instantiated)
			{
				p.BodyAnimatorCommon.enabled = true;
			}
		});

		// Ragdoll Sleep System: desativa PuppetMaster após o tempo configurado (default: 3.5s) para liberar CPU/physics
		if (VisceralEntry.Instance != null && VisceralEntry.Instance.DisableRagdollsAfterTime != null && VisceralEntry.Instance.DisableRagdollsAfterTime.Value)
		{
			float sleepDelay = (VisceralEntry.Instance.RagdollDisableTime != null) ? VisceralEntry.Instance.RagdollDisableTime.Value : 3.5f;
			GClass855.WaitSeconds((MonoBehaviour)(object)StaticManager.Instance, sleepDelay, (Action)delegate
			{
				if (componentInChildren != null && ((Component)componentInChildren).gameObject != null && Singleton<GameWorld>.Instantiated)
				{
					componentInChildren.mode = PuppetMaster.Mode.Kinematic;
					((Behaviour)componentInChildren).enabled = false;
				}
			});
		}
	}
}
