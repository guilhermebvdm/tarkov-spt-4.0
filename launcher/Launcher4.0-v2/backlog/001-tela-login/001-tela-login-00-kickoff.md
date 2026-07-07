# 001 — Nova tela de login · Kickoff (retroativo)

**Launcher:** Launcher4.0beta · **Data:** 2026-07-03 · **Origem:** Trello MTav8H5f item 1 (✅ concluído no card)

> Brief retroativo — o item foi entregue antes deste backlog existir (commit 88db747, "feat: Implement new UI design for Login, Register and Class Selection").

## Objetivo

Redesign TRL da tela de login do launcher (visual novo, fundo customizado, fluxo de conexão ao servidor).

## Escopo entregue

- [Views/LoginView.axaml](../../project/SPT.Launcher/Views/LoginView.axaml) + [LoginView.axaml.cs](../../project/SPT.Launcher/Views/LoginView.axaml.cs)
- [ViewModels/LoginViewModel.cs](../../project/SPT.Launcher/ViewModels/LoginViewModel.cs)
- [CustomControls/LoginBox.axaml](../../project/SPT.Launcher/CustomControls/LoginBox.axaml)

## Pendências

- **Code-review retroativo** (`/code-review Launcher4.0beta 001`) — rodar direto sobre o código entregue; sem `05-asbuild` prévio.
- Item 005 (definir senha em conta sem senha) cobre o gap funcional restante do fluxo de login (Trello 1.1).
