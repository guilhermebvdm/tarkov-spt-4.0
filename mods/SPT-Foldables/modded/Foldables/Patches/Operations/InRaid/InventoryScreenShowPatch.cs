using System.Reflection;
using System.Threading.Tasks;
using EFT.InventoryLogic;
using EFT.UI;
using Foldables.Utils;
using SPT.Reflection.Patching;

#pragma warning disable VSTHRD003
#pragma warning disable VSTHRD100
// ReSharper disable AsyncVoidMethod

namespace Foldables.Patches.Operations.InRaid;

/// <summary>
/// Force unfold headphones if folded
/// </summary>
public class InventoryScreenShowPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ItemsPanel).GetMethod(nameof(ItemsPanel.Show));
    }

    [PatchPostfix]
    protected static async void Postfix(InventoryController inventoryController, ItemsPanel.EItemsTab currentTab, bool inRaid, Task __result, CompoundItem lootItem)
    {
        if (!inRaid || currentTab != ItemsPanel.EItemsTab.Gear) return;

        // ref: Assembly-CSharp/EFT.UI/ItemsPanel.cs:219 (assinatura de Show, parâmetro lootItem),
        // :282-287 (lootItem is InventoryEquipment -> ComplexStashPanel.Show, caminho que processa
        // corpo/bot/outro jogador -- é exatamente aqui que o ArgumentException real acontece),
        // :288-292 (lootItem != null, caminho de container simples -- baú/mochila no chão).
        // lootItem só é null quando a tela mostra exclusivamente o próprio inventário do jogador
        // (inventoryController, usado abaixo, é sempre o do jogador local -- nunca muda; backlog
        // 001, PA-01-01). Sem esta checagem, abrir QUALQUER container externo dispara a espera de
        // dobra abaixo, que pode reentrar em ItemsPanel.Show() depois que o jogador já fechou
        // aquele painel -- causa raiz do bug de painel duplicado/corrompido em coop.
        if (lootItem != null) return;

        await __result;

        var headphoneSlot = inventoryController.Inventory.Equipment.GetSlot(EquipmentSlot.Earpiece);
        if (headphoneSlot.ContainedItem.IsFoldableFolded())
        {
            headphoneSlot.ContainedItem.FoldItemWithDelay(force: true);
        }
    }
}
