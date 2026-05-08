# 001 — Stamina e Velocidade por Postura · Review Técnica 02

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec técnica revisada:** [001-stamina-e-velocidade-technical-spec.md](001-stamina-e-velocidade-technical-spec.md)
**Spec funcional referência:** [001-stamina-e-velocidade-spec.md](001-stamina-e-velocidade-spec.md)
**Review anterior:** [001-stamina-e-velocidade-technical-review-01.md](001-stamina-e-velocidade-technical-review-01.md)
**Data:** 2026-05-07

> Análise crítica da spec técnica após resolução das PAs da review-01. Cada ponto novo recebe ID `PA-02-MM`. IDs antigos (PA-01-XX) confirmados resolvidos não voltam a ser levantados.
>
> Skills aplicadas: `spt-mod-best-practices` + `csharp-mod-best-practices`.

## Resumo

> 🔴 Bloqueadores: 3 (✅ **3 resolvidos**) · 🟡 Importantes: 3 (✅ **3 resolvidos**) · 🟢 Menores: 2 (✅ **2 resolvidos**) · Total: **8 — todos resolvidos** em 2026-05-07
>
> ✅ **Status:** todos os 8 PAs desta review foram aplicados na spec técnica. Pronto para rodar `/review-technical-spec` novamente (gera `technical-review-03.md`) para validar os fechamentos antes de `/build-item`.

## Reviews anteriores resolvidas

Todas as 16 PAs da review-01 foram resolvidas na spec atual:

- ✅ PA-01-01 resolvido — Drain reescrito como tick manual em `StanceManager.Update()`, postfix em `method_10` removido. Spec §1.1 e §5 (StanceManager.TickStanceStamina).
- ✅ PA-01-02 resolvido — Cause definida: `(Player.ESpeedLimit)9001` via `Plugin.StanceSpeedLimitCauseId`. Spec §1.3 e §5 (Plugin.cs).
- ✅ PA-01-03 resolvido — 3 patches de raid lifecycle criados (`GameWorld.OnGameStarted`, `GameWorld.OnDestroy`, `BaseLocalGame.Stop`). Spec §2 e §5 (RaidLifecyclePatches.cs).
- ✅ PA-01-04 resolvido — Recovery postfix recebe `__instance` e filtra por `__instance.Player_0 == MainPlayer`. Spec §5 (StanceStaminaRecoveryPatch).
- ✅ PA-01-05 resolvido — `IsActiveContext()` em `StanceManager` cobre raid + hideout guard.
- ✅ PA-01-06 resolvido — `Dictionary<int, StanceConfig>` + helper `BindStance(...)`.
- ✅ PA-01-07 resolvido — `StanceStaminaState.Reset()` adicionado, chamado em `OnRaidStart` e `OnRaidEnd`.
- ✅ PA-01-08 resolvido — Todos os patches/tick wrappados em `try/catch` + `Plugin.Logger.LogError`.
- ✅ PA-01-09 resolvido — `Player.IsInPronePose` usado, heurística `PoseLevel < 0.05f` removida.
- ✅ PA-01-10 resolvido — Defaults da tabela §3 batem com os valores no helper `BindStance`.
- ✅ PA-01-11 resolvido — Handler `OnStanceConfigChanged` registrado em todas as 20 entries.
- ✅ PA-01-12 resolvido — Recovery postfix usa `__instance.Player_0` direto.
- ✅ PA-01-13 resolvido — Postfix em `method_10` foi removido — PA fecha por construção.
- ✅ PA-01-14 resolvido — `Float_5`/`Single_0` `a confirmar` removidos junto com o postfix em `method_10`.
- ✅ PA-01-15 resolvido — `[HarmonyPriority(Priority.Low)]` no Recovery postfix.
- ✅ PA-01-16 resolvido — `Plugin.Logger.LogInfo`/`LogError` em todos os hooks.

## Índice de novos pontos

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| [PA-02-01](#pa-02-01) | C — Lógica | 🔴 | Mutação direta de `HandsStamina.Current` ignora os side-effects de `Consume()` (HUD, sons, exhaustion, `Multiplier` guard) | ✅ Resolvido |
| [PA-02-02](#pa-02-02) | A — Gap | 🔴 | `OnStanceChanged` é definido mas o **ponto de chamada** (de onde a lógica existente dispara) não está descrito | ✅ Resolvido |
| [PA-02-03](#pa-02-03) | C — Lógica | 🔴 | `_activeStance = 0` colide com a semântica de "Default = não-stance" do código existente | ✅ Resolvido |
| [PA-02-04](#pa-02-04) | B — Edge | 🟡 | `MaxSpeed` é dinâmico (varia com skill Strength); speed limit absoluto registrado fica stale | ✅ Resolvido |
| [PA-02-05](#pa-02-05) | C — Lógica | 🟡 | Spec marca `StanceManager` como `partial class` mas o arquivo original não é partial | ✅ Resolvido |
| [PA-02-06](#pa-02-06) | A — Gap | 🟡 | `Singleton<BackendConfigSettingsClass>` precisa ser do `Comfort.Common` — não documentado | ✅ Resolvido |
| [PA-02-07](#pa-02-07) | C — Lógica | 🟢 | `MainPlayer is not HideoutPlayer` exige C# 9+; `LangVersion` do `.csproj` não foi verificado | ✅ Resolvido |
| [PA-02-08](#pa-02-08) | A — Gap | 🟢 | `Plugin.Logger` referenciado por static — confirmar que o existente é `public static new` | ✅ Resolvido |

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Pontos

### PA-02-01 · C — Lógica · 🔴 Bloqueador {#pa-02-01}

**Mutação direta de `HandsStamina.Current` ignora todos os side-effects que `Consume()` aciona**

**Problema:** O stub de [`StanceManager.TickStanceStamina`](001-stamina-e-velocidade-technical-spec.md#5-stubs-de-código) faz drain via:

```csharp
hands.Current = Mathf.Max(0f, hands.Current - drain);
```

Mas [`GClass774.Consume(...)`](../../../../references/eft-decompiled/Assembly-CSharp/GClass774.cs#L241) é a API canônica de drenagem, e ela aciona uma série de side-effects que a mutação direta pula:

```csharp
public float Consume(PlayerPhysicalClass.GClass773 consumption, bool fromDamage = false)
{
    if (Multiplier <= 0f) return Current;       // ← guard de Multiplier
    // ...
    Current -= num;
    // ...
    action_3?.Invoke();                          // ← evento "stamina mudou" (HUD)
    if (!consumption.AllowsRestoration) DisableRestoration = ...;
    if ((Current < 15f) ^ (current < 15f)) action_1?.Invoke();   // ← cruzou threshold
    if (Current <= 0f) HandleExpiration();      // ← exhausted
    InvokeChangedAction();                       // ← invoca todos os listeners
    return result;
}
```

Pulando `Consume()` e mutando `Current` direto, o drain **não dispara**:
- HUD da stamina não atualiza (a barra fica visualmente estática enquanto o número interno cai).
- Sons/efeitos de stamina baixa não tocam.
- Estado `HandsExhausted` não é detectado quando `Current` chega a zero — então o sway/tremor que a spec funcional pediu (corner case "Stamina zero em modo Drain") **não acontece**.
- Threshold de 15f para o som "tired" não é cruzado.
- Outros mods que se inscrevem em `action_3`/`InvokeChangedAction` não são notificados.

**Por que importa:** AC funcional "Stamina zero em modo Drain: efeitos vanilla de exhausto (sway, arma tremendo) acontecem normalmente" **falha** com a implementação atual. O drain seria invisível na HUD — bug grave de UX.

**Sugestão:** Trocar `hands.Current -= drain` por chamada direta a `Consume(...)`. Construir um `PlayerPhysicalClass.GClass773` (consumption struct) com `Delta.Value = drain`, ou — alternativa mais simples — chamar `hands.UpdateStamina(novo)` em janelas grandes (≥1f de delta acumulado) e disparar `InvokeChangedAction()` manualmente nos frames intermediários:

```csharp
// Caminho A — usar Consume() (mais correto, requer construir GClass773 via reflection)
private static readonly Type GClass773Type = AccessTools.Inner(typeof(PlayerPhysicalClass), "GClass773");
private static readonly FieldInfo DeltaField = AccessTools.Field(GClass773Type, "Delta");
private static object _stanceConsumption;  // construído uma vez

// Caminho B — acumular drain em buffer e flushar via UpdateStamina quando |delta| >= 1f
private static float _accumulatedDrain;
public static void TickStanceStamina() {
    // ... guards ...
    _accumulatedDrain += baseRate * Intensity * Time.deltaTime;
    if (_accumulatedDrain >= 1f) {
        hands.UpdateStamina(Mathf.Max(0f, hands.Current - _accumulatedDrain));
        _accumulatedDrain = 0f;
    }
}
```

Caminho B é mais simples e respeita o `>1f` que `UpdateStamina` exige internamente ([GClass774.cs:392](../../../../references/eft-decompiled/Assembly-CSharp/GClass774.cs#L392)). Caminho A é mais "vanilla-fiel" mas envolve reflection sobre um tipo aninhado.

**Decisão:** `[x]` **Aceitar sugestão (caminho B — buffer + UpdateStamina)** · ✅ Resolvido em 2026-05-07
**Resolução:** `StanceStaminaState.AccumulatedDrain` adicionado como buffer; `TickStanceStamina` acumula `AimDrainRate × Intensity × Multiplier × Time.deltaTime` por frame e flusha via `hands.UpdateStamina(target)` quando `≥ 1f`. `UpdateStamina` dispara internamente `action_3` + `InvokeChangedAction` → HUD/sons/threshold de 15f/exhausted detectam normalmente. Buffer é zerado ao trocar de stance, entrar em ADS, ou suspender por prone. Aplicado em [001-stamina-e-velocidade-technical-spec.md §1.1 e §5 (StanceManager.TickStanceStamina)](001-stamina-e-velocidade-technical-spec.md#5-stubs-de-código).

---

### PA-02-02 · A — Gap · 🔴 Bloqueador {#pa-02-02}

**`OnStanceChanged` é definido mas o ponto de chamada não está documentado — em runtime nunca dispara**

**Problema:** A spec define [`StanceManager.OnStanceChanged(int previousStance, int newStance)`](001-stamina-e-velocidade-technical-spec.md#5-stubs-de-código) que chama `ApplyStanceConfig(newStance)` — esse método é a porta de entrada para registrar/remover speed limit e atualizar o cache de stamina.

Mas a spec **não mostra de onde** essa função é chamada. A lógica existente do mod ([StanceManager.cs](../../modded/StanceManager.cs)) já tem um campo `_currentStance` (ou similar) que é mutado pelo input de troca de stance (tecla V, scroll wheel). A spec não:
1. Identifica esse ponto na lógica existente.
2. Mostra como interceptar a mutação para chamar `OnStanceChanged(prev, new)`.
3. Não documenta se há um evento existente (`OnStanceChange`-like) ao qual podemos nos inscrever.

**Por que importa:** sem o wiring, `OnStanceChanged` nunca é chamado em runtime, `ApplyStanceConfig` nunca roda, speed limit nunca é registrado, cache de stamina fica em `Mode = None / Intensity = 1` — toda a feature de troca de stance fica inerte. ACs como "Após trocar de stance, o efeito da stance anterior cessa antes do próximo tick visível" **falham**.

**Sugestão:** Adicionar à §5 um trecho mostrando o ponto de wiring no `StanceManager.cs` existente. Provavelmente é um setter:

```csharp
// ANTES (existente):
private static int _currentStance = 0;

// DEPOIS (modificar para acionar nosso hook):
private static int _currentStanceField = 0;
private static int CurrentStance {
    get => _currentStanceField;
    set {
        if (value == _currentStanceField) return;
        int prev = _currentStanceField;
        _currentStanceField = value;
        OnStanceChanged(prev, value);
    }
}
// Substituir todos os `_currentStance = N` por `CurrentStance = N` na lógica existente.
```

Se a lógica existente usa um setter público ou um evento, documentar e usar. Se não, o caminho acima é o mais limpo. Adicionar à §4 (Arquivos do mod) que `StanceManager.cs` precisa converter o campo `_currentStance` para uma property com setter que dispara o hook.

**Decisão:** `[x]` **Aceitar sugestão** · ✅ Resolvido em 2026-05-07
**Resolução:** `_currentStance` (campo existente) é convertido em property `CurrentStance` com setter que detecta mudança e dispara `OnStanceChanged(prev, new)`. Todas as atribuições `_currentStance = N` da lógica existente devem ser substituídas por `CurrentStance = N`. Documentado em [§4 Arquivos do mod](001-stamina-e-velocidade-technical-spec.md#4-arquivos-do-mod) e em [§5 (Wiring da troca de stance)](001-stamina-e-velocidade-technical-spec.md#5-stubs-de-código). Item explícito no checklist §8.

---

### PA-02-03 · C — Lógica · 🔴 Bloqueador {#pa-02-03}

**`_activeStance = 0` em `OnRaidStart` colide com a semântica existente de "0 = sem stance ativa"**

**Problema:** A lógica original do mod ([Plugin.cs:164-186](../../modded/Plugin.cs)) tem `_EnableStance1`, `_EnableStance2`, `_EnableStance3` — só 3 stances numeradas. O cycle navega "Default → 1 → 2 → 3 → Default". A posição "Default" (vanilla, arma à frente) **não tem número** no código atual; é o estado fora-de-stance.

A spec deste backlog introduz "Stance 0" como sinônimo da posição vanilla, e o stub [`StanceManager.OnRaidStart`](001-stamina-e-velocidade-technical-spec.md#5-stubs-de-código) faz:

```csharp
_activeStance = 0;                  // raid começa em Padrão
ApplyStanceConfig(_activeStance);
```

Mas se a lógica existente tratar `_currentStance == 0` como "sem stance" (early-return em vários pontos), nosso `ApplyStanceConfig(0)` precisa funcionar no caminho "Stance 0 = vanilla com config" — semântica nova. Se a lógica existente faz `if (_currentStance > 0) { /* aplica offsets */ }`, então a Stance 0 nunca seria considerada "ativa" pelo código antigo, e nossa nova função aplicaria, criando inconsistência: o mod considera Stance 0 ativa para drain mas o código de offsets a considera inativa.

**Por que importa:** ambíguo entre "0 = nada acontece" (compatibilidade) e "0 = stance vanilla, gera drain" (nova semântica). Se mantermos os dois sentidos confusos, o build vai produzir bugs sutis (drain rodando em momentos onde o cycle achou que estava em Default).

**Sugestão:** Adicionar à §5 uma seção "Mapeamento Stance 0 ↔ Default" que torne explícito:
- Stance 0 **não tem offsets** de mãos (a posição visual já é a vanilla — nenhuma transformação aplicada).
- Stance 0 **tem config de stamina/velocidade/prone** (as 5 props deste backlog).
- O cycle do mod (`_EnableStance1/2/3 in Cycle`) **não toca** em Stance 0 — quando o cycle volta a "Default", entramos em Stance 0 implicitamente.
- Internamente, usar uma variável separada `_activeStaminaStance` (0–3) que reflete a stance "para fins de stamina/velocidade", desacoplando de `_currentStance` (1–3 ou 0=nenhuma) usado pela lógica de offsets.

OU: renomear `_activeStance` para `_activeStaminaStance` no escopo deste backlog para evitar colisão com nomenclatura existente.

**Decisão:** `[x]` **Aceitar sugestão (variável separada `_activeStaminaStance`)** · ✅ Resolvido em 2026-05-07
**Resolução:** Renomeado `_activeStance` para `_activeStaminaStance` em todos os lugares relevantes (`OnRaidStart`, `OnRaidEnd`, `OnStanceChanged`, `EvaluateProneSuspensionTick`, `GetActiveStaminaStance`). Adicionada nova subseção [§1 "Mapeamento Stance 0 ↔ Default do mod existente"](001-stamina-e-velocidade-technical-spec.md#1-estratégia) deixando explícito que `_currentStance` (existente, controla offsets visuais) e `_activeStaminaStance` (novo, controla stamina/velocidade) são variáveis paralelas atualizadas pelo mesmo setter. Stance 0 não tem offsets — só config de stamina/velocidade.

---

### PA-02-04 · B — Edge · 🟡 Importante {#pa-02-04}

**`MaxSpeed` é dinâmico — speed limit absoluto registrado uma vez fica stale ao longo da raid**

**Problema:** O stub `ApplyStanceConfig` faz:

```csharp
mc.AddStateSpeedLimit(limit * mc.MaxSpeed, Plugin.StanceSpeedLimitCause);
```

`mc.MaxSpeed` é `get-only` e calculado dinamicamente em [MovementContext.cs:910](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L910):

```csharp
public float MaxSpeed => GClass2298.Evaluate(BackendConfig.WalkSpeed,
    (float)SkillManager.Strength.SummaryLevel / 60f);
```

Depende da skill `Strength`, que pode subir mid-raid (correr ganha XP). Quando isso acontece, `MaxSpeed` aumenta — mas nosso `AddStateSpeedLimit` foi registrado com o valor antigo de `MaxSpeed`. Resultado: o cap absoluto continua congelado no valor antigo, e a "fração" efetiva (limite ÷ novo MaxSpeed) fica menor que `0.9` configurado.

O EFT lida com isso re-rodando `AddStateSpeedLimit(walkSpeedLimit * MaxSpeed, ESpeedLimit.Weight)` ([linha 1957](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L1957)) sempre que peso/conditions mudam — re-registro periódico.

**Por que importa:** numa raid longa com muito sprint (XP de Strength), o efetivo redutor de Stance 0 (90%) pode virar 88% ou 85% silenciosamente. Pequeno, mas viola o AC: "Stance N Movement Speed Multiplier = 75 → velocidade fica em 75% (medível)".

**Sugestão:** Adicionar dois caminhos:
- (a) Subscribir ao evento `MovementContext.OnMaxSpeedChangedEvent` ([linha 1511](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L1511)) e re-aplicar o limit quando dispara.
- (b) Re-aplicar `AddStateSpeedLimit` no tick (`StanceManager.Update`) — barato porque só roda enquanto a stance ativa modifica velocidade.

Caminho (a) é mais elegante; (b) é mais defensivo (cobre qualquer mudança, não só MaxSpeed). Recomendo (b) — adicionar trecho à `EvaluateProneSuspensionTick()` (já roda por frame e já sabe se é pra aplicar) ou em uma função-irmã `RefreshSpeedLimitTick()`.

**Decisão:** `[x]` **Aceitar sugestão (caminho b — re-aplica no tick)** · ✅ Resolvido em 2026-05-07
**Resolução:** `EvaluateProneSuspensionTick` agora também faz **refresh defensivo** do speed limit a cada frame quando a stance ativa modifica velocidade e não está suspensa: `mc.RemoveStateSpeedLimit(cause); mc.AddStateSpeedLimit(fraction × mc.MaxSpeed, cause);`. Cobre staleness se `MaxSpeed` mudar (skill Strength sobe, conditions, etc.). Custo: 2 ops O(1) num Dictionary por frame — aceitável. Caminho (b) escolhido sobre (a) por ser mais defensivo (cobre qualquer mudança, não só `MaxSpeed`). Aplicado em [§5 (StanceManager.EvaluateProneSuspensionTick)](001-stamina-e-velocidade-technical-spec.md#5-stubs-de-código).

---

### PA-02-05 · C — Lógica · 🟡 Importante {#pa-02-05}

**Spec usa `partial class StanceManager` mas o arquivo original não é partial — não compila**

**Problema:** O stub em §5 declara:

```csharp
public static partial class StanceManager
{
    private static bool _raidEnded;
    // ...
}
```

O `StanceManager.cs` existente em `modded/` é uma classe estática **não-partial** (precisa confirmar abrindo o arquivo, mas o padrão do projeto não usa partial). Misturar `partial` em uma definição com não-partial em outra produz erro de compilação **CS0260**.

**Por que importa:** build falha imediatamente. PA bloqueador para build, mas marquei 🟡 porque é trivialmente corrigível pelo dev na hora — não invalida design.

**Sugestão:** Trocar `public static partial class StanceManager` por instruções "adicionar os seguintes membros à classe `StanceManager` existente em `modded/StanceManager.cs`". Apresentar o trecho como "adições à classe", não como definição completa partial. Ou, se preferir mantê-lo como bloco, marcar `// In modded/StanceManager.cs — adicione estes membros à classe existente (não criar partial)`.

**Decisão:** `[x]` **Aceitar sugestão** · ✅ Resolvido em 2026-05-07
**Resolução:** O bloco em [§5 (StanceManager.cs)](001-stamina-e-velocidade-technical-spec.md#5-stubs-de-código) agora é apresentado com cabeçalho explícito "**adições à classe estática existente — não criar partial**". A nota destaca que misturar partial/non-partial gera CS0260. Imports são listados em comentários de bloco. Hook em `Plugin.Update()` é apresentado como bloco separado (também adição, não substituição).

---

### PA-02-06 · A — Gap · 🟡 Importante {#pa-02-06}

**`Singleton<BackendConfigSettingsClass>` no tick de drain — namespace não documentado**

**Problema:** O stub `TickStanceStamina` usa:

```csharp
float baseRate = Singleton<BackendConfigSettingsClass>.Instance.Stamina.AimDrainRate;
```

E o stub `RaidLifecyclePatches.cs` + outros usam `Singleton<GameWorld>.Instance`. Pelo skill `spt-mod-best-practices` §2:

> `Comfort.Common.Singleton<T>` — o **único** Singleton correto (existe também `RootMotion.Singleton` — não importar esse).

A spec não declara o `using Comfort.Common;` em nenhum stub. Se o dev importar `RootMotion.Singleton` (autocompletion possível), o tipo é diferente, e `BackendConfigSettingsClass` pode não estar registrado lá — `NullReferenceException` em runtime.

**Por que importa:** no melhor caso, build falha; no pior, runtime exception silenciosa que ninguém debuga porque "Singleton.Instance retornou null, mas Singleton compila".

**Sugestão:** Em §5, adicionar `using Comfort.Common;` no topo de cada stub que usa `Singleton<>`. Idealmente, listar imports completos no início de cada arquivo:

```csharp
using System;
using System.Reflection;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;
```

E adicionar uma nota em §7 (Riscos) destacando "atenção ao importar Singleton — somente Comfort.Common".

**Decisão:** `[x]` **Aceitar sugestão** · ✅ Resolvido em 2026-05-07
**Resolução:** Adicionada subseção [§1 "Singleton — atenção ao namespace"](001-stamina-e-velocidade-technical-spec.md#1-estratégia) explicitando o namespace correto. Nota em [§7 Riscos](001-stamina-e-velocidade-technical-spec.md#7-riscos-e-dependências) sobre `Comfort.Common.Singleton` vs `RootMotion.Singleton`. **Todos os stubs em §5** (Plugin.cs, StanceManager.cs, StanceStaminaRecoveryPatch.cs, RaidLifecyclePatches.cs) declaram `using Comfort.Common;` explicitamente.

---

### PA-02-07 · C — Lógica · 🟢 Menor {#pa-02-07}

**`MainPlayer is not HideoutPlayer` exige C# 9+ — `LangVersion` do `.csproj` não foi verificado**

**Problema:** O stub `IsActiveContext` usa:

```csharp
return gw != null
    && gw.MainPlayer != null
    && gw.MainPlayer is not HideoutPlayer;
```

`is not` é sintaxe de **C# 9** (pattern matching estendido). Se `mods/<mod>/modded/CameraRotationMod.csproj` tiver `<LangVersion>` < 9 (ou implícito da framework target), build falha com **CS8400**.

**Por que importa:** trivialmente corrigível, mas pode pegar de surpresa. Pelo skill `csharp-mod-best-practices` §9: "LangVersion: pin in the .csproj. Match what the Unity Mono / SPT server runtime supports (typically C# 9–11)."

**Sugestão:** Em §8 (Checklist), adicionar tarefa: "Verificar `<LangVersion>` em `CameraRotationMod.csproj`; se < 9, ou usar `!(... is HideoutPlayer)` em vez de `is not`, ou bumpar `<LangVersion>` para 9 (recomendado, é compatível com Unity Mono SPT 4.0+)."

**Decisão:** `[x]` **Aceitar sugestão (verificar `LangVersion`) + caminho alternativo (usar `!(... is HideoutPlayer)`)** · ✅ Resolvido em 2026-05-07
**Resolução:** Aplicados os dois caminhos em conjunto:
1. **Item explícito no checklist [§8](001-stamina-e-velocidade-technical-spec.md#8-checklist-de-implementação)**: verificar `<LangVersion>` em `CameraRotationMod.csproj`; bumpar para 9 se < 9.
2. **Stubs em §5 trocaram `is not X` por `!(... is X)`** — compatível com qualquer `LangVersion`. Ex.: `IsActiveContext` usa `return !(gw.MainPlayer is HideoutPlayer);` e o Recovery postfix usa `if (!(!(gw.MainPlayer is HideoutPlayer))) return;`. Defensivo — funciona mesmo se ninguém ajustar o `.csproj`.
3. [§4 Arquivos do mod](001-stamina-e-velocidade-technical-spec.md#4-arquivos-do-mod) lista `CameraRotationMod.csproj` como **VERIFICAR**.

---

### PA-02-08 · A — Gap · 🟢 Menor {#pa-02-08}

**`Plugin.Logger` referenciado por static — confirmar `public static new ManualLogSource`**

**Problema:** Stubs referenciam `Plugin.Logger.LogError(...)`, `Plugin.Logger.LogInfo(...)` em vários pontos. O [Plugin.cs:12](../../modded/Plugin.cs) existente declara:

```csharp
public static new ManualLogSource Logger;
```

`new` é necessário porque `BaseUnityPlugin.Logger` (instance, não static) já existe — sem `new`, o C# emite warning CS0108 e o tipo continuou sendo o herdado. Logger atual já está assim e funciona.

**Por que importa:** se algum dev modifica essa declaração (achando que `new` é desnecessário), o `static` shadow é perdido e os stubs do nosso backlog passam a referenciar `Plugin.Logger` instance. Quebra em chamadas estáticas.

**Sugestão:** Em §5 (stubs), comentário explícito antes do trecho do `Plugin.cs`: `// Mantém: public static new ManualLogSource Logger; — shadow do BaseUnityPlugin.Logger`. E em §8 (Checklist), tarefa: "Confirmar que `Plugin.Logger` permaneceu `public static new` após edição."

**Decisão:** `[x]` **Aceitar sugestão** · ✅ Resolvido em 2026-05-07
**Resolução:**
1. Stub do `Plugin.cs` em [§5](001-stamina-e-velocidade-technical-spec.md#5-stubs-de-código) agora inclui o comentário explícito: `// ⚠️ MANTER o "public static new" — shadow estático do BaseUnityPlugin.Logger. Sem o "new", vira shadowing implícito (warning CS0108) e os stubs static deste backlog quebram.`
2. Adicionado item de checklist em [§8](001-stamina-e-velocidade-technical-spec.md#8-checklist-de-implementação): "Confirmar `Plugin.Logger` permaneceu `public static new ManualLogSource Logger;`".
3. [§4 Arquivos do mod](001-stamina-e-velocidade-technical-spec.md#4-arquivos-do-mod) também menciona a preservação do `new` na linha do `Plugin.cs`.

---

## Próximos passos

✅ **Todas as 8 PAs aplicadas em 2026-05-07.** Próximo:

1. Rodar `/review-technical-spec` novamente para gerar `technical-review-03.md` validando que os 8 fechamentos foram corretamente refletidos na spec técnica e checando se surgiram pontos novos.
2. Se a review-03 vier sem 🔴 e sem 🟡, executar `/build-item`.
