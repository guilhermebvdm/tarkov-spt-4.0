# Documentação Técnica — Tarkov Red Line Launcher (v2.x)

Este diretório contém a documentação modular, técnica e arquitetural do **Tarkov Red Line Launcher**, cliente oficial de inicialização e sincronização do servidor Tarkov Red Line (SPT 4.0).

---

## 📚 Índice de Artigos Temáticos

| Artigo | Descrição | Status |
|---|---|---|
| [01 — Visão Geral e Arquitetura](./01-visao-geral-e-arquitetura.md) | Estrutura de projetos .NET 9, padrão MVVM/ReactiveUI, ciclo de vida e confinamento de caminhos | 🟢 Vivo |
| [02 — Autenticação e Seleção de Classes](./02-sistema-de-autenticacao-e-classes.md) | Fluxo de registro, login, cofre case-insensitive e seletor visual de classes customizadas (SP0) | 🟢 Vivo |
| [03 — Download do Jogo Base (MonoTorrent)](./03-sistema-de-download-do-jogo-base.md) | Motor de download BitTorrent/WebSeeds HTTP, integridade SHA-1, cache e retomada rápida | 🟢 Vivo |
| [04 — Sincronização de Mods e Configurações](./04-sincronizacao-de-mods-e-configs.md) | Algoritmo de diff por hash MD5/SHA-256, download paralelo e gestão de pastas gerenciadas | 🟢 Vivo |
| [05 — Mods e Configurações Opcionais](./05-mods-opcionais-e-gerenciamento.md) | Catálogo de opcionais, agrupamento por categorias e persistência local do jogador | 🟢 Vivo |
| [06 — Auto-Atualização e Segurança](./06-auto-atualizacao-e-seguranca.md) | Atualização no boot, verificação de assinatura digital RSA-2048 SHA-256 e HWID | 🟢 Vivo |
| [07 — TRL Design System e Custom Controls](./07-design-system-trl-e-custom-controls.md) | Tokens semânticos, controles XAML dedicados, tipografia Bender e carrossel de fundos | 🟢 Vivo |
| [08 — AutoSync (Servidor) e Verificar Arquivos](./08-fluxo-autosync-e-verificar-arquivos.md) | Interação entre pipeline de empacotamento 3D no servidor e auditoria de arquivos no cliente | 🟢 Vivo |
| [Relatório de Auditoria Técnica de Código (Review 01)](./relatorio-auditoria-codigo-01.md) | Auditoria estática profunda: 6 dimensões, caça a memory leaks, WMI e concorrência | 🟢 Vivo |

---

## 🗂️ Mapeamento de Código-Fonte e Subsistemas

### 1. Camada de Apresentação (`project/SPT.Launcher/`)
- **Telas Principais (Views):**
  - [MainWindow.axaml](../project/SPT.Launcher/Views/MainWindow.axaml) — Janela base com barra de título customizada.
  - [ConnectServerView.axaml](../project/SPT.Launcher/Views/ConnectServerView.axaml) — Conexão e handshake.
  - [LoginView.axaml](../project/SPT.Launcher/Views/LoginView.axaml) — Tela de login.
  - [RegisterView.axaml](../project/SPT.Launcher/Views/RegisterView.axaml) — Tela inicial de cadastro.
  - [ClassSelectionView.axaml](../project/SPT.Launcher/Views/ClassSelectionView.axaml) — Seletor de classes com detalhes e multiplicadores.
  - [ProfileView.axaml](../project/SPT.Launcher/Views/ProfileView.axaml) — Dashboard central e acionador do jogo.
  - [ModsConfigsView.axaml](../project/SPT.Launcher/Views/ModsConfigsView.axaml) — Gerenciador de mods opcionais.
  - [SettingsView.axaml](../project/SPT.Launcher/Views/SettingsView.axaml) — Configurações do Launcher.
- **Controles TRL (Custom Controls):**
  - [TrlPanel.cs](../project/SPT.Launcher/CustomControls/TrlPanel.cs) · [TrlScreenBar.cs](../project/SPT.Launcher/CustomControls/TrlScreenBar.cs) · [TrlLaserDivider.cs](../project/SPT.Launcher/CustomControls/TrlLaserDivider.cs) · [TrlSidebarNav.cs](../project/SPT.Launcher/CustomControls/TrlSidebarNav.cs) · [TrlTag.cs](../project/SPT.Launcher/CustomControls/TrlTag.cs) · [TrlVersionFooter.cs](../project/SPT.Launcher/CustomControls/TrlVersionFooter.cs)
- **Tema e Tokens:**
  - [Tokens.axaml](../project/SPT.Launcher/Assets/Theme/Tokens.axaml) · [Trl.axaml](../project/SPT.Launcher/Assets/Theme/Trl.axaml) · [Typography.axaml](../project/SPT.Launcher/Assets/Theme/Typography.axaml)

### 2. Camada de Negócio e Serviços (`project/SPT.Launcher.Base/`)
- **Contas e Autenticação:**
  - [AccountManager.cs](../project/SPT.Launcher.Base/Controllers/AccountManager.cs) · [VaultKeyMatcher.cs](../project/SPT.Launcher.Base/Helpers/VaultKeyMatcher.cs) · [HwidHelper.cs](../project/SPT.Launcher.Base/Helpers/HwidHelper.cs)
- **Rede e Requisições:**
  - [RequestHandler.cs](../project/SPT.Launcher.Base/Controllers/RequestHandler.cs) · [ServerManager.cs](../project/SPT.Launcher.Base/Controllers/ServerManager.cs) · [Request.cs](../project/SPT.Launcher.Base/MiniCommon/Request.cs)
- **Download do Jogo Base:**
  - [BaseGameTorrentDownloader.cs](../project/SPT.Launcher.Base/Download/BaseGameTorrentDownloader.cs) · [GameStateDetector.cs](../project/SPT.Launcher.Base/Download/GameStateDetector.cs)
- **Sincronização de Arquivos:**
  - [SyncEngine.cs](../project/SPT.Launcher.Base/Sync/SyncEngine.cs) · [DownloadRateMeter.cs](../project/SPT.Launcher.Base/Sync/DownloadRateMeter.cs) · [SptPathHelper.cs](../project/SPT.Launcher.Base/Helpers/SptPathHelper.cs)
