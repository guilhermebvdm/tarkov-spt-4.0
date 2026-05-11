# 002 — Ciclo linear, hotkeys e snap fogo · Review Técnica 02

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec técnica revisada:** [002-ciclo-linear-hotkeys-snap-fogo-02-spec-tech.md](002-ciclo-linear-hotkeys-snap-fogo-02-spec-tech.md)
**Data:** 2026-05-10

> Análise crítica da spec técnica após a aplicação das 8 resoluções da review-01. Foco: estratégias novas introduzidas — intercept-and-resurrect via `MethodInfo.Invoke` e resolução por reflection da operation-base.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 6 · Total: 6

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| PA-02-01 | C — Lógica | 🔴 | Ressurreição via `MethodInfo.Invoke` causa recursão infinita no Prefix do Harmony | ✅ Resolvido |
| PA-02-02 | A — Gap | 🟡 | `ResolveFirearmOperationBase` pode pegar uma operação concreta em vez da base | ✅ Resolvido |
| PA-02-03 | B — Edge | 🟡 | Trigger sintético + button-up imediato pode pular o tiro no animator | ✅ Resolvido |
| PA-02-04 | C — Lógica | 🟢 | `Action<bool>` lambda aloca closure + `object[]` a cada button-up | ✅ Resolvido |
| PA-02-05 | A — Gap | 🟢 | Comportamento de `ModulePatch.Enable()` com target null não verificado na spec | ✅ Resolvido |
| PA-02-06 | B — Edge | 🟢 | Sound do snap pode duplicar quando F4 + F5 disparam no primeiro frame de raid | ✅ Resolvido |

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

### PA-02-01 · C — Lógica · 🔴 Bloqueador · ✅ Resolvido em 2026-05-10

**Ressurreição via `MethodInfo.Invoke` causa recursão infinita no Prefix do Harmony**

**Problema:** Em §5.4 (`SnapFireTriggerPatch`), o stub faz no caminho de button-up:

```csharp
Action<bool> resurrect = (b) =>
    _originalSetTrigger.Invoke(__instance, new object[] { b });
StanceManager.OnTriggerUpAfterIntercept(fc, resurrect);
```

E `OnTriggerUpAfterIntercept` chama `originalSetTrigger?.Invoke(true)` quando `elapsed ≥ threshold`. O alvo `_originalSetTrigger` é exatamente o `MethodBase` retornado por `GetTargetMethod()` — que é o método **patcheado** pelo Harmony. Quando `MethodInfo.Invoke` é chamado num método patcheado, o stub do Harmony reescreveu o prólogo para chamar o Prefix → o nosso Prefix dispara de novo → vê `pressed=true`, possivelmente re-snap, possivelmente re-Invoke... → **stack overflow**.

Verificação: Harmony 2.x patcheia métodos via DynamicMethod replacement no IL prologue. O `MethodInfo` original continua existindo, mas chamá-lo via reflection passa pelo patch. Documentado no Harmony wiki: "Invoking the patched method via reflection invokes the patched version, not the original."

**Por que importa:** F4 sobe a stack até estourar no primeiro hold ≥ threshold. Crash imediato com a primeira tentativa de tiro com hold em snap-elegível. F4 não funciona em nenhum cenário "≥ threshold".

**Sugestão:** Adicionar **bypass explícito via flag thread-local** no Prefix. Padrão clássico para escapar do próprio patch:

```csharp
public class SnapFireTriggerPatch : ModulePatch
{
    [ThreadStatic] private static bool _inSyntheticCall;
    private static MethodBase _originalSetTrigger;

    [PatchPrefix]
    private static bool Prefix(object __instance, bool pressed)
    {
        // Reentry guard: se estamos dentro de uma ressurreição sintética,
        // deixa o trigger passar sem re-interceptar.
        if (_inSyntheticCall) return true;

        try
        {
            // ... mesmo fluxo de filtro FirearmController_0 ...
            if (pressed)
            {
                if (StanceManager.TryInterceptTriggerDown(fc))
                    return false;
                return true;
            }
            else
            {
                Action<bool> resurrect = (b) =>
                {
                    _inSyntheticCall = true;
                    try { _originalSetTrigger.Invoke(__instance, new object[] { b }); }
                    finally { _inSyntheticCall = false; }
                };
                StanceManager.OnTriggerUpAfterIntercept(fc, resurrect);
                return true;
            }
        }
        catch (Exception ex) { Plugin.Logger.LogError($"[F4] {ex}"); return true; }
    }
}
```

Atualizar §5.4 com o bypass + parágrafo explicativo "por que `[ThreadStatic]`" (Harmony Prefix pode rodar em qualquer thread em que o input chegar; `[ThreadStatic]` evita que uma chamada em outra thread também perca o reentry guard).

Alternativa mais "correta" mas mais complexa: usar `[HarmonyReversePatch]` para gerar um delegate "original-bypass". Como o target só é resolvido em runtime (nested type via reflection), `[HarmonyReversePatch]` exige `PatchProcessor` programático em vez do attribute — viável mas mais código. **Recomendação: a flag `[ThreadStatic]` é suficiente para o cenário do mod.**

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (flag `[ThreadStatic] _inSyntheticCall`)
- `[ ]` Caminho alternativo (HarmonyReversePatch via PatchProcessor): _________________
- `[ ]` Caminho alternativo: _________________

**Resolução:** §5.4 da spec técnica adiciona `[ThreadStatic] private static bool _inSyntheticCall;` no `SnapFireTriggerPatch`. O Prefix retorna `true` early se a flag estiver setada. A ressurreição agora roda dentro de um `try/finally` que seta/limpa a flag — garante bypass mesmo se Invoke jogar exceção.

---

### PA-02-02 · A — Gap · 🟡 Importante · ✅ Resolvido em 2026-05-10

**`ResolveFirearmOperationBase` pode pegar uma operação concreta em vez da base**

**Problema:** §2.1 propõe:

```csharp
foreach (var t in nested)
{
    var fcRef = t.GetField("FirearmController_0", ...);
    if (fcRef == null) continue;

    var m = AccessTools.DeclaredMethod(t, "SetTriggerPressed", new[] { typeof(bool) });
    if (m != null && m.IsVirtual)
    {
        _operationBaseType = t;
        _operationSetTriggerPressed = m;
        return;          // ← primeira que matcha vence
    }
}
```

O Assembly tem **12+ classes nested** com `SetTriggerPressed(bool)` declarado, vistas em [`Player.cs:2712, 3182, 4178, 4522, 5083, 5433, 6682, 7152, 7654, 8960, 10256, 11502`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs). A maioria (10+) são `public override` em **operações concretas** (DefaultWeaponOperationClass, ReloadOperationClass, etc.). Apenas uma é `public virtual` na **base abstrata** ([linha 3810](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L3810), corpo `method_0()`).

A `IsVirtual` checagem é insuficiente — métodos `override` também são `IsVirtual = true`. O loop pode pegar qualquer uma das concretas dependendo da ordem de iteração de `GetNestedTypes`, que **não é garantida pelo CLR**.

Pior: várias classes operação compartilham o mesmo backing field `FirearmController_0` (todas as operações herdam dele). Então o filtro `fcRef != null` casa com todas.

**Por que importa:** Patcheamos a operação errada → `[PatchPrefix]` só intercepta um caminho específico (ex: só durante reload, ou só durante default weapon op). Outros caminhos passam direto sem interceptação → snap inconsistente, F4 falha em N% dos casos sem mensagem.

**Sugestão:** Endurecer o filtro para preferir a base abstrata + corpo trivial:

```csharp
private static void ResolveFirearmOperationBase()
{
    var fc = typeof(Player.FirearmController);
    var nested = fc.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);

    // Critério primário: classe ABSTRATA com SetTriggerPressed virtual declarado.
    Type best = null;
    foreach (var t in nested)
    {
        if (!t.IsAbstract) continue;                                       // descarta concretas
        if (t.GetField("FirearmController_0",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) == null) continue;
        var m = AccessTools.DeclaredMethod(t, "SetTriggerPressed", new[] { typeof(bool) });
        if (m == null || !m.IsVirtual) continue;
        if (best != null)
        {
            Logger.LogError($"[F4] Multiple abstract operation-bases found: {best.FullName} " +
                            $"and {t.FullName}. Aborting F4 (ambiguous).");
            return;
        }
        best = t;
    }

    if (best == null)
    {
        // Fallback: top-most class na hierarquia que declara o método como virtual
        // (não-abstract pode ser válido se a base não for marcada IsAbstract).
        // Walk: começar de qualquer match e subir via BaseType até achar a primeira
        // declaração de SetTriggerPressed.
        foreach (var t in nested)
        {
            var m = AccessTools.DeclaredMethod(t, "SetTriggerPressed", new[] { typeof(bool) });
            if (m == null || !m.IsVirtual || m.IsFinal) continue;
            // Subir até a topmost declaração
            var declType = m.GetBaseDefinition().DeclaringType;
            if (declType?.IsNested == true && declType.DeclaringType == fc)
            {
                var declMethod = AccessTools.DeclaredMethod(declType, "SetTriggerPressed",
                    new[] { typeof(bool) });
                if (declMethod != null && declMethod.IsVirtual)
                {
                    best = declType;
                    Logger.LogInfo("[F4] Operation-base resolved via GetBaseDefinition fallback.");
                    break;
                }
            }
        }
    }

    if (best == null)
    {
        Logger.LogWarning("[F4] Failed to resolve FirearmController operation-base — F4 disabled.");
        return;
    }

    _operationBaseType = best;
    _operationSetTriggerPressed = AccessTools.DeclaredMethod(best, "SetTriggerPressed",
        new[] { typeof(bool) });
    Logger.LogInfo($"[F4] Operation-base = {best.FullName}");
}
```

Atualizar §2.1 com este código + nota: "preferir abstract; fallback via `MethodBase.GetBaseDefinition()`".

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (filtro `IsAbstract` + fallback `GetBaseDefinition`)
- `[ ]` Caminho alternativo: _________________

**Resolução:** §2.1 da spec técnica reescrita com filtro endurecido: prefere `IsAbstract == true`; ambiguidade (>1 abstract) loga erro e aborta F4. Fallback usa `MethodBase.GetBaseDefinition()` para subir até a topmost classe que declara `SetTriggerPressed(bool)` virtual + back-ref `FirearmController_0`.

---

### PA-02-03 · B — Edge · 🟡 Importante · ✅ Resolvido em 2026-05-10

**Trigger sintético + button-up imediato pode pular o tiro no animator**

**Problema:** Em §6 (Fluxo de dados), o caminho de hold ≥ threshold:

```
[6] StanceManager.OnTriggerUpAfterIntercept(fc, resurrectAction)
    └─ resurrectAction(true)  // sintetiza trigger-down
[7] operation.SetTriggerPressed(true) executa → animator dispara tiro
[8] Prefix retorna true → button-up natural propaga → operation.SetTriggerPressed(false)
```

O passo [7] e [8] acontecem na **mesma stack frame** — `resurrectAction` é executado dentro do Prefix de `pressed=false`, e logo após o Prefix retorna, o operation chama o `SetTriggerPressed(false)` original. Diferença temporal: ~0 ms.

Para semi-auto, o `FirearmsAnimator` reage ao `IsTriggerPressed` num evento de animação — se o trigger ficou `true` por menos que 1 frame, o animator pode não ter tido `Update()` entre o synthetic-set e o natural-reset → **o tiro pode não disparar**. EFT semi-auto tipicamente lê `IsTriggerPressed` no callback de animação `OnFireEvent`, que só roda quando o ciclo de animação chega lá.

**Por que importa:** Hold legítimo (≥ 200ms) pode resultar em "snap aconteceu mas tiro não saiu" — bug intermitente, dependente de timing. Critério de aceite "Em Stance 1 com snap ativo, segurar o gatilho faz snap para Stance 0 e começa a disparar" pode falhar.

**Sugestão:** Adiar a ressurreição em **um frame** via `Plugin.Update` em vez de executar inline no Prefix:

```csharp
// StanceManager.cs
private static FirearmController _pendingResurrectFc;
private static MethodBase _pendingResurrectMethod;
private static object _pendingResurrectInstance;

public static void OnTriggerUpAfterIntercept(object operationInstance,
                                             Player.FirearmController fc,
                                             MethodBase original)
{
    if (!_snapInterceptActive) return;
    _snapInterceptActive = false;
    float elapsedMs = (Time.unscaledTime - _triggerDownTimeUnscaled) * 1000f;
    _triggerDownTimeUnscaled = SnapIdleSentinel;

    int threshold = Plugin._SnapFireThreshold?.Value ?? 200;
    if (elapsedMs < threshold) return; // clique único — nada a fazer

    // Adia para o Update do próximo frame — dá tempo do button-up natural propagar
    // e do animator processar o reset antes do trigger-down sintético.
    _pendingResurrectInstance = operationInstance;
    _pendingResurrectMethod = original;
    _pendingResurrectFc = fc;
}

// Em StanceManager.Update(), no início (antes do snap stale guard):
private static void TryDispatchPendingResurrect()
{
    if (_pendingResurrectMethod == null) return;
    var inst = _pendingResurrectInstance;
    var method = _pendingResurrectMethod;
    _pendingResurrectInstance = null;
    _pendingResurrectMethod = null;
    _pendingResurrectFc = null;

    SnapFireTriggerPatch.RaiseSyntheticTriggerDown(inst, method);
}
```

E no Prefix, em vez de chamar resurrect inline, passar `__instance` + `_originalSetTrigger` para o `OnTriggerUpAfterIntercept`. A ressurreição vira async (1 frame de latência ≈ 16ms a 60fps — imperceptível mas suficiente para o animator).

Atualizar §5.3, §5.4 e §6 com este modelo. Trade-off: latência mínima, mas comportamento determinístico.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (defer 1 frame via Update)
- `[ ]` Caminho alternativo (testar inline e ver se EFT processa OK): _________________
- `[ ]` Caminho alternativo: _________________

**Resolução:** §5.3 da spec técnica adiciona campos pending (`_pendingResurrectInstance`, `_pendingResurrectMethod`, `_pendingResurrectFc`) e helper `TryDispatchPendingResurrect()` chamado no início de `Update()`. `OnTriggerUpAfterIntercept` apenas registra a intenção; a ressurreição efetiva acontece no próximo frame. §6 atualizado para refletir o novo timing (gap de ~16ms entre button-up natural e trigger sintético).

---

### PA-02-04 · C — Lógica · 🟢 Menor · ✅ Resolvido em 2026-05-10

**`Action<bool>` lambda aloca closure + `object[]` a cada button-up**

**Problema:** §5.4 cria a cada button-up:

```csharp
Action<bool> resurrect = (b) =>
    _originalSetTrigger.Invoke(__instance, new object[] { b });
```

Cada chamada aloca: (a) instância de `Action<bool>` (closure capturando `__instance` e `_originalSetTrigger`), (b) `object[1]` para o `Invoke`. Captura de `__instance` é especialmente custosa porque o closure mantém referência viva ao operation enquanto não-coletado.

Não é hot-path crítico (poucos button-ups por minuto), mas é desperdício de allocação numa codebase que segue [csharp-mod-best-practices §1.3](../../../../).

**Por que importa:** GC pressure mínimo, mas o `csharp-mod-best-practices` é citado como skill obrigatória — convém manter consistência (existing code também evita LINQ/closures em hot paths).

**Sugestão:** Se PA-02-03 for aceito, isso desaparece naturalmente (a ressurreição vira chamada direta no Update sem closure). Se PA-02-03 for rejeitado, refatorar para método estático:

```csharp
private static readonly object[] _trueArgs = new object[] { true };

private static void RaiseSyntheticTriggerDown(object operationInstance)
{
    _inSyntheticCall = true;
    try { _originalSetTrigger.Invoke(operationInstance, _trueArgs); }
    finally { _inSyntheticCall = false; }
}
```

E `OnTriggerUpAfterIntercept` recebe `(object operationInstance, FirearmController fc)` em vez de `Action<bool>`.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (sem closure, args cacheados)
- `[x]` Resolvido por consequência se PA-02-03 (defer) for aceito
- `[ ]` Não vale o esforço

**Resolução:** Eliminado pela aceitação de PA-02-03. O novo modelo (deferred dispatch via Update) não usa `Action<bool>` lambda — `TryDispatchPendingResurrect` chama `_originalSetTrigger.Invoke(operationInstance, _trueArgs)` com `object[] _trueArgs = new[] { (object)true }` cacheado em campo estático.

---

### PA-02-05 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-05-10

**Comportamento de `ModulePatch.Enable()` com target null não verificado na spec**

**Problema:** Spec diz em §7.4:

> "Se `Plugin.OperationOriginalSetTrigger == null`, o `GetTargetMethod()` retorna null e Harmony pula o registro silenciosamente — F4 fica off, demais features intactas."

Na prática, `ModulePatch` (de `SPT.Reflection.Patching`) chama `harmony.Patch(GetTargetMethod(), ...)` no `Enable()`. Se o `MethodBase` for null, **`Harmony.Patch` lança `ArgumentNullException`**, não pula silenciosamente. A spec está incorreta sobre esse fallback.

**Por que importa:** Plugin.Awake crasha se a operation-base não for resolvida → mod inteiro não carrega → não só F4 fica off, mas TUDO fica off. O contrário do que a spec promete.

**Sugestão:** No `Plugin.Awake()`, **condicionar o Enable** ao target estar resolvido:

```csharp
// Plugin.Awake (após PatchAll dos demais)

if (Plugin.OperationOriginalSetTrigger != null)
{
    new SnapFireTriggerPatch().Enable();
}
else
{
    Logger.LogWarning("[F4] SnapFireTriggerPatch não habilitado — operation-base não resolvida.");
}
```

E em §5.4, remover a "expectativa silenciosa" no `GetTargetMethod`:

```csharp
protected override MethodBase GetTargetMethod()
{
    return Plugin.OperationOriginalSetTrigger
        ?? throw new InvalidOperationException(
            "SnapFireTriggerPatch não deveria ser registrado quando OperationOriginalSetTrigger é null. " +
            "Plugin.Awake deve checar antes de chamar Enable().");
}
```

Atualizar §7.4 da spec com a sequência correta + checklist 8.5: "Plugin.Awake só chama `new SnapFireTriggerPatch().Enable()` se `OperationOriginalSetTrigger != null`."

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (Enable condicional + spec corrigida)
- `[ ]` Caminho alternativo: _________________

**Resolução:** §7.4 e §8.5 atualizadas — `Plugin.Awake` checa `OperationOriginalSetTrigger != null` antes de `new SnapFireTriggerPatch().Enable()`. `GetTargetMethod` lança `InvalidOperationException` defensivamente se for chamado com target null (não deveria acontecer dado o Enable condicional, mas garante crash visível em vez de NRE silenciosa).

---

### PA-02-06 · B — Edge · 🟢 Menor · ✅ Resolvido em 2026-05-10

**Sound do snap pode duplicar quando F4 + F5 disparam no primeiro frame de raid**

**Problema:** Sequência possível no início de raid:

1. `OnRaidStart` → `_activeStaminaStance = Stance.Default`.
2. `QueueInitialStance(Stance3)` (se F5 toggle on).
3. Próximo `Update`: `TryApplyPendingInitialStance` → `SpringGetPatch.ResetState()` → `CurrentStance = Stance3` → `OnStanceChanged(Default → Stance3)` → SpringGetPatch toca `PlayStanceChangeSound`. **Som 1.**
4. Jogador pressiona fogo enquanto Stance3 ainda em transição (raro mas possível em scav: spawnar com fogo apertado): `TryInterceptTriggerDown` → `CurrentStance = Stance.Default` → `OnStanceChanged(Stance3 → Default)` → SpringGetPatch toca som de novo. **Som 2.**

Resultado: dois "clack" em ≤ 100ms. Não é bug, é UX questionável — som pode soar como bug ou eco.

Outro caso: F5 toggle on + jogador segura `O` (Stance 3 Hotkey) no spawn, então F5 aplica Stance3 (som 1) e a hotkey toggles para Default (som 2). Ainda mais raro.

**Por que importa:** Ruído auditivo em situações específicas. Não impede gameplay, mas merece nota na verificação manual.

**Sugestão:** Adicionar AC em §8.8: "F5+F4 simultâneos no spawn: aceitável que dois sons de stance-change toquem em sequência rápida (não é bug)." Ou — se quiser evitar — adicionar cooldown de som de 100ms no `PlayStanceChangeSound` ([SpringGetPatch.cs:466](../../modded/Patches/SpringGetPatch.cs#L466)). Recomendação: apenas documentar (cooldown vira complexidade extra para benefício marginal).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (apenas documentar AC)
- `[ ]` Caminho alternativo (adicionar cooldown de 100ms): _________________

**Resolução:** §8.8 da spec técnica recebe AC: "F5+F4 simultâneos no spawn — aceitável que dois sons de stance-change toquem em sequência rápida (não é bug)."

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-05-10 | Review 02 criada — análise pós-resolução da review-01. 1 bloqueador (recursão por Invoke), 2 importantes (resolução de operation-base, timing do trigger sintético), 3 menores. |
| 2026-05-10 | Todas as 6 sugestões aceitas. Spec técnica atualizada (§2.1, §5.3, §5.4, §6, §7.4, §8.5, §8.8). PA-02-04 fechado por consequência da aceitação de PA-02-03. |
