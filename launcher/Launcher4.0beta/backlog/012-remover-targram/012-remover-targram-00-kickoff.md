# 012 — Remover Targram do menu · Kickoff

**Launcher:** Launcher4.0beta · **Data:** 2026-07-03 · **Origem:** Trello MTav8H5f item 6.1

> Brief de kickoff — insumo para `/create-spec`. Não é a spec. Item pequeno e mecânico — candidato a spec enxuta.

## Objetivo

Remover o item "Targram" dos menus laterais do launcher (botão + command).

## Pontos mapeados (2026-07-03)

- [Views/SettingsView.axaml:92](../../project/SPT.Launcher/Views/SettingsView.axaml#L92) — botão sidebar
- [Views/ProfileView.axaml:118](../../project/SPT.Launcher/Views/ProfileView.axaml#L118) — botão sidebar
- [ViewModels/SettingsViewModel.cs:101](../../project/SPT.Launcher/ViewModels/SettingsViewModel.cs#L101) — `OpenTargramCommand()`
- [ViewModels/ProfileViewModel.cs:276](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L276) — `OpenTargramCommand()`

Verificar sobras: localization keys, ícones/assets referenciando Targram.
