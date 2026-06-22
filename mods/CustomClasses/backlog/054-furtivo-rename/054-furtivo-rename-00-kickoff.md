# 054 — Rename Ghost/Fantasma → Stealth/Furtivo (implementação) · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-21 · **Origem:** redesign — rename de classe ([class-design.md](../../docs/class-design.md) §Contrato de gating)
**Wave:** R-W1 · **Deps:** — · **🔴 É pré-requisito do 050.0** (sem isso o gating do Furtivo cai na chave errada)

> Brief de kickoff — insumo para `/create-spec 054`. Não é a spec.

## Objetivo

A classe foi renomeada **Fantasma → Furtivo (Stealth)** nos docs, mas o **runtime ainda é `Ghost`/"Fantasma"** (`Info.GameVersion` = `displayName[lang]`). Alinhar a implementação:

- **`fantasma.jsonc`:** `name` `Ghost`→`Stealth` · `displayName` {`en` `Ghost`→`Stealth`, `pt` `Fantasma`→`Furtivo`} · (opcional) renomear arquivo → `furtivo.jsonc`.
- **`ClassRegistrar` / viewer / `class-matrix.mjs`** (`file`/`key`): refletir o novo `name`.
- **Re-sync** das classes (server) + **validação in-game** (launcher mostra "Furtivo"/"Stealth"; perfil novo cria com a edição certa).

## Escopo / Riscos

- ⚠️ **Mexe em `modded/Server/`** (config + registrar) → **coordenar** com a sessão paralela do editor web (risco de clobber). **Validar in-game** (escrita+hash não basta).
- **Perfis "Ghost" órfãos** quebram, mas **não importa** — server não está live (decisão do usuário).
- A chave de gating estável (`name`) muda de `Ghost`→`Stealth` → o 050 deve gatear no valor **pós-rename**.

## Refs

- **[../../docs/class-design.md](../../docs/class-design.md)** §Contrato de gating · `modded/Server/config/classes/fantasma.jsonc` · `modded/Server/ClassRegistrar.cs` · `modded/Server/CustomClassesMod.cs:77`

## DoD (resumo)

- Launcher e perfil novo mostram **Furtivo (pt) / Stealth (en)**; `Info.GameVersion` resolve a chave estável `Stealth`.
- `class-matrix.mjs` e o viewer refletem o novo `name` (cross-check ✅).
- Validado in-game (não só write+hash).
