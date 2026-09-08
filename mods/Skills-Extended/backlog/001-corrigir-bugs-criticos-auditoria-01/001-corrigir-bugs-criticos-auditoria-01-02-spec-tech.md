# 001 — Corrigir bugs críticos da auditoria 01 · Spec Técnica

**Mod:** Skills-Extended
**Spec funcional:** [001-corrigir-bugs-criticos-auditoria-01-01-spec.md](001-corrigir-bugs-criticos-auditoria-01-01-spec.md)
**Criado:** 2026-09-07

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/) para o cliente e [references/spt-source/](../../../../references/spt-source/) para o servidor. Toda referência cita `arquivo.cs:linha`.

Este item cobre 5 dos 6 achados críticos do [relatório de auditoria 01](../../docs/relatorio-auditoria-codigo-01.md), agrupados em 3 grupos técnicos independentes (podem ser implementados e testados separadamente):

- **Grupo A** (`AUD-01-01`, `AUD-01-02`) — patches client-side que aplicam a skill do `MainPlayer` a uma entidade diferente da que realmente executou a ação.
- **Grupo B** (`AUD-01-04`) — checagem de nulidade ausente dentro de uma coroutine multi-frame.
- **Grupo C** (`AUD-01-05`, `AUD-01-06`) — condição de corrida server-side entre requisições HTTP concorrentes.

## 1. Estratégia

**Grupo A — `DoorSoundPatch` (`AUD-01-01`):** a investigação nesta spec técnica revelou que a hipótese original do relatório de auditoria (adicionar um parâmetro de "dono" direto ao Prefix de `PlaySound`) **não é viável** — `WorldInteractiveObject.PlaySound(EDoorState state, float volume = 1f)` ([WorldInteractiveObject.cs:1060](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/WorldInteractiveObject.cs#L1060)) não recebe nem tem acesso a qualquer identificador de quem interagiu com a porta. O sinal de "é o jogador local?" existe um nível acima, em `SmoothDoorOpenCoroutine(EDoorState state, bool isLocalInteraction, float speed = 1f)` ([WorldInteractiveObject.cs:1088](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/WorldInteractiveObject.cs#L1088)), que é quem chama `PlaySound` na linha 1100. Estratégia: **dois Prefixes cooperando** — um novo patch em `SmoothDoorOpenCoroutine` captura `isLocalInteraction` por porta (chave = `WorldInteractiveObject.Id`, campo `string` confirmado em [WorldInteractiveObject.cs:468](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/WorldInteractiveObject.cs#L468)) num dicionário estático compartilhado; o `DoorSoundPatch` existente passa a consultar esse dicionário antes de aplicar o bônus, e deixa o som nativo tocar sem modificação (`return true`) quando a interação não é do jogador local ou quando o dicionário não tem entrada para aquela porta (fail-safe: sem dado = comportamento nativo).

**Grupo A — `MovementContextSetSpeedLimitPatch` (`AUD-01-02`):** o mod já tem o padrão correto implementado em outro patch do mesmo grupo de skills — [CanWalkPatch.cs:17-22](../../modded/Plugin/Skills/FirstAid/Patches/CanWalkPatch.cs#L17-L22) injeta `Player ____player` (convenção Harmony `___` + nome exato do campo privado `_player`, confirmado em [MovementContext.cs:157](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L157): `protected Player _player;`) e faz `if (!____player.IsYourPlayer) return;` antes de tocar em skill. Estratégia: replicar exatamente esse padrão em `MovementContextSetSpeedLimitPatch`, e trocar o `GameUtils.GetSkillManager()!` (null-forgiving sem checagem) por uma variável local checada com `return true` (executa o método original do EFT — [MovementContext.cs:1702-1729](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L1702-L1729) — em vez de quebrar) quando nula.

**Grupo B — `UpdateWeaponsPatch` (`AUD-01-04`):** ambas as coroutines (`UpdateUsecWeapons`/`UpdateEasternWeapons`) checam `SkillManager is null` uma única vez antes do loop, mas fazem `yield return null` a cada iteração e voltam a acessar `GameUtils.GetSkillManager()!`/`GameUtils.GetProfile(side)!` sem checagem nova. Estratégia: mover a resolução de `SkillManager`/`Profile` para **dentro** do corpo do loop, checada a cada iteração, com `yield break` imediato se qualquer um for nulo (raid terminou no meio do processo). **Fora de escopo** (conforme a spec funcional): não tocar na lógica de mutação de `weapon.Template.Ergonomics`/`RecoilForce*` (`AUD-01-08`) nem no gatilho `OnScreenChanged`/`.Clear()` incondicional (`AUD-01-09`) — só a timing do null-check.

**Grupo C — `CultistProductionPatch`/`GeneratePlayerScavPatch` (`AUD-01-05`, `AUD-01-06`):** ambos usam um campo `static` para passar dado do Prefix de um método para o Postfix do mesmo método (padrão que quebra sob concorrência real de requisições HTTP). Estratégia: usar o parâmetro `out`/`ref` `__state` do Harmony (mecanismo nativo para isso, documentado em `csharp-mod-best-practices` §3), que o Harmony aloca **por chamada** (via pilha/closure interna), eliminando o estado compartilhado entre requisições concorrentes sem mudar o efeito observável em nenhum cenário single-request.

## 2. Pontos de patch

| Alvo (Assembly / servidor) | Tipo | Motivo |
|---|---|---|
| [`WorldInteractiveObject.SmoothDoorOpenCoroutine`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/WorldInteractiveObject.cs#L1088) | Prefix (**novo**) | Capturar `isLocalInteraction` por porta antes que `PlaySound` seja chamado dentro da coroutine |
| [`WorldInteractiveObject.PlaySound`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/WorldInteractiveObject.cs#L1060) | Prefix (existente, **modificar**) | Só aplicar o bônus de Silent Ops quando a interação capturada acima for local |
| [`MovementContext.method_0`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L1702) | Prefix (existente, **modificar**) | Filtrar por `____player.IsYourPlayer` e checar null antes de usar a skill |
| `MenuTaskBar.OnScreenChanged` → coroutines `UpdateUsecWeapons`/`UpdateEasternWeapons` (mod, não EFT) | Prefix + coroutine (existente, **modificar**) | Re-checar `SkillManager`/`Profile` a cada iteração da coroutine, não só uma vez |
| [`CircleOfCultistService.StartSacrifice`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/CircleOfCultistService.cs#L58) | Prefix (existente, **modificar** — troca mecanismo) | Substituir campo estático por `__state` |
| `BotGenerator.GeneratePlayerScav` ([BotGenerator.cs:51](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Generators/BotGenerator.cs#L51)) | Prefix (existente, **modificar** — troca mecanismo) | Substituir campo estático por `__state` |

## 3. Novas propriedades F12 (BepInEx)

N/A — nenhuma das correções introduz `ConfigEntry` nova; todas usam configuração já existente (`SkillsExtendedPlugin.SkillData.*`).

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Plugin/Skills/SilentOps/Patches/DoorInteractionTrackerPatch.cs` | CRIAR | Novo patch em `SmoothDoorOpenCoroutine` que grava `isLocalInteraction` por porta |
| `modded/Plugin/Skills/SilentOps/Patches/DoorSoundPatch.cs` | MODIFICAR | Consultar o dicionário do patch acima antes de aplicar o bônus; `return true` (som nativo) quando não for interação local |
| `modded/Plugin/Skills/Strength/Patches/MovementContextSetSpeedLimitPatch.cs` | MODIFICAR | Injetar `Player ____player`, guardar `IsYourPlayer`, checar null do `SkillManager` |
| `modded/Plugin/Skills/WeaponSkills/Patches/UpdateWeaponsPatch.cs` | MODIFICAR | Re-checar `SkillManager`/`Profile` a cada iteração de `UpdateUsecWeapons`/`UpdateEasternWeapons` |
| `modded/Server/Patches/CultistProductionPatch.cs` | MODIFICAR | Substituir `PmcProfileId` estático por `__state` do Harmony |
| `modded/Server/Patches/GeneratePlayerScavPatch.cs` | MODIFICAR | Substituir `_generateAsCultist` estático por `__state` do Harmony |

## 5. Stubs de código

### Grupo A.1 — `DoorInteractionTrackerPatch.cs` (novo) + `DoorSoundPatch.cs` (modificado)

```csharp
// modded/Plugin/Skills/SilentOps/Patches/DoorInteractionTrackerPatch.cs
using System.Collections.Generic;
using System.Reflection;
using EFT.Interactive;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SkillsExtended.Skills.SilentOps.Patches;

// Grava, por porta, se a interação em andamento é do jogador local. WorldInteractiveObject.PlaySound
// (patcheado por DoorSoundPatch) não recebe esse dado diretamente — só SmoothDoorOpenCoroutine tem
// `isLocalInteraction`, um nível acima na pilha de chamada.
internal class DoorInteractionTrackerPatch : ModulePatch
{
    // chave = WorldInteractiveObject.Id (string, confirmado em Assembly-CSharp/EFT.Interactive/WorldInteractiveObject.cs:468)
    internal static readonly Dictionary<string, bool> LastInteractionIsLocal = new();

    protected override MethodBase GetTargetMethod()
    {
        // ref: Assembly-CSharp/EFT.Interactive/WorldInteractiveObject.cs:1088
        // SmoothDoorOpenCoroutine(EDoorState state, bool isLocalInteraction, float speed = 1f)
        return AccessTools.Method(typeof(WorldInteractiveObject), nameof(WorldInteractiveObject.SmoothDoorOpenCoroutine));
    }

    [PatchPrefix]
    private static void Prefix(WorldInteractiveObject __instance, bool isLocalInteraction)
    {
        // O Prefix roda de forma síncrona na CONSTRUÇÃO do IEnumerator (quando SmoothDoorOpenCoroutine
        // é chamado), antes de qualquer yield — o valor fica gravado antes da coroutine começar a rodar.
        LastInteractionIsLocal[__instance.Id] = isLocalInteraction;
    }
}
```

```csharp
// modded/Plugin/Skills/SilentOps/Patches/DoorSoundPatch.cs (diff — só o Prefix muda)
[PatchPrefix]
private static bool Prefix(WorldInteractiveObject __instance, EDoorState state)
{
    // Door sounds don't exist on the headless
    if (!SkillsExtendedPlugin.SkillData.SilentOps.Enabled || SkillsExtendedInfo.IsFikaHeadless)
    {
        return true;
    }

    // Sem dado = fail-safe pro comportamento nativo. Isso cobre tanto "não é interação local"
    // quanto "SmoothDoorOpenCoroutine não rodou antes disso por algum caminho não mapeado" (ver §7).
    if (!DoorInteractionTrackerPatch.LastInteractionIsLocal.TryGetValue(__instance.Id, out var isLocal) || !isLocal)
    {
        return true;
    }

    var skillManager = GameUtils.GetSkillManager();
    if (skillManager == null)
    {
        return true;
    }

    if (__instance.OpenSound.Length != 0 && state == EDoorState.Open)
    {
        PlayDoorOpenSound(__instance, skillManager);
    }

    if (__instance.SqueakSound.Length != 0)
    {
        PlayDoorSqueakSound(__instance, skillManager);
    }

    return false;
}

// PlayDoorOpenSound/PlayDoorSqueakSound passam a receber `skillManager` como parâmetro
// em vez de ler a property estática `SkillManager` (que reavalia GameUtils.GetSkillManager()
// sem garantia de não-nulidade) — evita reintroduzir o mesmo problema um nível abaixo.
```

### Grupo A.2 — `MovementContextSetSpeedLimitPatch.cs` (modificado)

```csharp
// modded/Plugin/Skills/Strength/Patches/MovementContextSetSpeedLimitPatch.cs (diff)
[PatchPrefix]
public static bool Prefix(MovementContext __instance, Player ____player)
    // ____player = campo privado `_player` de MovementContext (Assembly-CSharp/EFT/MovementContext.cs:157),
    // mesma convenção já usada em modded/Plugin/Skills/FirstAid/Patches/CanWalkPatch.cs:18-22.
{
    var skillData = SkillsExtendedPlugin.SkillData;
    if (!skillData.Strength.Enabled)
    {
        return true;
    }

    if (____player == null || !____player.IsYourPlayer)
    {
        return true; // não é o jogador local — deixa o método nativo (MovementContext.method_0) rodar
    }

    var skillManager = GameUtils.GetSkillManager();
    if (skillManager == null)
    {
        return true;
    }

    var skillMgrExt = skillManager.SkillManagerExtended;

    // ... resto do corpo idêntico ao atual, usando `skillMgrExt` em vez de
    // `GameUtils.GetSkillManager()!.SkillManagerExtended` ...
}
```

### Grupo B — `UpdateWeaponsPatch.cs` (modificado, só `UpdateUsecWeapons` mostrado — `UpdateEasternWeapons` espelha)

```csharp
// modded/Plugin/Skills/WeaponSkills/Patches/UpdateWeaponsPatch.cs (diff em UpdateUsecWeapons)
private static IEnumerator UpdateUsecWeapons()
{
    var profile = GameUtils.GetProfile(GameUtils.IsScav() ? EPlayerSide.Savage : EPlayerSide.Usec);
    if (profile == null)
    {
        yield break;
    }

    var natoWeapons = SkillsExtendedPlugin.SkillData.NatoWeapons;
    var weapons = profile.Inventory.AllRealPlayerItems
        .Where(x => natoWeapons.Weapons.Contains(x.TemplateId));

    foreach (var item in weapons)
    {
        if (item is not Weapon weapon) continue;

        // Re-checa a cada iteração — a coroutine atravessa vários frames (yield return no fim do
        // loop) e o jogador pode sair de raid no meio do processo (AUD-01-04).
        var skillManager = GameUtils.GetSkillManager();
        if (skillManager == null)
        {
            yield break;
        }

        if (!UsecOriginalWeaponValues.ContainsKey(weapon.TemplateId))
        {
            var origVals = new OrigWeaponValues
            {
                ergo = weapon.Template.Ergonomics,
                weaponUp = weapon.Template.RecoilForceUp,
                weaponBack = weapon.Template.RecoilForceBack
            };
            UsecOriginalWeaponValues.Add(item.TemplateId, origVals);
        }

        if (UsecWeaponInstanceIds.ContainsKey(item.Id))
        {
            if (UsecWeaponInstanceIds[item.Id] == skillManager.UsecArsystems.Level)
            {
                continue;
            }
            UsecWeaponInstanceIds.Remove(item.Id);
        }

        var skillMgrExt = skillManager.SkillManagerExtended;

        // Mutação de weapon.Template.* preservada EXATAMENTE como está hoje — fora de escopo
        // deste item (AUD-01-08, item de backlog separado).
        weapon.Template.Ergonomics = UsecOriginalWeaponValues[item.TemplateId].ergo * (1 + skillMgrExt.UsecArSystemsErgoBuff);
        weapon.Template.RecoilForceUp = UsecOriginalWeaponValues[item.TemplateId].weaponUp * (1 - skillMgrExt.UsecArSystemsRecoilBuff);
        weapon.Template.RecoilForceBack = UsecOriginalWeaponValues[item.TemplateId].weaponBack * (1 - skillMgrExt.UsecArSystemsRecoilBuff);

        UsecWeaponInstanceIds.Add(item.Id, skillManager.UsecArsystems.Level);

        yield return null;
    }
}
```

### Grupo C.1 — `CultistProductionPatch.cs` (modificado — versão final, resolvida na review 01 / `PA-01-02`)

`GetCircleCraftingInfo` não recebe `sessionId`/`pmcData` (Harmony `__state` não atravessa métodos diferentes), então a classe `CultistProductionPatch` (patch em `GetCircleCraftingInfo`) **deixa de existir**. A lógica de desconto move inteira para o Postfix de `StartSacrifice`, que já recebe `pmcData` nativamente — usando o campo `SptIsCultistCircle` do modelo de produção do hideout, confirmado em `references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/BotBase.cs:708` (`Hideout.Production: Dictionary<MongoId, Production?>`) e `:822-823` (`Production.SptIsCultistCircle: bool?`), com o tempo em `Production.ProductionTime` (`:776`, `double?`, segundos).

```csharp
// modded/Server/Patches/CultistProductionPatch.cs (versão final — substitui a classe inteira)
public class StartSacrificePatch : AbstractPatch
{
    private static readonly ConfigController ConfigController = ServiceLocator.ServiceProvider.GetRequiredService<ConfigController>();
    private static readonly SkillUtil SkillUtil = ServiceLocator.ServiceProvider.GetRequiredService<SkillUtil>();

    protected override MethodBase? GetTargetMethod()
    {
        // ref: spt-source/Libraries/SPTarkov.Server.Core/Services/CircleOfCultistService.cs:58
        // StartSacrifice(MongoId sessionId, PmcData pmcData, HideoutCircleOfCultistProductionStartRequestData request)
        return AccessTools.Method(typeof(CircleOfCultistService), nameof(CircleOfCultistService.StartSacrifice));
    }

    [PatchPostfix]
    public static void Postfix(MongoId sessionId, PmcData pmcData, ItemEventRouterResponse __result)
    {
        if (pmcData.Hideout?.Production == null) return;

        // ref: spt-source/.../BotBase.cs:708 (Hideout.Production: Dictionary<MongoId, Production?>)
        // ref: spt-source/.../BotBase.cs:822-823 (Production.SptIsCultistCircle: bool?)
        var cultistProduction = pmcData.Hideout.Production.Values
            .FirstOrDefault(p => p?.SptIsCultistCircle == true);
        if (cultistProduction?.ProductionTime is null) return;

        if (!SkillUtil.TryGetSkillLevel(sessionId, SkillTypes.Shadowconnections, out var skillLevel))
        {
            return;
        }

        var timeBonusPerLevel = ConfigController.SkillsConfig.ShadowConnections.CultistCircleReturnTimeReduction;
        var buff = Math.Clamp(1f - timeBonusPerLevel * skillLevel, 0f, 1f);

        // ref: spt-source/.../BotBase.cs:776 (Production.ProductionTime: double?, segundos)
        cultistProduction.ProductionTime *= buff;
    }
}
// Sem estado estático compartilhado entre requisições — sessionId/pmcData vêm direto dos
// parâmetros do Postfix da PRÓPRIA invocação, isolados por chamada pelo próprio Harmony/CLR.
```

### Grupo C.2 — `GeneratePlayerScavPatch.cs` (modificado — `__state`)

```csharp
// modded/Server/Patches/GeneratePlayerScavPatch.cs (diff)
public class GeneratePlayerScavPatch : AbstractPatch
{
    private static readonly DatabaseService DatabaseService = ServiceLocator.ServiceProvider.GetRequiredService<DatabaseService>();
    private static readonly ConfigController ConfigController = ServiceLocator.ServiceProvider.GetRequiredService<ConfigController>();
    private static readonly RandomUtil RandomUtil = ServiceLocator.ServiceProvider.GetRequiredService<RandomUtil>();
    private static readonly SkillUtil SkillUtil = ServiceLocator.ServiceProvider.GetRequiredService<SkillUtil>();
    private static readonly ISptLogger<SkillsExtendedPatch> Logger = ServiceLocator.ServiceProvider.GetRequiredService<ISptLogger<SkillsExtendedPatch>>();

    // Campo estático `_generateAsCultist` removido.

    protected override MethodBase? GetTargetMethod()
    {
        // ref: spt-source/Libraries/SPTarkov.Server.Core/Generators/BotGenerator.cs:51
        // GeneratePlayerScav(MongoId sessionId, string role, string difficulty, BotType botTemplate, PmcData profile)
        return AccessTools.Method(typeof(BotGenerator), "GeneratePlayerScav");
    }

    [PatchPrefix]
    public static void Prefix(MongoId sessionId, ref string role, out bool __state)
    {
        __state = false;

        if (!ConfigController.SkillsConfig.ShadowConnections.Enabled)
        {
            return;
        }

        if (!SkillUtil.TryGetSkillLevel(sessionId, SkillTypes.Shadowconnections, out var level))
        {
            return;
        }

        var chanceConfig = ConfigController.SkillsConfig.ShadowConnections.ScavGenerateAsCultistChance * level;

        __state = RandomUtil.GetChance100(chanceConfig);
        if (!__state)
        {
            return;
        }

        Logger.Info("[Skills Extended] Replacing scav as cultist");
        role = "sectantWarrior";
    }

    [PatchPostfix]
    public static void Postfix(PmcData __result, bool __state)
    {
        if (!ConfigController.SkillsConfig.ShadowConnections.Enabled)
        {
            return;
        }

        if (!__state)
        {
            return;
        }

        if (!DatabaseService.GetBots().Types.TryGetValue("sectantwarrior", out var bot))
        {
            Console.WriteLine("[Skills Extended] Failed to find sectantWarrior");
            return;
        }

        SetAppearance(__result, bot!);
        SetHealth(__result, bot!);
        // Sem reset manual de campo estático — __state morre com a chamada.
    }

    // SetAppearance / SetHealth permanecem idênticos.
}
```

## 6. Fluxo de dados

**Grupo A.1 (porta):**
```
[A] Player/bot interage com porta → EFT chama WorldInteractiveObject.SmoothDoorOpenCoroutine(state, isLocalInteraction, speed)
    (WorldInteractiveObject.cs:1088; isLocalInteraction=true só na chamada de WorldInteractiveObject.cs:1005 — interação direta do jogador local;
     false nas chamadas de :932/:941 — replicação de rede/outros atores)
  → [B] DoorInteractionTrackerPatch.Prefix grava LastInteractionIsLocal[door.Id] = isLocalInteraction
  → [C] a coroutine chama PlaySound(_interaction.ResultState, volume) (WorldInteractiveObject.cs:1100)
  → [D] DoorSoundPatch.Prefix consulta LastInteractionIsLocal[door.Id]; se true → aplica bônus de Silent Ops do MainPlayer;
        se false/ausente → return true (som nativo, sem bônus)
```

**Grupo A.2 (movimento):**
```
[A] Qualquer MovementContext entra/sai de obstáculo (OnEnterObstacle/OnExitObstacle, MovementContext.cs:1690/1696)
  → [B] chama method_0() (MovementContext.cs:1702) → MovementContextSetSpeedLimitPatch.Prefix intercepta
  → [C] Prefix lê ____player (campo _player injetado); se não for IsYourPlayer → return true (roda o cálculo nativo, sem skill)
  → [D] se for o jogador local → aplica StrengthBushSpeedIncBuff do próprio jogador (correto, pois ____player == MainPlayer nesse caso)
```

**Grupo C (servidor):**
```
[A] Cliente envia requisição HTTP (StartSacrifice / GeneratePlayerScav)
  → [B] Harmony Prefix roda no thread do pool ASP.NET que atende ESSA requisição, grava resultado em __state (local à invocação, não compartilhado)
  → [C] método original do SPT executa
  → [D] Harmony Postfix da MESMA invocação recebe o mesmo __state de volta — nenhuma outra requisição concorrente pode sobrescrevê-lo
```

## 7. Riscos e dependências

- **`DoorInteractionTrackerPatch` depende de `SmoothDoorOpenCoroutine` ser o único caminho que leva a `PlaySound`.** Só foi confirmado como único chamador **dentro de `WorldInteractiveObject.cs`** — não foi feita uma busca exaustiva em todo o Assembly por subclasses que sobrescrevam `PlaySound` ou o chamem de outro lugar. **TODO confirmar durante `/code-mod`:** grep por `.PlaySound(` em todo `references/eft-decompiled/Assembly-CSharp/` e por classes que herdam de `WorldInteractiveObject` e sobrescrevem `PlaySound`/`SmoothDoorOpenCoroutine`. Se existir outro caminho, o fail-safe (`return true` quando não há entrada no dicionário) já cobre esse caso com segurança — apenas resulta em "sem bônus aplicado" em vez de comportamento incorreto.
- **`CultistProductionPatch` (classe) foi reformulada, não só ajustada** — a mudança para `__state` não funciona entre métodos diferentes (`StartSacrifice` e `GetCircleCraftingInfo` eram dois patches distintos); resolvido na review 01 (`PA-01-02`) movendo a lógica de desconto para o Postfix de `StartSacrificePatch`, que tem acesso nativo a `pmcData.Hideout.Production` (ver §5, Grupo C.1 — versão final).
- **Compatibilidade com outros mods:** nenhuma das mudanças altera assinatura, nome ou visibilidade de método/classe pública (confirmado: `PmcProfileId` era `internal`, `_generateAsCultist` era `private` — ver histórico da spec funcional). `LockPickingHelpers.DoorAttempts` (público, mencionado no relatório de auditoria) não é tocado por este item.
- **Ordem de patches:** `DoorInteractionTrackerPatch` precisa estar registrado e habilitado antes que `DoorSoundPatch` rode pela primeira vez — como ambos são registrados no `Awake()` do plugin na mesma leva de `new XPatch().Enable()`, a ordem de registro não importa (o dicionário só precisa existir antes do primeiro `PlaySound`, que só acontece depois que o jogo já está em raid, muito depois do `Awake`).

## 8. Checklist de implementação

- [x] Criar `DoorInteractionTrackerPatch.cs` — registro automático via `PatchManager` (auto-discovery de `ModulePatch`, confirmado em `SkillsExtendedPlugin.cs:59-60`), sem necessidade de registro manual.
- [x] Modificar `DoorSoundPatch.cs`: consulta `DoorInteractionTrackerPatch.LastInteractionIsLocal`, checagem de null em `GameUtils.GetSkillManager()`, `skillManager` passado como parâmetro pros métodos privados.
- [ ] Confirmar (grep) se há outro caminho até `PlaySound` além de `SmoothDoorOpenCoroutine` (§7) — não feito nesta rodada; fail-safe já cobre o caso caso exista.
- [x] Modificar `MovementContextSetSpeedLimitPatch.cs`: `Player ____player` injetado, guard `IsYourPlayer`, null-check do `SkillManager`.
- [x] Modificar `UpdateWeaponsPatch.cs`: `SkillManager`/`Profile` resolvidos dentro do loop em `UpdateUsecWeapons` e `UpdateEasternWeapons`, com `yield break` se nulo.
- [x] ✅ Resolvido na review 01 (`PA-01-02`) e implementado: `CultistProductionPatch.cs` reescrito conforme §5 (versão final) — desconto aplicado no Postfix de `StartSacrifice` via `Production.SptIsCultistCircle`/`ProductionTime`.
- [ ] Confirmar se `Switch`/`LootableContainer`/`DoorSwitch`/`BufferGateSwitcher` chamam `base.SmoothDoorOpenCoroutine` (review 01, `PA-01-01`) — não feito nesta rodada; fail-safe já cobre o caso caso algum não chame. Registrado como pendência 🟡 pro `/code-review`.
- [x] Modificar `GeneratePlayerScavPatch.cs`: `_generateAsCultist` estático substituído por `__state`.
- [x] Compilar os dois projetos (`Plugin/Plugin.csproj` e `Server/Server.csproj`) — ambos compilaram com 0 erros de código (só o quirk de path do PostBuild ao usar `-o` externo, não afeta o `/compile-mod` real).
- [ ] Validar em raid — pendente de teste manual do usuário.
- [ ] Validar em Fika coop — pendente de teste manual do usuário (requer ambiente coop disponível).

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | N/A | Nenhuma das 5 correções introduz estado raid-scoped novo que precise de teardown; `DoorInteractionTrackerPatch.LastInteractionIsLocal` é um dicionário por-porta que só cresce até o número total de portas do jogo (todos os mapas, ao longo da vida do processo — não é limpo por raid, mas o total é finito e pequeno, mesmo padrão de risco já aceito para `LockPickingHelpers.DoorAttempts` no relatório de auditoria 01 — correção de texto, `PA-01-03`). |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | É o núcleo deste item — Grupo A adiciona exatamente esse filtro em `DoorSoundPatch` (§5, via `DoorInteractionTrackerPatch`) e `MovementContextSetSpeedLimitPatch` (§5, via `____player.IsYourPlayer`). |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | ✅ | `SmoothDoorOpenCoroutine` **é** `virtual` ([WorldInteractiveObject.cs:1088](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/WorldInteractiveObject.cs#L1088)) — corrigido na review 01 (`PA-01-01`, achado Categoria C no checklist original). Auditados os 6 overrides existentes; `Door.cs:394` e `KeycardDoor.cs:74` confirmados chamando `base.SmoothDoorOpenCoroutine` (patch dispara normalmente para os dois tipos mais comuns). `Switch`/`LootableContainer`/`DoorSwitch`/`BufferGateSwitcher` não verificados (item pendente no checklist §8) — mitigado pelo fail-safe de `DoorSoundPatch` (sem entrada no dicionário = som nativo, não quebra). Os outros 5 alvos não são virtuais nem ofuscados por número. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | Nenhuma correção deste item passa a escrever campo interno que antes não era escrito — os 5 fixes só adicionam guards (`return true`/`yield break`) que restauram o caminho **nativo** do EFT/SPT quando a condição de dono/nulidade não se sustenta, em vez de continuar executando lógica substituta. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | Critério "Estado entre raids" da spec funcional (§ Critérios de aceite) cobre especificamente o Grupo B (`UpdateWeaponsPatch`) — o `yield break` a cada iteração garante que a coroutine não sobrevive à saída de raid por qualquer meio. Grupos A e C não têm estado que atravesse raids (A é por-frame/por-request; C é por-requisição HTTP). |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | N/A | Nenhuma `ConfigEntry` nova (§3). |
| 7 | Re-invocação de método patcheado tem reentry-guard — AP-07 | N/A | Nenhum dos patches modificados invoca o próprio método patcheado (via `MethodInfo.Invoke` ou forwarding) — todos são Prefix/Postfix simples sem recursão. |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca — AP-08 | ✅ | `DoorInteractionTrackerPatch.LastInteractionIsLocal` é escrito imediatamente antes de cada interação de porta (não é um cache de longa duração que possa ficar stale entre trocas de contexto — é sobrescrito a cada nova interação com a mesma porta, chave por `Id`). |
| 9 | Todo patch-point reconfirmado no `.cs` do dump; "não existe" conferido no `types-index.json` — AP-09 | ✅ | Todas as 6 linhas de assinatura citadas em §1/§2/§5 foram lidas diretamente nos arquivos (`WorldInteractiveObject.cs:468,1060,1088`; `MovementContext.cs:157,1702`; `CircleOfCultistService.cs:58`; `BotGenerator.cs:51`) nesta sessão, não inferidas de recon anterior. |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | As skills tocadas (Silent Ops, Strength) já são funcionais no mod hoje (o bug é sobre A QUEM o bônus é aplicado, não sobre a skill estar inerte) — não há introdução de nova skill como alavanca. |
| 11 | Pacote FIKA próprio: envelope, `TryGet*`, `Valid`, etc. — AP-11 | N/A | Nenhuma correção deste item declara ou modifica um pacote `INetSerializable` próprio. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-07 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-07 | Review 01 aplicada — `PA-01-01`/`PA-01-02`/`PA-01-03` aceitos e incorporados (stub do Grupo C.1 reescrito, check 3 da §9 corrigido, imprecisão de texto no check 1 corrigida, item de checklist adicionado) |
