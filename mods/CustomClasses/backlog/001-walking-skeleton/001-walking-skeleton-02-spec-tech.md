# 001 — Scaffold + 1 classe (walking skeleton) · Spec Técnica

**Mod:** CustomClasses
**Spec funcional:** [001-walking-skeleton-01-spec.md](001-walking-skeleton-01-spec.md)
**Criado:** 2026-06-07

> Mod **server-side** (SPT 4.0, C# .NET 9). Fonte de verdade: código do servidor SPT em [references/spt-source/](../../../../references/spt-source/) (não há Assembly EFT/Harmony aqui). Toda referência cita `arquivo.cs:linha`. Esqueleto compilável espelhado do mod de referência [SkillDistributionServer](../../../SkillDistribution/original/SkillDistributionServer/).

## 1. Estratégia

Não há patch de cliente. A integração é um **serviço de servidor via DI**: uma classe `[Injectable]` que implementa `IOnLoad` e roda em `OnLoadOrder.PostDBModLoader + 1` (depois do banco de perfis carregado). No `OnLoad`, o mod:

1. Obtém o dicionário de templates de perfil (`DatabaseService.GetProfileTemplates()` → `Dictionary<string, ProfileSides>`).
2. **Clona** a edition base definida pela classe (campo `BaseEdition`, default `"SPT Zero to hero"` — começa com **stash vazio**, para a classe controlar 100% dos próprios itens; "Standard" traz itens iniciais indesejados) via `ICloner` — garante um `Character` (`PmcData`) completo e válido sem mutar o template vanilla; se a chave base não existir, **aborta com log claro** (sem fallback).
3. Sobrescreve as **skills** do `Character` (USEC e BEAR) com os níveis da classe de teste (`Progress = nível × 100`).
4. Define `DescriptionLocaleKey` com o **texto inglês literal** (o `ServerLocalisationService.GetText` retorna a própria chave quando não registrada → mostra o texto; locale real fica no item 008).
5. Insere sob uma **nova chave** (`"Test Class"`) no dicionário, com guarda de idempotência/colisão (nunca sobrescreve chave existente).

O launcher lista a edition automaticamente (lê as chaves do dicionário) e a criação de perfil clona esse template. Alternativas descartadas: (a) montar um `PmcData` do zero — frágil (precisa de equipment root, stash, containers internos); (b) Harmony no cliente — desnecessário, o registro é 100% server-side.

## 2. Pontos de integração (SPT server)

| Alvo (spt-source) | Tipo | Motivo |
|---|---|---|
| [`DatabaseService.cs:141` `GetProfileTemplates()`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/DatabaseService.cs#L141) | Ler + mutar dict | Registrar a edition (adicionar chave) |
| [`LauncherController.cs:48` `Editions = ...Select(x => x.Key)`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/LauncherController.cs#L48) | Comportamento | Launcher lista a edition pela chave |
| [`LauncherController.cs:63` `GetText(DescriptionLocaleKey)`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/LauncherController.cs#L63) | Comportamento | Descrição da edition (server locale) |
| [`ServerLocalisationService.cs:163` `return value ?? key`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/ServerLocalisationService.cs#L163) | Comportamento | Chave não registrada → retorna a própria string (permite texto literal) |
| [`CreateProfileService.cs:44` clona `GetProfileTemplateForSide(edition, side)`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/CreateProfileService.cs#L44) | Comportamento | Criar perfil clona o `Character` da edition |
| [`ProfileTemplate.cs:7/25` `ProfileSides`/`TemplateSide`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/ProfileTemplate.cs#L7) | Modelo | Shape do template (`DescriptionLocaleKey`, `Usec`/`Bear` → `Character`) |
| [`BotBase.cs:410/426` `Skills`/`CommonSkill`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/BotBase.cs#L410) | Modelo | `Common: IEnumerable<CommonSkill>`; `Id: SkillTypes` (enum), `Progress: double` |
| [`ProfileHelper.cs:460/543` `Progress >= 5100` (=51), cap 5100](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/ProfileHelper.cs#L460) | Constante | **Progress = nível × 100** (nível 51 = 5100) |
| [`SkillDisctributionMod.cs:8` `[Injectable] IOnLoad`](../../../SkillDistribution/original/SkillDistributionServer/SkillDisctributionMod.cs#L8) | Esqueleto | Padrão de server mod SPT 4.0 |
| [`SkillDistributionMetadata.cs:5` `AbstractModMetadata`](../../../SkillDistribution/original/SkillDistributionServer/SkillDistributionMetadata.cs#L5) | Esqueleto | Record de metadados do mod |

## 3. Novas propriedades F12 (BepInEx)

Não se aplica — item **server-side**, sem `ConfigEntry`. O F12 (e seu seletor de língua) entra a partir do item 008.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Server/CustomClasses.Server.csproj` | CRIAR | Projeto C# net9.0; `AssemblyName=CustomClasses-Server`; PackageReference `SPTarkov.Server.Core`/`SPTarkov.DI`/`SPTarkov.Common`. |
| `modded/Server/CustomClassesMetadata.cs` | CRIAR | Record `AbstractModMetadata` (GUID `customclasses.mdj`, autor mdj, SPT `~4.0.0`). |
| `modded/Server/CustomClassesMod.cs` | CRIAR | `[Injectable] IOnLoad` que injeta a edition da classe de teste no `PostDBModLoader`. |

> 001 **hardcoda** a classe de teste (sem JSON) — carregamento de arquivo + tratamento de malformado entram no item 002 (por isso o corner case "definição malformada" é vacuamente satisfeito aqui).

## 5. Stubs de código

> Compiláveis num projeto net9.0 com os pacotes `SPTarkov.*`. Cada referência ao servidor SPT comentada com `// ref:`.

```xml
<!-- modded/Server/CustomClasses.Server.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>CustomClasses</RootNamespace>
    <AssemblyName>CustomClasses-Server</AssemblyName>
    <Version>0.1.0</Version>
  </PropertyGroup>
  <ItemGroup>
    <!-- TODO confirmar versão do pacote: SkillDistribution usa 4.0.0, Skills-Extended 4.0.2; alinhar ao SPT 4.0.13 instalado -->
    <PackageReference Include="SPTarkov.Server.Core" Version="4.0.0" />
    <PackageReference Include="SPTarkov.DI" Version="4.0.0" />
    <PackageReference Include="SPTarkov.Common" Version="4.0.0" />
  </ItemGroup>
</Project>
```

```csharp
// modded/Server/CustomClassesMetadata.cs
using SPTarkov.Server.Core.Models.Spt.Mod;   // ref: SkillDistributionMetadata.cs:1

namespace CustomClasses;

public record CustomClassesMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "customclasses.mdj";
    public override string Name { get; init; } = "CustomClasses";
    public override string Author { get; init; } = "mdj";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("0.1.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; }
    public override string License { get; init; } = "MIT";
}
```

```csharp
// modded/Server/CustomClassesMod.cs
using SPTarkov.DI.Annotations;                         // ref: SkillDisctributionMod.cs:1
using SPTarkov.Server.Core.DI;                         // IOnLoad, OnLoadOrder  // ref: SkillDisctributionMod.cs:2
using SPTarkov.Server.Core.Services;                   // DatabaseService
using SPTarkov.Server.Core.Utils.Cloners;              // ICloner  // ref: Utils/Cloners/ICloner.cs:3
using SPTarkov.Server.Core.Models.Utils;               // ISptLogger
using SPTarkov.Server.Core.Models.Enums;               // SkillTypes
using SPTarkov.Server.Core.Models.Eft.Common;          // PmcData  // ref: PmcData.cs:5
using SPTarkov.Server.Core.Models.Eft.Common.Tables;   // ProfileSides, CommonSkill, Skills

namespace CustomClasses;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]   // ref: TestMod.cs:26 (PostDBModLoader+1); OnLoadOrder.cs:9
public class CustomClassesMod(
    DatabaseService databaseService,
    ICloner cloner,
    ISptLogger<CustomClassesMod> logger
) : IOnLoad
{
    // Definição de classe (no 001 hardcoded; vira o schema do JSON no item 002).
    // BaseEdition é por-classe (default "SPT Zero to hero" — stash vazio, classe controla os itens) — PA-01-01.
    private sealed record ClassDefinition(
        string EditionKey,
        string BaseEdition,
        string Description,
        Dictionary<SkillTypes, int> Skills   // nível inicial 0..51 (Progress = nível*100)
    );

    private static readonly ClassDefinition TestClass = new(
        EditionKey: "Test Class",
        BaseEdition: "SPT Zero to hero",
        Description: "Test class for the CustomClasses walking skeleton.",
        Skills: new() { { SkillTypes.Endurance, 5 }, { SkillTypes.Strength, 3 } }
    );

    public Task OnLoad()
    {
        try
        {
            var templates = databaseService.GetProfileTemplates();   // ref: DatabaseService.cs:141

            // Idempotência + colisão: nunca sobrescrever uma edition existente (vanilla / outro mod / nossa)
            if (templates.ContainsKey(TestClass.EditionKey))
            {
                logger.Info($"[CustomClasses] Edition '{TestClass.EditionKey}' already present — skipping (idempotent).");
                return Task.CompletedTask;
            }

            // Base por-classe; aborta com log claro se ausente (sem fallback) — PA-01-01
            if (!templates.TryGetValue(TestClass.BaseEdition, out var baseSides) || baseSides is null)
            {
                logger.Error($"[CustomClasses] Base edition '{TestClass.BaseEdition}' not found — cannot register '{TestClass.EditionKey}'.");
                return Task.CompletedTask;
            }

            var sides = cloner.Clone(baseSides);   // ref: ICloner.cs:5 — retorna T? (clone profundo)
            if (sides is null)                      // guarda de nullability — PA-01-03
            {
                logger.Error($"[CustomClasses] Clone of base edition '{TestClass.BaseEdition}' returned null.");
                return Task.CompletedTask;
            }

            // GetText retorna a própria chave quando não registrada (ref: ServerLocalisationService.cs:163);
            // texto literal en funciona agora; locale keys reais entram no item 008.
            sides.DescriptionLocaleKey = TestClass.Description;

            ApplySkills(sides.Usec?.Character);
            ApplySkills(sides.Bear?.Character);

            templates[TestClass.EditionKey] = sides;
            logger.Info($"[CustomClasses] Registered edition '{TestClass.EditionKey}' ({TestClass.Skills.Count} skills, base '{TestClass.BaseEdition}').");
        }
        catch (Exception ex)
        {
            logger.Error($"[CustomClasses] Failed to register edition '{TestClass.EditionKey}': {ex}");
        }

        return Task.CompletedTask;
    }

    private static void ApplySkills(PmcData? character)
    {
        if (character?.Skills?.Common is null)
        {
            return;
        }

        // Common é IEnumerable<CommonSkill> (ref: BotBase.cs:412) → materializar para mutar
        var common = character.Skills.Common.ToList();
        foreach (var (skill, level) in TestClass.Skills)
        {
            var progress = Math.Clamp(level, 0, 51) * 100d;          // ref: ProfileHelper.cs:460/543 (5100 == nível 51)
            var entry = common.FirstOrDefault(s => s.Id == skill);   // ref: ProfileHelper.cs:509
            if (entry is null)
            {
                common.Add(new CommonSkill { Id = skill, Progress = progress });
            }
            else
            {
                entry.Progress = progress;
            }
        }

        character.Skills.Common = common;
    }
}
```

## 6. Fluxo de dados

```
[boot do servidor]
  → SPT carrega DB de perfis (Templates.Profiles)
  → DI roda CustomClassesMod.OnLoad() em PostDBModLoader+1   // CustomClassesMod.cs
      → GetProfileTemplates()["Standard"]                    // ref: DatabaseService.cs:141
      → cloner.Clone(baseSides) → set Skills.Common.Progress (nível*100)  // ref: BotBase.cs:426 / ProfileHelper.cs:460
      → set DescriptionLocaleKey = "<texto en>"              // ref: ServerLocalisationService.cs:163
      → templates["Test Class"] = sides
[launcher conecta]
  → LauncherController.Connect(): Editions = templates.Keys  // ref: LauncherController.cs:48
  → ProfileDescriptions = GetText(DescriptionLocaleKey)      // ref: LauncherController.cs:63
[jogador cria perfil "Test Class"]
  → CreateProfileService.CreateProfile() clona o Character   // ref: CreateProfileService.cs:44
  → personagem nasce com Endurance 5 / Strength 3
```

## 7. Riscos e dependências

- **Edition base (por-classe, default `"SPT Zero to hero"`)**: o campo `BaseEdition` da classe define a base; default `"SPT Zero to hero"` — **começa com stash vazio**, requisito para a classe controlar 100% dos próprios itens (decisão de design confirmada no playtest 2026-06-07; "Standard" traz itens iniciais indesejados). Se a chave não existir → **aborta com log claro, sem fallback** (PA-01-01); o código loga as chaves disponíveis. Chave confirmada existente no SPT 4.0.13 instalado (consta no dropdown do launcher).
- **Idioma misto no launcher (PA-01-04):** com server locale ≠ `en`, a descrição da nossa classe aparece em inglês enquanto as nativas aparecem no idioma do server. Esperado no 001 (en-only); resolvido no item 008.
- **Remoção do mod (PA-01-06):** a injeção é **em memória no `OnLoad`** (não persiste em disco). Remover/desabilitar o DLL → `OnLoad` não roda → a edition some no próximo boot, sem cleanup.
- **`SkillTypes`** deve conter `Endurance`/`Strength` (vanilla — ok). Skills novas (Skills-Extended) não estão no enum → item 006.
- **`DescriptionLocaleKey` como texto literal** é stopgap (funciona via fallback do `GetText`); locale keys reais por idioma entram no 008.
- **Versão dos pacotes `SPTarkov.*`**: TODO confirmar (4.0.0 vs 4.0.2) alinhada ao SPT instalado; `dotnet restore` precisa de acesso ao feed NuGet.
- **Ordem de carregamento**: `PostDBModLoader + 1` garante DB de perfis pronto antes da injeção.
- **Idempotência/colisão**: guarda `ContainsKey` evita duplicar/ sobrescrever em restart ou colisão.
- **Sem client**: nenhuma dependência de BepInEx/Unity neste item.
- **Build**: depende do `compile-mod.sh` estendido (item 000, ✅) — detecta o `Server.csproj` como `server-csharp` e instala em `SPT/user/mods/CustomClasses/`.

## 8. Checklist de implementação

- [x] Criar `modded/Server/CustomClasses.Server.csproj` (net9.0, pacotes SPTarkov.*).
- [x] Criar `modded/Server/CustomClassesMetadata.cs` (record `AbstractModMetadata`).
- [x] Criar `modded/Server/CustomClassesMod.cs` (`[Injectable] IOnLoad` com a lógica de injeção).
- [x] `dotnet restore`/`build` resolve os pacotes (`SPTarkov.* 4.0.0` ✓; `SemanticVersioning` transitivo ✓) — PA-01-02 confirmado.
- [x] `/compile-mod CustomClasses` builda e instala `CustomClasses-Server.dll` em `SPT/user/mods/CustomClasses/` (0 warnings/erros).
- [ ] Subir o servidor SPT: confirmar log "Registered edition 'Test Class'" e ausência de exceção.
- [ ] **Confirmar a chave da edition base** no 1º boot (logar `GetProfileTemplates().Keys`); ajustar `TestClass.BaseEdition` se `"Standard"` não existir — PA-01-01.
- [ ] Launcher mostra a edition "Test Class" com a descrição em inglês.
- [ ] Criar perfil "Test Class" (USEC e BEAR): in-game, Endurance = 5 e Strength = 3; demais skills no padrão.
- [ ] Reiniciar o servidor: edition não duplica; perfis existentes intactos (idempotência).
- [ ] Remover o mod: launcher volta só com edições nativas, sem erro.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Spec técnica criada via `/create-technical-spec` |
| 2026-06-07 | Review 01 aplicada (PA-01-01..06): `BaseEdition` por-classe, abortar sem fallback, guarda de null no clone, notas de idioma/remoção, checklist de confirmação de chave/versão |
