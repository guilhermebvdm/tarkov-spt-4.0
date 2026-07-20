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

### Decisões da review 01 (2026-07-19)

| Ponto | Decisão |
|---|---|
| **PA-01-03** | **A tela não sincroniza.** Ela salva as preferências e devolve o player à aba Launcher; o `ProfileViewModel` detecta pendência e aplica, usando o `_syncGate` que já existe ([ProfileViewModel.cs:535](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L535)). O motor em produção **não é tocado** e CC-15 fica resolvido por construção — só existe um orquestrador. Alinhado com CA-030.19, que já descreve esse fluxo. A extração de um `SyncCoordinator` (que resolveria também a duplicação do `ModUpdateViewModel`, R-2) fica como **item futuro** |
| **PA-01-01** | Mod opcional desligado ganha ação **`MoveToDisabled` explícita** no planner (§2.3) — não depender do `ScanExtras`, que hoje protege esses arquivos |
| **PA-01-02** | Shape do manifesto **especificado** em §2.4, com `optionalId`/`performanceId` por arquivo |
| **PA-01-05** | Intenção de aplicar vira **`PendingApply` persistido** (§5.5), não conjunto em memória |
| **PA-01-06** | `justEnabled` é avaliado **antes** do Dev Mode: o Dev Mode protege contra reversão automática, não contra ação explícita do player (§5.4) |

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
| E-11 | [SyncPlanner.cs:66](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L66) | Filtro `!f.optional \|\| IsOptionalGroupEnabled(...)` | Repontar para `optionalId` (§2.4) **e** somar a quarentena explícita de §2.3 — o filtro sozinho não satisfaz CA-030.8 |
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

### 2.3 🔴 PA-01-01 — quarentena de mod desligado precisa de ação explícita

O motor **hoje faz o oposto** do que CA-030.8 pede. Em [SyncPlanner.cs:59-63](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L59-L63), `manifestPaths` inclui **todos** os arquivos do manifesto (opcionais ativos ou não), com o comentário explícito *"files of disabled optional groups are never treated as extras"* — proteção CC3 do item 007. O filtro de `filesToCheck` (`:65-67`) só remove o arquivo do download; como o path segue em `manifestPaths`, o `ScanExtras` não o toca e ele **fica onde está**.

Essa proteção era correta no modelo antigo (mod opcional vinha de `Opcionais/`, fora do `mods_repo` — não podia contar como extra). Com D-3 o mod passa a viver em `mods_repo/plugins/` como qualquer outro, e a premissa deixa de valer **para ele**.

**Desenho:** o planner emite ação explícita, em vez de delegar ao `ScanExtras`:

- para cada arquivo com `optionalId` cujo item está **desligado** e que **existe no disco** → `SyncActionKind.MoveToDisabled`, com `MoveTargetRelative = BuildDisabledTarget(path, prefix, DisabledOrigin.Optional)` (E-6/D-14);
- `manifestPaths` **continua** incluindo o path (a proteção CC3 segue válida — evita que o `ScanExtras` emita uma segunda ação para o mesmo arquivo);
- o filtro de `filesToCheck` continua removendo o arquivo do download.

Ganho colateral: a ação passa a aparecer no relatório (`moved-to-disabled`) e a contar em `IoActionCount` (E-12) — nada disso aconteceria pelo caminho do `ScanExtras`.

### 2.4 🔴 PA-01-02 — contrato do manifesto

Shape emitido por `GenerateManifestAsync` (S-7). Mantém o tagging por arquivo, que é o que o planner já sabe consumir, e soma as listas de metadados para a UI:

```jsonc
{
  "files": [
    { "path": "BepInEx/plugins/TarkovIRL.dll", "hash": "...", "size": 123,
      "optional": true, "optionalId": "tarkov-irl" },
    { "path": "BepInEx/config-performance/sombras.cfg", "hash": "...", "size": 45,
      "performanceId": "shadows-low" }
  ],
  "optionalMods":     [ { "id": "tarkov-irl",  "name": "...", "description": { "pt": "...", "en": "..." } } ],
  "performanceItems": [ { "id": "shadows-low", "name": "...", "description": { "pt": "...", "en": "..." } } ]
}
```

- `optionalGroup` → **`optionalId`** (rename semântico; `SyncPlanner.cs:66` sobrevive com ajuste mínimo). `ManifestFile` ([:14-15](../../project/SPT.Launcher.Base/Models/Launcher/ManifestFile.cs#L14-L15)) ganha `optionalId` e `performanceId` no lugar de `optionalGroup`.
- **`performanceId` resolve o `GroupIdOf(file)`** do stub §5.4 por leitura direta — o TODO da versão anterior está fechado.
- As duas listas de metadados alimentam a tela e o resumo (PA-01-07) sem o launcher precisar cruzar paths.
- Um arquivo sob `config-performance/` **sem** `performanceId` é erro de conteúdo: o servidor loga e **não** o emite (senão viraria config aplicada que o player não consegue desligar).

### 2.5 Servidor C#

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
| `ProfileViewModel.cs` | Remover o fluxo opcional (`:258-464`, `:667-693`, `:751-761`, `:891-901`); somar o resumo (CA-030.13, §4.1); gatilho do `PendingApply` na ativação (§5.7) |
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

### 4.1 Fonte das contagens do resumo (PA-01-07)

CA-030.13 exige contagens no resumo da tela logada, mas as definições (`optionalMods`/`performanceItems`, §2.4) chegam dentro do manifesto, lido só no fluxo de sync ([ProfileViewModel.cs:625](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L625)). Definição do comportamento:

| Situação | O que o resumo mostra |
|---|---|
| Manifesto já lido nesta sessão | Contagens reais: ligados / total, por eixo |
| Antes do primeiro sync, **com** preferências salvas | Conta a partir das preferências persistidas (`EnabledOptionals` / `EnabledPerformanceItems`) — o total é o número de ids conhecidos localmente |
| Sem manifesto **e** sem preferências (instalação nova) | Resumo **oculto** — o onboarding (D-4) é quem conduz nesse estado |
| Sync falhou (servidor offline) | Mantém a última contagem conhecida; nunca exibe `0 de 0` |

A regra que fecha o critério: **nunca exibir "0 de 0"** por ausência de dado — isso comunicaria "não há nada aqui", o oposto do convite ao clique que CA-030.13 pede. Estado vazio real (servidor sem itens) é outra coisa, e já está coberto por CA-030.15b.

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

    // ref: PA-01-06 — ORDEM IMPORTA: a ação explícita do player é avaliada ANTES do Dev Mode.
    // O Dev Mode protege contra reversão AUTOMÁTICA (sync de rotina não pode desfazer a build
    // local do dev); ele não deve anular um toggle que o próprio usuário acabou de clicar —
    // senão a tela "não funciona" justamente na máquina de quem desenvolve o servidor.
    if (justEnabled)
    {
        // AÇÃO EXPLÍCITA (D-16/CA-030.2): aplica mesmo divergente, com backup.
        AddPerformanceAction(plan, file, perfTargetRel, perfBackupRel, rule,
            "performance ligada (a sua config vai p/ config-disabled/performance e é substituída)");
        continue;
    }

    // ref: SyncPlanner.cs:199 — Dev Mode é o escape hatch do sync de ROTINA (CC-14/CC-19)
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
            // ref: PA-01-04 — o enum NÃO tem 'Preserve'; o valor correto é PreserveCustomized
            // (SyncAction.cs:9), que é o que SyncEngine.cs:66 e SyncPlan.cs:38 reconhecem.
            RelativePath = perfTargetRel,
            Kind = SyncActionKind.PreserveCustomized,
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

/// <summary>
/// ref: PA-01-05 — ids alternados pelo player que ainda NÃO foram aplicados com sucesso.
/// PERSISTIDO de propósito (sem [JsonIgnore], ao contrário de PendingOptionalChanges em :201):
/// se o sync falhar, for cancelado ou o launcher fechar no meio, a intenção sobrevive e o próximo
/// sync retenta sozinho. Sem isso o item ficaria "ligado" mas nunca aplicado — e, como já não
/// estaria em ForceApplyGroups, o preserve-divergent poderia preservá-lo assim para sempre.
/// Um id só sai daqui quando a ação dele conclui com sucesso no SyncResult.
/// </summary>
private List<string> _pendingApply = new();
public List<string> PendingApply
{
    get => _pendingApply;
    set => SetProperty(ref _pendingApply, value);
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

### 5.7 Aplicação ao sair da tela (PA-01-03) — a tela **não** sincroniza

A orquestração de sync permanece **exclusivamente** no `ProfileViewModel`, com o guard `_syncGate` ([ProfileViewModel.cs:535](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L535)) continuando a ser o único ponto de serialização — é isso que resolve CC-15 por construção, em vez de criar uma terceira cópia da lógica (a segunda, `ModUpdateViewModel`, já é a dívida R-2).

```csharp
// SPT.Launcher/ViewModels/ModsConfigsViewModel.cs

/// <summary>
/// Sair da tela NÃO sincroniza: grava as escolhas + a intenção pendente e devolve o player
/// à aba Launcher, onde o ProfileViewModel aplica (CA-030.19/CA-030.21).
/// Sem alteração pendente, nada é gravado e nenhum sync é disparado (CA-030.22).
/// </summary>
private void SaveAndReturn()
{
    var changed = CollectChangedItems();   // diff estado inicial × final (CC-18)
    if (changed.Count == 0)
    {
        NavigateBack();                     // ref: ViewModelBase.cs:132
        return;
    }

    var settings = LauncherSettingsProvider.Instance;
    foreach (var item in changed)
    {
        if (item.IsPerformance) settings.SetPerformanceItemEnabled(item.Id, item.IsEnabled);
        else                    settings.SetOptionalEnabled(item.Id, item.IsEnabled);

        if (!settings.PendingApply.Contains(item.Id))
            settings.PendingApply.Add(item.Id);   // ref: PA-01-05
    }

    settings.ModsConfigsOnboardingDone = true;    // CA-030.20 / D-17
    settings.SaveSettings();

    NavigateBack();   // ProfileViewModel detecta PendingApply não-vazio e chama CheckForUpdates
}
```

No `ProfileViewModel`, o gatilho entra na ativação da tela (o mesmo ponto que já re-raise `CanStartGame`, [ProfileViewModel.cs:223-230](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L223-L230)): se `PendingApply` não estiver vazio e não houver sync em curso (`IsSyncRunning`), dispara `CheckForUpdates()`. O `_syncGate` cobre o caso de o sync automático do login ainda estar rodando — a chamada simplesmente não entra, e a pendência persiste para a próxima (PA-01-05).

### 5.8 Chaves i18n previstas (PA-01-08)

Lista fechada para conferência de paridade (o loader é all-or-nothing — uma chave faltando derruba o locale inteiro):

| Chave | pt | en |
|---|---|---|
| `nav_mods_configs` | Mods e Configs | Mods and Configs |
| `mods_configs_title` | Mods e Configs | Mods and Configs |
| `mods_configs_intro` | *(texto introdutório da tela)* | *(idem)* |
| `mods_configs_optional_column` | Mods opcionais | Optional mods |
| `mods_configs_performance_column` | Configs de performance | Performance configs |
| `mods_configs_toggle_all` | Ativar todos | Enable all |
| `mods_configs_empty_optional` | Nenhum mod opcional disponível | No optional mods available |
| `mods_configs_empty_performance` | Nenhuma config de performance disponível | No performance configs available |
| `mods_configs_new_badge` | Novo | New |
| `mods_configs_summary_format` | {0} de {1} mods · Performance: {2} de {3} | {0} of {1} mods · Performance: {2} of {3} |
| `mods_configs_game_running` | Feche o jogo antes de aplicar | Close the game before applying |
| `onboarding_title` | Configure para a sua máquina | Set up for your machine |
| `onboarding_body` | *(texto do modal, D-4)* | *(idem)* |
| `onboarding_ok` | Entendi | Got it |
| `report_performance_applied` | config de performance aplicada | performance config applied |
| `report_performance_suppressed` | performance sobrepôs uma config forçada | performance overrode a forced config |
| `report_optional_disabled` | mod opcional desativado (movido para quarentena) | optional mod disabled (moved to quarantine) |

> Os textos longos (`mods_configs_intro`, `onboarding_body`) ficam a definir na implementação — o conteúdo é do usuário, mas as **chaves** já estão fixadas aqui.

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
| R-11 | 🔴 **Quarentena de mod desligado**: o motor hoje faz o oposto (protege do `ScanExtras`, `SyncPlanner.cs:59-63`) | §2.3 — ação `MoveToDisabled` explícita; teste dedicado + G-1 |
| R-12 | Concorrência tela × sync do login | §5.7 — a tela não sincroniza; `_syncGate` segue o único ponto de serialização |
| R-13 | Intenção de aplicar perdida em falha/cancelamento | §5.5 — `PendingApply` persistido, com id saindo só no sucesso |

## 8. Checklist de implementação

**Fase 1 — motor (isolado, testável sem UI)**
1. E-1..E-4: enum, parser, resolver, sufixo. Teste: `config-performance/x.cfg` resolve para `PerformanceToConfig` e deriva `config/x.cfg`.
2. E-5/E-6 + `DisabledOrigin`: namespace da quarentena. Teste: três origens homônimas coexistem (G-7).
3. `SyncPlannerOptions.ForceApplyGroups` + branch E-9 (híbrido). Testes: ligar sobre customizada aplica com backup; sync de rotina preserva; servidor novo atualiza quem não customizou; **Dev Mode ligado não anula a ação explícita** (PA-01-06/CC-19).
3b. **§2.3 — quarentena explícita de mod desligado.** Teste: mod desligado com arquivos no disco → ação `MoveToDisabled` emitida para **todos** os `paths`, nenhum arquivo remanescente, e o `ScanExtras` **não** emite ação duplicada para os mesmos paths.
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
