# 010 — perf-spawn-pipeline-r2 · Spec Técnica

**Mod:** TRL-DynamicSpawn
**Spec funcional:** [010-perf-spawn-pipeline-r2-01-spec.md](010-perf-spawn-pipeline-r2-01-spec.md)
**Criado:** 2026-08-22T23:46:00-03:00

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.
>
> **Perfil: plano de otimização** (rodada 2). Fonte editável: `Client/`. Todas as linhas do dump abaixo foram reconfirmadas em 2026-08-22 (AP-09).

## 1. Estratégia

| Achado | Problema | Mecanismo de impacto (eixos §1 da skill) | Solução | Classe |
|---|---|---|---|---|
| **AUD-01-08** | vanilla `NonWavesSpawnScenario.Update` (`NonWavesSpawnScenario.cs:115-159`, período ≥10 s `:32-34/:146-148`) tenta preencher `BotMax` a cada tick; o mod só recusa em `TryToSpawnInZoneAndDelay`, **depois** de `Create`+`ChooseProfile` | per-10 s × (cap − vivos) entidades × raid inteira; custo unitário alto (varredura de pool de centenas + log) | **recusar no primeiro passo**: prefix em `BotsController.ActivateBotsWithoutWave(int, IGetProfileData)` (`BotsController.cs:536-542`) retornando `false` para `assault`/`cursedAssault` no host. `marksman` passa (NR-2). O prefix antigo vira backstop | eliminar trabalho |
| **AUD-01-04** | `ChooseProfile` exige dificuldade exata (`BotProfileDataClass.cs:85-96`); miss → `LoadBots(3)` (`BotsPresets.cs:170-189`) → perfis órfãos; pré-carga 30/30/20 + 10/10/10 por onda | per-wave × raid inteira; **acúmulo** (GROW) → RAM | `ChooseProfilePatch` resolve **todos** os papéis: exato → mesmo Side+Role qualquer dificuldade → vanilla; remover pré-carga fixa por onda; pré-carga inicial via `ConfigEntry` (default 15) | eliminar trabalho + configuração |
| **AUD-01-05** | `ClearSptQueue()` 1×/s no warmup (`DynamicSpawnManager.cs:392`) cancela todo `BotCreationDataClass` (`BotCreationDataClass.cs:116/142-146`) → `Create` retorna `null` (`:102-105`) → NRE vanilla (`GClass1890.cs:15`) | per-1 s × warmup inteiro; 44 NRE/raid; cancela criações do próprio mod | chamar **uma vez** por sessão de warmup (antes do `while` interno), nunca no loop | reduzir frequência → corrigir lifecycle |
| **AUD-01-06** | coroutines de spawn só morrem com o `GameWorld`; onda calculada mesmo sem humano vivo | per-wave além do fim útil da raid | `RaidLifecycle.OnRaidEnd` → `DynamicSpawnManager.StopSpawnLoops()`; `SpawnHordeLoop` pausa quando `GetAliveHumanCount() == 0` | corrigir lifecycle |
| **AUD-01-07** | ~1.900 linhas Warning/raid sem gate; string formatada antes do check | per-profile-choice × pool; escrita síncrona no console | gate `Settings.enableDebugLogs.Value` **antes** de formatar; nível Info | LOG |
| **PA-01-05** | patch em `BaseLocalGame<>.Stop` inerte (V1) | — | remover `BaseLocalGameStopPatch`; `OnDestroy` é o hook | limpeza |

**Alternativas descartadas:**
- *Prefix em `NonWavesSpawnScenario.Run()` retornando `false`* (proposta original do AUD-01-08): zera o tick, mas **mata os snipers vanilla** (`gclass1881_1` sorteia `assault`/`marksman`, `NonWavesSpawnScenario.cs:167`) — quebraria NR-2. Recusar em `ActivateBotsWithoutWave` custa um branch por vaga a cada 10 s e preserva `marksman`.
- *Normalizar a dificuldade pedida para `normal` sempre:* mata a feature de dificuldade do painel (sem SAIN). A escolha tolerante só age no miss.
- *Remover `ClearSptQueue` de vez:* preferível tecnicamente, mas o item 006 o introduziu para um sintoma real ("fila presa"); manter 1× preserva a intenção e a V2 decide se some.
- *Patchear `WavesSpawnScenario.Run`:* é `async Task` awaitado em `LocalGame.cs:187-188` — prefix que pula o original devolve `null` e quebra o start. Fica no prefix de `ActivateBotsByWave` (17/raid).

## 2. Pontos de patch

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`EFT/BotsController.cs:536`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/BotsController.cs#L536) `public void ActivateBotsWithoutWave(int count, IGetProfileData data)` | Prefix (`bool`) | **Novo** (AUD-01-08). Host/solo + `data is BotProfileDataClass` com `WildSpawnType_0 ∈ {assault, cursedAssault}` + `!IsGeneratingDynamicWave` → `return false` (o original, que chama `BotSpawner_1.ActivateBotsWithoutWave` `:540`, não roda). Chamado por `NonWavesSpawnScenario.cs:157`. Único caller no dump (grep `ActivateBotsWithoutWave(`): `NonWavesSpawnScenario` — método não-virtual. |
| [`BotProfileDataClass.cs:85`](../../../../references/eft-decompiled/Assembly-CSharp/BotProfileDataClass.cs#L85) `public Profile ChooseProfile(List<Profile>, bool withDelete)` | Prefix (existente, estendido) | AUD-01-04/07. Match exato → Side+Role → vanilla. Logs gated. Não-virtual; chamado via `BotCreationDataClass.ChooseProfile` (`BotCreationDataClass.cs:125-128`) e `BotsPresets.cs:189`. |
| [`EFT/BotOwner.cs`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/BotOwner.cs) `BotOwner.Create` | Postfix (existente) | AUD-01-07: só gate + nível (sem mudança de alvo). |
| `EFT/BaseLocalGame-1.cs:1018` `Stop` | **Remover** patch | PA-01-05 — inerte na V1. |
| `EFT/GameWorld.cs:2584` / `:2111` | existentes (009) | `OnRaidEnd` ganha `DynamicSpawnManager.StopSpawnLoops()`. |

## 3. Novas propriedades F12 (BepInEx)

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `Profile Pool (Advanced)` | `Initial Profile Preload` | int | `15` | 0 a 30 | sim | Quantos perfis de bot por tipo (USEC, BEAR, Scav) o mod pede ao servidor no início da raid, antes da primeira onda. Valores altos aceleram a primeira onda ao custo de memória; 0 desliga a pré-carga inicial. |

Semântica: lido 1× em `FetchServerConfigAndStart` (`:147-149`); estado neutro = default. Rogue (+10) e Goons (+5×3) continuam fixos (condicionais à config de elite). `PROPRIEDADES.md` ganha a seção.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `Client/Patches/SpawnGatePatches.cs` | CRIAR | `ActivateBotsWithoutWavePatch` (prefix) — `// ref: AUD-01-08` |
| `Client/Patches/Patches.cs` | MODIFICAR | `ChooseProfilePatch`: resolução tolerante para todos os papéis + logs gated — `// ref: AUD-01-04`, `AUD-01-07` |
| `Client/Patches/BotSpawnLoggerPatch.cs` | MODIFICAR | gate antes de formatar; `LogInfo` — `// ref: AUD-01-07` |
| `Client/Patches/RaidLifecyclePatches.cs` | MODIFICAR | remover `BaseLocalGameStopPatch` — `// ref: PA-01-05` |
| `Client/Helpers/RaidLifecycle.cs` | MODIFICAR | `OnRaidEnd` chama `DynamicSpawnManager.StopSpawnLoops()` — `// ref: AUD-01-06` |
| `Client/Components/DynamicSpawnManager.cs` | MODIFICAR | `ClearSptQueue` 1×/warmup (`:392` → antes do `while`); remover pré-carga fixa `:496-498`; pré-carga inicial via config `:147-149`; `GetAliveHumanCount()` + pausa no `SpawnHordeLoop`; `StopSpawnLoops()`; gate nos `[SPY]`/`Horde Breakdown` — `// ref: AUD-01-04/05/06/07` |
| `Client/Helpers/Settings.cs` | MODIFICAR | `initialProfilePreload` |
| `Client/Plugin.cs` | MODIFICAR | registrar `ActivateBotsWithoutWavePatch`; desregistrar `BaseLocalGameStopPatch` |
| `PROPRIEDADES.md` | MODIFICAR | seção `Profile Pool (Advanced)` |

## 5. Stubs de código

```csharp
// Client/Patches/SpawnGatePatches.cs
using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using TRLDynamicSpawn.Components;
using TRLDynamicSpawn.Helpers;

namespace TRLDynamicSpawn.Patches
{
    /// <summary>
    /// Refuses the vanilla continuous scav spawner (NonWavesSpawnScenario.cs:157) at its FIRST step, before any
    /// profile is created/chosen. Previously the refusal happened in TryToSpawnInZoneAndDelay, after
    /// BotCreationDataClass.Create + ChooseProfile had already run (the 10 s metronome measured in V1).
    /// marksman (vanilla snipers) is deliberately allowed through (NR-2).
    /// ref: Assembly-CSharp/EFT/BotsController.cs:536 (public void ActivateBotsWithoutWave(int count, IGetProfileData data))
    /// ref: AUD-01-08
    /// </summary>
    public class ActivateBotsWithoutWavePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(BotsController), nameof(BotsController.ActivateBotsWithoutWave), new[] { typeof(int), typeof(IGetProfileData) });

        [PatchPrefix]
        private static bool Prefix(IGetProfileData data)
        {
            try
            {
                if (FikaHelper.IsClient()) return true;                         // guest: vanilla untouched
                if (DynamicSpawnManager.IsGeneratingDynamicWave) return true;   // defensive; the mod does not use this entry
                if (!(data is BotProfileDataClass bp)) return true;            // unknown provider: let vanilla decide

                var role = bp.WildSpawnType_0;                                  // ref: BotProfileDataClass.cs (public property, already used at Patches.cs:804)
                if (role != WildSpawnType.assault && role != WildSpawnType.cursedAssault) return true;

                if (Settings.enableDebugLogs.Value)                             // gate BEFORE formatting (AUD-01-07)
                    Plugin.LogSource.LogInfo($"[TRLDynamicSpawn] Refused vanilla continuous spawn ({role}) before profile creation.");
                return false;
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] ActivateBotsWithoutWavePatch: {ex}");
                return true;
            }
        }
    }
}
```

```csharp
// Client/Patches/Patches.cs — ChooseProfilePatch (substitui :799-853)  ref: AUD-01-04, AUD-01-07
[PatchPrefix]
private static bool PatchPrefix(ref Profile __result, BotProfileDataClass __instance, List<Profile> profiles2Select, bool withDelete)
{
    if (__instance == null || profiles2Select == null || profiles2Select.Count == 0) return true;

    var role = __instance.WildSpawnType_0;
    var side = __instance.Side;                     // ref: BotProfileDataClass.cs:87 (Side / WildSpawnType_0 / BotDifficulty_0 usados pelo vanilla)
    var diff = __instance.BotDifficulty_0;
    bool debug = Settings.enableDebugLogs.Value;

    if (debug)
    {
        Plugin.LogSource.LogInfo($"[TRLDynamicSpawn Logger] ChooseProfile CALLED for Role: {role} ({diff}) (profilesInList: {profiles2Select.Count})");
        int sample = Math.Min(5, profiles2Select.Count);
        for (int i = 0; i < sample; i++)
        {
            var p = profiles2Select[i];
            Plugin.LogSource.LogInfo($"   -> Available profile [{i}]: Name='{p?.Nickname}', Side={p?.Info?.Side}, Role={p?.Info?.Settings?.Role}, Diff={p?.Info?.Settings?.BotDifficulty}");
        }
    }

    bool isPmc = role == WildSpawnType.pmcUSEC || role == WildSpawnType.pmcBEAR;
    Profile exact = null, relaxed = null;
    int exactCount = 0, relaxedCount = 0;

    // Single pass, no LINQ: reservoir-style random pick for "exact" (Side+Role+Diff) and "relaxed" (Side+Role, any Diff).
    // PMC keeps today's tolerance (NR-4): any USEC/BEAR profile whose Side OR Role matches the requested faction.
    for (int i = 0; i < profiles2Select.Count; i++)
    {
        var p = profiles2Select[i];
        var info = p?.Info; var st = info?.Settings;
        if (info == null || st == null) continue;

        bool roleMatch = isPmc ? PmcMatches(info, st, role) : (info.Side == side && st.Role == role);
        if (!roleMatch) continue;

        relaxedCount++;
        if (UnityEngine.Random.Range(0, relaxedCount) == 0) relaxed = p;
        if (st.BotDifficulty == diff)
        {
            exactCount++;
            if (UnityEngine.Random.Range(0, exactCount) == 0) exact = p;
        }
    }

    Profile chosen = exact ?? relaxed;           // exact first (NR-4); relaxed = AC-X1 (only on miss)
    if (chosen == null) return true;             // nothing of this Side+Role → vanilla → null → LoadBots(3) (the only case where generating is right)

    if (withDelete) profiles2Select.Remove(chosen);
    __result = chosen;
    if (debug)
        Plugin.LogSource.LogInfo($"[TRLDynamicSpawn Logger] CHOSEN PROFILE: '{chosen.Nickname}' (Side={chosen.Info.Side}, Role={chosen.Info.Settings?.Role}, Diff={chosen.Info.Settings?.BotDifficulty}) for {role} ({diff}){(exact == null ? " [difficulty relaxed]" : "")}");
    return false;
}

private static bool PmcMatches(InfoClass info, ProfileSettingsClass st, WildSpawnType requested)   // tipos: os mesmos de Profile.Info / Info.Settings já usados em :826-830
{
    bool isUsec = requested == WildSpawnType.pmcUSEC;
    if (info.Side == (isUsec ? EPlayerSide.Usec : EPlayerSide.Bear)) return true;
    if (st.Role == requested) return true;
    return isUsec ? st.Role == WildSpawnType.pmcUSEC : st.Role == WildSpawnType.pmcBEAR;   // substitui o ToString().Contains() de :829-830
}
```

```csharp
// Client/Components/DynamicSpawnManager.cs — trechos  ref: AUD-01-04/05/06/07

// (a) pré-carga inicial (:147-149) — AUD-01-04 / AC-X2
int preload = Settings.initialProfilePreload.Value;
if (preload > 0)
{
    _botCreator.AddToTargetBackup(BotDifficulty.normal, WildSpawnType.pmcUSEC, preload);
    _botCreator.AddToTargetBackup(BotDifficulty.normal, WildSpawnType.pmcBEAR, preload);
    _botCreator.AddToTargetBackup(BotDifficulty.normal, WildSpawnType.assault, preload);
}
// (b) ProcessWave :494-499 — REMOVER o bloco fixo 10/10/10 (a pré-busca por vagas em :744-751 já pede a dificuldade da onda)

// (c) SpawnHordeLoop ESTÁGIO A — AUD-01-05: uma limpeza por sessão de warmup, fora do loop
if (aliveRealBots < dynamicCap)
{
    ClearSptQueue();                 // was inside the while → every 1 s (44 NRE/raid)
    int warmupAttempt = 1;
    while (true)
    {
        yield return new WaitForSeconds(1f);
        // AUD-01-06 / AC-X3: no human alive → nothing to spawn for; re-check instead of waving
        if (GetAliveHumanCount() == 0) { yield return new WaitForSeconds(5f); continue; }
        // ... resto INALTERADO ...

// (d) helper — conta humanos vivos (host, guests), ignora bots e o player headless
public int GetAliveHumanCount()
{
    if (_gameWorld == null) return 0;
    var list = _gameWorld.AllAlivePlayersList;     // ref: EFT/GameWorld.cs:556
    int n = 0;
    for (int i = 0; i < list.Count; i++)
    {
        var p = list[i];
        if (p == null || p.IsAI || IsHeadlessPlayer(p)) continue;
        if (p.HealthController != null && p.HealthController.IsAlive) n++;
    }
    return n;
}

// (e) fim de raid — AUD-01-06 (chamado por RaidLifecycle.OnRaidEnd)
public static void StopSpawnLoops()
{
    if (Instance == null) return;
    Instance.StopAllCoroutines();    // SpawnHordeLoop, ProcessWave, SpawnGroupBotsCoroutine, replacement
    Instance._activeWaveCoroutine = null;
    IsGeneratingDynamicWave = false; // finally{} of a stopped coroutine does not run
    IsWarmupActive = false;
}

// (f) AUD-01-07 — cada [SPY]/Horde Breakdown: `if (Settings.enableDebugLogs.Value) Plugin.LogSource.LogInfo(...)`
```

```csharp
// Client/Patches/BotSpawnLoggerPatch.cs — ref: AUD-01-07
[PatchPostfix]
private static void PatchPostfix(Player player)
{
    if (!TRLDynamicSpawn.Helpers.Settings.enableDebugLogs.Value) return;   // gate before any formatting
    if (player == null || !player.IsAI) return;
    Plugin.LogSource.LogInfo($"[TRLDynamicSpawn Logger] SPAWN -> Role: {player.Profile?.Info?.Settings?.Role.ToString() ?? "UnknownRole"} | Name: {player.Profile?.Nickname ?? "UnknownName"}");
}
```

```csharp
// Client/Helpers/RaidLifecycle.cs — OnRaidEnd (ref: AUD-01-06)
public static void OnRaidEnd(string source)
{
    if (!_raidActive) return;
    _raidActive = false;
    Plugin.LogSource?.LogInfo($"[TRL-DynamicSpawn] Raid end hook fired ({source}).");
    BotDespawnManager.StopLoop();
    DynamicSpawnManager.StopSpawnLoops();   // ref: AUD-01-06
}
// Client/Patches/RaidLifecyclePatches.cs — remover BaseLocalGameStopPatch (PA-01-05); Plugin.cs — remover o Enable() correspondente.
```

```csharp
// Client/Helpers/Settings.cs — adição
public static ConfigEntry<int> initialProfilePreload;
// em Init():
string poolSection = "Profile Pool (Advanced)";
initialProfilePreload = config.Bind(poolSection, "Initial Profile Preload", 15,
    new ConfigDescription("How many bot profiles per type (USEC, BEAR, Scav) the mod asks the server for at raid start, before the first wave. Higher = faster first wave, more memory; 0 disables the initial preload.",
        new AcceptableValueRange<int>(0, 30), new ConfigurationManagerAttributes { IsAdvanced = true }));
```

## 6. Fluxo de dados

```
[vanilla, a cada ≥10 s]  NonWavesSpawnScenario.Update (:115) → num = BotMax − vivos → TrySpawn(num)
    → para cada vaga: botsController.ActivateBotsWithoutWave(1, BotProfileDataClass{Savage, assault|marksman, diff})  (:157)
         ├─ assault/cursedAssault (host) ──► ActivateBotsWithoutWavePatch: return false   ← zero Create / ChooseProfile / log   (AUD-01-08)
         └─ marksman ──► vanilla: BotSpawner.ActivateBotsWithoutWave → Create → ChooseProfile (patch: exato→relaxado, logs gated) → TryToSpawnInZoneAndDelay (prefix antigo: passa)

[mod, por onda]  SpawnHordeLoop (humano vivo? senão espera 5 s) → [1ª vez por warmup] ClearSptQueue() → ProcessWave
    → AddToTargetBackup(diff da onda, slots)  (pré-busca certa; sem o 10/10/10 fixo)
    → SpawnGroupBotsCoroutine → Create(BotProfileDataClass{side, role, diff}) → ChooseProfile
         ├─ exato no pool → igual a hoje (NR-4)
         ├─ miss de dificuldade → perfil Side+Role em outra dificuldade (AC-X1) — sem LoadBots(3)
         └─ nada do Side+Role → vanilla → LoadBots(3)  (único caso de fabricar)
    → TryToSpawnInZoneAndDelay (IsGeneratingDynamicWave = true → passa)

[fim de raid]  GameWorld.OnDestroy → RaidLifecycle.OnWorldDestroyed → OnRaidEnd("GameWorld.OnDestroy")
    → BotDespawnManager.StopLoop() · DynamicSpawnManager.StopSpawnLoops() (coroutines + flags estáticas) · ForceRefresh()
```

## 7. Riscos e dependências

- **Patches existentes no mesmo alvo:** `ChooseProfilePatch` (substituído, mesmo alvo); `TryToSpawnInZoneAndDelayPatch` (mantido como backstop — agora quase sem tráfego de `assault`); `DisableVanillaWavesPatch`/`DisableVanillaBossWavesPatch` (inalterados). Nenhum outro mod do repo patcheia `ActivateBotsWithoutWave` (grep em `mods/**/*.cs`: 0 ocorrências fora deste mod).
- **Compatibilidade:** SAIN (dificuldade: o mod já força `normal` com SAIN; AC-X1 irrelevante); MoreBotsAPI (presente no log — expõe API de spawn própria, não passa por `ActivateBotsWithoutWave`; conferir na V2 que seus spawns seguem nascendo); Fika (guest: todos os novos pontos `return true/cedo`; headless = host).
- **Regressão funcional por achado:** AUD-01-08 — snipers vanilla preservados (marksman passa); se algum mapa depender do spawner contínuo para **assault** (não é o caso: o mod já recusava 100%), nada muda. AUD-01-04 — AC-X1 só no miss; **risco**: perfil "relaxado" de dificuldade diferente aparece no `[SPY-FALLBACK]`? Não — o fallback (`:1001-1011`) só roda quando `Create` devolve `null`, que agora é mais raro. AUD-01-05 — se a "fila presa" do 006 voltar, a V2 mostra (`Map needs bots … Attempt N` crescendo sem `SQUAD MEMBER SPAWNED`). AUD-01-06 — `StopAllCoroutines` também para `WaitForBotGameAndInjectCoroutine`? Não: essa roda no `GameWorld` (`DynamicSpawnManagerPatch.cs:44`), não no manager. AUD-01-07 — nenhuma perda: gate = mesma config de hoje.
- **Ordem de inicialização:** `Settings.Init` antes dos patches (já é). `initialProfilePreload` lido em `FetchServerConfigAndStart` (após `Init`).
- **Estado estático (inventário):** `IsGeneratingDynamicWave`, `IsWarmupActive` — resetados em `StopSpawnLoops` (antes: ficavam com o último valor até a próxima raid). `Instance` — reatribuído em `Init` por raid (inalterado).

## 8. Checklist de implementação

- [ ] `SpawnGatePatches.cs` novo + registro no `Plugin.cs` (`// ref: AUD-01-08`).
- [ ] `ChooseProfilePatch` reescrito (exato → relaxado → vanilla; sem LINQ; logs gated/Info) (`// ref: AUD-01-04`, `AUD-01-07`).
- [ ] `DynamicSpawnManager`: pré-carga inicial via config; remover 10/10/10 de `ProcessWave`; `ClearSptQueue` 1×/warmup; `GetAliveHumanCount` + pausa; `StopSpawnLoops`; gate nos `[SPY]`/`Horde Breakdown`/`Batch profile pre-fetching`/`SQUAD …` (`// ref: AUD-01-04/05/06/07`).
- [ ] `BotSpawnLoggerPatch` gated (`// ref: AUD-01-07`).
- [ ] `RaidLifecycle.OnRaidEnd` → `StopSpawnLoops()`; remover `BaseLocalGameStopPatch` + `Enable()` (`// ref: AUD-01-06`, `PA-01-05`).
- [ ] `Settings.initialProfilePreload` + `PROPRIEDADES.md`.
- [ ] Grep de sanidade: nenhum `LogWarning`/`LogInfo` com prefixo `[TRLDynamicSpawn Logger]`/`[SPY]` fora de `if (Settings.enableDebugLogs.Value)`; `MASTER FALLBACK`/`FAILED`/`Error` continuam sem gate.
- [ ] Compilar (`dotnet build` com refs temporárias, como no 009) → versão **minor** (3.3.0 → 3.4.0: propriedade F12 nova + AC-X1/X2) → deploy com rollback `.bak-3.3.0` → V2.

## 9. Conformidade com skills (auto-checklist)

> Preenchido pelo `/create-technical-spec` ANTES de salvar. Cada linha: ✅ com evidência (seção desta spec ou `arquivo:linha`), ou **N/A + razão**. Linha ❌ → a spec não está pronta. Validado pelo `/review-technical-spec`. Taxonomia: [docs/technical/spt-antipatterns.md](../../../../docs/technical/spt-antipatterns.md).

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes (`GameWorld.OnDestroy` + `BaseLocalGame.Stop`) — AP-01 | ✅ | Hooks do 009 reaproveitados (`GameWorld.cs:2584/:2111`); `OnRaidEnd` idempotente ganha `StopSpawnLoops()` (§5 e). `BaseLocalGame.Stop` **removido** com evidência de que é inerte (V1: fonte logada `GameWorld.OnDestroy`; `ClientGameWorld.cs:222` chama base) — AC-X4. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | `ActivateBotsWithoutWavePatch` e `ChooseProfilePatch` não reagem a ação de player; ambos `return true` em `FikaHelper.IsClient()` (§5) — no guest o vanilla fica intacto. `GetAliveHumanCount` ignora bots e headless. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | ✅ | `BotsController.ActivateBotsWithoutWave` (`:536`, não-virtual, 1 caller: `NonWavesSpawnScenario.cs:157`); `BotProfileDataClass.ChooseProfile` (`:85`, não-virtual; callers `BotCreationDataClass.cs:127`, `BotsPresets.cs:189`). Nenhum `GClass` novo pinado; `GClass1890.cs:15` citado só como evidência da NRE. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | Sem alteração de estado do EFT fora das APIs já usadas (`AddToTargetBackup`, `StopBotSpawn` 1×). Recusa em `ActivateBotsWithoutWave` = pular o original (side-effect: `BotSpawner_1.ActivateBotsWithoutWave` não roda — exatamente o objetivo). |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | `StopSpawnLoops` reseta `IsGeneratingDynamicWave`/`IsWarmupActive` (§7 inventário); coroutines param no `OnDestroy`; pool é por raid (`BotsPresets` criado em `LocalGame.cs:158`). |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | ✅ | §3: `Initial Profile Preload` int 15, 0–30, Avançado, 0 = desliga; lido 1× por raid. |
| 7 | Re-invocação de método patcheado tem reentry-guard/`ReversePatch` (sem recursão infinita) — AP-07 | N/A | Nenhum patch re-invoca o alvo. |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca — AP-08 | ✅ | `IsGeneratingDynamicWave` é setada/limpa em `try/finally` por spawn e **forçada a `false`** no `StopSpawnLoops` (coroutine parada não executa `finally`) — §5 e, corner case da 01-spec. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump; "não existe" conferido no `types-index.json` — AP-09 | ✅ | Reconfirmados 2026-08-22: `BotsController.cs:536-542`, `NonWavesSpawnScenario.cs:98-159/:167`, `BotProfileDataClass.cs:85-96`, `BotsPresets.cs:160-190`, `BotCreationDataClass.cs:46/:102-105/:116/:142-146`, `GClass1890.cs:15`, `LocalGame.cs:139-143/:158/:187-194`. |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Item não usa skills do EFT. |
| 11 | Pacote FIKA próprio — AP-11 | N/A | Nenhum `INetSerializable`. |

## Histórico

| Data | Evento |
|---|---|
| 2026-08-22 | Spec técnica criada via `/optimize-mod-performance --fase 2` (plano de otimização AUD-01-04/05/06/07/08 + PA-01-05) |
