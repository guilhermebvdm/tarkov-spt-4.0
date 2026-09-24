using System;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.InventoryLogic;
using Fika.Core.Main.Utils; // ref: CR-03-01 - FikaBackendUtils
using SPT.Reflection.Patching;
using UnityEngine;
using Object = UnityEngine.Object;
using VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;
using VisceralCombat.Ragdolls.Classes;

namespace VisceralCombat.Ragdolls.Patches;

public class LimbKillPatch : ModulePatch
{
	private static readonly System.Collections.Generic.HashSet<long> _evaluatedLivingVolleys = new System.Collections.Generic.HashSet<long>();

	public static void ClearLivingVolleys()
	{
		_evaluatedLivingVolleys.Clear();
	}

	protected override MethodBase GetTargetMethod()
	{
		return typeof(BallisticsCalculator).GetMethods(BindingFlags.Instance | BindingFlags.Public).First((MethodInfo m) => m.Name == "Shoot" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(EftBulletClass));
	}

	[PatchPostfix]
	private static void Postfix(EftBulletClass shot)
	{
		if (shot == null) return;
		VisceralCombat.Combined.Classes.VisceralShotProcessor.RegisterShot(shot);
	}

	public static void ProcessLimbKill(EftBulletClass shot)
	{
		if (shot == null || shot.HitCollider == null || shot.Ammo == null) return;

		Collider hitCollider = shot.HitCollider;
		Rigidbody rb = hitCollider.attachedRigidbody;
		if (rb == null) return;

		// --- 1. Resolve Player ---
		// On live bots: BodyPartCollider is present with a direct Player reference.
		// On dead ragdolls: colliders are physics bones ("Base HumanRThigh1" etc.) —
		// no BodyPartCollider attached. Use hierarchy fallback.
		BodyPartCollider bpc = hitCollider.GetComponent<BodyPartCollider>();
		if (bpc == null) bpc = hitCollider.GetComponentInParent<BodyPartCollider>();

		Player player = null;
		if (bpc?.Player is Player bpcPlayer)
		{
			player = bpcPlayer;
		}

		if (player == null)
		{
			GameObject rootGO = VisceralCombat.Dismemberment.Classes.Utils.GetRootGameObject(rb.gameObject);
			if (rootGO != null) player = rootGO.GetComponentInChildren<Player>(true);
		}

		if (player == null) player = rb.gameObject.GetComponentInParent<Player>();
		if (player == null) return;

		// Only process dead players OR living AI bots when VisceralCombat is present for all players
		bool isDead = (player.HealthController == null || !player.HealthController.IsAlive) && !RagdollHelperClass.IsPlayerDowned(player);
		if (!isDead)
		{
			if (!player.IsAI || !VisceralEntry.AllPlayersHaveVisceralCombat) return;

			// Boss e escolta nunca entram no desmembramento de perna em vivos (rastejo/agonia) —
			// só bots comuns. Pós-morte (isDead == true, mais abaixo neste mesmo método) continua
			// liberado geral, sem essa restrição.
			// ref: Assembly-CSharp/BotSettingsRepoClass.cs:555-559 — WildSpawnType.IsBossOrFollower()
			// (data-driven no próprio jogo; nenhuma lista de boss mantida pelo mod).
			WildSpawnType? role = player.Profile?.Info?.Settings?.Role;
			if (role.HasValue && role.Value.IsBossOrFollower()) return;
		}

		// --- 2. PuppetMaster: agony interruption ---
		GameObject rootForPm = VisceralCombat.Dismemberment.Classes.Utils.GetRootGameObject(rb.gameObject);
		PuppetMaster pm = rootForPm?.GetComponentInChildren<PuppetMaster>(true);

		if (pm != null && pm.muscles != null)
		{
			string rbName = rb.gameObject.name;
			foreach (Muscle muscle in pm.muscles)
			{
				if (muscle != null && muscle.name != null && muscle.name.Contains(rbName))
				{
					if (rbName.Contains("Head") && pm.mappingWeight > 0.05f)
					{
						pm.stateSettings.killDuration = 0f;
						pm.state = PuppetMaster.State.Dead;
					}
					muscle.props.muscleWeight *= 0.5f;
				}
			}

			if (pm.mappingWeight > 0.05f)
			{
				RagdollHelperClass.InterruptAgony(player, pm);
			}
		}

		// --- 3. Post-mortem dismemberment ---
		// Head and torso are intentionally excluded: "Base HumanHead" is the mesh root —
		// scaling it to 0.001f collapses the entire body model.
		if (VisceralEntry.Instance == null || !VisceralEntry.Instance.IsCategoryActive(VisceralEntry.Instance.EnableDismemberment)) return; // ref: item 005

		EBodyPart? dismemberPart = null;
		string boneName = null;
		string capAsset = null;
		string[] extraAssets = Array.Empty<string>();

		// Strategy A: typed BodyPartColliderType (live-bot shot-detection colliders)
		if (bpc != null)
		{
			switch (bpc.BodyPartColliderType)
			{
				case EBodyPartColliderType.LeftUpperArm:
				case EBodyPartColliderType.LeftForearm:
					dismemberPart = (EBodyPart)3;
					boneName = "lforearm1";
					capAsset = "Arm_LeftCap";
					extraAssets = new[] { "Arm_L_1", "Arm_L_2" };
					break;
				case EBodyPartColliderType.RightUpperArm:
				case EBodyPartColliderType.RightForearm:
					dismemberPart = (EBodyPart)4;
					boneName = "rforearm1";
					capAsset = "Arm_RightCap";
					extraAssets = new[] { "Arm_R_1", "Arm_R_2" };
					break;
				case EBodyPartColliderType.LeftThigh:
				case EBodyPartColliderType.LeftCalf:
					dismemberPart = (EBodyPart)5;
					boneName = "lthigh1";
					capAsset = "Leg_LeftCap";
					extraAssets = new[] { "gore_leg_torn01" };
					break;
				case EBodyPartColliderType.RightThigh:
				case EBodyPartColliderType.RightCalf:
					dismemberPart = (EBodyPart)6;
					boneName = "rthigh1";
					capAsset = "Leg_RightCap";
					extraAssets = new[] { "gore_leg_torn02" };
					break;
			}
		}

		// Strategy B: ragdoll physics bone names (dead corpses — EFT format: "Base Human[L/R][Part]")
		// Confirmed from log capture: "Base HumanRThigh1", "Base HumanLCalf", etc.
		if (dismemberPart == null)
		{
			string rbLow = rb.gameObject.name.ToLower();

			if (rbLow.Contains("humanlupperarm") || rbLow.Contains("humanlforearm") ||
			    rbLow.Contains("humanlarm") || rbLow.Contains("humanl_arm"))
			{
				dismemberPart = (EBodyPart)3;
				boneName = "lforearm1";
				capAsset = "Arm_LeftCap";
				extraAssets = new[] { "Arm_L_1", "Arm_L_2" };
			}
			else if (rbLow.Contains("humanrupperarm") || rbLow.Contains("humanrforearm") ||
			         rbLow.Contains("humanrarm") || rbLow.Contains("humanr_arm"))
			{
				dismemberPart = (EBodyPart)4;
				boneName = "rforearm1";
				capAsset = "Arm_RightCap";
				extraAssets = new[] { "Arm_R_1", "Arm_R_2" };
			}
			else if (rbLow.Contains("humanlthigh") || rbLow.Contains("humanlcalf") ||
			         rbLow.Contains("humanlleg"))
			{
				dismemberPart = (EBodyPart)5;
				boneName = "lthigh1";
				capAsset = "Leg_LeftCap";
				extraAssets = new[] { "gore_leg_torn01" };
			}
			else if (rbLow.Contains("humanrthigh") || rbLow.Contains("humanrcalf") ||
			         rbLow.Contains("humanrleg"))
			{
				dismemberPart = (EBodyPart)6;
				boneName = "rthigh1";
				capAsset = "Leg_RightCap";
				extraAssets = new[] { "gore_leg_torn02" };
			}
			else if (rbLow.Contains("humanhead") || rbLow.Contains("humanskull"))
			{
				// Post-mortem head detection. capAsset não é usado pra cabeça (backlog 004:
				// ResolveHeadOutcome decide o outcome, DismemberLimb decide Head_1/2/3 mais abaixo).
				dismemberPart = (EBodyPart)0;
				boneName = "head";
				extraAssets = Array.Empty<string>();
			}
		}

		if (!dismemberPart.HasValue || boneName == null) return;

		// --- Living bots/players branch ---
		// Can ONLY lose LEGS (LeftLeg=5 or RightLeg=6). Arms and Head are strictly dead-only!
		if (!isDead)
		{
			if (dismemberPart.Value != (EBodyPart)5 && dismemberPart.Value != (EBodyPart)6) return;

			// If the living bot already has a LivingDismembermentController, skip further leg dismemberment
			if (player.GetComponent<VisceralCombat.Dismemberment.Classes.LivingDismembermentController>() != null) return;

			// Buckshot protection: group all pellets from the same trigger pull/shot using (player.Id + shot.FireIndex)
			long volleyKey = ((long)player.Id << 32) | (uint)(shot.FireIndex & 0xFFFFFFFF);
			if (_evaluatedLivingVolleys.Contains(volleyKey))
			{
				return;
			}
			_evaluatedLivingVolleys.Add(volleyKey);
			if (_evaluatedLivingVolleys.Count > 1000) _evaluatedLivingVolleys.Clear();

			// Fixed 30% chance per hit/shot (counted once per shotgun volley)
			float livingChance = 0.30f;
			if (UnityEngine.Random.value <= livingChance)
			{
				Transform[] dummyLimbs;
				VisceralCombat.Combined.Patches.KillPatch.DismemberLimb(player, shot.Direction, dismemberPart.Value, boneName, capAsset, extraAssets, out dummyLimbs);
				VisceralCombat.Dismemberment.Classes.LivingDismembermentController.Attach(player, dismemberPart.Value);
				VisceralCombat.Combined.Classes.VisceralNetworkUtils.SendLivingDismemberment(player, dismemberPart.Value, shot.Direction, boneName, capAsset, extraAssets);
			}
			return;
		}

		// --- Dead corpses branch ---
		// ref: backlog 004 — chance por parte do corpo via ResolveDismemberChance/
		// ResolveHeadOutcome, em vez da tabela plana por calibre de antes. AmmoItemClass.Name
		// (herdado de Item.cs:392) é a chave de LOCALIZAÇÃO, não o nome interno — usa
		// ammo.AmmoTemplate.Name (o campo cru do template, igual a AmmoTemplate.Name em
		// KillPatch.Postfix) pra bater com as chaves de dismember_exceptions.
		string ammoName = null;
		int projectileCount = 1;
		float bulletMassGram = 0f;
		float initialSpeed = 0f;
		string caliberStr = null;
		if (shot.Ammo is AmmoItemClass ammo)
		{
			ammoName = ammo.AmmoTemplate?.Name;
			projectileCount = ammo.ProjectileCount;
			bulletMassGram = ammo.BulletMassGram;
			initialSpeed = ammo.InitialSpeed;
			caliberStr = ammo.Caliber; // já sem prefixo "Caliber" (AmmoItemClass.cs:32) — ResolveDismemberChance normaliza de qualquer forma
		}

		if (dismemberPart.Value == (EBodyPart)0)
		{
			VisceralCombat.Combined.Patches.KillPatch.HeadDismemberOutcome headOutcome =
				VisceralCombat.Combined.Patches.KillPatch.ResolveHeadOutcome(caliberStr, ammoName, projectileCount, bulletMassGram, initialSpeed, player.Id, shot.FireIndex);
			// ref: CR-HEAD-DUP-01 — "estourar" reusa o mesmo DismemberLimb do "arranca" (só
			// troca o prop), pra esconder a cabeça original do mesmo jeito comprovado — ver
			// KillPatch.cs case 0 do Postfix.
			if (headOutcome == VisceralCombat.Combined.Patches.KillPatch.HeadDismemberOutcome.HeadOff)
			{
				Transform[] dummyLimbs;
				VisceralCombat.Combined.Patches.KillPatch.DismemberLimb(player, shot.Direction, dismemberPart.Value, boneName, "Head_3", extraAssets, out dummyLimbs);
				// ref: [P-10.3] - Reativado. DropCorpseHeadEquipment agora usa o InventoryController
				// do host (vivo, sincronizado pela rede) em vez do controller do próprio cadáver
				// (GClass3385, sem integração Fika) - ver comentário no método.
				DropCorpseHeadEquipment(player);
			}
			else if (headOutcome == VisceralCombat.Combined.Patches.KillPatch.HeadDismemberOutcome.HeadBurst)
			{
				Transform[] dummyLimbs;
				VisceralCombat.Combined.Patches.KillPatch.DismemberLimb(player, shot.Direction, dismemberPart.Value, boneName, $"Head_{UnityEngine.Random.Range(1, 3)}", extraAssets, out dummyLimbs);
				// ref: [P-10.3] - ver comentário acima (ramo HeadOff).
				DropCorpseHeadEquipment(player);
			}
			return;
		}

		float chance = VisceralCombat.Combined.Patches.KillPatch.ResolveDismemberChance(caliberStr, ammoName, projectileCount, bulletMassGram, initialSpeed, dismemberPart.Value, player.Id, shot.FireIndex);
		if (UnityEngine.Random.value <= chance)
		{
			Transform[] dummyLimbs;
			VisceralCombat.Combined.Patches.KillPatch.DismemberLimb(player, shot.Direction, dismemberPart.Value, boneName, capAsset, extraAssets, out dummyLimbs);
		}
	}

	private static void DropCorpseHeadEquipment(Player player)
	{
		// ref: CR-02-04 - Se a cabeça de um cadáver for desmembrada por tiros posteriores,
		// derruba capacete e óculos que ainda estavam no cadáver para evitar artefatos visuais
		// (itens flutuando ou presos num pescoço decepado).
		try
		{
			// ref: CR-03-01 - Gate de autoridade (todo outro ponto de mutação de inventário deste
			// mod tem essa checagem) - agora combinado com o fix de [P-10.3] abaixo, que resolve o
			// problema de fundo que CR-03-01 tinha deixado pendente.
			if (!(FikaBackendUtils.IsServer || FikaBackendUtils.IsSinglePlayer)) return;
			if (VisceralEntry.Instance == null || !VisceralEntry.Instance.DropHeadEquipmentOnDismemberment.Value) return;

			// ref: pedido do usuário 2026-09-20 - fone (Earpiece) e máscara (FaceCover) ficam presos
			// na cabeça igual capacete/óculos - mesmo toggle (DropHeadEquipmentOnDismemberment).
			Item helmet = player.Inventory?.Equipment?.GetSlot(EquipmentSlot.Headwear)?.ContainedItem;
			Item eyewear = player.Inventory?.Equipment?.GetSlot(EquipmentSlot.Eyewear)?.ContainedItem;
			Item faceCover = player.Inventory?.Equipment?.GetSlot(EquipmentSlot.FaceCover)?.ContainedItem;
			Item earpiece = player.Inventory?.Equipment?.GetSlot(EquipmentSlot.Earpiece)?.ContainedItem;
			if (helmet == null && eyewear == null && faceCover == null && earpiece == null) return;

			// ref: [P-10.3] - CR-03-01 usava helmet.Owner (o TraderControllerClass do próprio
			// cadáver, GClass3385) para executar o ThrowItem - esse controller nunca teve override
			// de vmethod_1 pelo Fika, então a operação nunca era transmitida pela rede (mesma classe
			// de bug que motivou a reformulação completa de DeathInventoryDropPatch via
			// CR-NET-LOCK-01). InteractionsHandlerClass.Throw/smethod_17 (Assembly-CSharp,
			// decompilados via ilspycmd - InteractionsHandlerClass tem erro de decompile conhecido no
			// dump) não exigem que o item pertença ao controller que executa a operação: o parâmetro
			// "itemController" só é usado para contabilizar limite de descarte (comparando
			// item.Owner com o dono do ENDEREÇO DE DESTINO, não com itemController). E o lado Fika
			// (HostInventoryController.RunHostOperation) sincroniza pelo NetId de quem EXECUTA a
			// operação, não pelo dono atual do item - o pacote carrega a operação (com o ID do item)
			// e cada peer a replica localmente. Por isso: qualquer InventoryController vivo e
			// Fika-aware serve, mesmo sobre um item que não é dele. Usamos o do host (MainPlayer)
			// em vez do atirador porque este método só roda no host/singleplayer (gate acima) e o
			// atirador, quando é um peer remoto, é representado no host como ObservedInventoryController
			// - que NÃO sobrescreve vmethod_1 (cai no fallback local, sem broadcast). O host
			// (HostInventoryController, sempre com o override) evita essa armadilha por completo.
			InventoryController hostController = Singleton<GameWorld>.Instantiated
				? Singleton<GameWorld>.Instance?.MainPlayer?.InventoryController
				: null;
			if (hostController == null) return;

			// ref: [P-10.3] (fix 02) - hostController.ThrowItem(...) usava Player_0 do PRÓPRIO
			// hostController como origem do arremesso (Player.PlayerOwnerInventoryController.
			// ThrowItem sempre joga a partir do player DONO do controller - o host, não o cadáver).
			// Resultado observado em jogo: capacete/óculos apareciam na posição do host/atirador,
			// não na do cadáver (a 100m de distância, no teste do usuário). Corrigido construindo o
			// ThrowOperationClass manualmente (o mesmo caminho que ThrowItem usa por baixo:
			// InteractionsHandlerClass.Throw + vmethod_1 - ambos confirmados públicos via IL),
			// passando o CADÁVER (player) como o IPlayer de referência de posição/trajetória
			// (ThrowOperationClass.Iplayer_0), enquanto hostController continua sendo quem executa
			// a operação pela rede - mesma separação autoridade-vs-posição já comprovada acima pra
			// item.Owner. O cadáver é um Player/IPlayer válido em todos os peers (sincronizado desde
			// a morte), então a posição fica correta pra todo mundo, não só localmente no host.
			if (helmet != null) ThrowFromCorpsePosition(hostController, helmet, player);
			if (eyewear != null) ThrowFromCorpsePosition(hostController, eyewear, player);
			if (faceCover != null) ThrowFromCorpsePosition(hostController, faceCover, player);
			if (earpiece != null) ThrowFromCorpsePosition(hostController, earpiece, player);
		}
		catch (Exception ex)
		{
			QuickLogger.Log(ELogType.Error, $"[LimbKillPatch.DropCorpseHeadEquipment] {ex}");
		}
	}

	// ref: [P-10.3] (fix 02) — constrói e despacha o ThrowOperationClass manualmente (o mesmo
	// caminho que TraderControllerClass.ThrowItem usa por baixo) pra poder passar "corpse" como
	// origem de posição em vez do player dono de "itemController". Tipos/membros confirmados
	// públicos via IL do Assembly-CSharp (ilspycmd): InteractionsHandlerClass.Throw (static),
	// TraderControllerClass.method_12()/vmethod_1() (herdados por InventoryController).
	private static void ThrowFromCorpsePosition(TraderControllerClass itemController, Item item, Player corpse)
	{
		GStruct154<GClass3406> throwResult = InteractionsHandlerClass.Throw(item, itemController, simulate: true);
		if (throwResult.Failed) return;

		ThrowOperationClass operation = new ThrowOperationClass(itemController.method_12(), itemController, throwResult.Value, throwResult.Value.ItemsToDestroy, corpse, downDirection: false);
		itemController.vmethod_1(operation, null);
	}
}
