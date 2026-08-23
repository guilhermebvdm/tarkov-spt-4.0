---
title: "Handoff — Rodada de otimização de performance do TRL-ImmersiveCombatMedicine"
date: 2026-08-22
status: 🟢 Vivo
authors: Claude (investigação de performance 2026-08-22)
---

# Handoff — Rodada de otimização de performance do TRL-ImmersiveCombatMedicine

**Comando de entrada da sessão:** `/optimize-mod-performance TRL-ImmersiveCombatMedicine --escopo "modded-V3(review)"`

## Contexto (por que esta rodada existe)

Investigação de performance do Red Line (2026-08-22, raid Customs, baseline CapFrameX + logs) identificou o jogo 100% CPU/main-thread-bound. O ICM apareceu como 2º maior emissor de log da raid (~2,5 k linhas) e com um loop de trabalho periódico que roda independentemente de haver consumidores. Plano aprovado: 3 frentes paralelas em worktrees (DynamicSpawn, ICM, Stances), cada uma pelo harness `/optimize-mod-performance`.

## O que JÁ foi resolvido (não redescobrir)

- **Drift de config corrigido em 2026-08-22 (FASE 1 do plano):** `Verbose Engine Log` e `Debug Test Consumer` estavam `true` nos dois canais vivos — `D:\SPT\BepInEx\config\com.trl.immersivecombatmedicine.cfg` (l.158/195) e `D:\SPT\BepInEx\config-server\com.trl.immersivecombatmedicine.cfg` (l.146/183, canal de distribuição) — ambos revertidos para `false` (defaults do código). Backups `.bak-2026-08-22` ao lado. **Pendência do usuário: subir o config-server corrigido para produção.**
- Os ~47 k "reconcile sweep" no LogOutput acumulado eram efeito desse drift + `AppendLog=true` do BepInEx — não é bug de código de logging.

## Achados para a auditoria (F1) aprofundar

1. **Sweep de reconciliação 2 Hz independente de consumidores (FREQ×ENT):** `Reconcile()` em `modded-V3(review)/Patches/Trauma/TraumaEngine.cs:606` roda a raid inteira; único early-out é `_records.Count == 0`, e `_records` inclui **bots** (`ActivateTracking :283-291` varre `RegisteredPlayers` com `IsOwnedHere`; `OnPersonAdd :338-344`). Refaz 5 chamadas `IsBodyPartDestroyed/Broken` por jogador rastreado por sweep (`:631-635`), consumido ou não. Avaliar: early-out quando não há consumidor ativo para a região; e/ou cadência adaptativa.
2. **Emissores sem gate** (logam sempre, alguns por evento de dano): transições em `TraumaObservability.cs:27`; one-shot/toast/roll `:34/:36/:45/:69`; **Blackout por evento de dano incluindo hits ignorados** `TraumaBlackoutTrigger.cs:73/82`; bot fall ×N bots `TraumaBotFall.cs:116/159/166/248/265/298/332`; speed-limit RECOMPUTE `SpeedLimitPatches.cs:64/79`; tremor `TraumaArmsConsumer.cs:444`. Gatear por `Verbose Engine Log` (padrão já usado em `TraumaEngine.cs:612`, que checa o gate ANTES da interpolação — replicar).
3. **Registro incondicional do Debug Test Consumer** em `TraumaEngine.cs:155-157` (`Awake()`): o registro acontece sempre; só o `isActive` lê o toggle. Avaliar registrar apenas quando o toggle liga.

## Fricções conhecidas (resolver antes/durante a F1)

- **`modded/` é casca vazia** — fonte canônico é `modded-V3(review)/` (v1.13.5, `AssemblyName TRLImmersiveCombatMedicine`). Por isso o `--escopo` no comando. Consolidação do layout V1/V2/V3 → `modded/` foi **explicitamente adiada** (decisão do usuário).
- **Blocker `[P-9.1]`** (memory/sessions.md): validar a build consolidada v1.13.5 in-game — inclui cenário de console limpo com debug off. **Reconciliar com esta rodada, não duplicar** (a validação F4 da rodada pode cobrir o P-9.1).
- Backlog: nenhum item de perf existe; próximo número livre **022** (1 item por rodada via `/add-backlog-item`).

## Referências

- Relatório-irmão com o baseline completo da investigação: [mods/TRL-DynamicSpawn/docs/relatorio-auditoria-codigo-01.md](../../TRL-DynamicSpawn/docs/relatorio-auditoria-codigo-01.md) (§1 Resumo Executivo)
- Plano aprovado: `C:\Users\guime\.claude\plans\vamos-la-precisamos-criar-hidden-lampson.md`
- Validação integrada V2 (frames >100 ms, RAM, residual) roda depois que as 3 frentes mergearem — critérios no plano.
