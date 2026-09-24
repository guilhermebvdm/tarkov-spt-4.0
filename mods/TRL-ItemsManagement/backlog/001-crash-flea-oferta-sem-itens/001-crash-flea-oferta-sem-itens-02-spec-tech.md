# 001 — Crash flea oferta sem itens · Spec Técnica

**Mod:** TRL-ItemsManagement
**Spec funcional:** [001-crash-flea-oferta-sem-itens-01-spec.md](001-crash-flea-oferta-sem-itens-01-spec.md)
**Criado:** 2026-09-23

> Este item é **server-side puro** (processo `SPT.Server`, não Assembly-CSharp/EFT cliente). Fonte primária de verdade: [references/spt-source/](../../../../references/spt-source/) (vendorizado, SPT 4.0.13, commit `c87cc3c6853c622fd2addaf961f58467cd9754f2` — ver [VENDORED.md](../../../../references/spt-source/VENDORED.md)). Toda referência cita `arquivo.cs:linha`.

## 1. Estratégia

> **Revisado após [review 01](001-crash-flea-oferta-sem-itens-03-spec-tech-review-01.md) (PA-01-01, aceito):** o alvo original (`RagfairCategoriesService.GetCategoriesFromOffers`) só cobria o crash já observado em produção. Existe um segundo caminho de crash real (`RagfairOfferHelper.PassesSearchFilterCriteria`, ver evidência abaixo) que não passa por aquele método. O alvo foi movido para a origem comum dos dois caminhos.

**Harmony Postfix** em `RagfairOfferService.GetOffers()` (sem parâmetros, retorna `List<RagfairOffer>`) — [RagfairOfferService.cs:L42-L44](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/RagfairOfferService.cs#L42-L44).

- **Por que este método e não `GetCategoriesFromOffers`:** `RagfairController.GetOffers` monta **dois** conjuntos de ofertas a partir de fontes diferentes ([RagfairController.cs:L80-L99](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/RagfairController.cs#L80-L99)):
  1. `result.Offers` (o que é exibido) vem de `RagfairOfferHelper.GetValidOffers` ([RagfairOfferHelper.cs:L58-L109](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/RagfairOfferHelper.cs#L58-L109)), que faz `offer.Items.FirstOrDefault()` (linha 74) e passa o resultado (`null` p/ oferta quebrada) a `PassesSearchFilterCriteria`, cujas linhas 902/908 ([RagfairOfferHelper.cs:L902](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/RagfairOfferHelper.cs#L902) e [:L908](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/RagfairOfferHelper.cs#L908)) derreferenciam `offerRootItem.Upd.StackObjectsCount` **sem null-check**, quando a busca usa filtro de quantidade (`QuantityFrom`/`QuantityTo`) — um segundo crash real, nunca observado em produção só porque nenhuma busca ainda usou esse filtro.
  2. `result.Categories` (o que o alvo original protegia) só roda quando `searchRequest.UpdateOfferCount == true`, e para qualquer busca que não seja linked/required search, `GetSpecificCategories` **descarta** `result.Offers` e reobtém a lista do zero via `ragfairOfferService.GetOffers()` ([RagfairController.cs:L256-L282](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/RagfairController.cs#L256-L282), linha 268).

  `RagfairOfferService.GetOffers()` é a origem comum de **ambos** os caminhos — confirmado por grep exaustivo de todos os call-sites reais (não só os dois acima): [RagfairController.cs:L268](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/RagfairController.cs#L268), [:L1142](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/RagfairController.cs#L1142), [RagfairOfferHelper.cs:L70](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/RagfairOfferHelper.cs#L70) (`GetValidOffers`), [RagfairRequiredItemsService.cs:L54](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/RagfairRequiredItemsService.cs#L54), [RagfairServer.cs:L111](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Servers/RagfairServer.cs#L111) e [:L131](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Servers/RagfairServer.cs#L131). Filtrar aqui protege os 6 de uma vez, incluindo o crash de `PassesSearchFilterCriteria` (o `offerRootItem` nunca mais será `null` porque a oferta já não chega até `GetValidOffers`).
- **Por que não patchear uma camada abaixo (`RagfairOfferHolder.GetOffers()`):** `RagfairOfferHolder.FlagExpiredOffersAfterDate` ([RagfairOfferHolder.cs:L393-L420](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/RagfairOfferHolder.cs#L393-L420)) chama `GetOffers()` **internamente, na mesma classe** (linha 397) para decidir quais ofertas expirar. Se o filtro entrasse nessa camada, uma oferta quebrada nunca seria flagada como expirada nem removida de `_offersById` — viraria uma "zumbi" permanente em vez de ser eventualmente limpa pelo ciclo de vida natural da SPT (que já tem seu próprio guard em `GetExpiredOfferItems`, [RagfairOfferHolder.cs:L366-L370](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/RagfairOfferHolder.cs#L366-L370)). Patchear na camada do `Service` preserva esse ciclo de vida intacto: `FlagExpiredOffersAfterDate` continua vendo (e eventualmente removendo) a oferta quebrada, enquanto todo consumidor de apresentação passa a recebê-la já filtrada. **Decisão consciente — não "corrigir" para a camada `Holder` achando ser mais direto.**
- **Por que Harmony, não DI `TypeOverride`:** `RagfairOfferService.GetOffers()` **não é `virtual`** (confirmado lendo [RagfairOfferService.cs:L42](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/RagfairOfferService.cs#L42)). O padrão de override por DI que o CompoundingPerf usa em `RagfairServer.Update()` (método `virtual`) não se aplica. Harmony é obrigatório — mesma justificativa já documentada no mod para `SellItemPatch`/`FleaFloorOverridePatch` — ver [SellItemPatch.cs:30-31](../../../modded/Server/Pricing/SellItemPatch.cs#L30-L31).
- **Por que Postfix, não Prefix:** `GetOffers()` não recebe parâmetros — não há nada para um Prefix filtrar antes da execução. Um Postfix reescrevendo `ref List<RagfairOffer> __result` **depois** do vanilla montar a lista (mas antes de qualquer consumidor lê-la) é suficiente e mais simples. É também o mesmo idioma já usado em [FleaFloorOverridePatch.cs:L30-L38](../../../modded/Server/Pricing/FleaFloorOverridePatch.cs#L30-L38) (`Postfix(MongoId tpl, ref double __result)`) — evita introduzir uma técnica de Harmony sem precedente no mod (a versão anterior desta spec usava `ref` num argumento de entrada, o que teria sido o primeiro uso desse idioma específico aqui).

## 2. Pontos de patch

| Alvo (spt-source) | Tipo | Motivo |
|---|---|---|
| [`RagfairOfferService.cs:L42-L44`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/RagfairOfferService.cs#L42-L44) (`GetOffers()`) | Postfix | Remove de `__result` qualquer oferta com `Items` nulo/vazio antes de qualquer um dos 6 consumidores (§1) processá-la; loga a primeira ocorrência de cada oferta descartada. |

**Evidência de suporte (não são pontos de patch, mas fundamentam o design):**

| Arquivo:linha | Papel |
|---|---|
| [`RagfairOffer.cs:L17`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Ragfair/RagfairOffer.cs#L17) | `Items` é `List<Item>?` — nulo é um valor válido do tipo, não só lista vazia. O guard cobre os dois casos. |
| [`RagfairOfferHolder.cs:L366-L370`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/RagfairOfferHolder.cs#L366-L370) | Precedente vanilla: a própria SPT já trata `offer.Items?.Count == 0` como caso conhecido em `GetExpiredOfferItems` (loga `Error` e pula) — mesmo nível de log (`Error`) replicado aqui por consistência. Esse guard vanilla só protege o **próprio ciclo de expiração** (não os consumidores de apresentação), por isso ele não elimina o crash que este item resolve — ver §1 para por que o patch não entra nessa mesma camada (`Holder`). |
| [`RagfairOfferGenerator.cs:L97-L117`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Generators/RagfairOfferGenerator.cs#L97-L117) (`CreateOffer`) | Prova de que uma oferta **não pode nascer** com `Items` vazio — `rootItem = details.Items.FirstOrDefault()` seguido de `Root = rootItem.Id` já lançaria NRE na criação se `details.Items` estivesse vazio. Confirma a causa raiz (spec funcional) como perda de itens **pós-criação**, não bug de geração — este patch é puramente defensivo/diagnóstico, não uma correção da causa raiz (fora de escopo, ver spec funcional). |
| [`RagfairOfferExtensions.cs:L24`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Extensions/RagfairOfferExtensions.cs#L24) (`IsTraderOffer`), [`:L54`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Extensions/RagfairOfferExtensions.cs#L54) (`IsFakePlayerOffer`) | Usadas no log de diagnóstico para classificar o tipo de vendedor. `IsTraderOffer()` deref `offer.User.MemberType` sem null-check quando `CreatedBy` é nulo (linha 31) — nosso código checa `offer.User is null` **antes** de chamar essas extensions (ver §5), para não introduzir uma segunda NRE dentro do próprio guard. |
| [`RagfairOfferHolder.cs:L393-L420`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/RagfairOfferHolder.cs#L393-L420) (`FlagExpiredOffersAfterDate`) | Chama `GetOffers()` internamente **na própria classe `Holder`**, não via `RagfairOfferService`. Prova de que patchear na camada `Service` (não `Holder`) não interfere no ciclo de expiração/limpeza natural — ver §1. |

## 3. Novas propriedades F12 (BepInEx)

N/A — este é um mod 100% server-side (processo `SPT.Server`), sem plugin BepInEx/cliente associado a este item. Não há F12/`ConfigurationManager` envolvido.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Server/Ragfair/RagfairEmptyOfferGuardPatch.cs` | CRIAR | Harmony Postfix que filtra ofertas sem itens do retorno de `RagfairOfferService.GetOffers()`, com log deduplicado por oferta. |

Nenhum outro arquivo precisa mudar: `TraderPriceOnLoad.OnLoad()` já registra `new Harmony("trlitemsmanagement.trl.buyprice").PatchAll(typeof(TraderPriceOnLoad).Assembly)` ([TraderPriceOnLoad.cs:L74](../../../modded/Server/Pricing/TraderPriceOnLoad.cs#L74)) — um `PatchAll` no assembly inteiro, que já é como `FleaFloorOverridePatch` (classe diferente, pasta diferente) é registrado hoje sem seu próprio `OnLoad`. Nossa nova classe `[HarmonyPatch]` é descoberta automaticamente pelo mesmo `PatchAll` assim que existir no assembly — não precisa de um novo `IOnLoad`.

O logging reutiliza o campo estático `TraderPriceOnLoad.Log` (mesmo padrão que `FleaFloorOverridePatch.cs:L42` já usa) — patches Harmony são estáticos e não recebem injeção de DI, então o mod já resolveu esse problema uma vez; reaproveitar evita duplicar o mecanismo.

## 5. Stubs de código

```csharp
// modded/Server/Ragfair/RagfairEmptyOfferGuardPatch.cs
using System.Collections.Concurrent;
using HarmonyLib;
using SPTarkov.Server.Core.Models.Common;      // MongoId
using SPTarkov.Server.Core.Models.Eft.Ragfair; // RagfairOffer
using SPTarkov.Server.Core.Services;           // RagfairOfferService
using TRLItemsManagement.Pricing;              // TraderPriceOnLoad.Log (logger estático reaproveitado)

namespace TRLItemsManagement.Ragfair;

/// <summary>
///     Guarda defensiva — Harmony Postfix em <c>RagfairOfferService.GetOffers()</c> (não-virtual;
///     Harmony é obrigatório, mesma razão de <see cref="Pricing.SellItemPatch"/>). Ponto escolhido por
///     ser a origem comum de todos os consumidores de apresentação da flea (busca, categorias, preço
///     médio) SEM tocar no ciclo de expiração interno de <c>RagfairOfferHolder</c>, que chama seu
///     próprio <c>GetOffers()</c> diretamente — ver 001-crash-flea-oferta-sem-itens-02-spec-tech.md §1.
///     A causa raiz de uma oferta perder os itens após criada ainda não foi identificada (fora de
///     escopo deste item) — isto só impede o crash e loga o suficiente pra rastrear a origem depois.
/// </summary>
[HarmonyPatch(typeof(RagfairOfferService), nameof(RagfairOfferService.GetOffers))]
internal static class RagfairEmptyOfferGuardPatch
{
    // ref: RagfairOfferHolder.cs:366-370 — mesmo tipo de guard que a vanilla já usa em
    // GetExpiredOfferItems, só que lá ela não impede o crash nos consumidores de apresentação.
    // ConcurrentDictionary: múltiplas buscas simultâneas (servidor Fika populoso) podem achar a
    // mesma oferta quebrada ao mesmo tempo — TryAdd é atômico, na pior hipótese loga 2x (aceitável
    // pela spec funcional), nunca lança sob concorrência. Mesmo idioma de RagfairOfferHolder.cs:24
    // (_expiredOfferIds), já usado pela própria SPT no mesmo subsistema.
    // Em memória apenas — reseta a cada restart do servidor (decisão assumida na spec funcional,
    // marcada para confirmação humana). Assume ofertas quebradas raras — ver §7 se isso mudar.
    private static readonly ConcurrentDictionary<MongoId, byte> LoggedOfferIds = new();

    [HarmonyPostfix]
    private static void Postfix(ref List<RagfairOffer> __result)
    {
        try
        {
            if (__result is null)
            {
                return;
            }

            var hasBroken = false;
            for (var i = 0; i < __result.Count; i++)
            {
                if (__result[i].Items is not { Count: > 0 })
                {
                    hasBroken = true;
                    break;
                }
            }

            if (!hasBroken)
            {
                // Caminho comum (sem oferta quebrada): zero alocação extra, __result inalterado.
                return;
            }

            var clean = new List<RagfairOffer>(__result.Count);
            foreach (var offer in __result)
            {
                if (offer.Items is { Count: > 0 })
                {
                    clean.Add(offer);
                }
                else
                {
                    LogBrokenOfferOnce(offer);
                }
            }

            __result = clean;
        }
        catch (Exception ex)
        {
            TraderPriceOnLoad.Log?.Error("[TRLItemsManagement] ragfair empty-offer guard failed: " + ex);
        }
    }

    private static void LogBrokenOfferOnce(RagfairOffer offer)
    {
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
```

## 6. Fluxo de dados

```
[A] Cliente do jogo → POST /client/ragfair/find
  → RagfairController.GetOffers(sessionID, searchRequest)      (RagfairController.cs:L80-L99)
       │
       ├─ result.Offers = GetOffersForSearchType(...) → RagfairOfferHelper.GetValidOffers(...)
       │       (RagfairOfferHelper.cs:L58-L109) chama ragfairOfferService.GetOffers() na linha 70
       │       └─ [B] RagfairEmptyOfferGuardPatch.Postfix(ref __result)  <-- PATCH ENTRA AQUI
       │              (dentro de RagfairOfferService.GetOffers(), chamado por QUALQUER consumidor)
       │       └─ .Where(offer => PassesSearchFilterCriteria(...))
       │              (RagfairOfferHelper.cs:L874-L977) — sem o patch, offerRootItem pode ser null
       │              aqui e crashar nas linhas 902/908 se a busca usar filtro de quantidade
       │
       └─ se searchRequest.UpdateOfferCount == true (RagfairController.cs:L96-L99):
              result.Categories = GetSpecificCategories(pmcProfile, searchRequest, result.Offers)
                   (RagfairController.cs:L256-L282)
                   ├─ linked/required search → offerPool = result.Offers (já filtrado acima)
                   └─ busca comum (maioria) → offerPool = ragfairOfferService.GetOffers()
                          (linha 268) — MESMO ponto [B], mesma proteção
                   → RagfairServer.GetAllActiveCategories(fleaUnlocked, searchRequest, offerPool)
                        (RagfairServer.cs:L96-L103 — repasse direto)
                   → [C] RagfairCategoriesService.GetCategoriesFromOffers(offerPool, ...)
                        (RagfairCategoriesService.cs:L20-L68) — .Where().GroupBy() vanilla,
                        agora nunca mais vê uma oferta com Items vazio (crash original observado)
  → [D] resposta HTTP → cliente renderiza a busca
```

Sem o patch, o crash observado em produção acontece em `[C]` (`NullReferenceException` na linha 66, stack trace `RagfairCategoriesService.<>c__DisplayClass2_0.<GetCategoriesFromOffers>b__0`) sempre que a busca tem `UpdateOfferCount=true` e o filtro `OfferOwnerType`/`fleaUnlocked` do próprio `.Where()` de `GetCategoriesFromOffers` deixa a oferta quebrada passar — o que explica a intermitência por jogador observada (cada jogador filtra a busca de forma diferente: aba "Todos"/"Jogadores"/"Traders"). Um segundo crash, nunca observado ainda, aconteceria em `PassesSearchFilterCriteria` (linhas 902/908) para qualquer busca com filtro de quantidade — coberto pelo mesmo patch em `[B]`, já que os dois caminhos passam pela mesma origem.

## 7. Riscos e dependências

- **Ordem de inicialização:** o patch só existe depois que `TraderPriceOnLoad.OnLoad()` roda `PatchAll` ([TraderPriceOnLoad.cs:L74](../../../modded/Server/Pricing/TraderPriceOnLoad.cs#L74)), que tem `[Injectable(TypePriority = OnLoadOrder.RagfairCallbacks - 1)]` — já garantidamente antes de qualquer geração/busca de flea. Não é uma dependência nova: `SellItemPatch` e `FleaFloorOverridePatch` já contam com essa mesma garantia hoje.
- **Falha graciosa se o alvo mudar:** se uma futura versão do SPT renomear `RagfairOfferService.GetOffers()` ou mudar sua assinatura, `PatchAll` loga uma falha (capturada pelo try/catch já existente em `TraderPriceOnLoad.OnLoad`) em vez de derrubar o boot do servidor — o comportamento volta a ser o bug atual (crash na busca), não regride para algo pior.
- **Sem conflito com CompoundingPerf:** o `RagfairCalmUpdates` do CompoundingPerf é um `TypeOverride` de DI em `RagfairServer` (classe diferente); `RagfairCategoriesService` não é tocada por ele. Harmony e DI `TypeOverride` são mecanismos independentes — nenhum dos dois mods precisa saber da existência do outro (satisfaz o corner case da spec funcional sobre não presumir presença/ausência de outros mods).
- **Débito de deploy pré-existente, não deste item:** produção do TRL-ItemsManagement está em v1.0.2 (repo local em v1.1.0 pelo `.csproj`) — deployar este patch leva junto o acúmulo de versões anteriores. Risco de deploy, não de código; já era conhecido antes deste item (ver memória do mod, pendência P-6.1).
- **Decisão pendente (marcada na spec funcional com `<!-- review: -->`):** `LoggedOfferIds` é in-memory, reseta a cada restart do servidor. Se a decisão mudar para "persistir entre restarts", o design muda de `ConcurrentDictionary` estático para algo persistido em disco — impacto de implementação não-trivial, então vale confirmar antes do `/code-mod`.
- **Premissa de volume ([review 01](001-crash-flea-oferta-sem-itens-03-spec-tech-review-01.md), PA-01-03):** `LoggedOfferIds` assume que ofertas quebradas são raras (premissa central deste item). Se a causa raiz revelar alta frequência, revisitar com uma política de expiração (ex: `ConcurrentDictionary` com timestamp + limpeza periódica) — não necessário no design atual porque o volume esperado é próximo de zero.

## 8. Checklist de implementação

- [x] Criar pasta `mods/TRL-ItemsManagement/modded/Server/Ragfair/`.
- [x] Criar `RagfairEmptyOfferGuardPatch.cs` com o conteúdo do stub (§5).
- [x] Build isolado do projeto server (`dotnet build TRLItemsManagement.csproj -c Release -p:SkipDeploy=true`) — compilou sem erro. `References/0Harmony.dll` precisou ser populado manualmente a partir do `.spt-path` configurado (`E:/Tarkov Red Line/BepInEx/core/0Harmony.dll`) antes do primeiro build funcionar — não é feito automaticamente pro tipo `server-csharp` (o `/compile-mod` ainda não suporta esse tipo).
- [ ] Confirmar via `/compile-mod TRL-ItemsManagement` que o boot do servidor não loga nenhuma falha nova relacionada ao Harmony (`PatchAll` continua aplicando `SellItemPatch` + `FleaFloorOverridePatch` + o novo patch sem erro).
- [ ] Validação por leitura de código ([review 01](001-crash-flea-oferta-sem-itens-03-spec-tech-review-01.md), PA-01-02): confirmar, com um pequeno programa/console standalone referenciando o DLL compilado, que chamar a lógica do Postfix com um `List<RagfairOffer>` montado à mão (uma entrada com `Items = new List<Item>()` entre outras válidas) devolve a lista sem a oferta quebrada e sem lançar.
- [ ] Validação em produção/staging (a validação real, dado que não há como forçar deterministicamente a causa raiz ainda desconhecida): após deploy, monitorar o log por `"[TRLItemsManagement] ragfair offer sem itens descartada"` — a primeira ocorrência sem nenhum crash de flea na mesma janela confirma o guard funcionando.
- [x] Atualizar `mods/TRL-ItemsManagement/backlog/mod-backlog.md` (status ⚪→🟢) — responsabilidade do `/code-mod`, não desta spec.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start/stop hooks (AP-01) | N/A | Mod 100% server-side; `RagfairOfferService.GetOffers()` roda em resposta a HTTP request, sem relação com `GameWorld`/raid. |
| 2 | Filtro MainPlayer/Fika em patch reativo a ação de player (AP-02) | N/A | Não é um patch client-side reativo a input de jogador; roda no servidor pra qualquer request de busca, de qualquer jogador conectado — não há "MainPlayer" no contexto server. |
| 3 | Alvo virtual/ofuscado auditado (AP-03) | N/A | `GetOffers()` não é `virtual` (RagfairOfferService.cs:L42) e não é tipo ofuscado (SPT server é código-fonte limpo, não Assembly-CSharp decompilado) — não há overrides a auditar. Todos os 6 call-sites do método foram enumerados por grep (§1), não só o caller que gerou o crash observado — auditoria equivalente à exigida pra alvo virtual, feita por outro motivo (completude de cobertura, não dispatch). |
| 4 | Mutação via API canônica, side-effects mapeados (AP-04) | ✅ | Não mutamos estado do jogo/jogador — filtramos o parâmetro de entrada de um método de leitura antes de ele rodar. Nenhum side-effect da vanilla é pulado (§6). |
| 5 | Estado entre raids (raid1→raid2, alt-F4/morte/MIA) | N/A | Ver spec funcional — ofertas da flea vivem em memória do processo do servidor, não em escopo de raid. |
| 6 | Semântica/defaults de ConfigEntry (AP-05) | N/A | Sem `ConfigEntry` (§3). |
| 7 | Reentry-guard contra recursão (AP-07) | N/A | Postfix nunca invoca o método patcheado nem qualquer outro caminho que reentra nele; passe linear único sobre `__result` já pronto. |
| 8 | Flags/cache revalidados após troca de contexto (AP-08) | N/A | `LoggedOfferIds` é chaveado por `MongoId` (id da oferta) imutável, não por contexto que possa trocar (arma/operação/tela). |
| 9 | Patch-point reconfirmado no `.cs` real, não só recon (AP-09) | ✅ | `RagfairOfferService.cs:L42-L44` e todos os 6 call-sites citados em §1 lidos diretamente em `references/spt-source/` (código-fonte limpo, não precisa de deofuscação — AP-09 é sobre `GClassNNNN`, não aplicável aqui, mas a disciplina de "ler o arquivo real" foi seguida, inclusive além do que a v01 desta spec tinha coberto — ver review 01). |
| 10 | Skill EFT usada como lever confirmada não-inerte (AP-10) | N/A | Não envolve skill de personagem EFT. |
| 11 | Pacote FIKA próprio (AP-11) | N/A | Não declara `INetSerializable`, não envia nada pela rede FIKA. |
| 12 | Hot path sem alocação/LINQ desnecessária | ✅ | `for` simples na varredura (sem LINQ); zero alocação no caminho comum (sem oferta quebrada) — só aloca `List<RagfairOffer>` quando já vai logar um erro de qualquer forma (§5). |
| 13 | Threading / concorrência | ✅ | `ConcurrentDictionary.TryAdd` é atômico — cobre o corner case da spec funcional de duas buscas simultâneas acharem a mesma oferta quebrada sem lançar nem corromper o dedup. Mesmo idioma já usado pela própria SPT no mesmo subsistema (`ConcurrentDictionary<MongoId, byte> _expiredOfferIds`, [RagfairOfferHolder.cs:L24](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/RagfairOfferHolder.cs#L24)). Reforçado por lição já registrada na memória do mod (`memory/sessions.md:133`, sessão B-5/B-6): "qualquer estado compartilhado lido por um Harmony patch no caminho da flea... não é single-threaded" — mutação in-place de um dicionário compartilhado ali já causou uma race real (CR-01/04); por isso `ConcurrentDictionary` em vez de `Dictionary` simples aqui, desde o primeiro design. |
| 14 | Nullability / defensive coding | ✅ | `offer.User is null` checado antes de chamar `IsTraderOffer()`/`IsFakePlayerOffer()`, que não fazem esse guard internamente (RagfairOfferExtensions.cs:L31) — evita uma segunda NRE dentro do próprio guard. `__result is null` também checado no início do Postfix (defensivo, embora `GetOffers()` vanilla nunca retorne nulo — [RagfairOfferHolder.cs:L108-L111](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/RagfairOfferHolder.cs#L108-L111)). |
| 15 | Patch com try/catch + log (não lança sem querer) | ✅ | Corpo inteiro do Postfix em try/catch, log via `TraderPriceOnLoad.Log` em caso de falha — mesmo padrão de `SellItemPatch`/`FleaFloorOverridePatch`. |
| 16 | Sandbox: só `modded/` tocado | ✅ | Único arquivo novo em `modded/Server/Ragfair/` (§4); `original/` intocado. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-23 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-23 | Revisada após review 01 (PA-01-01/02/03 aceitos) — alvo do patch movido de `RagfairCategoriesService.GetCategoriesFromOffers` para `RagfairOfferService.GetOffers()` (Postfix em vez de Prefix); checklist de validação e nota sobre premissa de volume atualizados |
