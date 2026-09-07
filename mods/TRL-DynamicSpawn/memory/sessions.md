# Memory — TRL-DynamicSpawn

Memória cronológica de sessões de trabalho (timestamps em GMT-3). Cada entrada resume as alterações efetuadas, decisões de arquitetura e estado atual. Atualizada ao fim de cada sessão de trabalho.

> **Por que existe:** o usuário trabalha múltiplos chats/sessões em paralelo. Este arquivo preserva o contexto técnico, decisões de design e roadmap do mod `TRL-DynamicSpawn`, evitando que futuras sessões com assistentes AI reabram discussões do zero.

---

## Estado Atual (Snapshot ao Fim da Sessão — 2026-09-04)

**Mod C# Client (v3.7.6) + C# Server (v3.7.6) compilados com sucesso (0 erros).**

- **Identity**: `TRL-DynamicSpawn` (Client BepInEx DLL: `TRL-DynamicSpawn.dll`, Server C# DLL: `TRL-DynamicSpawn-Server.dll` com Web UI). Compatível com SPT 4.0.13 e EFT 0.16.9 / FIKA.
- **Correção da Interatividade de Loot na Conversão em Mochila (`v3.7.6`)**:
  - **Injeção de BoxCollider Sólido**: Adicionado `BoxCollider` (`0.5m x 0.5m x 0.45m`) ao `backpackVisual` da Flyye MBSS instanciada em tempo de execução, permitindo que a mira do jogador colida com o objeto.
  - **Layer Deadbody Recurssiva**: Configurada a camada do GameObject da mochila para `LayerMaskClass.DeadbodyLayer`, tornando-a detectável pelo `GameWorld.FindInteractable(ray, int_0)`.
  - **Detecção do Corpse**: Como `backpackVisual` é filho de `corpsePlayer.gameObject`, o EFT sobe a hierarquia via `hit.collider.GetComponentInParent<InteractableObject>()` e encontra o componente nativo `Corpse`, gerando imediatamente a ação `"Search"` / `"Lootear"`.
  - **Snap ao Solo e Física Não-Bloqueante**: Adicionado raycast vertical para assentar a mochila perfeitamente sobre o terreno/chão e removido `rb.detectCollisions = false`, mantendo `rb.isKinematic = true` (0% de custo contínuo de CPU da física e 0 draw calls do bot morto).
- **Evacuação Orgânica de Cultistas ao Amanhecer (`v3.7.5`)**:
  - **Mecanismo Orgânico (`CultistDawnEvacuationWatcher`)**: Coroutine leve que roda exclusivamente caso a raid inicie à noite (22:00 às 05:59). Em raids diurnas, finaliza imediatamente (`yield break`) com zero impacto de CPU.
  - Em raids noturnas, um heartbeat de 30s monitora a transição para 06:00. Ao amanhecer, comanda todos os cultistas vivos (`sectantPriest`, `sectantWarrior`, `sectantOni`, `sectantPrizrak`, `sectantPredvestnik`) via `bot.LeaveData.DoLeaveExternal()`.
  - **Zero Despawn Forçado e Preservação de Combate**: Se o cultista estiver ou entrar em combate (`Class103` / `LeaveMapLayer`), a inteligência nativa do EFT prioriza o tiroteio/autodefesa; assim que o perigo cessa, a flag `WannaLeave` acumulada faz com que ele retome o caminho até a saída do mapa.
  - **Bloqueio de Ondas Diurnas Tardias**: `DisableVanillaBossWavesPatch` bloqueia ondas nativas de cultistas caso caiam após as 06:00.
- **Novo Padrão Default de Configurações (`v3.7.5`)**:
  - Atualizado `config.json` e `config.default.json` com o novo template (`maxGroupSizeByMap` = 3 para assault e pmcbot, novos parâmetros de distância e bolhas para todos os mapas).
- **Correção de Fusão e Chance Per-Zone de Snipers (`v3.7.4`)**:
  - `_maxPersons = 1` forçado para todas as `BotZone` com `SnipeZone == true` no `SpawnPointManagerClass.smethod_1` e no início da raid. Elimina o comportamento do EFT de duplicar snipers no mesmo `ISpawnPoint` (`DuplicateIfAtLeastOne`), impedindo bots fundidos no mesmo modelo.
  - Sorteio individual de probabilidade por zona de sniper (`InitializeSniperZonesForRaid`) baseado no `SniperChance` configurado no Painel Web.
  - Zonas que não passam no teste de chance são bloqueadas no `ZonesLeaveController.BlockZoneFor` e filtradas em `DisableVanillaWavesPatch` e `SpawnGatePatches`, garantindo que torres/telhados fiquem vazios quando a chance falhar.
  - Remoção da injeção cega de marksman na onda 1 do `DynamicSpawnManager`.
- **Unificação de Spawn de Bosses e Guardas / Nomes Nativos EFT (`v3.7.4`)**:
  - Correção da separação entre Reshala (ScavBase) e seus guardas (Dorms).
  - Alinhamento dos nomes internos nativos do EFT (`bossBully`, `bossBoar`, `bossKojaniy`) com os nomes do painel (`bossreshala`, `bosskaban`, `bossshturman`) via mapeamento bidirecional em `AdjustVanillaBossWaves`, `HasNativeVanillaWave` e `eliteEntries`.
  - Garantido que ondas nativas de bosses com escolta mantenham Boss e guardas na mesma zona configurada, sem spawn paralelo de boss solo.
- **Restrição Estrita de Zonas para Bosses/Goons (`v3.7.3`)**:
  - `GetZoneFromConfig`: Se houver zonas configuradas no painel web, o sorteio é restrito exclusivamente a elas. Se nenhuma estiver disponível, o spawn é cancelado e não cai em zonas proibidas (como BigRed). O fallback para qualquer zona só ocorre se o campo de zonas estiver 100% vazio.
  - `AdjustVanillaBossWaves`: Re-inicializa `PossibleShuffledZones` e `BornZone` do `BossLocationSpawn` nativo da EFT, impedindo o motor vanilla de vazar Goons para a BigRed.
  - `SpawnGroup`: Bloqueado o fallback para zonas de Scav/PMC para grupos de Elites/Bosses com zonas configuradas.
- **Snap de Terreno e NavMesh contra Soterramento (`v3.7.3`)**:
  - `OnBotCreatedSafetySnap`: Registrado no evento `OnBotCreated` do `BotSpawner`, aplicando `NavMesh.SamplePosition` e `Physics.Raycast` vertical para cima da superfície física. Se o bot nascer soterrado no asfalto ou dentro de veículos/caixas, é reposicionado com segurança na superfície transitável (snipers em poleiros são preservados).
  - `AttemptToTeleportGroup`: Snap vertical similar aplicado no teleporte de seguidores para declives e rampas.
- **Estruturação do Workspace (Dual original/modded)**:
  - `original/`: Backup intacto da versão original canônica.
  - `modded/`: Código-fonte com as refatorações de alta performance e física aplicada.
- **Documentação de Engenharia e Ciclo de Vida**:
  - `docs/ciclo-de-vida-e-arquitetura-bot-spawning.md`: Especificação técnica de ponta a ponta cobrindo as 7 fases de ciclo de vida de bots, topologia FIKA, SAIN e SPT-Waypoints.
  - `docs/relatorio-auditoria-codigo-02.md`: Diagnóstico arquitetural completo de gargalos de CPU/GC e plano de refatoração.
  - `docs/relatorio-auditoria-codigo-01.md`: Relatório de auditoria das rodadas 1–2 de performance (v3.3.0/v3.4.0) com métricas in-raid.
- **Refatorações de Alta Performance Aplicadas (v3.4.1)**:
  - **ZoneCache ([AUD-02-03])**: Eliminação de travamentos de cena de 5ms–15ms por meio do cache estático de `BotZone`.
  - **Geração Atômica de Esquadrões ([AUD-02-01])**: Spawns de grupo criados em 1 única Task (`BotCreationDataClass.Create` com `groupSize`), preservando a coesão no `BotsGroup`.
  - **Erradicação de GC Spikes por LINQ ([AUD-02-04])**: Substituição de `.Where().ToList()` e `.OrderBy()` por loops indexados `for` em passagem única (0 bytes de GC lixo).
  - **Otimização com `sqrMagnitude` ([AUD-02-05])**: Substituição de `Vector3.Distance` por magnitude quadrada, cortando instruções de raiz quadrada (`Mathf.Sqrt`).
  - **Sequência Atômica de Física no Teleporte ([AUD-02-06] & [AUD-02-07])**: Parada de NavMesh e inércia antes do teleporte físico, com reset cirúrgico de combate e suporte defensivo para SAIN (`ClearEnemy`).
  - **Zero Memory Leaks entre Raids ([AUD-02-08])**: Limpeza completa de contêineres estáticos em `RaidLifecycle.OnRaidEnd`.
- **Fatos-chave consolidados da Rodada 2 (v3.4.0)**:
  - Config do painel buscada **1×/raid**; Spawner contínuo vanilla de Scavs recusado antes de criar perfil (0 barradas caras).
  - `AddToTargetBackup` registra **nível permanente por (papel, dificuldade)** reposto pelo SPT a cada ~30 s (`GClass684.cs:258-263`).
  - O SAIN **respeita** a etiqueta de dificuldade (seções easy/normal/hard/impossible por bot); ProgressiveBotSystem 2.2.1 seleciona equipamento por **Tier + papel**.

---

## Pendências / Próximos Passos Conhecidos (Roadmap)

> **Handoff (2026-08-24): os próximos itens são do Umbigo.** Porta de entrada: [relatorio-auditoria-codigo-01.md](../docs/relatorio-auditoria-codigo-01.md) e [relatorio-auditoria-codigo-02.md](../docs/relatorio-auditoria-codigo-02.md).

- 🟡 [P-ROADMAP-01] **Testes em Raid / Validação de Frametime**: Validar a estabilidade do frametime e a ausência de stutters em mapas densos (Streets of Tarkov, Lighthouse).
- 🟡 [P-6.1] **Resíduo da validação V2** (aberta 2026-08-24): medir curva de RAM numa raid longa; 1 raid host com `Enable Debug Logs = false` (AC-M5); conferir a fonte do hook de fim numa raid encerrada por **extração normal** (nas raids V2 o jogo foi fechado no alt+F4 → só `GameWorld.OnDestroy` rodou; esperado com Fika: `CoopGame.Stop`).
- 🟡 [P-6.2] **Rodada 3 (decisões de design — Umbigo)** (aberta 2026-08-24): [AUD-01-09](../docs/relatorio-auditoria-codigo-01.md) warmup que não converge (ondas de 1–3 vagas a cada 30 s para sempre; propor tolerância de teto); pool inicial vanilla ~452 perfis pré-gerados para waves que o mod bloqueia; item [011-perf-estoque-dificuldade](../backlog/011-perf-estoque-dificuldade/).
- 🟢 [P-6.3] **Log flood de terceiros observado na V2** (aberta 2026-08-24): ORBIT `value-skip` 1 linha/frame (1.134 em ~3k linhas) com bot preso em cadáver; NREs `EFT.Player.get_PointOfView` ao espectar no Fika. Nenhum é deste mod — reportar aos donos/rodada de perf deles.
- 🟡 [P-ROADMAP-02] **Retorno do Viés Direcional (Pós-Debug)**: Retornar a proporção do viés direcional de spawn/teleport para 70% frontal / 30% traseiro pós-testes.
- 🟡 [P-ROADMAP-04] **Standalone Mod — Limpador de Corpos Inteligente (Corpse Cleaner)**: Novo mod separado focado em performance (timer individual por corpo).

---

## Histórico de Sessões

### 2026-09-03 — Correção de Fusão de Snipers, Chance Per-Zone e Unificação de Bosses/Guardas (v3.7.4)

- **Correção de Fusão de Snipers e Chance Per-Zone (`v3.7.4`)**:
  - Resolvido o problema de excesso de snipers (6 snipers em raid na Customs) e fusão física (dois snipers gerados exatamente no mesmo `ISpawnPoint` sobrepostos).
  - Em `SpawnPointManagerClass.smethod_1` (`Patches.cs`), forçado `_maxPersons = 1` para qualquer `BotZone` marcada como `SnipeZone == true`. Isso elimina o caminho do EFT (`DuplicateIfAtLeastOne`) que replicava o único ponto de spawn da torre para acomodar `_maxPersons > 1`.
  - Criado `InitializeSniperZonesForRaid`: executa sorteio independente por cada zona de sniper do mapa de acordo com o `SniperChance` configurado no Painel Web (`Random.Range(1, 101) <= mapSniperChance`).
  - Zonas reprovadas no teste de probabilidade são bloqueadas nativamente via `ZonesLeaveController.BlockZoneFor(sz, WildSpawnType.marksman)` e registradas no conjunto estático `BlockedSniperZones`.
  - `DisableVanillaWavesPatch` agora intercepta ondas de `WildSpawnType.marksman`: cancela a onda se a zona estiver bloqueada pela probabilidade ou se já contiver um bot vivo.
  - Em `SpawnGatePatches` (`ActivateBotsWithoutWavePatch`), adicionado bloqueio a marksman quando nenhuma zona de sniper for autorizada (`AllowedSniperZones.Count == 0`).
  - Removida a injeção forçada de sniper na onda 1 do `DynamicSpawnManager.cs`.
- **Unificação de Spawn de Bosses e Guardas / Mapeamento de Nomes Nativos EFT (`v3.7.4`)**:
  - Resolvido o descompasso onde Reshala spawnava na ScavBase (Fortress) e seus guardas Zavodskoy spawnavam em Dorms.
  - Identificada divergência nos identificadores de bosses entre o painel e o core do EFT: internamente o EFT registra Reshala como `bossBully`, Kaban como `bossBoar` e Shturman como `bossKojaniy`.
  - Implementado mapeamento bidirecional de aliases em `AdjustVanillaBossWaves` e `HasNativeVanillaWave`: agora o mod reconhece a onda original do Boss com seus guardas e redireciona todo o esquadrão de forma atômica para a zona configurada no painel.
  - O mod não cria mais uma entidade dinâmica solo do Boss enquanto os guardas nascem em outra zona pela onda nativa.
  - Alinhadas as chaves em `eliteEntries` no `DynamicSpawnManager.cs` com os nomes canônicos do EFT (`bossbully`, `bossboar`, `bosskojaniy`).
- **Aplicação do Novo Padrão de Configuração (`config.default.json` e `config.json`)**:
  - Importado o novo conjunto canônico de configurações de `Novo padrão para usar no default/config.json` para `modded/Server/config/config.default.json` (usado pelo botão "PADRÃO" / `/resetConfig`) e `modded/Server/config/config.json` (configuração ativa do servidor).
  - Inclui novos presets, balanceamento de elites (`exUsec`, `sectantPriest`, limites de rogues em Lighthouse), limites de grupo por mapa (`maxGroupSizeByMap`) e distribuição de facções.
- **Code Review & Refinamento de Performance (`v3.7.4`)**:
  - Reutilização do `ZoneCache` em `InitializeSniperZonesForRaid`, `AdjustVanillaBossWaves` e `DisableVanillaWavesPatch`, eliminando varreduras de hierarquia do Unity (`LocationScene.GetAllObjects`).
  - Cache estático de `FieldInfo` para `_maxPersons` (`_maxPersonsField`) em conformidade com `csharp-mod-best-practices` §3.
  - Guarda antecipada para ondas marksman com `SpawnAreaName` vazio quando `AllowedSniperZones` estiver zerado.
  - Proteção defensiva em `IsSniperZoneAllowed` quando `AllowedSniperZones.Count == 0`.
- **Validação de Build**:
  - `TRL-DynamicSpawn-Client.csproj` e `TRL-DynamicSpawn-Server.csproj` compilados com **0 Erros**, **0 Avisos**.

### 2026-09-03 — Restrição Estrita de Zonas dos Goons e Snap NavMesh contra Soterramento (v3.7.3)

- **Restrição Estrita de Zonas para Bosses/Goons (`v3.7.3`)**:
  - Eliminado o vazamento de Goons para a BigRed (`ZoneCrossRoad`) na Customs quando apenas zonas específicas (ex: `ZoneOldAZS` e `ZoneScavBase`) foram selecionadas no Painel Web.
  - No `GetZoneFromConfig`, o mod agora restringe exclusivamente às zonas marcadas no painel. Se nenhuma estiver disponível na hora da onda, o spawn é abortado com aviso em vez de cair em zonas arbitrárias. O fallback para qualquer zona do mapa só é permitido se a configuração no painel estiver 100% vazia.
  - No `AdjustVanillaBossWaves`, foi forçada a re-inicialização de `PossibleShuffledZones` e `BornZone` do `BossLocationSpawn` nativo da EFT, impedindo a engine original de sortear a BigRed.
  - Em `SpawnGroup`, adicionada trava impedindo que elites com zonas configuradas vazem para o algoritmo dinâmico de Scavs/PMCs.
- **Snap Físico de Solo e NavMesh contra Soterramento (`v3.7.3`)**:
  - Implementado `OnBotCreatedSafetySnap` no evento `BotSpawner.OnBotCreated`: ao nascer qualquer bot, o mod verifica `NavMesh.SamplePosition` e executa um `Physics.Raycast` vertical para cima da superfície física. Se o bot estiver enterrado no asfalto (diferença vertical `> 0.15m`) ou sob veículos/caixotes, é reposicionado de forma limpa na superfície transitável.
  - Snipers em torres/chaminés são isentos de snap agressivo (`SpawnPointHelper.IsSniperRole`) para preservar poleiros estreitos.
  - Snap de solo similar adicionado em `AttemptToTeleportGroup` no `BotDespawnManager.cs`.
- **Validação de Build**:
  - `TRL-DynamicSpawn-Client.csproj` e `TRL-DynamicSpawn-Server.csproj` compilados com **0 Erros**.

### 2026-09-02 — Developer HUD no Menu F12 e Desativação por Padrão (v3.7.2)

- **Controle de Exibição do Developer HUD (`v3.7.2`)**:
  - Removido o atalho de teclado `F12` em `Update()` que entrava em conflito com o menu de configuração do BepInEx.
  - Criada a configuração `Enable Developer HUD` em `Settings.cs` (na seção `Debug Logs & Developer HUD`), **desabilitada por padrão (`false`)**.
  - O método `OnGUI()` em `DynamicSpawnManager.cs` agora depende exclusivamente de `Settings.enableDebugHUD.Value`.
- **Validação de Build**:
  - `TRL-DynamicSpawn-Client.csproj` e `TRL-DynamicSpawn-Server.csproj` compilados com **0 Erros**.

### 2026-08-31 — Garantia de Spawn Agrupado Ombro a Ombro (v3.7.1) e Eliminação de Log Spam (v3.7.0)

- **Garantia de Spawn Agrupado Ombro a Ombro (`v3.7.1`)**:
  - Implementada a inicialização de `spawnParams.ShallBeGroup = new ShallBeGroupParams(true, true, groupSize)` quando `groupSize > 1` no `DynamicSpawnManager.cs` (`SpawnGroupBotsCoroutine`).
  - Ativa o algoritmo nativo da EFT (`SpawnSystem.SelectAISpawnPoints`) para alocar pontos de nascimento adjacentes (3m a 8m de raio) para os membros do esquadrão no momento do spawn da onda.
- **Eliminação de Log Spam e Stutterings (`v3.7.0`)**:
  - `ChooseProfilePatch` e `BotSpawnLoggerPatch` silenciados sob o gate `Settings.enableDebugLogs.Value` e migrados para `LogInfo`.
  - Eliminado o gargalo de I/O síncrono que ocorria durante o pre-loading de perfis em segundo plano pelo gerador do SPT.
- **Code Review e Validação**:
  - Relatório de Code Review executado com 0 bloqueadores.
  - `TRL-DynamicSpawn-Client.csproj` e `TRL-DynamicSpawn-Server.csproj` compilados com **0 Erros**.

### 2026-08-25 — Auditoria Arquitetural, Refatoração de Alta Performance e Estruturação original/modded (v3.4.1)

- **Documentação do Ciclo de Vida e Auditoria Profunda**:
  - Criado `docs/ciclo-de-vida-e-arquitetura-bot-spawning.md` detalhando os contratos do SPT Server, Assembly EFT, FIKA coop, SAIN e pooling.
  - Elaborado `docs/relatorio-auditoria-codigo-02.md` com o diagnóstico dos 4 eixos de auditoria e snippets C#.
  - Adicionada a **Seção 8 (Catálogo Canônico de BotZones por Mapa)** no documento técnico com todos os nomes oficiais do EFT 0.16.9 / SPT 4.0.
- **Sincronização de Zonas no Web Server (`Index.razor`)**:
  - Atualizado o dicionário `BOT_ZONES_MAP` no Web UI com 100% das zonas oficiais de todos os mapas (incluindo `ZoneScavBase2` em Woods, `ZoneMeteoStation` em Shoreline, `ZoneCarShowroom` em Streets, telhados de Rogues em Lighthouse e zonas de Snipers).
- **Estruturação Dual original/modded**:
  - Reorganizado o repositório em `original/Client`, `original/Server`, `modded/Client`, `modded/Server`.
- **Implementação do ZoneCache & Otimizações de CPU**:
  - Criado `ZoneCache.cs` para acesso em $O(1)$ a zonas de bot.
  - Refatorados `DynamicSpawnManager.cs`, `BotDespawnManager.cs` e `Patches.cs` para eliminar LINQ e usar `sqrMagnitude`.
  - Refatorado `SpawnGroupBotsCoroutine` para geração atômica de esquadrões.
  - Corrigida a ordem de física do teleporte e adicionado reset defensivo para o mod SAIN.
- **Validação de Compilação & SemVer (v3.4.1)**:
  - `TRL-DynamicSpawn-Client.csproj` e `TRL-DynamicSpawn-Server.csproj` compilados com **0 Erros**.

---

## 2026-08-24 22:53 (GMT-3) — Sessão 6: Rodadas 1–2 de performance (v3.3.0/v3.4.0), validação V2 e handoff da rodada 3

> Sessão contínua de 22→24/08 (worktree `perf-dynspawn-config-cache`, mergeado via PR #7). Registrada como uma entrada única no fechamento.

**Tema central:** eliminar o custo de fundo do mod (stutter de 10 s, churn de perfis, 44 NREs) via processo `/optimize-mod-performance` com specs de não-regressão e reviews independentes; fechar com medição in-game.

**Decisões-chave:**
- **Rodada 1 (item 009, v3.3.0):** config do painel com cache por raid + backoff 30 s + poller de despawn só em raid — porque 111 HTTP síncronos/raid paravam a main thread. Ref: `Client/Helpers/ServerConfigProvider.cs`, `RaidLifecycle.cs`, relatório AUD-01-01/02/03.
- **Rodada 2 (item 010, v3.4.0):** recusar o spawner contínuo vanilla em `BotsController.ActivateBotsWithoutWave` (antes de criar perfil), `ChooseProfile` tolerante para todos os papéis, `ClearSptQueue` 1×/raid, `StopSpawnLoops` nos hooks de fim, logs sob gate — porque a V1 provou que o metrônomo de 10 s era o `NonWavesSpawnScenario` (163 tentativas barradas tarde), não o getConfig. Ref: `Client/Patches/SpawnGatePatches.cs`, AUD-01-04..08.
- **Stop hook concreto:** patch em `LocalGame.Stop` + `CoopGame.Stop` (soft por nome) no lugar do genérico `BaseLocalGame<>` — porque com Fika o `LocalGame` nunca é instanciado e `CoopGame.Stop` não chama a base. Ref: PA-01-03/PA-02-03 do 010.
- **Semântica do estoque corrigida (CR-01-01 do 010):** `AddToTargetBackup` = nível permanente reposto pelo SPT; F12 `Initial Profile Preload` documentado com a semântica real; pré-carga de Scav removida (era no-op — vanilla registra 8/dificuldade).
- **Decisão do usuário:** logs de debug ficaram ligados até a V1; mudança de edição ao vivo do painel (5 s → por raid + toggle F12) aceita como trade-off declarado (AC-X1..X6 nas specs).

**Lições / hipóteses descartadas:**
- **"O metrônomo de 10 s é o poll de getConfig" — parcialmente falsa:** V1 mediu getConfig 111→1 e o stutter persistiu; a causa era o `NonWavesSpawnScenario.Update` (piso de 10 s, `NonWavesSpawnScenario.cs:32-34`) criando perfis que o mod barrava tarde. Lição: correlação de cadência não é causa — medir por mecanismo.
- **"AddToTargetBackup pede N perfis" — falsa:** registra nível permanente, chave só se ausente (`GClass684.cs:258-263`); a "pré-carga" de Scav (20) nunca teve efeito. Descoberto por code review independente contra o dump.
- **"SAIN ignora a dificuldade" (premissa do item 004) — falsa:** SAIN aplica seções easy/normal/hard/impossible por bot (48 parâmetros diferem no BEAR, conferido em `D:\SPT\BepInEx\plugins\SAIN\Default Bot Config Values\`). Hoje o mod anula os pesos do painel com SAIN ativo — corrigir no item 011.
- **"Patch em `BaseLocalGame<EftGamePlayerOwner>.Stop` cobre o Fika" — falsa:** inerte na V1; `CoopGame.Stop` não chama a base (`CoopGame.cs:811-818`) e `LocalGame` nem existe com Fika (`TarkovApplication_LocalGameCreator_Patch.cs:192`).
- **"Dificuldade influencia equipamento" — falsa neste setup:** ProgressiveBotSystem 2.2.1 usa Tier (nível do jogador) + papel; "difficulty" só em logging (3 ocorrências).
- **"Onda 2 infinita" (relato do usuário na V2) — não é regressão:** é o ESTÁGIO A pré-existente que só entra no cooldown longo com o mapa 100% cheio; com mortes contínuas fica em ondas de 1–3 vagas a cada 30 s (Attempt 43 medido) → AUD-01-09, rodada 3.

**Atividade cronológica (resumo):**
1. Fase 2–4 do item 009: spec não-regressão + review (5 PA) + código + review de código (4 CR) + build 3.3.0 + deploy — V1: getConfig 111→1 ✓, stutter persistiu ✗ → AUD-01-08.
2. Decisão do usuário: atacar todos os pendentes → item 010 (AUD-01-04..08 + PA-01-05), reviews independentes por agente (12 PA em 2 rodadas + 5 CR), build 3.4.0 + deploy.
3. Push + PR #7 + merge na `main` (`e83d05e1`); worktree mantido até fechar a V2.
4. V2 (2 raids do usuário, 24/08): raid A host (0 barradas caras, 1 getConfig, 1 limpeza de fila, 0 NRE TrySpawnFreeInner, pool 452→504, 0 linhas pós-fim, FPS estável ✓); raid B guest (mod inerte ✓). Achados novos: AUD-01-09 (warmup), pool inicial vanilla ~452, floods de terceiros (ORBIT/Fika).
5. ProgressiveBotSystem 2.2.1 vendorizado + auditoria equipamento×dificuldade + grafo; item 011 especificado.

**Pendências abertas nesta sessão:** [P-6.1] resíduo V2 🟡 · [P-6.2] rodada 3 (AUD-01-09 + pool vanilla + 011, com Umbigo) 🟡 · [P-6.3] floods de terceiros 🟢 — detalhes no topo.

**Cross-refs:**
- Artefatos canônicos: [relatorio-auditoria-codigo-01.md](../docs/relatorio-auditoria-codigo-01.md) (achados, decisões, V1/V2 com números) · backlog [009](../backlog/009-perf-config-cache-raid/), [010](../backlog/010-perf-spawn-pipeline-r2/), [011](../backlog/011-perf-estoque-dificuldade/) · `PROPRIEDADES.md` (F12 novos).
- Item 004 (SAIN): premissa refutada nesta sessão — a reversão está desenhada no 011, não aplicada.
- Build sem `/compile-mod`: o script exige `modded/`; este mod compila com `dotnet build` direto (refs temporárias fora do repo — ver asbuilds dos itens 009/010). Dívida de harness.

---

### 2026-08-16 — Correção de Vazamento de Rogues/Raiders (v3.2.9)

- **Correção de Vazamento de Rogues e Raiders a 0% (`v3.2.9`)**:
  - Trava estrita adicionada em `DynamicSpawnManager.cs` exigindo `GetBossChanceForMap > 0`, `Enable == true` e `!DisableBosses` para grupos aleatórios.
- **Validação de Build**:
  - `TRL-DynamicSpawn-Client.csproj` e `TRL-DynamicSpawn-Server.csproj` compilados com **0 Erros**.

---

### 2026-08-06 — Invasão Dinâmica de Elites/Rogues (v3.2.3), MaxBot Dinâmico (v3.2.0) e Code Review 008

- **Correção da Causa Raiz do Spawning de Rogues/Elites Não-Nativos (`DynamicSpawnManager.cs`)**:
  - `SpawnHordeLoop` invocava `ProcessWave(false)` de forma hardcoded. Ajustado para `ProcessWave(warmupAttempt == 1)`, ativando `isFirstWave = true` no 1º ciclo de Warmup da raid.
- **Integridade e Spawn Conjunto de Esquadrões de Rogues**:
  - Eliminado o fracionamento de grupos em instâncias de 1 bot. O grupo é enfileirado como uma única unidade (`GroupSize = MaxGroupSize`).
  - Removido `exUsec` da sub-lista de PMCs comuns no algoritmo de interleaving, alocando Rogues no topo da lista (`elites`) para nascerem juntos no segundo 0 da onda na mesma zona.
- **Validações e QA (Code Review 008)**:
  - Confirmado que a bolha de distância (`enableSpawnBubble`) **não afeta Rogues/Elites** (já isentos em `IsValidSpawnZone`).
  - Aplicada comparação insensível a caixa (`StringComparison.OrdinalIgnoreCase`) em `GetZoneFromConfig`.
  - Adicionado pré-carregamento síncrono de Rogues (`exUsec`) no `AddToTargetBackup` do SPT.
- **Validação de Build & SemVer (`v3.2.3`)**:
  - BepInPlugin e Server csproj atualizados para `3.2.3`. Compilados com **0 Erros**.

---

### 2026-08-05 — Suporte a Copiar Mapa, Referência Imutável config.default.json, Modal do Default e Remoção do BotMountPatch

- **Remoção do `BotMountWeaponFixPatch` de TRL-DynamicSpawn**:
  - Migrado e centralizado no mod `TRL-Fixes` (`BotMountWeaponFixPatch.cs`). Removidas as referências em `TRL-DynamicSpawn/Client/Patches/Patches.cs`.
- **Referência Canônica para Restauração de Padrões (`config.default.json`)**:
  - Criado o arquivo `Server/config/config.default.json` com as 894 linhas completas das configurações originais do autor.
  - Atualizado o método `TRLConfigManager.ResetConfig()` para ler de `config.default.json` ao processar a rota `/trldynamicspawn/resetConfig`.
- **Melhorias e UX do Painel Web (`Index.razor`)**:
  - **Copiar Configuração de Mapa**: Adicionado dropdown `-- Copiar de outro mapa --` e botão `[📋 Copiar]` no cabeçalho de cada mapa. Clona profundamente as configurações do mapa selecionado para o mapa ativo e limpa a seleção.
  - **Modal de Confirmação no Botão PADRÃO**: Adicionado modal responsivo com `z-index: 99999` para evitar cliques acidentais ao restaurar o padrão.
  - **Remoção de Botões Obsoletos**: Removidos os botões `DESFAZER` (`Undo`) e `RECARREGAR` (`Reload`) da barra superior, mantendo salvamento automático em 1s.
  - **Temporizador da Primeiras Onda (`delayBeforeFirstWave`)**: Vinculado o slider de espera inicial da primeira onda para ler e gravar dinamicamente `delayBeforeFirstWave` por mapa.
- **Validação de Build**:
  - `TRL-DynamicSpawn-Client.csproj` e `TRL-DynamicSpawn-Server.csproj` compilados com **0 Erros**.

---

### 2026-08-04 — Fix de Sincronização do Raio no DynamicMaps, Backlogs 002, 003 e 005

- **Fix do Raio da Bolha & Normalização de Nomes de Mapa**:
  - Corrigida a divergência no JavaScript do Web UI (`Index.razor`) que salvava apenas `despawnDistance` e ignorava `spawnBubbleDistance`.
  - Corrigida a normalização de `MapNameHelper.Normalize("bigmap")` para retornar `"bigmap"`, alinhado com a chave do `config.json`.
  - Atualizada a consulta de `TRLMapBubbleOverlay` para `ServerConfigProvider.Config` (polling 5s).
- **Execução do Backlog 002 (Web UI)**:
  - Renomeadas as abas no Web UI e dicionários I18N para "ONDAS" e "BOTS".
  - Reduzido o limite máximo do slider `delayBeforeFirstWave` de 1200s para 120s.
- **Execução do Backlog 003 (Labs Exclusivo PMC)**:
  - Implementada trava no `DynamicSpawnManager` para zerar vagas de Scavs em Labs (`laboratory`) e alocar 100% da cota `playerCap` para PMCs (`sptBear`/`sptUsec`).
- **Execução do Backlog 005 (Revisão de Bloqueadores de Spawn)**:
  - Native Bosses e Followers isentados da filtragem do mod no `TryToSpawnInZoneAndDelayPatch`.
  - `IsValidSpawnZone` simplificado para não rejeitar `BotZone`s inteiras por LoS.
  - Linecast atualizado com `LayerMaskClass.PlayerStaticCollisionsMask` para reconhecer portas e objetos.
  - `heightLimit` ajustado para 4.0m em mapas com múltiplos andares.
- **Compilação e Versionamento**:
  - Incrementado BepInPlugin para `3.2.3` em `Plugin.cs`.
  - Cliente e Servidor compilados com sucesso (0 erros).

---

### 2026-08-02 — Fix da Injeção Prematura do DynamicSpawnManager no GameWorld.OnGameStarted

- **Diagnóstico do Log `LogOutput.log`**:
  - Identificado o erro `Cannot inject DynamicSpawnManager: IBotGame is not instantiated yet` no evento `OnGameStarted`.
- **Implementação do Aguardo Assíncrono (`DynamicSpawnManagerPatch.cs`)**:
  - Adicionado helper `TryInjectImmediate(GameWorld)` e Coroutine `WaitForBotGameAndInjectCoroutine`.
- **Validação de Build**:
  - Compilado `TRL-DynamicSpawn-Client.csproj` com 0 Erros (`TRL-DynamicSpawn.dll`).

---

### 2026-07-28 — Stuttering Fix, Sincronização de Timers, Regras de Warmup, Histórico de Teleporte & Master Fallback

- Eliminado Stuttering de 2s em `TRLMapBubbleOverlay.cs`.
- Sincronização do Timer de Teleporte em `BotDespawnManager.cs`.
- Refinamento do `SpawnHordeLoop` e paridade do Teleporte com Master Fallback.
