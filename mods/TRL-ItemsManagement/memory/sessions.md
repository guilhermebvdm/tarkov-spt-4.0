# Memória de sessões — TRL-ItemsManagement

## Estado atual (snapshot ao fim da última sessão)

> ⚙️ Sessões 2-6 abaixo foram **reconstruídas a partir do `git log`** (commits `23cda449..27ff1781`), não vividas nesta conversa — outra(s) sessão(ões) fizeram esse trabalho entre 2026-07-17 e 2026-07-19. As mensagens de commit deste mod são incomumente ricas (rationale + validação), o que tornou a reconstrução confiável, mas o "porquê" registrado é o do autor do commit, não uma decisão presenciada.

- Versão local/repo: **v1.0.4**. Produção (100.106.152.7) segue em **v1.0.2** — todo o trabalho de v1.0.3 em diante (redesign do audit log+undo, B-4, B-5, B-6, overhaul de UX de trader, coluna TRADER) ainda não foi deployado. Ver P-6.1.
- Audit log: reescrito em **inglês** (revertendo a escolha em português da Sessão 1 — ver "Revisão de fato anterior" na Sessão 2), com deltas direcionais (▲/▼ além de cor), undo sem full-reload, a11y (focus trap, `aria-labelledby`), skeleton de loading. Race de leitura/escrita concorrente do `audit.jsonl` corrigida (`FileShare.ReadWrite`).
- B-5 (flea floor override por item, abaixo do piso trader×handbook) e B-6 (trader stock/buy-limit por ciclo) implementados e validados via API+Chrome — **sem validação in-game/in-raid** (mesma classe de risco residual do B-3/buyback). Ver P-6.2.
- Painel de trader redesenhado: accordion inline (sem popover flutuante, sem scroll horizontal), disable-sale (remove o item do assort real via `RemoveItemFromAssort`, validado in-game), flag de conflito flea-banido-mas-vendido-por-trader (185 itens reais encontrados) com filtro dedicado.
- Coluna TRADER redesenhada: group-avatars (até 5 traders por linha, melhor preço primeiro), popover de hover com lista completa S/B, filtro por trader `[S]`/`[B]`.
- 3 rodadas de code-review aplicadas no intervalo (CR-01..06 do B-5/B-6, CR-U-01..05 da UX de trader, CR-T-01..05 da coluna TRADER) — todos os achados aceitos e corrigidos, docs em `mods/TRL-ItemsManagement/docs/code-review-*.md`.

## Pendências / próximos passos conhecidos

- [P-1.2] (aberta 2026-07-15) `FleaCapController` não resincroniza `data/items.json` após um toggle de teto (mesmo "known interim gap" que existia pro flea-price/ban) — decisão consciente de não corrigir porque é um fix bulk/categórico (afeta categorias inteiras — Weapon Mod/Electronics — não um tpl isolado), documentado no próprio campo `note` da resposta do endpoint. Sem novidade nas sessões 2-6. Categoria: 🟢 ideia.
- [P-6.1] (aberta 2026-07-19) Produção (100.106.152.7) segue em v1.0.2; local/repo já em v1.0.4 com audit log redesign+undo, B-4, B-5, B-6, overhaul de UX de trader e coluna TRADER em cima, nada disso deployado ainda. Categoria: 🟡 débito.
- [P-6.2] (aberta 2026-07-19) B-5 (flea floor override) e B-6 (stock/buy-limit) só validados via API+Chrome, sem validação in-game/in-raid (mesma classe de risco residual do B-3/buyback, citada pelo próprio autor do B-5). Confirmar antes de produção. Categoria: 🟡 débito.

---

## 2026-07-15 22:25 (GMT-3) — Sessão 1: bug de cache stale no flea-price/ban + redesign do audit log com undo

**Tema central:** Diagnosticar e corrigir o bug de produção onde edições de preço de flea não refletiam no viewer após reload, estender o fix para todos os endpoints de escrita relevantes, e redesenhar a UX do audit log (feed legível + botão de desfazer).

**Decisões-chave:**
- Root cause do bug reportado: `data/items.json` (cache do mod, não o `ragfair.json` que o jogo lê) nunca era resincronizado pelas escritas — a escrita real (game-facing) sempre esteve correta, só o cache do viewer ficava stale. Fix: [`ItemCatalogPatcher.cs`](../modded/Server/ItemCatalogPatcher.cs) (write-back atômico compartilhado) chamado a partir de `FleaPriceController.SetVanillaItemPrice`/`SetModItemPrice`/`DeletePrice` ([FleaPriceController.cs:239](../modded/Server/Api/FleaPriceController.cs#L239) `PatchCatalogFleaReset`, [:317](../modded/Server/Api/FleaPriceController.cs#L317)/[:403](../modded/Server/Api/FleaPriceController.cs#L403) mutação direta do `sptBlock`).
- A pedido explícito do usuário ("análise minuciosa antes de implementar"), a mesma classe de bug foi buscada em todos os 7 endpoints de escrita — achada também em `BanController.cs` (fix: [`PatchCatalogBan`](../modded/Server/Api/BanController.cs#L129)). `FleaCapController` tem o mesmo sintoma mas afeta categorias inteiras (Weapon Mod ×6/Electronics ×11), não um tpl — decisão de **não** corrigir agora, documentada no `note` da resposta ([FleaCapController.cs:106](../modded/Server/Api/FleaCapController.cs#L106)) e registrada como P-1.2.
- Undo do audit log reusa os MESMOS endpoints de escrita que a UI principal (nunca um endpoint de "revert" dedicado) — a própria ação de desfazer vira uma nova entrada no log, preservando o histórico completo. Ver `describeAuditEntry()` em [index.html:2266](../modded/Server/wwwroot/index.html#L2266).
- Regra já estabelecida em rodadas anteriores desta mesma sessão ("só toca disco em mutação real") foi violada pelo primeiro rascunho do cache-patch de `DeletePrice` (escrevia mesmo em no-op) — corrigida separando `ComputeDefaultEffectiveFleaPrice` (puro, [FleaPriceController.cs:194](../modded/Server/Api/FleaPriceController.cs#L194)) de `PatchCatalogFleaReset` (escreve, só chamado no branch de remoção real, [FleaPriceController.cs:150](../modded/Server/Api/FleaPriceController.cs#L150)).

**Lições / hipóteses descartadas:**
- UI otimista via mock em `localStorage` (proposta do usuário) foi avaliada e descartada a favor do fix server-side (cache-patch): um mock local não resolve a causa raiz pra outros usuários/abas vendo o mesmo dado stale, e não sobrevive a troca de dispositivo/navegador.
- `?? new JsonObject()` como fallback pra `item["spt"]` ausente em `BanController.PatchCatalogBan` foi descartado durante o code-review — apagaria silenciosamente todos os outros campos `spt.*` cacheados do item (fleaPrice, fleaFloor, etc.) na próxima serialização; trocado por bail-out explícito (`return`) quando o bloco `spt` não existe ([BanController.cs:144](../modded/Server/Api/BanController.cs#L144)).
- Guard do botão "Desfazer" de `flea-price/set` checava `b.value == null` pra decidir entre DELETE (restaurar padrão) e POST (re-setar preço anterior), mas o branch POST precisa de `b.effectiveFleaPrice`, não `b.value` — campo errado checado, risco de mandar `Math.round(undefined)` = `NaN` pro servidor num edge case raro (item nunca teve preço cacheado antes do edit sendo desfeito). Corrigido em [index.html:2277-2282](../modded/Server/wwwroot/index.html#L2277).

**Atividade cronológica:**
1. Usuário reportou (com screenshots de produção v1.0.2) que editar o preço de flea de "TerraGroup storage room keycard" mostrava sucesso + entrada no audit log, mas o preço no viewer nunca atualizava mesmo com hard-reload (F5/Ctrl+F5/Ctrl+Shift+R) — diagnosticado o root cause do cache `data/items.json`.
2. Avaliada e descartada a ideia de UI otimista via mock local, a favor do fix server-side.
3. A pedido do usuário, feita análise sistemática de todos os 7 endpoints de escrita — achado o mesmo bug em `BanController`; `FleaCapController` identificado como caso bulk e deixado documentado, não corrigido.
4. Implementado `ItemCatalogPatcher.cs` + cache-patch em `FleaPriceController.cs` e `BanController.cs`; code-review dessa primeira leva achou e corrigiu 3 problemas (no-op write no `DeletePrice`, fallback perigoso no `BanController`, um one-liner de filtro no `AuditLogController` que não compilaria).
5. Pedido pelo usuário: code-review do cache-patch + redesign de UX do audit log (menos técnico, "premium" pra jogadores, mostrar antes/depois com clareza) + sugestões extras — usuário aprovou tudo, incluindo a ideia de um botão de desfazer.
6. Implementado o redesign completo: [`AuditLogController.cs`](../modded/Server/Api/AuditLogController.cs) (novo — `GET /audit-log`, filtro `tpl` agora aceita lista separada por vírgula), feed de atividade em `wwwroot/index.html`/`components.css` com ícones por feature, deltas de preço coloridos (`--color-up`/`--color-down`), nomes de trader resolvidos, tempo relativo em PT-BR (`auditRelTime`, deliberadamente separado do `fmtRelTime` em inglês usado no resto da UI), resumo colapsável de entradas `baseline-unknown-date`, busca por nome de item, e botão de "Desfazer" (`describeAuditEntry()` + `undoApplyFlea`/`undoApplyBan`/`undoApplyTrader`).
7. Bugs achados e corrigidos ao vivo via Chrome DevTools durante a implementação: pluralização "açãoões" (contagem de baseline), tempo relativo em inglês vazando numa UI em português.
8. Code-review final pedido pelo usuário ("implemente o code-review") — achados e corrigidos 4 bugs novos, todos testados ao vivo contra o servidor local (`D:\SPT`, rebuild via `compile-mod.sh`) com Chrome DevTools MCP:
   - Guard errado no undo de `flea-price/set` (ver Lições).
   - `renderBaselineSummary()` reconstruía o `innerHTML` do zero a cada chamada, o que zerava uma lista já expandida (ficava "aberta" mas vazia) — acontecia sempre que um `loadPage(true)` disparava com o resumo aberto (ex.: logo após um undo). Fix: repopula a lista imediatamente se já estava expandida ([index.html:2518](../modded/Server/wwwroot/index.html#L2518)).
   - `resolveTplFilter()` retornava `''` tanto pra "campo vazio" quanto pra "busca sem nenhum item correspondente", e o chamador tratava os dois como "sem filtro" — buscar um nome inexistente mostrava o feed inteiro sem filtro, não "nenhum resultado". Fix: retorna `{filter, noMatch}`, `loadPage` mostra o estado vazio direto sem nem chamar o backend ([index.html:2549-2594](../modded/Server/wwwroot/index.html#L2549)).
   - Toggle "Flea cap on/OFF" do topbar guarda estado numa closure privada (`initFleaCapToggle`); o undo de `flea-cap/set` chamava a API direto, sem avisar esse componente — depois de desfazer, o indicador do topbar ficava mostrando o valor pré-undo até o próximo clique manual (que aí computava o próximo estado errado, partindo do valor stale). Fix: hook `fleaCapSync` exposto pelo toggle, chamado pelo undo ([index.html:103-106](../modded/Server/wwwroot/index.html#L103), [:1057](../modded/Server/wwwroot/index.html#L1057), [:2355-2363](../modded/Server/wwwroot/index.html#L2355)).

**Pendências abertas nesta sessão:**
- [P-1.1] (aberta 2026-07-15) Redesign do audit log + undo implementado e validado só localmente — falta commit, bump de versão e deploy em produção. Categoria: 🟡 débito.
- [P-1.2] (aberta 2026-07-15) `FleaCapController` não resincroniza `data/items.json` após toggle — decisão consciente, revisitar se virar reclamação recorrente. Categoria: 🟢 ideia.

---

## 2026-07-17 23:26 (GMT-3) — Sessão 2: commit da Sessão 1 + polish pré-1.0.3 do audit log

> ⚙️ **Reconstruído a partir do `git log`** (commits `98c1fe9b`..`487e509c`) — não vivido nesta conversa. Ver nota no topo do arquivo.

**Tema central:** Commitar o trabalho da Sessão 1 (v1.0.2) e polir o redesign do audit log antes do lançamento como v1.0.3: corrigir uma race de concorrência real, fechar um gap de guard nos endpoints de "set", e revisar i18n/a11y/perf do feed.

**Decisões-chave:**
- **Revisão de fato anterior:** a Sessão 1 registrou a decisão de manter `auditRelTime` (tempo relativo) em português, deliberadamente separado do `fmtRelTime` em inglês do resto da UI, "pra não quebrar a convenção existente". O commit `93c653f6` reverteu essa decisão: o audit log virou "uma ilha em português dentro de um viewer em inglês" — unificado pra inglês (labels, descrições, toasts, formatação en-US) e `auditRelTime` foi deletado (era um clone do `fmtRelTime` compartilhado). Ou seja, a convenção "certa" era o INVERSO do que a Sessão 1 concluiu: o audit log deveria seguir o idioma do resto do viewer, não o idioma da conversa com o usuário.
- Race de concorrência real corrigida: `AuditLogService.Append` (`File.AppendAllText`) e `AuditLogController`'s GET (`File.ReadLines`) abriam o arquivo com `FileShare.Read` (mutuamente exclusivo no Windows) — um GET durante um append em andamento podia lançar `IOException` (fora do try/catch por linha → HTTP 500) ou o append podia ser negado e engolido pelo catch best-effort (**entrada perdida**, silenciosamente). Realista sob Fika co-op com 2 operadores (um lendo o log, outro editando). Fix: `FileShare.ReadWrite` dos dois lados (commit `fe665dcd`).
- Endpoints de "set" não tinham o guard de no-op que os de "delete" já tinham — reaplicar um valor já igual reescrevia arquivos grandes E logava uma entrada espúria "X → X" com um botão de Desfazer ativo. Pior caso: duplo-clique no toggle de ban reescrevia ~36 MB (items.json + cache) à toa. Fix: guard de before/after em `BanController`, `FleaCapController`, `FleaLevelController`, `FleaPriceController` (a cláusula do cache mantém o comportamento da Sessão 1 — um cache stale ainda cai no write que resincroniza) e `TraderPriceController` (`64c1abc5`).
- Undo sem reload: a Sessão 1 implementou o undo chamando `loadPage(true)` (reset completo do feed) — isso resetava o scroll, colapsava a paginação e apagava a classe `.is-undone`. Trocado por fade-in-place do card + prepend da nova entrada que o undo cria (`93c653f6`).

**Lições / hipóteses descartadas:**
- Auditoria em português "porque a conversa é em português" foi a escolha errada — a convenção de idioma de uma feature nova deve seguir a convenção JÁ ESTABELECIDA no resto da UI onde ela vive, não o idioma da conversa que a motivou. Ver "Revisão de fato anterior" acima.
- `--fg-faint` (usado pros timestamps do feed) falha WCAG AA pra texto pequeno (~cor do painel) — trocado por `--fg-secondary`. Lição de contraste a conferir em qualquer cor "apagada" nova.

**Atividade cronológica:**
1. `98c1fe9b` — regen de snapshots de dados do pipeline (trivial, sem decisão).
2. `3a5626c5` — viewer mostrava v1.0.2 no header enquanto o código já era 1.0.3 (bump esqueceu o `<span class="app-version">` hardcoded); `package-release.sh` ganhou um gate que falha se o header divergir do `<Version>` do csproj.
3. `7b24253f` — modal do audit log limitado a `min(880px, 94vw)` (a 96vw deixava um vão vazio grande, o feed é uma coluna vertical única).
4. `fe665dcd` — fix da race de concorrência (ver Decisões).
5. `64c1abc5` — guard de no-op nos endpoints de "set" (ver Decisões).
6. `93c653f6` — inglês, deltas direcionais ▲/▼ (não só cor — acessibilidade pra daltonismo), undo sem reload, a11y (focus trap + `aria-labelledby` nos 2 modais, chip de item operável por teclado), perf (cards via `DocumentFragment`, resumo de baseline só rebuilda quando a contagem cresce).
7. `487e509c` — CSS: contraste dos timestamps, focus rings em todos os controles do modal, skeleton de loading, remoção de um `grid-column` morto (`.audit-feed-item__raw`, inerte num container flex), alias de `--color-up`/`--color-down` pra `--color-success`/`--color-danger` (evita duplicação que diverge).

**Cross-refs:**
- Resolve parte de [P-1.1] (commit — v1.0.2 e o redesign entraram no histórico).

---

## 2026-07-18 03:35 (GMT-3) — Sessão 3: B-4 (copy flea price) + v1.0.4 + B-5 (flea floor override)

> ⚙️ **Reconstruído a partir do `git log`** (commits `54c12e8c`..`35cb4860`) — não vivido nesta conversa.

**Tema central:** Entregar B-4 (atalho de copiar preço de referência tarkov.dev/tarkov-market pro flea), aplicar uma rodada de review no audit log, subir a versão pra v1.0.4, e entregar B-5 (permitir preço de flea abaixo do piso trader×handbook, por item).

**Decisões-chave:**
- B-4 implementado com **escopo por item** (não o multi-select em lote do B-4 original em `tools/trl-items-management/BACKLOG.md`) — botões de "Copy from" no menu de ação da célula de flea, aplicam o preço com clamp automático em [floor, ceiling] (nota no toast em vez de erro 422 seco) e Undo que restaura o estado exato pré-edição.
- "Copy from" nasceu dentro do editor inline (atrás de "Edit price"), mas foi movido no mesmo dia pro menu de ação da célula — aplicar um preço de referência virou 1 clique em vez de 2 (`bee308cd`).
- **Bug recorrente da mesma CLASSE da Sessão 1** (cache otimista não atualiza um campo exibido): depois de mudar o preço de flea, a célula "FLEA SPT" continuava mostrando o valor antigo — ela renderiza `consolidated.priceFleaSpt`, que a atualização otimista nunca tocava. Fix: `applyFleaPrice`/`undoApplyFlea`/`restoreOverride` agora também sincronizam **o campo** `item.consolidated.priceFleaSpt` — nunca `item.consolidated` inteiro, que vem `undefined` de `/api/price` e apagaria o objeto + quebraria `renderRow` (`3d285356`). Regra afiada: ao fazer update otimista, sempre listar TODOS os campos exibidos derivados do dado que mudou, não só o óbvio.
- B-5 (flea floor override): SPT trava toda oferta dinâmica de flea no piso `handbook × trader buyback` (`TraderHelper.GetHighestSellToTraderPrice`, reaplicado em `RagfairPriceService.GetDynamicItemPrice`) — o override aditivo não consegue superar isso. Abordagem escolhida (da investigação prévia): Harmony Postfix cirúrgico (`Math.Min(vanilla, override)`, nunca pode LEVANTAR um piso) só pros tpls na whitelist — não o switch global `useTraderPriceForOffersIfHigher`, que reabriria o exploit flea→trader pro catálogo inteiro.

**Lições / hipóteses descartadas:**
- Confirma o padrão da Sessão 1: qualquer atualização otimista de UI precisa varrer TODOS os campos derivados exibidos, não só o campo "óbvio" que a feature edita — 2ª ocorrência da mesma classe de bug (cache/estado stale após update otimista) neste mod.
- B-5 rejeitou conscientemente o switch global `useTraderPriceForOffersIfHigher` como solução — resolveria o piso mas reabriria um exploit conhecido (flea→trader) pro catálogo inteiro, não só pros itens liberados.

**Atividade cronológica:**
1. `54c12e8c` — B-4: botões "Copy from Tarkov.dev/Tarkov-Market" no editor de flea, com clamp + Undo.
2. `bee308cd` — "Copy from" movido pro menu de ação da célula (1 clique); `applyFleaPrice()` extraída como path compartilhado.
3. `3d285356` — fix da célula FLEA SPT stale (ver Decisões) + 4 achados de review do audit log (baseline summary ficando visível junto do "sem resultados"; undo duplicando o card do topo em caso de no-op; focus trap pegando um "Load more" escondido; tooltip do `fmtRelTime` mostrando ISO cru em vez de data localizada).
4. `ba2667b6` — bump pra v1.0.4 (5 fontes de versão sincronizadas); `UPDATE-SERVER.md` atualizado, passo de backfill do audit log marcado como "só no 1º install".
5. `63d0303f` — B-5 backend: `FleaFloorOverridePatch` (Harmony Postfix), `FleaFloorOverrideStore` (`config/flea-floor-overrides.json`), `POST /price` ganha `AllowBelowFloor` (422 sem a flag, aplica com ela); UI: `confirmDialog` de "Allow & apply" quando o preço fica abaixo do piso.
6. `35cb4860` — `/debug/verify-price` ganha `floorLive` (piso pós-patch, via chamada real a `GetHighestSellToTraderPrice`) vs `floorVanilla` (cache do pipeline) — valida o override sem precisar abrir o jogo. Confirmado num item real (RedRebel): piso 1.659.945 → liberado pra 995.967 → volta a 1.659.945 ao remover a whitelist.

**Pendências abertas nesta sessão:**
- [P-3.1] (aberta 2026-07-18) B-5 (flea floor override) validado só via API — falta confirmação in-game/in-raid (o autor do commit já registrou isso como "mesma classe de risco residual do B-3/buyback"). Categoria: 🟡 débito. Consolidada em [P-6.2].

**Cross-refs:**
- Resolve o restante de [P-1.1] (bump de versão feito — v1.0.4). Deploy em produção segue pendente, ver [P-6.1] (Sessão 6).

---

## 2026-07-19 00:19 (GMT-3) — Sessão 4: B-6 (trader stock/buy-limit por ciclo) + review B-5/B-6

> ⚙️ **Reconstruído a partir do `git log`** (commits `91961edb`..`ecac2f71`) — não vivido nesta conversa.

**Tema central:** Implementar B-6 (editor de estoque de trader + limite de recompra por ciclo) e aplicar a rodada de code-review conjunta de B-5+B-6.

**Decisões-chave:**
- B-6 usa **mutação da assort viva no boot** (mesmo padrão do `SellPriceApplier` do flea), não um Harmony patch contínuo — decisão validada contra a **DLL 4.0 real decompilada**: `TraderAssortHelper.ResetExpiredTrader` clona o `Assort.Items` **vivo** a cada refresh (a versão 3.x tinha um snapshot "pristine" separado que era descartado — esse comportamento **mudou** na 4.0). Ou seja, uma mutação feita no boot **sobrevive** aos refreshes de ciclo, em vez de ser revertida — fato específico da 4.0, não generalizável de conhecimento de versões antigas do SPT.
- Dois campos independentes por `(traderId, tpl)`: `stock` (`Item.Upd.StackObjectsCount` — teto vitalício, erode com compras, **não** reabastece por ciclo) vs `buyLimit` (`Item.Upd.BuyRestrictionMax` — cota por ciclo de refresh, **reseta** a cada ciclo). `UnlimitedCount` é **só um flag de exibição do cliente** — nunca é lido no caminho real de compra (`TradeHelper` decrementa `StackObjectsCount` numérico incondicionalmente); setar `UnlimitedCount=false` ao aplicar um cap é só pra manter o "∞" da UI coerente com o valor real.
- Confirmado que a **flea está imune** ao exploit de estoque gigante: ofertas dinâmicas de flea usam quantidades pequenas e aleatórias por oferta, múltiplas ofertas, expiração/re-roll e decremento por oferta — não vulnerável ao mesmo jeito que o assort direto do trader. O único caminho de estoque gigante até a flea seria via `StackObjectsCount` de um trader espelhado pra ragfair — e esse já é coberto pelo mesmo cap do B-6.
- Review (CR-01/04) achou uma race de concorrência real: `FleaFloorOverrideStore.Map` (B-5) é lido pelas **threads paralelas** de geração de oferta do SPT (`RagfairOfferGenerator` roda via `Task.Factory.StartNew`) mas era mutado in-place — trocado por `volatile` + copy-on-write (clona + troca a referência atomicamente) em vez de mutar o dicionário compartilhado.

**Lições / hipóteses descartadas:**
- **Fato versionado de engine, não generalizável:** o comportamento de "boot mutation sobrevive a refresh" depende de `ResetExpiredTrader` clonar o assort VIVO na 4.0 — isso foi um comportamento diferente na 3.x. Qualquer suposição sobre o ciclo de vida do assort de trader precisa reconfirmar contra a build atual, não assumir de conhecimento de versões antigas.
- **Lição de threading específica do SPT:** qualquer estado compartilhado lido por um Harmony patch no caminho de geração de oferta da flea (`RagfairOfferGenerator`) roda em threads paralelas (`Task.Factory.StartNew`) — não é single-threaded como a maioria dos outros patches deste repo. Mutação in-place de um dicionário compartilhado ali é uma race de verdade, não teórica.

**Atividade cronológica:**
1. `91961edb` — B-6 backend: `StockController` (GET/PATCH/DELETE `trader-stock(-overrides)`, CRUD + audit, mesmo padrão do `TraderPriceController`); `DebugController.verify-price` reporta stock/buyLimit/unlimited vivos do assort. Validado localmente: PATCH stock=5/buyLimit=2 no Scav Case @ Therapist, confirmado no assort após restart.
2. `51bd907e` — B-6 UI: seção "Availability" no editor de trader (stock + buy/cycle ao lado do preço de venda), 1 Save que persiste só o que mudou, chip de estoque na célula do nome com badge "cap".
3. `ecac2f71` — review conjunta B-5+B-6 (`docs/code-review-b5-b6-flea-floor-stock.md`): CR-01/04 concorrência (ver Decisões); CR-02 paridade — `SetModItemPrice` não honrava `allowBelowFloor` (itens de mod eram clampados silenciosamente ao piso em vez do handshake 422); CR-03 DRY — extraído `Api/RawConfigStore.cs` e `Pricing/TplValidation.cs`, usados por `StockController`/`TraderPriceController`; CR-05 tooltip esclarecendo que o cap de estoque vale por tier de fidelidade; CR-06 verificado como não-aplicável (nenhum item tem o mesmo trader em múltiplos tiers).

**Pendências abertas nesta sessão:**
- [P-4.1] (aberta 2026-07-19) B-6 (stock/buy-limit) validado só via API+Chrome — falta confirmação in-game/in-raid. Categoria: 🟡 débito. Consolidada em [P-6.2].

**Cross-refs:**
- Mesma classe de pendência de validação in-game que [P-3.1] (B-5) — consolidadas em [P-6.2] na Sessão 6.

---

## 2026-07-19 03:29 (GMT-3) — Sessão 5: overhaul de UX de disponibilidade de trader (accordion, disable-sale, flag de conflito)

> ⚙️ **Reconstruído a partir do `git log`** (commits `00cffab9`..`763d2681`) — não vivido nesta conversa.

**Tema central:** Redesenhar o painel de trader a partir de feedback real de usuário (props escondidas atrás de "Edit", editor flutuante forçando scroll horizontal), adicionar toggle de desabilitar-venda, e sinalizar itens com conflito flea-banido/vendido-por-trader.

**Decisões-chave:**
- Redesign **motivado por feedback de usuário**, não por iniciativa própria do agente — os problemas citados (props escondidas, editor flutuante com scroll horizontal) vieram de uso real. Accordion inline substitui o popover flutuante (`display: contents` na `<tr>` pra o `<td>` ocupar a largura do grid e crescer com o conteúdo — uma `<tr>` real como grid item corta o conteúdo).
- Disable-sale precisa do `RemoveItemFromAssort` do próprio SPT pra remover de fato o item (+ filhos, barter, todos os tiers) do assort vivo do trader — só marcar um flag interno não bastaria (mesma classe de raciocínio do AP-04 do catálogo de antipatterns: mutação direta de estado vs. API canônica). **Validado in-game**, não só API+Chrome: Artem sumiu do assort do MAG5-60 após restart.
- Flag de conflito (item banido na flea mas ainda vendido por um trader — contraditório) surfaceou **185 itens reais** com esse problema de dados — volume grande o suficiente pra justificar um filtro dedicado, não só um ícone isolado.

**Lições / hipóteses descartadas:**
- Review (CR-U-01) achou uma falta de normalização de config: `disabled: true` deveria **descartar** `stock`/`buyLimit` da mesma entrada (evita um registro incoerente tipo "desabilitado mas com estoque configurado"); `disabled: false` sozinho devia ser rejeitado (usar DELETE pra limpar) em vez de aceito como um valor "inerte". Mesmo espírito da regra "só toca disco em mutação real" da Sessão 1, mas aplicada à COERÊNCIA do config, não só ao ato de escrever.

**Atividade cronológica:**
1. `00cffab9` — accordion inline (Availability: stock · N/cyc, ou "⊘ sale off", à ESQUERDA do preço — sem precisar abrir o editor pra ver); disable-sale (`RemoveItemFromAssort`, validado in-game); flag de conflito flea/trader (185 itens); reordenação das seções de detalhe do item pra bater com a ordem de uso real.
2. `d64e174a` — tooltips longos (ex. os popovers de ajuda de stock/buy-cycle) quebravam em uma linha gigantesca (`white-space: nowrap`); trocado por `normal` + `max-width: 280px`.
3. `619dd33f` — review da UX overhaul (`docs/code-review-trader-availability-ux.md`): CR-U-01 normalização de config (ver Lições); CR-U-02 tooltip do ⚠ nota quando todo vendedor é quest-locked (31 dos 185 itens flagados); CR-U-03 badge/override não aparece numa linha de trader desabilitada (preço riscado já basta); CR-U-04 tooltip clampa na viewport (`--tip-shift`); CR-U-05 o ⚠ virou clicável (`openConflictResolver` expande o item e abre o editor do primeiro vendedor).
4. `763d2681` — follow-up de feedback: o ⚠ agora deriva de `regularMoneySellers()` (exclui barter, Fence e traders desabilitados) — desabilitar todos os vendedores regulares LIMPA a flag; filtro dedicado "Trader conflict" no dropdown de Banned (184 itens); tooltip vira pra baixo perto do topo da tabela (senão ficava atrás do header sticky); `.col-name` para de cortar o tooltip do ⚠.

**Cross-refs:**
- Aplica o mesmo princípio do AP-04 (`docs/technical/spt-antipatterns.md`) — mutação direta de estado (flag interno) não basta, precisa do caminho canônico do jogo (`RemoveItemFromAssort`).

---

## 2026-07-19 05:40 (GMT-3) — Sessão 6: coluna TRADER com group-avatars, popover e filtro

> ⚙️ **Reconstruído a partir do `git log`** (commits `54242162`..`27ff1781`) — não vivido nesta conversa.

**Tema central:** Reformular a coluna TRADER (antes só "melhor S / melhor B") pra mostrar múltiplos traders por linha com avatares, popover de hover com a lista completa, e um filtro dedicado por trader.

**Decisões-chave:**
- Group-avatars: até 5 avatares sobrepostos por linha (linha S = quem compra do jogador mais barato primeiro; linha B = quem vende mais caro primeiro), melhor à direita junto do valor, chip "+N" pro resto. Overrides e vendedores desabilitados (Sessão 5) são resolvidos dentro de `sellSideRows()`/`buySideRows()` — um trader desabilitado some do avatar automaticamente.
- Filtro "Trader": cada trader ativo aparece 2x na lista (`[B]` e `[S]`), multi-select é OR; `selectedTraders` entra no cache de filtro + URL/localStorage (mesma convenção de persistência já usada pros outros filtros deste mod).
- Performance: `sellSideRows`/`buySideRows` rodavam O(itens) vezes por render/filtro — memoizadas por `_dataVersion` (sinal de invalidação que já existia desde o tracking de edição da Sessão 1/2). Custo de build medido em ~18ms depois do fix.

**Lições / hipóteses descartadas:**
- Nenhuma hipótese descartada nesta sessão — trabalho de feature + review, sem reversão de abordagem.

**Atividade cronológica:**
1. `54242162` — coluna TRADER: group-avatars, popover de hover (300ms, mesmo padrão do popover de recompensa pré-existente), filtro "Trader" `[S]`/`[B]`, sincronização com o disable de trader da Sessão 5 (avatar some, contagem do filtro atualiza).
2. `27ff1781` — review (`docs/code-review-trader-column-groupavatars.md`): CR-T-01 removido um `data-tip` duplicado (o popover de hover já cobre a mesma informação); CR-T-02a `positionPopover()` extraída e compartilhada entre o popover de recompensa e o novo popover de trader; CR-T-02b memoização de `sellSideRows`/`buySideRows` por `_dataVersion` (ver Decisões); CR-T-04 qualquer clique na tabela fecha o popover de trader; CR-T-05 scroll da roda também fecha ambos os popovers.

**Cross-refs:**
- Reutiliza `positionPopover()` do popover de recompensa pré-existente (DRY) — mesma disciplina de "wiring de dismissal explícito" que os modais da Sessão 2 (focus trap) e o tooltip da Sessão 5 (`--tip-shift`) já precisaram.
