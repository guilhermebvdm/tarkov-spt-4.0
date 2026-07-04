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
- ⚠️ **Se a tendência for (b)**, antecipar a decisão para **antes de specar o 050** — um lever fora da zona vira patch comum, que pertence ao 050 (R-W1); decidir tarde gera retrabalho de mover o lever entre itens.

## Refs

- [../../docs/class-levers.md](../../docs/class-levers.md) §6.2 · `mods/stancesAndCameraPositionSPT4.0.11`
- Item 050 (mesma infra de patch)

## DoD (resumo)

- Os 2 levers funcionam **compondo** com o stances (opção a) ou substituídos por equivalente documentado (opção b). Sem regressão no stances p/ as outras classes.

---

## Análise de decisão (2026-07-03 — insumo pro usuário bater o martelo)

**Evidência do mecanismo (por que multiplicar por fora não funciona):** o stances tem **autoridade única** sobre a
stamina de braço do player local — `StaminaController.Tick()` roda 1×/frame, resolve UM `StaminaScenario` (16
cenários: StandAds, ProneAds, HoldBreath…) e **escreve `hands.Current` diretamente** pelo multiplicador do cenário
(F12 do stances); os Prefixes `HandsStaminaNeutralizePatch`/`HandsConsumeNeutralizePatch` **cancelam o
Process/Consume vanilla** para o braço enquanto `ControllingHands`
(refs: `mods/stancesAndCameraPositionSPT4.0.11/modded/StaminaController.cs:12-30` ·
`Patches/StanceStaminaRecoveryPatch.cs:10-33`). Qualquer patch do CustomClasses nessa zona é sobrescrito no frame seguinte.

### Opção (a) — coordenar (RECOMENDADA)

Hook de composição no próprio `StaminaController`: o stances expõe um multiplicador externo
(`public static Func<float>? ExternalHandsDrainMult`) aplicado sobre o multiplicador do cenário ao escrever
`hands.Current`; o CustomClasses o preenche via soft-detect (`AccessTools.TypeByName` — zero dependência hard,
degrada 100% sem o outro mod; mesmo padrão FIKA/SAIN já usado nos dois mods). O CustomClasses calcula o fator
por frame: Caçador em ADS → ×0.65 · Tanque segurando arma pesada → ×0 (reusa o gate de arma pesada do Bunker).

- ✅ Preserva a identidade já EXPOSTA na UI (cards "Steady Arms"/"Braços Firmes" e "Tireless Arms"/"Braços
  Incansáveis" já aparecem na aba CLASS) e o design do redesign 11→6.
- ✅ Mudança minúscula no stances (1 static + 1 multiply no Tick) — mesmo repo, build/deploy conhecidos.
- ⚠️ Toca o stances mod → coordenar com a sessão/handoff dele (mesma regra que valeu pro `modded/Server`).
- ⚠️ Sub-decisão de semântica: o fator externo multiplica só o componente de DRENO (cenários que drenam), nunca a
  recuperação — detalhar na spec técnica.

### Opção (b) — trocar o lever (fallback)

Substituir por equivalentes fora da zona: Caçador → tremor de mãos reduzido ou só o sway (mas o sway/hold-breath
também tangencia o stances — `HoldBreathPatch` existe lá); Tanque → dreno de stamina DE PERNA reduzido com arma
pesada. ❌ Exige redesenhar/rebalancear + reescrever catálogo/cards/notificação que já shipparam com os nomes
atuais; o equivalente de perna muda a identidade do perk. Só faz sentido se a coordenação (a) for vetada.

### ⚠️ Achado colateral (independe da decisão)

As linhas do catálogo **"Steady Arms" (Iron Lungs, ×0.65)** e **"Tireless Arms" (Bunker, flag)** estão SEM
`pending: true` — mas o efeito ainda não existe (é exatamente este item 051). Até o 051 entregar, os cards
prometem efeito inativo → marcar `pending: true` nas duas linhas no próximo build round (1 linha cada em
`PerksCatalog.cs`).

**Próximo passo:** usuário escolhe (a) ou (b) → `/create-spec 051` com a decisão fixada.
