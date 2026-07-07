# 020 — Integridade do cofre de senhas (005/010) · Kickoff

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Origem:** [AUDIT-2026-07-04](../../AUDIT-2026-07-04-code-product-ds.md) (005/010) · **Severidade:** 🟡 (risco de negócio) · **Deps:** 005, 010

> Brief de kickoff — insumo para `/create-spec`. Não é a spec.

## Objetivo
Corrigir a corrupção/colisão de senha e o delete não-atômico de conta no fluxo do `PasswordController` + cofre `redline_passwords.json`.

## Achados
- **Colisão case-insensitive** (`PasswordController.cs:152,174-186`): o core registra username **case-sensitive** ("Bob"≠"bob"), mas a escrita da senha casa por `OrdinalIgnoreCase` no **1º arquivo enumerado** e a chave do cofre é `ToLowerInvariant()` → grava no **perfil errado** / duas contas **colidem** na mesma entrada; a migração lazy apaga a variante da outra.
- **Delete não-atômico** (`ProfileViewModel.cs:1112-1118`): cofre limpo (`ChangePasswordAsync("")`) **antes** do `RemoveAsync`; se o remove falhar/`NoConnection` → conta sobrevive no server **sem senha**.
- **Plaintext** (`PasswordController.cs:275-281`): `/redline/profile/get` devolve a senha em texto puro a quem postar o username (contorna o gate D2).
- Cofre não é tocado por delete/wipe do core → entradas **órfãs**.

## Critérios de aceite (seed)
- Match de perfil e chave de cofre com o **mesmo critério do core** (case-sensitive) — sem colisão "Bob"/"bob".
- Delete transacional/ordem segura (remover conta antes de limpar cofre, ou rollback).
- `/redline/profile/get` não expõe senha em plaintext.
- Limpeza de entradas órfãs ao excluir/wipe.

## Gate humano
Inspecionar `redline_passwords.json` de produção por chaves colidentes **antes** do deploy da DLL (memória: escrita SPT precisa de validação em jogo, não só build).
