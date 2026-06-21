# 047 — Roster 11→6 (aplicar matriz) · Spec Técnica

**Mod:** CustomClasses
**Spec funcional:** [047-roster-6-classes-01-spec.md](047-roster-6-classes-01-spec.md)
**Criado:** 2026-06-21

> Fontes: 🥇 SPT server source (`references/spt-source/`) p/ lógica de servidor; 🥈 código do mod (`mods/CustomClasses/modded/Server/`). Toda referência com `arquivo.cs:linha`. **Não é uma feature Harmony/cliente** — é mudança server-side (config + DI), então o template de "patch" é adaptado.

## 1. Estratégia

047 é majoritariamente **dados** (a matriz nos `.jsonc`) + **dois pontos de código**, sem Harmony (o mod é server-side, padrão SPTarkov DI):

1. **Aplicar a matriz** — reescrever 4 `.jsonc`, criar 2, deletar 6, em `modded/Server/config/classes/`. O loader já existe: `CustomClassesMod.OnLoad` (`[Injectable] PostDBModLoader+1`, ref: CustomClassesMod.cs:19) lê cada `.jsonc`, pula `Enabled:false` (CustomClassesMod.cs:70) e registra via `ClassRegistrar.ValidateAndBuild`+`Commit`. **Deletar o arquivo = classe não registrada** no boot. Zero código novo aqui.
2. **Sync `SkillWeights.cs`** — adicionar 3 categorias de gem ao dict `Categories` (afeta só o *warning de custo* do editor; não é runtime de jogo).
3. **Rede de segurança contra perfil órfão** (requisito da spec funcional) — um `SaveLoadRouter` novo que, no load, remapeia perfis cuja `Edition` é de uma das **6 classes aposentadas** (lista pt+en) que sumiu dos templates → edição neutra (`orphanEditionFallback`, default `"Standard"`). **Escopado às deletadas** (não "qualquer edition ausente") p/ não pegar re-chave por idioma de classe mantida (PA-01-01). **Necessário** (não opcional): a investigação confirmou que deletar uma classe faz um perfil dela **crashar** (ver §2).

Alternativas descartadas: (a) *não deletar, só esconder da criação* (HiddenEditions/blacklist) — evitaria o órfão sem o router, mas deixa as 6 registradas (editor mostra 13, contradiz o roster limpo); decisão do usuário foi **deletar** (spec §corner case). (b) *Harmony em `TraderHelper`* — desnecessário; o `SaveLoadRouter` é o ponto de extensão canônico do SPT.

## 2. Pontos de integração / âncoras no SPT

| Alvo (SPT source) | Papel | Detalhe |
|---|---|---|
| [`SptProfile.cs:100`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Profile/SptProfile.cs#L100) | campo | `ProfileInfo.Edition` é `string?` nua (sem enum/validação) — `SptProfile.ProfileInfo` (SptProfile.cs:15) |
| `ProfileHelper.cs:806` | lookup | `databaseService.GetProfileTemplates()` → `Dictionary<string, ProfileSides>` |
| `ProfileHelper.cs:808-811` | órfão (1/2) | `GetProfileTemplateForSide`: `TryGetValue(edition)` falha → **retorna `null`** (loga erro) |
| `ClassRegistrar.cs:282` | dict | `Commit` escreve `GetProfileTemplates()[plan.Name] = Sides` — **mesmo dict** que o router lê (PA-01-06) |
| `TraderHelper.cs:147-150` | **crash** | `ResetTrader` passa `ProfileInfo.Edition` e acessa `.Trader` no resultado `null` → **NullReferenceException** p/ perfil órfão |
| `Router.cs:167-177` | **hook** | `abstract SaveLoadRouter` · `HandleLoadInternal(SptProfile)` — ponto de extensão do load |
| `SaveServer.cs:268` | invocação | `callback.HandleLoad(GetProfile(sessionID))` roda **todos** os `SaveLoadRouter` em cada load de perfil (sem validar edition) |
| `ProfileSaveLoadRouter.cs:11-21` | exemplo | shape de um `SaveLoadRouter` real (`GetHandledRoutes` + `HandleLoadInternal`) |
| `ClassRegistrar.cs:220` | parse mult | `Enum.TryParse<SkillTypes>(skillName, ignoreCase: true, ...)` — **caixa não importa no `.jsonc`** |
| `ClassRegistrar.cs:329,349` | set skill | parse ignoreCase + `CommonSkill { Id = SkillTypes, Progress = nível*100 }` |
| [`SkillTypes.cs:60,64,67`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Enums/SkillTypes.cs#L60) | enum | `Shadowconnections` (**c minúsculo!**), `BearAksystems`, `UsecArsystems` existem |

**⚠️ Caixa do `Shadowconnections`:** o enum é `Shadowconnections` (c minúsculo). No `.jsonc` tanto faz (`Enum.TryParse ignoreCase`, ClassRegistrar.cs:220/329). No **`SkillWeights.cs`** (usa o enum direto) **tem que ser `SkillTypes.Shadowconnections`** — `ShadowConnections` não compila. (Os docs/`class-matrix.mjs`/`skill-weights.mjs` usam `ShadowConnections` no lado JS — consistente entre si; só o C# exige a caixa do enum.)

## 3. Novas propriedades F12 (BepInEx)

**Nenhuma F12** — 047 é só a camada de skills (server-side); não há lever 🔧/🧪 (esses são 048–051). Introduz **1 config server-side** em `config/settings.jsonc`:

| Config (settings.jsonc) | Tipo | Padrão | Descrição |
|---|---|---|---|
| `orphanEditionFallback` | string | `"Standard"` | Edição neutra para a qual um perfil de classe removida é remapeado no load. Precisa existir nos templates (vanilla "Standard" persiste mesmo se escondida da criação pelo item 009). |

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `config/classes/medicoDeCombate.jsonc` | MODIFICAR | matriz nova (skills + skillMultipliers) — valores de `class-matrix.mjs` |
| `config/classes/fuzileiro.jsonc` | MODIFICAR | idem |
| `config/classes/cacador.jsonc` | MODIFICAR | idem |
| `config/classes/saqueador.jsonc` | MODIFICAR | idem (Lockpicking/Strength ×3 — ressalva peso-baixo, **não** é erro) |
| `config/classes/fantasma.jsonc` | CRIAR | nova classe + gear **placeholder**: clonar o loadout do `operadorFurtivo` (furtivo) **antes** de deletá-lo; curado depois (PA-01-04) |
| `config/classes/tanque.jsonc` | CRIAR | nova classe + gear **placeholder**: clonar o loadout do `operadorTatico`/`sobrevivencialista` (pesado) antes de deletar; curado depois (PA-01-04) |
| `config/classes/{armeiro,batedor,gerenteDeOperacoes,operadorFurtivo,operadorTatico,sobrevivencialista}.jsonc` | DELETAR | 6 aposentadas (repo + install) |
| `modded/Server/SkillWeights.cs` | MODIFICAR | +3 entradas em `Categories` (sub-tarefa c) |
| `modded/Server/OrphanEditionSaveLoadRouter.cs` | CRIAR | remap de edition órfã no load (rede de segurança) |
| `config/settings.jsonc` | MODIFICAR | + `orphanEditionFallback` |
| `scripts/class-matrix.mjs` | (fonte) | já contém a matriz — usar como referência ao escrever os `.jsonc` |

## 5. Stubs de código

```csharp
// modded/Server/OrphanEditionSaveLoadRouter.cs
using System.Reflection;
using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;                       // SaveLoadRouter, HandledRoute (ref: Router.cs:167,180)
using SPTarkov.Server.Core.Helpers;                  // ModHelper
using SPTarkov.Server.Core.Models.Eft.Profile;       // SptProfile (ref: SptProfile.cs:15,100)
using SPTarkov.Server.Core.Services;                 // DatabaseService
using SPTarkov.Server.Core.Utils;                    // FileUtil, JsonUtil
using SPTarkov.Server.Core.Models.Utils;             // ISptLogger

namespace CustomClasses;

/// <summary>
///   Item 047 — rede de segurança: ao carregar um perfil cuja Edition é de uma classe APOSENTADA
///   deletada (e some dos templates), remapeia para uma edição neutra. Sem isso, TraderHelper.ResetTrader
///   dá NRE no template null (ref: ProfileHelper.cs:808-811 → TraderHelper.cs:150).
///   PA-01-01: escopado à lista das 6 deletadas (pt+en), NÃO "qualquer edition ausente" — assim não
///   pega re-chaveamento por idioma de classe mantida (edition key = displayName[lang], CustomClassesMod.cs:77).
/// </summary>
[Injectable]
public class OrphanEditionSaveLoadRouter(
    ModHelper modHelper,
    FileUtil fileUtil,
    JsonUtil jsonUtil,
    DatabaseService databaseService,
    ISptLogger<OrphanEditionSaveLoadRouter> logger
) : SaveLoadRouter                                    // ref: Router.cs:167
{
    // name(==en) + pt das 6 deletadas no 047 (edition key = displayName[lang] ou name — CustomClassesMod.cs:77).
    private static readonly HashSet<string> RetiredEditions = new(StringComparer.OrdinalIgnoreCase)
    {
        "Armorer", "Armeiro", "Scout", "Batedor",
        "Operations Manager", "Gerente de Operações",
        "Stealth Operator", "Operador Furtivo",
        "Tactical Operator", "Operador Tático",
        "Survivalist", "Sobrevivencialista",
    };

    // HandleLoad roda p/ todo perfil no load (SaveServer.cs:268); a rota é só identificador.
    protected override List<HandledRoute> GetHandledRoutes() =>
        [new HandledRoute("customclasses-orphan-edition", false)];   // ref: ProfileSaveLoadRouter.cs:13

    protected override SptProfile HandleLoadInternal(SptProfile profile)   // ref: Router.cs:177
    {
        var edition = profile.ProfileInfo?.Edition;                       // ref: SptProfile.cs:100
        if (string.IsNullOrEmpty(edition) || !RetiredEditions.Contains(edition))
        {
            return profile;   // PA-01-01: só age em edition aposentada conhecida
        }

        // mesmo dict que o Commit escreve (ClassRegistrar.cs:282) e que GetProfileTemplateForSide lê (ProfileHelper.cs:806 / DatabaseService.cs:141)
        var templates = databaseService.GetProfileTemplates();
        if (templates.ContainsKey(edition))
        {
            return profile;   // ainda registrada (não deletada de fato) — nada a fazer
        }

        var fallback = LoadFallbackEdition();
        if (!templates.ContainsKey(fallback))                            // PA-01-03: defesa se o fallback não existir
        {
            var first = templates.Keys.FirstOrDefault();
            if (first is null)
            {
                logger.Error("[CustomClasses] Sem profile-templates — não foi possível remapear edition órfã.");
                return profile;
            }
            fallback = first;
        }

        logger.Warning($"[CustomClasses] Edition aposentada '{edition}' — remapeando perfil para '{fallback}'.");
        profile.ProfileInfo!.Edition = fallback;
        return profile;
    }

    /// <summary>Lê config/settings.jsonc → orphanEditionFallback (default "Standard"). PA-01-02.</summary>
    private string LoadFallbackEdition()
    {
        try
        {
            var configPath = System.IO.Path.Combine(
                modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()), "config");   // ref: ModHelper.cs:10
            var file = System.IO.Path.Combine(configPath, "settings.jsonc");
            if (!System.IO.File.Exists(file)) return "Standard";
            var s = jsonUtil.Deserialize<OrphanSettings>(fileUtil.ReadFile(file));
            var v = s?.OrphanEditionFallback?.Trim();
            return string.IsNullOrEmpty(v) ? "Standard" : v;
        }
        catch (Exception ex)
        {
            logger.Warning($"[CustomClasses] settings.jsonc inválido p/ orphanEditionFallback — usando 'Standard'. {ex.Message}");
            return "Standard";
        }
    }

    // PA-01-02: idealmente unificar com o LauncherSettings de CustomClassesMod.cs:143 num record compartilhado.
    private sealed record OrphanSettings
    {
        [JsonPropertyName("orphanEditionFallback")] public string? OrphanEditionFallback { get; init; }
    }
}
```

```csharp
// modded/Server/SkillWeights.cs — adicionar ao dict Categories (ref: SkillWeights.cs:122-148)
//   na seção C (Combat):
{ SkillTypes.UsecArsystems, "C" },      // ref: SkillTypes.cs:67
{ SkillTypes.BearAksystems, "C" },      // ref: SkillTypes.cs:64
//   na seção P (Practical):
{ SkillTypes.Shadowconnections, "P" },  // ref: SkillTypes.cs:60 — caixa do enum (c minúsculo)
```

> A matriz nos `.jsonc` é **dados** (sem stub) — cada classe segue o schema de `ClassDefinition` (name, displayName{en,pt}, baseEdition, description{en,pt}, skills{}, skillMultipliers{}, hideout{}, loadout{equipped,stash}); valores exatos em `class-matrix.mjs` + estações de §5 do class-levers.

## 6. Fluxo de dados

```
[boot] CustomClassesMod.OnLoad (CustomClassesMod.cs:28)
   → lê config/classes/*.jsonc (só os 6 + Peladão; 6 deletados não existem)
   → ClassRegistrar.ValidateAndBuild + Commit → injeta editions no profile-templates dict
   → skills iniciais: Enum.TryParse ignoreCase (ClassRegistrar.cs:329) → CommonSkill{Id, Progress=lvl*100} (:349)

[criar perfil] CreateProfileService.CreateProfile (CreateProfileService.cs:44)
   → GetProfileTemplateForSide(edition, side) → clona o template da classe → perfil novo

[load de perfil EXISTENTE] SaveServer.LoadProfileAsync (SaveServer.cs:249,268)
   → para cada SaveLoadRouter: HandleLoad(profile)
   → OrphanEditionSaveLoadRouter.HandleLoadInternal (Router.cs:177)
       → edition ∈ {6 aposentadas pt+en} e ∉ GetProfileTemplates()? → remap p/ orphanEditionFallback ("Standard")   ← evita o NRE de TraderHelper.cs:150
```

## 7. Riscos e dependências

- **Coordenação com a sessão do editor web** (🔴 a partir do `/code-mod`): o install é a fonte de verdade dos `.jsonc`; aplicar via `build-class-jsons.js --force` + `/sync-classes`, **sem clobberar** edições (memória `feedback_serve_inventory_clobber`; guard `--force-config` do compile-mod). O editor pode ter as classes registradas numa instância viva — coordenar antes.
- **`SkillWeights.cs` toca `modded/Server/`** (sessão paralela) — mudança aditiva mínima (3 linhas), mas recompila o server; coordenar.
- **Gear das 2 novas (fantasma/tanque):** depende de escolher um **profile-fonte** para o `extract-from-profile.mjs` (046). Itens compostos do stash devem nascer montados (preset/mods/ammo/contents) — pendência **P-7.3** da memória (validar in-game).
- **`OrphanEditionSaveLoadRouter` roda em todo load de perfil** — custo trivial (1 `ContainsKey`); idempotente (só age se órfão). Não toca perfis válidos.
- **`ClassRegistrar.Remove` existe** (hot-remove do editor) — não é usado aqui; o roster estático some por ausência do arquivo no boot.
- **Sem dependência de raid/Harmony** — nada de `GameWorld`/`BaseLocalGame`.

## 8. Checklist de implementação

- [x] Escrever os 6 `.jsonc` (4 modificar + 2 criar) com a matriz de `class-matrix.mjs` (hideout: Médico=MedStation, Fuzileiro=Workbench, Caçador=ShootingRange, Fantasma=WaterCloset, Saqueador=ScavCase, Tanque=RestSpace).
- [x] Gear de fantasma/tanque por **clone direto** do operadorFurtivo/operadorTatico (placeholder, antes de deletar).
- [x] Deletar os 6 `.jsonc` aposentados (repo). *(install: no `/compile-mod`)*
- [x] Adicionar as 3 entradas em `SkillWeights.cs` `Categories` (`UsecArsystems`/`BearAksystems`→C, `Shadowconnections`→P).
- [~] ~~Criar `OrphanEditionSaveLoadRouter.cs`~~ — **DESCOPADO** (decisão do usuário: sem perfis ao vivo; ver asbuild).
- [x] `check-skill-costs.mjs` — 6 em [28,32], custos batem com `class-matrix.mjs`.
- [ ] Validar in-game (item 052) — criar perfil de cada classe (skills/mults/loadout/hideout).

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid (AP-01) | N/A | Sem patch de raid; mod server DI (`IOnLoad` + `SaveLoadRouter`), não toca `GameWorld`/`BaseLocalGame`. |
| 2 | Filtro MainPlayer/Fika (AP-02) | N/A | Sem patch cliente que reage a ação de player. O multiplicador (005) é por-jogador; verificação Fika movida ao smoke do 052 (spec §critérios). |
| 3 | Alvos ofuscados/virtuais (AP-03) | N/A | Sem Harmony. Integração por API pública SPT (`SaveLoadRouter` Router.cs:167; `GetProfileTemplates` ProfileHelper.cs:806). |
| 4 | Estado via API canônica (AP-04) | ✅ | Skills via `CommonSkill`/`ProfileSides` (ClassRegistrar.cs:349); edition via `SaveLoadRouter` (Router.cs:177). Sem escrita crua fora de API. |
| 5 | Estado entre raids | ✅ | Skills/edition são do perfil (persistem); matriz nova só em perfil novo (spec §critério). Remap idempotente (age só se órfã). |
| 6 | ConfigEntry semântica (AP-05) | N/A (parcial ✅) | Sem F12. 1 config server `orphanEditionFallback` (default `"Standard"`, deve existir nos templates) — documentado §3. |
| 7 | Reentry-guard (AP-07) | N/A | Sem método patcheado re-invocado; `HandleLoad` roda 1×/load (SaveServer.cs:268). |
| 8 | Flags/caches pós-troca (AP-08) | N/A | Sem cache de intercept; sem estado entre invocações. |

## Histórico

| Data | Evento |
|---|---|
| 2026-06-21 | Spec técnica criada via `/create-technical-spec` |
| 2026-06-21 | Review 01 endereçada (6 pontos aceitos): remap escopado às 6 aposentadas pt+en (PA-01-01), `orphanEditionFallback` cabeado (PA-01-02), defesa de fallback ausente (PA-01-03), gear placeholder via clone (PA-01-04), âncora ProfileHelper 808-811 (PA-01-05), dict compartilhado Commit↔router confirmado (PA-01-06). |
| 2026-06-21 | `/code-mod`: **router DESCOPADO** (decisão do usuário — sem perfis ao vivo); PA-01-01/02/03 viram moot. Implementado config-only + sync do `SkillWeights.cs`. Ver `05-asbuild.md`. |
