# 005 — Multiplicadores de skill por classe (client) · Spec Técnica

**Mod:** CustomClasses
**Slug:** 005-skill-multipliers
**Criado:** 2026-06-07

> Item **híbrido** (1º trabalho no **client** BepInEx). **Server:** cada classe define `skillMultipliers`; um `StaticRouter` serve os fatores da classe do **session atual** (resolve sessionId → edition). **Client:** plugin BepInEx busca os fatores quando o perfil é conhecido, faz **Prefix** em `AbstractSkillClass.OnTrigger` escalando o XP (`val`), e **Postfix** em `SkillPanel`/`SkillTooltip` pra mostrar `+X%/−X%`. Referência **conceitual** (NÃO copiar — GPL): `mods/SkillDistribution/original/`.

## 1. Estratégia

- **Ganho de XP (client):** `AbstractSkillClass.OnTrigger(SkillManager.SkillActionClass, float val)` ([AbstractSkillClass.cs:100](../../../../references/eft-decompiled/Assembly-CSharp/AbstractSkillClass.cs#L100)) é o ponto onde a skill recebe XP (`SetCurrent(Current + val)`). **Prefix** que multiplica `val` pelo fator da skill (`__instance.Id`), com clamp `≥ 0`. Cobre raid e fora-de-raid (toda ação de skill passa por aqui). XP de fonte não-gameplay (quest/instantâneo) chama `SetCurrent` direto e **não** é escalado (D5 = só ganho natural).
- **Config por classe (server):** `ClassDefinition` ganha `skillMultipliers` (skill→fator). O loader agrega num **registry** por nome de classe (= edition). Um `StaticRouter` (`/customclasses/skill-multipliers`) resolve `sessionId → profile.Edition → registry[edition]` e retorna os fatores (JSON). Edition vanilla → vazio.
- **Descoberta no client:** ao selecionar/carregar o perfil, o client faz `GET` na rota e guarda o dict `ESkillId→fator` (1× por sessão; rebuild na troca de perfil). Sem rede em hot path.
- **UI (client):** Postfix em `SkillPanel.method_1()` (linha da skill) e `SkillTooltip.Show(...)` (tooltip) pra anexar `+X%/−X%`. Classes de UI **ofuscadas** → resolver por nome/assinatura via SPT.Reflection (como o SkillDistribution faz). **TODO confirmar** arquivo:linha exatos no decompilado durante o `/code-mod`.

## 2. Pontos de patch / referência

| Símbolo | Arquivo | Uso |
|---|---|---|
| `AbstractSkillClass.OnTrigger(SkillActionClass, float val)` | [AbstractSkillClass.cs:100](../../../../references/eft-decompiled/Assembly-CSharp/AbstractSkillClass.cs#L100) | **Prefix** escala `val` (XP) |
| `AbstractSkillClass.Id` (`ESkillId`) | [AbstractSkillClass.cs:14](../../../../references/eft-decompiled/Assembly-CSharp/AbstractSkillClass.cs#L14) | chave do fator (nome da skill) |
| `AbstractSkillClass.SetCurrent` | [AbstractSkillClass.cs:115](../../../../references/eft-decompiled/Assembly-CSharp/AbstractSkillClass.cs#L115) | aplica XP (clamp 0..5100) — não patch |
| `GameWorld.OnGameStarted` | [EFT/GameWorld.cs:2584](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L2584) | hook p/ garantir config carregada em raid |
| `SkillPanel.method_1()` | decompilado (ofuscado) — **TODO confirmar** | Postfix: `+X%/−X%` na linha (ref. SkillDistribution `SkillPanelPatch.cs`) |
| `SkillTooltip.Show(SkillClass)` | decompilado (ofuscado) — **TODO confirmar** | Postfix: `+X%/−X%` no tooltip (ref. SkillDistribution `SkillTooltipPatch.cs`) |
| `StaticRouter` / `RouteAction<T>` | [spt-source Router.cs:66/184](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/DI/Router.cs#L66) | rota do server |
| `RequestHandler.GetJson(url)` | client EFT (ref. SkillDistribution `ServerConfig.cs:23`) | client busca a config |

## 3. Novas propriedades F12 (client)

| Nome (EN) | pt-BR | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| `EnableSkillMultipliers` | Ativar multiplicadores de skill | bool | `true` | Liga/desliga a escala de ganho de XP de skill por classe. |
| `ShowMultiplierOnSkills` | Mostrar na tela de Skills | bool | `true` | Exibe `+X%/−X%` na linha e no tooltip de cada skill com multiplicador. |

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Server/ClassDefinition.cs` | MODIFICAR | + `skillMultipliers` (Dictionary<string,double>). |
| `modded/Server/SkillMultiplierRegistry.cs` | CRIAR | `[Injectable]` singleton: `Dictionary<string(edition), Dictionary<string(skill),double>>`. Preenchido pelo loader. |
| `modded/Server/SkillMultipliersRouter.cs` | CRIAR | `StaticRouter` `/customclasses/skill-multipliers` → resolve sessionId→edition→registry. |
| `modded/Server/CustomClassesMod.cs` | MODIFICAR | No `RegisterClass`, gravar `def.SkillMultipliers` no registry (com validação Enum/clamp). |
| `modded/Client/CustomClasses.Client.csproj` | CRIAR | netstandard2.1, Lib.Harmony, refs Assembly-CSharp/BepInEx/spt-* /Newtonsoft/Unity. |
| `modded/Client/Plugin.cs` | CRIAR | `[BepInPlugin]` + `[BepInDependency("com.SPT.core")]`; Awake: config F12 + Enable patches. |
| `modded/Client/SkillMultipliers.cs` | CRIAR | Estado: `Dictionary<ESkillId,float>` atual + `Fetch()` (GET na rota) + clamp. |
| `modded/Client/Patches/OnTriggerPatch.cs` | CRIAR | Prefix `AbstractSkillClass.OnTrigger` → `val *= factor` (≥0). |
| `modded/Client/Patches/SkillPanelPatch.cs` | CRIAR | Postfix → label `+X%/−X%` na linha (se `ShowMultiplierOnSkills`). |
| `modded/Client/Patches/SkillTooltipPatch.cs` | CRIAR | Postfix → `+X%/−X%` no tooltip. |
| `modded/Client/Patches/ProfileReadyPatch.cs` | CRIAR | Hook de perfil selecionado/game-start → `SkillMultipliers.Fetch()`. |
| `modded/Server/config/classes/*.jsonc` | MODIFICAR | Popular `skillMultipliers` temáticos das 10 classes (gerador). |

## 5. Stubs de código

### ClassDefinition.cs (acréscimo)

```csharp
/// <summary>Optional. Skill name → XP-gain multiplier (1 = vanilla, 1.5 = +50%, 0.5 = -50%). (Item 005)</summary>
[JsonPropertyName("skillMultipliers")]
public Dictionary<string, double>? SkillMultipliers { get; init; }
```

### SkillMultiplierRegistry.cs (server, novo)

```csharp
using SPTarkov.DI.Annotations;

namespace CustomClasses;

/// <summary>Fatores de XP de skill por classe (edition) → (skill → fator). Preenchido no load.</summary>
[Injectable(InjectionType.Singleton)]
public class SkillMultiplierRegistry
{
    private readonly Dictionary<string, Dictionary<string, double>> _byEdition = new(StringComparer.Ordinal);

    public void Set(string edition, Dictionary<string, double> multipliers) => _byEdition[edition] = multipliers;

    public Dictionary<string, double> Get(string edition) =>
        _byEdition.TryGetValue(edition, out var m) ? m : new Dictionary<string, double>();
}
```

### SkillMultipliersRouter.cs (server, novo)

```csharp
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;                 // StaticRouter, RouteAction
using SPTarkov.Server.Core.Models.Common;      // MongoId
using SPTarkov.Server.Core.Models.Eft.Httpresponse;  // EmptyRequestData (TODO confirmar namespace)
using SPTarkov.Server.Core.Servers;            // SaveServer (resolve edition)
using SPTarkov.Server.Core.Utils;              // JsonUtil

namespace CustomClasses;

[Injectable]
public class SkillMultipliersRouter : StaticRouter   // ref: spt-source Router.cs:66
{
    public SkillMultipliersRouter(JsonUtil jsonUtil, SkillMultiplierRegistry registry, SaveServer saveServer)
        : base(jsonUtil,
        [
            new RouteAction<EmptyRequestData>(   // ref: Router.cs:184
                "/customclasses/skill-multipliers",
                async (url, info, sessionId, output) =>
                {
                    var edition = saveServer.GetProfile(sessionId)?.ProfileInfo?.Edition ?? "";
                    return jsonUtil.Serialize(registry.Get(edition));
                })
        ])
    { }
}
```
> **TODO confirmar:** namespace de `EmptyRequestData`, assinatura exata de `RouteAction<T>` e como o `SaveServer`/`ProfileHelper` expõe a `Edition` por sessionId (ver `CreateProfileService`/`ProfileHelper`).

### Client/Patches/OnTriggerPatch.cs (novo)

```csharp
using SPT.Reflection.Patching;
using HarmonyLib;
using System.Reflection;

namespace CustomClasses.Client;

public class OnTriggerPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod() =>
        AccessTools.Method(typeof(AbstractSkillClass), nameof(AbstractSkillClass.OnTrigger));   // ref: AbstractSkillClass.cs:100

    [PatchPrefix]
    private static void Prefix(AbstractSkillClass __instance, ref float val)
    {
        if (!Plugin.Enabled || val <= 0f) return;
        if (SkillMultipliers.TryGet(__instance.Id, out var factor))   // dict ESkillId→float (cache)
        {
            val *= factor < 0f ? 0f : factor;   // clamp ≥ 0 (debuff nunca remove XP)
        }
    }
}
```

### Client/Plugin.cs (esqueleto)

```csharp
using BepInEx;

namespace CustomClasses.Client;

[BepInPlugin("customclasses.mdj.client", "CustomClasses", "0.1.0")]
[BepInDependency("com.SPT.core", "4.0.0")]
public class Plugin : BaseUnityPlugin
{
    public static bool Enabled = true;
    public static bool ShowOnUi = true;

    private void Awake()
    {
        Enabled = Config.Bind("General", "EnableSkillMultipliers", true, "...").Value;
        ShowOnUi = Config.Bind("General", "ShowMultiplierOnSkills", true, "...").Value;
        new ProfileReadyPatch().Enable();
        new OnTriggerPatch().Enable();
        new SkillPanelPatch().Enable();
        new SkillTooltipPatch().Enable();
    }
}
```

> UI (`SkillPanelPatch`/`SkillTooltipPatch`): Postfix que, lendo a skill da instância de UI, anexa `+X%/−X%` quando `ShowOnUi` e `factor != 1`. **TODO confirmar** os nomes ofuscados (classe da linha + tooltip + campo da skill + elemento de texto) lendo o decompilado no `/code-mod` (usar `SkillDistribution/.../SkillPanelPatch.cs` + `SkillTooltipPatch.cs` como guia de QUAIS classes, reimplementando).

## 6. Fluxo de dados

```
JSON classe .skillMultipliers (skill→fator)
  → [server load] CustomClassesMod.RegisterClass → SkillMultiplierRegistry.Set(edition, mults)
  → [client, perfil pronto] ProfileReadyPatch → GET /customclasses/skill-multipliers
        → server resolve sessionId→Edition→registry.Get → JSON {skill→fator}
        → SkillMultipliers cache: Dictionary<ESkillId,float>
  → [gameplay] AbstractSkillClass.OnTrigger(val) → OnTriggerPatch.Prefix → val *= fator (≥0)  (AbstractSkillClass.cs:100)
  → [UI] SkillPanel.method_1 / SkillTooltip.Show → Postfix anexa "+X%/−X%"
```

## 7. Riscos e dependências

- **Classes de UI ofuscadas** (`SkillPanel`/`SkillTooltip`) — resolver por assinatura; confirmar no decompilado no `/code-mod`. Maior risco do item.
- **Hot path** (`OnTrigger`): Prefix deve ser O(1) — lookup num `Dictionary<ESkillId,float>` cacheado, **sem LINQ/alocação/log** por chamada (csharp-best-practices §1/§3). Try/catch no corpo (não derrubar o original).
- **Lifecycle (client):** carregar a config **1× por perfil/sessão**, limpar/rebuild na troca de perfil; `OnTrigger` no menu/hideout também dispara → cache deve existir desde o perfil selecionado (não só em raid).
- **GPL:** SkillDistribution é só **referência conceitual** — reimplementar; não copiar código.
- **Server↔client:** primeiro uso de rota neste mod; confirmar `RequestHandler.GetJson` no client EFT + `StaticRouter` no SPT 4.0.
- **Scaffold client:** primeiro `.csproj` client do mod — o `compile-mod.sh` já suporta client-csharp (instala em `BepInEx/plugins/`). Mod vira **híbrido** (2 DLLs).
- **FIKA/coop:** multiplicador é por cliente (cada um aplica o seu) — OK.

## 8. Checklist de implementação

- [ ] `ClassDefinition` + `skillMultipliers`.
- [ ] `SkillMultiplierRegistry` (singleton) + preencher no `RegisterClass` (validar nome via `SkillTypes`, clamp ≥0, log).
- [ ] `SkillMultipliersRouter` (StaticRouter, resolve sessionId→edition). Confirmar APIs do SPT.
- [ ] `/compile-mod` server (rota responde).
- [ ] Projeto **client** (`csproj` + `Plugin.cs`) — scaffold BepInEx; `compile-mod` instala em `plugins/`.
- [ ] `SkillMultipliers` cache + `ProfileReadyPatch` (fetch no perfil pronto).
- [ ] `OnTriggerPatch` (Prefix escala `val`, clamp ≥0). Teste: skill com fator 2 ganha XP 2×.
- [ ] `SkillPanelPatch` + `SkillTooltipPatch` (UI `+X%/−X%`) — confirmar classes ofuscadas no decompilado.
- [ ] Popular `skillMultipliers` temáticos das 10 classes (gerador, ajustável no JSON).
- [ ] Playtest: fator >1/<1/1/ausente; raid + hideout; UI; perfil vanilla; multi-raid sem leak.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Spec técnica criada via `/create-technical-spec` (hook OnTrigger; registry+StaticRouter; projeto client BepInEx; UI SkillPanel/SkillTooltip; refs via Explore + AbstractSkillClass.cs verificado) |
