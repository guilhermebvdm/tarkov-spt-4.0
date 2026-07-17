# Análise do Fontaine-StanceOverhaul para o porte (item 016 do stancesAndCameraPositionSPT4.0.11)

> **Data:** 2026-07-17<br>
> **Status:** ✅ Aprovado<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [plano aprovado do item 016](../../stancesAndCameraPositionSPT4.0.11/backlog/016-transicao-realism-fork/), [docs/STANCE.md do autor](../original/docs/STANCE.md)<br>

---

Estudo que fundamenta o item **016** (fork `modded-realism`). Fonte: exploração do código em
`original/` (v1.0.0, recebido com **permissão de uso do autor**). Tudo com refs `arquivo:linha`.

## O que está ATIVO no Fontaine (e é o alvo do porte)

1. **Transição por progresso determinístico.** Nada de mola com velocidade: cada transição é um escalar
   `Progress 0..1` (`src/State/StanceSlot.cs:90-103`) que avança linearmente × modificadores
   (`TransitionFrom/To`, `GlobalStanceSpeed`); TODO o easing mora nos keyframes das curvas
   (`src/Resources/curves.json`, 1827 linhas, 25 curvas), avaliadas por eixo. Overshoot autoral é DESENHADO
   na curva (ex.: `low_ready` rotação Y: -8° em t=0.8 → -4° em t=1, curves.json:361-440).
2. **Dual-slot com blend.** `StanceState` (`src/State/StanceState.cs`, 329 linhas): `_primary` sai pela curva
   Exit enquanto `_incoming` entra pela Enter (pausado até `BlendIntoThreshold`); blend
   `Vector3.Lerp(primary, incoming, incoming.Progress)` (:131-152) + low-pass `StanceBlendSpeed` (:154-160).
   Reversão em voo = mesma curva com `Direction=-1` (:241-247).
3. **Gate de aim-speed — o mecanismo anti-overshoot/anti-briga-de-IK.** Postfix em
   `PWA.UpdateWeaponVariables` captura o `_aimingSpeed` original (`src/Patches/StancePatches.cs:298-320`);
   `StanceState.UpdateAimSpeed` (:164-179) escreve todo frame `original × ExitAimSpeedCurve(Progress)`.
   A curva default (`src/Stances/StanceBase.cs:27-49`) segura ~0 até 70-85% e libera no fim → a mira nativa
   do EFT quase não anda enquanto a arma ainda sai da stance; **stance e ADS nunca disputam o alvo**.
4. **Modelo ADS do Fontaine**: mirar CANCELA a stance (`StanceInputHandler.cs:160-207` → `CancelAll()` roda a
   curva Exit) e restaura ao soltar. **Decisão do 016: NÃO portar o cancela/restaura** (nosso `CurrentStance`
   alimenta snap-on-fire/stamina/Fika/mount) — portar SÓ o gate.
5. **Aplicação da pose**: injeção em CLONES das molas nativas `PWA.HandsContainer.HandsPosition/HandsRotation`
   (`StanceController.cs:425-429` via `Cloner.ShallowClone`; `ZeroAdjustmentsPatch` seta `.Zero`,
   `SpringGetPatch/GetRelative` SOMAM ao resultado — `StancePatches.cs:364-451`). Pré-IK.
   **Decisão do 016: manter o NOSSO ponto de aplicação** (`WeaponRootAnim`, validado no item 014) e portar só
   a evolução do estado.

## O que está MORTO no Fontaine (não portar — checado linha a linha)

- `Melee.cs` (112 linhas, 100% comentado) · `Mounting.cs` (stub) · `AimPIDHandler.cs` (o "PID" é só termo
  proporcional, todo comentado) · `PositionOffsetHandler.cs` (comentado) · TODOS os `SpringAnimators/`
  (interfaces/classes vazias com TODO) · o sistema legado de mola inteiro (~60-65% de `StanceController.cs`
  e `StancePatches.cs` comentados — é a MESMA arquitetura que a nossa, abandonada pelo autor).

## Dependência RealismCommonLib — o que o núcleo de pose realmente usa

A lib NÃO está no pacote (csproj referencia `..\RealismCommonLib\` inexistente — o mod não compila aqui).
Para o porte interessam só: **`Vector3Curve`** (3 AnimationCurves com `.Evaluate(t)`), **`CurveDrawer`**
(registry/parser do curves.json) e **`Cloner.ShallowClone`**. Reimplementáveis/adaptáveis em ~100 linhas.
O resto (PlayerState/WeaponState/StatModifiers/eventos/pipelines de input) é ecossistema Realism — fora.

## Mapeamento proposto Fontaine → nossas stances (a spec do 016 fecha)

| Nossa stance | Curvas-base do Fontaine | Nota |
|---|---|---|
| 0 Vanilla | — (idle; destino das Exits) | |
| 1 High Ready | `high_ready_*` | |
| 2 Low Ready | `low_ready_*` | overshoot autoral no Y é o caso-estudo |
| 3 Custom | `short_*` (Short-Stocking) | base de caráter; pose continua do slider |
| (ADS in/out) | `active_*` + `Enter/ExitAimSpeedCurve` | referência para as curvas de gate |

## Por que isso resolve nossos 2 bugs (fatos do NOSSO código)

- **(b) overshoot Low Ready→mira**: nossa mola é sub-amortecida por design (ζ≈0,49; damping fixo com stiffness
  escalando com o slider — `modded/Patches/ApplyComplexRotationPatch.cs:271-273`) e ao mirar o alvo troca NA
  MESMA mola com velocidade acumulada + kick de ADS (:236-244). Progresso determinístico + recaptura do `from`
  elimina a herança; o gate segura a mira nativa até a pose sair.
- **(a) braço deformado na transição**: a excursão do overshoot empurra os markers de IK para fora do envelope
  na janela em que a mira nativa já domina. O gate fecha essa janela. (Deformação em pose ESTÁTICA, se
  existir, está fora do 016 — diagnóstico na F0.)

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-17 | Guilherme | Criação — consolidação da exploração (2 agents) + decisões do plano aprovado. |
