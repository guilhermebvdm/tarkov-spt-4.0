# 002 — Schema de classe + loader multi-classe · Spec Técnica

**Mod:** CustomClasses
**Spec funcional:** [002-class-schema-loader-01-spec.md](002-class-schema-loader-01-spec.md)
**Criado:** 2026-06-07

> Mod **server-side** (SPT 4.0, C# .NET 9). Fonte de verdade: [references/spt-source/](../../../../references/spt-source/). Estende o `CustomClassesMod` do item 001 (que registrava 1 classe hardcoded) para ler N classes de arquivos JSON.

## 1. Estratégia

Refatorar o `CustomClassesMod` (`IOnLoad`, `PostDBModLoader + 1`) de "1 classe hardcoded" para um **loader de pasta**: enumera `config/classes/*.json[c]` na pasta do mod (`ModHelper.GetAbsolutePathToModFolder`), desserializa cada arquivo num DTO `ClassDefinition` (`JsonUtil`), valida e registra cada classe como edition (mesma lógica do 001: clone da base + skills + descrição + guarda de colisão). Erros são **por-arquivo** (try/catch): um arquivo inválido é pulado com log, os demais carregam. Ao final, loga um resumo (carregadas/puladas).

**Decisões de formato (resolvendo os 3 `<!-- review -->` da spec funcional):**
- **(a) Pasta:** `config/classes/` dentro do mod → no install: `SPT/user/mods/CustomClasses/config/classes/*.jsonc`. Aceita `.json` e `.jsonc` (o `JsonUtil` tolera comentários — permite exemplo auto-documentado).
- **(b) Identidade da edition:** **campo `name` no JSON** (vira a chave do dicionário de templates e o rótulo no launcher). Permite espaço/acento; o nome do arquivo é só organização.
- **(c) `enabled`:** campo `bool` (default `true`); `false` → classe não registra (paridade com o RZ).

Herdados do 001: **CR-01-01** resolvido (skill nova fora do base recebe `LastAccess = TimeUtil.GetTimeStamp()`); **CR-01-04** resolvido (strings saem para JSON).

## 2. Pontos de integração (SPT server)

| Alvo (spt-source) | Uso |
|---|---|
| [`ModHelper.cs:10` `GetAbsolutePathToModFolder`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/ModHelper.cs#L10) | Pasta do mod no install |
| [`FileUtil.cs:11` `GetFiles(path, recursive, pattern)`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/FileUtil.cs#L11) | Enumerar `*.json`/`*.jsonc` |
| [`FileUtil.cs:48` `DirectoryExists`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/FileUtil.cs#L48) · [`:63` `ReadFile`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/FileUtil.cs#L63) · [`:33` `GetFileNameAndExtension`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/FileUtil.cs#L33) | Existência / ler / nome p/ log |
| [`JsonUtil` `Deserialize<T>`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/ModHelper.cs#L28) | Parse de cada arquivo no DTO |
| [`DatabaseService.cs:141` `GetProfileTemplates()`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/DatabaseService.cs#L141) | Dicionário de editions |
| [`ProfileTemplate.cs:7` `ProfileSides`/`DescriptionLocaleKey`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/ProfileTemplate.cs#L7) | Template a clonar/mutar |
| [`BotBase.cs:426` `CommonSkill` (`Id:SkillTypes`, `Progress`, `LastAccess`)](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/BotBase.cs#L426) | Set de skills |
| [`ProfileHelper.cs:460/543` Progress=nível*100 (cap 5100)](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/ProfileHelper.cs#L460) | Conversão de nível |
| [`ServerLocalisationService.cs:163` `return value ?? key`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/ServerLocalisationService.cs#L163) | Descrição via texto literal |
| [`TimeUtil.GetTimeStamp()` (uso em CreateProfileService.cs:57)](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/CreateProfileService.cs#L57) | `LastAccess` de skill nova (CR-01-01) |
| [`ICloner.cs:5` `T? Clone<T>(T?)`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/Cloners/ICloner.cs#L5) | Deep clone do template |

## 3. Novas propriedades F12 (BepInEx)

Não se aplica — server-side. F12 entra no item 008.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Server/CustomClassesMod.cs` | MODIFICAR | Vira loader: enumera `config/classes/`, desserializa, valida e registra cada classe. |
| `modded/Server/ClassDefinition.cs` | CRIAR | DTO do JSON (schema): `name`, `enabled`, `baseEdition`, `description`, `skills`. |
| `modded/Server/config/classes/exampleClass.jsonc` | CRIAR | Exemplo auto-documentado (comentários) que carrega como classe válida. |
| `modded/Server/config/classes/testClass.jsonc` | CRIAR | Migra a "Test Class" do 001 para JSON (continuidade + valida N editions). |
| `.agents/scripts/compile-mod.sh` | MODIFICAR | server-csharp: copiar `<csproj-dir>/config` → `SPT/user/mods/<mod>/config`. |

## 5. Stubs de código

```csharp
// modded/Server/ClassDefinition.cs
using System.Text.Json.Serialization;

namespace CustomClasses;

/// <summary>JSON schema for one class file (config/classes/*.json[c]). Grows in items 003-008.</summary>
public sealed record ClassDefinition
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }                              // required — edition key + launcher label

    [JsonPropertyName("enabled")]
    public bool Enabled { get; init; } = true;                      // false → not registered

    [JsonPropertyName("baseEdition")]
    public string? BaseEdition { get; init; }                       // default "SPT Zero to hero" (resolved in loader)

    [JsonPropertyName("description")]
    public string? Description { get; init; }                       // launcher description (en for now)

    [JsonPropertyName("skills")]
    public Dictionary<string, int>? Skills { get; init; }           // skill name → starting level (0..51)
}
```

```csharp
// modded/Server/CustomClassesMod.cs
using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;                          // IOnLoad, OnLoadOrder
using SPTarkov.Server.Core.Helpers;                     // ModHelper
using SPTarkov.Server.Core.Services;                    // DatabaseService
using SPTarkov.Server.Core.Utils;                       // FileUtil, JsonUtil, TimeUtil
using SPTarkov.Server.Core.Utils.Cloners;               // ICloner
using SPTarkov.Server.Core.Models.Utils;                // ISptLogger
using SPTarkov.Server.Core.Models.Enums;                // SkillTypes
using SPTarkov.Server.Core.Models.Eft.Common;           // PmcData
using SPTarkov.Server.Core.Models.Eft.Common.Tables;    // ProfileSides, CommonSkill

namespace CustomClasses;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]   // ref: OnLoadOrder.cs:9
public class CustomClassesMod(
    ModHelper modHelper,
    FileUtil fileUtil,
    JsonUtil jsonUtil,
    TimeUtil timeUtil,
    DatabaseService databaseService,
    ICloner cloner,
    ISptLogger<CustomClassesMod> logger
) : IOnLoad
{
    private const string DefaultBaseEdition = "SPT Zero to hero";

    public Task OnLoad()
    {
        var classesPath = Path.Combine(
            modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()),   // ref: ModHelper.cs:10
            "config", "classes");

        if (!fileUtil.DirectoryExists(classesPath))   // ref: FileUtil.cs:48
        {
            logger.Info($"[CustomClasses] No classes folder at '{classesPath}' — no custom classes registered.");
            return Task.CompletedTask;
        }

        // PA-01-03: não-recursivo — só o topo de config/classes/ (subpastas ignoradas, úteis p/ rascunhos)
        var files = fileUtil.GetFiles(classesPath, false, "*.json")    // ref: FileUtil.cs:11
            .Concat(fileUtil.GetFiles(classesPath, false, "*.jsonc"))
            .ToList();

        var templates = databaseService.GetProfileTemplates();   // ref: DatabaseService.cs:141
        int loaded = 0, skipped = 0;

        foreach (var file in files)
        {
            var fileName = fileUtil.GetFileNameAndExtension(file);   // ref: FileUtil.cs:33
            try
            {
                var def = jsonUtil.Deserialize<ClassDefinition>(fileUtil.ReadFile(file));   // ref: ModHelper.cs:28
                if (def is null || string.IsNullOrWhiteSpace(def.Name))
                {
                    logger.Error($"[CustomClasses] '{fileName}': missing required 'name' — skipped.");
                    skipped++; continue;
                }
                if (!def.Enabled)
                {
                    logger.Info($"[CustomClasses] '{def.Name}' is disabled in '{fileName}' — skipped.");
                    skipped++; continue;
                }
                if (RegisterClass(templates, def, fileName)) { loaded++; } else { skipped++; }
            }
            catch (Exception ex)
            {
                logger.Error($"[CustomClasses] '{fileName}': failed to parse/register — skipped. {ex.Message}");
                skipped++;
            }
        }

        logger.Info($"[CustomClasses] Loaded {loaded} class(es), skipped {skipped}, from '{classesPath}'.");
        return Task.CompletedTask;
    }

    private bool RegisterClass(IDictionary<string, ProfileSides> templates, ClassDefinition def, string fileName)
    {
        // Collision guard: never overwrite a vanilla/other-mod/duplicate edition.
        if (templates.ContainsKey(def.Name!))
        {
            logger.Warning($"[CustomClasses] Edition '{def.Name}' already exists — '{fileName}' skipped (no overwrite).");
            return false;
        }

        var baseKey = string.IsNullOrWhiteSpace(def.BaseEdition) ? DefaultBaseEdition : def.BaseEdition!;
        if (!templates.TryGetValue(baseKey, out var baseSides) || baseSides is null)
        {
            logger.Error($"[CustomClasses] '{fileName}': base edition '{baseKey}' not found — skipped. Available: {string.Join(", ", templates.Keys)}");
            return false;
        }

        // Deep clone (ICloner.Clone is deep) — mutating nested Skills.Common is safe.  // ref: ICloner.cs:5, CR-01-03
        var sides = cloner.Clone(baseSides);
        if (sides is null)
        {
            logger.Error($"[CustomClasses] '{fileName}': clone of base '{baseKey}' returned null — skipped.");
            return false;
        }

        // GetText returns the key verbatim when unregistered (ref: ServerLocalisationService.cs:163); literal en text for now (item 008 = locale keys).
        sides.DescriptionLocaleKey = string.IsNullOrWhiteSpace(def.Description) ? def.Name : def.Description;

        // PA-01-02: contagens por-lado + aviso se um lado aplicar 0 com skills configuradas
        var usecApplied = ApplySkills(sides.Usec?.Character, def);
        var bearApplied = ApplySkills(sides.Bear?.Character, def);
        if (def.Skills is { Count: > 0 } && (usecApplied == 0 || bearApplied == 0))
        {
            logger.Warning($"[CustomClasses] '{def.Name}': a side applied 0 skills (usec={usecApplied}, bear={bearApplied}) — check base template.");
        }

        templates[def.Name!] = sides;
        logger.Info($"[CustomClasses] Registered '{def.Name}' (base '{baseKey}', skills usec={usecApplied}/bear={bearApplied}) from '{fileName}'.");
        return true;
    }

    /// <summary>Applies the class skills to one side; returns the count applied (0 if no skills/side).</summary>
    private int ApplySkills(PmcData? character, ClassDefinition def)
    {
        if (character?.Skills?.Common is null || def.Skills is null || def.Skills.Count == 0)
        {
            return 0;
        }

        var common = character.Skills.Common.ToList();   // ref: BotBase.cs:412
        var applied = 0;
        foreach (var (skillName, level) in def.Skills)
        {
            // PA-01-01: TryParse aceita numérico/indefinido — exigir IsDefined p/ rejeitar skill fantasma
            if (!Enum.TryParse<SkillTypes>(skillName, ignoreCase: true, out var skill) || !Enum.IsDefined(typeof(SkillTypes), skill))
            {
                logger.Warning($"[CustomClasses] '{def.Name}': unknown skill '{skillName}' — ignored.");
                continue;
            }

            var progress = Math.Clamp(level, 0, 51) * 100d;          // ref: ProfileHelper.cs:460/543
            var entry = common.FirstOrDefault(s => s.Id == skill);   // ref: ProfileHelper.cs:509
            if (entry is null)
            {
                // CR-01-01: new skill (not in base) — set LastAccess to now so fatigue/decay math is sane.
                common.Add(new CommonSkill { Id = skill, Progress = progress, LastAccess = timeUtil.GetTimeStamp() });
            }
            else
            {
                entry.Progress = progress;
            }
            applied++;
        }

        character.Skills.Common = common;
        return applied;
    }
}
```

```jsonc
// modded/Server/config/classes/exampleClass.jsonc
{
  // "name" (obrigatório): rótulo da edition no launcher + chave única
  "name": "Example Class",
  // "enabled" (opcional, default true): false desliga sem apagar o arquivo
  "enabled": true,
  // "baseEdition" (opcional, default "SPT Zero to hero" = stash vazio)
  "baseEdition": "SPT Zero to hero",
  // "description" (opcional): texto no launcher (em inglês por enquanto; pt-BR no item 008)
  "description": "Example class demonstrating the CustomClasses JSON format.",
  // "skills" (opcional): nome da skill -> nível inicial (0..51)
  "skills": {
    "Endurance": 5,
    "Strength": 3
  }
}
```

```jsonc
// modded/Server/config/classes/testClass.jsonc  (migra a Test Class do item 001)
{
  "name": "Test Class",
  "baseEdition": "SPT Zero to hero",
  "description": "Test class for the CustomClasses walking skeleton.",
  "skills": { "Endurance": 5, "Strength": 3 }
}
```

**Patch no `compile-mod.sh`** — inserir **logo após** `SERVER_DEST_SHOWN="$SERVER_DEST"; BUILT_SERVER=1` (dentro do ramo `server`, já sob o guard `-d "$SPT_PATH"`, com `SERVER_DEST` em escopo) — PA-01-04:
```bash
# Copiar config/ do mod (JSONs de classe etc.) para o install do servidor
if [[ -d "$(dirname "$CSPROJ")/config" ]]; then
  cp -r "$(dirname "$CSPROJ")/config" "$SERVER_DEST/"
  echo "  ✓ config/ → $SERVER_DEST"
fi
```

## 6. Fluxo de dados

```
[boot] DI → CustomClassesMod.OnLoad() em PostDBModLoader+1
  → path = GetAbsolutePathToModFolder()/config/classes      // ModHelper.cs:10
  → FileUtil.GetFiles("*.json"|"*.jsonc")                    // FileUtil.cs:11
  → para cada arquivo (try/catch isolado):
      → JsonUtil.Deserialize<ClassDefinition>(ReadFile)      // ModHelper.cs:28
      → valida name / enabled
      → RegisterClass: clona base (def.BaseEdition|default)  // ICloner.cs:5
          → DescriptionLocaleKey = description|name          // ServerLocalisationService.cs:163
          → ApplySkills(Usec/Bear): nome→SkillTypes, Progress=nível*100, LastAccess=now  // ProfileHelper.cs:460 / BotBase.cs:426
          → templates[name] = sides                          // DatabaseService.cs:141
  → log resumo (loaded/skipped)
[launcher] lista editions = templates.Keys                   // LauncherController.cs:48
```

## 7. Riscos e dependências

- **Shipping dos JSONs:** sem o patch no `compile-mod.sh`, os arquivos de classe **não** vão para o install e o loader não acha nada. Tarefa obrigatória do checklist.
- **`.json` vs `.jsonc`:** aceitamos os dois; o `JsonUtil` tolera comentários (usado pelo SkillDistribution com `.jsonc`). Confirmar no 1º build que comentários não quebram o parse.
- **`enabled`/`baseEdition` defaults:** dependem do System.Text.Json **não** chamar o setter quando o campo é omitido (mantém o inicializador). Confirmar no teste (campo omitido → default aplicado).
- **`Enum.TryParse<SkillTypes>`** aceita nomes do enum (ex.: "Endurance"). Skills do Skills-Extended (strings fora do enum) cairão em "unknown skill — ignored" → tratadas no item 006.
- **Coexistência com RZ** (clobber) — item 007; testar com RZ desabilitado.
- **CR-01-01** resolvido aqui (LastAccess); **CR-01-04** resolvido (JSON).
- **Ordem:** `PostDBModLoader+1` mantém DB de perfis pronto.

## 8. Checklist de implementação

- [x] Criar `modded/Server/ClassDefinition.cs` (DTO).
- [x] Refatorar `modded/Server/CustomClassesMod.cs` para o loader (enumera + valida + registra; resumo no log).
- [x] Criar `modded/Server/config/classes/exampleClass.jsonc` (exemplo documentado) e `testClass.jsonc` (migração do 001).
- [x] Patch no `.agents/scripts/compile-mod.sh`: copiar `config/` no install server-csharp.
- [x] `/compile-mod CustomClasses` → DLL + `config/classes/*.jsonc` em `SPT/user/mods/CustomClasses/` (0 warn/err; config confirmado no install).
- [ ] Playtest (RZ desabilitado): launcher mostra "Example Class" + "Test Class"; criar cada um → skills corretas + stash vazio; log de resumo "Loaded 2 class(es)".
- [ ] Corner cases: pasta vazia (0 classes, server ok); arquivo malformado (pulado, outros carregam); `enabled:false` (não registra); `name` ausente (pulado); skill inválida/numérica (ignorada — PA-01-01); base inexistente (classe pulada).
- [ ] PA-01-05: arquivo com **comentários** (`.jsonc`) carrega sem erro; arquivo **sem** `enabled` carrega como **habilitado** (default mantido). Se algum falhar, aplicar o caminho alternativo do PA-01-05.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Spec técnica criada via `/create-technical-spec` |
| 2026-06-07 | Review 01 aplicada (PA-01-01..05): `Enum.IsDefined`, contagem de skills por-lado, loader não-recursivo documentado, posição do patch no compile-mod, testes de JSONC/`enabled` no checklist |
