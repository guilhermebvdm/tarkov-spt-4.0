using System;
using System.Reflection;
using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using Fika.Core.Main.Utils;
using SPT.Reflection.Patching;

namespace VisceralCombat.Combined.Patches;

/// <summary>
/// Ponto único de remoção de item do Equipment de um player agonizante: arma na mão (qualquer
/// morte) e capacete/óculos (quando a cabeça vai ser desmembrada) — unificados aqui após uma
/// sessão inteira de depuração em coop mostrar que QUALQUER remoção feita em Player.OnDead
/// (DiedEvent) ou no Postfix de Player.ApplyDamageInfo chega tarde demais.
///
/// Cadeia real (Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs):
///   Player.ApplyDamageInfo (Player.cs:30463) seta LastDamageInfo/LastBodyPart (30477-30478)
///     -> ActiveHealthController.ApplyDamage (30480)
///          -> Kill(damageType) [ActiveHealthController.cs:3923]
///               base.IsAlive = false;                 (3927)
///               method_35(damageType);                (3928) <- ESTE método
///                    -> SendNetworkSyncPacket(IsAlive=false)
///                         -> [Fika] ClientHealthController/BotHealthController
///                              -> FikaPlayer.SetupCorpseSyncPacket
///                                   -> EFTItemSerializerClass.SerializeItem(Inventory.Equipment)
///                                      **aqui o Fika tira a "foto" do inventário do cadáver e
///                                      manda pra rede — é isso que os outros peers usam pra
///                                      reconstruir o cadáver**
///               DiedEvent?.Invoke(damageType);          (3929) <- só agora dispara Player.OnDead
///
/// Ou seja: por mais que a remoção rode ANTES de Player.OnDead, se rodar depois de method_35 já
/// é tarde — o pacote de sync do cadáver já foi montado com o item ainda dentro, e os outros
/// peers reconstroem o cadáver a partir desse snapshot velho. Foi exatamente isso que causou a
/// duplicação real documentada pelo usuário (MP-133: dropada no chão E ainda lootável do
/// cadáver, cópias independentes).
///
/// Por que não interceptar ANTES disso (Prefix em Kill, ou em ApplyDamageInfo): nesse ponto
/// IsAlive ainda é true, e tanto HostInventoryController quanto ClientInventoryController do
/// Fika adiam a operação de inventário pro PRÓXIMO FRAME quando o player ainda está vivo:
///   HandleOperation: "if (_player.HealthController.IsAlive) { await Task.Yield(); }"
/// Ou seja, jogar o item ali só terminaria de fato (RunHostOperation, que reparent o item de
/// verdade) um frame depois — de novo, depois do pacote de sync já ter saído. Só depois que
/// IsAlive vira false (uma linha antes de method_35, dentro de Kill) essa mesma operação roda
/// de forma síncrona (sem yield) — por isso o gancho é exatamente aqui, nem antes nem depois.
///
/// method_35 é público, sem overrides em nenhuma subclasse do Fika (ClientHealthController/
/// BotHealthController só sobrescrevem SendNetworkSyncPacket), e só tem UMA chamada em todo o
/// assembly (dentro de Kill) — gancho estável o bastante pra usar por nome via reflexão, no
/// mesmo padrão já usado em KillPatch._getInventoryController.
/// </summary>
public class DeathInventoryDropPatch : ModulePatch
{
	// Player.LastDamageInfo/LastBodyPart são "protected" (Assembly-CSharp/EFT/Player.cs:24360,
	// 24362) — inacessíveis daqui por fora. Mesmo padrão de reflexão já usado em
	// KillPatch._getInventoryController pra chegar em membros não-públicos.
	private static readonly FieldInfo _lastDamageInfoField = typeof(Player).GetField("LastDamageInfo", BindingFlags.Instance | BindingFlags.NonPublic);
	private static readonly FieldInfo _lastBodyPartField = typeof(Player).GetField("LastBodyPart", BindingFlags.Instance | BindingFlags.NonPublic);

	protected override MethodBase GetTargetMethod()
	{
		return typeof(ActiveHealthController).GetMethod("method_35", BindingFlags.Instance | BindingFlags.Public);
	}

	[PatchPrefix]
	private static void Prefix(ActiveHealthController __instance, EDamageType damageType)
	{
		try
		{
			Player player = __instance.Player;
			if (player == null || VisceralEntry.Instance == null) return;

			// ref: CR-02-01 - Só o host, singleplayer ou o próprio jogador humano cliente (player.IsYourPlayer)
			// executam a remoção real síncrona antes do pacote de sync do cadáver ser serializado pelo Fika.
			if (!(FikaBackendUtils.IsServer || FikaBackendUtils.IsSinglePlayer || player.IsYourPlayer)) return;

			if (VisceralEntry.Instance.DropWeaponOnDeath.Value)
			{
				DropHandsWeapon(player);
			}

			ResolveAndDropHeadEquipment(player, damageType);
		}
		catch (Exception ex)
		{
			QuickLogger.Log(ELogType.Error, $"[DeathInventoryDropPatch] {ex}");
		}
	}

	private static void DropHandsWeapon(Player player)
	{
		Item item = player.HandsController?.Item;
		if (item == null) return;

		// ref: Assembly-CSharp/KnifeItemClass.cs:7 — checagem por tipo concreto evita depender
		// de EFT.InventoryLogic.ItemComponent.Types.KnifeComponent (assembly não referenciado
		// aqui) sem mudar o resultado: faca nunca vira item solto no chão.
		if (item is KnifeItemClass) return;

		// ref: pedido do usuário 2026-09-20 - se o player/bot morre no meio de um ciclo de tiro ou
		// recarga, FirearmController.CanExecute (Assembly-CSharp/EFT/Player.cs:13271) recusa a
		// operação de arremesso em silêncio - a checagem só libera item em slot SEM animação de mão
		// direto (Player.cs:1466/method_20); arma É um slot animado, então cai na checagem de
		// CurrentOperation, que só permite uma lista pequena de exceções (nada relacionada a tiro/
		// recarga ativos). Fast-forward é o mecanismo OFICIAL do próprio jogo pra essa situação -
		// Player.OnDead (Player.cs:30607) chama exatamente isso quando o player morre, só que tarde
		// demais pra nossa janela de sincronização (depois do DiedEvent). Chamamos aqui, mais cedo,
		// só pra destravar o HandsController antes do ThrowItem - sem inventar mecanismo novo.
		player.FastForwardCurrentOperations();

		if (player.InventoryController is TraderControllerClass controller)
		{
			controller.ThrowItem(item, false, null);
		}
	}

	private static bool IsBallisticOrExplosiveDamage(EDamageType damageType)
	{
		// ref: CR-02-03 - Apenas tipos que representam impacto balístico, estilhaços ou explosões
		// podem causar desmembramento de cabeça ou arrancar capacete na morte.
		// Exclui mortes por sangramento (Light/HeavyBleeding), desidratação, fome, queda, veneno, etc.
		const EDamageType dismemberMask = EDamageType.Bullet | EDamageType.Sniper | EDamageType.GrenadeFragment |
		                                  EDamageType.Explosion | EDamageType.Landmine | EDamageType.Artillery |
		                                  EDamageType.Btr | EDamageType.ThermobaricExplosion | EDamageType.Blunt;
		return (damageType & dismemberMask) != 0;
	}

	private static void ResolveAndDropHeadEquipment(Player player, EDamageType damageType)
	{
		// ref: CR-03-02 - Checa a parte do corpo ANTES do tipo de dano, pra não gravar uma
		// entrada em PendingHeadOutcome (nunca lida) pra mortes sem nenhuma relação com a
		// cabeça (fome, desidratação, sangramento residual, queda).
		EBodyPart lastBodyPart = (EBodyPart)_lastBodyPartField.GetValue(player);
		if (lastBodyPart != EBodyPart.Head) return;

		// ref: CR-02-03 - Se a morte foi causada por dano não balístico/explosivo, aborta o drop
		// de cabeça e garante que o cache de desmembramento fique em None, evitando falsos positivos
		// gerados pelo enum EBodyPart.Head ser default 0 ou conter tiros antigos residuais.
		if (!IsBallisticOrExplosiveDamage(damageType))
		{
			KillPatch.PendingHeadOutcome[player.Id] = KillPatch.HeadDismemberOutcome.None;
			return;
		}

		// ref: backlog 004 — a mesma decisão (arranca/estoura/nenhum) que antes só existia no
		// Postfix de KillPatch (ApplyDamageInfo) precisa ser computada aqui, cedo o bastante pra
		// dar tempo de derrubar o capacete/óculos antes do snapshot de rede. O resultado é
		// cacheado (KillPatch.PendingHeadOutcome) e consumido UMA VEZ pelo Postfix pra aplicar o
		// efeito visual, sem rolar a chance de novo (evita dobrar o acúmulo de momento
		// multi-projétil).
		DamageInfoStruct damageInfo = (DamageInfoStruct)_lastDamageInfoField.GetValue(player);
		AmmoTemplate ammoTemplate = KillPatch.ResolveAmmoTemplate(damageInfo);
		string caliber = ammoTemplate?.Caliber;
		string ammoName = ammoTemplate?.Name;
		int projectileCount = ammoTemplate?.ProjectileCount ?? 1;
		float bulletMassGram = ammoTemplate?.BulletMassGram ?? 0f;
		float initialSpeed = ammoTemplate?.InitialSpeed ?? 0f;

		KillPatch.HeadDismemberOutcome outcome = KillPatch.ResolveHeadOutcome(
			caliber, ammoName, projectileCount, bulletMassGram, initialSpeed, player.Id, damageInfo.FireIndex);
		KillPatch.PendingHeadOutcome[player.Id] = outcome;

		// ref: item 005 — desacoplado de EnableDismemberment de propósito: se outro peer do raid
		// tem o toggle mestre/desmembramento ligado, o capacete/óculos cai pra ELE (Camada 2 roda
		// pro autor/host da morte, ver gate FikaBackendUtils.IsServer/IsSinglePlayer/IsYourPlayer
		// acima). Se este cliente não derrubasse também, o item ficaria em 2 lugares (chão + morto)
		// pra quem tem o mod ativo — mesma classe de bug que CR-NET-LOCK-01 resolveu pra fantasma
		// de arma. Só o toggle PRÓPRIO (DropHeadEquipmentOnDismemberment) continua controlando isso.
		if (outcome == KillPatch.HeadDismemberOutcome.None) return;
		if (!VisceralEntry.Instance.DropHeadEquipmentOnDismemberment.Value) return;

		if (!(player.InventoryController is TraderControllerClass controller)) return;
		// ref: pedido do usuário 2026-09-20 - fone (Earpiece) e máscara (FaceCover) ficam presos
		// na cabeça igual capacete/óculos - reaproveita o mesmo toggle (DropHeadEquipmentOnDismemberment),
		// mesmo mecanismo de drop já validado em raid (CR-NET-LOCK-01).
		Item helmet = player.Inventory?.Equipment?.GetSlot(EquipmentSlot.Headwear)?.ContainedItem;
		Item eyewear = player.Inventory?.Equipment?.GetSlot(EquipmentSlot.Eyewear)?.ContainedItem;
		Item faceCover = player.Inventory?.Equipment?.GetSlot(EquipmentSlot.FaceCover)?.ContainedItem;
		Item earpiece = player.Inventory?.Equipment?.GetSlot(EquipmentSlot.Earpiece)?.ContainedItem;
		if (helmet != null) controller.ThrowItem(helmet, false, null);
		if (eyewear != null) controller.ThrowItem(eyewear, false, null);
		if (faceCover != null) controller.ThrowItem(faceCover, false, null);
		if (earpiece != null) controller.ThrowItem(earpiece, false, null);
	}

	// ref: revertido em 2026-09-20 - contorno de "forçar viseira levantada" (RaiseHelmetVisorIfPresent)
	// removido. Investigação mostrou que o bug de ícone 2D de capacete não era causado pelo
	// VisceralCombat nem pelo estado da viseira - era conflito de outro mod do pacote do usuário,
	// confirmado comprando um capacete no mercado (fora de raid, sem nenhuma relação com este mod)
	// e reproduzindo o mesmo bug. Resolvido revertendo o pacote de mods do usuário. Ver
	// 002-drop-arma-capacete-oculos-cabeca-06-fix-04.md pro histórico completo da investigação.
}
