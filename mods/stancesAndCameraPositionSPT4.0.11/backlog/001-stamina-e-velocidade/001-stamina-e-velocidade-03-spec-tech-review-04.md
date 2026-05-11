# 001 — Stamina e Velocidade por Postura · Review Técnica 04

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec técnica revisada:** [001-stamina-e-velocidade-02-spec-tech.md](001-stamina-e-velocidade-02-spec-tech.md)
**Spec funcional referência:** [001-stamina-e-velocidade-01-spec.md](001-stamina-e-velocidade-01-spec.md)
**Reviews anteriores:** [01](001-stamina-e-velocidade-03-spec-tech-review-01.md) · [02](001-stamina-e-velocidade-03-spec-tech-review-02.md) · [03](001-stamina-e-velocidade-03-spec-tech-review-03.md)
**Data:** 2026-05-08

> Análise crítica após resolução das PAs da review-03, com foco no novo caminho introduzido pela reversão do PA-03-05 (drain por frame com reflection nos eventos privados de `GClass774`). Validação do Assembly real e do código existente em `modded/`.
>
> Skills aplicadas: `spt-mod-best-practices` + `csharp-mod-best-practices`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 3 (✅ **3 resolvidos**) · 🟢 Menores: 3 (✅ **3 resolvidos**) · Total: **6 — todos resolvidos** em 2026-05-08
>
> ✅ **Status:** todos os 6 PAs aplicados na spec técnica. **Pronto para `/build-item`.**

## Reviews anteriores resolvidas

Todas as 8 PAs da review-03 confirmadas resolvidas na spec atual:

- ✅ PA-03-01 resolvido — `enum Stance` em todo lugar (Plugin._stanceConfigs, OnStanceChanged, ApplyStaminaStance, GetActiveStaminaStance).
- ✅ PA-03-02 resolvido — Property `CurrentStance` existente é modificada (Opção A — setter customizado), `private set` mantido.
- ✅ PA-03-03 resolvido — `BaseLocalGame.Stop` resolve com `new[] { typeof(string), typeof(ExitStatus), typeof(string), typeof(float) }`.
- ✅ PA-03-04 resolvido — `OnRaidEnd` chama `ResetState()` existente.
- ✅ PA-03-05 resolvido — **Revertido para Opção B** após feedback do usuário: drain por frame + reflection cacheada em `action_3`/`action_1` + `InvokeChangedAction`. HUD atualiza fluido.
- ✅ PA-03-06 resolvido — `_staminaConfigDirty` + `MarkStaminaConfigDirty()` (padrão dirty-flag existente).
- ✅ PA-03-07 resolvido — Double-negação removida (`if (gw.MainPlayer is HideoutPlayer) return;`).
- ✅ PA-03-08 resolvido — Cache `_lastAppliedSpeedLimit` evita Remove+Add a 60 Hz.

## Índice de novos pontos

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| [PA-04-01](#pa-04-01) | C — Lógica | 🟡 | `action_1`/`action_3` são nomes do **decompilador** (backing fields de eventos públicos) — reflection fragil | ✅ Resolvido |
| [PA-04-02](#pa-04-02) | A — Gap | 🟡 | `Singleton<BackendConfigSettingsClass>.Instance` acessado por frame no tick — cachear `AimDrainRate` no raid start | ✅ Resolvido |
| [PA-04-03](#pa-04-03) | A — Gap | 🟡 | Reflection inicializa em `static readonly` — falha silenciosa se BSG renomear; spec menciona warning mas sem stub do check | ✅ Resolvido |
| [PA-04-04](#pa-04-04) | B — Edge | 🟢 | `HandleExpiration()` não é chamado quando drain hits 0 — event `OnExpired` não dispara | ✅ Resolvido |
| [PA-04-05](#pa-04-05) | C — Lógica | 🟢 | `Mathf.Abs(prev - target) < float.Epsilon` é tolerância excessiva (~1e-7) — em float deveria ser ~1e-4 | ✅ Resolvido |
| [PA-04-06](#pa-04-06) | A — Gap | 🟢 | `using System.Reflection;` não declarado nos imports do `StanceManager.cs` — spec usa `System.Reflection.FieldInfo` fully-qualified | ✅ Resolvido |

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

### PA-04-01 · C — Lógica · 🟡 Importante {#pa-04-01}

**`action_1`/`action_3` são nomes do decompilador — reflection nesses nomes é frágil**

**Problema:** A spec técnica em §1 ("Granularidade do drain") e §5 (`StanceManager` additions) faz reflection em campos privados:

```csharp
private static readonly FieldInfo _action3Field = AccessTools.Field(typeof(GClass774), "action_3");
private static readonly FieldInfo _action1Field = AccessTools.Field(typeof(GClass774), "action_1");
```

Olhando o Assembly real ([GClass774.cs:138-226](../../../../references/eft-decompiled/Assembly-CSharp/GClass774.cs#L138-L226)), esses não são campos privados arbitrários — são **backing fields de eventos públicos**:

```csharp
public event Action OnThresholdPass { add { ... action_1 ... } remove { ... } }   // action_1 = OnThresholdPass
public event Action OnChanged       { add { ... action_2 ... } remove { ... } }   // action_2 = OnChanged (= InvokeChangedAction)
public event Action OnValueChanged  { add { ... action_3 ... } remove { ... } }   // action_3 = OnValueChanged
```

O nome `action_3` é **invenção do ILSpy** porque o nome original do backing field se perdeu na compilação. A próxima versão do EFT, ao recompilar, pode gerar um descompilado com `action_2` e `action_4` no lugar (depende da ordem de declaração das events) — sem que BSG tenha mudado nada propositalmente.

**Por que importa:** silent failure. `AccessTools.Field` retorna null se o nome não bater. Sem warning visível, o jogador percebe que "a HUD não atualiza" só depois de testar. Pelo skill `spt-mod-best-practices` §1: "Para alvos ofuscados, resolva o `MethodBase` em um helper estático usando uma **assinatura/predicado estável**".

**Sugestão:** Trocar a reflection por **resolução por assinatura** baseada nos nomes públicos dos eventos (que são estáveis):

```csharp
private static FieldInfo ResolveBackingField(Type t, string eventName)
{
    var ev = t.GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance);
    if (ev == null) return null;

    // Backing field tem o mesmo nome do event em auto-events; em events com add/remove customizados,
    // o nome é renomeado pelo decompilador. Procurar Action privado cujos accessors do event referenciam.
    // Fallback: pegar todos os "private Action" e chutar pela ordem (frágil — só pra log de aviso).
    return t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.FieldType == typeof(Action))
            .ElementAtOrDefault(eventName switch {
                "OnThresholdPass" => 1,    // action_1 (índice 1 na ordem de declaração)
                "OnChanged"       => 2,
                "OnValueChanged"  => 3,
                _ => -1
            });
}

// Uso:
private static readonly FieldInfo _onValueChangedBacking =
    ResolveBackingField(typeof(GClass774), nameof(GClass774.OnValueChanged));
```

Alternativa mais elegante (mas mais invasiva) — **construir um `GClass773` consumption e chamar `Consume()`**, que dispara todos os events naturalmente:

```csharp
// Uma vez no Awake:
private static readonly Type _gclass773Type =
    typeof(PlayerPhysicalClass).GetNestedType("GClass773", BindingFlags.NonPublic);
private static readonly FieldInfo _deltaField = AccessTools.Field(_gclass773Type, "Delta");
// Consume retorna todos os events disparados naturalmente — sem reflection nos action_*.
```

A alternativa "Consume" é mais vanilla-fiel mas requer reflection na nested type `PlayerPhysicalClass.GClass773` (também decompilador-renamed — mesmo problema).

**Recomendo o caminho "resolução por evento público + log warning"** — usa nomes estáveis (eventos `public` que BSG mantém), com fallback gracioso (drain continua funcional, só HUD silenciosa) e log no Awake quando a resolução falha.

**Decisão:** `[x]` **Aceitar sugestão (resolução por evento público + log)** · ✅ Resolvido em 2026-05-08
**Resolução:** Helper `ResolveBackingFieldByCandidates(Type t, params string[] candidates)` adicionado em `StanceManager`. Resolve cada backing field tentando o nome do **evento público C#** primeiro (estável: `OnValueChanged`, `OnThresholdPass`) e o nome do decompilador (`action_3`, `action_1`) como fallback. Aplicado em [§1 "Granularidade do drain"](001-stamina-e-velocidade-02-spec-tech.md#1-estratégia) (tabela de mapeamento eventos↔backing fields) e em [§5 (`StanceManager` additions)](001-stamina-e-velocidade-02-spec-tech.md#5-stubs-de-código). `InvokeChangedAction()` e `HandleExpiration()` agora são chamadas diretas (são públicos, não precisam reflection).

---

### PA-04-02 · A — Gap · 🟡 Importante {#pa-04-02}

**`Singleton<BackendConfigSettingsClass>.Instance` acessado por frame no tick — cachear**

**Problema:** O stub `TickStanceStamina` em §5 faz:

```csharp
float baseRate = Singleton<BackendConfigSettingsClass>.Instance.Stamina.AimDrainRate;
```

Esse caminho roda **todo frame** (60 Hz) durante drain ativo. `Singleton<T>.Instance` é uma property que faz lookup, e `.Stamina.AimDrainRate` desreferencia por mais 2 níveis. `AimDrainRate` é uma constante (`= 3f` em [BackendConfigSettingsClass.cs:904](../../../../references/eft-decompiled/Assembly-CSharp/BackendConfigSettingsClass.cs#L904)) — não muda em runtime.

**Por que importa:** pelo skill `csharp-mod-best-practices` §3: "Cache reflection: ... resolve once in a static initializer, never per call". Mesma lógica vale para singleton lookup em hot path. Custo individual é baixo (~ns), mas em `TickStanceStamina` que roda 60 Hz × 4 stances cumulativamente, soma. E pelo skill §1: "no allocations in hot paths" — `Singleton<T>.Instance` provavelmente não aloca, mas é trabalho desnecessário.

**Sugestão:** Cachear `AimDrainRate` em `OnRaidStart` quando o `Singleton<BackendConfigSettingsClass>` está garantido pronto:

```csharp
// Em StanceManager (campo novo):
private static float _cachedAimDrainRate = 3f;   // fallback default

// Em OnRaidStart:
public static void OnRaidStart()
{
    try
    {
        // ...
        var backend = Singleton<BackendConfigSettingsClass>.Instance;
        if (backend?.Stamina != null)
            _cachedAimDrainRate = backend.Stamina.AimDrainRate;
        // ...
    }
}

// Em TickStanceStamina:
float drain = _cachedAimDrainRate * StanceStaminaState.Intensity * hands.Multiplier * Time.deltaTime;
```

Documentar em §1 (subseção "Singleton — atenção ao namespace") que constants do `BackendConfigSettingsClass` são imutáveis em runtime e podem ser cacheadas no raid start.

**Decisão:** `[x]` **Aceitar sugestão** · ✅ Resolvido em 2026-05-08
**Resolução:** Adicionado `_cachedAimDrainRate` field no `StanceManager` (default `3f`, fallback vanilla). `OnRaidStart` lê `Singleton<BackendConfigSettingsClass>.Instance.Stamina.AimDrainRate` uma vez e popula o cache. `TickStanceStamina` usa `_cachedAimDrainRate` em vez de re-resolver o singleton todo frame. Documentado em [§1 "Granularidade do drain"](001-stamina-e-velocidade-02-spec-tech.md#1-estratégia).

---

### PA-04-03 · A — Gap · 🟡 Importante {#pa-04-03}

**Reflection inicializa em `static readonly` — falha silenciosa se BSG renomear; spec menciona warning mas sem stub**

**Problema:** A spec em §7 (Riscos) diz:

> Logar warning no Awake se algum field-info for null (BSG renomeando esses campos privados em update futuro).

Mas em §5 (Plugin.cs ou StanceManager.cs additions), **não há stub mostrando esse log no Awake**. A inicialização é:

```csharp
private static readonly FieldInfo _action3Field = AccessTools.Field(typeof(GClass774), "action_3");
private static readonly FieldInfo _action1Field = AccessTools.Field(typeof(GClass774), "action_1");
private static readonly MethodInfo _invokeChangedActionMethod =
    AccessTools.Method(typeof(GClass774), nameof(GClass774.InvokeChangedAction));
```

Se algum retornar null:
- `NotifyHandsStaminaChanged` faz `?.Invoke()` defensivo — não crasha, mas o evento não dispara.
- O usuário só percebe que a HUD não atualiza (depois de testar in-game).
- Sem log no Awake, nada indica a causa.

**Por que importa:** se essa feature falhar silenciosamente após uma atualização do EFT, debug fica caro. Pelo skill `csharp-mod-best-practices` §8: "Error only for unexpected exceptions" — null em reflection cacheada é exatamente isso.

**Sugestão:** Adicionar à §5, no `Plugin.Awake` após registrar os patches:

```csharp
public void Awake()
{
    Logger = base.Logger;
    // ... binds, patches ...

    // Validar resoluções de reflection — log warning se algo falhou
    if (StanceManager.HasMissingReflection(out var missing))
    {
        Logger.LogWarning(
            $"[StanceStaminaPatch] Reflection incompleta — HUD pode não atualizar durante drain. " +
            $"Campos não resolvidos: {string.Join(", ", missing)}. " +
            $"Provavelmente uma nova versão do EFT renomeou campos privados de GClass774. " +
            $"Drain continua funcional, mas eventos para a HUD podem não disparar.");
    }
}
```

E em `StanceManager`:

```csharp
public static bool HasMissingReflection(out List<string> missing)
{
    missing = new List<string>();
    if (_action3Field == null) missing.Add("action_3 (OnValueChanged backing)");
    if (_action1Field == null) missing.Add("action_1 (OnThresholdPass backing)");
    if (_invokeChangedActionMethod == null) missing.Add("InvokeChangedAction");
    return missing.Count > 0;
}
```

Adicionar item explícito ao checklist [§8](001-stamina-e-velocidade-02-spec-tech.md#8-checklist-de-implementação): "Validar reflection no `Awake` — log warning se field/method-info for null."

**Decisão:** `[x]` **Aceitar sugestão** · ✅ Resolvido em 2026-05-08
**Resolução:** Helper `StanceManager.HasMissingReflection(out List<string> missing)` adicionado. `Plugin.Awake` chama-o após registrar os patches; se houver fields não resolvidos, loga warning explícito via `Logger.LogWarning` listando os campos faltantes. Drain segue funcional, só HUD silenciosa — degradação graciosa. Stub do warning está em [§5 (Plugin.cs)](001-stamina-e-velocidade-02-spec-tech.md#5-stubs-de-código). Item de checklist atualizado em [§8](001-stamina-e-velocidade-02-spec-tech.md#8-checklist-de-implementação).

---

### PA-04-04 · B — Edge · 🟢 Menor {#pa-04-04}

**`HandleExpiration()` não é chamado quando drain hits 0 — `OnExpired` event não dispara**

**Problema:** O stub `TickStanceStamina` muta `Current` direto e chama `NotifyHandsStaminaChanged`, mas não chama `HandleExpiration()` ([GClass774.cs:298-301](../../../../references/eft-decompiled/Assembly-CSharp/GClass774.cs#L298-L301)) quando `Current` chega a zero:

```csharp
public void HandleExpiration()
{
    action_0?.Invoke();   // backing de public event Action OnExpired
}
```

Em vanilla, `Consume()` chama `HandleExpiration()` quando `Current <= 0` (linha 272). Nosso drain manual não — então `OnExpired` event não dispara. Mods que assinam `gClass774.OnExpired` (raros, mas existem) não são notificados.

**Por que importa:** baixo. O uso de `Exhausted` (que é `Current < 15f`, derivado) cobre o estado visual de exhausted (sway, tremor) — esse continua funcionando. Apenas o evento de expiração não dispara para listeners externos.

**Sugestão:** Adicionar ao `TickStanceStamina`, após `NotifyHandsStaminaChanged`:

```csharp
// Replicar HandleExpiration vanilla quando drain hits 0
if (target <= 0f && prev > 0f)
    hands.HandleExpiration();   // método público — sem reflection
```

`HandleExpiration` é **public** (linha 298) — sem reflection necessária.

**Decisão:** `[x]` **Aceitar sugestão** · ✅ Resolvido em 2026-05-08
**Resolução:** `TickStanceStamina` agora chama `hands.HandleExpiration()` (público) quando o drain leva `Current` de >0 para 0 no mesmo frame, replicando o comportamento vanilla de `Consume()`. Listeners de `OnExpired` são notificados.

---

### PA-04-05 · C — Lógica · 🟢 Menor {#pa-04-05}

**`Mathf.Abs(prev - target) < float.Epsilon` é tolerância excessiva**

**Problema:** O stub do `TickStanceStamina` em §5 faz:

```csharp
if (Mathf.Abs(prev - target) < float.Epsilon) return;
```

`float.Epsilon` é ~1.4e-45 (menor float positivo) — useful para comparação contra zero, **inadequado** para comparar diferenças entre floats arbitrários onde erros de arredondamento são da ordem de 1e-7.

**Por que importa:** com drain real (1.5/s × 1/60 = 0.025/frame), `prev - target = 0.025`, o teste `< float.Epsilon` é falso e a função prossegue normalmente. Então **não há bug de runtime** — o teste só evita o caso patológico de drain 0. Mas a intenção da linha é "skip when no meaningful change", e `float.Epsilon` é literal demais.

**Sugestão:** Trocar por `< 0.0001f` (drain mínimo significativo): mantém o early-exit mas com tolerância prática:

```csharp
if (Mathf.Abs(prev - target) < 0.0001f) return;
```

Ou simplesmente checar `drain < 0.0001f` antes de calcular `target` — mais simples e direto:

```csharp
if (drain < 0.0001f) return;
float prev = hands.Current;
float target = Mathf.Max(0f, prev - drain);
```

**Decisão:** `[x]` **Aceitar sugestão (variante simples — early-exit no drain)** · ✅ Resolvido em 2026-05-08
**Resolução:** Trocado `Mathf.Abs(prev - target) < float.Epsilon` por early-exit `drain < 0.0001f` antes de calcular `target`. Mais legível e elimina computações desnecessárias quando drain é desprezível.

---

### PA-04-06 · A — Gap · 🟢 Menor {#pa-04-06}

**`using System.Reflection;` não declarado nos imports do `StanceManager.cs`**

**Problema:** O stub do `StanceManager` em §5 declara reflection sem `using System.Reflection;` no bloco de imports:

```csharp
using System;
using Comfort.Common;
using EFT;
using UnityEngine;
```

Mas usa `System.Reflection.FieldInfo` fully-qualified:

```csharp
private static readonly System.Reflection.FieldInfo _action3Field = ...;
```

Funciona, mas é inconsistente com os outros stubs (que usam `using System.Reflection;`).

**Por que importa:** legibilidade. Próximo dev reading o stub vê `System.Reflection.FieldInfo` e pode achar que tem alguma intenção (vs `FieldInfo` simples).

**Sugestão:** Adicionar `using System.Reflection;` aos imports do `StanceManager` em §5 e trocar `System.Reflection.FieldInfo` por `FieldInfo` simples nas declarações.

**Decisão:** `[x]` **Aceitar sugestão** · ✅ Resolvido em 2026-05-08
**Resolução:** Imports do `StanceManager` em [§5](001-stamina-e-velocidade-02-spec-tech.md#5-stubs-de-código) ampliados para incluir `using System.Reflection;`, `using System.Collections.Generic;`, `using HarmonyLib;`. Declarações usam `FieldInfo` simples.

---

## Próximos passos

✅ **Todas as 6 PAs aplicadas em 2026-05-08.** Spec técnica está pronta para `/build-item`.

1. (Opcional) Rodar `/review-technical-spec` para gerar `technical-review-05.md` validando os fechamentos antes do build.
2. Executar `/build-item mods\stancesAndCameraPositionSPT4.0.11\backlog\001-stamina-e-velocidade\` para implementar a feature em `mods/stancesAndCameraPositionSPT4.0.11/modded/`.
