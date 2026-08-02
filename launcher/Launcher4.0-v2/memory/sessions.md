# Memória de Sessões — Launcher TRL (Launcher4.0-v2)

> Memória cronológica do launcher, no mesmo formato dos mods (`mods/<mod>/memory/sessions.md`). O launcher segue o mesmo workflow SDD do backlog, então este arquivo é lido pelos commands de desenvolvimento e pelo `/update-me-about-this-mod`. Criada em 2026-07-26.

## Estado atual

> **Delta 2026-07-26:** Launcher em **v2.7.3** em produção, funcionando sem reports (validação por uso; pendências in-game dos itens entregues fechadas por uso). O **item 030 (tela "Mods e Configs")** foi **IMPLEMENTADO nesta data** via `/g-autodev` — Fases 1-3 (motor + servidor + UI) + code-review adversarial, 239 testes verdes, ver [[P-1.1]]. **Ainda NÃO em produção:** falta o conteúdo do servidor ([[P-1.2]]), os gates in-game e o rollout ordenado (R-11). O canal de performance virou regra de pasta `performance-to-config` (aposentou o overlay do item 008); mods opcionais vêm de `plugins-optional.json` (aposentou a pasta `Opcionais/` + rotas `optionals-*`).

- **Estrutura (papéis, análogos ao mod):** `project/` = código editável (papel do `modded/`); upstream intocado em `launcher/Launcher4.0/` (papel do `original/` — **nunca editar**, citar como `// ref: Launcher4.0/<arquivo>:<linha>`); `backlog/` (mesmo SDD dos mods); `docs/`; `tools/` (`sign-launcher.ps1`, geração de chave); `dist/` (builds); `assets/`; esta `memory/` (criada 2026-07-26).
- **Build:** self-contained single-file (~145 MB) — **não** framework-dependent. ⚠️ O csproj tem `RuntimeIdentifier=win-x64` mas **não** força `SelfContained`; depende do flag `--self-contained true` no `dotnet publish`. Publish/build sem o flag gera exe de ~244 KB que **exige .NET runtime instalado** e mostra "You must install .NET Desktop Runtime" — foi o que quebrou pro amigo do usuário (2026-07-26); resolvido entregando o single-file. `dotnet build launcher/Launcher4.0-v2/project/Launcher.sln` (o `/compile-mod` não cobre o launcher).
- **Deploy:** o exe **tem** que rodar de `D:\SPT` (deriva `GamePath` de `AppContext.BaseDirectory`). Renomear `Tarkov Red Line.exe` → `TRL.Launcher.exe` (nome que o mod procura no `Launcher-Updater/`). Assinar com `sign-launcher.ps1` (`Verified OK`) e subir o **par** exe + `.sig` no `Launcher-Updater/` de produção; clientes 2.x se auto-atualizam.
- **Ambientes:** ÚNICO ambiente de trabalho = `D:\SPT` (esta máquina). Produção = remoto `100.106.152.7` (Tailscale porta 6969, inalcançável daqui). Não há mais `dev`/`homolog` separados.
- **Versão:** o csproj tem **4** campos (`Version`/`AssemblyVersion`/`FileVersion`/`InformationalVersion`) — bumpar TODOS a cada release. `InformationalVersion` (=ProductVersion) é o que o auto-update lê; sem sufixo `+sha` (senão `Version.TryParse` falha).
- **i18n:** só `en`/`pt` oficiais (`SupportedIetfTags`). Loader **all-or-nothing** — 1 chave faltando num locale derruba o locale inteiro e cai no fallback pt. `LocaleBootstrap` embute os 2 JSON no exe e reescreve na pasta de runtime no boot. Templates com placeholder usam `LocalizedFormatConverter` (MultiBinding, reativo à troca de idioma).
- **Auto-update:** RSA-2048 SHA-256; chave privada `.keys/launcher-update-dev-private.pem` **nunca sai da máquina**; a pública é embutida no exe. O `.sig` prova que o exe baixado é autêntico antes de rodar (trava anti-RCE, item 018).

## Pendências / próximos passos conhecidos

- **[P-1.1]** ✅ **FECHADA 2026-07-26 (código)** — item 030 IMPLEMENTADO via `/g-autodev` (Fases 1-3 + code-review adversarial de 3 revisores). Commits: `fd8fdecf` (Fase 1 motor), `722e5630` (Fase 2 servidor), `1ca42684`+`f051d6a1` (Fase 3 UI + remoção do modelo antigo), `afad9913` (achados do review, incl. 1 🔴 NRE que derrubava o sync inteiro). 239 testes verdes. **Continua pendente (gate humano):** os gates in-game G-1..G-10 + o conteúdo do servidor ([[P-1.2]]) + o **rollout ordenado (R-11 — publicar o launcher ANTES de mover a pasta config-performance no servidor**, senão cliente antigo materializa a pasta-fonte). Decisões-chave do build: canal híbrido (o `PerformanceCopy` grava baseline, senão não converge); D-14 revisado (backup do force fica na RAIZ da quarentena, origens novas em subpasta — sem migração); fallback `optionalId ?? optionalGroup` até o rollout fechar em prod.
- **[P-1.2]** (aberta 2026-07-19) **Conteúdo do servidor** para os gates in-game do 030 (pré-requisito de TESTE, não do código): `plugins-optional.json`, pasta `config-performance/` no lugar novo (`mods_repo/BepInEx/`), e recriar `gore`/`hollywood`/`grass` no formato novo. Pendente do usuário.
- **[P-1.3]** (aberta 2026-07-19) **Feature "configs recomendadas"** (botão opt-in que aplica teclas/gráficos/fundo do menu curados) — **PARADA** a pedido do usuário, esperando ele fechar o conteúdo curado. Design em memória `project_recommended_settings_button`.
- **[P-1.4]** (aberta 2026-07-19) **Deploy da 2.7.3 pra produção** (par exe + `.sig` no `Launcher-Updater/`) — pendente do usuário. Feito local (D:\SPT) e verificado por log; falta subir pro remoto.
- **[P-1.5]** (aberta 2026-07-04) **Segurança deferida** — itens **026** (MD5 → SHA-256 no manifesto/baseline) e **027** (remover eco de senha em texto plano + fechar TLS bypass global). ⚪ no backlog, **código não escrito** — não são pendência de teste, é trabalho por fazer.
- **[P-1.6]** (aberta 2026-07-26, opcional) **Blindar o csproj** com `<SelfContained>true</SelfContained>` + `<PublishSingleFile>true</PublishSingleFile>` no `Configuration=Release`, pra nenhum publish/build futuro gerar exe framework-dependent que o auto-update distribua quebrado. Oferecido ao usuário; aguardando decisão.
- **[P-1.7]** (aberta 2026-07-15, reduzida por uso 2026-07-26) **Login Tailscale abre navegador** (item 006). Relato de 2026-07-15 de que ainda abre o site no login; sem novos relatos desde então. Marcado 🟢 "resolvido por uso" no backlog — **reabrir se voltar a acontecer**. Hipótese registrada: quem abre o navegador é a GUI do `tailscale-ipn.exe`, não o `--authkey --unattended` do launcher.

---

## 2026-07-26 (GMT-3) — Sessão: specs do item 030 + fixes de UX + criação desta memória

**Tema central:** fechar todo o ciclo de especificação da tela "Mods e Configs" (item 030) até estar pronta pra código, mais uma leva de correções de UX pequenas que foram para produção, e a criação desta estrutura de memória.

- **Item 030 — ciclo SDD completo até 0 pendências.** Debate com o usuário fechou o conceito (mods opcionais = liga/desliga mod inteiro; configs de performance = troca de config curada), depois: spec funcional (34 CA, 20 corner cases, 21 decisões) → `/review-spec` → 4 decisões pendentes fechadas → spec técnica v1 → `/review-technical-spec` (8 pontos, 3 bloqueadores) → todos resolvidos → **revisão completa com 3 lentes independentes** (rastreabilidade, adversarial no motor, consistência documental) → **review 02: 13 pontos, 7 bloqueadores estruturais** → spec técnica **reescrita (v2)** com todos aplicados. Achados que mudaram o desenho: (1) o canal de performance **não pode clonar o `ForceToConfig`** — o force não grava baseline, o canal precisa gravar (`PerformanceCopy`) senão aplica uma vez e nunca mais converge; (2) o **espelho de referência** (D-10) era impossível como especificado (um prefixo → uma regra) — resolvido por **D-18**: uma pasta física publicada sob dois prefixos lógicos (`config-performance/` aplica + `config-performance-ref/` espelha), sem conteúdo duplicado; (3) a quarentena de mod desligado passava por fora do **guard coop-safe do Fika** (quebraria o join); (4) o eixo **desligar** simplesmente não existia na v1. Todas as decisões novas (D-18..D-21) na spec funcional.
- **Fixes de UX que foram a produto (2.6.x → 2.7.3):** i18n completo en/pt (remoção de strings hardcoded via workflow), troca de idioma real, toggle "usar servidor local" + acesso a Configurações no erro de conexão e na tela de login, reconexão real ao trocar IP (com confirmação se logado), fixes de double-settings/gear/scroll, e por fim a tradução do painel de perfil (2.7.3, `LocalizedFormatConverter` + 6 chaves).
- **Diagnóstico do ".NET Desktop Runtime" (amigo do usuário):** o exe dele era framework-dependent (pequeno, pede runtime); o `9.0.14` da mensagem bate com o `runtimeconfig` do build. O single-file self-contained (145 MB) que o usuário publicou **não** tem esse problema. Resolvido passando o single-file. Causa raiz: o csproj não força `SelfContained` (ver P-1.6).
- **Pendências de validação fechadas por uso:** a pedido do usuário, os gates de teste in-game dos itens entregues (001-025, 028) foram considerados **fechados por uso real** — launcher em produção há dias sem reports. Não houve pente-fino formal; qualquer bug futuro reabre o item pontual.
- **Criada esta `memory/` do launcher** — antes o launcher não tinha memória de sessão como os mods; agora tem, no mesmo formato.

## 2026-08-02 02:43 (GMT-3) — Sessão 2: doc de fluxo (AutoSync + "Verificar arquivos") e mapa do motor de sync

**Tema central:** documentar em visão de produto como o cache 3D chega aos jogadores — o fluxo do AutoSync no servidor e o "Verificar arquivos" do launcher — como parte do rework do AutoSync (trabalho principal em `mods/TarkovRedLine4.0`).

**Decisões-chave:**
- **Criada a pasta `docs/` do launcher** com o primeiro doc: [docs/01-fluxo-autosync-e-verificar-arquivos.md](../docs/01-fluxo-autosync-e-verificar-arquivos.md) (commit `b73faa33`) — visão de produto (sem refs a código), 3 diagramas mermaid, tabela de arquivos gerados e critérios de aceite CA-A1..A7 (AutoSync) + CA-L1..L6 (launcher). Publicado também como artifact privado para visualização imediata.
- Nenhum código do launcher foi tocado nesta sessão.

**Lições / hipóteses descartadas:**
- **Hipótese refutada:** "o hash do AutoSync (`ultimo_mod_hash.txt`) tem algum papel no launcher 2.0.0". Não tem — nenhum `.cs` do launcher o referencia; o launcher só consome o manifesto MD5 por arquivo gerado por `ModUpdater.cs:437` (`GenerateManifestAsync`), que **não é arquivo em disco**: vive em memória (`_manifestCache`) e regenera via `GET /launcher/mods/refresh` ou restart do servidor.
- Não confundir os hashes homônimos: `manifest_hash.txt` (cliente, MD5 do manifesto inteiro — o skip de scan que ele habilitaria está **desativado por decisão**, `ProfileViewModel.cs:608`) ≠ o extinto `ultimo_mod_hash.txt` do AutoSync.
- O cache 3D (`SPT/user/cache`) entra no manifesto pela varredura **genérica** do `mods_repo` — não há lógica dedicada de cache no launcher; e como `SPT/user/cache` não é `managedPath`, extras locais do jogador nunca são deletados pelo sync.

**Atividade cronológica:**
1. Agente Explore mapeou o motor de sync (`SyncPlanner`/`SyncEngine`/`SyncBaseline`/`SyncRuleResolver` + `ModUpdater` no server mod) — base factual do doc.
2. Doc 01 escrito no padrão `NN-` com cabeçalho/histórico, commitado (`b73faa33`).
3. Mermaid não renderizava no preview do editor (Antigravity) — resolvido com artifact + extensão `bierner.markdown-mermaid` instalada com aprovação do usuário.

**Cross-refs:**
- Trabalho principal desta sessão (rework do AutoSync-Cache v2): ver `mods/TarkovRedLine4.0/memory/sessions.md` 2026-08-02 (Sessão 1).
