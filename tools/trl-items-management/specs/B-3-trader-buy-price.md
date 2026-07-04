# B-3 · Editar preço de COMPRA do trader (buyback)

> **Status:** 🟡 Spec (SDD) — **implementação BLOQUEADA por decisão de produto** (ver §3) · **Data:** 2026-07-04 · **Ref:** [BACKLOG.md](../BACKLOG.md) B-3

## 1. Funcional

**Objetivo:** editar quanto o **trader paga** por um item (player vende pro trader) — complementar ao preço de venda (assort) já feito.

## 2. Técnico — o achado que muda tudo (pesquisa no SPT source)

**O servidor NÃO calcula o buyback no sell path — ele confia no preço que o cliente manda.**
- `TradeController.ConfirmTrading` (`references/spt-source/.../Controllers/TradeController.cs:48`, branch `sell_to_trader` :63-69) → `TradeHelper.SellItem` (`.../Helpers/TradeHelper.cs:251`).
- `SellItem` remove os itens do inventário (:291) e credita **o total que o cliente mandou** (:295): `paymentService.GiveProfileMoney(..., sellRequest.Price, ...)`.
- `sellRequest.Price` é **um `double?` para a requisição inteira** (`ProcessSellTradeRequestData.cs:8-9`), não per-item; `Items` é só `{id,count}` sem preço (:11-12).
- O **preço exibido** na tela "Vender" é calculado **100% no cliente EFT** (de `buy_price_coef` + handbook + condição). O único cálculo server-side (`TraderHelper.GetHighestSellToTraderPrice:485-508`) **não é chamado no sell** — só serve de piso pro flea.
- `buy_price_coef` é **por loyalty level** no trader base (`Trader.cs:184-185`), **não per-item** → confirma: buyback não está no assort; não dá pra fazer com a mutação estática de DB do mod de venda.

**Consequência:** um patch **server-only** (Harmony Prefix em `TradeHelper.SellItem`) muda **o dinheiro que o player recebe**, mas **NÃO o número exibido** antes de confirmar → **desync visível** (vê vanilla, recebe override). Pra display + recebido baterem, precisa de **patch client-side (BepInEx + Harmony)** no cálculo de preço do cliente.

## 3. Decisão: **Rota B escolhida** (2026-07-04) — client + server, UX coerente

| Rota | O que entrega | Custo |
|---|---|---|
| A · Server-only | recebido = override, **exibido = vanilla** (desync) | baixo, mas confuso |
| **B · Client + server** ✅ | **exibido E recebido = override** (coerente) | mod client BepInEx + prefix server, exige validação in-game |

Os dois lados leem o **mesmo override** → o número na tela de venda e o dinheiro creditado batem.

### Plano de implementação (Rota B)
1. **Config compartilhada:** arquivo novo `user/mods/TRLTraderPrices/config/buy-overrides.json`, mesma shape do sell (`traderId → tpl → { count, currency }`). O viewer escreve; server e client leem. (Separado do `overrides.json` de venda p/ não reestruturar o que já funciona.)
2. **Patch CLIENT** (pasta `modded/Client/` no TRLTraderPrices, padrão CustomClasses): **Postfix** (`ModulePatch`, `SPT.Reflection.Patching`) em **`TraderClass.GetUserItemPrice(Item item)`** — confirmado na DLL (`public GStruct300? GetUserItemPrice(Item item)`, chama `Info.ApplyPriceModifier`, retorna `new GStruct300(currencyId, amount)`). Se há override p/ (`__instance.Id`, `item.TemplateId`) e não-Fence → reescreve `ref GStruct300? __result = new GStruct300(currencyId, count)`. Struct: `GStruct300(MongoID? currencyId, int amount)`. Lê o override via `RequestHandler.GetJson("/trltraderprices/buy-overrides")`. Precedente do padrão: `Skills-Extended/.../GetBarterPricePatch.cs` (Postfix no sibling `GetBarterPrice`, mesma GStruct300).
3. **Prefix SERVER** (backstop no TRLTraderPrices, §4): garante o dinheiro creditado = override, caso o cliente mande o preço vanilla.
4. **UI:** tornar o "B" (coluna trader, hoje referência tarkov.dev) **editável** → PATCH `/api/trader-buy-price` → grava `buy-overrides.json`. (Nasce no viewer atual; migra pro mod no B-2 M1.)
5. **Validação in-game:** vender item com override → tela mostra o valor E o player recebe o valor.

## 4. Design (quando a decisão sair)

**Patch point server (comum às duas rotas):** Harmony **Prefix** em
```csharp
// SPTarkov.Server.Core.Helpers.TradeHelper
public void SellItem(PmcData profileWithItemsToSell, PmcData profileToReceiveMoney,
    ProcessSellTradeRequestData sellRequest, MongoId sessionID, ItemEventRouterResponse output)
```
- No **entry** os itens ainda existem em `profileWithItemsToSell.Inventory.Items` → mapear `sellRequest.Items[i].Id`→item→`.Template` (tpl) + `count`.
- `traderId = sellRequest.TransactionId`. Fence ou sem override → `return true` (vanilla, não toca `sellRequest.Price`).
- Se **todos** os itens vendidos têm override → `sellRequest.Price = Σ(override×count)` na moeda do trader (`trader.Currency`, `PaymentService.cs:193`), `return true`. Se **algum** não tem override → `return true` sem tocar (fallback seguro; o cliente só manda 1 preço agregado, o servidor não tem breakdown vanilla per-item confiável).
- Harmony é **necessário** (métodos não-virtual; DI `typeOverride` não intercepta — a chamada é por referência do tipo base). Precedente server-side: `mods/OutfitPersistenceFix/.../ProfileFixerCustomizationPatch.cs` + `References/0Harmony.dll` (o TRLTraderPrices.csproj já tem a linha do Harmony? conferir — a spec assume adicionar).
- **Config:** reusar o `config/overrides.json` do TRLTraderPrices, adicionando um bloco `buy` (`traderId→tpl→{count,currency}`) OU um arquivo `buy-overrides.json` irmão. Compartilhar o loader (`TRLTraderPricesMod.cs:88-91`).
- **Patch client (rota B):** espelhar o cálculo no cliente — investigar o método client de preço de venda-ao-trader (decompilado EFT), padrão `mods/*/modded/Client/Patches/*.cs`. **A validar no código client + in-game.**

## 5. Verificação
- Rota A: vender item com override → log do prefix mostra `sellRequest.Price` recomputado; player recebe o valor. **Display desync esperado** (documentar).
- Rota B: exibido = recebido = override in-game. **Exige cliente + raid.**
