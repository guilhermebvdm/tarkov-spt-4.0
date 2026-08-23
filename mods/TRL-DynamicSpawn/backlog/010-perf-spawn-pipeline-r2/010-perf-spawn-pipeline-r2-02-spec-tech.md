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
| **AUD-01-08** | vanilla `EFT/NonWavesSpawnScenario.Update` (`:115-162`, período ≥10 s `:32-34/:146-148`; chamada em `:160`) tenta preencher `BotMax` a cada tick; o mod só recusa em `TryToSpawnInZoneAndDelay`, **depois** de `Create`+`ChooseProfile` | per-10 s × (cap − vivos) entidades × raid inteira; custo unitário alto (varredura de pool de centenas + log) | **recusar no primeiro passo**: prefix em `BotsController.ActivateBotsWithoutWave(int, IGetProfileData)` (`BotsController.cs:536-542`) retornando `false` para `assault`/`cursedAssault` no host. `marksman` passa (NR-2). O prefix antigo vira backstop | eliminar trabalho |
| **AUD-01-04** | `ChooseProfile` exige dificuldade exata (`BotProfileDataClass.cs:85-96`); miss → `LoadBots(3)` (`BotsPresets.cs:170-189`) → perfis órfãos; pré-carga 30/30/20 + 10/10/10 por onda | per-wave × raid inteira; **acúmulo** (GROW) → RAM | `ChooseProfilePatch` resolve **todos** os papéis: exato → mesmo Side+Role qualquer dificuldade → vanilla; remover pré-carga fixa por onda; pré-carga inicial via `ConfigEntry` (default 15) | eliminar trabalho + configuração |
| **AUD-01-05** | `ClearSptQueue()` 1×/s no warmup (`DynamicSpawnManager.cs:392`) cancela todo `BotCreationDataClass` (`BotCreationDataClass.cs:116/142-146`) → `Create` retorna `null` (`:102-105`) → NRE vanilla (`BotSpawner.cs:377` → `GClass1890.cs:17`) | per-1 s × warmup inteiro; 44 NRE/raid; cancela criações do próprio mod | chamar **uma vez por raid** (flag de instância), após o check de humano vivo, nunca no loop (PA-01-07) | reduzir frequência → corrigir lifecycle |
| **AUD-01-06** | coroutines de spawn só morrem com o `GameWorld`; onda calculada mesmo sem humano vivo | per-wave além do fim útil da raid | `RaidLifecycle.OnRaidEnd` → `DynamicSpawnManager.StopSpawnLoops()` (agora também no `Stop` concreto — PA-01-03); `SpawnHordeLoop` pausa quando `GetAliveHumanCount() == 0` **e interrompe a onda em andamento** (PA-01-05) | corrigir lifecycle |
| **AUD-01-07** | ~1.900 linhas Warning/raid sem gate; string formatada antes do check | per-profile-choice × pool; escrita síncrona no console | gate `Settings.enableDebugLogs.Value` **antes** de formatar; nível Info | LOG |
| **PA-01-05 (009)** | patch em `BaseLocalGame<>.Stop` inerte (V1) | — | trocar pelo override concreto `LocalGame.Stop` + `CoopGame.Stop` (soft) — PA-01-03 | corrigir lifecycle |

**Alternativas descartadas:**
- *Prefix em `NonWavesSpawnScenario.Run()` retornando `false`* (proposta original do AUD-01-08): zera o tick, mas **mata os snipers vanilla** (`gclass1881_1` sorteia `assault`/`marksman`, `NonWavesSpawnScenario.cs:167`) — quebraria NR-2. Recusar em `ActivateBotsWithoutWave` custa um branch por vaga a cada 10 s e preserva `marksman`.
- *Normalizar a dificuldade pedida para `normal` sempre:* mata a feature de dificuldade do painel (sem SAIN). A escolha tolerante só age no miss.
- *Remover `ClearSptQueue` de vez:* preferível tecnicamente, mas o item 006 o introduziu para um sintoma real ("fila presa"); manter 1× preserva a intenção e a V2 decide se some.
- *Patchear `WavesSpawnScenario.Run`:* é `async Task` awaitado em `LocalGame.cs:187-188` — prefix que pula o original devolve `null` e quebra o start. Fica no prefix de `ActivateBotsByWave` (17/raid).

## 2. Pontos de patch

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`EFT/BotsController.cs:536`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/BotsController.cs#L536) `public void ActivateBotsWithoutWave(int count, IGetProfileData data)` | Prefix (`bool`) | **Novo** (AUD-01-08). Host/solo + `data is BotProfileDataClass` com `WildSpawnType_0 ∈ {assault, cursedAssault}` + `!IsGeneratingDynamicWave` → `return false` (o original, que chama `BotSpawner_1.ActivateBotsWithoutWave` `:540`, não roda). Método não-virtual. **Callers no dump (PA-01-02):** `EFT/NonWavesSpawnScenario.cs:160` (spawner contínuo), `GClass1876.cs:51` (= *NonWaveGroupScenario*, grupo de `assault` — é a origem das rajadas de 3–8; coberto pelo prefix) e `BotHalloweenEvent.cs:176`, que chama `BotSpawner.ActivateBotsWithoutWave` **direto** e pula este prefix — fica no backstop `TryToSpawnInZoneAndDelay` (evento sazonal; AC-M1 tolera esse caso). |
| [`BotProfileDataClass.cs:85`](../../../../references/eft-decompiled/Assembly-CSharp/BotProfileDataClass.cs#L85) `public Profile ChooseProfile(List<Profile>, bool withDelete)` | Prefix (existente, estendido) | AUD-01-04/07. Match exato → Side+Role → vanilla. Logs gated. Não-virtual; chamado via `BotCreationDataClass.ChooseProfile` (`BotCreationDataClass.cs:125-128`) e `BotsPresets.cs:189`. Superfície usada: `WildSpawnType_0`/`BotDifficulty_0` são **campos** públicos (`:16/:19`), `Side` é `EPlayerSide?` (`:43`) — comparação com `EPlayerSide` compila (lifted), sem `.Value` (PA-01-08). `IGetProfileData` está no namespace global. |
| [`EFT/BotOwner.cs`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/BotOwner.cs) `BotOwner.Create` | Postfix (existente) | AUD-01-07: só gate + nível (sem mudança de alvo). |
| [`EFT/LocalGame.cs:357`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/LocalGame.cs#L357) `public override void Stop(string, ExitStatus, string, float)` | Prefix (**substitui** o de `BaseLocalGame<>`) | PA-01-05/PA-01-03: o patch no genérico fechado foi inerte na V1; `LocalGame` é classe concreta (`:24`), o override chama `base.Stop` (`:362`). Fecha a janela `Stop → OnDestroy` (AUD-01-06). |
| `Fika.Core.Main.GameMode.CoopGame.Stop` (`fika-plugin/.../CoopGame.cs:718`) | Prefix **soft** (tipo resolvido por nome em runtime; sem Fika → patch não registrado) | Mesma janela no host Fika/headless. `AccessTools.TypeByName("Fika.Core.Main.GameMode.CoopGame")`; `null` → `Enable()` não é chamado (sem dependência dura, como `FikaHelper`). |
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
| `Client/Patches/RaidLifecyclePatches.cs` | MODIFICAR | `BaseLocalGameStopPatch` → `LocalGameStopPatch` + `CoopGameStopPatch` (soft) — `// ref: PA-01-05 (009)`, `PA-01-03` |
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

// PA-01-01: tipos reais do dump — Profile.Info é `InfoClass` (EFT/Profile.cs:632); Info.Settings é `ProfileInfoSettingsClass`
// (InfoClass.cs:123; Role/BotDifficulty em ProfileInfoSettingsClass.cs:5-9). `ProfileSettingsClass` NÃO existe.
// PA-01-04: o fallback antigo "qualquer perfil do pool, até Savage" (Patches.cs:834-838) é REMOVIDO de propósito —
// vaga PMC sem perfil USEC/BEAR volta ao vanilla (LoadBots 3) em vez de nascer um Scav fantasiado de PMC (AC-X5).
private static bool PmcMatches(InfoClass info, ProfileInfoSettingsClass st, WildSpawnType requested)
{
    EPlayerSide wantedSide = requested == WildSpawnType.pmcUSEC ? EPlayerSide.Usec : EPlayerSide.Bear;
    return info.Side == wantedSide || st.Role == requested;   // cobre sptUsec/sptBear (Side) e pmcUSEC/pmcBEAR (Role)
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

// (c) SpawnHordeLoop ESTÁGIO A — AUD-01-05 (PA-01-07): UMA limpeza por raid, após o check de humano vivo
private bool _sptQueueClearedThisRaid;   // instance field → new per raid (component is recreated)

if (aliveRealBots < dynamicCap)
{
    int warmupAttempt = 1;
    while (true)
    {
        yield return new WaitForSeconds(1f);

        // AUD-01-06 / AC-X3 (PA-01-05): no human alive → nothing to spawn for. Stop the wave in flight
        // (the child SpawnGroupBotsCoroutine finishes its current group — Unity limitation, documented)
        // and keep re-checking; the raid is ending anyway.
        if (GetAliveHumanCount() == 0)
        {
            if (_activeWaveCoroutine != null) { StopCoroutine(_activeWaveCoroutine); _activeWaveCoroutine = null; }
            IsGeneratingDynamicWave = false;
            _nextWaveTime = 0f;            // HUD/overlay: no wave scheduled
            yield return new WaitForSeconds(5f);
            continue;
        }

        if (!_sptQueueClearedThisRaid)     // item 006 intent preserved: one clean-up, never periodic (was every 1 s → 44 NRE/raid)
        {
            _sptQueueClearedThisRaid = true;
            ClearSptQueue();
        }
        // ... resto INALTERADO (cap check/break, ProcessWave, timer) ...

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
    // PA-01-06: static flags FIRST — called from the GameWorld.OnDestroy prefix, the manager (same GameObject)
    // may already be destroyed (Unity fake-null) and the early return below would skip the reset.
    IsGeneratingDynamicWave = false;
    IsWarmupActive = false;
    if (Instance == null) return;
    Instance.StopAllCoroutines();    // SpawnHordeLoop, FetchServerConfigAndStart, ProcessWave, SpawnGroupBotsCoroutine, SpawnReplacementBotCoroutine
    Instance._activeWaveCoroutine = null;
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
// Client/Patches/RaidLifecyclePatches.cs — PA-01-03: substitui BaseLocalGameStopPatch (inerte na V1)
/// ref: Assembly-CSharp/EFT/LocalGame.cs:357 (public override void Stop — classe concreta, chama base :362)
public class LocalGameStopPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
        => AccessTools.Method(typeof(LocalGame), nameof(LocalGame.Stop));
    [PatchPrefix]
    private static void Prefix()
    {
        try { RaidLifecycle.OnRaidEnd("LocalGame.Stop"); }
        catch (Exception ex) { Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] LocalGameStopPatch: {ex}"); }
    }
}

/// Fika host/headless: CoopGame : BaseLocalGame<EftGamePlayerOwner> (fika-plugin CoopGame.cs:42, override Stop :718).
/// Soft: tipo resolvido por nome; sem Fika instalado, TargetType == null e o Plugin NÃO chama Enable().
public class CoopGameStopPatch : ModulePatch
{
    public static readonly Type TargetType = AccessTools.TypeByName("Fika.Core.Main.GameMode.CoopGame");
    protected override MethodBase GetTargetMethod()
        => AccessTools.Method(TargetType, "Stop", new[] { typeof(string), typeof(ExitStatus), typeof(string), typeof(float) });
    [PatchPrefix]
    private static void Prefix()
    {
        try { RaidLifecycle.OnRaidEnd("CoopGame.Stop"); }
        catch (Exception ex) { Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] CoopGameStopPatch: {ex}"); }
    }
}
// Plugin.cs: new LocalGameStopPatch().Enable(); if (CoopGameStopPatch.TargetType != null) new CoopGameStopPatch().Enable();
// (remover new BaseLocalGameStopPatch().Enable() e a classe)
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
- **Compatibilidade:** SAIN (dificuldade: o mod já força `normal` com SAIN; AC-X1 irrelevante); **MoreBotsAPI — não verificado** (PA-01-09: nenhuma fonte dele em `references/` nem em `mods/`; não se sabe se usa `ActivateBotsWithoutWave`). Sem regressão por construção — o backstop já recusava 100% de `assault` antes deste item — mas a V2 confere explicitamente que spawns do MoreBotsAPI (se houver nesse setup) continuam nascendo; Fika (guest: todos os novos pontos `return true/cedo`; headless = host). `BotHalloweenEvent.cs:176` contorna o prefix (chama `BotSpawner` direto) — sazonal, coberto pelo backstop.
- **Regressão funcional por achado:** AUD-01-08 — snipers vanilla preservados (marksman passa); se algum mapa depender do spawner contínuo para **assault** (não é o caso: o mod já recusava 100%), nada muda. AUD-01-04 — AC-X1 só no miss; **risco**: perfil "relaxado" de dificuldade diferente aparece no `[SPY-FALLBACK]`? Não — o fallback (`:1001-1011`) só roda quando `Create` devolve `null`, que agora é mais raro. AUD-01-05 — se a "fila presa" do 006 voltar, a V2 mostra (`Map needs bots … Attempt N` crescendo sem `SQUAD MEMBER SPAWNED`). AUD-01-06 — `StopAllCoroutines` também para `WaitForBotGameAndInjectCoroutine`? Não: essa roda no `GameWorld` (`DynamicSpawnManagerPatch.cs:44`), não no manager. AUD-01-07 — nenhuma perda: gate = mesma config de hoje.
- **Ordem de inicialização:** `Settings.Init` antes dos patches (já é). `initialProfilePreload` lido em `FetchServerConfigAndStart` (após `Init`).
- **Estado estático (inventário):** `IsGeneratingDynamicWave`, `IsWarmupActive` — resetados em `StopSpawnLoops` (antes: ficavam com o último valor até a próxima raid). `Instance` — reatribuído em `Init` por raid (inalterado).

## 8. Checklist de implementação

- [ ] `SpawnGatePatches.cs` novo + registro no `Plugin.cs` (`// ref: AUD-01-08`).
- [ ] `ChooseProfilePatch` reescrito (exato → relaxado → vanilla; sem LINQ; logs gated/Info) (`// ref: AUD-01-04`, `AUD-01-07`).
- [ ] `DynamicSpawnManager`: pré-carga inicial via config; remover 10/10/10 de `ProcessWave`; `ClearSptQueue` 1×/warmup; `GetAliveHumanCount` + pausa; `StopSpawnLoops`; gate nos `[SPY]`/`Horde Breakdown`/`Batch profile pre-fetching`/`SQUAD …` (`// ref: AUD-01-04/05/06/07`).
- [ ] `BotSpawnLoggerPatch` gated (`// ref: AUD-01-07`).
- [ ] `RaidLifecycle.OnRaidEnd` → `StopSpawnLoops()`; `BaseLocalGameStopPatch` → `LocalGameStopPatch` + `CoopGameStopPatch` soft (registro condicional no `Plugin.cs`) (`// ref: AUD-01-06`, `PA-01-03`).
- [ ] `_sptQueueClearedThisRaid` (campo de instância) + pausa sem humano interrompe `_activeWaveCoroutine` (`// ref: PA-01-05`, `PA-01-07`).
- [ ] `Settings.initialProfilePreload` + `PROPRIEDADES.md`.
- [ ] Grep de sanidade: nenhum `LogWarning`/`LogInfo` com prefixo `[TRLDynamicSpawn Logger]`/`[SPY]` fora de `if (Settings.enableDebugLogs.Value)`; `MASTER FALLBACK`/`FAILED`/`Error` continuam sem gate.
- [ ] Compilar (`dotnet build` com refs temporárias, como no 009) → versão **minor** (3.3.0 → 3.4.0: propriedade F12 nova + AC-X1/X2) → deploy com rollback `.bak-3.3.0` → V2.

## 9. Conformidade com skills (auto-checklist)

> Preenchido pelo `/create-technical-spec` ANTES de salvar. Cada linha: ✅ com evidência (seção desta spec ou `arquivo:linha`), ou **N/A + razão**. Linha ❌ → a spec não está pronta. Validado pelo `/review-technical-spec`. Taxonomia: [docs/technical/spt-antipatterns.md](../../../../docs/technical/spt-antipatterns.md).

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes (`GameWorld.OnDestroy` + `BaseLocalGame.Stop`) — AP-01 | ✅ | Hooks do 009 reaproveitados (`GameWorld.cs:2584/:2111`); `OnRaidEnd` idempotente ganha `StopSpawnLoops()` (§5 e). Stop hook trocado do genérico fechado (inerte na V1) pelos overrides concretos `LocalGame.Stop` (`LocalGame.cs:357`, base em `:362`) + `CoopGame.Stop` soft (PA-01-03) — a V2 confere no log a fonte `LocalGame.Stop`/`CoopGame.Stop`. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | `ActivateBotsWithoutWavePatch` e `ChooseProfilePatch` não reagem a ação de player; ambos `return true` em `FikaHelper.IsClient()` (§5) — no guest o vanilla fica intacto. `GetAliveHumanCount` ignora bots e headless. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | ✅ | `BotsController.ActivateBotsWithoutWave` (`:536`, não-virtual; callers: `EFT/NonWavesSpawnScenario.cs:160`, `GClass1876.cs:51` (*NonWaveGroupScenario*), e `BotHalloweenEvent.cs:176` que contorna via `BotSpawner` — backstop); `BotProfileDataClass.ChooseProfile` (`:85`, não-virtual; callers `BotCreationDataClass.cs:127`, `BotsPresets.cs:189`); `LocalGame.Stop` (`:357`, override concreto; `CoopGame.Stop` `:718` via soft patch). Nenhum `GClass` novo pinado; `GClass1876` = NonWaveGroupScenario (conceito), `GClass1890.cs:17` citado só como evidência da NRE. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | Sem alteração de estado do EFT fora das APIs já usadas (`AddToTargetBackup`, `StopBotSpawn` 1×). Recusa em `ActivateBotsWithoutWave` = pular o original (side-effect: `BotSpawner_1.ActivateBotsWithoutWave` não roda — exatamente o objetivo). |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | `StopSpawnLoops` reseta `IsGeneratingDynamicWave`/`IsWarmupActive` (§7 inventário); coroutines param no `OnDestroy`; pool é por raid (`BotsPresets` criado em `LocalGame.cs:158`). |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | ✅ | §3: `Initial Profile Preload` int 15, 0–30, Avançado, 0 = desliga; lido 1× por raid. |
| 7 | Re-invocação de método patcheado tem reentry-guard/`ReversePatch` (sem recursão infinita) — AP-07 | N/A | Nenhum patch re-invoca o alvo. |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca — AP-08 | ✅ | `IsGeneratingDynamicWave` é setada/limpa em `try/finally` síncronos (sem `yield` dentro — `:1017-1033`, `:1236-1243`) e ainda assim **forçada a `false`** no `StopSpawnLoops`/pausa, antes de qualquer early-return (PA-01-06) — §5 c/e. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump; "não existe" conferido no `types-index.json` — AP-09 | ✅ | Reconfirmados 2026-08-22 (review 01 corrigiu a deriva): `EFT/BotsController.cs:536-542`, `EFT/NonWavesSpawnScenario.cs:98-162/:167`, `BotProfileDataClass.cs:16/:19/:43/:85-96`, `BotsPresets.cs:160-190`, `BotCreationDataClass.cs:46/:102-105/:116/:142-146`, `BotSpawner.cs:377` → `GClass1890.cs:17`, `EFT/LocalGame.cs:24/:139-143/:158/:187-194/:357-362`, `GClass1876.cs:51`, `BotHalloweenEvent.cs:176`, `EFT/Profile.cs:632` (`InfoClass`), `InfoClass.cs:123` + `ProfileInfoSettingsClass.cs:5-9`. `ProfileSettingsClass` **não existe** (`types-index.json`) — corrigido. |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Item não usa skills do EFT. |
| 11 | Pacote FIKA próprio — AP-11 | N/A | Nenhum `INetSerializable`. |

## Histórico

| Data | Evento |
|---|---|
| 2026-08-22 | Spec técnica criada via `/optimize-mod-performance --fase 2` (plano de otimização AUD-01-04/05/06/07/08 + PA-01-05) |
| 2026-08-23 | Review 01 (independente): 1 🔴 · 4 🟡 · 4 🟢 — todos aceitos e aplicados: tipo `ProfileInfoSettingsClass`; callers completos de `ActivateBotsWithoutWave`; stop hook concreto `LocalGame.Stop` + `CoopGame.Stop` soft; AC-X5 (fallback PMC); pausa interrompe onda em voo; flags antes do early-return; `ClearSptQueue` 1×/raid; deriva de linhas; MoreBotsAPI marcado como não verificado |
