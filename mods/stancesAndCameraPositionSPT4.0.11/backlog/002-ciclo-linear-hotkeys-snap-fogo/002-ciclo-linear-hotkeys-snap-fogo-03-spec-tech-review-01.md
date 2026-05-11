# 002 — Ciclo linear, hotkeys e snap fogo · Review Técnica 01

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec técnica revisada:** [002-ciclo-linear-hotkeys-snap-fogo-02-spec-tech.md](002-ciclo-linear-hotkeys-snap-fogo-02-spec-tech.md)
**Data:** 2026-05-10

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 8 · Total: 8

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Lógica | 🔴 | Patches de F4 miram em métodos que não existem em `Player.FirearmController` | ✅ Resolvido |
| PA-01-02 | A — Gap | 🔴 | F4 race condition: tiro semi-auto sai antes do button-up para clique único | ✅ Resolvido |
| PA-01-03 | C — Lógica | 🟡 | `RefreshScrollModeVisibility` depende de internals do ConfigurationManager | ✅ Resolvido |
| PA-01-04 | B — Edge | 🟡 | Hotkeys com mesma tecla — prioridade implementada é a oposta da especificada | ✅ Resolvido |
| PA-01-05 | B — Edge | 🟡 | Snap state pode vazar em troca de arma durante a raid | ✅ Resolvido |
| PA-01-06 | A — Gap | 🟢 | `_snapPendingFromTrigger` redundante — duplica a fonte de verdade do timer | ✅ Resolvido |
| PA-01-07 | A — Gap | 🟢 | Confirmar explicitamente que o som de troca de stance toca no snap | ✅ Resolvido |
| PA-01-08 | A — Gap | 🟢 | Checklist 8.3 omite captura dos `ConfigurationManagerAttributes` de `_EnableStance1/2/3` | ✅ Resolvido |

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

### PA-01-01 · C — Lógica · 🔴 Bloqueador · ✅ Resolvido em 2026-05-10

**Patches de F4 miram em métodos que não existem em `Player.FirearmController`**

**Problema:** A spec técnica (§5.4 e §5.5) propõe `AccessTools.Method(typeof(Player.FirearmController), "SetTriggerPressed", new[] { typeof(bool) })` e candidatos `InternalOnFireEvent` / `OnFireEvent` / `Fire` em `Player.FirearmController`. Verificando o Assembly:

- `Player.FirearmController` abre em [`Player.cs:2441`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L2441) (`public class FirearmController : ItemHandsController, …`).
- A `public virtual void SetTriggerPressed(bool pressed)` em [`Player.cs:3810`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L3810) está dentro de uma **classe-base de operação aninhada** dentro de FirearmController (corpo é só `method_0();`, e o bloco circundante 3700–3819 é um dump de stubs virtuais — `OnMagAppeared`, `OnFold`, `BlindFire_Internal` que usa `FirearmController_0` como back-reference, ver linha 3719).
- `InternalOnFireEvent()` em [`Player.cs:2810`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L2810) está na **mesma classe-base de operação**: o corpo lê `Weapon_0`, `Player_0`, `FirearmsAnimator_0`, `SingleShotData_0` — todos backing fields de operação, não de FirearmController.
- As outras 12 ocorrências de `public override void SetTriggerPressed` (2712, 3182, 4178, 4522, 5083, 5433, 6682, 7152, …) estão em **classes de operação concretas** (`DefaultWeaponOperationClass` em 3170, etc.) — todas com `FirearmController_0.IsTriggerPressed = pressed;` ou `base.SetTriggerPressed(pressed);`.
- Nenhuma evidência no Assembly de que `Player.FirearmController` redefina `SetTriggerPressed` ou `OnFireEvent` *no escopo da própria classe* (não nas operações).

Resultado: `AccessTools.Method(typeof(Player.FirearmController), "SetTriggerPressed", …)` retorna a herança de `ItemHandsController` (a base abstrata), o que faz Harmony patchar a base e disparar para **todos** os HandsControllers (Knife, Grenade, Med, Empty). A guard `__instance is FirearmController` mitiga, mas o spec não menciona isso. Pior: o método pode nem estar declarado em `ItemHandsController` — pode ser apenas no enum de operações.

Para fire-event, o método exposto pelo FirearmController **diretamente** não foi confirmado; os candidatos do fallback (§5.5) testam por `typeof(Player.FirearmController)` e provavelmente encontram a herança da base ou retornam null.

**Por que importa:**

- `SnapFireTriggerPatch` pode disparar para HandsController errados (knife → snap inadvertido), ou não disparar (se a base não declara o método).
- `SnapFireOnFireEventPatch` provavelmente nunca patcheia nada útil (`Player.FirearmController` não tem método de fogo público com esse nome) → tiro do clique único nunca é abortado → F4 quebra silenciosamente.
- A spec passa para `/code-mod` com um stub que parece compilar mas falha em runtime.

**Sugestão:** Reescrever §2 (Pontos de patch) e §5.4/§5.5 com a estratégia correta:

1. **Pesquisa empírica obrigatória durante o `/code-mod`** — antes de codar os patches, listar os métodos da classe `Player.FirearmController` e suas operações:
   ```csharp
   var declaredOnFC = AccessTools.GetDeclaredMethods(typeof(Player.FirearmController));
   var nestedOpTypes = typeof(Player.FirearmController).GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
   ```

2. **Estratégia A — patchear a operação ativa, não FirearmController.** O caller-tree (line 4558 `FirearmController_0.CurrentOperation.SetTriggerPressed(pressed: true);`) confirma que a rota real do trigger é `FirearmController.CurrentOperation`. Patchear na **classe-base de operação** (a que tem `SetTriggerPressed` em `Player.cs:3810`) cobre todas as operações via dispatch virtual. Para resolver o `MethodBase` dessa classe nested:
   ```csharp
   protected override MethodBase GetTargetMethod()
   {
       // Achar a nested-class base que tem SetTriggerPressed virtual + back-ref FirearmController_0
       var fc = typeof(Player.FirearmController);
       var operationBase = fc.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
           .First(t => t.GetField("FirearmController_0", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) != null
                       && AccessTools.DeclaredMethod(t, "SetTriggerPressed", new[] { typeof(bool) }) != null);
       return AccessTools.DeclaredMethod(operationBase, "SetTriggerPressed", new[] { typeof(bool) });
   }
   ```
   E em vez de `OnFireEvent`/`InternalOnFireEvent` em FirearmController, mirar na mesma operation-base:
   ```csharp
   var fireEvent = AccessTools.DeclaredMethod(operationBase, "OnFireEvent", Type.EmptyTypes);
   // ou InternalOnFireEvent dependendo do que o snap precisa interceptar
   ```

3. **Estratégia B — reagir só no input do FirearmController.** Patchar `Player.FirearmController` *na sua própria SetTriggerPressed* — se ela não existe, ela é herdada de `ItemHandsController.SetTriggerPressed`. Encontrar a base concreta:
   ```csharp
   var baseSet = AccessTools.Method(typeof(Player.FirearmController), "SetTriggerPressed", new[] { typeof(bool) });
   // baseSet.DeclaringType é a classe que de fato declara — usar ela como target
   return baseSet;  // Harmony patcha o método declarado, dispatch virtual desce
   ```
   E filtrar no Postfix:
   ```csharp
   [PatchPostfix]
   private static void Postfix(object __instance, bool pressed)
   {
       if (__instance is not Player.FirearmController) return;
       StanceManager.OnTriggerPressed(pressed);
   }
   ```
   Para abortar o tiro, descobrir o método público da operação ativa (provavelmente `OnFireEvent` na operation-base) e patchar via Estratégia A.

4. **Adicionar §2.1 "Resolução por reflection"** com este código de descoberta documentado, e marcar **§5.4 e §5.5 como dependentes da resolução em runtime** (não hardcoded). Adicionar verificação `HasMissingReflection`-style no Awake (igual ao padrão já existente em [StanceManager.cs:690](../../modded/StanceManager.cs#L690)) que loga warning se algum target não foi encontrado, para o mod degradar graciosamente.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (Estratégia A — operação-base via reflection)
- `[ ]` Caminho alternativo: _________________

**Resolução:** Spec técnica atualizada — §2 e §5.4/§5.5 reescritos para resolver target via reflection na nested operation-base de `Player.FirearmController` (filtrar nested types pelo backing field `FirearmController_0` + método declarado `SetTriggerPressed(bool)`). `HasMissingReflection` no Awake loga warning se algum target não foi resolvido.

---

### PA-01-02 · A — Gap · 🔴 Bloqueador · ✅ Resolvido em 2026-05-10

**F4 race condition: tiro semi-auto sai antes do button-up para clique único**

**Problema:** O fluxo de F4 em §6 da spec técnica documenta o problema mas não o resolve:

> "Para semi-auto, o `InternalOnFireEvent` chamado em [4] geralmente acontece **antes** do button-up em [7], então o prefix em [5] deixa passar o tiro."

Mas a spec funcional (linha 107 de `002-…-spec.md`) é categórica:

> **"Clique único (pressionar e soltar rapidamente): nenhum tiro é disparado."**

A estratégia atual é:
1. Postfix `SetTriggerPressed(true)` → snap, marca `_snapPendingFromTrigger = true`, registra `_triggerDownTimeUnscaled`.
2. EFT segue o pipeline natural → fire-event dispara o tiro **imediatamente** para semi-auto.
3. Postfix `SetTriggerPressed(false)` → mede elapsed; se `< threshold`, marca `_abortNextFireEvent = true`.
4. **Próximo** fire-event é abortado.

Para semi-auto, o tiro do clique único já saiu na etapa 2 — `_abortNextFireEvent` aborta o tiro do **próximo clique**, gerando o bug oposto: o primeiro clique único atira (errado), e o próximo clique completo não atira (errado).

**Por que importa:** O critério de aceite "Em Stance 1 com `Snap to Stance 0 on Fire = true`, um clique único no gatilho não dispara e muda para Stance 0" (linha 163 da spec funcional) **não é satisfeito** pela implementação proposta. F4 não funciona para a maior parte das armas (todas as semi-auto + primeiro tiro de armas auto).

**Sugestão:** Trocar a estratégia para **bloquear o trigger no button-down e ressuscitá-lo no button-up se >= threshold**:

1. Prefix em `SetTriggerPressed(true)` quando em stance snap-elegível:
   - Em vez de deixar passar, **retorna `false` (skip original)** — o trigger NÃO é propagado para o operation, então nenhum tiro sai.
   - Snap imediato: `CurrentStance = Stance.Default`.
   - Registra `_triggerDownTimeUnscaled` e `_snapInterceptActive = true`.
2. Postfix em `SetTriggerPressed(false)`:
   - Se `!_snapInterceptActive`, no-op (clique normal já estava em Stance 0).
   - Senão, calcula elapsed.
     - Se `elapsed < threshold` → era clique único → nenhum tiro disparado, nada a fazer (já bloqueamos no button-down).
     - Se `elapsed >= threshold` → era hold → sintetizar o tiro: chamar `originalSetTriggerPressed(true)` no FirearmController (delegate cacheado) e logo em seguida deixar o button-up natural propagar.
   - Limpar `_snapInterceptActive`.

Pseudocódigo (resolução de operation-base via PA-01-01):

```csharp
// modded/Patches/SnapFireTriggerPatch.cs (revisado)
[PatchPrefix]
private static bool Prefix(object __instance, bool pressed)
{
    if (__instance is not Player.FirearmController fc) return true;
    if (pressed)
    {
        if (!StanceManager.ShouldInterceptTriggerDown(fc)) return true;
        StanceManager.OnTriggerInterceptedDown(fc);
        return false;          // bloqueia o trigger=true → operation nunca dispara
    }
    else
    {
        if (!StanceManager.WasTriggerIntercepted())
            return true;
        StanceManager.OnTriggerInterceptedUp(fc);  // pode chamar SetTriggerPressed(true) sintético se elapsed >= threshold
        return true;           // deixa o button-up natural propagar
    }
}
```

Onde `OnTriggerInterceptedUp` decide se ressuscita:

```csharp
public static void OnTriggerInterceptedUp(Player.FirearmController fc)
{
    _snapInterceptActive = false;
    float elapsedMs = (Time.unscaledTime - _triggerDownTimeUnscaled) * 1000f;
    int threshold = Plugin._SnapFireThreshold?.Value ?? 200;
    if (elapsedMs >= threshold)
    {
        // Sintetizar trigger-down via delegate cacheado do MethodInfo do prefix-skipped target
        // (resolvido no Awake do patch)
        SnapFireTriggerPatch.RaiseSyntheticTriggerDown(fc);
    }
    // Senão: clique único — nada acontece, snap já foi feito no down.
}
```

Considerações adicionais a documentar na spec:

- **Auto/burst**: button-down bloqueado significa que no `>= threshold`, ressuscitar `SetTriggerPressed(true)` reinicia o ciclo. Para auto, o trigger fica setado e a arma dispara em rajada normalmente (a operation continua em auto-fire enquanto IsTriggerPressed=true). Para burst, o EFT dispara a rajada no edge `false→true`, então sintetizar o `true` aciona a rajada.
- **Pressão sustentada cruzando o threshold**: o jogador pode segurar 199ms (sem tiro), 201ms ainda sem tiro (pois button-up não chegou). Só no button-up com elapsed >= threshold é que o tiro acontece. **Diferença com EFT vanilla:** o tiro acontece no release, não no press, quando o snap está ativo. Documentar como comportamento intencional ou ajustar.
- **Alternativa simples**: se o "tiro no release" for inaceitável, usar **timer assíncrono** — bloquear no down, agendar coroutine de threshold ms; se botão ainda pressionado quando expira, ressuscitar trigger-down; se já solto, no-op.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (intercept no down + ressurreição no up se >= threshold)
- `[ ]` Caminho alternativo (timer assíncrono / coroutine):  _________________
- `[ ]` Caminho alternativo (revisar spec funcional para aceitar tiro no down):  _________________

**Resolução:** Estratégia F4 reescrita em §5.3/§5.4/§6 da spec técnica. Prefix em `SetTriggerPressed(true)` retorna `false` (skip original) quando snap é elegível — bloqueia o trigger no down. No button-up, se `elapsed >= threshold`, sintetiza um trigger-down via delegate cacheado do `MethodInfo` original. Documentado no §6 que o tiro acontece no release (não no press) durante snap — comportamento intencional. AC explícita adicionada para verificação manual.

---

### PA-01-03 · C — Lógica · 🟡 Importante · ✅ Resolvido em 2026-05-10

**`RefreshScrollModeVisibility` depende de internals do ConfigurationManager**

**Problema:** A spec (§5.2) propõe forçar refresh do F12 chamando `BuildSettingList` via reflection:

```csharp
Type tCM = AccessTools.TypeByName("ConfigurationManager.ConfigurationManager");
if (tCM != null) {
    var inst = UnityEngine.Object.FindObjectOfType(tCM);
    AccessTools.Method(tCM, "BuildSettingList")?.Invoke(inst, null);
}
```

Três problemas:
1. **Não há garantia** de que ConfigurationManager esteja instalado — é mod separado (`com.bepis.bepinex.configurationmanager.cfg` está presente nesta instalação, mas isso não se generaliza para todo usuário).
2. **`BuildSettingList`** é internal/private; o nome pode mudar entre versões do CM. A spec não cita evidência empírica desse método existir.
3. **`FindObjectOfType`** é caríssimo (varre a scene) — não deve rodar a cada `SettingChanged` (toda mudança de config dispara isso).

**Por que importa:** Se CM ausente ou método renomeado, o critério de aceite "Trocar `Mouse Wheel Scroll Mode` com o F12 aberto oculta/exibe as propriedades dependentes imediatamente" não funciona — visibilidade só atualiza ao reabrir o F12. Pior: pode gerar `NullReferenceException` no `Invoke` se a tipagem mudar.

**Sugestão:** Tornar o refresh **defensivo + cacheado**:

1. Cachear o `MethodInfo` no Awake (uma vez):
   ```csharp
   private static MethodInfo _cmBuildSettingListMethod;
   private static UnityEngine.Object _cmInstance;

   private static void TryResolveConfigurationManager()
   {
       var tCM = AccessTools.TypeByName("ConfigurationManager.ConfigurationManager");
       if (tCM == null) return;
       _cmBuildSettingListMethod = AccessTools.Method(tCM, "BuildSettingList");
       _cmInstance = UnityEngine.Object.FindObjectOfType(tCM);
       if (_cmBuildSettingListMethod == null || _cmInstance == null)
           Logger.LogWarning("[F2] ConfigurationManager refresh indisponível — visibilidade só atualiza ao reabrir F12.");
   }
   ```
2. Em `RefreshScrollModeVisibility`, chamar só se ambos estão resolvidos:
   ```csharp
   try { _cmBuildSettingListMethod?.Invoke(_cmInstance, null); }
   catch (Exception ex) { Logger.LogError($"[F2] BuildSettingList falhou: {ex}"); }
   ```
3. **Atualizar AC de F2** na spec funcional: "se ConfigurationManager não estiver instalado, visibilidade atualiza só ao reabrir F12 — comportamento documentado, não bug." Ou marcar como dependência hard.
4. Adicionar §7.1 "Dependência de ConfigurationManager (F2)" na spec técnica.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** §5.2 e §7 da spec técnica atualizadas. `MethodInfo` e instância do CM cacheados no Awake via `TryResolveConfigurationManager`; `RefreshScrollModeVisibility` checa nulls e loga warning se CM ausente. Adicionada §7.1 documentando dependência soft do ConfigurationManager (visibilidade dinâmica é feature opcional; sem CM, atualiza ao reabrir F12).

---

### PA-01-04 · B — Edge · 🟡 Importante · ✅ Resolvido em 2026-05-10

**Hotkeys com mesma tecla — prioridade implementada é a oposta da especificada**

**Problema:** A spec funcional diz (linha 196):

> "Duas hotkeys de stance com a mesma tecla: a prioridade deve ser determinística e documentada (ex.: menor índice de stance tem prioridade…)."

Stub de §5.3 da spec técnica:

```csharp
TryHotkey(Plugin._Stance0Hotkey, Stance.Default);
TryHotkey(Plugin._Stance1Hotkey, Stance.Stance1);
TryHotkey(Plugin._Stance2Hotkey, Stance.Stance2);
TryHotkey(Plugin._Stance3Hotkey, Stance.Stance3);
```

Como `TryHotkey` chama `CurrentStance = …` cada vez que matcha, e os 4 são chamados em sequência, **o último que matcha sobrescreve o primeiro**. Resultado: maior índice de stance vence — **oposto** do que a spec sugere ("menor índice prioriza").

Pior — para o caso `Stance1Hotkey == Stance3Hotkey == O`: o callback de Stance1 muda CurrentStance para Stance1; em seguida o de Stance3 muda para Stance3; resultado final = Stance3. Dois efeitos colaterais (incluindo `OnStanceChanged` disparando 2x → som tocado 2x → snap re-aplicado se já estava em snap, etc.).

**Por que importa:** Comportamento não-determinístico no sentido contrário ao especificado, e dispara side-effects redundantes (`PlayStanceChangeSound`, `ApplyStaminaStance`).

**Sugestão:** Quebrar early na primeira hotkey que dispara, em ordem crescente de stance:

```csharp
private static void HandleStanceHotkeys()
{
    // ... guards de sprint/ADS ...

    if (TryHotkey(Plugin._Stance0Hotkey, Stance.Default)) return;
    if (TryHotkey(Plugin._Stance1Hotkey, Stance.Stance1)) return;
    if (TryHotkey(Plugin._Stance2Hotkey, Stance.Stance2)) return;
    if (TryHotkey(Plugin._Stance3Hotkey, Stance.Stance3)) return;
}

private static bool TryHotkey(ConfigEntry<KeyCode> entry, Stance target)
{
    var key = entry?.Value ?? KeyCode.None;
    if (key == KeyCode.None) return false;
    if (!UnityEngine.Input.GetKeyDown(key)) return false;

    if (CurrentStance == target)
    {
        if (target != Stance.Default) CurrentStance = Stance.Default;
        return true;
    }
    CurrentStance = target;
    return true;
}
```

E adicionar AC explícita: "Se Stance1Hotkey == Stance3Hotkey == X, pressionar X ativa Stance 1 (menor índice prioriza)."

Adicionar tarefa em §8.4 (checklist F3): "garantir early-return em `TryHotkey`."

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (early-return na primeira hotkey)
- `[ ]` Caminho alternativo (warn no Awake e ignorar duplicadas): _________________

**Resolução:** Stub §5.3 atualizado — `TryHotkey` retorna `bool`; `HandleStanceHotkeys` faz `if (TryHotkey(...)) return;` em ordem Stance0→1→2→3, garantindo prioridade do menor índice. AC adicionada à spec funcional via nota cruzada e checklist §8.4 estendido.

---

### PA-01-05 · B — Edge · 🟡 Importante · ✅ Resolvido em 2026-05-10

**Snap state pode vazar em troca de arma durante a raid**

**Problema:** Spec funcional, corner case linha 183:

> "Troca de arma em stance não-padrão: trocar de arma enquanto snap for pendente não deve causar snap residual na nova arma."

Estado de F4 em `StanceManager` (`_triggerDownTimeUnscaled`, `_snapPendingFromTrigger`, `_abortNextFireEvent`) só é limpo via `ResetState()` — chamado em `OnRaidEnd`, não em troca de arma. Cenários problemáticos:

- Jogador em Stance 1, pressiona fogo (button-down) → snap → `_snapPendingFromTrigger = true`.
- Antes do button-up, jogador troca para arma branca via tecla.
- `HandsController` muda; o `SetTriggerPressed(false)` da arma antiga nunca chega (operation foi destruída).
- `_snapPendingFromTrigger` permanece `true`. Próxima vez que voltar para arma: estado stale.

Idem para `_abortNextFireEvent = true` setado mas o "próximo fire-event" não chega — flag persiste e aborta o primeiro tiro da próxima arma.

**Por que importa:** Bug raro mas real. O usuário troca de arma enquanto segura o gatilho (cenário comum em loadouts pistola+rifle) e perde o primeiro tiro da nova arma.

**Sugestão:** Detectar troca de `HandsController` e resetar flags de snap. Duas formas:

1. **Hook no `HandsController` change** — patchear o setter de `Player.HandsController` e disparar reset:
   ```csharp
   if (StanceManager._lastFirearmController != null && hc != _lastFirearmController)
       StanceManager.ClearSnapInterceptState();
   ```
   Reusa a tracking de `_lastFirearmController` que já existe em [StanceManager.cs:63](../../modded/StanceManager.cs#L63).

2. **Stale-time guard** — no `Update()` do StanceManager, se `_snapPendingFromTrigger == true && (Time.unscaledTime - _triggerDownTimeUnscaled) > 2s`, limpar flags. Defesa em profundidade — cobre qualquer caminho que destrua a operation sem button-up.

**Recomendação:** Aceitar (2) como mínimo (simples, defensivo). (1) opcional para zero latência.

Adicionar tarefa em §8.5 (checklist F4): "limpar `_snapPendingFromTrigger`/`_abortNextFireEvent` quando `_lastFirearmController` muda OU em stale-timeout."

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (stale-timeout)
- `[ ]` Aceitar sugestão estendida (HandsController change hook + stale-timeout)
- `[ ]` Caminho alternativo: _________________

**Resolução:** §5.3 da spec técnica adiciona stale-timeout (2s) — no Update do StanceManager, se `_snapInterceptActive == true && (Time.unscaledTime − _triggerDownTimeUnscaled) > 2f`, limpa flags. Defesa em profundidade que cobre weapon swap durante hold sem precisar patchear o setter de HandsController. Checklist §8.5 atualizado.

---

### PA-01-06 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-05-10

**`_snapPendingFromTrigger` redundante — duplica a fonte de verdade do timer**

**Problema:** Em §5.3 da spec técnica, há três campos relacionados:
- `_triggerDownTimeUnscaled` — quando o button-down aconteceu
- `_snapPendingFromTrigger` — bool dizendo "tem snap pendente"
- `_abortNextFireEvent` — bool dizendo "abortar próximo fire"

O bool `_snapPendingFromTrigger` é setado/checado apenas para gating a lógica do button-up. Mas o button-up só aciona se o button-down rodou — `_triggerDownTimeUnscaled > 0` ou usar um sentinel `-1` já cobre. O bool é redundante.

Se PA-01-02 mudar a estratégia para intercept-and-resurrect, isto vira moot. Mas mantém a observação para o caso de a estratégia atual ficar.

**Por que importa:** Estado redundante = mais um campo a limpar em ResetState, mais uma fonte de bug futuro. Trivial agora, dor depois.

**Sugestão:** Remover `_snapPendingFromTrigger`. Usar `_triggerDownTimeUnscaled > 0` (com `-1f` como sentinel "sem button-down ativo") OU consolidar tudo num enum `SnapState { Idle, Pressed, Released }`.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (consolidar via sentinel ou enum)
- `[ ]` Não vale o esforço
- `[x]` Resolvido por consequência: PA-01-02 reescreveu F4 com `_snapInterceptActive` (uma única bool de estado) + `_triggerDownTimeUnscaled` (timer). `_snapPendingFromTrigger` deixa de existir.

**Resolução:** Eliminado pela reescrita de F4 em PA-01-02. O novo modelo de estado tem só `_snapInterceptActive` (bool) e `_triggerDownTimeUnscaled` (sentinel `-1f` quando inativo) — sem campo redundante.

---

### PA-01-07 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-05-10

**Confirmar explicitamente que o som de troca de stance toca no snap**

**Problema:** A spec funcional (linha 203) marca como **fora de escopo**:

> "Efeito sonoro ou visual específico para o snap de stance (além da transição já existente)."

Mas a spec técnica não esclarece se o som *atual* de troca de stance ([SpringGetPatch.cs:466 — `PlayStanceChangeSound`](../../modded/Patches/SpringGetPatch.cs#L466)) toca quando o snap muda CurrentStance. Como o snap chama `CurrentStance = Stance.Default`, `OnStanceChanged` dispara, e o `SpringGetPatch` detecta `stanceChanged && isHoldingFirearm && !isAiming` → toca o som. Então o som toca, sim — mas pode ser surpreendente para o jogador (clica para atirar, ouve "clack" de stance change).

**Por que importa:** Comportamento esperado vs. implementado pode divergir do que o jogador imagina (UX). Não é bug, mas o checklist precisa explicitar para evitar "feature ou bug?" depois.

**Sugestão:** Adicionar nota explícita em §1 (Estratégia) ou em §6 (fluxo de F4):

> "O som de troca de stance (existente) **toca** ao snap, porque o snap muda `CurrentStance` via setter, e o `SpringGetPatch` detecta a mudança e toca `PlayAimingSound`. Isto é intencional — o snap é uma troca de stance comum em todos os outros aspectos."

Adicionar AC em §8.8 (verificação manual): "som de stance change toca uma vez por snap (não duplicado)."

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (apenas documentar)
- `[ ]` Caminho alternativo (silenciar som no caminho de snap): _________________

**Resolução:** Nota explícita adicionada em §1 da spec técnica e AC adicionada em §8.8 ("som de stance change toca uma vez por snap").

---

### PA-01-08 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-05-10

**Checklist 8.3 omite captura dos `ConfigurationManagerAttributes` de `_EnableStance1/2/3`**

**Problema:** §5.2 da spec técnica declara campos privados:

```csharp
private static ConfigurationManagerAttributes _attrIncludeStance0;
private static ConfigurationManagerAttributes _attrEnableStance1Cycle;
private static ConfigurationManagerAttributes _attrEnableStance2Cycle;
private static ConfigurationManagerAttributes _attrEnableStance3Cycle;
```

E o `RefreshScrollModeVisibility` muta `Browsable` neles. Mas o checklist §8.3 só diz "Bind de `_MouseWheelScrollMode`…" e "Manter referências aos `ConfigurationManagerAttributes` de `_IncludeStance0InCycle` e `_EnableStance1/2/3` em campos privados de `Plugin`." — sem explicar **como**.

A captura precisa acontecer no momento do `Config.Bind` das ConfigEntries existentes — `_EnableStance1/2/3` foram bindados em [Plugin.cs:198-220](../../modded/Plugin.cs#L198) **antes** desta feature. O dev precisa reescrever os 3 binds existentes para extrair o `ConfigurationManagerAttributes` numa variável local antes de passar para `ConfigDescription`. A spec não diz isso explicitamente.

**Por que importa:** Implementador pode esquecer de capturar referências para `_EnableStance1/2/3` (já existentes) — só captura para o novo `_IncludeStance0InCycle`. Resultado: F2 funciona em parte (oculta `Include Stance 0` mas não os 3 toggles antigos).

**Sugestão:** Reescrever §8.3 com tarefas explícitas:

```markdown
- [ ] Modificar binds **existentes** de `_EnableStance1`, `_EnableStance2`, `_EnableStance3` em [Plugin.cs:198-220](../../modded/Plugin.cs#L198) — extrair o `ConfigurationManagerAttributes { Order = NN }` para variável local **antes** de passar ao `ConfigDescription`, e armazenar nos campos `_attrEnableStance1Cycle/2/3` da `Plugin`.
- [ ] Idem ao bindar o **novo** `_IncludeStance0InCycle` — capturar em `_attrIncludeStance0`.
- [ ] Implementar `RefreshScrollModeVisibility` mutando os 4 attrs.
- [ ] Subscribe `_MouseWheelScrollMode.SettingChanged` e `_EnableMouseWheelCycle.SettingChanged`.
- [ ] Chamar `RefreshScrollModeVisibility()` uma vez ao final do `Awake()` para inicializar visibilidade.
```

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (refinar checklist 8.3)
- `[ ]` Caminho alternativo: _________________

**Resolução:** Checklist §8.3 da spec técnica reescrita com 5 tarefas explícitas (modificar binds existentes, capturar nos campos `_attrEnableStance1Cycle/2/3`, idem para `_attrIncludeStance0`, implementar `RefreshScrollModeVisibility`, subscribe e chamada inicial).

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-05-10 | Review 01 criada — 2 bloqueadores (patches de F4 + race condition do snap), 3 importantes (CM dependency, hotkey priority, snap state leak), 3 menores. |
| 2026-05-10 | Todas as 8 sugestões aceitas pelo usuário; spec técnica atualizada com as resoluções (§2/§5.2/§5.3/§5.4/§5.5/§6/§7.1/§8). PA-01-06 fechado por consequência da reescrita de PA-01-02. |
