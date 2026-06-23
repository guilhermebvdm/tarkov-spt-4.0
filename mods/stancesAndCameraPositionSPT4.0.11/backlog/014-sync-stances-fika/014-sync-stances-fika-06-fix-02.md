# 014 — Fix 02 · Aplicar no ÚNICO ponto não-sobrescrito (Postfix de ObservedVisualPass)

**Mod:** stancesAndCameraPositionSPT4.0.11
**Item raiz:** [014-sync-stances-fika-01-spec.md](014-sync-stances-fika-01-spec.md)
**Fix anterior:** [06-fix-01](014-sync-stances-fika-06-fix-01.md) (ProcessEffectors — não funcionou)
**Criado:** 2026-06-22
**Disparado por:** o sync continuou sem efeito **e o log nunca apareceu** — sinal de que o hook não rodava nesse caminho.

## Investigação (3 sub-agents independentes, Assembly + Fika)

Mapeada a sequência completa de render da arma do observado em `ObservedPlayer.ObservedVisualPass` ([ObservedPlayer.cs:1839-1924](../../../../references/fika-plugin/Fika.Core/Main/Players/ObservedPlayer.cs#L1839)):

```
1851 ProcessEffectors            (internamente: ApplyComplexRotation → mas é re-trabalhado depois)
1852 Offset       = WeaponRootAnim.localPosition
1853 DeltaRotation = WeaponRootAnim.localRotation
1859 ObservedFBBIKUpdate          (IK das mãos)
1876 ShiftWeaponRoot(ThirdPerson) (posiciona Weapon_Root_Anim; usa Weapon_Root_Third.rotation*DeltaRotation, atenuado por thirdPersonAuthority)
1889 Kinematics(_observedMarkers[1], _rightHand)  ← REESCREVE Weapon_Root_Anim (segue o marcador da mão)
1918 LateTransformations
1921 HandsController.ManualLateUpdate
```

### Causa raiz definitiva
- **Todo offset aplicado antes da linha 1889 é APAGADO** pelo `Kinematics`, que reescreve `Weapon_Root_Anim` para seguir a mão. Isso explica por que **nenhuma** tentativa anterior funcionou: `ApplyComplexRotation` (não rodava nesse caminho), `ProcessEffectors` (não rodava — log nunca apareceu), e o `ShiftWeaponRoot`-Prefix (seria sobrescrito pelo `Kinematics`).
- O `thirdPersonAuthority` (curva `WEAPON_ROOT_3RD`, ObservedPlayer.cs:1866) ainda podia **atenuar** o `DeltaRotation` a quase nada.
- **Não existe mod de referência** externo: o RealismMod só aplica pose de arma em **bots** (`IsAI`), localmente, sem rede. Somos os primeiros a sincronizar pose de arma 3ª pessoa via Fika.

## Solução

**Postfix de `ObservedPlayer.ObservedVisualPass`** (`ObservedStanceVisualPatch`) — roda **depois de TUDO** (ShiftWeaponRoot + Kinematics + IK + ManualLateUpdate), aplicando o offset de stance **direto no transform final** `PlayerBones.Weapon_Root_Anim.localPosition/localRotation`. É o único ponto onde nada sobrescreve. Não passa pela `thirdPersonAuthority`.

## Mudanças aplicadas

| Arquivo | Mudança |
|---|---|
| `modded-beta/Patches/ObservedStanceVisualPatch.cs` | **CRIADO** — Postfix em `ObservedPlayer.ObservedVisualPass`; chama `ApplyToWeaponRoot`. (Substitui os patches ProcessEffectors/ShiftWeaponRoot das tentativas anteriores, removidos.) |
| `modded-beta/Networking/ObservedStanceAnimator.cs` | `ApplyToWeaponRoot(bones)` aplica o offset em `bones.Weapon_Root_Anim.local*`. Logs `[StanceSync-014]`. |
| `modded-beta/Plugin.cs` | `SafeEnable("ObservedStanceVisualPatch")`. |

## Diagnóstico instrumentado (1 teste decisivo)

No `LogOutput.log` do cliente que **observa** o outro, procurar (grep `StanceSync-014`):
1. `[enable] OK ObservedStanceVisualPatch` — o patch habilitou (se `FAIL`, o método `ObservedVisualPass` não resolveu).
2. `ObservedVisualPass Postfix RODOU` — o hook executa no observado.
3. `aplicando stance=N no Weapon_Root_Anim` — o offset é aplicado.

Os três aparecendo → o mecanismo está ativo. Se a arma mover (mesmo que a mão descole levemente), o caminho está correto — o ajuste fino (rotacionar em torno do pivô da mão) é o passo seguinte.

## Risco residual

- **Descolamento mão↔arma:** a IK posiciona as mãos antes; rotacionar a arma depois pode descolá-la levemente da mão. Se ocorrer, próximo passo é rotacionar `Weapon_Root_Anim` **em torno do ponto da mão** (RotateAround) em vez do próprio root.

## Checklist de validação (2 clientes Fika — **fechar e reabrir o EFT**)

- [x] Compila via `/compile-mod` sem erros
- [ ] Os 3 logs `[StanceSync-014]` aparecem no cliente observador
- [ ] A **arma** do outro player acompanha a stance
- [ ] Sem descolamento grave mão↔arma; lean/ombro coexistem

## Histórico

| Data | Evento |
|---|---|
| 2026-06-22 | Investigação por 3 sub-agents; causa raiz = `Kinematics` (1889) sobrescreve. Solução movida para Postfix de `ObservedVisualPass` (transform final). Compila 0 erros. |
