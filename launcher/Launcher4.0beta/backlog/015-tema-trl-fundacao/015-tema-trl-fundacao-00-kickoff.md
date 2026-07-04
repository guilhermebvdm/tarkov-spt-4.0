# 015 — Fundação de tema TRL · Kickoff

**Launcher:** Launcher4.0beta · **Data:** 2026-07-03 · **Origem:** decisão de redesign premium (plano aprovado 2026-07-03; decisão do usuário: seguir o design-system — tan como accent de trabalho)
**Executa ANTES do 004** (todos os restyles view-a-view consomem esta fundação).

> Brief de kickoff — insumo para a spec/implementação. Fonte de verdade visual: `design-system/{tokens.css, components.css, PATTERNS.md}` (regras R1–R7).

## Objetivo

Traduzir o TRL Design System (web/CSS) para uma fundação de tema Avalonia reutilizável: tokens semânticos, estilos de controles, controles de assinatura e fontes — corrigindo a violação R1 atual (vermelho como accent primário → tan `#c7b48a`; vermelho <5%: logo/laser/danger).

## Arquitetura

- **Estrutura:** `Assets/Theme/{Trl.axaml, Tokens.axaml, Typography.axaml, Controls/*.axaml}` + `CustomControls/{TrlLaserDivider, TrlScreenBar, TrlPanel, TrlTag, TrlVersionFooter, TrlSidebarNav, TrlDialogChrome}`. `App.axaml`: FluentTheme → DialogHost → `Trl.axaml` (ordem importa).
- **Tokens (~28 brushes `Trl*`)** mapeados 1:1 do `tokens.css` camada semântica; translúcidos como Color com alpha (ex.: `TrlEdgeBrush #33C7B48A`, `TrlBgHoverBrush #14C7B48A`). Essenciais: BgApp `#12130D`, GroundDeep `#0D0E09`, BgPanel `#1B1D14`, BgRaised `#22251A`, BgInput `#0D0E09`, Fg `#E9E7DD`, FgMuted `#9A978A`, FgFaint `#6F6D60`, FgOnAccent `#12130D`, Accent `#C7B48A`, AccentStrong `#D8C9A4`, AccentDim `#8F8560`, Danger `#D27A7A`, DangerStrong `#D92C20`, Success `#9AD27A`, Warning `#CC9A3E`, Brand `#FF0000` (SÓ glow/laser/logo). Métricas: controle 30/24px, screen-bar 34px, sidebar 280px.
- **Shim de compatibilidade:** re-apontar keys legadas no App.axaml (`AccentBrush`→tan, `AccentBrush2/3`→tan dim/escuro, `AltAccentBrush*`→tan ramp, `BackgroundBrush`→`#12130D`, `BackgroundBrush2`→`#1B1D14`, `AltBackgroundBrush`→`#22251A`, `ForegroundBrush`→`#E9E7DD`) — views não migradas mudam de paleta sem quebrar. Removido no 014.
- **Controles (Styles `/template/` sobre FluentTheme):** `ControlCornerRadius=0` + `OverlayCornerRadius=0` globais (R2). Button base outlined tan + classes `.primary` (tan sólido, texto ink-inverse, bold) / `.danger` (red-500 `#D92C20`, SÓ destrutivo) / `.ghost` / `.icon` (30×30) / `.sm` (24px). TextBox: bg input, borda edge, focus = borda accent-dim + BoxShadow glow tan. CheckBox 15px quadrado. **ToggleSwitch = ControlTheme completo** (trilho 32×16 retangular, knob 10×10 quadrado). ListBox classe `trl-nav`: item MinHeight 36, accent bar esquerda 2px em `:selected` + wash tan. ComboBox como input + popup surface-2. ScrollBar fino (10px, thumb `#282C1E`). ProgressBar 6px com gradiente `#6B6247→#C7B48A` no Foreground. Chamfer/clip-path: fase 2 — NÃO bloqueia.
- **Tipografia:** display = Bender (converter `design-system/fonts/*.woff2`→TTF via fonttools; licença: freeware Jovanny Lemonad, cf. `LICENSE-NOTE.txt`), fallback `Bahnschrift, Segoe UI`; corpo Segoe UI 12–13px; números `Cascadia Mono, Consolas` tabular. ⚠️ `AssemblyName` = "Tarkov Red Line" (espaços) → URI `avares://Tarkov%20Red%20Line/...`; validar no primeiro spike; plano B = registrar fonte via código ou lançar com Bahnschrift. Uppercase: escrito no XAML ou `ToUpperConverter` (Avalonia não tem text-transform). LetterSpacing: wide≈1.0px, wider≈1.6px, widest≈2.4px @11-12px.
- **Assinaturas:** `TrlLaserDivider` (Border 1px, LinearGradient transparente→`#FF0000`→transparente + BoxShadow `0 0 8 0 #73FF0000`) — **1 global no TitleBar**, máx. 1 por view (R1). TitleBar: 34px, fundo GroundDeep, close hover = DangerStrong. Vignette: Border overlay com RadialGradientBrush `#0FC7B48A`→transparente. Scanlines: omitidas no v1. Fotos bg1/bg2 mantidas com overlay olive (`#E612130D` bordas → `#8012130D` centro); painéis sobre foto = `#F21B1D14`.

## Escopo

1. Fundação completa (tokens, controles, custom controls, fontes, shim, TitleBar com laser).
2. **Views-piloto:** LoginView, RegisterView, ConnectServerView + SPTNotificationView (toast TRL: surface-2, borda edge-strong, barra esquerda 2px por tipo). Absorver/aposentar `Assets/Styles.axaml` legado (`.card` com CornerRadius=5 viola R2).
3. `TrlVersionFooter` nasce aqui com binding preparado (013 só liga o dado). Não re-hardcodar strings de versão.

## Restrições de coordenação (W1 paralela)

- NÃO tocar: `ProfileView/SettingsView` (012 remove Targram em paralelo), `ClassSelectionView` (004L na W2), `TailscaleHelper` (006).
- Não mudar bindings/`x:Name`/commands de nenhuma view — só estilo/layout/recursos.

## DoD

- `dotnet build SPT.Launcher.csproj` verde; launcher abre e navega Connect→Login→Register sem regressão funcional; pilotos com visual TRL (radius 0, tan, Bender/Bahnschrift); demais views com paleta shim coerente; zero `#FF0000`/vermelho fora de laser/logo/danger.
