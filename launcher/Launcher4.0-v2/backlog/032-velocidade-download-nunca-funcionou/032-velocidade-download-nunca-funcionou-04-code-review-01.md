# 032 — Velocidade de download · Code Review 01

**Mod:** Launcher4.0-v2 · **Data:** 2026-08-02

> O code-review foi feito em conjunto com o 031 (implementados juntos) — ver [031-04-code-review-01](../031-notificacao-sync-mensagem-final/031-notificacao-sync-mensagem-final-04-code-review-01.md) para o laudo completo.

## Resumo (parte 032)
> 🔴 0 · 🟡 0 · 🟢 2 (aceitos) — **nenhum bug de correção na velocidade**.

Confirmados **limpos** pelo revisor adversarial:
- **Thread-safety do meter** ✅ — `AddSample`/`Reset`/`Snapshot` sob o mesmo `lock`; as props antigas sem lock só têm caller nos testes single-thread; o ticker usa `Snapshot()`.
- **DispatcherTimer** ✅ — `??=` (uma instância), `Tick -=` antes de `+=`, `StopSpeedTicker` no `finally` dos dois VMs (sucesso/erro/cancelamento), UI thread — sem leak, sem CPU presa.
- **Downloader por-chunk** ✅ — `onProgress` reporta bytes acumulados; `delta = total - last` (1º chunk `last=0` correto); `sw.Restart()` dá o elapsed do chunk.
- **Streaming** ✅ — `Read`+`ToArray` = mesmo `byte[]` que `CopyTo`; `onProgress==null` = caminho antigo (CC-4); try/catch de log preservado.

Aceitos (🟢): `DownloadBytesPerSec` virou property morta (CR-01-04 do 031); o 1º sample subestima a taxa pela latência de conexão (CR-01-05 do 031, dilui na janela de 5).

## Gate in-game (o que só valida jogando)
Baixar um bundle grande contra o servidor real e ver a taxa **se atualizando durante** o download (não só no fim) — fecha o P-016.1 nunca validado.

## Histórico
| Data | Evento |
|---|---|
| 2026-08-02 | Code review 01 (junto com o 031). 0 🔴/🟡 na velocidade; thread-safety/ticker/streaming confirmados limpos. Falta o gate in-game (bundle grande contra o servidor real). |
