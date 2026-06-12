# 013 · 06-fix-01 — BUG: botão SKILLS não aparece (Menu-Overhaul)

**Mod:** CustomClasses · **Data:** 2026-06-09 · **Tipo:** bug report / fix do item 013 · **Prioridade:** 🟠

> Reportado pelo usuário: o botão **SKILLS** (item 013) **nunca aparece** no menu, inclusive em prints recentes. Ambiente tem o **Menu-Overhaul** ligado.

## Sintoma

`ShowSkillsButton = true`, mas nenhum botão SKILLS aparece abaixo de CHARACTER. **Sem erro** no log do BepInEx (o patch roda).

## Causa-raiz (confirmada)

O [SkillsNavButtonPatch](../../modded/Client/Patches/SkillsNavButtonPatch.cs) **clona o `_playerButton` (CHARACTER) vanilla** e o adiciona ao mesmo parent via `SetSiblingIndex`. Isso assume **layout por ordem/sibling**.

Mas o **Menu-Overhaul** reposiciona os botões do menu por **nome**, com `anchoredPosition` absoluta e offset fixo:
- `ButtonHelpers.cs` tem uma **lista fixa** de botões que ele gerencia: `PlayButton`, `CharacterButton`, `TradeButton`, `HideoutButton`, `ExitButtonGroup` (`ButtonYOffset = 60f`).
- O clone `CC_SkillsButton` **não está nessa lista** → o MO não o reposiciona → ele fica na posição herdada do clone (sobreposto/fora da pilha reorganizada do MO), efetivamente **invisível**.

→ O `SetSiblingIndex` não tem efeito quando o layout é por `anchoredPosition` absoluta (MO).

## Abordagem de fix (proposta)

1. **Detectar o MO** (já temos [MenuOverhaulBridge](../../modded/Client/UI/MenuOverhaulBridge.cs)). Com MO presente:
   - Posicionar o `CC_SkillsButton` por `anchoredPosition` relativa ao **CHARACTER já reposicionado pelo MO** (ler a `anchoredPosition` do `_playerButton` após o MO agir + aplicar o mesmo `ButtonYOffset = 60`), empurrando os botões abaixo (Trade/Hideout/Exit) para baixo — ou inserir entre Character e Trade.
   - **Timing:** o MO posiciona de forma assíncrona → aplicar via coroutine que espera o MO terminar (padrão já usado no [MenuClassIdentityPatch](../../modded/Client/Patches/MenuClassIdentityPatch.cs)).
2. **Sem o MO:** manter o comportamento atual (clone + sibling index) — validar que funciona no menu vanilla.
3. Avaliar copiar o **ícone** do CHARACTER (o MO usa ícones nos botões via `SetupButtonIcons`) para o SKILLS não ficar sem ícone.

**Risco:** posições do MO são empíricas (prefab/offsets internos). Provável precisar de slider de offset (X/Y) como nos selos, e iteração no playtest.

## Refs

- [SkillsNavButtonPatch.cs](../../modded/Client/Patches/SkillsNavButtonPatch.cs) (clona `_playerButton`, `SetSiblingIndex`)
- Menu-Overhaul: `original/Helpers/ButtonHelpers.cs` (lista fixa de botões + `ButtonYOffset`/`anchoredPosition`)
- Assembly: `EFT.UI.MenuScreen { DefaultUIButton _playerButton; }`

## Histórico

| Data | Evento |
|---|---|
| 2026-06-09 | Bug confirmado: clone do botão é órfão porque o Menu-Overhaul reposiciona botões por nome (lista fixa, `anchoredPosition`). Fix proposto: posicionar relativo ao CHARACTER pós-MO via coroutine. A implementar. |
| 2026-06-09 | **Implementado.** Coroutine pós-`Show` espera o MO (~10 frames) e, com MO presente, posiciona o SKILLS abaixo do Character + empurra Trade/Hideout/Exit (posições absolutas relativas ao Character, idempotente). Sem MO: `SetSiblingIndex` (vanilla). Compilado 0 warn/err (client 41.5 KB). A validar in-game. |
