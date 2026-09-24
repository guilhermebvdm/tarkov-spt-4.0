using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using EFT.AssetsManager;
using Fika.Core.Main.Utils;
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
		// ref: item 005 — toggle mestre desligado = deixa o ragdoll vanilla do EFT rodar (inverso dos demais gates)
		if (VisceralEntry.Instance != null && !VisceralEntry.Instance.VisceralCombatEnabled.Value) return true;

		// ref: hardening pós-relato "bot morre em pé, sem ragdoll" — este Prefix reimplementa
		// RagdollClass.Start() inteiro (retorna false, pula o vanilla sempre) sem nenhum
		// try/catch original. Uma exceção em QUALQUER joint/rigidbody (ex.: linha do massScale
		// abaixo, que assume GetComponent<Rigidbody>() != null sem checar) abortava o método
		// inteiro no meio — o resto dos joints/rigidbodies nunca era criado, E o vanilla nunca
		// rodava (return false já tinha sido decidido), deixando o corpo sem física de ragdoll
		// nenhuma. Cada iteração agora é isolada: um joint/rigidbody problemático é pulado (logado)
		// em vez de derrubar o ragdoll inteiro — degrada pra "ragdoll parcial" em vez de "nenhum".
		// Não repassamos pro vanilla (return true) em caso de erro aqui porque a essa altura já
		// podemos ter criado parte dos joints/rigidbodies reais — deixar o Start() nativo rodar por
		// cima criaria duplicatas.
		try
		{
			GClass4062.ReleaseBeginSample("CorpseRagdoll.SpawnRigidbodies", "Start");
			CharacterJointSpawner[] characterJointSpawner_ = __instance.CharacterJointSpawner_0;
			if (characterJointSpawner_ != null)
			{
				for (int i = 0; i < characterJointSpawner_.Length; i++)
				{
					try
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
					catch (Exception ex)
					{
						QuickLogger.Log(ELogType.Error, $"[RagdollClassPatch] Falha criando joint #{i} — pulando, resto do ragdoll continua: {ex}");
					}
				}
			}

			RigidbodySpawner[] rigidbodySpawner_ = __instance.RigidbodySpawner_0;
			if (rigidbodySpawner_ != null)
			{
				for (int j = 0; j < rigidbodySpawner_.Length; j++)
				{
					try
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
					catch (Exception ex)
					{
						QuickLogger.Log(ELogType.Error, $"[RagdollClassPatch] Falha criando rigidbody #{j} — pulando, resto do ragdoll continua: {ex}");
					}
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
		}
		catch (Exception ex)
		{
			QuickLogger.Log(ELogType.Error, $"[RagdollClassPatch] Erro inesperado montando o ragdoll — corpo pode ficar com física de ragdoll incompleta: {ex}");
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
