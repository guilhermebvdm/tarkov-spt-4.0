using HarmonyLib;
using SPTarkov.Server.Core.Models.Common;            // MongoId
using SPTarkov.Server.Core.Models.Eft.Common;        // PmcData
using SPTarkov.Server.Core.Models.Eft.Common.Tables; // CustomizationItem
using SPTarkov.Server.Core.Services;                 // ProfileFixerService

namespace CustomizationPersistenceFix;

/// <summary>
///     Patch de <c>ProfileFixerService.FixProfileBreakingInventoryItemIssues(PmcData)</c> — o método que
///     o SPT chama no <c>/client/game/start</c> (via <c>GameController</c>, quando
///     <c>core.json → fixes.fixProfileBreakingInventoryItemIssues == true</c>).
///     <para>
///     Bug do SPT (4.0.x): a checagem de <c>Customization.Body/Hands/Feet</c> está com a lógica
///     <b>invertida</b> — usa <c>if (db.ContainsKey(piece))</c> em vez de
///     <c>if (!db.ContainsKey(piece))</c>. Resultado: toda peça <b>válida</b> é resetada para o default
///     da facção a cada login (<c>Head</c> usa o <c>!</c> correto, por isso é deixado intacto).
///     </para>
///     <para>
///     Correção: o <see cref="Prefix"/> faz snapshot de Body/Hands/Feet antes do método rodar; o
///     <see cref="Postfix"/> reescreve o valor final de cada peça com a lógica <b>correta</b>:
///     peça originalmente válida → preservada; peça inválida/ausente → default da facção (o que o SPT
///     pretendia fazer). Os demais reparos do método (dupes, tags, StackObjectsCount) são preservados.
///     </para>
///     ref: <c>references/spt-source/.../Services/ProfileFixerService.cs</c> (linhas ~170-204).
/// </summary>
[HarmonyPatch(typeof(ProfileFixerService), nameof(ProfileFixerService.FixProfileBreakingInventoryItemIssues))]
internal static class ProfileFixerCustomizationPatch
{
    // Nomes dos defaults por facção, exatamente como o SPT/EFT os busca na DB de customização.
    // ATENÇÃO: o feet default USEC tem typo na própria DB do EFT — "DefaulUsecFeet" (sem o 2º 't' de
    // "Default"). Mantido verbatim para casar com a entry real (senão FirstOrDefault devolveria null).
    private const string DefaultUsecBody = "DefaultUsecBody";
    private const string DefaultBearBody = "DefaultBearBody";
    private const string DefaultUsecHands = "DefaultUsecHands";
    private const string DefaultBearHands = "DefaultBearHands";
    private const string DefaultUsecFeet = "DefaulUsecFeet"; // typo proposital — assim está na DB do EFT
    private const string DefaultBearFeet = "DefaultBearFeet";

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

    // Priority.Last: roda por último entre os Postfixes deste método, de modo que, se outro mod de
    // customização (AllTheClothes, WTT-*) também patchar e tentar resetar a aparência aqui, a nossa
    // restauração tenha a palavra final. Não interfere em mods que setam a aparência em outros hooks.
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(PmcData pmcProfile, Snapshot __state)
    {
        var c = pmcProfile?.Customization;
        var db = CustomizationPersistenceFixMod.Db?.GetTemplates()?.Customization;
        if (pmcProfile is null || c is null || db is null)
        {
            return;
        }

        var isUsec = string.Equals(pmcProfile.Info?.Side, "usec", StringComparison.OrdinalIgnoreCase);

        // Reescreve o valor final com a lógica correta (válido → mantém; inválido → default da facção),
        // anulando o reset indevido que o método bugado aplicou às peças válidas. Head fica intocado.
        var preserved = 0;
        c.Body = ResolvePiece(__state.Body, db, isUsec ? DefaultUsecBody : DefaultBearBody, ref preserved);
        c.Hands = ResolvePiece(__state.Hands, db, isUsec ? DefaultUsecHands : DefaultBearHands, ref preserved);
        c.Feet = ResolvePiece(__state.Feet, db, isUsec ? DefaultUsecFeet : DefaultBearFeet, ref preserved);

        // Confirma no log que o patch disparou e quantas peças foram preservadas (diagnóstico — a 1ª
        // versão deste mod era um no-op silencioso por patchar o método errado).
        if (preserved > 0)
        {
            CustomizationPersistenceFixMod.Log?.Debug(
                $"[CustomizationPersistenceFix] {preserved} peça(s) de roupa preservada(s) no game/start (Body={c.Body}, Hands={c.Hands}, Feet={c.Feet})."
            );
        }
    }

    /// <summary>
    ///     Peça originalmente válida (existe na DB) → preserva o valor do jogador (desfaz o reset do SPT)
    ///     e incrementa <paramref name="preserved"/>. Peça inválida/ausente → resolve o default da facção
    ///     pelo nome. Se nem o default existir na DB (não deveria acontecer), mantém o valor original para
    ///     não introduzir um <c>null</c>/NRE.
    /// </summary>
    private static MongoId? ResolvePiece(
        MongoId? original,
        Dictionary<MongoId, CustomizationItem> db,
        string defaultName,
        ref int preserved
    )
    {
        if (original is { } value && db.ContainsKey(value))
        {
            preserved++;
            return original;
        }

        var fallback = db.Values.FirstOrDefault(x => x.Name == defaultName);
        return fallback is not null ? fallback.Id : original;
    }
}
