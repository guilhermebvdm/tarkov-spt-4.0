# 014 — Fix 03 · Aplicar na janela PRÉ-IK (Postfix de ShiftWeaponRoot) para o braço acompanhar

**Mod:** stancesAndCameraPositionSPT4.0.11
**Item raiz:** [014-sync-stances-fika-01-spec.md](014-sync-stances-fika-01-spec.md)
**Fix anterior:** [06-fix-02](014-sync-stances-fika-06-fix-02.md) (Postfix de ObservedVisualPass — moveu a arma, mas o braço não seguiu)
**Criado:** 2026-07-09
**Disparado por:** validação in-game do fix-02 — **a arma se move (correto), mas o braço/mão não acompanha** (a arma descola da mão). Inverso do sintoma original (antes: braço movia, arma não).

## Investigação (2 sub-agents independentes, Assembly + Fika)

Mapeada a cadeia braço→mão→arma do observado em `ObservedPlayer.ObservedVisualPass` ([ObservedPlayer.cs:1839-1924](../../../../references/fika-plugin/Fika.Core/Main/Players/ObservedPlayer.cs#L1839)):

```
1876 ShiftWeaponRoot(ThirdPerson)   → posiciona Weapon_Root_Anim (SetPositionAndRotation, valor absoluto)
1884 method_20                      → seta o ALVO da LimbIK das mãos lendo os markers da arma (weapon_L/R_IK_marker)
1886 method_19                      → SOLVE da LimbIK: dobra o braço até o marker
1889 Kinematics(_observedMarkers[1])→ cola a ARMA na palma (RightPalm) já resolvida
1918 LateTransformations / ManualLateUpdate
```

### Causa raiz definitiva
- A **arma é filho** do `Weapon_Root_Anim` ([ObservedPlayer.cs:1902](../../../../references/fika-plugin/Fika.Core/Main/Players/ObservedPlayer.cs#L1902) `GetChild(0)`); o **braço é um ramo separado**, acoplado à arma **só por IK**, cujo alvo são os markers `weapon_L/R_IK_marker` ([TransformLinks.cs:92-100](../../../../references/eft-decompiled/Assembly-CSharp/TransformLinks.cs#L92)) — **filhos do `Weapon_Root_Anim`**.
- O fix-02 aplicava o offset num **Postfix de `ObservedVisualPass`**, que roda **depois** da IK (method_19, 1886) e do Kinematics (1889). Nesse ponto o braço **já foi solveado** para os markers na pose sem offset; mover o `Weapon_Root_Anim` no fim só desloca a arma (filho) → **descola da mão**.
- No jogador **local** o offset entra **antes** da IK (via `Offset`/`DeltaRotation` capturados antes do ShiftWeaponRoot), por isso lá braço+arma sempre acompanham. Ref: [Player.cs:26253-26323](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L26253).

## Solução

**Postfix de `PlayerBones.ShiftWeaponRoot`** (`ObservedStanceShiftPatch`) — roda logo após a linha 1876 e **antes** do alvo da IK (method_20, 1884). Move o `Weapon_Root_Anim` direto na **janela pré-IK**: os markers (filhos) se deslocam → a LimbIK leva o **braço** até a stance → o `Kinematics` mantém a **arma** colada na mão. **Braço e arma acompanham juntos.**

Por que este hook (e não outros): `ShiftWeaponRoot` tem **nome estável** (não obfuscado), `PlayerBones.Player` ([PlayerBones.cs:27](../../../../references/eft-decompiled/Assembly-CSharp/PlayerBones.cs#L27)) dá acesso ao observado, e mover o transform **direto** evita a atenuação do `thirdPersonAuthority` que penalizaria um Prefix via `DeltaRotation` ([PlayerBones.cs:415-416](../../../../references/eft-decompiled/Assembly-CSharp/PlayerBones.cs#L415)).

## Mudanças aplicadas

| Arquivo | Mudança |
|---|---|
| `modded/Patches/ObservedStanceShiftPatch.cs` | **CRIADO** — Postfix em `PlayerBones.ShiftWeaponRoot`; gate `pv != FirstPerson` + `!IsYourPlayer` (AP-02); chama `ApplyToWeaponRoot`. |
| `modded/Patches/ObservedStanceVisualPatch.cs` | **REMOVIDO** — hook antigo (Postfix de ObservedVisualPass, pós-IK). |
| `modded/Networking/ObservedStanceAnimator.cs` | Simplificado: removido o guard anti-acúmulo do CR-02-01 (desnecessário — o ShiftWeaponRoot re-seta o transform todo frame antes daqui; e no early-return de ObservedVisualPass o ShiftWeaponRoot nem é chamado). Comentários atualizados. |
| `modded/Plugin.cs` | `SafeEnable("ObservedStanceShiftPatch")` no lugar do antigo. |

## Diagnóstico instrumentado

No `LogOutput.log` do cliente que **observa** o outro (grep `StanceSync-014`):
1. `[enable] OK ObservedStanceShiftPatch` — o patch habilitou.
2. `ShiftWeaponRoot Postfix RODOU para observado (pré-IK)` — o hook executa no observado.
3. `aplicando stance=N ... (pré-IK)` — o offset é aplicado.

## Checklist de validação (2 clientes Fika — **fechar e reabrir o EFT**)

- [x] Compila via `dotnet build` sem erros; instalado em `RealisticMobility/` (hash `972f5f8389767082`)
- [x] Os 3 logs `[StanceSync-014]` aparecem no cliente observador
- [x] A **arma E o braço/mão** do outro player acompanham a stance **juntos** (sem descolamento)
- [x] Lean/troca de ombro coexistem; vanilla intacto
- [x] ADS remoto reflete a mira (CR-02-02); prone não aplica stance

## Risco residual

- **Eixo/magnitude (CR-02-03):** o offset usa `GetTargetRotation/GetTargetPosition` (calibrados para a 1ª pessoa). Se a pose 3ª pessoa ficar exagerada/invertida em algum eixo, calibrar um fator de conversão. Mas como agora aplicamos no mesmo `Weapon_Root_Anim` que a EFT usa para o *left stance* nativo, a convenção deve bater.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-09 | Investigação por 2 sub-agents; causa raiz = Postfix de ObservedVisualPass roda pós-IK (braço já solveado). Solução movida para Postfix de `PlayerBones.ShiftWeaponRoot` (janela pré-IK). Compila 0 erros; instalado. Aguarda validação in-game (2 clientes). |
| 2026-07-09 | ✅ **Validado in-game (2 clientes Fika):** braço E arma acompanham a stance juntos. Item 014 fechado 🟢. |
