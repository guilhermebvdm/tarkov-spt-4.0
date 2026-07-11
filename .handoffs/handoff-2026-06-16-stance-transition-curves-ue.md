# Handoff — Tunar a movimentação da arma na troca de stance + testar no UnityExplorer

> **Data:** 2026-06-16
> **Mod:** `mods/stancesAndCameraPositionSPT4.0.11`
> **Para:** sessão de IA do outro dev (co-autor do mod)
> **Stack:** SPT 4.0 / EFT 0.16.x, BepInEx 5 (Mono), Harmony via `SPT.Reflection.Patching.ModulePatch`

---

## 🎯 Objetivo (o que queremos fazer)

Ajustar a **animação procedural da arma durante a transição entre stances** — o "kick"/overshoot que acontece nos ~0.35s após trocar de postura (tecla V, scroll, ou hotkey dedicada). Os valores que controlam isso estão **hardcoded** em [StanceTransitionCurves.cs](../mods/stancesAndCameraPositionSPT4.0.11/modded/StanceTransitionCurves.cs).

A meta imediata é **iterar nesses valores com o UnityExplorer (UE)** — observar ao vivo o que é gerado na troca, simular sem recompilar, achar bons números, e só então transcrevê-los de volta ao código. O dono da sessão (Guilherme) está usando o UE no jogo e quer um loop de iteração rápido.

**Decisão em aberto (pedir ao usuário):** transformar as curvas em `ConfigEntry` editáveis no F12 (ConfigurationManager) **ou** manter hardcoded e só ajustar os valores? São ~10 keyframes; virar config dá trabalho mas libera tuning in-game sem rebuild. Default sugerido: prototipar via UE primeiro (abaixo), decidir depois.

---

## 🧩 Como a transição funciona (arquitetura)

O `StanceTransitionCurves` **não é lido pelo EFT** — é consumido pelo patch da mola das mãos:

- **Patch:** [SpringGetPatch.cs](../mods/stancesAndCameraPositionSPT4.0.11/modded/Patches/SpringGetPatch.cs) — Postfix em `EFT.Animations.Spring.Get()` (a mola de `ProceduralWeaponAnimation.HandsContainer.HandsRotation` e `.HandsPosition`).
- **Disparo:** em [SpringGetPatch.cs:632-637](../mods/stancesAndCameraPositionSPT4.0.11/modded/Patches/SpringGetPatch.cs#L632-L637), troca de stance liga `_isPlayingTransitionCurve = true` e zera o timer.
- **Avaliação:** [SpringGetPatch.cs:651-669](../mods/stancesAndCameraPositionSPT4.0.11/modded/Patches/SpringGetPatch.cs#L651-L669) — `progress = _transitionCurveTimer / 0.35f`. **A duração de 0.35s é hardcoded AQUI**, não no StanceTransitionCurves.
- **Aplicação:** o resultado é **somado** ao offset final da mola em [linhas 694 e 698](../mods/stancesAndCameraPositionSPT4.0.11/modded/Patches/SpringGetPatch.cs#L694-L698).

### As 5 curvas (eixos)

`EvaluateRotation` → `Vector3` em **graus**; `EvaluatePosition` → translação local em **unidades Unity (~metros)**:

| Curva | Eixo | Efeito | Unidade |
|---|---|---|---|
| `PitchXCurve` | rot.X | cano sobe/desce | graus |
| `YawYCurve` | rot.Y | arma tomba lateral | graus |
| `RollZCurve` | rot.Z | cant (inclinar) | graus |
| `PosXCurve` | pos.X | coronha afasta/aproxima do corpo | metros |
| `PosZCurve` | pos.Z | coronha frente/trás | metros |

`EvaluatePosition` força **pos.Y = 0** ([linha 96](../mods/stancesAndCameraPositionSPT4.0.11/modded/StanceTransitionCurves.cs#L96)) — não há curva de cima/baixo de posição. Criar `PosYCurve` se precisar.

Cada `Keyframe(tempo, valor)`: tempo é progresso normalizado `0→1`; valor é o offset. O padrão usado é **0 → pico → contra-pico → 0** (sensação elástica). `SmoothCurve` aplica `SmoothTangents` para virar spline.

### ⚠️ 4 camadas somadas na MESMA mola

O `__result` final empilha quatro sistemas independentes — não confunda um com o outro ao depurar:

1. `_currentRotation` / `_currentPosition` — transição-base suave (SmoothDamp) para a pose-alvo da stance (valores por stance no [Plugin.cs](../mods/stancesAndCameraPositionSPT4.0.11/modded/Plugin.cs)).
2. `_currentShoulderingRotation` — "shoulder throw" do Advanced ADS (só se ligado no F12).
3. `_currentWiggleRotation` / `_currentWigglePosition` — wiggle aleatório (item 009).
4. `_currentCurveRotation` / `_currentCurvePosition` — **estas curvas** (o foco deste handoff).

Se mexer nas curvas e "nada mudar", provavelmente outra camada domina, ou o efeito é sutil (valores pequenos). Confira qual sistema está ativo antes de culpar a curva.

---

## 🔬 Passo a passo — testar no UnityExplorer (UE)

> UE = UnityExplorer (sinai-dev), variante **BepInEx 5 Mono**, em `D:/SPT/BepInEx/plugins/`. Menu default: **F7**. Abas: Object Explorer · Inspector · C# Console · Hooks · Freecam · Clipboard · Log · Options. Barra superior tem o slider **`Time:`** (timeScale) e **`Lock`**.

**Princípio central:** quase todo o estado da transição vive em **classes estáticas com campos estáticos**, e o UE lê `private static` por reflexão. Não precisa caçar GameObject — basta abrir as classes do mod com Auto-update ligado.

### Mapa de observação

| O que ver | Onde (busca no UE) | Tipo |
|---|---|---|
| Stance ativa | `CameraRotationMod.StanceManager` → `CurrentStance` | Static class |
| **Offsets gerados frame a frame** | `CameraRotationMod.Patches.SpringGetPatch` → `_currentCurveRotation`, `_currentCurvePosition`, `_currentRotation`, `_currentPosition`, `_currentWiggleRotation`, `_currentShoulderingRotation`, `_isPlayingTransitionCurve`, `_transitionCurveTimer` | Static class |
| Alvo da stance | `StanceManager` → `_cachedStance1Rotation`, `_cachedStance2Rotation`… | Static (private) |
| As curvas | `shwngFpsCameraStances.StanceTransitionCurves` → `PitchXCurve`… | Static class |
| Mola real do EFT (valor BASE, sem o offset do mod) | `MainPlayer.ProceduralWeaponAnimation.HandsContainer.HandsRotation` / `.HandsPosition` | instância `Spring` |
| Estado mira/mount | `ProceduralWeaponAnimation.IsAiming`, `.AimingSpeed`, `.IsMountedState` | instância |

> **Ouro:** `_currentCurveRotation` e `_currentCurvePosition` no `SpringGetPatch` são *exatamente* o output das suas curvas. Observar esses dois = ver número a número o efeito da edição. O offset do mod **não** está em `Spring.Current` (essa é a mola base do EFT) — só no retorno de `Get()` e nesses campos estáticos.

### Procedimento

1. **F7** abre o UE. Vá em **Object Explorer → Object Search**, modo de busca **Class / Static** (não "Unity Object"). Digite `SpringGetPatch` → **Inspect**. Repita para `StanceManager` e `StanceTransitionCurves`.
   - Para o EFT real: modo **Singleton** → `GameWorld` → navegue `MainPlayer → ProceduralWeaponAnimation → HandsContainer → HandsRotation`.
2. No Inspector, ligue **`Auto-update`** (topo, ao lado de "Update displayed values"). Use os filtros **`Static` + `Field`** para reduzir ruído.
3. **Câmera lenta (essencial):** a transição dura só 0.35s. Baixe o slider **`Time:`** da barra superior para **0.1 ou 0.05**. A troca roda em slow-mo e dá pra ler os `Vector3` evoluindo. Use **`Lock`** para travar o cursor no UE.
4. Fique **parado, em hipfire** (ver caveat) e troque de stance (V). Observe no `SpringGetPatch`:
   - `_isPlayingTransitionCurve` → `True` por ~0.35s,
   - `_transitionCurveTimer` subindo 0 → 0.35,
   - `_currentCurveRotation`/`_currentCurvePosition` desenhando o kick e voltando a 0.

### Simular e tunar SEM recompilar — aba C# Console

`StanceManager.SetStance` é `public static`, e as curvas são `public static AnimationCurve` → dá pra forçar a troca e reescrever keyframes ao vivo (só em memória; some ao fechar o jogo):

```csharp
// força a troca (dispara as curvas, sem a tecla V)
CameraRotationMod.StanceManager.SetStance(CameraRotationMod.Stance.Stance1);

// lê estado / avalia a curva num ponto
return CameraRotationMod.StanceManager.CurrentStance;
return shwngFpsCameraStances.StanceTransitionCurves.EvaluateRotation(0.35f);

// reescreve keyframes ao vivo (ex.: kick de pitch mais forte)
var c = shwngFpsCameraStances.StanceTransitionCurves.PitchXCurve;
c.keys = new UnityEngine.Keyframe[] {
    new UnityEngine.Keyframe(0f, 0f),
    new UnityEngine.Keyframe(0.35f, 5f),
    new UnityEngine.Keyframe(0.75f, -2f),
    new UnityEngine.Keyframe(1f, 0f),
};
for (int i = 0; i < c.length; i++) c.SmoothTangents(i, 0f);
```

Loop: reescreve → troca de stance → sente → ajusta. Achou bom → **transcreva para o [StanceTransitionCurves.cs](../mods/stancesAndCameraPositionSPT4.0.11/modded/StanceTransitionCurves.cs) e recompile.**

### Hooks (ver "quando" e "com quais valores")

Aba **Hooks** → `CameraRotationMod.StanceManager` → método `SetStance` ou `OnStanceChanged`. Cada troca vira linha no Log; edite o source do hook gerado para imprimir args.
**Não** hooke `Spring.Get` — alta frequência (várias vezes/frame), inunda o log. Para a mola use Auto-update, não hook.

### Caveats

- **O `Update()` do mod pode reverter a stance:** se estiver sprintando, mirando (ADS), montado ou deitado, [StanceManager.Update](../mods/stancesAndCameraPositionSPT4.0.11/modded/StanceManager.cs#L163) força `Default`. **Teste parado, em hipfire.**
- Campos `private static` aparecem no UE — use filtros `Static`+`Field`.
- `Spring.Current` é a mola **base do EFT**; o offset do mod só existe no retorno de `Get()` e nos `_current*` do `SpringGetPatch`.

---

## 🛠️ Build

```
/compile-mod stancesAndCameraPositionSPT4.0.11 --flat
```
Saída: `D:/SPT/BepInEx/plugins/shwngFpsCameraStances4.dll` (flat, sem subpasta — compat com instalação anterior). References resolvidas automaticamente do install em `D:/SPT`.

⚠️ Builds locais de mod client podem ser revertidas pelo sync do launcher (Dev Mod off) — confirmar que a DLL foi para o servidor. Validar a mudança **no jogo**, não só por compile/hash.

---

## 📌 Comandos/keybinds do mod (referência rápida)

Definidos no [Plugin.cs](../mods/stancesAndCameraPositionSPT4.0.11/modded/Plugin.cs) (seção `Settings`):
- **V** — cicla stances (`_StanceToggleKey`).
- **LeftAlt + scroll** — cicla/eixo linear (se `_EnableMouseWheelCycle`; modo em `_MouseWheelScrollMode`, default Linear).
- Hotkeys dedicadas: Stance 0/1/2 = `None` por default; **Stance 3 = O**.
- **Mouse3** — mount de arma (`_MountingHotkey`).
- Manual chambering — tecla nativa `ECommand.ChamberUnload`.

Layout de stances (06-fix-01): Stance 1 = High Ready · Stance 2 = Low Ready · Stance 3 = Custom (lateral).

---

## 🤝 Suggested skills (próxima sessão)

- **`graph-code-navigation`** — para achar classes/métodos do EFT (ex.: `ProceduralWeaponAnimation`, `Spring`) via grafos em `references/graphs/` antes de Grep manual.
- **`spt-mod-best-practices`** + **`csharp-mod-best-practices`** — validar lifecycle/Harmony/Unity ao editar patches.
- **`compile-mod`** — build (comando acima).
- **`g-diagnose`** — se a movimentação bugar/glitchar (loop de reprodução → instrumentação).

## 📁 Artefatos por path (não duplicados aqui)

- Curvas: [StanceTransitionCurves.cs](../mods/stancesAndCameraPositionSPT4.0.11/modded/StanceTransitionCurves.cs)
- Patch que consome: [SpringGetPatch.cs](../mods/stancesAndCameraPositionSPT4.0.11/modded/Patches/SpringGetPatch.cs)
- Estado/input de stance: [StanceManager.cs](../mods/stancesAndCameraPositionSPT4.0.11/modded/StanceManager.cs)
- Config/keybinds: [Plugin.cs](../mods/stancesAndCameraPositionSPT4.0.11/modded/Plugin.cs)
- UnityExplorer: sinai-dev/UnityExplorer (GitHub) — variante BIE5 Mono.
