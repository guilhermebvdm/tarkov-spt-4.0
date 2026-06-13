# 021 — ClassRegistrar (dry-run) + ClassEditorService · Spec técnica

**Mod:** CustomClasses · **Data:** 2026-06-10 · **Spec:** [01-spec](021-class-registrar-editor-service-01-spec.md)

## 1. Fases do pipeline (`ClassRegistrar`, Singleton)

O monólito `CustomClassesMod.RegisterClass` (validação + build + mutação entrelaçados) vira três fases:

```
ValidateAndBuild (PURO)  ──RegistrationPlan──▶  Commit (muta)          Remove (muta)
  · name vazio → Error                            · registries.Set       · templates.Remove
  · colisão de edition → Error (c/ allowReplace)  · templates[name] =    · registries.Remove
  · baseEdition inexistente → Error                 sides (swap)         · guard de ownership
  · clone null → Error                            · log "Registered ..."
  · clone profundo do base (ICloner)
  · descrição resolvida (locale do servidor)
  · ApplySkills (usec/bear) + warnings
  · InventoryBuilder/HideoutBuilder/OutfitBuilder
  · skillMultipliers normalizados/clampados
```

### Contratos públicos

```csharp
// ClassDiagnostic.cs
public enum DiagnosticSeverity { Error, Warning, Info }
public sealed record ClassDiagnostic(DiagnosticSeverity Severity, string Code, string Message);
public static class DiagnosticCodes { ParseError, InvalidFileName, NameMissing, ClassDisabled,
    EditionCollision, BaseEditionNotFound, CloneFailed, SideSkillsNotApplied, UnknownSkill,
    UnknownMultiplierSkill, SkillsExtendedMissing, SerializeFailed }

// ClassRegistrar.cs
public sealed record RegistrationPlan { Name, BaseEdition, Definition, Sides(ProfileSides),
    SkillMultipliers(Dictionary<string,double>), SourceFileName?, Counts(RegistrationCounts) }
public sealed record RegistrationCounts(SkillsUsec, SkillsBear, ItemsUsec, ItemsBear,
    HideoutStations, OutfitsUsec, OutfitsBear, SkillMultipliers);

[Injectable(InjectionType.Singleton)]
public class ClassRegistrar
{
    public bool SkillsExtendedInstalled { get; }   // lazy, item 006
    public RegistrationPlan? ValidateAndBuild(ClassDefinition def, string fileName,
        bool allowReplace, out List<ClassDiagnostic> diagnostics);
    public void Commit(RegistrationPlan plan);
    public bool Remove(string name);
}
```

### Semântica de `allowReplace`

`templates.ContainsKey(name)` bloqueia **exceto** quando `allowReplace && classVisualRegistry.Contains(name)` — i.e., o editor só pode substituir edition que o PRÓPRIO mod registrou (`ClassVisualRegistry` é a fonte de ownership desde o item 011). Colisão com vanilla/outro mod é Error sempre. Boot: `allowReplace=false` (paridade com o guard pré-021).

### Diagnostics × logs (paridade)

`ValidateAndBuild` **loga as mesmas mensagens** que `RegisterClass` logava (Warning de colisão, Error de base/clone, warnings de skill/multiplicador/SE) **e** adiciona o diagnostic equivalente. Nuance: a colisão loga `Warning` (texto idêntico ao pré-021) mas o diagnostic é `Error` (bloqueia). Os builders seguem logando direto — seus warnings NÃO viram diagnostics (decisão: builders intactos neste item; sink de diagnostics é evolução futura). O warning de skill desconhecida em `skills` loga 2× (uma por lado, como antes) mas vira 1 diagnostic (dedupe por nome).

`NameMissing`/`ClassDisabled` são pré-checados pelo `OnLoad` (com os logs/contagens de skip atuais) e TAMBÉM cobertos em `ValidateAndBuild` (sem log, só diagnostic) para o caminho do editor — no boot o duplicado nunca dispara.

### Hot-apply: build-then-swap

`CreateProfileService.CreateProfile` → `ProfileHelper.GetProfileTemplateForSide` → `databaseService.GetProfileTemplates()` (ref: CreateProfileService.cs:44, ProfileHelper.cs:804-806, DatabaseService.cs:141) lê o dict **vivo** a cada criação de perfil; o launcher lista editions das keys do mesmo dict (LauncherController.cs:48). Logo: `templates[name] = plan.Sides` numa única escrita de referência troca a classe inteira de uma vez — leitores veem a versão antiga OU a nova, nunca uma meio-construída. Ordem no `Commit`: registries primeiro (keys ainda não expostas para edition nova), templates por último (ponto de swap). Multiplicadores vazios no re-save **limpam** a entry anterior (`Remove`) — evita multiplicador órfão de versão antiga; no boot é no-op.

### `Remove(name)` com guard de ownership

Recusa (`false` + warning) qualquer `name` que não esteja no `ClassVisualRegistry` — impossível remover edition vanilla/de outro mod por engano via editor/rota futura. Perfis já criados não são afetados (a edition só some da lista de perfil novo).

## 2. Registries

`SkillMultiplierRegistry` e `ClassVisualRegistry` ganham `bool Remove(string edition)` e `IReadOnlyCollection<string> Editions` (=`_byEdition.Keys`). API existente intacta. **Sem lock** (Dictionary simples, como hoje): leitores são o router HTTP e o fluxo de criação de perfil; corrida com hot-apply é aceita (kickoff — server local single-user). Risco residual: enumerar `Editions` durante um `Set`/`Remove` concorrente pode lançar — aceito e documentado.

## 3. `ClassEditorService` (Singleton)

Opera sobre `config/classes/` **do mod instalado** (`modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly())` — mesma resolução do loader; path cacheado lazy).

```csharp
public sealed record ClassFileEntry(string FileName, ClassDefinition? Definition,
    bool Enabled, bool Registered, List<ClassDiagnostic> Diagnostics);
public sealed record SaveResult(bool Success, List<ClassDiagnostic> Diagnostics);

[Injectable(InjectionType.Singleton)]
public class ClassEditorService
{
    public List<ClassFileEntry> ListClassFiles();                       // varredura + dry-run (allowReplace=true)
    public ClassDefinition? Load(string fileName, out List<ClassDiagnostic> diagnostics);  // parse-only
    public SaveResult Save(string fileName, ClassDefinition def, bool hotApply);
    public bool Delete(string fileName, bool hotRemove);
}
```

### Fluxo do `Save`

1. Valida `fileName` (nome puro `*.json|*.jsonc`, sem separador/traversal — boundary p/ rotas HTTP futuras).
2. `ValidateAndBuild(def, fileName, allowReplace: true)` → qualquer **Error** retorna diagnostics **sem escrever nada**.
3. Serializa (`jsonUtil.Serialize(def, indented: true)` — ref: JsonUtil.cs:174; opções do SPT: `WhenWritingNull`, sem BOM) **antes** de tocar o disco.
4. Backup rotativo: `bak3` apagado, `bak2→bak3`, `bak1→bak2`, atual→`bak1` (sufixos `.bakN` ficam fora dos globs `*.json|*.jsonc` do loader; não colidem com o `.bak` temporário do `FileUtil.WriteFileAsync`, não usado aqui).
5. Escreve (`fileUtil.WriteFile`).
6. `hotApply`: `enabled` → `Commit(plan)`; `enabled:false` → `Remove(name)` (kickoff).
7. Audit line em `config/classes/_audit.log` (TSV: timestamp UTC, arquivo, ação, resumo). Falha de audit não falha o save.

### Decisões de suporte

- **`LocalizedTextConverter.Write` implementado** (antes: `NotSupportedException` — o save serializaria `displayName`/`description` e explodiria). Round-trip preserva as duas formas: só `En` → string (legado); com `Pt` → objeto `{en, pt}`.
- **Comentários `.jsonc` perdidos** no save (reserialização) + normalização de formato (defaults como `"premium": false` podem aparecer explícitos) — documentado no código e no as-built; `.bak1` preserva o manuscrito.
- **Rename:** responsabilidade do caller remover a edition antiga (o service não adivinha rename vs. duplicação) — documentado no XML doc.
- **`Delete` com arquivo inquebrável de parse:** apaga mesmo assim (com backup), sem hot-remove (name desconhecido), warning logado.
- **`ListClassFiles` é "barulhento" por design:** o dry-run loga os mesmos warnings que o boot logaria (registrar + builders) — paridade > silêncio.

## 4. Riscos

| Risco | Mitigação |
| --- | --- |
| Regressão de boot (nº 1 do kickoff) | Refactor comportamento-preservante: mensagens/ordem/contagens comparadas linha a linha (ver as-built §paridade); try/catch por arquivo intacto no `OnLoad`. |
| Captive dependency (Singleton ← builders Scoped) | Padrão do próprio SPT (`ItemHelper` [Singleton] injeta `ICloner` [Scoped] — ItemHelper.cs:19); host não usa `ValidateScopes`. Builders são stateless. |
| Corrida hot-apply × leitores | Build-then-swap (1 escrita de referência); residual aceito (kickoff). |
| Serialização divergente do schema | Mesmo `JsonUtil`/converters do load; `LocalizedTextConverter.Write` espelha o `Read`. |
| Build integrado | `dotnet build` NÃO rodou neste item (conflito de `obj/` com agente 020) — orquestrador builda integrado depois. |

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-10 | Spec técnica escrita junto da implementação. |
