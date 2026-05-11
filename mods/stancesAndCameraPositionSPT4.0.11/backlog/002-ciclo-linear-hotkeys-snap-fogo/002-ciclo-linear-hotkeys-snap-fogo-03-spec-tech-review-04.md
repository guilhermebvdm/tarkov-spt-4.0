# 002 — Ciclo linear, hotkeys e snap fogo · Review Técnica 04

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec técnica revisada:** [002-ciclo-linear-hotkeys-snap-fogo-02-spec-tech.md](002-ciclo-linear-hotkeys-snap-fogo-02-spec-tech.md)
**Data:** 2026-05-10

> Análise crítica da spec técnica após as 5 resoluções da review-03. Foco: collisão de ordens no F12, ordem de processamento de input no `Update`, e edge-cases residuais do 2-frame pulse.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 5 · Total: 5

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| PA-04-01 | A — Gap | 🟡 | Colisão de `Order = 59` entre `Mouse Wheel Modifier Key` (existente) e `Mouse Wheel Scroll Mode` (novo) | ✅ Resolvido |
| PA-04-02 | B — Edge | 🟡 | Hotkey de stance e tecla `V` processadas em ordem errada — V cicla antes de hotkey override | ✅ Resolvido |
| PA-04-03 | B — Edge | 🟢 | Double-tap rápido de fogo durante F4 reset pode causar 1 frame de stutter em fullauto | ✅ Resolvido |
| PA-04-04 | A — Gap | 🟢 | F5 `SpringGetPatch.ResetState()` em raid start interrompe transições em vôo (cenário teórico) | ✅ Resolvido |
| PA-04-05 | A — Gap | 🟢 | Order da `_MouseWheelModifierKey` muda de 59 para 58 mas é marcada "sem alteração" no delta | ✅ Resolvido |

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

### PA-04-01 · A — Gap · 🟡 Importante · ✅ Resolvido em 2026-05-10

**Colisão de `Order = 59` entre `Mouse Wheel Modifier Key` (existente) e `Mouse Wheel Scroll Mode` (novo)**

**Problema:** Confirmado em [Plugin.cs:244](../../modded/Plugin.cs#L244): o `_MouseWheelModifierKey` existente já usa `new ConfigurationManagerAttributes { Order = 59 }`. A spec técnica em §5.2 e na tabela "Layout do F12" (§F12 Settings) coloca o **novo** `_MouseWheelScrollMode` também em `Order = 59`:

```csharp
_MouseWheelScrollMode = Config.Bind(
    Settings,
    "Mouse Wheel Scroll Mode",
    ScrollMode.Linear,
    new ConfigDescription(...,
        null,
        new ConfigurationManagerAttributes { Order = 59 }));
```

Dois `ConfigEntry` na mesma seção com mesmo `Order` → ConfigurationManager renderiza em ordem indeterminada (`SortedDictionary` interno usa fallback alfabético/insertion). O usuário verá os dois empilhados sem ordem garantida — quebra a previsibilidade do layout.

**Por que importa:** O critério de aceite F2 implícito é "o novo `Mouse Wheel Scroll Mode` aparece logo abaixo de `Enable Mouse Wheel Stance Cycle`". Com a colisão, pode aparecer acima ou abaixo do `Mouse Wheel Modifier Key` aleatoriamente. Comportamento visual inconsistente entre boots.

**Sugestão:** Mover `_MouseWheelScrollMode` para o **slot livre deixado por `_UseOnlyStances` removido** (`Order = 58`). Layout final:

| Order | Propriedade |
| --- | --- |
| 60 | `Enable Mouse Wheel Stance Cycle` (sem alteração) |
| 59 | `Mouse Wheel Modifier Key` (sem alteração — Order verdadeiramente preservado) |
| 58 | **`Mouse Wheel Scroll Mode`** [NOVO] (slot deixado por `Use Only Stances`) |
| ~~58~~ | ~~`Use Only Stances`~~ [REMOVIDO] |
| 57 | `Stance Transition Speed` |

Visualmente o `Scroll Mode` fica abaixo do `Modifier Key` em vez de entre `Cycle Toggle` e `Modifier Key`, mas essa adjacência é cosmética — o `Scroll Mode` é †visível-condicionalmente quando o `Cycle Toggle` está on, então o usuário sempre vê os dois juntos quando ambos estão visíveis.

Atualizar:
- §3 (tabela Novas propriedades F12) — Order 58 em vez de 59 para `Mouse Wheel Scroll Mode`.
- §5.2 stub — `Order = 58`.
- F12 layout (final do documento) — reorganizar tabela.
- Resolve PA-04-05 automaticamente (`Modifier Key` permanece em 59).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (mover `Scroll Mode` para Order 58)
- `[ ]` Caminho alternativo (manter Order 59 para `Scroll Mode` e bumbar `Modifier Key` para 58 — tratá-lo explicitamente como `[ALTERADO]` no delta): _________________

**Resolução:** §3 (tabela de novas props), §5.2 (stub do bind) e tabela "Layout do F12" da spec atualizadas — `_MouseWheelScrollMode` agora em Order 58 (slot livre deixado por `_UseOnlyStances` removido). `_MouseWheelModifierKey` permanece em Order 59 verdadeiramente "sem alteração". Resolve PA-04-05 automaticamente.

---

### PA-04-02 · B — Edge · 🟡 Importante · ✅ Resolvido em 2026-05-10

**Hotkey de stance e tecla `V` processadas em ordem errada — V cicla antes de hotkey override**

**Problema:** Em §5.3 stub do `Update()`:

```csharp
// V key (existente)
if (UnityEngine.Input.GetKeyDown(_stanceToggleKeyConfig.Value))
    CurrentStance = GetNextStance(CurrentStance);

// Hotkeys F3
HandleStanceHotkeys();
```

V é checado **antes** das hotkeys. Cenário do corner case da spec funcional linha 185 ("Tecla dedicada = mesma tecla que `V`"): se o usuário configurar `Stance3Hotkey = V` (mesma tecla):

1. Frame N: usuário aperta `V`.
2. Bloco V: `CurrentStance = GetNextStance(Stance.Default) = Stance1` → `OnStanceChanged(Default → Stance1)` → som toca + stamina aplicada.
3. Bloco hotkeys: `Stance3Hotkey == V` matched → `CurrentStance = Stance3` → `OnStanceChanged(Stance1 → Stance3)` → som toca DE NOVO + stamina aplicada DE NOVO.

**Resultado:** 2 sons por aperto de tecla, 2x ApplyStaminaStance. Resolução final é Stance3 (hotkey ganha), mas com side-effects redundantes.

A spec funcional linha 185 diz "tecla dedicada tem prioridade, ou documentar claramente o conflito". Ter prioridade implica processar a hotkey **antes** ou em vez do V, não depois.

**Por que importa:** Default config (`V` para ciclo, `O` para Stance3) não dispara isso. Mas usuários customizam — qualquer overlap V↔hotkey gera double-fire de eventos. Bug latente até alguém configurar.

**Sugestão:** Inverter a ordem em `Update()` e adicionar early-return quando hotkey matcha:

```csharp
public static void Update()
{
    TryApplyPendingInitialStance();
    TryDispatchPendingResurrect();
    EvaluateSnapStaleTimeout();

    var gameWorld = GetCachedGameWorld();
    if (gameWorld?.MainPlayer?.IsSprintEnabled == true) return;

    // F3 hotkeys PRIMEIRO — se uma matchar, return (evita double-fire com V se V == hotkey)
    if (HandleStanceHotkeys()) return;

    // V key
    if (UnityEngine.Input.GetKeyDown(_stanceToggleKeyConfig.Value))
        CurrentStance = GetNextStance(CurrentStance);

    // Scroll wheel (sem alteração)
    ...
}

private static bool HandleStanceHotkeys()
{
    var gw = GetCachedGameWorld();
    if (gw?.MainPlayer == null) return false;
    if (gw.MainPlayer.IsSprintEnabled) return false;
    if (gw.MainPlayer.ProceduralWeaponAnimation?.IsAiming == true) return false;

    if (TryHotkey(Plugin._Stance0Hotkey, Stance.Default)) return true;
    if (TryHotkey(Plugin._Stance1Hotkey, Stance.Stance1)) return true;
    if (TryHotkey(Plugin._Stance2Hotkey, Stance.Stance2)) return true;
    if (TryHotkey(Plugin._Stance3Hotkey, Stance.Stance3)) return true;
    return false;
}
```

Adicionar AC explícita em §8.4: "Se Stance3Hotkey == StanceToggleHotkey == `V`, pressionar `V` ativa Stance 3 (hotkey prioridade). Som de stance change toca exatamente uma vez."

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (hotkeys antes de V + early-return + AC)
- `[ ]` Caminho alternativo: _________________

**Resolução:** §5.3 reordenado — `HandleStanceHotkeys()` agora retorna `bool` e é chamado **antes** do bloco de tecla `V` no `Update()`; se retornar `true`, early-return. AC adicionada em §8.4: "Stance3Hotkey == V → V ativa Stance 3, som de stance change toca uma vez."

---

### PA-04-03 · B — Edge · 🟢 Menor · ✅ Resolvido em 2026-05-10

**Double-tap rápido de fogo durante F4 reset pode causar 1 frame de stutter em fullauto**

**Problema:** Cenário (raro mas possível em combate frenético):

1. Frame N: button-down em Stance 1, snap intercept ativo.
2. Frame N+15 (~250ms hold): button-up → resurrect agendado.
3. Frame N+16: dispatch resurrect → synthetic true → fire 1 tiro. Reset agendado para N+17.
4. Frame N+16 (mesmo frame): jogador, com reflexo rápido, aperta fogo de novo. Prefix processa: CurrentStance já é Default (snapped no passo 1) → não intercepta → trigger=true natural propaga → fire continuous (fullauto).
5. Frame N+17: dispatch reset → synthetic false → operation.SetTriggerPressed(false) → fire para 1 frame.
6. Frame N+17 (mesmo frame): natural input system continua detectando button held → SetTriggerPressed(true) → fire retoma.

Resultado: para fullauto, a segunda rajada do double-tap tem **1 frame de gap (~16ms)** no fire continuous. Para semi/burst, irrelevante (operation já resetou via fire-end-event).

**Por que importa:** Double-tap em ~16ms de gap entre release e re-press é fisicamente difícil mas não impossível em pico de combate. O stutter é visualmente mínimo (1 frame) mas mensurável em ferramentas de teste.

Spec funcional não cobre — fora do escopo das ACs. Apenas qualidade.

**Sugestão:** Adicionar guard no dispatch do reset que checa se trigger está sendo pressionado naturalmente agora:

```csharp
// Em TryDispatchPendingResurrect, dentro do bloco de reset:
if (_pendingResetMethod != null)
{
    var resetInst = _pendingResetInstance;
    var resetMethod = _pendingResetMethod;
    _pendingResetInstance = null;
    _pendingResetMethod = null;
    if (!IsOperationStillCurrent(resetInst)) return;

    // PA-04-03: skip reset se IsTriggerPressed já é true por input natural
    // (jogador re-apertou). Sem isso, fullauto stutter 1 frame em double-tap.
    var fc = (Player.FirearmController)((gw = GetCachedGameWorld()) != null
        ? gw.MainPlayer?.HandsController
        : null);
    bool naturalPressed = fc?.IsTriggerPressed == true;
    if (naturalPressed)
    {
        Plugin.Logger.LogDebug("[F4] reset skipped: trigger pressed by natural input");
        return;
    }

    SnapFireTriggerPatch.RaiseSyntheticTrigger(resetInst, resetMethod, pressed: false);
}
```

`Player.FirearmController.IsTriggerPressed` é referenciada extensivamente no Assembly ([Player.cs:2714, 2937](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L2714)). Acesso direto via cast é seguro.

Alternativa: ignorar o stutter (irrelevante na prática) e apenas documentar.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (skip reset se natural pressed)
- `[ ]` Caminho alternativo (apenas documentar como comportamento aceitável): _________________

**Resolução:** §5.3 `TryDispatchPendingResurrect` recebe guard no bloco de reset: lê `fc.IsTriggerPressed` e pula o synthetic false se `true` (input natural já mantém o trigger). Evita stutter de 1 frame em fullauto durante double-tap.

---

### PA-04-04 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-05-10

**F5 `SpringGetPatch.ResetState()` em raid start interrompe transições em vôo (cenário teórico)**

**Problema:** §5.3 stub de `TryApplyPendingInitialStance` chama `SpringGetPatch.ResetState()` antes de `CurrentStance = target`. ResetState zera `_currentRotation`, `_currentPosition`, etc.

Em raid start (cenário normal), não há transição em vôo — o `SpringGetPatch` está num estado inicial limpo. Mas a chamada de `ResetState` é incondicional. Cenário teórico:

- Mod recarregado mid-raid via BepInEx hot-reload (cenário de dev). Estado do `SpringGetPatch` do antes pode ainda estar in-flight (`_currentRotation` no meio de uma transição).
- Plugin Awake → `OnGameStarted` postfix → `QueueInitialStance` → no Update seguinte, `TryApplyPendingInitialStance` → `ResetState` zera tudo → o jogador vê snap visual abrupto para Stance3.

**Por que importa:** Apenas em hot-reload de dev. Usuários finais nunca atingem. Zero impacto em produção.

**Sugestão:** Documentar como comportamento aceitável em §5.3 (comentário inline):

```csharp
private static void TryApplyPendingInitialStance()
{
    if (_pendingInitialStance == null) return;
    var gw = GetCachedGameWorld();
    if (gw?.MainPlayer?.ProceduralWeaponAnimation?.HandsContainer == null) return;
    if (gw.MainPlayer is HideoutPlayer) return; // F5 só em raid

    var target = _pendingInitialStance.Value;
    _pendingInitialStance = null;

    // Set imediato — bypass spring lerp.
    // Nota: ResetState pode interromper uma transição em vôo, mas só relevante em hot-reload
    // de dev (raid start normal não tem transição em vôo). Aceitável.
    SpringGetPatch.ResetState();
    CurrentStance = target;
}
```

Bonus: adicionar guard `is HideoutPlayer` (não está no stub atual mas é o pattern do mod em [StanceManager.cs:718](../../modded/StanceManager.cs#L718)).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (comentário inline + guard HideoutPlayer)
- `[ ]` Caminho alternativo: _________________

**Resolução:** §5.3 `TryApplyPendingInitialStance` ganha guard `if (gw.MainPlayer is HideoutPlayer) return;` (alinhado com `IsActiveContext` em [StanceManager.cs:718](../../modded/StanceManager.cs#L718)). Comentário inline explica que `ResetState()` só impacta hot-reload de dev.

---

### PA-04-05 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-05-10

**Order da `_MouseWheelModifierKey` muda de 59 para 58 mas é marcada "sem alteração" no delta**

**Problema:** Tabela "F12 layout após implementação" no fim da spec técnica:

```
| 60 | `Enable Mouse Wheel Stance Cycle`     | sem alteração |
| 59 | **`Mouse Wheel Scroll Mode`**         | [NOVO] |
| 58 | `Mouse Wheel Modifier Key`            | sem alteração |
```

Mas o código atual ([Plugin.cs:244](../../modded/Plugin.cs#L244)) tem `Mouse Wheel Modifier Key` em `Order = 59`. Mudar para 58 É uma alteração de Order, mesmo que o comportamento permaneça idêntico — o **valor armazenado** no `.cfg` do usuário não muda, mas a **ordem visual** no F12 muda.

A label "sem alteração" sugere que nada precisa mudar no bind. Implementador pode interpretar literalmente, deixar Order = 59 no bind, e topar com PA-04-01 (collision).

**Por que importa:** Inconsistência entre spec e código atual. Implementador segue a tabela e pode introduzir bug ou ignorar a discrepância.

**Sugestão:** Resolvido automaticamente se PA-04-01 for aceito (`Scroll Mode` vai para Order 58, `Modifier Key` permanece em 59 como "sem alteração" verdadeiro). Se PA-04-01 for caminho alternativo (manter Mode em 59), então mudar a label de `Modifier Key` para `[ALTERADO] Order 59 → 58` no delta.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (marcar correção pelo PA-04-01)
- `[ ]` Caminho alternativo: _________________

**Resolução:** Resolvido por consequência de PA-04-01 — `_MouseWheelModifierKey` permanece em Order 59 ("sem alteração" agora é literalmente verdade); `_MouseWheelScrollMode` ocupa o slot 58 deixado por `_UseOnlyStances`.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-05-10 | Review 04 criada — 0 bloqueadores, 2 importantes (Order collision + V/hotkey priority), 3 menores (double-tap stutter, F5 ResetState comment, Order label inconsistência). Após 3 reviews anteriores, spec está em estado avançado; estes pontos são polimento. |
| 2026-05-10 | Todas as 5 sugestões aceitas. Spec atualizada (§3, §5.2, §5.3, §8 e tabela F12). PA-04-05 fechado por consequência de PA-04-01. |
