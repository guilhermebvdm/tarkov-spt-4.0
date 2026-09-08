# SPT-ContinuousLoadAmmo — Memória de Sessões

## Snapshot Delta
- **Versão:** 1.1.15 (SPT 4.0 / EFT 0.16.9)
- **Estado:** Dois bugs reais do Quick Load "fora do inventário" corrigidos e validados in-raid: (1) carregadores instalados em armas reserva eram selecionáveis pela busca e sempre falhavam; (2) `PlayerInventoryController.SetNextProcessLocked` ficava travado (`true`) após qualquer fechamento de inventário sem carregamento ativo, quebrando o atalho pelo resto da raid. Logging de falha agora expõe `FailedResult.Error` real em vez do nome do tipo.
- **Pendências:** 🟢 Nenhuma pendência registrada.

---

## 2026-09-08 03:01 (GMT-3) — Sessão 5: Dois bugs reais no Quick Load fora do inventário (v1.1.13 → v1.1.15)

**Tema central:** Usuário reportou que o atalho de Quick Load (tecla remapeada de K para H) falhava com `Comfort.Common.FailedResult` no console, enquanto o drag-and-drop manual de munição funcionava normalmente.

**Decisões-chave:**
- **Diagnóstico via decompiled source, não suposição:** traçamos a chamada real de `PlayerInventoryController.LoadMagazine` no jogo (`references/eft-decompiled/Assembly-CSharp/EFT/Player.cs:1250`) em vez de tentar fixes especulativos.
- **Logging melhorado primeiro:** `result.ToString()` numa falha só retornava o nome do tipo, sem detalhe. Criado `DescribeFailure()` ([LoadAmmoController.cs](../modded/Controllers/LoadAmmoController.cs)) que faz cast para `FailedResult` e expõe `.Error` — foi isso que revelou a causa real do bug #2 depois que o fix do bug #1 não resolveu o sintoma completo.
- **Bug #1 — carregador instalado em arma:** `GetMagazineForAmmo()` não excluía carregadores já instalados dentro de uma arma (ex.: pistola reserva no coldre, arma secundária na mochila). `MagazineItemClass.Apply()` (usado com `ignoreRestrictions:false`) rejeita incondicionalmente qualquer carregador cujo `Parent.Container is Slot`, antes de checar `weapon.SupportsInternalReload` (`MagazineItemClass.cs:479`). O drag-and-drop nativo não bate nisso porque a UI do jogo roteia ammo-sobre-mag-instalada para `LoadWeaponWithAmmo`, uma API diferente (`SlotView.cs:822`, `GridView.cs:904`) — o mod só chama `LoadMagazine`. Fix: `IsLoadableOutsideWeapon()` exclui esses carregadores da busca.
- **Bug #2 — trava "Next process is locked" (causa dominante do sintoma reportado):** `InventoryScreen.Close()` chama `SetNextProcessLocked(status: true)`, e só `InventoryScreen.Show()` desfaz isso (`InventoryScreen.cs:529,668`). O mod só desbloqueava esse flag em dois pontos pontuais (construtor do `LoadAmmoController`; `InventoryScreenClosePatch`, só se já houvesse carregamento ativo) — qualquer abre/fecha de inventário sem carregamento ativo deixava o flag travado pelo resto da raid, quebrando o atalho (que roda com inventário fechado, por definição) enquanto o drag-and-drop (inventário aberto) continuava funcionando. Fix: `PlayerInventoryController.SetNextProcessLocked(false)` chamado explicitamente logo antes de cada `LoadMagazine` real, em `LoadMagazineFireAndForgetAsync` e `LoadMagazineAsync`.
- **Bump em duas etapas:** v1.1.13 → v1.1.14 (bug #1 + logging), depois v1.1.14 → v1.1.15 (bug #2, só achado depois que o logging melhorado revelou a mensagem real "Next process is locked."). Compilado e instalado via `/compile-mod` em `E:/Tarkov Red Line/BepInEx/plugins/SPT-ContinuousLoadAmmo/`. Validado in-raid pelo usuário.

**Lições / hipóteses descartadas:**
- O fix do bug #1 parecia suficiente para explicar o sintoma completo, mas resolveu só uma fatia do problema — a causa dominante (bug #2) só apareceu depois que o log passou a expor `FailedResult.Error` em vez do nome do tipo. Lição: ao investigar uma falha de `IResult`/`Comfort.Common`, melhorar o log para expor `.Error` ANTES de tentar confirmar hipóteses — evita basear o diagnóstico em suposição quando o dado real está a uma linha de distância.
- Confirmado por leitura direta do decompiled: nem toda ação que parece equivalente ao drag-and-drop usa o mesmo caminho de código no jogo — a UI nativa tem atalhos especiais (aqui, `LoadWeaponWithAmmo`) que mods chamando a API "genérica" (`LoadMagazine`) não recebem de graça.

**Atividade cronológica:**
1. Usuário reportou atalho H falhando com `Comfort.Common.FailedResult`; investigação via decompiled source identificou dois pontos de checagem em `Player.LoadMagazine` (restrição de Slot em `Apply`, depois `Bool_2` em `method_30`).
2. Aplicado fix #1 (exclusão de carregadores instalados) + logging melhorado; build+install via `/compile-mod` → v1.1.14.
3. Usuário testou; novo erro apareceu ("Next process is locked.") — graças ao logging melhorado, a causa real ficou visível imediatamente, sem nova rodada de investigação às cegas.
4. Aplicado fix #2 (força `SetNextProcessLocked(false)` antes de cada `LoadMagazine`); build+install via `/compile-mod` → v1.1.15.
5. Usuário confirmou funcionamento em raid.

**Pendências abertas nesta sessão:** nenhuma.

---

## 2026-09-06 — Sessão 4: Harmonização de FSM de Mãos com LoadAmmoAnim e Release v1.1.12

**Tema central:** Eliminação da corrida assíncrona entre o `ContinuousLoadAmmo` e o `LoadAmmoAnim` durante o início do abastecimento contínuo de munição.

**Decisões-chave:**
1. **Verificação de Transição Dinâmica (`IsLoadAmmoAnimActiveOrPending`):**
   - Em `LoadAmmoController.cs`, criada a checagem que consulta se o `HandsController` é `LoadAmmoBundleController` ou se o estado `LoadAmmoAnimState.AnyIsOurAnimation()` está ativo via reflexão segura.
   - Usado em `SetPlayerStateRoutine` (evitando chamar `SetEmptyHands()` enquanto o LoadAmmoAnim estiver no drop da arma), em `StopLoading` e em `StopLoadingOnHandsChange`.
2. **Bump SemVer & Build:** Versão elevada para `1.1.12` sincronizada em `ContinuousLoadAmmo.cs`, `ContinuousLoadAmmo.csproj` e `mod.json`. Compilação Release em `mods/SPT-ContinuousLoadAmmo/builds/` com 0 erros e 0 avisos.

---

**Tema central:** Refinamento de interoperabilidade com o mod `Climbable Ladders` e consolidação de compatibilidade cruzada no ecossistema de mods.

**Decisões-chave:**
1. **Guarda de Escada no `LoadAmmoController`:** Em `CanLoadOutsideInventory()` e `TryQuickLoadAmmo()`, adicionada verificação dinâmica de `_player.gameObject.GetComponent("PlayerLadderController") != null`.
2. **Prevenção de FSM Desincronizada:** Impede que o jogador inicie o carregamento contínuo ou Quick Load fora do inventário enquanto escalando escadas de mão, evitando colisões entre o desarmamento do `PlayerLadderController` e o ciclo de abastecimento.
3. **Bump SemVer:** Versão elevada para `1.1.11` sincronizada em `ContinuousLoadAmmo.cs`, `ContinuousLoadAmmo.csproj` e `mod.json`.
4. **Compilação Release:** Compilado via MSBuild com 0 erros e 0 avisos.

---

## 2026-09-05 — Sessão 2: Resolução de Concorrência na Recarga de Armas (Tecla R), Coexistência com LoadAmmoAnim, Defensivas e Release v1.1.10

**Tema central:** Diagnóstico aprofundado e resolução definitiva do bug onde o jogador ficava impossibilitado de recarregar armas (tecla R) ou sofria travamento de interface ao fechar o inventário, conflito de mãos com o mod `LoadAmmoAnim` e aplicação de melhorias defensivas identificadas no Code Review 03.

**Decisões-chave:**
1. **Desbloqueio de Tecla R e Troca de Armas no InputNode:** Implementada a interceptação em `TranslateCommand` de `LoadAmmoComponent.cs` para `ECommand.ReloadWeapon`, `QuickReloadWeapon`, seletores de arma e slots rápidos 4 a 0 (`SelectFastSlot4` até `SelectFastSlot0`), cancelando o carregamento contínuo e repassando o comando nativo ao jogo (`Ignore`).
2. **Gating Estrito e Proteção Defensiva em `InventoryScreenClosePatch`:** Restringida a anulação de `___inventoryController_0` estritamente aos momentos em que o mod está ativamente abastecendo (`controller.IsActive`), protegido por bloco `try-catch`. Em qualquer outro fechamento de inventário normal, o mod não interfere, preservando o `StopProcesses()` vanilla do EFT. Se houver ações de mãos pendentes (`HasAnyHandsActionNonLinq()`), o carregamento contínuo é cancelado.
3. **Coexistência com `LoadAmmoAnim`:** Adicionada verificação dinâmica para o `LoadAmmoBundleController`. Se o mod de animação estiver ativo no controle das mãos, o `ContinuousLoadAmmo` não força `SetEmptyHands()` nem disputa `TrySetLastEquippedWeapon()`.
4. **Proteção de FSM e Estado Físico:** Implementada a limpeza explícita de `ESpeedLimit.BarbedWire` e `SprintDisabled` no cancelamento por `StopLoading()` e no `Dispose()`.
5. **Isolamento de Build:** Configurado o `Directory.Build.targets` para direcionar a saída unicamente para `mods/SPT-ContinuousLoadAmmo/builds/`.
6. **Bump SemVer:** Versão elevada de `1.1.8` $\rightarrow$ `1.1.9` $\rightarrow$ `1.1.10` sincronizada em `ContinuousLoadAmmo.cs`, `.csproj` e `mod.json`.
7. **Code Review Formal:** Gerado o relatório [docs/relatorio-code-review-03.md](../docs/relatorio-code-review-03.md) com 100% dos 4 achados aplicados e resolvidos. Compilação Release final executada com 0 erros e 0 avisos.

---

## 2026-08-27 / 2026-08-28 — Sessão 1: Documentação Técnica Modular, Auditoria Estática e Release v1.1.8

**Tema central:** Geração da documentação técnica e arquitetural completa do mod, auditoria estática rigorosa de código com base nas referências canônicas do EFT/SPT/FIKA e aplicação do pacote de estabilização v1.1.8.

**Decisões-chave:**
1. **Documentação Modular Estruturada:** Criada a suíte de documentação em `docs/` dividida em 5 artigos temáticos com diagramas Mermaid conceituais, tabelas comparativas e índice central `docs/README.md`.
2. **Eliminação de Memory Leak (`AUD-01-01` / `CR-02-01`):** Identificado que o encerramento de raid por extração ou abort não disparava `OnIPlayerDeadOrUnspawn`, retendo o `LoadAmmoController` e o `Player` em 5 eventos estáticos. Corrigido adicionando a chamada `Close()` e `_loadAmmoControllerController?.Dispose()` no `OnDestroy` do `LoadAmmoComponent`, tornando o `Dispose()` idempotente através de `_disposed`.
3. **Preservação de Estado da UI (`AUD-01-02` / `CR-02-02`):** Substituída a mutação destrutiva `___inventoryController_0 = null` no `InventoryScreenClosePatch` pelo padrão `__state` do Harmony, restaurando o ponteiro original no `Postfix` e evitando `NullReferenceException` em acessos subsequentes de interface.
4. **Desacoplamento de Classes Ofuscadas (`AUD-01-03` / `CR-02-03`):** Removida a referência volátil `MagazineBuildPresetClass.Class1023.String_0.Localized()` em `MagazinePresetLoader.cs`, adotando a chave canônica `"Preset missing ammo".Localized()`.
5. **Otimização Zero-Alloc (`AUD-01-04` / `CR-02-04`):** Implementado o scratch buffer `_allAmmoScratch` e o delegate estático `_ammoComparison` em `LoadAmmoController.GetAllAmmoForMagazine`, eliminando churn no GC.
6. **Prevenção de Concorrência (`AUD-01-06` / `CR-02-06`):** Adicionado cancelamento automático de presets assíncronos caso o jogador inicie um arrasto manual de munição no inventário.
7. **Bump SemVer:** Versão elevada de `1.1.7` para `1.1.8` sincronizada em `ContinuousLoadAmmo.cs` e `ContinuousLoadAmmo.csproj`.

**Atividade cronológica:**
1. Execução do `/document-mod` gerando 5 artigos modulares e o índice `docs/README.md`.
2. Execução do `/audit-mod-code` produzindo o [relatorio-auditoria-codigo-01.md](../docs/relatorio-auditoria-codigo-01.md) com 6 achados técnicos.
3. Elaboração e aprovação do Plano de Implementação para resolução dos achados.
4. Implementação do código corretivo em `LoadAmmoComponent.cs`, `LoadAmmoController.cs`, `InventoryScreenClosePatch.cs`, `MagazinePresetLoader.cs`, `ContinuousLoadAmmo.cs` e `.csproj`.
5. Compilação Release via MSBuild gerando binários `ContinuousLoadAmmo.dll` v1.1.8 e pacote `ozen-ContinuousLoadAmmo-1.1.8.zip` (0 erros, 0 avisos).
6. Execução do `/code-review` gerando o [relatorio-auditoria-codigo-02.md](../docs/relatorio-auditoria-codigo-02.md) com aprovação técnica final para produção.
