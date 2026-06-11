# 009 — Wiggle · 06-fix-01

**Mod:** stancesAndCameraPositionSPT4.0.11
**Data:** 2026-06-11
**Status:** 🟡 Implementado — requer validação in-game

## Sintoma

O wiggle disparava ao **encostar a arma em parede / montar** (colisão), não na troca de stance. O efeito esperado era exclusivamente durante a **transição entre stances**.

## Causa raiz

Em [SpringGetPatch.cs](../../modded/Patches/SpringGetPatch.cs), o wiggle disparava por comparação `currentStance != _previousStance`. Como o `StanceManager.Update()` força `SetStance(Stance.Default)` ao montar/colidir/prone/sprint, `CurrentStance` mudava sem ação do jogador → o wiggle disparava. Bug secundário: o bloco estava dentro de `if (stateChanged)` e atualizava `_previousStance` na passada de **rotação**, fazendo a passada de **posição** (mesmo frame) não disparar o wiggle.

## Correção — sinal explícito de intenção

| Arquivo | Mudança |
|---|---|
| `modded/StanceManager.cs` | `RequestWiggle(from, to)` + `ConsumeWiggleRequest(out from, out to)` + helper `ApplyUserStance(to)`. `RequestWiggle` é chamado **só** nos call-sites de input do jogador (tecla V, scroll cycle, scroll linear, hotkeys dedicadas) — via `ApplyUserStance`. **Não** é chamado no force-Default por mount/colisão/prone/sprint, em `Start/EndActionStance`, snap-on-fire, stance inicial de raid, nem `ResetState` (que limpa o request). |
| `modded/Patches/SpringGetPatch.cs` | Gatilho do wiggle trocado para `ConsumeWiggleRequest()` com **frame-guard** (consumido 1×/frame; aplicado em rot e pos). Bloco **movido para fora** do `if (stateChanged)`. Direção do impulso modulada por `from→to` (baixar p/ Stance 0 = coronha p/ trás; subir = arma p/ frente). |

**Gate ao MainPlayer (revisão #3):** já garantido pelo guard pré-existente `if (!isMainPlayerRot && !isMainPlayerPos) return;` — o wiggle nunca roda para springs de players remotos (Fika). Confirmado na leitura, nenhuma mudança necessária.

## Critérios de aceite

- [ ] Wiggle ocorre na troca de stance (0↔1/2/3 e diretas 1↔2↔3 por scroll/hotkey).
- [ ] Encostar a arma em parede / entrar/sair de mount **não** dispara wiggle (salvo troca real de stance).
- [ ] Não prejudica ADS; cancelar transição não deixa offset.
- [ ] Toggle e multiplicador no F12 funcionam.

## Premissas assumidas

1. **Wiggle direcional** (`from→to`) implementado de forma leve (sinal do impulso). Antes era 100% aleatório; o spec sugeria direcionalidade ("fisicamente coerente"). Magnitudes mantidas próximas do original — re-tunar in-game se necessário.
2. **Transições diretas 1↔2↔3** cobertas automaticamente (o request é por ação de input, não por par de/para específico).

## Histórico

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-11 | Claude (autônomo) | 06-fix-01: gatilho do wiggle por request intencional (não por comparação de stance); movido p/ fora do stateChanged; direção por transição. Não testado in-game. |
