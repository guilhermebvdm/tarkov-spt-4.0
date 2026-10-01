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
	// ref: backlog 004 — reformulação da chance de desmembramento por calibre e parte do
	// corpo. Substitui o Dictionary<string, float> plano (1 chance por calibre, igual pra
	// qualquer parte do corpo) por 3 mecanismos com precedência C -> A -> B -> 0:
	//   (C) caliberExceptions — exceção por munição individual (ammo.Name), pra casos de
	//       projétil único que não devem herdar o valor genérico do calibre (ex.: slug de
	//       23x75 "Barrikada" vs o flashbang "Zvezda" do mesmo calibre).
	//   (A) multi-projétil dinâmico — se a munição tiver mais de 1 projétil (chumbo/buckshot),
	//       soma o momento (N.s) de cada pellet que acerta a mesma parte do corpo no mesmo
	//       disparo (agrupado por FireIndex, mesmo padrão de _evaluatedLivingVolleys abaixo)
	//       e converte pra uma chance 0-1 via interpolação linear entre 2 limiares.
	//   (B) calibers — tabela por calibre (o antigo comportamento), agora com 4 valores por
	//       calibre (braço/perna/cabeça-arranca/cabeça-estourar) em vez de 1.
	public struct DismemberChances
	{
		public float Arm;
		public float Leg;
		public float HeadOff;
		public float HeadBurst;
	}

	public static Dictionary<string, DismemberChances> calibers = new Dictionary<string, DismemberChances>();
	public static Dictionary<string, DismemberChances> caliberExceptions = new Dictionary<string, DismemberChances>();

	// Lidos de VD_Calibers.json ("dismember_multiprojectile_curve") via ParseDismembermentJson.
	// Defaults abaixo só valem se a chave não existir no JSON.
	public static float MultiProjectileMomentumMin = 3.0f;
	public static float MultiProjectileMomentumMax = 15.0f;
	public static float HeadBurstMultiplier = 1.5f;

	// Acumulador do mecanismo (A) — chave: (player.Id, FireIndex, bodyPart). Todos os pellets
	// de um disparo de espingarda compartilham o mesmo FireIndex (ref: Assembly-CSharp/
	// EFT.Ballistics/BallisticsCalculator.cs:163-167), então cada pellet processado soma seu
	// momento aqui até cruzar o limiar — sem precisar coordenar "quando o disparo terminou".
	private static readonly Dictionary<long, float> _multiProjectileMomentum = new Dictionary<long, float>();

	public static void ClearMultiProjectileMomentum()
	{
		_multiProjectileMomentum.Clear();
	}

	private static long MakeMomentumKey(int playerId, int fireIndex, EBodyPart bodyPart)
	{
		return ((long)playerId << 40) | ((long)fireIndex << 8) | (byte)bodyPart;
	}

	/// <summary>Remove o prefixo "Caliber" se presente — AmmoTemplate.Caliber mantém o
	/// prefixo ("Caliber556x45NATO"), mas AmmoItemClass.Caliber já vem sem ele
	/// ("556x45NATO", ver AmmoItemClass.cs:32). Normalizar aqui garante que os dois
	/// caminhos (KillPatch.Postfix usa AmmoTemplate; LimbKillPatch usa AmmoItemClass)
	/// batam com a mesma chave no dicionário.</summary>
	internal static string NormalizeCaliber(string caliber)
	{
		if (string.IsNullOrEmpty(caliber)) return caliber;
		return caliber.StartsWith("Caliber") ? caliber.Substring(7) : caliber;
	}

	/// <summary>Soma o momento (N.s) de um pellet no acumulador e retorna o total até agora.
	/// Chamar EXATAMENTE UMA VEZ por (pellet, parte do corpo) — para cabeça, ResolveHeadOutcome
	/// já garante isso lendo o valor uma única vez e derivando as duas chances dele.</summary>
	private static float AccumulateMultiProjectileMomentum(float bulletMassGram, float initialSpeed, int playerId, int fireIndex, EBodyPart bodyPart)
	{
		float massKg = (bulletMassGram > 0f) ? (bulletMassGram / 1000f) : 0f;
		float speed = (initialSpeed > 0f) ? initialSpeed : 0f;
		float pelletMomentum = massKg * speed;

		long key = MakeMomentumKey(playerId, fireIndex, bodyPart);
		float accumulated = _multiProjectileMomentum.TryGetValue(key, out float existing) ? existing + pelletMomentum : pelletMomentum;
		_multiProjectileMomentum[key] = accumulated;

		if (_multiProjectileMomentum.Count > 2000) _multiProjectileMomentum.Clear(); // guarda de segurança, mesmo padrão de _evaluatedLivingVolleys

		return accumulated;
	}

	private static float MomentumToChance(float accumulated)
	{
		if (accumulated <= MultiProjectileMomentumMin) return 0f;
		if (accumulated >= MultiProjectileMomentumMax) return 1f;
		return (accumulated - MultiProjectileMomentumMin) / (MultiProjectileMomentumMax - MultiProjectileMomentumMin);
	}

	private static float SelectByBodyPart(DismemberChances c, EBodyPart bodyPart)
	{
		switch ((int)bodyPart)
		{
			case 3:
			case 4:
				return c.Arm;
			case 5:
			case 6:
				return c.Leg;
			default:
				return c.HeadOff; // Head (0) tratado à parte por ResolveHeadOutcome — nunca chega aqui na prática
		}
	}

	/// <summary>Resolve a chance de desmembrar braço/perna. NÃO usar para cabeça — ver
	/// ResolveHeadOutcome (cabeça tem 2 rolagens independentes: arranca/estourar).</summary>
	internal static float ResolveDismemberChance(string caliber, string ammoName, int projectileCount, float bulletMassGram, float initialSpeed, EBodyPart bodyPart, int playerId, int fireIndex)
	{
		// (C) Exceção por munição — checada primeiro, nome exato do template.
		if (!string.IsNullOrEmpty(ammoName) && caliberExceptions.TryGetValue(ammoName, out DismemberChances exc))
		{
			return SelectByBodyPart(exc, bodyPart);
		}

		// (A) Multi-projétil dinâmico — soma o momento dos pellets do mesmo disparo/parte do corpo.
		if (projectileCount > 1)
		{
			float accumulated = AccumulateMultiProjectileMomentum(bulletMassGram, initialSpeed, playerId, fireIndex, bodyPart);
			return MomentumToChance(accumulated);
		}

		// (B) Tabela por calibre — fallback pra projétil único sem exceção.
		string normalizedCaliber = NormalizeCaliber(caliber);
		if (!string.IsNullOrEmpty(normalizedCaliber) && calibers.TryGetValue(normalizedCaliber, out DismemberChances cal))
		{
			return SelectByBodyPart(cal, bodyPart);
		}

		return 0f; // fallback final: nunca um default alto (corrige o 0.5f implícito de antes)
	}

	/// <summary>Decisão de cabeça computada cedo por VisceralCombat.Combined.Patches.
	/// DeathInventoryDropPatch (Prefix em ActiveHealthController.method_35 — roda ANTES do Fika
	/// serializar o Equipment pro pacote de sync do cadáver, ver comentário na classe daquele
	/// arquivo) e consumida uma única vez aqui no Postfix pra aplicar o efeito visual. Nunca rolar
	/// de novo aqui: dobraria o acúmulo de momento no caso multi-projétil (AccumulateMultiProjectileMomentum
	/// soma o pellet a cada chamada). Chave: player.Id; removida assim que lida.</summary>
	internal static readonly Dictionary<int, HeadDismemberOutcome> PendingHeadOutcome = new Dictionary<int, HeadDismemberOutcome>();

	// ref: CR-02-05 - Limpeza ao iniciar nova raid para prevenir resíduos órfãos em memória
	internal static void ClearPendingHeadOutcomes()
	{
		PendingHeadOutcome.Clear();
	}

	/// <summary>Resolve o AmmoTemplate da munição que causou o dano, a partir de damageInfo.SourceId.
	/// Extraído do Postfix pra ser reaproveitado por DeathInventoryDropPatch (que precisa da mesma
	/// resolução, mas rodando bem mais cedo, em ActiveHealthController.method_35).</summary>
	internal static AmmoTemplate ResolveAmmoTemplate(DamageInfoStruct damageInfo)
	{
		if (string.IsNullOrEmpty(damageInfo.SourceId) || !Singleton<ItemFactoryClass>.Instantiated || Singleton<ItemFactoryClass>.Instance.ItemTemplates == null)
		{
			return null;
		}
		if (((Dictionary<MongoID, ItemTemplate>)(object)Singleton<ItemFactoryClass>.Instance.ItemTemplates).TryGetValue((MongoID)damageInfo.SourceId, out ItemTemplate itemTemplate) && itemTemplate is AmmoTemplate ammoTemplate)
		{
			return ammoTemplate;
		}
		return null;
	}

	public enum HeadDismemberOutcome { None, HeadOff, HeadBurst }

	/// <summary>Cabeça é uma decisão de 2 rolagens independentes: primeiro "arranca" (Head_3),
	/// se não vencer tenta "estourar" (Head_1/2). Os dois usam o MESMO pipeline de
	/// DismemberLimb (esconde a cabeça original via escala, comprovado em jogo) — só o
	/// capAssetName muda (ver KillPatch.Postfix case 0 / LimbKillPatch.ProcessLimbKill).
	/// No caso multi-projétil, o momento do pellet é somado UMA ÚNICA VEZ
	/// (AccumulateMultiProjectileMomentum) e as duas chances derivam da mesma leitura —
	/// nunca soma o mesmo pellet duas vezes.</summary>
	internal static HeadDismemberOutcome ResolveHeadOutcome(string caliber, string ammoName, int projectileCount, float bulletMassGram, float initialSpeed, int playerId, int fireIndex)
	{
		float offChance;
		float burstChance;

		if (!string.IsNullOrEmpty(ammoName) && caliberExceptions.TryGetValue(ammoName, out DismemberChances exc))
		{
			offChance = exc.HeadOff;
			burstChance = exc.HeadBurst;
		}
		else if (projectileCount > 1)
		{
			float accumulated = AccumulateMultiProjectileMomentum(bulletMassGram, initialSpeed, playerId, fireIndex, EBodyPart.Head); // UMA chamada só
			offChance = MomentumToChance(accumulated);
			burstChance = Mathf.Min(1f, offChance * HeadBurstMultiplier); // estourar satura mais cedo que arrancar
		}
		else
		{
			string normalizedCaliber = NormalizeCaliber(caliber);
			if (!string.IsNullOrEmpty(normalizedCaliber) && calibers.TryGetValue(normalizedCaliber, out DismemberChances cal))
			{
				offChance = cal.HeadOff;
				burstChance = cal.HeadBurst;
			}
			else
			{
				return HeadDismemberOutcome.None;
			}
		}

		if (Random.value <= offChance) return HeadDismemberOutcome.HeadOff;
		if (Random.value <= burstChance) return HeadDismemberOutcome.HeadBurst;
		return HeadDismemberOutcome.None;
	}

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
		if (__instance.HealthController == null || __instance.HealthController.IsAlive || RagdollHelperClass.IsPlayerDowned(__instance))
		{
			return;
		}
		bool isFirstDeath = !deadPlayers.ContainsKey(__instance);
		if (isFirstDeath)
		{
			deadPlayers.Add(__instance, 0);
		}

		AmmoTemplate currentAmmoTemplate = ResolveAmmoTemplate(damageInfo);
		string caliber = currentAmmoTemplate?.Caliber;

		// ref: backlog 004 — chance por parte do corpo (braço/perna/cabeça-arranca/
		// cabeça-estourar) via ResolveDismemberChance/ResolveHeadOutcome, em vez do valor
		// único por calibre de antes. Cabeça é resolvida à parte (2 rolagens independentes,
		// ver HeadDismemberOutcome); o resultado vira um "dismemberChance" 0 ou 1 só pra
		// reaproveitar o mesmo gate de baixo (linha "Random.value > dismemberChance") sem
		// duplicar a lógica de agonia/ragdoll que já existe ali.
		string ammoName = currentAmmoTemplate?.Name;
		int projectileCount = currentAmmoTemplate?.ProjectileCount ?? 1;
		float bulletMassGram = currentAmmoTemplate?.BulletMassGram ?? 0f;
		float initialSpeed = currentAmmoTemplate?.InitialSpeed ?? 0f;

		HeadDismemberOutcome headOutcome = HeadDismemberOutcome.None;
		float dismemberChance;
		if ((int)bodyPartType == 0)
		{
			// ref: VisceralCombat.Combined.Patches.DeathInventoryDropPatch — a decisão já foi
			// computada bem mais cedo (ActiveHealthController.method_35, ANTES do Fika serializar
			// o Equipment pro pacote de sync do cadáver) pra poder derrubar capacete/óculos a
			// tempo. NÃO rolar de novo aqui: dobraria o acúmulo de momento multi-projétil.
			if (PendingHeadOutcome.TryGetValue(__instance.Id, out HeadDismemberOutcome cachedOutcome))
			{
				headOutcome = cachedOutcome;
				PendingHeadOutcome.Remove(__instance.Id);
			}
			dismemberChance = (headOutcome != HeadDismemberOutcome.None) ? 1f : 0f;
		}
		else
		{
			dismemberChance = ResolveDismemberChance(caliber, ammoName, projectileCount, bulletMassGram, initialSpeed, bodyPartType, __instance.Id, damageInfo.FireIndex);
		}

		bool isHeavyNoAgony = IsHeavyCaliberNoAgony(caliber, currentAmmoTemplate);

		if ((int)damageInfo.DamageType != 2048 && (int)damageInfo.DamageType != 4 && (int)damageInfo.DamageType != 32 && (int)damageInfo.DamageType != 8 && (int)damageInfo.DamageType != 16 && (int)damageInfo.DamageType != 8192)
		{
			if (Random.value > dismemberChance)
			{
				bool triggeredActiveRagdoll = false;
				if (isHeavyNoAgony)
				{
					// Heavy caliber fatal kill: block agony animation so the corpse reacts pure physically to shot impulse
					PuppetMaster pm = __instance.gameObject.GetComponentInChildren<PuppetMaster>(true);
					if (pm != null)
					{
						RagdollHelperClass.InterruptAgony(__instance, pm, forceInstant: true);
						triggeredActiveRagdoll = true;
					}
					else if (__instance.BodyAnimatorCommon != null)
					{
						__instance.BodyAnimatorCommon.enabled = false;
					}
				}
				else if (isFirstDeath && VisceralEntry.Instance.IsCategoryActive(VisceralEntry.Instance.UseActiveRagdolls) && (FikaBackendUtils.IsServer || FikaBackendUtils.IsSinglePlayer)) // ref: item 005
				{
					if (!VisceralEntry.Instance.OnlyPlayersCanActiveRagdollEnemies.Value || !damageInfo.Player.IsAI)
					{
						if (Vector3.Distance(damageInfo.Player.iPlayer.Position, __instance.Position) <= (float)VisceralEntry.Instance.RagdollMaxDistance.Value && RagdollHelperClass.ShouldRagdoll(bodyPartType))
						{
							int chance2 = Random.Range(0, 10);
							if (!VisceralEntry.Instance.dismemberedPlayers.Contains(__instance))
							{
								DeathSetup(__instance, bodyPartType, chance2);
								triggeredActiveRagdoll = true;
							}
						}
					}
				}

				// ref: Fallback defensivo para quando o bot morre fora do alcance de agonia (RagdollMaxDistance),
				// por tiro de outro bot, ou por causa que não se qualifica para DeathSetup.
				// Desativa os músculos do PuppetMaster e o Animator para que o cadáver não fique "congelado em pé"
				// sustentado pelos pesos musculares ativos do PuppetMaster que foi anexado no spawn.
				if (!triggeredActiveRagdoll)
				{
					PuppetMaster pm = __instance.gameObject.GetComponentInChildren<PuppetMaster>(true);
					if (pm != null)
					{
						pm.state = PuppetMaster.State.Dead;
						pm.muscleWeight = 0f;
						pm.pinWeight = 0f;
						((Behaviour)pm).enabled = false;
					}
					if (__instance.BodyAnimatorCommon != null)
					{
						__instance.BodyAnimatorCommon.enabled = false;
					}
				}
				return;
			}
		}

		// ref: item 005 — toggle mestre
		if (!VisceralEntry.Instance.IsCategoryActive(VisceralEntry.Instance.EnableDismemberment))
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
				// ref: backlog 004 — headOutcome já foi decidido acima (ResolveHeadOutcome),
				// não rola de novo aqui. Correção pós-teste em jogo (CR-HEAD-DUP-01): "estourar"
				// usava um método separado (BurstHead) que não encolhia a cabeça original,
				// resultando em 2 cabeças visíveis no mesmo corpo (a real + o prop). Decisão do
				// usuário: "estourar" reusa o MESMO pipeline de DismemberLimb que já esconde a
				// cabeça original corretamente (comprovado no "arranca") — só troca qual prop
				// aparece no lugar (Head_3 = coto sem cabeça; Head_1/2 = cabeça caída/estourada).
				if (headOutcome == HeadDismemberOutcome.HeadOff)
				{
					DismemberLimb(__instance, damageInfo.Direction, bodyPartType, value3, "Head_3", Array.Empty<string>(), out affectedLimbs);
				}
				else if (headOutcome == HeadDismemberOutcome.HeadBurst)
				{
					DismemberLimb(__instance, damageInfo.Direction, bodyPartType, value3, $"Head_{Random.Range(1, 3)}", Array.Empty<string>(), out affectedLimbs);
				}
				break;
			}

			// After dismembering an arm or leg, trigger agony animation on the remaining body.
			// Head (case 0) is intentionally excluded — agony with a 0.001f skull bone is not safe.
			bool isLimbDismember = (int)bodyPartType == 3 || (int)bodyPartType == 4
			                    || (int)bodyPartType == 5 || (int)bodyPartType == 6;
			if (isLimbDismember
			    && !isHeavyNoAgony
			    && isFirstDeath
			    && VisceralEntry.Instance.IsCategoryActive(VisceralEntry.Instance.UseActiveRagdolls) // ref: item 005
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

	internal static void DismemberLimb(Player player, Vector3 Direction, EBodyPart bodyPartType, string bone, string capAssetName, string[] assetNames, out Transform[] affectedLimbs, bool isFromNetwork = false)
	{
		affectedLimbs = null;
		if (player == null || RagdollHelperClass.IsPlayerDowned(player))
		{
			return;
		}

		if (!isFromNetwork)
		{
			VisceralCombat.Combined.Classes.VisceralNetworkUtils.SendDismemberment(player, Direction, bodyPartType, bone, capAssetName, assetNames);
		}

		bool isDeadPlayer = (player == null || player.HealthController == null || !player.HealthController.IsAlive);
		if (isDeadPlayer && player != null && player.BodyAnimatorCommon != null)
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

			Transform[] allBranchTransforms = val.GetComponentsInChildren<Transform>(true);
			foreach (Transform tBranch in allBranchTransforms)
			{
				if (tBranch != null)
				{
					// Unparent any attached blood effect gameobjects so they aren't crushed by 0.0001f scale
					List<Transform> directChildrenToReparent = new List<Transform>();
					foreach (Transform child in tBranch)
					{
						if (child != null && child != tBranch && child.GetComponentInChildren<ParticleSystem>() != null)
						{
							directChildrenToReparent.Add(child);
						}
					}
					foreach (Transform child in directChildrenToReparent)
					{
						if (tBranch.parent != null)
						{
							Vector3 worldPos = child.position;
							Quaternion worldRot = child.rotation;
							child.SetParent(tBranch.parent, false);
							child.position = worldPos;
							child.rotation = worldRot;
							child.localScale = Vector3.one;
						}
					}

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

			// Disable all Colliders on the dismembered limb so it never acts as a floating obstacle in the air
			Collider[] limbColliders = val.GetComponentsInChildren<Collider>(true);
			foreach (Collider col in limbColliders)
			{
				if (col != null)
				{
					col.enabled = false;
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
							// Reparent any attached blood particle systems on ragdoll bone before scaling
							List<Transform> rbChildrenToReparent = new List<Transform>();
							foreach (Transform rbChild in m.rigidbody.transform)
							{
								if (rbChild != null && rbChild != m.rigidbody.transform && rbChild.GetComponentInChildren<ParticleSystem>() != null)
								{
									rbChildrenToReparent.Add(rbChild);
								}
							}
							foreach (Transform rbChild in rbChildrenToReparent)
							{
								if (m.rigidbody.transform.parent != null)
								{
									Vector3 wPos = rbChild.position;
									Quaternion wRot = rbChild.rotation;
									rbChild.SetParent(m.rigidbody.transform.parent, false);
									rbChild.position = wPos;
									rbChild.rotation = wRot;
									rbChild.localScale = Vector3.one;
								}
							}

							m.rigidbody.transform.localScale = RagdollHelperClass.limbSize;
							if (m.rigidbody.transform.gameObject.GetComponent<DismemberedLimbScaler>() == null)
							{
								m.rigidbody.transform.gameObject.AddComponent<DismemberedLimbScaler>();
							}
							m.rigidbody.isKinematic = true;
							m.rigidbody.detectCollisions = false;

							Collider[] mCols = m.rigidbody.GetComponentsInChildren<Collider>(true);
							foreach (Collider mc in mCols)
							{
								if (mc != null) mc.enabled = false;
							}
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
					// Compat sem dependência com mods de limpeza/otimização de cadáver (ex.:
					// TRL-DynamicSpawn/CorpseCleanupManager): tanto o "converter em mochila"
					// (esconde tudo via Renderer.forceRenderingOff em player.GetComponentsInChildren<Renderer>())
					// quanto o "despawn total" (Destroy(player.gameObject)) só enxergam o que está
					// DENTRO da hierarquia do Player. Sem isso, o prop ficava solto na cena — invisível
					// pro outro mod, mas ainda renderizando (o "pescoço/cabeça flutuando no ar" relatado).
					// SetParent com worldPositionStays:true não muda a pose atual (Skin.Init já vinculou
					// os bones certos antes disso, independente do parent do Transform).
					val5.transform.SetParent(player.gameObject.transform, true);
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
					// ref: mesmo motivo do val5 acima — sem isso este pedaço (braço/perna/cabeça
					// avulsa) fica de fora da hierarquia do Player e nenhum mod de limpeza de
					// cadáver (nem o próprio VisceralCombat) consegue escondê-lo/destruí-lo junto.
					val7.transform.SetParent(player.gameObject.transform, true);

					int deadbodyLayer = LayerMask.NameToLayer("Deadbody");
					if (deadbodyLayer >= 0)
					{
						val7.layer = deadbodyLayer;
						foreach (Transform child in val7.transform)
						{
							child.gameObject.layer = deadbodyLayer;
						}
					}

					// Ignore collision between severed 3D limb and all player body colliders to prevent depenetration snags
					Collider[] severedCols = val7.GetComponentsInChildren<Collider>(true);
					Collider[] playerCols = player.GetComponentsInChildren<Collider>(true);
					foreach (Collider sc in severedCols)
					{
						if (sc == null) continue;
						foreach (Collider pc in playerCols)
						{
							if (pc == null) continue;
							Physics.IgnoreCollision(sc, pc, true);
						}
					}
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
			SpawnArterialSprays(player, val, Direction, bone);
		}
		// ref: VisceralCombat.Combined.Patches.DeathInventoryDropPatch — capacete/óculos já foram
		// derrubados bem mais cedo (ActiveHealthController.method_35), antes até do cadáver ser
		// criado. Nada a fazer aqui além do efeito visual (acima).
		if (player.IsYourPlayer && (int)bodyPartType == 0)
		{
			if (VisceralEntry.Instance.effectContainer != null && VisceralEntry.Instance.effectContainer.blood3dFxEffects != null && VisceralEntry.Instance.effectContainer.blood3dFxEffects.Count > 0)
			{
				VisceralEntry.Instance.effectContainer.blood3dFxEffects[0].SetActive(true);
			}
		}
	}

	public static void DeathSetup(Player p, EBodyPart eBodyPart, int Chance, bool isFromNetwork = false)
	{
		try
		{
			if ((Object)(object)p == (Object)null || (Object)(object)((Component)p).gameObject == (Object)null || RagdollHelperClass.IsPlayerDowned(p))
			{
				return;
			}

			if (!isFromNetwork)
			{
				VisceralCombat.Combined.Classes.VisceralNetworkUtils.SendRagdollSync(p, eBodyPart, Chance);
			}

			if (VisceralEntry.Instance != null && !VisceralEntry.Instance.dismemberedPlayers.Contains(p))
			{
				VisceralEntry.Instance.dismemberedPlayers.Add(p);
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

			componentInChildren.pinWeight = 0.02f;
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
			Vector3 lookDir = p.LookDirection;
			if (lookDir.sqrMagnitude < 0.001f) lookDir = Vector3.forward;
			componentInChildren.Teleport(((Component)p).gameObject.transform.position, Quaternion.LookRotation(lookDir), moveToTarget: true);
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
		if (!VisceralEntry.Instance.IsCategoryActive(VisceralEntry.Instance.EnableBloodEffects)) // ref: item 005
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
		if (direction.sqrMagnitude < 0.001f) direction = Vector3.forward;
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

	internal static Transform GetPhysicalBone(Player player, Transform animatedTarget, string boneKeyword = null)
	{
		if (player == null) return animatedTarget;

		// 1. Try resolving via PlayerBones
		if (player.PlayerBones != null && !string.IsNullOrEmpty(boneKeyword))
		{
			string kw = boneKeyword.ToLower();
			if (kw.Contains("head") || kw.Contains("neck"))
			{
				Transform t = player.PlayerBones.Head?.Original?.GetComponent<Rigidbody>() != null ? player.PlayerBones.Head.Original : player.PlayerBones.Neck;
				if (t != null) return t;
			}
			else if (kw.Contains("rib") || kw.Contains("spine"))
			{
				Transform t = player.PlayerBones.Ribcage?.Original ?? player.PlayerBones.Spine3?.Original;
				if (t != null) return t;
			}
			else if (kw.Contains("pelvis"))
			{
				Transform t = player.PlayerBones.Pelvis?.Original;
				if (t != null) return t;
			}
			else if (kw.Contains("lupperarm") || kw.Contains("lforearm") || kw.Contains("larm"))
			{
				Transform t = player.PlayerBones.LeftShoulder?.Original ?? (player.PlayerBones.Upperarms != null && player.PlayerBones.Upperarms.Length > 0 ? player.PlayerBones.Upperarms[0] : null);
				if (t != null) return t;
			}
			else if (kw.Contains("rupperarm") || kw.Contains("rforearm") || kw.Contains("rarm"))
			{
				Transform t = player.PlayerBones.RightShoulder?.Original ?? (player.PlayerBones.Upperarms != null && player.PlayerBones.Upperarms.Length > 1 ? player.PlayerBones.Upperarms[1] : null);
				if (t != null) return t;
			}
			else if (kw.Contains("lthigh") || kw.Contains("lcalf") || kw.Contains("lleg"))
			{
				Transform t = player.PlayerBones.LeftThigh1?.Original ?? player.PlayerBones.LeftThigh2?.Original;
				if (t != null) return t;
			}
			else if (kw.Contains("rthigh") || kw.Contains("rcalf") || kw.Contains("rleg"))
			{
				Transform t = player.PlayerBones.RightThigh1?.Original ?? player.PlayerBones.RightThigh2?.Original;
				if (t != null) return t;
			}
		}

		// 2. Search all physical Rigidbodies on PlayerBody
		if (player.PlayerBody != null)
		{
			Rigidbody[] rbs = ((Component)player.PlayerBody).gameObject.GetComponentsInChildren<Rigidbody>(true);
			if (rbs != null && rbs.Length > 0)
			{
				if (!string.IsNullOrEmpty(boneKeyword))
				{
					string kw = boneKeyword.ToLower();
					foreach (Rigidbody rb in rbs)
					{
						if (rb == null) continue;
						if (rb.gameObject.name.ToLower().Contains(kw))
						{
							return rb.transform;
						}
					}
				}

				// 3. Fallback: closest physical rigidbody to animatedTarget position
				if (animatedTarget != null)
				{
					Rigidbody closest = null;
					float minDist = float.MaxValue;
					foreach (Rigidbody rb in rbs)
					{
						if (rb == null) continue;
						float d = Vector3.SqrMagnitude(rb.position - animatedTarget.position);
						if (d < minDist)
						{
							minDist = d;
							closest = rb;
						}
					}
					if (closest != null) return closest.transform;
				}
			}
		}

		return animatedTarget;
	}

	private static void SpawnArterialSprays(Player player, Transform target, Vector3 direction, string boneKeyword = null)
	{
		if (!VisceralEntry.Instance.ArterySpray.Value || !VisceralEntry.Instance.IsCategoryActive(VisceralEntry.Instance.EnableBloodEffects)) // ref: item 005
			return;
		if (VisceralEntry.Instance.effectContainer == null || (Object)(object)VisceralEntry.Instance.effectContainer.limbSquirter == (Object)null)
			return;

		Transform physBone = GetPhysicalBone(player, target, boneKeyword);
		Transform attachTransform = physBone ?? target;

		GameObject bloodParticleObject = Object.Instantiate<GameObject>(VisceralEntry.Instance.effectContainer.limbSquirter);
		bloodParticleObject.AddComponent<ParticleFloorPainter>();

		// Parent directly to moving physical bone so emitter follows ragdoll from Point A to Point B
		bloodParticleObject.transform.SetParent(attachTransform, false);
		bloodParticleObject.transform.localPosition = Vector3.zero;
		bloodParticleObject.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
		bloodParticleObject.transform.localScale = Vector3.one;

		ParticleSystem[] componentsInChildren = bloodParticleObject.GetComponentsInChildren<ParticleSystem>();
		float num = Random.Range(VisceralEntry.Instance.ArterySprayMin.Value, VisceralEntry.Instance.ArterySprayMax.Value);
		ParticleSystem[] array = componentsInChildren;
		foreach (ParticleSystem val in array)
		{
			val.loop = false;
			var main = val.main;
			main.duration = num;
			main.simulationSpace = ParticleSystemSimulationSpace.World;
			RagdollHelperClass.ConfigureBloodParticleCollision(val);
			((Component)val).gameObject.AddComponent<ParticleFloorPainter>();
			RagdollHelperClass.ApplyDarkCoagulatedBloodFx(val);
			val.Play();
		}
		GClass855.WaitSeconds((MonoBehaviour)(object)StaticManager.Instance, num + 1f, (Action)delegate
		{
			Object.Destroy((Object)(object)bloodParticleObject);
		});
	}

	private static bool IsHeavyCaliberNoAgony(string caliber, AmmoTemplate ammo)
	{
		if (ammo != null)
		{
			float massKg = (ammo.BulletMassGram > 0f) ? (ammo.BulletMassGram / 1000f) : 0.008f;
			float speed = (ammo.InitialSpeed > 0f) ? ammo.InitialSpeed : 400f;
			float rawMomentum = massKg * speed; // p = m * v in N.s
			if (rawMomentum >= 5.0f)
			{
				return true;
			}
		}

		if (string.IsNullOrEmpty(caliber)) return false;
		string c = caliber.StartsWith("Caliber") ? caliber.Substring(7) : caliber;

		if (c == "86x70" || c == "20g" || c == "23x75" || c == "40x46" || c == "127x108" || c == "12.7x99" || c == "127x55" || c == "30x29")
		{
			return true;
		}

		if (c == "12g" && ammo != null)
		{
			if (ammo.ProjectileCount <= 1 || ammo.BulletMassGram > 15f || (ammo.Name != null && ammo.Name.ToLower().Contains("slug")))
			{
				return true;
			}
		}

		return false;
	}
}
