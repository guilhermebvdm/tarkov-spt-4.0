# 032 — Velocidade de download (medição intra-arquivo) · Spec técnica

**Mod:** Launcher4.0-v2
**Criado:** 2026-08-02
**Spec funcional:** [032-velocidade-download-nunca-funcionou-01-spec.md](./032-velocidade-download-nunca-funcionou-01-spec.md)
**Irmão:** [031 (notificações)](../031-notificacao-sync-mensagem-final/031-notificacao-sync-mensagem-final-02-spec-tech.md) — reset e o layout da barra (Grid.Column=1) vêm de lá; implementar juntos.

> Verificação = `dotnet build` (o `DownloadRateMeter` já tem testes verdes — não muda). O "número útil durante um download grande" é **gate in-game** contra o servidor real (a medição de rede não é testável em unidade).

## 1. Estratégia

Hoje o download é **100% bufferizado**: `RequestHandler.DownloadBinary` faz `responseStream.CopyTo(memStream).ToArray()` ([RequestHandler.cs:229-230](../../project/SPT.Launcher.Base/Controllers/RequestHandler.cs#L229)), e o `WithSpeedMeter` só amostra **depois** que o arquivo inteiro terminou ([ProfileViewModel.cs:807-827](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L807)). Três mudanças:

1. **Progresso intra-arquivo no downloader** — trocar o `CopyTo` por um laço `Read(buffer)` que reporta os bytes acumulados **durante** a transferência, via um `Action<long>` opcional. Sem o callback, comportamento idêntico ao atual (preserva o contrato da Base, CC-4). O retorno continua `byte[]` (o `SyncEngine` precisa do buffer inteiro para `WriteAtomic`/`ComputeMd5`).
2. **Alimentar o `DownloadRateMeter` por chunk + torná-lo thread-safe** — o downloader concreto dos VMs passa o callback que faz `AddSample(deltaBytes, deltaElapsed)` a cada bloco; o `WithSpeedMeter` (medição por-arquivo) é **removido**. ⚠️ **CR-01-01:** isso muda o padrão de acesso — hoje `AddSample` e a leitura rodam na **mesma** thread (a continuação do `await` do `WithSpeedMeter`, que resume na UI thread); com a medição dentro do `Task.Run` e a leitura no ticker (UI thread), passam a ser **duas** threads sobre `_sumBytes`/`_sumSeconds`/`Queue` sem trava → **race** (torn read / par de somas incoerente). Então o `DownloadRateMeter` **ganha um `lock` e um `Snapshot()` de leitura atômica** — a linha "motor inalterado" cai. Os testes single-thread seguem verdes (o `lock` não muda o comportamento).
3. **Ticker de UI (~500 ms)** — um `DispatcherTimer` no VM lê o `_rateMeter` em cadência fixa e atualiza `DownloadSpeedText`, desacoplando a UI dos chunks (a taxa se mexe mesmo com um único arquivo grande em voo). Start no início do apply, Stop no fim. O **reset** e o **zerar no fim** já existem ([:456](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L456), [:769](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L769)) — coordenados com o item 031 (mesmo bloco).

## 2. Pontos de extensão

| Ponto | Local | Papel |
|---|---|---|
| `DownloadBinary` / `DownloadModFile` | [RequestHandler.cs:207,217-238](../../project/SPT.Launcher.Base/Controllers/RequestHandler.cs#L207) | +overload com `Action<long> onProgress`; laço `Read` quando presente. |
| downloader concreto | [ProfileViewModel.cs:790](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L790) e [ModUpdateViewModel.cs:401](../../project/SPT.Launcher/ViewModels/ModUpdateViewModel.cs#L401) | passa o callback que alimenta o `_rateMeter` por chunk. |
| `WithSpeedMeter` | [ProfileViewModel.cs:807-827](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L807), [ModUpdateViewModel.cs:419-439](../../project/SPT.Launcher/ViewModels/ModUpdateViewModel.cs#L419) | **removido** (medição migra pro downloader). |
| ticker | ProfileViewModel + ModUpdateViewModel | `DispatcherTimer` novo (padrão de [BackgroundCarousel.cs:105-112](../../project/SPT.Launcher/Models/BackgroundCarousel.cs#L105)). |
| rebinding | [ProfileView.axaml:219](../../project/SPT.Launcher/Views/ProfileView.axaml#L219) (Grid.Column=1) e [ModUpdateView.axaml:29](../../project/SPT.Launcher/Views/ModUpdateView.axaml#L29) | `TextBlock Classes="trl-mono"` bindando `DownloadSpeedText`/`HasDownloadSpeed`. |
| `DownloadRateMeter` | [DownloadRateMeter.cs](../../project/SPT.Launcher.Base/Sync/DownloadRateMeter.cs) | +`lock` + `Snapshot()` (thread-safety, CR-01-01); API de amostras inalterada. |

## 3. F12 · Harmony
`N/A`.

## 4. Arquivos (MODIFICAR)

`RequestHandler.cs` (overload streaming) · `ProfileViewModel.cs` (downloader com medição + ticker, remove `WithSpeedMeter`) · `ModUpdateViewModel.cs` (idem) · `ProfileView.axaml` + `ModUpdateView.axaml` (rebind). Sem arquivo novo; sem teste novo (motor já testado; medição de rede = gate in-game).

## 5. Stubs

### 5.0 DownloadRateMeter thread-safe (CR-01-01)

```csharp
// DownloadRateMeter.cs — envolver o estado mutável num lock; ticker lê via Snapshot atômico.
private readonly object _gate = new object();

public void AddSample(long bytes, TimeSpan elapsed)
{
    double seconds = elapsed.TotalSeconds;
    if (bytes <= 0 || seconds <= 0) return;
    lock (_gate) { /* corpo atual: enfileira + somas + evicção da janela */ }
}

public void Reset() { lock (_gate) { /* corpo atual */ } }

/// <summary>Leitura ATÔMICA para o ticker da UI: o par (tem-taxa, texto) sob um único lock,
/// evitando ler _sumBytes somado e _sumSeconds ainda não (torn/incoerente).</summary>
public (bool has, string formatted) Snapshot()
{
    lock (_gate)
    {
        bool has = _sumSeconds > 0 && _sumBytes > 0;
        return (has, has ? Format(_sumBytes / _sumSeconds) : "");
    }
}
```
> As props `HasRate`/`BytesPerSecond`/`FormattedRate` podem permanecer (usadas pelos testes single-thread), mas o **ticker** usa `Snapshot()`. Nenhum comportamento single-thread muda → os 13 testes seguem verdes.

### 5.1 Downloader com progresso intra-arquivo (Base)

```csharp
// RequestHandler.cs
public static byte[] DownloadModFile(string filePath, int timeoutMs = 30000, Action<long> onProgress = null)
    => DownloadBinary($"{request.RemoteEndPoint}{M("/launcher/mods/download")}?file={Uri.EscapeDataString(filePath)}", timeoutMs, onProgress);

private static byte[] DownloadBinary(string url, int timeoutMs = 30000, Action<long> onProgress = null)
{
    var httpRequest = WebRequest.Create(new Uri(url));
    httpRequest.Method = "GET";
    httpRequest.Timeout = timeoutMs;

    using var response = httpRequest.GetResponse();
    using var responseStream = response.GetResponseStream();
    using var memStream = new MemoryStream();

    if (onProgress == null)
    {
        responseStream.CopyTo(memStream);          // caminho antigo intacto (CC-4)
    }
    else
    {
        var buffer = new byte[81920];
        long total = 0; int read;
        while ((read = responseStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            memStream.Write(buffer, 0, read);
            total += read;
            onProgress(total);                     // bytes ACUMULADOS, durante a transferência
        }
    }
    return memStream.ToArray();                     // mesmo byte[] de sempre
}
```
> **CR-01-02:** o `DownloadBinary` atual tem um `try/catch` que loga `LogManager.Instance.Error("[ModUpdate] Download error…")` e re-lança ([RequestHandler.cs:219-237](../../project/SPT.Launcher.Base/Controllers/RequestHandler.cs#L219)). O stub acima o omite por brevidade — **preservar** o `try/catch` no overload (o `using`/laço ficam dentro do `try`), senão perde-se a linha de diagnóstico central.

### 5.2 Downloader concreto alimenta o meter por chunk (VM)

```csharp
// substitui a linha :790 (e a :401 no ModUpdate). O WithSpeedMeter some.
SyncDownloader downloader = (path, ct) => Task.Run(() =>
{
    var sw = Stopwatch.StartNew();
    long last = 0;
    return RequestHandler.DownloadModFile(path, 30000, onProgress: total =>
    {
        long delta = total - last;
        last = total;
        _rateMeter.AddSample(delta, sw.Elapsed);   // (bytes do chunk, tempo do chunk) — média móvel cuida
        sw.Restart();
    });
}, ct);
// (não há mais downloader = WithSpeedMeter(downloader))
```

### 5.3 Ticker de UI (VM)

```csharp
private DispatcherTimer _speedTicker;  // campo

private void StartSpeedTicker()
{
    _speedTicker ??= new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
    _speedTicker.Tick -= OnSpeedTick; _speedTicker.Tick += OnSpeedTick;
    _speedTicker.Start();
}
private void OnSpeedTick(object sender, EventArgs e)
{
    var (has, text) = _rateMeter.Snapshot();       // leitura ATÔMICA (CR-01-01)
    DownloadSpeedText = has ? text : "";           // HasDownloadSpeed segue o texto
}

private void StopSpeedTicker() => _speedTicker?.Stop();

// ProfileViewModel: StartSpeedTicker() logo antes do ExecuteAsync quando plan.IoActionCount > 0;
//   StopSpeedTicker() no finally do run (junto de DownloadSpeedText="" já existente, :769).
// CR-01-04: ModUpdateViewModel NÃO tem `using Avalonia.Threading;` (usa fully-qualified) → adicionar
//   o using (ou qualificar `Avalonia.Threading.DispatcherTimer`). Os downloads reais só ocorrem em
//   UpdateMods (não em CheckForUpdates) → Start antes do ExecuteAsync de UpdateMods (:314), Stop no
//   finally de UpdateMods (:348-355).
```

### 5.4 Rebinding (dentro do Grid da barra — que o 031 põe sob IsSyncRunning)

```xml
<!-- ProfileView.axaml, Grid.Column="1" (onde estava o comentário :219) -->
<TextBlock Grid.Column="1" Text="{Binding DownloadSpeedText}" IsVisible="{Binding HasDownloadSpeed}"
           Classes="trl-mono trl-muted" VerticalAlignment="Center" Margin="8,0,0,0"
           FontSize="{DynamicResource TrlTextXs}"/>
<!-- ModUpdateView.axaml:29 — equivalente -->
```

## 6. Fluxo de dados

```
downloader concreto (Task.Run) → RequestHandler.DownloadModFile(onProgress: total => AddSample(delta, sw))
   → _rateMeter acumula (thread do Task.Run)                          [RequestHandler.cs:Read loop]
DispatcherTimer 500ms (UI thread) → DownloadSpeedText = _rateMeter.FormattedRate   [ticker §5.3]
   → TextBlock Grid.Column=1 (ProfileView.axaml) mostra "12,3 MB/s"   [rebind §5.4]
início do run → _rateMeter.Reset()  ·  fim → StopSpeedTicker() + DownloadSpeedText=""
```

## 7. Riscos e dependências

- **R-1 (Base compartilhada — CC-4):** o overload com `onProgress == null` mantém o `CopyTo` — nenhum caller existente muda de comportamento; os mesmos bytes são entregues e `WriteAtomic`/`ComputeMd5` seguem operando sobre o `byte[]` completo.
- **R-2 (cancelamento intra-arquivo — CC-3):** o `WebRequest` síncrono não cancela um `Read` em andamento; o `ct` corta **entre** arquivos (comportamento atual preservado). O ticker para no `StopSpeedTicker`.
- **R-3 (ticker vivo — CC-6):** o `DispatcherTimer` é parado no `finally` do run; `Tick -= OnSpeedTick` antes de re-assinar evita handler duplicado em runs repetidos.
- **R-4 (janela do meter):** com amostras por-chunk (81920 B), a janela=5 do `DownloadRateMeter` cobre os últimos ~5 blocos — suaviza o suficiente para o ticker de 500 ms; revisitar a janela só se oscilar demais no gate.
- **R-5 (coordenação 031):** o `TextBlock` da velocidade entra **dentro** do `Grid` que o 031 coloca sob `IsVisible="{Binding IsSyncRunning}"` — some junto com a barra ao concluir. Implementar os dois no mesmo passo do `.axaml`.

## 8. Checklist de implementação

1. `RequestHandler.cs`: overload `DownloadModFile`/`DownloadBinary` com `onProgress` + laço `Read`.
2. `ProfileViewModel`: downloader concreto alimenta `_rateMeter` por chunk; remover `WithSpeedMeter`; `DispatcherTimer` (Start no apply, Stop no finally).
3. `ModUpdateViewModel`: idem.
4. `ProfileView.axaml` + `ModUpdateView.axaml`: `TextBlock` da velocidade em Grid.Column=1 (coordenado com o 031).
5. `dotnet build` + `dotnet test` verdes (motor inalterado).
6. `/code-review`. **Gate in-game:** baixar um bundle grande contra o servidor real e ver a taxa **se atualizando durante** o download (não só no fim) — fecha o P-016.1 nunca validado.

## 9. Conformidade com skills

| Item | Status | Evidência |
|---|---|---|
| Lifecycle raid / Harmony / leak | N/A | Launcher pré-jogo (spec §CA N/A). |
| Coop/Fika | N/A | Barra de progresso da UI; sem pacote de rede além do download HTTP do sync. |
| Thread-safety | ✅ | Meter alimentado na thread do `Task.Run`; UI atualizada só pelo `DispatcherTimer` na UI thread (§5.2/§5.3). |
| Não quebrar a Base | ✅ | Overload aditivo; `onProgress==null` = caminho antigo (R-1, CC-4); try/catch de log preservado (CR-01-02). |
| Reaproveitar o motor testado | ✅ | `DownloadRateMeter` ganha só `lock`+`Snapshot` (thread-safety, CR-01-01); a média/formatação e os 13 testes single-thread ficam intactos. |
| Thread-safety da taxa | ✅ | `AddSample` (thread do download) e `Snapshot` (ticker UI) sob o mesmo `lock` (§5.0, CR-01-01). |

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-08-02 | Guilherme | Spec técnica via `/create-technical-spec`. Abordagem: progresso intra-arquivo no `DownloadBinary` (laço `Read` + `Action<long>`), meter alimentado por chunk, `DispatcherTimer` de 500 ms desacoplando a UI, rebinding em Grid.Column=1. Coordenada com o 031 (barra sob `IsSyncRunning`, reset compartilhado). |
| 2026-08-02 | Guilherme | `/review-technical-spec` review 01 (sub-agent adversarial) — **1 🔴** + 2 🟡 aplicados: **race no `_rateMeter`** (a 032 introduz acesso multi-thread) → o `DownloadRateMeter` ganha `lock`+`Snapshot()` (a linha "motor inalterado" caiu); `try/catch` de log preservado no downloader; `DispatcherTimer` no ModUpdate precisa do `using` + Start/Stop em `UpdateMods`. |
