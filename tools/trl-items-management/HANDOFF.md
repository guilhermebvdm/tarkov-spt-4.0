# HANDOFF — TRL Items Management (backlog B-1..B-4)

> **Data:** 2026-07-04 · **Branch:** `feat/trl-items-autodev` · **Origem:** run autônomo `/g-autodev` sobre [BACKLOG.md](BACKLOG.md)

## ⭐ Próxima ação (a única coisa que importa começar)

**Implementar o B-3 (preço de COMPRA do trader / buyback) — Rota B, já decidida.** Está **100% destravado e especificado**; falta o build de 2 mods + o teste in-game. Sem decisão pendente. Ver [specs/B-3-trader-buy-price.md](specs/B-3-trader-buy-price.md) — os dois pontos de patch já foram confirmados na fonte (abaixo).

## Skills sugeridas p/ a próxima sessão

- `spt-mod-best-practices` + `csharp-mod-best-practices` (patch Harmony server + client BepInEx).
- `repo-workflow-best-practices` (o ciclo SDD; ver [WORKFLOW.md](../../WORKFLOW.md)).
- `/compile-mod` (build/deploy do mod server).
- Ao terminar: `/update-memory tools\trl-items-management` (registrar a sessão) e `/g-review-content` na implementação.

---

## Estado do backlog (o que já foi feito neste run)

| Item | Estado | Refs |
|---|---|---|
| **B-1** teto do flea | ✅ **Feito + testado** (endpoint `GET/POST /api/flea-cap` + toggle no topbar; rota + Chrome MCP). **Pendente: validação in-game** (desligar → GPU > 2.178M). | `dacd3b1`, `34e395f` · [specs/B-1-flea-cap.md](specs/B-1-flea-cap.md) |
| **B-2** virar mod | 🟢 **Spec + SPIKE PROVADO** (mod `Sdk.Web`+`SPTarkov.Server.Web` serve `wwwroot/index.html` + controller `/api/ping` na Kestrel do SPT 6969 — verificado no boot 200/200). **Falta: Milestone 1** (portar endpoints do serve.js → controllers C# + servir o index.html real). | `bf239e7` · [specs/B-2-tool-as-mod.md](specs/B-2-tool-as-mod.md) · source em `mods/TRLItemsManagement/` |
| **B-3** buy price | 🟡 **Spec completa + destravado** (Rota B). **A implementar (este handoff).** | `203b844`,`03213dc`,`15b20a9` · [specs/B-3-trader-buy-price.md](specs/B-3-trader-buy-price.md) |
| **B-4** bulk copy → flea | 🟢 **Spec pronta, adiada** (depende do B-2 M1). | `abc03cd` · [specs/B-4-bulk-copy-flea.md](specs/B-4-bulk-copy-flea.md) |

Commits do run (branch `feat/trl-items-autodev`): `abc03cd 203b844 bf239e7 dacd3b1 34e395f 1e03f02 03213dc 15b20a9`. Arquivos: `tools/trl-items-management/` e `mods/TRLItemsManagement/`.

---

## B-3 — plano de build (Rota B: client + server, exibido = recebido)

**Por quê Rota B:** o buyback é calculado **no cliente**; o servidor confia no preço que o cliente manda. Patch só-server muda o dinheiro recebido mas não o exibido (desync). Rota B = patch client (exibido) + backstop server (recebido), ambos lendo o **mesmo override** → coerente.

### 1. Config (fonte única)
`user/mods/TRLTraderPrices/config/buy-overrides.json`, shape `{ "<traderId>": { "<tpl>": { "count": <n>, "currency": "RUB|USD|EUR|GP" } } }` (igual ao sell `overrides.json`). Loader: espelhar `mods/TRLTraderPrices/modded/Server/TRLTraderPricesMod.cs:88-91` (`fileUtil.GetModPath` + `jsonUtil.DeserializeFromFile`). **ATENÇÃO:** JsonUtil bind case-sensitive → o record precisa de `[JsonPropertyName("count")]`/`("currency")` (mesmo bug que já mordeu o sell; ver o record `TraderOverride` no mesmo arquivo).

### 2. Server (no mod TRLTraderPrices existente, em `modded/Server/`)
- **Harmony:** adicionar `References/0Harmony.dll` (copiar de `mods/OutfitPersistenceFix/modded/Server/References/0Harmony.dll`) + no csproj `<Reference Include="0Harmony"><HintPath>References\0Harmony.dll</HintPath><Private>false</Private></Reference>`. Wiring: `IOnLoad` com `new Harmony("trltraderbuyprices").PatchAll(assembly)` (padrão `mods/OutfitPersistenceFix/modded/Server/OutfitPersistenceFixMod.cs`).
- **Prefix backstop** em `TradeHelper.SellItem` (garante o dinheiro creditado = override):
  - Assinatura (ref `references/spt-source/.../Helpers/TradeHelper.cs:251`): `public void SellItem(PmcData profileWithItemsToSell, PmcData profileToReceiveMoney, ProcessSellTradeRequestData sellRequest, MongoId sessionID, ItemEventRouterResponse output)`.
  - O servidor credita `sellRequest.Price` (agregado da requisição) em `TradeHelper.cs:295` via `PaymentService.GiveProfileMoney`. `sellRequest` = `ProcessSellTradeRequestData` (`Price` double?, `Items` = lista `{id,count}` sem preço, `TransactionId` = traderId). Ver `references/spt-source/.../Models/Eft/Trade/ProcessSellTradeRequestData.cs`.
  - **Lógica do Prefix:** no entry, os itens ainda existem em `profileWithItemsToSell.Inventory.Items` → mapear `sellRequest.Items[i].Id`→item→`.Template` (tpl) + `count`. `traderId = sellRequest.TransactionId`. Fence (`579dc571d53a0658a154fbec`) ou sem override → `return true` (vanilla, não toca Price). Se **todos** têm override → `sellRequest.Price = Σ(override.count × item.count)` na moeda do trader (`trader.Currency`, ver `PaymentService.cs:193`); se **algum** não tem → `return true` sem tocar (fallback seguro; o cliente só manda 1 preço agregado). Precisa de Harmony (métodos não-virtual; DI typeOverride não intercepta).
- **StaticRouter** `/trltraderprices/buy-overrides` (serve o `buy-overrides.json` cru pro client): `[Injectable] class ... : StaticRouter` com `RouteAction<EmptyRequestData>(...)`. Padrão: `mods/CustomClasses/modded/Server/SkillMultipliersRouter.cs:16-53`.

### 3. Client (mod BepInEx novo — `mods/TRLTraderPrices/modded/Client/`, padrão CustomClasses)
- **Alvo confirmado (ilspycmd na DLL):** `public GStruct300? TraderClass.GetUserItemPrice(Item item)` (linha 221 de `D:/SPT/EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll`; chama `Info.ApplyPriceModifier`, retorna `new GStruct300(currencyId, amount)`). Struct: `GStruct300(MongoID? currencyId, int amount)`.
- **Patch:** `ModulePatch` (`SPT.Reflection.Patching`) **Postfix**, `ref GStruct300? __result`, args `TraderClass __instance, Item item`. Se override p/ (`__instance.Id`, `item.TemplateId`) e não-Fence → `__result = new GStruct300(currencyTplDaMoeda, count)`. Precedente exato (mesma GStruct300): `mods/Skills-Extended/modded/Plugin/Skills/SilentOps/Patches/GetBarterPricePatch.cs:19-63` (Postfix no sibling `TraderAssortmentControllerClass.GetBarterPrice`).
- **Scaffold:** copiar `mods/CustomClasses/modded/Client/` — `Plugin.cs` (`[BepInPlugin]`+`[BepInDependency("com.SPT.core","4.0.0")] : BaseUnityPlugin`, `Awake()` → `new TraderBuyPricePatch().Enable()`), `CustomClasses.Client.csproj` (netstandard2.1, `<Reference HintPath="References/…">` p/ Assembly-CSharp/BepInEx/0Harmony/SPT.Reflection/SPT.Common/Newtonsoft/UnityEngine*). **DLLs em `mods/CustomClasses/modded/Client/References/`** (copiar a pasta; é gitignored).
- **Config no client:** `RequestHandler.GetJson("/trltraderprices/buy-overrides")` (de `SPT.Common.Http`) → `JsonConvert.DeserializeObject<...>`; cache lazy (padrão `mods/CustomClasses/modded/Client/SkillMultipliers.cs:63-114`). **NÃO** ler arquivo local — usa a rota do §2.

### 4. UI (viewer) — tornar o "B" editável
Hoje o "B" (coluna trader) é referência do tarkov.dev (não-editável). Tornar editável → `PATCH /api/trader-buy-price {tpl,traderId,count,currency}` grava `buy-overrides.json`. **Espelhar** o handler de sell `handlePatchTraderPrice` em `viewer/serve.js` (~linha 855) + a UI de edição de trader no `index.html`. (Nasce no viewer atual; migra pro mod no B-2 M1.)

### 5. Validação in-game (fecha o B-3 — precisa do usuário)
Setar 1 override em `buy-overrides.json` (ex.: um item que um trader compra), bootar SPT + launcher, **vender o item pro trader**: confirmar que o **preço exibido** na tela = override **E** o **dinheiro recebido** = override.

---

## Ambiente / gotchas (ler antes de começar)

- **Git — worktree compartilhada:** o branch `feat/trl-items-autodev` está na worktree **principal**, que **sessões paralelas trocam de branch** e onde uma sessão de **launcher** commitou junto (`8bce72e`,`431bc25`, arquivos `launcher/`, sem conflito). **No início da nova sessão: `git branch --show-current` e, se preciso, `git switch feat/trl-items-autodev` (ou criar um branch fresco a partir de `15b20a9`).** Commitar frequente.
- **Permissões:** `.claude/settings.local.json` já está em `bypassPermissions` — sem prompts. Push/gh continuam exigindo aprovação humana (regra do usuário).
- **Server/viewer dev box (`D:/SPT/SPT`):** SPT server pode estar parado (sessão paralela). Bootar: `Start-Process 'D:\SPT\SPT\SPT.Server.exe'` (console real — `SetConsoleOutputMode` mata se stdout for redirecionado). Viewer: `cd tools/trl-items-management/viewer && SPT_PATH=D:/SPT/SPT node serve.js 8080`. Log do SPT: `D:/SPT/SPT/user/logs/spt/spt<AAAAMMDD>.log`.
- **Client EFT:** `D:/SPT/EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll` (autoritativo p/ a API do client). `ilspycmd` em `/c/Users/guime/.dotnet/tools/ilspycmd` — `ilspycmd "<dll>" -t TraderClass` p/ reconfirmar assinaturas.
- **Build server:** `bash .agents/scripts/compile-mod.sh TRLTraderPrices --spt-path D:/SPT` (copia a DLL pro install; **exige o SPT server parado** — DLL travada no Windows). Build só-`builds/` (sem tocar install): `--spt-path /nonexistent-build-only`.
- **Build client:** `dotnet build -c Release` no csproj do Client (netstandard2.1). Deploy do plugin: `BepInEx/plugins/` do **cliente** (raiz `D:/SPT`, não `D:/SPT/SPT`). Confirmar o path exato de deploy dos mods client no repo (ex.: como o CustomClasses client é deployado).
- **Servidor é Fika Coop PVE** — sinalizar qualquer gap de coop-sync (ver memória `feedback_coop_multiplayer_sync`). O buyback é UI local, mas validar no host.
- **Estado preservado:** `ragfair.json` íntegro (teto on), override do usuário (Polytech belt @ Ragman 1M) intacto em `overrides.json`. O spike do B-2 foi removido do install (source fica em `mods/TRLItemsManagement/`).

## Referências
- Backlog + progresso: [BACKLOG.md](BACKLOG.md) · Specs: [specs/](specs/) (B-1..B-4) · Ciclo SDD: [../../WORKFLOW.md](../../WORKFLOW.md).
- Precedentes de código: `mods/CustomClasses/modded/{Client,Server}/` (web mod + client patch + router), `mods/OutfitPersistenceFix/modded/Server/` (Harmony server), `mods/Skills-Extended/.../GetBarterPricePatch.cs` (Postfix de preço GStruct300).
- SPT source vendorizado: `references/spt-source/` · EFT decompilado (texto, sem `TraderClass`): `references/eft-decompiled/`.
