# 024 — Migração DS da SettingsView + unificar chrome · Spec técnica

> **Data:** 2026-07-04<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [00-kickoff](./024-settingsview-ds-migration-00-kickoff.md) · [AUDIT-2026-07-04](../../AUDIT-2026-07-04-code-product-ds.md) (§B3 + §DS) · [01-spec](./024-settingsview-ds-migration-01-spec.md)<br>

---

## Abordagem

Três frentes, todas verificadas no código real:

1. **Promover o chrome de navegação `Button.nav` para o tema** (single source), para a `SettingsView` reusar o mesmo chrome da `ProfileView`.
2. **Reescrever `SettingsView.axaml`** trocando styles locais (`SidebarMenu`/`PanelCard`/`CleanupButton`) e cores cruas por `cc:TrlSidebarNav`, `cc:TrlPanel`, classes DS (`Button.*`, `TextBlock.trl-*`) e tokens.
3. **Remover a cor crua da VM** (`DevModeStatusColor` string hex) e mover a decisão de cor para XAML dirigido por token via *class binding* no `IsDevMode` reativo.

O DS já tem tudo o que a tela precisa — nenhum token novo é necessário.

### Inventário do que existe hoje (SettingsView.axaml)

| Trecho | Linha(s) | Problema | Destino DS |
|---|---|---|---|
| `Style Button.SidebarMenu` (+hover/active) | `18-45` | `#1A1A1A`, `#222`, literal `White` | apagar; usar `Button.nav` (promovido) |
| `Style Border.PanelCard` | `46-51` | `#F2111111`, `#333333` | apagar; usar `cc:TrlPanel` |
| `Style Button.CleanupButton` (+hover) | `54-66` | `#333`, `#444`, `White` | apagar; usar `Button` base |
| `Grid ColumnDefinitions="250, *"` | `69` | sidebar 250px | `Auto, *` + `TrlSidebarNav` 280px |
| `Border Background="#111111" BorderBrush="#222"` | `72` | sidebar crua | `cc:TrlSidebarNav` |
| botões de nav `Classes="SidebarMenu"` | `80,86,92,98` | classe local | `Classes="nav"` / `nav active` |
| `Image` fundo (sem overlay) | `113` | sem scrim | + `Border TrlPhotoOverlayBrush` |
| `TextBlock "CONFIGURAÇÕES" Foreground="White" FontWeight="Black" FontSize="24"` | `119` | cru | `Classes="trl-h1"` |
| `Border Classes="PanelCard"` (×3) | `122,139,204` | style local | `cc:TrlPanel Title="…"` |
| headers `TextBlock … Foreground="White" FontSize="16"` | `124,141,206` | título no corpo | vira `Title` do `TrlPanel` |
| botões limpeza `Classes="CleanupButton"` | `127,130,133` | classe local | `Button` base (outlined) |
| `Label … Foreground="LightGray"` | `144,152,210` | cru | `TextBlock Classes="trl-label"` |
| `ComboBox … Background="#222" Foreground="White" BorderThickness="0"` | `145-148` | inline | remover inline (herda tema) |
| `ToggleSwitch` UsePerformanceConfigs | `158-171` | **já usa `trl-label`/`trl-muted`** | manter; só revisar `TrlTextXs` |
| `TextBox` senha `Background="#222" Foreground="White"` | `176-180` | inline | remover inline |
| botão olho `Background="Transparent" Foreground="Gray"` | `181-187` | cru | `Button.icon`/`.ghost` |
| botão "🔑 Dev Mode" `Background="#333" Foreground="White"` | `189-194` | cru | `Button` base |
| `Border Background="{Binding DevModeStatusColor}" CornerRadius="10"` | `195-197` | cor da VM + radius 10 | `Border.dev-dot` + class binding, radius 0 |
| `TextBox` URL `Background="#222" Foreground="White"` | `211-213` | inline | remover inline |
| `CheckBox … Foreground="LightGray"` | `216-218` | cru | remover inline |
| botão "Abrir pasta" `Background="#333"`, `Path Fill="White"` | `220-224` | cru | `Button` base; `Fill="{DynamicResource TrlFgBrush}"` |

### Frente 1 — Promover `Button.nav` ao tema

Hoje os estilos `Button.nav` vivem **locais** na `ProfileView.axaml:16-46` (base, `:pointerover`, `.active`, `.active:pointerover`, `Button.nav Path`). Para a `SettingsView` reusar o mesmo chrome sem duplicar:

- **Criar** `project/SPT.Launcher/Assets/Theme/Controls/Nav.axaml` (`<Styles>`), copiando **verbatim** o bloco `Button.nav` da `ProfileView.axaml:17-45` (tokens já são `TrlFgMutedBrush`/`TrlBgHoverBrush`/`TrlAccentBrush`/`TrlBgActiveBrush` — puro).
- **Registrar** em `Assets/Theme/Trl.axaml` um `StyleInclude Source="/Assets/Theme/Controls/Nav.axaml"` (junto aos demais, ~`:32`, antes do `Legacy.axaml`).
- A `SettingsView` passa a usar `Classes="nav"` globalmente disponível.
- **Não tocar na `ProfileView.axaml` neste item** (ver Paralelismo): o bloco local dela pode coexistir com o global (seletores idênticos, mesmos setters → sem conflito visual). A remoção do bloco redundante da `ProfileView` fica como limpeza do 025.

### Frente 2 — Reescrever `SettingsView.axaml`

Estrutura final (espelhando `ProfileView.axaml:48-93`):

```xml
<Grid ColumnDefinitions="Auto, *">
  <cc:TrlSidebarNav Grid.Column="0">
    <Grid RowDefinitions="Auto, *, Auto">
      <Image .../>                             <!-- logo -->
      <StackPanel Grid.Row="1" Spacing="2">
        <Button Classes="nav" Command="{Binding GoBackCommand}"> LAUNCHER </Button>
        <Button Classes="nav active"> CONFIGURAÇÕES </Button>  <!-- ativo, sem comando -->
        <Button Classes="nav" Command="{Binding OpenKofiCommand}"> APOIE UM CAFEZINHO </Button>
        <Button Classes="nav" IsEnabled="False" Opacity="0.5"> LISTA DE MODS </Button>
      </StackPanel>
    </Grid>
  </cc:TrlSidebarNav>

  <Grid Grid.Column="1">
    <Image Source="{Binding Background.Path, Converter=...}" .../>
    <Border Background="{DynamicResource TrlPhotoOverlayBrush}" IsHitTestVisible="False"/>
    <ScrollViewer Margin="30">
      <StackPanel Spacing="20">
        <TextBlock Classes="trl-h1" Text="CONFIGURAÇÕES"/>
        <cc:TrlPanel Title="LIMPEZA E DADOS"> … 3 Button base … </cc:TrlPanel>
        <cc:TrlPanel Title="OPÇÕES BÁSICAS"> … combo/selector/toggle/devrow … </cc:TrlPanel>
        <cc:TrlPanel Title="FERRAMENTAS DEV" IsVisible="{Binding …IsDevMode}"> … </cc:TrlPanel>
      </StackPanel>
    </ScrollViewer>
  </Grid>
</Grid>
```

Notas de fidelidade:
- `cc:TrlSidebarNav` já entrega 280px (`TrlSidebarWidth`, `TokenS.axaml:108`), fundo `TrlBgPanelBrush`, hairline direito (`TrlCustomControls.axaml:93-121`). O logo mantém a mesma margem da `ProfileView.axaml:55` (`Margin="24,44,24,8"` p/ descer abaixo da top bar de 34px).
- `cc:TrlPanel` renderiza o `Title` no header interno com `TrlFgLabelBrush` + fonte display (`TrlCustomControls.axaml:32-36`) — some o `TextBlock` de título bold branco do corpo.
- Botões de limpeza: `Button` base (outlined neutro do `Button.axaml:11-23`). Manter o `Grid ColumnDefinitions="*,*,*"` e os `Command`/`Content` localizados intactos.
- Dot Dev Mode: ver Frente 3.
- `ToggleSwitch` (`:158-171`) já está em conformidade (usa `trl-label`/`trl-muted`); manter como está, apenas garantir que continua dentro do novo `TrlPanel`.

### Frente 3 — Remover cor crua da VM (dot Dev Mode)

**`SettingsViewModel.cs:50`** hoje:
```csharp
public string DevModeStatusColor => LauncherSettingsProvider.Instance.IsDevMode ? "#4CAF50" : "#555555";
```
- **Remover** a propriedade `DevModeStatusColor` e o `RaisePropertyChanged(nameof(DevModeStatusColor))` de `ToggleDevModeCommand` (`:115`).
- **Manter** `DevModeStatusText` (`:51`) e seu `RaisePropertyChanged` (`:116`) — é texto de tooltip, não cor.
- No XAML, o dot vira um `Border` com *class binding* no `IsDevMode` reativo (o provider usa `SetProperty`, confirma em `LauncherSettingsProvider.cs:312-317`):

```xml
<!-- styles locais da view (ou em Nav.axaml se preferir global) -->
<Style Selector="Border.dev-dot">
  <Setter Property="Width" Value="10"/>
  <Setter Property="Height" Value="10"/>
  <Setter Property="CornerRadius" Value="0"/>
  <Setter Property="Background" Value="{DynamicResource TrlFgFaintBrush}"/>
</Style>
<Style Selector="Border.dev-dot.on">
  <Setter Property="Background" Value="{DynamicResource TrlSuccessBrush}"/>
</Style>
```
```xml
<Border Classes="dev-dot"
        Classes.on="{Binding IsDevMode, Source={x:Static helpers:LauncherSettingsProvider.Instance}}"
        VerticalAlignment="Center"
        ToolTip.Tip="{Binding DevModeStatusText}"/>
```

Alternativa considerada e **rejeitada**: manter a propriedade na VM devolvendo `IBrush` via `Application.Current.FindResource("TrlSuccessBrush")`. Rejeitada porque mantém decisão de cor na VM (viola separação DS) e acopla a VM ao ciclo de recursos do tema. A abordagem por *class binding* mantém a cor 100% no XAML/token.

## Arquivos a tocar

| Arquivo | Mudança | Tipo |
|---|---|---|
| `project/SPT.Launcher/Views/SettingsView.axaml` | reescrita completa (styles locais → DS; sidebar/cards/inputs/dot) | 🔴 principal |
| `project/SPT.Launcher/ViewModels/SettingsViewModel.cs` | remover `DevModeStatusColor` (`:50`) + o `RaisePropertyChanged` correspondente (`:115`) | 🟡 pequeno |
| `project/SPT.Launcher/Assets/Theme/Controls/Nav.axaml` | **novo**: estilos `Button.nav` (copiados da `ProfileView.axaml:17-45`) | 🟢 novo |
| `project/SPT.Launcher/Assets/Theme/Trl.axaml` | + 1 linha `StyleInclude` de `Nav.axaml` (~`:32`) | 🟢 1 linha |

**Não tocar:** `ProfileView.axaml` (frente 1 evita conflito com itens paralelos), `Legacy.axaml`, `Tokens.axaml` (nenhum token novo), `TrlCustomControls.axaml`.

## Contratos / DTOs

Nenhum DTO novo. Contratos preservados:
- `SettingsViewModel` mantém `GoBackCommand`, `OpenKofiCommand`, `ClearGameSettingsCommand`, `CleanTempFilesCommand`, `CopyLogsToClipboard`, `ToggleDevModeCommand`, `ToggleDevPasswordVisibilityCommand`, `OpenGameFolderCommand`, `DevPassword`, `DevPasswordChar`, `DevPasswordEyeIcon`, `DevModeStatusText`, `UsePerformanceConfigs`, `Locales`, `Background`.
- Bindings via `x:Static helpers:LauncherSettingsProvider.Instance` (`Server.Url`, `DisableUpdates`, `IsDevMode`) — alvos inalterados.
- Único contrato **removido**: `DevModeStatusColor` (nenhum outro consumidor — confirmado: só a `SettingsView.axaml:195` usava).

## Riscos

1. **Class binding `Classes.on` no `Border`** — sintaxe válida no Avalonia 11, mas menos comum. Se falhar em runtime, fallback é um `Style` com `DataTrigger`/`ConditionalStyle` no `IsDevMode`, ou converter bool→brush. Mitigação: validar visualmente o toggle (AC6).
2. **Herança de tema nos inputs** — ao remover `Background`/`Foreground` inline do `ComboBox`/`TextBox`, eles passam a depender do `ControlTheme` default (`ComboBox.axaml`, `TextBox.axaml`). Risco baixo (são themes default type-targeted), mas confirmar que o popup do combo e o `PasswordChar` seguem legíveis.
3. **`Label` → `TextBlock.trl-label`** — trocar o control muda a semântica de acessibilidade (`Label` tem `Target`). Nenhum dos labels aqui usa `Target`; troca segura. Alternativa conservadora: manter `<Label>` e só remover o `Foreground` inline (herda `TrlFgBrush` do `Legacy.axaml:67-69`) — mas aí perde o look "caption" neutro do `trl-label`. Decisão: usar `trl-label` (paridade com ProfileView).
4. **Overlay sobre a foto** pode escurecer demais se combinado com fundo já escuro — `TrlPhotoOverlayBrush` é radial 50%→90%, mesmo usado na ProfileView; risco estético baixo.
5. **Regressão funcional silenciosa** — como a tela hospeda ações destrutivas (apaga `sptsettings`, temp), um erro de re-binding poderia desligar um botão sem quebrar o build. Mitigação: gate funcional in-game (01-spec, Gate 3).

## Plano de teste

**Build/estático (máquina):**
- `dotnet build SPT.Launcher.csproj -c Release` verde (XAML compila; `Nav.axaml` resolvido).
- `dotnet test SPT.Launcher.Tests.csproj -c Release` verde (a suíte atual é de `Sync/`; não há teste de VM de Settings hoje).
- **Lint de pureza** (grep no `SettingsView.axaml` final): zero `#`, zero `White|LightGray|Gray|Black`, zero `CornerRadius="[^0]`, zero `Classes="SidebarMenu|PanelCard|CleanupButton"`.

**Unit (onde couber, xUnit em `SPT.Launcher.Tests`):**
- O ganho testável é magro (migração é XAML). Teste candidato: um `SettingsViewModelTests` verificando que `DevModeStatusColor` **não existe mais** (via reflection, garante que a cor saiu da VM) e que `DevModeStatusText` reflete `IsDevMode`. Baixa prioridade; a maior parte da verificação é visual/in-game.

**Manual (humano — ver Gates da 01-spec):** paridade visual com ProfileView, toggle do dot, e as ações in-game (limpeza, temp, logs, perf-config, dev URL).

## Nota de paralelismo

Itens rodando em paralelo neste lote (019-025) e o que este 024 compartilha:

- **`ProfileView.axaml`** — hub visual adjacente. **Este item NÃO o toca** (frente 1 promove `Button.nav` para `Nav.axaml` global em vez de mexer no bloco local da Profile). Evita colisão com qualquer ajuste de binding que 022 (command-ui-robustness) possa fazer na Profile. A remoção do bloco `Button.nav` redundante da `ProfileView.axaml:17-45` é deixada para o **025** (limpeza), não para cá.
- **`SettingsViewModel.cs`** — exclusivo do 024 neste lote (nenhum dos 019-023 mexe em Settings; 006/tailscale mexe em `ConnectServerViewModel`). Baixo risco de conflito.
- **`Trl.axaml`** — inclui o novo `Nav.axaml`; append de 1 linha. 015 (fundação de tema) e 025 podem tocar o mesmo arquivo → conflito trivial de merge (linha isolada).
- **`Legacy.axaml`** — **compartilhado com 025** (que remove os shims `.card/.acc/.alt`). Este 024 **não** toca `Legacy.axaml` e **não** pode reintroduzir classes legadas (`.card/.acc/.alt/.SidebarMenu`) na SettingsView — do contrário reabre o débito que o 025 vai fechar. Contrato: 024 entrega a Settings 100% DS, deixando o `Legacy.axaml` livre para encolher no 025.
- **`OptionalModsHelper` / motor de sync** — 021/023 mexem aí; 024 não encosta. O `UsePerformanceConfigs` só liga/desliga a flag (via `LauncherSettingsProvider`), sem tocar o motor.
- **Dependência declarada:** 024 depende do **015** (fundação de tema — tokens/controls já entregues e verificados na auditoria). 025 depende do 024.

## Gates

Ver [01-spec](./024-settingsview-ds-migration-01-spec.md) (seção "Gates humanos"). Resumo: build+test verdes (máquina) · paridade visual + toggle do dot (humano) · ações in-game preservadas, incl. checagem coop dos campos Dev de URL/updates (humano). Nunca rodar o exe no gate automatizado.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-04 | Guilherme | Criação — spec técnica da migração DS da SettingsView (B3), com file:line reais. |
