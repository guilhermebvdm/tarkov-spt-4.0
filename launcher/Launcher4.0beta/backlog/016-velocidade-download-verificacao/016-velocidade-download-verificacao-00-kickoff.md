# 016 — Velocidade de download na "Verificar arquivos" · Kickoff

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Origem:** pedido do usuário (2026-07-04) · **Deps:** 007 (motor de sync)

> Brief de kickoff — insumo para `/create-spec`. Não é a spec.

## Objetivo

Durante a verificação/sincronização de arquivos ("Verificar arquivos"), exibir a **velocidade do download** (ex.: `12,4 MB/s`) para o usuário acompanhar o progresso.

## Estado atual

- O motor do 007 baixa via delegate/`RequestHandler` (`SPT.Launcher.Base/Sync/SyncEngine.cs` + `DownloadBinary` no `RequestHandler.cs`); o progresso hoje é por contagem de arquivos (`SummaryText`, ProgressBar) — **não** há taxa de transferência.
- UI: barra de update na [ProfileView.axaml](../../project/SPT.Launcher/Views/ProfileView.axaml) + a [ModUpdateView.axaml](../../project/SPT.Launcher/Views/ModUpdateView.axaml) (VM `ModUpdateViewModel`).

## Direções para a spec

- Medir bytes/tempo no ponto de download (o downloader do `SyncEngine` já é injetável — medir ali, sem acoplar à UI). Suavizar a taxa (média móvel) para não piscar.
- Expor a taxa no VM (`ModUpdateViewModel`/`ProfileViewModel`) e renderizar com token do tema (`.trl-mono` p/ o número).
- Corner cases: arquivo pequeno/instantâneo (evitar divisão por zero / picos), cache hit (sem download → não mostrar taxa), cancelamento no meio, exibir em `MB/s` vs `MiB/s` (decidir e registrar).
- Formato i18n (separador decimal PT-BR).
