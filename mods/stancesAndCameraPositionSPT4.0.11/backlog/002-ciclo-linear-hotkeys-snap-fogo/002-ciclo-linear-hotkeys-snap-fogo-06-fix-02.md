# 002 — Fix 02 · Hotkey labels + ordem F12 da seção Stance 0

**Mod:** stancesAndCameraPositionSPT4.0.11
**Item raiz:** [002-ciclo-linear-hotkeys-snap-fogo-01-spec.md](002-ciclo-linear-hotkeys-snap-fogo-01-spec.md)
**Asbuild:** [002-ciclo-linear-hotkeys-snap-fogo-05-asbuild.md](002-ciclo-linear-hotkeys-snap-fogo-05-asbuild.md)
**Fix anterior:** [002-ciclo-linear-hotkeys-snap-fogo-06-fix-01.md](002-ciclo-linear-hotkeys-snap-fogo-06-fix-01.md)
**Data:** 2026-05-10
**Disparado por:** Feedback do usuário pós 06-fix-01 com 3 screenshots in-game.

## Contexto

Após `/compile-mod` do 06-fix-01, o usuário abriu F12 e identificou 3 problemas residuais (screenshots em [assets/](../../assets/)):

1. **Hotkey labels desatualizadas (Settings section):**
   - F12 mostra "Stance 2 - **Custom** Hotkey" mas o slot Stance.Stance2 agora ativa Low Ready (Pitch +30°).
   - F12 mostra "Stance 3 - **Low Ready** Hotkey" mas o slot Stance.Stance3 agora ativa Custom (Yaw -30°).
   - Resultado: usuário aperta a tecla que diz "Custom" e a arma vai pra Low Ready — confusão.

2. **Ordem das seções no F12 — "Stance 0 - Vanilla" aparece NO FIM** (depois de Tac Sprint, FOV, Debug). Usuário esperava ela acima de "Stance 1 - High Ready".

3. **Hotkey ConfigDescription tooltips** ainda referenciam os nomes antigos ("activate Stance 2 - Custom" / "activate Stance 3 - Low Ready").

## Causas raiz

### Bug 1+3: literais não atualizados em `_Stance2Hotkey` / `_Stance3Hotkey`

No 06-fix-01, eu atualizei as **constantes** `Stance2Section` / `Stance3Section` (linhas 62-63 do Plugin.cs) e os binds das hand rotations dentro de cada seção. Mas as hotkeys (linhas 399-419) usam **strings literais** porque ficam na seção `Settings`, não em `Stance2Section`/`Stance3Section`:

```csharp
_Stance2Hotkey = Config.Bind(
    Settings,                       // ← seção genérica, não a constant
    "Stance 2 - Custom Hotkey",     // ← STRING LITERAL, era para virar "Low Ready"
    ...
    "Dedicated key to activate Stance 2 - Custom. ..."  // tooltip também
);
```

Esqueci de fazer a substituição manual nessas 2 entries.

### Bug 2: ordem das seções no F12 — Stance 0 sem entries de alto Order

ConfigurationManager v18+ ordena seções pelo **MAX Order de qualquer entry** que pertença a elas (DESC). No mod:

| Seção | Max Order da seção | Origem |
| --- | --- | --- |
| Stance 1 - High Ready | 27 | hand rotation entries |
| Stance 2 - Low Ready | 20 | hand rotation entries |
| Stance 3 - Custom | 14 | hand rotation entries |
| Tac Sprint Settings | 7 | bind explícito |
| **Stance 0 - Vanilla** | **5** | só de `BindStance` (stamina/speed/etc.) |
| FOV / Debug | menor | bind explícito |

A seção "Stance 0 - Vanilla" só recebe entries vindas do helper `BindStance` (que dá Order 5/3/2/1/0). Como Stance 0 não tem hand rotations (sem deslocamento — é a posição vanilla), nenhuma entry alta sustenta a seção. Resultado: cai depois de Tac Sprint/FOV/Debug.

## Mudanças aplicadas

### 1. Plugin.cs — labels e tooltips das hotkeys (Bug 1+3)

[Plugin.cs:399-419](../../modded/Plugin.cs#L399):

```diff
  _Stance2Hotkey = Config.Bind(
      Settings,
-     "Stance 2 - Custom Hotkey",
+     "Stance 2 - Low Ready Hotkey",
      KeyCode.None,
      new ConfigDescription(
-         "Dedicated key to activate Stance 2 - Custom. " +
+         "Dedicated key to activate Stance 2 - Low Ready. " +
          "Toggle: pressing while already in Stance 2 returns to Stance 0. " +
          ...

  _Stance3Hotkey = Config.Bind(
      Settings,
-     "Stance 3 - Low Ready Hotkey",
+     "Stance 3 - Custom Hotkey",
      KeyCode.O,
      new ConfigDescription(
-         "Dedicated key to activate Stance 3 - Low Ready. " +
+         "Dedicated key to activate Stance 3 - Custom. " +
          ...
```

### 2. Plugin.cs — BindStance Order parameterizado (Bug 2)

[Plugin.cs:1139](../../modded/Plugin.cs#L1139): a constante `Order = 5` para `StaminaMultiplier` virou variável `orderBase`, que vale **35 para `Stance.Default`** e **5 para as outras**.

```csharp
int orderBase = d.Stance == Stance.Default ? 35 : 5;

return new StanceConfig
{
    StaminaMultiplier = Config.Bind(d.Section, ...,
        new ConfigurationManagerAttributes { Order = orderBase }),    // 35 ou 5
    ModifiesMovementSpeed = Config.Bind(d.Section, ...,
        new ConfigurationManagerAttributes { Order = orderBase - 2 }), // 33 ou 3
    MovementSpeedMultiplier = Config.Bind(...,
        new ConfigurationManagerAttributes { IsAdvanced = true, Order = orderBase - 3 }), // 32 ou 2
    ApplyWhenProne = Config.Bind(...,
        new ConfigurationManagerAttributes { IsAdvanced = true, Order = orderBase - 4 }), // 31 ou 1
    ...
};
```

- Para `Stance.Default`: entries com Order 35/33/32/31 → **max Order 35** → seção flutua **acima** de "Stance 1 - High Ready" (max 27).
- Para `Stance.Stance1/2/3`: Order interno 5/3/2/1 (igual antes) — irrelevante para ordem da seção porque o MAX já vem das hand rotations (27/20/14).

Resultado esperado no F12: **Stance 0 - Vanilla** aparece no topo do bloco de stances.

### 3. PROPRIEDADES.md — labels swap

[PROPRIEDADES.md:41-42](../../PROPRIEDADES.md): `Stance 2 - Custom Hotkey` → `Stance 2 - Low Ready Hotkey`, idem para Stance 3. Tooltips swap.

## Migração de `.cfg`

Mais uma rodada de breaking change. Após boot:

- Entries antigas no `.cfg`:
  - `[Settings] Stance 2 - Custom Hotkey = Caps Lock` ← órfã
  - `[Settings] Stance 3 - Low Ready Hotkey = O` ← órfã
- Entries novas (recriadas com default):
  - `[Settings] Stance 2 - Low Ready Hotkey = None` ← perdeu Caps Lock
  - `[Settings] Stance 3 - Custom Hotkey = O` ← preserva O (era o default)

**Para preservar a Caps Lock que o usuário configurou na hotkey 2:** abrir `BepInEx/config/shwng.camerarotation.cfg`, copiar `= Caps Lock` da entry antiga (`Stance 2 - Custom Hotkey`) para a nova (`Stance 2 - Low Ready Hotkey`), salvar. Reboot.

OU mais fácil: depois do boot, reconfigurar via F12 (clicar em "Set..." na nova entry e pressionar Caps Lock).

## Limitação reconhecida

O usuário pediu originalmente que **Stance 2 (Low Ready) aparecesse ACIMA de Stance 1 (High Ready)** no F12. Não atendemos isso — a opção que escolhemos foi "aceitar ordem alfabética", então Stance 2 fica entre Stance 1 e Stance 3.

Este fix-02 endereça apenas a **ordem de Stance 0** (que ficou perdida atrás de FOV/Debug). Se mais tarde o usuário quiser revisitar a ordem Low Ready vs High Ready, abre um novo fix com a estratégia "Stance 02" alfabético OU renumeração total do enum.

## O que NÃO mudou

- Comportamento das hotkeys em runtime: já estava correto pós 06-fix-01 (Stance.Stance2 = Low Ready axis, Stance.Stance3 = Custom axis). Apenas os labels visíveis no F12 estavam desatualizados.
- Stance 1/2/3 entries de hand rotation: Order preservado (27-8). Visual interno das seções inalterado.
- F4 (patch target em `Player.FirearmController.SetTriggerPressed`): preservado de 06-fix-01.
- Swap de defaults de axis/stamina/speed/snap: preservado de 06-fix-01.

## Como verificar

1. **Hotkey labels:**
   - F12 → seção `Settings` → ver "Stance 2 - **Low Ready** Hotkey" e "Stance 3 - **Custom** Hotkey" (era inverso antes).
   - Hotkey de Stance 2 (qualquer tecla configurada) → arma vai pra Low Ready (Pitch +30°, cano desce). ✓
   - Hotkey de Stance 3 (`O` ou outra) → arma vai pra Custom (Yaw -30°, lateral). ✓
2. **Ordem F12:** abrir F12 e rolar até as seções de stance. **Stance 0 - Vanilla** deve aparecer **antes** de "Stance 1 - High Ready" (não mais depois de Debug).
3. **Caps Lock preservado** (se migrou manualmente o `.cfg`) OU re-bind via F12.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-05-10 | Fix 02 criado — 2 labels de hotkey atualizadas (Stance 2 / Stance 3) + BindStance helper parameterizado para forçar Stance 0 no topo do F12. Disparado por feedback do usuário com 3 screenshots in-game. |
