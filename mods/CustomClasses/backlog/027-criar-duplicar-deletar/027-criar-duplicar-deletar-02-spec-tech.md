# 027 — Criar / duplicar / deletar classe — Spec técnica

**Mod:** CustomClasses
**Criado:** 2026-06-10
**Refs:** [01-spec](./027-criar-duplicar-deletar-01-spec.md) · [021 as-built](../021-class-registrar-editor-service/021-class-registrar-editor-service-05-asbuild.md) · `docs/class-schema.md`

## Contratos novos (`ClassEditorService`, seção "Lifecycle (item 027)")

```csharp
public sealed record CreateResult(bool Success, string? FileName, List<ClassDiagnostic> Diagnostics);

public HashSet<string> ExistingEditionNames();                       // snapshot p/ validação live
public string? ValidateNewClassName(string? name,
    IReadOnlySet<string>? existingNames = null);                     // msg de erro ou null
public CreateResult Create(string name);
public CreateResult Duplicate(string sourceFileName, string newName);
public List<string> ProfilesUsingEdition(string editionName);        // "username (file.json)"
```

Novo código de diagnóstico: `DiagnosticCodes.InvalidClassName`. `ClassRegistrar.DefaultBaseEdition` virou `public const` (o Create escreve o default explícito no arquivo).

## Validações de nome

- Trim; vazio → "Name is required.".
- Colisão **case-insensitive** (`StringComparer.OrdinalIgnoreCase`) contra `ExistingEditionNames()` = chaves de `databaseService.GetProfileTemplates()` (vanilla + ocultas pelo 009 + classes já registradas) ∪ `name` parseado de TODO arquivo em `config/classes/` (classe desabilitada/não registrada também reserva o nome — segundo arquivo com o mesmo nome explodiria no próximo boot). Mais estrito que o check ordinal do registrar, de propósito (editions diferindo só por caixa = footgun).
- Caracteres livres no nome (editions como "Caçador" existem) — a segurança de filename fica TODA no slug, nunca em restringir o nome.
- Camada dupla: dialogs validam live contra snapshot (barato por tecla); `Create`/`Duplicate` re-validam autoritativamente antes de escrever.

## Slug / nome de arquivo

`Slugify`: `Normalize(FormD)` + drop de `NonSpacingMark` (ç→c, ã→a), lowercase, fora de `[a-z0-9]` colapsa pra `-` único, trim de `-`; vazio → `"class"`. `UniqueClassFileName`: primeiro `<slug>.jsonc` livre, sufixo `-2`, `-3`…; checa **as duas extensões** (`x.jsonc` nunca nasce ao lado de `x.json`).

## Fluxos

- **Create:** valida nome → template mínimo (`name`, `displayName`/`description` `{en,pt}`=nome, `enabled:true`, `baseEdition="SPT Zero to hero"` explícito) → `Save(file, def, hotApply:true)` (pipeline 021: dry-run → backup → write → `Commit`) → audit extra `create` → retorna fileName.
- **Duplicate:** `Load(source)` (falha → diagnostics) → valida novo nome → `source with { Name, DisplayName }` (record `with` — resto verbatim, incl. `iconFile`/`nameColor`/`loadout`/`enabled`) → slug novo → `Save(hotApply:true)` → audit `duplicate`. Duplicar classe desabilitada gera cópia desabilitada (fiel ao fonte); o hot-apply de `enabled:false` é `Remove` de edition não registrada → warning inócuo no log.
- **Delete:** `Delete(fileName, hotRemove:true)` (021, inalterado): backup → delete → `ClassRegistrar.Remove(name)`.
- **Disable:** `Load` + `Save(file, def with { Enabled = false }, hotApply:true)`. **O Save já trata `enabled:false` como `ClassRegistrar.Remove`** (ClassEditorService.cs, decisão do 021 — "enabled:false hot-applies as removal") → o caller NÃO chama `Remove` por fora.

## Varredura de perfis (`ProfilesUsingEdition`)

- Path resolvido do install: `ModHelper.GetAbsolutePathToModFolder` (= `user/mods/CustomClasses`) + `../../profiles` → `Path.GetFullPath`. No ambiente real: `D:/SPT/SPT/user/profiles`.
- **Evidência do campo (2026-06-10, 36 perfis reais):** todo perfil tem `info` no root com chaves `[id, scavId, aid, username, wipe, edition]`; ex.: `6a0a12f03ee3fe9c94220e47.json` → `info.edition = "Armeiro"`, `info.username = "TestePerfil1"`; `6a25abdc…json` → `info.edition = "Caçador"`. Match **ordinal** (launcher grava a chave verbatim).
- Leitura barata: `JsonDocument.Parse(stream)` lendo só `info.edition`/`info.username` (perfis têm MBs — nada de desserializar `SptProfile`). Arquivo ilegível/corrompido → `logger.Warning` + skip. Chamada roda em `Task.Run` no dialog (não bloqueia o circuit Blazor).

## UI (MudBlazor 8.13, padrões do 023: `IMudDialogInstance` cascading + `IDialogService.ShowAsync<T>(title, parameters, options)` + `DialogResult.Ok`)

| Componente | Papel |
|---|---|
| `Web/Shared/ClassLifecycleCreateDialog.razor` | Input + validação live (snapshot em `OnInitialized`), aviso de ícone ausente, diagnostics de falha inline; fecha com `Ok(fileName)`. |
| `Web/Shared/ClassLifecycleDuplicateDialog.razor` | Params `SourceFileName`/`SourceClassName`; mesmo input/validação; fecha com `Ok(newFileName)`. |
| `Web/Shared/ClassLifecycleDeleteDialog.razor` | Params `FileName`/`ClassName`/`CanDisable`; varredura de perfis em `OnInitializedAsync` (spinner → lista + aviso forte); ações Delete file / Disable instead / Cancel; fecha com `Ok("deleted"\|"disabled")`. |
| `Web/Pages/Classes.razor` | Toolbar "New class"; coluna Actions (ícones Duplicate/Delete em `div @onclick:stopPropagation` — a linha navega); `LoadRows()` extraído p/ reload pós-ação; create→edit, duplicate→detail, delete/disable→reload+snackbar. |
| `Web/Pages/ClassDetail.razor` | Botões Duplicate (def≠null) e Delete (sempre) no header; mesmos dialogs; delete→volta pra lista, disable→`Reload()` (extraído de `OnParametersSet`). |

## Corner cases

- Arquivo inválido: Duplicate desabilitado na lista; Delete remove só o arquivo (sem hot-remove — name desconhecido) com aviso de que perfis não foram checados.
- "Disable instead" oculto p/ classe já desabilitada ou inparseável; Save de disable bloqueado por Error (ex.: baseEdition inexistente) → diagnostics no dialog, Delete continua disponível.
- Pasta `user/profiles` ausente (dev sem install) → lista vazia + warning no log, dialog segue com "No user profile uses this class".
