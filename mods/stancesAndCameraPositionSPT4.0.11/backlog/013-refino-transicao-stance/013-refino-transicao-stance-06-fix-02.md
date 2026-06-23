# 013 — Fix 02 · Stance 3 aparece abaixo de Stance 2 no F12

**Mod:** stancesAndCameraPositionSPT4.0.11
**Item raiz:** [013-refino-transicao-stance-01-spec.md](013-refino-transicao-stance-01-spec.md)
**Relacionado:** critério de organização F12 do [item 012](../012-controlador-central-stamina/012-controlador-central-stamina-02-spec-tech.md) ("Stance 3 abaixo de Stance 2").
**Criado:** 2026-06-22
**Disparado por:** validação in-game (todos os itens passaram, exceto a ordem da seção Stance 3 no F12).

## Contexto

In-game, a seção **`Stance 3 - Custom`** aparecia **deslocada** no F12 — depois do grupo Wiggle, separada das outras Stances — em vez de logo abaixo de `Stance 2 - Low Ready`. Todos os demais pontos da validação passaram (inclusive o grupo `Stamina Management` acima de `9. Respiração`, confirmando que a ordem das seções é por **descoberta**).

## Causa raiz

A ordem das seções no ConfigurationManager é a **ordem de descoberta** (primeira `Config.Bind` de cada seção). As seções de Stance eram descobertas em: Stance 0 ([Plugin.cs:772](../../modded-beta/Plugin.cs#L772)), Stance 1 (~778), Stance 2 (~837) — **mas o bloco da Stance 3 estava lá embaixo no `Awake`** ([Plugin.cs:~1239](../../modded-beta/Plugin.cs)), depois de Passive Mount / Stamina Management / Hold Breath / Wiggle. Logo, `Stance 3 - Custom` era descoberta por último e caía fora da sequência 0→1→2→3.

## Solução

**Descoberta antecipada** da seção Stance 3 (mesma técnica já usada na Stance 0): a **1ª Bind** da `Stance3Section` (`_Stance3SprintAnimationEnabled`) foi **movida** para logo após o bloco da Stance 2 (antes do Weapon Mount). A seção passa a ser descoberta cedo e aparece na ordem **0 → 1 → 2 → 3**; as hand-rotations da Stance 3 continuam no bloco original, apenas anexando à seção já descoberta (a ordem das *entries* dentro da seção é por `Order`, inalterada).

## Mudanças aplicadas

| Arquivo | Mudança |
|---|---|
| `modded-beta/Plugin.cs` | `_Stance3SprintAnimationEnabled` (1ª Bind da `Stance3Section`) movido para logo após o bloco da Stance 2. Nenhuma config nova/removida. |

## Checklist de validação

- [x] Compila via `/compile-mod` sem erros
- [ ] **F12 (reiniciar o jogo):** `Stance 3 - Custom` aparece **logo abaixo** de `Stance 2 - Low Ready`; as configs da Stance 3 (sprint + hand rotations) seguem todas na mesma seção.
- [ ] Nenhum valor de config foi resetado (mesma `(section, key)`).

## Histórico

| Data | Evento |
|---|---|
| 2026-06-22 | Fix criado — descoberta antecipada da Stance 3 corrige a ordem no F12. Compila 0 erros; aguarda confirmação visual (reiniciar o jogo). |
