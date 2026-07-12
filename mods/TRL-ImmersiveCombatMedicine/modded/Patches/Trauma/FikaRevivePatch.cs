using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using HarmonyLib;
using TRLImmersiveCombatMedicine;

namespace Band_Aid
{
    [HarmonyPatch]
    public class FikaReviveGetActionsPatch
    {
        static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("Fika.Core.Main.Components.ReviveInteractable");
            if (type == null) return null;
            return AccessTools.Method(type, "GetActions");
        }

        [HarmonyPostfix]
        private static void Postfix(GamePlayerOwner owner, ref ActionsReturnClass __result)
        {
            if (__result == null || __result.Actions == null) return;

            bool hasDefib = HasDefibrillator(owner.Player);

            if (!hasDefib)
            {
                __result.Actions.RemoveAll(a => a.Name != "Search");
            }
        }

        private static bool HasDefibrillator(Player player)
        {
            if (player == null || player.Profile == null || player.Profile.Inventory == null) return false;

            var items = player.Profile.Inventory.GetAllItemByTemplate("5c052e6986f7746b207bc3c9");
            return items != null && items.Any();
        }
    }

    [HarmonyPatch]
    public class FikaRevivePlayerPatch
    {
        static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("Fika.Core.Main.Components.ReviveInteractable");
            if (type == null) return null;
            return AccessTools.Method(type, "RevivePlayer");
        }

        [HarmonyPrefix]
        private static void Prefix(bool success, object __instance)
        {
            // ref: CR-01-04 — este prefix roda DENTRO do callback de Plant do Fika
            // (ReviveInteractable.RevivePlayer): uma exceção aqui cancela o revive
            // inteiro. Nunca deixar escapar.
            try
            {
                if (!success) return;

                var type = AccessTools.TypeByName("Fika.Core.Main.Components.ReviveInteractable");
                var localPlayerField = AccessTools.Field(type, "_localPlayer");
                var player = localPlayerField.GetValue(__instance) as Player;

                if (player == null) return;

                ConsumeDefibrillator(player);
            }
            catch (Exception ex)
            {
                TrueTrauma.TraumaState.Logger?.LogError($"FikaRevivePlayerPatch: {ex.Message} — revive segue sem consumo do desfibrilador.");
            }
        }

        private static void ConsumeDefibrillator(Player player)
        {
            if (player == null || player.Profile == null || player.Profile.Inventory == null) return;

            var items = player.Profile.Inventory.GetAllItemByTemplate("5c052e6986f7746b207bc3c9");
            if (items != null && items.Any())
            {
                var defib = items.First();

                var inventoryController = player.InventoryController as EFT.InventoryLogic.InventoryController;
                if (inventoryController != null)
                {
                    if (TrueTrauma.TraumaState.Logger != null) TrueTrauma.TraumaState.Logger.LogInfo("Consumindo Desfibrilador para reviver aliado!");

                    // ref: CR-01-04 — a reflection antiga (Invoke com 1 arg num método de 2,
                    // passando discardResult.Value em vez do GStruct) lançava
                    // TargetParameterCountException. Chamada tipada, igual ao
                    // MedicalLogic.DiscardItemNetworked, propaga a transação à rede.
                    var discardResult = InteractionsHandlerClass.Discard(defib, inventoryController);
                    if (discardResult.Succeeded)
                    {
                        _ = inventoryController.TryRunNetworkTransaction(discardResult);
                    }
                }
            }
        }
    }
}
