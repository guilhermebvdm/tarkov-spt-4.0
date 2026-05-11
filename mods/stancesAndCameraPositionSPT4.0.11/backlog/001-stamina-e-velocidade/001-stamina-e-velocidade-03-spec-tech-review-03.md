# 001 — Stamina e Velocidade por Postura · Review Técnica 03

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec técnica revisada:** [001-stamina-e-velocidade-02-spec-tech.md](001-stamina-e-velocidade-02-spec-tech.md)
**Spec funcional referência:** [001-stamina-e-velocidade-01-spec.md](001-stamina-e-velocidade-01-spec.md)
**Reviews anteriores:** [01](001-stamina-e-velocidade-03-spec-tech-review-01.md) · [02](001-stamina-e-velocidade-03-spec-tech-review-02.md)
**Data:** 2026-05-08

> Análise crítica da spec técnica após resolução das PAs da review-02. Foco principal desta rodada: confrontar os stubs com a **estrutura real** do `StanceManager.cs` existente em `modded/`, que não foi inspecionado nas reviews anteriores.
>
> Skills aplicadas: `spt-mod-best-practices` + `csharp-mod-best-practices`.

## Resumo

> 🔴 Bloqueadores: 3 (✅ **3 resolvidos**) · 🟡 Importantes: 3 (✅ **3 resolvidos**) · 🟢 Menores: 2 (✅ **2 resolvidos**) · Total: **8 — todos resolvidos** em 2026-05-08
>
> ✅ **Status:** todos os 8 PAs aplicados na spec técnica. PA-03-05 foi **revertido para Opção B** após feedback adicional do usuário (HUD precisa atualizar continuamente). Pronto para rodar `/review-technical-spec` novamente.

## Reviews anteriores resolvidas

Todas as 8 PAs da review-02 confirmadas resolvidas na spec atual:

- ✅ PA-02-01 resolvido — Drain via buffer + `UpdateStamina`. Spec §1.1 e §5 (`TickStanceStamina`).
- ✅ PA-02-02 resolvido — `_currentStance` → property setter (mas ver PA-03-02 abaixo: a instrução está errada porque a property já existe).
- ✅ PA-02-03 resolvido — `_activeStaminaStance` separado.
- ✅ PA-02-04 resolvido — Refresh defensivo no tick.
- ✅ PA-02-05 resolvido — Stub apresentado como adições, não partial.
- ✅ PA-02-06 resolvido — `using Comfort.Common;` em todos os stubs + nota em §1 e §7.
- ✅ PA-02-07 resolvido — `!(... is X)` + checklist `LangVersion`.
- ✅ PA-02-08 resolvido — Comentário `public static new` + checklist.

## Índice de novos pontos

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| [PA-03-01](#pa-03-01) | C — Lógica | 🔴 | Modelo `int 0..3` da spec não bate com `enum Stance { Default, Stance1, Stance2, Stance3 }` real | ✅ Resolvido |
| [PA-03-02](#pa-03-02) | C — Lógica | 🔴 | Instrução "converter campo `_currentStance` em property" é incorreta — `CurrentStance` já é property | ✅ Resolvido |
| [PA-03-03](#pa-03-03) | C — Lógica | 🔴 | `AccessTools.Method(typeof(BaseLocalGame), "Stop")` pode não resolver — `Stop` tem 4 parâmetros, especificar tipos | ✅ Resolvido |
| [PA-03-04](#pa-03-04) | A — Gap | 🟡 | `ResetState()` existente em `StanceManager.cs:365` faz parte do cleanup que `OnRaidEnd` precisa fazer — duplicação ou divergência | ✅ Resolvido |
| [PA-03-05](#pa-03-05) | B — Edge | 🟡 | Drain via buffer flusha em saltos de 1f — em baixa intensidade (Stance 0 default, 0.50) HUD atualiza só a cada ~0.67s | ✅ Resolvido (**Opção B**) |
| [PA-03-06](#pa-03-06) | A — Gap | 🟡 | `MarkStanceValuesDirty()` / `MarkSprintEnabledDirty()` existentes — padrão de invalidação a reusar em vez de reinventar | ✅ Resolvido |
| [PA-03-07](#pa-03-07) | C — Lógica | 🟢 | Recovery postfix tem double-negação `!(!(... is HideoutPlayer))` — confuso e propenso a erro | ✅ Resolvido |
| [PA-03-08](#pa-03-08) | B — Edge | 🟢 | `EvaluateProneSuspensionTick` faz Remove+Add todo frame mesmo quando MaxSpeed/fraction não mudaram | ✅ Resolvido |

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

### PA-03-01 · C — Lógica · 🔴 Bloqueador {#pa-03-01}

**Modelo `int 0..3` da spec não bate com `enum Stance { Default, Stance1, Stance2, Stance3 }` real**

**Problema:** A spec técnica modela stances como `int 0..3`:

```csharp
// Plugin.cs (stub spec)
public static readonly Dictionary<int, StanceConfig> _stanceConfigs = new(4);

// StanceManager.cs (stub spec)
private static int _activeStaminaStance = -1;
public static int GetActiveStaminaStance() => _activeStaminaStance;
public static void OnStanceChanged(int previousStance, int newStance) { ... }
public static void ApplyStaminaStance(int stance) { ... }
```

Mas o código real ([modded/StanceManager.cs:9, 22](../../modded/StanceManager.cs)) usa `enum Stance`:

```csharp
public enum Stance
{
    Default,
    Stance1,
    Stance2,
    Stance3,
}

public static Stance CurrentStance { get; private set; } = Stance.Default;
```

E todas as transições internas usam o enum:

```csharp
// modded/StanceManager.cs:140-144
Stance.Default => Stance.Stance1,
Stance.Stance1 => Stance.Stance2,
// ...
```

**Por que importa:** o build seguindo o stub vai criar `Dictionary<int, StanceConfig>` paralelo ao `enum Stance` do código existente, gerando dois mapeamentos diferentes para a mesma stance — fonte de bugs (`int 0` vs `Stance.Default`, `int 1` vs `Stance.Stance1`...). Cada chamada precisaria de cast explícito (`(int)Stance.Stance1`), e a integridade depende de a ordem do enum nunca mudar.

**Sugestão:** Reescrever os stubs usando o **enum real**:

```csharp
// Plugin.cs:
public static readonly Dictionary<Stance, StanceConfig> _stanceConfigs = new(4);

private static readonly (Stance Stance, string Section, EStanceStaminaMode Mode, float Intensity, bool ModSpeed, int Multiplier, bool ApplyProne)[]
    _stanceDefaults =
{
    (Stance.Default, "Stance 0", EStanceStaminaMode.Drain,    0.50f, true,  90,  false),
    (Stance.Stance1, "Stance 1", EStanceStaminaMode.Recovery, 2.00f, true,  100, false),
    (Stance.Stance2, "Stance 2", EStanceStaminaMode.None,     1.00f, false, 100, false),
    (Stance.Stance3, "Stance 3", EStanceStaminaMode.Recovery, 1.50f, true,  95,  false),
};

// StanceManager.cs (adições):
private static Stance _activeStaminaStance = Stance.Default;
public static Stance GetActiveStaminaStance() => _activeStaminaStance;
public static void OnStanceChanged(Stance previousStance, Stance newStance) { ... }
public static void ApplyStaminaStance(Stance stance) { ... }
```

Mapeamento Stance ↔ texto F12 fica explícito na tupla `_stanceDefaults`. **Stance 0** do nosso backlog = `Stance.Default` do enum existente. A seção F12 continua sendo "Stance 0" (texto livre), mas a chave interna é o enum.

Atualizar todas as referências em §3, §5, §6 e §8 da spec técnica. Adicionar à §1 nota: "**Modelo de stance:** usamos o `enum Stance` já definido em `modded/StanceManager.cs:9` (`Default`/`Stance1`/`Stance2`/`Stance3`). 'Stance 0' no F12 e tabelas refere-se a `Stance.Default`."

**Decisão:** `[x]` **Aceitar sugestão** · ✅ Resolvido em 2026-05-08
**Resolução:** `Plugin._stanceConfigs` agora é `Dictionary<Stance, StanceConfig>`. `_stanceDefaults` é uma tupla com `Stance.Default/Stance1/Stance2/Stance3`. Métodos `OnStanceChanged(Stance, Stance)` / `ApplyStaminaStance(Stance)` / `GetActiveStaminaStance() → Stance`. Subseção [§1 "Modelo de stance"](001-stamina-e-velocidade-02-spec-tech.md#1-estratégia) explicita que "Stance 0" no F12 = `Stance.Default` interno.

---

### PA-03-02 · C — Lógica · 🔴 Bloqueador {#pa-03-02}

**Instrução "converter campo `_currentStance` em property" é incorreta — `CurrentStance` já é property**

**Problema:** A spec técnica em §4 e §5 instrui:

> Converter o campo `_currentStance` existente em **property com setter** que dispara `OnStanceChanged(prev, new)`.

```csharp
// ANTES:
private static int _currentStance = 0;

// DEPOIS:
private static int _currentStanceField = 0;
public static int CurrentStance { get => _currentStanceField; set { ... } }
```

Mas o código real ([modded/StanceManager.cs:22](../../modded/StanceManager.cs#L22)) **já tem** uma property:

```csharp
public static Stance CurrentStance { get; private set; } = Stance.Default;
```

E a mutação acontece em 3 sítios internos ([linhas 94, 111, 116](../../modded/StanceManager.cs#L94)):

```csharp
CurrentStance = GetNextStance(CurrentStance);   // tecla
CurrentStance = GetNextStance(CurrentStance);   // scroll up
CurrentStance = GetPreviousStance(CurrentStance); // scroll down
```

**Por que importa:** o desenvolvedor seguindo o stub literal vai criar uma property `int CurrentStance` paralela à `Stance CurrentStance` existente — colisão de nomes ou shadow, dependendo da visibilidade. Bug óbvio.

**Sugestão:** Reescrever a instrução em §4 e §5 para refletir a estrutura real:

> **Modificar a property `CurrentStance` existente em `modded/StanceManager.cs:22`** para detectar mudança e disparar `OnStanceChanged`. Como o setter atual é `private set`, há duas opções:
>
> **Opção A — converter para setter customizado:**
> ```csharp
> private static Stance _currentStanceField = Stance.Default;
> public static Stance CurrentStance
> {
>     get => _currentStanceField;
>     private set
>     {
>         if (value == _currentStanceField) return;
>         var prev = _currentStanceField;
>         _currentStanceField = value;
>         OnStanceChanged(prev, value);
>     }
> }
> ```
> O `private set` é mantido — os 3 call-sites internos não mudam. Apenas a property ganha lógica.
>
> **Opção B — wrapper method:**
> ```csharp
> private static void SetCurrentStance(Stance value)
> {
>     if (value == CurrentStance) return;
>     var prev = CurrentStance;
>     CurrentStance = value;       // private set ainda
>     OnStanceChanged(prev, value);
> }
> ```
> E substituir os 3 sítios `CurrentStance = X` por `SetCurrentStance(X)`.
>
> Recomendo **Opção A** — menos invasivo, lógica fica dentro da própria property.

**Decisão:** `[x]` **Aceitar sugestão (Opção A — setter customizado)** · ✅ Resolvido em 2026-05-08
**Resolução:** A property `CurrentStance` em [`StanceManager.cs:22`](../../modded/StanceManager.cs#L22) é modificada para detectar mudança e disparar `OnStanceChanged(prev, value)`. `private set` mantido — os 3 call-sites internos (linhas 94/111/116) não precisam mudar. Documentado em [§1 "Wiring da troca de stance"](001-stamina-e-velocidade-02-spec-tech.md#1-estratégia) e nos checklists de §4 e §8.

---

### PA-03-03 · C — Lógica · 🔴 Bloqueador {#pa-03-03}

**`AccessTools.Method(typeof(BaseLocalGame), nameof(BaseLocalGame.Stop))` pode falhar — método tem 4 parâmetros e potenciais overloads**

**Problema:** O stub do `BaseLocalGameStopPatch` em §5 faz:

```csharp
=> AccessTools.Method(typeof(BaseLocalGame), nameof(BaseLocalGame.Stop));
```

Mas [BaseLocalGame.cs:1018](../../../../references/eft-decompiled/Assembly-CSharp/EFT/BaseLocalGame.cs#L1018) declara:

```csharp
public virtual void Stop(string profileId, ExitStatus exitStatus, string exitName, float delay = 0f)
```

`AccessTools.Method` por nome simples retorna o **primeiro** match. Se houver overloads (a classe-base `AbstractGame` ou outras podem declarar `Stop` com assinaturas diferentes — não verifiquei), o resolver pode pegar o método errado, ou retornar `null` em caso ambíguo. Resultado: patch silenciosamente não registra (Harmony loga warning), `OnRaidEnd` nunca dispara via esse caminho, leak entre raids.

**Por que importa:** a redundância entre `GameWorld.OnDestroy` e `BaseLocalGame.Stop` é justamente para cobrir os 3 caminhos de saída (`Left`/`Killed`/`MIA`). Se um deles não registrar, perdemos parte da cobertura — AC funcional "Cleanup em todas as saídas de raid: Left/Killed/MIA testados pelo menos uma vez" pode falhar silenciosamente.

**Sugestão:** Especificar tipos dos parâmetros no `AccessTools.Method`:

```csharp
=> AccessTools.Method(
    typeof(BaseLocalGame),
    nameof(BaseLocalGame.Stop),
    new[] { typeof(string), typeof(ExitStatus), typeof(string), typeof(float) });
```

Confirmar o tipo do `ExitStatus` no Assembly (provavelmente `EFT.ExitStatus` ou similar — `using EFT;` já importa). Aplicar à seção "Stubs de código" do `RaidLifecyclePatches.cs`.

Adicionalmente, em §8 (Checklist), incluir tarefa: "Confirmar que `Stop`/`OnGameStarted`/`OnDestroy` são resolvidos corretamente por `AccessTools.Method` — logar `Plugin.Logger.LogInfo` no `Awake` se algum deles retornar null."

**Decisão:** `[x]` **Aceitar sugestão** · ✅ Resolvido em 2026-05-08
**Resolução:** Stub do `BaseLocalGameStopPatch.GetTargetMethod` em [§5](001-stamina-e-velocidade-02-spec-tech.md#5-stubs-de-código) agora usa `AccessTools.Method(typeof(BaseLocalGame), nameof(BaseLocalGame.Stop), new[] { typeof(string), typeof(ExitStatus), typeof(string), typeof(float) })`. Item adicionado ao checklist [§8](001-stamina-e-velocidade-02-spec-tech.md#8-checklist-de-implementação): "Logar no `Awake` se algum `MethodInfo` resolver para null".

---

### PA-03-04 · A — Gap · 🟡 Importante {#pa-03-04}

**`ResetState()` existente em `StanceManager.cs:365` sobrepõe parte do cleanup que `OnRaidEnd` precisa fazer**

**Problema:** O `StanceManager` existente já tem [`ResetState()` (linha 365)](../../modded/StanceManager.cs#L365):

```csharp
public static void ResetState()
{
    CurrentStance = Stance.Default;
    _isTacSprintActive = false;
    _wasAiming = false;
    // ... outros reset
    _stanceValuesDirty = true;
    _sprintEnabledDirty = true;
    // ...
}
```

A spec técnica define `OnRaidEnd()` que faz `StanceStaminaState.Reset()` + `RemoveStateSpeedLimit` + `_activeStaminaStance = Stance.Default` (após PA-03-01) — mas **não chama `ResetState()`**. Resultado: nosso `OnRaidEnd` deixa caches do mod existente (`_isTacSprintActive`, `_wasAiming`, `_stanceValuesDirty`...) sem reset. A próxima raid herda esses estados.

Também: o código existente provavelmente já chama `ResetState()` em algum hook de raid existente (hipoteticamente). Se sim, estamos duplicando trabalho. Se não, há um leak pré-existente que vale documentar.

**Por que importa:** consistência. Ou nosso `OnRaidEnd` chama `ResetState()` (cobre o existente também), ou a spec deixa explícito que esse cleanup é opcional / responsabilidade do código existente. Sem essa decisão, build pode esquecer.

**Sugestão:** Em §5 (StanceManager additions), modificar o stub de `OnRaidEnd`:

```csharp
public static void OnRaidEnd()
{
    if (_raidEnded) return;
    _raidEnded = true;
    try
    {
        var mc = Singleton<GameWorld>.Instance?.MainPlayer?.MovementContext;
        mc?.RemoveStateSpeedLimit(Plugin.StanceSpeedLimitCause);

        StanceStaminaState.Reset();
        _activeStaminaStance = Stance.Default;

        // Também resetar estado existente do mod (CurrentStance, tac sprint, caches).
        // ResetState() já existe em StanceManager.cs:365 e faz tudo isso.
        ResetState();

        Plugin.Logger.LogInfo("[StanceManager] Raid end — state cleaned");
    }
    catch (Exception ex) { Plugin.Logger.LogError($"[StanceManager.OnRaidEnd] {ex}"); }
}
```

Adicionar à §1: "Reuso do `ResetState()` existente — `OnRaidEnd` chama ResetState() para garantir cleanup consolidado (sem duplicar lógica)."

**Decisão:** `[x]` **Aceitar sugestão** · ✅ Resolvido em 2026-05-08
**Resolução:** `OnRaidEnd` agora chama `ResetState()` ao final, consolidando o cleanup do nosso backlog com o cleanup pré-existente do mod (`CurrentStance`, tac sprint, dirty-flags de offsets). Subseção [§1 "Reuso do `ResetState()` existente"](001-stamina-e-velocidade-02-spec-tech.md#1-estratégia) explicita a integração.

---

### PA-03-05 · B — Edge · 🟡 Importante {#pa-03-05}

**Drain via buffer flusha em saltos de 1f — em baixa intensidade HUD atualiza esporádico**

**Problema:** O caminho B aprovado em PA-02-01 acumula drain frame-a-frame e flusha via `UpdateStamina(target)` quando `AccumulatedDrain ≥ 1f`. Para o **default Stance 0 (Drain, Intensity 0.50)**:

- Drain por segundo = `AimDrainRate × Intensity = 3 × 0.50 = 1.5/s`.
- Tempo até 1f de buffer = `1 / 1.5 ≈ 0.67s`.

A barra de stamina das mãos atualiza em **saltos de 1f a cada ~0.67s**, não suavemente. Ainda pior em `Intensity = 0.25` (custom): `0.75/s` → atualização a cada 1.34s.

**Por que importa:** AC funcional "Stance 0 + Intensity = 1.0 produz drain cronometrável próximo de 3/s" — cronometrável sim, mas visualmente choppy. Não é falha funcional, mas UX ruim.

**Sugestão:** Duas opções:

**Opção A — flush a cada N frames se buffer > 0:** força flush quando buffer ≥ 0.5f mesmo abaixo de 1f, usando `UpdateStamina` numa janela menor. Risco: se `UpdateStamina` recusar < 1f delta (linha 392 do GClass774), no-op. Não funciona — descartar.

**Opção B — manter mutação direta de `Current` no tick e disparar `InvokeChangedAction()` manualmente:**

```csharp
public static void TickStanceStamina()
{
    // ... guards ...

    float drain = baseRate * Intensity * hands.Multiplier * Time.deltaTime;
    if (!float.IsFinite(drain) || drain <= 0f) return;

    float prev = hands.Current;
    float target = Mathf.Max(0f, prev - drain);
    if (Mathf.Abs(prev - target) < float.Epsilon) return;

    hands.Current = target;

    // Replicar manualmente os eventos críticos que UpdateStamina dispara:
    InvokeChangedActionViaReflection(hands);   // helper que chama action_3 + InvokeChangedAction
    if ((target < 15f) ^ (prev < 15f))
        InvokeThresholdActionViaReflection(hands);   // action_1 do GClass774
}
```

Reflection cacheada em `static readonly FieldInfo` no Awake. Side-effects ficam preservados (HUD/sons), mas drain é suave por frame em vez de chunky por buffer. Mais complexo mas resolve o UX.

**Opção C — aceitar o chunky e documentar:** para Drain a chunkness é aceitável — afinal, drain só rola em hipfire e o jogador raramente foca na barra de stamina das mãos lá. Adicionar nota em §1: "Drain é flushado em janelas de 1f para ativar os eventos do `GClass774`. Em baixa intensidade, atualizações de HUD são esporádicas — comportamento aceito."

**Recomendo Opção C** (aceitar) — Opção B é hack frágil que depende de reflection nos `action_*` privados, e o ganho de UX é marginal. Mas vale registrar a decisão.

**Decisão:** `[x]` **Aceitar sugestão (Opção B — reflection nos `action_*`)** · ✅ Resolvido em 2026-05-08
**Resolução (revertida da Opção C inicialmente recomendada):** após feedback do usuário "É importante ter isso na HUD", o caminho B foi escolhido. `TickStanceStamina` agora muta `hands.Current` direto por frame (drain suave) e dispara manualmente:
- `action_3.Invoke()` — sinal "stamina mudou" que a HUD escuta
- `InvokeChangedAction()` — propaga para listeners gerais
- `action_1.Invoke()` quando cruza threshold de 15f — som "tired"

`FieldInfo` e `MethodInfo` são cacheados em `static readonly` no `StanceManager`. Custo: 1 invoke/frame quando há drain ativo. Subseção [§1 "Granularidade do drain"](001-stamina-e-velocidade-02-spec-tech.md#1-estratégia) atualizada. Item adicionado ao checklist e nota em [§7](001-stamina-e-velocidade-02-spec-tech.md#7-riscos-e-dependências) sobre logar warning se `AccessTools.Field` retornar null (BSG renomeando esses campos privados em update futuro).

---

### PA-03-06 · A — Gap · 🟡 Importante {#pa-03-06}

**`MarkStanceValuesDirty()` / `MarkSprintEnabledDirty()` existentes — padrão de invalidação a reusar**

**Problema:** O `StanceManager` existente já tem padrão de invalidação por flags ([linhas 70, 75](../../modded/StanceManager.cs#L70)):

```csharp
public static void MarkStanceValuesDirty() => _stanceValuesDirty = true;
public static void MarkSprintEnabledDirty() => _sprintEnabledDirty = true;
```

E o [Plugin.cs:712-751](../../modded/Plugin.cs#L712-L751) já registra ~30 `SettingChanged += MarkStanceValuesDirty` para os offsets existentes. O padrão é: handler seta uma flag, e a próxima leitura no tick percebe e recalcula.

A spec técnica reinventa esse padrão criando `OnStanceConfigChanged` que **chama `ApplyStaminaStance(active)` direto**. Funciona, mas duplica conceito.

**Por que importa:** consistência arquitetural. Outros desenvolvedores lendo o código verão dois padrões diferentes para a mesma coisa. Também: chamar `ApplyStaminaStance` direto do `SettingChanged` event handler pode ter side-effects reentrantes se ele tocar em config (raro, mas possível).

**Sugestão:** Reusar o padrão existente:

```csharp
// Plugin.cs (em vez de OnStanceConfigChanged → ApplyStaminaStance):
private static void OnStanceConfigChanged(object sender, EventArgs e)
{
    StanceManager.MarkStaminaConfigDirty();
}

// StanceManager.cs (adicionar):
private static bool _staminaConfigDirty = true;
public static void MarkStaminaConfigDirty() => _staminaConfigDirty = true;

public static void TickStanceStamina()
{
    if (_staminaConfigDirty) {
        ApplyStaminaStance(_activeStaminaStance);   // re-cache
        _staminaConfigDirty = false;
    }
    // ... resto do tick ...
}
```

Atualizar §5 e §1 mencionando a integração com o padrão dirty-flag existente.

Alternativa: manter o caminho direto da spec atual e justificar em §7 (Riscos) por que diverge — válido se a justificativa for a latência (ApplyStaminaStance roda em ≤ 1 frame, dirty-flag pode levar até 1 tick). Mas o ganho é insignificante (16ms a 60 FPS) e o custo arquitetural não compensa.

**Decisão:** `[x]` **Aceitar sugestão (reusar dirty-flag)** · ✅ Resolvido em 2026-05-08
**Resolução:** Adicionados `_staminaConfigDirty` e `MarkStaminaConfigDirty()` no `StanceManager`. `OnStanceConfigChanged` no Plugin agora chama `MarkStaminaConfigDirty()` (não `ApplyStaminaStance` direto). `TickStanceStamina` checa a flag no início e re-aplica config uma vez quando suja. Padrão coerente com `MarkStanceValuesDirty`/`MarkSprintEnabledDirty` existentes.

---

### PA-03-07 · C — Lógica · 🟢 Menor {#pa-03-07}

**Recovery postfix tem double-negação `!(!(... is HideoutPlayer))` — confuso**

**Problema:** Stub do `StanceStaminaRecoveryPatch` em §5:

```csharp
if (gw?.MainPlayer == null) return;
if (!(!(gw.MainPlayer is HideoutPlayer))) return;       // hideout — feature inerte
```

A double-negação `!(!(X))` é equivalente a `X`. A linha quer dizer "return se está em hideout", então deveria ser `if (gw.MainPlayer is HideoutPlayer) return;`. Aparentemente o autor (eu) estava aplicando PA-02-07 (`!(... is X)` em vez de `is not X`) mas mecanicamente — o `is not` original era para o caminho positivo (`return !(... is X)` = "return when not in hideout") em `IsActiveContext`, que está correto. Aqui a semântica é oposta.

**Por que importa:** legibilidade. Confunde quem lê. E o teste é error-prone — fácil errar uma negação a mais ou a menos. Build atual provavelmente compila e funciona, mas o próximo dev pode introduzir bug ao "simplificar".

**Sugestão:** Trocar a linha por:

```csharp
if (gw.MainPlayer is HideoutPlayer) return;       // hideout — feature inerte
```

Não tem ambiguidade, não usa pattern matching complexo, e é igual ao padrão usado em `IsActiveContext` (`return !(gw.MainPlayer is HideoutPlayer);` que é "returns true se não-hideout").

**Decisão:** `[x]` **Aceitar sugestão** · ✅ Resolvido em 2026-05-08
**Resolução:** Stub do `StanceStaminaRecoveryPatch` em [§5](001-stamina-e-velocidade-02-spec-tech.md#5-stubs-de-código) agora usa `if (gw.MainPlayer is HideoutPlayer) return;` direto, sem double-negação.

---

### PA-03-08 · B — Edge · 🟢 Menor {#pa-03-08}

**`EvaluateProneSuspensionTick` faz Remove+Add todo frame mesmo sem mudança**

**Problema:** Stub atual de `EvaluateProneSuspensionTick`:

```csharp
if (cfg.ModifiesMovementSpeed.Value && !StanceStaminaState.IsSuspendedByProne)
{
    var mc = player.MovementContext;
    float fraction = cfg.MovementSpeedMultiplier.Value / 100f;
    mc.RemoveStateSpeedLimit(Plugin.StanceSpeedLimitCause);
    mc.AddStateSpeedLimit(fraction * mc.MaxSpeed, Plugin.StanceSpeedLimitCause);
}
```

Roda **todo frame** em raid quando há stance ativa que modifica velocidade. Faz Remove+Add num `Dictionary<,>` mesmo quando `fraction × mc.MaxSpeed` não mudou (que é o caso comum — `MaxSpeed` muda raramente; só quando skill Strength sobe). Cada Remove+Add invoca `method_5()` (recalcula StateSpeedLimit), que dispara `OnCharacterControllerSpeedLimitChanged` event.

**Por que importa:** performance pequena (Dictionary ops são O(1)), mas **eventos disparados a 60 Hz** podem ser ouvidos por outros sistemas — UI, HUD, animação — que não precisam recalcular. Pelo skill `csharp-mod-best-practices` §1: "no allocations in hot paths" e §6: "events ... weak-event patterns ou strict subscribe/unsubscribe". Disparar eventos em vão é desperdício.

**Sugestão:** Cachear o último valor aplicado e só re-aplicar se mudou:

```csharp
private static float _lastAppliedSpeedLimit = -1f;

// Dentro do tick:
if (cfg.ModifiesMovementSpeed.Value && !StanceStaminaState.IsSuspendedByProne)
{
    var mc = player.MovementContext;
    float target = (cfg.MovementSpeedMultiplier.Value / 100f) * mc.MaxSpeed;
    // Tolerância para float — evita re-apply por flutuação numérica
    if (Mathf.Abs(target - _lastAppliedSpeedLimit) > 0.001f)
    {
        mc.RemoveStateSpeedLimit(Plugin.StanceSpeedLimitCause);
        mc.AddStateSpeedLimit(target, Plugin.StanceSpeedLimitCause);
        _lastAppliedSpeedLimit = target;
    }
}
```

Resetar `_lastAppliedSpeedLimit = -1f` em `OnRaidStart`/`OnRaidEnd`/`ApplyStaminaStance` para forçar re-aplicação quando a configuração muda.

**Decisão:** `[x]` **Aceitar sugestão** · ✅ Resolvido em 2026-05-08
**Resolução:** Adicionado `_lastAppliedSpeedLimit` static field no `StanceManager`. `EvaluateProneSuspensionTick` calcula `target` e só faz `Remove+Add` quando `Mathf.Abs(target − _lastAppliedSpeedLimit) > 0.001f`. Reset para `-1f` em `OnRaidStart`/`OnRaidEnd`/`ApplyStaminaStance` força re-apply na próxima tick. Evita disparar `OnCharacterControllerSpeedLimitChanged` a 60 Hz desnecessariamente.

---

## Próximos passos

✅ **Todas as 8 PAs aplicadas em 2026-05-08.** PA-03-05 foi revertido para Opção B (drain por frame com reflection nos eventos privados) após feedback do usuário sobre necessidade de HUD continuamente atualizada. Próximo:

1. Rodar `/review-technical-spec` novamente — gera `technical-review-04.md` validando os fechamentos e levantando o que possa surgir do uso de reflection sobre `action_3`/`action_1` privados.
2. Se a review-04 vier sem 🔴, executar `/build-item`.
