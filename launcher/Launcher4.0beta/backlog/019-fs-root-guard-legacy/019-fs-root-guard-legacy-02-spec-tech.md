# 019 — Guard de raiz + atomicidade nos caminhos legados de FS · Spec técnica

> **Data:** 2026-07-04<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [00-kickoff](./019-fs-root-guard-legacy-00-kickoff.md) · [AUDIT-2026-07-04](../../AUDIT-2026-07-04-code-product-ds.md) (§B2, §Motor de sync) · [01-spec](./019-fs-root-guard-legacy-01-spec.md)<br>

---

## Abordagem

O motor já implementa as três garantias, mas em membros **privados** do `SyncEngine`:

- guard de raiz: `SyncEngine.ResolveUnderRoot` — `SyncEngine.cs:248-258` (usa `_gameRootFullPrefix` calculado em `:41-42`).
- write atômico: `SyncEngine.ApplyAtomic` — `SyncEngine.cs:261-289`.
- deleção p/ lixeira: `ProfileViewModel.DeleteToRecycleBin` — `ProfileViewModel.cs:858-870` (injetado no motor via `BuildSyncEngine`, `:827`).

A decisão de projeto é **extrair o guard e o write atômico para utilitários públicos e puros no `SPT.Launcher.Base/Sync/`** (sem HTTP, sem UI — testáveis no xUnit já existente) e fazer **o próprio motor delegar para eles**, garantindo *fonte única de verdade* (CA-8). Os dois caminhos legados passam então a chamar os mesmos utilitários. A lixeira compartilhada fica no `SPT.Launcher` (UI) porque depende de `Microsoft.VisualBasic.FileIO`, que já é referência daquele projeto.

Rejeitada a alternativa de **rotear os caminhos legados como ações do `SyncPlan`/`SyncEngine`**: `deleteFiles` não é manifest entry (não tem baseline nem hash), e os opcionais têm UI/progresso/`offFolders`/`targetSubDir`/skip-por-MD5 próprios; enfiá-los no plano exigiria refactor grande e gravaria baseline indevido (viola RN-4 da 01-spec). A extração de utilitário é cirúrgica e preserva a semântica.

## Contratos novos (SPT.Launcher.Base/Sync/)

### `SyncPathUtil.ResolveUnderRoot(string root, string relativePath) → string`
Guard puro, movido da lógica de `SyncEngine.cs:248-258`. Assinatura:

```csharp
// throws InvalidOperationException("path escapes game root: {relativePath}")
public static string ResolveUnderRoot(string root, string relativePath)
{
    string fullPath = Path.GetFullPath(ToLocalPath(root, relativePath));
    string rootPrefix = Path.GetFullPath(root)
        .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
    if (!fullPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException($"path escapes game root: {relativePath}");
    return fullPath;
}
```

Reusa `SyncPathUtil.ToLocalPath` (`SyncPathUtil.cs:21-24`), que já normaliza `/` e `\`. Mesma mensagem de exceção (`"escapes game root"`) que o teste do motor já casa (`SyncEngineTests.cs:292`).

### `SyncFileOps.WriteAtomic(string absolutePath, byte[] data)` (arquivo novo)
Write atômico puro, extraído de `SyncEngine.ApplyAtomic` (`:261-289`): cria diretório, escreve `absolutePath + ".sync-tmp"`, `File.Move(temp, dest, overwrite:true)`, rollback deletando o temp em falha. **Não** faz guard — recebe path já resolvido (o chamador resolve antes).

### Recycle bin compartilhado (SPT.Launcher)
Extrair `DeleteToRecycleBin` (`ProfileViewModel.cs:858-870`) para `SPT.Launcher.Helpers.RecycleBinHelper.Delete(string path)` (mesmo corpo: `Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(..., SendToRecycleBin)` com fallback `File.Delete` em `PlatformNotSupportedException`). `ProfileViewModel` e `OptionalModsHelper` passam a usá-lo.

## Arquivos a tocar

| Arquivo | Mudança |
|---|---|
| `SPT.Launcher.Base/Sync/SyncPathUtil.cs` | + `ResolveUnderRoot(root, rel)` público (guard puro) |
| `SPT.Launcher.Base/Sync/SyncFileOps.cs` **(novo)** | + `WriteAtomic(absolutePath, data)` (temp+move+rollback) |
| `SPT.Launcher.Base/Sync/SyncEngine.cs` | `ResolveUnderRoot` (`:248`) delega para `SyncPathUtil.ResolveUnderRoot(_gameRoot, rel)`; `ApplyAtomic` (`:261`) delega para `SyncFileOps.WriteAtomic`. Comportamento idêntico — refactor puro, coberto pelos testes atuais do motor |
| `SPT.Launcher/Helpers/RecycleBinHelper.cs` **(novo)** | + `Delete(path)` (movido de `ProfileViewModel.DeleteToRecycleBin`) |
| `SPT.Launcher/ViewModels/ProfileViewModel.cs` | loop `deleteFiles` (`:643-659`): envolver cada entrada em `try { full = SyncPathUtil.ResolveUnderRoot(gamePath, deleteFile); if (File.Exists(full)) RecycleBinHelper.Delete(full); } catch (InvalidOperationException) { log Warning; }`. `DeleteToRecycleBin` (`:858`) vira thin wrapper de `RecycleBinHelper.Delete` (ou removido; `BuildSyncEngine:827` passa a injetar `RecycleBinHelper.Delete`) |
| `SPT.Launcher/Helpers/OptionalModsHelper.cs` | `DownloadOptionalGroupAsync` (`:252-255`) e `DownloadFromOpcionaisFolder` (`:351-354`): trocar `Path.Combine`+`File.WriteAllBytes` por `dest = SyncPathUtil.ResolveUnderRoot(GamePath, rel); SyncFileOps.WriteAtomic(dest, bytes)` dentro do `try` existente (a exceção de guard cai no `catch` que já loga `Warning` e segue). `RemoveOptionalGroupAsync` (`:301-303`)/`DeleteFileIfExists` (`:386-389`): resolver via guard e deletar via `RecycleBinHelper.Delete` em vez de `File.Delete` |
| `SPT.Launcher.Tests/Sync/` | testes novos (ver §Plano de teste) |

### Detalhe do guard em `DownloadFromOpcionaisFolder`
O destino combina `targetSubDir` (config do grupo, server) + `file.path` (manifesto do offFolder, server). Validar o destino **final** contra a raiz: `ResolveUnderRoot(GamePath, Path.Combine(targetSubDir ?? "", file.path))` — cobre traversal tanto em `targetSubDir` quanto em `file.path` numa só chamada (CA-6). `ToLocalPath` já normaliza separadores.

### Nota de performance (motor)
`SyncEngine` hoje pré-computa `_gameRootFullPrefix` uma vez (`:41`). A versão delegada recalcula `Path.GetFullPath(root)` por chamada. Custo desprezível ante disco/hash no loop; se um profiling futuro reclamar, adicionar overload `ResolveUnderRoot(rootFullPrefix, root, rel)` que recebe o prefixo pré-computado. Não fazer agora (YAGNI) — prioridade é fonte única.

## Riscos

| Risco | Mitigação |
|---|---|
| Refactor do motor (`ResolveUnderRoot`/`ApplyAtomic` delegando) introduzir regressão sutil no fluxo aprovado do 007/008/017 | Manter mensagem de exceção e semântica idênticas; a suíte atual (`SyncEngineTests` incl. `Download_path_with_traversal_...:257-293`, `Move_collision_...`, seed) precisa continuar 100% verde sem alteração — é o oráculo de equivalência |
| `Microsoft.VisualBasic.FileIO` indisponível fora do Windows | Já tratado: fallback `File.Delete` em `PlatformNotSupportedException`, preservado no `RecycleBinHelper` |
| Guard rejeitar path legítimo de produção que usava `..`/absoluto | Gate humano §Inspeção de produção (01-spec) — inspecionar `deleteFiles` real antes do deploy; RN-5 (não-regressão) coberta por CA-3 |
| Symlink/junction dentro da raiz apontando pra fora ainda escapa | **Residual conhecido** (herdado do motor). Stretch opcional: em `ResolveUnderRoot`, após `GetFullPath`, resolver via `File.ResolveLinkTarget`/`Directory.ResolveLinkTarget` e re-checar. Fora do escopo duro (exige link pré-plantado) — não bloquear o item por isso |
| `OptionalModsHelper` é `static` e roda I/O na UI thread (AUDIT) | **Não** é deste item — 021. 019 só troca as primitivas de FS; não mexe em threading nem em `GetServerBaseUrl` |

## Plano de teste (`SPT.Launcher.Tests/Sync/` — xUnit)

Reusar o padrão de `SyncTestFixture` (`SyncTestFixture.cs`: raiz temp real via `Guid`, `WriteLocal`/`LocalExists`/`ReadLocal`, `Md5Of`). Novos arquivos:

**`SyncPathGuardTests.cs`** — cobre `SyncPathUtil.ResolveUnderRoot` direto (unidade pura, sem IO):
- `"../../evil.txt"` → lança `InvalidOperationException` com `"escapes game root"` (CA-1/CA-4).
- caminho absoluto (`"C:/Windows/x"` / `Path.Combine` descartando raiz) → lança (CA-2).
- `"a/../b.txt"` (traversal benigno sob a raiz) → retorna path sob a raiz, não lança (corner case).
- prefixo-irmão: `root=".../SPT"`, alvo resolvendo em `.../SPTevil/x` → lança (RN-2).
- path legítimo `"BepInEx/plugins/x.dll"` → retorna `GetFullPath` correto (CA-3/CA-8).
- **Equivalência (CA-8):** parametrizar com os mesmos inputs de `SyncEngineTests.Download_path_with_traversal_does_not_escape_game_root` e assertar mesmo veredito.

**`SyncFileOpsTests.cs`** — `WriteAtomic`:
- destino inexistente → cria diretório + arquivo com o conteúdo (CA-5).
- destino existente → sobrescreve; conteúdo final = novo; nenhum `.sync-tmp` remanescente (CA-5).
- (se viável simular) falha no move → arquivo original intacto, temp limpo (rollback).

**`OptionalModsGuardTests.cs`** *(se a superfície estática permitir teste direto; senão, cobrir a lógica extraída)* — como `OptionalModsHelper` é `static` e acopla `HttpClient`/`RequestHandler`, testar preferencialmente a **primitiva extraída** (`ResolveUnderRoot`/`WriteAtomic`) já cobre o núcleo; a integração fica no gate in-game. Documentar essa fronteira no teste.

O loop de `deleteFiles` vive dentro de `CheckForUpdatesCore` (privado, acoplado a `LauncherSettingsProvider`/`RequestHandler`), então a lógica de guard é validada pela unidade `ResolveUnderRoot` + gate in-game (item §Gates da 01-spec), não por teste de VM.

## Nota de paralelismo

Arquivos compartilhados com outros itens do lote — coordenar merge:

- **`ProfileViewModel.cs`** é o **hub dos itens 019–023**. 019 toca **só** o loop `deleteFiles` (`:643-659`) e os helpers `DeleteToRecycleBin`/`BuildSyncEngine` (`:808-870`). Outras regiões do mesmo arquivo pertencem a itens vizinhos: delete não-atômico de conta (010, `:1112-1118`), confirmação frágil de wipe/remove (`:1088`,`:1182`), comandos `async Task` sem try/catch (`:960`). Editar em blocos localizados; conflito provável só nos helpers se outro item também mexer em `DeleteToRecycleBin` — daí a extração para `RecycleBinHelper` reduz o atrito.
- **`OptionalModsHelper.cs`** é compartilhado com **019 e 021**. 019 troca as linhas de FS (`:252-255`, `:301-303`, `:351-354`, `:386-389`). 021 muda `GetServerBaseUrl` (`:45-57`, porta/esquema) e a thread-safety de `OnOptionalToggled` (I/O+MD5 na UI). Regiões distintas do mesmo arquivo → merge textual simples, mas alinhar ordem de entrada.
- **`SyncEngine.cs` / `SyncPathUtil.cs`** são código **aprovado** (motor 007/008/016/017). O refactor de 019 (delegação) é *behavior-preserving* e sob teste-oráculo; ainda assim é churn em arquivo estável — revisar com cuidado e não alterar assinaturas públicas existentes.
- **`Legacy.axaml`** (itens 024/025) e demais views **não** são tocados por 019.

## Gates

`dotnet build SPT.Launcher.csproj -c Release` · `dotnet build SPT.Launcher.Base.csproj -c Release` · `dotnet test SPT.Launcher.Tests.csproj -c Release` — verdes. Gates humanos (adulteração + in-game host/cliente + inspeção de produção) na [01-spec §Gates humanos](./019-fs-root-guard-legacy-01-spec.md). Nunca rodar o exe num gate automatizado.
