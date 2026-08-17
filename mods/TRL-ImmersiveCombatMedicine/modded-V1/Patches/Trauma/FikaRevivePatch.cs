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

    /// <summary>Reflection do ReviveInteractable do Fika, resolvida UMA vez.
    /// ref: CR-01-02 — os lookups estavam sendo refeitos a cada revive dentro do Prefix; o padrão do
    /// repo é cachear MethodInfo/FieldInfo em static readonly (csharp-mod-best-practices §3).
    /// Tipo é `internal sealed` no Fika, daí o acesso por nome em vez de referência de assembly.</summary>
    internal static class FikaReviveReflection
    {
        internal const string TypeName = "Fika.Core.Main.Components.ReviveInteractable";

        internal static readonly Type Type = AccessTools.TypeByName(TypeName);
        internal static readonly FieldInfo LocalPlayerField = Type != null ? AccessTools.Field(Type, "_localPlayer") : null;
        internal static readonly FieldInfo ObservedPlayerField = Type != null ? AccessTools.Field(Type, "_observedPlayer") : null;
    }

    [HarmonyPatch]
    public class FikaReviveGetActionsPatch
    {
        static MethodBase TargetMethod()
        {
            if (FikaReviveReflection.Type == null) return null;
            return AccessTools.Method(FikaReviveReflection.Type, "GetActions");
        }

        [HarmonyPostfix]
        private static void Postfix(GamePlayerOwner owner, ref ActionsReturnClass __result)
        {
            // ref: CR-01-03 — corpo de patch sem try/catch: uma exceção aqui (inventário em estado
            // transitório durante o GetAllItemByTemplate) propagaria para o GetActions do Fika e o
            // jogador perderia o prompt de interação inteiro, não só a ação de revive.
            try
            {
                if (__result == null || __result.Actions == null) return;
                if (owner == null) return;

                if (!HasDefibrillator(owner.Player))
                {
                    // "Search" é literal hardcoded no Fika (ReviveInteractable.cs:176), não
                    // localizado — a comparação por nome é estável em qualquer idioma.
                    __result.Actions.RemoveAll(a => a.Name != "Search");
                }
            }
            catch (Exception ex)
            {
                TrueTrauma.TraumaState.Logger?.LogError($"FikaReviveGetActionsPatch: {ex.Message} — ações de revive seguem sem o gate do desfibrilador.");
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
            if (FikaReviveReflection.Type == null) return null;
            return AccessTools.Method(FikaReviveReflection.Type, "RevivePlayer");
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
                if (FikaReviveReflection.LocalPlayerField == null) return;

                var player = FikaReviveReflection.LocalPlayerField.GetValue(__instance) as Player;
                if (player == null) return;

                // ref: item 013 §2 — espelhar os guards de abort do corpo do Fika
                // (ReviveInteractable.cs:221,229,231): `!success || !IsAlive` e alvo destruído
                // abortam o revive DEPOIS deste prefix. Sem estes checks o desfibrilador era
                // cobrado por um revive que não aconteceu (reanimador morto no último instante
                // do plant) — o aliado seguia caído e o item era perdido.
                if (player.HealthController == null || !player.HealthController.IsAlive) return;
                if (FikaReviveReflection.ObservedPlayerField?.GetValue(__instance) == null) return;

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
