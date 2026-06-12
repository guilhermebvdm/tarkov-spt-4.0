# 029 — Docs e fechamento do editor · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-09 · **Origem:** plano aprovado do editor web de classes (`~/.claude/plans/`, sessão 2026-06-09; renumerado 028→029)
**Wave:** W5 (paralelo ao 028 — só docs) · **Deps:** todos (018–028)

> Brief de kickoff — insumo para `/create-spec 029` (ou execução direta como item operacional, a decidir).

## Objetivo

Documentar o editor e fechar o épico com verificação ponta a ponta.

## Escopo

- **README do mod:** como acessar o editor (`https://localhost:6969/customclasses`, server rodando), fluxo install↔repo (`/sync-classes` + guarda `--force-config` do compile-mod), gerador congelado como bootstrap.
- **Limites documentados (os 4):**
  1. Hot-apply vale p/ **perfis novos**; client com jogo aberto não vê identidade/multiplicadores novos (cache lazy 1×/sessão).
  2. Perfis existentes nunca mudam (template só na criação).
  3. Save reserializa o `.jsonc` → **comentários são perdidos** (`.bak` preserva o último estado manual).
  4. Rename não existe → duplicar + desabilitar a antiga.
- Atualizar `mod-backlog.md` (status do épico) e `memory/sessions.md`.
- **Smoke test ponta a ponta documentado:** criar classe no editor → ajustar campos/equipado/stash → custo confere → perfil novo no launcher nasce correto → `/sync-classes` → diff no repo limpo.

## DoD (resumo)

- Doc cobre os 4 limites + fluxo de sync.
- Smoke test executado e registrado.
