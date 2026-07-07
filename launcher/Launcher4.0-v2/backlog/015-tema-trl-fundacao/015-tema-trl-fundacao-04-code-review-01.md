# 015 — Fundação de tema TRL · Code Review 01 (adversarial)

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Commits revisados:** `6cddece` (fundação + pilotos) **+** `8fa0190` (re-sync grafite + bg-hero), como conjunto · **Insumos:** [00-kickoff](./015-tema-trl-fundacao-00-kickoff.md) · [01-spec](./015-tema-trl-fundacao-01-spec.md) · [05-asbuild](./015-tema-trl-fundacao-05-asbuild.md) · `design-system/{tokens.css, components.css, PATTERNS.md}` @ HEAD (recalibração grafite `e29fced`)

> Review de contexto limpo (revisor não escreveu o código). Todos os scans (classes, keys, hex) foram **pinados no tree de `8fa0190`** — commits posteriores (004L/013L etc.) aterrissaram em paralelo durante a review e estão fora de escopo. Referências de linha no estado de `8fa0190`. Paleta olive→grafite NÃO é apontada como defeito (é a recalibração do DS). Build gate: `dotnet build SPT.Launcher.csproj -c Release` → **0 erros** (127 warnings pré-existentes de nullability/CA1416 em ViewModels/TailscaleHelper, fora do escopo).

**Placar:** 0 🔴 · 3 🟡 · 3 🟢

Nenhum defeito que cause quebra em runtime ou regressão visual grave foi encontrado. Os 3 🟡 são desvios latentes vs o design-system (nenhum tem consumidor renderizado em `8fa0190`; todos ficam visíveis quando 004+ adotarem os controles).

---

## Achados

### CR-01-01 [🟡] TrlTag: moldura neutra em vez de `TrlEdgeAccentBrush` + dot sem default — perde a assinatura "gold" do tag

`CustomControls/TrlTag.axaml:6`: `BorderBrush="{DynamicResource TrlEdgeStrongBrush}"`. Antes da recalibração isso ERA a moldura tan (`#6BC7B48A`); depois do `8fa0190`, `TrlEdgeStrongBrush` virou hairline neutra `#33FFFFFF` — e o próprio `8fa0190` criou `TrlEdgeAccentBrush` (`#6BC7B48A`) com o comentário "moldura tan reservada a elementos que precisam ler gold: tags/badges", mas **não re-apontou o TrlTag**. Fonte de verdade: `components.css:1315` (`.trl-tag { border: … var(--trl-edge-accent) }`) e PATTERNS R2 ("A moldura tan é `--trl-edge-accent`, reservada a elementos de accent (tag, badge)").

Dois desvios secundários no mesmo controle vs `.trl-tag`:
- `TrlTag.axaml:13`: `DotBrush` não tem default (`IBrush?` = null) → tag criado só com `Text` renderiza **sem dot** (o DS default é dot accent: `.trl-tag__dot { background: var(--trl-accent) }`).
- Dot sem glow (`--trl-glow-tan` no DS). `Ellipse` não suporta `BoxShadow` em Avalonia — se for entregar o glow, precisa de `Border` circular (círculo em dot é exceção sancionada pela R2) ou fica registrado como fase 2.

Cenário concreto: 004 monta a screen-bar/status com `<cc:TrlTag Text="ONLINE"/>` → sai uma caixinha de borda branca 20% sem dot — lê como "estrutura neutra", não como o tag-assinatura gold do DS. Zero crash, mas é exatamente o tipo de deriva que a fundação existe para impedir.

**Fix:** `BorderBrush` → `{DynamicResource TrlEdgeAccentBrush}`; default de `DotBrush` → tan accent (ex.: registrar a StyledProperty sem default e no template usar fallback, ou setter no code-behind com `TrlAccentBrush` resolvido via recurso). Glow do dot: decidir agora (Border circular + `TrlGlowTanShadow`) ou anotar fase 2 no as-built.

### CR-01-02 [🟡] TrlPanel: título do header em gold-dim viola R4 ("gold em label é defeito") pós-grafite

`Assets/Theme/Controls/TrlCustomControls.axaml:35`: o título do `TrlPanel` usa `Foreground="{DynamicResource TrlAccentDimBrush}"`. A recalibração grafite (PATTERNS R4 + `components.css:257` `.trl-panel__title { color: var(--trl-fg-label) }`) definiu que **título de painel é chrome → neutro** (`fg-label` = ink-muted); o `8fa0190` trocou o `.trl-label` do Text.axaml para `TrlFgLabelBrush` mas esqueceu o header do `TrlPanel`, que hardcoda a cor por fora da classe.

Agravante de contraste: `TrlAccentDimBrush` (`tan-500 #8F8560`) sobre superfícies elevadas é 3.9–4.3:1 (PATTERNS R5 — "nunca conteúdo essencial em superfície elevada").

Cenário concreto: 004/012 embrulham os cards de Profile/Settings em `TrlPanel Title="..."` → toda a tela ganha títulos dourados de chrome, exatamente o anti-pattern que a recalibração removeu do DS web ("gold marca significado, não tinge a tela").

Nota de escopo: `TrlDialogChrome` (`:75`) e `TrlScreenBar` usam `TrlAccentBrush` no título e isso **está certo** — `.trl-screen-bar__title { color: var(--trl-accent) }` é exceção por design (confirmado em `components.css:198-200`).

**Fix:** só o `TrlPanel`: `Foreground` do título → `{DynamicResource TrlFgLabelBrush}`.

### CR-01-03 [🟡] ToggleSwitch: trilho "on" com hex órfão da era olive (`#27251B`) hardcoded fora do Tokens.axaml

`Assets/Theme/Controls/ToggleSwitch.axaml:80`: `SwitchKnobBounds` (trilho checked) tem `Background="#27251B"` — um marrom-oliva que (a) não existe em nenhuma camada do `tokens.css` atual, (b) é o **único hex olive remanescente** no launcher inteiro em `8fa0190` (scan pinado por toda a família `0D0E09/12130D/1B1D14/22251A/282C1E/E9E7DD/9A978A/6F6D60/45443A` + tan-edges antigos: nenhum outro hit), e (c) contradiz o header do próprio Tokens.axaml ("This file is the ONLY place hex values are allowed"). Os duplicados intencionais do Trl.axaml (overrides de keys Fluent) estão documentados e foram re-sincronizados no `8fa0190` — este não.

Cenário concreto: o primeiro consumidor de ToggleSwitch (Settings no restyle 004) liga o switch → trilho marrom-oliva sobre chrome grafite, destoando de todas as superfícies vizinhas.

**Fix:** trocar por token — opções fiéis ao DS: `TrlBgActiveBrush` (wash tan .14 sobre o input escuro, coerente com "hover/seleção usam wash" da R4) ou criar `TrlTan800Brush #2B2719` espelhando `--trl-tan-800` ("fills and gradient tails"). Qualquer uma remove o hex solto do arquivo de controle.

### CR-01-04 [🟢] GameLaunchBar: `ProgressBar Height="4"` é vencido pelo `MinHeight=6` do tema (cosmético, morre na migração)

`Assets/Theme/Controls/ProgressBar.axaml` define `MinHeight=6`/`Height=6 (:horizontal)`; `CustomControls/GameLaunchBar.axaml:36-40` pede `Height="4"` inline → layout resolve max(4, 6) = **6px**. Barra de update do launcher fica 2px mais alta que o autor da view pediu. Sem quebra funcional; view não-piloto que será migrada no 004. Registrar e seguir.

### CR-01-05 [🟢] Classes no-op herdadas — sem regressão, limpar na migração

- `GameLaunchBar.axaml:38` `Classes="accent"` em ProgressBar: nunca teve definição (nem no Styles.axaml antigo, nem no Fluent para ProgressBar) — era e continua no-op.
- `Classes="dark"` (TextBoxes de Login/Register pré-015): também nunca teve definição; o 015 removeu as ocorrências — zero restantes em `8fa0190`.

### CR-01-06 [🟢] Notas menores de fidelidade/completude (nenhuma exige ação agora)

- `--trl-surface-4 #303034` não tem correspondente `TrlSurface4Brush` (nenhum consumidor precisa dele hoje; criar quando precisar).
- `TrlVersionFooter` tem rótulos PT hardcoded ("Versão do launcher:") — comportamento idêntico ao footer antigo (que também não usava `LocalizationProvider`); débito pré-existente, não regressão do 015.
- `.trl-btn--primary` do DS tem um top-light gradiente ("machined-metal") que a versão Avalonia não replica — coerente com o corte declarado de chamfer/fase 2 no kickoff/as-built.

---

## Áreas auditadas e limpas (com evidência)

### 1. Cobertura do Legacy.axaml — RISCO Nº1: **completa, zero classe órfã**

Inventário exaustivo de `Classes=`/`Classes.x=` nas views/controles NÃO-piloto no tree de `8fa0190`, classe a classe:

| Classe | Onde é usada (não-piloto) | Definição no tema novo |
|---|---|---|
| `card` (Border) | ModInfoView, DetailedProfileCard, GameLaunchBar, LoginBox, ModInfoCard, ProfileCard, TotalModsCard | `Legacy.axaml` `Border.card` (radius 0) ✓ |
| `acc` (Label) | GameLaunchBar, DetailedProfileCard, TotalModsCard, ModInfoCard, LoginBox, dialogs | `Legacy.axaml` `Label.acc` ✓ |
| `acc` (Button, dialogs) / `acc` (TextBlock) | dialogs; ModInfoView | `Button.axaml` shim `Button.acc` ✓ / `Legacy.axaml` `TextBlock.acc` ✓ |
| `alt` (TextBlock/Label) | ModInfoView, DetailedProfileCard | `Legacy.axaml` `TextBlock.alt` + `Label.alt` ✓ |
| `link` / `ulink` | ModInfoView, ModInfoCard; GameLaunchBar, ProfileCard | `Button.axaml` shims ✓ |
| `icon` | ProfileCard, TotalModsCard | `Button.axaml` `Button.icon` ✓ |
| `transparent` | ClassSelectionView, LoginBox | `Button.axaml` `Button.transparent` ✓ |
| `outlined` | (sem uso em `8fa0190`) | base do Button já é outlined ✓ |
| `versionMismatch` (Label, via `Classes.versionMismatch` binding) | ProfileCard, DetailedProfileCard | `Legacy.axaml` `Label.versionMismatch` ✓ |
| `versiontag` (cc:TitleBar) | MainWindow (`Classes.versiontag="False"`) | `Legacy.axaml` `cc|TitleBar.versiontag` ✓ |
| `error` (ProgressBar) | ConnectServerView | `Legacy.axaml` `ProgressBar.error` (animação 0→100 preservada) ✓ |
| `SidebarMenu/Active/PanelCard/ActionButton/AltButton/PanelButton/CleanupButton` | ProfileView/SettingsView | **locais** (`UserControl.Styles` das próprias views) — independem do tema ✓ |
| `accent` (ProgressBar) | GameLaunchBar | órfã **desde antes** do 015 (no-op → no-op, CR-01-05) |

Estilos globais do Styles.axaml antigo (TextBox/Button/ProgressBar/CheckBox/Label/Separator/NotificationCard/WindowNotificationManager/cc|TitleBar) todos re-cobertos pelo tema novo ou pelo Legacy.axaml. Refs residuais a `Assets/Styles.axaml` existem apenas em `Launcher3.11/` e `Launcher4.0/` (projetos irmãos intocados, cada um com sua cópia própria do arquivo).

### 2. Keys de recurso: **todas resolvem**

Extração automatizada no tree pinado: **71 keys** consumidas via `{DynamicResource}`/`{StaticResource}` em todos os .axaml do projeto — `comm` contra o conjunto de `x:Key` definidos = **zero ausentes**. Em particular: os 12 `PathGeometry` (FolderWithPlus/OpenFolder/Alert/Delete/Gear/Server/Profile/Open/Info/BackArrow/Close/Minimize) continuam no `App.axaml`; o shim Dark cobre **todas** as keys legadas consumidas (`AccentBrush`, `AccentBrush2/3`, `AltAccentBrush`, `AltAccentBrush2/3`, `BackgroundBrush`, `BackgroundBrush2`, `AltBackgroundBrush`, `ForegroundBrush`) — inclusive os dois lookups em code-behind (`DetailedProfileCard.axaml.cs:57,72` `TryFindResource("AltBackgroundBrush"/"BackgroundBrush")`, cobertos em ambos os variants).

### 3. Ordem de merge/parse: **overrides vencem**

- `ControlCornerRadius=0`/`OverlayCornerRadius=0` em `Application.Resources`: `Application.TryGetResource` consulta `Resources` **antes** de `Styles` → vence as definições do FluentTheme (que vivem em resources de style). ✓
- Overrides de keys Fluent (`ComboBoxDropDown*`, `ComboBoxItem*`, `ScrollBarSize`) em `Trl.axaml` (último `StyleInclude`): lookup em `Styles` itera em ordem **reversa** → Trl vence Fluent/DialogHost. ✓
- ControlThemes keyed por `x:Type` (ToggleSwitch, TrlPanel/TrlSidebarNav/TrlDialogChrome) em resources do último Styles → vencem o Fluent pelo mesmo mecanismo. ✓
- **ToggleSwitch ControlTheme vs Fluent 11.1.1:** `PART_SwitchKnob` (Canvas), `PART_MovingKnobs` (Grid), `PART_On/OffContentPresenter` e a propriedade `KnobTransitions` verificados **presentes na Avalonia.Controls 11.1.1** (strings inspecionadas na DLL do NuGet); ambos os parts são `Panel` como o code-behind exige; travel 18px consistente (canvas 28 − knob 10).

### 4. Fontes: **verificado em runtime** (além do smoke do as-built)

Harness headless próprio (console net9 + Avalonia 11.1.1 `SetupWithoutStarting`, carregando a DLL Release real do launcher):
- `AssetLoader.GetAssets("avares://Tarkov%20Red%20Line/Assets/Fonts")` → **2 assets** (`Bender-Bold.ttf`, `Bender-Regular.ttf`) — a URI percent-encoded com AssemblyName com espaços **resolve** (o UriParser `avares` do Avalonia usa GenericAuthority; a forma com espaço literal também funciona).
- `FontManager.TryGetGlyphTypeface` sobre `TrlFontDisplay` → resolve **"Bender"** (regular **e** Bold), ou seja, não caiu no fallback Bahnschrift — a pendência nº3 do as-built (confirmação de glyphs) está **fechada por evidência**.
- Nota: `new Uri("avares://Tarkov%20Red%20Line/…")` **lança** `UriFormatException` em .NET puro sem o UriParser do Avalonia registrado — o funcionamento depende da init do app (sempre presente no launcher real; cuidado apenas em testes/ferramentas fora do AppBuilder.
- csproj: `AvaloniaResource Include="Assets\**"` cobre `Assets/Fonts/*.ttf` e `bg-hero.jpg`; o `Remove` atinge só `Assets\Theme\**\*.axaml` (anti double-include, padrão já usado antes). ✓

### 5. Pilotos: **bindings/x:Name/commands 1:1**

Diff `6cddece~1 → 8fa0190` conferido linha a linha:
- **Login:** `Login.Username`, `Login.Password`, `ResetPasswordCommand`, `LoginCommand`, `GoToRegisterCommand` intactos; nenhum `x:Name` existia; nenhum TextBlock perdeu texto (títulos re-escritos em UPPERCASE conforme spec).
- **Register:** `RegisterUsername/RegisterPassword/ConfirmPassword`, `RegisterErrorMsg` (+converter IsNotNullOrEmpty), `GoToClassSelectionCommand`, `GoToLoginCommand` intactos; erro → `trl-danger` (`#D27A7A`, o único vermelho legal para texto — R1 ✓).
- **ConnectServer:** `connectModel.InfoText/ConnectionFailed/IsDownloading/...`, `Classes.error` binding e `RetryCommand` intactos; Retry virou outlined base (spec).
- **SPTNotification:** `Title`, `Message`, `BarColor` intactos (cores por tipo seguem no VM, decisão registrada no as-built); `WindowNotificationManager` Margin `0 35` alinha com TitleBar 34px + laser 1px.
- **TrlVersionFooter:** defaults `"15.0"`/`"0.10"` nas StyledProperties ✓ (013 liga o dado); textos do footer preservados.
- **MainWindow/TitleBar:** `SettingsButtonCommand` segue exposto (nenhum botão foi removido — o TitleBar antigo também só tinha Min/Close); `StaticResource Minimize/Close` resolvem no App.axaml.

### 6. R1/R2: **limpos nos pilotos e no tema**

- Scan pinado por vermelho (`#FF0000`, `Red`, `#CC1111`, `#990000`, `Crimson`, `IndianRed`, `OrangeRed`) em todos os .axaml do projeto: únicas ocorrências são `TrlBrandBrush`/`TrlLaserBrush`/`TrlGlowRedShadow` (Tokens) — os `#CC1111`/`#990000`/IndianRed/Crimson antigos foram todos removidos. Danger usado só em: close do TitleBar (hover/pressed, spec'ado), `Button.danger`, `ProgressBar.error`, `trl-danger` (texto de erro). Exatamente **1 laser** global (TitleBar); nenhum laser adicional nos pilotos. ✓
- `CornerRadius` > 0: zero no tema/pilotos/custom controls. Único hit no projeto: dot circular de status no SettingsView (`CornerRadius="10"`, arquivo do 012) — círculo de status é exceção explícita da R2. ✓

### 7. Tokens.axaml vs `tokens.css` @ HEAD: **1:1, zero drift**

Conferência valor a valor dos ~36 brushes pós-`8fa0190`, incluindo alphas: surfaces grafite (`#0D0D0E/#131314/#1B1B1D/#222225/#29292C`), inks (`#E8E7E4/#9B9A96/#706F6B/#464541/#131314`), edges neutros (.05/.09/.20 → `#0D/#17/#33FFFFFF`), `edge-accent` .42 → `#6B`, `edge-red` .35 → `#59`, washes (tan .08/.14, green .10, amber .12, red .10), tan ramp 200–700, red ramp (soft/500/400/600), glows (.35 tan / .45 red), shadow-2 (.5), laser (stops .55/1.0 nos offsets 22/50/78%), screen-bar bg (rgba(0,0,0,.25) → `#40000000`), progress fill (tan-600→tan-300). **Atenção:** `TrlBgOverlayBrush #A6080905` parece olive mas é fiel — o próprio DS manteve `--trl-bg-overlay: rgba(8,9,5,.65)` na recalibração. Métricas 30/24/34/280 ✓. Único hex fora da tabela: CR-01-03.

### 8. Hex olive órfão (pergunta direta da re-review)

Scan pinado em todos os `.axaml`/`.cs` do launcher pela família olive completa + tan-edges antigos (`1A/33/6BC7B48A`): **único órfão = `#27251B`** (CR-01-03). `#6BC7B48A` sobrevive apenas como `TrlEdgeAccentBrush` (legítimo, é o `--trl-edge-accent` do DS).

---

## Veredito

Fundação sólida: cobertura legacy completa, grafo de recursos fechado, ordem de merge correta, fontes provadas em runtime, pilotos sem regressão funcional, R1/R2 cumpridas. Os 3 🟡 são correções pequenas e localizadas (2 setters de brush + 1 hex→token) que valem entrar **antes** do 004 consumir TrlTag/TrlPanel/ToggleSwitch — depois disso viram regressão visual visível em várias telas de uma vez.

---

## Resoluções (2026-07-04, aplicadas pelo orquestrador)

| Achado | Resolução |
|---|---|
| CR-01-01 🟡 | ✅ Aplicado — TrlTag: moldura → `TrlEdgeAccentBrush` (gold), dot com default tan (`FallbackValue`/`TargetNullValue`) + glow (`TrlGlowTanShadow`, Border radius 99 no lugar de Ellipse). |
| CR-01-02 🟡 | ✅ Aplicado — título do TrlPanel → `TrlFgLabelBrush` (chrome neutro v2); TrlScreenBar/TrlDialogChrome mantidos em accent (por design). |
| CR-01-03 🟡 | ✅ Aplicado — trilho "on" do ToggleSwitch `#27251B` → `TrlBgActiveBrush` (zero hex olive no launcher). |
| CR-01-04/05/06 🟢 | ⏭️ Aceitos sem ação (notas; revisitar no passe do 014). |
