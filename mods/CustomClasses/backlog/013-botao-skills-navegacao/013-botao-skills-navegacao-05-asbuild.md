# 013 — Botão SKILLS na navegação · As-Built

**Mod:** CustomClasses
**Spec funcional:** [013-botao-skills-navegacao-01-spec.md](013-botao-skills-navegacao-01-spec.md)
**Spec técnica:** [013-botao-skills-navegacao-02-spec-tech.md](013-botao-skills-navegacao-02-spec-tech.md)
**Build:** 2026-06-09

> Botão "SKILLS" no menu (clone do CHARACTER) → abre o inventário e seleciona a aba Skills. Implementado de forma autônoma (usuário ausente). Compilado **0 warn/err** (client 35.8 KB). **A validar in-game.**

## Arquivos alterados

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `modded/Client/Patches/SkillsNavButtonPatch.cs` | postfix `MenuScreen.Show`; clona `_playerButton` → `CC_SkillsButton` (label SKILLS/HABILIDADES) abaixo de CHARACTER; onClick: `_playerButton.OnClick.Invoke()` + coroutine. |
| CRIADO | `modded/Client/UI/InventoryTabNavigator.cs` | espera o `InventoryScreen` e seleciona a aba Skills via `_tabDictionary[EInventoryTab.Skills]` → `Tab.Select(true)` (reflection). |
| MODIFICADO | `modded/Client/Plugin.cs` | config `ShowSkillsButton` (default true) + registra `SkillsNavButtonPatch`. |
| MODIFICADO | `mods/CustomClasses/PROPRIEDADES.md` | `ShowSkillsButton`. |

## Decisões implementadas

- **Abrir CHARACTER:** `_playerButton.OnClick.Invoke()` (mesmo fluxo do botão original) — evita depender de `method_8`/`EMenuType` obfuscados.
- **Selecionar aba:** reflection em `_tabDictionary` + `Tab.Select(true)`; coroutine espera o `InventoryScreen` (timeout 120 frames + 5 frames p/ init).
- **Label:** i18n (SKILLS / HABILIDADES). Ícone: herdado do clone (caveira do CHARACTER) — a ajustar se quiser ícone próprio.
- **Idempotente:** `Find("CC_SkillsButton")`; gated por `ShowSkillsButton`.

## A validar (playtest)

- Botão **SKILLS** aparece abaixo de **CHARACTER** no menu (com **e** sem Menu-Overhaul); alinhamento/estilo ok.
- Clicar abre a tela de personagem **na aba Skills**.
- Sem NRE no console.
- Possível ajuste: posição/estilo (MO) e o ícone do botão.

## Mudanças posteriores

(vazio inicialmente)

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-09 | Build via fluxo SSD (autônomo). 0 warn/err. Botão SKILLS + navegação para a aba Skills. |
