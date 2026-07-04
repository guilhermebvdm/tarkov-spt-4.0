# 015 — Fundação de tema TRL · Spec (funcional + técnica)

**Data:** 2026-07-03 · **Base:** kickoff 00 + `design-system/{tokens.css, components.css, PATTERNS.md}` · **Régua:** média (spec fundida)

## O que muda

Fundação de tema Avalonia traduzindo o TRL DS (web) — corrige R1 (vermelho era accent; vira tan `#C7B48A`, vermelho só laser/logo/danger) e R2 (radius 0 global). Views-piloto: Login, Register, ConnectServer, SPTNotification. Demais views mudam de paleta via shim das keys legadas.

## Plano de arquivos

**Novos — tema** (`Assets/Theme/`): `Trl.axaml` (master `<Styles>`: resources merged + StyleIncludes), `Tokens.axaml` (brushes/cores/shadows/métricas), `Typography.axaml` (fonts + tamanhos + tracking), `Controls/{Text, Button, TextBox, CheckBox, ComboBox, ListBox, ScrollBar, ProgressBar, Legacy}.axaml` (Styles), `Controls/ToggleSwitch.axaml` (ControlTheme completo, trilho 32×16 ret., knob 10×10 quadrado — base copiada do Fluent 11.1.1 p/ manter PART_MovingKnobs/PART_SwitchKnob funcionais), `Controls/TrlCustomControls.axaml` (ControlThemes dos content-hosts).

**Novos — CustomControls/**: `TrlLaserDivider` (UserControl, 1px gradiente laser + glow), `TrlScreenBar` (UserControl: Title/Meta/DotBrush/ShowDot, 34px), `TrlTag` (UserControl: Text/DotBrush), `TrlVersionFooter` (UserControl: LauncherVersion="15.0"/ServerVersion="0.10" defaults atuais; 013 liga o dado), `TrlPanel` (ContentControl: Title/ShowHeader), `TrlSidebarNav` (ContentControl: Header, 280px), `TrlDialogChrome` (ContentControl: Title). *Desvio registrado:* content-hosts são ContentControl+ControlTheme (UserControl não hospeda Content externo sem substituir o próprio XAML); controles "folha" seguem o padrão TitleBar (UserControl+StyledProperties).

**Editados:** `App.axaml` (ordem FluentTheme→DialogHost→Trl; `ControlCornerRadius=0`/`OverlayCornerRadius=0`; shim Dark), `SPT.Launcher.csproj` (`AvaloniaResource Remove` p/ Theme/*.axaml; limpa refs de Styles.axaml), `MainWindow.axaml` (remove StyleInclude legado; TitleBar sem Background inline; overlay do DialogHost → TrlBgOverlay), `CustomControls/TitleBar.axaml` (34px, GroundDeep via estilo, close hover DangerStrong, laser global no rodapé), 4 views-piloto.

**Removido:** `Assets/Styles.axaml` (absorvido em `Controls/Legacy.axaml` com radius 0 e paleta TRL).

## Mapa de tokens (semântica → brush `Trl*`)

| Token CSS | Key | Valor |
|---|---|---|
| ground-deep / bg-input | TrlGroundDeepBrush / TrlBgInputBrush | `#0D0E09` |
| bg-app / bg-panel / bg-raised | TrlBgAppBrush / TrlBgPanelBrush / TrlBgRaisedBrush | `#12130D` / `#1B1D14` / `#22251A` |
| surface-3 | TrlSurface3Brush | `#282C1E` |
| bg-hover / bg-active | TrlBgHoverBrush / TrlBgActiveBrush | `#14C7B48A` / `#24C7B48A` |
| bg-overlay | TrlBgOverlayBrush | `#A6080905` |
| edge faint/base/strong/red | TrlEdge{Faint,,Strong,Red}Brush | `#1A…/#33…/#6BC7B48A/#59FF3020` |
| fg / muted / faint / ghost / on-accent | TrlFg{,Muted,Faint,Ghost,OnAccent}Brush | `#E9E7DD/#9A978A/#6F6D60/#45443A/#12130D` |
| accent / strong / dim | TrlAccent{,Strong,Dim}Brush | `#C7B48A/#D8C9A4/#8F8560` |
| tan-400/600/700 (pressed/gradiente/scroll) | TrlTan{400,600,700}Brush | `#AB9A71/#6B6247/#453F2D` |
| danger / strong / hover / pressed | TrlDanger{,Strong,Hover,Pressed}Brush | `#D27A7A/#D92C20/#F04438/#A8231A` |
| success / warning / brand | TrlSuccess/TrlWarning/TrlBrandBrush | `#9AD27A/#CC9A3E/#FF0000` (brand SÓ laser/logo) |
| washes green/amber/red | TrlWash{Green,Amber,Red}Brush | alpha 10–12% |
| tier 3 | TrlLaserBrush, TrlProgressFillBrush, TrlScreenBarBgBrush, TrlPhotoOverlayBrush, TrlPanelOverPhotoBrush (`#F21B1D14`), TrlVignetteBrush | gradientes/translúcidos |
| glows/shadows | TrlGlowTanShadow, TrlGlowRedShadow, TrlShadow2 | BoxShadows |

Métricas: TrlControlHeight 30 · TrlControlHeightSm 24 · TrlScreenBarHeight 34 · TrlSidebarWidth 280. Tipos: TrlFontDisplay = `avares://Tarkov%20Red%20Line/Assets/Fonts#Bender, Bahnschrift, Segoe UI` (TTFs convertidos de woff2 via fonttools; validação em runtime — plano B: Bahnschrift), TrlFontBody = Segoe UI, TrlFontMono = Cascadia Mono/Consolas. Tracking: 1.0/1.6/2.4px (`LetterSpacing`).

## Controles (styles sobre FluentTheme)

- **Button** base outlined tan (30px, borda edge-strong, texto accent display 12) + `.primary` (tan sólido, FgOnAccent, bold) / `.danger` (DangerStrong; só destrutivo) / `.ghost` / `.icon` (30×30) / `.sm` (24px). Shims legados: `acc`→primary-look, `alt`/`outlined`→base, `link`/`ulink`→link tan, `transparent`→ghost-look, `icon` mantém.
- **TextBox** bg input, borda edge, watermark ghost; focus = borda AccentDim + BoxShadow glow tan; hover = edge-strong. **CheckBox** caixa quadrada, borda edge-strong, glyph/borda accent no checked. **ToggleSwitch** ControlTheme completo (retangular). **ComboBox** como input; popup raised via override das keys Fluent (`ComboBoxDropDownBackground`, `ComboBoxItemBackground*`). **ListBox.trl-nav** item 36px, barra accent 2px `:selected` + wash. **ScrollBar** `ScrollBarSize=10`, thumb surface-3. **ProgressBar** 6px, fill gradiente tan-600→tan-300; `.error` → DangerStrong (anima 0→100 como hoje). **Legacy.axaml**: `card` (radius 0, bg panel, borda edge), Labels `acc/alt/versionMismatch`, Separator, NotificationCard/WindowNotificationManager, cc|TitleBar (GroundDeep).

## Views-piloto (sem mudar bindings/x:Name/commands/fluxo)

- **Login/Register:** foto bg1 + overlay olive radial (`#8012130D` centro → `#E612130D` borda); painel 450px `#F21B1D14` c/ borda edge e padding-top 34 (título-bar agora sólida); labels display UPPERCASE (AccentDim, tracking); ação principal `.primary` ("ENTRAR"/"CRIAR CONTA"), secundária outlined; "Redefinir" vira link tan; erro Register → TrlDanger; footer → `TrlVersionFooter` (valores atuais como default).
- **ConnectServer:** fundo TrlBgApp, label status FgMuted, ProgressBar temado, Retry outlined.
- **SPTNotification:** surface-2 + borda edge-strong 1px + barra esquerda 2px `{Binding BarColor}` (cores por tipo continuam no VM — VM é intocável neste item; migração p/ tokens = fase 2).
- **TitleBar:** 34px, GroundDeep, minimize hover wash tan, close hover DangerStrong/pressed DangerPressed, `TrlLaserDivider` no rodapé — o único laser (R1).

## Riscos

1. **URI avares com espaço** (`AssemblyName` = "Tarkov Red Line") p/ fonte — mitigação: fallback Bahnschrift na própria FontFamily + smoke-run; plano B documentado.
2. **Styles legadas absorvidas** — views não-piloto dependem de `card/acc/alt/...`; Legacy.axaml preserva todas as classes usadas (auditadas via grep).
3. **ToggleSwitch ControlTheme** — sem consumidor atual; parts PART_* mantidos 1:1 com Fluent 11.1.1.
4. Double-include de axaml sob `Assets\**` — resolvido com `AvaloniaResource Remove` (mesmo padrão do Styles.axaml antigo).

## DoD

Build Release verde; pilotos TRL (radius 0, tan, display font); zero vermelho fora laser/logo/danger; shim coerente nas demais views.
