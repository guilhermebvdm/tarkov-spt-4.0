# 030 — Tela "Mods e Configs" · Spec técnica (v2)

> **Data:** 2026-07-20<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [01-spec funcional](./030-mods-e-configs-tela-01-spec.md) · [review 01](./030-mods-e-configs-tela-03-spec-tech-review-01.md) · [review 02](./030-mods-e-configs-tela-03-spec-tech-review-02.md) · [007 — motor de sync](../007-sincronizacao-arquivos/007-sincronizacao-arquivos-02-spec-tech.md)<br>

---

> **Adaptação de workflow (launcher ≠ mod).** Sem patch Harmony e sem Assembly do EFT: *Pontos de patch* e *Propriedades F12* são **N/A** (§3). Fonte primária = código do launcher (`launcher/Launcher4.0-v2/project/`) e do servidor C# (`mods/TarkovRedLine4.0/Server/`), conforme as adaptações do [mod-backlog](../mod-backlog.md#adaptações-do-workflow-launcher--mod).

> **v2 (2026-07-20)** — reescrita após a [review 02](./030-mods-e-configs-tela-03-spec-tech-review-02.md), que achou 7 bloqueadores estruturais na v1. Principais correções: o eixo **desligar** passa a existir (PA-02-01); o espelho de D-10 ganha desenho viável (D-18); resíduos do modelo antigo eliminados (PA-02-03); o canal grava **baseline**, sem o que não convergia (PA-02-04); a quarentena explícita reusa as guardas do `ScanExtras`, incluindo coop-safe do Fika (PA-02-05); o filtro de opcionais **permanece** (PA-02-06).

## 1. Estratégia

Três frentes, nesta ordem de dependência: **(A)** motor de sync, **(B)** servidor, **(C)** launcher/UI + remoção do modelo antigo.

O eixo de **configs de performance** vira um canal de pasta (`SyncFolderRule.PerformanceToConfig`), irmão do `config-force`, aposentando o `SyncManifestOverlay` (D-13). O eixo de **mods opcionais** reusa o filtro de manifesto que já existe, somando uma ação de quarentena explícita.

### 1.1 O canal é híbrido — e precisa gravar baseline

O `ForceToConfig` compara hash-do-alvo vs manifesto e força se divergir ([SyncPlanner.cs:193-194](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L193-L194)), **sem baseline**. Clonar entregaria "sempre sobrescreve", contradizendo CA-030.3/4. O canal de performance precisa de dois comportamentos:

| Momento | Comportamento |
|---|---|
| Player **alterna** o item na tela (ação explícita, D-16) | Aplica/remove **mesmo divergente**, preservando o anterior na quarentena |
| **Syncs de rotina** | `preserve-divergent`: só atualiza se o local ainda for igual ao baseline |

🔴 **O que a v1 errou (PA-02-04):** o `SyncEngine` só grava baseline no branch `Download` ([SyncEngine.cs:106](../../project/SPT.Launcher.Base/Sync/SyncEngine.cs#L106)); o `ForceCopy` (`:207-281`) **não grava**. Sem baseline, o sync seguinte cai em *"no baseline, treated as customized"* ([SyncPlanner.cs:273](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L273)) e a config **nunca mais** atualiza nem reverte — o wedge documentado em `SyncOverlayTests.cs:189`.

**Portanto:** o `SyncActionKind` novo — **`PerformanceCopy`** — é tratado no engine como *"escreve **e grava baseline**"*, diferente do `ForceCopy`. É essa gravação que faz o híbrido convergir.

### 1.2 Discriminadores (nomes definitivos — PA-02-13)

A v1 usava `ForceApplyGroups` com três definições conflitantes. Nomes canônicos:

| Nome | Tipo | Papel |
|---|---|---|
| `IsOptionalModEnabled` | `Func<string,bool>` | Item do eixo **mods** está ligado? (rename de `IsOptionalGroupEnabled`, [SyncPlannerOptions.cs:35](../../project/SPT.Launcher.Base/Sync/SyncPlannerOptions.cs#L35)) |
| `IsPerformanceItemEnabled` | `Func<string,bool>` | Item do eixo **performance** está ligado? **Não existia na v1** (PA-02-01) |
| `JustToggledIds` | `IReadOnlyCollection<string>` | Ids alternados pelo player e ainda não aplicados. Vem de `PendingApply` (persistido, §5.5). Cobre **os dois eixos** |

## 2. Pontos de extensão

### 2.1 Motor de sync

| # | Arquivo:linha | Hoje | Mudança |
|---|---|---|---|
| E-1 | [SyncFolderRule.cs:46](../../project/SPT.Launcher.Base/Sync/SyncFolderRule.cs#L46) | `ForceToConfig = 6` é o último | `PerformanceToConfig = 7` |
| E-2 | [SyncFolderRule.cs:77-79](../../project/SPT.Launcher.Base/Sync/SyncFolderRule.cs#L77-L79) | `case "force-to-config"` | `case "performance-to-config"` antes do `default:` |
| E-3 | [SyncRuleResolver.cs:34-35](../../project/SPT.Launcher.Base/Sync/SyncRuleResolver.cs#L34-L35) | Entradas do force (raw + `BepInEx/`) | **4 entradas novas**: `config-performance` e `BepInEx/config-performance` → `performance-to-config`; `config-performance-ref` e `BepInEx/config-performance-ref` → `mirror-reference` (D-18) |
| E-4 | [SyncPathUtil.cs:63](../../project/SPT.Launcher.Base/Sync/SyncPathUtil.cs#L63) | `SourceFolderSuffixes = { "-server", "-force" }` | Somar `"-performance"` — ponto único que faz `config-performance/<rel>` → `config/<rel>` |
| E-5 | [SyncPathUtil.cs:120,151](../../project/SPT.Launcher.Base/Sync/SyncPathUtil.cs#L120) | `DeriveDisabledBackup` → `<prefixo>-disabled/<rel>` | Parâmetro `DisabledOrigin`; ver §2.2 |
| E-6 | [SyncPlanner.cs:456-461](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L456-L461) | `BuildDisabledTarget` (extras do mirror) | Parâmetro `DisabledOrigin`, com `MirrorExtra` **preservando o formato atual** (§2.2) |
| E-7 | [SyncPlanner.cs:95-99](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L95-L99) | Guard de colisão com `forceTargets` | Precedência de 2 canais; §2.3 |
| E-8 | [SyncPlanner.cs:74](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L74) | `BuildForceTargets(filesToCheck)` | Somar `BuildPerformanceTargets` (só itens **ligados**) |
| E-9 | após [SyncPlanner.cs:235](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L235) | — | Branch `PerformanceToConfig` completo (ligado **e** desligado); §5.4 |
| E-10 | [SyncPlanner.cs:352-362](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L352-L362) | Regras puladas no `ScanExtras` | Somar `PerformanceToConfig` |
| E-11 | [SyncPlanner.cs:66](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L66) | Filtro `!f.optional \|\| IsOptionalGroupEnabled(...)` | 🔴 **PERMANECE** — só renomeia para `optionalId`/`IsOptionalModEnabled`. Removê-lo causa loop (§2.4) |
| E-12 | [SyncPlan.cs:49](../../project/SPT.Launcher.Base/Sync/SyncPlan.cs#L49) | `IoActionCount` soma kinds explicitamente | Somar `PerformanceCount` (do `PerformanceCopy`) |
| E-13 | [SyncEngine.cs:112](../../project/SPT.Launcher.Base/Sync/SyncEngine.cs#L112) | Label por `Rule` no `Download` | Padrão a copiar; novo case `PerformanceCopy` **com `SetHash`** (§1.1) |
| E-14 | [SyncReport.cs:24-36](../../project/SPT.Launcher.Base/Sync/SyncReport.cs#L24-L36) | `ActionPriority` (10 labels) | Somar `performance-applied`, `performance-reverted`, `performance-suppressed-force` |
| E-15 | [SyncReport.cs:42-54](../../project/SPT.Launcher.Base/Sync/SyncReport.cs#L42-L54) | `ActionDescricao` (frases PT) | Somar as frases dos 3 labels |
| E-16 | [SyncResult.cs:37](../../project/SPT.Launcher.Base/Sync/SyncResult.cs#L37) | Contadores `Forced`, `ConfigsBackedUp` | Somar `PerformanceApplied` + linha no `Summary` (`:58-75`) — é o único texto que o auto-apply mostra |

### 2.2 Quarentena: origem e natureza (D-14 revisado, D-20)

Estrutura final:

```
BepInEx/config-disabled/                     ← backup do config-force (INALTERADO)
BepInEx/config-disabled/optional/<rel>       ← config de mod opcional desligado
BepInEx/config-disabled/performance/replaced/<rel>   ← config do PLAYER, sobrescrita ao ligar
BepInEx/config-disabled/performance/removed/<rel>    ← config do SERVIDOR, retirada ao desligar
BepInEx/plugins-disabled/<rel>               ← extra do mirror (INALTERADO)
BepInEx/plugins-disabled/optional/<rel>      ← plugin de mod opcional desligado
```

🔴 **Revisão de D-14 (mudança em relação à v1, precisa do seu aval).** A v1 mandava mover **também** o backup do `config-force` para `config-disabled/force/`. Descobri dois problemas:

1. **Quebra 6 testes** de `SyncForceConfigTests` e toca um caminho em produção desde a 2.3.0, sem ganho — o force não colide com ninguém se os **novos** vierem em subpasta.
2. **Pior:** backups antigos ficam na raiz e novos na subpasta, **partindo os backups do force em dois lugares** — o oposto do objetivo de D-14 (recuperação previsível).

Então: **o force fica onde está**; só as origens novas ganham subpasta. A colisão que D-14 existe para impedir continua impossível. O `MirrorExtra` idem — preserva o formato atual e mantém verdes os testes de `SyncPlannerTests`/`SyncEngineTests` (PA-02-12).

### 2.3 E-7 — precedência de dois canais

Guard escalonado (`performance > force > config`, D-1):

- entrada de `config` cujo alvo tem force **ou** performance ligada → pulada;
- entrada de `config-force` cujo alvo tem performance **ligada** → pulada, e o relatório registra `performance-suppressed-force` (**RN-2**). Sem isso os dois emitiriam ação para o mesmo alvo e o force re-dispararia todo sync (o hash do alvo nunca bateria com o dele);
- entrada de `config-performance` → nunca pulada.

`BuildPerformanceTargets` considera **apenas itens ligados** — item desligado não suprime force.

> **RN-2 tem dois lados** (a funcional pede aviso no servidor **e** no relatório): o **planner** emite o label no relatório; o **servidor** (S-6) valida na geração. Ambos especificados.

### 2.4 🔴 Mods opcionais: filtro PERMANECE + quarentena com as guardas

**O filtro fica** ([SyncPlanner.cs:66](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L66)), só renomeado. Removê-lo (como o plano da v1 sugeria) faria o arquivo de mod desligado — ausente do disco por estar na quarentena — virar `Download`, e no sync seguinte `MoveToDisabled` de novo: **loop de download-e-quarentena a cada verificação**, com baseline escrito e apagado toda vez (PA-02-06).

**Quarentena explícita** (PA-01-01): para cada arquivo com `optionalId` cujo item está desligado e que existe no disco → `MoveToDisabled` com origem `Optional`. `manifestPaths` continua incluindo o path (a proteção CC3 de `SyncPlanner.cs:59-63` segue válida, evitando ação duplicada pelo `ScanExtras`).

🔴 **As guardas do `ScanExtras` são obrigatórias aqui (PA-02-05).** A ação explícita passa por fora de proteções que só existem dentro do `ScanExtras`. Antes de emitir, aplicar **as mesmas checagens**, extraídas para um método compartilhado:

| Guarda | Linha | Se ignorada |
|---|---|---|
| `_protectedNormalized` | [:347](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L347) | Contradiz o próprio `ProtectedPaths` |
| `IsIgnored` / `IsExcludedFromCleanup` | `:345-346` | `ignoredFiles` do manifesto deixa de proteger |
| **coop-safe (Fika)** | `:386-391` | 🔴 Plugin Fika opcional desligado **quebra o join do cliente** (item 023) |
| Dev Mode | `:364-376` | Contradiz CC-14 (build local do dev não é movida) |

> Dev Mode aqui ≠ Dev Mode no §5.4. Aqui vale **CC-14** (não mover build local não solicitada). Lá vale **CC-19** (ação explícita do player vence). Distinção deliberada.

### 2.5 Contrato do manifesto

```jsonc
{
  "files": [
    { "path": "BepInEx/plugins/TarkovIRL.dll", "hash": "...", "size": 123,
      "optional": true, "optionalId": "tarkov-irl" },
    { "path": "BepInEx/config-performance/sombras.cfg",     "hash": "...", "size": 45,
      "performanceId": "shadows-low" },
    { "path": "BepInEx/config-performance-ref/sombras.cfg", "hash": "...", "size": 45 }
  ],
  "optionalMods":     [ { "id": "tarkov-irl",  "name": "...", "description": { "pt": "...", "en": "..." } } ],
  "performanceItems": [ { "id": "shadows-low", "name": "...", "description": { "pt": "...", "en": "..." } } ]
}
```

- `optionalGroup` → **`optionalId`**; novo **`performanceId`** (resolve o `GroupIdOf` da v1 por leitura direta).
- Campo **`performanceItems`** — `performanceOverlay` era do modelo antigo e **não existe mais** (PA-02-03).
- **D-18:** os arquivos de performance aparecem **duas vezes**, sob dois prefixos lógicos apontando para o **mesmo arquivo físico** — padrão já usado em [ModUpdater.cs:428](../../../../mods/TarkovRedLine4.0/Server/TarkovRedLine.Server/Controllers/ModUpdater.cs#L428). O `-ref` é espelhado no cliente; o outro é aplicado em `config/` e nunca materializado.
- Arquivo sob `config-performance/` **sem** `performanceId` → erro de conteúdo: o servidor loga e **não** emite (senão vira config que o player não consegue desligar).

### 2.6 Servidor C#

| # | Mudança |
|---|---|
| S-1 | `GetPerformancePath()` ([:50](../../../../mods/TarkovRedLine4.0/Server/TarkovRedLine.Server/Controllers/ModUpdater.cs#L50)) → `mods_repo/BepInEx/config-performance` (D-9) |
| S-2 | 🔴 No scan do `mods_repo` (`:455-463`), pular **apenas os arquivos de metadados** (`plugins-optional.json`, `performance.json`). Os arquivos de `config-performance/` **entram** no manifesto — governados pela `folderRule`, nunca por `Default`. *(A v1 mandava pular a pasta inteira, o que esvaziaria o canal — PA-02-03.)* |
| S-3 | Emitir o **segundo prefixo lógico** `config-performance-ref/<rel>` para os mesmos arquivos físicos (D-18), populando `_fileMapCache` para ambos |
| S-4 | Ler `plugins-optional.json` e `performance.json`; emitir `optionalMods[]`/`performanceItems[]` e taggear `optionalId`/`performanceId` nos `files[]` |
| S-5 | **Validações de conteúdo:** recusar `paths` sob `user/mods/` (D-15/CA-030.8b); recusar arquivo em **dois** itens (D-19/CC-4/CC-5); avisar quando o mesmo arquivo está em `config-force` **e** `config-performance` (RN-2) |
| S-6 | `folderRules` default (`:493-523`) ganha as 2 entradas novas; e passa a ser **emitido explicitamente** no manifesto (ver §7 R-11) |
| S-7 | **Remover:** `ScanOptionalGroups` (`:365-435`), rotas `optionals-list`/`optionals-manifest`/`optional-download` (`:226-335`), rota `performance-download` (`:195-218`) e `_performanceFileMapCache` (`:24`) — todos órfãos com D-13 |

## 3. Propriedades F12 · Pontos de patch Harmony

**N/A** — launcher standalone (Avalonia), sem Harmony e sem BepInEx. Config do usuário em `SPT/user/launcher/config.json` ([LauncherSettingsProvider.cs:24](../../project/SPT.Launcher.Base/Helpers/LauncherSettingsProvider.cs#L24)), tratada em §5.5.

## 4. Arquivos

### CRIAR
`ViewModels/ModsConfigsViewModel.cs` · `Views/ModsConfigsView.axaml(.cs)` · `ViewModels/OptionalItemToggle.cs` · `ViewModels/Dialogs/OnboardingDialogViewModel.cs`(+View) · `Models/Launcher/OptionalModDefinition.cs` · `Models/Launcher/PerformanceItemDefinition.cs` · testes: `SyncPerformanceChannelTests`, `SyncOptionalQuarantineTests`, `SyncDisabledNamespaceTests`

### MODIFICAR
`SyncFolderRule.cs` (E-1,E-2) · `SyncRuleResolver.cs` (E-3) · `SyncPathUtil.cs` (E-4,E-5) · `SyncPlanner.cs` (E-6..E-11) · `SyncPlan.cs` (E-12) · `SyncEngine.cs` (E-13) · `SyncReport.cs`/`SyncResult.cs` (E-14..E-16) · `SyncPlannerOptions.cs` (§1.2) · `ManifestFile.cs` (§2.5) · `LauncherSettingsProvider.cs` (§5.5, `EXPECTED_CONFIG_VERSION` 4→5) · `ProfileViewModel.cs` · `ModUpdateViewModel.cs` · `ProfileView.axaml` · `SettingsView.axaml` (remover `:127-142`) · `SettingsViewModel.cs` (remover `UsePerformanceConfigs` `:59-71`) · `LocalizationProvider.cs` + 2 JSONs · `ModUpdater.cs`

### REMOVER
`OptionalModsHelper.cs` · `OptionalGroupApplier.cs` · `OptionalOpResult.cs` · `SyncManifestOverlay.cs` · `OptionalModToggle.cs` · `OptionalGroupApplierTests.cs` · `SyncOverlayTests.cs` · `RequestHandler`: `RequestOptionalsList` (`:249`), `RequestOptionalsManifest` (`:229`), `DownloadOptionalFile` (`:218`), **`DownloadPerformanceFile` (`:240`)**

🔴 **`DownloadModFile` ([RequestHandler.cs:207](../../project/SPT.Launcher.Base/Controllers/RequestHandler.cs#L207)) NÃO sai** — é o downloader base de **todo** o sync (`ProfileViewModel:893`, `ModUpdateViewModel:416`). Remover derruba a sincronização inteira.

**Call-sites órfãos fora dos ranges** (PA-02-13), a limpar explicitamente: `ProfileViewModel.cs:770` (`ProtectedPaths = OptionalModsHelper.GetAllKnownOptionalPaths()` → passa a vir das definições novas) e `:71` (`OptionalModToggle`).

### 4.1 Fonte das contagens do resumo (CA-030.13)

| Situação | Resumo mostra |
|---|---|
| Manifesto lido nesta sessão, **com** itens | Contagens reais por eixo |
| Manifesto lido, **zero** itens | Oculto (CA-030.15b) |
| Antes do 1º sync, com preferências salvas | Conta pelas preferências persistidas |
| Sem manifesto e sem preferências | Oculto — o onboarding conduz |
| Sync falhou | Última contagem conhecida |

**Nunca exibir "0 de 0"** por ausência de dado.

## 5. Stubs

### 5.1 Regra e origem da quarentena

```csharp
// SyncFolderRule.cs
/// <summary>
/// config-performance → config quando o item está LIGADO. Vence config-force e config (D-1).
/// Híbrido (§1.1): no momento em que o player alterna, aplica/remove mesmo divergente;
/// nos syncs de rotina respeita a customização via baseline. NÃO é clone do ForceToConfig.
/// </summary>
PerformanceToConfig = 7,

// SyncPathUtil.cs
/// <summary>Origem da quarentena (D-14/D-20). MirrorExtra mantém o formato legado
/// (sem subpasta) — os extras do ScanExtras não colidem com nada e mudá-los quebraria
/// testes em produção sem ganho. Force idem: manter na raiz evita partir os backups
/// já existentes em dois lugares.</summary>
public enum DisabledOrigin { MirrorExtra, Force, Optional, PerformanceReplaced, PerformanceRemoved }

private static string OriginSegment(DisabledOrigin origin) => origin switch
{
    DisabledOrigin.Optional            => "optional",
    DisabledOrigin.PerformanceReplaced => "performance/replaced",
    DisabledOrigin.PerformanceRemoved  => "performance/removed",
    _                                  => null,   // MirrorExtra e Force: raiz (formato legado)
};

// ref: SyncPathUtil.cs:151 — era: prefix + "-disabled/" + remainder
private static string BuildDisabledPath(string prefix, string remainder, DisabledOrigin origin)
{
    string seg = OriginSegment(origin);
    return seg == null
        ? prefix + "-disabled/" + remainder
        : prefix + "-disabled/" + seg + "/" + remainder;
}
```

### 5.2 Opções do planner

```csharp
// SyncPlannerOptions.cs
/// <summary>Item do eixo MODS está ligado? (rename de IsOptionalGroupEnabled)</summary>
public Func<string, bool> IsOptionalModEnabled { get; set; } = _ => false;

/// <summary>Item do eixo PERFORMANCE está ligado? Não existia na v1 (PA-02-01) —
/// sem isso o planner não consegue distinguir ligado de desligado.</summary>
public Func<string, bool> IsPerformanceItemEnabled { get; set; } = _ => false;

/// <summary>Ids alternados pelo player e ainda não aplicados (vem de PendingApply,
/// persistido). Para eles a aplicação/remoção é explícita: ignora divergência,
/// sempre com quarentena. Vazio = sync de rotina.</summary>
public IReadOnlyCollection<string> JustToggledIds { get; init; } = Array.Empty<string>();
```

### 5.3 Branch do canal de performance (E-9) — ligado **e** desligado

```csharp
// SyncPlanner.cs — após o branch ForceToConfig (:235)
if (rule == SyncFolderRule.PerformanceToConfig)
{
    string itemId      = file.performanceId;
    string targetRel   = SyncPathUtil.DeriveSeedTarget(file.path, matchedPrefix); // ref: :163
    string targetLocal = SyncPathUtil.ToLocalPath(_options.GameRoot, targetRel);
    bool   enabled     = _options.IsPerformanceItemEnabled(itemId);
    bool   justToggled = _options.JustToggledIds.Contains(itemId);
    bool   exists      = File.Exists(targetLocal);

    string localHash = exists
        ? await Task.Run(() => SyncPathUtil.ComputeMd5(targetLocal), cancellationToken)
        : null;
    bool matchesBaseline = _baseline.TryGetHash(targetRel, out var baseHash)
                           && string.Equals(baseHash, localHash, StringComparison.OrdinalIgnoreCase);

    // ---------- ITEM LIGADO ----------
    if (enabled)
    {
        if (exists && string.Equals(localHash, file.hash, StringComparison.OrdinalIgnoreCase))
            continue;                                   // já aplicado — no-op

        // ref: PA-01-06/CC-19 — ação explícita ANTES do Dev Mode: o Dev Mode protege contra
        // reversão automática, não contra um toggle que o próprio usuário acabou de clicar.
        if (justToggled || !exists || matchesBaseline)
        {
            plan.Actions.Add(new SyncAction
            {
                RelativePath       = file.path,      // fonte: config-performance/<rel>
                SeedTargetRelative = targetRel,      // destino: config/<rel>
                MoveTargetRelative = exists          // backup só se havia algo (D-20)
                    ? SyncPathUtil.DeriveDisabledBackup(
                        targetRel, matchedPrefix, SyncPathUtil.DisabledOrigin.PerformanceReplaced)
                    : null,
                Kind       = SyncActionKind.PerformanceCopy,
                Rule       = rule,
                ServerHash = file.hash,
                Reason     = justToggled
                    ? "performance ligada (sua config anterior foi preservada na quarentena)"
                    : "performance (atualizada pelo servidor)",
            });
            continue;
        }

        if (_options.DevMode)                          // ref: :199 — rotina, CC-14
        {
            plan.Actions.Add(new SyncAction { RelativePath = targetRel, Kind = SyncActionKind.PreserveDevMode,
                Rule = rule, ServerHash = file.hash, Reason = "Dev Mode: config de performance preservada" });
            continue;
        }

        plan.Actions.Add(new SyncAction { RelativePath = targetRel, Kind = SyncActionKind.PreserveCustomized,
            Rule = rule, ServerHash = file.hash, Reason = "você customizou — sua versão foi mantida" });
        continue;
    }

    // ---------- ITEM DESLIGADO (CA-030.2b / CA-030.5 — ausente na v1) ----------
    if (!exists) continue;

    if (!matchesBaseline)
    {
        // Customizado desde a aplicação: a edição do player prevalece (RN-3/CC-3).
        plan.Actions.Add(new SyncAction { RelativePath = targetRel, Kind = SyncActionKind.PreserveCustomized,
            Rule = rule, ServerHash = file.hash,
            Reason = "você customizou — reversão da performance foi pulada" });
        continue;
    }

    // Intocado. Se há versão base (config/ ou config-force/) o sync normal a restaura nesta
    // mesma passada; se o arquivo só existe em performance, sai para a quarentena (D-8).
    if (!_baseSourcePaths.Contains(SyncPathUtil.Normalize(targetRel)))
    {
        plan.Actions.Add(new SyncAction
        {
            RelativePath       = targetRel,
            MoveTargetRelative = SyncPathUtil.DeriveDisabledBackup(
                targetRel, matchedPrefix, SyncPathUtil.DisabledOrigin.PerformanceRemoved),
            Kind   = SyncActionKind.MoveToDisabled,
            Rule   = rule,
            Reason = "performance desligada (arquivo sem versão base — movido para a quarentena)",
        });
    }
    continue;
}
```

> `_baseSourcePaths` = conjunto dos alvos que **têm** origem em `config/` ou `config-force/`, montado no mesmo pre-pass de E-8. Sem ele não dá para distinguir "reverte" de "remove".

### 5.4 Engine: o `PerformanceCopy` grava baseline (§1.1)

```csharp
// SyncEngine.cs — novo case, espelhando ForceCopy (:207-281) com UMA diferença central
case SyncActionKind.PerformanceCopy:
{
    string destination = ResolveUnderRoot(action.SeedTargetRelative);

    if (File.Exists(destination) && !string.IsNullOrEmpty(action.MoveTargetRelative))
    {
        string backupRel = ResolveFreeBackupRelative(action.MoveTargetRelative, destination); // ref: :240
        Directory.CreateDirectory(Path.GetDirectoryName(ResolveUnderRoot(backupRel)));
        File.Copy(destination, ResolveUnderRoot(backupRel), overwrite: true);
        result.ConfigsBackedUp++;
    }

    byte[] data = await downloader(action.RelativePath, cancellationToken);
    SyncFileOps.WriteAtomic(destination, data);

    // 🔴 A DIFERENÇA vs ForceCopy (PA-02-04): sem isto o híbrido não converge — o próximo
    // sync veria "sem baseline" e trataria como customizado para sempre (SyncPlanner.cs:273).
    _baseline.SetHash(action.SeedTargetRelative, SyncPathUtil.ComputeMd5(data));

    result.PerformanceApplied++;
    AddEntry(result, action.SeedTargetRelative, "performance-applied", action.Reason);
    break;
}
```

### 5.5 Preferências persistidas

```csharp
// LauncherSettingsProvider.cs — class Settings (ref: :190-195, mesmo padrão de EnabledOptionals)
private Dictionary<string, bool> _enabledPerformanceItems = new();
public Dictionary<string, bool> EnabledPerformanceItems { get => _enabledPerformanceItems; set => SetProperty(ref _enabledPerformanceItems, value); }

/// <summary>CA-030.16b / D-17 — fonte de verdade do onboarding; o estado do disco não decide.</summary>
private bool _modsConfigsOnboardingDone;
public bool ModsConfigsOnboardingDone { get => _modsConfigsOnboardingDone; set => SetProperty(ref _modsConfigsOnboardingDone, value); }

/// <summary>PA-01-05/CC-20 — ids alternados ainda não aplicados. PERSISTIDO (sem [JsonIgnore],
/// ao contrário de PendingOptionalChanges em :201): falha, cancelamento ou fechar o launcher
/// no meio não perdem a intenção. Um id sai daqui só quando a ação conclui com sucesso.</summary>
private List<string> _pendingApply = new();
public List<string> PendingApply { get => _pendingApply; set => SetProperty(ref _pendingApply, value); }

/// <summary>CA-030.11/D-6 — ids já apresentados ao player. Item do servidor que não está
/// aqui é "novo": ganha marcador até ele SAIR da tela (não ao abri-la).</summary>
private List<string> _seenItemIds = new();
public List<string> SeenItemIds { get => _seenItemIds; set => SetProperty(ref _seenItemIds, value); }

public bool IsPerformanceItemEnabled(string itemId) =>
    !string.IsNullOrEmpty(itemId) && EnabledPerformanceItems.TryGetValue(itemId, out bool v) && v;

public void SetPerformanceItemEnabled(string itemId, bool enabled)
{
    if (string.IsNullOrEmpty(itemId)) return;
    EnabledPerformanceItems[itemId] = enabled;
    SaveSettings();
}
```

🔴 **`EXPECTED_CONFIG_VERSION` ([:67](../../project/SPT.Launcher.Base/Helpers/LauncherSettingsProvider.cs#L67), hoje `4`) → `5`.** `LoadSettings` (`:68-72`) força re-save quando a versão salva é menor, gravando os campos novos com default sem perder dados do jogador.

### 5.6 Sair da tela (PA-01-03 + correção do PA-02-07)

A orquestração de sync fica **exclusivamente** no `ProfileViewModel`, com o `_syncGate` ([:535](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L535)) como único ponto de serialização (CC-15).

```csharp
// ModsConfigsViewModel.cs
private void SaveAndReturn()
{
    var settings = LauncherSettingsProvider.Instance;
    var changed  = CollectChangedItems();          // diff estado inicial × final (CC-18)

    foreach (var item in changed)
    {
        if (item.IsPerformance) settings.SetPerformanceItemEnabled(item.Id, item.IsEnabled);
        else                    settings.SetOptionalEnabled(item.Id, item.IsEnabled);
        if (!settings.PendingApply.Contains(item.Id)) settings.PendingApply.Add(item.Id);
    }

    // 🔴 PA-02-07: gravar SEMPRE, fora do early-return. No onboarding o player que aceita os
    // defaults sai com changed == 0 — se a marca ficasse depois do return, o modal voltaria a
    // cada login (CA-030.20) e a primeira ingestão nunca rodaria (CA-030.19).
    foreach (var id in AllVisibleIds())
        if (!settings.SeenItemIds.Contains(id)) settings.SeenItemIds.Add(id);   // CA-030.11

    bool firstRun = !settings.ModsConfigsOnboardingDone;
    if (firstRun)
    {
        settings.ModsConfigsOnboardingDone = true;
        foreach (var item in AllItems())            // persiste o estado inicial aceito
            if (item.IsPerformance) settings.EnabledPerformanceItems[item.Id] = item.IsEnabled;
            else                    settings.EnabledOptionals[item.Id]        = item.IsEnabled;
    }

    settings.SaveSettings();

    // CA-030.22: fora do onboarding, sem mudança não dispara sync. No primeiro acesso
    // dispara sempre (CA-030.19) — é a ingestão que instala os mods.
    if (changed.Count > 0 || firstRun) settings.PendingApply.Add(SyncTriggers.InitialIngest);

    NavigateBack();   // ref: ViewModelBase.cs:132
}
```

No `ProfileViewModel`, na ativação (mesmo ponto que já re-raise `CanStartGame`, [:223-230](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L223-L230)): se `PendingApply` não estiver vazio, `CanStartGame` (jogo fechado — CA-030.23) e `!IsSyncRunning`, chama `CheckForUpdates()`. Se o sync do login ainda roda, a pendência **persiste** e é retomada ao fim dele — o `finally` de `CheckForUpdatesCore` (`:872-881`) reavalia.

### 5.7 Chaves i18n

Lista fechada (o loader é all-or-nothing — uma chave faltando derruba o locale inteiro):

`nav_mods_configs` · `mods_configs_title` · `mods_configs_intro` · `mods_configs_optional_column` · `mods_configs_performance_column` · `mods_configs_toggle_all` · `mods_configs_empty_optional` · `mods_configs_empty_performance` · `mods_configs_new_badge` · `mods_configs_summary_format` · `mods_configs_game_running` · `onboarding_title` · `onboarding_body` · `onboarding_ok` · `report_performance_applied` · `report_performance_reverted` · `report_performance_suppressed_force` · `report_optional_disabled`

> Os labels do relatório (`report_*`) exigem que `SyncReport.ActionDescricao` deixe de ser PT hardcoded ou que a tradução ocorra na UI. **Decisão:** o relatório é arquivo de diagnóstico — mantém PT no `detail`; as chaves `report_*` servem à **UI** que exibe o resumo.

## 6. Fluxo de dados

```
SERVIDOR — Launcher-Updater/mods_repo/BepInEx/
  plugins-optional.json ─┐  (metadado: NUNCA sincronizado — S-2)
  config-performance/    │
    performance.json ────┤  (metadado: idem)
    sombras.cfg ─────────┼──> emitido DUAS vezes (D-18):
                         │      config-performance/sombras.cfg      → performance-to-config
                         │      config-performance-ref/sombras.cfg  → mirror-reference
  plugins/TarkovIRL.dll ─┴──> files[] com optionalId
                                        │
  GenerateManifestAsync + validações S-5 ▼
     { files[], optionalMods[], performanceItems[], folderRules }
                                        │
LAUNCHER                                ▼
  SyncPlannerOptions { IsOptionalModEnabled, IsPerformanceItemEnabled, JustToggledIds }
                                        │
  SyncPlanner ── pre-pass: forceTargets + performanceTargets + baseSourcePaths (E-8)
    │          ── precedência performance > force > config (E-7/§2.3)
    │          ── LIGADO: PerformanceCopy   ·  DESLIGADO: reverte ou MoveToDisabled (§5.3)
    │          ── mod desligado: MoveToDisabled COM as guardas do ScanExtras (§2.4)
    ▼
  SyncEngine ── escreve config/<rel> · backup na quarentena por origem+natureza
    │          ── PerformanceCopy GRAVA BASELINE (§5.4) ← sem isto não converge
    ▼
  SyncReport ── performance-applied / -reverted / -suppressed-force
    ▼
  last-update.json
```

## 7. Riscos

| # | Risco | Mitigação |
|---|---|---|
| R-1 | Híbrido sem baseline não converge | §5.4 grava `SetHash`; teste de 2º sync sem I/O |
| R-2 | `ModUpdateViewModel` duplica a lógica do `ProfileViewModel` | Aplicar nos dois; item futuro extrai `SyncCoordinator` |
| R-3 | Remover `DownloadModFile` derruba o sync | Não remover (§4) |
| R-4 | `IoActionCount` sem o kind novo → barra e `Pending` errados; `ModUpdateViewModel:305` nunca aplica | E-12 + teste |
| R-5 | Quarentena de mod pula guardas (coop/Fika) | §2.4 — guardas compartilhadas, obrigatórias |
| R-6 | Filtro removido → loop download/quarentena | E-11: o filtro permanece |
| R-7 | Testes existentes | Inventário em §8; `SyncTestFixture.cs:81` **primeiro** (senão o assembly de testes não compila) |
| R-8 | Coop: escolhas divergentes entre clientes | CC-7 + G-5; conteúdo marca o que não pode ser opcional |
| R-9 | `EXPECTED_CONFIG_VERSION` não incrementado | §5.5 + teste de migração |
| R-10 | Onboarding avaliado depois do sync (CC-2) | Checagem no `MainWindowViewModel` antes de `CheckForUpdates` |
| R-11 | 🔴 **Skew de versão (PA-02-11).** O `config.json` de prod **não define `folderRules`**; todos usam o fallback do exe. Mover a pasta antes de os launchers atualizarem faz o cliente antigo resolver como `Default` e **baixar a pasta-fonte** para o jogo | **Duas travas:** (1) servidor passa a emitir `folderRules` explícito (S-6) — cliente antigo ignora regra desconhecida no `TryParse`; (2) **ordem de rollout**: publicar o launcher e confirmar adoção **antes** de mover a pasta no servidor |
| R-12 | Intenção perdida em falha | `PendingApply` persistido (§5.5) |
| R-13 | Bundles × cache 3D | **Pendente (D-21)** — verificar ao montar o conteúdo; G-9 condicional |

## 8. Checklist de implementação

**Fase 0 — destravar os testes**
1. `SyncTestFixture.cs:81` (`optionalGroup` → `optionalId`) — sem isso **todo** `SPT.Launcher.Tests` deixa de compilar.

**Fase 1 — motor** (testável sem UI)
2. E-1..E-4 (enum, parser, resolver ×4 entradas, sufixo). Teste: `config-performance/x.cfg` → `PerformanceToConfig` → alvo `config/x.cfg`; `config-performance-ref/x.cfg` → `MirrorReference`.
3. E-5/E-6 + `DisabledOrigin` (com `MirrorExtra`/`Force` na raiz). Teste: origens novas não colidem; **testes atuais de force e de extras seguem verdes**.
4. §1.2 + §5.2: as 3 opções do planner.
5. E-9/§5.3: branch completo (ligado **e** desligado). Testes: ligar sobre customizada aplica com backup; rotina preserva; servidor novo atualiza quem não customizou; **desligar reverte**; **desligar arquivo só-de-performance manda p/ quarentena**; Dev Mode não anula ação explícita.
6. E-13/§5.4: `PerformanceCopy` **com `SetHash`**. Teste de convergência: 2º sync com `IoActionCount == 0` (recria a garantia de `SteadyStateOn_SecondRunHasNoIoActions`, que morre com o overlay).
7. §2.4: quarentena de mod desligado **com as guardas**. Testes: move todos os paths do item; **não** move plugin coop-safe; **não** move o que está em `ProtectedPaths`; 2 syncs seguidos convergem (sem loop).
8. E-7/E-8/§2.3: precedência + `performance-suppressed-force`.
9. E-10, E-12, E-14..E-16.
10. Atualizar os testes de `SyncForceConfigTests` (só os que mudam de fato) e **endurecer os 2 falso-positivos** (`:148`, `:270` usam `Contains("config-disabled")`, que passaria mesmo com o motor errado). Remover `SyncOverlayTests` **só depois** que as 4 garantias dele existirem no canal novo (revert no OFF, preservação de customização, convergência, wedge sem baseline).
11. `dotnet build` + `dotnet test` verdes.

**Fase 2 — servidor**
12. S-1/S-2/S-3 (realocação, skip só de metadados, 2º prefixo lógico). Teste: manifesto sem os JSONs de metadado, **com** os arquivos de performance nos dois prefixos.
13. S-4 (leitura dos JSONs + tagging), S-5 (3 validações), S-6 (`folderRules` explícito — trava do R-11).
14. S-7 (remoções).

**Fase 3 — launcher/UI**
15. §5.5 + `EXPECTED_CONFIG_VERSION` → 5.
16. Tela + `OptionalItemToggle` + modal; `SaveAndReturn` conforme §5.6.
17. Resumo (§4.1) + item no sidebar + gatilho do `PendingApply` no `ProfileViewModel`.
18. i18n (§5.7) com paridade verificada.
19. Remover o modelo antigo (§4), nesta ordem: call-sites → helpers → testes órfãos.
20. `dotnet build` + `dotnet test` verdes · **`/code-review` antes de publicar**.

**Rollout** (R-11): publicar launcher → confirmar adoção → mover a pasta no servidor → configurar conteúdo (CA-030.26) → gates in-game.

## 9. Conformidade com skills

| # | Check | Status | Evidência |
|---|---|---|---|
| 1 | Refs ancoradas em `arquivo:linha` real | ✅ | §2 (16 E + 7 S), §4, §7 — verificadas nesta sessão e na review 02 |
| 2 | Sem invenção de API | ✅ | Enum real conferido (`SyncAction.cs:3-29`) — a v1 usava `SyncActionKind.Preserve`, inexistente; agora `PreserveCustomized` e o novo `PerformanceCopy` explicitamente nomeado |
| 3 | Lifecycle / ordem de inicialização | ✅ | R-10 (onboarding antes do sync) + §5.6 (pendência retomada no `finally` do sync em curso) |
| 4 | Estado entre execuções | ✅ | Baseline por path normalizado (`SyncBaseline.cs:62-76`); preferências por id + `SeenItemIds` + `PendingApply` (§5.5); `EXPECTED_CONFIG_VERSION` (R-9) |
| 5 | Coop / Fika | ✅ | §2.4 guard coop-safe obrigatório (R-5); RN-2 torna visível a supressão de config forçada; G-5 |
| 6 | Threading / UI | ✅ | Hash e I/O off-thread (`Task.Run`, §5.3); progresso via `Progress<SyncProgress>`; um único orquestrador (§5.6) |
| 7 | Não duplicar mecanismo | ✅ | Canal reusa `folderRules` e **aposenta** `SyncManifestOverlay` (D-13); guardas do `ScanExtras` extraídas, não copiadas (§2.4) |
| 8 | Escrita em disco protegida | ✅ | `ResolveUnderRoot`, `WriteAtomic`, quarentena em vez de delete (RN-4); `ContainsDisabledSegment` é por segmento, então cobre as subpastas novas (`ProfileViewModel.cs:716-725`) |
| 9 | Patches Harmony / F12 | N/A | §3 — launcher standalone |

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-19 | Guilherme | Criação (v1) — 16 pontos de extensão, 7 stubs |
| 2026-07-19 | Guilherme | Aplicados os 8 pontos da review 01 |
| 2026-07-20 | Guilherme | **Reescrita (v2)** após a review 02 (7 bloqueadores estruturais): eixo desligar criado; D-18 (espelho por 2 prefixos lógicos); resíduos do modelo antigo removidos; `PerformanceCopy` grava baseline; guardas do `ScanExtras` reusadas na quarentena; filtro de opcionais mantido; `SaveAndReturn` corrigido; D-14 revisado (force e extras permanecem na raiz); inventário de testes; ordem de rollout |
