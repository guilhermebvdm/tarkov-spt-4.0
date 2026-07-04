# 026 — Migração de integridade MD5 → SHA-256 (manifesto + baseline) · Kickoff

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Origem:** deferido do item 019 (correlato 🟢) · **Severidade:** 🟡 · **Deps:** 007, 019

> Brief de kickoff — insumo para `/create-spec`. Não é a spec.

## Objetivo
Migrar a âncora de integridade do sync de **MD5 → SHA-256**. MD5 tem colisão forjável: um arquivo malicioso do server com hash colidente é tratado como "up-to-date" e nunca corrigido; o baseline (`sync-state.json`) também é MD5.

## Escopo (mudança coordenada server + launcher)
- Manifesto do server (`TarkovRedLine.Server` → `ModUpdater.GetFileHash`) → SHA-256.
- Baseline do launcher (`SyncPathUtil.ComputeMd5` / `user/launcher/sync-state.json`) → SHA-256.
- Skip-por-hash dos opcionais (`OptionalGroupApplier`, item 021) e o log de hash do rate meter (016).
- Compatibilidade: migrar/versionar o `sync-state.json` MD5 existente — re-baseline no 1º run pós-upgrade.

## Corner cases
- Server e launcher trocam **juntos** (hash mismatch → re-download geral no 1º sync — aceitável 1×).
- Custo de CPU do SHA-256 vs MD5 em muitos arquivos (medir; o hash roda por arquivo).

## Nota
Tirado do 019 de propósito: toca dados de 007/008/016/017/019/021 — é item de **dados** isolado, não inflar o guard de FS do 019.
