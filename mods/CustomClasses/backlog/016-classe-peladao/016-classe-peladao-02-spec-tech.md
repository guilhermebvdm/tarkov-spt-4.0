# 016 — Classe "Peladão" · Spec Técnica

**Mod:** CustomClasses
**Spec funcional:** [016-classe-peladao-01-spec.md](016-classe-peladao-01-spec.md)
**Criado:** 2026-06-09

> Classe nova = uma recipe no gerador + um JSON. Sem mecanismo novo (reusa 002/004/011). **Conteúdo PLACEHOLDER** (skin/itens/descrição/cor a revisar pelo usuário).

## 1. Estratégia

Adicionar a recipe `peladao` em `scripts/class-recipes.js` (mesma estrutura das 10 classes). O `build-class-jsons.js` gera `config/classes/peladao.jsonc` com `baseEdition: "SPT Zero to hero"`. O `CLASS_VISUAL.peladao` (ícone+cor) já existia; **sem** entrada em `SKILL_MULTIPLIERS` → sem buff/debuff. O loader (`CustomClassesMod`) registra automaticamente todo `.jsonc` em `config/classes/`.

## 2. Recipe (placeholder)

| Campo | Valor |
|---|---|
| `fileName`/`name` | `peladao` / "Peladão" |
| `description` | engraçada (en/pt) — placeholder |
| `skillOverrides` | `{}` (sem skills iniciais) |
| `hideout` | `{}` |
| `backupCount` | `0` |
| `outfit` | camisa havaiana placeholder (usec: Blue Hawaii + Outdoor Tactical; bear: Green Hawaii + Centurion) |
| `primary`/`backup`/`tema` | `[]` (só o `BASELINE` comum: dinheiro/meds/comida/faca) |
| `iconFile`/`nameColor` | `peladao.png` / `#c28a60` (já no CLASS_VISUAL) |

Resultado gerado: **equipado=0 (nasce "pelado")**, stash=9 (BASELINE), sem `skillMultipliers`.

## 3. Arquivos

| Ação | Path | Resumo |
|---|---|---|
| MODIFICAR | `scripts/class-recipes.js` | + recipe `peladao` (placeholder). |
| GERADO | `modded/Server/config/classes/peladao.jsonc` | via `node scripts/build-class-jsons.js`. |

> **Cuidado tomado:** os outros 10 JSONs tinham edições não-commitadas (cores/ícones do usuário). Backup antes do build + restauração depois → só `peladao.jsonc` é novo; os demais ficaram intactos.

## 4. A revisar (placeholder → conteúdo final)

- **Skin:** trocar a camisa havaiana por algo "menos roupa" definitivo (catálogo em `scripts/suits-catalog.json`).
- **Itens:** decidir se nasce 100% sem nada (remover o BASELINE só p/ o peladao no build) ou mantém o kit mínimo.
- **Descrição/cor:** ajustar a piada e o tom de cor.
- **Ícone:** `peladao.png` é placeholder.

## 5. Riscos

- **Nascer "pelado" (equipped vazio):** validar in-game que o perfil cria sem estado inválido (corner case da spec).
- **Regenerar o build** sobrescreve todos os JSONs → sempre fazer backup/restore enquanto houver edições manuais não-commitadas (feito).

## Histórico

| Data | Evento |
|---|---|
| 2026-06-09 | Spec técnica + recipe placeholder (a pedido: "qualquer skin, reviso depois"). |
