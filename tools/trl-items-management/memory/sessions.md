# Memória de sessões — `tools/trl-items-management`

Trilha cronológica das sessões de chat sobre o tool (viewer/editor) e sua feature de preço de
trader (mod companion `TRLTraderPrices`). Ver `memory-curation` para as regras de escrita.

## Estado atual (snapshot ao fim da última sessão)

- **Feature de preço de trader CONCLUÍDA e em produção como v1.1.0** (mod `TRLTraderPrices` + tool viewer). Edita o preço de venda do trader em **moeda nativa** (₽/$/€/GP, sem conversão RUB); aplica in-game na compra direta E no flea.
- **Arquitetura:** viewer grava `user/mods/TRLTraderPrices/config/overrides.json` (`{traderId:{tpl:{count,currency}}}`); o mod C# (`OnLoadOrder.RagfairCallbacks-1`) reescreve `Assort.BarterScheme[id][0][0].Count` antes de o flea gerar as ofertas. Ref: [`mods/TRLTraderPrices/modded/Server/TRLTraderPricesMod.cs`](../../../mods/TRLTraderPrices/modded/Server/TRLTraderPricesMod.cs).
- **Validado por matriz de rota 8/8** (RUB/USD/EUR/GP nativo, multi-loyalty, 4 cenários, edge cases mismatch/Fence/barter/badTrader/stale). Harness: [`scripts/verify-trader-matrix.js`](../scripts/verify-trader-matrix.js).
- **Deploy VM = 1 comando:** bundle único `trl-release-vX.Y.Z.zip` (via [`scripts/package-release.sh`](../scripts/package-release.sh)) + [`scripts/update-vm.ps1`](../scripts/update-vm.ps1) (idempotente/offline). Doc: [`DEPLOY.md`](../DEPLOY.md) §7 e "Atualizar pra uma nova versão".
- Dev box: server rodando com override de teste (Polytech belt @ Ragman, `applied 1`). VM: v1.1.0 instalada e verificada (coluna trader com B/S no viewer).

## Pendências / próximos passos conhecidos

- [P-1.1] (aberta 2026-07-03) `update-vm.ps1` não testado ponta-a-ponta (roda na VM; testá-lo no dev box derrubaria o server). 1ª execução na VM é o teste real. 🟡 débito.
- [P-1.2] (aberta 2026-07-03) Viewer roda manual na VM (não é serviço/tarefa) → não sobe no reboot; registrar como Scheduled Task `TRLItemsManagement` p/ auto-start + controle limpo pelo `update-vm.ps1`. 🟡 débito.
- [P-1.3] (aberta 2026-07-03) Commits locais (e777731, d5da76b, 2495a80, b6a01e9) à frente do origin, aguardando aprovação de push. 🟢 ideia.

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
