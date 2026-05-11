# 002 — Ciclo linear, hotkeys e snap fogo · Code Review 01

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec funcional:** [002-ciclo-linear-hotkeys-snap-fogo-01-spec.md](002-ciclo-linear-hotkeys-snap-fogo-01-spec.md)
**Spec técnica:** [002-ciclo-linear-hotkeys-snap-fogo-02-spec-tech.md](002-ciclo-linear-hotkeys-snap-fogo-02-spec-tech.md)
**Asbuild:** ⚠️ Ausente — item 002 entregue antes do `/code-mod` passar a gerar `05-asbuild.md`. Análise usou §4 da spec técnica como fonte canônica dos arquivos tocados (fallback (b) da pré-condição #2 do `/code-review`).
**Data:** 2026-05-10

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 6 · Total: 6

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | B — Bug latente | 🟠 | F4 snap dispara em fogo de OUTROS players em multiplayer (Fika) | ✅ Aplicado |
| CR-01-02 | B — Bug latente | 🟡 | Weapon swap mid-intercept dentro do threshold pode disparar tiro espúrio na nova arma | ✅ Aplicado |
| CR-01-03 | D — Arquitetura | 🟢 | `TryInterceptTriggerDown` ignora o parâmetro `firearmControllerInstance` | ✅ Aplicado |
| CR-01-04 | D — Arquitetura | 🟢 | `IsHoldingFirearm()` re-resolve o `HandsController` que o patch caller já tem | ✅ Aplicado |
| CR-01-05 | E — Legibilidade | 🟢 | `StanceConfig.SnapToStance0OnFire` declarado como não-nullable mas recebe `null` para Stance 0 | ✅ Aplicado |
| CR-01-06 | F — Melhoria opcional | 🟢 | `SnapStaleTimeoutSec = 2f` hard-coded — útil expor como Advanced para troubleshooting | ✅ Aplicado |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Pontos

### CR-01-01 · B — Bug latente · 🟠 Forte · ✅ Aplicado em 2026-05-10

**F4 snap dispara em fogo de OUTROS players em multiplayer (Fika)**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded/Patches/SnapFireTriggerPatch.cs:67-92`](../../modded/Patches/SnapFireTriggerPatch.cs#L67)

**Problema:** O Prefix do `SnapFireTriggerPatch` resolve a `FirearmController_0` via reflection da operation, e segue se `fc != null`:

```csharp
var fc = _cachedFcField?.GetValue(__instance) as Player.FirearmController;
if (fc == null) return true; // não é firearm — deixa passar (defensivo).

if (pressed)
{
    if (StanceManager.TryInterceptTriggerDown(fc))
        return false;
    return true;
}
```

Não há check de que **`fc` é a `FirearmController` do `MainPlayer`**. O usuário tem [Fika](D:\SPT\BepInEx\plugins\Fika) instalado (multiplayer). Em raid Fika, outros players têm seus próprios `Player.FirearmController`. Quando um remote player atira, a sincronização chama `SetTriggerPressed` na operation do controller remoto. Nosso Prefix dispara, `fc != null` (é um `Player.FirearmController`, do remote), e chama `TryInterceptTriggerDown(fc)` — que opera sobre `gw.MainPlayer.HandsController` e `CurrentStance` (estado do **local player**).

Resultado: cada tiro de outro jogador em raid Fika pode acionar snap da stance do jogador local + bloqueio do trigger do remote (que volta como dessincronia de input no Fika).

**Por que importa:** Modo multiplayer fica quebrado. Usuário relataria como "minha stance muda sozinha quando vejo gente atirando". Spec funcional não menciona multiplayer explicitamente, mas o usuário tem Fika instalado — é cenário real do ambiente.

**Sugestão:** Adicionar guard no Prefix verificando que `fc` é a do MainPlayer:

```csharp
var fc = _cachedFcField?.GetValue(__instance) as Player.FirearmController;
if (fc == null) return true;

// CR-01-01: filtrar para apenas MainPlayer — em multiplayer (Fika), outros players
// também passam por este patch, mas snap só faz sentido no local player.
var mainPlayer = Comfort.Common.Singleton<EFT.GameWorld>.Instance?.MainPlayer;
if (mainPlayer == null || fc != mainPlayer.HandsController) return true;

// ... resto do código ...
```

Reuso de `StanceManager.GetCachedGameWorld()` é possível mas implicaria expor o método público; pode-se justificar a chamada direta de `Singleton<GameWorld>.Instance` aqui por ser hot-path do input local.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (guard `fc == MainPlayer.HandsController`)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `Patches/SnapFireTriggerPatch.cs` — adicionado `using Comfort.Common;` + guard `var mainPlayer = Singleton<GameWorld>.Instance?.MainPlayer; if (mainPlayer == null || fc != mainPlayer.HandsController) return true;` antes da bifurcação `if (pressed)`. Comentário inline `// ref: CR-01-01` explica o motivo (Fika).

---

### CR-01-02 · B — Bug latente · 🟡 Médio · ✅ Aplicado em 2026-05-10

**Weapon swap mid-intercept dentro do threshold pode disparar tiro espúrio na nova arma**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded/StanceManager.cs:281-295`](../../modded/StanceManager.cs#L281)

**Problema:** Cenário:

1. Jogador em Stance 1, weapon A equipado.
2. Button-down → `TryInterceptTriggerDown` → `_snapInterceptActive = true`, snap, `_triggerDownTimeUnscaled = T`.
3. Sem button-up natural, jogador troca para weapon B (operation A é destruída, operation B vira CurrentOperation).
4. Em B, jogador faz button-up (≥ threshold mas < 2s).
5. `OnTriggerUpAfterIntercept(operationB, …)` é chamado pelo Prefix.
6. `_snapInterceptActive` ainda é `true` (stale-timeout só fira a >2s). `elapsedMs >= threshold`.
7. `_pendingResurrectInstance = operationB; _pendingResurrectMethod = …`.
8. Frame N+1: `TryDispatchPendingResurrect` valida `IsOperationStillCurrent(operationB)` → true (B é current). Dispara **synthetic true em B**.
9. Frame N+2: synthetic false em B.

Resultado: jogador troca de arma, aperta + solta em ≥ 200ms, e a **nova arma dispara 1 tiro inesperado** com snap aplicado.

A spec funcional corner-case linha 183 ("Troca de arma em stance não-padrão … não deve causar snap residual") pede zero residual. O stale-timeout cobre o caso de hold > 2s, mas não o caso de swap + nova interação rápida.

**Por que importa:** Cenário possível em combate (rapid weapon swap + fire). Sintoma: "atirou sem querer" na primeira interação após swap.

**Sugestão:** Validar em `OnTriggerUpAfterIntercept` que o `operationInstance` recebido é o mesmo da operation onde o intercept começou. Cachear o operation do intercept no down:

```csharp
// novo campo:
private static object _interceptOperationInstance;

public static bool TryInterceptTriggerDown(object firearmControllerInstance, object operationInstance)
{
    // ... guards ...
    _interceptOperationInstance = operationInstance;  // novo
    _triggerDownTimeUnscaled = Time.unscaledTime;
    _snapInterceptActive = true;
    return true;
}

public static void OnTriggerUpAfterIntercept(object operationInstance, MethodBase originalMethod)
{
    if (!_snapInterceptActive) return;
    if (operationInstance != _interceptOperationInstance)
    {
        // CR-01-02: operation mudou entre down e up — abortar sem agendar.
        _snapInterceptActive = false;
        _triggerDownTimeUnscaled = SnapIdleSentinel;
        _interceptOperationInstance = null;
        return;
    }
    // ... resto ...
}
```

Atualizar `SnapFireTriggerPatch.Prefix` para passar `__instance` (a operation) para `TryInterceptTriggerDown`. Limpar `_interceptOperationInstance` em `ResetState()` também.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (validar operation entre down e up)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `StanceManager.cs` — adicionado campo `_interceptOperationInstance` (cacheado no `TryInterceptTriggerDown`); `OnTriggerUpAfterIntercept` checa `if (operationInstance != _interceptOperationInstance)` e drop com log debug. `ResetState` e `EvaluateSnapStaleTimeout` também limpam o campo. Comentários inline `// ref: CR-01-02` nos pontos chave.

---

### CR-01-03 · D — Arquitetura · 🟢 Menor · ✅ Aplicado em 2026-05-10

**`TryInterceptTriggerDown` ignora o parâmetro `firearmControllerInstance`**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded/StanceManager.cs:257-275`](../../modded/StanceManager.cs#L257)

**Problema:** A assinatura recebe `object firearmControllerInstance`, mas o método **nunca usa o parâmetro**:

```csharp
public static bool TryInterceptTriggerDown(object firearmControllerInstance)
{
    var gw = GetCachedGameWorld();
    if (gw?.MainPlayer == null) return false;
    if (gw.MainPlayer.ProceduralWeaponAnimation?.IsAiming == true) return false;
    if (CurrentStance == Stance.Default) return false;
    if (!Plugin._stanceConfigs.TryGetValue(CurrentStance, out var cfg)) return false;
    if (cfg.SnapToStance0OnFire == null) return false;
    if (!cfg.SnapToStance0OnFire.Value) return false;
    if (!IsHoldingFirearm()) return false;
    // ...
}
```

Todas as verificações usam `MainPlayer.HandsController` indireto (via `IsHoldingFirearm`), enquanto o caller (`SnapFireTriggerPatch.Prefix`) já tem o `Player.FirearmController` resolvido como `fc`. Há lint warning de parâmetro não-usado + re-query desnecessária por chamada.

**Por que importa:** Code smell, custo trivial. Se PA-01-01 (sugestão do CR-01-01) for aceito e introduzir guard via MainPlayer no patch, este parâmetro fica ainda mais redundante. Se CR-01-02 for aceito, parâmetro deve mudar para `object operationInstance` (a operation), não `firearmControllerInstance`.

**Sugestão:** Remover o parâmetro, OU substituí-lo por `object operationInstance` se CR-01-02 for aceito (que precisa do operation). Em ambos os casos, atualizar caller no `SnapFireTriggerPatch.Prefix`.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (remover ou trocar por operationInstance — alinhado com CR-01-02)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto — caminho "trocar por operationInstance".
**Aplicação:** `StanceManager.cs` — assinatura `TryInterceptTriggerDown(object firearmControllerInstance)` → `TryInterceptTriggerDown(object operationInstance)`. Parâmetro agora é USADO (cacheado em `_interceptOperationInstance` para CR-01-02). `Patches/SnapFireTriggerPatch.cs` — caller atualizado de `TryInterceptTriggerDown(fc)` → `TryInterceptTriggerDown(__instance)` (passa a operation, não a FC). Comentário inline `// ref: CR-01-03`.

---

### CR-01-04 · D — Arquitetura · 🟢 Menor · ✅ Aplicado em 2026-05-10

**`IsHoldingFirearm()` re-resolve o `HandsController` que o patch caller já tem**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded/StanceManager.cs:266`](../../modded/StanceManager.cs#L266) (chamada) e [`StanceManager.cs:238-247`](../../modded/StanceManager.cs#L238) (implementação)

**Problema:** `TryInterceptTriggerDown` chama `IsHoldingFirearm()` (linha 266), que internamente faz `gameWorld.MainPlayer.HandsController is Player.FirearmController`. Mas o caller no patch já tem o `Player.FirearmController` em mãos (acabou de resolvê-lo via reflection do operation). E o caller poderia simplesmente passar `fc != null` como sinal de "é firearm".

Pelo `csharp-mod-best-practices §1.3` (hot path allocations), evitar trabalho redundante. A chamada `is FirearmController` aqui é trivial em custo, mas o pattern de re-query do estado do mundo a partir do StanceManager (em vez de receber via parâmetro) acopla camadas.

**Por que importa:** Manutenção: se algum dia `MainPlayer.HandsController` divergir do `fc` resolvido pelo patch (ex.: ordering issue durante swap), a checagem da função e do caller seriam inconsistentes.

**Sugestão:** Se CR-01-03 for aceito, o caller já passa o `Player.FirearmController` validado (via guard de MainPlayer do CR-01-01) ou o `operation`. `TryInterceptTriggerDown` pode pular `IsHoldingFirearm()` e confiar no parâmetro. Caso contrário, manter como está — custo é mínimo.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (eliminar `IsHoldingFirearm()` quando caller já validou)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto. CR-01-01 já garante que o caller validou `fc == MainPlayer.HandsController is FirearmController` antes de chamar `TryInterceptTriggerDown`, então a re-verificação interna é redundante.
**Aplicação:** `StanceManager.cs` — linha `if (!IsHoldingFirearm()) return false;` removida de `TryInterceptTriggerDown`. XMLDOC atualizado citando o pré-contrato do caller.

---

### CR-01-05 · E — Legibilidade · 🟢 Menor · ✅ Aplicado em 2026-05-10

**`StanceConfig.SnapToStance0OnFire` declarado como não-nullable mas recebe `null` para Stance 0**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded/StanceConfig.cs:15`](../../modded/StanceConfig.cs#L15) (declaração) e [`Plugin.cs:1037-1045`](../../modded/Plugin.cs#L1037) (atribuição)

**Problema:** A declaração:

```csharp
public ConfigEntry<bool> SnapToStance0OnFire;
```

Sem `?`, em código sem `<Nullable>enable</Nullable>` no `.csproj`. O helper `BindStance` atribui `null` para Stance.Default:

```csharp
SnapToStance0OnFire = (d.Stance == Stance.Default)
    ? null
    : Config.Bind(d.Section, ...),
```

O comentário menciona "sentinel null" e o consumidor (`StanceManager.TryInterceptTriggerDown`) checa explicitamente `cfg.SnapToStance0OnFire == null`. Funciona. **Mas** o tipo do campo não comunica que `null` é um valor válido. Um futuro leitor pode adicionar `cfg.SnapToStance0OnFire.Value` em outro lugar sem null-check e crashar com NRE quando CurrentStance == Default.

**Por que importa:** Armadilha latente para manutenção. Aderência ao `csharp-mod-best-practices §4` (nullability).

**Sugestão:** Anotar o campo como `ConfigEntry<bool>?` se o csproj suporta nullable (`<LangVersion>9.0+</LangVersion>` + `<Nullable>annotations</Nullable>`):

```csharp
public ConfigEntry<bool>? SnapToStance0OnFire;
```

Caso contrário, adicionar comentário XMLDOC explicando o sentinel:

```csharp
/// <summary>
/// Snap automático para Stance 0 ao atirar.
/// **Pode ser null** — sentinel indicando que a stance é Stance.Default (não há snap nela).
/// Callers devem checar `if (SnapToStance0OnFire == null) return;` antes de `.Value`.
/// </summary>
public ConfigEntry<bool> SnapToStance0OnFire;
```

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (nullable annotation se possível; XMLDOC caso contrário)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada — caminho XMLDOC (csproj não tem `<Nullable>` habilitado; mudar isso geraria warnings em todo o resto do mod).
**Aplicação:** `StanceConfig.cs` — XMLDOC `<summary>` explícita acima de `SnapToStance0OnFire` alertando "⚠️ PODE SER NULL" + instrução de null-check obrigatório antes de `.Value`. Comentário inline `(ref: CR-01-05)` na própria doc.

---

### CR-01-06 · F — Melhoria opcional · 🟢 Menor · ✅ Aplicado em 2026-05-10

**`SnapStaleTimeoutSec = 2f` hard-coded — útil expor como Advanced para troubleshooting**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded/StanceManager.cs:242`](../../modded/StanceManager.cs#L242)

**Problema:** O timeout do stale guard é constante:

```csharp
private const float SnapStaleTimeoutSec = 2f;          // PA-01-05: stale guard contra weapon swap
```

Spec funcional/técnica não especifica o valor — é detalhe interno. **Mas**: se algum dia aparecer relato de bug "snap não funciona após weapon swap rápido", o usuário precisaria recompilar o mod para testar valores diferentes. Expor como ConfigEntry Advanced custa pouco e dá flexibilidade de debug em campo.

**Por que importa:** Qualidade de vida para futuras investigações. Padrão já estabelecido no mod (existem várias entries Advanced).

**Sugestão:** Adicionar em Plugin.cs:

```csharp
_SnapStaleTimeoutSec = Config.Bind(
    Settings,
    "Snap Stale Timeout (s)",
    2f,
    new ConfigDescription(
        "Maximum time (seconds) the snap intercept can stay active without a button-up before being auto-cleared. " +
        "Lower values reduce risk of stale state in weapon swap edge cases. Default 2s is safe.",
        new AcceptableValueRange<float>(0.5f, 10f),
        new ConfigurationManagerAttributes { IsAdvanced = true, Order = -2 }));
```

E em `StanceManager.EvaluateSnapStaleTimeout`:

```csharp
float timeout = Plugin._SnapStaleTimeoutSec?.Value ?? 2f;
if (Time.unscaledTime - _triggerDownTimeUnscaled <= timeout) return;
```

Atualizar PROPRIEDADES.md.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (expor como Advanced)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / não vale o esforço): _________________

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `Plugin.cs` — novo `_SnapStaleTimeoutSec = Config.Bind(Settings, "Snap Stale Timeout (s)", 2f, ... Range 0.5-10, IsAdvanced=true, Order=47)`. `StanceManager.cs` — `SnapStaleTimeoutSec` const renomeada para `SnapStaleTimeoutDefaultSec` (fallback); `EvaluateSnapStaleTimeout` lê `Plugin._SnapStaleTimeoutSec?.Value ?? SnapStaleTimeoutDefaultSec`. `PROPRIEDADES.md` — nova linha "Snap Stale Timeout (s)" + contador 89 → 90 + nota de origem no header. Comentário inline `// ref: CR-01-06`.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-05-10 | Code review 01 criada via `/code-review` — 0 bloqueadores; 1 forte (multiplayer Fika), 1 médio (weapon swap mid-intercept), 4 menores (code smell + nullability + advanced config). Asbuild ausente — análise usou §4 da spec técnica como fonte (fallback (b) da pré-condição). |
| 2026-05-10 | Aplicação automática de 6 achados via `/apply-code-review` — IDs aplicados: CR-01-01, CR-01-02, CR-01-03, CR-01-04, CR-01-05, CR-01-06. Asbuild criado retroativamente (`05-asbuild.md`). |
| 2026-05-10 | **F4 reescrita via [`06-fix-01.md`](002-ciclo-linear-hotkeys-snap-fogo-06-fix-01.md)** — patch target trocado de operation-base para `Player.FirearmController.SetTriggerPressed` (Player.cs:13668). Decisão original do **PA-01-01 review-01 (Estratégia A: patch operation-base via reflection)** revisitada com base em evidência runtime: F4 não funcionou in-raid porque virtual dispatch bypassa Harmony quando override não chama base — auditoria do Assembly confirmou que apenas 1 de 14 overrides chama `base.SetTriggerPressed()` (Player.cs:3184). Os pontos CR-01-01..06 desta review **permanecem aplicados** (MainPlayer guard, anti-swap, etc.) — apenas o nível do patch foi corrigido. |
