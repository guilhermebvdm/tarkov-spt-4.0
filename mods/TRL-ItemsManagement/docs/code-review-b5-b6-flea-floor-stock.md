# Code review — B-5 (flea-floor override) + B-6 (trader stock / buy-limit)

> **Data:** 2026-07-18<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [validacao-endpoints-api.md](./validacao-endpoints-api.md)<br>

---

**Escopo:** revisão crítica do código implementado para **B-5** (permitir preço de flea abaixo do
piso de trader-price, por item) e **B-6** (editar estoque `StackObjectsCount` e limite por ciclo
`BuyRestrictionMax` dos traders). Metodologia do `/code-review` (6 categorias × 4 impactos) aplicada
manualmente — este mod não usa o fluxo SDD com specs numeradas (`01-spec`/`02-spec-tech`/`05-asbuild`),
então o skill formal não roda; os checklists de `spt-mod-best-practices` e `csharp-mod-best-practices`
foram usados como base.

**Arquivos revisados:**
- B-5: `Pricing/FleaFloorOverridePatch.cs`, `Pricing/FleaFloorOverrideStore.cs`, `Api/FleaPriceController.cs` (branch floor), `Api/DebugController.cs` (floorLive), `Pricing/TraderPriceOnLoad.cs` (load)
- B-6: `Pricing/StockOverride.cs`, `Pricing/StockApplier.cs`, `Api/StockController.cs`, `Api/DebugController.cs` (stock rows), `Pricing/TraderPriceOnLoad.cs` (load), `wwwroot/index.html` + `components.css` (UI, commit `51bd907e`)

**Contadores:** 🔴 0 · 🟠 1 · 🟡 2 · 🟢 3

**Veredito:** sem bloqueadores — os itens podem fechar. CR-01 (🟠) é recomendado corrigir antes do
deploy pois é uma race confirmada por evidência do SPT source, com fix trivial. O restante é
débito/observação opcional.

---

## Resolução (2026-07-18) — todos aceitos e aplicados

O usuário aprovou aplicar todos os achados. Estado após rebuild + redeploy local (`D:\SPT`) +
validação via API/Chrome:

| ID | Resolução |
|---|---|
| **CR-01** | ✅ Aplicado. `Map` agora `volatile`; `Set`/`Remove` fazem copy-on-write (clone → swap de ref) em vez de mutar in-place. Validado: below-floor com whitelist grava, o patch lê o `Map` novo em runtime (`floorLive` cai), remove restaura. |
| **CR-02** | ✅ Aplicado (opção **a** — paridade completa). `SetModItemPrice` recebe `allowBelowFloor`, retorna o mesmo 422 `belowFloor` e chama `FleaFloorOverrideStore.Set`/`Remove`. Validado: mod item (Scav Case) agora 422 sem allow, e whitelist + `effectiveFleaPrice` não-clampado com allow. |
| **CR-03** | ✅ Aplicado. Extraídos `Api/RawConfigStore.cs` (I/O raw) e `Pricing/TplValidation.cs` (`IsHex24`); `StockController`/`TraderPriceController` e `StockApplier`/`FleaFloorOverrideStore` delegam. `TplPattern` one-liner mantido local (churn desproporcional). Build limpo (0/0); trader-stock/price CRUD validado. |
| **CR-04** | ✅ Aplicado junto ao CR-01 (`Math.Abs(existing - floor) < 0.5`). |
| **CR-05** | ✅ Aplicado. Label "Availability **per tier**" + tooltip explicando stock vs buy/cycle e a semântica por-tier. |
| **CR-06** | ✅ Verificado — **sem ação**. Medido no cache: **0 de 6270 itens** têm o mesmo trader vendendo em múltiplas tiers, então não há rows-irmãs. Um fix seria dead code e `rerenderTraderRowEl` não é tier-aware (o "fix" mostraria a tier errada se a premissa mudasse). Registrado como não-aplicável. |

---

## CR-01 · B/D — Concorrência · 🟠 Forte

**`FleaFloorOverrideStore.Map`: leitura sem lock no patch concorrente com mutação in-place**

**Local:** [`Pricing/FleaFloorOverridePatch.cs:35`](../modded/Server/Pricing/FleaFloorOverridePatch.cs#L35) (leitor) · [`Pricing/FleaFloorOverrideStore.cs:81,94`](../modded/Server/Pricing/FleaFloorOverrideStore.cs#L81) (escritor)

**Problema:** o `Postfix` lê `FleaFloorOverrideStore.Map` **sem lock**:

```csharp
if (FleaFloorOverrideStore.Map is { } map && map.TryGetValue(tpl, out var floor))
    __result = Math.Min(__result, floor);
```

enquanto `Set` (`Map[key] = floor`) e `Remove` (`Map.Remove(...)`) mutam **o mesmo dicionário
in-place** sob `lock (Gate)`. O lock protege escritor↔escritor, mas **não** escritor↔leitor — o
Postfix não pega o lock. O SPT gera ofertas de flea em paralelo (`RagfairOfferGenerator.cs:298-309`
→ `Task.Factory.StartNew` + `Task.WaitAll`), e cada task chama `GetHighestSellToTraderPrice`
(`RagfairPriceService.cs:93/318/557`) — logo o Postfix roda em N threads. Um `PATCH /price` com
below-floor (que chama `Set`) durante um refresh de flea muta o `Dictionary` (podendo redimensionar
os buckets internos) enquanto essas tasks fazem `TryGetValue` no mesmo objeto.

**Por que importa:** leitura×escrita simultânea num `Dictionary<,>` é comportamento indefinido — pode
lançar `InvalidOperationException` ou retornar valor inconsistente. O `try/catch` do Postfix evita
crash do servidor (a oferta cai para o piso vanilla nessa chamada), mas o comportamento está
objetivamente incorreto e o efeito é silencioso. Num servidor Fika com vários jogadores puxando o
flea, a janela de colisão com um edit via web é pequena mas real.

**Sugestão:** copy-on-write, como o `Load` já faz (`Map = map` — troca de referência é atômica). Em
`Set`/`Remove`, clonar → mutar a cópia → trocar `Map`:

```csharp
// Set
var copy = Map is null ? new Dictionary<MongoId, double>() : new Dictionary<MongoId, double>(Map);
copy[key] = floor;
Map = copy;              // atomic ref swap; readers see the old or new dict, never a half-resized one
```

O leitor sem lock passa a ser seguro (sempre vê um dicionário imutável e completo).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## CR-02 · C — Gap vs. escopo · 🟡 Médio

**Below-floor (B-5) não existe para itens moddados — clamp silencioso ao piso**

**Local:** [`Api/FleaPriceController.cs:431-434`](../modded/Server/Api/FleaPriceController.cs#L431)

**Problema:** `SetVanillaItemPrice` implementa o fluxo below-floor (422 com `belowFloor:true` → re-POST
com `AllowBelowFloor` → `FleaFloorOverrideStore.Set`). O branch de itens moddados `SetModItemPrice`
só faz `if (eff < floor) eff = floor;` — clampa silenciosamente e **nunca** chama `Set`. Um mod item
pedido abaixo do piso é aceito, mas aplicado/exibido no valor clampado, sem aviso e sem whitelist.

**Por que importa:** assimetria não documentada. O operador que baixa o preço de um item moddado
abaixo do piso vê "sucesso" mas o preço real fica no piso — sem o 422 nem o botão "allow below floor"
que o branch vanilla oferece. Confuso, embora mod items sejam nicho.

**Sugestão:** escolher uma de três — (a) espelhar o fluxo `allowBelowFloor` no branch mod (chamar
`Set` quando `eff` clampado < pedido e `allowBelowFloor`), (b) retornar o mesmo aviso `belowFloor` em
vez de clampar em silêncio, ou (c) documentar explicitamente (comentário + `note` na resposta) que
below-floor não se aplica a mod items e por quê.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (qual: a / b / c) _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## CR-03 · D — Arquitetura / DRY · 🟡 Médio

**I/O de config raw e validação de tpl duplicados entre controllers**

**Local:** [`Api/StockController.cs:156-221`](../modded/Server/Api/StockController.cs#L156) (helpers clonados de `TraderPriceController`) · `IsHex24` em [`StockApplier.cs:119`](../modded/Server/Pricing/StockApplier.cs#L119), [`FleaFloorOverrideStore.cs:119`](../modded/Server/Pricing/FleaFloorOverrideStore.cs#L119) + `TplPattern` regex em StockController/DebugController/FleaPriceController

**Problema:** `StockController` reimplementa `ReadRawForResponse`/`ReadRawForWrite`/`WriteRaw`
(tmp+rename, backup de corrupto) — admitido em comentário ("same shape/behaviour as
TraderPriceController's helpers"). `IsHex24`/`TplPattern` (validação de MongoId 24-hex) aparece
reescrito em ao menos 4 arquivos.

**Por que importa:** débito que cresce a cada feature de config (B-5, B-6 já herdaram). Um fix de bug
no I/O raw (ex.: encoding, lock) teria que ser replicado em N cópias.

**Sugestão:** extrair `RawConfigStore` (read-for-response / read-for-write / atomic-write, parametrizado
pelo path) + um `TplValidation.IsHex24` / regex compartilhado. Não bloqueia — mas vale antes que
surja a próxima feature de override.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## CR-04 · B — Bug latente · 🟢 Menor

**Comparação de `double` com `==` no no-op guard**

**Local:** [`Pricing/FleaFloorOverrideStore.cs:76`](../modded/Server/Pricing/FleaFloorOverrideStore.cs#L76)

**Problema:** `if (Map.TryGetValue(key, out var existing) && existing == floor) return;` compara dois
`double` com `==` exato. Hoje `floor` é sempre um rouble inteiro vindo de um preço já validado
(`price != Math.Floor(price)`), então bate — mas comparação exata de ponto flutuante é frágil se a
origem do valor mudar.

**Sugestão:** aceitável como está (valores inteiros). Se quiser blindar: `Math.Abs(existing - floor) < 0.5`.
Anotar como conhecido.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar · `[ ]` Rejeitar (dívida)

---

## CR-05 · F/E — Semântica não sinalizada · 🟢 Menor

**Stock por-tpl aplica a TODAS as loyalty tiers (cap por-tier, não total)**

**Local:** [`Pricing/StockApplier.cs:83-106`](../modded/Server/Pricing/StockApplier.cs#L83)

**Problema:** o loop aplica `StackObjectsCount` a todo root entry do tpl (todas as tiers). Se o
trader vende o tpl em L1 e L4, ambos recebem o cap — estoque efetivo = cap×tiers, não cap total. É a
mesma semântica do `SellPriceApplier` (preço por-tpl também é all-tiers), então é consistente, mas a
UI não avisa. Raro (poucos itens repetem tpl em tiers no mesmo trader).

**Sugestão:** nota curta no hint/tooltip do editor de estoque ("cap por tier de lealdade") ou no
comentário. Opcional.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar · `[ ]` Rejeitar (dívida)

---

## CR-06 · E — Refresh cosmético · 🟢 Menor

**Rows-irmãs (mesmo traderId+tpl, tiers diferentes) não re-renderizam juntas após save de estoque**

**Local:** [`wwwroot/index.html`](../modded/Server/wwwroot/index.html) — `openTraderEdit`/`reflectAll` só re-renderizam a row editada

**Problema:** após um save de estoque, só a `.trader-row` editada é re-renderizada. Se o mesmo
traderId+tpl aparece em outra tier (outra row), ela mostra o valor antigo até um reload — embora o
override (chave traderId+tpl, sem tier) já valha para ambas. Cosmético, herdado do padrão de preço
(B-3), não é regressão do B-6.

**Sugestão:** opcional — re-renderizar todas as `.trader-row[data-trader-tpl][data-trader-id]` que
casem, não só a editada.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar · `[ ]` Rejeitar (dívida)

---

## Pontos positivos (não requerem ação)

- Fluxo save/undo/optimistic/audit do B-6 validado end-to-end via API + Chrome (save grava config +
  audit, undo reverte, avisos de corner-case disparam, layout confere).
- `FleaFloorOverridePatch` usa `Math.Min` (só abaixa o piso) — um entry mal-configurado nunca quebra
  o pricing vanilla. `try/catch` + log no Postfix.
- B-6 não tem o problema de concorrência do CR-01: `StockApplier` só muta o assort no boot
  (single-threaded), sem leitor in-memory concorrente; o refresh re-clona o assort já mutado.
- Todos os writers de config usam tmp+rename atômico; no-op guards honram "só toca disco em mutação
  real".

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-18 | Guilherme | Criação — review de B-5 + B-6 (1 🟠, 2 🟡, 3 🟢; sem bloqueadores). |
| 2026-07-18 | Guilherme | Todos os achados aceitos e aplicados (CR-01..05) ou verificados sem ação (CR-06). Rebuild + redeploy local + validação via API/Chrome. |
