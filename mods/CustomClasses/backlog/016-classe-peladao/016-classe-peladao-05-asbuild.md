# 016 — Classe "Peladão" · As-Built

**Mod:** CustomClasses
**Spec funcional:** [016-classe-peladao-01-spec.md](016-classe-peladao-01-spec.md)
**Spec técnica:** [016-classe-peladao-02-spec-tech.md](016-classe-peladao-02-spec-tech.md)
**Build:** 2026-06-09

> 11ª classe — "Peladão". Base SPT Zero to Hero, **sem skills nem multiplicadores**, nasce **pelado** (equipado=0) + kit mínimo no stash + skin havaiana placeholder. **Conteúdo placeholder — o usuário revisa skin/itens/descrição/cor.** Compilado/instalado 0 erros.

## Arquivos alterados

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `scripts/class-recipes.js` | + recipe `peladao` (placeholder; outfit havaiana, sem skills, primary/backup/tema vazios). |
| GERADO/INSTALADO | `modded/Server/config/classes/peladao.jsonc` → `D:/SPT/SPT/user/mods/CustomClasses/config/classes/` | `baseEdition: SPT Zero to hero`, `skills:{}`, sem `skillMultipliers`, `iconFile:peladao.png`, `nameColor:#c28a60`. |

## Resultado gerado

- **equipado=0** (nasce sem nada vestido — "peladão"), **stash=9** (BASELINE: 100k rublos, meds, comida, baioneta).
- **Sem buff/debuff** (não está em `SKILL_MULTIPLIERS`).
- Skin: Blue/Green Hawaii shirt (placeholder).

## Cuidado tomado

Os 10 JSONs de classe estavam **modificados (não commitados)** — edições de cor/ícone do usuário. Fiz **backup → build → restore dos 10** → só `peladao.jsonc` ficou novo; as edições do usuário foram **preservadas** (confirmado por `git status`).

## A revisar (pelo usuário)

- Skin definitiva ("menos roupa"), itens (kit mínimo vs 100% pelado), descrição (piada), cor, ícone.

## Mudanças posteriores

**2026-06-09 — 100% pelado (pedido do usuário):** o usuário confirmou que quer o Peladão **sem nenhum item e sem skill inicial**. A 1ª versão ainda trazia o `BASELINE` (kit comum: 100k rublos, meds, comida, baioneta) no stash. Correção:
- Flag **`noBaseline: true`** na recipe + condicional `if (!p.noBaseline) add(BASELINE)` no `build-class-jsons.js` (`aggregate`).
- Regenerado: **`skills:{}`, sem `skillMultipliers`, `equipped:{}`, stash 0 itens**. Confirmado no install do server.
- Skin havaiana (placeholder) mantida — única coisa a revisar. Os outros 10 JSONs preservados (restaurados do backup, exceto o peladao).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-09 | Build via fluxo SSD. Placeholder funcional; conteúdo a revisar. 0 erros. |
