# 034 — Quarentena move a pasta do mod inteira + faxina de vazias · Spec técnica

**Mod:** Launcher4.0-v2
**Criado:** 2026-08-01
**Spec funcional:** [034-quarentena-mover-pasta-do-mod-01-spec.md](./034-quarentena-mover-pasta-do-mod-01-spec.md)

> Fonte primária = o próprio código do launcher em `project/` (papel do `modded/`); upstream intocado em `launcher/Launcher4.0/` (🥈). Não há Assembly EFT, Harmony, F12 nem lifecycle de raid neste item — é motor de sync puro (`SPT.Launcher.Base/Sync/`).

## 1. Estratégia

Hoje a quarentena é **por arquivo**: `QuarantineDisabledOptionalMods` ([SyncPlanner.cs](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs)) e `ScanExtras` emitem uma ação `MoveToDisabled` por arquivo; o engine ([SyncEngine.cs:152](../../project/SPT.Launcher.Base/Sync/SyncEngine.cs#L152)) faz `File.Move` em cada uma via `MoveWithOverwrite`. O destino já preserva a subestrutura (`plugins-disabled/optional/PiP-Disabler/…`), mas a pasta-pai na origem fica como **casca vazia**.

A mudança tem **duas frentes**, ambas no motor (`SPT.Launcher.Base/Sync/`), zero no servidor:

1. **Consolidação por pasta (detecção por disco).** Um passo novo no planner, `ConsolidateFolderMoves`, roda **depois** de `QuarantineDisabledOptionalMods` + `ScanExtras`: agrupa as ações `MoveToDisabled` pela **pasta de 1º nível** sob cada root de espelho-com-quarentena (`plugins/`, `patchers/`) e, para cada pasta cujo **conteúdo real no disco** pode ir inteiro (nenhum arquivo precisa ficar — §5.2 `FileMustStay`), **substitui** as N ações por arquivo por **uma** ação nova `MoveDirToDisabled` (pasta inteira, `Directory.Move`). Se algum arquivo precisa ficar (protegido coop-safe, mod ligado na mesma pasta, `-disabled`), mantém o modo por-arquivo — cobre o fallback coop-safe (CA-034.4) e a pasta compartilhada (CC-1) de graça. Detectar pelo disco, e não por catálogo do servidor, evita acoplar launcher↔servidor (o manifesto não emite os `paths` do mod — confirmado em `ModUpdater.LoadOptionalDefs`, que só emite `id/name/description/category`) e leva junto os arquivos que o mod gerou em runtime (não catalogados).

2. **Faxina de pastas vazias (pós-passo no engine).** Após aplicar o plano, o engine varre os roots de espelho-com-quarentena e remove pastas vazias (bottom-up), sem gerar entradas no relatório (CA-034.7). É guiada por `SyncPlan.EmptyDirCleanupRoots`, que o planner **só popula fora do Dev Mode** (CC-3) — em Dev Mode a lista fica vazia e o engine não faxina nada.

A ação de pasta gera **uma** entrada agregada `moved-to-disabled` no relatório (CA-034.7); a faxina é silenciosa.

## 2. Pontos de extensão

| Ponto | Local | Papel |
|---|---|---|
| `QuarantineDisabledOptionalMods` | [SyncPlanner.cs:~460](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs) | Já emite `MoveToDisabled` por arquivo p/ mod opcional desligado (origem `Optional`). **Inalterado** — a consolidação roda depois. |
| `ScanExtras` | [SyncPlanner.cs:476](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L476) | Já emite `MoveToDisabled` por arquivo p/ extra sob `mirror-move-disabled` (origem `MirrorExtra`). **Inalterado.** |
| `BuildPlanAsync` (fim, antes do `return plan`, [:464-468](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L464)) | SyncPlanner | Chamar `ConsolidateFolderMoves` + popular `EmptyDirCleanupRoots` (fora do Dev Mode). |
| `ResolveOnDiskCasing` | [SyncPlanner.cs:594](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L594) | Reusado p/ os roots da faxina (casing visível). |
| `_resolver.MirrorPrefixes` | [SyncRuleResolver.cs:90](../../project/SPT.Launcher.Base/Sync/SyncRuleResolver.cs#L90) | Fonte dos roots (`MirrorMoveDisabled`) p/ consolidação e faxina. |
| `ExecuteAsync` (dentro do try, logo após o foreach de ações, [:346](../../project/SPT.Launcher.Base/Sync/SyncEngine.cs#L346)) | SyncEngine | Chamar `CleanupEmptyDirectories(plan.EmptyDirCleanupRoots)` — pulada no cancelamento (PA-01-04). |
| `case MoveToDisabled` | [SyncEngine.cs:152](../../project/SPT.Launcher.Base/Sync/SyncEngine.cs#L152) | Modelo do novo `case MoveDirToDisabled`. |
| `SyncCoopSafe.IsCoopEssentialPlugin` | [SyncPlanner.cs (guardas)](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs) | Reusado em `FileMustStay`. |

## 3. Propriedades F12 · Harmony

`N/A` — launcher Avalonia; não há Assembly EFT, Harmony nem ConfigEntry F12.

## 4. Arquivos

### MODIFICAR

| Arquivo | Mudança |
|---|---|
| `Sync/SyncActionKind.cs` | Novo valor `MoveDirToDisabled` (move de pasta inteira). |
| `Sync/SyncPlan.cs` | `MoveDirCount`; incluir em `IoActionCount`; nova lista `EmptyDirCleanupRoots`. |
| `Sync/SyncPlanner.cs` | Novo `ConsolidateFolderMoves` + helpers `FirstLevelFolder`, `FileMustStay`, `DeriveFolderDisabledTarget`; chamada + popular `EmptyDirCleanupRoots` (fora do Dev Mode) no fim de `BuildPlanAsync`. |
| `Sync/SyncEngine.cs` | `case MoveDirToDisabled`; helpers `MoveDirectoryMerge`, `CleanupEmptyDirectories`, `RemoveEmptyDirsBottomUp`; chamada da faxina no fim do loop. |
| `Sync/SyncActionKind.cs` (doc) / `SyncReport.cs` | Garantir que o label agregado da pasta use o mesmo `"moved-to-disabled"` já mapeado no relatório (item 031). Sem novo label. |

### CRIAR

| Arquivo | Conteúdo |
|---|---|
| `SPT.Launcher.Tests/Sync/SyncFolderQuarantineTests.cs` | Testes da consolidação, do fallback coop-safe, do `.dll` solto, do `paths` misto, da faxina e do merge (CC-4). |

## 5. Stubs

### 5.1 Novo action kind + plano

```csharp
// SyncActionKind.cs
/// <summary>
/// Item 034: move a PASTA inteira de um mod para a quarentena de uma vez (Directory.Move),
/// em vez de arquivo a arquivo — RelativePath = pasta de origem, MoveTargetRelative = pasta destino.
/// Evita deixar a pasta-pai vazia na origem.
/// </summary>
MoveDirToDisabled,
```

```csharp
// SyncPlan.cs
/// <summary>Item 034: roots de espelho-com-quarentena a varrer p/ remover pastas vazias no fim do
/// sync. Vazio em Dev Mode (a faxina não roda). Populado pelo planner.</summary>
public List<string> EmptyDirCleanupRoots { get; } = new List<string>();

public int MoveDirCount => Actions.Count(a => a.Kind == SyncActionKind.MoveDirToDisabled);

// IoActionCount passa a somar MoveDirCount:
public int IoActionCount => DownloadCount + DeleteCount + MoveCount + MoveDirCount
                          + SeedCount + ForceCount + OptionalConfigCount;
```

### 5.2 Consolidação por pasta (planner)

```csharp
// SyncPlanner.cs — chamado no fim de BuildPlanAsync, DEPOIS de ScanExtras:
//   ConsolidateFolderMoves(plan, manifestPaths, cancellationToken);
//   // Faxina (item 034): roots MirrorMoveDisabled EXISTENTES, sem duplicata, fora do Dev Mode
//   // (CC-3 + PA-01-06 — MirrorPrefixes traz plugins E bepinex/plugins; 2 não existem no install real).
//   if (!_options.DevMode)
//       foreach (var kv in _resolver.MirrorPrefixes.Where(p => p.Value == SyncFolderRule.MirrorMoveDisabled))
//       {
//           string root = ResolveOnDiskCasing(kv.Key);
//           string abs = SyncPathUtil.ToLocalPath(_options.GameRoot, root);
//           if (Directory.Exists(abs) && !plan.EmptyDirCleanupRoots.Contains(root))
//               plan.EmptyDirCleanupRoots.Add(root);
//       }

/// <summary>
/// Item 034: onde a pasta inteira de um mod pode ir para a quarentena, troca as N ações
/// MoveToDisabled por-arquivo por UMA MoveDirToDisabled. Só consolida quando (a) todas as ações do
/// grupo apontam para o MESMO destino de pasta (origem única — PA-01-02) e (b) o conteúdo REAL da
/// pasta no disco pode ir inteiro (nenhum arquivo precisa ficar — FileMustStay). Cobre fallback
/// coop-safe (CA-034.4) e pasta compartilhada (CC-1): basta um arquivo que fica → mantém per-file.
/// </summary>
private void ConsolidateFolderMoves(SyncPlan plan, HashSet<string> manifestPaths, CancellationToken ct)
{
    var moveActions = plan.Actions.Where(a => a.Kind == SyncActionKind.MoveToDisabled).ToList();
    if (moveActions.Count == 0) return;

    // Conjunto dos arquivos que ESTÃO indo para a quarentena (por RelativePath normalizado).
    var moving = new HashSet<string>(
        moveActions.Select(a => SyncPathUtil.Normalize(a.RelativePath)), StringComparer.Ordinal);

    // Agrupa pela pasta de 1º nível NORMALIZADA (PA-01-02: casing manifesto×disco cai numa chave só);
    // arquivo solto no root não agrupa (FirstLevelFolder devolve null → segue per-file).
    var byFolder = moveActions
        .Select(a => (action: a, folder: FirstLevelFolder(a.RelativePath)))
        .Where(x => x.folder != null)
        .ToLookup(x => SyncPathUtil.Normalize(x.folder), x => x.action);

    foreach (var group in byFolder)
    {
        ct.ThrowIfCancellationRequested();
        string folderRel = ResolveOnDiskCasing(group.Key);   // casing real do disco (CC-7)
        string folderAbs = SyncPathUtil.ToLocalPath(_options.GameRoot, folderRel);
        if (!Directory.Exists(folderAbs)) continue;

        var actions = group.ToList();

        // PA-01-02: origens mistas (mod opcional + extra na mesma pasta) apontam p/ destinos-disabled
        // diferentes → NÃO consolida (cai per-file; a faxina limpa a casca). Só se convergem para 1.
        var targets = actions
            .Select(a => DeriveFolderDisabledTarget(folderRel, a.RelativePath, a.MoveTargetRelative))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (targets.Count != 1) continue;

        bool anyStays = Directory
            .EnumerateFiles(folderAbs, "*", SearchOption.AllDirectories)
            .Any(f => FileMustStay(f, manifestPaths, moving));
        if (anyStays) continue; // pasta compartilhada ou com protegido → mantém per-file

        foreach (var a in actions) plan.Actions.Remove(a);
        plan.Actions.Add(new SyncAction
        {
            RelativePath = folderRel,
            MoveTargetRelative = targets[0],
            Kind = SyncActionKind.MoveDirToDisabled,
            Rule = actions[0].Rule,
            Reason = actions[0].Reason,
        });
    }
}

/// <summary>Pasta de 1º nível sob um root MirrorMoveDisabled; null se o arquivo está solto no root.</summary>
private string FirstLevelFolder(string relative)
{
    string norm = SyncPathUtil.Normalize(relative);
    if (_resolver.Resolve(norm, out string matched) != SyncFolderRule.MirrorMoveDisabled) return null;
    if (string.IsNullOrEmpty(matched)) return null;

    string forward = (relative ?? string.Empty).Replace('\\', '/').TrimStart('/');
    if (forward.Length <= matched.Length) return null;
    string remainder = forward.Substring(matched.Length).TrimStart('/'); // "PiP-Disabler/x.dll" ou "Foo.dll"
    int slash = remainder.IndexOf('/');
    if (slash < 0) return null;                     // arquivo solto no root
    string prefixOriginalCase = forward.Substring(0, matched.Length); // casing do root
    return prefixOriginalCase + "/" + remainder.Substring(0, slash);  // "plugins/PiP-Disabler"
}

/// <summary>
/// Um arquivo real precisa FICAR (impede o move da pasta inteira) se: é protegido (coop-safe,
/// ignored, excluído da limpeza, protegido), está sob um segmento -disabled, ou é entrada do
/// manifesto que NÃO está indo à quarentena (mod ligado / arquivo mandatório na mesma pasta).
/// Arquivo neutro não-catalogado sob a pasta de um mod desligado NÃO impede — vai junto.
/// </summary>
private bool FileMustStay(string absoluteFile, HashSet<string> manifestPaths, HashSet<string> moving)
{
    string norm = SyncPathUtil.Normalize(
        Path.GetRelativePath(_options.GameRoot, absoluteFile).Replace('\\', '/'));
    if (SyncPathUtil.ContainsDisabledSegment(norm)) return true;
    if (IsIgnored(norm) || IsExcludedFromCleanup(norm) || _protectedNormalized.Contains(norm)) return true;
    if (SyncCoopSafe.IsCoopEssentialPlugin(norm)) return true;
    // PA-01-01: arquivo sob regra que NÃO é quarentena (preserve-divergent, mirror-reference, force,
    // optional-config) nunca vai para -disabled — espelha o skip do ScanExtras (SyncPlanner.cs:522-533).
    // Impede arrastar uma config preservada/forçada só porque ela mora sob a pasta de um mod desligado.
    if (_resolver.Resolve(norm, out _) != SyncFolderRule.MirrorMoveDisabled) return true;
    if (manifestPaths.Contains(norm) && !moving.Contains(norm)) return true; // mod ligado / mandatório
    return false;
}

/// <summary>
/// Destino da PASTA na quarentena: sobe no MoveTargetRelative do arquivo-amostra tantos níveis
/// quantos ele está abaixo da pasta de origem. Ex.: pasta "plugins/PiP-Disabler",
/// arquivo "plugins/PiP-Disabler/x.dll" → alvo "plugins-disabled/optional/PiP-Disabler/x.dll"
/// (depth 1) → sobe 1 → "plugins-disabled/optional/PiP-Disabler".
/// </summary>
private static string DeriveFolderDisabledTarget(string folderRel, string sampleSource, string sampleTarget)
{
    string folderF = folderRel.Replace('\\', '/').TrimEnd('/');
    string srcF = (sampleSource ?? string.Empty).Replace('\\', '/');
    string rem = srcF.Length > folderF.Length ? srcF.Substring(folderF.Length).TrimStart('/') : string.Empty;
    int depth = rem.Length == 0 ? 0 : rem.Split('/').Length;
    var segs = (sampleTarget ?? string.Empty).Replace('\\', '/').TrimEnd('/').Split('/');
    return string.Join("/", segs.Take(Math.Max(1, segs.Length - depth)));
}
```

### 5.3 Engine: mover a pasta + merge

```csharp
// SyncEngine.cs — novo case no switch de ExecuteAsync:
case SyncActionKind.MoveDirToDisabled:
    try
    {
        string srcAbs = ResolveUnderRoot(action.RelativePath);
        string dstAbs = ResolveUnderRoot(action.MoveTargetRelative);

        // Baseline: o caminho de cada arquivo muda — remove ANTES de mover.
        if (Directory.Exists(srcAbs))
        {
            foreach (var f in Directory.EnumerateFiles(srcAbs, "*", SearchOption.AllDirectories))
            {
                string rel = Path.GetRelativePath(_gameRoot, f).Replace('\\', '/');
                _baseline.Remove(rel);
            }
        }

        MoveDirectoryMerge(srcAbs, dstAbs);
        result.MovedToDisabled++;
        ioDone++;
        // CA-034.7: UMA entrada agregada pela pasta (não uma por arquivo).
        AddEntry(result, action.RelativePath, "moved-to-disabled", action.MoveTargetRelative);
    }
    catch (Exception ex)
    {
        result.Errors++;
        ioDone++;
        AddEntry(result, action.RelativePath, "error", ex.Message);
        _log($"[Sync] Falha ao mover a pasta {action.RelativePath}: {ex.Message}");
    }

    break;

/// <summary>
/// Move a pasta inteira para a quarentena. Se o destino já existe (CC-4: quarentena anterior),
/// mescla arquivo a arquivo sobrescrevendo homônimos e remove a origem esvaziada. Directory.Move
/// preserva o nome exato da origem (CC-7).
/// </summary>
private static void MoveDirectoryMerge(string source, string destination)
{
    if (!Directory.Exists(source)) return;

    if (!Directory.Exists(destination))
    {
        string parent = Path.GetDirectoryName(destination);
        if (!string.IsNullOrEmpty(parent)) Directory.CreateDirectory(parent);
        Directory.Move(source, destination);
        return;
    }

    // PA-01-03: materializa a lista ANTES de mover — mutar a árvore durante enumeração lazy poderia
    // PULAR arquivos, e o delete recursivo os apagaria (perda de dados silenciosa).
    var files = Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories).ToList();
    foreach (var file in files)
    {
        string rel = Path.GetRelativePath(source, file);
        string target = Path.Combine(destination, rel);
        string dir = Path.GetDirectoryName(target);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.Move(file, target, overwrite: true);
    }

    // Só remove a origem se ela ficou REALMENTE sem arquivos (nunca apaga conteúdo não movido).
    if (!Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories).Any())
    {
        Directory.Delete(source, recursive: true); // só restam pastas vazias
    }
}
```

### 5.4 Engine: faxina de pastas vazias

```csharp
// SyncEngine.cs — chamado DENTRO do try de ExecuteAsync, logo APÓS o foreach de ações (PA-01-04):
//   CleanupEmptyDirectories(plan.EmptyDirCleanupRoots);
// Fica dentro do try de propósito: no cancelamento o foreach lança e a faxina é PULADA — seguro,
// pois uma ação cancelada deixa o arquivo na origem e a pasta não fica vazia.

/// <summary>
/// Item 034: remove pastas vazias sob os roots de espelho-com-quarentena (bottom-up). Silenciosa
/// (não entra no relatório — CA-034.7). Nunca remove o próprio root; nunca desce em -disabled (CC-6).
/// Lista vazia em Dev Mode (o planner não a popula — CC-3). Falha isolada, nunca derruba o sync.
/// </summary>
private void CleanupEmptyDirectories(IReadOnlyList<string> roots)
{
    if (roots == null) return;
    foreach (var root in roots)
    {
        string rootAbs = SyncPathUtil.ToLocalPath(_gameRoot, root);
        if (!Directory.Exists(rootAbs)) continue;
        try { RemoveEmptyDirsBottomUp(rootAbs, isRoot: true); }
        catch (Exception ex) { _log($"[Sync] Faxina de pastas vazias falhou em {root}: {ex.Message}"); }
    }
}

private static void RemoveEmptyDirsBottomUp(string dir, bool isRoot)
{
    string name = Path.GetFileName(dir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
    if (name.EndsWith("-disabled", StringComparison.Ordinal)) return; // CC-6

    foreach (var sub in Directory.GetDirectories(dir))
    {
        RemoveEmptyDirsBottomUp(sub, isRoot: false);
    }

    if (!isRoot && Directory.GetFileSystemEntries(dir).Length == 0)
    {
        Directory.Delete(dir, recursive: false);
    }
}
```

## 6. Fluxo de dados

```
LOGIN → CheckForUpdatesCore → SyncPlanner.BuildPlanAsync
  ├─ QuarantineDisabledOptionalMods → N× MoveToDisabled (origem Optional)      [SyncPlanner.cs:~460]
  ├─ ScanExtras → N× MoveToDisabled (origem MirrorExtra)                       [SyncPlanner.cs:476]
  ├─ ConsolidateFolderMoves(plan, manifestPaths)                               [novo §5.2]
  │     agrupa por FirstLevelFolder → se !anyStays(FileMustStay) →
  │     remove N per-file + add 1 MoveDirToDisabled(pasta → pasta)
  └─ if !DevMode: EmptyDirCleanupRoots ← MirrorMoveDisabled roots (casing disco)
        │
        ▼
SyncEngine.ExecuteAsync(plan)                                                  [SyncEngine.cs:44]
  ├─ foreach action:
  │     case MoveDirToDisabled → remove baseline dos arquivos + MoveDirectoryMerge + 1 entry  [novo §5.3]
  │     case MoveToDisabled    → File.Move per-file (inalterado)               [SyncEngine.cs:152]
  ├─ CleanupEmptyDirectories(plan.EmptyDirCleanupRoots)  (silencioso)          [novo §5.4]
  └─ finally: baseline.Save + SyncReport.Write                                 [SyncEngine.cs:353]
```

## 7. Riscos e dependências

- **R-1 (paths de subpasta):** um mod cujo `paths` no servidor é uma **subpasta** (`plugins/A/B`) faz `FirstLevelFolder` retornar `plugins/A`. Se `plugins/A` só tem esse mod → move `A` inteira (ok). Se tem outro mod ligado sob `plugins/A/C` → `FileMustStay` detecta (entrada de manifesto não-movida) → cai per-file. Se `plugins/A/C` é extra → já virou `MoveToDisabled` no `ScanExtras` → também vai junto (extra vai à quarentena de qualquer forma). Caso patológico (dois mods opcionais compartilhando `plugins/A` de 1º nível, um ligado) é coberto pelo per-file. **Sem perda**, no pior caso deixa casca que a faxina limpa.
- **R-2 (ordem no plano):** a consolidação roda **depois** de `QuarantineDisabledOptionalMods` e `ScanExtras` (as ações já existem para agrupar) e **antes** do `return plan`. A faxina roda **depois** de todas as ações no engine (vê o disco final).
- **R-3 (baseline):** `MoveDirToDisabled` remove o baseline de cada arquivo sob a pasta **antes** do move (o caminho muda); sem isso o sync seguinte veria "sumiu" e re-baixaria.
- **R-4 (Dev Mode):** os guards existentes já impedem o move em Dev Mode; a faxina é adicionalmente desligada por `EmptyDirCleanupRoots` vazio (CC-3).
- **R-5 (Recycle Bin):** o `deleteFile` injetado manda extras para a lixeira; o move de pasta e a faxina usam `Directory.Move`/`Directory.Delete` diretos (a quarentena já é o "não-destrutivo"; a faxina só apaga pasta **vazia**, sem conteúdo a recuperar).
- **R-6 (correlato item 031):** o relatório reusa o label `moved-to-disabled` (1 entrada agregada = CA-034.7); **nenhum label novo** no 034. O refino do **texto humano** ("mod X movido para a quarentena") e a distinção de contadores pasta×arquivo no `Summary` ficam no **item 031** (PA-01-05) — fora de escopo aqui.

## 8. Checklist de implementação

1. `SyncActionKind.cs`: adicionar `MoveDirToDisabled`.
2. `SyncPlan.cs`: `MoveDirCount`, somar em `IoActionCount`, adicionar `EmptyDirCleanupRoots`.
3. `SyncPlanner.cs`: `FirstLevelFolder`, `FileMustStay`, `DeriveFolderDisabledTarget`, `ConsolidateFolderMoves`; chamada + popular `EmptyDirCleanupRoots` (fora do Dev Mode) no fim de `BuildPlanAsync`.
4. `SyncEngine.cs`: `case MoveDirToDisabled` (com remoção de baseline), `MoveDirectoryMerge`, `CleanupEmptyDirectories`, `RemoveEmptyDirsBottomUp`; chamar a faxina no fim do loop.
5. Testes (`SyncFolderQuarantineTests`): (a) mod-pasta opcional desligado → 1 `MoveDirToDisabled`, origem removida, destino com a pasta; (b) mod-pasta extra idem em `patchers/`; (c) `.dll` solto → segue `MoveToDisabled` per-file, sem `MoveDirToDisabled`; (d) pasta com arquivo coop-safe → per-file, protegido permanece; (e) pasta compartilhada (mod ligado + desligado) → per-file; (f) `paths` misto pasta + `.dll` → `MoveDirToDisabled` da pasta **e** `MoveToDisabled` do `.dll`; (g) faxina remove casca vazia (incl. subpasta só com vazias), não remove o root, pula `-disabled`; (h) faxina **não** roda em Dev Mode; (i) CC-4 destino já existe → merge sobrescreve homônimos e remove a origem; (j) idempotência (2º sync não erra).
6. `dotnet build` + `dotnet test SPT.Launcher.Tests` verdes.
7. `/code-review` (gate obrigatório antes de release).

## 9. Conformidade com skills (auto-checklist)

| Item | Status | Evidência |
|---|---|---|
| Lifecycle de raid / GameWorld | N/A | Launcher pré-jogo; nada roda em raid (spec §CA estado-entre-raids). |
| Harmony patch shape | N/A | Sem Harmony/Assembly EFT. |
| Memory leak / estado raid-scoped | N/A | Sem estado raid-scoped; só I/O de arquivos síncrono no sync. |
| Coop/Fika paridade | ✅ | `FileMustStay` chama `SyncCoopSafe.IsCoopEssentialPlugin` → plugin coop nunca é arrastado na pasta (spec §Fika, CA-034.4; §5.2). |
| Thread-safety | ✅ | Roda no fluxo de sync já existente (mesma thread do `ExecuteAsync`); nenhuma estrutura compartilhada nova entre threads. |
| Atomicidade / não-destrutivo de FS | ✅ | Quarentena é move (não delete); faxina só apaga pasta **vazia**; merge (CC-4) sobrescreve só homônimos e remove origem já esvaziada (§5.3); erro isolado por ação (§5.3 try/catch). |
| Guard anti-traversal | ✅ | `ResolveUnderRoot` aplicado a origem e destino do move (§5.3), igual ao `MoveToDisabled` existente. |
| Baseline consistente | ✅ | Baseline removido dos arquivos movidos antes do `Directory.Move` (§5.3, R-3). |

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-08-01 | Guilherme | Spec técnica criada via `/create-technical-spec`. Abordagem: consolidação por disco (`ConsolidateFolderMoves` + `MoveDirToDisabled`) sem acoplar o servidor; faxina como pós-passo no engine guiada por `EmptyDirCleanupRoots` (vazia em Dev Mode). 8 pontos de extensão, 4 stubs. |
| 2026-08-02 | Guilherme | `/review-technical-spec` review 01 (revisor + sub-agent adversarial) — 6 achados aplicados: `FileMustStay` checa a regra do arquivo (PA-01-01, 🔴); consolidação por chave normalizada + destino único (PA-01-02, 🟡); `MoveDirectoryMerge` materializa lista + delete guardado (PA-01-03, 🔴); faxina fixada dentro do try/pulada no cancel (PA-01-04); label do relatório deferido ao 031 (PA-01-05); roots da faxina filtrados por existência (PA-01-06). |
