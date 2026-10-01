using System;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using EFT.AssetsManager;
using EFT.Interactive;
using Fika.Core.Main.Utils;
using SPT.Reflection.Patching;
using UnityEngine;

namespace VisceralCombat.Ragdolls.Patches;

public class CreateBSGRagdollPatch : ModulePatch
{
	private static readonly FieldInfo _rbSpawnersField = typeof(Corpse).GetField("rigidbodySpawner_0", BindingFlags.Instance | BindingFlags.NonPublic);
	private static readonly FieldInfo _jointSpawnersField = typeof(Corpse).GetField("characterJointSpawner_0", BindingFlags.Instance | BindingFlags.NonPublic);
	private static readonly FieldInfo _sleepHierarchyField = typeof(Corpse).GetField("list_0", BindingFlags.Instance | BindingFlags.NonPublic);
	private static readonly FieldInfo _velocityField = typeof(Corpse).GetField("vector3_1", BindingFlags.Instance | BindingFlags.NonPublic);
	private static readonly FieldInfo _playerBodyField = typeof(Corpse).GetField("PlayerBody", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

	protected override MethodBase GetTargetMethod()
	{
		return typeof(Corpse).GetMethod("method_16", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
	}

	[PatchPrefix]
	private static bool Prefix(Corpse __instance, bool forceStill = false)
	{
		// ref: item 005 — toggle mestre desligado = deixa o ragdoll vanilla do EFT rodar (inverso dos demais gates)
		if (VisceralEntry.Instance != null && !VisceralEntry.Instance.VisceralCombatEnabled.Value) return true;

		PlayerBody playerBody = _playerBodyField?.GetValue(__instance) as PlayerBody ?? __instance.GetComponentInChildren<PlayerBody>();
		if (playerBody == null) return true; // fallback to EFT native logic if PlayerBody is unavailable

		// ref: hardening pós-relato "bot morre em pé, sem ragdoll" — este Prefix reimplementa
		// Corpse.method_16 inteiro (retorna false, pula o vanilla sempre) sem nenhum try/catch
		// original. Qualquer exceção aqui (reflexão falhando, spawner nulo, etc.) deixava o corpo
		// sem NENHUM ragdoll — nem o nosso (quebrou no meio) nem o vanilla (nunca rodou, porque já
		// tínhamos retornado false). Se falhar ANTES de atribuir __instance.Ragdoll, é seguro
		// devolver true (deixa o vanilla montar do zero). Se falhar DEPOIS (OnRigidbodyStarted/
		// method_19), não é seguro — o vanilla criaria um segundo RagdollClass por cima do que já
		// foi atribuído — nesse caso só loga e mantém o comportamento antigo (return false).
		bool ragdollAssigned = false;
		try
		{
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
			ragdollAssigned = true;
			__instance.OnRigidbodyStarted();
			__instance.method_19();
			return false;
		}
		catch (Exception ex)
		{
			string fallbackNote = ragdollAssigned
				? "falhou DEPOIS de atribuir Ragdoll — mantendo comportamento antigo (sem repassar pro vanilla, evita duplicar o ragdoll), corpo pode ficar sem física"
				: "falhou ANTES de atribuir Ragdoll — revertendo para o ragdoll nativo do EFT (return true)";
			QuickLogger.Log(ELogType.Error, $"[CreateBSGRagdollPatch] Erro montando ragdoll custom: {fallbackNote}. {ex}");
			return !ragdollAssigned;
		}
	}

	private static void CounterDeleteRigidBody()
	{
	}
}
