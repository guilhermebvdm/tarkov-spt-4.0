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
    /// <summary>Template do Desfibrilador — o item que gateia e é consumido pelo revive do Fika.
    /// ref: item 013 — estava duplicado em HasDefibrillator e ConsumeDefibrillator; um único ponto
    /// evita que os dois divirjam (gate exigindo um item e o consumo cobrando outro).</summary>
    internal static class DefibrillatorItem
    {
        internal const string TemplateId = "5c052e6986f7746b207bc3c9";
    }

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

            var items = player.Profile.Inventory.GetAllItemByTemplate(DefibrillatorItem.TemplateId);
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

                // ref: item 013 §2 — espelhar os guards de abort do corpo do Fika
                // (ReviveInteractable.cs:221,229,231): `!success || !IsAlive` e alvo destruído
                // abortam o revive DEPOIS deste prefix. Sem estes checks o desfibrilador era
                // cobrado por um revive que não aconteceu (reanimador morto no último instante
                // do plant) — o aliado seguia caído e o item era perdido.
                if (player.HealthController == null || !player.HealthController.IsAlive) return;

                var observedPlayerField = AccessTools.Field(type, "_observedPlayer");
                if (observedPlayerField?.GetValue(__instance) == null) return;

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

            var items = player.Profile.Inventory.GetAllItemByTemplate(DefibrillatorItem.TemplateId);
            if (items != null && items.Any())
            {
                var defib = items.First();

                if (TrueTrauma.TraumaState.Logger != null) TrueTrauma.TraumaState.Logger.LogInfo("Consumindo Desfibrilador para reviver aliado!");

                // ref: item 013 — era Discard() SEM simulate + TryRunNetworkTransaction SEM callback:
                // a operação ficava em CommandStatus.Begin sem Succeed/Failed, então ItemView ligava
                // IsBeingRemoved e nunca desligava (ItemView.cs:578,596 / SlotView.cs:555-573) — item
                // piscando, inutilizável, slot travado (achado do 1º teste in-game). Mesmo padrão que
                // CR-04/CR-05 já abandonou no sistema de cura; o revive era o último ponto no antigo.
                // Reusa o descarte diferido/networked validado em 2 PCs. A espera de mãos da coroutine
                // é no-op benigno aqui (o reanimador está em Plant, não em MedsController) — spec §3.1.
                MedicalLogic.DiscardItemNetworked(player, defib);
            }
        }
    }
}
