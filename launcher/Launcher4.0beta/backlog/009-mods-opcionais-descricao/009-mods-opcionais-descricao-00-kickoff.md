# 009 — Mods opcionais com descrição · Kickoff

**Launcher:** Launcher4.0beta · **Data:** 2026-07-03 · **Origem:** Trello MTav8H5f itens 4.3, 4.3.2 e 4.3.2.1–4

> Brief de kickoff — insumo para `/create-spec`. Não é a spec.

## Objetivo

Seção de **mods opcionais** na tela logada com toggle + **descrição em todos**:

| Mod | Nota do card |
|---|---|
| Hollywood Effects | — |
| PiP Disable | Avaliar se precisa desabilitar o `ExternalResolution` ao marcar |
| IRL | (TarkovIRL) |
| Visceral | — |

## Estado atual

- Base já existe: [Helpers/OptionalModsHelper.cs](../../project/SPT.Launcher/Helpers/OptionalModsHelper.cs) e [ViewModels/OptionalModToggle.cs](../../project/SPT.Launcher/ViewModels/OptionalModToggle.cs) — mapear quais mods já estão cadastrados e como o toggle age (mover DLL? pasta `plugins-disabled`?).

## Perguntas p/ a spec

- Fonte das descrições (hardcoded no launcher vs servida pelo server junto com a lista).
- Interação com o sync do 007 (mod opcional desabilitado não pode ser re-baixado pelo espelho de `plugins`).
