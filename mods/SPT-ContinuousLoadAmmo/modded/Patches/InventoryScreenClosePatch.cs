using System;
using System.Reflection;
using ContinuousLoadAmmo.Utils;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using SPT.Reflection.Patching;

namespace ContinuousLoadAmmo.Patches;

public class InventoryScreenClosePatch : ModulePatch
{
    public static event Action OnInventoryClose;

    protected override MethodBase GetTargetMethod()
    {
        return typeof(InventoryScreen).GetMethod(nameof(InventoryScreen.Close));
    }

    /// <summary>
    /// UI, Patch to NOT stop loading ammo on close
    /// </summary>
    [PatchPrefix]
    protected static void Prefix(ref InventoryController ___inventoryController_0, out InventoryController __state)
    {
        __state = ___inventoryController_0;
        if (!CommonUtils.InRaid) return;

        try
        {
            // Se NÃO houver controlador ativo ou NÃO houver carregamento ativo, NÃO interferir no Close() vanilla!
            var controller = Controllers.LoadAmmoController.Instance;
            if (controller == null || !controller.IsActive)
            {
                return;
            }

            if (___inventoryController_0 is Player.PlayerInventoryController playerInventoryController)
            {
                // Se houver qualquer ação de mãos/arma em andamento, cancelamos o carregamento e respeitamos o StopProcesses vanilla
                if (playerInventoryController.HasAnyHandsActionNonLinq())
                {
                    controller.StopLoading();
                    return;
                }

                playerInventoryController.SetNextProcessLocked(false);
            }

            // Somente ignora StopProcesses se o carregamento contínuo estiver de fato em execução e seguro
            ___inventoryController_0 = null;

            OnInventoryClose?.Invoke();
        }
        catch (Exception ex)
        {
            ContinuousLoadAmmo.LogSource.LogError($"ContinuousLoadAmmo: Erro defensivo em InventoryScreenClosePatch.Prefix: {ex}");
        }
    }

    [PatchPostfix]
    protected static void Postfix(ref InventoryController ___inventoryController_0, InventoryController __state)
    {
        if (__state != null && ___inventoryController_0 == null)
        {
            ___inventoryController_0 = __state;
        }
    }
}
