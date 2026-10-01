# 004 — Corrigir achados restantes da auditoria 01 · Spec Técnica

**Mod:** Skills-Extended
**Spec funcional:** [004-corrigir-achados-restantes-auditoria-01-01-spec.md](004-corrigir-achados-restantes-auditoria-01-01-spec.md)
**Criado:** 2026-09-07

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

**Memória consultada:** [mods/Skills-Extended/memory/sessions.md](../../memory/sessions.md) — Sessão 2 (snapshot 2026-09-07). Pendências que afetam este item: `P-1.2` (este próprio item). `P-2.1` (débito de Recoil do item 003) e `P-2.2` (validação pendente do item 003) não afetam este item — arquivos diferentes.

**Docs técnicos lidos (gatilho disparado):** [spt-antipatterns.md](../../../../docs/technical/spt-antipatterns.md) (obrigatório). Nenhum outro doc disparado.

---

## 0. Achado durante a investigação — `AUD-01-20` é mais grave do que a auditoria classificou

A auditoria original classificou `AUD-01-20` como 🟡 Médio, descrito como "alocação evitável" (`.Clone()` descartado). Ao ler `SkillManagerExt.AdjustStimulatorBuff` ([SkillManagerExt.cs:275-287](../../modded/Plugin/Skills/Core/SkillManagerExt.cs#L275-L287)) inteiro, confirmei que o método **muta o objeto recebido por parâmetro e não retorna nada**. Como os dois call-sites (`PersonalBuffPatch.cs:25`, `PersonalBuffStringPatches.cs:25`) chamam esse método passando **um clone descartado logo em seguida** (nunca reatribuído a `__result`/`__instance`), a consequência real não é "alocação desnecessária" — é que **o bônus de duração/chance do Field Medicine sobre estimulantes nunca tem efeito observável**, nem no valor realmente aplicado (`GetPersonalBuffSettings`) nem no tooltip (`GetStringValue`). Tratando isso como o achado mais importante desta rodada (ver §1.5).

## 1. Estratégia

### 1.1 · `AUD-01-16` — Velocidade de golpe de arma branca resolvida pelo dono real

**Achado confirmado:** `ObjectInHandsAnimator.SetMeleeSpeed(float speed)` ([ObjectInHandsAnimator.cs:214-217](../../../../references/eft-decompiled/Assembly-CSharp/ObjectInHandsAnimator.cs#L214-L217)) não recebe nem expõe o `Player` dono. `Player.HandsAnimator` é uma property **pública** ([Player.cs:24721](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L24721)) do tipo `ObjectInHandsAnimator` — e `GameWorld.AllAlivePlayersList` é uma lista pública de todos os jogadores vivos no mundo ([GameWorld.cs:556](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L556)).

**Fix confirmado:** no Prefix, iterar `Singleton<GameWorld>.Instance.AllAlivePlayersList` procurando `p.HandsAnimator == __instance` — o `Player` encontrado é o dono real do golpe. Usar `owner.Skills` (a própria instância de `SkillManager` do dono, sempre pública em qualquer `Player`) em vez de `GameUtils.GetSkillManager()` (sempre MainPlayer) — resolve o achado pela raiz para **qualquer** jogador (local ou peer Fika observado), não só um guard "aplica só se for o MainPlayer". Custo: uma busca linear numa lista tipicamente pequena (jogadores vivos no raid), disparada só por golpe de arma branca — não é hot-path por frame.

### 1.2 · `AUD-01-17` — Nullability escondida na property `SkillManager` do minigame

**Achado confirmado:** `LockPickingGame.cs:30` — `private static SkillManager SkillManager => GameUtils.GetSkillManager();` — o tipo de retorno não deixa claro que pode ser `null` (o projeto tem `<Nullable>disable</Nullable>` no `.csproj`, então anotar `SkillManager?` não muda nada em tempo de compilação — a única correção efetiva é checar antes de usar). Usada sem checagem em `SetSweetSpotRange` (linha 405) e `SetTimeLimit` (linha 423).

**Fix:** checar `null` no início dos dois métodos e sair de forma segura (`return` com um valor padrão razoável) em vez de deixar o `NullReferenceException` propagar.

### 1.3 · `AUD-01-18` — Acesso não-defensivo inconsistente em `ConsoleCommands`

**Achado confirmado:** `DoDamage`/`DoDie`/`DoFracture` ([ConsoleCommands.cs:59-95](../../modded/Plugin/Helpers/ConsoleCommands.cs#L59-L95)) fazem `Singleton<GameWorld>.Instance.MainPlayer` sem checar `Singleton<GameWorld>.Instantiated` — ao contrário de `ResetDoorLocks` (linha 51), no mesmo arquivo, que já faz `if (!Singleton<GameWorld>.Instantiated) return;` corretamente. `GetAllWeaponIDsInInventory` (linha 38-47) itera `foreach (var weapon in weapons)` onde `weapons` vem de uma cadeia `?.` (`GameUtils.GetProfile(side)?.Inventory?.AllRealPlayerItems.Where(...)`) que pode ser `null`.

**Fix:** adicionar `if (!Singleton<GameWorld>.Instantiated) return;` no topo dos 3 métodos de saúde (mesmo padrão de `ResetDoorLocks`); em `GetAllWeaponIDsInInventory`, usar `weapons?.ToList() ?? []` (ou checagem `is null`) antes do `foreach`.

### 1.4 · `AUD-01-19` — Closure alocada a cada tick de `ClampSpeed` durante Prone

**Achado confirmado:** `ProneMoveStatePatch.Prefix` ([ProneMoveStatePatch.cs:42](../../modded/Plugin/Skills/ProneMovement/Patches/ProneMoveStatePatch.cs#L42)) cria `() => player.Skills.ProneAction.Complete(ProneData.XpPerAction)` toda vez que `MovementContext.ClampSpeed` é chamado (contínuo enquanto o jogador local está de bruços) — o guard `if (!player.IsYourPlayer) return true;` (linha 30) garante que `player` é sempre a mesma instância (o jogador local) durante toda a raid.

**Fix:** cachear o delegate num campo estático, recriando só quando a instância de `player` mudar (troca de raid) — evita a alocação por tick sem mudar o comportamento.

### 1.5 · `AUD-01-20` — Bônus de Field Medicine em estimulantes nunca é aplicado (achado reclassificado, ver §0)

**Fix confirmado (raiz do bug):** em `PersonalBuffPatch.PostFix` ([PersonalBuffPatch.cs:25](../../modded/Plugin/Skills/FieldMedicine/Patches/PersonalBuffPatch.cs#L25)), trocar `skills.SkillManagerExtended.AdjustStimulatorBuff((InjectorBuff)__result.Clone());` por `skills.SkillManagerExtended.AdjustStimulatorBuff(__result);` — muta o `__result` de fato retornado pelo método patcheado, em vez de um clone descartado. Mesma troca em `PersonalBuffFullStringPatch.Prefix` ([PersonalBuffStringPatches.cs:25](../../modded/Plugin/Skills/FieldMedicine/Patches/PersonalBuffStringPatches.cs#L25)): `skillManager?.AdjustStimulatorBuff(__instance);`.

**`TODO confirmar` (residual, não bloqueante — mitigado com evidência indireta):** `InjectorBuff` e `BuffSettings.GetPersonalBuffSettings` (mapeada como `BackendConfigSettingsClass+GClass1786` → `EFT.GlobalConfiguration+BuffSettings` na [tabela de deofuscação](../../../../docs/files-from-4.1/consolidated-mappings.txt:1783)) não foram localizados diretamente em `references/eft-decompiled/Assembly-CSharp/` — vivem em `ItemComponent.Types.dll` ([Plugin.csproj:27-29](../../modded/Plugin/Plugin.csproj#L27-L29)), assembly referenciada pelo mod mas não decompilada neste repo. Não foi possível confirmar 100% por leitura estática direta se o objeto retornado/recebido é fresco por uso ou compartilhado — mas encontrei o **caller real** de `GetPersonalBuffSettings` em [ActiveHealthController.cs:2587-2597](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L2587-L2597):
```csharp
ActiveBuffs[i] = new Class2222
{
    OriginalSettings = array[i],
    Settings = GClass3008.GClass3019_0.Stimulator.GetPersonalBuffSettings(Store.BuffsName, i, base.HealthController.SkillManager_0, base.Strength),
    BodyPart = base.BodyPart
};
```
O código do próprio EFT já guarda `OriginalSettings = array[i]` (a referência compartilhada, vinda de `Stimulator.Buffs[...]`) **separadamente** de `Settings = ...GetPersonalBuffSettings(...)` — essa separação só faz sentido se `GetPersonalBuffSettings` retornar um objeto **derivado/computado**, não a mesma referência de `array[i]`. Isso é evidência forte (não uma prova definitiva, já que a implementação interna do método não foi lida) de que mutar o retorno é seguro. Mitigação residual: validar em teste manual como descrito abaixo antes de fechar o achado por completo.

### 1.6 · `AUD-01-21` — Alocação em toda montagem do menu de porta trancada

**Achado confirmado:** `WorldInteractionUtils.AddLockpickingInteraction`/`AddKeyCardInteraction`/`AddInspectInteraction` ([WorldInteractionUtils.cs:22-109](../../modded/Plugin/Skills/LockPicking/WorldInteractionUtils.cs#L22-L109)) alocam `LockPickingInteraction`/`HackTerminalInteraction`/`LockInspectInteraction` + `ActionsTypesClass` a cada chamada de `GetActionsClass.smethod_5` (toda vez que o jogador olha para a porta).

**Risco identificado na parte de "pool por porta" sugerida pela auditoria:** os handlers (`LockPickingInteraction` etc.) guardam o `GamePlayerOwner owner` recebido no construtor. Cachear o `ActionsTypesClass`/handler inteiro **por porta** (ignorando o `owner`) seria inseguro em coop — se dois jogadores diferentes olharem para a mesma porta, o cache reteria o delegate vinculado ao **primeiro** `owner`, aplicando a ação de um jogador em nome de outro (mesma classe de bug sistêmica do resto da auditoria). **Fix ajustado:** cachear só o resultado das checagens caras e sem dependência de `owner` — `LockPickingHelpers.GetLockPicksInInventory().Any()` ([LockPickingHelpers.cs:105-111](../../modded/Plugin/Skills/LockPicking/LockPickingHelpers.cs#L105-L111), que já resolve `Singleton<GameWorld>.Instance.MainPlayer` internamente, independente do `owner` passado) — memoizado por `Time.frameCount`, recalculado no máximo uma vez por quadro não importa quantas portas sejam consultadas naquele quadro. Os objetos `ActionsTypesClass`/handler continuam sendo criados frescos a cada chamada (baratos — só a busca no inventário é cara), preservando o binding correto ao `owner` de cada chamada.

### 1.7 · `AUD-01-22` — Assinatura dormente de `KeyCardDoorActionPatch` desatualizada

**Achado confirmado:** `KeyCardDoorActionPatch.cs:17` ([KeyCardDoorActionPatch.cs](../../modded/Plugin/Skills/LockPicking/Patches/KeyCardDoorActionPatch.cs)) declara `Postfix(ref ActionsReturnClass __result, GamePlayerOwner owner, KeycardDoor door)`, mas o alvo real `GetActionsClass.smethod_9` ([GetActionsClass.cs:1705](../../../../references/eft-decompiled/Assembly-CSharp/GetActionsClass.cs#L1705)) tem assinatura `smethod_9(GamePlayerOwner owner, Item rootItem, TraderControllerClass lootItemOwner, string lootItemId, string lootItemName, IPlayer lootItemLastOwner)` — trata de loot de container/trader, **não** porta com keycard. A classe já tem `[IgnoreAutoPatch]` e o corpo comentado — feature incompleta e inofensiva hoje.

**Decisão (não é código-fix, é decisão de escopo):** manter `[IgnoreAutoPatch]` e a assinatura como está — a spec funcional aceita "permanece desativada com nota clara" como resultado válido. Adicionar só um comentário explícito no arquivo apontando para este documento, para quem for retomar a feature no futuro não precisar redescobrir o problema.

### 1.8 · `AUD-01-23` — `LockPickingGame._onUnlocked` não é zerado em `OnDisable`

**Fix confirmado:** adicionar `_onUnlocked = null;` em `LockPickingGame.OnDisable()` ([LockPickingGame.cs:129-152](../../modded/Plugin/Skills/LockPicking/LockPickingGame.cs#L129-L152)).

### 1.9 · `AUD-01-24`/`AUD-01-25`/`AUD-01-26` — Higiene de `UpdateChecker`

**Fix confirmado**, três mudanças no mesmo arquivo ([UpdateChecker.cs](../../modded/Server/Core/UpdateChecker.cs)):
- `AUD-01-24`: `var httpClient = new HttpClient();` (linha 45) → `using var httpClient = new HttpClient();`.
- `AUD-01-25`: `catch { }` (linhas 82-83) → `catch (Exception ex) { logger.Debug($"UpdateChecker: falha ao verificar atualização — {ex.Message}"); }` (reusa o `logger` já injetado via `ISptLogger<UpdateChecker>`, sem precisar de novo campo).
- `AUD-01-26`: adicionar `httpClient.Timeout = TimeSpan.FromSeconds(5);` logo após a criação do `HttpClient`, antes da chamada `GetFromJsonAsync`.

### 1.10 · `AUD-01-27` — Reflection resolvida repetidamente

**Achado confirmado:** `SkillManagerConstructorPatch.LockSkills` ([SkillManagerConstructorPatch.cs:128-153](../../modded/Plugin/Skills/Core/Patches/SkillManagerConstructorPatch.cs#L128-L153) — arquivo já modificado pelo item 003, mas este método específico não foi tocado lá) chama `AccessTools.Field(typeof(SkillClass), "Locked")` 8 vezes seguidas, uma por skill. `HackingActionHandler.HackTerminalAction` ([HackingActionHandler.cs:17](../../modded/Plugin/Skills/LockPicking/Actions/HackingActionHandler.cs#L17)) resolve `AccessTools.Method(typeof(WorldInteractiveObject), "Unlock")` a cada terminal hackeado com sucesso.

**Fix:** cachear ambos em `private static readonly FieldInfo`/`private static readonly MethodInfo` no topo das respectivas classes, resolvidos uma única vez.

### 1.11 · `AUD-01-28` — Reasserção incondicional de cursor/input a cada quadro

**Achado confirmado:** `LockPickingGame.Update()` ([LockPickingGame.cs:171-211](../../modded/Plugin/Skills/LockPicking/LockPickingGame.cs#L171-L211)) escreve `CursorSettings.SetCursor(ECursorType.Idle)`, `Cursor.lockState`, e os 2 flags de `GamePlayerOwner.IgnoreInput*` todo quadro enquanto o minigame está aberto, mesmo sem mudar.

**Decisão (mantendo a ressalva da própria auditoria):** a auditoria já registra que não é possível confirmar por leitura estática se algum sistema nativo do EFT reimpõe esses valores por conta própria a cada quadro (padrão comum em overlays de UI) — mover para `OnEnable()` sem validar em runtime arrisca quebrar o cursor/input do minigame silenciosamente. **Fix proposto:** mover a atribuição para `OnEnable()` (que já faz setup equivalente, linha 116-127), mas o checklist de implementação (§8) exige validação manual em jogo antes de fechar este achado — se o cursor "vazar" de volta ao padrão do jogo durante o minigame, reverter e documentar a reasserção contínua como intencional.

## 2. Pontos de patch

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`ObjectInHandsAnimator.cs:214`](../../../../references/eft-decompiled/Assembly-CSharp/ObjectInHandsAnimator.cs#L214) `SetMeleeSpeed` (já patcheado hoje) | Prefix (modificado) | Resolve o dono real via `AllAlivePlayersList`/`HandsAnimator` — `AUD-01-16` |
| `WorldInteractiveObject.MovementContext.ClampSpeed` — já patcheado hoje (`ProneMoveStatePatch`) | Prefix (modificado) | Cacheia o delegate de XP — `AUD-01-19` |
| `BuffSettings.GetPersonalBuffSettings`/`InjectorBuff.GetStringValue` — já patcheados hoje | Postfix/Prefix (modificados) | Aplica o ajuste no objeto real, não num clone descartado — `AUD-01-20` |
| `GetActionsClass.smethod_5` — já patcheado hoje (`DoorActionPatch`) | Sem mudança no ponto de patch | Fix é em `WorldInteractionUtils` (arquivo do mod), não no alvo — `AUD-01-21` |

Os demais achados (`AUD-01-17`, `AUD-01-18`, `AUD-01-22`, `AUD-01-23`, `AUD-01-24`–`26`, `AUD-01-27`, `AUD-01-28`) são edições dentro de classes já existentes do mod, sem novo ponto de patch no Assembly.

## 3. Novas propriedades F12 (BepInEx)

N/A — nenhuma `ConfigEntry` nova.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Plugin/Skills/SilentOps/Patches/MeleeSpeedPatch.cs` | MODIFICAR | Resolve o dono real do golpe via `AllAlivePlayersList` (`AUD-01-16`) |
| `modded/Plugin/Skills/LockPicking/LockPickingGame.cs` | MODIFICAR | Null-check em `SetSweetSpotRange`/`SetTimeLimit` (`AUD-01-17`); `_onUnlocked = null` em `OnDisable` (`AUD-01-23`); reasserção de cursor/input movida para `OnEnable` (`AUD-01-28`) |
| `modded/Plugin/Helpers/ConsoleCommands.cs` | MODIFICAR | Guard `Instantiated` em 3 comandos + null-safety em `GetAllWeaponIDsInInventory` (`AUD-01-18`) |
| `modded/Plugin/Skills/ProneMovement/Patches/ProneMoveStatePatch.cs` | MODIFICAR | Cacheia o delegate de XP em vez de recriar por tick (`AUD-01-19`) |
| `modded/Plugin/Skills/FieldMedicine/Patches/PersonalBuffPatch.cs` | MODIFICAR | Remove o `.Clone()` — aplica o ajuste no `__result` real (`AUD-01-20`) |
| `modded/Plugin/Skills/FieldMedicine/Patches/PersonalBuffStringPatches.cs` | MODIFICAR | Remove o `.Clone()` — aplica o ajuste no `__instance` real (`AUD-01-20`) |
| `modded/Plugin/Skills/LockPicking/WorldInteractionUtils.cs` | MODIFICAR | Memoiza `GetLockPicksInInventory().Any()` por quadro; handlers continuam frescos por chamada (`AUD-01-21`) |
| `modded/Plugin/Skills/LockPicking/Patches/KeyCardDoorActionPatch.cs` | MODIFICAR | Comentário apontando a assinatura desatualizada para este documento (`AUD-01-22`) |
| `modded/Server/Core/UpdateChecker.cs` | MODIFICAR | `using var httpClient`; `catch` com log; `Timeout` de 5s (`AUD-01-24`, `AUD-01-25`, `AUD-01-26`) |
| `modded/Plugin/Skills/Core/Patches/SkillManagerConstructorPatch.cs` | MODIFICAR | Cacheia `FieldInfo` de `"Locked"` em `LockSkills` (`AUD-01-27`) |
| `modded/Plugin/Skills/LockPicking/Actions/HackingActionHandler.cs` | MODIFICAR | Cacheia `MethodInfo` de `"Unlock"` (`AUD-01-27`) |

## 5. Stubs de código

### 5.1 · `MeleeSpeedPatch.cs` (reescrito) — `AUD-01-16`

```csharp
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SkillsExtended.Helpers;
using SkillsExtended.Skills.Core;
using SPT.Reflection.Patching;

namespace SkillsExtended.Skills.SilentOps.Patches;

public class MeleeSpeedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        // ref: ObjectInHandsAnimator.cs:214 (SetMeleeSpeed)
        return AccessTools.Method(typeof(ObjectInHandsAnimator), nameof(ObjectInHandsAnimator.SetMeleeSpeed));
    }

    [PatchPrefix]
    private static void Prefix(ObjectInHandsAnimator __instance, ref float speed)
    {
        if (!SkillsExtendedPlugin.SkillData.SilentOps.Enabled)
        {
            return;
        }

        if (!Singleton<GameWorld>.Instantiated)
        {
            return;
        }

        // ref: AUD-01-16 — Player.HandsAnimator (EFT/Player.cs:24721) não tem referência inversa embutida
        // no ObjectInHandsAnimator, então resolvemos o dono varrendo GameWorld.AllAlivePlayersList
        // (EFT/GameWorld.cs:556) em vez de assumir sempre o MainPlayer.
        Player owner = null;
        var players = Singleton<GameWorld>.Instance.AllAlivePlayersList;
        for (var i = 0; i < players.Count; i++)
        {
            if (players[i].HandsAnimator == __instance)
            {
                owner = players[i];
                break;
            }
        }

        var skillManager = owner?.Skills;
        if (skillManager?.SkillManagerExtended == null)
        {
            return;
        }

        speed *= 1 + skillManager.SkillManagerExtended.SilentOpsIncMeleeSpeedBuff;
    }
}
```

### 5.2 · `LockPickingGame.cs` (diffs) — `AUD-01-17`, `AUD-01-23`, `AUD-01-28`

```csharp
private void SetSweetSpotRange(int doorLevel)
{
    // ref: AUD-01-17 — SkillManager é [CanBeNull] por contrato de GameUtils.GetSkillManager();
    // a property escondia isso atrás de um tipo não-anotável (Nullable desabilitado no .csproj).
    var skillManager = SkillManager;
    if (skillManager == null)
    {
        return;
    }

    var skillMod = 1 + skillManager.SkillManagerExtended.LockPickingForgiveness;
    var doorMod = Mathf.Clamp(doorLevel / 35f, 0.05f, 1.5f);

    var configVal = SkillsExtendedPlugin.SkillData.LockPicking.SweetSpotRangeBase;

    _sweetSpotRange = Mathf.Clamp((configVal - doorMod) * skillMod, 0f, 20f);
}

private void SetTimeLimit(int doorLevel)
{
    // ref: AUD-01-17 — mesmo guard.
    var skillManager = SkillManager;
    if (skillManager == null)
    {
        return;
    }

    var skillMod = 1 + skillManager.SkillManagerExtended.LockPickingTimeBuff;
    var doorMod = Mathf.Clamp(doorLevel / 50f, 0.05f, 1f);

    var configVal = SkillsExtendedPlugin.SkillData.LockPicking.PickStrengthBase;

    var originalLimit = Mathf.Clamp((configVal - doorMod) * skillMod, 1f, 20f);

    _wiggleTimeLimit = MathUtils.RandomizePercentage(originalLimit, 0.10f);
}

public void OnEnable()
{
    _disabled = false;

    // ref: AUD-01-28 — reasserção de cursor/input movida de Update() (rodava todo quadro sem necessidade
    // aparente) pra cá, que já faz setup equivalente. Validar em jogo antes de fechar (checklist §8) —
    // se algum sistema do EFT reimpuser esses valores por conta própria, reverter e documentar como
    // reasserção intencional em Update().
    CursorSettings.SetCursor(ECursorType.Idle);
    Cursor.lockState = CursorLockMode.None;

    if (GamePlayerOwner.MyPlayer is not null)
    {
        GamePlayerOwner.IgnoreInputWithKeepResetLook = true;
        GamePlayerOwner.IgnoreInputInNPCDialog = true;
    }

    if (Player is null || !Player.IsYourPlayer)
    {
        return;
    }

    Player.MovementContext.ToggleBlockInputPlayerRotation(true);
    Player.CurrentManagedState.ChangePose(-1f);
}

public void OnDisable()
{
    _disabled = true;

    // ref: AUD-01-23 — retinha LockPickActionHandler/GamePlayerOwner/WorldInteractiveObject da raid
    // anterior até a próxima chamada de Activate() sobrescrever.
    _onUnlocked = null;

    if (Player is null || !Player.IsYourPlayer)
    {
        return;
    }

    Player.MovementContext.ToggleBlockInputPlayerRotation(false);
    Player.CurrentManagedState.ChangePose(1f);

    CursorSettings.SetCursor(ECursorType.Invisible);
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;

    if (GamePlayerOwner.MyPlayer is not null)
    {
        GamePlayerOwner.IgnoreInputWithKeepResetLook = false;
        GamePlayerOwner.IgnoreInputInNPCDialog = false;
    }

    Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuDropdown);
}

// Update(): remover as linhas de CursorSettings.SetCursor/Cursor.lockState/IgnoreInput* (agora em OnEnable).
// Resto do corpo (MoveLockPick, rotação do cylinder, etc.) inalterado.
```

### 5.3 · `ConsoleCommands.cs` (diffs) — `AUD-01-18`

```csharp
private static void GetAllWeaponIDsInInventory()
{
    var side = GameUtils.IsScav() ? EPlayerSide.Savage : EPlayerSide.Usec;
    // ref: AUD-01-18 — a cadeia ?. propaga null se Inventory for null; foreach sobre null lançava NRE.
    var weapons = GameUtils.GetProfile(side)?.Inventory?.AllRealPlayerItems.Where(x => x is Weapon) ?? [];

    foreach (var weapon in weapons)
    {
        SkillsExtendedPlugin.Log.LogDebug($"Template ID: {weapon.TemplateId}, locale name: {weapon.LocalizedName()}");
    }
}

private static void DoDamage()
{
    // ref: AUD-01-18 — mesmo guard já usado corretamente em ResetDoorLocks (linha 51) neste arquivo.
    if (!Singleton<GameWorld>.Instantiated) return;

    var player = Singleton<GameWorld>.Instance.MainPlayer;
    var Blunt = new DamageInfoStruct();

    if (!player)
    {
        return;
    }

    player.ActiveHealthController.ApplyDamage(EBodyPart.LeftLeg, 20, Blunt);
}

private static void DoDie()
{
    if (!Singleton<GameWorld>.Instantiated) return;

    var player = Singleton<GameWorld>.Instance.MainPlayer;
    var Blunt = new DamageInfoStruct();

    if (!player)
    {
        return;
    }

    player.ActiveHealthController.ApplyDamage(EBodyPart.Head, int.MaxValue, Blunt);
}

private static void DoFracture()
{
    if (!Singleton<GameWorld>.Instantiated) return;

    var player = Singleton<GameWorld>.Instance.MainPlayer;

    if (!player)
    {
        return;
    }

    player.ActiveHealthController.DoFracture(EBodyPart.LeftLeg);
}
```

### 5.4 · `ProneMoveStatePatch.cs` (diff) — `AUD-01-19`

```csharp
public class ProneMoveStatePatch : ModulePatch
{
    private static FieldInfo _playerField;
    private static ProneMovementData ProneData => SkillsExtendedPlugin.SkillData.ProneMovement;

    // ref: AUD-01-19 — cacheia o delegate; só recria quando o Player mudar (ex.: nova raid).
    private static Player _cachedPlayer;
    private static Action _cachedProneAction;

    protected override MethodBase GetTargetMethod()
    {
        _playerField = AccessTools.Field(typeof(MovementContext), "_player");
        return AccessTools.Method(typeof(MovementContext), nameof(MovementContext.ClampSpeed));
    }

    [PatchPrefix]
    [HarmonyAfter(["RealismMod"])]
    private static bool Prefix(MovementContext __instance, float speed, ref float __result)
    {
        if (!ProneData.Enabled) return true;
        if (__instance.CurrentState is not ProneMoveStateClass) return true;

        var player = (Player)_playerField.GetValue(__instance);

        if (!player.IsYourPlayer) return true;

        var buff = player.Skills.ProneMovementSpeed;
        var bonus = 1f + buff;

        if (!player.Skills.ProneMovement.IsEliteLevel)
        {
            if (!ReferenceEquals(player, _cachedPlayer))
            {
                _cachedPlayer = player;
                _cachedProneAction = () => player.Skills.ProneAction.Complete(ProneData.XpPerAction);
            }

            player.ExecuteSkill(_cachedProneAction);
        }

        __result = Mathf.Clamp(speed * bonus, 0f, __instance.StateSpeedLimit * bonus);

        return false;
    }
}
```

> Requer `using System;` novo no arquivo (para `Action`).

### 5.5 · `PersonalBuffPatch.cs` / `PersonalBuffStringPatches.cs` (diffs) — `AUD-01-20`

```csharp
[PatchPostfix]
public static void PostFix(SkillManager skills, InjectorBuff __result)
{
    if (!SkillsExtendedPlugin.SkillData.FieldMedicine.Enabled || !__result.IsBuff)
    {
        return;
    }

    // ref: AUD-01-20 — antes clonava e descartava o clone; o ajuste nunca chegava a valer pro
    // __result de fato retornado. Ver spec técnica §0/§1.5 — TODO confirmar em teste manual.
    skills.SkillManagerExtended.AdjustStimulatorBuff(__result);
}
```

```csharp
[PatchPrefix]
public static void Prefix(InjectorBuff __instance)
{
    if (!SkillsExtendedPlugin.SkillData.FieldMedicine.Enabled)
    {
        return;
    }

    // ref: AUD-01-20 — mesma correção; mutava um clone descartado antes de GetStringValue() rodar.
    var skillManager = GameUtils.GetSkillManager()?.SkillManagerExtended;
    skillManager?.AdjustStimulatorBuff(__instance);
}
```

### 5.6 · `WorldInteractionUtils.cs` (diff) — `AUD-01-21`

```csharp
// ref: AUD-01-21 — memoiza a checagem de inventário (a parte cara), não os handlers (que precisam
// ficar frescos por chamada, pois guardam o `owner` — cachear por porta ignorando o owner seria
// inseguro em coop: dois jogadores olhando a mesma porta reusariam o delegate do primeiro).
private static int _lockPicksCacheFrame = -1;
private static bool _lockPicksCacheValue;

private static bool HasLockPicksThisFrame()
{
    var frame = Time.frameCount;
    if (frame != _lockPicksCacheFrame)
    {
        _lockPicksCacheFrame = frame;
        _lockPicksCacheValue = LockPickingHelpers.GetLockPicksInInventory().Any();
    }

    return _lockPicksCacheValue;
}
```

> `AddLockpickingInteraction` (linha 50) troca `LockPickingHelpers.GetLockPicksInInventory().Any()` por `HasLockPicksThisFrame()`. Requer `using UnityEngine;` novo no arquivo (para `Time.frameCount`).

### 5.7 · `KeyCardDoorActionPatch.cs` (diff) — `AUD-01-22`

```csharp
[IgnoreAutoPatch]
public class KeyCardDoorActionPatch : ModulePatch
{
    // ref: AUD-01-22 — assinatura de smethod_9 mudou (GetActionsClass.cs:1705, trata loot de
    // container/trader, não porta com keycard). Ver 004-...-02-spec-tech.md §1.7 antes de reativar:
    // é preciso re-resolver o alvo real por assinatura antes de remover [IgnoreAutoPatch].
    protected override MethodBase GetTargetMethod() =>
        typeof(GetActionsClass).GetMethod("smethod_9", BindingFlags.Public | BindingFlags.Static);

    [PatchPostfix]
    private static void Postfix(ref ActionsReturnClass __result, GamePlayerOwner owner, KeycardDoor door)
    {
        /* ...corpo comentado existente, sem mudança... */
    }
}
```

### 5.8 · `UpdateChecker.cs` (diff) — `AUD-01-24`, `AUD-01-25`, `AUD-01-26`

```csharp
private async Task CheckForUpdate()
{
    try
    {
        // ref: AUD-01-24 — using garante Dispose; AUD-01-26 — Timeout evita pendência indefinida
        // se a API do GitHub não responder.
        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("CJ-SPT");
        httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

        var release = await httpClient.GetFromJsonAsync<ReleaseInformation>(
            "https://api.github.com/repos/CJ-SPT/Skills-Extended/releases/latest"
        );
        if (release != null)
        {
            if (release.Version.StartsWith('V'))
            {
                release.Version = release.Version[1..];
            }

            Version latestVersion = new(release.Version);

            var currentVersion = SeModMetadata.Instance.Version;
            Range currentVersionRange = new($"~{currentVersion.Major}.x");

            if (!currentVersionRange.IsSatisfied(latestVersion))
            {
                return;
            }

            if (latestVersion > currentVersion)
            {
                UpdateAvailable = true;
                ReleaseInformation = release;
            }
        }
    }
    // ref: AUD-01-25 — catch vazio sem log; agora registra em nível debug (não assusta usuário, mas
    // fica rastreável se a checagem parar de funcionar).
    catch (Exception ex)
    {
        logger.Debug($"UpdateChecker: falha ao verificar atualização — {ex.Message}");
    }
}
```

> Requer `using System;` (para `Exception`/`TimeSpan`) — os demais usings já existem no arquivo.

### 5.9 · `SkillManagerConstructorPatch.cs` (diff) — `AUD-01-27`

```csharp
// ref: AUD-01-27 — resolvido uma vez em vez de 8x a cada SkillManager construído.
private static readonly FieldInfo LockedField = AccessTools.Field(typeof(SkillClass), "Locked");

private static void LockSkills(SkillManager skillManager)
{
    LockedField.SetValue(skillManager.UsecArsystems, !SkillsExtendedPlugin.SkillData.NatoWeapons.Enabled);
    LockedField.SetValue(skillManager.BearAksystems, !SkillsExtendedPlugin.SkillData.EasternWeapons.Enabled);
    LockedField.SetValue(skillManager.Lockpicking, !SkillsExtendedPlugin.SkillData.LockPicking.Enabled);
    LockedField.SetValue(skillManager.FieldMedicine, !SkillsExtendedPlugin.SkillData.FieldMedicine.Enabled);
    LockedField.SetValue(skillManager.FirstAid, !SkillsExtendedPlugin.SkillData.FirstAid.Enabled);
    LockedField.SetValue(skillManager.ProneMovement, !SkillsExtendedPlugin.SkillData.ProneMovement.Enabled);
    LockedField.SetValue(skillManager.SilentOps, !SkillsExtendedPlugin.SkillData.SilentOps.Enabled);
    LockedField.SetValue(skillManager.Shadowconnections, !SkillsExtendedPlugin.SkillData.ShadowConnections.Enabled);
}
```

### 5.10 · `HackingActionHandler.cs` (diff) — `AUD-01-27`

```csharp
public class HackingActionHandler
{
    // ref: AUD-01-27 — resolvido uma vez em vez de a cada terminal hackeado com sucesso.
    private static readonly MethodInfo UnlockMethod = AccessTools.Method(typeof(WorldInteractiveObject), "Unlock");

    public GamePlayerOwner Owner;
    public WorldInteractiveObject InteractiveObject;

    public void HackTerminalAction(bool unlocked)
    {
        if (unlocked)
        {
            LockPickingHelpers.ApplyLockPickActionXp(InteractiveObject, Owner);
            UnlockMethod.Invoke(InteractiveObject, null);
            return;
        }

        /* ...resto do método sem mudança... */
    }
}
```

## 6. Fluxo de dados

```
[AUD-01-16] Jogador/bot golpeia com arma branca → ObjectInHandsAnimator.SetMeleeSpeed → Prefix
            → varre AllAlivePlayersList procurando p.HandsAnimator == __instance → owner.Skills
            → bônus aplicado com a skill do dono real, não do MainPlayer

[AUD-01-20] Estimulante aplicado → BuffSettings.GetPersonalBuffSettings → Postfix
            → AdjustStimulatorBuff(__result) muta o objeto realmente retornado
            → duração/chance ajustadas de fato refletem no jogo e no tooltip

[AUD-01-21] Jogador olha pra porta trancada → GetActionsClass.smethod_5 → DoorActionPatch.Postfix
            → AddLockpickingInteraction → HasLockPicksThisFrame() (memoizado por Time.frameCount)
            → handler fresco por chamada, vinculado ao owner correto
```

## 7. Riscos e dependências

- **`AUD-01-20` é o achado de maior risco/valor desta rodada** — reclassificado de 🟡 pra um bug funcional real (ver §0). Recomendo priorizar sua validação manual acima dos demais.
- **`AUD-01-27` (`SkillManagerConstructorPatch.cs`)** — mesmo arquivo já modificado pelo item 003 (`AUD-01-10`, `ConditionalWeakTable`). Sem sobreposição de linha (`LockSkills` não foi tocado pelo item 003), mas confirmar que o merge mental das duas mudanças não perde nenhuma delas ao implementar.
- **`AUD-01-16` depende de `Player.HandsAnimator` ser sempre populado** antes do primeiro golpe — se algum fluxo de spawn deixar uma janela onde `HandsAnimator` ainda é `null`/não atribuído, a busca simplesmente não encontra o dono (retorna `null`, guard já cobre) — sem crash, só sem bônus nesse frame raro.
- **`AUD-01-21`** depende de `LockPickingHelpers.GetLockPicksInInventory()` continuar resolvendo sempre `Singleton<GameWorld>.Instance.MainPlayer` internamente (não `owner`) — se isso mudar no futuro, a memoização por-frame (sem chave por jogador) pararia de ser válida em coop. Documentado inline.
- **Nenhum conflito** com patches de outros itens (001/002/003) — arquivos tocados aqui não se sobrepõem linha a linha com nenhum deles.

## 8. Checklist de implementação

- [x] `MeleeSpeedPatch.cs` — resolver dono real via `AllAlivePlayersList` (`AUD-01-16`)
- [x] `LockPickingGame.cs` — null-checks em `SetSweetSpotRange`/`SetTimeLimit` (`AUD-01-17`); `_onUnlocked = null` em `OnDisable` (`AUD-01-23`); mover reasserção de cursor/input pra `OnEnable` (`AUD-01-28`)
- [ ] Validar in-game que o cursor/input do minigame de arrombamento continua funcionando corretamente após mover a reasserção pra `OnEnable` — se não, reverter e documentar como intencional (`AUD-01-28`) — **pendente de teste do usuário**
- [x] `ConsoleCommands.cs` — guard `Instantiated` em `DoDamage`/`DoDie`/`DoFracture`; null-safety em `GetAllWeaponIDsInInventory` (`AUD-01-18`)
- [x] `ProneMoveStatePatch.cs` — cachear o delegate de XP (`AUD-01-19`)
- [x] `PersonalBuffPatch.cs`/`PersonalBuffStringPatches.cs` — remover `.Clone()`, aplicar no objeto real (`AUD-01-20`)
- [ ] Validar in-game (prioridade alta): usar o mesmo estimulante duas vezes com FieldMedicine em níveis diferentes, confirmar que o ajuste não "vaza" de uma leitura pra outra — se vazar, revisitar com clone-e-troca (`AUD-01-20`, ver §1.5 TODO confirmar) — **pendente de teste do usuário**
- [x] `WorldInteractionUtils.cs` — memoizar `GetLockPicksInInventory().Any()` por quadro (`AUD-01-21`)
- [x] `KeyCardDoorActionPatch.cs` — comentário apontando a assinatura desatualizada para este documento (`AUD-01-22`)
- [x] `UpdateChecker.cs` — `using var httpClient`; `catch (Exception ex)` com log; `Timeout` de 5s (`AUD-01-24`, `AUD-01-25`, `AUD-01-26`) — confirmado via reflection na DLL real que `ISptLogger<T>.Debug(string, Exception ex = null)` existe
- [x] `SkillManagerConstructorPatch.cs` — cachear `FieldInfo` de `"Locked"` (`AUD-01-27`)
- [x] `HackingActionHandler.cs` — cachear `MethodInfo` de `"Unlock"` (`AUD-01-27`)
- [ ] Compilar Plugin + Server (Release) com 0 erros antes de considerar o item pronto para `/code-review`

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes (`GameWorld.OnDestroy` + `BaseLocalGame.Stop`) — AP-01 | N/A | Nenhum achado desta rodada introduz estado novo que precise de start/stop hook — `_onUnlocked`/`_cachedPlayer` já são limpos nos pontos corretos (`OnDisable`, mudança de `Player`) sem precisar de um novo hook de raid |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | `AUD-01-16` deixa de depender do MainPlayer (resolve o dono real via `AllAlivePlayersList`); demais patches tocados (`ProneMoveStatePatch`) já tinham o guard `IsYourPlayer` correto, preservado sem mudança |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | N/A | Nenhum novo ponto de patch em método virtual nesta rodada — todos os alvos tocados já eram patcheados antes, sem mudança de alvo |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | `AUD-01-20` passa a mutar `__result`/`__instance` (objetos já entregues pelo próprio método patcheado) em vez de um clone órfão — API canônica, sem invenção |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | `_cachedPlayer`/`_cachedProneAction` (`AUD-01-19`) se auto-corrigem na troca de `Player` (comparação por referência a cada Prefix, não precisam de hook de fim de raid); `_onUnlocked` limpo em `OnDisable` (`AUD-01-23`), que já roda ao sair do minigame independente do motivo |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | N/A | Nenhuma `ConfigEntry` nova ou alterada |
| 7 | Re-invocação de método patcheado tem reentry-guard/`ReversePatch` (sem recursão infinita) — AP-07 | N/A | Nenhum patch desta rodada invoca de volta o método que ele mesmo patcheia |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca (arma/operação/tela) — AP-08 | ✅ | `_cachedPlayer` (`AUD-01-19`) é comparado por referência a cada chamada antes de reusar o cache; `HasLockPicksThisFrame` (`AUD-01-21`) invalida por `Time.frameCount`, nunca serve um valor de um frame anterior |
| 9 | Todo patch-point reconfirmado no `.cs` do dump (não só no recon); "não existe" conferido no `types-index.json`, nunca num grep vazio — AP-09 | ✅ (com 1 exceção mitigada) | Todas as refs a `arquivo.cs:linha` foram lidas nesta sessão diretamente do dump, exceto `InjectorBuff`/`BuffSettings` (`AUD-01-20`) — confirmado que vivem em `ItemComponent.Types.dll` (assembly referenciada pelo mod mas não decompilada neste repo), não uma busca malfeita. Mitigado com evidência indireta forte do caller real (`ActiveHealthController.cs:2587-2597`, ver §1.5) — documentado como `TODO confirmar` residual, não uma lacuna silenciada |
| 10 | Skill EFT usada como lever confirmada **não-inerte** (`SkillsSettings` ≠ `[]` no `globals.json`); se inerte, efeito entregue por patch direto — AP-10 | N/A | Nenhuma skill nativa do EFT usada como lever nesta rodada |
| 11 | Pacote FIKA próprio: envelope de comprimento + só `TryGet*` + flag `Valid`, campos resetados no `Deserialize`, envio só na main thread, registro por instância/evento (nunca `bool`), zero `UnregisterPacket`, airbag com throttle em todo callback — AP-11 | N/A | Nenhum pacote `INetSerializable` tocado nesta rodada |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-07 | Spec técnica criada via `/create-technical-spec` |
