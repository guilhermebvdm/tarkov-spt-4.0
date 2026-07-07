# 016 — Velocidade de download na "Verificar arquivos" · As-built

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Specs:** [00-kickoff](./016-velocidade-download-verificacao-00-kickoff.md) · [01-spec (fundida)](./016-velocidade-download-verificacao-01-spec.md)

> Sessão autônoma; execução sequencial após o 017 (ambos tocam o motor do 007). Item menor — spec funcional+técnica fundida.

## O que foi construído

Taxa de download suavizada (`12,4 MB/s`) exibida na barra de progresso durante a verificação/sync, medida no **ponto de download** (downloader injetável do 007), sem acoplar o `SyncEngine` à UI.

### Motor — `SPT.Launcher.Base/Sync/DownloadRateMeter.cs` (novo)

Medidor puro e testável. Janela deslizante de N=5 amostras; taxa = **soma(bytes)/soma(segundos)** da janela (ponderada por tempo, não média das taxas por-arquivo). `AddSample(bytes, elapsed)` ignora amostras com bytes ≤ 0 ou elapsed ≤ 0 (cache hit / arquivo instantâneo ⇒ nunca div/0 nem taxa infinita). `Format(bps)` estático: PT-BR (vírgula via `NumberFormatInfo` custom, imune ao host culture / InvariantGlobalization), MB/s decimal (1e6) com fallback KB/s (1e3) abaixo de 1 MB/s; não-positivo/não-finito → string vazia.

### Client — UI (`SPT.Launcher`)

- **`ModUpdateViewModel.cs`** / **`ProfileViewModel.cs`** — cada um com `_rateMeter`, props `DownloadSpeedText`/`DownloadBytesPerSec`/`HasDownloadSpeed`, e `WithSpeedMeter(inner)` que envolve o downloader como **camada mais externa** (captura base + overlay 008 + seed 017), cronometra com `Stopwatch`, alimenta o medidor e empurra a taxa pro VM via `Dispatcher.UIThread.Post` (o delegate roda em thread de Task). Reset do medidor no início de cada verificação/apply; texto limpo no `finally` (some ao terminar).
- **`Views/ModUpdateView.axaml`** e **`Views/ProfileView.axaml`** — coluna nova na linha do progresso: `TextBlock` `Classes="trl-mono trl-accent"`, `FontSize={DynamicResource TrlTextXs}`, `IsVisible={Binding HasDownloadSpeed}`. Zero hex. Alteração cirúrgica na ProfileView (só a coluna da taxa; o restyle da update bar intacto).

## Decisões e assunções

1. **A-016.1/4** — MB/s decimal (1e6), com KB/s abaixo de 1 MB/s (sem "0,0 MB/s").
2. **A-016.2** — média móvel = soma/soma da janela (N=5), não média das taxas.
3. **A-016.3** — separador PT-BR via `NumberFormatInfo` custom (robusto a InvariantGlobalization).
4. **A-016.5/6** — cache hit e arquivo instantâneo não geram taxa (sem div/0); rótulo oculto quando vazio.
5. **A-016.7** — medição no downloader injetável, empurrada por `Dispatcher` (thread-safe); engine inalterado.
6. **A-016.8** — granularidade por arquivo (o `DownloadBinary` bufferiza o arquivo inteiro); intra-arquivo descartado por custo na Base compartilhada.

## Testes — `SPT.Launcher.Tests/Sync/DownloadRateMeterTests.cs` (9)

`Single_sample_rate_is_bytes_over_seconds` · `Moving_average_is_sum_bytes_over_sum_seconds` · `Window_evicts_oldest_samples` · `Zero_elapsed_sample_is_ignored_no_division_by_zero` · `Zero_byte_sample_cache_hit_is_ignored` · `Reset_clears_all_samples` · `Format_uses_decimal_units_and_ptbr_separator` (4 casos) · `Format_non_positive_is_empty` (2) · `Format_non_finite_is_empty`.

## Gates

```
dotnet build SPT.Launcher.csproj -c Release            → 0 Erro(s) (169 warnings pré-existentes)
dotnet test  SPT.Launcher.Tests.csproj -c Release      → Aprovado! 82/82, 0 falhas (69 + 13 do 016)
```
Server não tocado neste item.

## Pendências

- **P-016.1 — validação visual E2E** (gate humano): rodar "Verificar arquivos" contra o server real com downloads grandes o suficiente p/ a taxa ser legível; confirmar (a) taxa aparece e suaviza durante o download, (b) some em cache hit / ao terminar, (c) formatação PT-BR (`12,4 MB/s`, `300 KB/s`), (d) sem flicker, (e) render `.trl-mono` correto em ProfileView e ModUpdateView.
