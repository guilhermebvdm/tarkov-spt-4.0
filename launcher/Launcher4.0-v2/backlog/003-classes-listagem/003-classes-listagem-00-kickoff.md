# 003 — Tela de classes: listagem · Kickoff (retroativo)

**Launcher:** Launcher4.0beta · **Data:** 2026-07-03 · **Origem:** Trello MTav8H5f item 3.1 (✅ concluído no card)

> Brief retroativo — entregue antes deste backlog existir (commit 88db747). A UI está pronta; os **dados são mock** — a integração real é o item [004](../004-classes-dados-reais/).

## Objetivo

Tela de seleção de classe no fluxo de criação de conta: lista de classes à esquerda + painel de detalhe (imagem, descrição, vantagens/desvantagens, habilidades) à direita.

## Escopo entregue

- [Views/ClassSelectionView.axaml](../../project/SPT.Launcher/Views/ClassSelectionView.axaml) — layout completo (lista, detalhe, footer de versões)
- [ViewModels/ClassSelectionViewModel.cs](../../project/SPT.Launcher/ViewModels/ClassSelectionViewModel.cs) — `ClassProfile`, navegação, registro+auto-login

## Ressalvas conhecidas (insumo p/ o code-review e p/ o 004)

- Dados 100% mockados em `LoadMockClasses()` ([ClassSelectionViewModel.cs:113-134](../../project/SPT.Launcher/ViewModels/ClassSelectionViewModel.cs#L113)) — 10 classes hardcoded, só "Caçador" com detalhe completo.
- Divergência mock × server real: "Armeiro", "Batedor", "Gerente de Operações", "Sobrevivencialista" **não existem** como classe no CustomClasses; "Operador Tático"→real é "Tanque", "Operador Furtivo"→"Furtivo"; falta "Fuzileiro".
- Seleção default hardcoded (`AvailableClasses[3]`, índice mágico).
- Registro envia `SelectedClass.Name` cru como edition — funciona por coincidência (editions do CustomClasses usam `displayName.pt`).
- Footer com versões hardcoded ("15.0" / "0.10") — vira dinâmico no item 013.

## Pendências

- **Code-review retroativo** (`/code-review Launcher4.0beta 003`) — sem `05-asbuild` prévio.
