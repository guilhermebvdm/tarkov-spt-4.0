using System;
using System.Collections;
using System.Reflection;
using EFT.Interactive;
using Fika.Core.Main.Components;
using Fika.Core.Main.Utils;
using SPT.Reflection.Patching;
using UnityEngine;

namespace VisceralCombat.Ragdolls.Patches;

/// <summary>
/// Com "Item Physics" (ItemForce) ativo, mantém o Rigidbody de um item largado vivo (só
/// "dormindo", sem simulação ativa) em vez de deixar o jogo destruí-lo quando o item acomoda —
/// ref: LootItem.StopPhysics (Assembly-CSharp/EFT.Interactive/LootItem.cs:521). Sem isso, tiro/
/// explosão num item parado há mais de alguns segundos não tinha em cima do que aplicar força
/// (BodiesImpulsePatch/GrenadeItemsPatch dependem de attachedRigidbody != null).
/// </summary>
public class LootItemStopPhysicsPatch : ModulePatch
{
	// ref: LootItem.cs:94 — private IEnumerator ienumerator_0 (corrotina de assentamento)
	private static readonly FieldInfo _ienumeratorField = typeof(LootItem).GetField("ienumerator_0", BindingFlags.Instance | BindingFlags.NonPublic);

	protected override MethodBase GetTargetMethod()
	{
		// ref: LootItem.cs:521 — não é virtual, alvo único, sem overrides pra auditar (AP-03 N/A)
		return typeof(LootItem).GetMethod("StopPhysics", BindingFlags.Instance | BindingFlags.Public);
	}

	[PatchPrefix]
	private static bool Prefix(LootItem __instance)
	{
		try
		{
			if (VisceralEntry.Instance == null || !VisceralEntry.Instance.IsCategoryActive(VisceralEntry.Instance.ItemForce)) return true; // ref: item 005
			if (__instance == null || __instance.gameObject.GetComponent<ObservedLootItem>() == null) return true; // mesmo critério de BodiesImpulsePatch/GrenadeItemsPatch/PhysicalItemsPatch

			// Replica só a parte de "parar a corrotina" do StopPhysics original — evita o custo
			// de checagem por quadro rodando pra sempre — mas NÃO destrói o Rigidbody (fica
			// "dormindo", Unity acorda ele sozinho quando uma força é aplicada).
			object coroutine = _ienumeratorField?.GetValue(__instance);
			if (coroutine is IEnumerator enumerator)
			{
				((MonoBehaviour)__instance).StopCoroutine(enumerator); // ref: PA-01-01 — cast único, mesmo padrão de VisceralShotProcessor.cs:28
				_ienumeratorField.SetValue(__instance, null);
			}
			return false; // pula o resto do método original (que destruiria o Rigidbody)
		}
		catch (Exception ex)
		{
			QuickLogger.Log(ELogType.Error, $"[LootItemStopPhysicsPatch] {ex}");
			return true; // em caso de erro, comportamento vanilla (seguro)
		}
	}
}

/// <summary>
/// Garante que um Rigidbody preservado por LootItemStopPhysicsPatch seja destruído antes do
/// item voltar pro pool de reuso (AssetPoolObject.ReturnToPool, LootItem.cs:547) — evita que um
/// GameObject reciclado carregue um Rigidbody órfão pro próximo item que o usar.
/// </summary>
public class LootItemKillCleanupPatch : ModulePatch
{
	// ref: LootItem.cs:56 — protected Rigidbody _rigidBody
	private static readonly FieldInfo _rigidBodyField = typeof(LootItem).GetField("_rigidBody", BindingFlags.Instance | BindingFlags.NonPublic);

	protected override MethodBase GetTargetMethod()
	{
		// ref: LootItem.cs:536 — public override void Kill() (não redeclarado em ObservedLootItem)
		return typeof(LootItem).GetMethod("Kill", BindingFlags.Instance | BindingFlags.Public);
	}

	[PatchPrefix]
	private static bool Prefix(LootItem __instance)
	{
		try
		{
			if (__instance == null) return true;

			// ref: item 006, fix 02 — Fika.Core.Main.Components.ItemPositionSyncer (componente do
			// FIKA, não deste mod) sincroniza a posição de um item fisicamente ativo enquanto
			// _lootItem.RigidBody != null (PhysicsDone == false); ao ficar null, ela chama
			// NotifyDone() e se auto-destrói. Como este item mantém o Rigidbody vivo por muito mais
			// tempo que o vanilla, o syncer do FIKA continua "vigiando" o item bem depois do normal;
			// quando ESTE patch zera o Rigidbody durante Kill() (item sendo lootado), o próximo
			// FixedUpdate do syncer detecta RigidBody==null nesse momento de remoção (não de
			// assentamento natural, cenário que o FIKA nunca esperava por esse caminho) e tenta
			// desinscrever de ItemOwner.RemoveItemEvent — mas ItemOwner já pode estar limpo pela
			// própria remoção em andamento, gerando NullReferenceException repetida a cada quadro
			// (o componente nunca chega a se auto-destruir porque a exceção acontece antes do
			// Destroy(this) dele). Destruir o syncer aqui, antes de zerar o Rigidbody, evita que ele
			// chegue a rodar nesse estado — o item está sendo removido do mundo de qualquer forma,
			// então sincronizar a posição dele pra outros peers deixa de fazer sentido.
			ItemPositionSyncer syncer = __instance.gameObject.GetComponent<ItemPositionSyncer>();
			if (syncer != null)
			{
				UnityEngine.Object.Destroy(syncer);
			}

			object rb = _rigidBodyField?.GetValue(__instance);
			if (rb is Rigidbody rigidbody && rigidbody != null)
			{
				UnityEngine.Object.Destroy(rigidbody);
				_rigidBodyField.SetValue(__instance, null);
			}
		}
		catch (Exception ex)
		{
			QuickLogger.Log(ELogType.Error, $"[LootItemKillCleanupPatch] {ex}");
		}
		return true; // sempre deixa o Kill() original rodar — este patch só limpa, nunca substitui
	}
}
