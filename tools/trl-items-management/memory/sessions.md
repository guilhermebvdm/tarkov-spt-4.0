# Memória de sessões — `tools/trl-items-management`

Trilha cronológica das sessões de chat sobre o tool (viewer/editor) e sua feature de preço de
trader (mod companion `TRLTraderPrices`). Ver `memory-curation` para as regras de escrita.

## Estado atual (snapshot ao fim da última sessão)

- **Feature de preço de trader (VENDA) em produção como v1.1.0** (mod `TRLTraderPrices` + viewer). Moeda nativa (₽/$/€/GP); aplica na compra direta E no flea. Deploy VM = 1 comando ([`scripts/package-release.sh`](../scripts/package-release.sh) + [`scripts/update-vm.ps1`](../scripts/update-vm.ps1)). Mod C# reescreve `Assort.BarterScheme` em `OnLoadOrder.RagfairCallbacks-1`.
- **Backlog de evolução B-1..B-4 aberto** ([`BACKLOG.md`](../BACKLOG.md)), rodado via `/g-autodev` no branch `feat/trl-items-autodev`. Specs SDD em [`specs/`](../specs/):
  - **B-1** (teto do flea) ✅ **feito+testado** — toggle `/api/flea-cap` no topbar liga/desliga `unreasonableModPrices` (WeaponMod ×6 / Electronics ×11).
  - **B-2** (virar mod SPT) 🟢 **spike PROVADO** — mod `Sdk.Web`+`SPTarkov.Server.Web` serve UI+API na Kestrel do SPT (6969); falta Milestone 1 (portar `serve.js`). Source em `mods/TRLItemsManagement/`.
  - **B-3** (preço de COMPRA / buyback) 🟡 **destravado, a implementar** — Rota B (patch client + backstop server); métodos confirmados. Retomar por [`HANDOFF.md`](../HANDOFF.md).
  - **B-4** (bulk copy dev/market → flea) 🟢 **spec** (depende do B-2 M1).
- Dev box preservado: `ragfair.json` íntegro (teto on), override do usuário (Polytech belt @ Ragman) intacto.

## Pendências / próximos passos conhecidos

- [P-2.1] (aberta 2026-07-04) **B-3 a implementar** (Rota B): patch client `TraderClass.GetUserItemPrice` + prefix server `TradeHelper.SellItem` + `buy-overrides.json` + router + UI + validação in-game. Roteiro turnkey em [`HANDOFF.md`](../HANDOFF.md). 🟢 backlog spec'd.
- [P-2.2] (aberta 2026-07-04) **B-1 validação in-game** pendente: desligar o teto → confirmar GPU > 2.178M no jogo (checar 2º limite no cliente). 🟡 débito.
- [P-2.3] (aberta 2026-07-04) **B-2 Milestone 1**: portar os endpoints do `serve.js` → controllers C# + servir o `index.html` real (o spike já provou o mecanismo). 🟢 ideia.
- [P-2.4] (aberta 2026-07-04) Branch `feat/trl-items-autodev` está na **worktree principal compartilhada** (sessões paralelas trocam o branch + commitam junto, ex.: launcher `8bce72e`/`431bc25`); verificar/isolar antes de retomar. 🟡 débito.
- [P-1.1] (aberta 2026-07-03) `update-vm.ps1` não testado ponta-a-ponta na VM. 🟡 débito.
- [P-1.2] (aberta 2026-07-03) Viewer roda manual na VM → sem auto-start; registrar Scheduled Task `TRLItemsManagement`. 🟡 débito.
- [P-1.3] (aberta 2026-07-03) Commits locais à frente do origin, aguardando aprovação de push. 🟢 ideia.

---

## 2026-07-03 20:08 (GMT-3) — Sessão 1: trader-prices — testes de borda, fix quest-locked, release v1.1.0 e update de 1 comando

**Tema central:** fechar a feature de preço de trader com testes de cenários não previstos + melhorias essenciais, empacotar/versionar pra VM, e automatizar updates futuros. (Sessão continuada — a construção inicial da feature foi em portção compactada anterior.)

**Decisões-chave:**
- **Edição em moeda nativa (não conversão RUB):** o mod seta `bs.Count` direto; hint calculado em ₽ só p/ USD/EUR (taxa real Peacekeeper/Skier ~136/158, derivada do assort de coin-sell), nunca p/ GP (não se compra GP com ₽). Ref: [`scripts/load-spt.js`](../scripts/load-spt.js) (`deriveSellRate`), `TRLTraderPricesMod.cs`.
- **Detecção de quest-locked de mod:** `load-spt` passou a ler o quest-assort no path não-padrão `db/CustomQuests/<traderId>/QuestAssort/*.json` (padrão WTT-Artem), além do `questassort.json` padrão → marca `questLocked:true`; viewer mostra 🔒 "aplica após unlock" (mantém editável, pois aplica pós-quest). Por quê: viewer mostrava oferta editável **fantasma** (MedBox@Artem) que no-opava in-game. Ref: [`scripts/load-spt.js`](../scripts/load-spt.js) §5, `viewer/index.html:1372,2229`.
- **Release v1.1.0:** bump em DOIS lugares (csproj `<Version>` + `TRLTraderPricesMetadata.cs` `SemanticVersioning.Version`, exige rebuild da DLL) + rótulo `app-version` no `viewer/index.html` + sufixo `-v1.1.0` nos zips. Ref: commits d5da76b, 2495a80.
- **Update VM de 1 comando:** bundle único `trl-release-vX.Y.Z.zip` ([`scripts/package-release.sh`](../scripts/package-release.sh), versão lida do csproj) + [`scripts/update-vm.ps1`](../scripts/update-vm.ps1) (idempotente; offline por default via `load-spt`+`normalize`; preserva `.env` e `overrides.json`). Escolhido sobre git-pull/sync porque a entrega é AnyDesk manual. Ref: commit b6a01e9.

**Lições / hipóteses descartadas:**
- `getTraderAssort` é **filtrado por loyalty + quest do perfil** — itens LL-altos (AR-15 LL4) e quest-locked (MedBox) NÃO aparecem na rota, embora estejam no assort completo em memória que o mod muta. Gerou 4 "FAIL" iniciais no verify que eram (a) asserção estrita demais e (b) filtragem — não bugs. Fix: asserção **money-only** + casos "absent" verificados via contadores do log do mod. Ref: [`scripts/verify-trader-matrix.js`](../scripts/verify-trader-matrix.js).
- Um mesmo tpl pode ser vendido **money E barter pelo mesmo trader** (M4A1 no Mechanic/Peacekeeper): o mod seta as entradas money e pula as barter — correto, não bug. Explica `barterSkip`/`mixedSkip` altos no log.
- `SPT.Server.exe` **exige console real**: bootar com stdout redirecionado pra arquivo mata em `SetConsoleOutputMode` ("Unable to get console mode"). Fix: `Start-Process` (console novo) + ler `user/logs/spt/spt<data>.log` (baseline de linhas antes do boot p/ pegar só o novo).
- Versão do mod **precisa bump em 2 lugares + rebuild** — renomear o zip só deixa a DLL/log em 1.0.0. O header do viewer (`index.html app-version`) é um 3º lugar independente (foi o que o user viu "desatualizado").
- `mods/TRLTraderPrices/builds/` é **gitignored** → a DLL não está no git; deploy precisa de artefato deliberado (bundle), não referenciável via `git archive`.

**Atividade cronológica:**
1. Matriz de override (10 cenários) + boot + `verify-trader-matrix.js` → 8/8 após ajuste de asserção (money-only + casos absent); edge cases confirmados no log (`badTrader 1, tplNotSold 3, barterSkip 2, currencyMismatchSkip 1, fenceSkip 1, mixedSkip 4`).
2. Fix quest-locked no `load-spt` + hint no viewer; rebuild `items.json`; validado via Chrome MCP (MedBox renderiza `S Ⓐ 324.250 ₽ LL1 🔒`). Commit e777731.
3. Bump v1.1.0 (mod csproj+metadata) + rebuild build-only (SPT_PATH inexistente → sem parar o server). Commit d5da76b. Depois rótulo do viewer. Commit 2495a80.
4. `package-release.sh` + `update-vm.ps1` + `/dist/` no `.gitignore` + doc no `DEPLOY.md`. Commit b6a01e9. Bundle v1.1.0 gerado em `D:/SPT/_vm-deploy/trl-release-v1.1.0.zip` (dogfood do packager).
5. Restaurado override do usuário no dev box; server reiniciado (`applied 1`).

**Pendências abertas nesta sessão:**
- [P-1.1] (aberta 2026-07-03) `update-vm.ps1` sem teste ponta-a-ponta (roda na VM). 🟡
- [P-1.2] (aberta 2026-07-03) Viewer manual na VM → sem auto-start no boot; registrar Scheduled Task `TRLItemsManagement`. 🟡
- [P-1.3] (aberta 2026-07-03) Push dos commits (e777731, d5da76b, 2495a80, b6a01e9) aguardando aprovação. 🟢

**Cross-refs:**
- Limitação scenario-3 (item de mod quest-locked aplica override só pós-unlock) documentada em [`DEPLOY.md`](../DEPLOY.md) (§ mod).
- Preferência de versionamento salva na auto-memória do usuário (`feedback_version_increment_on_release`).
- Infra repo-wide desta sessão (`.gitignore /dist/`): registrada aqui por ser do packager do tool; não houve trabalho em `.claude/`/`.agents/`.

---

## 2026-07-04 15:13 (GMT-3) — Sessão 2: run autônomo /g-autodev do backlog B-1..B-4 (specs + B-1 feito + spike B-2 + B-3 destravado)

**Tema central:** executar o backlog de evolução (B-1 teto flea, B-2 virar mod, B-3 buy price, B-4 bulk) via `/g-autodev` — SDD + paralelismo + autonomia (usuário ausente parte do tempo).

**Decisões-chave:**
- **Escopo travado pelo usuário:** B-4 alvo = **flea**; ordem **B-2 primeiro** (as features de UI nascem no mod novo). Ref: [`BACKLOG.md`](../BACKLOG.md).
- **B-2 arquitetura:** mod `Microsoft.NET.Sdk.Web` + pacote first-party **`SPTarkov.Server.Web`** (marca `IModWebMetadata`) servindo o **`index.html` vanilla reaproveitado** (não Blazor) + **controllers ASP.NET** p/ a API. Ref: [`specs/B-2-tool-as-mod.md`](../specs/B-2-tool-as-mod.md).
- **B-1 implementação:** só **flipa `enabled`** das 2 categorias de `unreasonableModPrices` (mults intactos) → religar é exato, sem sidecar. `load-spt` já filtra por `enabled` → `fleaCeiling=null` quando off. Ref: `viewer/serve.js` (`/api/flea-cap`), `scripts/load-spt.js:295-298`.
- **B-3 Rota B escolhida** (patch client + backstop server) sobre a Rota A (server-only): só server desincroniza exibido≠recebido. Ref: [`specs/B-3-trader-buy-price.md`](../specs/B-3-trader-buy-price.md).
- **Isolamento:** branch dedicado `feat/trl-items-autodev` a partir do backlog-lock (`af5a162`).

**Lições / hipóteses descartadas:**
- **Server NÃO calcula o buyback no sell** — confia no `sellRequest.Price` agregado do cliente (`references/spt-source/.../Helpers/TradeHelper.cs:251,295` → `PaymentService.GiveProfileMoney`). Por isso B-3 **não pode ser só server** (mudaria o recebido, não o exibido) → precisa patch client. Descartou a premissa inicial "prefix no SellItem resolve".
- **`ModValidator` rejeita o mod INTEIRO** se houver qualquer `.js`/`.ts` na pasta (`references/spt-source/.../ModValidator.cs:316-335`) → o build Node não pode viver dentro do mod B-2; scripts de browser vão como `.mjs`.
- **Sdk.Web `dotnet build` NÃO copia o `wwwroot` físico** (usa `staticwebassets` manifest); o host do SPT procura `wwwroot` físico ao lado da DLL → o deploy tem que copiar o `wwwroot` fonte (ou `publish`). Descoberto no spike.
- **Método client do buyback = `TraderClass.GetUserItemPrice(Item)` → `GStruct300?`** (confirmado por `ilspycmd` em `D:/SPT/EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll:221`; **não está** no decompile em texto — `TraderClass` só existe na DLL). Fallback `Profile.TraderInfo.ApplyPriceModifier` não serve (sem identidade de item).
- **Worktree principal é compartilhada:** sessões paralelas trocam o branch da worktree e commitam nele (launcher commitou `8bce72e`/`431bc25` junto). Verificar `git branch --show-current` antes de cada commit.

**Atividade cronológica:**
1. Fase 0: branch dedicado + WORKFLOW/ambiente. 2 subagents de pesquisa (web-serving B-2 + buyback B-3, paralelos).
2. Specs SDD B-1/B-2/B-3/B-4 escritas + commitadas (`abc03cd`, `203b844`).
3. **B-2 spike** (`bf239e7`): mod Sdk.Web serve `wwwroot/index.html` + controller `/api/ping` → verificado no boot (mod carregado, `api/ping` 200 `{ok:true}`, `index.html` 200).
4. **B-1** (`dacd3b1`+`34e395f`): `GET/POST /api/flea-cap` + toggle no topbar → testado por rota (checks.dat atualiza, mults preservados) + Chrome MCP (on→OFF→on, 0 erro).
5. **B-3**: 2ª pesquisa (método client via ilspycmd) → `GetUserItemPrice` confirmado; spec Rota B com os 2 pontos de patch (`03213dc`,`15b20a9`).
6. Progresso no `BACKLOG.md` + **`HANDOFF.md`** turnkey (`1e03f02`,`e5ba194`). Cleanup: spike removido do install (source no repo), ragfair restaurado, override do usuário intacto.

**Pendências abertas nesta sessão:**
- [P-2.1] (aberta 2026-07-04) B-3 (Rota B) a implementar — roteiro em [`HANDOFF.md`](../HANDOFF.md). 🟢
- [P-2.2] (aberta 2026-07-04) B-1 validação in-game (teto off → GPU > 2.178M). 🟡
- [P-2.3] (aberta 2026-07-04) B-2 Milestone 1 (portar `serve.js` → controllers C#). 🟢
- [P-2.4] (aberta 2026-07-04) Branch em worktree compartilhada — verificar/isolar. 🟡

**Cross-refs:**
- Retomada do B-3 (turnkey, com métodos/refs confirmados): [`HANDOFF.md`](../HANDOFF.md). Backlog + progresso: [`BACKLOG.md`](../BACKLOG.md).
- A lição "getTraderAssort filtra por loyalty/quest" é da Sessão 1 (não repetida aqui).
- Trabalho de launcher paralelo no mesmo branch (arquivos `launcher/`) — não pertence a este mod.
