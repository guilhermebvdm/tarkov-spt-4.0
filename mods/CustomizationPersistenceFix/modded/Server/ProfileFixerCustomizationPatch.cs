using HarmonyLib;
using SPTarkov.Server.Core.Models.Common;       // MongoId
using SPTarkov.Server.Core.Models.Eft.Common;   // PmcData
using SPTarkov.Server.Core.Services;            // ProfileFixerService

namespace CustomizationPersistenceFix;

/// <summary>
///     Patch de <c>ProfileFixerService.CheckForAndFixPmcProfileIssues(PmcData)</c>. O método reseta
///     Body/Hands/Feet <b>válidos</b> para o default (bug do SPT). Capturamos os valores no Prefix e,
///     se ainda forem válidos na DB de customização, restauramos no Postfix — desfazendo apenas o reset
///     indevido. Peças realmente inválidas seguem o que o método decidiu (não pioramos esse caso).
/// </summary>
[HarmonyPatch(typeof(ProfileFixerService), nameof(ProfileFixerService.CheckForAndFixPmcProfileIssues))]
internal static class ProfileFixerCustomizationPatch
{
    internal struct Snapshot
    {
        public MongoId? Body;
        public MongoId? Hands;
        public MongoId? Feet;
    }

    [HarmonyPrefix]
    private static void Prefix(PmcData pmcProfile, out Snapshot __state)
    {
        var c = pmcProfile?.Customization;
        __state = new Snapshot { Body = c?.Body, Hands = c?.Hands, Feet = c?.Feet };
    }

    [HarmonyPostfix]
    private static void Postfix(PmcData pmcProfile, Snapshot __state)
    {
        var c = pmcProfile?.Customization;
        var db = CustomizationPersistenceFixMod.Db?.GetTemplates()?.Customization;
        if (c is null || db is null)
        {
            return;
        }

        // Restaura cada peça SE o valor original era válido (existe na DB) — reverte só o reset bugado.
        if (__state.Body is { } body && db.ContainsKey(body))
        {
            c.Body = __state.Body;
        }

        if (__state.Hands is { } hands && db.ContainsKey(hands))
        {
            c.Hands = __state.Hands;
        }

        if (__state.Feet is { } feet && db.ContainsKey(feet))
        {
            c.Feet = __state.Feet;
        }
    }
}
