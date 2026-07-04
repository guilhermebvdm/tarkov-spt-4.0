# Backlog — Launcher (Launcher4.0beta)

> Índice de itens de backlog do launcher TRL (fork do SPT Launcher, Avalonia). Origem: card Trello [MTav8H5f — "Ajustes no Launcher para aceitar classes de personagens"](https://trello.com/c/MTav8H5f/19-ajustes-no-launcher-para-aceitar-classes-de-personagens), importado em 2026-07-03 — cada item cita o nº do checklist do card. Ciclo: mesmo workflow SDD dos mods ([WORKFLOW.md](../../../WORKFLOW.md)) com as adaptações abaixo.

## Adaptações do workflow (launcher ≠ mod)

- **Sandbox:** todo o `launcher/Launcher4.0beta/` é editável (papel do `modded/`). O upstream intocado é [`launcher/Launcher4.0/`](../../Launcher4.0/) (papel do `original/` — **nunca editar**; citar como `// ref: Launcher4.0/<arquivo>:<linha>`).
- **Build:** `dotnet build launcher/Launcher4.0beta/project/Launcher.sln` — o `/compile-mod` não cobre o launcher. Deploy: o exe **tem** que rodar de `D:\SPT` (deriva `GamePath` de `AppContext.BaseDirectory`).
- **Evidência:** hierarquia normal do repo; o upstream `Launcher4.0/` entra como fonte 🥈 (padrões do launcher vanilla).
- **`<ref>` nos commands:** `Launcher4.0beta NNN` ou o path da pasta do item.
- **Itens 001–003 (entregues antes deste backlog existir):** status 🟢 com **code-review retroativo pendente** — o `/code-review` roda direto sobre o código entregue (sem `05-asbuild` prévio; gerar o asbuild retroativo durante a review se fizer sentido).
- **Server-side:** quando um item exigir mudança no mod CustomClasses (ex.: item 004), a parte server vive num **item irmão no backlog do CustomClasses** — aqui fica só a parte launcher + o link.

| # | Título | Resumo | Pasta | Status |
|---|---|---|---|---|
| 001 | Nova tela de login | Redesign TRL da tela de login (Trello 1 ✅). Entregue no commit 88db747. | [001-tela-login/](./001-tela-login/) | 🟢 code-review pendente |
| 002 | Tela de criação de conta | Redesign TRL do fluxo de criar conta (Trello 2 ✅). Entregue no commit 88db747. | [002-tela-criacao-conta/](./002-tela-criacao-conta/) | 🟢 code-review pendente |
| 003 | Tela de classes — listagem | Tela de seleção de classe (lista + painel de detalhe) (Trello 3.1 ✅). Entregue no 88db747, **mas com dados 100% mockados** — dados reais são o 004. | [003-classes-listagem/](./003-classes-listagem/) | 🟢 code-review pendente |
| 015 | Fundação de tema TRL | Tradução do TRL Design System p/ Avalonia: tokens semânticos `Trl*`, ControlThemes (radius 0, tan accent, vermelho disciplinado), fontes Bender, controles de assinatura (laser, panel, screen-bar) + shim de keys legadas + views-piloto (Login/Register/ConnectServer/notifications). Executa ANTES do 004. | [015-tema-trl-fundacao/](./015-tema-trl-fundacao/) | 🟢 |
| 004 | Tela de classes — dados reais (CustomClasses) | Núcleo do card (Trello 3 + 3.2): rota pública de classes no CustomClasses (item irmão 058 lá) + launcher consome lista/descrição reais, remove mock, fallback p/ editions vanilla. **Vant/desv e painel de arte descopados (decisão 2026-07-03).** | [004-classes-dados-reais/](./004-classes-dados-reais/) | 🟢 |
| 005 | Definir senha em conta sem senha | Validar/corrigir o fluxo de criar senha ao logar em conta sem senha (Trello 1.1). `CreatePasswordDialog` já existe — validar ponta a ponta. | [005-definir-senha-conta-sem-senha/](./005-definir-senha-conta-sem-senha/) | 🟢 |
| 006 | Login Tailscale sem navegador | Ao abrir o launcher, não abrir navegador p/ login do Tailscale (Trello 0 + 0.1). | [006-login-tailscale-sem-navegador/](./006-login-tailscale-sem-navegador/) | 🟢 |
| 007 | Sincronização de arquivos | Regras por pasta: `config` (preserva divergentes), `config-server` (espelho c/ exclusão), `patchers`/`plugins` (espelho movendo removidos p/ `*-disabled`) + cancelar verificação c/ confirmação + manifesto "X arquivos atualizados" em `/user/launcher` (Trello 4.1, 4.1.1×4, 4.1.2, 4.1.3). | [007-sincronizacao-arquivos/](./007-sincronizacao-arquivos/) | 🟢 |
| 008 | Opções customizadas: configs performance | Toggle "Usar configs performance" + descrição; sobrepõe `config-performance` do server na `config` do usuário, mantendo divergentes (Trello 4.2 + 4.2.1). Usa o motor de sync do 007. | [008-configs-performance/](./008-configs-performance/) | 🟢 |
| 009 | Mods opcionais com descrição | Descrição em todos os mods opcionais + toggles: Hollywood Effects, PiP Disable (avaliar desabilitar ExternalResolution), IRL, Visceral (Trello 4.3 + 4.3.2.1–4). | [009-mods-opcionais-descricao/](./009-mods-opcionais-descricao/) | 🟢 |
| 010 | Botão "Excluir conta" | Excluir conta na tela logada — excluir ≠ wipe (hoje só existe wipe); verificar suporte do server SPT (Trello 4.4). | [010-excluir-conta/](./010-excluir-conta/) | 🟢 |
| 011 | Lista de mods | **ADIADO (decisão 2026-07-03)** — item vago no card (Trello 5); escopo será definido com o usuário quando sair do adiamento. Base: `ModInfoCollection`/`TotalModsCard`/`ModInfoView`. | [011-lista-mods/](./011-lista-mods/) | ⚫ |
| 012 | Remover Targram do menu | Remover botão/command Targram dos menus (Trello 6.1). 4 pontos já mapeados. | [012-remover-targram/](./012-remover-targram/) | 🟢 |
| 013 | Versão do server dinâmica | Server reporta `0.1.0-beta` via arquivo/endpoint; launcher exibe dinamicamente (hoje footers hardcoded) (Trello 6.2). | [013-versao-server-dinamica/](./013-versao-server-dinamica/) | 🟢 |
| 014 | Release launcher 2.0.0 | Bump de versão (hoje `1.4.7.0`) + strings hardcoded + build + distribuição (Trello 6.3). Fecha o épico — depende de todos. | [014-release-v2/](./014-release-v2/) | 🟢 |
| 016 | Velocidade de download na "Verificar arquivos" | Exibir a velocidade do download (ex.: MB/s) durante a verificação/sync de arquivos, na barra de update da ProfileView e/ou na `ModUpdateView`. Estende o motor/relatório do 007. | [016-velocidade-download-verificacao/](./016-velocidade-download-verificacao/) | 🟢 |
| 017 | Preencher `config` do usuário a partir de `config-server` (seed por nome) | Seed unidirecional: para cada arquivo em `BepInEx/config-server` do server, se **não existir por nome** em `BepInEx/config` do usuário → copiar; se **já existir por nome** (metadados/conteúdo irrelevantes) → não fazer nada. ⚠️ **Reconcilia/ajusta a regra `config-server` do [007](./007-sincronizacao-arquivos/)** (hoje mirror-delete). | [017-seed-config-de-config-server/](./017-seed-config-de-config-server/) | 🟢 |
| 018 | Segurança do auto-update (cert pinning + assinatura) | 🔴 RCE: TLS desligado + exe executado sem verificar assinatura/hash. [AUDIT](../AUDIT-2026-07-04-code-product-ds.md) §B1. | [018-auto-update-security/](./018-auto-update-security/) | ⚪ |
| 019 | Guard de raiz + atomicidade nos caminhos legados de FS | 🔴 `deleteFiles` do manifesto + `OptionalModsHelper` deletam/escrevem com traversal, sem guard/atômico. AUDIT §B2. | [019-fs-root-guard-legacy/](./019-fs-root-guard-legacy/) | 🟢 |
| 020 | Integridade do cofre de senhas | 🟡 match case-insensitive grava no perfil errado / colide contas; delete não-atômico; `/profile/get` plaintext. AUDIT (005/010). | [020-password-vault-integrity/](./020-password-vault-integrity/) | 🟢 |
| 021 | Mods opcionais: grupos faltantes + base-URL | 🟡 toggles PiP/IRL não existem; descrição só alcança hollywood; `GetServerBaseUrl` derruba porta/TLS → download falha em silêncio. AUDIT (009). | [021-optional-mods-groups-baseurl/](./021-optional-mods-groups-baseurl/) | 🟢 |
| 022 | Robustez de comandos + thread-safety de UI | 🟡 confirmação frágil de wipe/remove; `async Task` commands com exceção não observada; ConnectServer fora da UI thread. AUDIT (client). | [022-command-ui-robustness/](./022-command-ui-robustness/) | 🟢 |
| 023 | Coop-sync hardening (Fika) | 🟡 mirror-move quarentena `Fika.Core.dll` ausente do manifesto; excluir conta do host em sessão coop; authkey headless reusável. AUDIT (coop). | [023-coop-sync-hardening/](./023-coop-sync-hardening/) | ⚪ |
| 024 | Migração DS da SettingsView + unificar chrome | 🔴 (DS) SettingsView não migrou (~20 hex + sidebar/cards próprios); dot Dev Mode hex. AUDIT §B3. | [024-settingsview-ds-migration/](./024-settingsview-ds-migration/) | ⚪ |
| 025 | Aposentar código morto + fechar shims Legacy | 🟡 5 controls órfãos + helpers mortos (WireGuard TLS bypass); ModInfoView legada; shims `.card/.acc/.alt` (fecha débito do 014). AUDIT (DS/client). | [025-dead-code-legacy-shims/](./025-dead-code-legacy-shims/) | ⚪ |

## Itens 018–025 (derivados do review)

> Gerados a partir da auditoria [AUDIT-2026-07-04-code-product-ds.md](../AUDIT-2026-07-04-code-product-ds.md) (review de código + produto + DS de todo o launcher, 2026-07-04). Cada item tem `00-kickoff.md` com achados, `file:line` e critérios de aceite seed. **Bloqueadores (fazer antes de distribuir em produção):** 018 (RCE auto-update), 019 (guard de FS), 024 (DS SettingsView). Riscos de negócio: 020 (senha), 021 (mods opcionais/coop). Os 🟢 menores foram absorvidos como "correlatos" dentro dos kickoffs temáticos (019/022/025).

## Épico: Tela Logado (Trello 4.x → itens 007–010)

> O item "4. Tela Logado" do card é um guarda-chuva: sync de arquivos (4.1 → **007**), opções customizadas (4.2 → **008**), mods opcionais (4.3 → **009**) e excluir conta (4.4 → **010**). O 008 consome o motor de sync do 007 — executar 007 antes.

## Legenda

- ⚪ Backlog · 🟡 Em progresso · 🟢 Entregue · 🔴 Cancelado · ⚫ Adiado/descopado

## Fluxo

1. `/create-spec <ref>` → spec funcional (critérios de aceite + corner cases)
2. `/review-spec <ref>` → editor crítico da spec funcional
3. `/create-technical-spec <ref>` → pré-código com refs
4. `/review-technical-spec <ref>` → review-NN.md incremental; resolver até zerar 🔴
5. `/code-mod <ref>` → implementa em `Launcher4.0beta/`
6. `dotnet build project/Launcher.sln` → build (fora do `/compile-mod`)
7. `/code-review <ref>` → revisão do build
