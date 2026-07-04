# 013 — Versão do server dinâmica · Kickoff

**Launcher:** Launcher4.0beta · **Data:** 2026-07-03 · **Origem:** Trello MTav8H5f item 6.2

> Brief de kickoff — insumo para `/create-spec`. Não é a spec.

## Objetivo

O servidor passa a ter versão própria **`0.1.0-beta`**, definida **dinamicamente por arquivo no server** (não hardcoded), e o launcher exibe essa versão nos footers.

## Estado atual

- Footers com versões **hardcoded** no XAML — ex.: "Versão do launcher: 15.0" e "Versão do servidor: 0.10" em [Views/ClassSelectionView.axaml:56-62](../../project/SPT.Launcher/Views/ClassSelectionView.axaml#L56) — varrer as demais views (Login/Register/Profile) pelo mesmo padrão.
- Launcher já conhece [Models/SPT/SPTVersion.cs](../../project/SPT.Launcher.Base/Models/SPT/SPTVersion.cs) (versão do SPT core) — a versão **TRL do servidor** é outra coisa: decidir o canal (arquivo servido por rota de mod? campo extra no `/launcher/server/connect`?).

## Perguntas p/ a spec

- Quem serve a versão (mod server TRL? arquivo estático?) e formato (semver + sufixo `-beta`).
- Fallback quando o campo não existe (server antigo).
