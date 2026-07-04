# 008 — Opções customizadas: configs performance · As-built

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Specs:** [00-kickoff](./008-configs-performance-00-kickoff.md) · [01-spec (fundida)](./008-configs-performance-01-spec.md)

> Desvio de processo registrado: sessão autônoma (Wave 3) — spec funcional+técnica fundida em `01-spec` e reviews dispensadas por instrução do coordenador.

## O que foi construído

### Server — `mods/TarkovRedLine4.0/Server/.../Controllers/ModUpdater.cs`

- `GenerateManifestAsync` escaneia `Launcher-Updater/config-performance` (cria a pasta se ausente, mesmo padrão do `mods_repo`) e emite a seção **`performanceOverlay`** no manifesto existente: `[{path, hash, size}]`, paths relativos à raiz do jogo. O hash do manifesto passa a cobrir o pack ⇒ mudou o pack, clients rescaneiam (decisão D4 — menor atrito, zero round-trip extra).
- Novo endpoint **`GET /launcher/mods/performance-download?file=`** com guarda de path traversal (mesmo padrão do `optional-download`). Endpoint separado porque os paths do pack colidem de propósito com os do `mods_repo` (são overrides).

### Client — Base (`SPT.Launcher.Base`)

| Arquivo | Mudança |
|---|---|
| `Sync/SyncManifestOverlay.cs` (novo) | `Merge(base, overlay)` → manifesto **efetivo** (override por path normalizado; entradas só-do-pack anexadas como obrigatórias; flags `optional`/`optionalGroup` preservadas da base — A-008.3) + `IsOverlayPath()` + `CreateDownloader(base, overlay)` (roteia o download por origem). Puro, sem I/O — o motor 007 roda inalterado sobre o resultado |
| `Helpers/LauncherSettingsProvider.cs` | Prop persistida `UsePerformanceConfigs` (default false, por máquina — D5); `EXPECTED_CONFIG_VERSION` 2→3 (re-save grava o campo novo) |
| `Controllers/RequestHandler.cs` | `DownloadPerformanceFile()` → `performance-download`; miolo de download binário extraído p/ `DownloadBinary(url)` (dedup com `DownloadModFile`). Aproveitado: `RequestOptionalsList()` do item 009 (lote coordenado) |

### Client — UI (`SPT.Launcher`)

- **`Views/SettingsView.axaml`** — card OPÇÕES BÁSICAS ganhou o toggle **"USAR CONFIGS PERFORMANCE"**: `ToggleSwitch` do tema TRL (retangular), label `.trl-label`, descrição `.trl-muted` `TrlTextXs` com wrap explicando a semântica (aplica na próxima verificação · customizações preservadas · desligar restaura o padrão). Zero hex novo.
- **`ViewModels/SettingsViewModel.cs`** — prop `UsePerformanceConfigs` com **save imediato** no clique (não depende do GoBack/close).
- **`ViewModels/ProfileViewModel.cs`** (fluxo principal de verificação) — `CheckForUpdates` parseia `performanceOverlay`; com o toggle ON e pack não-vazio, o planner recebe `SyncManifestOverlay.Merge(...).Files` e o `BuildSyncEngine` ganha o downloader roteado (path do pack → `DownloadPerformanceFile`).
- **`ViewModels/ModUpdateViewModel.cs`** — mesma fiação (paridade da tela de referência do motor); `PopulateFileStatuses` passou a receber o manifesto efetivo (tamanhos corretos p/ entradas do pack).

## Decisões e assunções (consolidadas — detalhe na spec)

1. **D1 — desligar = revert adiado via sync normal.** O apply do overlay grava o **hash do pack no baseline** (o `SyncEngine` já grava `ServerHash` da ação — que agora vem do manifesto efetivo, sem mudança no engine). Com o toggle OFF, local == baseline ⇒ R1.3 (não customizado, server evoluiu) ⇒ re-baixa o padrão. Sem I/O no clique (A-008.2).
2. **D2 — overlay respeita preserve-divergent com baseline.** Config customizada (local ≠ baseline) nunca é sobrescrita pelo pack — mesma R1.4 da sync normal.
3. **D3 — merge em 1 passada, não 2ª passada literal.** 2ª passada literal churnaria toda verificação (a passada normal reverteria o pack pro padrão e a 2ª re-aplicaria — 2 downloads/arquivo, nunca converge). O merge é semanticamente idêntico ("overlay vence") e é o "planner com fonte extra — reuso, não fork" pedido.
4. **D4 — exposição = seção no manifesto + endpoint próprio de download** (vs rota `performance-manifest` separada).
5. **D5 — persistência por máquina** (`user/launcher/config.json`), modelo do `EnabledOptionals`.
6. **A-008.1 — pack = overrides da distribuição padrão.** Arquivo só-do-pack sob pasta preserve-divergent fica no disco após OFF (extras de config nunca são tocados — design R1 do 007). Orientação ao operador: só colocar overrides no `config-performance/`.
7. **A-008.3 — pack não força arquivos de grupos opcionais desligados** (flags da base preservadas no merge; filtro de grupos do planner continua valendo).

## Testes — `SPT.Launcher.Tests/Sync/SyncOverlayTests.cs` (5 novos; suíte 39→44)

1. `Merge_OverridesByPath_AppendsPackOnly_AndKeepsOptionalFlags` — override case/separator-insensitive, casing da base preservado, append só-do-pack, flags opcionais da base.
2. `Overlay_Applies_WhenLocalEqualsBaseline_AndRoutesToOverlaySource` — pack aplica sobre estado convergido; download sai da fonte do pack (base downloader nunca tocado); **baseline := hash do pack**.
3. `Overlay_PreservesUserCustomizedConfig` — local ≠ baseline ⇒ `PreserveCustomized`, conteúdo intacto (D2).
4. `TurningOverlayOff_RevertsToServerDefault_ViaNormalSync` — regime ON → rodada com manifesto base re-baixa o padrão e baseline := hash padrão (D1).
5. `SteadyStateOn_SecondRunHasNoIoActions` — 2ª rodada ON converge, zero ações de I/O (anti-churn D3).

## Gates

```
dotnet build SPT.Launcher.csproj -c Release            → 0 Erro(s) (155 warnings pré-existentes nullability/CA1416)
dotnet test  SPT.Launcher.Tests.csproj -c Release      → Aprovado! 44/44, 0 falhas
dotnet build TarkovRedLine.Server.csproj -c Release    → 0 Erro(s) (32 warnings pré-existentes)
```

## Pendências

- **P-008.1 — E2E contra o server real** (gate humano, memória do repo: escrita em SPT exige validação no jogo): popular `Launcher-Updater/config-performance/` no server com overrides reais (ex.: `BepInEx/config/*.cfg` com presets de FPS), ligar o toggle, verificar apply/preservação/revert no `D:\SPT` + `sync-state.json`/`last-update.json`.
- **P-008.2 — conteúdo do pack é decisão de operação**: quais configs entram no performance pack (candidatos óbvios pelos configs de `D:\SPT`: HollywoodGraphics, grass/shadow tweaks) não faz parte deste item.
