using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using Fika.Core.Main.Players;
using SPT.Reflection.Patching;

namespace Fika.Core.Main.Patches.PlayerPatches;

/// <summary>
/// Previne a falha de descarte de mochila ("ZZ" / DropBackpack) em clientes remotos no Headless/Host Coop.
/// No EFT nativo, Player.TryRemoveFromHands tenta validar/animar as mãos para slots animados (Backpack).
/// Em instâncias de ObservedPlayer (jogadores remotos no servidor), o HandsController não deve bloquear
/// o descarte de itens que já foram fisicamente autorizados e desprendidos pelo cliente de origem.
/// </summary>
public class ObservedPlayer_DropBackpackSafety_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(Player).GetMethod(nameof(Player.TryRemoveFromHands), BindingFlags.Public | BindingFlags.Instance);
    }

    [PatchPrefix]
    public static bool Prefix(Player __instance, Item item, object abstractOperation, Callback callback)
    {
        // Se a entidade for um ObservedPlayer (jogador remoto espelhado no servidor)
        // E o item que está saindo NÃO for o item que está atualmente empunhado nas mãos:
        if (__instance is ObservedPlayer && (__instance.HandsController == null || __instance.HandsController.Item != item))
        {
            callback?.Succeed();
            return false; // Evita que o HandsController tente executar animações locais de FPS que falham no ObservedPlayer
        }

        return true; // Continua o fluxo normal do EFT (ex: jogador local ou descarte da arma empunhada)
    }
}
