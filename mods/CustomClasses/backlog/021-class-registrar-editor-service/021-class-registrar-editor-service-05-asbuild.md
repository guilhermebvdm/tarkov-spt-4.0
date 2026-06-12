# 021 — ClassRegistrar (dry-run) + ClassEditorService · As-Built

**Mod:** CustomClasses · **Build:** 2026-06-10 · **Spec:** [01-spec](021-class-registrar-editor-service-01-spec.md) · **Spec técnica:** [02-spec-tech](021-class-registrar-editor-service-02-spec-tech.md)

## Arquivos

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `modded/Server/ClassDiagnostic.cs` | `DiagnosticSeverity` (Error/Warning/Info), `ClassDiagnostic(Severity, Code, Message)`, `DiagnosticCodes` (12 codes estáveis). |
| CRIADO | `modded/Server/ClassRegistrar.cs` | Pipeline em fases: `ValidateAndBuild` (dry-run puro, `allowReplace`, diagnostics + logs de paridade), `Commit` (registries → `templates[name] = sides`, build-then-swap, log "Registered ..."), `Remove` (guard de ownership via `ClassVisualRegistry.Contains`). `RegistrationPlan`/`RegistrationCounts`. `SkillsExtendedInstalled` (lazy, item 006). `ApplySkills` movido de `CustomClassesMod` (verbatim + diagnostics). Singleton. |
| CRIADO | `modded/Server/ClassEditorService.cs` | `ListClassFiles` (globs idênticos ao boot, parse + dry-run allowReplace=true + flags enabled/registered, ordenado por nome), `Load` (parse-only), `Save` (validar → Error bloqueia sem escrever → serializa antes de tocar disco → backup `.bak1..bak3` → write → hot-apply: enabled→`Commit`, disabled→`Remove` → audit), `Delete` (parse best-effort do name → backup → delete → hot-remove → audit). Valida fileName (nome puro, anti-traversal). Audit em `config/classes/_audit.log` (TSV UTC; extensão fora dos globs do loader). Singleton. |
| MODIFICADO | `modded/Server/CustomClassesMod.cs` | `OnLoad` delega ao `ClassRegistrar` (dry-run + commit). `RegisterClass`/`ApplySkills`/builders/registries/cloner/locale/TimeUtil/SptMod saem do construtor (vivem no registrar). Logs e contagens preservados (ver §Paridade). |
| MODIFICADO | `modded/Server/SkillMultiplierRegistry.cs` | + `Remove(edition)`, + `Editions` (`IReadOnlyCollection<string>`). API atual intacta; sem lock (decisão mantida, concorrência aceita). |
| MODIFICADO | `modded/Server/ClassVisualRegistry.cs` | Idem (+`Remove`, +`Editions`). |
| MODIFICADO | `modded/Server/LocalizedText.cs` | `LocalizedTextConverter.Write` implementado (era `NotSupportedException` — o `Save` do editor serializa `ClassDefinition` e quebraria). Round-trip: só `En` → string legada; com `Pt` → objeto `{en, pt}`. |

## Paridade de boot (argumentada linha a linha)

Sequência de logs do `OnLoad` pré × pós-021 — **idêntica**:

1. `No classes folder at '...'` — inalterado (segue no `OnLoad`).
2. `Skills-Extended detectado: sim/não` — mesma posição; fonte agora é `classRegistrar.SkillsExtendedInstalled` (mesma detecção `SkillsExtendedCompat.IsPresent`, lazy).
3. Por arquivo: `missing required 'name' — skipped.` (Error) e `'X' is disabled in 'f' — skipped.` (Info) — inalterados, seguem no `OnLoad` antes do pipeline (contagem `skipped` igual).
4. `Edition 'X' already exists — 'f' skipped (no overwrite).` (Warning) — texto idêntico, agora em `ValidateAndBuild` (boot usa `allowReplace=false` → guard igual ao pré-021).
5. `'f': base edition 'X' not found — skipped. Available: ...` (Error) — idêntico.
6. `'f': clone of base 'X' returned null — skipped.` (Error) — idêntico.
7. `'X': unknown skill 'Y' — ignored.` — idêntico, inclusive logando 2× (uma por lado), como antes.
8. `'X': a side applied 0 skills (usec=.., bear=..) — check base template 'B'.` — idêntico.
9. Warnings de `skillMultipliers` (skill desconhecida; SE ausente) — idênticos.
10. `Registered 'X' (base 'B', skills usec=../bear=.., items usec=../bear=.., hideout=.., outfit usec=../bear=.., skillMults=..) from 'f'.` — formato e valores idênticos (counts capturados no `RegistrationPlan.Counts`; `skillMults` = `clean.Count`, igual ao `mults` antigo).
11. `Loaded N class(es), skipped M, from '...'` — inalterado; `try/catch` por arquivo intacto (`'f': failed to parse/register — skipped. <msg>`).

Diferenças intencionais e **não observáveis no boot**: templates obtidos via `databaseService.GetProfileTemplates()` dentro do registrar a cada chamada (mesmo dict vivo — DatabaseService.cs:141 retorna `Tables.Templates.Profiles` direto); `skillMultiplierRegistry.Set` movido do meio do processamento para o `Commit` (mesmo efeito, boot single-thread); multiplicadores vazios chamam `skillMultiplierRegistry.Remove` no `Commit` (no-op no boot — registry nasce vazio; relevante só no hot-apply).

## Verificações de símbolo SPT (spt-source)

- `DatabaseService.GetProfileTemplates` → `Tables.Templates.Profiles` (dict vivo) — DatabaseService.cs:141.
- `CreateProfileService.CreateProfile` clona `profileHelper.GetProfileTemplateForSide(...)` a cada criação — CreateProfileService.cs:44; `GetProfileTemplateForSide` lê `GetProfileTemplates()` na hora — ProfileHelper.cs:804-806. **Hot-apply confirmado viável.**
- Launcher lista editions das keys do mesmo dict — LauncherController.cs:48 (`Editions = profileTemplates.Select(x => x.Key)`). **`Remove` tira a edition do launcher.**
- `JsonUtil.Serialize<T>(obj, bool indented = false)` — JsonUtil.cs:174; opções com `ReadCommentHandling.Skip` (JSONC no load), `WhenWritingNull`, `NewLine="\n"`.
- `FileUtil`: `GetFiles/DirectoryExists/FileExists/ReadFile/WriteFile/CopyFile/DeleteFile` — FileUtil.cs:11/48/58/63/78/177/160. Sem `Move` → `System.IO.File.Move` direto no shift de backups.
- `ModHelper.GetAbsolutePathToModFolder` — ModHelper.cs:10. `ICloner.Clone<T>` (deep) — ICloner.cs:5.
- DI: `ItemHelper` é `[Injectable(InjectionType.Singleton)]` (ItemHelper.cs:19) e injeta `ICloner` (Scoped) — precedente do próprio SPT para Singleton←Scoped; host não usa `ValidateScopes`. `ClassRegistrar`/`ClassEditorService` Singleton seguem o mesmo padrão (builders stateless).

## Decisões

- **Ownership de edition = `ClassVisualRegistry.Contains`** (toda classe registrada entra nele desde o item 011): usado no `allowReplace` (editor só substitui classe do mod) e no guard do `Remove` (impossível apagar edition vanilla por engano).
- **Warnings dos builders não viram diagnostics** neste item (builders intactos, logam direto); sink de diagnostics é evolução futura se o viewer precisar.
- **Comentários `.jsonc` perdidos no save** + normalização de formato (reserialização DTO; defaults explícitos podem aparecer). `.bak1` preserva o manuscrito. Documentado também no XML doc do service.
- **Rename não é resolvido pelo `Save`** — caller remove a edition antiga (documentado no XML doc).
- **`_audit.log`** com extensão `.log` de propósito (globs do loader pegam só `*.json|*.jsonc`).
- **mod-backlog.md não foi tocado** — itens 020/021/022 rodaram em paralelo no mesmo working tree; transição de status fica com o orquestrador (evita conflito de escrita).

## Driver de teste

Sem UI ainda: o item **024 (class viewer)** é o driver natural de `ListClassFiles`. Validação antes disso: pela página smoke do item 020, injetar `ClassEditorService` num componente/rota temporária e chamar `ListClassFiles()` — esperado: 11 entries, todas `Registered=true`, diagnostics só com warnings já conhecidos do boot (e nenhum log de erro novo). Sem teste runtime neste item (decisão do kickoff/orquestração).

## Pendências

- **Build integrado pelo orquestrador** — `dotnet build` NÃO rodou aqui (conflito de `obj/` com o agente 020, que converteu o csproj p/ Sdk.Web em paralelo). Riscos residuais p/ o build: nenhum símbolo novo fora dos verificados acima; recursos C# usados (`required`, primary constructors, collection expressions) cobertos por `LangVersion=latest`/net9.0 já no csproj.
- Validação runtime do boot (paridade real de log, 11 classes) e do hot-apply (perfil novo sem reiniciar) — primeiro `/compile-mod` + subida do servidor após a integração.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-10 | As-built da implementação (item 021, wave W1, paralelo a 020/022). |
