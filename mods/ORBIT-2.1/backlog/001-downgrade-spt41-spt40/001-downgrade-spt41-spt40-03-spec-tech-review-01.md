# 001 — Downgrade SPT 4.1 → 4.0 · Review Técnica 01

**Mod:** ORBIT-2.1
**Spec técnica revisada:** [001-downgrade-spt41-spt40-02-spec-tech.md](001-downgrade-spt41-spt40-02-spec-tech.md)
**Data:** 2026-09-30

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 20 · Total: 20

Os três bloqueadores eram de texto da spec: o código do commit `03c9290e` já continha a correção de PA-01-01 e de PA-01-02 e a spec não as registrava.

**Estado em 2026-09-30, depois da aplicação:** os 20 pontos foram aplicados na spec técnica, no programa de verificação e no código. Cada ponto traz a decisão marcada e a **Resolução**.

## O que foi revisado

| Item | Versão |
|---|---|
| Spec técnica | a do commit `a7854745` (gravada às 22:58). Outra sessão commitou a spec de novo às 23:19 (`9759d7d7`), durante esta review. Comparei as duas versões: mudaram duas coisas, `GClass510.cs:104` → `:85` (PA-01-04, fechado) e "46 apelidos" → "47" (parte de PA-01-15). Os demais pontos valem para a versão `9759d7d7` |
| Spec funcional | `001-downgrade-spt41-spt40-01-spec.md`, gravada às 22:56 |
| Código | commit `a7854745` (o que a spec descreve) e commit `03c9290e`, feito às 23:12 por outra sessão durante esta review. Toda citação `modded/...:linha` e `scripts/...:linha` abaixo é do commit `03c9290e`, salvo quando o texto diz `a7854745` |
| Memória do mod | `mods/ORBIT-2.1/memory/sessions.md` não existia no início da review e entrou no commit `9759d7d7`. Snapshot de 2026-09-30 (sessão 22:10–23:30). Pendências que afetam esta review: P-1.1 🔴 e P-1.2 🔴 (nada foi exercido em jogo nem em coop), P-1.3 🟡 (PA-01-13), P-1.4 🟡 (PA-01-10) |
| Reviews anteriores | nenhuma |
| Docs de `docs/technical/` cujo gatilho dispara | `spt-antipatterns.md` (sempre), `spt4-vs-spt41-gclass-deobfuscation.md` (a spec cita `GClassNNNN`), `fika-packet-desync-prevention-plan.md` (o addon declara dois `INetSerializable`; ver PA-01-11) |
| Grafo de código | o servidor MCP `graphify-eft` não conectou nesta sessão. A auditoria de overrides de PA-01-03 foi feita por busca de texto no decompile |

## Conferência das afirmações contra as fontes

### Linhas cuja fonte é o decompile (todas abertas)

| Seção | Afirmação | Citação | Resultado |
|---|---|---|---|
| §2 | `BotMover.method_10(Vector3 posiblePos, bool withExtra)` | `BotMover.cs:756` | confere |
| §2, §5.3 | `BotMover.method_4(Vector3 castPoint)`, chamado em `SetPlayerToNavMesh` | `BotMover.cs:631` | confere; a chamada está em `BotMover.cs:583`, dentro de `SetPlayerToNavMesh` (`:555`) |
| §2, §5.3 | `GClass494.method_21(CollisionFlags flags, Vector3 deltaMove)` | `GClass494.cs:103` | confere |
| §2 | `GClass510.method_0()` | `GClass510.cs:104` | **não confere** na versão `a7854745`: o arquivo tem 100 linhas; `method_0()` está em `GClass510.cs:85`. Corrigido em `9759d7d7` (PA-01-04) |
| §2 | camadas `GClass45`, `GClass75`, `GClass79`, `GClass48` pelo literal de `Name()` | sem linha | confere: `GClass45.cs:45-48` "AssaultEnemyFar", `GClass75.cs:81-84` "Exfiltration", `GClass79.cs:153-156` "PtrlBirdEye", `GClass48.cs:83-86` "AvoidDanger" |
| §2 | `BotDoorOpener` `method_2`, `method_7`, `method_0`, `method_8` | `BotDoorOpener.cs:455`, `:626`, `:428`, `:737` | os quatro métodos existem nessas linhas. A linha prova que o método existe no 4.0; não prova que é o método que o 4.1 chama pelo nome da coluna "Alvo no 4.1" (PA-01-13) |
| §2 | `BotFirstAidClass.method_3(int? varianAnim, Action callback)` | `BotFirstAidClass.cs:447` | confere; mesma ressalva de PA-01-13 |
| §2 | `GClass49` não sobrescreve `ShallUseNow` | `GClass49.cs` | confere (`GClass49.cs:9-76` só sobrescreve `FindPoint` e `Name`); `GClass49` é a única subclasse de `GClass48` |
| §5.3 | `BotMover.DirCurPoint_1` | `BotMover.cs:40`, `:177` | confere (campo em `:40`, propriedade `DirCurPoint => DirCurPoint_1` em `:177`) |
| §5.3 | `BotMover.LinkedToNavmeshInitially`, `PrevOffsetGoodCasted`, `PrevOffsetGoodCastedTime` | `BotMover.cs:82`, `:124`, `:127` | confere |
| §5.3 | `GClass494.Bool_0`, `CollisionFlags_0` | `GClass494.cs:13`, `:16` | confere; `GClass494.cs:24` (`IsImpostorWorks => Bool_0`) confirma o papel de `Bool_0` |
| §5.3 | `BaseBrain.Owner` | `BaseBrain.cs:10` | confere |
| §5.3 | `BotDoorOpener.Owner`, `CurrentDoorLink`, `DoBreach`, `EnteringDoorSequence` | `BotDoorOpener.cs:250`, `:247`, `:274`, `:283` | confere |
| §5.3 | `BotBoss.method_1`, corpo `BossLogic.SetPatrolMode()` | `BotBoss.cs:312` | confere (corpo em `:314`); é o único método de `BotBoss.cs` que chama `SetPatrolMode` |
| §5.3 | `GClass124.CustomNavigationPoint_0`, `HashSet_0` | `GClass124.cs:38`, `:41` | confere |
| §5.3 | `GClass442.List_0`, `Gclass25_0` | `GClass442.cs:77`, `:32` | confere; papel em `GClass442.cs:87` (`Gclass25_0 = new GClass25(10f, method_0)`) e `:316-328` |
| §5.3 | `GClass3411.Item_0`, `ItemAddress_1` | `GClass3411.cs:12`, `:20` | confere; o papel de `ItemAddress_1` como destino está em `GClass3411.cs:89` (`ItemAddress_1 = to`) |
| §5.3 | `GClass3687.Boolean_0` | `GClass3687.cs:90` | confere; é a única propriedade `bool` do tipo |
| §5.3 | `PatrolDataFollower.FollowerAIBase` | `PatrolDataFollower.cs:8` | confere |
| §2, §5.3 | `FikaPlayer.vmethod_1` / `vmethod_0` | `FikaPlayer.cs:1361`, `:1350` | confere na fonte vendorizada, que é o Fika 2.3.8; o instalado é o 2.3.21 (PA-01-17) |
| §5.5 | campo do dono é `BotOwner_0` nas derivadas de `GClass429` e de `GClass177<T>`, e `Owner` em `BotDoorOpener` e `PatrolPointChooserBasic` | sem linha | confere para os 14 tipos que `NativeGhostBodyPatches.Bind` recebe: `GClass429.cs:8`, `GClass177-1.cs:8`, `BotDoorOpener.cs:250`, `PatrolPointChooserBasic.cs:85`. Cada tipo tem um só dos dois campos na sua cadeia de herança |

### Linhas cuja fonte é o commit `00e7ad8` (12 abertas, todas as que a spec cita)

| Seção | Par 4.1 → 4.0 | Linha do diff | Resultado |
|---|---|---|---|
| §5.3 | `BotMover._owner` → `BotOwner_0` | `:628-629` | confere |
| §5.3 | `BotMover._player` → `Player` | `:61-62` | confere |
| §5.3 | `_lastGoodCastPoint`, `_lastGoodCastPointTime` → `LastGoodCastPoint`, `LastGoodCastPointTime` | `:44-48` | confere |
| §5.3 | `_prevSuccessLinkedFrom`, `_prevPosLinkedTime` → `PrevSuccessLinkedFrom_1`, `PrevPosLinkedTime_1` | `:44-46`, `:641-642` | confere |
| §5.3 | `_prevLinkPos`, `_positionOnWayCasted` → `PrevLinkPos`, `PositionOnWayCasted` | `:644-649` | confere |
| §2, §5.3 | `BotMover.CastFromPos` → `method_10` | `:668-669` | confere |
| §5.3 | `BaseLogicLayerAbstractClass._owner` → `BotOwner_0` | `:538-539` | confere |
| §5.3 | `Player.ExecuteInteraction` → `vmethod_1` | `:866-867` | confere |
| §2, §5.3 | `ClientAirDrop.PlayLandingSound`, `_syncObject` → `AirdropLogicClass.method_0`, `AirdropSynchronizableObject_0` | `:513-522` | confere |
| §2, §5.3 | `AICoreController.Update`, `_enable` → `AICoreControllerClass.Update`, `Bool_0` | `:602-603`, `:610-613` | confere; `AICoreControllerClass.cs:8` é o único campo `bool` do tipo |
| §5.3 | `ActiveEvents` → `List_0` | `:83-84`, `:96-97` | confere |
| §5.3 | `BotSpawner._allBotZones` → `AllBotZones` | `:982-985` | confere (`EFT/BotSpawner.cs:125`) |

### Demais tabelas

| Seção | O que conferi | Resultado |
|---|---|---|
| §5.1 | os pares de `Spt40TypeAliases.cs` contra `docs/files-from-4.1/consolidated-mappings.txt` | os 47 pares conferem; a versão `a7854745` da spec dizia 46 (PA-01-15) |
| §5.2 | as 10 linhas contra `consolidated-mappings.txt` | conferem (a décima está em `consolidated-mappings.txt:2304`) |
| §5.4 | os 17 pares de `Spt40TypeNames.cs` contra `consolidated-mappings.txt` e contra o literal de `Name()` de cada classe | os 17 conferem; `GClass92` e `GClass138` não declaram `Name()` e herdam de `GClass91` e `GClass135`, como o arquivo diz |
| §5.6 | `SPTWeb.cs:16`, `AbstractModMetadata.cs:13-83`, `IOnLoad.cs`, `OnLoadOrder.cs:6-7`, `Router.cs:182-184`, `D:\SPT\SPT\SPT.Server.runtimeconfig.json` | conferem; `PreSptModLoader` (100000) vem antes de `Database` (200000) |
| §1.1 | versões instaladas | conferem pela versão de arquivo: EFT 0.16.9.40087, SPT 4.0.13, Fika.Core 2.3.21, SAIN 4.8.0, BigBrain 1.4.0, Waypoints 1.8.2, MudBlazor 8.13.0, 0Harmony 2.9.0 |

### Execução do dry-run

Copiei `scripts/patch-dryrun/bin/Release/PatchDryRun.exe` (binário de 23:02) para a pasta temporária desta sessão e rodei contra `D:\SPT` e `builds/client` (build de 23:07), às 23:10. Saída: 43 classes `ModulePatch` com `OK`, 90 chamadas a `Harmony.Patch` em 80 métodos distintos, 4 delas com transpiler, `body guards ready (32 entry points)`, seção 3b com 8 e 3 referências resolvidas, `RESULT: OK — no failure`. Não rodei `verify-port.sh`, que compila dentro do repositório.

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | A — Gap | 🔴 | §5.5 não lista os campos buscados por texto | ✅ Resolvido |
| PA-01-02 | A — Gap | 🔴 | §5.6 só lista o que o compilador acusa; falta o carregamento do MudBlazor pelo layout | ✅ Resolvido |
| PA-01-03 | C — Lógica | 🔴 | §9 check 3 está ✅ com evidência que ainda não existe | ✅ Resolvido |
| PA-01-04 | C — Lógica | 🟡 | `GClass510.cs:104` não existe | ✅ Resolvido |
| PA-01-05 | C — Lógica | 🟡 | Decisão de `DormantBoarAvoidDangerBypassPatch`: o resultado confere, a justificativa omite três fatos | ✅ Resolvido |
| PA-01-06 | C — Lógica | 🟡 | Checkpoint 2 afirma mais do que `PatchDryRun` confere | ✅ Resolvido |
| PA-01-07 | A — Gap | 🟡 | Checkpoint 3: o script cobre nome de tipo; nome de membro buscado depois do início não tem verificação | ✅ Resolvido |
| PA-01-08 | A — Gap | 🟡 | Critérios de aceite e corner cases sem checkpoint em §8 | ✅ Resolvido |
| PA-01-09 | B — Edge case | 🟡 | Caminho de instalação escrito em texto não é categoria de §1.3 | ✅ Resolvido |
| PA-01-10 | A — Gap | 🟡 | §7 dá os mods de facção como não verificáveis; UNTAR e MoreBotsAPI estão instalados | ✅ Resolvido |
| PA-01-11 | A — Gap | 🟡 | §9 check 11 está N/A com dois `INetSerializable` no addon | ✅ Resolvido |
| PA-01-12 | A — Gap | 🟡 | §7 "build do jogo diferente": a mitigação citada cobre 4 de 80 alvos | ✅ Resolvido |
| PA-01-13 | A — Gap | 🟡 | Cinco pares de método de §2 não têm prova do lado 4.1 | ✅ Resolvido |
| PA-01-14 | A — Gap | 🟡 | §9 checks 1 e 2 estão N/A com razão que o código não sustenta | ✅ Resolvido |
| PA-01-15 | C — Lógica | 🟢 | Contagens da spec que não batem com o repositório | ✅ Resolvido |
| PA-01-16 | A — Gap | 🟢 | Nome 4.0 formado por tipo e ordinal: a spec cita a declaração, não a linha que prova o papel | ✅ Resolvido |
| PA-01-17 | A — Gap | 🟢 | A evidência do Fika é do 2.3.8; o instalado é o 2.3.21 | ✅ Resolvido |
| PA-01-18 | A — Gap | 🟢 | Dois grupos de nome em texto fora de §1.3 que hoje conferem | ✅ Resolvido |
| PA-01-19 | A — Gap | 🟢 | O cartão `HomePage` do 4.1 não existe no 4.0 e não virou pendência | ✅ Resolvido |
| PA-01-20 | A — Gap | 🟢 | A build do port tem a mesma versão do upstream | ✅ Resolvido |

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Pontos

### PA-01-01 · A — Gap · 🔴 Bloqueador · ✅ Resolvido em 2026-09-30

**§5.5 não lista os campos buscados por texto**

**Problema:** §1.3 (linha E) e §5.5 dão a categoria E como coberta por `Spt40Members.cs`: cinco métodos e o campo do dono do bot. No commit `a7854745`, que é o que a spec descreve, cinco nomes de campo do 4.1 continuavam passados como texto a `AccessTools.Field`:

| Nome 4.1 em texto | Onde | Campo no 4.0 |
|---|---|---|
| `_lootingNow` | `modded/Orbit/Systems/NativeGhostLoot.cs:18` | `LootingNow` (`PatrolLootPointsData.cs:135`) |
| `_shallStartInteract` | `modded/Orbit/Systems/NativeGhostPatrol.cs:21`, para três tipos | `bool_1` (`DropItemReservWay.cs:11`), `bool_0` (`DropItemAndHealReservWay.cs:11`), `bool_0` (`UseSurgeKitReservWay.cs:14`) |
| `_comeTime` | `modded/Orbit/Systems/NativePatrolDiagnostics.cs:32` | `ComeTime` (`PatrollingData.cs:34`) |
| `_reservChoosedTime` | `modded/Orbit/Systems/NativePatrolDiagnostics.cs:33` | `ReservChoosedTime` (`PatrollingData.cs:37`) |
| `_nextChangeWay` | `modded/Orbit/Systems/NativePatrolDiagnostics.cs:34` | `NextChangeWay` (`PatrolPointChooserBasic.cs:88`) |

A busca pelo nome 4.1 retorna nulo, `CreateLootingReader` e `PendingReader` retornam nulo, e nenhuma linha vai para o log. O commit `03c9290e` adicionou `Spt40Members.Field` (`Spt40Members.cs:51`), que lê campo do jogo pelo nome 4.0, e a tabela `Fields` (`Spt40Members.cs:37-48`). A spec não foi alterada: §4 não lista os três arquivos, §5.5 não tem a tabela de campos, e o checkpoint 2 de §8 não cita a seção 3b do dry-run, que é a que confere essas referências.

**Por que importa:** com o leitor nulo, `NativeGhostLoot.BodyReason` retorna `native-loot-operation` para todo bot nativo com `USE_REAL_LOOTING` (`NativeGhostLoot.cs:37`), e `NativeGhostPatrol` mantém acordado o bot que está num dos três scripts de patrulha (o comentário de `NativeGhostPatrol.cs:27` diz isso). Li o código; não executei em jogo. É o corner case "nenhum pode falhar em silêncio" da spec funcional. O `Program.cs` do commit `a7854745` não tinha verificação que alcançasse essas buscas (a seção 3b entrou em `03c9290e`); não rodei aquela versão. §1.3 diz que o diff pequeno existe para refazer o port numa versão futura do ORBIT; quem refizer seguindo §5.5 repete o defeito.

**Sugestão:** (1) em §5.5, adicionar a tabela acima com as sete linhas de `Fields`, incluindo as linhas onde cada `bool_N` é atribuído (o comentário de `Spt40Members.cs:45-47` já as tem); (2) em §4, adicionar `NativeGhostLoot.cs`, `NativeGhostPatrol.cs` e `NativePatrolDiagnostics.cs` como MODIFICAR (categoria E) e `Spt40Members.Field` no resumo de `Spt40Members.cs`; (3) em §1.3 linha E, trocar "nome de membro" por "nome de método e de campo"; (4) no checkpoint 2 de §8, citar a seção 3b e o critério dela (nenhuma referência nula fora da lista `ExpectedNull` de `Program.cs:487`).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Spec §5.5 ganhou a tabela dos sete campos lidos por reflexão (com quem lê e o efeito de errar) e §4 lista os três arquivos; o checkpoint 2 em §8 cita a seção 3b do dry-run. O código já tinha a correção (`Spt40Members.Field`, commit `03c9290e`).

### PA-01-02 · A — Gap · 🔴 Bloqueador · ✅ Resolvido em 2026-09-30

**§5.6 só lista o que o compilador acusa; falta o carregamento do MudBlazor pelo layout**

**Problema:** §1.3 (linha G) fala em "4 pontos de contrato" e §5.6 lista oito linhas, duas com evidência "compilação". Falta uma diferença que compila e muda o comportamento do painel. O documento do host do 4.0 só carrega `/_framework/blazor.web.js` (`references/spt-source/Libraries/SPTarkov.Server.Web/Components/App.razor:1-16`); o CSS e o JS do MudBlazor são carregados por cada layout (`.../Components/Layout/BaseMudBlazorLayout.razor:5` e `:15`). As 13 páginas do ORBIT usam o layout próprio `MainLayout.razor`, que no commit `a7854745` não carregava nenhum dos dois (zero ocorrências de `MudBlazor.min`). O commit `03c9290e` adicionou as duas referências (`modded/Orbit.Server/Web/Shared/MainLayout.razor:16` e `:24`); o comentário em `MainLayout.razor:10-13` descreve o sintoma: sem o script, a página responde, fica sem estilo e não reage a clique. Não rodei o servidor; li os três arquivos.

**Por que importa:** critérios de aceite 2 ("abrir cada página do painel sem erro") e 6 ("mudar uma opção, salvar"). O checkpoint 4 de §8 pede "conferir carga, as 13 páginas e as 3 rotas", e o script que o executa declara que só prova a renderização no servidor (`scripts/server-smoke-test.ps1:11-12`). Uma resposta 200 de cada página passa o checkpoint com o painel sem interação. Aqui "compila" e "responde 200" foram tratados como prova de que o painel funciona.

**Sugestão:** (1) adicionar em §5.6 a linha "CSS e JS do MudBlazor: no 4.0 quem carrega é o layout, não o documento do host", com as três citações acima; o lado 4.1 dessa linha precisa de fonte própria, que não está vendorizada aqui (`references/spt-source` é o 4.0.13); (2) em §4, adicionar `Web/Shared/MainLayout.razor` como MODIFICAR; (3) em §1.3 linha G, dizer que a categoria inclui diferença de comportamento do host web, que o compilador não acusa; (4) no checkpoint 4, separar o que o script prova (carga, HTML de cada página, três rotas) do passo em navegador (abrir uma página, acionar um controle, salvar e reler o valor), e dizer qual critério cada um cobre.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Spec §1.3 ganhou a categoria H (hospedeiro web), §5.6 a linha do MudBlazor e §4 o `MainLayout.razor`; o checkpoint 4 em §8 separa o que o script prova do passo em navegador. Código corrigido no commit `03c9290e`.

### PA-01-03 · C — Erro de lógica · 🔴 Bloqueador · ✅ Resolvido em 2026-09-30

**§9 check 3 está ✅ com evidência que ainda não existe**

**Problema:** o check 3 (alvos virtuais com overrides auditados, AP-03) está ✅ com o texto "overrides auditados no checkpoint 5". O checkpoint 5 está desmarcado em §8 e a spec não traz o resultado da auditoria. O comando de review trata ✅ sem evidência verificável como bloqueador. Fiz a auditoria por busca no decompile e na fonte do Fika 2.3.8:

| Alvo do patch | Declaração | Override | O override chama a base? |
|---|---|---|---|
| `GClass45.ShallUseNow`, `GClass75.ShallUseNow`, `GClass79.ShallUseNow` | virtual | nenhuma subclasse | — |
| `GClass48.ShallUseNow` | `GClass48.cs:88` | `GClass49` não sobrescreve | — |
| `BotMover.GoToByWay` (`NativeGhostWayOrderPatch`) | `BotMover.cs:378`, virtual | `GClass493.cs:30` (mover do BTR), corpo vazio | não; o patch não roda para o BTR |
| `Player.CreateCorpse()` (`CorpseRegistrationPatch`) | `EFT/Player.cs:30694`, virtual | `FikaPlayer.cs:1296`; `ObservedPlayer.cs:1096` | `FikaPlayer` só chama a base quando `FikaBackendUtils.IsServer`; `ObservedPlayer` não chama |
| `Player.OnItemAddedOrRemoved` (`InventoryChangePatch`) | `EFT/Player.cs:29212`, virtual | `ObservedPlayer.cs:642`, corpo vazio | não |
| `Player.InitVaultingComponent` (`BotVaultingPatch`) | `EFT/Player.cs:28727`, virtual | `EFT/HideoutPlayer.cs:665`, `EFT/NarratePlayer.cs:70` | não conferi |
| `MovementContext.UpdateGroundCollision` (`DormantGroundCollisionPatch`) | `EFT/MovementContext.cs:2621`, virtual | `ObservedMovementContext.cs:175` | não aparece `base.UpdateGroundCollision` em `:175-190`; não li o método até o fim |
| `GameWorld.Dispose` (`OrbitDisposePatch`) | `EFT/GameWorld.cs:2115`, virtual | `EFT/ClientGameWorld.cs:324`; `FikaHostGameWorld.cs:109`; `FikaClientGameWorld.cs:121` | os três chamam (`:326`, `:111`, `:123`) |
| `LootItem.Kill` (`LootItemKilledPatch`) | `EFT.Interactive/LootItem.cs:536` | `EFT.Interactive/Corpse.cs:278` | chama (`Corpse.cs:284`) |

As linhas do Fika são da versão 2.3.8; o instalado é o 2.3.21.

**Por que importa:** AP-03. No host e no headless todo jogador humano remoto é um `ObservedPlayer`, então `CorpseRegistrationPatch` e `InventoryChangePatch` não rodam para ele. A memória do mod registra o caso do corpo como herdado do upstream (P-1.5); a spec não registra nenhum dos dois, e o ✅ do check 3 diz ao leitor que a auditoria foi feita.

**Sugestão:** trocar a evidência do check 3 pela tabela acima, completando as duas linhas que deixei como "não conferi". Para cada linha em que o override não chama a base, escrever a consequência e a decisão ("igual ao upstream, fora do escopo do port" com a pendência da memória, ou correção). Enquanto a tabela não estiver na spec, o status do check 3 é pendente, não ✅.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Spec §7.4 traz a tabela de overrides completa, com decisão por linha; as duas linhas que estavam como "não conferi" foram fechadas com a leitura do revisor de patches (corpos vazios em `HideoutPlayer`/`NarratePlayer`; `ObservedMovementContext.UpdateGroundCollision` não chama a base). §9 check 3 aponta para §7.4.

### PA-01-04 · C — Erro de lógica · 🟡 Importante · ✅ Resolvido em 2026-09-30

**`GClass510.cs:104` não existe**

**Problema:** na versão `a7854745`, §2 cita `GClass510.cs:104` como evidência de `GClass510.method_0()`. O arquivo tem 100 linhas. O método está em `GClass510.cs:85` e é chamado em `GClass510.cs:68`. O alvo está certo: o dry-run resolve `NativePatrolArrivalDiagnosticPatch` em `GClass510.method_0()`.

**Por que importa:** é a única das citações de decompile que não abre no ponto citado, e o check 9 de §9 está ✅ com base nessas citações.

**Sugestão:** trocar `GClass510.cs:104` e a âncora `#L104` por `GClass510.cs:85` em §2.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** o commit `9759d7d7` (23:19, outra sessão) trocou a citação por `GClass510.cs:85`. Conferi a linha no decompile.

### PA-01-05 · C — Erro de lógica · 🟡 Importante · ✅ Resolvido em 2026-09-30

**Decisão de `DormantBoarAvoidDangerBypassPatch`: o resultado confere, a justificativa omite três fatos**

**Problema:** §7 diz que os dois prefixos fazem o mesmo teste, que o resultado é igual, e que o custo é "uma comparação". O resultado confere:

- `GClass49` não sobrescreve `ShallUseNow`, então `AccessTools.Method(typeof(BoarAvoidDangerLayer), "ShallUseNow")` retorna `GClass48.ShallUseNow`. O dry-run mostra os dois patches no mesmo método.
- O HarmonyX 2.9.0 instalado chama todos os prefixos e junta os retornos com `And`; o segundo prefixo roda mesmo quando o primeiro retorna `false` (decompilei `HarmonyLib.Public.Patching.HarmonyManipulator.WritePrefixes` de `D:\SPT\BepInEx\core\0Harmony.dll`).
- Os dois prefixos chamam `DormantLayerGate.OwnerIsDormant` (`modded/Orbit/Patches/DormantDangerLayerBypassPatch.cs:41` e `:56`) e gravam `__result = false` na mesma condição.

O que a justificativa não diz:

1. **A instância que não é `GClass49`.** O parâmetro do segundo prefixo é `BoarAvoidDangerLayer __instance`, isto é, `GClass49` (`DormantDangerLayerBypassPatch.cs:54`). O alvo é `GClass48.ShallUseNow`, e `new GClass48(` aparece em 46 arquivos do decompile (por exemplo `GClass311.cs:52`); `new GClass49(` só em `GClass312.cs:25` e `GClass331.cs:40`. O HarmonyX passa `__instance` com `ldarg.0`, sem conversão nem checagem de tipo (mesmo decompile, ramo `item.Name == InstanceParam`). Na maioria das chamadas o prefixo recebe um `GClass48` num parâmetro declarado `GClass49`. Hoje isso não lança erro porque o prefixo só repassa a instância a `OwnerIsDormant(BaseLogicLayerSimple layer)` (`:19`), que lê `BotOwner_0`, declarado em `BaseLogicLayerAbstractClass.cs:9`. Um acesso a membro declarado em `GClass49` (`List_0`, `Bool_4`, `Float_4`, `GClass49.cs:26-32`) leria memória fora do objeto, sem exceção no Mono.
2. **O alcance.** No 4.0 a chamada duplicada acontece para todo bot que tem a camada AvoidDanger, a cada avaliação de `ShallUseNow` (`AICoreStrategyAbstractClass-1.cs:101`), e não só para os guardas do Kaban.
3. **O custo.** Cada chamada lê `BotOwner_0.GetPlayer.ProfileId` e chama `DormancySystem.IsDormantProfile`, que consulta um `HashSet<string>` e retorna antes da consulta quando ele está vazio (`modded/Orbit/Systems/DormancySystem.cs:93-94`). É pequeno, e é mais do que uma comparação.

O dry-run não detecta o item 1: `Compatible` aceita o tipo do parâmetro quando ele é base **ou** derivado do tipo que declara o alvo (`scripts/patch-dryrun/Program.cs:414-415`).

Não verifiquei se no 4.1 `BoarAvoidDangerLayer` sobrescreve `ShallUseNow`: não há assembly do 4.1 no repositório.

**Por que importa:** "comportamento correto" é verdade hoje e depende de uma condição que a spec não escreve. Uma versão futura do upstream que use um membro de `GClass49` nesse prefixo compila, passa no dry-run e corrompe memória em raid.

**Sugestão:** manter a decisão e reescrever o parágrafo de §7 com os três fatos, mais a regra "o prefixo de `DormantBoarAvoidDangerBypassPatch` não pode acessar membro declarado em `GClass49`". Alternativa que remove a condição: em `modded/Orbit/Plugin.cs:144`, só habilitar `DormantBoarAvoidDangerBypassPatch` quando o método resolvido for declarado em `BoarAvoidDangerLayer`; custa uma edição a mais num arquivo do upstream. Nos dois casos, fazer o dry-run reportar (a) dois patches resolvidos no mesmo método e (b) `__instance` declarado como subclasse do tipo que declara o alvo.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[x]` Caminho alternativo: habilitar o patch só quando o alvo é declarado em `GClass49`

**Resolução:** Caminho alternativo, mais forte que reescrever o parágrafo: o patch ganhou a propriedade `Applies` e `Plugin.cs` só o liga quando `BoarAvoidDangerLayer` declara `ShallUseNow`. No 4.0 ele não liga. §7.3 registra os três fatos e a decisão; o dry-run lista o patch como `N/A`.

### PA-01-06 · C — Erro de lógica · 🟡 Importante · ✅ Resolvido em 2026-09-30

**Checkpoint 2 afirma mais do que `PatchDryRun` confere**

**Problema:** §8 diz que o checkpoint 2 "roda `Enable()` de todos os patches e confere alvo, ligação de parâmetros e transpilers", e o título é "patches aplicam". Lendo `scripts/patch-dryrun/Program.cs`:

| Afirmação da spec | O que o programa faz | O que fica sem prova |
|---|---|---|
| "patches aplicam" | substitui `Harmony.Patch` por um prefixo que retorna `false` (`Program.cs:108-110`, `:341`); nenhum desvio de método é construído | que o HarmonyX consegue gerar o método desviado |
| "confere ligação de parâmetros" | reimplementa as regras do HarmonyX em `Bind` (`Program.cs:354-412`); não executa o código do HarmonyX | divergência entre a reimplementação e o HarmonyX |
| idem, parâmetro `___campo` | confere que o campo existe (`Program.cs:390`) | o tipo do campo; o HarmonyX também não confere e emite `ldfld` direto |
| idem, `__instance` | aceita tipo base ou derivado (`Program.cs:414-415`) | o caso de PA-01-05 |
| "confere transpilers" | chama o transpiler sobre o IL do alvo e só falha com exceção ou lista vazia (`Program.cs:417-435`) | que o IL devolvido é válido |
| critério "zero `FAIL`" | linha de `Warning` ou `Error` que o próprio mod grava é impressa e não conta como falha (`Program.cs:192-196`, `:281`) | o critério de aceite 4 exige nenhuma linha "failed to enable" nem "unavailable" |
| §7: "o checkpoint 2 mostra qual resolve" (SAIN) | `Probe` imprime os campos e não falha com `NULL` (`Program.cs:460-484`); só imprime campo de tipo `MemberInfo`, delegate ou `bool` (`:474`) | `_untar`, `_ruaf` e `_isb` de `NativeGhostAdapters` não aparecem; `_rvrBoard`, `_rvrMember` e `_rvrOrders` aparecem `NULL` com `RESULT: OK` |
| "todos os patches" | enumera toda classe `ModulePatch` do assembly (`Program.cs:116`) e três conjuntos manuais citados por nome (`:138-144`) | um conjunto manual novo no upstream fica fora sem aviso. Hoje as 43 classes são as mesmas 43 que `Plugin.cs:102-156` habilita |
| — | roda no CLR de desktop, onde 53 tipos do `Assembly-CSharp` não carregam e são retirados de `GetTypes()` (`Program.cs:106-107`, `:287-292`) | `NativeGhostBodyPatches.Bind` procura overrides em `GetTypes()` (`modded/Orbit/Patches/NativeGhostBodyPatches.cs:71-73`); um override num desses 53 tipos ficaria fora |

O que o programa prova: o método alvo existe com aquele nome nas DLLs instaladas; os nomes de parâmetro e de campo dos patches existem no alvo; os transpilers, nos 4 métodos em que entram, encontram exatamente uma vez a chamada que procuram; as referências de reflexão em campo `static readonly` não são nulas.

**Por que importa:** §1.2 chama o compilador e o dry-run de "juiz" das fontes 1 a 3, e §1.3 diz que as categorias D, E e F são "o alvo dos checkpoints 2 e 3". O leitor conclui que zero `FAIL` cobre essas categorias. PA-01-01 mostra um caso em que não cobria.

**Sugestão:** (1) reescrever o checkpoint 2 com a frase "o que prova" acima e a lista do que não prova, e trocar o título para "alvos e nomes resolvem"; (2) no programa: contar como `FAIL` toda linha `Warning` ou `Error` do mod que não esteja numa lista de esperadas com motivo; contar como `FAIL` todo `NULL` da seção 3 fora dessa lista; conferir que o tipo do campo de `___campo` é atribuível ao tipo do parâmetro; exigir que `__instance` seja atribuível a partir do tipo que declara o alvo; (3) gravar a saída do dry-run junto do as-built, porque §2 a cita como evidência ("lista completa na saída de `scripts/verify-port.sh`") e ela só existe em máquina com o jogo instalado.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** §8 tem a tabela "o que prova / o que não prova" por checkpoint. O `PatchDryRun` passou a contar como falha qualquer aviso ou erro que o mod grave no próprio log, e os membros que não ligam quando o mod opcional está instalado.

### PA-01-07 · A — Gap · 🟡 Importante · ✅ Resolvido em 2026-09-30

**Checkpoint 3: o script cobre nome de tipo; nome de membro buscado depois do início não tem verificação**

**Problema:** o checkpoint 3 tem duas partes. A primeira é `scripts/check-type-name-literals.py`. Ele prova que todo literal de texto de `Orbit`, `Orbit.Fika` e `Shared` igual ao nome curto de um tipo 4.1 da tabela tem uma linha `typeof(X), "Nome"` em `Spt40TypeNames.cs` (`check-type-name-literals.py:49`, `:66`). Rodei: 21 literais, 17 cobertos, 4 ignorados, 0 faltando. Ele não prova:

- que o par da linha está certo (conferi os 17 à mão; ver a tabela de conferência);
- que o ponto de comparação lê o nome por `Spt40TypeNames.Of`. Hoje toda comparação passa por `NativeGhostPartisan.Layer` (`modded/Orbit/Systems/NativeGhostPartisan.cs:10`); os outros `GetType().Name` do mod só entram em linha de log. Uma comparação nova com `GetType().Name` direto sairia como `OK`;
- nome de tipo com namespace (a chave da busca é o nome curto, `check-type-name-literals.py:42`) e `nameof(Apelido)`, que retorna o nome 4.1 do apelido;
- nome de membro: o script não lê membro nenhum.

A segunda parte, "auditoria independente de toda reflexão por texto contra as DLLs instaladas", não tem script, artefato nem critério. As buscas por nome que rodam depois do início não passam por nenhuma verificação, porque a seção 3b do dry-run só confere campo `static readonly` (`Program.cs:499`):

| Busca por texto | Onde | Conferi no 4.0? |
|---|---|---|
| campo `Player` de `ActiveHealthController` | `modded/Orbit/Patches/DormantDamageProbePatch.cs:72` | existe (`EFT.HealthSystem/ActiveHealthController.cs:3369`) |
| campo `_player` de `MovementContext` | `modded/Orbit/Patches/DormantGroundCollisionPatch.cs:33` | existe (`EFT/MovementContext.cs:157`) |
| campo `LastAggressor` de `Player` | `modded/Orbit/Patches/CorpseRegistrationPatch.cs:100` | existe (`EFT/Player.cs:24358`) |
| propriedade `iPlayer` | `modded/Orbit/Patches/DormantDamageProbePatch.cs:46` | não conferi o tipo do objeto lido |
| quatro tipos e cinco membros do Fika | `modded/Orbit/Helpers/GhostSpectatorPlayers.cs:29-44` | os nomes aparecem como texto em `Fika.Core.dll` 2.3.21; não conferi tipo nem assinatura |
| `MoreBotsAPI.Components.BotHuntManager` e membros | `modded/Orbit/Systems/NativeGhostSystem.cs:145-149`, `NativePatrolDiagnostics.cs:229-235` | o nome do tipo aparece como texto em `MoreBotsPlugin.dll` |

**Por que importa:** a categoria E é a que a spec diz que falha em silêncio. Para ela, a única verificação automática cobre o que roda no início; o restante depende de uma auditoria que a spec não descreve.

**Sugestão:** (1) em §8, descrever a auditoria manual: o comando de busca, a lista de pontos encontrados e o resultado por ponto, gravados no as-built; (2) estender o dry-run para resolver essas buscas contra as DLLs instaladas, chamando o construtor de `GhostSpectatorPlayers` e os métodos que fazem as buscas tardias, como a seção 3 já faz para `SainPersonality.InitIfNeeded`; (3) no script Python, falhar quando houver comparação (`==`, `is`, `switch`) de `GetType().Name` ou `GetType().FullName` fora de `NativeGhostPartisan.Layer`.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** §8 descreve a auditoria por leitura (escopo, 16 helpers, 14 achados). O dry-run passou a exercer os resolvedores tardios de MoreBotsAPI (`NativeGhostRegroup.Resolve`, `NativeGhostSystem.HuntUpdater`) e de UNTAR (`NativeGhostAdapters.ResolveBindings`). `GhostSpectatorPlayers` não roda fora do jogo (o `Chainloader` do BepInEx não inicializa): aparece como `SKIP` e ficou coberto só pela leitura contra a DLL 2.3.21.

### PA-01-08 · A — Gap · 🟡 Importante · ✅ Resolvido em 2026-09-30

**Critérios de aceite e corner cases sem checkpoint em §8**

**Problema:** §8 não liga cada critério da spec funcional a um checkpoint.

| Item da spec funcional | Checkpoint de §8 | Lacuna |
|---|---|---|
| Critério 1 (compilar) | checkpoint 1 | — |
| Critério 2 (servidor carrega, nenhuma linha de erro, cada página abre) | checkpoint 4 | não exige log sem erro; não exige navegador (PA-01-02); roda numa cópia sem os outros 18 mods de `D:\SPT\SPT\user\mods` |
| Critério 3 (configuração e zonas em JSON válido) | checkpoint 4 | o texto diz "conferir as 3 rotas", sem dizer que o JSON é validado |
| Critérios 4 a 8 | uma linha: "validação em jogo (humana)" | sem roteiro, sem evidência a guardar. O critério 4 (linhas de prontidão, nenhuma "failed to enable" nem "unavailable") pode ser conferido em parte fora do jogo (PA-01-06) |
| Corner case 1 (patch sem alvo: os demais seguem, o log diz qual) | nenhum | `EnableSafe` existe (`modded/Orbit/Plugin.cs:276-293`); nada o exercita |
| Corner case 2 (nome de classe em texto) | checkpoint 3 | — |
| Corner case 3 (mods opcionais em versão 4.0) | nenhum | PA-01-10 |
| Corner case 4 (servidor sem o mod ou fora do ar) | nenhum | — |
| Corner case 5 (ORBIT 1.2.1 instalado junto) | só risco em §7 | `D:\SPT\BepInEx\plugins\ORBIT\` (1.2.1) existe nesta máquina; falta o passo de instalação e a conferência |
| Corner case 6 (pasta com nome diferente de `ORBIT`) | nenhum | PA-01-09 |
| Corner case 7 (headless: preço vem do servidor) | nenhum | `HandbookPriceCache` lê a rota `/client/handbook/templates` e, se ela falha, `SPT/SPT_Data/database/templates/handbook.json` (`modded/Orbit/Looting/HandbookPriceCache.cs:84`); o arquivo existe nesse caminho em `D:\SPT` |

**Por que importa:** seis dos sete corner cases e os critérios 4 a 8 não têm passo de verificação escrito. O item pode ser marcado entregue sem que ninguém os percorra (AP-06).

**Sugestão:** adicionar a §8 uma tabela "critério → checkpoint → evidência" com uma linha por critério e por corner case. Para os que só se verificam em jogo, escrever o passo e o que guardar (trecho de log, captura). A memória do mod cita um roteiro de sete passos no as-built; a spec pode apontar para ele.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** §8 ganhou a tabela "critério → checkpoint → estado", com uma linha por critério de aceite e por corner case.

### PA-01-09 · B — Edge case · 🟡 Importante · ✅ Resolvido em 2026-09-30

**Caminho de instalação escrito em texto não é categoria de §1.3**

**Problema:** §1.3 lista oito categorias de diferença e nenhuma cobre caminho de pasta escrito em texto. No commit `a7854745`, o arquivo que monta o ZIP de exportação de preset usava `SPT_Runtime/user/mods/ORBIT/addon/{name}/` (`modded/Orbit.Server/Presets/PresetArchive.cs:46`), e duas telas mostravam o mesmo caminho (`Web/Pages/Presets.razor:66`, `Web/Shared/PresetScopeDialog.razor:32`). `SPT_Runtime` é a pasta do servidor no 4.1; no 4.0 a pasta é `SPT` (`D:\SPT\SPT`), e a pasta do mod neste repositório é `ORBIT-2.1`. O commit `03c9290e` adicionou `Spt40Paths.AddonFolder` (`modded/Orbit.Server/Compat/Spt40ServerCompat.cs:35-43`), que monta o caminho com `SPT/` e com o nome da pasta onde a DLL está. A spec não registra a categoria nem a correção.

**Por que importa:** corner case 6 da spec funcional. Um ZIP exportado no painel do 4.0 e extraído "na pasta do jogo" cria `SPT_Runtime\user\mods\ORBIT\addon\`, e o servidor lê os addons da pasta onde a DLL está. A memória registra que a exportação não foi executada depois da correção (P-1.7).

**Sugestão:** (1) adicionar a §1.3 a categoria "I. Caminho de instalação em texto" (pasta do servidor `SPT_Runtime` → `SPT`; pasta do mod `ORBIT` → pasta real), tratada em `Spt40ServerCompat.cs`; (2) adicionar a §4 `PresetArchive.cs`, `Presets.razor` e `PresetScopeDialog.razor`; (3) adicionar ao `verify-port.sh` uma busca que falha quando `SPT_Runtime`, `user/mods/ORBIT/` ou `plugins/ORBIT/` aparecem em `modded/` fora de `Compat/`; (4) incluir no checkpoint 4 a exportação de um preset e a conferência do caminho dentro do ZIP.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Categoria I criada em §1.3, três arquivos em §4, linha em §5.6. `verify-port.sh` ganhou a parte 3/3, que falha quando uma linha de código contém `SPT_Runtime` ou `user/mods/ORBIT/`; conferido reintroduzindo o caminho antigo de propósito.

### PA-01-10 · A — Gap · 🟡 Importante · ✅ Resolvido em 2026-09-30

**§7 dá os mods de facção como não verificáveis; UNTAR e MoreBotsAPI estão instalados**

**Problema:** §7 diz que as integrações com UNTAR, RUAF, Black Division, ISB e Combine Soldiers usam reflexão em mods "que não estão instalados na máquina de desenvolvimento" e conclui "não verificável aqui". Em `D:\SPT\BepInEx\plugins` existem `tacticaltoaster-untargohome\UNTARGHPlugin.dll` e `MoreBotsAPI\MoreBotsPlugin.dll`. Os nomes que o ORBIT busca aparecem como texto nas duas DLLs: `BotUntarManager`, `CanDoCheckpointActions`, `guardPoint`, `GetCheckpointCoverPoint`, `SetGuardPoint`, `guardPointDirty` e o GUID `com.untargh.tacticaltoaster` na primeira; `BotHuntManager` e `GetRegroupPoint` na segunda. Conferi a presença do texto, não o tipo nem a assinatura.

O dry-run não aproveita isso. A lista de pastas de `Program.cs:58` inclui `MoreBotsAPI`, mas nenhuma das duas DLLs é carregada, e `AccessTools.TypeByName` só procura em assembly carregado. `NativeGhostAdapters.ResolveBindings` (`modded/Orbit/Systems/NativeGhostAdapters.cs:79-106`) roda sem encontrar os tipos, e o estado de `_untar` não é impresso (PA-01-06).

Outra dependência desses mods fora da reflexão: `NativeGhostPolicy.IsBlackDivisionPatrol` compara o papel do bot com seis números fixos (`modded/Orbit/Systems/NativeGhostPolicy.cs:8`), que são valores de `WildSpawnType` registrados pelo mod de facção e podem ser outros na versão 4.0 dele.

**Por que importa:** corner case 3 da spec funcional. O UNTAR é usado no servidor do grupo; a integração pode ser conferida fora do jogo e a spec a deixa só para validação em jogo.

**Sugestão:** (1) corrigir §7: separar "instalado e verificável fora do jogo" (UNTAR, MoreBotsAPI) de "não instalado" (a memória lista RUAF, Black Division, ISB, Combine Soldiers, RoguesVRaiders em P-1.4); (2) no dry-run, carregar as DLLs dos mods opcionais presentes antes de `ResolveBindings` e imprimir, por mod, se a ligação resolveu; (3) registrar em §7 os seis números de `NativeGhostPolicy.cs:8` como dependência da versão do mod de facção.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** §7.6 separa instalado de não instalado. O dry-run carrega as DLLs dos mods opcionais presentes (`UNTARGHPlugin.dll`, `MoreBotsPlugin.dll`) e confere os membros: UNTAR 5, MoreBotsAPI 11.

### PA-01-11 · A — Gap · 🟡 Importante · ✅ Resolvido em 2026-09-30

**§9 check 11 está N/A com dois `INetSerializable` no addon**

**Problema:** o check 11 (pacote FIKA, AP-11) está N/A com a razão "os dois pacotes do addon são do upstream e não mudam de formato". O addon declara `OrbitDoorPacket` e `OrbitGhostFightPacket` (`modded/Orbit.Fika/OrbitDoorPacket.cs:8`, `OrbitGhostFightPacket.cs:22`). O gatilho de `docs/technical/fika-packet-desync-prevention-plan.md` é "mod declara `INetSerializable`", e a spec não cita o documento. O que a razão do N/A não cobre:

- **Colisão de hash com os pacotes desta instalação.** O autor não testou o addon ao lado dos mods deste repositório. Rodei `node scripts/check-packet-hashes.js --list`: os dois tipos entram (hashes 38924 e 43571) e o script reporta "Nenhuma colisão de hash CRC-16" entre 80 tipos. O script lê os pacotes do Fika da fonte 2.3.8.
- **Forma dos pacotes contra o guia.** Os dois leem com `Get*`, sem envelope de comprimento e sem `TryGet*` (`OrbitDoorPacket.cs:34-45`, `OrbitGhostFightPacket.cs:50-70`). `OrbitGhostFightPacket.Deserialize` decide se a lista de atiradores existe por `reader.AvailableBytes < sizeof(int)` (`OrbitGhostFightPacket.cs:59`), o que só funciona se o pacote for o último do leitor; o AP-11 descreve o Fika lendo vários pacotes do mesmo leitor num laço `while (AvailableBytes > 0)`. Com host e clientes na mesma versão o ramo não é usado.

Não li o guia inteiro; li o resumo do AP-11 em `spt-antipatterns.md:108-118`. Não conferi de qual thread o addon envia (`modded/Orbit.Fika/OrbitFikaPlugin.cs:98`).

**Por que importa:** o AP-11 registra que um pacote mal lido descarta todos os eventos de rede do frame, de todos os mods. O servidor do grupo roda outros mods com pacote próprio.

**Sugestão:** trocar o N/A por ✅ ou pendente com evidência: (1) o resultado de `check-packet-hashes.js` com os dois hashes; (2) a lista dos desvios dos dois pacotes em relação ao checklist §7 do guia, cada um aceito como herdado do upstream ou corrigido; (3) em §7, o risco de versões diferentes do addon entre host e cliente, por causa de `OrbitGhostFightPacket.cs:59`.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** §7.5 traz o resultado de `check-packet-hashes.js` e a tabela de desvios contra o checklist §7 do guia de pacotes. A captura de exceção do callback do pacote de combate fantasma foi corrigida no port; os quatro desvios de formato ficaram no item de backlog 002. §9 check 11 passou de N/A para "não conforme, herdado".

### PA-01-12 · A — Gap · 🟡 Importante · ✅ Resolvido em 2026-09-30

**§7 "build do jogo diferente": a mitigação citada cobre 4 de 80 alvos**

**Problema:** §7 cita como mitigação que "os transpilers do ORBIT falham de forma explícita quando o IL esperado não aparece". Os transpilers lançam exceção quando a chamada procurada não aparece exatamente uma vez (`modded/Orbit/Patches/LootPatrolResumePatch.cs:51-52`, `SanitarPatrolMedicinePatch.cs:60-61`, `NativeGhostBodyPatches.cs:118`). No dry-run são 4 chamadas com transpiler em 4 métodos, de 80 métodos distintos. Os outros 76 só recebem prefixo, postfix ou finalizer, que não olham o corpo do alvo.

Dois riscos da mesma origem não estão em §7:

1. **Prefixo que retorna `false` e refaz parte do corpo.** `OrbitMoverMotionPatch` grava `Bool_0 = false` e `CollisionFlags_0 = flags` e pula o original (`modded/Orbit/Patches/OrbitMoverMotionPatch.cs:23-25`). No 4.0 são as duas primeiras instruções de `GClass494.method_21` (`GClass494.cs:105-106`), então confere. A spec não lista quais prefixos refazem corpo nem diz que foram comparados.
2. **`Bind` encontra parte dos métodos.** `NativeGhostBodyPatches.Bind` procura por nome no tipo e nas subclasses e só falha quando não encontra nenhum (`NativeGhostBodyPatches.cs:71-74`). Uma sobrecarga ou override que no 4.1 tem um dos 27 nomes passados a `Bind` (`NativeGhostBodyPatches.cs:27-47`) e no 4.0 se chama `method_N` fica sem o prefixo, sem erro. O 4.0 liga 32 pontos de entrada (linha `body guards ready (32 entry points)` do dry-run). O número do 4.1 não está na spec.

**Por que importa:** o bloqueio que impede bot nativo dormindo de recarregar, curar ou abrir porta depende de `Bind` cobrir todos os caminhos. Um caminho de fora só aparece em raid, e só se o bot passar por ele.

**Sugestão:** (1) reescrever a mitigação: "4 dos 80 alvos têm transpiler que confere a forma da chamada; os demais não têm verificação de corpo"; (2) listar em §7 os prefixos que retornam `false` e refazem estado do alvo, com a linha do corpo 4.0 comparada; (3) registrar os 32 pontos de entrada do 4.0 (a lista sai do dry-run) e pedir ao upstream, ou tirar de um log de 4.1, o número que a mesma linha de log mostra lá; diferença entre os dois números vira item de investigação.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** §7.1 reescrito com os números: transpilers cobrem 4 dos 80 métodos com patch; `Bind` liga 32 pontos de entrada no 4.0, sem número do 4.1 para comparar.

### PA-01-13 · A — Gap · 🟡 Importante · ✅ Resolvido em 2026-09-30

**Cinco pares de método de §2 não têm prova do lado 4.1**

**Problema:** §2 apresenta `TryPassCurrentDoor` → `method_2`, `RunEnteringDoorSequence` → `method_7`, `WaitForDoorOpen` → `method_0`, `InteractionWithDoor` → `method_8` e `ApplyToSelf` → `method_3` com evidência `BotDoorOpener.cs:455`, `:626`, `:428`, `:737` e `BotFirstAidClass.cs:447`. Essas linhas mostram os métodos do 4.0. O par vem da leitura do corpo, porque o commit `00e7ad8` não cobre o Ghost Mode e a tabela oficial não cobre membros. A memória do mod registra isso como pendência (P-1.3); a spec apresenta os cinco pares no mesmo formato dos que têm fonte dos dois lados.

Um fato reduz o risco de três deles: `method_0`, `method_7` e `method_8` são os três métodos `void` sem parâmetro de nome ordinal de `BotDoorOpener` (`BotDoorOpener.cs:428`, `:626`, `:737`), entram na mesma chamada de `Bind` (`modded/Orbit/Patches/NativeGhostBodyPatches.cs:31-32`) e recebem o mesmo prefixo, que para `BotDoorOpener` só distingue `ManualUpdate` dos demais (`NativeGhostBodyPatches.cs:89-92`). Uma troca entre os três não muda o comportamento. O risco fica em `method_2`: `BotDoorOpener` tem outros quatro métodos de nome ordinal que retornam `bool` (`method_3`, `method_6`, `method_9`, `method_10`, em `BotDoorOpener.cs:485`, `:593`, `:782`, `:791`), e `Bind` aceita retorno `void` ou `bool` com qualquer lista de parâmetros.

**Por que importa:** o leitor da tabela de §2 não distingue par confirmado de par inferido.

**Sugestão:** marcar as duas linhas de §2 como "inferido pelo corpo do método 4.0; sem fonte do lado 4.1", acrescentar o argumento dos três métodos `void` e levar o caso de `method_2` e de `method_3` para §7 como risco, com o passo de validação em jogo (bot nativo dormindo diante de porta fechada; bot nativo ferido dormindo).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** §2 e §5.3 ganharam a coluna "Base do par" (autor / tabela oficial / inferido). §7.2 registra os cinco métodos como risco, com o sintoma de cada erro e o passo de validação em jogo.

### PA-01-14 · A — Gap · 🟡 Importante · ✅ Resolvido em 2026-09-30

**§9 checks 1 e 2 estão N/A com razão que o código não sustenta**

**Problema:**

- **Check 1 (ciclo de raid).** A razão é que `OrbitInitPatch` e `OrbitDisposePatch` "resolvem nos mesmos alvos (checkpoint 2)". O checkpoint 2 prova que o patch liga em `GameWorld.Dispose`; não prova que o jogo 4.0 chama `GameWorld.Dispose` em cada caminho de fim de raid. O único gancho de fim de raid do mod é esse (`modded/Orbit/Patches/Lifecycle.cs:103-108`); o AP-01 recomenda `GameWorld.OnDestroy` e `BaseLocalGame.Stop`. Conferi que os três overrides de `Dispose` chamam a base (tabela de PA-01-03).
- **Check 2 (filtro de jogador).** A razão é que os patches do upstream "filtram por bot (`BotRoster.IsOrbitActive`)". Cinco patches reagem a ação de jogador e não contêm `IsOrbitActive`, `IsYourPlayer` nem `IsAI`: `CorpseRegistrationPatch`, `InventoryChangePatch`, `DoorUnlockTracePatch`, `LootItemKilledPatch` e os quatro postfixes de `DoorSyncBridge` (`modded/Orbit.Fika/DoorSyncBridge.cs:63-67`). Conferi por busca de texto nos arquivos; não li o corpo de cada um.

**Por que importa:** o comando de review manda confrontar N/A nos checks 1, 2 e 5 com os alvos reais. O critério de aceite 8 (estado entre raids) depende do check 1.

**Sugestão:** check 1: trocar a razão por "herdado do upstream: único gancho é `GameWorld.Dispose`; overrides conferidos em `ClientGameWorld.cs:326`, `FikaHostGameWorld.cs:111`, `FikaClientGameWorld.cs:123`; os caminhos de morte e de fechar o jogo ficam para o critério 8". Check 2: listar os cinco patches com o alcance real de cada um (todo jogador, só host, só bot) e a linha que o decide.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** §9 check 1 cita o único gancho (`GameWorld.Dispose`) e os três overrides; check 2 lista os cinco patches que reagem a jogador com o alcance de cada um.

### PA-01-15 · C — Erro de lógica · 🟢 Menor · ✅ Resolvido em 2026-09-30

**Contagens da spec que não batem com o repositório**

**Problema:**

| Onde | A spec diz | O repositório tem |
|---|---|---|
| §4 | "46 apelidos" em `Spt40TypeAliases.cs` | 47 linhas `global using`. Corrigido para 47 no commit `9759d7d7` |
| §2 | "os demais 39 patches `ModulePatch`" | 43 classes `ModulePatch` no total (saída do dry-run). 43 − 39 = 4, e a tabela de §2 lista 11 classes com alvo renomeado: 6 com método renomeado e 5 com tipo renomeado |
| §4 | "30 arquivos em `modded/Orbit/`", "cerca de 115 linhas" | 31 arquivos, 129 linhas adicionadas e 123 removidas entre `b1bedb0b` e `a7854745`, fora `Compat/` e o `.csproj` |

**Por que importa:** as contagens são o que um port futuro usa para saber se reaplicou tudo.

**Sugestão:** corrigir os dois números que restam (39 e 30), ou trocar por "ver a saída de `git diff --stat b1bedb0b..HEAD -- mods/ORBIT-2.1/modded`".

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** §2 diz "43 classes `ModulePatch` ... 89 chamadas sobre 80 métodos"; §4 aponta para `git diff --shortstat` em vez de fixar a contagem.

### PA-01-16 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-09-30

**Nome 4.0 formado por tipo e ordinal: a spec cita a declaração, não a linha que prova o papel**

**Problema:** §1.2 (item 4) diz que o compilador e o dry-run são "o juiz" das fontes 1 a 3. Para membro cujo nome 4.0 é o tipo mais um número (`Bool_0`, `List_0`, `Boolean_0`, `Gclass25_0`, `ItemAddress_1`, `HashSet_0`, `CustomNavigationPoint_0`, `method_1`, `method_4`), o compilador aceita qualquer outro membro do mesmo tipo: `GClass3411` tem `ItemAddress_0` e `ItemAddress_1` (`GClass3411.cs:16`, `:20`), e trocar um pelo outro compila. O que distingue é a leitura do corpo. Em várias linhas de §5.3 a citação é a declaração do campo. Conferi o papel de cada um e todos estão certos (tabela de conferência).

**Por que importa:** a citação da declaração prova que o campo existe, não que é o campo certo.

**Sugestão:** em §1.2, trocar "juiz" por "confere que o nome existe e que o tipo é compatível; o papel do membro só se prova pela leitura do corpo". Em §5.3, trocar `GClass3411.cs:20` por `:89`, e acrescentar `GClass442.cs:87` e `GClass494.cs:24` às linhas respectivas.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** §1.2 item 4 deixou de chamar o compilador e o dry-run de "juiz": verificam existência e ligação, não o papel. §5.3 ganhou a coluna "Linha que mostra o papel" (`GClass3411.cs:89`, `GClass442.cs:87`, `GClass494.cs:24` e as demais).

### PA-01-17 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-09-30

**A evidência do Fika é do 2.3.8; o instalado é o 2.3.21**

**Problema:** §2 e §5.3 citam `references/fika-plugin/Fika.Core/Main/Players/FikaPlayer.cs:1361` e `:1350`. A fonte vendorizada é o Fika 2.3.8 (`references/fika-plugin/Fika.Core/FikaPlugin.cs:48`). §1.1 diz que o instalado é o Fika.Core 2.3.21, o que confere pela versão de arquivo. Os nomes `vmethod_0` e `vmethod_1` existem no 2.3.21: o addon compila contra ele e o dry-run liga os quatro postfixes. O corpo dos métodos no 2.3.21 não está em nenhuma fonte do repositório.

**Por que importa:** a auditoria de PA-01-03 e a leitura de `DoorSyncBridge` valem para o 2.3.8.

**Sugestão:** escrever em §1.1 que a fonte vendorizada do Fika é a 2.3.8 e que as citações de linha são dessa versão; ou atualizar `references/fika-plugin` para a versão instalada e reconferir as duas linhas.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** §1.1 registra que a fonte vendorizada do Fika é a 2.3.8 e o instalado é o 2.3.21, e como cada afirmação foi conferida.

### PA-01-18 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-09-30

**Dois grupos de nome em texto fora de §1.3 que hoje conferem**

**Problema:** dois grupos de texto que o mod compara com valor produzido pelo jogo não estão em §1.3 nem em script:

- **Nome de cérebro e de camada passado ao BigBrain.** `modded/Orbit/Plugin.cs:190-215` registra a camada do ORBIT em 12 nomes de cérebro e remove a camada `LootPatrol`. Os 12 nomes e `LootPatrol` existem como literal retornado no decompile 4.0 (por exemplo `GClass117.cs` para `LootPatrol`, `GClass311.cs` e `GClass349.cs` para `PMC`).
- **Nome de membro de `BotLogicDecision` comparado como texto.** `modded/Orbit/Systems/NativeGhostPolicy.cs:41-47` compara o nome da decisão com 12 literais. Os 12 são membros de `BotLogicDecision.cs` no 4.0. Só conferi esse arquivo do mod.

**Por que importa:** a build do jogo é outra; esses nomes poderiam não existir nela, e a falha seria silenciosa.

**Sugestão:** adicionar os dois grupos a §1.3 como "conferido, sem mudança", com a busca usada, para que um port futuro repita a conferência.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** §1.3 lista os grupos conferidos e sem mudança: nomes de cérebro, camada `LootPatrol`, nomes de `BotLogicDecision`, limite 9000, `"SAIN : Combat Layer"`.

### PA-01-19 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-09-30

**O cartão `HomePage` do 4.1 não existe no 4.0 e não virou pendência**

**Problema:** o port remove de `ModMetadata.cs` as propriedades `HomePage = "/orbit"`, `HomePageDescription`, `WWWRootUrl` e `HasPrepatcher`, que o contrato do 4.0 não tem. §1.1 registra a diferença ("marcador vazio"). No 4.1 essas propriedades colocam o link do painel na tela de mods do servidor; no 4.0 o painel só abre por quem conhece o endereço `/orbit`.

**Por que importa:** a spec funcional pede que o que não puder ser portado fique registrado como pendência com o motivo.

**Sugestão:** registrar em §7, ou numa seção de pendências, "link do painel na tela de mods do servidor: não portável, o 4.0 não tem o cartão `HomePage`".

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** §5.6 e §7.7 registram o cartão `HomePage` como funcionalidade do 4.1 sem equivalente no 4.0.

### PA-01-20 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-09-30

**A build do port tem a mesma versão do upstream**

**Problema:** o port mantém `OrbitVersion = "2.1.0"` (`modded/Orbit/Plugin.cs:42`) e a versão 1.1.0 do addon (`modded/Orbit.Fika/OrbitFikaPlugin.cs:32`). `builds/` tem cinco builds diferentes desta noite com a mesma versão. §7 exige que host, headless e clientes tenham "o mesmo `ORBIT.dll` e o mesmo `Orbit.Fika.dll`", e o addon só confere `OrbitDoorPacket.CurrentProtocol` (`modded/Orbit.Fika/OrbitDoorPacket.cs:10`).

**Por que importa:** pela versão não dá para distinguir o port do ORBIT 2.1.0 do upstream, nem uma build do port de outra.

**Sugestão:** decidir em §7 como a build do port se identifica (por exemplo um sufixo na versão informativa do assembly e na linha de log de carga) e registrar a decisão. O critério de aceite 4 espera a linha "ORBIT 2.1.0 fully loaded"; se a linha mudar, ajustar o critério.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________
<!-- Após resolver: marcar a opção escolhida, trocar título para ✅ Resolvido em YYYY-MM-DD e adicionar **Resolução:** ... -->

---

## O que não foi verificado

- Nada foi executado em jogo, em raid nem em coop. Não rodei o servidor.
- Não rodei `verify-port.sh` nem `/compile-mod`. Rodei uma cópia do `PatchDryRun.exe` já compilado, `check-type-name-literals.py` e `check-packet-hashes.js`.
- Não há assembly nem fonte do 4.1 no repositório: nenhuma afirmação sobre o lado 4.1 foi conferida além do diff do commit `00e7ad8` e da tabela de nomes de tipo.
- De §1.1 e §1.2: a build do EFT do 4.1 (0.16.9.5.40743), "SAIN 4.5+", "124 commits" e o texto da mensagem do commit `00e7ad8` (o arquivo salvo só tem o diff).
- A fonte do Fika lida é a 2.3.8; o instalado é o 2.3.21.
- Nas DLLs de UNTAR, MoreBotsAPI, SAIN e Fika só conferi a presença do nome como texto, não tipo nem assinatura.
- Não li `docs/technical/fika-packet-desync-prevention-plan.md` inteiro, nem as skills `spt-mod-best-practices` e `csharp-mod-best-practices`.
- Não li `05-asbuild.md`, `04-code-review-01.md` nem `docs/technical/spt41-to-spt4-mod-downgrade.md`, que entraram no commit `9759d7d7` durante esta review. Pontos desta review podem repetir achados do `04-code-review-01.md`.
- O código mudou duas vezes durante a review (`03c9290e`, `9759d7d7`). As linhas citadas de `modded/` e `scripts/` foram conferidas em `03c9290e`; o `9759d7d7` não alterou esses arquivos, exceto por `scripts/install-to-spt.sh`, que não li.
- Em §5.2, conferi as dez linhas contra a tabela de nomes, não contra a declaração de cada tipo no decompile.
