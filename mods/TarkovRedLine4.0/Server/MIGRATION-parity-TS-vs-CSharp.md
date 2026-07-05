# Migração TS → C# — Auditoria de paridade (Fase 1)

> **Data:** 2026-07-05<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Escopo:** trocar o mod de server que o **launcher TRL** consome, de `TarkovRedLine-ServerMod` (TS, produção remota `100.106.152.7`) para `TarkovRedLine.Server` (C#, homolog dev `C:\Escape From Tarkov\SPT-4.0\SPT\`).<br>

---

## TL;DR — a migração é viável e é um superset, com 2 bloqueadores pequenos

- **Topologia de porta: já alinhada.** O launcher do repo (dev, v2.0.0) manda **100% das rotas** para um único `request.RemoteEndPoint` (a porta raiz do SPT). O C# serve **tudo** na porta raiz (mesmo Kestrel do `SPTarkov.Server.Web`). Os comentários "porta 7075" no `RequestHandler.cs` são resquício do TS antigo — o código **não** usa 7075. → Nenhuma mudança de porta no launcher.
- **O C# é um superset funcional do TS** (adiciona cofre de senha, `.sig`, overlay de performance, `folderRules`, `server-version.txt`). O launcher novo já espera esses campos e faz optional-chaining onde eles podem faltar.
- **2 bloqueadores** de path que o launcher chama e o C# ainda não serve igual (ver §1). Fora isso, é deploy + config.

---

## 1. 🔴 Bloqueadores (o launcher chama, o C# não serve / serve diferente)

| # | Launcher chama | C# hoje | Correção no C# |
|---|---|---|---|
| B1 | `POST /redline/register-player-ip` (`RequestHandler.cs:163`) | `POST /redline/register-ip` (`PlayerIpsManager.cs`) | **Renomear** a rota do C# para `register-player-ip` (ou aceitar as duas). Path exato importa. |
| B2 | `GET /launcher/mods/version` (`RequestHandler.cs:171`, `GetModVersion`) | não existe (C# tem `/redline/server/version` e o manifesto) | **Adicionar** `GET /launcher/mods/version → {"version": <serverVersion>}` no `ModUpdaterController`. O launcher usa `/redline/server/version` (013, footer) **e** `/launcher/mods/version` (versão dos mods) — são consumidos em pontos distintos. |

Ambos são de 1 linha de rota. Sem eles, o registro de IP do jogador e o display de versão de mods falham silenciosamente (o launcher trata erro e segue, mas a função perde efeito).

## 2. 🟢 C# adiciona (aditivo — launcher já preparado, TS não tinha)

| Recurso | C# | TS (prod) | Observação |
|---|---|---|---|
| Cofre de senha (`/redline/password/change`·`/delete`·`/redline/profile/get`) | ✅ (020) | ❌ **ausente** | Em produção, trocar senha pelo launcher **não persiste** (SPT 4.0 apaga `info.password`). A migração **conserta** isso. Gate: inspecionar `redline_passwords.json` de prod antes (colisão de casing). |
| Assinatura `.sig` no self-update (`/redline/launcher/signature`, `signature`+`algorithm` no `/version`) | ✅ (018) | ❌ sem assinatura | Launcher é **fail-closed**: se `.sig` faltar, aborta o auto-update. → No dev é preciso **assinar o exe** (gate 018) ou o self-update não roda. |
| Overlay de performance (`/launcher/mods/performance-download`, `performanceOverlay` no manifesto) | ✅ (008) | ❌ | Launcher optional-chain (`manifest["performanceOverlay"]?…`) → ausência é segura. |
| `folderRules` no manifesto (motor de sync 007) | ✅ (007) | ❌ | Idem — ausência cai na tabela built-in do launcher. |
| `server-version.txt` + `/redline/server/version` (013) | ✅ | ❌ (TS lê versão do `config.json`) | Deploy precisa criar o arquivo. |
| `optionals-list` shape rico `{id,name,description{pt,en}}` | ✅ (009) | shape antigo `["PastaA",…]` | Launcher **tolera os dois** (`OptionalModsHelper.cs:90-91`). Não-bloqueador. |
| Guardas anti-traversal (`TryResolveUnder`) + escrita atômica do cofre | ✅ | parcial | Endurecimento. |

## 3. 🟡 Paridade do mod inteiro (não é do launcher, mas migra junto)

O mod TS também serve o **cliente in-game** (plugin BepInEx) e um serviço de presença. Se a migração troca o mod inteiro, isto precisa continuar funcionando:

| Recurso | Situação | Ação |
|---|---|---|
| **Vote/restart** (`/redline/vote/*`, `/redline/headless/ack-restart`) | C# tem **paridade completa** de rotas. **PORÉM:** `vote/status` retorna `timeLeft`/`cooldownLeft` em **milissegundos** no C# e em **segundos** no TS. | ⚠️ **Verificar contra o plugin in-game** — divergência de 1000×. Alinhar a unidade ao que o cliente espera antes de trocar em prod. C# também adiciona `noVotes` (aditivo, ok). |
| **PlayerStatus → Supabase** (monkey-patch de `HttpListener.getResponse` no TS) | C# **não tem**. Empurra presença in-game (No inventário / Em Raid / etc.) pro Supabase. | **Decisão:** portar ou descartar. Não é rota do launcher. Se a comunidade usa o status, portar; senão, descartar. |
| **WireGuard VPN** (`POST /api/vpn/register`, reescreve `.conf`, roda `wg.exe`) | C# não tem. | **Descartar** — o launcher hoje usa **Tailscale** (`TailscaleHelper`), não este endpoint. Superseded. |
| `/redline/debug/state` (dump diagnóstico TS) | C# não tem. | Opcional — portar só se útil no diagnóstico. |
| Patches de runtime TS: **Fika `getCompleteProfile` fix** + **flash-reload IA fix** (zera `BotReload.min/max`) | C# **não tem** (`FikaProfilePatch.Enable()` é stub vazio) | ⚠️ **Verificar se ainda são necessários** no server 4.0. O Fika fix evitava crash do `sanitizeProfileForClient`; se o core 4.0 já resolve, dispensável. O flash-reload é ajuste de gameplay. |

## 4. Estrutural / deploy (arrumar no homolog antes de ligar o C#)

- **Layout de arquivos difere.** TS: `mods_repo/` e `Opcionais/` na raiz do cwd do server. C#: **tudo sob `Launcher-Updater/`** (descoberto subindo ≤4 níveis a partir do `BaseDirectory`). Montar no homolog:
  ```
  Launcher-Updater/
    mods_repo/            # mods obrigatórios (= modsRepoPath do TS)
    Opcionais/<grupo>/    # + description.json por grupo
    config-performance/   # overlay 008
    config.json           # managedPaths, deleteFiles, ignoredFiles, optionalGroups, folderRules
    server-version.txt    # versão TRL (senão fallback "0.10.0-beta")
    Tarkov Red Line.exe   # binário servido no self-update
    Tarkov Red Line.exe.sig
  ```
- **Hash = MD5 nos dois** (arquivos e hash do manifesto). → **Compatível**, não força re-download geral. (SHA-256 do item 026 é futuro; **não** aplicar junto da migração — mudaria todos os hashes e baixaria tudo de novo.)
- **Fontes de versão divergem no C#:** o manifesto crava `serverVersion`/`launcherVersion = "1.4.1"` **hardcoded** (`ModUpdater.cs:493-494`), mas o `/redline/server/version` lê `server-version.txt` e o self-update lê `ProductVersion` do exe. Unificar: manifesto deve ler das mesmas fontes.
- **`config.json` muda de forma:** TS carrega `serverVersion`/`launcherVersion`/`modsRepoPath`/`opcionaisPath` no config; C# tirou versão do config (foi pra `server-version.txt` + exe) e usa paths fixos sob `Launcher-Updater/`. Migrar os valores atuais de prod (`managedPaths`, `deleteFiles` — 14 entradas, `optionalGroups` — gore/grass/hollywood, `ignoredFiles`).

## 5. Tabela-mestre de rotas

| Rota | TS (prod) | C# (alvo) | Consumidor | Nota |
|---|---|---|---|---|
| `GET /launcher/mods/manifest-hash` | ✅ 7075+static | ✅ | launcher | MD5 |
| `GET /launcher/mods/manifest` | ✅ | ✅ | launcher | shapes ~iguais; C# adiciona `folderRules`/`performanceOverlay`/`launcherVersion` |
| `GET /launcher/mods/download` | ✅ | ✅ | launcher | resolve por fileMap + fallback modsRepo |
| `GET /launcher/mods/performance-download` | ❌ | ✅ (008) | launcher | aditivo |
| `GET /launcher/mods/optionals-list` | ✅ (shape antigo) | ✅ (shape rico) | launcher | launcher tolera os 2 |
| `GET /launcher/mods/optionals-manifest` | ✅ | ✅ | launcher | |
| `GET /launcher/mods/optional-download` | ✅ | ✅ | launcher | |
| `GET /launcher/mods/refresh` | ✅ | ✅ | launcher/op | |
| `GET /launcher/mods/version` | ✅ | ❌ **B2** | launcher | **portar** |
| `GET /redline/launcher/version` | ✅ | ✅ (+signature/algorithm) | launcher | |
| `GET /redline/launcher/download` | ✅ | ✅ | launcher | |
| `GET /redline/launcher/signature` | ❌ | ✅ (018) | launcher | aditivo |
| `POST /redline/password/change` | ❌ | ✅ (020) | launcher | aditivo (conserta prod) |
| `POST /redline/password/delete` | ❌ | ✅ (020) | launcher | aditivo |
| `POST /redline/profile/get` | ❌ | ✅ | launcher | aditivo |
| `GET /redline/server/version` | ❌ | ✅ (013) | launcher | aditivo |
| `POST /launcher/hwid/register` | ✅ | ✅ | launcher | |
| `POST /launcher/hwid/reset-password` | ✅ | ✅ | launcher | |
| `GET /launcher/hwid/version` | ✅ | ✅ | launcher | |
| `POST /redline/register-player-ip` | ✅ | ❌ (é `register-ip`) **B1** | launcher | **renomear** |
| `GET /redline/player-ips` | ✅ | ✅ | launcher/op | |
| `GET/POST /redline/vote/status·cast·cancel·terminate` | ✅ | ✅ | **in-game** | ⚠️ unidade timeLeft (s vs ms) |
| `POST /redline/headless/ack-restart` | ✅ | ✅ | headless | |
| `GET /redline/debug/state` | ✅ | ❌ | diag | opcional |
| `POST /api/vpn/register` | ✅ | ❌ | — | **descartar** (Tailscale) |

---

## 6. Plano de corte (Fase 2 → 3)

1. **Fase 2 (código, no homolog dev):** aplicar B1 + B2 no C#; alinhar unidade do `vote/status` (verificar plugin in-game); unificar fontes de versão do manifesto; decidir PlayerStatus/Supabase + patches Fika/flash-reload. Cada mudança com build + review.
2. **Deploy no homolog:** montar a árvore `Launcher-Updater/` com o conteúdo de prod (config, mods_repo, Opcionais, server-version.txt); assinar o exe (018).
3. **Teste no dev** (`C:\Escape From Tarkov\SPT-4.0`): launcher → connect → sync completo → self-update → login/senha → optionals → vote (com cliente in-game). Server Fika coop compartilhado ⇒ janela combinada.
4. **Só então** produção (`100.106.152.7`) — gate humano, afeta todos os jogadores.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-05 | Guilherme | Criação — auditoria de paridade TS↔C# (Fase 1), 2 inventários sintetizados. |
