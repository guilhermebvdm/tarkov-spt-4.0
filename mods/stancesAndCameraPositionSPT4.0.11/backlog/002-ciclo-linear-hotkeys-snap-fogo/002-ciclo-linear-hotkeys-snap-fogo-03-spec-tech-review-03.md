# 002 — Ciclo linear, hotkeys e snap fogo · Review Técnica 03

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec técnica revisada:** [002-ciclo-linear-hotkeys-snap-fogo-02-spec-tech.md](002-ciclo-linear-hotkeys-snap-fogo-02-spec-tech.md)
**Data:** 2026-05-10

> Análise crítica da spec técnica após a aplicação das 6 resoluções da review-02. Foco: comportamento do trigger sintético em diferentes fire modes do EFT, e robustez do estado pendente entre frames.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 5 · Total: 5

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| PA-03-01 | C — Lógica | 🔴 | Auto-fire runaway: synthetic trigger=true sem follow-up false esvazia o carregador | ✅ Resolvido |
| PA-03-02 | B — Edge | 🟡 | `_pendingResurrectInstance` pode estar stale após weapon swap entre os frames N e N+1 | ✅ Resolvido |
| PA-03-03 | A — Gap | 🟡 | Spec não cobre interação F4 quando `ChangeFireMode` é acionado entre intercept e resurrect | ✅ Resolvido |
| PA-03-04 | A — Gap | 🟢 | `SettingChanged` subscriptions não são removidas no teardown do plugin | ✅ Resolvido |
| PA-03-05 | A — Gap | 🟢 | Stub de `BuildStanceConfig` modificado para F4 não está em §5 | ✅ Resolvido |

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

### PA-03-01 · C — Lógica · 🔴 Bloqueador · ✅ Resolvido em 2026-05-10

**Auto-fire runaway: synthetic trigger=true sem follow-up false esvazia o carregador**

**Problema:** Em §6 (fluxo F4) e §5.4 (`SnapFireTriggerPatch`), a sequência de hold ≥ threshold é:

```
Frame N:   button-up natural → operation.SetTriggerPressed(false)  // operation reseta trigger
Frame N+1: TryDispatchPendingResurrect → method.Invoke(operation, _trueArgs)  // synthetic true
```

O frame N+1 seta `IsTriggerPressed = true`. Para **semi-auto** ([`Player.cs:2933`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L2933)) e **burst**, o operation auto-reseta `IsTriggerPressed = false` no `InternalOnFireEndEvent` ([`Player.cs:2926`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L2926), `:2940`) — então 1 tiro sai e a animação para. ✓

Para **fullauto** ([`Player.cs:14338`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L14338) — `EFireMode.fullauto` é confirmado no Assembly), o ciclo de animação **não auto-reseta** `IsTriggerPressed = false` enquanto for `true`. Como o button-up natural do usuário **já passou** no frame N (antes da nossa Invoke), não chega novo `false` na operation. Resultado: arma dispara continuamente até a magazine esvaziar (ou até o jogador apertar o gatilho de novo).

**Por que importa:** Em qualquer arma fullauto (AKM em modo auto, M4, todos os PDWs), holding ≥ 200ms em Stance 1/2 com snap on → arma esvazia o carregador inteiro. **Spec funcional linha 165:** "Em Stance 1 com snap ativo, segurar o gatilho faz snap para Stance 0 e **começa a disparar**." — implementação atual transforma isso em "esvazia mag de 60 munições com 1 hold". Critério de aceite claramente quebrado para auto.

Não é detectado pelo stale-timeout (`SnapStaleTimeoutSec = 2f`) porque as flags já foram limpas no Update do frame N+1 — o stale guard só vigia o intercept ainda ativo, não a ressurreição despachada.

**Sugestão:** Adicionar **agendamento de synthetic false no frame seguinte ao synthetic true**. Implementação:

1. Em `StanceManager.cs`, adicionar segundo par de campos:
   ```csharp
   private static object     _pendingResetInstance;
   private static MethodBase _pendingResetMethod;
   ```

2. Modificar `TryDispatchPendingResurrect` para encadear o reset:
   ```csharp
   private static void TryDispatchPendingResurrect()
   {
       // 1. Despachar reset agendado (synthetic false do frame anterior)
       if (_pendingResetMethod != null)
       {
           SnapFireTriggerPatch.RaiseSyntheticTrigger(_pendingResetInstance, _pendingResetMethod, pressed: false);
           _pendingResetInstance = null;
           _pendingResetMethod = null;
       }

       // 2. Despachar resurrect (synthetic true) e agendar reset
       if (_pendingResurrectMethod != null)
       {
           var inst = _pendingResurrectInstance;
           var method = _pendingResurrectMethod;
           _pendingResurrectInstance = null;
           _pendingResurrectMethod = null;

           SnapFireTriggerPatch.RaiseSyntheticTrigger(inst, method, pressed: true);
           _pendingResetInstance = inst;
           _pendingResetMethod = method;
       }
   }
   ```

3. Em `SnapFireTriggerPatch`, generalizar para aceitar bool e cachear ambos `_trueArgs`/`_falseArgs`:
   ```csharp
   private static readonly object[] _trueArgs  = new object[] { true };
   private static readonly object[] _falseArgs = new object[] { false };

   public static void RaiseSyntheticTrigger(object operationInstance, MethodBase original, bool pressed)
   {
       if (operationInstance == null || original == null) return;
       _inSyntheticCall = true;
       try { original.Invoke(operationInstance, pressed ? _trueArgs : _falseArgs); }
       catch (Exception ex) { Plugin.Logger.LogError($"[F4] synthetic trigger {pressed} failed: {ex}"); }
       finally { _inSyntheticCall = false; }
   }
   ```

Resultado:
- Frame N: button-up natural → operation recebe false.
- Frame N+1: synthetic true → operation recebe true, fire animation começa.
- Frame N+2: synthetic false → operation recebe false, fire animation para após 1 ciclo.

Para fullauto a 600 RPM (10/sec), 2 frames a 60fps ≈ 33ms ≈ ~0,33 tiros — para fins práticos, "1 tiro natural" como spec funcional pede. Para semi/burst, o segundo `false` é redundante (operation já resetou via fire-end-event) mas inofensivo.

Atualizar §5.3, §5.4, §6 e checklist §8.5 com este modelo de **2-frame pulse**.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (2-frame pulse: synthetic true no frame +1, synthetic false no frame +2)
- `[ ]` Caminho alternativo (detectar fire mode via `fc.Item.FireMode.FireMode == EFireMode.fullauto` e ajustar duração proporcionalmente): _________________
- `[ ]` Caminho alternativo (revisar spec funcional para aceitar "esvazia mag" em auto): _________________

**Resolução:** §5.3 da spec adiciona segundo par de campos `_pendingResetInstance`/`_pendingResetMethod`. `TryDispatchPendingResurrect` reescrita para encadear: despacha o reset agendado primeiro, então despacha a ressurreição e agenda o reset do frame seguinte. §5.4 generaliza `RaiseSyntheticTriggerDown` → `RaiseSyntheticTrigger(inst, method, pressed)` com `_trueArgs` e `_falseArgs` cacheados. §6 adiciona passo de frame N+2 no fluxo. Pulse de 2 frames a 60fps (~33ms) ≈ ~0,33 tiro em fullauto a 600 RPM — efetivamente "1 tiro" alinhado com spec funcional.

---

### PA-03-02 · B — Edge · 🟡 Importante · ✅ Resolvido em 2026-05-10

**`_pendingResurrectInstance` pode estar stale após weapon swap entre os frames N e N+1**

**Problema:** Em §5.3, `OnTriggerUpAfterIntercept` armazena o operation instance:

```csharp
_pendingResurrectInstance = operationInstance;
_pendingResurrectMethod   = originalMethod;
```

E `TryDispatchPendingResurrect` (frame seguinte) faz `Invoke` cego nesse instance. Janela de 1 frame (~16ms a 60fps) é pequena mas cobre cenários reais:

- Jogador segura `O` (Stance 3 hotkey) e clica fogo simultaneamente: Stance3 hotkey → CurrentStance muda → potencial mudança de operation.
- Animação de reload começa entre frames: operation muda de DefaultWeaponOperationClass para ReloadOperationClass.
- Quick-swap de arma via slot: novo HandsController instalado entre frames.

A operation antiga ainda existe como objeto C# (não foi GC), mas seu estado interno pode estar inválido. `Invoke(SetTriggerPressed, true)` em operation defunct pode setar `FirearmController_0.IsTriggerPressed = true` no FirearmController — que **não é mais o ativo**, mas a flag persiste. Se o jogador voltar para a mesma arma depois, primeiro tiro pode comportar-se inesperadamente.

**Por que importa:** Bug raro mas não impossível, sintomas confusos. O stale-timeout (PA-01-05) cobre o caso de hold sem button-up; **não cobre** o caso de hold-release-swap-em-1-frame.

Spec funcional corner case linha 183: "Troca de arma em stance não-padrão: trocar de arma enquanto snap for pendente não deve causar snap residual na nova arma." — janela de 1 frame entre button-up e dispatch é exatamente esse caso, ainda não coberto.

**Sugestão:** Validar que a operation ainda está ativa antes de invocar. Adicionar guard em `TryDispatchPendingResurrect`:

```csharp
private static void TryDispatchPendingResurrect()
{
    if (_pendingResurrectMethod == null) return;
    var inst = _pendingResurrectInstance;
    var method = _pendingResurrectMethod;
    _pendingResurrectInstance = null;
    _pendingResurrectMethod = null;

    // Guard: validar que operation ainda é a CurrentOperation do FirearmController do MainPlayer.
    var gw = GetCachedGameWorld();
    var fc = gw?.MainPlayer?.HandsController as Player.FirearmController;
    if (fc == null)
    {
        Plugin.Logger.LogDebug("[F4] resurrect skipped: HandsController não é firearm");
        return;
    }
    // CurrentOperation é resolvida via reflection (mesma classe nested)
    var currentOpField = typeof(Player.FirearmController).GetField("CurrentOperation",
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    if (currentOpField?.GetValue(fc) != inst)
    {
        Plugin.Logger.LogDebug("[F4] resurrect skipped: operation mudou entre frames");
        return;
    }

    SnapFireTriggerPatch.RaiseSyntheticTrigger(inst, method, pressed: true);
}
```

> Nota: `CurrentOperation` é referenciado em [`Player.cs:4558`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L4558) como propriedade pública de `Player.FirearmController`; `AccessTools.PropertyGetter(typeof(Player.FirearmController), "CurrentOperation")` é alternativa mais limpa que `GetField`.

Atualizar §5.3 e checklist §8.5 com a validação.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (validar `fc.CurrentOperation == _pendingResurrectInstance` antes do dispatch)
- `[ ]` Caminho alternativo: _________________

**Resolução:** §5.3 adiciona guard em `TryDispatchPendingResurrect` que valida `(fc as Player.FirearmController).CurrentOperation == _pendingResurrectInstance` (resolvido via `AccessTools.PropertyGetter` cacheado no Awake). Se não bater, drop silencioso com `LogDebug`. Mesmo guard aplicado ao reset agendado em frame N+2 para garantir que não vaze para a nova arma.

---

### PA-03-03 · A — Gap · 🟡 Importante · ✅ Resolvido em 2026-05-10

**Spec não cobre interação F4 quando `ChangeFireMode` é acionado entre intercept e resurrect**

**Problema:** A spec descreve o comportamento de F4 para hold ≥ threshold mas não menciona o que acontece se o jogador trocar o fire mode (`B` no EFT) durante o hold. EFT permite trocar fire mode mesmo com gatilho pressionado (`ChangeFireMode` em [`Player.cs:3829`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L3829)).

Cenário:
1. Stance 1, weapon em fullauto, snap-on.
2. Button-down → snap, intercept ativo.
3. Hold > 200ms.
4. Player aperta `B` → fire mode muda para single.
5. Button-up → resurrect: synthetic true em frame N+1.
6. Synthetic false em frame N+2 (se PA-03-01 aceito).

Comportamento: arma agora é single — synthetic true dispara 1 tiro. Aceitável mas não documentado.

Cenário oposto:
1. Weapon em single, snap-on.
2. Hold ≥ threshold, swap para fullauto durante hold.
3. Resurrect → fullauto fires until synthetic false (PA-03-01) — comportamento "1 frame de fullauto" similar ao fullauto natural.

**Por que importa:** Comportamento implícito mas não testado nem documentado. Critério de aceite ausente. Implementador pode "descobrir" o comportamento durante teste e questionar se é bug.

**Sugestão:** Adicionar AC explícita em §8.8:

> "F4 + ChangeFireMode mid-hold: trocar fire mode durante hold (ex: fullauto → single) não cancela o snap pendente. Resurrect dispara segundo o modo ATIVO no frame de release+1, não o modo ativo no button-down. Comportamento intencional."

Sem alteração de código necessária — apenas documentação para evitar reclassificação como bug.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (apenas documentar AC)
- `[ ]` Caminho alternativo (cancelar resurrect se ChangeFireMode for detectado): _________________

**Resolução:** AC adicionada em §8.8: "F4 + ChangeFireMode mid-hold — comportamento intencional, dispara segundo o modo no release+1."

---

### PA-03-04 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-05-10

**`SettingChanged` subscriptions não são removidas no teardown do plugin**

**Problema:** §5.2 da spec adiciona:

```csharp
_MouseWheelScrollMode.SettingChanged += (_, __) => RefreshScrollModeVisibility();
_EnableMouseWheelCycle.SettingChanged += (_, __) => RefreshScrollModeVisibility();
```

Subscribe sem unsubscribe correspondente. Para um plugin BepInEx normal (boot-up único, vida da sessão inteira), isso não vaza — `ConfigEntry` e `Plugin` morrem juntos. **Mas** se o usuário tentar BepInEx hot-reload (raro mas possível em desenvolvimento), os ConfigEntries antigos continuariam segurando lambdas que apontam para o método estático antigo do tipo já descarregado → potencial referência morta na primeira mudança.

Per [csharp-mod-best-practices §1.2](../../../../.claude/skills/csharp-mod-best-practices.md): "Subscribed events are strong references … long-lived static publishers, use weak-event patterns or strict subscribe/unsubscribe pairing."

**Por que importa:** Edge case real apenas em hot-reload de desenvolvimento. Não afeta usuários finais. Mas viola checklist §1.2 do skill.

**Sugestão:** Adicionar `OnDestroy()` no `Plugin.cs` para limpar:

```csharp
private void OnDestroy()
{
    if (_MouseWheelScrollMode != null)
        _MouseWheelScrollMode.SettingChanged -= OnScrollModeSettingChanged;
    if (_EnableMouseWheelCycle != null)
        _EnableMouseWheelCycle.SettingChanged -= OnScrollModeSettingChanged;
}

private static void OnScrollModeSettingChanged(object _, EventArgs __)
    => RefreshScrollModeVisibility();
```

Trocar lambdas por método nomeado estático para permitir unsubscribe. Atualizar §5.2 e checklist §8.3.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (método nomeado + OnDestroy unsubscribe)
- `[ ]` Caminho alternativo (deixar como está — hot-reload não é cenário suportado): _________________

**Resolução:** §5.2 troca lambdas por método nomeado `OnScrollModeSettingChanged(object, EventArgs)`; `Plugin.OnDestroy()` faz unsubscribe explícito. Checklist §8.3 atualizada.

---

### PA-03-05 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-05-10

**Stub de `BuildStanceConfig` modificado para F4 não está em §5**

**Problema:** Checklist §8.5 menciona:

> "No helper `BuildStanceConfig` em `Plugin.cs`, adicionar bind de `Stance N Snap to Stance 0 on Fire` (default `true` para 1/2, `false` para 3, Order 0 — final da seção da stance)."

Mas §5 (Stubs de código) não mostra como integrar isso ao helper existente. O helper atual em [Plugin.cs:854](../../modded/Plugin.cs#L854) é uma factory que recebe um tuple de defaults — adicionar o `SnapToStance0OnFire` requer estender a tuple OU passar parâmetro extra OU usar um per-stance overlay no chamador.

Defaults divergem por stance (`true`/`true`/`false`), enquanto o tuple atual `_stanceDefaults` em [Plugin.cs:28](../../modded/Plugin.cs#L28) já cobre essa divergência por stance. Solução natural é estender o tuple — mas o stub não mostra isso, e é exatamente o tipo de trabalho que pode ser feito de várias formas inconsistentes.

**Por que importa:** Implementador pode escolher formato diferente do que a spec assume, criando atrito em review pós-código.

**Sugestão:** Adicionar §5.7 mostrando a extensão concreta do tuple e do helper:

```csharp
// modded/Plugin.cs (modificar tuple existente em :28)
private static readonly (Stance Stance, string Section, float StaminaMultiplier, bool ModSpeed,
                         int Multiplier, bool ApplyProne, bool SnapOnFire)[]
    _stanceDefaults =
{
    (Stance.Default, Stance0Section, 0.5f,  true,  90,  false, false),  // Stance 0 nunca snap
    (Stance.Stance1, Stance1Section, 1.5f,  true,  95,  false, true),
    (Stance.Stance2, Stance2Section, 2.0f,  true,  100, false, true),
    (Stance.Stance3, Stance3Section, 1.0f,  true,  90,  false, false),  // Low Ready: default off
};

// modded/Plugin.cs (no helper, após o último Config.Bind dentro do return new StanceConfig {...}):
// (Stance 0 não recebe SnapToStance0OnFire — guard em StanceManager checa CurrentStance != Default)
SnapToStance0OnFire = (d.Stance == Stance.Default)
    ? null
    : Config.Bind(d.Section, $"Stance {n} Snap to Stance 0 on Fire", d.SnapOnFire,
        new ConfigDescription(
            "When enabled, firing while in this stance snaps to Stance 0 - Vanilla. " +
            "Does not trigger in ADS or with non-firearm items.",
            null,
            new ConfigurationManagerAttributes { Order = 0 })),
```

Documentar também: "Stance 0 não tem `SnapToStance0OnFire` ConfigEntry — `null` é valor sentinela; o guard `if (CurrentStance == Stance.Default) return false;` em `TryInterceptTriggerDown` (§5.3) já cobre."

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (estender tuple + null para Stance 0)
- `[ ]` Caminho alternativo (criar 3 binds inline em Awake fora do helper): _________________

**Resolução:** §5.7 adicionada com stub concreto da extensão da tuple `_stanceDefaults` (campo `SnapOnFire`) e da modificação do helper `BuildStanceConfig` (campo `SnapToStance0OnFire = null` para Stance 0; bind para Stance 1/2/3 com defaults `true/true/false`).

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-05-10 | Review 03 criada — análise pós-resolução da review-02. 1 bloqueador (auto-fire runaway sem follow-up false), 2 importantes (operation stale + ChangeFireMode mid-hold), 2 menores. |
| 2026-05-10 | Todas as 5 sugestões aceitas. Spec atualizada: §5.2 (método nomeado + OnDestroy), §5.3 (reset agendado + CurrentOperation guard), §5.4 (RaiseSyntheticTrigger generalizado), §5.7 (BuildStanceConfig stub), §6 (frame N+2), §8.3/§8.5/§8.8. |
