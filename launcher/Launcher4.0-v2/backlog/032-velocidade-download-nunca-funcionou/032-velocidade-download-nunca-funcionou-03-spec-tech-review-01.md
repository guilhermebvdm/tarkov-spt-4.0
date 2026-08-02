# 032 — Velocidade de download · Review Técnica 01

**Mod:** Launcher4.0-v2 · **Data:** 2026-08-02 · **Spec:** [02-spec-tech](032-velocidade-download-nunca-funcionou-02-spec-tech.md)

> Review adversarial por sub-agent independente (verificou as 2 specs 031/032 contra o código real). Aplicado em `/g-autodev`.

## Resumo
> 🔴 1 · 🟡 2 · 🟢 (confirmados corretos: bytes idênticos no streaming, length-less OK) · ✅ Resolvidos: 3

| ID | Impacto | Título | Status |
|---|---|---|---|
| PA-01-01 | 🔴 | Race no `_rateMeter` — a 032 introduz o acesso multi-thread; meter sem lock | ✅ Resolvido |
| PA-01-02 | 🟡 | Stub do downloader derruba o `try/catch` de log do `DownloadBinary` | ✅ Resolvido |
| PA-01-03 | 🟡 | `DispatcherTimer` no ModUpdate: falta `using` + Start/Stop não localizados | ✅ Resolvido |

## PA-01-01 · 🔴 ✅ Resolvido — Race no `DownloadRateMeter`

**Problema:** hoje `AddSample` e a leitura rodam na **mesma** thread (a continuação do `await` do `WithSpeedMeter` resume na UI thread). A 032 move `AddSample` para dentro do `Task.Run` (thread de download) e o ticker lê no `DispatcherTimer` (UI thread) → duas threads sobre `_sumBytes`/`_sumSeconds`/`Queue` **sem trava**. O `DownloadRateMeter` não tem `lock` (verificado: "pure, single-thread by design"). Não crasha a `Queue` (só um escritor, downloads sequenciais), mas **lê taxa incoerente** (par de somas de janelas diferentes) e torn read de `long`/`double` (não garantido atômico pelo ECMA). A afirmação da spec "motor inalterado / thread-safety ✅" era **falsa**.

**Resolução:** o `DownloadRateMeter` ganha um `lock` (`AddSample`/`Reset` sob o gate) e um `Snapshot()` que devolve `(has, formatted)` atomicamente; o ticker usa `Snapshot()` (§5.0). Os 13 testes single-thread seguem verdes. Spec §1/§2/§5.0/§5.3/§9 atualizadas.

## PA-01-02 · 🟡 ✅ Resolvido — `try/catch` de log preservado

**Problema:** o `DownloadBinary` atual envolve o corpo num `try/catch` que loga `LogManager.Error("[ModUpdate] Download error…")` + rethrow ([RequestHandler.cs:219-237](../../project/SPT.Launcher.Base/Controllers/RequestHandler.cs#L219)). O stub do overload o omitia → perda de diagnóstico central.

**Resolução:** nota no §5.1 — o overload preserva o `try/catch` (laço/`using` dentro do `try`).

## PA-01-03 · 🟡 ✅ Resolvido — `DispatcherTimer` no ModUpdate

**Problema:** o `ModUpdateViewModel` usa tudo fully-qualified — **não tem** `using Avalonia.Threading;`, então `private DispatcherTimer _speedTicker;` não compila. E a spec não dizia onde dar Start/Stop nessa tela (os downloads só ocorrem em `UpdateMods`, não em `CheckForUpdates`).

**Resolução:** nota no §5.3 — adicionar o `using` (ou qualificar), Start antes do `ExecuteAsync` de `UpdateMods` (:314), Stop no `finally` de `UpdateMods` (:348-355).

## Confirmados corretos (sem achado)
Streaming: o laço `Read`+`ToArray` dá o **mesmo `byte[]`** que `CopyTo` (que já usa buffer 81920); `onProgress==null` mantém o `CopyTo` literal (CC-4); stream sem `Length` é irrelevante (lê até `Read`==0). `WriteAtomic`/`ComputeMd5` intactos.

## Histórico
| Data | Evento |
|---|---|
| 2026-08-02 | Review 01 (sub-agent adversarial). 1 🔴 (race do meter) + 2 🟡, todos aplicados na spec técnica no mesmo passo (`/g-autodev`). |
