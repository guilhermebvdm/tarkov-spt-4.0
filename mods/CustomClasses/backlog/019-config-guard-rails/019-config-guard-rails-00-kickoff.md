# 019 — Guard rails de config (anti-clobber + sync) · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-09 · **Origem:** plano aprovado do editor web de classes (`~/.claude/plans/`, sessão 2026-06-09; renumerado 018→019)
**Wave:** W1 — **merge PRIMEIRO** (020 só inicia depois; pré-condição de qualquer save do editor) · **Deps:** —

> Brief de kickoff — insumo para `/create-spec 019`. Não é a spec.

## Objetivo

O editor (021+) escreve os `.jsonc` no **install** (`D:/SPT/SPT/user/mods/CustomClasses/config/classes/`) — fonte de verdade runtime. Hoje o `compile-mod.sh` copia `config/` repo→install **com clobber** em todo build, e o gerador `build-class-jsons.js` reescreve os `.jsonc` do repo. Sem guardas, qualquer build/regeração destrói edições feitas no editor (precedente: incidente serve-inventory; backup/restore manual no item 016).

## Escopo

- **(a) Anti-clobber no `compile-mod.sh`:** antes de copiar `config/` repo→install, comparar (hash/diff) `config/classes/` do install com o do repo; se divergir, **abortar com diff resumido**; flag `--force-config` para sobrescrever conscientemente.
- **(b) `/sync-classes`:** script (+ skill no padrão do repo) que traz install→repo com preview de diff e confirmação — caminho oficial para commitar edições feitas no editor.
- **(c) Freeze do gerador:** `build-class-jsons.js` recusa sobrescrever `.jsonc` que divergem do que ele geraria (congelado como bootstrap; rodar só com flag explícita).
- **(d) Cópia genérica de `wwwroot/`** repo→install para projetos server-csharp (clobber OK — wwwroot é código, não dado). Pré-requisito do 020.
- Concentrar **todas** as mudanças de `compile-mod.sh` neste item (evita conflito com 020 na mesma wave).

## Riscos / atenção

- Diff deve ignorar diferenças triviais (EOL/whitespace) para não gerar falso-positivo a cada build.
- `/sync-classes` não pode clobberar edições manuais não-commitadas no repo (mostrar diff antes; ver memória `feedback_serve_inventory_clobber`).

## Refs

- [.agents/scripts/compile-mod.sh](../../../../.agents/scripts/compile-mod.sh) — cópia de `config/` (server-csharp install)
- [scripts/build-class-jsons.js](../../scripts/build-class-jsons.js) — gerador a congelar
- Skill `repo-workflow-best-practices` — convenções de skills/scripts do repo

## DoD (resumo)

- Clobber de `config/classes/` impossível sem `--force-config` (testado com divergência plantada).
- `/sync-classes` round-trip testado (editar no install → sync → diff limpo).
- `wwwroot/` copiado no install de projeto server.
