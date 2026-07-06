# 008 — Opções customizadas: configs performance · Spec (funcional + técnica)

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Kickoff:** [008-configs-performance-00-kickoff.md](./008-configs-performance-00-kickoff.md) · **Dep:** motor de sync do item 007 (`SPT.Launcher.Base/Sync/`)

> Spec fundida (funcional + técnica) — sessão autônoma, instrução do coordenador.

## Funcional

Toggle **"USAR CONFIGS PERFORMANCE"** na tela de Configurações (`SettingsView`), com descrição:

- **ON** → a próxima **verificação de arquivos** (auto-check do login ou "VERIFICAR ARQUIVOS") aplica por cima da instalação os arquivos da pasta `Launcher-Updater/config-performance` do server ("performance pack"), **sem sobrescrever configs que o usuário personalizou**.
- **OFF** → a próxima verificação **restaura os arquivos padrão do server** (os que o pack tinha sobreposto voltam à versão do manifesto principal). Nenhum I/O acontece no momento do clique — só na próxima sync.
- Persistência: `UsePerformanceConfigs` no config do launcher (`user/launcher/config.json`), **por máquina** (D5).

## Decisões e assunções

- **D1 — semântica do desligar = revert adiado via sync normal.** Desligar não reverte na hora; a próxima verificação reaplica a config padrão do server pela regra normal do motor. Funciona porque o apply do overlay **atualiza o baseline** (hash do pack): com o toggle OFF, o arquivo local == baseline ⇒ regra `preserve-divergent` R1.3 o trata como "não customizado, server evoluiu" ⇒ re-baixa o padrão. Arquivos que o usuário editou **depois** do overlay (≠ baseline) continuam preservados também no OFF.
- **D2 — overlay usa a MESMA regra preserve-divergent com baseline.** Configs customizadas pelo usuário (hash local ≠ baseline) **não são sobrescritas** pelo performance pack — mesma proteção R1.4 da sync normal. O pack só aplica onde o usuário não mexeu (local == baseline ou arquivo ausente).
- **D3 — overlay implementado como MERGE no manifesto efetivo (1 passada), não como 2ª passada literal.** O enunciado diz "aplica o overlay DEPOIS da sync normal"; uma 2ª passada literal geraria **churn permanente**: a passada normal reverteria os arquivos do pack para o padrão (local == baseline ⇒ R1.3 baixa) e a 2ª passada re-aplicaria o pack — dois downloads por arquivo em **toda** verificação, e o launcher nunca convergiria para "atualizado". O merge (`SyncManifestOverlay`: entrada do pack **sobrepõe** a entrada do manifesto principal por path; entradas só-do-pack são anexadas) é semanticamente idêntico ("o overlay vence a fonte normal") sem o churn — e é exatamente o "planner com fonte extra, reuso não fork": o planner/engine rodam inalterados sobre o manifesto efetivo, e o download roteia por origem (path do pack → endpoint do pack).
- **D4 — exposição server = seção `performanceOverlay` no manifesto existente + `GET /launcher/mods/performance-download?file=`.** Menor atrito: 1 scan a mais no `GenerateManifestAsync` (mesmo padrão do `mods_repo`), zero round-trip extra no client (o manifesto já é buscado), e o **hash do manifesto passa a cobrir o pack** (mudou o pack ⇒ hash muda ⇒ rescan). Rota separada (`performance-manifest`) exigiria fetch extra e não entraria no hash. Endpoint de download separado porque o path do pack **colide de propósito** com o path do `mods_repo` (é um override) — não dá pra servir os dois pelo `download` com a mesma chave.
- **D5 — persistência por máquina** (mesmo modelo do `EnabledOptionals`): configs são arquivos locais da instalação, não dado de conta.
- **A-008.1 — pack = overrides.** Assume-se que `config-performance/` contém **apenas overrides de arquivos que existem na distribuição padrão** (paths relativos à raiz do jogo, espelhando o `mods_repo`). Arquivo só-do-pack sob pasta `preserve-divergent` (ex.: `BepInEx/config/...`) **fica no disco após o OFF** (extras de config nunca são tocados, por design R1 do 007) — orientação ao operador no doc do server.
- **A-008.2 — toggle não dispara sync imediata.** O clique só persiste o setting; o efeito vem na próxima verificação (texto da descrição avisa). Mantém o toggle barato e evita concorrência com sync em andamento.
- **A-008.3 — arquivos do pack em grupos opcionais desligados não são forçados.** O merge preserva `optional`/`optionalGroup` da entrada base; o filtro de grupos do planner continua valendo.

## Interação com o baseline (raciocínio exigido pelo coordenador)

| Estado local | Toggle | Resultado |
|---|---|---|
| local == padrão == baseline | ON | manifesto efetivo = hash do pack ⇒ local ≠ efetivo, local == baseline ⇒ **baixa pack**; baseline := hash pack |
| local == pack == baseline (regime ON) | ON | local == hash efetivo ⇒ `UpToDate` (sem churn, converge) |
| local == pack == baseline | OFF | manifesto efetivo = padrão ⇒ local ≠ efetivo, local == baseline ⇒ **re-baixa padrão** (revert D1); baseline := hash padrão |
| local ≠ baseline (customizado) | ON ou OFF | `PreserveCustomized` — pack e padrão **nunca** sobrescrevem |
| pack atualizado no server | ON | local == baseline (pack antigo) ⇒ baixa pack novo |

O ponto crítico: o `SyncEngine` já grava `baseline[path] = ServerHash` a cada download aplicado — como o hash da ação vem do manifesto **efetivo**, o apply do overlay registra o hash do pack no baseline **sem mudança no engine**. É isso que torna o OFF reversível.

## Mudanças

### Server — `TarkovRedLine.Server/Controllers/ModUpdater.cs`

- `GenerateManifestAsync`: scan de `Launcher-Updater/config-performance` (cria a pasta se ausente, como faz com `mods_repo`) ⇒ `performanceOverlay = [{path, hash, size}]` no manifesto + cache path→físico.
- Novo `GET /launcher/mods/performance-download?file=` (guarda de path traversal, mesmo padrão do `optional-download`).

### Client

| Arquivo | Mudança |
|---|---|
| `SPT.Launcher.Base/Sync/SyncManifestOverlay.cs` (novo) | `Merge(baseFiles, overlayFiles)` → manifesto efetivo + `IsOverlayPath()` + `CreateDownloader(base, overlay)` (roteia download por origem). Puro/testável |
| `SPT.Launcher.Base/Helpers/LauncherSettingsProvider.cs` | Prop persistida `UsePerformanceConfigs` (default false); `EXPECTED_CONFIG_VERSION` 2→3 |
| `SPT.Launcher.Base/Controllers/RequestHandler.cs` | `DownloadPerformanceFile()` → `performance-download` |
| `ViewModels/SettingsViewModel.cs` | Prop `UsePerformanceConfigs` com save imediato |
| `Views/SettingsView.axaml` | ToggleSwitch do tema (label `.trl-label` + descrição `.trl-muted`, zero hex) no card OPÇÕES BÁSICAS |
| `ViewModels/ProfileViewModel.cs` | `CheckForUpdates`: parse `performanceOverlay`; toggle ON ⇒ planner recebe o manifesto **merged**; engine recebe downloader roteado |
| `ViewModels/ModUpdateViewModel.cs` | Mesma fiação (paridade da tela de referência do motor) |

## Testes (`SPT.Launcher.Tests/Sync/SyncOverlayTests.cs`)

1. Merge: override por path (case-insensitive) + append de entradas só-do-pack + flags `optional` preservadas da base.
2. Overlay aplica quando local == baseline (conteúdo vira o do pack; baseline := hash do pack; download roteado pro endpoint do pack).
3. Overlay **respeita customização**: local ≠ baseline ⇒ `PreserveCustomized`, conteúdo intacto.
4. OFF reverte via sync normal: após overlay aplicado, rodar com manifesto base ⇒ re-baixa padrão, baseline := hash padrão (cobre D1).
5. Regime ON converge: 2ª rodada com overlay ⇒ zero ações de I/O (cobre o anti-churn do D3).

## Gates

- `dotnet build SPT.Launcher.csproj -c Release` · `dotnet test SPT.Launcher.Tests.csproj -c Release` · `dotnet build TarkovRedLine.Server.csproj -c Release` — verdes (retry 3× em lock transitório). Nunca rodar o exe.
