# 014 — Release launcher 2.0.0 · As-built

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Insumo:** [00-kickoff](./014-release-v2-00-kickoff.md)

## Migração de keys legadas (shim removido)

Restyle mecânico: os últimos consumidores das 10 keys legadas do tema (`AccentBrush*`, `AltAccentBrush*`, `BackgroundBrush*`, `AltBackgroundBrush`, `ForegroundBrush`) foram migrados para os tokens `Trl*` (definidos em `Assets/Theme/Tokens.axaml`) e o SHIM foi eliminado do `App.axaml`. Sem redesign — os valores grafite v2 foram preservados 1:1 (cada key legada apontava para o mesmo hex do token de destino).

### Mapa aplicado

| Key legada | Token Trl* | Hex |
|---|---|---|
| `AccentBrush` | `TrlAccentBrush` | `#C7B48A` |
| `AltAccentBrush` | `TrlTan400Brush` | `#AB9A71` |
| `BackgroundBrush` | `TrlBgAppBrush` | `#131314` |
| `AltBackgroundBrush` | `TrlBgRaisedBrush` | `#222225` |
| `ForegroundBrush` | `TrlFgBrush` | `#E8E7E4` |

(As demais keys do shim — `AccentBrush2/3`, `AltAccentBrush2/3`, `BackgroundBrush2` → `TrlAccentDimBrush`/`TrlTan600Brush`/`TrlAccentDimBrush`/`TrlTan700Brush`/`TrlBgPanelBrush` — não tinham nenhum consumidor vivo no projeto; removidas do shim sem troca.)

### Arquivos tocados

| Arquivo | Trocas |
|---|---|
| `project/SPT.Launcher/Views/MainWindow.axaml` | 1× `AltBackgroundBrush` → `TrlBgRaisedBrush` (Background da janela) |
| `project/SPT.Launcher/Views/SettingsView.axaml` | 3× `AccentBrush` → `TrlAccentBrush` (estilos `Button.SidebarMenu.Active`) |
| `project/SPT.Launcher/CustomControls/ModInfoCard.axaml` | 6×: 1× `BackgroundBrush` → `TrlBgAppBrush`; 3× `AltAccentBrush` → `TrlTan400Brush`; 2× `AccentBrush` → `TrlAccentBrush` |
| `project/SPT.Launcher/CustomControls/TotalModsCard.axaml` | 2×: 1× `BackgroundBrush` → `TrlBgAppBrush`; 1× `ForegroundBrush` → `TrlFgBrush` |
| `project/SPT.Launcher/CustomControls/DetailedProfileCard.axaml` | 1× `AccentBrush` → `TrlAccentBrush` (Run RemainingExp) |
| `project/SPT.Launcher/CustomControls/DetailedProfileCard.axaml.cs` | 2× string de key no `TryFindResource`: `AltBackgroundBrush` → `TrlBgRaisedBrush` (hover in), `BackgroundBrush` → `TrlBgAppBrush` (hover out). Guard `if (Application.Current != null && TryFindResource(...))` mantém degradação segura — com o nome novo o recurso resolve. |
| `project/SPT.Launcher/CustomControls/GameLaunchBar.axaml` | 1× `BackgroundBrush` → `TrlBgAppBrush` |
| `project/SPT.Launcher/CustomControls/LoginBox.axaml` | 1× `AccentBrush` → `TrlAccentBrush` |
| `project/SPT.Launcher/CustomControls/ProfileCard.axaml` | 1× `BackgroundBrush` → `TrlBgAppBrush` |
| `project/SPT.Launcher/App.axaml` | **Shim removido:** as 10 keys legadas nas DUAS seções (`ThemeDictionaries` Light + Dark) — bloco `ResourceDictionary.ThemeDictionaries` inteiro apagado. FluentTheme, DialogHost, `StyleInclude` do `Trl.axaml`, `ControlCornerRadius`/`OverlayCornerRadius` e os `PathGeometry` de ícones intactos. |

**Total:** 18 trocas de key em 9 arquivos consumidores + shim (20 defs em 2 seções) removido do `App.axaml`.

Notas:
- `Views/ProfileView.axaml` já vinha migrado (usava `TrlAccentBrush`) — não precisou de troca.
- Nenhum arquivo em `Assets/Theme/Controls/*.axaml` consumia key legada bare; todos já referenciavam `Trl*`. Logo, não houve migração dentro de `Assets/Theme/` (Tokens.axaml/Typography.axaml não tocados, por escopo).

### Verificação anti-regressão (runtime = crítico)

Grep final por qualquer referência bare às 10 keys legadas (`{DynamicResource ...}`, `{StaticResource ...}`, `TryFindResource("...")`, com word-boundary excluindo o prefixo `Trl`) no projeto `SPT.Launcher/` inteiro:

**ZERO ocorrências.** Nenhum recurso ausente — sem risco de elemento invisível/crash em runtime por key de tema não resolvida.

### Build

`dotnet build project/SPT.Launcher/SPT.Launcher.csproj -c Release` → **0 Erros** (169 warnings pré-existentes: nullable `CS86xx` e `CA1416` de registry Windows; nenhum relacionado a recurso XAML). Tempo ~3,4s.

### Fora de escopo (polish fase 2)

A **neutralização do chrome** — trocar os ícones/molduras hoje em tan (`TrlAccentBrush`/`TrlTan400Brush`, ex.: ícones Server/Profile do `ModInfoCard`, ícone Open do `TotalModsCard`) por tons neutros, para que o gold marque significado em vez de tingir o chrome inteiro (recalibração grafite v2) — **fica como polish de fase 2**. Este item foi restyle mecânico key-por-key preservando a aparência atual; nenhuma decisão de cor foi alterada.
