# 015 — Fundação de tema TRL · As-built

**Data:** 2026-07-03 · **Status:** ✅ Entregue (build Release verde + smoke-run sem crash) · **Spec:** [01-spec](./015-tema-trl-fundacao-01-spec.md)

## O que foi entregue

### Tema (`Assets/Theme/`)
- `Tokens.axaml` — ~35 brushes semânticos `Trl*` (bg/fg/accent/status/edge/wash) + tier 3 (TrlLaserBrush, TrlProgressFillBrush, TrlScreenBarBgBrush, TrlPhotoOverlayBrush, TrlPanelOverPhotoBrush, TrlVignetteBrush), BoxShadows (TrlGlowTan/GlowRed/Shadow2) e métricas (30/24/34/280).
- `Typography.axaml` — `TrlFontDisplay` = `avares://Tarkov%20Red%20Line/Assets/Fonts#Bender, Bahnschrift, Segoe UI` + body/mono + escala de tamanhos + tracking (LetterSpacing 1.0/1.6/2.4).
- `Controls/` — `Text` (classes trl-label/trl-h1..3/trl-muted/trl-danger/…), `Button` (base outlined tan + `.primary`/`.danger`/`.ghost`/`.icon`/`.sm` + shims `acc`/`alt`/`link`/`ulink`/`transparent`/`outlined`), `TextBox` (bg input, focus accent-dim + glow tan), `CheckBox`, `ToggleSwitch` (**ControlTheme completo** — trilho 32×16 retangular, knob 10×10 quadrado, PART_* 1:1 com Fluent 11.1.1, travel 18px), `ComboBox` (+ overrides de keys Fluent p/ popup/itens no Trl.axaml), `ListBox` (classe `trl-nav`: item 36px, barra accent 2px), `ScrollBar` (10px, thumb surface-3), `ProgressBar` (6px, gradiente tan; `.error` → DangerStrong com animação preservada), `Legacy` (absorve o antigo Styles.axaml: `card` radius 0, Labels, Separator, NotificationCard/WindowNotificationManager, cc|TitleBar), `TrlCustomControls` (ControlThemes dos content-hosts).
- `Trl.axaml` — master include; registrado no `App.axaml` na ordem FluentTheme → DialogHost → Trl.

### CustomControls de assinatura
`TrlLaserDivider` (1px laser + glow), `TrlScreenBar` (34px; Title/Meta/ShowDot/DotBrush), `TrlTag` (Text/DotBrush), `TrlVersionFooter` (defaults "15.0"/"0.10" — 013 liga o dado real), `TrlPanel` (Title/ShowHeader), `TrlSidebarNav` (Header, 280px), `TrlDialogChrome` (Title, header screen-bar 34px).

### App/janela
- `App.axaml`: `ControlCornerRadius=0` + `OverlayCornerRadius=0` (R2 global); shim Dark re-apontado (AccentBrush→tan `#C7B48A`, AccentBrush2/3→`#8F8560`/`#6B6247`, AltAccentBrush*→tan ramp, Background*→oliva `#12130D`/`#1B1D14`/`#22251A`, ForegroundBrush→`#E9E7DD`). Sai no 014.
- `MainWindow.axaml`: StyleInclude legado removido; TitleBar sem Background inline (estilo do tema = GroundDeep); DialogHost OverlayBackground → `TrlBgOverlayBrush` (era `Gray`).
- `TitleBar.axaml`: 34px, ícones 14px, minimize hover wash tan, close hover DangerStrong/pressed DangerPressed, **laser global no rodapé** (único vermelho de chrome — R1).
- `SPT.Launcher.csproj`: `AvaloniaResource Remove="Assets\Theme\**\*.axaml"` (evita double-include), refs de Styles.axaml removidas. **`Assets/Styles.axaml` deletado.**

### Views-piloto
- **Login/Register**: overlay olive radial sobre bg1, painel 450px `#F21B1D14` c/ hairline edge e padding-top 34, labels display UPPERCASE (trl-label), "ENTRAR"/"CRIAR CONTA" `.primary`, secundários outlined/ghost, "Redefinir" link tan, erro Register em TrlDanger, footer `TrlVersionFooter`. Bindings/commands/x:Name intactos.
- **ConnectServer**: fundo TrlBgApp, status em display muted, ProgressBars temadas (300px), Retry outlined padrão.
- **SPTNotification**: surface-2 + borda edge-strong 1px + barra esquerda 2px `{Binding BarColor}`.

### Fontes
`Assets/Fonts/Bender-{Regular,Bold}.ttf` convertidas de `design-system/fonts/*.woff2` via fonttools (name table: family "Bender", weights 400/700). Licença: freeware (cf. `design-system/fonts/LICENSE-NOTE.txt`).

## Validação
- `dotnet build SPT.Launcher.csproj -c Release` → **0 erros** (127 warnings pré-existentes de ViewModels/TailscaleHelper, fora do escopo).
- Smoke-run do exe Release: processo estável, log sem exceções; fluxo Connect→Tailscale→servidor→**LoginView renderizada** (piloto TRL) sem crash — a URI `avares://` com espaço (percent-encoded) não quebrou o load.

## Assunções/decisões registradas
1. **Content-hosts como ContentControl+ControlTheme** (TrlPanel/TrlSidebarNav/TrlDialogChrome) em vez de UserControl — UserControl não hospeda `Content` externo sem perder o próprio XAML; controles "folha" seguem o padrão TitleBar (UserControl+StyledProperties).
2. **Cores da barra do toast continuam no `SPTNotificationViewModel`** (DodgerBlue/Gold/ForestGreen/IndianRed) — ViewModels são intocáveis neste item; migração p/ tokens TRL = fase 2 (junto com pulse do dot live).
3. **Confirmação visual das glyphs Bender pendente** — smoke-run provou ausência de crash e navegação, mas sem screenshot da janela (jogo em fullscreen na máquina durante o run); pior caso a FontFamily composta cai silenciosamente em Bahnschrift (plano B do kickoff). Verificar no próximo run manual.
4. **CheckBox mantém caixa 20px do Fluent** (não 15px) — encolher o `NormalRectangle` desalinha o glyph do template; quadrado + cores TRL entregues, ajuste fino de tamanho = fase 2.
5. Overrides de keys Fluent (ComboBox popup/itens, ScrollBarSize) duplicam hex dos tokens **de propósito** (StaticResource cross-dictionary é sensível a ordem de parse); fonte da verdade segue `tokens.css`.
6. Uppercase é escrito no XAML (Avalonia não tem text-transform); `ToUpperConverter` não foi necessário nos pilotos.
7. Smoke-run criou artefatos em `bin/` (config gerado, log, conexão Tailscale via fluxo normal do Connect) — sem efeito em `D:\SPT` nem no repo.

## Fase 2 (não bloqueia 004)
- Chamfer/clip-path nos botões sólidos; scanlines; dot "live" pulsante (animation no TrlScreenBar); vignette aplicada por view.
- Migrar cores do toast VM → tokens; NotificationCard sem chrome Fluent residual.
- Tamanho 15px do CheckBox via ControlTheme próprio; ComboBox popup polish (item hover/selected já cobertos por resource override — validar visualmente).
- `trl-nav` adoption nas views com sidebar (004L/012) via `TrlSidebarNav` + ListBox.
- Screenshot de validação visual dos pilotos (Bender vs fallback) no próximo run com desktop livre.
