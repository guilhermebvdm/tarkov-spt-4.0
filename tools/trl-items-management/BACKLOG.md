# TRL Items Management — Backlog / roadmap

> **Status:** 🟢 Vivo<br>
> **Última revisão:** 2026-07-11<br>
> **Objetivo:** validar o escopo de features levantadas antes de agendar/implementar. Nada aqui está em execução até validação explícita.

---

## Progresso — run autônomo /g-autodev (2026-07-04, branch `feat/trl-items-autodev`)

| Item | Estado | O que foi feito |
|---|---|---|
| **B-1** teto flea | ✅ **Implementado + testado** | endpoint `GET/POST /api/flea-cap` (dacd3b1) + toggle no topbar (34e395f). Testado: rota (checks.dat atualiza, mults preservados) + UI via Chrome MCP. **Pendente: validação in-game** (desligar → GPU > 2.178M no jogo). |
| **B-2** virar mod | 🟢 **Spec + SPIKE PROVADO** | spec (203b844) + spike (bf239e7): mod Sdk.Web (`SPTarkov.Server.Web`) serve `wwwroot/index.html` + controller `/api/ping` na Kestrel do SPT (6969) — verificado no boot (mod carregado, 200/200). **Falta: Milestone 1** (portar os endpoints do serve.js + servir o index.html real). |
| **B-3** buy price | ✅ **Implementado (Rota B)** | client (Postfix `TraderClass.GetUserItemPrice`) + server backstop (Prefix `TradeHelper.SellItem`, Harmony) + rota `/trltraderprices/buy-overrides` + UI do viewer ("B" agora editável, espelha o "S"). Build+deploy limpos (client `BepInEx/plugins`, server `user/mods`); boot do SPT confirmado sem erros; API do viewer testada ponta a ponta (Chrome DevTools). **Falta: validação in-game** (vender 1 item com override → exibido = recebido). |
| **B-4** bulk flea | 🟢 **Spec (adiada)** | spec pronta (abc03cd); depende do B-2 M1 (UI nasce no mod novo). |

## Baseline (já implementado)

- ✅ **Preço de VENDA do trader** (player compra do trader) — moeda nativa (₽/$/€/GP), **vanilla + mod** (items e traders). Mod `TRLTraderPrices` muta o `BarterScheme` do assort. Cobre os 4 cenários (vanilla×vanilla, vanilla×mod, mod×mod, mod×vanilla). **v1.1.0**, testado 8/8.
- ✅ **Preço do flea** (override em `configs/ragfair.json`, fórmula aditiva + piso + teto), **ban de item**, **nível do flea** — viewer escreve config + atualiza `checks.dat`.

---

## Itens a validar

### B-1 · Remover / expor o teto do flea
- **O quê:** hoje só **Weapon Mod (×6)** e **Electronics (×11)** têm teto — `configs/ragfair.json` → `dynamic.unreasonableModPrices`; o resto já é sem teto. Liberar/subir por categoria, ou expor como controle no viewer.
- **Viabilidade:** 🟢 baixa — **config puro, sem plugin**. Evidência in-game: GPU capada em handbook×11 = 2.178.000 (smoke-matrix) → o SPT aplica o teto desse arquivo. O mod só foi necessário pro **trader**; o flea é config.
- **Nota (achado 2026-07-03):** a "trava" na edição do viewer (`serve.js` rejeita `price > ceiling`, `serve.js:566-577`; input `max=ceiling`, `index.html:1839`) **não é limite nosso** — ela **espelha** o teto do SPT. O `fleaCeiling` é **derivado** de `unreasonableModPrices` pelo `load-spt`. Então mudar a config + regerar `items.json` → `fleaCeiling` vira `null` → **a trava do viewer some sozinha**. Doc: `docs/flea-override-plan.md:18`.
- **A validar:** confirmar in-game que desligar/subir `unreasonableModPrices` deixa o preço ultrapassar (checar se não há 2º limite no cliente EFT). Decidir: desligar global vs expor toggle por-categoria no viewer.

### B-2 · Transformar o tool num MOD do SPT
- **O quê:** hoje é app Node separado (`serve.js`) + a DLL companion. Virar **um único mod SPT** que serve a UI web pela HTTP do próprio SPT, faz as edições in-process e aplica os overrides. Instala em `user/mods/`, sobe junto com o SPT.
- **Viabilidade:** 🟡 alta (re-arquitetura). Ganhos: 1 instalação; **auto-start** (resolve [P-1.2] da memória); sem processo Node separado; sem `serve.js`/`update-vm.ps1`.
- **A validar:** um server mod SPT consegue registrar rotas HTTP + servir estáticos na porta do SPT? O que muda no deploy? Estratégia de transição (conviver com o Node atual durante a migração?). Onde ficam os dados (`items.json`, caches, `.env`).

### B-3 · Editar preço de COMPRA do trader (buyback) — ✅ Implementado (Rota B)
- **O quê:** complementar o de venda — editar quanto o **trader paga** por um item (player vende pro trader).
- **Contexto (assort):** o **assort** é o estoque de loja do trader (`assort.json`: `items` = o que vende + `barter_scheme` = preço/requisito de cada + `loyal_level_items` = loyalty). A feature de **venda** edita o `count` do `barter_scheme`. O **buy price NÃO está no assort** — o SPT o **calcula** (`handbook × buy_price_coef` por trader/loyalty × condição). Por isso o método é diferente.
- **Achado (pesquisa 2026-07-04):** o buyback é calculado **inteiramente no cliente** (`TraderClass.GetUserItemPrice`); o servidor só confia no `sellRequest.Price` agregado que o cliente manda (`TradeHelper.SellItem`). Um patch só-server mudaria o dinheiro recebido mas não o exibido (desync).
- **Implementação (Rota B — client + server, mesmo override):**
  - Config: `user/mods/TRLTraderPrices/config/buy-overrides.json` (mesma shape do sell: `traderId → tpl → {count, currency}`).
  - Client (`mods/TRLTraderPrices/modded/Client/`, novo projeto BepInEx): Postfix em `TraderClass.GetUserItemPrice` (padrão `GetBarterPricePatch` do Skills-Extended) — reescreve o preço exibido na tela de venda.
  - Server (`mods/TRLTraderPrices/modded/Server/`): Harmony Prefix em `TradeHelper.SellItem` (backstop — garante que o dinheiro creditado bate com o exibido) + `StaticRouter` servindo `/trltraderprices/buy-overrides` pro client ler o mesmo config.
  - Viewer: coluna "B" (antes só referência do tarkov.dev) agora **editável**, espelhando a UI/API do "S" (`PATCH /api/trader-buy-price`).
- **Validado:** build limpo (client+server), boot do SPT sem erros (Harmony aplicado, rota respondendo), round-trip da API do viewer via Chrome DevTools (editar → override aplicado com badge → restaurar → estado limpo).
- **Code review:** revisão adversarial encontrou 6 pontos (Fence escapando pro client via rota crua, client não validando moeda contra o trader real, schema `int`/`double` causando crash-e-trava do cache no client, célula "B" da lista principal não refletindo override, endpoint de reset-all sem botão, duplicação sell↔buy) — **todos corrigidos** e revalidados (build+boot+UI). Detalhe completo em [HANDOFF.md](HANDOFF.md) §7.
- **Pendente:** validação in-game (vender 1 item com override setado → confirmar que o preço exibido na tela E o dinheiro recebido batem com o override).

### B-4 · Bulk: copiar preço tarkov.dev / tarkov-market → override de FLEA
- **O quê:** multi-selecionar itens + ação "copiar preço [tarkov.dev | tarkov-market]" → aplica como override de **flea** (decidido: alvo = flea) **em massa**.
- **Viabilidade:** 🟢 média — os dados já estão no viewer (colunas dev/market). Falta: multi-seleção na UI + endpoint de **batch-PATCH** de flea.
- **A validar:** item sem preço dev/market → pular + reportar quantos. Item acima do teto (mods/electronics) → aplicar `min(preço, ceiling)` e avisar. Restart-para-aplicar continua valendo.
- **Depende de B-2** (é feature de UI → construir dentro do mod novo).

### B-5 · Piso de flea configurável por item (abaixo do buyback teórico do trader)
- **O quê:** hoje não dá pra colocar o preço de flea de um item abaixo de um **piso** que o próprio SPT impõe — mesmo com um override aditivo válido, a oferta final é re-elevada se o piso for maior. Pra alguns itens (rebalanceamento de economia) precisamos ir abaixo desse piso.
- **Achado (pesquisa 2026-07-07):** o piso **não lê nenhum override nosso** — é recalculado do zero em `TraderHelper.GetHighestSellToTraderPrice` (`references/spt-source/.../TraderHelper.cs:485-515`): `handbook do item × coeficiente de buyback (loyalty 0)`, o maior entre todos os traders, cacheado. Aplicado em `RagfairPriceService.cs:316-323`, gatilhado por `ragfair.json:dynamic.useTraderPriceForOffersIfHigher` (default `true`). Editar o override de buyback ("B", já implementado) **não afeta esse cálculo** — ele ignora nossos overrides e recomputa via handbook+coeficiente sempre.
- **Duas rotas identificadas, com raio de efeito bem diferente (verificado: `handbookHelper.GetTemplatePrice` é lido por ~10 sistemas — bot loot, Fence, geração de oferta do flea, recompensa de quest repetível, conversão de moeda, entre outros):**
  - **(a) Editar `handbook.json` do item** — muda o valor "canônico" do item no jogo inteiro (loot de bot, Fence, quest, moeda — tudo que hoje lê handbook pra esse item). Sem patch novo, mas exige escrever em mais um arquivo do SPT_Data (checks.dat, atomicidade) e reconferir a fórmula de compensação/teto do flea (também handbook-derivada).
  - **(b) Harmony novo em `TraderHelper.GetHighestSellToTraderPrice`** — override isolado, só pro piso do flea daquele item, sem tocar em mais nada. Patch que não existe hoje; nenhum precedente nesta sessão.
- **Proposta de UX:** quando o valor desejado cair abaixo do piso calculado, em vez de só recusar com 422, mostrar as duas opções lado a lado (com o raio de efeito de cada uma) e deixar o operador escolher por item — a intenção real ("esse item vale menos" vs. "só quero ele mais barato no flea") só o operador sabe.
- **Viabilidade:** 🟠 média — path (a) é config puro mas com blast radius grande; path (b) é patch novo, sem precedente. Nenhum dos dois tem spec ainda.
- **A validar:** decidir a forma do override do path (b) (fixo por tpl, ou `min(vanilla, override)`?); confirmar que a UI consegue mostrar as duas opções sem confundir com o teto (B-1, mecanismo diferente); levantar a lista completa dos ~10 consumidores de handbook price pra path (a) documentar o aviso certo.
- **Depende de B-2** (nasce dentro do mod novo, igual B-4).

### B-6 · Editar quantidade em estoque (`StackObjectsCount`) dos itens no assort dos traders
- **O quê:** hoje o `TRL-ItemsManagement` só edita PREÇO (sell/buy); a quantidade disponível pro jogador comprar (estoque do assort) não é editável. Traders de mod costumam vir com estoque "infinito" — na prática um valor gigante — e às vezes queremos limitar isso (rebalanceamento) ou o oposto, aumentar o estoque de um item vanilla que vem curto.
- **Achado (pesquisa 2026-07-08):** o campo é `Item.Upd.StackObjectsCount` (`double?`) dentro de `TraderAssort.Items[i].Upd` (`references/spt-source/.../Models/Eft/Common/Tables/Item.cs:133`) — mesmo objeto `Upd` que carrega `BuyRestrictionMax/Current` (limite de compra por refresh, conceito diferente, não confundir). "Estoque ilimitado" no SPT nativo é `UnlimitedCount:true` **combinado com** um `StackObjectsCount` gigante — o próprio código nativo usa `99999999` (`Generators/RagfairAssortGenerator.cs:73,137`), confirmando o palpite de "traders de mod habilitam ~99999". `Helpers/TradeHelper.cs:108,154,157,162` decrementa e checa `StackObjectsCount < buyCount` a cada compra sem um desvio óbvio por `UnlimitedCount` — ou seja, é o valor numérico gigante (não o bool sozinho) que evita esgotar o estoque na prática. **Não confirmado:** se existe algum ponto do fluxo de compra de trader (fora da geração de oferta do flea, que foi o único lugar verificado) que ignora o decremento quando `UnlimitedCount=true` sozinho — vale checar antes de especificar a UI/endpoint.
- **Proposta:** endpoint por-item análogo ao de preço (`PATCH /api/trader-stock {tpl, traderId, count}`, reaproveitando a infra já compartilhada — `TraderOverrideConfigParser`, `WriteLockService`, mutação em boot via um `IOnLoad` no padrão do `TraderPriceOnLoad`) **mais** um mecanismo de edição em massa direto por config (ex.: `config/stock-overrides.json` com um modo "aplicar a todos os itens de um trader" — `{"traderId": {"default": 99999999}}` ou um flag `"unlimited"` — em vez de exigir editar item por item na UI quando o objetivo é só "esse trader de mod fica com estoque infinito em tudo", caso comum).
- **Viabilidade:** 🟢 média — a infraestrutura de override (parser compartilhado, lock, mutação em boot) já existe no `TRL-ItemsManagement` e pode ser estendida; o ponto em aberto é só confirmar o comportamento de `UnlimitedCount` acima.
- **A validar:** confirmar se `UnlimitedCount=true` sozinho (sem o valor gigante) já basta em algum ponto do fluxo de compra de trader normal que a pesquisa não cobriu; decidir a forma do "modo em massa" (`default` por trader vs. lista explícita de tpls); checar se estoque tem alguma interação com loyalty level (múltiplas entradas do mesmo tpl em tiers diferentes).
- **Depende de:** nada bloqueante — o mod já está unificado (`mods/TRL-ItemsManagement/`, ex-B-2, Estágios 0-5 implementados) com a infra de override/lock pronta; esta feature é só mais um endpoint dentro do mod existente.

### B-7 · Portar o pipeline de dados (Node) inteiro pra C#, in-process no mod
- **O quê:** eliminar de vez a dependência de Node.js/`.js` no servidor — portar `load-spt.js`, `normalize.js`, `fetch-tarkov-dev.js`, `fetch-tarkov-market.js` e `refresh-item.js` (`tools/trl-items-management/scripts/`) pra C#, rodando in-process dentro do próprio mod (sem `Process.Start`, sem pasta externa nenhuma). É o "Milestone 2" que já estava registrado como opcional/adiado na arquitetura do B-2 (ver seção abaixo) — este item o formaliza com o que aprendemos desde então.
- **Achado que motivou revisitar isso agora (2026-07-11):** tentei aninhar o pipeline Node dentro de `user/mods/TRL-ItemsManagement/pipeline/` pra atender ao pedido "mod 100% dentro de BepInEx/plugins + SPT/user/mods" — o **SPT rejeitou o mod INTEIRO no boot**. Mensagem do `ModValidator` é enganosa ("feito para servidores pré-4.0.0"), mas a causa real foi confirmada por teste isolado (mover a pasta pra fora do install e reiniciar corrigiu): **qualquer arquivo `.js`/`.ts` em qualquer lugar da pasta instalada do mod trava o `ModValidator` inteiro** — o mesmo motivo, já descoberto na pesquisa original do B-2, por que o `wwwroot/index.html` do viewer já é 100% inline (nunca separado em `.js` próprio).
- **Achado adicional que reforça a motivação (bug real, corrigido nesta sessão):** `load-spt.js` tem sua PRÓPRIA lógica de descoberta de trader (escaneia padrões de pasta `db/base.json`, `db/traders/<id>/base.json`) — **diferente e mais frágil** que a fonte de verdade que o resto do mod já usa (`DatabaseService.GetTraders()`, populado pelo próprio SPT). O trader "Trudy" (mod `c11-tn-4`/True North, pasta `db/trudy/`) não batia em nenhum dos 2 padrões conhecidos, ficando invisível no catálogo até o fix (commit `979562e`). Portar pra C# eliminaria essa categoria de bug inteira, lendo da MESMA fonte que já funciona pro resto do mod — não seria só um port mecânico, seria estruturalmente mais simples nesse ponto específico (sem re-parsear arquivo que o C# já tem tipado em memória).
- **Escopo (arquivos a portar):**
  - `load-spt.js` — o mais complexo: lê itens/traders(vanilla+mod)/ragfair(overrides+multiplicadores+blacklist)/handbook/quests do SPT, calcula bônus/piso/teto de flea por categoria, monta ofertas de trader com barter/quest-lock/dedup.
  - `normalize.js` — reconcilia SPT × tarkov.dev × tarkov-market num catálogo só, com regra de prioridade (market > dev-avg24h > dev-lastLow > spt) que **já divergiu uma vez no passado** (aviso no próprio código-fonte sobre isso).
  - `fetch-tarkov-dev.js` / `fetch-tarkov-market.js` — chamadas HTTP (GraphQL/REST) — port mais mecânico via `HttpClient`.
  - `refresh-item.js` — refresh de 1 item, reusa as peças acima.
- **Ganhos:** (a) resolve a trava do `ModValidator` — mod cabe 100% em `BepInEx/plugins` + `SPT/user/mods`; (b) elimina Node.js como dependência de runtime do servidor (não precisa mais documentar "instale Node LTS" no `DEPLOY.md`); (c) fonte de dado única, sem 2ª implementação de descoberta de trader/item; (d) simplifica MUITO o deploy tooling (sem `-ToolDir`, sem `config/pipeline.json`, sem sync separado em `package-release.sh`/`update-vm.ps1`).
- **Custo/risco:** lógica de negócio genuína e não-trivial (fórmula de preço, montagem de oferta com barter/quest-lock/dedup, reconciliação com histórico de divergência); validação exige comparar o catálogo INTEIRO (milhares de itens) contra a saída atual do Node antes de aposentá-lo — não é um teste rápido. Esforço de múltiplas sessões, não uma tarde.
- **Proposta de execução:** começar por `load-spt.js` (onde mora o bug de descoberta de trader — maior ganho imediato de correção), depois `normalize.js`, por último os scripts de fetch (mais mecânico, menor risco). Cada etapa validada bit-a-bit contra a saída do Node atual antes de decidir aposentar aquele script especificamente.
- **Viabilidade:** 🟠 esforço médio-alto, ganho real (não só estético) — reduz risco operacional (bug de trader + dependência externa). Não é urgente/bloqueante hoje: a arquitetura atual (pipeline externo) já funciona em produção, só não fica 100% dentro das pastas padrão do SPT.
- **Depende de:** nada bloqueante — pode começar a qualquer momento.
- **Decisão pendente:** critério objetivo de "pronto pra aposentar o Node" (ex.: catálogo C# bate 100% com o catálogo Node atual, rodado lado a lado por N rescans sem divergência, antes de remover o script correspondente).

### B-8 · Refatorar 100% do layout do viewer web pra usar os componentes/design system canônico do TRL
- **O quê:** hoje `wwwroot/{index.html, components.css, tokens.css}` do `TRL-ItemsManagement` usa CSS/tokens ad-hoc, próprios desse mod (herdados do `serve.js` original) — não os componentes/tokens canônicos do **TRL Design System** (`design-system/`, v1.0.0, já entregue — ver [[project_trl_design_system]]). Substituir 100% do layout (topbar, filtros, tabela de itens, modais de edição, badges, toasts) pelos componentes/tokens do DS.
- **Contexto:** o DS já tem "próxima fase = refatorar mods" declarada; `CustomClasses` é outro candidato na mesma fila (bridge `--mud-palette-*` → tokens). Este item formaliza o `TRL-ItemsManagement` como mais um alvo dessa fase.
- **Restrição técnica que precisa ser respeitada (mesma trava do B-7, achado 2026-07-11):** nenhum arquivo `.js`/`.ts` pode existir em lugar nenhum da pasta instalada do mod (`ModValidator` rejeita o mod inteiro) — se o DS distribuir componentes como módulos/arquivos `.js` separados, eles precisam ser **inlined** no próprio `wwwroot/index.html` (mesma razão pela qual o `index.html` de hoje já é 100% inline). CSS/assets (fonte, ícones) como arquivo separado continuam seguros — só `.js`/`.ts` que não pode.
- **Escopo a confirmar:** quais componentes do DS já existem prontos pra reuso direto (botão, input, dropdown, tabela, badge, toast, modal) vs. o que precisaria ser criado; se o DS já suporta light/dark e como isso se aplica a uma UI hoje single-theme; usar a skill `trl-ds-validation` (4 lentes: readability, a11y, i18n PT-BR/EN, dataviz) como critério de aceite.
- **Viabilidade:** 🟡 a definir — depende do estado real atual dos componentes do DS (não verificado nesta sessão); esforço provavelmente comparável a uma reescrita de UI completa (índice de ~6 mil itens, filtros, tabela, modais de edição, toasts).
- **Depende de:** nada bloqueante tecnicamente, mas faz sentido esperar o DS amadurecer via outro mod primeiro (ex. CustomClasses, já citado como próximo na fila) pra não ser o primeiro consumidor descobrindo gaps do DS.
- **A validar:** inventário completo do que o DS já oferece vs. o que esse viewer precisa; decidir se faz sentido portar em fases (tokens primeiro, componentes depois) ou tudo de uma vez.

### B-9 · Filtrar apenas itens vanilla (sem mod nenhum)
- **O quê:** hoje o filtro "Mod" (dropdown no topbar, `buildModFilter`/`selectedMods`) só filtra POR um mod específico (agrupa por `modSource`) — não há como fazer o inverso: mostrar **só os itens vanilla** (base do EFT/SPT, sem `modSource`). Útil pra revisar/editar preço/estoque só dos itens originais, sem o ruído dos itens adicionados por mods (WTT-Artem, WTT-PackNStrap etc.).
- **Como (provável):** adicionar uma opção **"Vanilla"** (ou "sem mod") ao próprio dropdown "Mod" — selecionada, o filtro casa `it => !it.modSource`, com contagem própria. Reaproveita 100% a infra do filtro Mod atual (é literalmente o inverso do que ele já faz); um valor sentinela (ex. `__vanilla__`) em `selectedMods`, tratado no ramo de Mod do `filterItems`, evita um dropdown novo.
- **Esforço:** pequeno — UI-only (`index.html`), sem backend. O dado (`it.modSource` presente/ausente) já está no cache e o badge "MOD" na listagem já distingue os dois grupos.
- **Depende de:** nada — o filtro Mod já existe; esta feature é só o complemento "nenhum mod".

### B-10 · Toggle "exclusivo" no filtro Trader
- **O quê:** o filtro "Trader" (`buildTraderFilter`/`selectedTraders`, opções `[S]`/`[B]` por trader) hoje é **OR** — o item aparece se QUALQUER trader selecionado o oferece. Falta um modo pra achar exclusividades.
- **Decisão travada (2026-07-25):** o toggle é **exclusivo** — mostra só itens que **nenhum trader de fora** oferece (o conjunto de traders que oferecem o item ⊆ conjunto selecionado). Fala dos traders de FORA, não exige que todos os selecionados vendam. Com 1 trader selecionado = "só ele oferece". (Descartados: AND/interseção e "exatamente os selecionados".)
- **Como (provável):** um toggle no widget do filtro Trader (ex.: chip "só estes" ao lado do dropdown) que muta o ramo de Trader do `filterItems`: em vez de "algum selecionado oferece", passa "todos os que oferecem estão na seleção". Reaproveita `sellSideRows`/`buySideRows` (já dão o conjunto S/B por item, já excluindo disabled/Fence/barter).
- **Esforço:** pequeno-médio — UI-only (`index.html`), sem backend.
- **Depende de:** nada — o filtro Trader já existe (v1.1.0).

### B-11 · Editar o loyalty level (LL) de venda de um item por trader
- **O quê:** o editor de trader (accordion em TRADER SELL) hoje edita preço / estoque / buy-limit / disable-sale, mas **não** o **loyalty level** em que aquele trader vende o item. Poder mover um item de, ex., LL1 → LL3 na loja do trader (rebalanceamento de acesso).
- **Decisão travada (2026-07-25):** escopo = **só mudar o LL** de uma venda existente (não adicionar/remover item do trader — isso é o disable-sale, já feito). O alvo é entre os LLs que o trader tem (tipicamente 1–4).
- **Como (provável):** backend muta `trader.Assort.LoyalLevelItems[<rootId>]` no boot, mesmo mecanismo do `StockApplier` (mutação do assort live, persiste no refresh, reseta no restart) — provável novo campo no `stock-overrides.json` (ex.: `loyaltyLevel`) ou um `loyalty-overrides.json` próprio + endpoint. UI: um campo/dropdown LL na seção Availability do editor accordion. "Restart SPT to apply" como as outras edições.
- **A validar:** o `LoyalLevelItems` é chaveado por **itemId (rootId do assort)**, não por tpl — mapear (trader, tpl) → rootId(s) na edição (um tpl pode ter mais de uma entry). Confirmar se o cliente/EFT aceita o LL mudado sem reindexar o assort. Faixa válida de LL por trader (o trader tem N loyalty levels no `base.json`).
- **Esforço:** médio — backend (applier + config + API) + UI, análogo ao B-6.
- **Depende de:** nada — reusa a infra de override/`StockApplier` e o editor accordion.

### B-12 · Árvore lateral: clique no chevron só expande, não seleciona
- **O quê:** na árvore de categorias da sidebar esquerda, clicar no chevron (▸/▾) hoje **seleciona a categoria E expande**. Deveria: o chevron **só** expande/colapsa; a **seleção** da categoria só ocorre ao clicar na área da linha **à direita** do chevron (o rótulo/contagem).
- **Como (provável):** separar o hit-target do chevron do resto da linha (`.tree-node`) — o handler do chevron faz `stopPropagation` + só toggle de expand; o handler do rótulo faz a seleção/filtro por categoria. UI-only.
- **Esforço:** pequeno — UI-only (`index.html`).
- **Depende de:** nada.

---

## Decisões travadas (2026-07-04)

1. **B-4 alvo = flea** (não trader).
2. **Ordem: B-2 primeiro** (as features de UI B-1/B-4 e a UI do B-3 nascem dentro do mod novo).
3. **"preço de venda de itens de mod"** (chat) = **trader**, já pronto e testado. B-3 é a evolução para o **buy**.

## Arquitetura assumida do B-2 (para o SDD detalhar)

Novo **server mod C#** usando o pacote first-party **`SPTarkov.Server.Web`** (mesmo padrão do `CustomClasses`, que já serve editor web como mod). Estratégia **faseada** para de-riscar:

- **Milestone 1 (core "virar mod"):** o mod C# serve o **`index.html` + assets + `data/items.json` atuais como estáticos** (wwwroot) na HTTP do próprio SPT, e **porta os endpoints do `serve.js`** para rotas C# (`/api/price`, `/api/trader-price(+/all)`, `/api/ban`, `/api/flea-level`, GETs de overrides) — mesmas escritas atômicas em `configs/ragfair.json`/`items.json`/`globals.json` + refresh de `checks.dat`. O **catálogo (`items.json`) continua gerado pelo build Node** (`load-spt`+`normalize`) nesta fase. **Ganhos já entregues:** 1 instalação, **auto-start** com o SPT (resolve P-1.2), sem processo Node separado no runtime, sem `serve.js`/`update-vm.ps1`.
- **Milestone 2 (opcional, depois):** portar `load-spt`/`normalize` para C# in-process (gera o catálogo do DB vivo) → elimina o build Node de vez. **Fora do escopo desta rodada** (maior + arriscado).
- A UI (`index.html` vanilla-JS) **é reaproveitada como está** — sem reescrever em Blazor.

## Plano de execução (SDD + autônomo)

Ordem e paralelismo (pesquisa/spec/review via subagents independentes onde não houver dependência de arquivo):

| Fase | Item | Entrega desta rodada | Paraleliza com |
|---|---|---|---|
| 1 | **B-2** | spec funcional + técnica; **spike** (mod mínimo servindo `index.html` + 1 rota via `SPTarkov.Server.Web`, boot-testado); se o spike passar, **Milestone 1** | — (gate) |
| 2 | **B-3** | spec + **implementar o patch de buyback** no `TRLTraderPrices` (independe do B-2) + teste por rota | pode rodar em paralelo ao spec do B-2 |
| 3 | **B-1** | spec + implementar lado config/viewer (derivar `fleaCeiling` do `unreasonableModPrices`; toggle) — **flag: validação in-game pendente** | com B-3 |
| 4 | **B-4** | **spec apenas** (UI depende do Milestone 1 do B-2) | — |

**Critérios de aceite (resumo):**
- B-2 M1: server sobe com o mod; UI abre na porta do SPT; editar flea/trader/ban/nível via a UI servida pelo mod → grava e reflete igual ao `serve.js`; sem processo Node no runtime.
- B-3: editar buy price de 1 item → in-game o trader paga o valor setado (prova por rota `getUserAssort`/venda simulada ou log do patch); coeficiente global de outros itens intacto.
- B-1: com `unreasonableModPrices` desligado + rebuild, o viewer aceita preço acima do antigo teto e grava; **validação in-game marcada como pendente**.
- B-4: spec com fluxo de UI + endpoint batch + tratamento de item sem preço/acima do teto.

**Guardrails da execução autônoma:**
- **SDD-first:** spec antes de código em cada item; artefatos versionados.
- **Nada de push** (precisa aprovação) — só commits locais.
- **B-1 toca `configs/ragfair.json`:** backup antes; mudança reversível.
- **Não completar o Milestone 2 do B-2** nem o full-port do `load-spt` nesta rodada.
- **Validação in-game (cliente EFT)** não é possível enquanto o user dorme → marcar cada ponto que exige e deixar no relatório.
- Decisões assumidas registradas; relatório final com feito / spec'd / pendente-de-validação.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-03 | Guilherme | Criação — 4 itens (B-1 teto flea, B-2 virar mod, B-3 buy price, B-4 bulk copy) levantados para validação de escopo. |
| 2026-07-07 | Guilherme | B-5 adicionado — piso de flea configurável por item (override abaixo do buyback teórico do trader), levantado durante o planejamento do B-2/unificação. Duas rotas identificadas (editar handbook vs. Harmony dedicado), nenhuma spec'd ainda. |
| 2026-07-08 | Guilherme | B-6 adicionado — editar quantidade em estoque (`Item.Upd.StackObjectsCount`) do assort dos traders, com foco em traders de mod que habilitam estoque ~99999 (padrão nativo do SPT confirmado: `UnlimitedCount:true` + valor gigante, não um flag isolado), mais um modo de edição em massa via config. Nenhuma spec ainda. |
| 2026-07-11 | Guilherme | B-7 adicionado — portar o pipeline Node (`load-spt.js`/`normalize.js`/fetch scripts) pra C# in-process (era o "Milestone 2" do B-2, agora formalizado com o achado de que `.js` em qualquer lugar da pasta do mod trava o `ModValidator` inteiro, e o bug real da Trudy provando o risco de descoberta de trader divergente do Node). B-8 adicionado — refatorar 100% do viewer web pra usar o TRL Design System, com a mesma restrição de `.js` inline documentada. |
