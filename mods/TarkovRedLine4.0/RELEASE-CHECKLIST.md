# TRL — Checklist de release (o que deployar a cada versão)

> **Data:** 2026-07-09<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Objetivo:** a cada nova build, saber **quais dos componentes mudaram** para deployar só o necessário.<br>

---

## Os componentes (o que existe, onde vive, como deploya)

| # | Componente | Fonte (repo) | Versão em | Build | Onde deploya |
|---|---|---|---|---|---|
| 1 | **Launcher (exe)** | `launcher/Launcher4.0-v2/project/SPT.Launcher/` | `SPT.Launcher.csproj → <Version>` | `dotnet publish …` single-file | **Server:** `Launcher-Updater/TRL.Launcher.exe` (+ `.sig`) → clientes pegam via **auto-update** |
| 2 | **Mod server (C#)** | `mods/TarkovRedLine4.0/Server/TarkovRedLine.Server/` | `Plugin.cs → Version` (metadata) | `dotnet build -c Release` | **Server:** `SPT\user\mods\TarkovRedLine.Server\` (DLL+pdb) + **reiniciar** o SPT.Server |
| 3 | **Plugin client `RedLineRestart`** (vote UI) | `mods/TarkovRedLine4.0/Client/RedLineRestart/` | `RedLinePlugin.cs → [BepInPlugin(…, "x.y.z")]` | `dotnet build`/`/compile-mod` | **Server:** `Launcher-Updater/mods_repo/BepInEx/plugins/…` → clientes pegam via **sync do launcher** |

> Existe também o **`RedLineShutdown`** (headless, `Client/RedLineShutdown/`, v1.0.0) — só na máquina headless, raramente muda.

**Chave:** o **exe** chega ao cliente pelo **auto-update** (basta o server ter o `TRL.Launcher.exe` assinado); o **plugin** chega pelo **sync** (basta estar no `mods_repo/`); o **mod server** é o único que é **deploy manual + restart** no server.

## Como saber o que mudou desde o último deploy

`git log` filtrado pelo path de cada componente (troque `<último-deploy>` pelo commit/tag do último release publicado):

```bash
# 1. exe (launcher) mudou?
git log --oneline <último-deploy>..HEAD -- launcher/Launcher4.0-v2/project/
# 2. mod server mudou?
git log --oneline <último-deploy>..HEAD -- mods/TarkovRedLine4.0/Server/TarkovRedLine.Server/
# 3. plugin client mudou?
git log --oneline <último-deploy>..HEAD -- mods/TarkovRedLine4.0/Client/RedLineRestart/
```

Cada um com commits = precisa redeployar aquele componente. Vazio = não precisa.

## Regra de bump de versão

Toda build oficial nova **incrementa** a versão do componente que mudou (memória `feedback_version_increment_on_release`): patch = bugfix, minor = feature nova, major = quebra. Sufixar o zip de distribuição com `-vX.Y.Z`. Para o **auto-update do exe** disparar, o `TRL.Launcher.exe` no server tem que ter `ProductVersion` **maior** que o do cliente **e** estar assinado (`.sig`, item 018).

---

## Histórico de releases

| Data | Launcher (exe) | Mod server | Plugin RedLineRestart | O que deployar |
|---|---|---|---|---|
| 2026-08-02 | sem mudança | **4.1.0** — item 001: **cache persistente do manifesto** (persiste manifesto+hash em disco com impressão do `mods_repo`, carrega no boot → acaba com a espera "preparing the list" de 30s a cada restart) | sem mudança | **só o mod server (2)** — copiar a DLL em `SPT\user\mods\TarkovRedLine.Server\` + reiniciar |
| 2026-07-18 | **2.5.0** — painel de detalhe da classe em **2 zonas**: arte full-body da classe à esquerda (fundo removido via u2net, `Assets/ClassImages/`, resolvida por nome) + info à direita (descrição + multiplicadores + perks/drawbacks). Só launcher. | 4.0.0 — sem mudança | 2.4.3 — sem mudança | **só o exe (1)** |
| 2026-07-18 | **2.4.0** — seletor de classe mostra **multiplicadores de XP** (dado já servido em `/customclasses/classes`) + **cards de perks/drawbacks** em 2 colunas (contrato novo `ClassInfo.effects`, empty state + preview Dev Mode). Handoff do contrato de servidor em `launcher/…/backlog/029-…` para a sessão do CustomClasses | 4.0.0 — sem mudança (perks dependem de rota nova a implementar lá) | 2.4.3 — sem mudança | **só o exe (1)** |
| 2026-07-17 | **2.3.0** — `config-server` vira **só referência** (`mirror-reference`, não semeia mais em `config/`); guard do `deleteFiles` protege pastas `-disabled/`; UX Configurações (descrição do Dev Mode, rename "Bloquear atualização de launcher" + reset do `DisableUpdates` ao desligar o Dev Mode = item 028); relatório `last-update.json` v2 (counts com `forced`/`configsBackedUp`, ordenado por ação, label `reference-updated`, `descricao` PT) | 4.0.0 — sem mudança (só higiene do default no `ModUpdater.cs`, **não redeployar**) | 2.4.3 — sem mudança | **só o exe (1)** + check pré-deploy da migração `config-server`→`config` |
| 2026-07-09 | **2.2.1** ✅ — force **não-destrutivo**: a config do jogador é preservada em `config-disabled/` (versionada) antes de ser trocada. Corrige o 🔴 TOCTOU do `/code-review` (2.2.0 destruía config sem backup). **⚠️ NÃO distribuir a 2.2.0** | 4.0.0 — sem mudança | 2.4.3 — sem mudança | **só o exe (1)** |
| 2026-07-09 | ~~2.2.0~~ ❌ **não distribuir** — **canal `config-force`** (config que vai pra TODO MUNDO), mas o force **destruía** a config do jogador sem backup | 4.0.0 — mudou só o **default do `config.json`** (só escrito em server SEM config.json) → não precisa redeployar | 2.4.3 | — |
| 2026-07-09 | **2.1.0** ✅ (era 2.0.0) — msg de sucesso, remove velocidade, toggle homolog, **config-server seed-and-mirror**, exe→`TRL.Launcher.exe` | **4.0.0** ✅ (paridade B1/B2/vote/versão, homolog namespaced, default `config-server`=seed-and-mirror, filename `TRL.Launcher.exe`) — deployado em prod 2026-07-09 ✅ | 2.4.3 — sem mudança | exe + mod server |

> ⚠️ **Atenção:** o mod server C# acumulou várias mudanças no repo que podem nunca ter ido pra máquina de produção. Rotas como `/launcher/mods/version` (B2), `/redline/register-player-ip` (B1), cofre de senha (020), `.sig` do self-update (018) e a versão do server só existem no **mod C#**. Se prod roda o mod **TS** antigo (ou um C# desatualizado), essas funções degradam/falham no launcher. Ver [Server/MIGRATION-parity-TS-vs-CSharp.md](Server/MIGRATION-parity-TS-vs-CSharp.md).

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-09 | Guilherme | Criação — checklist dos 3 componentes + como detectar mudança + histórico de releases. |
