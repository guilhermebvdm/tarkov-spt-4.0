# 027 — Criar / duplicar / deletar classe · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-09 · **Origem:** plano aprovado do editor web de classes (`~/.claude/plans/`, sessão 2026-06-09; renumerado 026→027)
**Wave:** W4 (paralelo ao 026) · **Deps:** 021, 024, 025

> Brief de kickoff — insumo para `/create-spec 027`. Não é a spec.

## Objetivo

Ciclo de vida completo de classes pelo editor (Etapa 10 do plano do usuário).

## Escopo

- **Criar:** novo `.jsonc` a partir de template mínimo válido (name + baseEdition default); abre direto no form do 025.
- **Duplicar:** cópia com novo `name` — é o **caminho oficial de "rename"** (rename direto é bloqueado no 025 porque órfã perfis existentes).
- **Deletar / desabilitar:** hot-remove via `ClassRegistrar.Remove` (021) + confirmação destacando perfis existentes que usam a edition (varrer `user/profiles` se barato — decidir na spec; senão aviso genérico "perfis existentes ficarão sem identidade/multiplicadores").
- **Aviso de ícone ausente** ao criar (PNG do client não existe → degrada pra texto; edição de imagem é fase futura).
- Nome novo: validação de colisão com editions existentes (vanilla + classes) — mesma regra do loader.

## Refs

- `ClassEditorService`/`ClassRegistrar.Commit/Remove` (021), lista (024), form (025)

## DoD (resumo)

- Criar → aparece no launcher **sem restart**; deletar → some do launcher.
- Duplicar gera `.jsonc` válido e independente do original.
