using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using Fika.Core.Main.Utils;
using SPT.Reflection.Patching;
using UnityEngine;

namespace VisceralCombat.Ragdolls.Patches;

/// <summary>
/// Trava de segurança: DropItemDead (vanilla) roda depois de VisceralCombat.Combined.Patches.
/// DeathInventoryDropPatch já ter jogado a arma no chão (ver aquele arquivo — o drop real
/// acontece em ActiveHealthController.method_35, bem antes de Player.OnDead/DropItemDead).
///
/// ref: CR-WEAPON-GHOST-01 — Player.cs:30686 mostra a única chamada real: DropItemDead(_handsController.Item,
/// _handsController.ControllerGameObject) — ou seja, "prefab" AQUI NÃO é um prefab novo, é o
/// PRÓPRIO GameObject do modelo visual da arma na mão. Pular DropItemDead inteiro (pra não
/// duplicar o Item já jogado no chão) deixa esse modelo congelado na pose da mão — "fantasma
/// flutuando".
///
/// ref: CR-HANDS-DISPOSE-01 (bug real reportado depois: softlock de extração pra raid inteira em
/// coop) — a 1ª correção usava UnityEngine.Object.Destroy(prefab) direto, e isso quebrou outra
/// coisa: prefab é o MESMO GameObject que FirearmController usa internamente (_controllerObject),
/// e o Animator dele (FirearmsAnimator) só é limpo/devolvido ao pool quando o PRÓPRIO
/// FirearmController.Destroy() roda (Player.cs:13532-13552):
///   if (firearmsAnimator_0 != null) firearmsAnimator_0.SetBoltCatch(active: false);
///   ...
///   AssetPoolObject.ReturnToPool(_controllerObject.gameObject);
/// O guard "firearmsAnimator_0 != null" é uma checagem de referência C# comum — não detecta que
/// o GameObject por trás já foi destruído por fora. Resultado: no fim da raid, quando
/// Player.Dispose() -> method_118() -> HandsController.Destroy() roda de novo pra ESSE jogador
/// (ele ainda está na lista de disposal do Fika mesmo já morto), o SetBoltCatch cai num Animator
/// nativo já destruído -> NullReferenceException -> Fika.Core.Main.GameMode.CoopGame.Stop()
/// relança e aborta o Dispose() pra TODOS os jogadores da sala (softlock de extração).
///
/// Correção: em vez de destruir "prefab" por fora, chamamos o MESMO caminho oficial que o
/// próprio jogo usa pra descartar a mão (Player.method_118 -> HandsController.Destroy(), que já
/// faz o SetBoltCatch(false) com o Animator ainda válido + AssetPoolObject.ReturnToPool — esconde
/// e recicla o modelo do jeito certo) e o MESMO guard de "já descartado" que Player.Dispose() usa
/// (Player.cs:31420-31427: só chama Destroy() se _handsController != null, e zera o campo depois
/// — idempotente). Fazendo isso aqui, quando a raid terminar e o Fika percorrer os jogadores pra
/// Dispose(), esse jogador já está com HandsController null — o Destroy() duplicado nunca roda.
/// </summary>
public class WeaponDropOnDeathSkipVanillaFlingPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		// ref: Assembly-CSharp/EFT/Player.cs:26802 — public void DropItemDead(Item item, GameObject prefab)
		return typeof(Player).GetMethod(
			"DropItemDead",
			BindingFlags.Instance | BindingFlags.Public,
			null,
			new System.Type[] { typeof(Item), typeof(GameObject) },
			null);
	}

	[PatchPrefix]
	private static bool Prefix(Player __instance, Item item)
	{
		if (item == null) return true;
		// ref: CR-02-06 - Se o item já mudou de dono, teve seu dono nulificado ou foi desanexado
		// do slot (CurrentAddress == null), ele já foi jogado pelo DeathInventoryDropPatch.
		if (item.Owner != __instance.InventoryController || item.CurrentAddress == null)
		{
			// já foi jogado por DeathInventoryDropPatch — não deixa o vanilla flingar/registrar
			// como LootItem um item órfão (criaria uma 2ª representação física do mesmo dado).
			DisposeHandsControllerSafely(__instance);
			return false;
		}
		return true; // item ainda é do player (config desligada, faca, ou não autorizado) — comportamento vanilla normal
	}

	private static void DisposeHandsControllerSafely(Player player)
	{
		try
		{
			var handsController = player.HandsController;
			if (handsController == null) return;

			// ref: Player.cs:31420-31427 — method_118() é público, faz exatamente
			// "_handsController.Destroy(); _handsController = null;" com guard de idempotência
			// (só age se _handsController != null). Chamar aqui é o mesmo efeito que o próprio
			// Player.Dispose() teria feito nesse exato momento — só adiantado.
			player.method_118();

			// ref: Player.cs:31400-31405 — Dispose() também destrói o COMPONENTE HandsController
			// em si (distinto do GameObject do modelo da arma, que o Destroy() acima já devolveu
			// ao pool). Fazendo isso aqui também, o bloco "if (HandsController != null)" de
			// Dispose() nunca mais entra pra esse jogador (HandsController já é null) — sem
			// Destroy() duplicado, sem ReturnToPool duplicado.
			Object.Destroy(handsController);
		}
		catch (System.Exception ex)
		{
			QuickLogger.Log(ELogType.Error, $"[WeaponDropOnDeathSkipVanillaFlingPatch] {ex}");
		}
	}
}
