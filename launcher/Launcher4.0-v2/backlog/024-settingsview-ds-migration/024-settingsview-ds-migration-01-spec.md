# 024 — Migração DS da SettingsView + unificar chrome · Spec funcional

> **Data:** 2026-07-04<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [00-kickoff](./024-settingsview-ds-migration-00-kickoff.md) · [AUDIT-2026-07-04](../../AUDIT-2026-07-04-code-product-ds.md) (§B3 + §DS)<br>

---

## Objetivo

Migrar a `SettingsView` inteira para o TRL Design System (DS grafite v2), eliminando o **maior furo de pureza** do launcher: ~20 hex crus (`#1A1A1A`, `#222`, `#F2111111`, `#333`, `#111`…) e literais de cor (`White`/`LightGray`/`Gray`) espalhados na view (bloqueador **B3** da auditoria). A tela passa a consumir **exclusivamente** tokens `Trl*` e os controls de chrome do tema (`cc:TrlSidebarNav`, `cc:TrlPanel`), ficando visualmente irmã da `ProfileView`.

É migração **puramente visual/estrutural**: nenhuma regra de negócio, comando, binding de dado ou fluxo de navegação muda de comportamento. O que muda é *como* a tela é pintada, não *o que* ela faz.

## Escopo

- `SettingsView.axaml` — a view inteira (styles locais, sidebar, cards, inputs, dot Dev Mode).
- `SettingsViewModel.cs` — **apenas** a remoção da cor crua exposta como string (`DevModeStatusColor`); a cor migra para XAML dirigido por token.
- Promoção do chrome de navegação `Button.nav` (hoje local da `ProfileView`) para o tema, de modo que `SettingsView` reuse o **mesmo** chrome (critério do kickoff: "mesmo chrome do Profile").

## Critérios de aceite (testáveis)

Cada item é verificável por leitura do XAML final e/ou inspeção visual em runtime.

### AC1 — Pureza de tokens (zero cor crua)
- [ ] **Dado** o `SettingsView.axaml` final, **quando** se busca por hex (`#`) e por literais de cor (`White`, `LightGray`, `Gray`, `Black`) em atributos de `Background`/`Foreground`/`BorderBrush`/`Fill`, **então** o resultado é **zero** ocorrências — toda cor vem de `{DynamicResource Trl*}` ou de classe DS (`trl-*`, `.primary`, `.danger`, etc.).
- [ ] **Dado** o `SettingsViewModel.cs` final, **quando** se busca por strings de cor (`#4CAF50`, `#555555`), **então** o resultado é **zero** — a propriedade `DevModeStatusColor` (hoje `SettingsViewModel.cs:50`) não existe mais retornando hex.

### AC2 — Sidebar unificada
- [ ] **Dado** que a `ProfileView` usa `cc:TrlSidebarNav` (280px) com botões `Button.nav`/`Button.nav.active`, **quando** a `SettingsView` é aberta, **então** ela usa o **mesmo** `cc:TrlSidebarNav` e os **mesmos** botões `Button.nav`, não mais o `Border #111111` de 250px com `Button.SidebarMenu`.
- [ ] **Dado** a sidebar migrada, **quando** se compara largura, cor de fundo, hairline direito e estados hover/active com a `ProfileView`, **então** são idênticos (mesmo token `TrlSidebarWidth`, mesmo `TrlBgPanelBrush`, mesmo `TrlAccentBrush` no item ativo).
- [ ] **Dado** o item "CONFIGURAÇÕES" na sidebar de Settings, **quando** a tela está aberta, **então** ele aparece com a classe `active` (marca tan à esquerda), e "LAUNCHER" continua acionando `GoBackCommand`, "APOIE UM CAFEZINHO" `OpenKofiCommand`, "LISTA DE MODS" desabilitado — **sem mudança de comando**.

### AC3 — Cards via `cc:TrlPanel`
- [ ] **Dado** os três blocos ("LIMPEZA E DADOS", "OPÇÕES BÁSICAS", "FERRAMENTAS DEV"), **quando** renderizados, **então** cada um é um `cc:TrlPanel` com `Title` no header do control (não mais `Border.PanelCard` + `TextBlock` bold branco de título no corpo).
- [ ] **Dado** o painel "FERRAMENTAS DEV", **quando** `IsDevMode` é `false`, **então** ele permanece oculto (`IsVisible` ligado a `LauncherSettingsProvider.Instance.IsDevMode`), preservando o comportamento atual.

### AC4 — Inputs herdam o tema
- [ ] **Dado** o `ComboBox` de idioma e os `TextBox` (senha admin, URL do servidor), **quando** renderizados, **então** não carregam mais `Background="#222"`/`Foreground="White"`/`BorderThickness="0"` inline — herdam o tema DS (`TrlBgInputBrush`, borda `TrlEdgeStrongBrush`).
- [ ] **Dado** o `CheckBox` "Bloquear atualizações", **quando** renderizado, **então** não usa `Foreground="LightGray"` — herda o foreground do tema.
- [ ] **Dado** os captions de campo ("IDIOMA PADRÃO", "AO INICIAR O JOGO", "URL DO SERVIDOR"), **quando** renderizados, **então** usam o estilo de label do DS (`TextBlock.trl-label`, neutro `TrlFgLabelBrush`), não `Foreground="LightGray"`.

### AC5 — Botões via classes DS
- [ ] **Dado** os 3 botões de limpeza, o botão "🔑 Dev Mode", o toggle de olho da senha e o botão "Abrir pasta", **quando** renderizados, **então** usam classes DS de `Button` (base outlined / `.icon` / `.ghost`) — nenhum define `Background="#333"`/`#444` nem `Foreground="White"` inline; as classes locais `Button.CleanupButton` deixam de existir.

### AC6 — Dot Dev Mode via token + radius 0
- [ ] **Dado** o indicador de status do Dev Mode, **quando** `IsDevMode == true`, **então** ele é preenchido com `TrlSuccessBrush`; **quando** `false`, com `TrlFgFaintBrush`; em ambos os casos com `CornerRadius="0"` (não mais `CornerRadius="10"`).
- [ ] **Dado** que o usuário aciona "🔑 Dev Mode" e alterna o estado, **quando** o toggle ocorre, **então** o dot e seu tooltip (`DevModeStatusText`) atualizam **reativamente** na hora (sem reabrir a tela).

### AC7 — Título e legibilidade sobre a foto
- [ ] **Dado** o título "CONFIGURAÇÕES" do corpo, **quando** renderizado, **então** usa uma classe de heading do DS (`trl-h1`/`trl-h2`), não `Foreground="White" FontWeight="Black"` cru.
- [ ] **Dado** a imagem de fundo (`Binding Background.Path`), **quando** a tela é exibida, **então** existe uma camada `TrlPhotoOverlayBrush` entre a foto e o conteúdo (paridade com a `ProfileView`), garantindo legibilidade sobre fundos claros.

### AC8 — Radius 0 e sem regressão de tema (R2)
- [ ] **Dado** todo o XAML final, **quando** se busca `CornerRadius` ≠ 0, **então** não há nenhum (respeita a regra R2 de cantos agudos do DS).
- [ ] **Dado** o build, **quando** `dotnet build SPT.Launcher.csproj -c Release` roda, **então** compila verde e a tela abre sem `XamlLoadException`.

## Regras de negócio (preservadas — não podem mudar)

| Regra | Fonte atual | Invariante |
|---|---|---|
| Dev Mode ativa por senha, desativa livre | `SettingsViewModel.cs:87-117` | comando e senha inalterados |
| Painel "FERRAMENTAS DEV" só visível em Dev Mode | `SettingsView.axaml:204` | binding `IsDevMode` mantido |
| `UsePerformanceConfigs` persiste na hora, aplica na próxima verificação | `SettingsViewModel.cs:57-69` | toggle e texto explicativo mantidos |
| URL do servidor / DisableUpdates editam `LauncherSettingsProvider.Instance` | `SettingsView.axaml:211-217` | alvos de binding via `x:Static` inalterados |
| `GoBackCommand` recria `ProfileViewModel` se há opcionais pendentes | `SettingsViewModel.cs:223-232` | navegação inalterada |

## Corner cases

- **Toggle de Dev Mode com a tela aberta:** o dot deve refletir o novo estado imediatamente. `IsDevMode` no provider usa `SetProperty` (`INotifyPropertyChanged`), então o binding de classe reage; o `DevModeStatusText` continua sendo levantado pela VM no toggle.
- **Fundo escuro vs. claro:** a foto de fundo é escolhida pelo usuário (`Background.Path`); o overlay `TrlPhotoOverlayBrush` cobre o caso de arte clara que derrubaria o contraste do texto/inputs abaixo de WCAG AA.
- **Sem regressão dos consumidores de SPT:** os comandos que **escrevem/apagam** em arquivos SPT (`ClearGameSettingsCommand` apaga `SPT/user/sptsettings`; `CleanTempFilesCommand`; `CopyLogsToClipboard`; `UsePerformanceConfigs` que agenda overlay de sync) continuam ligados aos mesmos comandos — a migração é só de aparência, mas os botões precisam continuar disparando exatamente as mesmas ações (ver Gates).
- **Coop (Fika PVE):** o campo "URL do Servidor (Local/Custom)" e "Bloquear atualizações" do painel Dev seguem editando `LauncherSettingsProvider.Instance.Server.Url`/`DisableUpdates`. A migração **não pode** trocar o alvo desses bindings — apontar clientes coop para uma URL/host errado dessincroniza a sessão. Nenhum comportamento novo é introduzido; apenas confirmar que o binding permanece idêntico.

## Fora de escopo

- Corrigir a **senha de Dev Mode hardcoded** (`SettingsViewModel.cs:30`, `"Redline123"`) — é achado de segurança, não de DS (fica para item de segurança/025).
- Reescrever a lógica de `ClearGameSettings`/`CleanTempFiles`/`CopyLogsToClipboard` — comportamento intacto.
- Migrar o custom control `cc:LocalizedLauncherActionSelector` (`SettingsView.axaml:153`) — é control próprio; só entra se ele mesmo carregar cor crua (verificar; se puro, não tocar).
- Deletar os 5 custom controls órfãos e fechar shims do `Legacy.axaml` — isso é o **item 025** (depende deste 024).
- Refatorar `ImageSourceConverter` (dispose/off-thread) — correlato do 025.

## Gates humanos (obrigatórios)

A migração é visual, mas a `SettingsView` **hospeda** ações que escrevem/apagam arquivos SPT. Regra do projeto: *escrita em arquivos SPT exige validação no jogo, não só build*. Portanto, além dos gates de build:

1. **Gate de build (máquina):** `dotnet build SPT.Launcher.csproj -c Release` + `dotnet test SPT.Launcher.Tests.csproj -c Release` verdes. Nunca rodar o exe no gate automatizado.
2. **Gate visual (humano):** abrir a tela e confirmar paridade com a `ProfileView` — sidebar 280px idêntica, cards `TrlPanel`, inputs com look grafite, dot Dev Mode quadrado (radius 0) verde/cinza conforme estado. Comparar lado a lado com Profile.
3. **Gate funcional in-game (humano):** com o SPT instalado, validar que **cada** ação ainda funciona após a migração:
   - "Limpar configurações do jogo" realmente apaga/recria `SPT/user/sptsettings`;
   - "Limpar temporários" e "Copiar logs" funcionam;
   - toggle "USAR CONFIGS PERFORMANCE" persiste e é aplicado na próxima verificação de arquivos;
   - Dev Mode ativa com senha e revela o painel; edição de "URL do Servidor" e "Bloquear atualizações" persiste.
4. **Gate coop (humano):** confirmar que "URL do Servidor (Local/Custom)" e "Bloquear atualizações" continuam apontando para `LauncherSettingsProvider.Instance` (não um alvo novo) — para não arriscar apontar clientes coop ao host errado.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-04 | Guilherme | Criação — spec funcional da migração DS da SettingsView (B3). |
