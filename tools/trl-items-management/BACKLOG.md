# TRL Items Management — Backlog / roadmap

> **Status:** 🟢 Vivo<br>
> **Última revisão:** 2026-07-03<br>
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
- **Pendente:** validação in-game (vender 1 item com override setado → confirmar que o preço exibido na tela E o dinheiro recebido batem com o override).

### B-4 · Bulk: copiar preço tarkov.dev / tarkov-market → override de FLEA
- **O quê:** multi-selecionar itens + ação "copiar preço [tarkov.dev | tarkov-market]" → aplica como override de **flea** (decidido: alvo = flea) **em massa**.
- **Viabilidade:** 🟢 média — os dados já estão no viewer (colunas dev/market). Falta: multi-seleção na UI + endpoint de **batch-PATCH** de flea.
- **A validar:** item sem preço dev/market → pular + reportar quantos. Item acima do teto (mods/electronics) → aplicar `min(preço, ceiling)` e avisar. Restart-para-aplicar continua valendo.
- **Depende de B-2** (é feature de UI → construir dentro do mod novo).

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
