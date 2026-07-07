# 016 — Velocidade de download na "Verificar arquivos" · Spec (funcional + técnica)

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Kickoff:** [00-kickoff](./016-velocidade-download-verificacao-00-kickoff.md) · **Dep:** motor de sync do item 007 (`SPT.Launcher.Base/Sync/`)

> Spec fundida (funcional + técnica) — sessão autônoma, execução sequencial 017→016. Item menor: doc = 01-spec + 05-asbuild (matéria do 008).

## Funcional

Durante a verificação/sincronização de arquivos, exibir a **velocidade de download** (ex.: `12,4 MB/s`) na barra de progresso, para o usuário acompanhar o andamento. Aparece só enquanto há download real; some ao terminar e em cache hit.

- Local: barra de update da **ProfileView** (fluxo de login) e a **ModUpdateView** (tela de referência do motor).
- Render: número em `.trl-mono` + token de tamanho `Trl*`, cor `.trl-accent`, **zero hex**.

## Decisões e assunções (aplicadas, não perguntadas)

- **A-016.1 — MB/s decimal (1 MB = 1.000.000 bytes), não MiB.** Unidade de rede/marketing, mais familiar. Registrada como a escolha "MB/s vs MiB/s" do kickoff.
- **A-016.2 — média móvel curta** = soma(bytes)/soma(segundos) das últimas N amostras (N=5, janela deslizante). Não é a média aritmética das taxas por-arquivo (essa distorce com arquivos de tamanhos diferentes) — é a taxa agregada ponderada por tempo. Suaviza sem piscar.
- **A-016.3 — separador decimal PT-BR (vírgula)** via `NumberFormatInfo` custom (não depende de `CultureInfo` do host / modo InvariantGlobalization).
- **A-016.4 — sub-1 MB/s cai para KB/s** (decimal, 1.000) p/ links lentos não lerem "0,0 MB/s". Refinamento do A-016.1.
- **A-016.5 — cache hit / sem download → sem taxa.** Sem amostra usável ⇒ texto vazio ⇒ rótulo oculto (`HasDownloadSpeed`).
- **A-016.6 — arquivo instantâneo (elapsed ≤ 0) e bytes ≤ 0 são ignorados** na amostragem ⇒ nunca divide por zero nem emite taxa infinita.
- **A-016.7 — medição no ponto de download, fora da UI.** O `SyncDownloader` do 007 é injetável: o VM envolve o downloader (camada mais externa, captura base + overlay 008 + seed 017), cronometra a chamada, alimenta o medidor e empurra a taxa pro VM via `Dispatcher` (o delegate roda em thread de Task). O `SyncEngine` não muda.
- **A-016.8 — granularidade por arquivo.** O `DownloadBinary` bufferiza o arquivo inteiro em memória (um `CopyTo`), então cada arquivo = uma amostra. A média móvel de N arquivos suaviza. Intra-arquivo (chunk loop no `RequestHandler`) seria mais granular mas mexeria na Base compartilhada por mais superfície — descartado por custo/benefício.

## Mudanças

| Arquivo | Mudança |
|---|---|
| `SPT.Launcher.Base/Sync/DownloadRateMeter.cs` (novo) | Medidor puro/testável: `AddSample(bytes, TimeSpan)`, `BytesPerSecond`, `HasRate`, `Reset()`, `Format(bps)` estático (PT-BR, MB/s decimal + KB/s fallback) |
| `ViewModels/ModUpdateViewModel.cs` | Props `DownloadSpeedText`/`DownloadBytesPerSec`/`HasDownloadSpeed`; `_rateMeter` resetado no início de check/update; `WithSpeedMeter(inner)` como camada externa do downloader; texto limpo no `finally` |
| `ViewModels/ProfileViewModel.cs` | Mesma fiação na barra de update (via `Dispatcher`); reset no início de `CheckForUpdatesCore`, limpo no `finally` |
| `Views/ModUpdateView.axaml` | Coluna nova na linha do progresso: `TextBlock` `.trl-mono .trl-accent` `TrlTextXs`, `IsVisible={Binding HasDownloadSpeed}` |
| `Views/ProfileView.axaml` | Idem na região da barra de update (cirúrgico — só a coluna da taxa) |

## Testes (`SPT.Launcher.Tests/Sync/DownloadRateMeterTests.cs`)

Cálculo bytes/elapsed determinístico · média móvel = soma/soma (não média das taxas) · eviction da janela · elapsed 0 ignorado (sem div/0) · bytes 0 (cache) ignorado · `Reset` · `Format` (12,4 MB/s · 1,0 MB/s · KB/s sub-1MB · não-positivo/não-finito → vazio).

## Gates

`dotnet build SPT.Launcher.csproj -c Release` · `dotnet test SPT.Launcher.Tests.csproj -c Release` — verdes. (Server não tocado.) Nunca rodar o exe.
