using System;
using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using Fika.Core.Main.Utils;
using SPT.Reflection.Patching;
using UnityEngine;

namespace VisceralCombat.Ragdolls.Patches;

public class WeaponDropOnDeathPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		// ref: Assembly-CSharp/EFT/Player.cs:26802 — public void DropItemDead(Item item, GameObject prefab)
		return typeof(Player).GetMethod(
			"DropItemDead",
			BindingFlags.Instance | BindingFlags.Public,
			null,
			new Type[] { typeof(Item), typeof(GameObject) },
			null);
	}

	[PatchPrefix]
	private static bool Prefix(Player __instance, Item item)
	{
		try
		{
			if (VisceralEntry.Instance == null || !VisceralEntry.Instance.DropWeaponOnDeath.Value) return true;
			if (item == null) return true;

			// ref: Assembly-CSharp/KnifeItemClass.cs:7 — KnifeItemClass : Item. O vanilla
			// (Player.cs:26848) testa via GetItemComponent<KnifeComponent>(), mas esse
			// componente vive num assembly (ItemComponent.Types) não referenciado por este
			// projeto — checagem por tipo concreto evita a dependência extra sem mudar o
			// resultado (exceção explícita: faca nunca vira item solto no chão).
			if (item is KnifeItemClass) return true;

			// Só o host (ou singleplayer) executa a remoção real do inventário. A replicação
			// para os demais peers acontece pelo canal nativo de operação de inventário do
			// EFT/Fika (RemoveOperationClass) — mesmo padrão de gate já usado neste mod em
			// VisceralCombat.Combined.Patches.KillPatch.cs para outras ações que não podem
			// rodar em duplicidade em cada peer de um raid coop.
			if (!(FikaBackendUtils.IsServer || FikaBackendUtils.IsSinglePlayer)) return true;

			// ref: VisceralCombat.Ragdolls.Patches.ShootOffHelmetPatch.cs — mesmo padrão de
			// drop já usado pelo mod para o capacete.
			// ref: CR-01-01 — só pula o vanilla (return false) quando o ThrowItem de fato
			// rodou; caso contrário deixa o fling cosmético do vanilla acontecer em vez de
			// arriscar a arma desaparecer sem cair em lugar nenhum.
			if (__instance.InventoryController is TraderControllerClass controller)
			{
				controller.ThrowItem(item, false, null);
				return false; // pula o fling cosmético do vanilla (Player.cs:26853 AttachWeapon) — a arma já foi removida do inventário e virou item solto no mundo
			}

			QuickLogger.Log(ELogType.Warn, $"[WeaponDropOnDeathPatch] InventoryController nao e TraderControllerClass para {__instance.Profile?.Nickname} - mantendo comportamento vanilla.");
			return true;
		}
		catch (Exception ex)
		{
			QuickLogger.Log(ELogType.Error, $"[WeaponDropOnDeathPatch] {ex}");
			return true; // falha seguro: deixa o vanilla rodar em vez de arriscar corpo inconsistente
		}
	}
}
