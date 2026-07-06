# 019 — Guard de raiz + atomicidade nos caminhos legados de FS · Kickoff

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Origem:** [AUDIT-2026-07-04](../../AUDIT-2026-07-04-code-product-ds.md) §B2 + §Motor de sync · **Severidade:** 🔴 Blocker (perda de dados) · **Deps:** 007

> Brief de kickoff — insumo para `/create-spec`. Não é a spec.

## Objetivo
Levar todos os caminhos de mutação de FS que ficaram **fora** do `SyncEngine` a herdar o guard CR-01-05 (`ResolveUnderRoot`), apply atômico e deleção para a lixeira.

## Achados
- **`deleteFiles` do manifesto** (`ProfileViewModel.cs:644-659`): `Path.Combine(gamePath, deleteFile)` + delete **sem** `ResolveUnderRoot` → traversal `../../` ou caminho absoluto apaga arquivo do SO/usuário; roda **automático em todo login**.
- **`OptionalModsHelper`** (`:234-255,301,351`): `Path.Combine(GamePath, file.path)` sem guard; `File.WriteAllBytes` direto (não-atômico, sem temp+move); remoção com `File.Delete` (**permanente**, não vai p/ lixeira).

## Critérios de aceite (seed)
- Todo delete/write/move desses caminhos passa por `ResolveUnderRoot` (rejeita `..`/absoluto/fora da raiz) — testado com manifesto adulterado.
- Writes atômicos (temp + move same-volume) com rollback.
- Deleções vão para a lixeira (consistente com o resto do fluxo).

## Correlatos 🟢 (avaliar na spec)
Migrar manifesto/baseline de **MD5 → SHA-256**; resolver symlink/junction antes de mutar (`SyncEngine.cs:248-258`); teto em `managedPaths` (`SyncPlanner.cs:264-274`).
