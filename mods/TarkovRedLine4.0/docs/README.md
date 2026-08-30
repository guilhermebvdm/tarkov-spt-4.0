# Documentação Técnica — Tarkov Red Line 4.0 (Server & Client)

Este diretório contém a documentação técnica, modular e arquitetural do ecossistema **Tarkov Red Line 4.0** (servidor C# ASP.NET Core, pipeline de cache 3D e plugins BepInEx).

---

## 📚 Índice de Artigos Temáticos

| Artigo | Descrição | Status |
|---|---|---|
| [01 — Visão Geral e Arquitetura](./01-visao-geral-e-arquitetura.md) | Arquitetura geral, integração de serviços, controllers REST e fluxos entre Launcher e Servidor | 🟢 Vivo |
| [02 — Distribuição e Sincronização de Mods](./02-distribuicao-e-sincronizacao-de-mods.md) | Geração de manifesto em memória, hashing MD5/SHA-256 e entrega de mods via ModUpdater | 🟢 Vivo |
| [03 — Download do Jogo Base e WebSeeds HTTP](./03-download-do-jogo-base-e-torrent.md) | Distribuição do cliente de 56 GB via BitTorrent/MonoTorrent e HTTP Range Requests (206) | 🟢 Vivo |
| [04 — Autenticação, Cofre de Senhas e Segurança](./04-autenticacao-cofre-e-seguranca.md) | Cofre case-insensitive `redline_passwords.json`, gestão de HWID e distribuição com assinatura RSA | 🟢 Vivo |
| [05 — Pipeline AutoSync e Aquecimento de Cache 3D](./05-pipeline-autosync-e-aquecimento-3d.md) | Compilação headless de bundles 3D, detecção por cobertura de cache e espelho em `mods_repo` | 🟢 Vivo |
| [06 — Rede Tailscale, QoS e Suporte ao FIKA Coop](./06-rede-tailscale-qos-e-coop.md) | Descoberta de IPs da malha Tailscale, controle dinâmico de largura de banda e compatibilidade FIKA | 🟢 Vivo |
| [07 — Plugins Client BepInEx e Controle de Sessão](./07-plugins-cliente-e-controle-de-sessao.md) | Plugins de cliente `RedLineRestart` e `RedLineShutdown` para ciclo de vida e encerramento | 🟢 Vivo |
| [Relatório de Auditoria Técnica de Código (Review 01)](./relatorio-auditoria-codigo-01.md) | Auditoria estática profunda: 6 dimensões, SaveServer, concorrência, HWID e resiliência | 🟢 Vivo |

---

## 🗂️ Mapeamento de Código-Fonte e Estrutura de Pastas

### 1. Servidor C# (`Server/TarkovRedLine.Server/`)
- **Controllers:**
  - [ModUpdater.cs](../Server/TarkovRedLine.Server/Controllers/ModUpdater.cs) — Manifesto e download de mods.
  - [BaseGameDownloadController.cs](../Server/TarkovRedLine.Server/Controllers/BaseGameDownloadController.cs) — Streaming do jogo base com Range requests.
  - [PasswordController.cs](../Server/TarkovRedLine.Server/Controllers/PasswordController.cs) — Cofre de senhas de usuários.
  - [HwidManager.cs](../Server/TarkovRedLine.Server/Controllers/HwidManager.cs) — Registro e validação de HWID.
  - [ServerBandwidthController.cs](../Server/TarkovRedLine.Server/Controllers/ServerBandwidthController.cs) — Throttling dinâmico de download durante raids.
  - [PlayerIpsManager.cs](../Server/TarkovRedLine.Server/Controllers/PlayerIpsManager.cs) — Descoberta de IPs Tailscale.
  - [CarouselController.cs](../Server/TarkovRedLine.Server/Controllers/CarouselController.cs) — Imagens do carrossel do launcher.
  - [LauncherUpdater.cs](../Server/TarkovRedLine.Server/Controllers/LauncherUpdater.cs) — Auto-update do launcher com `.sig`.
- **Patches:**
  - [FikaProfilePatch.cs](../Server/TarkovRedLine.Server/Patches/FikaProfilePatch.cs) — Interoperabilidade de perfis customizados com FIKA Coop.

### 2. Automação e Ferramentas (`Server/` e Raiz)
- [AutoSync-Cache.ps1](../AutoSync-Cache.ps1) — Script de aquecimento headless de modelos 3D.
- [generate-base-torrent.js](../Server/scripts/generate-base-torrent.js) — Gerador de torrent para distribuição do cliente base.

### 3. Plugins de Cliente (`Client/`)
- [Client/RedLineRestart/](../Client/RedLineRestart/) — Plugin BepInEx de reinício limpo do cliente.
- [Client/RedLineShutdown/](../Client/RedLineShutdown/) — Plugin BepInEx de salvamento e desligamento seguro.
