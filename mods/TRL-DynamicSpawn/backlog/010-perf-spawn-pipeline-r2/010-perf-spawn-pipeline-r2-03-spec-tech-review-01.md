# 010 — perf-spawn-pipeline-r2 · Review Técnica 01

**Mod:** TRL-DynamicSpawn
**Spec técnica revisada:** [010-perf-spawn-pipeline-r2-02-spec-tech.md](010-perf-spawn-pipeline-r2-02-spec-tech.md)
**Data:** 2026-08-22

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 9 · Total: 9
>
> **2026-08-23:** os 9 pontos foram aceitos e aplicados na spec técnica e na 01-spec (edição do coordenador); verificação das resoluções em [review 02](010-perf-spawn-pipeline-r2-03-spec-tech-review-02.md).

**Contexto consultado:** memória do mod — snapshot de 2026-08-16 (v3.2.9; o item 009 ainda não foi registrado na memória); pendências P-ROADMAP-01/04 não afetam este item; nenhuma pendência 🔴. Docs canônicos: `spt-antipatterns.md` (AP-01/03/05/08/09 aplicáveis). Dump do EFT **não está neste worktree** — todas as linhas foram conferidas no checkout principal (`C:\Repos\spt\tarkov-spt-4.0\references\eft-decompiled\Assembly-CSharp\`, mesma build 0.16.9) e os tipos no `references/eft-decompiled/types-index.json` deste worktree. Grafo do mod ausente (`references/graphs/mods/TRL-DynamicSpawn/` não existe) — callers/overrides auditados por grep no dump. Sem `modded/` — fonte editável é `Client/`; conflitos checados em `Client/Patches/` (nenhum outro patch em `ActivateBotsWithoutWave`; `ChooseProfilePatch` é substituição do existente). Tabela de deofuscação: `GClass1890 → OnlineBotSpawner`, `GClass1876 → EFT.NonWaveGroupScenario`, `GClass1881 → EFT.WDictionary` (`consolidated-mappings.txt:3471/4561/5392`) — a spec não rotula nenhum deles, sem conflito. Reviews anteriores: nenhuma (primeira review do item).

**Conferência item a item das linhas citadas (AP-09):** `BotsController.cs:536-542` ✓ (não-virtual) · `BotProfileDataClass.cs:85-96` ✓ · `BotsPresets.cs:170-189` ✓ · `BotCreationDataClass.cs:46/:102-105/:116/:142-146` ✓ · `GClass1890.cs:15` ✓ (assinatura; o `data.SpawnStopped` que lança a NRE com `data == null` está em `:17`) · `LocalGame.cs:139-143/:158/:187-194` ✓ · `GameWorld.cs:556/:2111/:2584` ✓ · `EFT/BotSpawner.cs:375-378` ✓ · `NonWavesSpawnScenario.cs:32-34/:146-148/:167` ✓ · **`NonWavesSpawnScenario.cs:157` ✗** (a chamada é `:160`; ver PA-01-08) · **`ProfileSettingsClass` ✗ — tipo não existe** (ver PA-01-01).

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Lógica | 🔴 | `ProfileSettingsClass` não existe no Assembly — o stub de `PmcMatches` não compila | ✅ Resolvido |
| PA-01-02 | C — Lógica | 🟡 | Auditoria de callers de `ActivateBotsWithoutWave` incompleta (AP-03): `GClass1876.cs:51` e `BotHalloweenEvent.cs:176` | ✅ Resolvido |
| PA-01-03 | B — Edge Case | 🟡 | Remover `BaseLocalGameStopPatch` sem substituto deixa a janela `Stop → OnDestroy` sem hook — patchear o override concreto `LocalGame.Stop` | ✅ Resolvido |
| PA-01-04 | A — Gap | 🟡 | Fallback PMC "qualquer perfil do pool" some em silêncio — mudança de comportamento não declarada (NR-4) | ✅ Resolvido |
| PA-01-05 | B — Edge Case | 🟡 | Pausa por "sem humano vivo" não alcança a onda em andamento (`_activeWaveCoroutine`) | ✅ Resolvido |
| PA-01-06 | B — Edge Case | 🟢 | `StopSpawnLoops`: reset das flags estáticas depois do `Instance == null` pode não rodar; justificativa do `finally` está errada | ✅ Resolvido |
| PA-01-07 | B — Edge Case | 🟢 | `ClearSptQueue` "1× por warmup" roda a cada reentrada do ESTÁGIO A, antes do check de humano, e ainda cancela `marksman` em voo (AC-M3) | ✅ Resolvido |
| PA-01-08 | C — Lógica | 🟢 | Deriva de linhas e rótulos no dump (`:157` → `:160`, `EFT/`, campos vs. propriedades, `Side` é `EPlayerSide?`) | ✅ Resolvido |
| PA-01-09 | A — Gap | 🟢 | Compatibilidade com MoreBotsAPI afirmada sem evidência; caminho `BotSpawner.ActivateBotsWithoutWave` direto não coberto pelo prefix | ✅ Resolvido |

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

### PA-01-01 · C — Lógica · 🔴 Bloqueador · ✅ Resolvido em 2026-08-23

**`ProfileSettingsClass` não existe no Assembly — o stub de `PmcMatches` não compila**

**Problema:** o stub da §5 declara `private static bool PmcMatches(InfoClass info, ProfileSettingsClass st, WildSpawnType requested)` com o comentário "tipos: os mesmos de `Profile.Info` / `Info.Settings` já usados em :826-830". Os tipos reais no dump são: `Profile.Info` → `public readonly InfoClass Info;` (`EFT/Profile.cs:632`) ✓, e `InfoClass.Settings` → `public ProfileInfoSettingsClass Settings` (`InfoClass.cs:123`). A classe `ProfileInfoSettingsClass` (`ProfileInfoSettingsClass.cs:5`) é a que carrega `public WildSpawnType Role` (`:7`) e `public BotDifficulty BotDifficulty` (`:9`). O nome `ProfileSettingsClass` **não consta** no `types-index.json` (grep exato: 0 ocorrências; `ProfileInfoSettingsClass` e `InfoClass` constam). O código atual do mod (`Patches.cs:826-830`) nunca nomeia esse tipo — usa `x.Info.Settings?.Role` com inferência, por isso o erro não existia até agora.

**Por que importa:** `error CS0246: The type or namespace name 'ProfileSettingsClass' could not be found` — o `/code-mod` trava na primeira compilação. É o caso literal de AP-09 (membro plausível porém inexistente tratado como verdade).

**Sugestão:** trocar a assinatura para `PmcMatches(InfoClass info, ProfileInfoSettingsClass st, WildSpawnType requested)` e, no loop, `var info = p?.Info; var st = info?.Settings;` continua válido (inferência). Atualizar o comentário do stub para citar `EFT/Profile.cs:632` (`InfoClass Info`), `InfoClass.cs:123` (`ProfileInfoSettingsClass Settings`) e `ProfileInfoSettingsClass.cs:7/:9` (`Role`/`BotDifficulty`). Registrar os dois tipos na §9 check 9.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________
<!-- Após resolver: marcar a opção escolhida, trocar título para ✅ Resolvido em YYYY-MM-DD e adicionar **Resolução:** ... -->

**Resolução:** spec §5 — `PmcMatches(InfoClass info, ProfileInfoSettingsClass st, WildSpawnType requested)` com comentário citando `EFT/Profile.cs:632`, `InfoClass.cs:123`, `ProfileInfoSettingsClass.cs:5-9`; §9 check 9 registra que `ProfileSettingsClass` não existe. Conferido no dump (review 02).

### PA-01-02 · C — Lógica · 🟡 Importante · ✅ Resolvido em 2026-08-23

**Auditoria de callers de `ActivateBotsWithoutWave` incompleta (AP-03)**

**Problema:** a §2 e a §9 check 3 afirmam "Único caller no dump (grep `ActivateBotsWithoutWave(`): `NonWavesSpawnScenario` — `:157`". O grep no dump devolve **três** chamadores fora do próprio `BotsController`:
1. `EFT/NonWavesSpawnScenario.cs:160` — `botsController_0.ActivateBotsWithoutWave(1, data)` (a linha `:157` é `WildSpawnType role = gclass1881_1.Random();`).
2. `GClass1876.cs:51` — `botsController.ActivateBotsWithoutWave(groupSize, botProfileDataClass)`, dentro de `method_0` (`:42-52`), que cria `BotProfileDataClass(Savage, assault, diff)` com `ShallBeGroup` (grupo de Scavs). `GClass1876` é o `NonWaveGroupScenario` (mapping 4.1) instanciado em `NonWavesSpawnScenario.cs:88` e invocado em `:154` (`gclass1876_0.TrySpawn(...)`) **antes** do loop de `:155-161`. Esse caminho passa pelo mesmo `BotsController.ActivateBotsWithoutWave` → o prefix novo **cobre** (role `assault` → recusa) — resultado positivo, mas não auditado.
3. `BotHalloweenEvent.cs:176` — `Spawner.ActivateBotsWithoutWave(Spawner.MaxBots / 2, new BotProfileDataClass(Savage, assault, normal, 0f))` chama **`BotSpawner.ActivateBotsWithoutWave`** (`EFT/BotSpawner.cs:375`) direto, sem passar pelo `BotsController` → o prefix novo **não** cobre; sobra o backstop em `TryToSpawnInZoneAndDelay` (cria perfil + `ChooseProfile` antes de recusar). Só dispara no evento de Halloween (`RitualCompleted`).

**Por que importa:** (a) a §9 check 3 está ✅ com evidência factualmente errada — a regra do command é confrontar isso; (b) o caminho 2 é o que produz as **rajadas de 3–8** recusas medidas na V1 (grupo `MinToBeGroup..MaxToBeGroup` de uma vez) — a spec deveria nomeá-lo porque é ele que prova que AC-M1/AC-M2 fecham; (c) o caminho 3 é a exceção documentável para AC-M1 ("= 0") — sem ela, uma única linha `Blocked Vanilla Assault Scav Spawn` num evento sazonal faria a V2 "falhar" sem bug.

**Sugestão:** na §2 (linha do `BotsController.cs:536`) e na §9 check 3, substituir "Único caller … `:157`" por: "Callers via `BotsController`: `EFT/NonWavesSpawnScenario.cs:160` (1 bot por vaga) e `GClass1876.cs:51` (`NonWaveGroupScenario`, grupo de `assault`, chamado em `NonWavesSpawnScenario.cs:154`) — ambos cobertos pelo prefix. Caller que **pula** o `BotsController`: `BotHalloweenEvent.cs:176` → `BotSpawner.ActivateBotsWithoutWave` (`EFT/BotSpawner.cs:375`), coberto só pelo backstop `TryToSpawnInZoneAndDelayPatch`." Na 01-spec, AC-M1 ganha a ressalva "fora do evento de Halloween". Opcional: se quiser cobertura total sem custo, mover o alvo do prefix para `BotSpawner.ActivateBotsWithoutWave(int, IGetProfileData)` (`:375`, `async Task`, não-virtual) — mas aí o prefix que pula o original precisa setar `__result = Task.CompletedTask` (mesmo padrão de `DisableVanillaWavesPatch`, `Patches.cs:398`); a spec deve escolher um dos dois e registrar.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** spec §2 (linha do `BotsController.cs:536`) e §9 check 3 listam `EFT/NonWavesSpawnScenario.cs:160`, `GClass1876.cs:51` (NonWaveGroupScenario) e `BotHalloweenEvent.cs:176` (contorna via `BotSpawner`, fica no backstop); 01-spec ganhou AC-X6 (tolerância de AC-M1 com Halloween ativo). Alvo mantido em `BotsController` (opção 1 da sugestão).

### PA-01-03 · B — Edge Case · 🟡 Importante · ✅ Resolvido em 2026-08-23

**Remover `BaseLocalGameStopPatch` sem substituto deixa a janela `Stop → OnDestroy` sem hook**

**Problema:** a spec remove o patch em `BaseLocalGame<EftGamePlayerOwner>.Stop` porque a V1 provou que ele não dispara (PA-01-05 do 009; fonte logada `GameWorld.OnDestroy`). Mas o próprio AUD-01-06, que este item corrige, descreve a janela: "entre `Stop` e `OnDestroy` continuam". O vanilla desliga os três cenários de spawn no início do encerramento (`EFT/LocalGame.cs:357-361`: `bossSpawnScenario_0.Stop(); nonWavesSpawnScenario_0.Stop(); wavesSpawnScenario_0.Stop();`) e só depois a cena cai; o mod, com a remoção, passa a parar **só** no `OnDestroy` — `SpawnHordeLoop`/`ProcessWave` seguem criando perfis (`bot/generate`) durante a tela de extração/morte. AC-M6 ("após o hook de fim de raid, nenhuma linha de onda") fica verdadeira por definição (o hook é o último evento), mas o trabalho zumbi que o AUD-01-06 mira continua existindo nessa janela. A causa provável da inércia do patch é a que a review do 009 já apontou: Harmony em **tipo genérico fechado com argumento por referência** — o código nativo é compartilhado entre instanciações. O override concreto existe e chama a base: `public override void Stop(string profileId, ExitStatus exitStatus, string exitName, float delay = 0f)` em `EFT/LocalGame.cs:357` (→ `base.Stop` em `:362`); no Fika, `CoopGame.Stop` (`references/fika-plugin/Fika.Core/Main/GameMode/CoopGame.cs:718`) é o equivalente para host/headless.

**Por que importa:** sem o hook cedo, cada saída de raid paga alguns segundos de `ProcessWave` (pré-busca `AddToTargetBackup`, `Create`, `TryToSpawnInZoneAndDelay`) com a raid já encerrada — justamente o cenário "raid acabando" do AUD-01-06; e no Fika host a janela entre `Stop` e a destruição do mundo é maior (espera de peers). NR-6/AC-X3 cobrem "sem humano vivo", não "raid encerrada com humano vivo que extraiu".

**Sugestão:** em vez de só remover, **substituir o alvo**: `AccessTools.Method(typeof(LocalGame), nameof(LocalGame.Stop))` (tipo concreto `EFT.LocalGame`, sem genérico) chamando `RaidLifecycle.OnRaidEnd("LocalGame.Stop")`; para o Fika, resolver `Fika.Core.Main.GameMode.CoopGame` por reflection (`AccessTools.TypeByName`) e patchear o `Stop` dele se o tipo existir (soft — `Enable()` em `try/catch`, como já é o padrão dos patches de lifecycle). Manter `OnDestroy` como primário/idempotente (já é). Registrar na §2 os dois alvos, na §9 check 1 a evidência `LocalGame.cs:357/:362` + `CoopGame.cs:718`, e no checklist V2 "log `Raid end hook fired (LocalGame.Stop)` aparece 1×" — se de novo não aparecer, aí sim remover de vez (AC-X4 passa a ser "hook `Stop` só via override concreto"). Caminho alternativo aceitável: manter a remoção e **declarar** na 01-spec (AC-X4) que a janela `Stop → OnDestroy` fica sem parada de ondas, com a estimativa de duração medida na V2.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** spec §2/§5 — `BaseLocalGameStopPatch` substituído por `LocalGameStopPatch` (`typeof(LocalGame)`, `EFT/LocalGame.cs:357`) + `CoopGameStopPatch` soft (`AccessTools.TypeByName("Fika.Core.Main.GameMode.CoopGame")`, `CoopGame.cs:718`, registro condicional no `Plugin.cs`); §9 check 1 e 01-spec AC-X4 reescritos; V2 confere a fonte no log.

### PA-01-04 · A — Gap · 🟡 Importante · ✅ Resolvido em 2026-08-23

**Fallback PMC "qualquer perfil do pool" some em silêncio — mudança de comportamento não declarada**

**Problema:** o `ChooseProfilePatch` atual, para `pmcUSEC`/`pmcBEAR`, tem um segundo degrau (`Patches.cs:834-838`): se nenhum perfil USEC/BEAR casa, ele **pega qualquer perfil do pool** (`list = profiles2Select.Where(x => x != null && x.Info != null)`) — ou seja, entrega um perfil **Savage** para uma vaga de PMC em vez de devolver ao vanilla. O stub novo (§5) faz `Profile chosen = exact ?? relaxed; if (chosen == null) return true;` → vanilla → `null` → `LoadBots(3)` (`BotsPresets.cs:178-189`). A 01-spec NR-4 diz "a lógica PMC do patch (qualquer lado USEC/BEAR) permanece" e AC-X1 só declara a tolerância de **dificuldade**; nenhum AC declara que o degrau "qualquer perfil" foi removido. Também some a heurística `Role.ToString().Contains("usec"/"bear")` (`:829-830`) — inofensivo em 0.16.9 (`WildSpawnType` tem `pmcUSEC`/`pmcBEAR`; o mod não usa `sptUsec`/`sptBear` em lugar nenhum de `Client/`), mas é outra diferença não listada. De quebra, a terceira linha de `PmcMatches` (`isUsec ? st.Role == pmcUSEC : st.Role == pmcBEAR`) é redundante com a segunda (`st.Role == requested`).

**Por que importa:** é quase certamente a mudança **certa** (um "PMC" nascendo com perfil de Scav é bug de composição, e é exatamente o caso em que fabricar 3 perfis PMC é necessário), mas hoje o pool parte de 30/30 PMC e passa a partir de 15/15 (AC-X2) — o degrau some no mesmo item em que o cenário "pool PMC vazio" fica **mais provável**. Se a V2 mostrar `bot/generate` de PMC a mais ou uma onda atrasada, ninguém vai saber que veio daqui, porque a spec diz que a lógica PMC "permanece".

**Sugestão:** na 01-spec, adicionar **AC-X5 — degrau "qualquer perfil" removido do match PMC**: "vaga PMC sem nenhum perfil USEC/BEAR no pool passa a fabricar 3 perfis PMC (vanilla) em vez de consumir um perfil Scav; trade-off: 1 `bot/generate` a mais nesse caso raro, composição correta". Na spec técnica §5, anotar no stub `// ref: AC-X5 — the old "ANY profile" fallback (Patches.cs:834-838) is intentionally dropped` e simplificar `PmcMatches` para `info.Side == targetSide || st.Role == requested`. Ajustar NR-4 para "match exato primeiro; PMC aceita qualquer dificuldade do mesmo lado/role".

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** 01-spec — AC-X5 novo (vaga PMC sem USEC/BEAR volta ao vanilla → `LoadBots(3)`), NR-4 ajustado; spec §5 — `PmcMatches` simplificado para `info.Side == wantedSide || st.Role == requested`, comentário declara a remoção do fallback antigo (`Patches.cs:834-838`).

### PA-01-05 · B — Edge Case · 🟡 Importante · ✅ Resolvido em 2026-08-23

**Pausa por "sem humano vivo" não alcança a onda em andamento**

**Problema:** o stub (c) coloca o check `if (GetAliveHumanCount() == 0) { yield return new WaitForSeconds(5f); continue; }` no topo do `while` interno do ESTÁGIO A, **depois** do `yield return new WaitForSeconds(1f)`. Isso impede a **próxima** `ProcessWave`, mas a onda corrente (`_activeWaveCoroutine = StartCoroutine(ProcessWave(...))`, `DynamicSpawnManager.cs:413`) continua: `ProcessWave` itera a lista de grupos com `smoothSpawningDelay` (1,5 s default) entre grupos (`:939-946`) e cada `SpawnGroupBotsCoroutine` faz `Create` + `TryToSpawnInZoneAndDelay` por membro (`:981-1044`) — uma onda de 25 vagas leva dezenas de segundos, todos depois da morte do último humano. O `continue` também pula `warmupAttempt++`, o `for` de `warmupInterval` s e a atualização de `_nextWaveTime` (o HUD passa a mostrar "Próxima Wave em: 0s" congelado durante a pausa). AC-X3 promete "o mod deixa de calcular ondas" — a onda em voo não é "calculada", mas é **spawnada**.

**Por que importa:** solo morto aos 30 s de warmup (caso comum) = a onda inteira nasce para ninguém, com `bot/generate` e `ChooseProfile` que AC-M6 conta como zero; e `GetAliveHumanCount()` em `AllAlivePlayersList` **remove** o player morto da lista (`GameWorld.cs:2324`) — o check fica `== 0` imediatamente, mas a onda já está rodando.

**Sugestão:** no ramo de pausa, antes do `yield` de 5 s: `if (_activeWaveCoroutine != null) { StopCoroutine(_activeWaveCoroutine); _activeWaveCoroutine = null; _isSpawningWave = false; }` e `_nextWaveTime = Time.time + 5f` (HUD honesto). Registrar na spec a limitação conhecida do Unity: `StopCoroutine(pai)` **não** para a `SpawnGroupBotsCoroutine` filha já iniciada via `yield return StartCoroutine(...)` (`:932`) — ela termina o grupo corrente (≤ `groupSize × 0,2 s` + os `Create` pendentes) e para sozinha; é aceitável e deve constar no corner case "Host Fika morto / solo morto". O mesmo comportamento (filha sobrevive) já existe hoje em `:411`.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** spec §5 (c) — ramo de pausa faz `StopCoroutine(_activeWaveCoroutine)`, zera `_activeWaveCoroutine`, `IsGeneratingDynamicWave = false`, `_nextWaveTime = 0f`; limitação da coroutine filha documentada no stub e em AC-X3 (01-spec).

### PA-01-06 · B — Edge Case · 🟢 Menor · ✅ Resolvido em 2026-08-23

**`StopSpawnLoops`: reset das flags depois do `Instance == null` pode não rodar; justificativa do `finally` está errada**

**Problema:** o stub (e) faz `if (Instance == null) return;` **antes** de `IsGeneratingDynamicWave = false; IsWarmupActive = false;`. `StopSpawnLoops()` é chamado do prefix de `GameWorld.OnDestroy` (`GameWorld.cs:2111`); o `DynamicSpawnManager` é um componente **do mesmo `GameObject`** (`DynamicSpawnManagerPatch.cs:60`) e a ordem de `OnDestroy` entre componentes do mesmo objeto não é garantida pelo Unity — se o manager já foi destruído, `Instance == null` é `true` (igualdade "fake null" do `UnityEngine.Object`) e as flags estáticas **não** são resetadas, exatamente o que a §7 ("Estado estático") e a §9 checks 5/8 dizem garantir. Além disso, a justificativa "finally{} of a stopped coroutine does not run" não se aplica: o `try/finally` de `SpawnGroupBotsCoroutine` (`:1017-1033`) e o de `SpawnReplacementBotCoroutine` (`:1236-1243`) **não contêm `yield`** — a flag é sempre `false` em qualquer ponto de suspensão, logo nenhuma parada de coroutine pode deixá-la presa em `true`. O reset defensivo é bom; a razão declarada é outra (boot de raid seguinte com `IsWarmupActive` herdado — que hoje ninguém lê: grep em `Client/` mostra só atribuições em `:22/:344/:355/:369`).

**Por que importa:** impacto prático nulo hoje (a flag não tem leitor e o `finally` não pode ser pulado), mas a spec descreve uma garantia que o código não dá — o próximo a ler vai confiar nela.

**Sugestão:** reordenar o stub: `IsGeneratingDynamicWave = false; IsWarmupActive = false; if (Instance == null) return; Instance.StopAllCoroutines(); Instance._activeWaveCoroutine = null; Instance._isSpawningWave = false;`. Trocar o comentário por `// static flags first: the component may already be destroyed when GameWorld.OnDestroy fires; try/finally blocks around the flag contain no yield, so this is belt-and-braces for the next raid, not a StopCoroutine fix`. Opcional: remover `IsWarmupActive` da lista de "estado a resetar" na §7 (sem leitor) ou apontar o leitor pretendido.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** spec §5 (e) — flags estáticas resetadas **antes** do `if (Instance == null) return;`, comentário corrigido (fake-null do Unity; `try/finally` sem `yield`); §9 check 8 reescrito com a evidência `:1017-1033`/`:1236-1243`.

### PA-01-07 · B — Edge Case · 🟢 Menor · ✅ Resolvido em 2026-08-23

**`ClearSptQueue` "1× por warmup" roda a cada reentrada do ESTÁGIO A, antes do check de humano, e ainda cancela `marksman` em voo**

**Problema:** o stub (c) coloca `ClearSptQueue()` logo após `if (aliveRealBots < dynamicCap)` — isso é "1× por **sessão** de warmup", e o ESTÁGIO A reentra a cada ciclo do `while (true)` externo em que bots morreram (após cada cooldown de `_secondsBetweenWaves`, `:455-456`). Na prática: N cancelamentos globais por raid (N = número de cooldowns que terminam com mapa abaixo do cap), não 1. Cada um continua sendo `BotEventHandler.StopBotSpawn()` global (`BotCreationDataClass.cs:116/:142-146`): agora que `marksman` passa pelo vanilla (NR-2), um `Create` de sniper em voo nesse instante vira `null` → `TrySpawnFreeAndDelay(null)` (`EFT/BotSpawner.cs:377`) → NRE em `GClass1890.cs:17` — a cadeia do AUD-01-05 com frequência menor, não zero. AC-M3 exige **0** NRE. A spec já prevê isso no corner case "Limpeza única … boss tardio", mas o critério não tem tolerância. Também: o cancelamento roda **antes** do check `GetAliveHumanCount() == 0` — com todos mortos, cada reentrada ainda cancela a fila do jogo.

**Por que importa:** AC-M3 pode "falhar" na V2 por 1–2 NREs legítimos da limpeza, e a decisão "remover de vez" seria tomada com base em ruído; o item 006 introduziu a limpeza para uma "fila presa" **no início** da raid, não a cada cooldown.

**Sugestão:** limitar a uma vez **por raid**: campo `private bool _sptQueueCleared;` (instância — morre com o componente, sem reset estático) e `if (!_sptQueueCleared) { _sptQueueCleared = true; ClearSptQueue(); }` no mesmo ponto do stub, **depois** de um `if (GetAliveHumanCount() == 0)` inicial (ou mover a chamada para dentro do `while`, guardada pelo bool, após o check de humano). Na 01-spec, AC-M3 vira "NRE em `TrySpawnFreeInner` ≤ 1 por raid (a limpeza única) — meta 0" e NR-5 explicita "limpeza única = 1× por raid, no primeiro ESTÁGIO A". Se o usuário preferir a semântica por sessão, registrar o número esperado de NREs como "≤ nº de cooldowns".

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** spec §1/§5 (c) — `_sptQueueClearedThisRaid` (campo de instância; o componente é recriado por raid em `DynamicSpawnManagerPatch.cs:60`), limpeza só após o check de humano vivo; 01-spec AC-M3 com tolerância ≤ 1 NRE/raid.

### PA-01-08 · C — Lógica · 🟢 Menor · ✅ Resolvido em 2026-08-23

**Deriva de linhas e rótulos no dump**

**Problema:** conferência contra o dump (build 0.16.9, checkout principal):
- `NonWavesSpawnScenario.cs:157` (§2, §6, §9 check 3) → a chamada `botsController_0.ActivateBotsWithoutWave(1, data)` é `:160`; `:115-159` (§1) → `Update()` vai de `:115` a `:162`. O arquivo está em **`EFT/`** (`EFT/NonWavesSpawnScenario.cs`); a §1/§9 citam sem o prefixo, diferente do padrão usado para `EFT/BotsController.cs`.
- §5 (stub `ChooseProfilePatch`): "`BotProfileDataClass.cs:87` (Side / WildSpawnType_0 / BotDifficulty_0 … public property)" — `WildSpawnType_0` e `BotDifficulty_0` são **campos** públicos (`:16`/`:19`), e `Side` é **propriedade `EPlayerSide?`** (nullable, `:43-50`). A comparação `info.Side == side` no stub compila pelo operador "levantado" (`EPlayerSide == EPlayerSide?`), igual ao vanilla (`:87`) — mas um dev que leia "property `EPlayerSide`" pode "corrigir" com `.Value` e lançar `InvalidOperationException` quando `Side` for `null` (construtor sempre preenche, mas `IGetProfileData.Side` é nullable por contrato, `IGetProfileData.cs:7`).
- `GClass1890.cs:15` é a assinatura de `TrySpawnFreeInner`; a desreferência que gera a NRE com `data == null` é `:17` (`if (data.SpawnStopped)`). Rótulo da tabela 4.1: `OnlineBotSpawner` — a spec não rotula, ok.
- `IGetProfileData` está no namespace global (`IGetProfileData.cs:5`, sem `namespace`) — o `using EFT;` do stub não é o que o resolve; compila do mesmo jeito (mesmo assembly), mas vale anotar para o `AccessTools.Method(... typeof(IGetProfileData))`.

**Por que importa:** qualidade de rastreabilidade (AP-09): a regra do repo é `arquivo.cs:linha` exato; e a nota sobre `EPlayerSide?` evita um "fix" errado no `/code-mod`.

**Sugestão:** corrigir as citações para `EFT/NonWavesSpawnScenario.cs:115-162` e `:160`; no stub, trocar o comentário por `// ref: BotProfileDataClass.cs:16 (WildSpawnType_0, public field), :19 (BotDifficulty_0, public field), :43 (Side, EPlayerSide? property) — lifted == on purpose, do not .Value`; citar `GClass1890.cs:17` para a NRE; anotar `IGetProfileData` (namespace global).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** spec §1/§2/§9 check 9 — citações corrigidas (`EFT/NonWavesSpawnScenario.cs:115-162`, `:160`; campos `:16/:19`, `Side` `EPlayerSide?` `:43`, sem `.Value`; `GClass1890.cs:17`; `IGetProfileData` em namespace global). Restam ocorrências antigas em comentários de stub/§6 — ver PA-02-02 (review 02).

### PA-01-09 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-08-23

**Compatibilidade com MoreBotsAPI afirmada sem evidência; caminho `BotSpawner` direto fora do prefix**

**Problema:** a §7 diz "MoreBotsAPI (presente no log — expõe API de spawn própria, não passa por `ActivateBotsWithoutWave`)". Não há fonte disso no repo: grep por `MoreBots` em `references/` devolve 0 arquivos e em `mods/` só a própria spec, o README/OrbitManager do ORBIT e um doc do ICM — nenhum código do MoreBotsAPI está vendorizado. A afirmação é plausível mas **não verificada** (AP-09: recon ≠ fato). Se o MoreBotsAPI (ou qualquer outro mod) spawnar via `BotsController.ActivateBotsWithoutWave` com `assault`, o prefix novo recusa **sempre** no host (o mod não usa `IsGeneratingDynamicWave` nesse caminho); se spawnar via `BotSpawner.ActivateBotsWithoutWave` direto (como o `BotHalloweenEvent.cs:176`), passa pelo backstop e é recusado lá. Nos dois casos o resultado é o mesmo de hoje (o backstop já recusava 100% de `assault`), então não há regressão — mas a spec deveria dizer "não verificado; comportamento igual ao atual por construção" em vez de afirmar o mecanismo do outro mod.

**Por que importa:** clareza de risco para a V2: se um Scav de outro mod sumir, a causa será investigada no lugar certo (recusa de `assault` fora de `IsGeneratingDynamicWave` — que já era assim na 3.3.0).

**Sugestão:** reescrever a frase da §7 para: "MoreBotsAPI: fonte não disponível no repo (não verificado). Qualquer spawn de `assault`/`cursedAssault` de terceiros fora de `IsGeneratingDynamicWave` já era recusado no backstop (`Patches.cs:546-556`); o prefix novo só antecipa a recusa — sem mudança de comportamento para outros mods. Conferir na V2 que os bots do MoreBotsAPI seguem nascendo (se forem `assault`, já não nasciam na 3.3.0)." Adicionar ao checklist V2.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** spec §7 — MoreBotsAPI marcado "não verificado", sem regressão por construção (backstop já recusava 100% de `assault`), check explícito na V2; `BotHalloweenEvent.cs:176` registrado como caminho que contorna o prefix.

---

## Verificações sem achado (registro)

Itens conferidos a pedido e que **batem** com a spec — deixados aqui para a próxima review não refazer:

- `BotsController.ActivateBotsWithoutWave(int, IGetProfileData)` — `EFT/BotsController.cs:536-542`, `public void`, não-virtual; prefix `bool` retornando `false` pula o original (que só chamaria `BotSpawner_1.ActivateBotsWithoutWave(...).HandleExceptions()` em `:540`). Nome do parâmetro `data` ✓ (binding por nome no Harmony).
- `BotProfileDataClass : IGetProfileData` (`BotProfileDataClass.cs:8`); `ChooseProfile` é de instância, não-virtual (`:85`); patch no método da classe intercepta também as chamadas via interface (`BotCreationDataClass.cs:126`, `GClass680.cs:169`, `BotsPresets.cs:189`). Outras implementações de `IGetProfileData` (`GClass687/688/689`, `ProfileDataClass`) caem no `data is BotProfileDataClass` → `return true` ✓.
- `NonWavesSpawnScenario`: piso de 10 s (`const float float_1 = 10f` `:32`, `float_2 = 10f` `:34`, `if (float_2 < 10f) float_2 = 10f` `:146-148`) ✓; sorteio de papel entre `assault` e `marksman` por peso `BotAssault`/`BotMarksman` (`:167`), fallback `assault` (`:173-176`) ✓. Com o prefix recusando `assault`, as chamadas com `marksman` seguem intactas pelo vanilla — `num` (`:131`) não muda porque a recusa atual também não incrementava `AliveLoadingDelayedBotsCount`; a frequência de `marksman` **não** aumenta (NR-2 ✓).
- `StopAllCoroutines()` no manager para: `FetchServerConfigAndStart` (`:50`), `SpawnHordeLoop` (`:171`), `ProcessWave` (`:413`), `SpawnGroupBotsCoroutine` (`:932`), `SpawnReplacementBotCoroutine` (`:1203`). Nenhuma precisa sobreviver ao fim da raid. `WaitForBotGameAndInjectCoroutine` roda no `GameWorld` (`__instance.StartCoroutine`, `DynamicSpawnManagerPatch.cs:44`) — não é afetada ✓.
- `ConfigurationManagerAttributes` (`Client/ConfigurationManagerAttributes.cs:28`, `internal sealed`, namespace global) tem `public bool? IsAdvanced` (`:141`); passa como `tags` do `ConfigDescription(string, AcceptableValueBase, params object[])` ✓.
- `GameWorld.AllAlivePlayersList` é `List<Player>` (`EFT/GameWorld.cs:556`); `Player.IsAI` e `HealthController.IsAlive` já são usados pelo mod na mesma lista (`Patches.cs:603`); `DynamicSpawnManager.IsHeadlessPlayer(Player)` existe, `public static` (`:1641`) ✓. Player morto é removido da lista em `:2324` (e `IsAlive` cobre o intervalo).
- Reservoir de passagem única no stub: sorteio uniforme independente para "exato" e "relaxado" (`Random.Range(0, n) == 0` com `n` crescente) ✓; `withDelete` remove da **mesma** lista recebida, igual ao vanilla (`:93-96`) ✓; prefix `bool` com `ref Profile __result` + `return false` é o padrão já em produção no mesmo patch (`Patches.cs:847-849`) ✓.
- `GetAliveHumanCount() == 0 → continue`: pula o `break` de cap por construção (pausa até `StopSpawnLoops`/destruição do componente); não é loop infinito porque o componente morre com o `GameWorld` — intencional (AC-X3). Ver PA-01-05 para a onda em voo.
- Nenhum outro `.cs` em `mods/` patcheia `ActivateBotsWithoutWave` (grep: só docs deste item e do relatório) ✓.
