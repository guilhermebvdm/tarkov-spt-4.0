# 024 — Migração DS da SettingsView + unificar chrome · Kickoff

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Origem:** [AUDIT-2026-07-04](../../AUDIT-2026-07-04-code-product-ds.md) §B3 + §DS · **Severidade:** 🔴 Blocker (DS) · **Deps:** 015

> Brief de kickoff — insumo para `/create-spec`. Não é a spec.

## Achados
- **`SettingsView.axaml` não migrou ao DS:** ~20 hex (`#1A1A1A`, `#222`, `#F2111111`, `#333`, `#111`…) + literais `White/LightGray/Gray`; recria estilos próprios (`SidebarMenu`/`PanelCard`/`CleanupButton`) divergentes dos tokens. Maior furo de pureza do launcher, ao lado de dialogs 100% migrados.
- **Duas sidebars** para o mesmo menu: `ProfileView` usa `cc:TrlSidebarNav` (280px, token-puro); `SettingsView` usa `Border #111111` (250px, `Button.SidebarMenu`).
- **Dot Dev Mode** vem do VM como hex cru (`SettingsViewModel.cs:50` → `#4CAF50`/`#555555`) + `Border CornerRadius="10"` (`SettingsView.axaml:195`).

## Critérios de aceite (seed)
- `SettingsView` 100% em tokens `Trl*` — **zero** hex/literal na view.
- Sidebar via `cc:TrlSidebarNav` + `Button.nav` (mesmo chrome do Profile).
- Cards via `cc:TrlPanel`; dot Dev Mode via token (`TrlSuccess`/`TrlFgFaint`) com radius 0.
