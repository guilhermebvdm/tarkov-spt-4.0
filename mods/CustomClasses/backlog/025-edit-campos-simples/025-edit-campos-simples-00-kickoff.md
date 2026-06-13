# 025 — Edição de campos simples + outfit · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-09 · **Origem:** plano aprovado do editor web de classes (`~/.claude/plans/`, sessão 2026-06-09; renumerado 024→025)
**Wave:** W3 (solo — cria o shell de abas que W4/W5 estendem) · **Deps:** 021, 022, 023, 024

> Brief de kickoff — insumo para `/create-spec 025`. Não é a spec. Fecha o MVP (W0–W3).

## Objetivo

Primeira edição persistida: campos simples da classe + outfit, com validação e hot-apply.

## Escopo

- **Shell com abas** na página de edição: Geral | Skills | Hideout | Outfit | Equipado (placeholder → 026) | Stash (placeholder → 028).
- **Geral:** displayName en/pt, description en/pt, `nameColor` (color picker), `enabled`, `baseEdition` (dropdown das editions vanilla disponíveis), `iconFile` (dropdown dos PNGs do `wwwroot/icons/` do server, com **preview** + aviso de degradação pra texto quando ausente no client).
- **`name` read-only** — rename = nova edition key → perfis existentes (`ProfileInfo.Edition` é string) ficariam órfãos de multiplicadores/identidade. Tooltip aponta o caminho oficial: duplicar (027).
- **Skills:** nível 0..51 por skill (enum SkillTypes). **SkillMultipliers:** fator ≥ 0, badge "requer Skills-Extended" nas 4 skills do SE. **Hideout:** HideoutAreas → nível.
- **Outfit:** usec/bear × upper/lower via customization picker (023).
- **Custo ao vivo** (022) recalculado a cada mudança + aviso quando fora do budget 28–32.
- **Save** via 021 (validar → backup → salvar → hot-apply) com banner dos limites do hot-apply (perfil novo OK sem restart; client com jogo aberto não vê identidade/multiplicadores novos — cache 1×/sessão; perfis existentes não mudam).

## Refs

- `ClassEditorService`/`ClassRegistrar` (021), `CostService` (022), pickers (023), shell sobre o detalhe do 024
- `mods/Skills-Extended/modded/Server/Web/Layouts/BaseLayout.razor` — padrão do botão Save

## DoD (resumo)

- Editar e salvar reflete em **perfil novo sem reiniciar o server**.
- Validação bloqueia save inválido com mensagens claras (mesmos diagnósticos do loader).
- Rename bloqueado com tooltip explicativo.
