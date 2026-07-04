# HANDOFF — TRL Items Management (backlog B-1..B-4)

> **Data:** 2026-07-04 · **Branch:** `feat/trl-items-autodev` · **Origem:** run autônomo `/g-autodev` sobre [BACKLOG.md](BACKLOG.md), continuado em sessão interativa (implementação do B-3)

## ⭐ Próxima ação (a única coisa que importa começar)

**Validar o B-3 in-game.** Implementação (client + server + viewer) está **100% pronta, buildada, deployada e boot-testada** — falta só o usuário jogar. Passos:
1. Setar 1 override de buy-price em qualquer item pelo viewer (`http://127.0.0.1:8080/viewer/` → busca o item → expande o detalhe → seção "Trader buy (tarkov.dev ref)" → clica no valor de um trader editável → salva).
2. Bootar o SPT server (se não estiver rodando: `Start-Process 'D:\SPT\SPT\SPT.Server.exe'`) + o launcher/EFT.
3. **Vender o item pro trader que recebeu o override**: confirmar que o **preço exibido** na tela de venda = override **E** o **dinheiro recebido** após confirmar = override.
4. Se bater → B-3 fechado. Reportar no BACKLOG.md e considerar `/g-review-content` na implementação.

Depois disso, os próximos itens do backlog (sem decisão pendente) são B-2 Milestone 1 (portar `serve.js` → controllers C#) e B-4 (depende do B-2 M1) — ver [BACKLOG.md](BACKLOG.md).

## Skills sugeridas p/ a próxima sessão

- `/update-memory tools\trl-items-management` (registrar a sessão desta implementação).
- `/g-review-content` na implementação do B-3 (client+server+viewer).
- `repo-workflow-best-practices` (o ciclo SDD; ver [WORKFLOW.md](../../WORKFLOW.md)) quando for atacar o B-2 M1.

---

## Estado do backlog

| Item | Estado | Refs |
|---|---|---|
| **B-1** teto do flea | ✅ **Feito + testado** (endpoint `GET/POST /api/flea-cap` + toggle no topbar; rota + Chrome MCP). **Pendente: validação in-game** (desligar → GPU > 2.178M). | `dacd3b1`, `34e395f` · [specs/B-1-flea-cap.md](specs/B-1-flea-cap.md) |
| **B-2** virar mod | 🟢 **Spec + SPIKE PROVADO** (mod `Sdk.Web`+`SPTarkov.Server.Web` serve `wwwroot/index.html` + controller `/api/ping` na Kestrel do SPT 6969 — verificado no boot 200/200). **Falta: Milestone 1** (portar endpoints do serve.js → controllers C# + servir o index.html real). | `bf239e7` · [specs/B-2-tool-as-mod.md](specs/B-2-tool-as-mod.md) · source em `mods/TRLItemsManagement/` |
| **B-3** buy price | ✅ **Implementado (Rota B)** — client (`TraderClass.GetUserItemPrice` Postfix) + server (`TradeHelper.SellItem` Prefix Harmony) + rota `/trltraderprices/buy-overrides` + viewer ("B" editável). Build+deploy limpos, boot do SPT sem erros, API testada via Chrome DevTools. **Pendente: validação in-game** (ver acima). | ver §Commits abaixo · [specs/B-3-trader-buy-price.md](specs/B-3-trader-buy-price.md) |
| **B-4** bulk copy → flea | 🟢 **Spec pronta, adiada** (depende do B-2 M1). | `abc03cd` · [specs/B-4-bulk-copy-flea.md](specs/B-4-bulk-copy-flea.md) |

---

## B-3 — o que foi implementado (Rota B: client + server, exibido = recebido)

**Por quê Rota B:** o buyback é calculado **no cliente** (`TraderClass.GetUserItemPrice`); o servidor confia no preço que o cliente manda (`TradeHelper.SellItem` → `sellRequest.Price`). Um patch só-server mudaria o dinheiro recebido mas não o exibido (desync). Rota B = patch client (exibido) + backstop server (recebido), ambos lendo o **mesmo override** → coerente.

### 1. Config (fonte única)
`user/mods/TRLTraderPrices/config/buy-overrides.json`, shape `{ "<traderId>": { "<tpl>": { "count": <n>, "currency": "RUB|USD|EUR|GP" } } }` (igual ao sell `overrides.json`, arquivo separado). Escrito pelo viewer, lido por client e server.

### 2. Server (`mods/TRLTraderPrices/modded/Server/`)
- `TRLTraderPricesMod.cs`: `TraderOverride` virou `internal` (reusado pelos dois lados); novos statics `BuyOverrides`/`Db`/`Log` (padrão OutfitPersistenceFixMod — Harmony patch é estático, sem DI); bootstrap `new Harmony("trltraderbuyprice.trl").PatchAll(...)`; loader + parser de `buy-overrides.json` (`ParseBuyOverrides`, pré-parseia pra `MongoId`, dropa Fence).
- `TraderBuyPricePatch.cs` (novo): Harmony **Prefix** em `TradeHelper.SellItem` — se TODOS os itens vendidos têm override (mesma moeda do trader) → reescreve `sellRequest.Price` antes do método original rodar; senão, vanilla intocado.
- `TraderBuyPriceRouter.cs` (novo): `StaticRouter` servindo `GET /trltraderprices/buy-overrides` (conteúdo cru do `buy-overrides.json`, `{}` se ausente).
- csproj: referência `0Harmony.dll` adicionada (copiada de `mods/OutfitPersistenceFix/modded/Server/References/`).

### 3. Client (`mods/TRLTraderPrices/modded/Client/`, projeto BepInEx **novo**)
- **Alvo confirmado (ilspycmd na DLL viva):** `public TraderClass.GStruct300? GetUserItemPrice(Item item)` — struct `readonly struct GStruct300(MongoID? currencyId, int amount)`, campos `CurrencyId`/`Amount`. `TraderClass.Id` é `string`; `Item.TemplateId` é `MongoID` (`Template._id`).
- `Patches/TraderBuyPricePatch.cs`: `ModulePatch` (`SPT.Reflection.Patching`) Postfix, mesmo padrão de `Skills-Extended/.../GetBarterPricePatch.cs`.
- `BuyPriceOverrides.cs`: cache lazy (padrão `SkillMultipliers.cs`), busca `/trltraderprices/buy-overrides` via `RequestHandler.GetJson`; moeda→tpl hardcoded (mesmos 4 ids do `Money` enum server-side).
- `Plugin.cs`: `[BepInPlugin]` mínimo, só habilita o patch.
- csproj: `netstandard2.1`, refs mínimas (Assembly-CSharp, BepInEx, 0Harmony, SPT.Reflection, SPT.Common, Newtonsoft.Json, UnityEngine + CoreModule).

### 4. Viewer — "B" agora editável
- `serve.js`: `GET/PATCH/DELETE /api/trader-buy-price[/all]` + `GET /api/trader-buy-overrides`, espelhando 1:1 os handlers de sell (`handlePatchTraderPrice` etc.), gravando `config/buy-overrides.json` (arquivo separado do sell).
- `index.html`: seção "Trader buy (tarkov.dev ref)" no detalhe do item virou uma tabela editável (`renderBuyRow`/`openBuyEdit`/`restoreBuyOverride`, clique delegado em `onTableClick`), mesmo UX do "S" (edição inline, badge OVR, ↺ restaurar, preview de conversão). Fence e vendors não resolvíveis continuam read-only.
- `components.css`: bloco novo `.buy-*` espelhando `.trader-*` (reaproveita `.trader-edit-form`/`.trader-price-val`/`.badge--override` como estão).

### 5. Validado nesta sessão
- Build limpo (client + server, 0 erros) via `.agents/scripts/compile-mod.sh TRLTraderPrices`.
- Deploy real: client → `D:/SPT/BepInEx/plugins/TRLTraderPrices/`, server → `D:/SPT/SPT/user/mods/TRLTraderPrices/`.
- Boot do SPT server: mod carregado, `Harmony patch applied — buy price backstop`, sem exceptions, servidor sobe normal.
- Rota `/trltraderprices/buy-overrides` responde 200 (corpo comprimido pelo próprio SPT — decodificado manualmente pra confirmar `{}` válido).
- Viewer: round-trip completo via Chrome DevTools MCP (abrir item Colt M4A1 → editar Peacekeeper USD 73→150 → badge OVR + strike-through + hint ₽ aparecem → restaurar → estado limpo). Sem erros novos no console (só um 404 de favicon.ico pré-existente, não relacionado).

### 6. Validação in-game (fecha o B-3 — precisa do usuário)
Ver "⭐ Próxima ação" no topo.

---

## Ambiente / gotchas (ler antes de começar)

- **Git — worktree compartilhada:** o branch `feat/trl-items-autodev` está na worktree **principal**, que **sessões paralelas trocam de branch**. **No início da nova sessão: `git branch --show-current`** e, se preciso, `git switch feat/trl-items-autodev`. Commitar frequente.
- **Permissões:** `.claude/settings.local.json` já está em `bypassPermissions` — sem prompts. Push/gh continuam exigindo aprovação humana (regra do usuário).
- **Server/viewer dev box (`D:/SPT/SPT`):** Bootar: `Start-Process 'D:\SPT\SPT\SPT.Server.exe'` (console real — `SetConsoleOutputMode` mata se stdout for redirecionado). Viewer: `cd tools/trl-items-management/viewer && node serve.js 8080` (default `SPT_PATH=D:/SPT/SPT` já correto — **não** precisa setar env var). Log do SPT: `D:/SPT/SPT/user/logs/spt/spt<AAAAMMDD>.log`.
- **Client EFT:** `D:/SPT/EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll` (autoritativo p/ a API do client). `ilspycmd` em `/c/Users/guime/.dotnet/tools/ilspycmd` — `ilspycmd "<dll>" -t TraderClass` p/ reconfirmar assinaturas.
- **Build+deploy server+client juntos:** `bash .agents/scripts/compile-mod.sh TRLTraderPrices --spt-path D:/SPT` (compila os 2 projetos, resolve refs do client a partir do install, copia DLLs; **exige o SPT server parado** — DLL travada no Windows). Build só-`builds/` (sem tocar install): `--spt-path /nonexistent-build-only`.
- **Deploy do client:** `BepInEx/plugins/TRLTraderPrices/` na raiz do **cliente** (`D:/SPT`, não `D:/SPT/SPT`) — o script já faz isso sozinho.
- **Servidor é Fika Coop PVE** — sinalizar qualquer gap de coop-sync (ver memória `feedback_coop_multiplayer_sync`). O buyback é UI local (cada client calcula o próprio preço), mas o servidor (host) precisa ter o backstop pra creditar certo — validar host + client se possível.
- **Estado preservado:** `ragfair.json` íntegro (teto on), override do usuário (Polytech belt @ Ragman 1M) intacto em `overrides.json` (sell). `buy-overrides.json` (novo) está vazio `{}` — nenhum override de buy foi deixado ativo de propósito (usado só como teste transitório nesta sessão, revertido).

## Referências
- Backlog + progresso: [BACKLOG.md](BACKLOG.md) · Specs: [specs/](specs/) (B-1..B-4) · Ciclo SDD: [../../WORKFLOW.md](../../WORKFLOW.md).
- Precedentes de código: `mods/CustomClasses/modded/{Client,Server}/` (web mod + client patch + router), `mods/OutfitPersistenceFix/modded/Server/` (Harmony server), `mods/Skills-Extended/.../GetBarterPricePatch.cs` (Postfix de preço GStruct300).
- Implementação do B-3: `mods/TRLTraderPrices/modded/{Client,Server}/`.
- SPT source vendorizado: `references/spt-source/` · EFT decompilado: `references/eft-decompiled/Assembly-CSharp/EFT/MongoID.cs` (única classe já extraída; `TraderClass`/`Item` reconfirmados via ilspycmd na DLL viva, não neste snapshot).
