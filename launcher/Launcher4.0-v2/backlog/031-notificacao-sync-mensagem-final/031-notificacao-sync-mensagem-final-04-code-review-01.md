# 031 — Notificações de sync · Code Review 01

**Mod:** Launcher4.0-v2 · **Data:** 2026-08-02 · Cobre a implementação de **031 + 032** (feitos juntos).

> Review adversarial por sub-agent independente sobre o código implementado. Build 0 erros, 292 testes verdes. Aplicado em `/g-autodev`.

## Resumo
> 🔴 0 · 🟡 2 · 🟢 3 · ✅ Resolvidos: 2 · Aceitos/documentados: 3

| ID | Impacto | Título | Status |
|---|---|---|---|
| CR-01-01 | 🟡 | Um ramo do ModUpdateView ("nada a aplicar") ainda usava `result.Summary` (PT) | ✅ Resolvido |
| CR-01-02 | 🟡 | Contagem do link "ver detalhes" divergia entre run ao vivo e reload do JSON | ✅ Resolvido |
| CR-01-03 | 🟢 | `IsUpdateVisible=true` com status vazio por ms no load standalone | Aceito (transiente cosmético) |
| CR-01-04 | 🟢 | `DownloadBytesPerSec` virou property morta (ticker só seta o texto) | Aceito (inócuo; dívida de limpeza) |
| CR-01-05 | 🟢 | 1º sample da taxa inclui a latência do `GetResponse` → subestima | Aceito (dilui na janela de 5; medição, não bug) |

## CR-01-01 · 🟡 ✅ — ModUpdateView "nada a aplicar" com Summary PT
`ModUpdateViewModel.cs:260` (ramo up-to-date do `CheckForUpdates`) ficou com `SummaryText = result.Summary` — as outras duas migrações (linhas 322/340) foram feitas, esta escapou. Em inglês mostrava PT cru. **Resolução:** `SummaryText = SyncMessages.BuildSummary(result)`.

## CR-01-02 · 🟡 ✅ — Contagem do link inconsistente
O `SetLastUpdate` ao vivo somava `Updated + … + OptionalConfigApplied`, mas o `LoadLastUpdateInfo` (reload do `last-update.json`) somava sem `optionalConfigApplied` (o `SyncReport.Write` **não gravava** essa chave) e sem `referenceUpdated` — enquanto o `r.Updated` ao vivo inclui as reference-updates. Efeito: um run só de config opcional mostrava o link ao vivo e **perdia** ao reabrir o launcher; runs com reference-update mostravam número maior ao vivo. **Resolução:** `SyncReport.Write` passa a gravar `optionalConfigApplied`; o `LoadLastUpdateInfo` soma `updated + referenceUpdated + moved + deleted + forced + seeded + optionalConfigApplied` — alinhado com o ao vivo (`updated+referenceUpdated == r.Updated`).

## Aceitos (🟢)
- **CR-01-03:** no arranque o painel fica visível com status em branco por ~ms até o auto-sync preencher `update_checking`. Cosmético; não vale complicar o load.
- **CR-01-04:** `DownloadBytesPerSec` sem setter ativo nem binding — sempre 0. Inócuo; remoção fica como limpeza futura (não removido para não arriscar binding oculto).
- **CR-01-05:** o `sw` inclui a latência de conexão no 1º chunk → subestima o primeiro sample; a janela móvel de 5 dilui. É característica de medição, não erro.

## Confirmados limpos (as preocupações principais)
Ticker: `??=` (uma instância), `Tick -= / +=` (sem duplicata), `StopSpeedTicker` no `finally` dos dois VMs (sucesso/erro/cancelamento), UI thread — sem leak/CPU presa. i18n: as 14 chaves existem nos 4 lugares, placeholders casam, loader all-or-nothing blinda contra NRE. Meter thread-safe (lock em Add/Reset/Snapshot; props antigas só nos testes). Downloader por-chunk (`delta=total-last`, `sw.Restart`) correto. `_syncRunId` sem falso-positivo. Sem órfãos de `WithSpeedMeter`/`update_completed`.

## Histórico
| Data | Evento |
|---|---|
| 2026-08-02 | Code review 01 (sub-agent adversarial). 0 🔴 · 2 🟡 (aplicados) · 3 🟢 (aceitos). Build 0 erros, 292 testes verdes. |
