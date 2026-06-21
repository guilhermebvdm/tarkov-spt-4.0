# 051 — Levers da zona stances (🔧⚠️) · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-20 · **Origem:** redesign 11→6, Fase 5 ([class-levers.md](../../docs/class-levers.md) §6.2; ponta solta #6)
**Wave:** R-W2 · **Deps:** 050 + coordenação com o stances mod · **Gate:** decisão (coordenar vs trocar lever) **antes** do `/create-spec 051`

> Brief de kickoff — insumo para `/create-spec 051`. Não é a spec.

## Objetivo

Os 2 levers que tocam **stamina de braço/mãos** — território do stances mod (`GetHandsRestorationFunc`→0, `Priority.Low`, MainPlayer):

- **Caçador** — resistência de braço em ADS.
- **Tanque** — stamina segurando arma pesada `×0`.

## Escopo / Riscos

- Multiplicar nessa zona seria **zerado** pelo stances. Decidir entre: **(a)** coordenar via estado compartilhado (mesmo repo — `mods/stancesAndCameraPositionSPT4.0.11`) ou **(b)** trocar o lever por equivalente fora da zona (ex.: BuffType `HandsTremor` / stamina de perna).
- **Levar a decisão ao usuário antes de codar** (é escolha de design + coordenação entre mods).

## Refs

- [../../docs/class-levers.md](../../docs/class-levers.md) §6.2 · `mods/stancesAndCameraPositionSPT4.0.11`
- Item 050 (mesma infra de patch)

## DoD (resumo)

- Os 2 levers funcionam **compondo** com o stances (opção a) ou substituídos por equivalente documentado (opção b). Sem regressão no stances p/ as outras classes.
