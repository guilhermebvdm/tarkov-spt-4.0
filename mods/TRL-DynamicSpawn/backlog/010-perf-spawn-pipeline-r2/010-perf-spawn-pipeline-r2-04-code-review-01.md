# 010 — perf-spawn-pipeline-r2 · Code Review 01

**Mod:** TRL-DynamicSpawn
**Spec funcional:** [010-perf-spawn-pipeline-r2-01-spec.md](010-perf-spawn-pipeline-r2-01-spec.md)
**Spec técnica:** [010-perf-spawn-pipeline-r2-02-spec-tech.md](010-perf-spawn-pipeline-r2-02-spec-tech.md)
**Asbuild:** [010-perf-spawn-pipeline-r2-05-asbuild.md](010-perf-spawn-pipeline-r2-05-asbuild.md)
**Data:** 2026-08-23

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 5 · Total: 5

**Contexto consultado:** memória do mod — snapshot de 2026-08-16 (v3.2.9; os itens 009 e 010 ainda não foram registrados na memória — "sem memória prévia" para este item); pendências P-ROADMAP-01/04 não afetam; nenhuma pendência 🔴. Reviews técnicas 01/02 (12 PA fechados — nenhum volta aqui); code review 01 do item 009 (CR-01-01..03 aplicados, CR-01-04 `OnGameStartPatches.cs` deferido — continua morto, fora de escopo por decisão da 01-spec). Docs canônicos: `spt-antipatterns.md` (AP-01/02/03/08/09 aplicáveis). Código revisado: diff do commit `cc16a36d` ("perf(dynspawn): round 2 …") + leitura integral de `Client/Components/DynamicSpawnManager.cs`, `Client/Patches/SpawnGatePatches.cs`, `Patches.cs` (`ChooseProfilePatch`, `TryToSpawnInZoneAndDelayPatch`, `DisableVanillaWavesPatch`), `BotSpawnLoggerPatch.cs`, `RaidLifecyclePatches.cs`, `Helpers/RaidLifecycle.cs`, `Settings.cs`, `Plugin.cs`, `DynamicSpawnManagerPatch.cs`, `FikaHelper.cs`. Dump do EFT no checkout principal (`C:\Repos\spt\tarkov-spt-4.0\references\eft-decompiled\Assembly-CSharp\`, build 0.16.9) e Fika (`references/fika-plugin/Fika.Core/`). Grafo do mod **não existe** (`references/graphs/mods/TRL-DynamicSpawn/` ausente) — callers auditados por grep em `Client/` e no dump. Este mod não tem `modded/`: a fonte editável é `Client/`, e os links abaixo apontam para lá.

**Checklist `spt-performance-analysis` (obrigatório neste item):** (1) panorama — as superfícies novas são um prefix por vaga a cada ≥10 s (`ActivateBotsWithoutWavePatch`, custo = 3 comparações) e um `GetAliveHumanCount()` a cada 1 s no warmup (loop sobre ≤5 humanos após o filtro `IsAI`) — ambas frias ✓; (2) patches dimensionados com early-return no topo ✓; (5) lifecycle — `StopSpawnLoops` fecha as 5 etapas para as coroutines do manager, mas a **pausa sem humano não alcança o timer do warmup** (CR-01-02); (6) GROW — o mecanismo real de crescimento do pool **não é o que o código assume**: `AddToTargetBackup` registra um alvo de reposição permanente (CR-01-01); (7) log gated antes de formatar ✓ (AC-M5 verificável — ver "Verificações sem achado"); (10) cada NR da 01-spec tem como ser conferido na V2 — tabela ao fim deste arquivo.

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | B — Bug latente | 🟠 | `AddToTargetBackup` é alvo de reposição **permanente** (1ª chamada por chave vence; `assault` já vem registrado pelo vanilla): pré-carga de Scav é no-op, pré-busca por onda fixa alvos por dificuldade sorteada (o motor real do GROW) e a propriedade nova documenta outro mecanismo | ✅ Aplicado (com modificação) |
| CR-01-02 | C — Gap vs. spec | 🟠 | Pausa "sem humano vivo" só roda no topo do `while` interno — a onda em voo continua até `warmupInterval` s (30–120 s) depois do último humano morrer (AC-X3) | ✅ Aplicado |
| CR-01-03 | E — Legibilidade | 🟢 | `_isSpawningWave` é código morto (nenhum leitor) e fica preso em `true` quando a pausa interrompe `ProcessWave` | ✅ Aplicado |
| CR-01-04 | E — Legibilidade | 🟢 | Comentários desatualizados: `RaidLifecycle.cs:27-31` ainda descreve o hook `BaseLocalGame.Stop` removido; `Patches.cs:855` cita `:91-94` (é `:93-96`) | ✅ Aplicado |
| CR-01-05 | F — Melhoria opcional | 🟢 | Emissores Info por onda que continuam fora do gate (1–8 linhas/onda) + HUD mostra contagem regressiva do cooldown com todos mortos | ✅ Aplicado (a+b; HUD do cooldown deferido) |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Pontos

### CR-01-01 · B — Bug latente · 🟠 Forte · ✅ Aplicado em 2026-08-23

**`AddToTargetBackup` é um alvo de reposição permanente, não um "pedido de N perfis": a pré-carga de Scav é no-op, a pré-busca por onda fixa alvos por dificuldade sorteada e a propriedade nova documenta um mecanismo que não existe**

**Local:** [`mods/TRL-DynamicSpawn/Client/Components/DynamicSpawnManager.cs:147-156`](../../Client/Components/DynamicSpawnManager.cs#L147) (pré-carga inicial) · [`:556-557`](../../Client/Components/DynamicSpawnManager.cs#L556) (comentário) · [`:804-812`](../../Client/Components/DynamicSpawnManager.cs#L804) (pré-busca por onda) · [`mods/TRL-DynamicSpawn/Client/Helpers/Settings.cs:127-131`](../../Client/Helpers/Settings.cs#L127) (tooltip) · `PROPRIEDADES.md` (seção `Profile Pool (Advanced)`)

**Problema:** o item inteiro (AUD-01-04 / AC-X2) modela `IBotCreator.AddToTargetBackup(diff, role, count)` como "pedir `count` perfis ao servidor agora". O dump diz outra coisa. A implementação (`BotsPresets.cs:156-159` → `GClass684.cs:258-263`) é:

```csharp
public void AddToTargetBackup(BotDifficulty difficulty, WildSpawnType role, int count)
{
    if (!Gclass624_0.ContainsKey(role, difficulty))   // GClass684.cs:260
        Gclass624_0.Add(role, difficulty, count);
}
```

`Gclass624_0` é um dicionário (role, dificuldade) → **nível-alvo do cache de perfis**, lido por um timer de 5 s (`GClass684.cs:122-125`, `method_0` em `:129-192`): para cada chave, se `alvo − perfis em cache > 3`, ele enfileira um `WaveInfoClass(deficit)` e dispara `LoadBots` (`byBackup`, `method_1` `:216-256`), com throttle de 30 s (`Float_4`, `:173`). O construtor já registra `assault` e `marksman` em easy/normal/hard com alvo **8** (`:113-118`). Consequências concretas no código entregue:

1. **Pré-carga de Scav nunca teve efeito** — `AddToTargetBackup(normal, assault, preload)` (`:155`) cai no `ContainsKey` → ignorado (era assim com 20, é assim com 15, e com 0 também). O log de `:150` ("15 per type: USEC, BEAR, Scav") afirma algo que não acontece.
2. **A pré-busca por onda (`:806-809`) não "pede exatamente o que a onda consome"** (comentário `:556-557`): para `assault` em easy/normal/hard é ignorada; para `pmcUSEC`/`pmcBEAR` a **primeira** onda em que a dificuldade sorteada aparece registra um alvo **permanente** com o tamanho daquela onda (ex.: `hard → 7 USEC`), que o jogo passa a **repor a cada ~30 s** pelo resto da raid sempre que o cache daquela chave cair abaixo de `alvo − 3`. Com 4 dificuldades × 2 papéis, o mod pode fixar até 8 alvos paralelos de PMC (+ `impossible` de Scav) — ou seja, a pré-busca por onda é exatamente o mecanismo GROW que o AUD-01-04 quer eliminar, e a mudança de `ChooseProfile` (que reduz os misses) **não** desliga esse reabastecimento.
3. **`Initial Profile Preload` faz outra coisa do que o tooltip diz:** não é "quantos perfis o mod pede no início", é o **nível de cache permanente de USEC/BEAR em dificuldade `normal`** que o jogo mantém a raid inteira; e `0` não "desliga a pré-carga" — deixa os PMCs sem alvo de backup, então **cada** vaga PMC vira miss → `LoadBots(3)` síncrono por bot (`BotsPresets.cs:170-189`), o oposto do trade-off declarado em AC-X2 ("primeira onda pode esperar um `bot/generate` a mais").

**Por que importa:** AC-M4 (`profilesInList` ≤ +50 e `bot/generate` ≤ 2 × bots) pode falhar na V2 por uma causa que o código não enxerga, e a conclusão seria "a escolha tolerante não bastou" quando o reabastecimento por alvo é quem gera. A propriedade F12 nova entra no `PROPRIEDADES.md` com semântica errada (AP-05) e o comentário de `:556-557` ensina o modelo errado ao próximo dev. Nada disso é regressão (o mecanismo já era esse na 3.3.0), por isso 🟠 e não 🔴.

**Sugestão:**
1. **Remover o bloco `:804-812`** (pré-busca por onda): para Scav é no-op; para PMC fixa alvos permanentes por dificuldade. Com o `ChooseProfilePatch` tolerante, o cache `normal` de PMC (alvo = `initialProfilePreload`) atende qualquer dificuldade da onda; só o caso "nenhum USEC/BEAR em cache" fabrica 3 (AC-X5), que é o comportamento desejado. Se quiser manter um alvo por dificuldade sorteada **sem SAIN**, registrar uma única vez por raid, com valor pequeno (ex.: 3), e documentar que é permanente.
2. **Corrigir a semântica documentada** em `Settings.cs:128`, `PROPRIEDADES.md` e no comentário `:147-148`: "Nível de cache de perfis PMC (USEC e BEAR, dificuldade normal) que o jogo mantém reposto durante toda a raid (reposição a cada ~30 s quando o cache cai abaixo do nível − 3). Scavs são governados pelo vanilla (8 por dificuldade). 0 = sem cache de PMC: cada PMC gera 3 perfis no momento do spawn (primeira onda mais lenta)". Trocar a faixa mínima para 1 ou aceitar 0 com esse aviso. Remover a chamada `assault` de `:155` (ou manter com comentário `// no-op: vanilla registers assault easy/normal/hard = 8 (GClass684.cs:113-115)`) e corrigir o log de `:150`.
3. **V2:** além de `profilesInList`, contar `bot/generate` por origem — o SPT loga `byBackup`; se o número de `byBackup` cair com o passo 1, o achado fecha; se não cair, o próximo suspeito é o vanilla `assault` 8/8/8 (fora do mod).
4. Registrar na 01-spec (AC-X2) a semântica corrigida e, na spec técnica §9 check 9, as linhas `GClass684.cs:113-118/:129-192/:258-263` (AP-09: "pedir N perfis" era recon, não fato).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[x]` Aceitar com modificação: passos 2, 3 e 4 integrais; passo 1 **parcial** — o bloco por onda fica, porque o número de chaves é limitado (2 facções × 4 dificuldades) e é ele que mantém a feature de dificuldade do painel sem SAIN; ajustes: só roda sem SAIN, Scav só registra `impossible` (easy/normal/hard são chaves vanilla → no-op), log renomeado para "standing backup targets". Faixa mínima 5 (não 0/1): sem cache PMC cada vaga vira `LoadBots(3)` síncrono.
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** semântica de nível permanente documentada no código, no `Settings.cs`, no `PROPRIEDADES.md` e na 01-spec (AC-X2); chamada no-op de Scav removida da pré-carga; `GClass684.cs` citado no §9 check 9 da spec técnica. Bloco por onda mantido com as modificações acima.
**Aplicação:** `Client/Components/DynamicSpawnManager.cs` (pré-carga `:143-158`, comentário do `ProcessWave`, bloco por onda), `Client/Helpers/Settings.cs` (tooltip + faixa 5–30), `PROPRIEDADES.md`, `010-…-01-spec.md` (AC-X2), `010-…-02-spec-tech.md` (§9 check 9). `// ref: CR-01-01`.

---

### CR-01-02 · C — Gap vs. spec · 🟠 Forte · ✅ Aplicado em 2026-08-23

**A pausa "sem humano vivo" só é avaliada no topo do `while` interno — a onda em voo continua até `warmupInterval` segundos depois do último humano morrer**

**Local:** [`mods/TRL-DynamicSpawn/Client/Components/DynamicSpawnManager.cs:433-445`](../../Client/Components/DynamicSpawnManager.cs#L433) (checagem) · [`:472-502`](../../Client/Components/DynamicSpawnManager.cs#L472) (timer do warmup sem checagem) · [`:505-518`](../../Client/Components/DynamicSpawnManager.cs#L505) (ESTÁGIO B)

**Problema:** o ramo de pausa (`GetAliveHumanCount() == 0` → `StopCoroutine(_activeWaveCoroutine)`) está em `:438-445`, logo após o `yield return new WaitForSeconds(1f)` do topo. Mas depois de `_activeWaveCoroutine = StartCoroutine(ProcessWave(...))` (`:474`), a coroutine entra no `for (elapsed < warmupInterval)` de `:478-494`, que espera 1 s por volta e só testa o **cap** (`aliveRealBots >= dynamicCap`). O próximo `GetAliveHumanCount()` acontece apenas após `warmupAttempt++` (`:501`) e o novo `WaitForSeconds(1f)` — isto é, **até `warmupInterval` s** (`DelayBeforeFirstWave` por mapa, 30 s default, até 120 s no slider) após a morte. Enquanto isso `ProcessWave` segue: `smoothSpawningDelay` 1,5 s entre grupos (`:1001-1004`) × N grupos + `Create`/`TryToSpawnInZoneAndDelay` por membro (`:1044-1108`) — uma onda de 20 vagas termina inteira nessa janela. Caso comum: solo morre 5 s depois do "Map needs bots … Attempt 1" (`:470`) → a primeira onda nasce completa para ninguém. No ESTÁGIO B (`:516-517`, cooldown de até 600 s) não há checagem, mas ali nada nasce — o efeito é só o HUD contando regressivamente (`_nextWaveTime`, `:514`) com todos mortos.

A 01-spec AC-X3 diz: "o mod **interrompe a onda em andamento** (o grupo que já estava nascendo termina…)". A spec técnica §5 (c) e a resolução de PA-01-05 descrevem o `StopCoroutine` como solução — e ele funciona, mas só chega a rodar quando o loop volta ao topo. AC-M6 ("após o hook de fim de raid, nenhuma linha de onda") continua verdadeiro porque o `Stop`/`OnDestroy` mata tudo; o gap é entre a morte e o hook (em solo, a tela de morte dura alguns segundos; no Fika host, a raid continua enquanto houver guest).

**Por que importa:** é exatamente o cenário que AC-X3 promete cobrir e o que AUD-01-06 mira ("trabalho com a raid acabando/vazia"): `bot/generate` + `ChooseProfile` + spawn de uma onda inteira sem ninguém vivo. Na V2, o log mostrará `SQUAD MEMBER SPAWNED` depois da morte e alguém vai ler como "a pausa não funciona".

**Sugestão:** mover a checagem para dentro do timer do warmup, mantendo a do topo. Em `:480` (logo após o `yield return new WaitForSeconds(1f)` do `for`):

```csharp
if (GetAliveHumanCount() == 0)   // ref: AC-X3 — reach the wave in flight within 1 s, not after warmupInterval
{
    if (_activeWaveCoroutine != null) { StopCoroutine(_activeWaveCoroutine); _activeWaveCoroutine = null; }
    IsGeneratingDynamicWave = false;
    _nextWaveTime = 0f;
    break;   // falls through to warmupAttempt++ → top of the inner while → pause branch (5 s re-check)
}
```

(`capReachedEarly` fica `false`, então o `break` externo de `:496-499` não dispara e o loop volta ao topo, onde o ramo existente assume a pausa.) Opcional, para o HUD honesto no cooldown: trocar o `WaitForSeconds(waitNormal)` de `:517` por um laço de 1 s que zera `_nextWaveTime` quando `GetAliveHumanCount() == 0` — sem efeito em spawn, só cosmético; pode ficar como dívida.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (a parte opcional do HUD no cooldown fica como dívida)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `Client/Components/DynamicSpawnManager.cs` — check de humano dentro do `for` do timer de warmup (`StopCoroutine` + flags + `break`), comentário `// ref: CR-01-02`.

---

### CR-01-03 · E — Legibilidade · 🟢 Menor · ✅ Aplicado em 2026-08-23

**`_isSpawningWave` é código morto e fica preso em `true` quando a pausa interrompe `ProcessWave`**

**Local:** [`mods/TRL-DynamicSpawn/Client/Components/DynamicSpawnManager.cs:24`](../../Client/Components/DynamicSpawnManager.cs#L24) · `:542` · `:562` · `:1011`

**Problema:** grep em `Client/` mostra só **atribuições** (`:24`, `:542`, `:562`, `:1011`) — nenhum leitor. Com o `StopCoroutine(_activeWaveCoroutine)` novo (`:440`) a coroutine morre entre `:542` e `:1011`, e o campo fica `true` até o fim da raid. Hoje é inofensivo (ninguém lê), mas a review técnica 01 (PA-01-06) chegou a sugerir `Instance._isSpawningWave = false` no `StopSpawnLoops` — o próximo dev que "ligar" o campo herda o valor preso.

**Por que importa:** campo com nome sugestivo de gate ("estou spawnando?") que não é gate e não é confiável após este item.

**Sugestão:** remover o campo e as 3 atribuições. Se a intenção futura for um gate de reentrada de onda, o que já existe é `_activeWaveCoroutine != null` (zerado pela pausa e pelo `StopSpawnLoops`).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `Client/Components/DynamicSpawnManager.cs` — campo e 3 atribuições removidos; comentário `// ref: CR-01-03` no topo do `ProcessWave`.

---

### CR-01-04 · E — Legibilidade · 🟢 Menor · ✅ Aplicado em 2026-08-23

**Comentários desatualizados pelo próprio item**

**Local:** [`mods/TRL-DynamicSpawn/Client/Helpers/RaidLifecycle.cs:27-31`](../../Client/Helpers/RaidLifecycle.cs#L27) · [`mods/TRL-DynamicSpawn/Client/Patches/Patches.cs:855`](../../Client/Patches/Patches.cs#L855)

**Problema:**
- `RaidLifecycle.cs:27` ainda cita `// ref: Assembly-CSharp/EFT/BaseLocalGame-1.cs:1018 (Stop)` e `:30-31` diz que o `source` "proves whether the **BaseLocalGame.Stop** patch is alive (PA-01-05)". O item removeu esse patch; as fontes possíveis agora são `LocalGame.Stop` (SPT puro), `CoopGame.Stop` (Fika) e `GameWorld.OnDestroy`, e a expectativa da V2 (PA-02-03) é `CoopGame.Stop` com Fika.
- `Patches.cs:855`: `// same semantics as vanilla :91-94` — em `BotProfileDataClass.cs` o `if (withDelete) { profiles2Select.Remove(profile); }` está em `:93-96` (`:91` é o `}` do `if (list.Count == 0)`).

**Por que importa:** rastreabilidade (AP-09) — o comentário é o que o próximo `/review-technical-spec` vai conferir.

**Sugestão:** `RaidLifecycle.cs:27` → `// ref: Assembly-CSharp/EFT/LocalGame.cs:357 (Stop override), fika-plugin CoopGame.cs:718 (Stop) and EFT/GameWorld.cs:2111 (OnDestroy)`; `:30-31` → `// source: which hook fired — logged once per raid; expected "CoopGame.Stop" with Fika, "LocalGame.Stop" in plain SPT, "GameWorld.OnDestroy" only if neither early hook fired (PA-02-03)`. `Patches.cs:855` → `:93-96`.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `Client/Helpers/RaidLifecycle.cs` (comentário do `OnRaidEnd`), `Client/Patches/Patches.cs` (comentário do `withDelete`). `// ref: CR-01-04`.

---

### CR-01-05 · F — Melhoria opcional · 🟢 Menor · ✅ Aplicado em 2026-08-23

**Emissores Info por onda que continuam fora do gate, e pequenas alocações no check de humano**

**Local:** [`mods/TRL-DynamicSpawn/Client/Components/DynamicSpawnManager.cs:466`](../../Client/Components/DynamicSpawnManager.cs#L466) · `:470` · `:490` · `:513` · `:561` · `:634` · `:652` · `:674` · `:699` · `:708` · `:889` · [`:1705-1753`](../../Client/Components/DynamicSpawnManager.cs#L1705) (`IsHeadlessPlayer`)

**Problema:** AC-M5 lista os emissores por perfil/bot (`Logger`, `SPY`, `SPAWN ->`, `Available profile`, `Horde Breakdown`) e todos estão gated ✓. Sobram, sem gate, as linhas **operacionais por onda** (1 a ~8 por onda, nível Info): "Map needs bots … Attempt N" (`:470`), "Map is 100% full …" (`:466`/`:513`), "Max cap reached early" (`:490`), "No slots available" (`:561`), invasões de elite/rogue/raider (`:652/:674/:699/:708`), "Max dynamic cap reached during smooth spawn" (`:889`). São úteis para ler a cadência da raid e a spec não pede que sumam — registro só para o grep de sanidade do §8 da spec técnica ("nenhum `LogInfo` com prefixo `[TRLDynamicSpawn Logger]`/`[SPY]` fora do gate") ficar documentado como satisfeito **com** essas exceções. Também: `GetAliveHumanCount()` (1×/s) chama `IsHeadlessPlayer(p)` para cada humano, que faz `Nickname?.ToLower()` e `AccountId?.ToLower()` (`:1744-1745`) — 2 strings por humano por segundo (≤10 allocs/s); desprezível, mas é a única alocação nova em loop deste item.

**Por que importa:** nenhum impacto medível; apenas clareza do critério AC-M5 na V2 e higiene do ALLOC.

**Sugestão:** (a) manter os Info por onda como estão e anotar na 01-spec AC-M5 "linhas operacionais por onda (`Map needs bots`, `Map is 100% full`, invasões) continuam sem gate — ≤ 8/onda"; (b) opcional: em `IsHeadlessPlayer` trocar `ToLower()` + `Contains` por `IndexOf("headless", StringComparison.OrdinalIgnoreCase) >= 0` (zero alocação; `csharp-mod-best-practices` §1/§9).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (a e b)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** (a) exceção anotada na 01-spec (complemento do AC-M5); (b) `IsHeadlessPlayer` sem `ToLower()` (`IndexOf` ordinal, zero alocação).
**Aplicação:** `010-…-01-spec.md` (AC-M5), `Client/Components/DynamicSpawnManager.cs` (`IsHeadlessPlayer`). `// ref: CR-01-05`.

---

## Verificações sem achado (registro)

Itens que o `/code-review` foi instruído a caçar e que **batem** — deixados aqui para a próxima rodada não refazer:

- **NR-1 / corner case "onda do mod em andamento":** grep em `Client/` — o mod **nunca** chama `BotsController.ActivateBotsWithoutWave` (único hit é o próprio patch); suas ondas usam `BotCreationDataClass.Create` + `BotSpawner.TryToSpawnInZoneAndDelay` (`DynamicSpawnManager.cs:1046/:1087`, `:1275/:1302`) com `IsGeneratingDynamicWave = true` setado em `try/finally` **síncrono** (`:1080-1097`, `:1299-1307`, sem `yield`), então o backstop antigo (`Patches.cs:546-556`) continua deixando os spawns do mod passar. O check de `IsGeneratingDynamicWave` em `SpawnGatePatches.cs:32` é de fato só defensivo.
- **NR-2:** `EFT/NonWavesSpawnScenario.cs:155-161` sorteia `assault`/`marksman` por vaga (`gclass1881_1.Random()`, pesos `BotAssault`/`BotMarksman` `:167`); `num` (`:131`) não depende de tentativas recusadas, então a frequência de `marksman` não muda. O grupo (`GClass1876.cs:42-52`, NonWaveGroupScenario) é sempre `assault` → recusado ✓. `BotHalloweenEvent.cs:176` contorna (declarado em AC-X6).
- **NR-3:** `BossSpawnScenario`/`ActivateBotsByWave(BossLocationSpawn)` intocados ✓.
- **`ChooseProfilePatch`:** reservoir de passagem única é uniforme (`Random.Range(0, n) == 0` com `n` crescente, independente para "exato" e "relaxado" — `Patches.cs:843-849`) ✓; `__instance.Side` é `EPlayerSide?` (`BotProfileDataClass.cs:43-50`, sempre preenchido pelo construtor `:66`) e `info.Side == side` usa o operador levantado, igual ao vanilla `:87` — com `Side == null` o match não-PMC é vazio → `return true` → vanilla também devolve `null` ✓; `withDelete` remove da mesma lista (`:855` vs vanilla `:93-96`) ✓; PMC: `PmcMatches` = `Side == wanted || Role == requested` preserva o conjunto antigo exceto o degrau "qualquer perfil" (AC-X5) e a heurística `ToString().Contains` para roles `spt*` inexistentes em 0.16.9 (`EFT/WildSpawnType.cs:55-56`) ✓; **não existe** caminho com `return false` e `__result == null` (`chosen == null → return true`) ✓; perfis com `Info`/`Settings` nulos são pulados (o vanilla lançaria NRE) ✓; chamada via `BotsPresets.CreateProfile` `:189` após `LoadBots(3)` encontra match exato ✓.
- **`SpawnHordeLoop` pausa:** `continue` pula `warmupAttempt++` e o `break` de cap — loop de 5 s até `StopSpawnLoops`/destruição do componente, intencional (AC-X3); não há starvation porque humano morto não ressuscita (Fika não tem join-in-progress). `StopCoroutine` de um `Coroutine` já concluído é no-op ✓. `_activeWaveCoroutine` também é zerado em `StopSpawnLoops` ✓. O gap de alcance está em CR-01-02.
- **`ClearSptQueue` 1×/raid:** `_sptQueueClearedThisRaid` é campo de instância e o componente é recriado por raid (`DynamicSpawnManagerPatch.cs:32-35/:60`) ✓; roda após o check de humano e antes da primeira `ProcessWave` — nenhum `Create` do mod em voo nesse instante (`RequestReplacementBot` é código morto) ✓; `AddToTargetBackup` não cria `BotCreationDataClass` (`GClass684.cs:258-263`), então a pré-carga inicial não é cancelada ✓. `BotEventHandler.StopBotSpawn()` só invoca `OnStopBotSpawn` (`BotEventHandler.cs:1252-1255`) — sem flag persistente que bloqueie spawns futuros ✓.
- **`StopSpawnLoops`:** `Instance == null` usa o `==` sobrecarregado de `UnityEngine.Object` (fake-null após `Destroy`) ✓; flags estáticas antes do early-return ✓; idempotente (segunda chamada é no-op pela `_raidActive` de `RaidLifecycle.OnRaidEnd` `:34`, e `StopAllCoroutines` em componente vivo sem coroutines é no-op) ✓; `FetchServerConfigAndStart` não tem `yield` antes de atribuir `_serverConfig` (`:57-178` — HTTP síncrono via `RequestHandler.GetJson`), então um `StopAllCoroutines` nunca deixa `_serverConfig` nulo para `ServerConfig`/`IsValidSpawnZone` ✓.
- **`CoopGameStopPatch`:** `static readonly Type TargetType` é inicializado no primeiro acesso ao tipo (`Plugin.cs:48`), dentro de `Plugin.Start` — Unity chama `Start` após o chainloader ter adicionado todos os plugins (e `SoftDependency("com.fika.core")` garante ordem); `AccessTools.TypeByName` devolve `null` sem lançar quando o tipo não existe, e o `Enable()` fica atrás do guard ✓. Assinatura `Stop(string, ExitStatus, string, float)` confere com `CoopGame.cs:718`; `CoopGame.Stop` não chama `base.Stop` (encerra via `ExitManager` `:811-818`) — por isso o hook genérico do 009 era inerte ✓. `LocalGame.Stop` (`EFT/LocalGame.cs:357-363`) é o único overload ✓.
- **Fika headless:** `GetAliveHumanCount` exclui o "player" headless via `IsHeadlessPlayer` (`Application.isBatchMode` + `IsYourPlayer`, `:1709-1711`) e bots via `IsAI` (`EFT/Player.cs:25135` → `AIData.IsAI`); guests (`ObservedCoopPlayer`) contam ✓. No headless, a raid só começa com os guests carregados (`OnGameStarted` depois do lobby), então "0 humanos no warmup" só acontece quando todos morreram/extraíram — e aí o Fika encerra a raid; a pausa é o comportamento desejado ✓. Host não-headless conta o próprio player ✓ (NR-6).
- **AUD-01-07 (grep `Log(Warning|Info)` em `Client/`):** todo emissor por perfil/bot está gated antes de formatar (`Patches.cs:815-826/:857-858`, `BotSpawnLoggerPatch.cs:19`, `DynamicSpawnManager.cs:1032/:1078`, `SpawnGatePatches.cs:38`); avisos reais continuam sem gate (`MASTER FALLBACK` `Patches.cs:769`, `FAILED` `:998/:1311`, `SPY-FALLBACK` `:1066`, `Member safely skipped` `:1101`) ✓ NR-7. Restam os Info por onda de CR-01-05.
- **Backstop `TryToSpawnInZoneAndDelayPatch` (`Patches.cs:546-556`):** continua gated e é a string que AC-M1 conta ("Blocked Vanilla Assault Scav Spawn" = 0) — **não renomear** antes da V2.
- **Estado entre raids:** `IsGeneratingDynamicWave`/`IsWarmupActive` resetados em `StopSpawnLoops` ✓; `_sptQueueClearedThisRaid`/`_activeWaveCoroutine` morrem com o componente ✓; `Instance` stale de uma raid cujo hook de fim falhou é fake-null na próxima chamada ✓.
- **Config:** `initialProfilePreload` bind após `Settings.Init` (`Plugin.cs:29`) e lido em `FetchServerConfigAndStart` ✓; `ConfigurationManagerAttributes.IsAdvanced` existe ✓; `PROPRIEDADES.md` atualizado ✓. Versão ainda `3.3.0` em `Plugin.cs:12` — o bump para 3.4.0 é o passo não marcado do §8 da spec (`/compile-mod`), não achado.
- **Readiness 4.1:** o diff não pina nenhum `GClassNNNN` em `typeof`/`AccessTools`; `GClass1876`/`GClass1890` aparecem só em comentários com o conceito nomeado ✓.

### Como conferir cada NR na V2 (rastreabilidade da skill de performance, check 10)

| NR | Evidência no log / in-game |
|---|---|
| NR-1 | Contagem de `SQUAD MEMBER SPAWNED` por papel e zonas (debug on) igual à V1 para o mesmo preset/mapa |
| NR-2 | `SPAWN -> Role: marksman` fora das ondas do mod ≥ 1 em mapa com `BotMarksman > 0`; nenhum `Refused vanilla continuous spawn (marksman)` |
| NR-3 | `Configured vanilla boss wave …` e spawns de boss/guard iguais à V1 |
| NR-4 | `CHOSEN PROFILE … for pmcUSEC (hard)` sem `[difficulty relaxed]` quando o pool tem `hard`; com `[difficulty relaxed]` só após miss |
| NR-5 | `Clearing pending/stuck bot profile creation queue` exatamente 1×/raid, depois de `Initial Ns elapsed` |
| NR-6 | Fika host morto + guest vivo: `Map needs bots … Attempt N` continua |
| NR-7 | Com debug on, as mesmas linhas da V1 em nível Info; `MASTER FALLBACK`/`FAILED` presentes com debug off |
| NR-8 | Com SAIN: `Sampled Wave Difficulties (SAIN Active: True): PMC=normal, Scav=normal` |
| AC-M6 / AC-X3 | Nenhum `SQUAD SPAWN INITIATED` após `Raid end hook fired`; após a morte do último humano, no máximo 1 grupo (CR-01-02 aplicado) |

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-08-23 | Code review 01 criada via `/code-review` (revisor independente) |
| 2026-08-23 | Aplicação automática de 5 achados via `/apply-code-review` — IDs aplicados: CR-01-01 (com modificação), CR-01-02, CR-01-03, CR-01-04, CR-01-05; rejeitados: nenhum |
