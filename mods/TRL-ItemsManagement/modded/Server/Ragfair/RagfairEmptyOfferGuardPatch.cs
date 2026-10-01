using System.Collections.Concurrent;
using System.Linq;
using HarmonyLib;
using SPTarkov.Server.Core.Extensions;         // IsTraderOffer/IsFakePlayerOffer
using SPTarkov.Server.Core.Models.Common;      // MongoId
using SPTarkov.Server.Core.Models.Eft.Ragfair; // RagfairOffer
using SPTarkov.Server.Core.Services;           // RagfairOfferService
using SPTarkov.Server.Core.Utils;              // RagfairOfferHolder
using TRLItemsManagement.Pricing;              // TraderPriceOnLoad.Log (logger estático reaproveitado)

namespace TRLItemsManagement.Ragfair;

/// <summary>
///     Estado e lógica compartilhada pelos 4 patches deste arquivo — todos os acessores de leitura de
///     ofertas da flea alcançáveis a partir de features de apresentação (busca, categorias, preço
///     médio, oferta específica por id, ofertas de um trader). Nenhum toca no ciclo de expiração
///     interno de <c>RagfairOfferHolder</c>: <c>FlagExpiredOffersAfterDate</c> só chama
///     <c>GetOffers()</c> internamente (mesma classe, não via <see cref="RagfairOfferService"/>) —
///     <c>GetOfferById</c>/<c>GetOffersByTemplate</c>/<c>GetOffersByTrader</c> não são usados por esse
///     mecanismo, então filtrá-los aqui não impede a limpeza natural de ofertas expiradas.
///     Ver 001-crash-flea-oferta-sem-itens-02-spec-tech.md §1 e
///     001-crash-flea-oferta-sem-itens-04-code-review-01.md (CR-01-01) para o raciocínio completo.
///     A causa raiz de uma oferta perder os itens após criada ainda não foi identificada (fora de
///     escopo deste item) — isto só impede o crash e loga o suficiente pra rastrear a origem depois.
/// </summary>
internal static class RagfairOfferGuardCore
{
    // ref: RagfairOfferHolder.cs:366-370 — mesmo tipo de guard que a vanilla já usa em
    // GetExpiredOfferItems, só que lá ela não impede o crash nos consumidores de apresentação.
    // ConcurrentDictionary: múltiplas buscas simultâneas (servidor Fika populoso) podem achar a
    // mesma oferta quebrada ao mesmo tempo — TryAdd é atômico, na pior hipótese loga 2x (aceitável
    // pela spec funcional), nunca lança sob concorrência. Mesmo idioma de RagfairOfferHolder.cs:24
    // (_expiredOfferIds), já usado pela própria SPT no mesmo subsistema.
    // Em memória apenas — reseta a cada restart do servidor (decisão assumida na spec funcional,
    // marcada para confirmação humana). Assume ofertas quebradas raras — ver §7 da spec técnica.
    private static readonly ConcurrentDictionary<MongoId, byte> LoggedOfferIds = new();

    // Fix 02 (produção continuou quebrando após o Fix 01): a PRÓPRIA ENTRADA da lista pode ser
    // `null` — não só "oferta com Items ruim", mas "a oferta em si é null" dentro de
    // List<RagfairOffer>. `x.Items` num `x` nulo dá o MESMO NullReferenceException, mesma
    // mensagem, mesmo stack — indistinguível do caso do Fix 01 só pelo log de erro. Se isso
    // acontecer, `offer.Items` dentro do IsBroken original também lançaria, e como IsBroken não
    // tinha seu próprio try/catch, a exceção subia pro Postfix, era capturada pelo catch de FORA
    // (logando "guard failed"), e a lista NUNCA era reatribuída — ou seja, saía sem filtro
    // nenhum, silenciosamente, exatamente o sintoma observado.
    private static bool _loggedNullOfferOnce;

    /// <summary>
    ///     Espelha a expressão que quebra na vanilla: RagfairCategoriesService.cs:66 faz
    ///     `x.Items.FirstOrDefault().Template`. Nunca deve lançar — qualquer exceção ao inspecionar
    ///     a oferta é tratada como "quebrada" (é exatamente o cenário que este guard existe pra
    ///     cobrir: não confiar em nenhuma invariante de uma oferta já comprovadamente corrompida).
    /// </summary>
    public static bool IsBroken(RagfairOffer? offer)
    {
        try
        {
            return offer is null || offer.Items?.FirstOrDefault() is null;
        }
        catch
        {
            return true;
        }
    }

    /// <summary>Filtra uma lista já materializada. Caminho comum (sem oferta quebrada): zero alocação extra.
    /// Cada item é isolado — uma oferta que dá erro ao ser inspecionada não derruba o filtro inteiro.
    /// A assinatura usa <see cref="RagfairOffer"/> não-anulável (mesma anotação do resto da API vanilla),
    /// mas <see cref="IsBroken"/>/<see cref="LogBrokenOfferOnce"/> tratam uma entrada realmente nula em
    /// runtime mesmo assim — a anotação de tipo não impede um valor nulo já ter entrado na coleção.</summary>
    public static List<RagfairOffer> FilterList(List<RagfairOffer> source)
    {
        var hasBroken = false;
        for (var i = 0; i < source.Count; i++)
        {
            if (IsBroken(source[i]))
            {
                hasBroken = true;
                break;
            }
        }

        if (!hasBroken)
        {
            return source;
        }

        var clean = new List<RagfairOffer>(source.Count);
        foreach (var offer in source)
        {
            if (IsBroken(offer))
            {
                LogBrokenOfferOnce(offer);
            }
            else
            {
                clean.Add(offer);
            }
        }

        return clean;
    }

    /// <summary>Filtra um <see cref="IEnumerable{T}"/> (GetOffersByTemplate/GetOffersByTrader retornam
    /// enumeráveis potencialmente lazy) — materializa uma vez só, mesma lógica de <see cref="FilterList"/>.</summary>
    public static List<RagfairOffer> FilterEnumerable(IEnumerable<RagfairOffer> source)
    {
        return FilterList(source as List<RagfairOffer> ?? source.ToList());
    }

    public static void LogBrokenOfferOnce(RagfairOffer? offer)
    {
        if (offer is null)
        {
            // Sem Id não dá pra dedupear por oferta — loga só a primeira ocorrência do processo.
            if (!_loggedNullOfferOnce)
            {
                _loggedNullOfferOnce = true;
                TraderPriceOnLoad.Log?.Error(
                    "[TRLItemsManagement] ragfair offer null (entrada nula na lista, não uma oferta com itens ruins) descartada da busca — sem id disponível pra rastrear.");
            }

            return;
        }

        if (!LoggedOfferIds.TryAdd(offer.Id, 0))
        {
            return; // já logada nesta sessão do processo
        }

        // offer.User é setado incondicionalmente em RagfairOfferGenerator.CreateOffer
        // (ref: RagfairOfferGenerator.cs:113-116), mas checa null aqui mesmo assim: IsTraderOffer()/
        // IsFakePlayerOffer() derreferenciam offer.User sem guard quando CreatedBy é nulo
        // (ref: RagfairOfferExtensions.cs:31) — o próprio propósito deste patch é não confiar em
        // invariantes de uma oferta já comprovadamente corrompida.
        var sellerType = offer.User is null
            ? "desconhecido(user=null)"
            : offer.IsTraderOffer() ? "trader" : offer.IsFakePlayerOffer() ? "npc-fake" : "player";

        TraderPriceOnLoad.Log?.Error(
            $"[TRLItemsManagement] ragfair offer sem itens descartada — id={offer.Id} " +
            $"tipo={sellerType} vendedorId={offer.User?.Id} apelido={offer.User?.Nickname} " +
            $"expira={offer.EndTime}");
    }
}

/// <summary>
///     Harmony Postfix em <c>RagfairOfferService.GetOffers()</c> — cobre busca (via
///     <c>RagfairOfferHelper.GetValidOffers</c>) e categorias (via <c>GetSpecificCategories</c>), os
///     dois caminhos identificados em 001-crash-flea-oferta-sem-itens-03-spec-tech-review-01.md
///     (PA-01-01). Não-virtual — Harmony é obrigatório, mesma razão de <see cref="Pricing.SellItemPatch"/>.
/// </summary>
[HarmonyPatch(typeof(RagfairOfferService), nameof(RagfairOfferService.GetOffers))]
internal static class RagfairEmptyOfferGuardPatch
{
    [HarmonyPostfix]
    private static void Postfix(ref List<RagfairOffer> __result)
    {
        try
        {
            if (__result is null)
            {
                return;
            }

            __result = RagfairOfferGuardCore.FilterList(__result);
        }
        catch (Exception ex)
        {
            TraderPriceOnLoad.Log?.Error("[TRLItemsManagement] ragfair empty-offer guard (GetOffers) failed: " + ex);
        }
    }
}

/// <summary>
///     ref: CR-01-01 — cobre a checagem de preço médio de um item (<c>RagfairController.GetItemMinAvgMaxFleaPriceValues</c>
///     → <c>GetAveragePriceFromOffers</c>), que lê via <c>RagfairOfferService.GetOffersOfType</c> →
///     <c>GetOffersByTemplate</c>, um caminho diferente de <c>GetOffers()</c> e não coberto por ele.
/// </summary>
[HarmonyPatch(typeof(RagfairOfferHolder), nameof(RagfairOfferHolder.GetOffersByTemplate))]
internal static class RagfairOfferHolderGetOffersByTemplateGuardPatch
{
    [HarmonyPostfix]
    private static void Postfix(ref IEnumerable<RagfairOffer>? __result)
    {
        try
        {
            if (__result is null)
            {
                return;
            }

            __result = RagfairOfferGuardCore.FilterEnumerable(__result);
        }
        catch (Exception ex)
        {
            TraderPriceOnLoad.Log?.Error("[TRLItemsManagement] ragfair empty-offer guard (GetOffersByTemplate) failed: " + ex);
        }
    }
}

/// <summary>
///     ref: CR-01-01 — cobre a listagem de ofertas de um trader específico.
/// </summary>
[HarmonyPatch(typeof(RagfairOfferHolder), nameof(RagfairOfferHolder.GetOffersByTrader))]
internal static class RagfairOfferHolderGetOffersByTraderGuardPatch
{
    [HarmonyPostfix]
    private static void Postfix(ref IEnumerable<RagfairOffer> __result)
    {
        try
        {
            if (__result is null)
            {
                return;
            }

            __result = RagfairOfferGuardCore.FilterEnumerable(__result);
        }
        catch (Exception ex)
        {
            TraderPriceOnLoad.Log?.Error("[TRLItemsManagement] ragfair empty-offer guard (GetOffersByTrader) failed: " + ex);
        }
    }
}

/// <summary>
///     ref: CR-01-01 — cobre a busca de uma oferta específica por id (<c>RagfairController.GetOfferByInternalId</c>
///     e qualquer outro consumidor de <c>GetOfferById</c>). Usado internamente por
///     <c>GetExpiredOfferItems</c> ([RagfairOfferHolder.cs:359]) — filtrar aqui é inofensivo pra esse
///     caminho: a oferta cai no branch "not found, skipping" em vez de "has no items, skipping" (mesmo
///     efeito prático, mensagem de log diferente); a remoção da oferta expirada não depende deste método.
/// </summary>
[HarmonyPatch(typeof(RagfairOfferHolder), nameof(RagfairOfferHolder.GetOfferById))]
internal static class RagfairOfferHolderGetOfferByIdGuardPatch
{
    [HarmonyPostfix]
    private static void Postfix(ref RagfairOffer? __result)
    {
        try
        {
            if (__result is null || !RagfairOfferGuardCore.IsBroken(__result))
            {
                return;
            }

            RagfairOfferGuardCore.LogBrokenOfferOnce(__result);
            __result = null;
        }
        catch (Exception ex)
        {
            TraderPriceOnLoad.Log?.Error("[TRLItemsManagement] ragfair empty-offer guard (GetOfferById) failed: " + ex);
        }
    }
}
