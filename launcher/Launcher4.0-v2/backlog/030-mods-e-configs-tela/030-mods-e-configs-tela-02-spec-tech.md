# 030 — Tela "Mods e Configs" · Spec técnica

> **Data:** 2026-07-19<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [01-spec funcional](./030-mods-e-configs-tela-01-spec.md) · [007 — motor de sync](../007-sincronizacao-arquivos/007-sincronizacao-arquivos-02-spec-tech.md)<br>

---

> **Adaptação de workflow (launcher ≠ mod).** Este item não tem patch Harmony nem Assembly do EFT: as seções *Pontos de patch* e *Propriedades F12* são **N/A** (ver §3). A fonte primária de evidência é o **código do launcher** (`launcher/Launcher4.0-v2/project/`) e do **servidor C#** (`mods/TarkovRedLine4.0/Server/`), conforme as adaptações declaradas no [mod-backlog](../mod-backlog.md#adaptações-do-workflow-launcher--mod). Toda decisão abaixo cita `arquivo.cs:linha` real.

## 1. Estratégia

Três frentes, nesta ordem de dependência:

**(A) Motor de sync — canal `config-performance` como irmão do `config-force`.** O canal novo é `SyncFolderRule.PerformanceToConfig`, resolvido por prefixo e derivando alvo pela remoção do sufixo `-performance` — o mesmo mecanismo que já serve `-force` e `-server` ([SyncPathUtil.cs:63](../../project/SPT.Launcher.Base/Sync/SyncPathUtil.cs#L63)). Isso substitui o `SyncManifestOverlay` (item 008), que só existia porque o pack morava fora do `mods_repo`.

**(B) Servidor — dois JSONs de definição + realocação do pack.** `config-performance` passa a ser lido de `mods_repo/BepInEx/config-performance/` (D-9) e ambos os JSONs de metadados são excluídos do manifesto (RN-5). As quatro rotas do modelo antigo saem.

**(C) Launcher — tela nova + onboarding, e remoção do modelo antigo.** Tela roteável no padrão `ViewModelBase`, resumo na tela logada, e aposentadoria de `OptionalModsHelper`/`OptionalGroupApplier`/`SyncManifestOverlay`/`OptionalModToggle`.

### 🔴 Achado que define o desenho: o canal é HÍBRIDO, não um clone do force

O `ForceToConfig` **não usa baseline**. Ele compara hash-do-alvo vs hash-do-manifesto e força se divergir ([SyncPlanner.cs:193-194](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L193-L194)); o `SyncEngine` não grava baseline no `ForceCopy` (compare [SyncEngine.cs:106](../../project/SPT.Launcher.Base/Sync/SyncEngine.cs#L106), que grava no `Download`, com o bloco `:250-267`, que não grava).

Clonar esse comportamento entregaria **"sempre sobrescreve"** — o que **contradiz** CA-030.3 (edição posterior preservada) e CA-030.4 (só atualiza quem não customizou). O canal de performance precisa dos dois comportamentos, em momentos distintos:

| Momento | Comportamento | Mecanismo |
|---|---|---|
| Player **liga** o item na tela (ação explícita, D-16) | Aplica **mesmo se divergente**, preservando o anterior em `config-disabled/performance/` | force-like: compara hash-do-alvo, backup, escreve |
| **Syncs de rotina** com o item já ligado | `preserve-divergent`: atualiza só se o local ainda for igual ao baseline | baseline, como o canal `config` |

Implementação: `SyncPlannerOptions` ganha `IReadOnlyCollection<string> ForceApplyGroups` — os itens **recém-alternados** pela tela. Paths pertencentes a esses grupos seguem o caminho force-like; todos os demais seguem preserve-divergent com baseline. É o mesmo motor, com o momento da ação como discriminador.

## 2. Pontos de extensão (equivalente a "pontos de patch")

### 2.1 Motor de sync

| # | Arquivo:linha | O que está lá hoje | Mudança |
|---|---|---|---|
| E-1 | [SyncFolderRule.cs:46](../../project/SPT.Launcher.Base/Sync/SyncFolderRule.cs#L46) | `ForceToConfig = 6` é o último do enum | Adicionar `PerformanceToConfig = 7` |
| E-2 | [SyncFolderRule.cs:77-79](../../project/SPT.Launcher.Base/Sync/SyncFolderRule.cs#L77-L79) | `case "force-to-config"` no `TryParse` | Adicionar `case "performance-to-config"` antes do `default:` (`:80`) |
| E-3 | [SyncRuleResolver.cs:34-35](../../project/SPT.Launcher.Base/Sync/SyncRuleResolver.cs#L34-L35) | Entradas do force (raw + `BepInEx/`) | Adicionar as **duas** variantes de `config-performance` |
| E-4 | [SyncPathUtil.cs:63](../../project/SPT.Launcher.Base/Sync/SyncPathUtil.cs#L63) | `SourceFolderSuffixes = { "-server", "-force" }` | Adicionar `"-performance"` — **ponto único** que faz o canal derivar `config-performance/<rel>` → `config/<rel>` |
| E-5 | [SyncPathUtil.cs:151](../../project/SPT.Launcher.Base/Sync/SyncPathUtil.cs#L151) | `return prefix + "-disabled/" + remainder;` (`DeriveDisabledBackup`) | **D-14**: inserir o segmento de origem → `prefix + "-disabled/" + origem + "/" + remainder` |
| E-6 | [SyncPlanner.cs:456-461](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L456-L461) | `BuildDisabledTarget` monta o outro path de quarentena | **D-14**: mesmo tratamento (origem `optional` para mods desligados) |
| E-7 | [SyncPlanner.cs:95-99](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L95-L99) | `if (rule != ForceToConfig && forceTargets.Contains(...))` | 🔴 Vira precedência de 2 canais — ver §2.2 |
| E-8 | [SyncPlanner.cs:74](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L74) | `BuildForceTargets(filesToCheck)` | Somar `BuildPerformanceTargets` |
| E-9 | [SyncPlanner.cs:161-235](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L161-L235) | Branch `ForceToConfig` (o irmão a espelhar) | Novo branch `PerformanceToConfig`, híbrido (§1) |
| E-10 | [SyncPlanner.cs:352-362](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L352-L362) | Lista de regras puladas no `ScanExtras` | Adicionar `PerformanceToConfig` |
| E-11 | [SyncPlanner.cs:66](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L66) | Filtro `!f.optional \|\| IsOptionalGroupEnabled(...)` | Repontar para o modelo novo (por `id` de mod, não `optionalGroup`) |
| E-12 | [SyncPlan.cs:49](../../project/SPT.Launcher.Base/Sync/SyncPlan.cs#L49) | `IoActionCount` | 🔴 Somar o novo `ActionKind`, senão barra de progresso e `result.Pending` ficam errados |
| E-13 | [SyncEngine.cs:112](../../project/SPT.Launcher.Base/Sync/SyncEngine.cs#L112) | `Rule == MirrorReference ? "reference-updated" : "updated"` | Padrão a copiar para os labels novos |
| E-14 | [SyncReport.cs:24-36](../../project/SPT.Launcher.Base/Sync/SyncReport.cs#L24-L36) | `ActionPriority` (10 labels) | Somar labels novos com prioridade |
| E-15 | [SyncReport.cs:42-54](../../project/SPT.Launcher.Base/Sync/SyncReport.cs#L42-L54) | `ActionDescricao` (frases PT) | Somar frases dos labels novos |
| E-16 | [SyncReport.cs:87-88](../../project/SPT.Launcher.Base/Sync/SyncReport.cs#L87-L88) | `referenceUpdated` desconta de `Updated` | Se o label novo reusar contador, aplicar o mesmo desconto |

### 2.2 🔴 E-7 em detalhe — a precedência vira de 2 canais

O guard atual pula a entrada `config/<rel>` quando existe um force para o mesmo alvo. Com dois canais sobrepondo o `config`, a condição `rule != ForceToConfig` fica ambígua: uma entrada `config-performance` **também** não é `ForceToConfig` e seria descartada pelo próprio `forceTargets`.

A precedência exigida é **performance > force > config** (D-1), então o guard passa a ser escalonado:

- entrada de `config` cujo alvo tem force **ou** performance → pulada;
- entrada de `config-force` cujo alvo tem performance **ligada** → pulada, e o relatório registra `performance-sobrepos-force` (**RN-2**);
- entrada de `config-performance` → nunca pulada.

O aviso de RN-2 é emitido aqui, no planner, onde as duas listas coexistem.

### 2.3 Servidor C#

| # | Arquivo:linha | Mudança |
|---|---|---|
| S-1 | [ModUpdater.cs:50](../../../../mods/TarkovRedLine4.0/Server/TarkovRedLine.Server/Controllers/ModUpdater.cs#L50) | `GetPerformancePath()` passa a apontar para `mods_repo/BepInEx/config-performance` (D-9) |
| S-2 | ModUpdater.cs:455-463 | Skip no scan do `mods_repo`: `plugins-optional.json`, `performance.json` e **tudo sob `BepInEx/config-performance/`** — este último por **prefixo** de path, não igualdade (RN-5, CA-030.8) |
| S-3 | ModUpdater.cs:468-482 | Loop do pack passa a varrer o novo caminho; paths relativos a `BepInEx/config-performance/` |
| S-4 | ModUpdater.cs:365-435 | **Remover** `ScanOptionalGroups` (chamador único em `:541`) |
| S-5 | ModUpdater.cs:226-281 / 283-317 / 319-335 | **Remover** as rotas `optionals-list`, `optionals-manifest`, `optional-download` |
| S-6 | ModUpdater.cs:493-523 | `folderRules` default ganha `BepInEx/config-performance` → `performance-to-config` |
| S-7 | ModUpdater.cs:551-564 | Manifesto: trocar `optionalGroups` por `optionalMods` (novo shape) e manter `performanceOverlay` como lista de **itens**, não de arquivos soltos |
| S-8 | ModUpdater.cs:103 (`TryResolveUnder`) | Reusar para validar D-15: recusar `paths` sob `user/mods/` (CA-030.8b) |

> **Nota factual:** hoje `config-performance` é **irmão** de `mods_repo` ([ModUpdater.cs:48,50](../../../../mods/TarkovRedLine4.0/Server/TarkovRedLine.Server/Controllers/ModUpdater.cs#L48-L50)) e por isso **não** entra no scan do `mods_repo`. Ao mover para dentro (D-9), ela passa a entrar — daí o skip de S-2 ser obrigatório, não opcional. Sem ele, o defeito descrito na spec funcional (pasta distribuída inerte) continua.

## 3. Propriedades F12 · Pontos de patch Harmony

**N/A — não se aplica ao launcher.** O launcher é um executável Avalonia standalone: não injeta no processo do EFT, não usa Harmony e não tem `BepInPlugin`/`ConfigEntry`. Configuração do usuário mora em `SPT/user/launcher/config.json` via [LauncherSettingsProvider.cs:24](../../project/SPT.Launcher.Base/Helpers/LauncherSettingsProvider.cs#L24) — tratado em §4.

## 4. Arquivos a modificar / criar / remover

### CRIAR

| Arquivo | Resumo |
|---|---|
| `SPT.Launcher/ViewModels/ModsConfigsViewModel.cs` | Tela nova. `ViewModelBase`, `[RequireLoggedIn]`, 2 coleções + toggles "todos" + aplicação ao sair |
| `SPT.Launcher/Views/ModsConfigsView.axaml` (+ `.axaml.cs`) | 2 colunas, barra de progresso reusada, estado vazio (CA-030.15b) |
| `SPT.Launcher/ViewModels/OptionalItemToggle.cs` | Substitui `OptionalModToggle`; serve mods e itens de performance |
| `SPT.Launcher/ViewModels/Dialogs/OnboardingDialogViewModel.cs` (+ View) | Modal one-shot (CA-030.18) |
| `SPT.Launcher.Base/Models/Launcher/OptionalModDefinition.cs` | Shape do `plugins-optional.json` |
| `SPT.Launcher.Base/Models/Launcher/PerformanceItemDefinition.cs` | Shape do `performance.json` |
| `SPT.Launcher.Tests/Sync/SyncPerformanceChannelTests.cs` | Cobre CA-030.1..5 |
| `SPT.Launcher.Tests/Sync/SyncDisabledNamespaceTests.cs` | Cobre D-14 / CC-11 (G-7) |

### MODIFICAR

| Arquivo | Mudança |
|---|---|
| `SyncFolderRule.cs` | E-1, E-2 |
| `SyncRuleResolver.cs` | E-3 |
| `SyncPathUtil.cs` | E-4, E-5 |
| `SyncPlanner.cs` | E-6..E-11 |
| `SyncPlan.cs` | E-12 |
| `SyncEngine.cs` | E-13 + novo `ActionKind` |
| `SyncReport.cs` / `SyncResult.cs` | E-14..E-16 |
| `SyncPlannerOptions.cs` | `ForceApplyGroups` (§1); repontar `IsOptionalGroupEnabled` (`:35`) |
| `LauncherSettingsProvider.cs` | Estado por-`id` dos dois eixos + marca de onboarding; **incrementar `EXPECTED_CONFIG_VERSION` (`:67`, hoje 4)** |
| `ProfileViewModel.cs` | Remover o fluxo opcional (`:258-464`, `:667-693`, `:751-761`, `:891-901`); somar o resumo (CA-030.13) |
| `ModUpdateViewModel.cs` | 🔴 Espelha a lógica do ProfileViewModel (`:150`, `:201-226`, `:238-242`, `:416-423`) — **precisa do mesmo tratamento**; e os mapas `:496-506`, `:520-532`, `:570-588` precisam dos labels novos |
| `ProfileView.axaml` | Remover painel `:139-158`; somar resumo clicável; item de menu novo no sidebar |
| `SettingsView.axaml` | Remover o bloco `:127-142` (toggle de performance, D-12) |
| `SettingsViewModel.cs` | Remover `UsePerformanceConfigs` (`:59-71`) |
| `LocalizationProvider.cs` + 2 JSONs | Chaves novas com **paridade total** (CA-030.15) |
| `ModUpdater.cs` (servidor) | S-1..S-8 |

### REMOVER

| Arquivo | Consumidores a limpar antes |
|---|---|
| `Helpers/OptionalModsHelper.cs` | `ProfileViewModel` (16 refs), `ModUpdateViewModel` (5 refs) |
| `Sync/OptionalGroupApplier.cs`, `Sync/OptionalOpResult.cs` | `OptionalModsHelper`, `ProfileViewModel:290,319,326` |
| `Sync/SyncManifestOverlay.cs` | `ProfileViewModel:755,759,891,895,898`, `ModUpdateViewModel:150,224,418,421` |
| `ViewModels/OptionalModToggle.cs` | `ProfileViewModel:71,263,326,440,683,691`, `ProfileView.axaml:141` |
| `Tests/Sync/OptionalGroupApplierTests.cs`, `Tests/Sync/SyncOverlayTests.cs` | — (somem com o código) |
| `RequestHandler`: `RequestOptionalsList` (`:249`), `RequestOptionalsManifest` (`:229`), `DownloadOptionalFile` (`:218`) | Só `OptionalModsHelper` |

> 🔴 **`DownloadModFile` ([RequestHandler.cs:207](../../project/SPT.Launcher.Base/Controllers/RequestHandler.cs#L207)) NÃO sai.** Apesar de ser usado pelo fluxo opcional, é o **downloader base de todo o sync** (`ProfileViewModel:893`, `ModUpdateViewModel:416`). Remover derruba a sincronização inteira. Só o *uso* com timeout ampliado é do fluxo antigo.

## 5. Stubs de código

### 5.1 Regra nova (E-1, E-2)

```csharp
// SPT.Launcher.Base/Sync/SyncFolderRule.cs
public enum SyncFolderRule
{
    // ... Default=0 .. ForceToConfig=6 (ref: SyncFolderRule.cs:46)

    /// <summary>
    /// config-performance → config, quando o item está LIGADO. Vence config-force e config (D-1).
    /// Híbrido (ver spec-tech §1): no momento em que o player liga (grupo em
    /// <see cref="SyncPlannerOptions.ForceApplyGroups"/>) aplica mesmo divergente, preservando o
    /// anterior em config-disabled/performance/; nos syncs seguintes respeita a customização
    /// via baseline (preserve-divergent). NÃO é clone do ForceToConfig, que ignora baseline.
    /// </summary>
    PerformanceToConfig = 7,
}

public static class SyncFolderRuleParser
{
    public static bool TryParse(string value, out SyncFolderRule rule)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            // ... cases existentes (ref: SyncFolderRule.cs:59-79)
            case "performance-to-config":
                rule = SyncFolderRule.PerformanceToConfig;
                return true;
            default:
                rule = SyncFolderRule.Default;
                return false;
        }
    }
}
```

### 5.2 Derivação de alvo e quarentena com namespace (E-4, E-5 · D-14)

```csharp
// SPT.Launcher.Base/Sync/SyncPathUtil.cs

// ref: SyncPathUtil.cs:63 — o sufixo novo faz DeriveSeedTarget mapear
// config-performance/<rel> → config/<rel> sem nenhuma outra mudança.
private static readonly string[] SourceFolderSuffixes = { "-server", "-force", "-performance" };

/// <summary>Origem da quarentena (D-14): evita que backup de force, mod opcional e
/// performance colidam por homonímia dentro de config-disabled/.</summary>
public enum DisabledOrigin { Force, Optional, Performance }

private static string OriginSegment(DisabledOrigin origin) => origin switch
{
    DisabledOrigin.Force => "force",
    DisabledOrigin.Optional => "optional",
    DisabledOrigin.Performance => "performance",
    _ => "other",
};

// ref: SyncPathUtil.cs:120 — assinatura ganha a origem; o corpo até :150 não muda.
public static string DeriveDisabledBackup(
    string targetPath,
    string normalizedSourcePrefix,
    DisabledOrigin origin)
{
    // ... cálculo de prefix/remainder inalterado (ref: SyncPathUtil.cs:128-150)
    string prefix = /* ... */ string.Empty;
    string remainder = /* ... */ string.Empty;

    // ref: SyncPathUtil.cs:151 — era: prefix + "-disabled/" + remainder
    return prefix + "-disabled/" + OriginSegment(origin) + "/" + remainder;
}
```

### 5.3 Opções do planner — o discriminador do híbrido (§1)

```csharp
// SPT.Launcher.Base/Sync/SyncPlannerOptions.cs

/// <summary>
/// Ids de itens de performance que o player ACABOU de ligar nesta rodada. Para os paths
/// desses itens o canal PerformanceToConfig aplica mesmo se o alvo divergir (ação explícita,
/// D-16), preservando o anterior em config-disabled/performance/. Fora deste conjunto, o
/// canal respeita a customização via baseline (CA-030.3/4). Vazio = sync de rotina.
/// </summary>
public IReadOnlyCollection<string> ForceApplyGroups { get; init; } = Array.Empty<string>();
```

### 5.4 Branch do planner (E-9) — esqueleto do híbrido

```csharp
// SPT.Launcher.Base/Sync/SyncPlanner.cs — dentro do foreach, após o branch ForceToConfig (:235)

if (rule == SyncFolderRule.PerformanceToConfig)
{
    // ref: SyncPlanner.cs:163 — mesma derivação do force (o sufixo novo cuida do mapeamento)
    string perfTargetRel = SyncPathUtil.DeriveSeedTarget(file.path, matchedPrefix);
    string perfTargetLocal = SyncPathUtil.ToLocalPath(_options.GameRoot, perfTargetRel);

    // ref: SyncPlanner.cs:185 — backup com namespace de origem (D-14)
    string perfBackupRel = SyncPathUtil.DeriveDisabledBackup(
        perfTargetRel, matchedPrefix, SyncPathUtil.DisabledOrigin.Performance);

    bool justEnabled = _options.ForceApplyGroups.Contains(GroupIdOf(file));

    if (!File.Exists(perfTargetLocal))
    {
        AddPerformanceAction(plan, file, perfTargetRel, perfBackupRel, rule,
            "performance (ausente no config)");
        continue;
    }

    string targetHash = await Task.Run(
        () => SyncPathUtil.ComputeMd5(perfTargetLocal), cancellationToken);

    if (string.Equals(targetHash, file.hash, StringComparison.OrdinalIgnoreCase))
    {
        continue; // já aplicado — no-op
    }

    // ref: SyncPlanner.cs:199 — Dev Mode continua sendo o escape hatch (CC-14)
    if (_options.DevMode)
    {
        plan.Actions.Add(new SyncAction
        {
            RelativePath = perfTargetRel,
            Kind = SyncActionKind.PreserveDevMode,
            Rule = rule,
            ServerHash = file.hash,
            Reason = "Dev Mode: config de performance preservada (difere do servidor)",
        });
        continue;
    }

    if (justEnabled)
    {
        // AÇÃO EXPLÍCITA (D-16/CA-030.2): aplica mesmo divergente, com backup.
        AddPerformanceAction(plan, file, perfTargetRel, perfBackupRel, rule,
            "performance ligada (a sua config vai p/ config-disabled/performance e é substituída)");
        continue;
    }

    // SYNC DE ROTINA: preserve-divergent — só atualiza quem não customizou (CA-030.3/4).
    // ref: SyncPlanner.cs:263-266 — mesma comparação com baseline do canal 'config'.
    if (_baseline.TryGetHash(perfTargetRel, out var baselineHash)
        && string.Equals(baselineHash, targetHash, StringComparison.OrdinalIgnoreCase))
    {
        AddPerformanceAction(plan, file, perfTargetRel, perfBackupRel, rule,
            "performance (atualizada pelo servidor)");
    }
    else
    {
        plan.Actions.Add(new SyncAction
        {
            RelativePath = perfTargetRel,
            Kind = SyncActionKind.Preserve,
            Rule = rule,
            ServerHash = file.hash,
            Reason = "você customizou — sua versão foi mantida",
        });
    }

    continue;
}
```

> **TODO confirmar:** `GroupIdOf(file)` depende do shape final do manifesto (S-7). Se o item de performance vier como lista de arquivos com o `id` do item em cada entrada, é leitura direta; se vier agrupado, o planner precisa do mapa `path → id` montado antes do loop.

### 5.5 Preferência persistida (LauncherSettingsProvider)

```csharp
// SPT.Launcher.Base/Helpers/LauncherSettingsProvider.cs — dentro de class Settings

// ref: LauncherSettingsProvider.cs:190-195 — mesmo padrão do EnabledOptionals já existente.
private Dictionary<string, bool> _enabledPerformanceItems = new();
public Dictionary<string, bool> EnabledPerformanceItems
{
    get => _enabledPerformanceItems;
    set => SetProperty(ref _enabledPerformanceItems, value);
}

/// <summary>CA-030.16b / D-17: fonte de verdade do onboarding — o estado do disco não decide.</summary>
private bool _modsConfigsOnboardingDone;
public bool ModsConfigsOnboardingDone
{
    get => _modsConfigsOnboardingDone;
    set => SetProperty(ref _modsConfigsOnboardingDone, value);
}

// ref: LauncherSettingsProvider.cs:263-275 — espelha IsOptionalEnabled/SetOptionalEnabled.
public bool IsPerformanceItemEnabled(string itemId) =>
    !string.IsNullOrEmpty(itemId)
    && EnabledPerformanceItems.TryGetValue(itemId, out bool enabled)
    && enabled;

public void SetPerformanceItemEnabled(string itemId, bool enabled)
{
    if (string.IsNullOrEmpty(itemId)) return;
    EnabledPerformanceItems[itemId] = enabled;
    SaveSettings();
}
```

> 🔴 **`EXPECTED_CONFIG_VERSION` ([LauncherSettingsProvider.cs:67](../../project/SPT.Launcher.Base/Helpers/LauncherSettingsProvider.cs#L67), hoje `4`) precisa virar `5`.** `LoadSettings` (`:68-72`) força re-save quando a versão salva é menor, gravando os campos novos com default sem perder dados do jogador.

### 5.6 Tela nova (padrão roteável)

```csharp
// SPT.Launcher/ViewModels/ModsConfigsViewModel.cs
// ref: ViewModelBase.cs:15 (UrlPathSegment/HostScreen já vêm da base — não redeclarar)
// ref: ProfileViewModel.cs:30 — [RequireLoggedIn] redireciona p/ ConnectServer se deslogado
// ref: ViewLocator.cs:12 — o nome ModsConfigsViewModel exige Views/ModsConfigsView

using System.Collections.ObjectModel;
using ReactiveUI;
using SPT.Launcher.Attributes;

namespace SPT.Launcher.ViewModels
{
    [RequireLoggedIn]
    public class ModsConfigsViewModel : ViewModelBase
    {
        public ObservableCollection<OptionalItemToggle> OptionalMods { get; } = new();
        public ObservableCollection<OptionalItemToggle> PerformanceItems { get; } = new();

        public ModsConfigsViewModel(IScreen Host, bool onboarding = false) : base(Host)
        {
            // onboarding == true → estado inicial "tudo ligado" (CA-030.17) + modal (CA-030.18)
        }
    }
}
```

## 6. Fluxo de dados

```
SERVIDOR (Launcher-Updater/)
  mods_repo/BepInEx/plugins-optional.json ──┐   (metadados; NUNCA sincronizado — S-2/RN-5)
  mods_repo/BepInEx/config-performance/     │
      performance.json ───────────────────┐ │   (metadados; NUNCA sincronizado)
      <arquivos .cfg> ──────────────┐     │ │
                                    │     │ │
  GenerateManifestAsync ────────────┴─────┴─┴──> manifesto
    ref: ModUpdater.cs:437                        { files[], optionalMods[],
                                                    performanceOverlay[], folderRules }
                                                            │
LAUNCHER                                                    ▼
  ProfileViewModel.CheckForUpdatesCore ─── lê manifesto ─────┤  ref: ProfileViewModel.cs:625
                                                            ▼
  SyncPlannerOptions { ForceApplyGroups = itens recém-ligados pela tela }
                                                            │
  SyncRuleResolver.Resolve ──> PerformanceToConfig           │  ref: SyncRuleResolver.cs:34 (E-3)
                                                            ▼
  SyncPlanner  ── pre-pass: BuildForceTargets + BuildPerformanceTargets (E-8)
    │            ── precedência performance > force > config (E-7)
    │            ── DeriveSeedTarget: config-performance/<rel> → config/<rel>  (E-4)
    │            ── DeriveDisabledBackup(..., Performance) → config-disabled/performance/<rel> (E-5/D-14)
    ▼
  SyncPlan.Actions ── IoActionCount inclui o Kind novo (E-12)
    ▼
  SyncEngine ── escreve config/<rel> · backup em config-disabled/<origem>/<rel>
    │          ── baseline gravado só no caminho preserve-divergent (§1)
    ▼
  SyncReport ── labels novos + descrição PT (E-14/E-15) · RN-2 registra
                "performance-sobrepos-force"
    ▼
  last-update.json   ref: ProfileViewModel.cs:886-889
```

## 7. Riscos e dependências

| # | Risco | Mitigação |
|---|---|---|
| R-1 | 🔴 **Clonar o force quebraria CA-030.3/4** — o force não tem baseline (§1) | Canal híbrido com `ForceApplyGroups`; testes cobrindo os dois momentos |
| R-2 | 🔴 **`ModUpdateViewModel` duplica a lógica do `ProfileViewModel`** (`:150`, `:201-226`, `:238-242`, `:416-423`) | Toda mudança precisa ser aplicada nos dois, ou a tela de update diverge silenciosamente |
| R-3 | 🔴 Remover `DownloadModFile` derrubaria o sync inteiro | Não remover — só os 3 métodos exclusivos (§4) |
| R-4 | `IoActionCount` esquecido → barra de progresso e `Pending` errados | E-12 no checklist, com teste |
| R-5 | D-14 muda o destino do backup do `config-force`, que está em produção desde a 2.3.0 | Backups antigos na raiz continuam válidos (sem migração); G-7b valida |
| R-6 | Mover `config-performance` para dentro do `mods_repo` sem o skip S-2 mantém o defeito atual | S-2 é obrigatório; teste de manifesto |
| R-7 | Testes existentes fixam o comportamento atual: `SyncForceConfigTests`, `SyncOverlayTests`, `SyncRuleResolverTests`, `SyncPathGuardTests`, `SyncTestFixture.cs:81` | `SyncOverlayTests` some com o overlay; os outros precisam de atualização, não deleção |
| R-8 | Coop (Fika): escolhas divergentes entre clientes | CC-7 + G-5; validação de conteúdo marca o que não pode ser opcional |
| R-9 | `EXPECTED_CONFIG_VERSION` não incrementado → campos novos não persistem para quem já tem config | 5.5, com teste de migração |
| R-10 | Ordem de execução do onboarding (CC-2): gatilho **antes** do primeiro sync | Checagem no `MainWindowViewModel`, antes de `CheckForUpdates` |

## 8. Checklist de implementação

**Fase 1 — motor (isolado, testável sem UI)**
1. E-1..E-4: enum, parser, resolver, sufixo. Teste: `config-performance/x.cfg` resolve para `PerformanceToConfig` e deriva `config/x.cfg`.
2. E-5/E-6 + `DisabledOrigin`: namespace da quarentena. Teste: três origens homônimas coexistem (G-7).
3. `SyncPlannerOptions.ForceApplyGroups` + branch E-9 (híbrido). Testes: ligar sobre customizada aplica com backup; sync de rotina preserva; servidor novo atualiza quem não customizou.
4. E-7/E-8: precedência de 2 canais + aviso RN-2. Teste: arquivo nos 3 canais → performance vence, warning emitido.
5. E-10, E-12: `ScanExtras` e `IoActionCount`.
6. E-13..E-16: labels, prioridade, descrição PT, contadores.
7. `dotnet test` verde, incluindo os testes atualizados de R-7.

**Fase 2 — servidor**
8. S-1/S-3: realocar o pack; S-2: skips (por prefixo). Teste: manifesto não contém metadados nem `config-performance/*` como arquivo comum.
9. S-6/S-7: `folderRules` + shape novo do manifesto.
10. S-8: validação D-15 (recusar `user/mods/`).
11. S-4/S-5: remover `ScanOptionalGroups` e as 3 rotas.

**Fase 3 — launcher/UI**
12. 5.5: preferências + `EXPECTED_CONFIG_VERSION` → 5.
13. Tela nova + `OptionalItemToggle` + modal de onboarding; aplicação ao sair reusando a barra (`ProfileViewModel.cs:778-803`).
14. Resumo na tela logada + item no sidebar; gate de jogo aberto (CA-030.23) e "sem mudanças não sincroniza" (CA-030.22).
15. i18n: chaves nos 3 lugares, com paridade verificada.
16. Remover o modelo antigo (§4 REMOVER), **nesta ordem**: call-sites → helpers → testes órfãos.
17. `dotnet build` + `dotnet test` verdes; `/code-review` antes de qualquer release.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência |
|---|---|---|---|
| 1 | Toda ref técnica ancorada em `arquivo:linha` real | ✅ | §2 (16 pontos), §4, §7 — todos com linha verificada nesta sessão |
| 2 | Sem invenção de API | ✅ | Assinaturas conferidas: `DeriveSeedTarget` `SyncPathUtil.cs:74`, `DeriveDisabledBackup` `:120`, `BuildForceTargets` `SyncPlanner.cs:469` |
| 3 | Lifecycle / ordem de inicialização | ✅ | R-10 + CC-2: gatilho do onboarding antes do primeiro sync (`MainWindowViewModel.cs:83`) |
| 4 | Estado entre execuções (equivalente a "entre raids") | ✅ | Baseline por path normalizado (`SyncBaseline.cs:62-76`); preferências por `id` (5.5); `EXPECTED_CONFIG_VERSION` (R-9) |
| 5 | Coop / Fika | ✅ | R-8, CC-7, G-5; RN-2 torna visível a supressão de config forçada — o risco real de paridade |
| 6 | Threading / UI | ✅ | I/O e hash off-thread (`Task.Run` em 5.4, padrão de `SyncPlanner.cs:193`); progresso via `Progress<SyncProgress>` (`ProfileViewModel.cs:778-803`) |
| 7 | Não duplicar mecanismo existente | ✅ | §1: canal reusa `folderRules` e **aposenta** o `SyncManifestOverlay` em vez de somar um terceiro caminho |
| 8 | Escrita em disco protegida | ✅ | `ResolveUnderRoot` (`SyncPathUtil.cs:35`), escrita atômica (`SyncFileOps.WriteAtomic`), quarentena em vez de delete (RN-4), guard do `deleteFiles` cobre subpastas por segmento (`ProfileViewModel.cs:716-725`) |
| 9 | Patches Harmony / F12 | N/A | §3 — launcher standalone, sem Harmony e sem BepInEx |

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-19 | Guilherme | Criação — 16 pontos de extensão mapeados com linha, 6 stubs, plano de remoção do modelo antigo. Achado principal: o canal de performance **não pode ser clone do `ForceToConfig`** (que ignora baseline), precisa ser híbrido |
