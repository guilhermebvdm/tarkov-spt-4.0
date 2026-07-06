# 013 — Versão do server dinâmica · Kickoff

**Launcher:** Launcher4.0beta · **Data:** 2026-07-03 · **Origem:** Trello MTav8H5f item 6.2

> Brief de kickoff — insumo para `/create-spec`. Não é a spec.

## Objetivo

O servidor passa a ter versão própria **`0.1.0-beta`**, definida **dinamicamente por arquivo no server** (não hardcoded), e o launcher exibe essa versão nos footers.

## Estado atual

- Footers com versões **hardcoded** no XAML — ex.: "Versão do launcher: 15.0" e "Versão do servidor: 0.10" em [Views/ClassSelectionView.axaml:56-62](../../project/SPT.Launcher/Views/ClassSelectionView.axaml#L56) — varrer as demais views (Login/Register/Profile) pelo mesmo padrão.
- Launcher já conhece [Models/SPT/SPTVersion.cs](../../project/SPT.Launcher.Base/Models/SPT/SPTVersion.cs) (versão do SPT core) — a versão **TRL do servidor** é outra coisa: decidir o canal (arquivo servido por rota de mod? campo extra no `/launcher/server/connect`?).

## Contrato SP0 (congelado 2026-07-03)

- **Server (TarkovRedLine.Server):** `GET /redline/server/version` → `{ "version": "0.1.0-beta" }`, lido de `Launcher-Updater/server-version.txt` no disco do server (mesmo padrão do `LauncherUpdaterController`, que já resolve essa pasta). Sem arquivo → default embutido no controller.
- **Launcher:** propriedade única (ServerManager/VM base) alimenta os footers via `TrlVersionFooter` (nasce no 015); fetch junto do connect; offline → "—". Não confundir com `/launcher/server/version` do SPT core (versão do SPT).
- **Coordenação:** 004L instala o `TrlVersionFooter` na `ClassSelectionView`; este item NÃO toca essa view — só entrega a propriedade/fetch + liga Login/Register/Profile.
